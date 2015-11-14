using System;
using System.Threading.Tasks;

namespace Mendo.UAP.Cache
{
    public interface INetworkCache
    {
        //int GetMaxEntries();

        //Task<CacheResult<string>> GetStringAsync(string uri, TimeSpan? expiry);

        //Task<CacheResult<byte[]>> GetBytesAsync(string uri, TimeSpan? expiry);

        //Task SaveAsync(string Uri, string result);

        //Task SaveAsync(string Uri, byte[] data);

        //Task TrimAsync();

        //Task InitialiseAsync();

        Task ClearCacheAsync();
    }

    /// <summary>
    /// A simple wrapper class for returning
    /// generic cached data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheResult<T>
    {
        public T Result { get; set; }
        public bool Exists { get; set; }
        public bool Expired { get; set; }
    }

    /// <summary>
    /// Dictates how the caching mechanism should operate
    /// </summary>
    public enum CacheMode
    {
        /// <summary>
        /// Do not use the cache
        /// </summary>
        Skip,
        /// <summary>
        /// Returns the cached item immediately, then updates the cache asynchronously regardless of expiry time. If offline, returns cached data.
        /// </summary>
        UpdateAsync,
        /// <summary>
        /// Returns the cached item immediately. If expired, then updates the cache asynchronously. If offline, returns cached data.
        /// </summary>
        UpdateAsyncIfExpired,
        /// <summary>
        /// Updates the cache first and then returns the result regardless of expiry time. If offline, returns cached data.
        /// </summary>
        UpdateImmediately,
        /// <summary>
        /// If expired, updates the cache first and then returns the result. Otherwise returns the cached item immediately. If offline, returns cached data.
        /// </summary>
        UpdateImmediatelyIfExpired
    }
}
