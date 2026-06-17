using Newtonsoft.Json;

namespace BinanceFuturesViewer.Models
{
    internal class BinanceKlineData
    {
        [JsonProperty("t")]
        public long OpenTime { get; set; }

        [JsonProperty("T")]
        public long CloseTime { get; set; }

        [JsonProperty("o")]
        public string Open { get; set; }

        [JsonProperty("h")]
        public string High { get; set; }

        [JsonProperty("l")]
        public string Low { get; set; }

        [JsonProperty("c")]
        public string Close { get; set; }

        [JsonProperty("x")]
        public bool IsClosed { get; set; }
    }
}
