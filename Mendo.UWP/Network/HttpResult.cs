using System;
using System.Collections.Generic;
using Windows.Web.Http;

namespace Mendo.UWP.Network
{
    /// <summary>
    /// Basic wrapper class for returning results of HttpCalls. 
    /// This version returns only hte status code - if returning data
    /// use the generic HttpResult[T] instead.
    /// </summary>
    public class HttpResult
    {
        /// <summary>
        /// If true, internet connection was available when the call was made
        /// </summary>
        public Boolean WasConnectionAvaliable { get; set; }

        /// <summary>
        /// If true, data has come from the local cache
        /// </summary>
        public Boolean FromCache { get; set; }

        /// <summary>
        /// If true, data came from cache, but an HTTP request was still done 
        /// to update the cache.
        /// </summary>
        public bool CacheExpired { get; set; }

        /// <summary>
        /// If true, the Http call completed successfully, and any additional 
        /// parsing or type conversions succeeded
        /// </summary>
        public Boolean Success { get; set; }
        
        /// <summary>
        /// If any exception is raised during the call or parsing, it will be
        /// returned here
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// The original request message used to create this call. Different
        /// implementation wrappers that are creating this result that DO NOT
        /// use <see cref="Windows.Web.Http.HttpClient"/> underneath may not
        /// return a full HttpRequestMessage. But when implementing manually, 
        /// do return AT LEAST the original RequestUri - helpful for debugging
        /// purposes.
        /// </summary>
        //public HttpRequestMessage RequestMessage { get; set; }

        /// <summary>
        /// A string representation of the Uri originally passed to HttpClient
        /// (or whatever makes the request in manual implementations)
        /// </summary>
        public String OriginalUri { get; set; }

        /// <summary>
        /// The HttpStatusCode returned by the HttpClient whilst making the call.
        /// HttpNET DOES NOT use this property. Only the new Http class using the
        /// <see cref="Windows.Web.Http.HttpClient"/> supports this. When 
        /// implementing your own wrapper that returns HttpResult, try to at least
        /// return 404 or 0 if your underlying request code does not return a 
        /// StatusCode to you if some translatable form
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Returns the headers returned by the result of the HTTP request.
        /// If from cache, this is expected to be null
        /// </summary>
        public IDictionary<String, String> ResponseHeaders { get; set; }

        /// <summary>
        /// Returns the cookies returned by the result of the HTTP request.
        /// </summary>
        public IDictionary<String, String> Cookies { get; set; }
    }

    /// <summary>
    /// A basic generic wrapper class for returning type data
    /// from HTTP calls
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpResult<T> : HttpResult
    {
        public T Content { get; set; }
    }
}
