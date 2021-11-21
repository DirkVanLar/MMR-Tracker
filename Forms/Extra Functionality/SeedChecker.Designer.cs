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
            this.label3 = new System.Windows.Forms.Label();
            this.chkShowObtainable = new System.Windows.Forms.CheckBox();
            this.chkShowUnobtainable = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.btnLocationLookup = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnShpereLookup = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnAreaLookup = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // LBNeededItems
            // 
            this.LBNeededItems.FormattingEnabled = true;
            this.LBNeededItems.Location = new System.Drawing.Point(6, 41);
            this.LBNeededItems.Name = "LBNeededItems";
            this.LBNeededItems.Size = new System.Drawing.Size(129, 160);
            this.LBNeededItems.TabIndex = 0;
            this.LBNeededItems.DoubleClick += new System.EventHandler(this.LBNeededItems_DoubleClick);
            // 
            // LBResult
            // 
            this.LBResult.FormattingEnabled = true;
            this.LBResult.HorizontalScrollbar = true;
            this.LBResult.Location = new System.Drawing.Point(141, 41);
            this.LBResult.Name = "LBResult";
            this.LBResult.Size = new System.Drawing.Size(272, 446);
            this.LBResult.TabIndex = 1;
            // 
            // LBIgnoredChecks
            // 
            this.LBIgnoredChecks.FormattingEnabled = true;
            this.LBIgnoredChecks.Location = new System.Drawing.Point(6, 223);
            this.LBIgnoredChecks.Name = "LBIgnoredChecks";
            this.LBIgnoredChecks.Size = new System.Drawing.Size(129, 264);
            this.LBIgnoredChecks.TabIndex = 2;
            this.LBIgnoredChecks.DoubleClick += new System.EventHandler(this.LBIgnoredChecks_DoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(3, 22);
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
            this.label2.Location = new System.Drawing.Point(3, 204);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Ignored Checks";
            // 
            // btnAddNeeded
            // 
            this.btnAddNeeded.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAddNeeded.Location = new System.Drawing.Point(98, 21);
            this.btnAddNeeded.Name = "btnAddNeeded";
            this.btnAddNeeded.Size = new System.Drawing.Size(34, 19);
            this.btnAddNeeded.TabIndex = 5;
            this.btnAddNeeded.Text = "Add";
            this.btnAddNeeded.UseVisualStyleBackColor = true;
            this.btnAddNeeded.Click += new System.EventHandler(this.BtnAddNeeded_Click);
            // 
            // btnAddIgnored
            // 
            this.btnAddIgnored.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAddIgnored.Location = new System.Drawing.Point(98, 202);
            this.btnAddIgnored.Name = "btnAddIgnored";
            this.btnAddIgnored.Size = new System.Drawing.Size(37, 19);
            this.btnAddIgnored.TabIndex = 6;
            this.btnAddIgnored.Text = "Add";
            this.btnAddIgnored.UseVisualStyleBackColor = true;
            this.btnAddIgnored.Click += new System.EventHandler(this.BtnAddIgnored_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(138, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Result";
            // 
            // chkShowObtainable
            // 
            this.chkShowObtainable.AutoSize = true;
            this.chkShowObtainable.BackColor = System.Drawing.Color.Transparent;
            this.chkShowObtainable.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowObtainable.Location = new System.Drawing.Point(181, 21);
            this.chkShowObtainable.Name = "chkShowObtainable";
            this.chkShowObtainable.Size = new System.Drawing.Size(107, 17);
            this.chkShowObtainable.TabIndex = 10;
            this.chkShowObtainable.Text = "Show Obtainable";
            this.chkShowObtainable.UseVisualStyleBackColor = false;
            this.chkShowObtainable.CheckedChanged += new System.EventHandler(this.BtnCheckSeed_Click);
            // 
            // chkShowUnobtainable
            // 
            this.chkShowUnobtainable.AutoSize = true;
            this.chkShowUnobtainable.BackColor = System.Drawing.Color.Transparent;
            this.chkShowUnobtainable.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowUnobtainable.Location = new System.Drawing.Point(294, 21);
            this.chkShowUnobtainable.Name = "chkShowUnobtainable";
            this.chkShowUnobtainable.Size = new System.Drawing.Size(119, 17);
            this.chkShowUnobtainable.TabIndex = 11;
            this.chkShowUnobtainable.Text = "Show Unobtainable";
            this.chkShowUnobtainable.UseVisualStyleBackColor = false;
            this.chkShowUnobtainable.CheckedChanged += new System.EventHandler(this.BtnCheckSeed_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.LBResult);
            this.groupBox1.Controls.Add(this.chkShowUnobtainable);
            this.groupBox1.Controls.Add(this.LBNeededItems);
            this.groupBox1.Controls.Add(this.chkShowObtainable);
            this.groupBox1.Controls.Add(this.LBIgnoredChecks);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnAddIgnored);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnAddNeeded);
            this.groupBox1.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(421, 492);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Seed Checker";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(81, 202);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(19, 20);
            this.label8.TabIndex = 22;
            this.label8.Text = "?";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(81, 20);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(19, 20);
            this.label7.TabIndex = 21;
            this.label7.Text = "?";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // button1
            // 
            this.button1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button1.Location = new System.Drawing.Point(6, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(233, 23);
            this.button1.TabIndex = 13;
            this.button1.Text = "Generate Playthrough";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnLocationLookup
            // 
            this.btnLocationLookup.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnLocationLookup.Location = new System.Drawing.Point(6, 234);
            this.btnLocationLookup.Name = "btnLocationLookup";
            this.btnLocationLookup.Size = new System.Drawing.Size(77, 23);
            this.btnLocationLookup.TabIndex = 14;
            this.btnLocationLookup.Text = "Location";
            this.btnLocationLookup.UseVisualStyleBackColor = true;
            this.btnLocationLookup.Click += new System.EventHandler(this.spoilerLogLookupToolStripMenuItem_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.Location = new System.Drawing.Point(6, 60);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(233, 173);
            this.listBox1.TabIndex = 15;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // textBox1
            // 
            this.textBox1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox1.Location = new System.Drawing.Point(6, 32);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(233, 20);
            this.textBox1.TabIndex = 16;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // btnShpereLookup
            // 
            this.btnShpereLookup.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnShpereLookup.Location = new System.Drawing.Point(164, 234);
            this.btnShpereLookup.Name = "btnShpereLookup";
            this.btnShpereLookup.Size = new System.Drawing.Size(75, 23);
            this.btnShpereLookup.TabIndex = 17;
            this.btnShpereLookup.Text = "Sphere";
            this.btnShpereLookup.UseVisualStyleBackColor = true;
            this.btnShpereLookup.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.listBox2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox2.Location = new System.Drawing.Point(439, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(244, 222);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Playthrough Generator";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(219, 46);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(19, 20);
            this.label6.TabIndex = 20;
            this.label6.Text = "?";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // textBox2
            // 
            this.textBox2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox2.Location = new System.Drawing.Point(6, 67);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(233, 20);
            this.textBox2.TabIndex = 19;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 51);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(118, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Select Game Clear Item";
            // 
            // listBox2
            // 
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.Location = new System.Drawing.Point(6, 93);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(232, 121);
            this.listBox2.TabIndex = 16;
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.Color.Transparent;
            this.groupBox3.Controls.Add(this.btnAreaLookup);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.listBox1);
            this.groupBox3.Controls.Add(this.btnLocationLookup);
            this.groupBox3.Controls.Add(this.btnShpereLookup);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox3.Location = new System.Drawing.Point(439, 240);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(244, 264);
            this.groupBox3.TabIndex = 19;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Spoiler Log Search";
            // 
            // btnAreaLookup
            // 
            this.btnAreaLookup.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAreaLookup.Location = new System.Drawing.Point(85, 234);
            this.btnAreaLookup.Name = "btnAreaLookup";
            this.btnAreaLookup.Size = new System.Drawing.Size(77, 23);
            this.btnAreaLookup.TabIndex = 19;
            this.btnAreaLookup.Text = "Area";
            this.btnAreaLookup.UseVisualStyleBackColor = true;
            this.btnAreaLookup.Click += new System.EventHandler(this.btnAreaLookup_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Select An Item";
            // 
            // SeedChecker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(694, 509);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SeedChecker";
            this.Text = "Spoiler Log Tools (WARNING! Information here may spoil your seed!)";
            this.Load += new System.EventHandler(this.SeedChecker_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox LBNeededItems;
        private System.Windows.Forms.ListBox LBResult;
        private System.Windows.Forms.ListBox LBIgnoredChecks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnAddNeeded;
        private System.Windows.Forms.Button btnAddIgnored;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkShowObtainable;
        private System.Windows.Forms.CheckBox chkShowUnobtainable;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnLocationLookup;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnShpereLookup;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnAreaLookup;
    }
}