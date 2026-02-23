namespace PT.Common.Algorithms;

public static class ElementListSorting
{
    /// <summary>
    /// Additional Explanation:
    /// Purpose: The GroupElements function is designed to divide a list into smaller sublists(groups) where each group meets the specified size constraints, and the distribution of items is as balanced as possible.
    /// Exceptions:
    ///     -ArgumentException: Thrown if the input list doesn't have enough elements to form even a single group of the minimum size, or if the minimum group size is larger than the maximum group size.
    ///     -InvalidOperationException: Thrown if it's impossible to create a grouping that satisfies the constraints with the given list of elements.
    /// Implementation Details:
    /// Group Count Range:
    ///     -The function calculates the minimum and maximum possible number of groups based on the total number of items and the group size constraints.
    ///    Variance Minimization:
    ///     -It iterates over the possible number of groups, calculating the group sizes and their variance.The goal is to find the grouping with the lowest variance in group sizes.
    ///    Early Exit Optimization:
    ///     -If a grouping with zero variance is found (all groups are of equal size), the function exits early since this is the optimal distribution.
    ///    Partitioning:
    ///     -The original list is partitioned into groups according to the best group sizes found during the iteration.
    /// Usage Notes:
    ///     -The function is generic and can be used with any type of list (List<T>).
    ///     -It's important to ensure that the constraints provided (minGroupSize and maxGroupSize) make it possible to group all elements. Otherwise, an exception will be thrown.
    ///     -The function maintains the order of elements from the original list in the resulting groups.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_items">The original list of elements to be grouped</param>
    /// <param name="a_minGroupSize">The smallest number of elements that a group can contain</param>
    /// <param name="a_maxGroupSize"The largest number of elements that a group can contain></param>
    /// <returns>A list of groups (each itself a list of elements) that together contain all the elements from the original list, adhering to the specified group size constraints</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static List<List<T>> OptimalGroupDistributionSorter<T>(this List<T> a_items, int a_minGroupSize, int a_maxGroupSize)
    {
        int totalItemCount = a_items.Count;
        if (totalItemCount < a_minGroupSize)
            throw new ArgumentException($"List must contain at least {a_minGroupSize} elements.");

        if (a_minGroupSize > a_maxGroupSize)
            throw new ArgumentException("Minimum group size cannot be greater than maximum group size.");

        int minGroupCount = (totalItemCount + a_maxGroupSize - 1) / a_maxGroupSize; // Minimum number of groups
        int maxGroupCount = totalItemCount / a_minGroupSize;                      // Maximum number of groups

        List<int> bestGroupSizes = null;
        double minVariance = double.MaxValue;

        // Iterate over possible group counts
        for (int groupCount = minGroupCount; groupCount <= maxGroupCount; groupCount++)
        {
            double averageGroupSize = (double)totalItemCount / groupCount;
            int floorGroupSize = Math.Max(a_minGroupSize, Math.Min(a_maxGroupSize, (int)Math.Floor(averageGroupSize)));
            int ceilGroupSize = Math.Max(a_minGroupSize, Math.Min(a_maxGroupSize, (int)Math.Ceiling(averageGroupSize)));

            if (floorGroupSize == ceilGroupSize)
            {
                if (totalItemCount == groupCount * floorGroupSize)
                {
                    List<int> groupSizes = Enumerable.Repeat(floorGroupSize, groupCount).ToList();

                    // Update best group sizes
                    bestGroupSizes = groupSizes;

                    // Early exit since variance is 0
                    break;
                }
            }
            else
            {
                int numerator = groupCount * ceilGroupSize - totalItemCount;
                int denominator = ceilGroupSize - floorGroupSize;

                if (denominator <= 0)
                    continue;

                if (numerator % denominator != 0)
                    continue;

                int numFloorSizeGroups = numerator / denominator;

                if (numFloorSizeGroups >= 0 && numFloorSizeGroups <= groupCount)
                {
                    List<int> groupSizes = new List<int>();
                    for (int i = 0; i < numFloorSizeGroups; i++)
                    {
                        groupSizes.Add(floorGroupSize);
                    }

                    for (int i = 0; i < groupCount - numFloorSizeGroups; i++)
                    {
                        groupSizes.Add(ceilGroupSize);
                    }

                    double meanGroupSize = (double)totalItemCount / groupCount;
                    double variance = groupSizes.Select(size => (size - meanGroupSize) * (size - meanGroupSize)).Sum() / groupCount;

                    if (variance < minVariance)
                    {
                        minVariance = variance;
                        bestGroupSizes = groupSizes;

                        // Early exit if variance is 0
                        if (variance == 0)
                            break;
                    }
                }
            }
        }

        if (bestGroupSizes == null)
            throw new InvalidOperationException("Cannot group elements with the given constraints.");

        // Partition the items into groups according to the best group sizes
        List<List<T>> result = new List<List<T>>();
        int currentIndex = 0;
        foreach (int groupSize in bestGroupSizes)
        {
            result.Add(a_items.GetRange(currentIndex, groupSize));
            currentIndex += groupSize;
        }
        return result;
    }

    /// <summary>
    /// Evenly distributes elements from a list into sublists, ensuring that
    /// each sublist has a size between the specified minimum and maximum constraints.
    /// The function tries to distribute the elements as evenly as possible.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="a_elements">The master list of elements to distribute.</param>
    /// <param name="a_minGroupSize">The minimum number of elements per group (must be >= 1).</param>
    /// <param name="a_maxGroupSize">The maximum number of elements per group (must be >= minGroupSize).</param>
    /// <returns>A list of sublists, where each sublist contains elements grouped according to the specified constraints.</returns>
    /// <exception cref="ArgumentException">Thrown if minGroupSize is less than 1 or maxGroupSize is less than minGroupSize.</exception>
    public static List<List<T>> QuickGroupDistributionSorter<T>(this List<T> a_elements, int a_minGroupSize, int a_maxGroupSize)
    {
        if (a_minGroupSize < 1 || a_maxGroupSize < a_minGroupSize)
        {
            throw new ArgumentException("Invalid group size constraints.");
        }

        List<List<T>> result = new List<List<T>>();
        int totalElements = a_elements.Count;

        // Calculate the optimal number of groups
        int numberOfGroups = (int)Math.Ceiling((decimal)totalElements / a_maxGroupSize);

        int currentIndex = 0;

        // Loop through and manually create sublists
        for (int i = 0; i < numberOfGroups; i++)
        {
            // Calculate the size of the current group based on remaining elements and group constraints
            int remainingElements = totalElements - currentIndex;
            int groupsLeft = numberOfGroups - i;

            // Determine the appropriate group size based on how many elements are left and how many groups we need to fill
            int groupSize = Math.Min(Math.Max(remainingElements / groupsLeft, a_minGroupSize), a_maxGroupSize);

            // Create a new sublist and populate it
            List<T> group = new List<T>();
            for (int j = 0; j < groupSize && currentIndex < totalElements; j++)
            {
                group.Add(a_elements[currentIndex]);
                currentIndex++;
            }

            result.Add(group);
        }

        return result;
    }
}
