using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Company.Common.Extensions
{
    public static class Collection
    {
        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var itemsToDelete = new Collection<T>();
            foreach (var item in collection.Where(predicate))
            {
                itemsToDelete.Add(item);
            }

            itemsToDelete.ForEach(i => collection.Remove(i));
        }

        public static void ForEach<T>(this ICollection<T> collection, Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in collection)
            {
                action(item);
            }
        }

        public static void RemoveAt<T>(this ICollection<T> collection, int index)
        {
            if (index < 0 || index >= collection.Count)
                throw new IndexOutOfRangeException();

            collection.Remove(collection.ElementAt(index));
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}