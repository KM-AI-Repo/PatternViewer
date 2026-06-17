using Newtonsoft.Json;
using System.Collections.Generic;

namespace BinanceFuturesViewer.Models
{
    internal class BinanceExchangeInfo
    {
        [JsonProperty("symbols")]
        public List<BinanceSymbol> Symbols { get; set; }
    }
}
