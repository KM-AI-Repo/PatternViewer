using BinanceFuturesViewer.Models;
using Newtonsoft.Json;
using System;
using System.Globalization;
using WebSocketSharp;

namespace BinanceFuturesViewer.Services
{
    internal class BinanceWebSocketService
    {
        private WebSocket ws;

        public event Action<BinanceCandle> CandleReceived;
        public event Action<string> StatusChanged;
        public event Action<string> ErrorOccurred;

        public void Connect(string symbol, string interval)
        {
            Disconnect();

            string streamName = $"{symbol.ToLowerInvariant()}@kline_{interval}";
            string url = Properties.Settings.Default.FuturesWebSocketBaseUrl + "/" + streamName;

            ws = new WebSocket(url);

            ws.OnOpen += (s, e) =>
            {
                StatusChanged?.Invoke($"WebSocket подключен: {symbol}, {interval}");
            };

            ws.OnMessage += (s, e) =>
            {
                try
                {
                    var message = JsonConvert.DeserializeObject<BinanceKlineStreamMessage>(e.Data);
                    if (message?.Kline == null)
                        return;

                    var candle = new BinanceCandle
                    {
                        OpenTimeMs = message.Kline.OpenTime,
                        OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(message.Kline.OpenTime).LocalDateTime,
                        Open = decimal.Parse(message.Kline.Open, CultureInfo.InvariantCulture),
                        High = decimal.Parse(message.Kline.High, CultureInfo.InvariantCulture),
                        Low = decimal.Parse(message.Kline.Low, CultureInfo.InvariantCulture),
                        Close = decimal.Parse(message.Kline.Close, CultureInfo.InvariantCulture)
                    };

                    CandleReceived?.Invoke(candle);
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
                StatusChanged?.Invoke("WebSocket отключен");
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
    }
}
