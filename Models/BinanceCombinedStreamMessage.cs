using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
