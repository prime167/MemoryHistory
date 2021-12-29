using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoryHistory
{
    public class SimpleMovingAverage:IMovingAverage
    {
        private readonly int _k;
        private readonly Queue<double> _values;

        public SimpleMovingAverage(int k)
        {
            if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k), "Must be greater than 0");

            _k = k;
            _values = new Queue<double>(k);
        }

        public double Update(double nextInput)
        {
            if (_values.Count >= _k)
            {
                _values.Dequeue();
            }
            _values.Enqueue(nextInput);

            return _values.Average();
        }
    }
}
