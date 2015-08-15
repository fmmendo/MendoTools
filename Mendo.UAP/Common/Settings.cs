﻿using Mendo.UAP.IO;
using Mendo.UAP.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mendo.UAP.Common
{
    /// <summary>
    /// Describes the location of Setting
    /// </summary>
    public enum SettingsLocation
    {
        Local = 0,
        Roaming = 1
    }

    /// <summary>
    /// A class that simplifies the management of settings stored in the 
    /// currents application's Local & Roaming ApplicationDataContainers
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Returns the Local ApplicationDataContainer
        /// </summary>
        public static ApplicationDataContainer LocalSettings
        {
            get { return Windows.Storage.ApplicationData.Current.LocalSettings; }
        }

        /// <summary>
        /// Returns the Roaming ApplicationDataContainer
        /// </summary>
        public static ApplicationDataContainer RoamingSettings
        {
            get { return Windows.Storage.ApplicationData.Current.RoamingSettings; }
        }

        /// <summary>
        /// Returns the Appropriate ApplicationDataContainer for the given SettingsLocation
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static ApplicationDataContainer GetContainer(SettingsLocation location)
        {
            if (location == SettingsLocation.Local)
                return LocalSettings;
            else
                return RoamingSettings;
        }



        /// <summary>
        /// Creates a setting. If it already exists, no changes to the existing setting value is made
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="location"></param>
        public static void CreateDefault(String key, object value, SettingsLocation location)
        {
            Set(key, value, location, false);
        }

        /// <summary>
        /// Returns whether or not a key already exists in a given SettingsLocation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Boolean Exists(String key, SettingsLocation location)
        {
            return GetContainer(location).Values.ContainsKey(key);
        }

        /// <summary>
        /// Attempts to remove a key, and returns whether the operation was successful or not
        /// </summary>
        /// <param name="key"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Boolean Remove(String key, SettingsLocation location)
        {
            if (Exists(key, location))
            {
                GetContainer(location).Values.Remove(key);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static object Get(String key, SettingsLocation location)
        {
            if (Exists(key, location))
                return (object)GetContainer(location).Values[key];
            else
                return null;
        }

        public static T Get<T>(String key, SettingsLocation location, T defaultValue = default(T))
        {
            if (Exists(key, location))
                return (T)GetContainer(location).Values[key];
            else
            {
                Set(key, defaultValue, location);
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets the value of a setting in a SettingsLocation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="location"></param>
        /// <param name="overwrite">If true, setting is always set. If false, setting is only set if the key does not already exist</param>
        /// <returns>If true, setting was set. If false, key already existed and no changes were made</returns>
        public static Boolean Set(String key, object value, SettingsLocation location, bool overwrite = true)
        {
            try
            {
                var values = GetContainer(location).Values;
                if (Exists(key, location))
                {
                    if (overwrite)
                        values[key] = value;
                    return overwrite;
                }
                else
                    values.Add(key, value);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }



        #region FileCaching

        /// <summary>
        /// Returns the folder we use for the local settings cache
        /// </summary>
        /// <returns></returns>
        public static Task<StorageFolder> GetLocalSettingsCacheFolderAsync()
        {
            return ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("SettingsCache", CreationCollisionOption.OpenIfExists).AsTask();
        }

        public static async Task<Boolean> DeleteFileAsync(String key)
        {
            try
            {
                var folder = await GetLocalSettingsCacheFolderAsync().ConfigureAwait(false);
                await Task.Run(async () =>
                {
                    var file = await folder.TryGetItemAsync(key).AsTask().ConfigureAwait(false) as StorageFile;
                    if (file != null)
                        await file.DeleteAsync().AsTask().ConfigureAwait(false);
                });
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Saves the string data to a file named after the given key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="location"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static async Task<Boolean> SetSerializedAsync<T>(String key, String data)
        {
            try
            {
                var folder = await GetLocalSettingsCacheFolderAsync().ConfigureAwait(false);
                await Task.Run(async () =>
                {
                    var file = await folder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);
                    await FileIO.WriteTextAsync(file, data).AsTask().ConfigureAwait(false);
                });
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads a file in the local settings cache
        /// with the matching key file name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<String> GetSerializedAsync(String key)
        {
            String result = null;

            try
            {
                var folder = await GetLocalSettingsCacheFolderAsync().ConfigureAwait(false);
                await Task.Run(async () =>
                {
                    var file = await folder.TryGetItemAsync(key).AsTask().ConfigureAwait(false) as StorageFile;
                    if (file != null)
                        result = await FileIO.ReadTextAsync(file).AsTask().ConfigureAwait(false);
                });
            }
            catch (Exception ex)
            {
                return result;
            }

            return result;
        }

        /// <summary>
        /// Saves the given serialisble-value to a file named 
        /// after the key in the local settings cache folder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="location"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static async Task<Boolean> SetSerializedAsync<T>(String key, T value, ISerializer serializer)
        {
            try
            {
                var folder = await GetLocalSettingsCacheFolderAsync().ConfigureAwait(false);
                await Task.Run(async () =>
                {
                    var file = await folder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);

                    if (serializer.SupportedModes() == SerializationMode.String)
                    {
                        await Files.WriteSerializedStringAsync(file, value, serializer).ConfigureAwait(false);
                    }
                    else if (serializer.SupportedModes() == SerializationMode.Stream)
                    {
                        using (var stream = await file.OpenStreamForWriteAsync())
                        {
                            await serializer.SerializeStreamAsync<T>(value, stream).ConfigureAwait(false);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }



        /// <summary>
        /// Deserializes a file in the local settings cache
        /// with the matching key file name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<T> GetSerializedAsync<T>(String key, ISerializer serializer)
        {
            T result = default(T);

            try
            {
                var folder = await GetLocalSettingsCacheFolderAsync().ConfigureAwait(false);
                await Task.Run(async () =>
                {
                    var file = await folder.TryGetItemAsync(key).AsTask().ConfigureAwait(false) as StorageFile;

                    if (file != null)
                    {

                        if (serializer.SupportedModes() == SerializationMode.String)
                        {
                            result = await Files.ReadSerializedStringAsync<T>(file, serializer).ConfigureAwait(false);
                        }
                        else if (serializer.SupportedModes() == SerializationMode.Stream)
                        {
                            using (var stream = await file.OpenStreamForReadAsync())
                            {
                                result = await serializer.DeserializeStreamAsync<T>(stream).ConfigureAwait(false);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return result;
            }

            return result;
        }

        #endregion


    }
}