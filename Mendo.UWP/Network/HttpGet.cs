using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Web.Http.Headers;
using System.Threading;
using System.IO;
using Mendo.UWP.Serialization;
using Mendo.UWP.Extensions;

namespace Mendo.UWP.Network
{
    public static partial class Http
    {
        /* 
        /*
        /* This file contains all the methods related to HttpGet requests 
        /*
        */

        #region GET

        /// <summary>
        /// Attempts to download and parse Serialized data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Uri"></param>
        /// <param name="serialiser"></param>
        /// <param name="mode"></param>
        /// <param name="cacheExpiry"></param>
        /// <param name="cacheFolderName"></param>
        /// <param name="client"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<HttpResult<T>> GetAsync<T>(
            String Uri,
            ISerializer serializer,
            CacheMode mode = CacheMode.Skip,
            TimeSpan? cacheExpiry = null,
            HttpClient client = null,
            INetworkCache cacheOverride = null)
        {
            // 1. Download the data
            var str = await GetAsync(Uri, mode, cacheExpiry, client, cacheOverride).ConfigureAwait(false);

            // 2. Create the shell return object based on the downloaded data result
            var result = HttpExtensions.CreateBasedOn<T>(str);

            if (str.Success)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        using (Stream stream = str.Content.AsStream())
                        {
                            // 3. Attempt to parse it
                            result.Content = serializer.Deserialize<T>(stream);
                        }
                    }).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Exception = ex;
                }
            }

            return result;
        }


        /// <summary>
        /// Returns a string of HTTP data from the internet. If the data already exists in the cache, cached data is returned.
        /// If the cached data has expired, the cached data will be returned immediately, and then the cached data updated
        /// asynchronously.
        /// </summary>
        /// <param name="Uri"></param>
        /// <param name="cacheResult"></param>
        /// <param name="cacheExpiry"></param>
        /// <returns></returns>
        public static async Task<HttpResult<String>> GetStringAsync(
            String Uri,
            CacheMode mode = CacheMode.Skip,
            TimeSpan? cacheExpiry = null,
            HttpClient client = null,
            INetworkCache cacheOverride = null)
        {
            HttpResult<Byte[]> data = await GetAsync(Uri, mode, cacheExpiry, client, cacheOverride).ConfigureAwait(false);

            HttpResult<String> result = data.CreateBasedOn<String>();
            if (result.Success)
            {
                try
                {
                    result.Content = await DataExtensions.AsStringAsync(data.Content).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    result.Success = false;
                }
            }

            return result;
        }

        public static async Task<HttpResult<Byte[]>> GetAsync(
              String Uri,
              CacheMode mode = CacheMode.Skip,
              TimeSpan? cacheExpiry = null,
              HttpClient client = null,
              INetworkCache cacheOverride = null)
        {
            EnsureInitialised();
            HttpResult<Byte[]> result = new HttpResult<Byte[]>
            {
                OriginalUri = Uri,
                WasConnectionAvaliable = IsConnectionAvailable,
                Success = false,
                FromCache = false
            };

            // 2.  Attempt to retrieve cached data
            INetworkCache cache = (cacheOverride == null) ? Http.DefaultCache : cacheOverride;
            CacheResult<Byte[]> cachedata = null;
            if (mode != CacheMode.Skip)
            {
                if (cache == null)
                    throw new ArgumentNullException("Http.DefaultCache", "The default HTTP cache has not been set. Using the Http cache will fail");

                cachedata = await cache.GetBytesAsync(Uri, cacheExpiry).ConfigureAwait(false);
                if (cachedata.Exists)
                {
                    result.Success = true;
                    result.FromCache = true;
                    result.Content = cachedata.Result;
                }
            }


            if (IsConnectionAvailable)
            {
                try
                {
                    bool _isPrivateClient = false;
                    if (client == null)
                    {
                        _isPrivateClient = true;
                        client = CreateOptimisedClient();
                    }

                    // 3. If connection is available, and there is no cached data, 
                    //    or the cached data exists BUT the cache is set to update immediately,
                    //    or the cached data exists, is expired, and is set to update immediately if expired
                    //    for the data to be downloaded
                    if (result.Content == null
                        || mode == CacheMode.UpdateImmediately
                        || (mode == CacheMode.UpdateImmediatelyIfExpired && cachedata.Exists && cachedata.Expired))
                    {
                        // 3.0. Configure our timeout settings for the request
                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS));

                        // 3.1. We get the result as a plain HttpResponseMessage first
                        using (HttpResponseMessage response = await client.GetAsync(new Uri(Uri, UriKind.Absolute)).AsTask(cts.Token).ConfigureAwait(false))
                        {
                            // 3.2. We extract some meta-data we need to return to our result
                            //result.RequestMessage = response.RequestMessage;
                            result.StatusCode = response.StatusCode;

                            // 3.3 Record the request headers
                            result.ResponseHeaders = response.Headers;

                            try
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    throw new InvalidDataException($"Status code was invalid ({response.StatusCode})");
                                }

                                result.Content = (await response.Content.ReadAsBufferAsync().AsTask().ConfigureAwait(false)).ToArray();
                                result.Success = true;
                            }
                            catch (Exception ex)
                            {
                                result.Exception = ex;
                            }

                            result.FromCache = false;
                        }
                    }
                    else
                    {
                        // 4. Update the cache asynchronously if required
                        if (mode == CacheMode.UpdateAsync || (cachedata.Expired && mode == CacheMode.UpdateAsyncIfExpired))
                        {
#pragma warning disable CS4014
                            Task.Run(async () =>
#pragma warning restore CS4014
                            {
                                try
                                {
                                    HttpClient cacheClient = (_isPrivateClient) ? Http.CreateNonCachingClient() : client;

                                    using (HttpResponseMessage res = await cacheClient.GetAsync(new System.Uri(Uri, UriKind.Absolute)).AsTask().ConfigureAwait(false))
                                    {
                                        if (res.IsSuccessStatusCode)
                                        {
                                            var buffer = await res.Content.ReadAsBufferAsync().AsTask().ConfigureAwait(false);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                            cache.SaveAsync(Uri, buffer.ToArray());
#pragma warning restore CS4014
                                        }
                                    }

                                    if (_isPrivateClient)
                                        cacheClient.Dispose();
                                }
                                catch (Exception ex)
                                {

                                }
                            });
                        }
                    }

                    if (_isPrivateClient)
                        client.Dispose();

                    if (result.Content != null && mode != CacheMode.Skip)
                    {
                        try
                        {
                            if (mode == CacheMode.UpdateImmediately || ((cachedata == null || cachedata.Expired || !cachedata.Exists) && mode == CacheMode.UpdateImmediatelyIfExpired))
                                await cache.SaveAsync(Uri, result.Content).ConfigureAwait(false);
                            else if (mode == CacheMode.UpdateAsync || ((cachedata == null || cachedata.Expired || !cachedata.Exists) && mode == CacheMode.UpdateAsyncIfExpired))
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                cache.SaveAsync(Uri, result.Content).ConfigureAwait(false);
#pragma warning restore CS4014 
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            // If we're told it's unauthorized access we don't want to fail the whole feed, the cache failed but the get didn't
                        }
                    }



                }
                catch (TaskCanceledException tce)
                {
                    // We probably timed out here - BUT THIS SHOULD NOT BE REQUIRED?
                    if (cachedata != null && cachedata.Exists)
                    {
                        result.FromCache = true;
                        result.WasConnectionAvaliable = false;
                        result.Content = cachedata.Result;
                    }
                    else
                    {
                        result.Exception = tce;
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;

                    var details = GetConnectionExceptionDetails(ex);
                    if (!String.IsNullOrWhiteSpace(details) && cachedata != null && cachedata.Exists)
                    {
                        result.FromCache = true;
                        result.WasConnectionAvaliable = false;
                        result.Content = cachedata.Result;
                    }
                    else
                    {
                        result.Success = false;
                    }
                }
            }

            // 5 - Return our final result
            return result;
        }

        #endregion
    }
}
