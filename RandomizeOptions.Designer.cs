namespace MMR_Tracker_V2
{
    partial class RandomizeOptions
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.Entry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Randomized = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Starting = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BTNRandomized = new System.Windows.Forms.Button();
            this.BTNUnrando = new System.Windows.Forms.Button();
            this.BTNUnrandMan = new System.Windows.Forms.Button();
            this.BTNStarting = new System.Windows.Forms.Button();
            this.BTNJunk = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Entry,
            this.Randomized,
            this.Starting});
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(13, 13);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(486, 603);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // Entry
            // 
            this.Entry.Text = "Entry";
            this.Entry.Width = 257;
            // 
            // Randomized
            // 
            this.Randomized.Text = "Randomized State";
            this.Randomized.Width = 104;
            // 
            // Starting
            // 
            this.Starting.Text = "Starting Item";
            this.Starting.Width = 75;
            // 
            // BTNRandomized
            // 
            this.BTNRandomized.Location = new System.Drawing.Point(506, 13);
            this.BTNRandomized.Name = "BTNRandomized";
            this.BTNRandomized.Size = new System.Drawing.Size(122, 23);
            this.BTNRandomized.TabIndex = 1;
            this.BTNRandomized.Text = "Randomized";
            this.BTNRandomized.UseVisualStyleBackColor = true;
            this.BTNRandomized.Click += new System.EventHandler(this.BTNRandomized_Click);
            // 
            // BTNUnrando
            // 
            this.BTNUnrando.Location = new System.Drawing.Point(506, 43);
            this.BTNUnrando.Name = "BTNUnrando";
            this.BTNUnrando.Size = new System.Drawing.Size(122, 23);
            this.BTNUnrando.TabIndex = 2;
            this.BTNUnrando.Text = "Unrandomized";
            this.BTNUnrando.UseVisualStyleBackColor = true;
            this.BTNUnrando.Click += new System.EventHandler(this.BTNUnrando_Click);
            // 
            // BTNUnrandMan
            // 
            this.BTNUnrandMan.Location = new System.Drawing.Point(506, 73);
            this.BTNUnrandMan.Name = "BTNUnrandMan";
            this.BTNUnrandMan.Size = new System.Drawing.Size(122, 23);
            this.BTNUnrandMan.TabIndex = 3;
            this.BTNUnrandMan.Text = "Unrandomized Manual";
            this.BTNUnrandMan.UseVisualStyleBackColor = true;
            this.BTNUnrandMan.Click += new System.EventHandler(this.BTNUnrandMan_Click);
            // 
            // BTNStarting
            // 
            this.BTNStarting.Location = new System.Drawing.Point(506, 131);
            this.BTNStarting.Name = "BTNStarting";
            this.BTNStarting.Size = new System.Drawing.Size(122, 23);
            this.BTNStarting.TabIndex = 4;
            this.BTNStarting.Text = "Toggle Starting";
            this.BTNStarting.UseVisualStyleBackColor = true;
            this.BTNStarting.Click += new System.EventHandler(this.BTNStarting_Click);
            // 
            // BTNJunk
            // 
            this.BTNJunk.Location = new System.Drawing.Point(506, 102);
            this.BTNJunk.Name = "BTNJunk";
            this.BTNJunk.Size = new System.Drawing.Size(122, 23);
            this.BTNJunk.TabIndex = 5;
            this.BTNJunk.Text = "Force Junk";
            this.BTNJunk.UseVisualStyleBackColor = true;
            this.BTNJunk.Click += new System.EventHandler(this.BTNJunk_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(506, 161);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(122, 62);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Save Settings";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(506, 229);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(122, 62);
            this.btnLoad.TabIndex = 7;
            this.btnLoad.Text = "Load Settings";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.BtnLoad_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(506, 298);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(118, 20);
            this.txtSearch.TabIndex = 8;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // RandomizeOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 628);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.BTNJunk);
            this.Controls.Add(this.BTNStarting);
            this.Controls.Add(this.BTNUnrandMan);
            this.Controls.Add(this.BTNUnrando);
            this.Controls.Add(this.BTNRandomized);
            this.Controls.Add(this.listView1);
            this.Name = "RandomizeOptions";
            this.Text = "RandomizeOptions";
            this.Load += new System.EventHandler(this.RandomizeOptions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader Entry;
        private System.Windows.Forms.ColumnHeader Randomized;
        private System.Windows.Forms.ColumnHeader Starting;
        private System.Windows.Forms.Button BTNRandomized;
        private System.Windows.Forms.Button BTNUnrando;
        private System.Windows.Forms.Button BTNUnrandMan;
        private System.Windows.Forms.Button BTNStarting;
        private System.Windows.Forms.Button BTNJunk;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.TextBox txtSearch;
    }
}