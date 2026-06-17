using Newtonsoft.Json;

namespace BinanceFuturesViewer.Models
{
    internal class BinanceKlineStreamMessage
    {
        [JsonProperty("k")]
        public BinanceKlineData Kline { get; set; }
    }
}
