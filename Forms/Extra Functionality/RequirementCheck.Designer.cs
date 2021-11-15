namespace MMR_Tracker.Forms.Extra_Functionality
{
    partial class RequirementCheck
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RequirementCheck));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.NN1 = new System.Windows.Forms.CheckBox();
            this.ND1 = new System.Windows.Forms.CheckBox();
            this.ND2 = new System.Windows.Forms.CheckBox();
            this.NN2 = new System.Windows.Forms.CheckBox();
            this.ND3 = new System.Windows.Forms.CheckBox();
            this.NN3 = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.chkShowUnaltered = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 26);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(182, 264);
            this.listBox1.TabIndex = 0;
            this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            // 
            // listBox2
            // 
            this.listBox2.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.Location = new System.Drawing.Point(200, 26);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(397, 355);
            this.listBox2.TabIndex = 1;
            this.listBox2.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox2_DrawItem);
            this.listBox2.DoubleClick += new System.EventHandler(this.listBox2_DoubleClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(106, 293);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Go Back";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 293);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(88, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Go To";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Requirements";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(197, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Conditionals";
            // 
            // NN1
            // 
            this.NN1.AutoSize = true;
            this.NN1.BackColor = System.Drawing.Color.Transparent;
            this.NN1.ForeColor = System.Drawing.SystemColors.Control;
            this.NN1.Location = new System.Drawing.Point(14, 362);
            this.NN1.Name = "NN1";
            this.NN1.Size = new System.Drawing.Size(60, 17);
            this.NN1.TabIndex = 7;
            this.NN1.Text = "Night 1";
            this.NN1.UseVisualStyleBackColor = false;
            this.NN1.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // ND1
            // 
            this.ND1.AutoSize = true;
            this.ND1.BackColor = System.Drawing.Color.Transparent;
            this.ND1.ForeColor = System.Drawing.SystemColors.Control;
            this.ND1.Location = new System.Drawing.Point(14, 339);
            this.ND1.Name = "ND1";
            this.ND1.Size = new System.Drawing.Size(54, 17);
            this.ND1.TabIndex = 8;
            this.ND1.Text = "Day 1";
            this.ND1.UseVisualStyleBackColor = false;
            this.ND1.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // ND2
            // 
            this.ND2.AutoSize = true;
            this.ND2.BackColor = System.Drawing.Color.Transparent;
            this.ND2.ForeColor = System.Drawing.SystemColors.Control;
            this.ND2.Location = new System.Drawing.Point(74, 339);
            this.ND2.Name = "ND2";
            this.ND2.Size = new System.Drawing.Size(54, 17);
            this.ND2.TabIndex = 10;
            this.ND2.Text = "Day 2";
            this.ND2.UseVisualStyleBackColor = false;
            this.ND2.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // NN2
            // 
            this.NN2.AutoSize = true;
            this.NN2.BackColor = System.Drawing.Color.Transparent;
            this.NN2.ForeColor = System.Drawing.SystemColors.Control;
            this.NN2.Location = new System.Drawing.Point(74, 362);
            this.NN2.Name = "NN2";
            this.NN2.Size = new System.Drawing.Size(60, 17);
            this.NN2.TabIndex = 9;
            this.NN2.Text = "Night 2";
            this.NN2.UseVisualStyleBackColor = false;
            this.NN2.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // ND3
            // 
            this.ND3.AutoSize = true;
            this.ND3.BackColor = System.Drawing.Color.Transparent;
            this.ND3.ForeColor = System.Drawing.SystemColors.Control;
            this.ND3.Location = new System.Drawing.Point(134, 339);
            this.ND3.Name = "ND3";
            this.ND3.Size = new System.Drawing.Size(54, 17);
            this.ND3.TabIndex = 12;
            this.ND3.Text = "Day 3";
            this.ND3.UseVisualStyleBackColor = false;
            this.ND3.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // NN3
            // 
            this.NN3.AutoSize = true;
            this.NN3.BackColor = System.Drawing.Color.Transparent;
            this.NN3.ForeColor = System.Drawing.SystemColors.Control;
            this.NN3.Location = new System.Drawing.Point(134, 362);
            this.NN3.Name = "NN3";
            this.NN3.Size = new System.Drawing.Size(60, 17);
            this.NN3.TabIndex = 11;
            this.NN3.Text = "Night 3";
            this.NN3.UseVisualStyleBackColor = false;
            this.NN3.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(14, 319);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Available On";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(545, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "= Aquired";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            this.label5.Location = new System.Drawing.Point(517, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Bold";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // chkShowUnaltered
            // 
            this.chkShowUnaltered.AutoSize = true;
            this.chkShowUnaltered.BackColor = System.Drawing.Color.Transparent;
            this.chkShowUnaltered.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowUnaltered.Location = new System.Drawing.Point(386, 8);
            this.chkShowUnaltered.Name = "chkShowUnaltered";
            this.chkShowUnaltered.Size = new System.Drawing.Size(125, 17);
            this.chkShowUnaltered.TabIndex = 16;
            this.chkShowUnaltered.Text = "Show unaltered logic";
            this.chkShowUnaltered.UseVisualStyleBackColor = false;
            this.chkShowUnaltered.CheckedChanged += new System.EventHandler(this.chkShowUnaltered_CheckedChanged);
            // 
            // RequirementCheck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(610, 392);
            this.Controls.Add(this.chkShowUnaltered);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ND3);
            this.Controls.Add(this.NN3);
            this.Controls.Add(this.ND2);
            this.Controls.Add(this.NN2);
            this.Controls.Add(this.ND1);
            this.Controls.Add(this.NN1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.listBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RequirementCheck";
            this.Text = "RequirementCheck";
            this.Load += new System.EventHandler(this.RequirementCheck_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox NN1;
        private System.Windows.Forms.CheckBox ND1;
        private System.Windows.Forms.CheckBox ND2;
        private System.Windows.Forms.CheckBox NN2;
        private System.Windows.Forms.CheckBox ND3;
        private System.Windows.Forms.CheckBox NN3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkShowUnaltered;
    }
}