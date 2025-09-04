using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ReplayFixer.Library.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> TakeAndRemove<T>(this List<T> array, int count)
        {
            foreach (var n in array.Take(count))
                yield return n;
            array.RemoveRange(0, count);
        }
        public static IEnumerable<T> DequeueChunk<T>(this Queue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                yield return queue.Dequeue();
            }
        }
        public static IEnumerable<byte> RemoveByteNulls(this IEnumerable<byte> list)
        {
            return list.Select(x => x).Where(x => x != 0);
        }

        public static IEnumerable<IList<T>> SplitBy<T>(this IEnumerable<T> sequence, Predicate<T> matchFilter)
        {
            List<T> list = new List<T>();
            foreach (T item in sequence)
            {
                if (matchFilter(item))
                {
                    yield return list;
                    list = new List<T>();
                }
                else
                {
                    list.Add(item);
                }
            }
            if (list.Any()) yield return list;
        }
        public static T[] Subset<T>(this T[] array, int start, int count)
        {
            T[] result = new T[count];
            Array.Copy(array, start, result, 0, count);
            return result;
        }
    }
}
