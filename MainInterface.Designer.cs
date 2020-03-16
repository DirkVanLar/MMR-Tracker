namespace MMR_Tracker_V2
{
    partial class FRMTracker
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FRMTracker));
            this.TXTLocSearch = new System.Windows.Forms.TextBox();
            this.TXTEntSearch = new System.Windows.Forms.TextBox();
            this.TXTCheckedSearch = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.BTNSetItem = new System.Windows.Forms.Button();
            this.BTNSetEntrance = new System.Windows.Forms.Button();
            this.CHKShowAll = new System.Windows.Forms.CheckBox();
            this.CMBStart = new System.Windows.Forms.ComboBox();
            this.CMBEnd = new System.Windows.Forms.ComboBox();
            this.LBValidLocations = new System.Windows.Forms.ListBox();
            this.LBValidEntrances = new System.Windows.Forms.ListBox();
            this.LBCheckedLocations = new System.Windows.Forms.ListBox();
            this.LBPathFinder = new System.Windows.Forms.ListBox();
            this.BTNFindPath = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.entranceRandoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useSongOfTimeInPathfinderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleEntranceRandoFeaturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.includeItemLocationsAsDestinationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logicOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editRadnomizationOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importSpoilerLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stricterLogicHandelingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.devToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createDictionaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printLogicObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateDisplayNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miscOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showEntryNameToolTipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.coupleEntrancesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TXTLocSearch
            // 
            this.TXTLocSearch.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TXTLocSearch.Location = new System.Drawing.Point(12, 39);
            this.TXTLocSearch.Name = "TXTLocSearch";
            this.TXTLocSearch.Size = new System.Drawing.Size(100, 20);
            this.TXTLocSearch.TabIndex = 0;
            this.TXTLocSearch.TextChanged += new System.EventHandler(this.TXTLocSearch_TextChanged);
            // 
            // TXTEntSearch
            // 
            this.TXTEntSearch.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TXTEntSearch.Location = new System.Drawing.Point(12, 65);
            this.TXTEntSearch.Name = "TXTEntSearch";
            this.TXTEntSearch.Size = new System.Drawing.Size(100, 20);
            this.TXTEntSearch.TabIndex = 1;
            this.TXTEntSearch.TextChanged += new System.EventHandler(this.TXTEntSearch_TextChanged);
            // 
            // TXTCheckedSearch
            // 
            this.TXTCheckedSearch.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TXTCheckedSearch.Location = new System.Drawing.Point(12, 91);
            this.TXTCheckedSearch.Name = "TXTCheckedSearch";
            this.TXTCheckedSearch.Size = new System.Drawing.Size(100, 20);
            this.TXTCheckedSearch.TabIndex = 2;
            this.TXTCheckedSearch.TextChanged += new System.EventHandler(this.TXTCheckedSearch_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.label1.Location = new System.Drawing.Point(118, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Available Locations";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.label2.Location = new System.Drawing.Point(118, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Checked locations";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.label3.Location = new System.Drawing.Point(118, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Available Entrances";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.label4.Location = new System.Drawing.Point(118, 120);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Path Finder";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.label5.Location = new System.Drawing.Point(118, 146);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Start";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.label6.Location = new System.Drawing.Point(118, 169);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Destination";
            // 
            // BTNSetItem
            // 
            this.BTNSetItem.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BTNSetItem.Location = new System.Drawing.Point(12, 174);
            this.BTNSetItem.Name = "BTNSetItem";
            this.BTNSetItem.Size = new System.Drawing.Size(80, 20);
            this.BTNSetItem.TabIndex = 11;
            this.BTNSetItem.Text = "Set Item";
            this.BTNSetItem.UseVisualStyleBackColor = false;
            this.BTNSetItem.Click += new System.EventHandler(this.BTNSetItem_Click);
            // 
            // BTNSetEntrance
            // 
            this.BTNSetEntrance.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BTNSetEntrance.Location = new System.Drawing.Point(12, 203);
            this.BTNSetEntrance.Name = "BTNSetEntrance";
            this.BTNSetEntrance.Size = new System.Drawing.Size(80, 20);
            this.BTNSetEntrance.TabIndex = 12;
            this.BTNSetEntrance.Text = "Set Entrance";
            this.BTNSetEntrance.UseVisualStyleBackColor = false;
            this.BTNSetEntrance.Click += new System.EventHandler(this.BTNSetEntrance_Click);
            // 
            // CHKShowAll
            // 
            this.CHKShowAll.AutoSize = true;
            this.CHKShowAll.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CHKShowAll.Location = new System.Drawing.Point(121, 203);
            this.CHKShowAll.Name = "CHKShowAll";
            this.CHKShowAll.Size = new System.Drawing.Size(67, 17);
            this.CHKShowAll.TabIndex = 13;
            this.CHKShowAll.Text = "Show All";
            this.CHKShowAll.UseVisualStyleBackColor = false;
            this.CHKShowAll.CheckedChanged += new System.EventHandler(this.CHKShowAll_CheckedChanged);
            // 
            // CMBStart
            // 
            this.CMBStart.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CMBStart.ForeColor = System.Drawing.SystemColors.InfoText;
            this.CMBStart.FormattingEnabled = true;
            this.CMBStart.Location = new System.Drawing.Point(12, 120);
            this.CMBStart.Name = "CMBStart";
            this.CMBStart.Size = new System.Drawing.Size(100, 21);
            this.CMBStart.TabIndex = 14;
            this.CMBStart.DropDown += new System.EventHandler(this.CMBStart_DropDown);
            // 
            // CMBEnd
            // 
            this.CMBEnd.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CMBEnd.ForeColor = System.Drawing.SystemColors.InfoText;
            this.CMBEnd.FormattingEnabled = true;
            this.CMBEnd.Location = new System.Drawing.Point(12, 147);
            this.CMBEnd.Name = "CMBEnd";
            this.CMBEnd.Size = new System.Drawing.Size(100, 21);
            this.CMBEnd.TabIndex = 15;
            this.CMBEnd.DropDown += new System.EventHandler(this.CMBEnd_DropDown);
            // 
            // LBValidLocations
            // 
            this.LBValidLocations.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBValidLocations.FormattingEnabled = true;
            this.LBValidLocations.Location = new System.Drawing.Point(223, 38);
            this.LBValidLocations.Name = "LBValidLocations";
            this.LBValidLocations.Size = new System.Drawing.Size(120, 95);
            this.LBValidLocations.TabIndex = 16;
            this.LBValidLocations.DoubleClick += new System.EventHandler(this.LBValidLocations_DoubleClick);
            this.LBValidLocations.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LBValidLocations_MouseMove);
            // 
            // LBValidEntrances
            // 
            this.LBValidEntrances.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBValidEntrances.FormattingEnabled = true;
            this.LBValidEntrances.Location = new System.Drawing.Point(349, 38);
            this.LBValidEntrances.Name = "LBValidEntrances";
            this.LBValidEntrances.Size = new System.Drawing.Size(120, 95);
            this.LBValidEntrances.TabIndex = 17;
            this.LBValidEntrances.DoubleClick += new System.EventHandler(this.LBValidEntrances_DoubleClick);
            this.LBValidEntrances.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LBValidEntrances_MouseMove);
            // 
            // LBCheckedLocations
            // 
            this.LBCheckedLocations.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBCheckedLocations.FormattingEnabled = true;
            this.LBCheckedLocations.Location = new System.Drawing.Point(223, 139);
            this.LBCheckedLocations.Name = "LBCheckedLocations";
            this.LBCheckedLocations.Size = new System.Drawing.Size(120, 95);
            this.LBCheckedLocations.TabIndex = 18;
            this.LBCheckedLocations.DoubleClick += new System.EventHandler(this.LBCheckedLocations_DoubleClick);
            this.LBCheckedLocations.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LBCheckedLocations_MouseMove);
            // 
            // LBPathFinder
            // 
            this.LBPathFinder.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBPathFinder.FormattingEnabled = true;
            this.LBPathFinder.Location = new System.Drawing.Point(349, 139);
            this.LBPathFinder.Name = "LBPathFinder";
            this.LBPathFinder.Size = new System.Drawing.Size(120, 95);
            this.LBPathFinder.TabIndex = 19;
            this.LBPathFinder.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LBPathFinder_MouseMove);
            // 
            // BTNFindPath
            // 
            this.BTNFindPath.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BTNFindPath.Location = new System.Drawing.Point(12, 232);
            this.BTNFindPath.Name = "BTNFindPath";
            this.BTNFindPath.Size = new System.Drawing.Size(80, 20);
            this.BTNFindPath.TabIndex = 20;
            this.BTNFindPath.Text = "Find Path";
            this.BTNFindPath.UseVisualStyleBackColor = false;
            this.BTNFindPath.Click += new System.EventHandler(this.BTNFindPath_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.infoToolStripMenuItem,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(377, 24);
            this.menuStrip1.TabIndex = 21;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.loadToolStripMenuItem,
            this.newToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.LoadToolStripMenuItem_Click);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.entranceRandoToolStripMenuItem,
            this.logicOptionsToolStripMenuItem,
            this.devToolStripMenuItem,
            this.miscOptionsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // entranceRandoToolStripMenuItem
            // 
            this.entranceRandoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.useSongOfTimeInPathfinderToolStripMenuItem,
            this.toggleEntranceRandoFeaturesToolStripMenuItem,
            this.includeItemLocationsAsDestinationToolStripMenuItem,
            this.coupleEntrancesToolStripMenuItem});
            this.entranceRandoToolStripMenuItem.Name = "entranceRandoToolStripMenuItem";
            this.entranceRandoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.entranceRandoToolStripMenuItem.Text = "Entrance Rando";
            // 
            // useSongOfTimeInPathfinderToolStripMenuItem
            // 
            this.useSongOfTimeInPathfinderToolStripMenuItem.Name = "useSongOfTimeInPathfinderToolStripMenuItem";
            this.useSongOfTimeInPathfinderToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.useSongOfTimeInPathfinderToolStripMenuItem.Text = "Use Song of Time in Pathfinder";
            this.useSongOfTimeInPathfinderToolStripMenuItem.Click += new System.EventHandler(this.useSongOfTimeInPathfinderToolStripMenuItem_Click);
            // 
            // toggleEntranceRandoFeaturesToolStripMenuItem
            // 
            this.toggleEntranceRandoFeaturesToolStripMenuItem.Name = "toggleEntranceRandoFeaturesToolStripMenuItem";
            this.toggleEntranceRandoFeaturesToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.toggleEntranceRandoFeaturesToolStripMenuItem.Text = "Toggle Entrance Rando Features";
            this.toggleEntranceRandoFeaturesToolStripMenuItem.Click += new System.EventHandler(this.ToggleEntranceRandoFeaturesToolStripMenuItem_Click);
            // 
            // includeItemLocationsAsDestinationToolStripMenuItem
            // 
            this.includeItemLocationsAsDestinationToolStripMenuItem.Name = "includeItemLocationsAsDestinationToolStripMenuItem";
            this.includeItemLocationsAsDestinationToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.includeItemLocationsAsDestinationToolStripMenuItem.Text = "Include Item Locations as destination";
            this.includeItemLocationsAsDestinationToolStripMenuItem.Click += new System.EventHandler(this.includeItemLocationsAsDestinationToolStripMenuItem_Click);
            // 
            // logicOptionsToolStripMenuItem
            // 
            this.logicOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editRadnomizationOptionsToolStripMenuItem,
            this.importSpoilerLogToolStripMenuItem,
            this.stricterLogicHandelingToolStripMenuItem});
            this.logicOptionsToolStripMenuItem.Name = "logicOptionsToolStripMenuItem";
            this.logicOptionsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.logicOptionsToolStripMenuItem.Text = "Logic Options";
            // 
            // editRadnomizationOptionsToolStripMenuItem
            // 
            this.editRadnomizationOptionsToolStripMenuItem.Name = "editRadnomizationOptionsToolStripMenuItem";
            this.editRadnomizationOptionsToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.editRadnomizationOptionsToolStripMenuItem.Text = "Edit Randomization Options";
            this.editRadnomizationOptionsToolStripMenuItem.Click += new System.EventHandler(this.EditRadnomizationOptionsToolStripMenuItem_Click);
            // 
            // importSpoilerLogToolStripMenuItem
            // 
            this.importSpoilerLogToolStripMenuItem.Name = "importSpoilerLogToolStripMenuItem";
            this.importSpoilerLogToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.importSpoilerLogToolStripMenuItem.Text = "Import Spoiler Log";
            this.importSpoilerLogToolStripMenuItem.Click += new System.EventHandler(this.ImportSpoilerLogToolStripMenuItem_Click);
            // 
            // stricterLogicHandelingToolStripMenuItem
            // 
            this.stricterLogicHandelingToolStripMenuItem.Name = "stricterLogicHandelingToolStripMenuItem";
            this.stricterLogicHandelingToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.stricterLogicHandelingToolStripMenuItem.Text = "Stricter Logic Handeling";
            this.stricterLogicHandelingToolStripMenuItem.Click += new System.EventHandler(this.stricterLogicHandelingToolStripMenuItem_Click);
            // 
            // devToolStripMenuItem
            // 
            this.devToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createDictionaryToolStripMenuItem,
            this.printLogicObjectToolStripMenuItem,
            this.updateDisplayNamesToolStripMenuItem});
            this.devToolStripMenuItem.Name = "devToolStripMenuItem";
            this.devToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.devToolStripMenuItem.Text = "Dev";
            // 
            // createDictionaryToolStripMenuItem
            // 
            this.createDictionaryToolStripMenuItem.Name = "createDictionaryToolStripMenuItem";
            this.createDictionaryToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.createDictionaryToolStripMenuItem.Text = "Create Dictionary";
            this.createDictionaryToolStripMenuItem.Click += new System.EventHandler(this.CreateDictionaryToolStripMenuItem_Click);
            // 
            // printLogicObjectToolStripMenuItem
            // 
            this.printLogicObjectToolStripMenuItem.Name = "printLogicObjectToolStripMenuItem";
            this.printLogicObjectToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.printLogicObjectToolStripMenuItem.Text = "Print Logic Object";
            this.printLogicObjectToolStripMenuItem.Click += new System.EventHandler(this.PrintLogicObjectToolStripMenuItem_Click);
            // 
            // updateDisplayNamesToolStripMenuItem
            // 
            this.updateDisplayNamesToolStripMenuItem.Name = "updateDisplayNamesToolStripMenuItem";
            this.updateDisplayNamesToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.updateDisplayNamesToolStripMenuItem.Text = "Update Display Names";
            this.updateDisplayNamesToolStripMenuItem.Click += new System.EventHandler(this.UpdateDisplayNamesToolStripMenuItem_Click);
            // 
            // miscOptionsToolStripMenuItem
            // 
            this.miscOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showEntryNameToolTipToolStripMenuItem});
            this.miscOptionsToolStripMenuItem.Name = "miscOptionsToolStripMenuItem";
            this.miscOptionsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.miscOptionsToolStripMenuItem.Text = "Misc Options";
            // 
            // showEntryNameToolTipToolStripMenuItem
            // 
            this.showEntryNameToolTipToolStripMenuItem.Name = "showEntryNameToolTipToolStripMenuItem";
            this.showEntryNameToolTipToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.showEntryNameToolTipToolStripMenuItem.Text = "Show Entry Name ToolTip";
            this.showEntryNameToolTipToolStripMenuItem.Click += new System.EventHandler(this.showEntryNameToolTipToolStripMenuItem_Click);
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.infoToolStripMenuItem.Text = "Info";
            this.infoToolStripMenuItem.Click += new System.EventHandler(this.InfoToolStripMenuItem_Click);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.UndoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.RedoToolStripMenuItem_Click);
            // 
            // coupleEntrancesToolStripMenuItem
            // 
            this.coupleEntrancesToolStripMenuItem.Name = "coupleEntrancesToolStripMenuItem";
            this.coupleEntrancesToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.coupleEntrancesToolStripMenuItem.Text = "Couple Entrances";
            this.coupleEntrancesToolStripMenuItem.Click += new System.EventHandler(this.coupleEntrancesToolStripMenuItem_Click);
            // 
            // FRMTracker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(377, 586);
            this.Controls.Add(this.BTNFindPath);
            this.Controls.Add(this.LBPathFinder);
            this.Controls.Add(this.LBCheckedLocations);
            this.Controls.Add(this.LBValidEntrances);
            this.Controls.Add(this.LBValidLocations);
            this.Controls.Add(this.CMBEnd);
            this.Controls.Add(this.CMBStart);
            this.Controls.Add(this.CHKShowAll);
            this.Controls.Add(this.BTNSetEntrance);
            this.Controls.Add(this.BTNSetItem);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TXTCheckedSearch);
            this.Controls.Add(this.TXTEntSearch);
            this.Controls.Add(this.TXTLocSearch);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FRMTracker";
            this.Text = "MMR Tracker";
            this.Load += new System.EventHandler(this.FRMTracker_Load);
            this.ResizeEnd += new System.EventHandler(this.FRMTracker_ResizeEnd);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TXTLocSearch;
        private System.Windows.Forms.TextBox TXTEntSearch;
        private System.Windows.Forms.TextBox TXTCheckedSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button BTNSetItem;
        private System.Windows.Forms.Button BTNSetEntrance;
        private System.Windows.Forms.CheckBox CHKShowAll;
        private System.Windows.Forms.ComboBox CMBStart;
        private System.Windows.Forms.ComboBox CMBEnd;
        private System.Windows.Forms.ListBox LBValidLocations;
        private System.Windows.Forms.ListBox LBValidEntrances;
        private System.Windows.Forms.ListBox LBCheckedLocations;
        private System.Windows.Forms.ListBox LBPathFinder;
        private System.Windows.Forms.Button BTNFindPath;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem infoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem entranceRandoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem useSongOfTimeInPathfinderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleEntranceRandoFeaturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem devToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createDictionaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printLogicObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateDisplayNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logicOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editRadnomizationOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importSpoilerLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stricterLogicHandelingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miscOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showEntryNameToolTipToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem includeItemLocationsAsDestinationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem coupleEntrancesToolStripMenuItem;
    }
}

