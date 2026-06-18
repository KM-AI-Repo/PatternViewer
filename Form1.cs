using BinanceFuturesViewer.Constants;
using BinanceFuturesViewer.Models;
using BinanceFuturesViewer.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BinanceFuturesViewer
{
    public partial class Form1 : Form
    {
        private readonly BinanceRestService restService = new BinanceRestService();
        private readonly BinanceWebSocketService webSocketService = new BinanceWebSocketService();
        private readonly MarketCacheService marketCacheService = new MarketCacheService();
        private readonly object candlesLock = new object();

        private List<BinanceCandle> candles = new List<BinanceCandle>();

        private bool isUpdatingUi;
        private bool isLoadingChart;
        private bool isResyncInProgress;
        private bool isRunning;
        
        private DateTime? lastStartUtc;

        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;

            comboBoxInterval.SelectedIndexChanged += ComboBoxInterval_SelectedIndexChanged;
            listBoxSymbols.SelectedIndexChanged += ListBoxSymbols_SelectedIndexChanged;

            webSocketService.CandleReceived += WebSocketService_CandleReceived;
            webSocketService.StatusChanged += WebSocketService_StatusChanged;
            webSocketService.ErrorOccurred += WebSocketService_ErrorOccurred;

            listBoxSymbols.SelectionMode = SelectionMode.One;
            listBoxSymbols.DisplayMember = nameof(BinanceSymbol.Symbol);
            listBoxSymbols.ValueMember = nameof(BinanceSymbol.Symbol);

            comboBoxInterval.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeChart();
            InitializeIntervals();

            lock (candlesLock)
            {
                candles = new List<BinanceCandle>();
            }

            DrawCandles("-", GetSelectedInterval() ?? "1h");
            lblStatus.Text = "Готово к запуску";

            UpdateControlsState();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            webSocketService.Disconnect();
            isRunning = false;
        }

        private void InitializeIntervals()
        {
            comboBoxInterval.Items.Clear();
            comboBoxInterval.Items.AddRange(BinanceIntervals.All);

            string defaultInterval = Properties.Settings.Default.DefaultInterval;

            if (comboBoxInterval.Items.Contains(defaultInterval))
                comboBoxInterval.SelectedItem = defaultInterval;
            else
                comboBoxInterval.SelectedItem = "1h";
        }

        private void InitializeChart()
        {
            chartCandles.Series.Clear();
            chartCandles.ChartAreas.Clear();
            chartCandles.Titles.Clear();
            chartCandles.Legends.Clear();

            chartCandles.BackColor = Color.FromArgb(20, 20, 20);

            var area = new ChartArea(BinanceConstants.ChartAreaName);
            area.BackColor = Color.FromArgb(20, 20, 20);

            area.AxisX.MajorGrid.LineColor = Color.FromArgb(45, 45, 45);
            area.AxisX.LineColor = Color.Gray;
            area.AxisX.LabelStyle.ForeColor = Color.LightGray;
            area.AxisX.LabelStyle.Format = "dd.MM HH:mm";
            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;

            area.AxisY.MajorGrid.LineColor = Color.FromArgb(45, 45, 45);
            area.AxisY.LineColor = Color.Gray;
            area.AxisY.LabelStyle.ForeColor = Color.LightGray;
            area.AxisY.IsStartedFromZero = false;
            area.AxisY.LabelStyle.Format = "0.########";

            chartCandles.ChartAreas.Add(area);

            var series = new Series(BinanceConstants.CandleSeriesName);
            series.ChartType = SeriesChartType.Candlestick;
            series.ChartArea = BinanceConstants.ChartAreaName;
            series.XValueType = ChartValueType.DateTime;
            series["OpenCloseStyle"] = "Rectangle";
            series["ShowOpenClose"] = "Both";
            series["PointWidth"] = "0.8";
            series["PriceUpColor"] = "LimeGreen";
            series["PriceDownColor"] = "Red";

            chartCandles.Series.Add(series);
        }

        private void UpdateControlsState()
        {
            bool isBusy = isUpdatingUi || isLoadingChart || isResyncInProgress;

            btnStart.Enabled = !isBusy && !isRunning;
            btnStop.Enabled = !isBusy && isRunning;

            comboBoxInterval.Enabled = !isBusy && !isRunning;
            listBoxSymbols.Enabled = !isBusy && isRunning;
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (isUpdatingUi || isLoadingChart || isResyncInProgress || isRunning)
                return;

            if (lastStartUtc.HasValue)
            {
                TimeSpan elapsed = DateTime.UtcNow - lastStartUtc.Value;

                if (elapsed < BinanceConstants.StartCooldown)
                {
                    int secondsLeft = (int)Math.Ceiling((BinanceConstants.StartCooldown - elapsed).TotalSeconds);
                    lblStatus.Text = $"Повторный запуск будет доступен через {secondsLeft} сек.";
                    return;
                }
            }

            try
            {
                isRunning = true;
                lastStartUtc = DateTime.UtcNow;
                UpdateControlsState();

                await LoadSymbolsAsync();
            }
            catch (Exception ex)
            {
                isRunning = false;
                lblStatus.Text = "Ошибка запуска";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateControlsState();
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            if (!isRunning || isUpdatingUi || isLoadingChart || isResyncInProgress)
                return;

            webSocketService.Disconnect();

            isRunning = false;
            lblStatus.Text = "Остановлено";

            UpdateControlsState();
        }

        private async void ComboBoxInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isUpdatingUi || isLoadingChart || isResyncInProgress)
                return;

            string selectedInterval = GetSelectedInterval();

            if (!string.IsNullOrWhiteSpace(selectedInterval) &&
                !string.Equals(Properties.Settings.Default.DefaultInterval, selectedInterval, StringComparison.Ordinal))
            {
                Properties.Settings.Default.DefaultInterval = selectedInterval;
                Properties.Settings.Default.Save();
            }
        }

        private async void ListBoxSymbols_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isRunning || isUpdatingUi || isLoadingChart || isResyncInProgress)
                return;

            await ReloadChartAndStreamAsync();
        }

        private async Task LoadSymbolsAsync()
        {
            if (!isRunning)
                return;

            try
            {
                isUpdatingUi = true;
                UpdateControlsState();

                string interval = GetSelectedInterval();
                int candleLimit = Properties.Settings.Default.DefaultCandlesLimit;

                lblStatus.Text = $"Загрузка рынка ({interval}, {candleLimit} свечей на инструмент)...";

                await marketCacheService.InitializeAsync(interval, candleLimit);

                var symbols = marketCacheService.GetDisplaySymbols();

                listBoxSymbols.BeginUpdate();
                listBoxSymbols.DataSource = null;
                listBoxSymbols.Items.Clear();
                listBoxSymbols.DataSource = symbols;
                listBoxSymbols.DisplayMember = nameof(BinanceSymbol.Symbol);
                listBoxSymbols.ValueMember = nameof(BinanceSymbol.Symbol);
                listBoxSymbols.EndUpdate();

                if (symbols.Count > 0)
                {
                    string defaultSymbol = Properties.Settings.Default.DefaultSymbol;
                    var selected = symbols.FirstOrDefault(x => x.Symbol == defaultSymbol);

                    if (selected != null)
                        listBoxSymbols.SelectedItem = selected;
                    else
                        listBoxSymbols.SelectedIndex = 0;
                }
                else
                {
                    lock (candlesLock)
                    {
                        candles = new List<BinanceCandle>();
                    }

                    DrawCandles("-", GetSelectedInterval());
                }

                lblStatus.Text = $"Инструментов для показа: {symbols.Count}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Ошибка загрузки рынка";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isRunning = false;
                return;
            }
            finally
            {
                isUpdatingUi = false;
                UpdateControlsState();
            }

            if (isRunning)
                await ReloadChartAndStreamAsync();
        }

        private async Task ReloadChartAndStreamAsync()
        {
            if (!isRunning)
                return;

            string symbol = GetSelectedSymbolCode();
            string interval = GetSelectedInterval();

            if (string.IsNullOrWhiteSpace(symbol) || string.IsNullOrWhiteSpace(interval))
                return;

            try
            {
                isLoadingChart = true;
                UpdateControlsState();

                webSocketService.Disconnect();

                lblStatus.Text = $"Загрузка свечей: {symbol}, {interval}...";

                var loadedCandles = await restService.GetKlinesAsync(
                    symbol,
                    interval,
                    Properties.Settings.Default.DefaultCandlesLimit);

                lock (candlesLock)
                {
                    candles = loadedCandles
                        .OrderBy(x => x.OpenTimeMs)
                        .ToList();
                }

                DrawCandles(symbol, interval);
                webSocketService.Connect(symbol, interval);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Ошибка загрузки графика";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoadingChart = false;
                UpdateControlsState();
            }
        }

        private async void WebSocketService_CandleReceived(BinanceCandle incoming)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => WebSocketService_CandleReceived(incoming)));
                return;
            }

            if (!isRunning || isLoadingChart || isResyncInProgress)
                return;

            bool needResync = false;

            lock (candlesLock)
            {
                candles = candles
                    .OrderBy(c => c.OpenTimeMs)
                    .ToList();

                int existingIndex = candles.FindIndex(c => c.OpenTimeMs == incoming.OpenTimeMs);

                if (existingIndex >= 0)
                {
                    candles[existingIndex] = incoming;
                }
                else
                {
                    if (candles.Count == 0)
                    {
                        candles.Add(incoming);
                    }
                    else
                    {
                        var lastCandle = candles[candles.Count - 1];

                        bool isNextCandle = incoming.OpenTimeMs == lastCandle.CloseTimeMs + 1;

                        if (!isNextCandle)
                        {
                            needResync = true;
                        }
                        else if (!lastCandle.IsClosed)
                        {
                            needResync = true;
                        }
                        else
                        {
                            candles.Add(incoming);

                            candles = candles
                                .OrderBy(c => c.OpenTimeMs)
                                .Skip(Math.Max(0, candles.Count - Properties.Settings.Default.DefaultCandlesLimit))
                                .ToList();
                        }
                    }
                }
            }

            if (needResync)
            {
                await ResyncSelectedSymbolAsync("Обнаружено нарушение последовательности свечей, выполняется синхронизация...");
                return;
            }

            DrawCandles(GetSelectedSymbolCode(), GetSelectedInterval());
        }

        private async Task ResyncSelectedSymbolAsync(string statusMessage)
        {
            if (isResyncInProgress || !isRunning)
                return;

            try
            {
                isResyncInProgress = true;
                UpdateControlsState();

                lblStatus.Text = statusMessage;
                await ReloadChartAndStreamAsync();
            }
            finally
            {
                isResyncInProgress = false;
                UpdateControlsState();
            }
        }

        private void WebSocketService_StatusChanged(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => WebSocketService_StatusChanged(message)));
                return;
            }

            lblStatus.Text = message;
        }

        private void WebSocketService_ErrorOccurred(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => WebSocketService_ErrorOccurred(message)));
                return;
            }

            lblStatus.Text = "Ошибка WebSocket: " + message;
        }

        private void DrawCandles(string symbol, string interval)
        {
            List<BinanceCandle> snapshot;

            lock (candlesLock)
            {
                snapshot = candles
                    .OrderBy(c => c.OpenTimeMs)
                    .ToList();
            }

            var series = chartCandles.Series[BinanceConstants.CandleSeriesName];
            series.Points.Clear();

            foreach (var candle in snapshot)
            {
                int pointIndex = series.Points.AddXY(candle.OpenTime, candle.High);
                var point = series.Points[pointIndex];

                point.YValues = new[]
                {
                    (double)candle.High,
                    (double)candle.Low,
                    (double)candle.Open,
                    (double)candle.Close
                };

                point.Color = candle.Close >= candle.Open ? Color.LimeGreen : Color.Red;
            }

            chartCandles.ChartAreas[BinanceConstants.ChartAreaName].RecalculateAxesScale();

            chartCandles.Titles.Clear();
            chartCandles.Titles.Add($"{symbol} - {interval} - последние {snapshot.Count} свечей");
            chartCandles.Titles[0].ForeColor = Color.White;
            chartCandles.Titles[0].Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        private BinanceSymbol GetSelectedSymbol()
        {
            return listBoxSymbols.SelectedItem as BinanceSymbol;
        }

        public string GetSelectedSymbolCode()
        {
            return (listBoxSymbols.SelectedItem as BinanceSymbol)?.Symbol;
        }

        public string GetSelectedInterval()
        {
            return comboBoxInterval.SelectedItem?.ToString();
        }
    }
}