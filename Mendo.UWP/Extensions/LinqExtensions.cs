using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mendo.UWP.Extensions
{
    public static class LinqExtensions
    {
        public static PropertyInfo GetProperty(this Type type, string propertyName)
        {
            return type.GetRuntimeProperties().FirstOrDefault(m => m.Name.Equals(propertyName));
        }

        public static IEnumerable<MethodInfo> GetMethods(this Type type)
        {
            return type.GetRuntimeMethods();
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderBy");
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderByDescending");
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenBy");
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenByDescending");
        }

        static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods()
                                             .Single(method => method.Name == methodName
                                                     && method.IsGenericMethodDefinition
                                                     && method.GetGenericArguments().Length == 2
                                                     && method.GetParameters().Length == 2)
                                             .MakeGenericMethod(typeof(T), type)
                                             .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                    yield return element;
            }
        }

        public static void RemoveLast<T>(this IList<T> source)
        {
            source.RemoveAt(source.Count - 1);
        }

        /// <summary>
        /// Runs an action on all items of an IEnumerable. Causes the Input IEnumerable to evaluate itself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">IEnumerable to be evaluated and processed</param>
        /// <param name="action"></param>
        public static IEnumerable<T> DoImmediate<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var list = source as IList<T>;
            if (list != null)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                    action(list[i]);
            }
            else
            {
                foreach (var value in source)
                    action(value);
            }

            return source;
        }

        /// <summary>
        /// Runs an Asynchronous task on all items of an IEnumerable. Will cause evaluation of the input IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">IEnumerable to be evaluated and processed</param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> DoImmediateAsync<T>(this IEnumerable<T> source, Func<T, Task> func, bool configureAwaiter = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            // perf optimization. try to not use enumerator if possible
            var list = source as IList<T>;
            if (list != null)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    await func(list[i]).ConfigureAwait(configureAwaiter);
                }
            }
            else
            {
                foreach (var value in source)
                {
                    await func(value).ConfigureAwait(configureAwaiter);
                }
            }

            return source;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> list)
        {
            if (list == null)
                return true;

            if (list is ICollection<T>)
                return ((ICollection<T>)list).Count == 0;
            else
                return !list.Any();
        }

        /// <summary>
        /// This code isn't used for anything. I just find it useful ~
        /// sorter.AddSort("NAME", m => m.Name);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="func"></param>
        public static void AddSort<T>(string name, Expression<Func<T, object>> func)
        {
            string fieldName = (func.Body as MemberExpression).Member.Name;
        }
    }
}
