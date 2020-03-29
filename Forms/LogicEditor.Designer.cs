namespace MMR_Tracker.Forms
{
    partial class LogicEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogicEditor));
            this.LBRequired = new System.Windows.Forms.ListBox();
            this.LBConditional = new System.Windows.Forms.ListBox();
            this.chkNeedDay1 = new System.Windows.Forms.CheckBox();
            this.chkNeedNight1 = new System.Windows.Forms.CheckBox();
            this.chkNeedDay2 = new System.Windows.Forms.CheckBox();
            this.chkNeedNight2 = new System.Windows.Forms.CheckBox();
            this.chkNeedDay3 = new System.Windows.Forms.CheckBox();
            this.chkNeedNight3 = new System.Windows.Forms.CheckBox();
            this.chkOnDay1 = new System.Windows.Forms.CheckBox();
            this.chkOnNight1 = new System.Windows.Forms.CheckBox();
            this.chkOnDay2 = new System.Windows.Forms.CheckBox();
            this.chkOnNight2 = new System.Windows.Forms.CheckBox();
            this.chkOnDay3 = new System.Windows.Forms.CheckBox();
            this.chkOnNight3 = new System.Windows.Forms.CheckBox();
            this.nudIndex = new System.Windows.Forms.NumericUpDown();
            this.lblDicName = new System.Windows.Forms.Label();
            this.btnAddReq = new System.Windows.Forms.Button();
            this.btnAddCond = new System.Windows.Forms.Button();
            this.btnRemoveReq = new System.Windows.Forms.Button();
            this.btnRemoveCond = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblItemName = new System.Windows.Forms.Label();
            this.lblLocName = new System.Windows.Forms.Label();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnGoTo = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnEditSelected = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLogicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyToTrackerLogicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadLogicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newLogicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useLocationItemNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displaySpoilerLogNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudIndex)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LBRequired
            // 
            this.LBRequired.FormattingEnabled = true;
            this.LBRequired.HorizontalScrollbar = true;
            this.LBRequired.Location = new System.Drawing.Point(12, 44);
            this.LBRequired.Name = "LBRequired";
            this.LBRequired.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBRequired.Size = new System.Drawing.Size(249, 329);
            this.LBRequired.TabIndex = 0;
            this.LBRequired.SelectedIndexChanged += new System.EventHandler(this.LBRequired_SelectedIndexChanged);
            this.LBRequired.DoubleClick += new System.EventHandler(this.LBRequired_DoubleClick);
            this.LBRequired.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LBRequired_KeyDown);
            // 
            // LBConditional
            // 
            this.LBConditional.FormattingEnabled = true;
            this.LBConditional.HorizontalScrollbar = true;
            this.LBConditional.Location = new System.Drawing.Point(267, 44);
            this.LBConditional.Name = "LBConditional";
            this.LBConditional.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBConditional.Size = new System.Drawing.Size(530, 329);
            this.LBConditional.TabIndex = 1;
            this.LBConditional.SelectedIndexChanged += new System.EventHandler(this.LBConditional_SelectedIndexChanged);
            this.LBConditional.DoubleClick += new System.EventHandler(this.LBConditional_DoubleClick);
            this.LBConditional.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LBConditional_KeyDown);
            // 
            // chkNeedDay1
            // 
            this.chkNeedDay1.AutoSize = true;
            this.chkNeedDay1.BackColor = System.Drawing.Color.Transparent;
            this.chkNeedDay1.ForeColor = System.Drawing.SystemColors.Control;
            this.chkNeedDay1.Location = new System.Drawing.Point(12, 426);
            this.chkNeedDay1.Name = "chkNeedDay1";
            this.chkNeedDay1.Size = new System.Drawing.Size(54, 17);
            this.chkNeedDay1.TabIndex = 2;
            this.chkNeedDay1.Text = "Day 1";
            this.chkNeedDay1.UseVisualStyleBackColor = false;
            this.chkNeedDay1.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkNeedNight1
            // 
            this.chkNeedNight1.AutoSize = true;
            this.chkNeedNight1.BackColor = System.Drawing.Color.Transparent;
            this.chkNeedNight1.ForeColor = System.Drawing.SystemColors.Control;
            this.chkNeedNight1.Location = new System.Drawing.Point(72, 426);
            this.chkNeedNight1.Name = "chkNeedNight1";
            this.chkNeedNight1.Size = new System.Drawing.Size(60, 17);
            this.chkNeedNight1.TabIndex = 3;
            this.chkNeedNight1.Text = "Night 1";
            this.chkNeedNight1.UseVisualStyleBackColor = false;
            this.chkNeedNight1.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkNeedDay2
            // 
            this.chkNeedDay2.AutoSize = true;
            this.chkNeedDay2.BackColor = System.Drawing.Color.Transparent;
            this.chkNeedDay2.ForeColor = System.Drawing.SystemColors.Control;
            this.chkNeedDay2.Location = new System.Drawing.Point(12, 449);
            this.chkNeedDay2.Name = "chkNeedDay2";
            this.chkNeedDay2.Size = new System.Drawing.Size(54, 17);
            this.chkNeedDay2.TabIndex = 4;
            this.chkNeedDay2.Text = "Day 2";
            this.chkNeedDay2.UseVisualStyleBackColor = false;
            this.chkNeedDay2.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkNeedNight2
            // 
            this.chkNeedNight2.AutoSize = true;
            this.chkNeedNight2.BackColor = System.Drawing.Color.Transparent;
            this.chkNeedNight2.ForeColor = System.Drawing.SystemColors.Control;
            this.chkNeedNight2.Location = new System.Drawing.Point(72, 449);
            this.chkNeedNight2.Name = "chkNeedNight2";
            this.chkNeedNight2.Size = new System.Drawing.Size(60, 17);
            this.chkNeedNight2.TabIndex = 5;
            this.chkNeedNight2.Text = "Night 2";
            this.chkNeedNight2.UseVisualStyleBackColor = false;
            this.chkNeedNight2.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkNeedDay3
            // 
            this.chkNeedDay3.AutoSize = true;
            this.chkNeedDay3.BackColor = System.Drawing.Color.Transparent;
            this.chkNeedDay3.ForeColor = System.Drawing.SystemColors.Control;
            this.chkNeedDay3.Location = new System.Drawing.Point(12, 472);
            this.chkNeedDay3.Name = "chkNeedDay3";
            this.chkNeedDay3.Size = new System.Drawing.Size(54, 17);
            this.chkNeedDay3.TabIndex = 6;
            this.chkNeedDay3.Text = "Day 3";
            this.chkNeedDay3.UseVisualStyleBackColor = false;
            this.chkNeedDay3.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkNeedNight3
            // 
            this.chkNeedNight3.AutoSize = true;
            this.chkNeedNight3.BackColor = System.Drawing.Color.Transparent;
            this.chkNeedNight3.ForeColor = System.Drawing.SystemColors.Control;
            this.chkNeedNight3.Location = new System.Drawing.Point(72, 472);
            this.chkNeedNight3.Name = "chkNeedNight3";
            this.chkNeedNight3.Size = new System.Drawing.Size(60, 17);
            this.chkNeedNight3.TabIndex = 7;
            this.chkNeedNight3.Text = "Night 3";
            this.chkNeedNight3.UseVisualStyleBackColor = false;
            this.chkNeedNight3.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkOnDay1
            // 
            this.chkOnDay1.AutoSize = true;
            this.chkOnDay1.BackColor = System.Drawing.Color.Transparent;
            this.chkOnDay1.ForeColor = System.Drawing.SystemColors.Control;
            this.chkOnDay1.Location = new System.Drawing.Point(138, 426);
            this.chkOnDay1.Name = "chkOnDay1";
            this.chkOnDay1.Size = new System.Drawing.Size(54, 17);
            this.chkOnDay1.TabIndex = 8;
            this.chkOnDay1.Text = "Day 1";
            this.chkOnDay1.UseVisualStyleBackColor = false;
            this.chkOnDay1.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkOnNight1
            // 
            this.chkOnNight1.AutoSize = true;
            this.chkOnNight1.BackColor = System.Drawing.Color.Transparent;
            this.chkOnNight1.ForeColor = System.Drawing.SystemColors.Control;
            this.chkOnNight1.Location = new System.Drawing.Point(198, 426);
            this.chkOnNight1.Name = "chkOnNight1";
            this.chkOnNight1.Size = new System.Drawing.Size(60, 17);
            this.chkOnNight1.TabIndex = 9;
            this.chkOnNight1.Text = "Night 1";
            this.chkOnNight1.UseVisualStyleBackColor = false;
            this.chkOnNight1.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkOnDay2
            // 
            this.chkOnDay2.AutoSize = true;
            this.chkOnDay2.BackColor = System.Drawing.Color.Transparent;
            this.chkOnDay2.ForeColor = System.Drawing.SystemColors.Control;
            this.chkOnDay2.Location = new System.Drawing.Point(138, 449);
            this.chkOnDay2.Name = "chkOnDay2";
            this.chkOnDay2.Size = new System.Drawing.Size(54, 17);
            this.chkOnDay2.TabIndex = 10;
            this.chkOnDay2.Text = "Day 2";
            this.chkOnDay2.UseVisualStyleBackColor = false;
            this.chkOnDay2.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkOnNight2
            // 
            this.chkOnNight2.AutoSize = true;
            this.chkOnNight2.BackColor = System.Drawing.Color.Transparent;
            this.chkOnNight2.ForeColor = System.Drawing.SystemColors.Control;
            this.chkOnNight2.Location = new System.Drawing.Point(198, 449);
            this.chkOnNight2.Name = "chkOnNight2";
            this.chkOnNight2.Size = new System.Drawing.Size(60, 17);
            this.chkOnNight2.TabIndex = 11;
            this.chkOnNight2.Text = "Night 2";
            this.chkOnNight2.UseVisualStyleBackColor = false;
            this.chkOnNight2.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkOnDay3
            // 
            this.chkOnDay3.AutoSize = true;
            this.chkOnDay3.BackColor = System.Drawing.Color.Transparent;
            this.chkOnDay3.ForeColor = System.Drawing.SystemColors.Control;
            this.chkOnDay3.Location = new System.Drawing.Point(138, 472);
            this.chkOnDay3.Name = "chkOnDay3";
            this.chkOnDay3.Size = new System.Drawing.Size(54, 17);
            this.chkOnDay3.TabIndex = 12;
            this.chkOnDay3.Text = "Day 3";
            this.chkOnDay3.UseVisualStyleBackColor = false;
            this.chkOnDay3.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // chkOnNight3
            // 
            this.chkOnNight3.AutoSize = true;
            this.chkOnNight3.BackColor = System.Drawing.Color.Transparent;
            this.chkOnNight3.ForeColor = System.Drawing.SystemColors.Control;
            this.chkOnNight3.Location = new System.Drawing.Point(198, 472);
            this.chkOnNight3.Name = "chkOnNight3";
            this.chkOnNight3.Size = new System.Drawing.Size(60, 17);
            this.chkOnNight3.TabIndex = 13;
            this.chkOnNight3.Text = "Night 3";
            this.chkOnNight3.UseVisualStyleBackColor = false;
            this.chkOnNight3.CheckedChanged += new System.EventHandler(this.TimeCheckBox_CheckedChanged);
            // 
            // nudIndex
            // 
            this.nudIndex.Location = new System.Drawing.Point(632, 379);
            this.nudIndex.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudIndex.Name = "nudIndex";
            this.nudIndex.Size = new System.Drawing.Size(38, 20);
            this.nudIndex.TabIndex = 14;
            this.nudIndex.ValueChanged += new System.EventHandler(this.NudIndex_ValueChanged);
            // 
            // lblDicName
            // 
            this.lblDicName.AutoSize = true;
            this.lblDicName.BackColor = System.Drawing.Color.Transparent;
            this.lblDicName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDicName.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblDicName.Location = new System.Drawing.Point(264, 419);
            this.lblDicName.Name = "lblDicName";
            this.lblDicName.Size = new System.Drawing.Size(61, 13);
            this.lblDicName.TabIndex = 15;
            this.lblDicName.Text = "Item_Name";
            // 
            // btnAddReq
            // 
            this.btnAddReq.Location = new System.Drawing.Point(12, 379);
            this.btnAddReq.Name = "btnAddReq";
            this.btnAddReq.Size = new System.Drawing.Size(104, 21);
            this.btnAddReq.TabIndex = 16;
            this.btnAddReq.Text = "Add Required";
            this.btnAddReq.UseVisualStyleBackColor = true;
            this.btnAddReq.Click += new System.EventHandler(this.btnAddReq_Click);
            // 
            // btnAddCond
            // 
            this.btnAddCond.Location = new System.Drawing.Point(267, 379);
            this.btnAddCond.Name = "btnAddCond";
            this.btnAddCond.Size = new System.Drawing.Size(104, 21);
            this.btnAddCond.TabIndex = 17;
            this.btnAddCond.Text = "Add Conditional";
            this.btnAddCond.UseVisualStyleBackColor = true;
            this.btnAddCond.Click += new System.EventHandler(this.btnAddCond_Click);
            // 
            // btnRemoveReq
            // 
            this.btnRemoveReq.Location = new System.Drawing.Point(122, 379);
            this.btnRemoveReq.Name = "btnRemoveReq";
            this.btnRemoveReq.Size = new System.Drawing.Size(104, 21);
            this.btnRemoveReq.TabIndex = 22;
            this.btnRemoveReq.Text = "Remove Selected";
            this.btnRemoveReq.UseVisualStyleBackColor = true;
            this.btnRemoveReq.Click += new System.EventHandler(this.btnRemoveReq_Click);
            // 
            // btnRemoveCond
            // 
            this.btnRemoveCond.Location = new System.Drawing.Point(377, 379);
            this.btnRemoveCond.Name = "btnRemoveCond";
            this.btnRemoveCond.Size = new System.Drawing.Size(104, 21);
            this.btnRemoveCond.TabIndex = 23;
            this.btnRemoveCond.Text = "Remove Selected";
            this.btnRemoveCond.UseVisualStyleBackColor = true;
            this.btnRemoveCond.Click += new System.EventHandler(this.btnRemoveCond_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(9, 410);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Needed By";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(135, 410);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "Available On";
            // 
            // lblItemName
            // 
            this.lblItemName.AutoSize = true;
            this.lblItemName.BackColor = System.Drawing.Color.Transparent;
            this.lblItemName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemName.ForeColor = System.Drawing.SystemColors.Control;
            this.lblItemName.Location = new System.Drawing.Point(264, 448);
            this.lblItemName.Name = "lblItemName";
            this.lblItemName.Size = new System.Drawing.Size(61, 13);
            this.lblItemName.TabIndex = 27;
            this.lblItemName.Text = "Item Name:";
            // 
            // lblLocName
            // 
            this.lblLocName.AutoSize = true;
            this.lblLocName.BackColor = System.Drawing.Color.Transparent;
            this.lblLocName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLocName.ForeColor = System.Drawing.SystemColors.Control;
            this.lblLocName.Location = new System.Drawing.Point(264, 476);
            this.lblLocName.Name = "lblLocName";
            this.lblLocName.Size = new System.Drawing.Size(82, 13);
            this.lblLocName.TabIndex = 28;
            this.lblLocName.Text = "Location Name:";
            // 
            // btnBack
            // 
            this.btnBack.Location = new System.Drawing.Point(739, 379);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(58, 21);
            this.btnBack.TabIndex = 29;
            this.btnBack.Text = "Go Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnGoTo
            // 
            this.btnGoTo.Location = new System.Drawing.Point(675, 379);
            this.btnGoTo.Name = "btnGoTo";
            this.btnGoTo.Size = new System.Drawing.Size(58, 21);
            this.btnGoTo.TabIndex = 30;
            this.btnGoTo.Text = "Go To";
            this.btnGoTo.UseVisualStyleBackColor = true;
            this.btnGoTo.Click += new System.EventHandler(this.BtnGoTo_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label3.Location = new System.Drawing.Point(264, 406);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 33;
            this.label3.Text = "Logic Name:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label4.Location = new System.Drawing.Point(264, 435);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 34;
            this.label4.Text = "Item Name:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label5.Location = new System.Drawing.Point(264, 463);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 35;
            this.label5.Text = "Location Name:";
            // 
            // btnEditSelected
            // 
            this.btnEditSelected.Location = new System.Drawing.Point(487, 379);
            this.btnEditSelected.Name = "btnEditSelected";
            this.btnEditSelected.Size = new System.Drawing.Size(104, 21);
            this.btnEditSelected.TabIndex = 36;
            this.btnEditSelected.Text = "Edit Selected";
            this.btnEditSelected.UseVisualStyleBackColor = true;
            this.btnEditSelected.Click += new System.EventHandler(this.btnEditSelected_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.newItemToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(810, 24);
            this.menuStrip1.TabIndex = 37;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveLogicToolStripMenuItem,
            this.applyToTrackerLogicToolStripMenuItem,
            this.loadLogicToolStripMenuItem,
            this.newLogicToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveLogicToolStripMenuItem
            // 
            this.saveLogicToolStripMenuItem.Name = "saveLogicToolStripMenuItem";
            this.saveLogicToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.saveLogicToolStripMenuItem.Text = "Save Logic";
            this.saveLogicToolStripMenuItem.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // applyToTrackerLogicToolStripMenuItem
            // 
            this.applyToTrackerLogicToolStripMenuItem.Name = "applyToTrackerLogicToolStripMenuItem";
            this.applyToTrackerLogicToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.applyToTrackerLogicToolStripMenuItem.Text = "Apply Logic to Tracker";
            this.applyToTrackerLogicToolStripMenuItem.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // loadLogicToolStripMenuItem
            // 
            this.loadLogicToolStripMenuItem.Name = "loadLogicToolStripMenuItem";
            this.loadLogicToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.loadLogicToolStripMenuItem.Text = "Load Logic";
            this.loadLogicToolStripMenuItem.Click += new System.EventHandler(this.BtnLoad_Click);
            // 
            // newLogicToolStripMenuItem
            // 
            this.newLogicToolStripMenuItem.Name = "newLogicToolStripMenuItem";
            this.newLogicToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.newLogicToolStripMenuItem.Text = "New Logic";
            this.newLogicToolStripMenuItem.Click += new System.EventHandler(this.btnNewLogic_Click);
            // 
            // newItemToolStripMenuItem
            // 
            this.newItemToolStripMenuItem.Name = "newItemToolStripMenuItem";
            this.newItemToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.newItemToolStripMenuItem.Text = "New Item";
            this.newItemToolStripMenuItem.Click += new System.EventHandler(this.btnNewItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.useLocationItemNamesToolStripMenuItem,
            this.displaySpoilerLogNamesToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // useLocationItemNamesToolStripMenuItem
            // 
            this.useLocationItemNamesToolStripMenuItem.Name = "useLocationItemNamesToolStripMenuItem";
            this.useLocationItemNamesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.useLocationItemNamesToolStripMenuItem.Text = "Use Location/Item Names";
            this.useLocationItemNamesToolStripMenuItem.Click += new System.EventHandler(this.useLocationItemNamesToolStripMenuItem_Click);
            // 
            // displaySpoilerLogNamesToolStripMenuItem
            // 
            this.displaySpoilerLogNamesToolStripMenuItem.Name = "displaySpoilerLogNamesToolStripMenuItem";
            this.displaySpoilerLogNamesToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.displaySpoilerLogNamesToolStripMenuItem.Text = "Display Spoiler Log Names";
            this.displaySpoilerLogNamesToolStripMenuItem.Click += new System.EventHandler(this.displaySpoilerLogNamesToolStripMenuItem_Click);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.btnRedo_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.Control;
            this.label6.Location = new System.Drawing.Point(9, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 13);
            this.label6.TabIndex = 38;
            this.label6.Text = "Required Items";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.SystemColors.Control;
            this.label7.Location = new System.Drawing.Point(264, 28);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(104, 13);
            this.label7.TabIndex = 39;
            this.label7.Text = "Conditional Items";
            // 
            // btnUp
            // 
            this.btnUp.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnUp.BackgroundImage")));
            this.btnUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnUp.Location = new System.Drawing.Point(762, 27);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(15, 15);
            this.btnUp.TabIndex = 41;
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnDown.BackgroundImage")));
            this.btnDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDown.Location = new System.Drawing.Point(783, 27);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(15, 15);
            this.btnDown.TabIndex = 42;
            this.btnDown.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // LogicEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(810, 495);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnEditSelected);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnGoTo);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.lblLocName);
            this.Controls.Add(this.lblItemName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRemoveCond);
            this.Controls.Add(this.btnRemoveReq);
            this.Controls.Add(this.btnAddCond);
            this.Controls.Add(this.btnAddReq);
            this.Controls.Add(this.lblDicName);
            this.Controls.Add(this.nudIndex);
            this.Controls.Add(this.chkOnNight3);
            this.Controls.Add(this.chkOnDay3);
            this.Controls.Add(this.chkOnNight2);
            this.Controls.Add(this.chkOnDay2);
            this.Controls.Add(this.chkOnNight1);
            this.Controls.Add(this.chkOnDay1);
            this.Controls.Add(this.chkNeedNight3);
            this.Controls.Add(this.chkNeedDay3);
            this.Controls.Add(this.chkNeedNight2);
            this.Controls.Add(this.chkNeedDay2);
            this.Controls.Add(this.chkNeedNight1);
            this.Controls.Add(this.chkNeedDay1);
            this.Controls.Add(this.LBConditional);
            this.Controls.Add(this.LBRequired);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "LogicEditor";
            this.Text = "LogicEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogicEditor_FormClosing);
            this.Load += new System.EventHandler(this.LogicEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudIndex)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LBRequired;
        private System.Windows.Forms.ListBox LBConditional;
        private System.Windows.Forms.CheckBox chkNeedDay1;
        private System.Windows.Forms.CheckBox chkNeedNight1;
        private System.Windows.Forms.CheckBox chkNeedDay2;
        private System.Windows.Forms.CheckBox chkNeedNight2;
        private System.Windows.Forms.CheckBox chkNeedDay3;
        private System.Windows.Forms.CheckBox chkNeedNight3;
        private System.Windows.Forms.CheckBox chkOnDay1;
        private System.Windows.Forms.CheckBox chkOnNight1;
        private System.Windows.Forms.CheckBox chkOnDay2;
        private System.Windows.Forms.CheckBox chkOnNight2;
        private System.Windows.Forms.CheckBox chkOnDay3;
        private System.Windows.Forms.CheckBox chkOnNight3;
        private System.Windows.Forms.NumericUpDown nudIndex;
        private System.Windows.Forms.Label lblDicName;
        private System.Windows.Forms.Button btnAddReq;
        private System.Windows.Forms.Button btnAddCond;
        private System.Windows.Forms.Button btnRemoveReq;
        private System.Windows.Forms.Button btnRemoveCond;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblItemName;
        private System.Windows.Forms.Label lblLocName;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnGoTo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnEditSelected;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveLogicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadLogicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newLogicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyToTrackerLogicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newItemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem useLocationItemNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displaySpoilerLogNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
    }
}