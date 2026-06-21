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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.listBoxSymbols = new System.Windows.Forms.ListBox();
            this.comboBoxInterval = new System.Windows.Forms.ComboBox();
            this.chartCandles = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chartCandles)).BeginInit();
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
            chartArea1.Name = "ChartArea1";
            this.chartCandles.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chartCandles.Legends.Add(legend1);
            this.chartCandles.Location = new System.Drawing.Point(192, 36);
            this.chartCandles.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.chartCandles.Name = "chartCandles";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartCandles.Series.Add(series1);
            this.chartCandles.Size = new System.Drawing.Size(1131, 757);
            this.chartCandles.TabIndex = 4;
            this.chartCandles.Text = "chart1";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(226, 12);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Status";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(105, 8);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(56, 21);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(165, 8);
            this.btnStop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(56, 21);
            this.btnStop.TabIndex = 7;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1334, 805);
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
    }
}

