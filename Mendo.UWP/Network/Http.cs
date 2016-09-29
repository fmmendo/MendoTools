using Mendo.UWP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Networking.Connectivity;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Mendo.UWP.Network
{
    /// <summary>
    /// Modern HTTP Client helper using Windows.Web.Http
    /// Default cache mode is set to Skip
    /// </summary>
    public static partial class Http
    {
        private static Boolean _isConnectionAvailable = false;
        private static Boolean _initialised = false;

        #region Properties

        /// <summary>
        /// Default HTTP request timeout in seconds - 30 Seconds
        /// </summary>
        public const Int32 DEFAULT_TIMEOUT_SECONDS = 30;

        /// <summary>
        /// Default Cache expiry in Days - 5 minutes
        /// </summary>
        public static TimeSpan DEFAULT_CACHE_EXPIRY = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Returns whether or not we currently have an active internet connection
        /// </summary>
        public static Boolean IsConnectionAvailable
        {
            get
            {
                UpdateNetworkStatus();
                return _isConnectionAvailable;
            }
        }

        /// <summary>
        /// Returns whether or not we should be restricting bandwidth. 
        /// This occurs if roaming, or if the GalleryData limit is being approached
        /// </summary>
        public static Boolean IsBandwidthRestricted { get; private set; }

        public static INetworkCache DefaultCache { get; set; }

        /// <summary>
        /// A list of HResult Error codes with friendlier identifiers for typical connection failed exceptions
        /// </summary>
        public static Dictionary<String, String> ConnectionFailedExceptions = new Dictionary<string, string>
        {
            { "0x80072F30", "ERROR_WINHTTP_NO_CM_CONNECTION" },
            { "0x80072EFELL", "WININET_E_CONNECTION_ABORTED" },
            { "0x80072EFDL", "WININET_E_CANNOT_CONNECT" },
            { "0x80072EE7", "INVALID_IP_ADDRESS" }
        };

        #endregion

        public static void EnsureInitialised()
        {
            if (!_initialised)
            {
                _initialised = true;
                NetworkInformation.NetworkStatusChanged += UpdateNetworkStatus;
                UpdateNetworkStatus();
            }
        }

        /// <summary>
        /// You typically shouldn't need to manually call this, but public just in case 
        /// race conditions are met with two separate classes hooking up to the 
        /// NetworkStatusChanged event and relying on IsConnectionAvailable
        /// </summary>
        /// <param name="sender"></param>
        public static void UpdateNetworkStatus(object sender = null)
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();

            if (profile == null)
            {
                _isConnectionAvailable = false;
                return;
            }

            if (profile.GetConnectionCost().ApproachingDataLimit
                || profile.GetConnectionCost().Roaming
                || profile.GetConnectionCost().OverDataLimit)
                IsBandwidthRestricted = true;

            var lvl = profile.GetNetworkConnectivityLevel();

            _isConnectionAvailable = (lvl == NetworkConnectivityLevel.InternetAccess);

        }

        /// <summary>
        /// Create a new HttpClient with optimal caching settings and gzip / deflate support
        /// </summary>
        /// <returns></returns>
        public static HttpClient CreateOptimisedClient()
        {
            // We can optimize caching even further by using both our local cache AND WinINET's
            // cache in tandem with each other. This also enforces cache busting
            var RootFilter = new HttpBaseProtocolFilter();
            RootFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;

            if (DeviceInformation.Instance.SupportsSDK10586)
                RootFilter.MaxConnectionsPerServer = 15;

            var client = new HttpClient(RootFilter);
            client.EnableGzipDeflate();

            return client;
        }

        /// <summary>
        /// Returns a new HttpClient that does not write to the WinINET cache, and has Gzip/Deflate supports,
        /// and cache-control header set to prevent caching as 'private, max-age=0, no-cache'
        /// </summary>
        /// <returns></returns>
        public static HttpClient CreateNonCachingClient()
        {
            var RootFilter = new HttpBaseProtocolFilter();
            RootFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            RootFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;

            var client = new HttpClient(RootFilter);

            client.EnableGzipDeflate();
            client.DefaultRequestHeaders.CacheControl.TryParseAdd("private");
            client.DefaultRequestHeaders.CacheControl.TryParseAdd("max-age=0");
            client.DefaultRequestHeaders.CacheControl.TryParseAdd("no-cache");
            return client;
        }

        /// <summary>
        /// Attempts too add gzip and deflate entries to the default AcceptEncoding header
        /// </summary>
        /// <param name="client"></param>
        public static void EnableGzipDeflate(this HttpClient client)
        {
            client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip");
            client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("deflate");
            client.DefaultRequestHeaders.TryAppendWithoutValidation("vary", "Accept-Encoding");
        }

        /// <summary>
        /// Returns a friendly identifier for exceptions if a known connection failure is recognized
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static String GetConnectionExceptionDetails(Exception ex)
        {
            String details = null;

            foreach (var entry in ConnectionFailedExceptions)
            {
                if (ex.Message.IndexOf(entry.Key, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    details = entry.Value;
                    break;
                }
            }

            return details;
        }

        /// <summary>
        /// Returns a dictionary of cookies associated with a certai uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IDictionary<String, String> GetCookiesForUri(Uri uri)
        {
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            HttpCookieManager manager = filter.CookieManager;
            HttpCookieCollection cookies = manager.GetCookies(uri);

            IDictionary<String, String> cookieDict = new Dictionary<string, string>();
            foreach (HttpCookie cookie in cookies.Where(c => cookieDict.All(co => co.Key != c.Name)))
            {
                cookieDict.Add(cookie.Name, cookie.Value);
            }

            return cookieDict;
        }
    }
}
