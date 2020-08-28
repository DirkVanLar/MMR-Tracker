namespace MMR_Tracker_V2
{
    partial class ItemSelect
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemSelect));
            this.LBItemSelect = new System.Windows.Forms.ListBox();
            this.BTNJunk = new System.Windows.Forms.Button();
            this.TXTSearch = new System.Windows.Forms.TextBox();
            this.chkAddSeperate = new System.Windows.Forms.CheckBox();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.lbCheckItems = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.nudForPlayer = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudForPlayer)).BeginInit();
            this.SuspendLayout();
            // 
            // LBItemSelect
            // 
            this.LBItemSelect.FormattingEnabled = true;
            this.LBItemSelect.Location = new System.Drawing.Point(13, 39);
            this.LBItemSelect.Name = "LBItemSelect";
            this.LBItemSelect.Size = new System.Drawing.Size(418, 485);
            this.LBItemSelect.TabIndex = 0;
            this.LBItemSelect.DoubleClick += new System.EventHandler(this.LBItemSelect_DoubleClick);
            // 
            // BTNJunk
            // 
            this.BTNJunk.Location = new System.Drawing.Point(12, 530);
            this.BTNJunk.Name = "BTNJunk";
            this.BTNJunk.Size = new System.Drawing.Size(238, 38);
            this.BTNJunk.TabIndex = 1;
            this.BTNJunk.Text = "Junk Item";
            this.BTNJunk.UseVisualStyleBackColor = true;
            this.BTNJunk.Click += new System.EventHandler(this.BTNJunk_Click);
            // 
            // TXTSearch
            // 
            this.TXTSearch.Location = new System.Drawing.Point(13, 13);
            this.TXTSearch.Name = "TXTSearch";
            this.TXTSearch.Size = new System.Drawing.Size(137, 20);
            this.TXTSearch.TabIndex = 2;
            this.TXTSearch.TextChanged += new System.EventHandler(this.TXTSearch_TextChanged);
            this.TXTSearch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TXTSearch_MouseUp);
            // 
            // chkAddSeperate
            // 
            this.chkAddSeperate.AutoSize = true;
            this.chkAddSeperate.BackColor = System.Drawing.Color.Transparent;
            this.chkAddSeperate.ForeColor = System.Drawing.SystemColors.Control;
            this.chkAddSeperate.Location = new System.Drawing.Point(256, 16);
            this.chkAddSeperate.Name = "chkAddSeperate";
            this.chkAddSeperate.Size = new System.Drawing.Size(175, 17);
            this.chkAddSeperate.TabIndex = 4;
            this.chkAddSeperate.Text = "Add Each Conditional Seperatly";
            this.chkAddSeperate.UseVisualStyleBackColor = false;
            this.chkAddSeperate.CheckedChanged += new System.EventHandler(this.chkAddSeperate_CheckedChanged);
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(256, 530);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(31, 20);
            this.btnUp.TabIndex = 5;
            this.btnUp.Text = "UP";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(256, 548);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(29, 20);
            this.btnDown.TabIndex = 6;
            this.btnDown.Text = "Down";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // lbCheckItems
            // 
            this.lbCheckItems.CheckBoxes = true;
            this.lbCheckItems.HideSelection = false;
            this.lbCheckItems.Location = new System.Drawing.Point(335, 530);
            this.lbCheckItems.Name = "lbCheckItems";
            this.lbCheckItems.Size = new System.Drawing.Size(62, 40);
            this.lbCheckItems.TabIndex = 7;
            this.lbCheckItems.UseCompatibleStateImageBehavior = false;
            this.lbCheckItems.View = System.Windows.Forms.View.List;
            this.lbCheckItems.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lbCheckItems_ItemChecked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(317, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "For Player";
            this.label1.Visible = false;
            // 
            // nudForPlayer
            // 
            this.nudForPlayer.Location = new System.Drawing.Point(377, 13);
            this.nudForPlayer.Name = "nudForPlayer";
            this.nudForPlayer.Size = new System.Drawing.Size(54, 20);
            this.nudForPlayer.TabIndex = 9;
            this.nudForPlayer.Visible = false;
            // 
            // ItemSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(443, 580);
            this.Controls.Add(this.nudForPlayer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbCheckItems);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.chkAddSeperate);
            this.Controls.Add(this.TXTSearch);
            this.Controls.Add(this.BTNJunk);
            this.Controls.Add(this.LBItemSelect);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ItemSelect";
            this.Text = "ItemSelect";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ItemSelect_FormClosing);
            this.Load += new System.EventHandler(this.ItemSelect_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudForPlayer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LBItemSelect;
        private System.Windows.Forms.Button BTNJunk;
        private System.Windows.Forms.TextBox TXTSearch;
        private System.Windows.Forms.CheckBox chkAddSeperate;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.ListView lbCheckItems;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudForPlayer;
    }
}