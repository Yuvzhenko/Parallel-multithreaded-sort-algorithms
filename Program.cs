using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SortAlgorithms
{
    /// <summary>
    /// Загальний інтерфейс для всіх алгоритмів сортування.
    /// Забезпечує використання патерну Strategy для легкого перемикання між алгоритмами.
    /// </summary>
    /// <typeparam name="T">Тип елементів у масиві. Повинен реалізовувати інтерфейс IComparable.</typeparam>
    public interface ISorter<T> where T : IComparable<T>
    {
        /// <summary>
        /// Виконує сортування масиву на місці (in-place) або зі створенням тимчасових структур (залежно від реалізації).
        /// </summary>
        /// <param name="array">Масив, який потрібно відсортувати.</param>
        void Sort(T[] array);

        /// <summary>
        /// Повертає читабельну назву алгоритму сортування для виводу в консоль або звіти.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// Реалізація послідовного алгоритму швидкого сортування (Quick Sort).
    /// Складність: O(N log N) у середньому, O(N^2) у найгіршому випадку.
    /// </summary>
    /// <typeparam name="T">Тип елементів у масиві.</typeparam>
    public class SequentialQuickSort<T> : ISorter<T> where T : IComparable<T>
    {
        public virtual string Name => "Sequential QuickSort";

        public virtual void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;
            QuickSort(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Допоміжний метод для обміну двох елементів масиву місцями.
        /// </summary>
        protected void Swap(T[] array, int i, int j)
        {
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }

        /// <summary>
        /// Розділяє масив на дві частини відносно опорного елемента (pivot).
        /// Елементи, менші за pivot, переміщуються ліворуч, більші - праворуч.
        /// </summary>
        /// <param name="array">Масив для розділення.</param>
        /// <param name="minIndex">Початковий індекс підмасиву.</param>
        /// <param name="maxIndex">Кінцевий індекс підмасиву.</param>
        /// <returns>Кінцевий індекс опорного елемента після розділення.</returns>
        protected int Partition(T[] array, int minIndex, int maxIndex)
        {
            T pivot = array[maxIndex];
            int i = minIndex - 1;

            for (int j = minIndex; j < maxIndex; j++)
            {
                if (array[j].CompareTo(pivot) <= 0)
                {
                    i++;
                    Swap(array, i, j);
                }
            }
            Swap(array, i + 1, maxIndex);
            return i + 1;
        }

        /// <summary>
        /// Основний рекурсивний метод швидкого сортування.
        /// </summary>
        private void QuickSort(T[] array, int minIndex, int maxIndex)
        {
            if (minIndex < maxIndex)
            {
                int pivot = Partition(array, minIndex, maxIndex);
                QuickSort(array, minIndex, pivot - 1);
                QuickSort(array, pivot + 1, maxIndex);
            }
        }
    }

    /// <summary>
    /// Реалізація паралельного алгоритму швидкого сортування за допомогою Task Parallel Library (TPL).
    /// </summary>
    /// <remarks>
    /// Використовує обмеження глибини рекурсії (_maxDepth), щоб уникнути створення 
    /// занадто великої кількості потоків для дрібних підмасивів.
    /// </remarks>
    public class ParallelQuickSort<T> : SequentialQuickSort<T> where T : IComparable<T>
    {
        public override string Name => "Parallel QuickSort";

        private readonly int _maxDepth;

        /// <summary>
        /// Ініціалізує новий екземпляр паралельного сортувальника та розраховує оптимальну глибину паралелізму.
        /// </summary>
        public ParallelQuickSort()
        {
            _maxDepth = (int)Math.Log(Environment.ProcessorCount, 2) + 4;
        }

        private void QuickSortSequential(T[] array, int minIndex, int maxIndex)
        {
            if (minIndex < maxIndex)
            {
                int pivot = Partition(array, minIndex, maxIndex);
                QuickSortSequential(array, minIndex, pivot - 1);
                QuickSortSequential(array, pivot + 1, maxIndex);
            }
        }

        public override void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;
            QuickSortParallel(array, 0, array.Length - 1, 0);
        }

        /// <summary>
        /// Рекурсивний метод паралельного швидкого сортування.
        /// Розділяє виконання на кілька потоків за допомогою Parallel.Invoke.
        /// </summary>
        /// <param name="depth">Поточна глибина рекурсії для контролю розпаралелювання.</param>
        private void QuickSortParallel(T[] array, int minIndex, int maxIndex, int depth)
        {
            if (minIndex < maxIndex)
            {
                int pivot = Partition(array, minIndex, maxIndex);

                if (depth < _maxDepth)
                {
                    Parallel.Invoke(
                        () => QuickSortParallel(array, minIndex, pivot - 1, depth + 1),
                        () => QuickSortParallel(array, pivot + 1, maxIndex, depth + 1)
                    );
                }
                else
                {
                    // Fallback до послідовного сортування
                    QuickSortSequential(array, minIndex, pivot - 1);
                    QuickSortSequential(array, pivot + 1, maxIndex);
                }
            }
        }
    }

    /// <summary>
    /// Реалізація послідовного алгоритму сортування злиттям (Merge Sort).
    /// Складність: гарантовано O(N log N) пам'ять: O(N).
    /// </summary>
    public class SequentialMergeSort<T> : ISorter<T> where T : IComparable<T>
    {
        public virtual string Name => "Sequential MergeSort";
        
        public virtual void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;

            var temp_array = new T[array.Length];
            MergeSort(array, temp_array, 0, array.Length - 1);
        }

        /// <summary>
        /// Зливає два відсортовані підмасиви в один впорядкований масив.
        /// </summary>
        /// <param name="temp_array">Глобальний тимчасовий масив для уникнення зайвих виділень пам'яті.</param>
        protected void Merge(T[] array, T[] temp_array, int minIndex, int middleIndex, int maxIndex)
        {
            int left = minIndex;
            int right = middleIndex + 1;
            int index = minIndex;

            while ((left <= middleIndex) && (right <= maxIndex))
            {
                if (array[left].CompareTo(array[right]) <= 0)
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

            for (int i = left; i <= middleIndex; i++)
            {
                temp_array[index] = array[i];
                index++;
            }
            for (int i = right; i <= maxIndex; i++)
            {
                temp_array[index] = array[i];
                index++;
            }
            for (int i = minIndex; i <= maxIndex; i++)
            {
                array[i] = temp_array[i];
            }
        }

        /// <summary>
        /// Основний рекурсивний метод сортування злиттям.
        /// </summary>
        protected virtual void MergeSort(T[] array, T[] temp_array, int minIndex, int maxIndex)
        {
            if (minIndex < maxIndex)
            {
                int middleIndex = minIndex + (maxIndex - minIndex) / 2;
                MergeSort(array, temp_array, minIndex, middleIndex);
                MergeSort(array, temp_array, middleIndex + 1, maxIndex);
                Merge(array, temp_array, minIndex, middleIndex, maxIndex);
            }
        }
    }

    /// <summary>
    /// Реалізація паралельного алгоритму сортування злиттям (Merge Sort).
    /// </summary>
    public class ParallelMergeSort<T> : SequentialMergeSort<T> where T : IComparable<T>
    {
        public override string Name => "Parallel MergeSort";
        private readonly int _maxDepth;

        public ParallelMergeSort()
        {
            _maxDepth = (int)Math.Log(Environment.ProcessorCount, 2) + 4;
        }

        private void MergeSortParallel(T[] array, T[] temp_array, int minIndex, int maxIndex, int depth)
        {
            if (minIndex < maxIndex)
            {
                int middleIndex = minIndex + (maxIndex - minIndex) / 2;
                if (depth < _maxDepth)
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

    /// <summary>
    /// Реалізація лінійного алгоритму сортування підрахунком (Counting Sort).
    /// Складність: O(N + K), де K - діапазон значень.
    /// Працює виключно з цілими числами.
    /// </summary>
    public class SequentialCountingSort : ISorter<int>
    {
        public virtual string Name => "Sequential CountingSort";
        
        public virtual void Sort(int[] array)
        {
            if (array == null || array.Length <= 1) return;

            int min = array[0];
            int max = array[0];
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] < min) min = array[i];
                else if (array[i] > max) max = array[i];
            }

            int range = max - min + 1;
            int[] counts = new int[range];

            for (int i = 0; i < array.Length; i++)
            {
                counts[array[i] - min]++;
            }

            int index = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                while (counts[i] > 0)
                {
                    array[index] = i + min;
                    index++;
                    counts[i]--;
                }
            }
        }
    }

    /// <summary>
    /// Реалізація паралельного алгоритму сортування підрахунком.
    /// Використовує патерн Map-Reduce за допомогою Thread-Local змінних 
    /// для уникнення стану гонитви (race condition) без втрати продуктивності.
    /// </summary>
    public class ParallelCountingSort : SequentialCountingSort
    {
        public override string Name => "Parallel CountingSort";

        public override void Sort(int[] array)
        {
            if (array == null || array.Length <= 1) return;

            int min = array[0];
            int max = array[0];
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] < min) min = array[i];
                else if (array[i] > max) max = array[i];
            }
            int range = max - min + 1;
            int[] global_counts = new int[range];

            var partitioner = Partitioner.Create(0, array.Length);

            // Фаза Map: Паралельний підрахунок частот у локальних масивах потоків
            Parallel.ForEach(
                partitioner,
                () => new int[range],
                (chunk, loop_state, local_counts) =>
                {
                    for (int i = chunk.Item1; i < chunk.Item2; i++)
                    {
                        local_counts[array[i] - min]++;
                    }
                    return local_counts;
                },
                // Фаза Reduce: Безпечне злиття локальних лічильників у глобальний
                (local_counts) =>
                {
                    lock (global_counts)
                    {
                        for (int i = 0; i < range; i++)
                        {
                            global_counts[i] += local_counts[i];
                        }
                    }
                }
            );

            int index = 0;
            for (int i = 0; i < global_counts.Length; i++)
            {
                int count = global_counts[i];
                while (count > 0)
                {
                    array[index] = i + min;
                    index++;
                    count--;
                }
            }
        }
    }

    /// <summary>
    /// Клас, що представляє вузол бінарного дерева пошуку (BST).
    /// </summary>
    /// <typeparam name="T">Тип даних вузла.</typeparam>
    public class TreeNode<T>
    {
        /// <summary>Дані, що зберігаються у вузлі.</summary>
        public T Data;
        /// <summary>Вказівник на лівого нащадка (менші значення).</summary>
        public TreeNode<T> Left;
        /// <summary>Вказівник на правого нащадка (більші значення).</summary>
        public TreeNode<T> Right;

        public TreeNode(T data)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Реалізація алгоритму сортування за допомогою бінарного дерева (Tree Sort).
    /// Будує бінарне дерево пошуку та виконує In-Order обхід для отримання відсортованих даних.
    /// </summary>
    public class SequentialTreeNodeSort<T> : ISorter<T> where T : IComparable<T>
    {
        public virtual string Name => "Sequential TreeNode Sort";

        /// <summary>
        /// Вставляє новий елемент у бінарне дерево пошуку ітеративним шляхом.
        /// </summary>
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
                    if (current.Right == null)
                    {
                        current.Right = new TreeNode<T>(data);
                        break;
                    }
                    current = current.Right;
                }
            }
        }

        /// <summary>
        /// Рекурсивний обхід дерева In-Order (Зліва-Корінь-Справа).
        /// Гарантує зчитування елементів у відсортованому порядку.
        /// </summary>
        protected void InOrderTraversal(TreeNode<T> node, T[] array, ref int index)
        {
            if (node == null) return;

            InOrderTraversal(node.Left, array, ref index);
            array[index++] = node.Data;
            InOrderTraversal(node.Right, array, ref index);
        }

        public virtual void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;

            TreeNode<T> root = new TreeNode<T>(array[0]);

            for (int i = 1; i < array.Length; i++)
            {
                Insert(root, array[i]);
            }

            int index = 0;
            InOrderTraversal(root, array, ref index);
        }
    }

    /// <summary>
    /// Паралельна реалізація Tree Sort за допомогою потокобезпечного бінарного дерева.
    /// </summary>
    public class ParallelTreeNodeSort<T> : SequentialTreeNodeSort<T> where T : IComparable<T>
    {
        public override string Name => "Parallel TreeNode Sort";

        /// <summary>
        /// Потокобезпечна вставка елемента в дерево.
        /// Використовує патерн Double-Checked Locking (Двохетапна перевірка з блокуванням) 
        /// для мінімізації накладних витрат на синхронізацію.
        /// </summary>
        private void InsertConcurrent(TreeNode<T> root, T data)
        {
            TreeNode<T> current = root;
            while (true)
            {
                if (data.CompareTo(current.Data) <= 0)
                {
                    if (current.Left == null)
                    {
                        lock (current)
                        {
                            if (current.Left == null)
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
                    if (current.Right == null)
                    {
                        lock (current)
                        {
                            if (current.Right == null)
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

            // Багатопоточна вставка елементів
            Parallel.For(1, array.Length, i =>
            {
                InsertConcurrent(root, array[i]);
            });

            int index = 0;
            InOrderTraversal(root, array, ref index);
        }
    }

    /// <summary>
    /// Реалізація двонаправленого бульбашкового сортування (Cocktail Sort).
    /// Складність: O(N^2).
    /// </summary>
    public class SequentialCocktrailSort<T> : ISorter<T> where T : IComparable<T>
    {
        public virtual string Name => "Sequential CocktailSort";

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

                // Прохід зліва направо
                for (int i = start; i < end; i++)
                {
                    if (array[i].CompareTo(array[i + 1]) > 0)
                    {
                        Swap(array, i, i + 1);
                        swapped = true;
                    }
                }

                if (!swapped) break;

                swapped = false;
                end--;

                // Прохід справа наліво
                for (int i = end - 1; i >= start; i--)
                {
                    if (array[i].CompareTo(array[i + 1]) > 0)
                    {
                        Swap(array, i, i + 1);
                        swapped = true;
                    }
                }
                start++;
            }
        }
    }

    /// <summary>
    /// Реалізація паралельного коктейльного сортування у вигляді Odd-Even (Парне-непарне) алгоритму.
    /// Цей підхід усуває математичну залежність між сусідніми обмінами.
    /// </summary>
    public class ParallelCocktrailSort<T> : SequentialCocktrailSort<T> where T : IComparable<T>
    {
        public override string Name => "Parallel CocktailSort";

        public override void Sort(T[] array)
        {
            if (array == null || array.Length <= 1) return;

            int has_swapped = 1;

            while (has_swapped == 1)
            {
                has_swapped = 0;

                // Фаза парних індексів
                Parallel.For(0, array.Length / 2, i =>
                {
                    int j = i * 2;
                    if (j < array.Length - 1 && array[j].CompareTo(array[j + 1]) > 0)
                    {
                        Swap(array, j, j + 1);
                        // Потокобезпечна зміна прапорця
                        Interlocked.Exchange(ref has_swapped, 1);
                    }
                });

                // Фаза непарних індексів
                Parallel.For(0, array.Length / 2, i =>
                {
                    int j = i * 2 + 1;
                    if (j < array.Length - 1 && array[j].CompareTo(array[j + 1]) > 0)
                    {
                        Swap(array, j, j + 1);
                        Interlocked.Exchange(ref has_swapped, 1);
                    }
                });
            }
        }
    }

    /// <summary>
    /// Головний клас програми. Використовується для проведення бенчмаркінгу (заміру часу) алгоритмів.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Точка входу в програму. Запускає тестування всіх реалізованих алгоритмів.
        /// </summary>
        static void Main(string[] args)
        {
            int array_size = 10_000_000;
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

            // Зменшений розмір для O(N^2) алгоритму CocktailSort
            array_size = 10_000;
            int[] array_for_cocktrailsort_sequential = GenerateRandomArray(array_size);
            int[] array_for_cocktrailsort_parallel = (int[])array_for_cocktrailsort_sequential.Clone();

            // ----------------------------------------------------
            // Тестування алгоритму QuickSort
            // ----------------------------------------------------
            ISorter<int> sequential_quick_sorter = new SequentialQuickSort<int>();
            Console.WriteLine($"\n::Starting: {sequential_quick_sorter.Name}");

            Stopwatch swSequential = Stopwatch.StartNew();
            sequential_quick_sorter.Sort(array_for_quicksort_sequential);
            swSequential.Stop();

            Console.WriteLine($"Sorted for: {swSequential.ElapsedMilliseconds}ms");

            ISorter<int> parallel_quick_sorter = new ParallelQuickSort<int>();
            Console.WriteLine($"\n::Starting: {parallel_quick_sorter.Name}");

            Stopwatch swParallel = Stopwatch.StartNew();
            parallel_quick_sorter.Sort(array_for_quicksort_parallel);
            swParallel.Stop();

            Console.WriteLine($"Sorted for: {swParallel.ElapsedMilliseconds}ms");
            Console.WriteLine($"Parallel version outperforms Sequential by a {(int)(100 * ((double)swSequential.ElapsedMilliseconds / swParallel.ElapsedMilliseconds - 1))}%");

            // ----------------------------------------------------
            // Тестування алгоритму MergeSort
            // ----------------------------------------------------
            ISorter<int> sequential_merge_sorter = new SequentialMergeSort<int>();
            Console.WriteLine($"\n::Starting: {sequential_merge_sorter.Name}");

            swSequential = Stopwatch.StartNew();
            sequential_merge_sorter.Sort(array_for_mergesort_sequential);
            swSequential.Stop();

            Console.WriteLine($"Sorted for: {swSequential.ElapsedMilliseconds}ms");

            ISorter<int> parallel_merge_sorter = new ParallelMergeSort<int>();
            Console.WriteLine($"\n::Starting: {parallel_merge_sorter.Name}");

            swParallel = Stopwatch.StartNew();
            parallel_merge_sorter.Sort(array_for_mergesort_parallel);
            swParallel.Stop();

            Console.WriteLine($"Sorted for: {swParallel.ElapsedMilliseconds}ms");
            Console.WriteLine($"Parallel version outperforms Sequential by a {(int)(100 * ((double)swSequential.ElapsedMilliseconds / swParallel.ElapsedMilliseconds - 1))}%");

            // ----------------------------------------------------
            // Тестування алгоритму CountingSort
            // ----------------------------------------------------
            ISorter<int> sequential_counting_sorter = new SequentialCountingSort();
            Console.WriteLine($"\n::Starting: {sequential_counting_sorter.Name}");

            swSequential = Stopwatch.StartNew();
            sequential_counting_sorter.Sort(array_for_countingsort_sequential);
            swSequential.Stop();

            Console.WriteLine($"Sorted for: {swSequential.ElapsedMilliseconds}ms");

            ISorter<int> parallel_counting_sorter = new ParallelCountingSort();
            Console.WriteLine($"\n::Starting: {parallel_counting_sorter.Name}");

            swParallel = Stopwatch.StartNew();
            parallel_counting_sorter.Sort(array_for_countingsort_parallel);
            swParallel.Stop();

            Console.WriteLine($"Sorted for: {swParallel.ElapsedMilliseconds}ms");
            Console.WriteLine($"Parallel version outperforms Sequential by a {(int)(100 * ((double)swSequential.ElapsedMilliseconds / swParallel.ElapsedMilliseconds - 1))}%");

            // ----------------------------------------------------
            // Тестування алгоритму TreeSort
            // ----------------------------------------------------
            ISorter<int> sequential_treeNode_sorter = new SequentialTreeNodeSort<int>();
            Console.WriteLine($"\n::Starting: {sequential_treeNode_sorter.Name}");

            swSequential = Stopwatch.StartNew();
            sequential_treeNode_sorter.Sort(array_for_treeNodesort_sequential);
            swSequential.Stop();

            Console.WriteLine($"Sorted for: {swSequential.ElapsedMilliseconds}ms");

            ISorter<int> parallel_treeNode_sorter = new ParallelTreeNodeSort<int>();
            Console.WriteLine($"\n::Starting: {parallel_treeNode_sorter.Name}");

            swParallel = Stopwatch.StartNew();
            parallel_treeNode_sorter.Sort(array_for_treeNodesort_parallel);
            swParallel.Stop();

            Console.WriteLine($"Sorted for: {swParallel.ElapsedMilliseconds}ms");
            Console.WriteLine($"Parallel version outperforms Sequential by a {(int)(100 * ((double)swSequential.ElapsedMilliseconds / swParallel.ElapsedMilliseconds - 1))}%");

            // ----------------------------------------------------
            // Тестування алгоритму CocktailSort
            // ----------------------------------------------------
            ISorter<int> sequential_cocktrail_sorter = new SequentialCocktrailSort<int>();
            Console.WriteLine($"\n::Starting: {sequential_cocktrail_sorter.Name}");

            swSequential = Stopwatch.StartNew();
            sequential_cocktrail_sorter.Sort(array_for_cocktrailsort_sequential);
            swSequential.Stop();

            Console.WriteLine($"Sorted for: {swSequential.ElapsedMilliseconds}ms");

            ISorter<int> parallel_cocktrail_sorter = new ParallelCocktrailSort<int>();
            Console.WriteLine($"\n::Starting: {parallel_cocktrail_sorter.Name}");

            swParallel = Stopwatch.StartNew();
            parallel_cocktrail_sorter.Sort(array_for_cocktrailsort_parallel);
            swParallel.Stop();

            Console.WriteLine($"Sorted for: {swParallel.ElapsedMilliseconds}ms");
            Console.WriteLine($"Parallel version outperforms Sequential by a {(int)(100 * ((double)swSequential.ElapsedMilliseconds / swParallel.ElapsedMilliseconds - 1))}%");
            
            Console.ReadLine();
        }

        /// <summary>
        /// Генерує масив псевдовипадкових цілих чисел заданого розміру.
        /// Використовує фіксований seed (42) для забезпечення відтворюваності результатів при різних запусках програми.
        /// </summary>
        /// <param name="size">Кількість елементів у масиві.</param>
        /// <returns>Масив, заповнений випадковими цілими числами у діапазоні від 0 до 1 000 000.</returns>
        static int[] GenerateRandomArray(int size)
        {
            Random rand = new Random(42);
            int[] arr = new int[size];

            for (int i = 0; i < size; i++)
            {
                arr[i] = rand.Next(0, 1_000_000);
            }
            return arr;
        }
    }
}