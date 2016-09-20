using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mendo.UAP.Extensions
{
    public static class GenericExtensions
    {
        /// <summary>
        /// Returns whether or not the input is null or equal to the default
        /// value for the type if non-nullable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Boolean IsNullOrDefault<T>(this T obj)
        {
            if (obj == null)
                return true;

            try
            {
                return (EqualityComparer<T>.Default.Equals(obj, default(T)));
            }
            catch (NullReferenceException)
            {
                // Is Null
                return true;
            }
            catch (ArgumentNullException)
            {
                return true;
            }
        }

        public static bool AreAnyNull(params object[] items)
        {
            return items.Any(i => i == null);
        }
    }
}
