using BinanceFuturesViewer.Constants;
using BinanceFuturesViewer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BinanceFuturesViewer.Services
{
    internal class BinanceRestService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<List<BinanceSymbol>> GetActiveUsdtPerpetualSymbolsAsync()
        {
            string url = Properties.Settings.Default.FuturesRestBaseUrl
                       + Properties.Settings.Default.ExchangeInfoEndpoint;

            string json = await httpClient.GetStringAsync(url);

            var exchangeInfo = JsonConvert.DeserializeObject<BinanceExchangeInfo>(json);

            return exchangeInfo.Symbols
                .Where(s =>
                    s.ContractType == BinanceConstants.ContractTypePerpetual &&
                    s.QuoteAsset == BinanceConstants.QuoteAssetUsdt &&
                    s.Status == BinanceConstants.SymbolStatusTrading)
                .OrderBy(s => s.Symbol)
                .ToList();
        }

        public async Task<List<BinanceCandle>> GetKlinesAsync(string symbol, string interval, int limit)
        {
            string url = $"{Properties.Settings.Default.FuturesRestBaseUrl}" +
                         $"{Properties.Settings.Default.KlinesEndpoint}" +
                         $"?symbol={symbol}&interval={interval}&limit={limit}";

            string json = await httpClient.GetStringAsync(url);
            var rawKlines = JsonConvert.DeserializeObject<List<List<object>>>(json);

            return rawKlines.Select(item => new BinanceCandle
            {
                OpenTimeMs = Convert.ToInt64(item[0]),
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(item[0])).LocalDateTime,
                Open = decimal.Parse(item[1].ToString(), CultureInfo.InvariantCulture),
                High = decimal.Parse(item[2].ToString(), CultureInfo.InvariantCulture),
                Low = decimal.Parse(item[3].ToString(), CultureInfo.InvariantCulture),
                Close = decimal.Parse(item[4].ToString(), CultureInfo.InvariantCulture),
                CloseTimeMs = Convert.ToInt64(item[6]),
                IsClosed = true
            }).ToList();
        }
    }
}
