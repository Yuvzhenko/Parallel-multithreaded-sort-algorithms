using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SortAlgorithms;

namespace SortAlgorithms.Tests
{
    public class SortingTests
    {
        public static IEnumerable<object[]> GetSorters()
        {
            yield return new object[] { new SequentialQuickSort<int>() };
            yield return new object[] { new ParallelQuickSort<int>() };
            yield return new object[] { new SequentialMergeSort<int>() };
            yield return new object[] { new ParallelMergeSort<int>() };
            yield return new object[] { new SequentialCountingSort() };
            yield return new object[] { new ParallelCountingSort() };
            yield return new object[] { new SequentialTreeNodeSort<int>() };
            yield return new object[] { new ParallelTreeNodeSort<int>() };
            yield return new object[] { new SequentialCocktrailSort<int>() };
            yield return new object[] { new ParallelCocktrailSort<int>() };
        }

        [Theory]
        [MemberData(nameof(GetSorters))]
        public void Sort_RandomArray_SortsCorrectly(ISorter<int> sorter)
        {
            int[] array = { 42, 7, 13, 100, 1, 5, 8, 13 };
            int[] expected = { 1, 5, 7, 8, 13, 13, 42, 100 };

            sorter.Sort(array);

            Assert.Equal(expected, array);
        }

        [Theory]
        [MemberData(nameof(GetSorters))]
        public void Sort_AlreadySortedArray_RemainsSorted(ISorter<int> sorter)
        {
            int[] array = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int[] expected = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            sorter.Sort(array);

            Assert.Equal(expected, array);
        }

        [Theory]
        [MemberData(nameof(GetSorters))]
        public void Sort_ReverseSortedArray_SortsCorrectly(ISorter<int> sorter)
        {
            int[] array = { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            int[] expected = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            sorter.Sort(array);

            Assert.Equal(expected, array);
        }

        [Theory]
        [MemberData(nameof(GetSorters))]
        public void Sort_AllIdenticalElements_SortsCorrectly(ISorter<int> sorter)
        {
            int[] array = { 5, 5, 5, 5, 5 };
            int[] expected = { 5, 5, 5, 5, 5 };

            sorter.Sort(array);

            Assert.Equal(expected, array);
        }

        [Theory]
        [MemberData(nameof(GetSorters))]
        public void Sort_EmptyArray_DoesNotThrow(ISorter<int> sorter)
        {
            int[] array = Array.Empty<int>();
            int[] expected = Array.Empty<int>();

            var exception = Record.Exception(() => sorter.Sort(array));

            Assert.Null(exception);
            Assert.Equal(expected, array);
        }

        [Theory]
        [MemberData(nameof(GetSorters))]
        public void Sort_SingleElementArray_DoesNotThrow(ISorter<int> sorter)
        {
            int[] array = { 42 };
            int[] expected = { 42 };

            var exception = Record.Exception(() => sorter.Sort(array));

            Assert.Null(exception);
            Assert.Equal(expected, array);
        }

        [Theory]
        [MemberData(nameof(GetSorters))]
        public void Sort_NullArray_DoesNotThrow(ISorter<int> sorter)
        {
            int[] array = null;

            var exception = Record.Exception(() => sorter.Sort(array));

            Assert.Null(exception);
            Assert.Null(array);
        }
    }
}