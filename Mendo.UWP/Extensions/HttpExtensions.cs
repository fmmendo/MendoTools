using Mendo.UWP.Network;

namespace Mendo.UWP.Extensions
{
    public static class HttpExtensions
    {
        /// <summary>
        /// Returns a type specific HttpResult based on the input result.
        /// The actual Result property is not parsed or set, you must do this manually.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static HttpResult<TOutput> CreateBasedOn<TOutput>(this HttpResult original)
        {
            return new HttpResult<TOutput>
            {
                WasConnectionAvaliable = original.WasConnectionAvaliable,
                Success = original.Success,
                Exception = original.Exception,
                FromCache = original.FromCache,
                StatusCode = original.StatusCode
            };
        }
    }
}
