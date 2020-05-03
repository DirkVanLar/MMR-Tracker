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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RandomizeOptions));
            this.listView1 = new System.Windows.Forms.ListView();
            this.Entry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Randomized = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Starting = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TrickEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BTNRandomized = new System.Windows.Forms.Button();
            this.BTNUnrando = new System.Windows.Forms.Button();
            this.BTNUnrandMan = new System.Windows.Forms.Button();
            this.BTNStarting = new System.Windows.Forms.Button();
            this.BTNJunk = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.chkShowRandom = new System.Windows.Forms.CheckBox();
            this.chkShowUnrand = new System.Windows.Forms.CheckBox();
            this.chkShowUnrandMan = new System.Windows.Forms.CheckBox();
            this.chkShowJunk = new System.Windows.Forms.CheckBox();
            this.chkShowStartingItems = new System.Windows.Forms.CheckBox();
            this.chkShowDisabledTricks = new System.Windows.Forms.CheckBox();
            this.chkShowEnabledTricks = new System.Windows.Forms.CheckBox();
            this.btnToggleTricks = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCustomItemString = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtJunkItemString = new System.Windows.Forms.TextBox();
            this.btnApplyString = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Entry,
            this.Randomized,
            this.Starting,
            this.TrickEnabled});
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.Name = "listView1";
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new System.Drawing.Size(697, 603);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView1_ItemChecked);
            // 
            // Entry
            // 
            this.Entry.Text = "Entry";
            this.Entry.Width = 411;
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
            // TrickEnabled
            // 
            this.TrickEnabled.Text = "Trick Enabled";
            this.TrickEnabled.Width = 86;
            // 
            // BTNRandomized
            // 
            this.BTNRandomized.Location = new System.Drawing.Point(720, 13);
            this.BTNRandomized.Name = "BTNRandomized";
            this.BTNRandomized.Size = new System.Drawing.Size(153, 23);
            this.BTNRandomized.TabIndex = 1;
            this.BTNRandomized.Text = "Randomized";
            this.BTNRandomized.UseVisualStyleBackColor = true;
            this.BTNRandomized.Click += new System.EventHandler(this.BTNRandomized_Click);
            // 
            // BTNUnrando
            // 
            this.BTNUnrando.Location = new System.Drawing.Point(720, 43);
            this.BTNUnrando.Name = "BTNUnrando";
            this.BTNUnrando.Size = new System.Drawing.Size(153, 23);
            this.BTNUnrando.TabIndex = 2;
            this.BTNUnrando.Text = "Unrandomized";
            this.BTNUnrando.UseVisualStyleBackColor = true;
            this.BTNUnrando.Click += new System.EventHandler(this.BTNUnrando_Click);
            // 
            // BTNUnrandMan
            // 
            this.BTNUnrandMan.Location = new System.Drawing.Point(720, 73);
            this.BTNUnrandMan.Name = "BTNUnrandMan";
            this.BTNUnrandMan.Size = new System.Drawing.Size(153, 23);
            this.BTNUnrandMan.TabIndex = 3;
            this.BTNUnrandMan.Text = "Unrandomized Manual";
            this.BTNUnrandMan.UseVisualStyleBackColor = true;
            this.BTNUnrandMan.Click += new System.EventHandler(this.BTNUnrandMan_Click);
            // 
            // BTNStarting
            // 
            this.BTNStarting.Location = new System.Drawing.Point(720, 131);
            this.BTNStarting.Name = "BTNStarting";
            this.BTNStarting.Size = new System.Drawing.Size(153, 23);
            this.BTNStarting.TabIndex = 4;
            this.BTNStarting.Text = "Toggle Starting";
            this.BTNStarting.UseVisualStyleBackColor = true;
            this.BTNStarting.Click += new System.EventHandler(this.BTNStarting_Click);
            // 
            // BTNJunk
            // 
            this.BTNJunk.Location = new System.Drawing.Point(720, 102);
            this.BTNJunk.Name = "BTNJunk";
            this.BTNJunk.Size = new System.Drawing.Size(153, 23);
            this.BTNJunk.TabIndex = 5;
            this.BTNJunk.Text = "Force Junk";
            this.BTNJunk.UseVisualStyleBackColor = true;
            this.BTNJunk.Click += new System.EventHandler(this.BTNJunk_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(720, 198);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(153, 23);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Create Template Save File";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoad.Location = new System.Drawing.Point(720, 227);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(153, 23);
            this.btnLoad.TabIndex = 7;
            this.btnLoad.Text = "Load Settings From Save File";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.BtnLoad_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(720, 274);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(153, 20);
            this.txtSearch.TabIndex = 8;
            this.txtSearch.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);
            this.txtSearch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtSearch_MouseUp);
            // 
            // chkShowRandom
            // 
            this.chkShowRandom.AutoSize = true;
            this.chkShowRandom.BackColor = System.Drawing.Color.Transparent;
            this.chkShowRandom.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowRandom.Location = new System.Drawing.Point(720, 322);
            this.chkShowRandom.Name = "chkShowRandom";
            this.chkShowRandom.Size = new System.Drawing.Size(115, 17);
            this.chkShowRandom.TabIndex = 9;
            this.chkShowRandom.Text = "Show Randomized";
            this.chkShowRandom.UseVisualStyleBackColor = false;
            this.chkShowRandom.CheckedChanged += new System.EventHandler(this.CHKShowRandom_CheckedChanged);
            // 
            // chkShowUnrand
            // 
            this.chkShowUnrand.AutoSize = true;
            this.chkShowUnrand.BackColor = System.Drawing.Color.Transparent;
            this.chkShowUnrand.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowUnrand.Location = new System.Drawing.Point(720, 345);
            this.chkShowUnrand.Name = "chkShowUnrand";
            this.chkShowUnrand.Size = new System.Drawing.Size(129, 17);
            this.chkShowUnrand.TabIndex = 10;
            this.chkShowUnrand.Text = "Show UnRandomized";
            this.chkShowUnrand.UseVisualStyleBackColor = false;
            this.chkShowUnrand.CheckedChanged += new System.EventHandler(this.ChkShowUnrand_CheckedChanged);
            // 
            // chkShowUnrandMan
            // 
            this.chkShowUnrandMan.AutoSize = true;
            this.chkShowUnrandMan.BackColor = System.Drawing.Color.Transparent;
            this.chkShowUnrandMan.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowUnrandMan.Location = new System.Drawing.Point(720, 368);
            this.chkShowUnrandMan.Name = "chkShowUnrandMan";
            this.chkShowUnrandMan.Size = new System.Drawing.Size(132, 17);
            this.chkShowUnrandMan.TabIndex = 11;
            this.chkShowUnrandMan.Text = "Show UnRando (Man)";
            this.chkShowUnrandMan.UseVisualStyleBackColor = false;
            this.chkShowUnrandMan.CheckedChanged += new System.EventHandler(this.ChkShowUnrandMan_CheckedChanged);
            // 
            // chkShowJunk
            // 
            this.chkShowJunk.AutoSize = true;
            this.chkShowJunk.BackColor = System.Drawing.Color.Transparent;
            this.chkShowJunk.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowJunk.Location = new System.Drawing.Point(720, 391);
            this.chkShowJunk.Name = "chkShowJunk";
            this.chkShowJunk.Size = new System.Drawing.Size(115, 17);
            this.chkShowJunk.TabIndex = 12;
            this.chkShowJunk.Text = "Show Forced Junk";
            this.chkShowJunk.UseVisualStyleBackColor = false;
            this.chkShowJunk.CheckedChanged += new System.EventHandler(this.ChkJunk_CheckedChanged);
            // 
            // chkShowStartingItems
            // 
            this.chkShowStartingItems.AutoSize = true;
            this.chkShowStartingItems.BackColor = System.Drawing.Color.Transparent;
            this.chkShowStartingItems.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowStartingItems.Location = new System.Drawing.Point(720, 415);
            this.chkShowStartingItems.Name = "chkShowStartingItems";
            this.chkShowStartingItems.Size = new System.Drawing.Size(120, 17);
            this.chkShowStartingItems.TabIndex = 13;
            this.chkShowStartingItems.Text = "Show Starting Items";
            this.chkShowStartingItems.UseVisualStyleBackColor = false;
            this.chkShowStartingItems.CheckedChanged += new System.EventHandler(this.ChkStartingItems_CheckedChanged);
            // 
            // chkShowDisabledTricks
            // 
            this.chkShowDisabledTricks.AutoSize = true;
            this.chkShowDisabledTricks.BackColor = System.Drawing.Color.Transparent;
            this.chkShowDisabledTricks.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowDisabledTricks.Location = new System.Drawing.Point(720, 474);
            this.chkShowDisabledTricks.Name = "chkShowDisabledTricks";
            this.chkShowDisabledTricks.Size = new System.Drawing.Size(129, 17);
            this.chkShowDisabledTricks.TabIndex = 15;
            this.chkShowDisabledTricks.Text = "Show Disabled Tricks";
            this.chkShowDisabledTricks.UseVisualStyleBackColor = false;
            this.chkShowDisabledTricks.CheckedChanged += new System.EventHandler(this.chkShowDisabledTricks_CheckedChanged);
            // 
            // chkShowEnabledTricks
            // 
            this.chkShowEnabledTricks.AutoSize = true;
            this.chkShowEnabledTricks.BackColor = System.Drawing.Color.Transparent;
            this.chkShowEnabledTricks.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowEnabledTricks.Location = new System.Drawing.Point(720, 450);
            this.chkShowEnabledTricks.Name = "chkShowEnabledTricks";
            this.chkShowEnabledTricks.Size = new System.Drawing.Size(127, 17);
            this.chkShowEnabledTricks.TabIndex = 14;
            this.chkShowEnabledTricks.Text = "Show Enabled Tricks";
            this.chkShowEnabledTricks.UseVisualStyleBackColor = false;
            this.chkShowEnabledTricks.CheckedChanged += new System.EventHandler(this.chkShowEnabledTricks_CheckedChanged);
            // 
            // btnToggleTricks
            // 
            this.btnToggleTricks.Location = new System.Drawing.Point(719, 160);
            this.btnToggleTricks.Name = "btnToggleTricks";
            this.btnToggleTricks.Size = new System.Drawing.Size(153, 23);
            this.btnToggleTricks.TabIndex = 16;
            this.btnToggleTricks.Text = "Toggle Trick";
            this.btnToggleTricks.UseVisualStyleBackColor = true;
            this.btnToggleTricks.Click += new System.EventHandler(this.btnToggleTricks_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(717, 306);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Randomized State";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(717, 434);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Tricks";
            // 
            // txtCustomItemString
            // 
            this.txtCustomItemString.Location = new System.Drawing.Point(720, 527);
            this.txtCustomItemString.Name = "txtCustomItemString";
            this.txtCustomItemString.Size = new System.Drawing.Size(153, 20);
            this.txtCustomItemString.TabIndex = 19;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(717, 511);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Custom Item String";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(717, 550);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Force Junk String";
            // 
            // txtJunkItemString
            // 
            this.txtJunkItemString.Location = new System.Drawing.Point(720, 566);
            this.txtJunkItemString.Name = "txtJunkItemString";
            this.txtJunkItemString.Size = new System.Drawing.Size(153, 20);
            this.txtJunkItemString.TabIndex = 21;
            // 
            // btnApplyString
            // 
            this.btnApplyString.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApplyString.Location = new System.Drawing.Point(719, 592);
            this.btnApplyString.Name = "btnApplyString";
            this.btnApplyString.Size = new System.Drawing.Size(153, 23);
            this.btnApplyString.TabIndex = 23;
            this.btnApplyString.Text = "Apply Custom Item Strings";
            this.btnApplyString.UseVisualStyleBackColor = true;
            this.btnApplyString.Click += new System.EventHandler(this.btnApplyString_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            this.label5.Location = new System.Drawing.Point(717, 258);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 24;
            this.label5.Text = "Filter";
            // 
            // RandomizeOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(883, 628);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnApplyString);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtJunkItemString);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtCustomItemString);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnToggleTricks);
            this.Controls.Add(this.chkShowDisabledTricks);
            this.Controls.Add(this.chkShowEnabledTricks);
            this.Controls.Add(this.chkShowStartingItems);
            this.Controls.Add(this.chkShowJunk);
            this.Controls.Add(this.chkShowUnrandMan);
            this.Controls.Add(this.chkShowUnrand);
            this.Controls.Add(this.chkShowRandom);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.BTNJunk);
            this.Controls.Add(this.BTNStarting);
            this.Controls.Add(this.BTNUnrandMan);
            this.Controls.Add(this.BTNUnrando);
            this.Controls.Add(this.BTNRandomized);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
        private System.Windows.Forms.CheckBox chkShowRandom;
        private System.Windows.Forms.CheckBox chkShowUnrand;
        private System.Windows.Forms.CheckBox chkShowUnrandMan;
        private System.Windows.Forms.CheckBox chkShowJunk;
        private System.Windows.Forms.CheckBox chkShowStartingItems;
        private System.Windows.Forms.ColumnHeader TrickEnabled;
        private System.Windows.Forms.CheckBox chkShowDisabledTricks;
        private System.Windows.Forms.CheckBox chkShowEnabledTricks;
        private System.Windows.Forms.Button btnToggleTricks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCustomItemString;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtJunkItemString;
        private System.Windows.Forms.Button btnApplyString;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}