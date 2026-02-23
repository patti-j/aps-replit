using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.Common
{
    public static class ListExtentions
    {
        /// <summary>
        /// Method <c>Replace</c> replaces the first element that matches replaceBy
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        /// <param name="replaceBy"><summary>A function that compares list elements to <c>item</c>.
        /// </summary>
        /// <param>The current item from the list</param>
        /// <param>The item to be inserted</param>
        /// <returns><c>True</c> if the current item should be replaced. <c>False</c> otherwise.</returns></param>
        /// <returns><c>True</c> if an item was replaced, <c>False</c> otherwise.</returns>
        public static bool Replace<T>(this List<T> list, T item, Func<T, T, bool>? replaceBy = null)
        {
            if (replaceBy == null)
            {
                replaceBy = (item1, item2) => item1?.Equals(item2) ?? false;
            }
            var idx = list.FindIndex(x => replaceBy(x, item));
            if (idx == -1)
            {
                return false;
            } 
            else
            {
                list[idx] = item;
                return true;
            }
        }

        /// <summary>
        /// Method <c>Synchronize</c> synchronizes the content of one list with the content of another. It does this by adding all elements from <c>desiredList</c> that are not present, and removes all elements that are not.
        /// <br></br>It does not overwrite any items that are present in both lists. This is useful for update many-to-may relationships without breaking Entity Framework Tracking.
        /// </summary>
        /// <param name="listToModify">The that will be changed.</param>
        /// <param name="desiredList">The list to synchronize with.</param>
        /// <param name="compareFunction"><summary>A function that compares list elements to determine if they are equal.
        /// </summary>
        /// <param name="item1">Param 1: The current item</param>
        /// <param name="item2">Param 2: The item to compare</param>
        /// <returns>Should return <c>True</c> if the items are equal. <c>False</c> otherwise.</returns></param>
        /// <returns>A tuple containing all elemnts added, and all elements removed.</returns>
        public static (ICollection<T> added, ICollection<T> removed) Synchronize<T>(this ICollection<T> listToModify, IEnumerable<T> desiredList, Func<T, T, bool>? compareFunction = null)
        {
            if (compareFunction == null)
            {
                compareFunction = (item1, item2) => item1?.Equals(item2) ?? false;
            }
            ICollection<T> added = desiredList.Where(i1 => listToModify.All(i2 => !compareFunction(i1, i2))).ToList();
            ICollection<T> removed = listToModify.Where(i1 => desiredList.All(i2 => !compareFunction(i1, i2))).ToList();
            foreach (var item in added)
            {
                listToModify.Add(item);
            }
            foreach (var item in removed)
            {
                listToModify.Remove(item);
            }

            return (added, removed);
        }

        /// <summary>
        /// Method <c>IsNullOrEmpty</c> checks if the enumerable is null or contains no elements.
        /// </summary>
        /// <param name="list">The list to check.</param>
        /// <returns><c>True</c> if the list is null or contains no elements, <c>False</c> otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || !list.Any();
        }
    }

    
}
