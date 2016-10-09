using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mendo.UWP.Extensions
{
    public static class CollectionExtensions
    {
        public static string Concatenate(this List<KeyValuePair<string, string>> collection, string separator, string spacer)
        {
            var sb = new StringBuilder();

            var total = collection.Count;
            var count = 0;

            foreach (var item in collection)
            {
                sb.Append(item.Key);
                sb.Append(separator);
                sb.Append(item.Value);

                count++;
                if (count < total)
                {
                    sb.Append(spacer);
                }
            }

            return sb.ToString();
        }

        public static string ToQueryString(this Dictionary<string, string> collection)
        {
            var sb = new StringBuilder();
            if (collection.Count > 0)
            {
                sb.Append("?");
            }

            var count = 0;
            foreach (var key in collection.Keys)
            {
                sb.AppendFormat("{0}={1}", key, collection[key].UrlEncode());
                count++;

                if (count >= collection.Count)
                {
                    continue;
                }
                sb.Append("&");
            }
            return sb.ToString();
        }
    }
}
