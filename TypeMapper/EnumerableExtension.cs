using System;
using System.Collections.Generic;

namespace LINQBridge.TypeMapper
{
    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> @t, Action<T> action) where T : class
        {
            foreach (var element in @t)
            {
                var el = element; //Avoiding enclosures
                action(el);
            }

        }
    }
}
