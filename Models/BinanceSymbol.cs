using Newtonsoft.Json;

namespace BinanceFuturesViewer.Models
{
    internal class BinanceSymbol
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("baseAsset")]
        public string BaseAsset { get; set; }

        [JsonProperty("quoteAsset")]
        public string QuoteAsset { get; set; }

        [JsonProperty("contractType")]
        public string ContractType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        public override string ToString()
        {
            return Symbol;
        }
    }
}
