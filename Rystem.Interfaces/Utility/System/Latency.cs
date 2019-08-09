using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Interfaces.Utility
{
    public class LatencyMeter
    {
        private long Offset { get; set; } = DateTime.UtcNow.Ticks;
        public long Ticks => DateTime.UtcNow.Ticks - Offset;
        public double Millieconds => (double)Ticks / 10_000D;
        public double Seconds => (double)Ticks / 10_000_000D;
        public double Reset() => Offset = DateTime.UtcNow.Ticks;
    }
}
