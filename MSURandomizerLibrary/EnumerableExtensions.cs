using System;
using System.Collections.Generic;
using System.Linq;

namespace MSURandomizerLibrary;

public static class EnumerableExtensions
{
    public static T? Random<T>(this IEnumerable<T> source, Random rnd)
    {
        var list = source.ToList();
        return list.Count == 0 ? default : list.ElementAt(rnd.Next(list.Count));
    }
}