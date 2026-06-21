using BinanceFuturesViewer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WebSocketSharp;

namespace BinanceFuturesViewer.Services
{
    internal class MarketWebSocketService
    {
        private WebSocket ws;

        public event Action<string, BinanceCandle> CandleReceived;
        public event Action<string> StatusChanged;
        public event Action<string> ErrorOccurred;

        public void Connect(IEnumerable<string> symbols, string interval)
        {
            Disconnect();

            var streamNames = symbols
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => $"{x.ToLowerInvariant()}@kline_{interval}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (streamNames.Count == 0)
                return;

            string url = Properties.Settings.Default.FuturesWebSocketBaseUrl
                .Replace("/ws", "/stream")
                + "?streams="
                + string.Join("/", streamNames);

            ws = new WebSocket(url);

            ws.OnOpen += (s, e) =>
            {
                StatusChanged?.Invoke($"Market WebSocket подключен: {streamNames.Count} streams");
            };

            ws.OnMessage += (s, e) =>
            {
                try
                {
                    var combined = JsonConvert.DeserializeObject<BinanceCombinedStreamMessage<BinanceKlineStreamMessage>>(e.Data);
                    if (combined?.Data?.Kline == null)
                        return;

                    string stream = combined.Stream ?? string.Empty;
                    string symbol = ExtractSymbolFromStream(stream);

                    if (string.IsNullOrWhiteSpace(symbol))
                        return;

                    var candle = new BinanceCandle
                    {
                        OpenTimeMs = combined.Data.Kline.OpenTime,
                        CloseTimeMs = combined.Data.Kline.CloseTime,
                        OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(combined.Data.Kline.OpenTime).LocalDateTime,
                        Open = decimal.Parse(combined.Data.Kline.Open, CultureInfo.InvariantCulture),
                        High = decimal.Parse(combined.Data.Kline.High, CultureInfo.InvariantCulture),
                        Low = decimal.Parse(combined.Data.Kline.Low, CultureInfo.InvariantCulture),
                        Close = decimal.Parse(combined.Data.Kline.Close, CultureInfo.InvariantCulture),
                        IsClosed = combined.Data.Kline.IsClosed
                    };

                    CandleReceived?.Invoke(symbol, candle);
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(ex.Message);
                }
            };

            ws.OnError += (s, e) =>
            {
                ErrorOccurred?.Invoke(e.Message);
            };

            ws.OnClose += (s, e) =>
            {
                StatusChanged?.Invoke("Market WebSocket отключен");
            };

            ws.Connect();
        }

        public void Disconnect()
        {
            if (ws != null)
            {
                try
                {
                    ws.Close();
                }
                finally
                {
                    ws = null;
                }
            }
        }

        private string ExtractSymbolFromStream(string stream)
        {
            if (string.IsNullOrWhiteSpace(stream))
                return null;

            int atIndex = stream.IndexOf('@');
            if (atIndex <= 0)
                return null;

            return stream.Substring(0, atIndex).ToUpperInvariant();
        }
    }
}
