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
            this.lbCheckItems = new System.Windows.Forms.CheckedListBox();
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
            this.BTNJunk.Size = new System.Drawing.Size(293, 38);
            this.BTNJunk.TabIndex = 1;
            this.BTNJunk.Text = "Junk Item";
            this.BTNJunk.UseVisualStyleBackColor = true;
            this.BTNJunk.Click += new System.EventHandler(this.BTNJunk_Click);
            // 
            // TXTSearch
            // 
            this.TXTSearch.Location = new System.Drawing.Point(13, 13);
            this.TXTSearch.Name = "TXTSearch";
            this.TXTSearch.Size = new System.Drawing.Size(418, 20);
            this.TXTSearch.TabIndex = 2;
            this.TXTSearch.TextChanged += new System.EventHandler(this.TXTSearch_TextChanged);
            // 
            // lbCheckItems
            // 
            this.lbCheckItems.FormattingEnabled = true;
            this.lbCheckItems.Location = new System.Drawing.Point(311, 530);
            this.lbCheckItems.Name = "lbCheckItems";
            this.lbCheckItems.Size = new System.Drawing.Size(120, 34);
            this.lbCheckItems.TabIndex = 3;
            this.lbCheckItems.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.LbCheckItems_ItemCheck);
            // 
            // ItemSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(443, 580);
            this.Controls.Add(this.lbCheckItems);
            this.Controls.Add(this.TXTSearch);
            this.Controls.Add(this.BTNJunk);
            this.Controls.Add(this.LBItemSelect);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ItemSelect";
            this.Text = "ItemSelect";
            this.Load += new System.EventHandler(this.ItemSelect_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LBItemSelect;
        private System.Windows.Forms.Button BTNJunk;
        private System.Windows.Forms.TextBox TXTSearch;
        private System.Windows.Forms.CheckedListBox lbCheckItems;
    }
}