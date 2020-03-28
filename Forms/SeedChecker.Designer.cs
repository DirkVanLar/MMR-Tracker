namespace MMR_Tracker
{
    partial class SeedChecker
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SeedChecker));
            this.LBNeededItems = new System.Windows.Forms.ListBox();
            this.LBResult = new System.Windows.Forms.ListBox();
            this.LBIgnoredChecks = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnAddNeeded = new System.Windows.Forms.Button();
            this.btnAddIgnored = new System.Windows.Forms.Button();
            this.btnCheckSeed = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LBNeededItems
            // 
            this.LBNeededItems.FormattingEnabled = true;
            this.LBNeededItems.Location = new System.Drawing.Point(12, 31);
            this.LBNeededItems.Name = "LBNeededItems";
            this.LBNeededItems.Size = new System.Drawing.Size(176, 329);
            this.LBNeededItems.TabIndex = 0;
            this.LBNeededItems.DoubleClick += new System.EventHandler(this.LBNeededItems_DoubleClick);
            // 
            // LBResult
            // 
            this.LBResult.FormattingEnabled = true;
            this.LBResult.Location = new System.Drawing.Point(376, 31);
            this.LBResult.Name = "LBResult";
            this.LBResult.Size = new System.Drawing.Size(176, 329);
            this.LBResult.TabIndex = 1;
            // 
            // LBIgnoredChecks
            // 
            this.LBIgnoredChecks.FormattingEnabled = true;
            this.LBIgnoredChecks.Location = new System.Drawing.Point(194, 31);
            this.LBIgnoredChecks.Name = "LBIgnoredChecks";
            this.LBIgnoredChecks.Size = new System.Drawing.Size(176, 329);
            this.LBIgnoredChecks.TabIndex = 2;
            this.LBIgnoredChecks.DoubleClick += new System.EventHandler(this.LBIgnoredChecks_DoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(9, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Items Needed";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(191, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Checks Ignored";
            // 
            // btnAddNeeded
            // 
            this.btnAddNeeded.Location = new System.Drawing.Point(147, 7);
            this.btnAddNeeded.Name = "btnAddNeeded";
            this.btnAddNeeded.Size = new System.Drawing.Size(41, 23);
            this.btnAddNeeded.TabIndex = 5;
            this.btnAddNeeded.Text = "Add";
            this.btnAddNeeded.UseVisualStyleBackColor = true;
            this.btnAddNeeded.Click += new System.EventHandler(this.BtnAddNeeded_Click);
            // 
            // btnAddIgnored
            // 
            this.btnAddIgnored.Location = new System.Drawing.Point(329, 7);
            this.btnAddIgnored.Name = "btnAddIgnored";
            this.btnAddIgnored.Size = new System.Drawing.Size(41, 23);
            this.btnAddIgnored.TabIndex = 6;
            this.btnAddIgnored.Text = "Add";
            this.btnAddIgnored.UseVisualStyleBackColor = true;
            this.btnAddIgnored.Click += new System.EventHandler(this.BtnAddIgnored_Click);
            // 
            // btnCheckSeed
            // 
            this.btnCheckSeed.Location = new System.Drawing.Point(476, 7);
            this.btnCheckSeed.Name = "btnCheckSeed";
            this.btnCheckSeed.Size = new System.Drawing.Size(76, 23);
            this.btnCheckSeed.TabIndex = 7;
            this.btnCheckSeed.Text = "CheckSeed";
            this.btnCheckSeed.UseVisualStyleBackColor = true;
            this.btnCheckSeed.Click += new System.EventHandler(this.BtnCheckSeed_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(427, 7);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(43, 23);
            this.btnClear.TabIndex = 8;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(376, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Result";
            // 
            // SeedChecker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(564, 369);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnCheckSeed);
            this.Controls.Add(this.btnAddIgnored);
            this.Controls.Add(this.btnAddNeeded);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LBIgnoredChecks);
            this.Controls.Add(this.LBResult);
            this.Controls.Add(this.LBNeededItems);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SeedChecker";
            this.Text = "SeedChecker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LBNeededItems;
        private System.Windows.Forms.ListBox LBResult;
        private System.Windows.Forms.ListBox LBIgnoredChecks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnAddNeeded;
        private System.Windows.Forms.Button btnAddIgnored;
        private System.Windows.Forms.Button btnCheckSeed;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label3;
    }
}