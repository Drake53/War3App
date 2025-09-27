using System;
using System.Collections.Generic;
using System.Linq;

namespace War3App.MapAdapter.Extensions
{
    public static class EnumerableExtensions
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            bool ascending)
        {
            return ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            bool ascending)
        {
            return ascending
                ? source.ThenBy(keySelector)
                : source.ThenByDescending(keySelector);
        }
    }
}