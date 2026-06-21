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
            Dictionary<string, List<BinanceCandle>> candlesSnapshot;

            lock (syncRoot)
            {
                symbolsSnapshot = activeSymbols.ToList();
                candlesSnapshot = new Dictionary<string, List<BinanceCandle>>(candlesBySymbol, StringComparer.OrdinalIgnoreCase);
            }

            var symbolsByCode = symbolsSnapshot.ToDictionary(x => x.Symbol, StringComparer.OrdinalIgnoreCase);

            var result = new List<BinanceSymbol>();

            foreach (var forced in BinanceConstants.ForcedSymbols)
            {
                if (symbolsByCode.TryGetValue(forced, out var forcedSymbol))
                {
                    result.Add(forcedSymbol);
                }
            }

            foreach (var symbol in symbolsSnapshot)
            {
                if (result.Any(x => x.Symbol.Equals(symbol.Symbol, StringComparison.OrdinalIgnoreCase)))
                    continue;

                if (!candlesSnapshot.ContainsKey(symbol.Symbol))
                    continue;

                if (MatchesFilter(symbol, candlesSnapshot[symbol.Symbol]))
                {
                    result.Add(symbol);
                }
            }

            return result
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

        private bool MatchesFilter(BinanceSymbol symbol, List<BinanceCandle> candles)
        {
            // Заглушка до следующей итерации.
            // Сейчас показываем только forced symbols.
            return false;
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
    }
}