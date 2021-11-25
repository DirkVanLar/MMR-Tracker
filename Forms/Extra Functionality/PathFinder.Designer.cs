namespace MMR_Tracker.Forms
{
    partial class PathFinder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PathFinder));
            this.BTNFindPath = new System.Windows.Forms.Button();
            this.LBPathFinder = new System.Windows.Forms.ListBox();
            this.CMBEnd = new System.Windows.Forms.ComboBox();
            this.CMBStart = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSwapPathfinder = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BTNFindPath
            // 
            this.BTNFindPath.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BTNFindPath.Location = new System.Drawing.Point(76, 8);
            this.BTNFindPath.Name = "BTNFindPath";
            this.BTNFindPath.Size = new System.Drawing.Size(259, 20);
            this.BTNFindPath.TabIndex = 26;
            this.BTNFindPath.Text = "Find Path";
            this.BTNFindPath.UseVisualStyleBackColor = false;
            this.BTNFindPath.Click += new System.EventHandler(this.BTNFindPath_Click);
            // 
            // LBPathFinder
            // 
            this.LBPathFinder.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBPathFinder.FormattingEnabled = true;
            this.LBPathFinder.Location = new System.Drawing.Point(12, 88);
            this.LBPathFinder.Name = "LBPathFinder";
            this.LBPathFinder.Size = new System.Drawing.Size(323, 511);
            this.LBPathFinder.TabIndex = 25;
            this.LBPathFinder.DoubleClick += new System.EventHandler(this.LBPathFinder_DoubleClick);
            // 
            // CMBEnd
            // 
            this.CMBEnd.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CMBEnd.ForeColor = System.Drawing.SystemColors.InfoText;
            this.CMBEnd.FormattingEnabled = true;
            this.CMBEnd.Location = new System.Drawing.Point(76, 61);
            this.CMBEnd.Name = "CMBEnd";
            this.CMBEnd.Size = new System.Drawing.Size(259, 21);
            this.CMBEnd.TabIndex = 24;
            this.CMBEnd.DropDown += new System.EventHandler(this.CMBEnd_DropDown);
            // 
            // CMBStart
            // 
            this.CMBStart.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CMBStart.ForeColor = System.Drawing.SystemColors.InfoText;
            this.CMBStart.FormattingEnabled = true;
            this.CMBStart.Location = new System.Drawing.Point(76, 34);
            this.CMBStart.Name = "CMBStart";
            this.CMBStart.Size = new System.Drawing.Size(259, 21);
            this.CMBStart.TabIndex = 23;
            this.CMBStart.DropDown += new System.EventHandler(this.CMBStart_DropDown);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.ForeColor = System.Drawing.SystemColors.Control;
            this.label6.Location = new System.Drawing.Point(9, 64);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "Destination";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            this.label5.Location = new System.Drawing.Point(9, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Start";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(8, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 13);
            this.label4.TabIndex = 27;
            this.label4.Text = "Path Finder";
            // 
            // lblSwapPathfinder
            // 
            this.lblSwapPathfinder.AutoSize = true;
            this.lblSwapPathfinder.BackColor = System.Drawing.Color.Transparent;
            this.lblSwapPathfinder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSwapPathfinder.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblSwapPathfinder.Location = new System.Drawing.Point(44, 35);
            this.lblSwapPathfinder.Name = "lblSwapPathfinder";
            this.lblSwapPathfinder.Size = new System.Drawing.Size(24, 16);
            this.lblSwapPathfinder.TabIndex = 28;
            this.lblSwapPathfinder.Text = "↑↓";
            this.lblSwapPathfinder.Click += new System.EventHandler(this.lblSwapPathfinder_Click);
            // 
            // PathFinder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(347, 608);
            this.Controls.Add(this.lblSwapPathfinder);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.BTNFindPath);
            this.Controls.Add(this.LBPathFinder);
            this.Controls.Add(this.CMBEnd);
            this.Controls.Add(this.CMBStart);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PathFinder";
            this.Text = "PathFinder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BTNFindPath;
        private System.Windows.Forms.ListBox LBPathFinder;
        private System.Windows.Forms.ComboBox CMBEnd;
        private System.Windows.Forms.ComboBox CMBStart;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblSwapPathfinder;
    }
}