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
            this.LBNeededItems = new System.Windows.Forms.ListBox();
            this.LBResult = new System.Windows.Forms.ListBox();
            this.LBIgnoredChecks = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnAddNeeded = new System.Windows.Forms.Button();
            this.btnAddIgnored = new System.Windows.Forms.Button();
            this.btnCheckSeed = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LBNeededItems
            // 
            this.LBNeededItems.FormattingEnabled = true;
            this.LBNeededItems.Location = new System.Drawing.Point(12, 31);
            this.LBNeededItems.Name = "LBNeededItems";
            this.LBNeededItems.Size = new System.Drawing.Size(176, 186);
            this.LBNeededItems.TabIndex = 0;
            // 
            // LBResult
            // 
            this.LBResult.FormattingEnabled = true;
            this.LBResult.Location = new System.Drawing.Point(12, 223);
            this.LBResult.Name = "LBResult";
            this.LBResult.Size = new System.Drawing.Size(176, 186);
            this.LBResult.TabIndex = 1;
            // 
            // LBIgnoredChecks
            // 
            this.LBIgnoredChecks.FormattingEnabled = true;
            this.LBIgnoredChecks.Location = new System.Drawing.Point(194, 31);
            this.LBIgnoredChecks.Name = "LBIgnoredChecks";
            this.LBIgnoredChecks.Size = new System.Drawing.Size(176, 186);
            this.LBIgnoredChecks.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Items Needed";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
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
            // 
            // btnAddIgnored
            // 
            this.btnAddIgnored.Location = new System.Drawing.Point(329, 7);
            this.btnAddIgnored.Name = "btnAddIgnored";
            this.btnAddIgnored.Size = new System.Drawing.Size(41, 23);
            this.btnAddIgnored.TabIndex = 6;
            this.btnAddIgnored.Text = "Add";
            this.btnAddIgnored.UseVisualStyleBackColor = true;
            // 
            // btnCheckSeed
            // 
            this.btnCheckSeed.Location = new System.Drawing.Point(195, 224);
            this.btnCheckSeed.Name = "btnCheckSeed";
            this.btnCheckSeed.Size = new System.Drawing.Size(175, 41);
            this.btnCheckSeed.TabIndex = 7;
            this.btnCheckSeed.Text = "CheckSeed";
            this.btnCheckSeed.UseVisualStyleBackColor = true;
            // 
            // SeedChecker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 420);
            this.Controls.Add(this.btnCheckSeed);
            this.Controls.Add(this.btnAddIgnored);
            this.Controls.Add(this.btnAddNeeded);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LBIgnoredChecks);
            this.Controls.Add(this.LBResult);
            this.Controls.Add(this.LBNeededItems);
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
    }
}