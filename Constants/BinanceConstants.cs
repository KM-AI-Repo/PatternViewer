using System;

namespace BinanceFuturesViewer.Constants
{
    internal class BinanceConstants
    {
        public const string QuoteAssetUsdt = "USDT";
        public const string ContractTypePerpetual = "PERPETUAL";
        public const string SymbolStatusTrading = "TRADING";

        public const string ChartAreaName = "MainArea";
        public const string CandleSeriesName = "Candles";

        public static readonly TimeSpan StartCooldown = TimeSpan.FromMinutes(1);

        public static readonly string[] ForcedSymbols =
        {
            "BTCUSDT",
            "ETHUSDT"
        };
    }
}
