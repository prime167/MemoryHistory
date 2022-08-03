namespace MemHistoryWinform
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.formsPlot1 = new ScottPlot.FormsPlot();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.formsPlot3 = new ScottPlot.FormsPlot();
            this.formsPlot2 = new ScottPlot.FormsPlot();
            this.formsPlot4 = new ScottPlot.FormsPlot();
            this.formsPlot5 = new ScottPlot.FormsPlot();
            this.formsPlot6 = new ScottPlot.FormsPlot();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // formsPlot1
            // 
            this.formsPlot1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlot1.Location = new System.Drawing.Point(4, 3);
            this.formsPlot1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.formsPlot1.Name = "formsPlot1";
            this.formsPlot1.Size = new System.Drawing.Size(489, 256);
            this.formsPlot1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.formsPlot1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.formsPlot3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.formsPlot2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.formsPlot4, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.formsPlot5, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.formsPlot6, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(994, 788);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // formsPlot3
            // 
            this.formsPlot3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlot3.Location = new System.Drawing.Point(4, 265);
            this.formsPlot3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.formsPlot3.Name = "formsPlot3";
            this.formsPlot3.Size = new System.Drawing.Size(489, 256);
            this.formsPlot3.TabIndex = 1;
            // 
            // formsPlot2
            // 
            this.formsPlot2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlot2.Location = new System.Drawing.Point(501, 3);
            this.formsPlot2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.formsPlot2.Name = "formsPlot2";
            this.formsPlot2.Size = new System.Drawing.Size(489, 256);
            this.formsPlot2.TabIndex = 2;
            // 
            // formsPlot4
            // 
            this.formsPlot4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlot4.Location = new System.Drawing.Point(501, 265);
            this.formsPlot4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.formsPlot4.Name = "formsPlot4";
            this.formsPlot4.Size = new System.Drawing.Size(489, 256);
            this.formsPlot4.TabIndex = 3;
            // 
            // formsPlot5
            // 
            this.formsPlot5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlot5.Location = new System.Drawing.Point(4, 527);
            this.formsPlot5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.formsPlot5.Name = "formsPlot5";
            this.formsPlot5.Size = new System.Drawing.Size(489, 258);
            this.formsPlot5.TabIndex = 4;
            // 
            // formsPlot6
            // 
            this.formsPlot6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsPlot6.Location = new System.Drawing.Point(501, 527);
            this.formsPlot6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.formsPlot6.Name = "formsPlot6";
            this.formsPlot6.Size = new System.Drawing.Size(489, 258);
            this.formsPlot6.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(994, 788);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ScottPlot.FormsPlot formsPlot1;
        private TableLayoutPanel tableLayoutPanel1;
        private ScottPlot.FormsPlot formsPlot3;
        private ScottPlot.FormsPlot formsPlot2;
        private ScottPlot.FormsPlot formsPlot4;
        private ScottPlot.FormsPlot formsPlot5;
        private ScottPlot.FormsPlot formsPlot6;
    }
}