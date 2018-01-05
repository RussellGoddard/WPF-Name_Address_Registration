using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGoddard_Project_4
{
    public class FixedSizeObservableCollection<T> : ObservableCollection<T>
    {
        int MaxSize { get { return maxSize; } set { maxSize = value; } }
        int maxSize;

        new public void Add(T value)
        {
            if (this.Count == maxSize)
            {
                this.RemoveAt(0);
            }
            base.Add(value);
        }

        public FixedSizeObservableCollection(int initialSize = 4)
        {
            maxSize = initialSize;
        }
    }
}
