using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Utility
{
    internal class ThreadMetric
    {
        private readonly Queue<int> Buffer = new Queue<int>();
        private readonly Queue<int> BufferIOC = new Queue<int>();
        private readonly int Max;
        public ThreadMetric(int max)
            => this.Max = max;
        public void AddDifference(int value)
        {
            this.Buffer.Enqueue(value);
            if (this.Buffer.Count > this.Max)
                this.Buffer.Dequeue();
        }
        public void AddDifferenceIOC(int value)
        {
            this.BufferIOC.Enqueue(value);
            if (this.BufferIOC.Count > this.Max)
                this.BufferIOC.Dequeue();
        }
        public int BufferCount => this.Buffer.Count;
        public int BufferIOCCount => this.BufferIOC.Count;
        public IEnumerable<int> GetBuffer()
            => this.Buffer.AsEnumerable();
        public IEnumerable<int> GetBufferIOC()
           => this.BufferIOC.AsEnumerable();
    }
}
