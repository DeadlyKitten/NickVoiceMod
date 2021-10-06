using System;
using System.Collections.Generic;
using System.Linq;

namespace VoiceMod.Extensions
{
    static class IEnumerableExtensions
    {
        public static T GetRandomItem<T>(this IEnumerable<T> items, Func<T, float> weightKey)
        {
            var totalWeight = items.Sum(x => weightKey(x));
            var randomWeightedIndex = UnityEngine.Random.Range(0, totalWeight);
            var itemWeightedIndex = 0f;
            foreach (var item in items)
            {
                itemWeightedIndex += weightKey(item);
                if (randomWeightedIndex < itemWeightedIndex)
                    return item;
            }
            return default;
        }
    }
}
