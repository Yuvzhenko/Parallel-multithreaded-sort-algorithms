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

    class Program
    {
        static void Main(string[] args)
        {
            int array_size = 10_000_000;
            Console.WriteLine($"Generating {array_size} elements array...");
            int[] original_array = GenerateRandomArray(array_size);

            int[] array_for_sequential = (int[])original_array.Clone();
            int[] array_for_parallel = (int[])original_array.Clone();

            ISorter<int> sequential_sorter = new  SequentialQuickSort<int>();
            Console.WriteLine($"\n::Starting: {sequential_sorter.Name}");

            Stopwatch swSequential = Stopwatch.StartNew();
            sequential_sorter.Sort(array_for_sequential);
            swSequential.Stop();

            Console.WriteLine($"Sorted for: {swSequential.ElapsedMilliseconds}ms");

            ISorter<int> parallel_sorter = new ParallelQuickSort<int>();
            Console.WriteLine($"\n::Starting: {parallel_sorter.Name}");

            Stopwatch swParallel = Stopwatch.StartNew();
            parallel_sorter.Sort(array_for_parallel);
            swParallel.Stop();

            Console.WriteLine($"Sorted for: {swParallel.ElapsedMilliseconds}ms");

            Console.WriteLine("\n---Matching the results---");

            bool is_correct = array_for_sequential.SequenceEqual(array_for_parallel);

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