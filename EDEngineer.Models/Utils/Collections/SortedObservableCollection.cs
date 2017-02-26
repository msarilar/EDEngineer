using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EDEngineer.Models.Utils.Collections
{
    public class SortedObservableCollection<T> : ObservableCollection<T>
    {
        private Func<T, T, int> func;

        public void RefreshSort()
        {
            var temp = this.ToList();
            Clear();
            foreach (var item in temp)
            {
                Add(item);
            }
        }

        public void RefreshSort(Func<T, T, int> comparer)
        {
            func = comparer;
            var temp = this.ToList();
            Clear();
            foreach (var item in temp)
            {
                Add(item);
            }
        }

        public SortedObservableCollection(Func<T, T, int> comparer)
        {
            func = comparer;
        }
 
        protected override void InsertItem(int a, T item)
        {
            if (sorting)
            {
                base.InsertItem(a, item);
            }
            else
            {
                var idx = BinarySearch(item);
                base.InsertItem(idx, item);
            }
        }

        private bool sorting = false;
        public void SortInPlace()
        {
            sorting = true;
            InsertionSort();
            sorting = false;
        }

        private void InsertionSort()
        {
            for (var counter = 0; counter < Count - 1; counter++)
            {
                var index = counter + 1;
                while (index > 0)
                {
                    if (func(this[index - 1], this[index]) > 0)
                    {
                        var leftValue = this[index - 1];
                        var rightValue = this[index];

                        RemoveAt(index - 1);
                        InsertItem(index - 1, rightValue);
                        RemoveAt(index);
                        InsertItem(index, leftValue);
                    }
                    index--;
                }
            }
        }

        private int BinarySearch(T value)
        {
            var min = 0;
            var max = Count - 1;

            while (min <= max)
            {
                var mid = min + ((max - min) >> 1);

                var c = func(Items[mid], value);

                if (c == 0)
                    return mid;

                if (c < 0)
                {
                    min = mid + 1;
                }
                else
                {
                    max = mid - 1;
                }
            }
            return min;
        }
    }
}