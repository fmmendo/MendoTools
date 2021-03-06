﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Mendo.UWP.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, OrderType orderType)
        {
            if (orderType == OrderType.Ascending)
            {
                return source.OrderBy(keySelector);
            }
            else
            {
                return source.OrderByDescending(keySelector);
            }
        }
    }

    public enum OrderType
    {
        Ascending,
        Descending
    }
}
