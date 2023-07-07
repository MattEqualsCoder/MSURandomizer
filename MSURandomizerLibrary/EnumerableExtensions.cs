using System;
using System.Collections.Generic;
using System.Linq;

namespace MSURandomizerLibrary;

internal static class EnumerableExtensions
{
    public static T? Random<T>(this IEnumerable<T> source, Random rnd)
    {
        var list = source.ToList();
        return list.Any() != true ? default : list.Count == 1 ? list.ElementAt(0) : list.ElementAt(rnd.Next(list.Count));
    }
}