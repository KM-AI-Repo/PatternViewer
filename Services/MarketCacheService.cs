using BinanceFuturesViewer.Constants;
using BinanceFuturesViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceFuturesViewer.Services
{
    internal class MarketCacheService
    {
        private readonly BinanceRestService restService = new BinanceRestService();
        private readonly object syncRoot = new object();

        private Dictionary<string, List<BinanceCandle>> candlesBySymbol =
            new Dictionary<string, List<BinanceCandle>>(StringComparer.OrdinalIgnoreCase);

        private List<BinanceSymbol> activeSymbols =
            new List<BinanceSymbol>();

        private int comparisonWindow = 50;
        private decimal distanceThreshold = 0.03m;

        public async Task InitializeAsync(string interval, int candleLimit)
        {
            var symbols = await restService.GetActiveUsdtPerpetualSymbolsAsync();

            var loadTasks = symbols
                .Select(symbol => LoadSymbolCandlesSafeAsync(symbol, interval, candleLimit))
                .ToList();

            var results = await Task.WhenAll(loadTasks);

            var newCache = results
                .Where(x => x.Success)
                .ToDictionary(
                    x => x.Symbol.Symbol,
                    x => x.Candles,
                    StringComparer.OrdinalIgnoreCase);

            lock (syncRoot)
            {
                activeSymbols = symbols;
                candlesBySymbol = newCache;
            }
        }

        public List<BinanceSymbol> GetDisplaySymbols()
        {
            List<BinanceSymbol> symbolsSnapshot;

            lock (syncRoot)
            {
                symbolsSnapshot = activeSymbols.ToList();
            }

            return symbolsSnapshot
                .Where(x => ShouldDisplaySymbol(x.Symbol))
                .OrderBy(x => x.Symbol)
                .ToList();
        }

        public List<BinanceCandle> GetCandles(string symbol)
        {
            lock (syncRoot)
            {
                if (!candlesBySymbol.TryGetValue(symbol, out var candles))
                    return new List<BinanceCandle>();

                return candles
                    .OrderBy(x => x.OpenTimeMs)
                    .ToList();
            }
        }

        public bool HasSymbol(string symbol)
        {
            lock (syncRoot)
            {
                return candlesBySymbol.ContainsKey(symbol);
            }
        }

        private async Task<SymbolCandlesLoadResult> LoadSymbolCandlesSafeAsync(
            BinanceSymbol symbol,
            string interval,
            int candleLimit)
        {
            try
            {
                var candles = await restService.GetKlinesAsync(symbol.Symbol, interval, candleLimit);

                return new SymbolCandlesLoadResult
                {
                    Symbol = symbol,
                    Candles = candles,
                    Success = true
                };
            }
            catch
            {
                return new SymbolCandlesLoadResult
                {
                    Symbol = symbol,
                    Candles = new List<BinanceCandle>(),
                    Success = false
                };
            }
        }

        private bool MatchesFilter(string symbol)
        {
            decimal minDistance;
            return IsIndependentFromMarketLeaders(symbol, out minDistance);
        }

        private bool IsIndependentFromMarketLeaders(string symbol, out decimal minDistance)
        {
            minDistance = decimal.MaxValue;

            if (string.IsNullOrWhiteSpace(symbol))
                return false;

            List<BinanceCandle> symbolSeries;
            List<BinanceCandle> btcSeries;
            List<BinanceCandle> ethSeries;
            int window;
            decimal threshold;

            lock (syncRoot)
            {
                if (!candlesBySymbol.TryGetValue(symbol, out symbolSeries) || symbolSeries == null)
                    return false;

                if (!candlesBySymbol.TryGetValue("BTCUSDT", out btcSeries) || btcSeries == null)
                    return false;

                if (!candlesBySymbol.TryGetValue("ETHUSDT", out ethSeries) || ethSeries == null)
                    return false;

                symbolSeries = symbolSeries
                    .OrderBy(x => x.OpenTimeMs)
                    .ToList();

                btcSeries = btcSeries
                    .OrderBy(x => x.OpenTimeMs)
                    .ToList();

                ethSeries = ethSeries
                    .OrderBy(x => x.OpenTimeMs)
                    .ToList();

                window = comparisonWindow;
                threshold = distanceThreshold;
            }

            var symbolWindow = GetLastWindow(symbolSeries, window);
            var btcWindow = GetLastWindow(btcSeries, window);
            var ethWindow = GetLastWindow(ethSeries, window);

            if (symbolWindow == null || btcWindow == null || ethWindow == null)
                return false;

            decimal distanceToBtc = CalculateNormalizedPathDistance(symbolWindow, btcWindow);
            decimal distanceToEth = CalculateNormalizedPathDistance(symbolWindow, ethWindow);

            minDistance = Math.Min(distanceToBtc, distanceToEth);

            return minDistance > threshold;
        }

        private class SymbolCandlesLoadResult
        {
            public BinanceSymbol Symbol { get; set; }
            public List<BinanceCandle> Candles { get; set; }
            public bool Success { get; set; }
        }

        public List<string> GetTrackedSymbols()
        {
            lock (syncRoot)
            {
                return candlesBySymbol.Keys
                    .OrderBy(x => x)
                    .ToList();
            }
        }

        public MarketCandleUpdateResult UpdateCandle(string symbol, BinanceCandle incoming, int candleLimit)
        {
            if (string.IsNullOrWhiteSpace(symbol) || incoming == null)
                return MarketCandleUpdateResult.Ignored;

            lock (syncRoot)
            {
                if (!candlesBySymbol.TryGetValue(symbol, out var candles))
                    return MarketCandleUpdateResult.Ignored;

                candles = candles
                    .OrderBy(x => x.OpenTimeMs)
                    .ToList();

                int existingIndex = candles.FindIndex(x => x.OpenTimeMs == incoming.OpenTimeMs);

                if (existingIndex >= 0)
                {
                    candles[existingIndex] = incoming;
                    candlesBySymbol[symbol] = candles;
                    return MarketCandleUpdateResult.UpdatedExisting;
                }

                if (candles.Count == 0)
                {
                    candles.Add(incoming);
                    candlesBySymbol[symbol] = candles;
                    return MarketCandleUpdateResult.Appended;
                }

                var last = candles[candles.Count - 1];

                bool isNext = incoming.OpenTimeMs == last.CloseTimeMs + 1;
                bool previousClosed = last.IsClosed;

                if (!isNext || !previousClosed)
                {
                    return MarketCandleUpdateResult.ResyncRequired;
                }

                candles.Add(incoming);

                candles = candles
                    .OrderBy(x => x.OpenTimeMs)
                    .Skip(Math.Max(0, candles.Count - candleLimit))
                    .ToList();

                candlesBySymbol[symbol] = candles;
                return MarketCandleUpdateResult.Appended;
            }
        }

        public void ReplaceSymbolCandles(string symbol, List<BinanceCandle> candles)
        {
            if (string.IsNullOrWhiteSpace(symbol) || candles == null)
                return;

            lock (syncRoot)
            {
                candlesBySymbol[symbol] = candles
                    .OrderBy(x => x.OpenTimeMs)
                    .ToList();
            }
        }

        public async Task<bool> TryResyncSymbolAsync(string symbol, string interval, int candleLimit)
        {
            if (string.IsNullOrWhiteSpace(symbol) || string.IsNullOrWhiteSpace(interval))
                return false;

            if (!HasSymbol(symbol))
                return false;

            try
            {
                var candles = await restService.GetKlinesAsync(symbol, interval, candleLimit);
                ReplaceSymbolCandles(symbol, candles);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ShouldDisplaySymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return false;

            if (BinanceConstants.ForcedSymbols.Any(x =>
                x.Equals(symbol, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return MatchesFilter(symbol);
        }

        public BinanceSymbol GetSymbolByCode(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            lock (syncRoot)
            {
                return activeSymbols.FirstOrDefault(x =>
                    x.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
            }
        }

        public void SetSimilarityFilterSettings(int window, decimal threshold)
        {
            if (window < 2)
                window = 2;

            if (threshold < 0)
                threshold = 0;

            comparisonWindow = window;
            distanceThreshold = threshold;
        }

        private List<BinanceCandle> GetLastWindow(List<BinanceCandle> source, int window)
        {
            if (source == null || source.Count < window)
                return null;

            return source
                .OrderBy(x => x.OpenTimeMs)
                .Skip(source.Count - window)
                .ToList();
        }

        private List<decimal> BuildNormalizedClosePath(List<BinanceCandle> candles)
        {
            if (candles == null || candles.Count == 0)
                return null;

            decimal baseClose = candles[0].Close;

            if (baseClose == 0)
                return null;

            return candles
                .Select(x => (x.Close / baseClose) - 1m)
                .ToList();
        }

        private decimal CalculateNormalizedPathDistance(List<BinanceCandle> leftCandles, List<BinanceCandle> rightCandles)
        {
            var left = BuildNormalizedClosePath(leftCandles);
            var right = BuildNormalizedClosePath(rightCandles);

            if (left == null || right == null || left.Count != right.Count || left.Count == 0)
                return decimal.MaxValue;

            decimal sum = 0m;

            for (int i = 0; i < left.Count; i++)
            {
                sum += Math.Abs(left[i] - right[i]);
            }

            return sum / left.Count;
        }
    }
}