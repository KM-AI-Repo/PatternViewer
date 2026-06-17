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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.listBoxSymbols = new System.Windows.Forms.ListBox();
            this.comboBoxInterval = new System.Windows.Forms.ComboBox();
            this.btnReloadSymbols = new System.Windows.Forms.Button();
            this.chartCandles = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.chartCandles)).BeginInit();
            this.SuspendLayout();
            // 
            // listBoxSymbols
            // 
            this.listBoxSymbols.FormattingEnabled = true;
            this.listBoxSymbols.ItemHeight = 16;
            this.listBoxSymbols.Location = new System.Drawing.Point(12, 44);
            this.listBoxSymbols.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listBoxSymbols.Name = "listBoxSymbols";
            this.listBoxSymbols.Size = new System.Drawing.Size(237, 916);
            this.listBoxSymbols.TabIndex = 0;
            // 
            // comboBoxInterval
            // 
            this.comboBoxInterval.FormattingEnabled = true;
            this.comboBoxInterval.Location = new System.Drawing.Point(12, 10);
            this.comboBoxInterval.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxInterval.Name = "comboBoxInterval";
            this.comboBoxInterval.Size = new System.Drawing.Size(121, 24);
            this.comboBoxInterval.TabIndex = 1;
            // 
            // btnReloadSymbols
            // 
            this.btnReloadSymbols.Location = new System.Drawing.Point(140, 10);
            this.btnReloadSymbols.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnReloadSymbols.Name = "btnReloadSymbols";
            this.btnReloadSymbols.Size = new System.Drawing.Size(151, 26);
            this.btnReloadSymbols.TabIndex = 2;
            this.btnReloadSymbols.Text = "Reload Symbols";
            this.btnReloadSymbols.UseVisualStyleBackColor = true;
            // 
            // chartCandles
            // 
            chartArea2.Name = "ChartArea1";
            this.chartCandles.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.chartCandles.Legends.Add(legend2);
            this.chartCandles.Location = new System.Drawing.Point(256, 42);
            this.chartCandles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chartCandles.Name = "chartCandles";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chartCandles.Series.Add(series2);
            this.chartCandles.Size = new System.Drawing.Size(1508, 930);
            this.chartCandles.TabIndex = 4;
            this.chartCandles.Text = "chart1";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(297, 14);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(44, 16);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Status";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1779, 986);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.chartCandles);
            this.Controls.Add(this.btnReloadSymbols);
            this.Controls.Add(this.comboBoxInterval);
            this.Controls.Add(this.listBoxSymbols);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.chartCandles)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxSymbols;
        private System.Windows.Forms.ComboBox comboBoxInterval;
        private System.Windows.Forms.Button btnReloadSymbols;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartCandles;
        private System.Windows.Forms.Label lblStatus;
    }
}

