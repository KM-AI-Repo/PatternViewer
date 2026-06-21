namespace BinanceFuturesViewer.Models
{
    internal enum MarketCandleUpdateResult
    {
        Ignored = 0,
        UpdatedExisting = 1,
        Appended = 2,
        ResyncRequired = 3
    }
}
