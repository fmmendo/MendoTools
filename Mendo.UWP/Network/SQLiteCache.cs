using Mendo.UWP.Common;
using Mendo.UWP.Compression;
using Mendo.UWP.Extensions;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mendo.UWP.Network
{
    public class SQLiteCache : Singleton<SQLiteCache>, INetworkCache
    {
        #region Cache Instances

        private static SQLiteCache _compressedCache = new SQLiteCache(@"\cnetcache.cudb", true, true);
        public static SQLiteCache CompressedInstance => _compressedCache;

        private static SQLiteCache _resumeCache = new SQLiteCache(@"\cuniresumecache.cudb", true, useGzipCompression: false);
        public static SQLiteCache ResumeData => _resumeCache;

        #endregion


        public SQLiteConnection _connection = null;
        private string _databasePath = null;
        private IStreamCompressor _compressor = null;


        //private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        public int MaxEntries = 300;

        public SQLiteCache()
        {
            _databasePath = Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path + @"\networkcache.udb";
            _connection = new SQLiteConnection(_databasePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
            _connection.CreateTable<SQLCacheEntry>();
        }

        public SQLiteCache(String databasepath, bool isPathRelative = true)
        {
            _databasePath = isPathRelative ? Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path + @"\" + databasepath : databasepath;
            _connection = new SQLiteConnection(_databasePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
            _connection.CreateTable<SQLCacheEntry>();
        }

        public SQLiteCache(String databasepath, bool isPathRelative = true, bool useGzipCompression = false)
        {
            _databasePath = isPathRelative ? Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path + @"\" + databasepath : databasepath;
            _connection = new SQLiteConnection(_databasePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
            _connection.CreateTable<SQLCacheEntry>();

            if (useGzipCompression)
                _compressor = GZip.Instance;
        }

        public int GetMaxEntries() => MaxEntries;

        public Task<CacheResult<byte[]>> GetBytesAsync(string uri, TimeSpan? expiry = null)
        {
            return Task.Run(async () =>
            {
                CacheResult<byte[]> result = new CacheResult<byte[]>();
                try
                {
                    _rwLock.EnterReadLock();

                    // If table doesn't exist, there's no data to get.
                    if (!DoesCacheTableExist(_connection))
                    {
                        result.Exists = false;
                    }
                    else
                    {
                        // Check to see if there's a matching entry in the DB
                        SQLCacheEntry entry = _connection.Find<SQLCacheEntry>(e => e.Key == uri);
                        if (entry == null)
                        {
                            result.Exists = false;
                        }
                        else
                        {
                            // If there is, get the data and set the expiry times.
                            // For this cache, we currently don't have a "last accessed" property on cache entries
                            result.Result = entry.Data;
                            result.Exists = true;
                            if (expiry == null || !expiry.HasValue)
                            {
                                result.Expired = false;
                            }
                            else if (expiry.HasValue)
                            {
                                result.Expired = (DateTime.Now - entry.DateAdded > expiry.Value);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }

                try
                {
                    // We attempt decompression *outside* of the ReadLock, just so we're not wasting the
                    // time of anything wanting to access the WriteLock
                    if (result != null && result.Exists && result.Result != null && _compressor != null)
                    {
                        result.Result = await DecompressDataAsync(result.Result).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                }

                return result;
            });
        }


        public Task<CacheResult<string>> GetStringAsync(string uri, TimeSpan? expiry)
        {
            throw new NotImplementedException();

            //CacheResult<String> result = new CacheResult<string>();
            //SQLiteAsyncConnection asyncConn = new SQLiteAsyncConnection(_databasePath);

            //await asyncConn.(conn =>
            //{
            //    SQLCacheStringEntry entry = conn.Find<SQLCacheStringEntry>(e => e.Key == uri);
            //    if (entry == null)
            //    {
            //        result.Exists = false;
            //    }
            //    else
            //    {
            //        result.Result = entry.Data;
            //        result.Exists = true;
            //        if (expiry == null || !expiry.HasValue)
            //        {
            //            result.Expired = false;
            //        }
            //        else if (expiry.HasValue)
            //        {
            //            result.Expired = (DateTime.Now - entry.DateAdded > expiry.Value);
            //        }
            //    }
            //});

            //return result;
        }




        public Task SaveAsync(string Uri, byte[] data)
        {
            return Task.Run(async () =>
            {
                byte[] blob = (_compressor == null) ? data : await CompressDataAsync(data).ConfigureAwait(false);
                data = null;
                try
                {
                    _rwLock.EnterWriteLock();

                    _connection.RunInTransaction(() =>
                    {
                        {
                            if (!DoesCacheTableExist(_connection))
                                _connection.CreateTable<SQLCacheEntry>();

                            SQLCacheEntry entry = _connection.Find<SQLCacheEntry>(e => e.Key == Uri);

                            if (entry != null)
                            {
                                entry.Data = blob;
                                entry.DateLastAccessed = DateTime.Now;
                                _connection.Update(entry);
                            }
                            else
                            {
                                entry = new SQLCacheEntry()
                                {
                                    DateLastAccessed = DateTime.Now,
                                    DateAdded = DateTime.Now,
                                    Data = blob,
                                    Key = Uri
                                };

                                _connection.Insert(entry);
                            }
                        }
                    });
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }
            });
        }



        public Task SaveAsync(string Uri, string result)
        {
            return SaveAsync(Uri, result.ToBytes());
        }


        public Task InitialiseAsync()
        {
            return Task.FromResult(0);
        }

        public Task TrimAsync()
        {
            return Task.Run(() =>
            {
                try
                {

                    _connection.RunInTransaction(() =>
                    {
                        int itemCount = 0;
                        TableQuery<SQLCacheEntry> toodelete;
                        try
                        {
                            _rwLock.EnterReadLock();

                            // If no cache table, nothing to trim!
                            if (!DoesCacheTableExist(_connection))
                                return;

                            // Count the items - we only care if we're over the count in this case
                            itemCount = _connection.Table<SQLCacheEntry>().Count();
                            if (itemCount <= GetMaxEntries())
                                return;
                        }
                        finally
                        {
                            _rwLock.ExitReadLock();
                        }


                        try
                        {

                            _rwLock.EnterWriteLock();

                            // Find what we want to delete
                            toodelete = _connection.Table<SQLCacheEntry>().OrderBy(entry => entry.DateLastAccessed).Take(GetMaxEntries() - itemCount);

                            // Delete all the items we need too
                            foreach (var entry in toodelete.ToList())
                                _connection.Delete(entry);

                            // Clear the blank space
                            Vacuum();
                        }
                        finally
                        {
                            _rwLock.ExitWriteLock();
                        }


                    });

                }
                catch { }

            });
        }


        public Task ClearCacheAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    _rwLock.EnterWriteLock();

                    _connection.RunInTransaction(() =>
                    {
                        if (DoesCacheTableExist(_connection))
                            _connection.DropTable<SQLCacheEntry>();

                        _connection.CreateTable<SQLCacheEntry>();
                    });

                    Vacuum();
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }
            });
        }

        /// <summary>
        /// WARNING: This cannot be done inside a transaction.
        /// Compress' database by removing blankspace / holes / fragmentation inside the data structure
        /// </summary>
        void Vacuum()
        {
            SQLiteCommand vacuum = _connection.CreateCommand("vacuum");
            vacuum.ExecuteNonQuery();
        }

        bool DoesCacheTableExist(SQLiteConnection c)
        {
            if (c.TableMappings.Any())
                return true;

            return false;
            //return c.GetTableInfo(nameof(SQLCacheEntry)).Count > 0;
            //return c.TableMappings.FirstOrDefault(t => t.TableName == nameof(SQLCacheEntry)) != null;
        }


        #region Compression Helpers


        private async Task<byte[]> CompressDataAsync(byte[] data)
        {
            byte[] bytes = null;

            using (MemoryStream destination = new MemoryStream())
            using (var stream = data.AsStream())
            {
                bytes = await _compressor.CompressToBytesAsync(stream).ConfigureAwait(false);
            }

            return bytes;
        }

        private async Task<byte[]> DecompressDataAsync(byte[] data)
        {
            byte[] bytes = null;

            using (var dataStream = data.AsStream())
            using (var resultStream = new MemoryStream())
            {
                // 3. Decompress the data from the file stream into our buffer stream
                await _compressor.DecompressToAsync(resultStream, dataStream).ConfigureAwait(false);

                // 5. Return our buffer as a basic stream
                bytes = resultStream.ToArray();
            }

            return bytes;
        }



        #endregion


    }
}
