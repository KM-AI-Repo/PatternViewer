using System;

namespace BinanceFuturesViewer.Models
{
    internal class BinanceCandle
    {
        public long OpenTimeMs { get; set; }
        public DateTime OpenTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }
}
