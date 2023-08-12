using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Tools
{
    /// <summary>
    /// Provides extension methods for IList<T> to offer additional utility functions such as Swap, Shuffle, and GetRandomItem.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Swaps the elements at the given indices within the list.
        /// </summary>
        /// <param name="list">The list on which the operation will be performed.</param>
        /// <param name="indexA">The index of the first element to be swapped.</param>
        /// <param name="indexB">The index of the second element to be swapped.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            var temporary = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temporary;
        }

        /// <summary>
        /// Shuffles the elements of the list using the Fisher-Yates algorithm.
        /// </summary>
        /// <param name="list">The list to be shuffled.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list.Swap(i, Random.Range(i, list.Count));
            }
        }

        /// <summary>
        /// Returns a random item from the list.
        /// </summary>
        /// <param name="list">The list from which a random item will be selected.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>A random item from the list.</returns>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list.Count == 0)
                throw new System.ArgumentException("List is empty. Cannot get a random item from an empty list.");

            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Determines whether a list contains all of the elements of a specified sub-list.
        /// </summary>
        /// <param name="list">The main list.</param>
        /// <param name="subList">The sub-list of elements to check against.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>Returns true if the main list contains all the elements in the sub-list, otherwise false.</returns>
        public static bool ContainsRange<T>(this IList<T> list, IList<T> subList)
        {
            foreach (T item in subList)
            {
                if (!list.Contains(item))
                    return false;
            }
            return true;
        }
    }
}
