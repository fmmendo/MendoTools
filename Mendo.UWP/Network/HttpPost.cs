using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Web.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.IO;
using Windows.Web.Http.Filters;
using System.Collections.Generic;
using System.Diagnostics;
using Mendo.UWP.Serialization;
using Mendo.UWP.Extensions;

namespace Mendo.UWP.Network
{
    public static partial class Http
    {
        /* 
        /*
        /* This file contains all the methods related to HttpPost requests 
        /*
        */

        #region POST

        public static Task<HttpResult<T2>> PostAsync<T1, T2>(string url, T1 data)
        {
            return Task.Run(() =>
            {
                return PostRawAsync<T2>(url, Json.Instance.Serialize(data));
            });
        }

        /// <summary>
        /// Makes a post request to the server (used for adding new items and updating)
        /// </summary>
        /// <param name="urlQuery">The url that you want to post to</param>
        /// <param name="rawContent">The content that you wish to be posted with the request. Pass in null if you wish to leave content clear</param>
        /// <returns>A string representation of the response from the server</returns>
        public static async Task<HttpResult<T>> PostRawAsync<T>(string url, string rawContent)
        {
            HttpResult<T> result = new HttpResult<T>
            {
                OriginalUri = url,
                WasConnectionAvaliable = IsConnectionAvailable,
                Success = false,
                FromCache = false
            };

            if (IsConnectionAvailable)
            {
                using (HttpClient client = CreateNonCachingClient())
                using (HttpStreamContent content = new HttpStreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rawContent)).AsInputStream()))
                {
                    HttpResponseMessage response;

                    try
                    {
                        response = await client.PostAsync(new Uri(url), content).AsTask().ConfigureAwait(false);
                    }
                    catch (Exception exn)
                    {
                        return null;
                    }

                    if (response.Content.Headers.ContentLength == 0)
                    {
                        return null;
                    }

                    byte[] responseByteArray = (await response.Content.ReadAsBufferAsync().AsTask().ConfigureAwait(false)).ToArray();

                    using (Stream responseStream = responseByteArray.AsStream())
                    {
                        ISerializer serializer = Json.Instance;
                        result.Content = await serializer.DeserializeAsync<T>(responseStream).ConfigureAwait(false);
                    }

                    result.ResponseHeaders = response.Headers;
                }
            }

            return result;
        }

        /// <summary>
        /// Makes a post request to the server (used for adding new items and updating)
        /// </summary>
        /// <param name="urlQuery">The url that you want to post to</param>
        /// <param name="formContent">The content that you wish to be posted with the request. Pass in null if you wish to leave content clear</param>
        /// <returns>A string representation of the response from the server</returns>
        public static async Task<HttpResult<T>> Post<T>(string url, Dictionary<string, string> formContent, HttpClient client = null)
        {
            HttpResult<T> result = new HttpResult<T>
            {
                OriginalUri = url,
                WasConnectionAvaliable = IsConnectionAvailable,
                Success = false,
                FromCache = false
            };

            if (IsConnectionAvailable)
            {
                if (client == null)
                {
                    client = CreateNonCachingClient();
                }

                using (HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(formContent))
                {
                    Uri uri = new Uri(url);

                    HttpResponseMessage response;

                    try
                    {
                        response = await client.PostAsync(uri, content);
                    }
                    catch (Exception exn)
                    {
                        result.Exception = exn;
                        return result;
                    }

                    if (response.Content.Headers.ContentLength == 0)
                    {
                        return result;
                    }

                    byte[] responseByteArray = (await response.Content.ReadAsBufferAsync().AsTask().ConfigureAwait(false)).ToArray();

                    using (Stream responseStream = responseByteArray.AsStream())
                    {
                        ISerializer serializer = Json.Instance;

                        result.Content = await serializer.DeserializeAsync<T>(responseStream).ConfigureAwait(false);
                        result.Success = true;
                    }

                    result.Cookies = GetCookiesForUri(uri);
                    result.ResponseHeaders = response.Headers;
                }
            }

            return result;
        }

        /// <summary>
        /// Makes a post request to the server (used for adding new items and updating)
        /// </summary>
        /// <param name="urlQuery">The url that you want to post to</param>
        /// <param name="formContent">The content that you wish to be posted with the request. Pass in null if you wish to leave content clear</param>
        /// <returns>A string representation of the response from the server</returns>
        public static async Task<HttpResult<string>> PostForString(string url, Dictionary<string, string> formContent, HttpClient client = null)
        {
            HttpResult<string> result = new HttpResult<string>
            {
                OriginalUri = url,
                WasConnectionAvaliable = IsConnectionAvailable,
                Success = false,
                FromCache = false
            };

            if (IsConnectionAvailable)
            {
                if (client == null)
                {
                    client = CreateNonCachingClient();
                }

                using (HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(formContent))
                {
                    Uri uri = new Uri(url);

                    HttpResponseMessage response;

                    try
                    {
                        response = await client.PostAsync(uri, content);
                    }
                    catch (Exception exn)
                    {
                        return result;
                    }

                    if (response.Content.Headers.ContentLength == 0)
                    {
                        return result;
                    }

                    result.Content = await response.Content.ReadAsStringAsync().AsTask().ConfigureAwait(false);
                    result.Success = true;

                    result.Cookies = GetCookiesForUri(uri);
                    result.ResponseHeaders = response.Headers;
                }
            }

            return result;
        }

        #endregion


    }
}
