using Newtonsoft.Json;

namespace BinanceFuturesViewer.Models
{
    internal class BinanceCombinedStreamMessage<T>
    {
        [JsonProperty("stream")]
        public string Stream { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
