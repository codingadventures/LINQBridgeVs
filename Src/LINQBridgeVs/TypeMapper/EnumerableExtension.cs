using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LINQBridgeVs.TypeMapper
{
    public static class EnumerableExtension
    {
        [DebuggerStepThrough]
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
