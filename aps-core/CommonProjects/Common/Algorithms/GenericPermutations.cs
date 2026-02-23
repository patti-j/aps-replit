using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PT.Common.Algorithms;

//TODO: MultiThreaded Permutations algorithm
/// <summary>
/// EO: 2016-04-14
/// Generator of all permutations of an array of anything.
/// Base on Heap's Algorithm. See: https://en.wikipedia.org/wiki/Heap%27s_algorithm#cite_note-3
/// </summary>
public static class GenericPermutations
{
    /// <summary>
    /// Heap's algorithm to find all permutations. Non recursive, more efficient.
    /// </summary>
    /// <param name="a_items">Items to permute in each possible ways</param>
    /// <param name="a_funcExecuteAndTellIfShouldStop"></param>
    /// <returns>Return true if cancelled</returns>
    public static bool ForAllPermutation<T>(T[] a_items, Func<T[], bool> a_funcExecuteAndTellIfShouldStop)
    {
        int countOfItem = a_items.Length;

        if (countOfItem <= 1)
        {
            return a_funcExecuteAndTellIfShouldStop(a_items);
        }

        int[] indexes = new int[countOfItem];
        for (int i = 0; i < countOfItem; i++)
        {
            indexes[i] = 0;
        }

        if (a_funcExecuteAndTellIfShouldStop(a_items))
        {
            return true;
        }

        for (int i = 1; i < countOfItem;)
        {
            if (indexes[i] < i)
            {
                // On the web there is an implementation with a multiplication which should be less efficient.
                if ((i & 1) == 1) // if (i % 2 == 1)  ... more efficient ??? At least the same.
                {
                    Swap(ref a_items[i], ref a_items[indexes[i]]);
                }
                else
                {
                    Swap(ref a_items[i], ref a_items[0]);
                }

                if (a_funcExecuteAndTellIfShouldStop(a_items))
                {
                    return true;
                }

                indexes[i]++;
                i = 1;
            }
            else
            {
                indexes[i++] = 0;
            }
        }

        return false;
    }

    /// <summary>
    /// Swap 2 elements of same type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_a"></param>
    /// <param name="a_b"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Swap<T>(ref T a_a, ref T a_b)
    {
        T temp = a_a;
        a_a = a_b;
        a_b = temp;
    }

    /// <summary>
    /// Func to show how to call. It does a little test for an array of 4 items.
    /// </summary>
    public static void Test()
    {
        ForAllPermutation("123".ToCharArray(),
            vals =>
            {
                Console.WriteLine(string.Join("", vals));
                return false;
            });

        int[] values = { 0, 1, 2, 4 };

        Console.WriteLine("Ouellet heap's algorithm implementation");
        ForAllPermutation(values,
            vals =>
            {
                Console.WriteLine(string.Join("", vals));
                return false;
            });


        // Performance Heap's against Linq version : huge differences
        int count = 0;

        values = new int[10];
        for (int n = 0; n < values.Length; n++)
        {
            values[n] = n;
        }

        Stopwatch stopWatch = new ();

        ForAllPermutation(values,
            vals =>
            {
                foreach (int v in vals)
                {
                    count++;
                }

                return false;
            });

        stopWatch.Stop();
        Console.WriteLine($"Ouellet heap's algorithm implementation {count} items in {stopWatch.ElapsedMilliseconds} millisecs");

        count = 0;
        stopWatch.Reset();
        stopWatch.Start();
    }
}