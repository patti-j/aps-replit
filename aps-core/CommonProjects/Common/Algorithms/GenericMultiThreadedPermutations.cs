using System.Runtime.CompilerServices;

namespace PT.Common.Algorithms;

public static class GenericMultiThreadedPermutations
{
    public class PermutationMixOuelletSaniSinghHuttunen<T>
    {
        private readonly long m_indexFirst;
        private readonly long m_indexLastExclusive;
        private readonly T[] m_items;
        private readonly int[] m_lexOrderedList;

        public PermutationMixOuelletSaniSinghHuttunen(T[] a_items, int[] a_lexOrderedList, long a_indexFirst = -1, long a_indexLastExclusive = -1)
        {
            if (a_indexFirst == -1)
            {
                a_indexFirst = 0;
            }

            if (a_indexLastExclusive == -1)
            {
                a_indexLastExclusive = Factorial.GetFactorial(a_items.Length);
            }

            if (a_indexFirst >= a_indexLastExclusive)
            {
                throw new ArgumentException($"{nameof(a_indexFirst)} should be less than {nameof(a_indexLastExclusive)}");
            }

            m_indexFirst = a_indexFirst;
            m_indexLastExclusive = a_indexLastExclusive;
            m_items = a_items;
            m_lexOrderedList = a_lexOrderedList;
        }

        private void ExecuteForEachPermutation(Action<T[]> a_action)
        {
            //Console.WriteLine($"Thread {System.Threading.Thread.CurrentThread.ManagedThreadId} started: {_indexFirst} {_indexLastExclusive}");

            long index = m_indexFirst;

            PermutationOuelletLexico3<T> permutationOuellet = new (m_items);

            permutationOuellet.GetSortedValuesFor(index);
            a_action(permutationOuellet.Result);
            index++;

            T[] values = permutationOuellet.Result;
            while (index < m_indexLastExclusive)
            {
                PermutationSaniSinghHuttunen<T>.NextPermutation(values, m_lexOrderedList);
                a_action(values);
                index++;
            }

            //Console.WriteLine($"Thread {System.Threading.Thread.CurrentThread.ManagedThreadId} ended: {DateTime.Now.ToString("yyyyMMdd_HHmmss_ffffff")}");
        }

        public static void ExecuteForEachPermutationMT(T[] a_sortedValues, Action<T[]> a_action)
        {
            int coreCount = Environment.ProcessorCount; // Hyper treading are taken into account (ex: on a 4 cores hyperthreaded = 8)
            long itemsFactorial = Factorial.GetFactorial(a_sortedValues.Length);
            long partCount = (long)Math.Ceiling(itemsFactorial / (double)coreCount);
            long startIndex = 0;
            int[] lexOrderedList = new int[a_sortedValues.Length];

            for (int i = 0; i < a_sortedValues.Length; i++)
            {
                lexOrderedList[i] = i;
            }

            List<Task> tasks = new ();

            for (int coreIndex = 0; coreIndex < coreCount; coreIndex++)
            {
                long stopIndex = Math.Min(startIndex + partCount, itemsFactorial);

                PermutationMixOuelletSaniSinghHuttunen<T> mix = new (a_sortedValues, lexOrderedList, startIndex, stopIndex);
                Task task = Task.Run(() => mix.ExecuteForEachPermutation(a_action));
                tasks.Add(task);

                if (stopIndex == itemsFactorial)
                {
                    break;
                }

                startIndex = startIndex + partCount;
            }

            Task.WaitAll(tasks.ToArray());
        }
    }

    /// <summary>
    /// Knuths
    /// Find the largest index j such that a_lexOrderedList[j] less than a_lexOrderedList[j + 1]. If no such index exists, the permutation is the last permutation.
    /// Find the largest index l such that a_lexOrderedList[j] less than a_lexOrderedList[l]. Since j + 1 is such an index, l is well defined and satisfies j less than l.
    /// Swap a_itemsList[j] with a_itemsList[l].
    /// Reverse the sequence from a_itemsList[j + 1] up to and including the final element a_itemsList[n].
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PermutationSaniSinghHuttunen<T>
    {
        public static bool NextPermutation(T[] a_itemsList, int[] a_lexOrderedList)
        {
            int largestIndex = -1;
            for (int i = a_itemsList.Length - 2; i >= 0; i--)
            {
                if (a_lexOrderedList[i] < a_lexOrderedList[i + 1])
                {
                    largestIndex = i;
                    break;
                }
            }

            if (largestIndex < 0)
            {
                return false;
            }

            int largestIndex2 = -1;
            for (int i = a_itemsList.Length - 1; i >= 0; i--)
            {
                if (a_lexOrderedList[largestIndex] < a_lexOrderedList[i])
                {
                    largestIndex2 = i;
                    break;
                }
            }

            T tmp = a_itemsList[largestIndex];
            a_itemsList[largestIndex] = a_itemsList[largestIndex2];
            a_itemsList[largestIndex2] = tmp;

            for (int i = largestIndex + 1, j = a_itemsList.Length - 1; i < j; i++, j--)
            {
                tmp = a_itemsList[i];
                a_itemsList[i] = a_itemsList[j];
                a_itemsList[j] = tmp;
            }

            return true;
        }
    }

    public class PermutationOuelletLexico3<T> // Enable indexing 
    {
        private readonly T[] m_items;
        private readonly bool[] m_valueUsed;
        public readonly long MaxIndex; // long to support 20! or less 
        public T[] Result { get; }

        public PermutationOuelletLexico3(T[] a_items)
        {
            m_items = a_items;
            Result = new T[m_items.Length];
            m_valueUsed = new bool[m_items.Length];

            MaxIndex = Factorial.GetFactorial(m_items.Length);
        }

        /// <summary>
        /// Sort Index is 0 based and should be less than MaxIndex. Otherwise you get an exception.
        /// </summary>
        /// <param name="a_sortIndex"></param>
        /// <param name="result">Value is not used as inpu, only as output. Re-use buffer in order to save memory</param>
        /// <returns></returns>
        public void GetSortedValuesFor(long a_sortIndex)
        {
            int size = m_items.Length;

            if (a_sortIndex < 0)
            {
                throw new ArgumentException("sortIndex should greater or equal to 0.");
            }

            if (a_sortIndex >= MaxIndex)
            {
                throw new ArgumentException("sortIndex should less than factorial(the lenght of items)");
            }

            for (int n = 0; n < m_valueUsed.Length; n++)
            {
                m_valueUsed[n] = false;
            }

            long factorialLower = MaxIndex;

            for (int index = 0; index < size; index++)
            {
                long factorialBigger = factorialLower;
                factorialLower = Factorial.GetFactorial(size - index - 1); //  factorialBigger / inverseIndex;

                int resultItemIndex = (int)(a_sortIndex % factorialBigger / factorialLower);

                int correctedResultItemIndex = 0;
                for (;;)
                {
                    if (!m_valueUsed[correctedResultItemIndex])
                    {
                        resultItemIndex--;
                        if (resultItemIndex < 0)
                        {
                            break;
                        }
                    }

                    correctedResultItemIndex++;
                }

                Result[index] = m_items[correctedResultItemIndex];
                m_valueUsed[correctedResultItemIndex] = true;
            }
        }
    }

    public class Factorial
    {
        protected static long[] m_factorialTable = new long[21];

        static Factorial()
        {
            m_factorialTable[0] = 1; // To prevent divide by 0
            long f = 1;
            for (int i = 1; i <= 20; i++)
            {
                f = f * i;
                m_factorialTable[i] = f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetFactorial(int a_val) // a long can only support up to 20!
        {
            if (a_val > 20)
            {
                throw new OverflowException($"{nameof(Factorial)} only support a factorial value <= 20");
            }

            return m_factorialTable[a_val];
        }
    }
}