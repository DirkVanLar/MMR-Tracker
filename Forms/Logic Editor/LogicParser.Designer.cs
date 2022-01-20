namespace MMR_Tracker.Forms
{
    partial class LogicParser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogicParser));
            this.lvReference = new System.Windows.Forms.ListView();
            this.ID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DIC = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LOC = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ITEM = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnParseExpression = new System.Windows.Forms.Button();
            this.lbUsedlogic = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radItem = new System.Windows.Forms.RadioButton();
            this.radLoc = new System.Windows.Forms.RadioButton();
            this.radDic = new System.Windows.Forms.RadioButton();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvReference
            // 
            this.lvReference.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ID,
            this.DIC,
            this.LOC,
            this.ITEM});
            this.lvReference.HideSelection = false;
            this.lvReference.Location = new System.Drawing.Point(15, 393);
            this.lvReference.MultiSelect = false;
            this.lvReference.Name = "lvReference";
            this.lvReference.Size = new System.Drawing.Size(541, 237);
            this.lvReference.TabIndex = 1;
            this.lvReference.UseCompatibleStateImageBehavior = false;
            this.lvReference.View = System.Windows.Forms.View.Details;
            this.lvReference.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // ID
            // 
            this.ID.Text = "ID";
            this.ID.Width = 30;
            // 
            // DIC
            // 
            this.DIC.Text = "Dictionary Name";
            this.DIC.Width = 278;
            // 
            // LOC
            // 
            this.LOC.Text = "Location Name";
            this.LOC.Width = 119;
            // 
            // ITEM
            // 
            this.ITEM.Text = "Item Name";
            this.ITEM.Width = 103;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Enter Logical Expression Below.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label2.Location = new System.Drawing.Point(561, 363);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Used Logic Items";
            // 
            // btnParseExpression
            // 
            this.btnParseExpression.Location = new System.Drawing.Point(678, 4);
            this.btnParseExpression.Name = "btnParseExpression";
            this.btnParseExpression.Size = new System.Drawing.Size(110, 23);
            this.btnParseExpression.TabIndex = 4;
            this.btnParseExpression.Text = "Parse Expression";
            this.btnParseExpression.UseVisualStyleBackColor = true;
            this.btnParseExpression.Click += new System.EventHandler(this.btnParseExpression_Click);
            // 
            // lbUsedlogic
            // 
            this.lbUsedlogic.FormattingEnabled = true;
            this.lbUsedlogic.Location = new System.Drawing.Point(563, 379);
            this.lbUsedlogic.Name = "lbUsedlogic";
            this.lbUsedlogic.Size = new System.Drawing.Size(227, 251);
            this.lbUsedlogic.TabIndex = 6;
            this.lbUsedlogic.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.radItem);
            this.groupBox1.Controls.Add(this.radLoc);
            this.groupBox1.Controls.Add(this.radDic);
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.groupBox1.Location = new System.Drawing.Point(276, 357);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(279, 34);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filter By:";
            // 
            // radItem
            // 
            this.radItem.AutoSize = true;
            this.radItem.Location = new System.Drawing.Point(200, 14);
            this.radItem.Name = "radItem";
            this.radItem.Size = new System.Drawing.Size(76, 17);
            this.radItem.TabIndex = 2;
            this.radItem.TabStop = true;
            this.radItem.Text = "Item Name";
            this.radItem.UseVisualStyleBackColor = true;
            this.radItem.CheckedChanged += new System.EventHandler(this.RadChanged);
            // 
            // radLoc
            // 
            this.radLoc.AutoSize = true;
            this.radLoc.Location = new System.Drawing.Point(106, 14);
            this.radLoc.Name = "radLoc";
            this.radLoc.Size = new System.Drawing.Size(97, 17);
            this.radLoc.TabIndex = 1;
            this.radLoc.TabStop = true;
            this.radLoc.Text = "Location Name";
            this.radLoc.UseVisualStyleBackColor = true;
            this.radLoc.CheckedChanged += new System.EventHandler(this.RadChanged);
            // 
            // radDic
            // 
            this.radDic.AutoSize = true;
            this.radDic.Location = new System.Drawing.Point(6, 14);
            this.radDic.Name = "radDic";
            this.radDic.Size = new System.Drawing.Size(103, 17);
            this.radDic.TabIndex = 0;
            this.radDic.TabStop = true;
            this.radDic.Text = "Dictionary Name";
            this.radDic.UseVisualStyleBackColor = true;
            this.radDic.CheckedChanged += new System.EventHandler(this.RadChanged);
            // 
            // txtFilter
            // 
            this.txtFilter.Location = new System.Drawing.Point(14, 368);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(256, 20);
            this.txtFilter.TabIndex = 8;
            this.txtFilter.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            this.txtFilter.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBox2_MouseUp);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label3.Location = new System.Drawing.Point(12, 354);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Filter Items";
            // 
            // txtInput
            // 
            this.txtInput.Location = new System.Drawing.Point(12, 33);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(776, 318);
            this.txtInput.TabIndex = 11;
            this.txtInput.Text = "";
            this.txtInput.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // LogicParser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(800, 642);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lbUsedlogic);
            this.Controls.Add(this.btnParseExpression);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lvReference);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogicParser";
            this.Text = "Logic Parser";
            this.Load += new System.EventHandler(this.LogicParser_Load);
            this.Shown += new System.EventHandler(this.LogicParser_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView lvReference;
        private System.Windows.Forms.ColumnHeader ID;
        private System.Windows.Forms.ColumnHeader DIC;
        private System.Windows.Forms.ColumnHeader LOC;
        private System.Windows.Forms.ColumnHeader ITEM;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnParseExpression;
        private System.Windows.Forms.ListBox lbUsedlogic;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radItem;
        private System.Windows.Forms.RadioButton radLoc;
        private System.Windows.Forms.RadioButton radDic;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox txtInput;
    }
}