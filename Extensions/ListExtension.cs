using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.Characters.Extensions
{
    internal static class ListExtension
    {
        public static bool ContainsAny<T>(this IEnumerable<T> sequence, params T[] matches) => matches.Any(value => sequence.Contains(value));

        public static bool ContainsAll<T>(this IEnumerable<T> sequence, params T[] matches) => matches.All(value => sequence.Contains(value));
    }
}
