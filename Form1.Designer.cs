namespace BinanceFuturesViewer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.listBoxSymbols = new System.Windows.Forms.ListBox();
            this.comboBoxInterval = new System.Windows.Forms.ComboBox();
            this.chartCandles = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblSimilarityWindow = new System.Windows.Forms.Label();
            this.numericComparisonWindow = new System.Windows.Forms.NumericUpDown();
            this.lblSimilarityThreshold = new System.Windows.Forms.Label();
            this.numericDistanceThreshold = new System.Windows.Forms.NumericUpDown();
            this.checkBoxShowSimilar = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.chartCandles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericComparisonWindow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDistanceThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // listBoxSymbols
            // 
            this.listBoxSymbols.FormattingEnabled = true;
            this.listBoxSymbols.Location = new System.Drawing.Point(9, 36);
            this.listBoxSymbols.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listBoxSymbols.Name = "listBoxSymbols";
            this.listBoxSymbols.Size = new System.Drawing.Size(179, 758);
            this.listBoxSymbols.TabIndex = 0;
            // 
            // comboBoxInterval
            // 
            this.comboBoxInterval.FormattingEnabled = true;
            this.comboBoxInterval.Location = new System.Drawing.Point(9, 8);
            this.comboBoxInterval.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBoxInterval.Name = "comboBoxInterval";
            this.comboBoxInterval.Size = new System.Drawing.Size(92, 21);
            this.comboBoxInterval.TabIndex = 1;
            // 
            // chartCandles
            // 
            chartArea4.Name = "ChartArea1";
            this.chartCandles.ChartAreas.Add(chartArea4);
            legend4.Name = "Legend1";
            this.chartCandles.Legends.Add(legend4);
            this.chartCandles.Location = new System.Drawing.Point(192, 36);
            this.chartCandles.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.chartCandles.Name = "chartCandles";
            series4.ChartArea = "ChartArea1";
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            this.chartCandles.Series.Add(series4);
            this.chartCandles.Size = new System.Drawing.Size(1131, 757);
            this.chartCandles.TabIndex = 4;
            this.chartCandles.Text = "chart1";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(676, 11);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Status";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(554, 6);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(56, 21);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(614, 6);
            this.btnStop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(56, 21);
            this.btnStop.TabIndex = 7;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // lblSimilarityWindow
            // 
            this.lblSimilarityWindow.AutoSize = true;
            this.lblSimilarityWindow.Location = new System.Drawing.Point(104, 11);
            this.lblSimilarityWindow.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSimilarityWindow.Name = "lblSimilarityWindow";
            this.lblSimilarityWindow.Size = new System.Drawing.Size(46, 13);
            this.lblSimilarityWindow.TabIndex = 8;
            this.lblSimilarityWindow.Text = "Window";
            // 
            // numericComparisonWindow
            // 
            this.numericComparisonWindow.Location = new System.Drawing.Point(155, 8);
            this.numericComparisonWindow.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericComparisonWindow.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericComparisonWindow.Name = "numericComparisonWindow";
            this.numericComparisonWindow.Size = new System.Drawing.Size(120, 20);
            this.numericComparisonWindow.TabIndex = 9;
            this.numericComparisonWindow.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // lblSimilarityThreshold
            // 
            this.lblSimilarityThreshold.AutoSize = true;
            this.lblSimilarityThreshold.Location = new System.Drawing.Point(281, 10);
            this.lblSimilarityThreshold.Name = "lblSimilarityThreshold";
            this.lblSimilarityThreshold.Size = new System.Drawing.Size(54, 13);
            this.lblSimilarityThreshold.TabIndex = 10;
            this.lblSimilarityThreshold.Text = "Threshold";
            // 
            // numericDistanceThreshold
            // 
            this.numericDistanceThreshold.DecimalPlaces = 4;
            this.numericDistanceThreshold.Increment = new decimal(new int[] {
            5,
            0,
            0,
            196608});
            this.numericDistanceThreshold.Location = new System.Drawing.Point(341, 8);
            this.numericDistanceThreshold.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericDistanceThreshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numericDistanceThreshold.Name = "numericDistanceThreshold";
            this.numericDistanceThreshold.Size = new System.Drawing.Size(120, 20);
            this.numericDistanceThreshold.TabIndex = 11;
            this.numericDistanceThreshold.Value = new decimal(new int[] {
            3,
            0,
            0,
            131072});
            // 
            // checkBoxShowSimilar
            // 
            this.checkBoxShowSimilar.AutoSize = true;
            this.checkBoxShowSimilar.Location = new System.Drawing.Point(466, 9);
            this.checkBoxShowSimilar.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBoxShowSimilar.Name = "checkBoxShowSimilar";
            this.checkBoxShowSimilar.Size = new System.Drawing.Size(84, 17);
            this.checkBoxShowSimilar.TabIndex = 12;
            this.checkBoxShowSimilar.Text = "Show similar";
            this.checkBoxShowSimilar.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 805);
            this.Controls.Add(this.checkBoxShowSimilar);
            this.Controls.Add(this.numericDistanceThreshold);
            this.Controls.Add(this.lblSimilarityThreshold);
            this.Controls.Add(this.numericComparisonWindow);
            this.Controls.Add(this.lblSimilarityWindow);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.chartCandles);
            this.Controls.Add(this.comboBoxInterval);
            this.Controls.Add(this.listBoxSymbols);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.chartCandles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericComparisonWindow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDistanceThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxSymbols;
        private System.Windows.Forms.ComboBox comboBoxInterval;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartCandles;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblSimilarityWindow;
        private System.Windows.Forms.NumericUpDown numericComparisonWindow;
        private System.Windows.Forms.Label lblSimilarityThreshold;
        private System.Windows.Forms.NumericUpDown numericDistanceThreshold;
        private System.Windows.Forms.CheckBox checkBoxShowSimilar;
    }
}

