using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SortAlgorithms
{
    public interface ISorter<T> where T : IComparable<T>
    {
        void Sort(T[] array);
        string Name { get; }
    }

    public class SequentialQuickSort<T> : ISorter<T> where T : IComparable<T>
    {
        public virtual string Name => "Sequential QuickSort";
        public virtual void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;
            QuickSort(array, 0, array.Length - 1);
        }

        protected void Swap(T[] array, int i, int j)
        {
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }

        protected int Partition(T[] array, int minIndex, int maxIndex)
        {
            T pivot = array[maxIndex];
            int i = minIndex - 1;

            for(int j = minIndex; j < maxIndex; j++)
            {
                if(array[j].CompareTo(pivot) <= 0)
                {
                    i++;
                    Swap(array, i, j);
                }
            }
            Swap(array, i+1, maxIndex);
            return i+1;
        }

        private void QuickSort(T[] array, int minIndex, int maxIndex)
        {
            if(minIndex < maxIndex)
            {
                int pivot = Partition(array, minIndex, maxIndex);

                QuickSort(array, minIndex, pivot - 1);
                QuickSort(array, pivot + 1, maxIndex);
            }
        }
        
    }

    public class ParallelQuickSort<T> : SequentialQuickSort<T> where T : IComparable<T>
    {
        public override string Name => "Parallel QuickSort";

        private readonly int _maxDepth;

        public ParallelQuickSort()
        {
            _maxDepth = (int)Math.Log(Environment.ProcessorCount, 2) + 4;
        }

        private void QuickSortSequential(T[] array, int minIndex, int maxIndex)
        {
            if(minIndex < maxIndex)
            {
                int pivot = Partition(array, minIndex, maxIndex);
                QuickSortSequential(array, minIndex, pivot - 1);
                QuickSortSequential(array, pivot + 1, maxIndex);
            }
        }

        public override void Sort(T[] array)
        {
            if(array == null || array.Length <= 1) return;
            QuickSortParallel(array, 0, array.Length - 1, 0);
        }

        private void QuickSortParallel(T[] array, int minIndex, int maxIndex, int depth)
        {
            if (minIndex < maxIndex)
            {
                int pivot = Partition(array, minIndex, maxIndex);

                if(depth < _maxDepth)
                {
                    Parallel.Invoke(
                        () => QuickSortParallel(array, minIndex, pivot - 1, depth + 1),
                        () => QuickSortParallel(array, pivot + 1, maxIndex, depth + 1)
                    );
                }
                else
                {
                    QuickSortSequential(array, minIndex, pivot - 1);
                    QuickSortSequential(array, pivot + 1, maxIndex);
                }
            }
        }
    }

    public class SequentialMergeSort<T> : ISorter<T> where T : IComparable<T>
    {
        public virtual string Name => "Sequential MergeSort";
        public virtual void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;

            var temp_array = new T[array.Length];
            MergeSort(array, temp_array, 0, array.Length - 1);
        }

        protected void Merge(T[] array, T[] temp_array, int minIndex, int middleIndex, int maxIndex)
        {
            int left = minIndex;
            int right = middleIndex + 1;
            int index = minIndex;

            while((left <= middleIndex) && (right <= maxIndex))
            {
                if(array[left].CompareTo(array[right]) <= 0)
                {
                    temp_array[index] = array[left];
                    left++;
                }
                else
                {
                    temp_array[index] = array[right];
                    right++;
                }
                index++;
            }

            for(int i = left; i <= middleIndex; i++)
            {
                temp_array[index] = array[i];
                index++;
            }
            for(int i = right; i <= maxIndex; i++)
            {
                temp_array[index] = array[i];
                index++;
            }
            for(int i = minIndex; i < maxIndex; i++)
            {
                array[i] = temp_array[i];
            }
        }

        protected virtual void MergeSort(T[] array, T[] temp_array, int minIndex, int maxIndex)
        {
            if(minIndex < maxIndex)
            {
                int middleIndex = minIndex + (maxIndex - minIndex) / 2;
                MergeSort(array, temp_array, minIndex, middleIndex);
                MergeSort(array, temp_array, middleIndex + 1, maxIndex);
                Merge(array, temp_array, minIndex, middleIndex, maxIndex);
            }
        }
    }

    public class ParallelMergeSort<T>: SequentialMergeSort<T> where T : IComparable<T>
    {
        public override string Name => "Parallel MergeSort";
        private readonly int _maxDepth;

        public ParallelMergeSort()
        {
            _maxDepth = (int)Math.Log(Environment.ProcessorCount, 2) + 4;
        }

        private void MergeSortParallel(T[] array, T[] temp_array, int minIndex, int maxIndex, int depth)
        {
            if(minIndex < maxIndex)
            {
                int middleIndex = minIndex + (maxIndex - minIndex) / 2;
                if(depth < _maxDepth)
                {
                    Parallel.Invoke(
                        () => MergeSortParallel(array, temp_array, minIndex, middleIndex, depth + 1),
                        () => MergeSortParallel(array, temp_array, middleIndex + 1, maxIndex, depth + 1)
                    );
                }
                else
                {
                    base.MergeSort(array, temp_array, minIndex, middleIndex);
                    base.MergeSort(array, temp_array, middleIndex + 1, maxIndex);
                }

                Merge(array, temp_array, minIndex, middleIndex, maxIndex);
            }
        }

        public override void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;

            var temp_array = new T[array.Length];
            MergeSortParallel(array, temp_array, 0, array.Length - 1, 0);
        }
    }

    public class SequentialCountingSort : ISorter<int>
    {
        public virtual string Name => "Sequential CountingSort";
        public virtual void Sort(int[] array)
        {
            if (array == null || array.Length <= 1) return;

            int min = array[0];
            int max = array[0];
            for(int i = 1; i < array.Length; i++)
            {
                if(array[i] < min)
                    min = array[i];
                else if (array[i] > max)
                    max = array[i];
            }

            int range = max - min + 1;
            int[] counts = new int[range];

            for(int i = 0; i < array.Length; i++)
            {
                counts[array[i] - min]++;
            }

            int index = 0;
            for(int i = 0; i < counts.Length; i++)
            {
                while(counts[i] > 0)
                {
                    array[index] = i + min;
                    index++;
                    counts[i]--;
                }
            }
        }
    }

    public class ParallelCountingSort : SequentialCountingSort
    {
        public override string Name => "Parallel CountingSort";

        public override void Sort(int[] array)
        {
            if(array == null || array.Length <= 1) return;

            int min = array[0];
            int max = array[0];
            for(int i = 1; i < array.Length; i++)
            {
                if(array[i] < min)
                    min = array[i];
                else if (array[i] > max)
                    max = array[i];
            }
            int range = max - min + 1;
            int[] global_counts = new int[range];

            var partitioner = Partitioner.Create(0, array.Length);

            Parallel.ForEach(
                partitioner,
                () => new int[range],
                (chunk, loop_state, local_counts) =>
                {
                    for(int i = chunk.Item1; i < chunk.Item2; i++)
                    {
                        local_counts[array[i] - min]++;
                    }
                    return local_counts;
                },
                (local_counts) =>
                {
                    lock (global_counts)
                    {
                        for(int i = 0; i < range; i++)
                        {
                            global_counts[i] += local_counts[i];
                        }
                    }
                }
            );

            int index = 0;
            for(int i = 0; i < global_counts.Length; i++)
            {
                int count = global_counts[i];
                while(count > 0)
                {
                    array[index] = i + min;
                    index++;
                    count--;
                }
            }
        }
    }
    public class TreeNode<T>
    {
        public T Data;
        public TreeNode<T> Left;
        public TreeNode<T> Right;

        public TreeNode(T data)
        {
            Data = data;
        }
    }

    public class SequentialTreeNodeSort<T>: ISorter<T> where T : IComparable<T>
    {
        public virtual string Name => "Sequential TreeNode";

        protected virtual void Insert(TreeNode<T> root, T data)
        {
            TreeNode<T> current = root;
            while (true)
            {
                if (data.CompareTo(current.Data) <= 0)
                {
                    if (current.Left == null)
                    {
                        current.Left = new TreeNode<T>(data);
                        break;
                    }
                    current = current.Left;
                }
                else
                {
                    if(current.Right == null)
                    {
                        current.Right = new TreeNode<T>(data);
                        break;
                    }
                    current = current.Right;
                }
            }
        }

        protected void InOrderTraversal(TreeNode<T> node, T[] array, ref int index)
        {
            if (node == null) return;

            InOrderTraversal(node.Left, array, ref index);
            array[index++] = node.Data;
            InOrderTraversal(node.Right, array, ref index);
        }

        public virtual void Sort(T[] array)
        {
            if(array == null || array.Length <= 1) return;

            TreeNode<T> root = new TreeNode<T>(array[0]);

            for(int i = 1; i < array.Length; i++)
            {
                Insert(root, array[i]);
            }

            int index = 0;
            InOrderTraversal(root, array, ref index);
        }
    }

    public class ParallelTreeNodeSort<T> : SequentialTreeNodeSort<T> where T : IComparable<T>
    {
        public override string Name => "Parallel TreeNode";

        private void InsertConcurrent(TreeNode<T> root, T data)
        {
            TreeNode<T> current = root;
            while (true)
            {
                if (data.CompareTo(current.Data) <= 0)
                {
                    if(current.Left == null)
                    {
                        lock (current)
                        {
                            if(current.Left == null)
                            {
                                current.Left = new TreeNode<T>(data);
                                break;
                            }
                        }
                    }
                    current = current.Left;
                }
                else
                {
                    if(current.Right == null)
                    {
                        lock (current)
                        {
                            if(current.Right == null)
                            {
                                current.Right = new TreeNode<T>(data);
                                break;
                            }
                        }
                    }
                    current = current.Right;
                }
            }
        }

        public override void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;

            TreeNode<T> root = new TreeNode<T>(array[0]);

            Parallel.For(1, array.Length, i =>
            {
                InsertConcurrent(root, array[i]);
            });

            int index = 0;
            InOrderTraversal(root, array, ref index);
        }
    }

    public class SequentialCocktrailSort<T> : ISorter<T> where T : IComparable<T>
    {
        public virtual string Name => "Sequential CocktrailSort";

        protected void Swap(T[] array, int i, int j)
        {
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }

        public virtual void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;

            bool swapped = true;
            int start = 0;
            int end = array.Length - 1;

            while (swapped)
            {
                swapped = false;

                for(int i = start; i < end; i++)
                {
                    if(array[i].CompareTo(array[i+1]) > 0)
                    {
                        Swap(array, i, i+1);
                        swapped = true;
                    }
                }

                if (!swapped) break;

                swapped = false;
                end--;

                for(int i = end-1; i >= start; i--)
                {
                    if(array[i].CompareTo(array[i+1]) > 0)
                    {
                        Swap(array, i, i+1);
                        swapped = true;
                    }
                }
                start++;
            }
        }
    }

    public class ParallelCocktrailSort<T> : SequentialCocktrailSort<T> where T : IComparable<T>
    {
        public override string Name => "Parallel CocktrailSort";

        public override void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;

            int has_swapped = 1;

            while (has_swapped == 1)
            {
                has_swapped = 0;

                Parallel.For(0, array.Length / 2, i =>
                {
                   int j = i * 2;
                   if(j < array.Length - 1 && array[j].CompareTo(array[j + 1]) > 0)
                    {
                        Swap(array, j, j+1);
                        Interlocked.Exchange(ref has_swapped, 1);
                    } 
                });

                Parallel.For(0, array.Length / 2, i =>
                {
                    int j = i * 2 + 1;
                    if(j < array.Length - 1 && array[j].CompareTo(array[j+1]) > 0)
                    {
                        Swap(array, j, j + 1);
                        Interlocked.Exchange(ref has_swapped, 1);
                    }
                });
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int array_size = 1_000_000;
            Console.WriteLine($"Generating {array_size} elements array...");
            int[] original_array = GenerateRandomArray(array_size);

            int[] array_for_quicksort_sequential = (int[])original_array.Clone();
            int[] array_for_quicksort_parallel = (int[])original_array.Clone();
            int[] array_for_mergesort_sequential = (int[])original_array.Clone();
            int[] array_for_mergesort_parallel = (int[])original_array.Clone();
            int[] array_for_countingsort_sequential = (int[])original_array.Clone();
            int[] array_for_countingsort_parallel = (int[])original_array.Clone();
            int[] array_for_treeNodesort_sequential = (int[])original_array.Clone();
            int[] array_for_treeNodesort_parallel = (int[])original_array.Clone();

            array_size = 10_000;
            int[] array_for_cocktrailsort_sequential = GenerateRandomArray(array_size);
            int[] array_for_cocktrailsort_parallel = (int[])array_for_cocktrailsort_sequential.Clone();

            ISorter<int> sequential_quick_sorter = new  SequentialQuickSort<int>();
            Console.WriteLine($"\n::Starting: {sequential_quick_sorter.Name}");

            Stopwatch swSequentialQuick = Stopwatch.StartNew();
            sequential_quick_sorter.Sort(array_for_quicksort_sequential);
            swSequentialQuick.Stop();

            Console.WriteLine($"Sorted for: {swSequentialQuick.ElapsedMilliseconds}ms");

            ISorter<int> parallel_quick_sorter = new ParallelQuickSort<int>();
            Console.WriteLine($"\n::Starting: {parallel_quick_sorter.Name}");

            Stopwatch swParallelQuick = Stopwatch.StartNew();
            parallel_quick_sorter.Sort(array_for_quicksort_parallel);
            swParallelQuick.Stop();

            Console.WriteLine($"Sorted for: {swParallelQuick.ElapsedMilliseconds}ms");

            Console.WriteLine("\n---Matching the results---");

            bool is_correct = array_for_quicksort_sequential.SequenceEqual(array_for_quicksort_parallel);

            if (is_correct)
            {
                Console.WriteLine("The sorting results are the same");
            }
            else
            {
                Console.WriteLine("The sorting results are different!");
            }

            ISorter<int> sequential_merge_sorter = new  SequentialMergeSort<int>();
            Console.WriteLine($"\n::Starting: {sequential_merge_sorter.Name}");

            Stopwatch swSequentialMerge = Stopwatch.StartNew();
            sequential_merge_sorter.Sort(array_for_mergesort_sequential);
            swSequentialMerge.Stop();

            Console.WriteLine($"Sorted for: {swSequentialMerge.ElapsedMilliseconds}ms");

            ISorter<int> parallel_merge_sorter = new  ParallelMergeSort<int>();
            Console.WriteLine($"\n::Starting: {parallel_merge_sorter.Name}");

            Stopwatch swParallelMerge = Stopwatch.StartNew();
            sequential_merge_sorter.Sort(array_for_mergesort_parallel);
            swParallelMerge.Stop();

            Console.WriteLine($"Sorted for: {swParallelMerge.ElapsedMilliseconds}ms");

            Console.WriteLine("\n---Matching the results---");

            is_correct = array_for_mergesort_sequential.SequenceEqual(array_for_mergesort_parallel);

            if (is_correct)
            {
                Console.WriteLine("The sorting results are the same");
            }
            else
            {
                Console.WriteLine("The sorting results are different!");
            }

            ISorter<int> sequential_counting_sorter = new  SequentialCountingSort();
            Console.WriteLine($"\n::Starting: {sequential_counting_sorter.Name}");

            Stopwatch swSequentialCounting = Stopwatch.StartNew();
            sequential_counting_sorter.Sort(array_for_countingsort_sequential);
            swSequentialCounting.Stop();

            Console.WriteLine($"Sorted for: {swSequentialCounting.ElapsedMilliseconds}ms");

            ISorter<int> parallel_counting_sorter = new  ParallelCountingSort();
            Console.WriteLine($"\n::Starting: {parallel_counting_sorter.Name}");

            Stopwatch swParallelCounting = Stopwatch.StartNew();
            parallel_counting_sorter.Sort(array_for_countingsort_parallel);
            swParallelCounting.Stop();

            Console.WriteLine($"Sorted for: {swParallelCounting.ElapsedMilliseconds}ms");

            Console.WriteLine("\n---Matching the results---");

            is_correct = array_for_countingsort_sequential.SequenceEqual(array_for_countingsort_parallel);

            if (is_correct)
            {
                Console.WriteLine("The sorting results are the same");
            }
            else
            {
                Console.WriteLine("The sorting results are different!");
            }

            ISorter<int> sequential_treeNode_sorter = new  SequentialTreeNodeSort<int>();
            Console.WriteLine($"\n::Starting: {sequential_treeNode_sorter.Name}");

            Stopwatch swSequentialTreeNode = Stopwatch.StartNew();
            sequential_treeNode_sorter.Sort(array_for_treeNodesort_sequential);
            swSequentialTreeNode.Stop();

            Console.WriteLine($"Sorted for: {swSequentialTreeNode.ElapsedMilliseconds}ms");

            ISorter<int> parallel_treeNode_sorter = new ParallelTreeNodeSort<int>();
            Console.WriteLine($"\n::Starting: {parallel_treeNode_sorter.Name}");

            Stopwatch swParallelTreeNode = Stopwatch.StartNew();
            parallel_treeNode_sorter.Sort(array_for_treeNodesort_parallel);
            swParallelTreeNode.Stop();

            Console.WriteLine($"Sorted for: {swParallelTreeNode.ElapsedMilliseconds}ms");

            Console.WriteLine("\n---Matching the results---");

            is_correct = array_for_treeNodesort_sequential.SequenceEqual(array_for_treeNodesort_parallel);

            if (is_correct)
            {
                Console.WriteLine("The sorting results are the same");
            }
            else
            {
                Console.WriteLine("The sorting results are different!");
            }

            ISorter<int> sequential_cocktrail_sorter = new SequentialCocktrailSort<int>();
            Console.WriteLine($"\n::Starting: {sequential_cocktrail_sorter.Name}");

            Stopwatch swSequentialCocktrail = Stopwatch.StartNew();
            sequential_cocktrail_sorter.Sort(array_for_cocktrailsort_sequential);
            swSequentialCocktrail.Stop();

            Console.WriteLine($"Sorted for: {swSequentialCocktrail.ElapsedMilliseconds}ms");

            ISorter<int> parallel_cocktrail_sorter = new ParallelCocktrailSort<int>();
            Console.WriteLine($"\n::Starting: {parallel_cocktrail_sorter.Name}");

            Stopwatch swParallelCocktrail = Stopwatch.StartNew();
            parallel_cocktrail_sorter.Sort(array_for_cocktrailsort_parallel);
            swParallelCocktrail.Stop();

            Console.WriteLine($"Sorted for: {swParallelCocktrail.ElapsedMilliseconds}ms");

            Console.WriteLine("\n---Matching the results---");

            is_correct = array_for_cocktrailsort_sequential.SequenceEqual(array_for_cocktrailsort_parallel);

            if (is_correct)
            {
                Console.WriteLine("The sorting results are the same");
            }
            else
            {
                Console.WriteLine("The sorting results are different!");
            }
            
            Console.ReadLine();
        }

        static int[] GenerateRandomArray(int size)
        {
            Random rand = new Random(42);
            int[] arr = new int[size];

            for(int i = 0; i < size; i++)
            {
                arr[i] =rand.Next(0, 1_000_000);
            }
            return arr;
        }
    }
}