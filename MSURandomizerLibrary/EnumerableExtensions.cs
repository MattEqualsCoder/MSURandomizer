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
    
    public static List<T> Shuffle<T>(this IEnumerable<T> source, Random rnd)
    {
        var copy = new List<T>(source);
        var n = copy.Count;
        while ((n -= 1) > 0)
        {
            var k = rnd.Next(n + 1);
            (copy[n], copy[k]) = (copy[k], copy[n]);
        }
        return copy;
    }
}