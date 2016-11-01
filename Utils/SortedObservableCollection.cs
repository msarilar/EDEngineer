using System;
using System.Collections.ObjectModel;

namespace EDEngineer.Utils
{
    public class SortedObservableCollection<T> : ObservableCollection<T>
    {
        private readonly Func<T, T, int> func;

        public SortedObservableCollection(Func<T, T, int> comparer)
        {
            func = comparer;
        }

        protected override void InsertItem(int a, T item)
        {
            var idx = BinarySearch(item);
            base.InsertItem(idx, item);
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