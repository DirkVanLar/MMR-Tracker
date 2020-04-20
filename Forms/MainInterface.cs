using Microsoft.VisualBasic;
using MMR_Tracker;
using MMR_Tracker.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using MMR_Tracker.Class_Files;
using System.Threading.Tasks;

namespace MMR_Tracker_V2
{
    public partial class MainInterface : Form
    {
        public static event EventHandler LocationChecked = delegate { };
        public static event EventHandler TrackerUpdate = delegate { };

        public MainInterface()
        {
            InitializeComponent();
        }

        #region Form Objects
        //Form Events---------------------------------------------------------------------------
        #region Form Events
        private void FRMTracker_Load(object sender, EventArgs e)
        {
            Debugging.ISDebugging = (Control.ModifierKeys == Keys.Control) ? (!Debugger.IsAttached) : (Debugger.IsAttached);
            Tools.CreateOptionsFile();
            if (VersionHandeling.GetLatestTrackerVersion()) { this.Close(); }
            ResizeObject();
            FormatMenuItems();
        }

        private void FRMTracker_ResizeEnd(object sender, EventArgs e) { ResizeObject(); }

        private void FRMTracker_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !Tools.PromptSave(LogicObjects.MainTrackerInstance);
        }
        #endregion Form Events
        //Menu Strip---------------------------------------------------------------------------
        #region Form Events
        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.Redo(LogicObjects.MainTrackerInstance);
            PrintToListBox();
            FormatMenuItems();
            FireEvents();
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.Undo(LogicObjects.MainTrackerInstance);
            PrintToListBox();
            FormatMenuItems();
            FireEvents();
        }
        #endregion Form Events
        //Menu Strip => File---------------------------------------------------------------------------
        #region File
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.SaveInstance(LogicObjects.MainTrackerInstance);
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.LoadInstance();
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
            FireEvents();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.fileToolStripMenuItem.HideDropDown();
            if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; }
            string file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSET)|*.txt;*.MMRTSET");
            if (file == "") { return; }

            bool SettingsFile = file.EndsWith(".MMRTSET");
            var lines = (SettingsFile) ? File.ReadAllLines(file).Skip(2) : File.ReadAllLines(file);

            LogicObjects.MainTrackerInstance = new LogicObjects.TrackerInstance();

            Tools.CreateTrackerInstance(LogicObjects.MainTrackerInstance, lines.ToArray());

            if (SettingsFile)
            {
                RandomizeOptions.UpdateRandomOptionsFromFile(File.ReadAllLines(file), LogicObjects.MainTrackerInstance);
                LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled = Utility.CheckForRandomEntrances(LogicObjects.MainTrackerInstance);
                LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable = (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled != LogicObjects.MainTrackerInstance.EntranceRando);
            }

            Console.WriteLine("Settings Entrance: " + LogicObjects.MainTrackerInstance.Options.UnradnomizeEntranesOnStartup);
             
            if (LogicObjects.MainTrackerInstance.EntranceRando && !SettingsFile && LogicObjects.MainTrackerInstance.Options.UnradnomizeEntranesOnStartup)
            {
                LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled = false;
                LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable = true;
                foreach (var item in LogicObjects.MainTrackerInstance.Logic)
                {
                    if (item.IsEntrance()) { item.Options = 1; }
                }
            }
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
            FireEvents();
        }
        #endregion File
        //Menu Strip => File => New---------------------------------------------------------------------------
        #region New
        private void CasualLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; }
            System.Net.WebClient wc = new System.Net.WebClient();
            string webData = wc.DownloadString("https://raw.githubusercontent.com/ZoeyZolotova/mm-rando/dev/MMR.Randomizer/Resources/REQ_CASUAL.txt");
            string[] Lines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            LogicObjects.MainTrackerInstance = new LogicObjects.TrackerInstance();
            Tools.CreateTrackerInstance(LogicObjects.MainTrackerInstance, Lines.ToArray());
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
            FireEvents();
        }

        private void GlitchedLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; }
            System.Net.WebClient wc = new System.Net.WebClient();
            string webData = wc.DownloadString("https://raw.githubusercontent.com/ZoeyZolotova/mm-rando/dev/MMR.Randomizer/Resources/REQ_GLITCH.txt");
            string[] Lines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            LogicObjects.MainTrackerInstance = new LogicObjects.TrackerInstance();
            Tools.CreateTrackerInstance(LogicObjects.MainTrackerInstance, Lines.ToArray());
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
            FireEvents();
        }
        #endregion New
        //Menu Strip => Options => Logic Options---------------------------------------------------------------------------
        #region Logic Options
        private void EditRadnomizationOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; }
            bool EntrancesRandoBefore = Utility.CheckForRandomEntrances(LogicObjects.MainTrackerInstance);
            RandomizeOptions RandoOptionScreen = new RandomizeOptions();
            RandoOptionScreen.ShowDialog();
            bool EntrancesRandoAfter = Utility.CheckForRandomEntrances(LogicObjects.MainTrackerInstance);

            if (!LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable || (EntrancesRandoBefore != EntrancesRandoAfter))
            {
                LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled = Utility.CheckForRandomEntrances(LogicObjects.MainTrackerInstance);
                LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable = (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled != LogicObjects.MainTrackerInstance.EntranceRando);
            }

            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }

        private void ImportSpoilerLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var instance = LogicObjects.MainTrackerInstance;
            if (Utility.CheckforSpoilerLog(LogicObjects.MainTrackerInstance.Logic))
            {
                foreach (var entry in LogicObjects.MainTrackerInstance.Logic) { entry.SpoilerRandom = -2; }
            }
            else
            {
                string file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt;*html)|*.txt;*html");
                if (file == "") { return; }
                LogicEditing.WriteSpoilerLogToLogic(instance, file);
                if (!Utility.CheckforSpoilerLog(instance.Logic)) { MessageBox.Show("No spoiler data found!"); return; }
                else if (!Utility.CheckforSpoilerLog(instance.Logic, true)) { MessageBox.Show("Not all checks have been assigned spoiler data!"); }

                bool EntrancesRandoBefore = Utility.CheckForRandomEntrances(instance);
                Utility.FixSpoilerInconsistency(LogicObjects.MainTrackerInstance);
                bool EntrancesRandoAfter = Utility.CheckForRandomEntrances(instance);
                if (!instance.Options.OverRideAutoEntranceRandoEnable || (EntrancesRandoBefore != EntrancesRandoAfter))
                {
                    instance.Options.EntranceRadnoEnabled = EntrancesRandoAfter;
                    instance.Options.OverRideAutoEntranceRandoEnable = (instance.Options.EntranceRadnoEnabled != LogicObjects.MainTrackerInstance.EntranceRando);
                }

                LogicEditing.CalculateItems(instance);
            }
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }

        private void StricterLogicHandelingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.StrictLogicHandeling = !LogicObjects.MainTrackerInstance.Options.StrictLogicHandeling;
            FormatMenuItems();
        }
        #endregion Logic Options
        //Menu Strip => Options => Entrance Rando---------------------------------------------------------------------------
        #region Entrance Rando
        private void UseSongOfTimeInPathfinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.UseSongOfTime = !LogicObjects.MainTrackerInstance.Options.UseSongOfTime;
            FormatMenuItems();
        }

        private void ToggleEntranceRandoFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled = !LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable = (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled != LogicObjects.MainTrackerInstance.EntranceRando);
            ResizeObject();
            PrintToListBox();
            FormatMenuItems();
        }

        private void IncludeItemLocationsAsDestinationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.IncludeItemLocations = !LogicObjects.MainTrackerInstance.Options.IncludeItemLocations;
            FormatMenuItems();
        }

        private void CoupleEntrancesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!LogicObjects.MainTrackerInstance.Options.CoupleEntrances) { if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; } }
            LogicObjects.MainTrackerInstance.Options.CoupleEntrances = !LogicObjects.MainTrackerInstance.Options.CoupleEntrances;
            if (!LogicObjects.MainTrackerInstance.Options.CoupleEntrances) { MessageBox.Show("Entrances will not uncouple automatically."); }
            if (LogicObjects.MainTrackerInstance.Options.CoupleEntrances)
            {
                LogicObjects.MainTrackerInstance.UnsavedChanges = true;
                Tools.SaveState(LogicObjects.MainTrackerInstance);
                foreach (var entry in LogicObjects.MainTrackerInstance.Logic)
                {
                    if (entry.Checked && entry.RandomizedItem > -1)
                    {
                        LogicEditing.CheckEntrancePair(LogicObjects.MainTrackerInstance.Logic[entry.ID], LogicObjects.MainTrackerInstance, true);
                    }
                }
                LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance, true);
                PrintToListBox();
            }
            FormatMenuItems();
        }
        #endregion Entrance Rando
        //Menu Strip => Options => Dev---------------------------------------------------------------------------
        #region Dev
        private void CreateDictionaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.CreateDictionary();
        }

        private void PrintLogicObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InformationDisplay DebugScreen = new InformationDisplay();
            Debugging.PrintLogicObject(LogicObjects.MainTrackerInstance.Logic);
            DebugScreen.DebugFunction = 1;
            DebugScreen.Show();
        }

        private void UpdateDisplayNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; }
            Tools.UpdateNames(LogicObjects.MainTrackerInstance);
            PrintToListBox();
        }

        private void DumbStuffToolStripMenuItem_Click(object sender, EventArgs e) { Debugging.TestDumbStuff(); }

        private void CreateOOTFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void VerifyCustomRandoCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
        #endregion Dev
        //Menu Strip => Options => MISC Options---------------------------------------------------------------------------
        #region MISC Options
        private void ShowEntryNameToolTipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.ShowEntryNameTooltip = !LogicObjects.MainTrackerInstance.Options.ShowEntryNameTooltip;
            FormatMenuItems();
        }

        private void seperateMarkedItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom = !LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom;
            FormatMenuItems();
            PrintToListBox();
        }
        #endregion MISC Options
        //Menu Strip => Tools---------------------------------------------------------------------------
        #region Tools
        private void SeedCheckerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SeedChecker SeedCheckerForm = new SeedChecker();
            SeedCheckerForm.Show();
        }

        private void GeneratePlaythroughToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaythroughGenerator.GeneratePlaythrough(LogicObjects.MainTrackerInstance);
        }

        private void WhatUnlockedThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine(this.ActiveControl == LBValidLocations);
            Console.WriteLine(LBValidLocations.SelectedItem is LogicObjects.LogicEntry);
            if ((this.ActiveControl == LBValidLocations) && LBValidLocations.SelectedItem is LogicObjects.LogicEntry)
            {
                Tools.CurrentSelectedItem = LBValidLocations.SelectedItem as LogicObjects.LogicEntry;
            }
            else if ((this.ActiveControl == LBValidEntrances) && LBValidEntrances.SelectedItem is LogicObjects.LogicEntry)
            {
                Tools.CurrentSelectedItem = LBValidEntrances.SelectedItem as LogicObjects.LogicEntry;
            }
            else
            {
                ItemSelect ItemSelectForm = new ItemSelect();
                ItemSelect.Function = 3;
                var dialogResult = ItemSelectForm.ShowDialog();
                if (dialogResult != DialogResult.OK) { Tools.CurrentSelectedItem = new LogicObjects.LogicEntry(); return; }
            }
            Tools.WhatUnlockedThis();
        }

        private void LogicEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicEditor Editor = new LogicEditor();
            Editor.ShowDialog();
            PrintToListBox();
            FormatMenuItems();
        }

        private void UpdateLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; }
            LogicEditing.RecreateLogic(LogicObjects.MainTrackerInstance);
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }

        private void PopoutPathfinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PathFinder Poputpathfinder = new PathFinder();
            Poputpathfinder.Show();
        }

        private void FilterMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map FilterMap = new Map();
            FilterMap.MainInterfaceInstance = this;
            FilterMap.Show();
        }

        private void itemTrackerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ItemDisplay id = new ItemDisplay();
            id.MainInterfaceInstance = this;
            id.Show();

        }
        #endregion Tools
        //Menu strip => Info---------------------------------------------------------------------------
        #region Info
        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InformationDisplay DebugScreen = new InformationDisplay();
            Debugging.PrintLogicObject(LogicObjects.MainTrackerInstance.Logic);
            DebugScreen.DebugFunction = 2;
            DebugScreen.Show();
        }

        private void IkanaWellMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string link = "https://lh3.googleusercontent.com/C0lTSDAQVpM_AeYM_WAGsbFCXvOLHkrgw2pFjh5BGLKfyyIs-S8iUboYrapNpiHIYqEKdQTrLPSCkG-EBOztDKnhEfDNu-IqXspp5cjfmjumpEYqGb6u_-h0SpUsR28c41NljrXIJA";
            Form form = new Form();
            var request = WebRequest.Create(link);
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream()) { form.BackgroundImage = Bitmap.FromStream(stream); }
            form.Width = 500;
            form.Height = 500;
            form.BackgroundImageLayout = ImageLayout.Stretch;
            form.Text = "Showing Web page: " + link;
            form.Show();
        }

        private void WoodsOfMysteryRouteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string link = "https://gamefaqs.gamespot.com/n64/197770-the-legend-of-zelda-majoras-mask/map/761?raw=1";
            Form form = new Form();
            var request = WebRequest.Create(link);
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream()) { form.BackgroundImage = Bitmap.FromStream(stream); }
            form.Width = 500;
            form.Height = 500;
            form.BackgroundImageLayout = ImageLayout.Stretch;
            form.Text = "Showing Web page: " + link;
            form.Show();
        }

        private void BombersCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var text = (LogicObjects.MainTrackerInstance.Options.BomberCode == "") ? "Enter your bombers code below." : "Bomber code: \n" + LogicObjects.MainTrackerInstance.Options.BomberCode + "\nEnter a new code to change it.";
            string name = Interaction.InputBox(text, "Bomber Code", "");
            if (name != "") { LogicObjects.MainTrackerInstance.Options.BomberCode = name; }
        }

        private void TimedEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var text = (LogicObjects.MainTrackerInstance.Options.LotteryNumber == "") ? "Enter your Lottery Number(s) below." : "Lottery Number(s): \n" + LogicObjects.MainTrackerInstance.Options.LotteryNumber + "\nEnter Lottery Number(s) to change it.";
            string name = Interaction.InputBox(text, "Lottery Number(s)", "");
            if (name != "") { LogicObjects.MainTrackerInstance.Options.LotteryNumber = name; }
        }

        private void OcarinaSongsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form
            {
                BackgroundImage = Bitmap.FromFile(@"Recources\Images\Ocarina Songs.PNG"),
                Width = 500,
                Height = 500,
                BackgroundImageLayout = ImageLayout.Stretch
            };
            form.Show();
        }
        #endregion Info
        //Text Boxes---------------------------------------------------------------------------
        #region Text Boxes
        private void TXTLocSearch_TextChanged(object sender, EventArgs e) { PrintToListBox(); }

        private void TXTEntSearch_TextChanged(object sender, EventArgs e) { PrintToListBox(); }

        private void TXTCheckedSearch_TextChanged(object sender, EventArgs e) { PrintToListBox(); }

        private void TXTLocSearch_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { TXTLocSearch.Clear(); }
        }

        private void TXTEntSearch_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { TXTEntSearch.Clear(); }
        }

        private void TXTCheckedSearch_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { TXTCheckedSearch.Clear(); }
        }
        #endregion Text Boxes
        //List Boxes---------------------------------------------------------------------------
        #region List Boxes
        private void LBValidLocations_DoubleClick(object sender, EventArgs e) { CheckItemSelected(LBValidLocations, true); }

        private void LBValidEntrances_DoubleClick(object sender, EventArgs e) { CheckItemSelected(LBValidEntrances, true); }

        private void LBCheckedLocations_DoubleClick(object sender, EventArgs e) { CheckItemSelected(LBCheckedLocations, true); }

        private void LBPathFinder_DoubleClick(object sender, EventArgs e)
        {
            if (LBPathFinder.SelectedItem is LogicObjects.ListItem)
            {
                var item = LBPathFinder.SelectedItem as LogicObjects.ListItem;
                var partition = item.Identifier;
                PrintPaths(item.ID, partition);
            }
            else { return; }
        }

        private void LBValidLocations_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBValidLocations); }

        private void LBValidEntrances_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBValidEntrances); }

        private void LBCheckedLocations_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBCheckedLocations); }

        private void LBPathFinder_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBPathFinder); }

        private void LBValidLocations_MouseUp(object sender, MouseEventArgs e)
        {
            int index = LBValidLocations.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            if (!(LBValidLocations.Items[index] is LogicObjects.LogicEntry)) { return; }
            if (e.Button == MouseButtons.Middle)
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { LBValidLocations.SelectedItems.Clear(); }
                LBValidLocations.SetSelected(index, true);
                CheckItemSelected(LBValidLocations, false);
            }
            else if (e.Button == MouseButtons.Right)
            {
                LBValidLocations.SelectedItems.Clear();
                LBValidLocations.SetSelected(index, true);
                this.ActiveControl = LBValidLocations;

            }
        }

        private void LBValidEntrances_MouseUp(object sender, MouseEventArgs e)
        {
            int index = LBValidEntrances.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            if (!(LBValidEntrances.Items[index] is LogicObjects.LogicEntry)) { return; }
            if (e.Button == MouseButtons.Middle)
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { LBValidEntrances.SelectedItems.Clear(); }
                LBValidEntrances.SetSelected(index, true);
                CheckItemSelected(LBValidEntrances, false);
            }
            else if (e.Button == MouseButtons.Right)
            {
                LBValidEntrances.SelectedItems.Clear();
                LBValidEntrances.SetSelected(index, true);
                this.ActiveControl = LBValidEntrances;
            }
        }
        #endregion List Boxes
        //Buttons---------------------------------------------------------------------------
        #region Buttons
        private void BTNSetItem_Click(object sender, EventArgs e) { CheckItemSelected(LBValidLocations, false); }

        private void BTNSetEntrance_Click(object sender, EventArgs e) { CheckItemSelected(LBValidEntrances, false); }

        private async void BTNFindPath_Click(object sender, EventArgs e)
        {
            int partition = PathFinder.paths.Count();
            PathFinder.paths.Add(new List<List<LogicObjects.MapPoint>>());
            LBPathFinder.Items.Clear();

            if (!(CMBStart.SelectedItem is KeyValuePair<int, string>) || !(CMBEnd.SelectedItem is KeyValuePair<int, string>)) { return; }
            var Startindex = Int32.Parse(CMBStart.SelectedValue.ToString());
            var DestIndex = Int32.Parse(CMBEnd.SelectedValue.ToString());
            if (Startindex < 0 || DestIndex < 0) { return; }
            LBPathFinder.Items.Add("Finding Path.....");
            LBPathFinder.Items.Add("Please Wait");
            LBPathFinder.Refresh();

            bool DestinationAtStarting = await Task.Run(() => PathFinder.Calculatepath(Startindex, DestIndex, partition));

            LBPathFinder.Items.Clear();

            if (DestinationAtStarting)
            {
                foreach (var i in Utility.SeperateStringByMeasurement(LBPathFinder, "Your destination is available from your starting area.", "")) { LBPathFinder.Items.Add(i); }
                return;
            }

            if (PathFinder.paths.Count == 0)
            {
                LBPathFinder.Items.Add("No Path Found!");
                LBPathFinder.Items.Add("");

                foreach (var i in Utility.SeperateStringByMeasurement(LBPathFinder, "This path finder is still in beta and may not always work as intended.", "")) { LBPathFinder.Items.Add(i); }
                LBPathFinder.Items.Add("");
                if (!LogicObjects.MainTrackerInstance.Options.UseSongOfTime)
                {
                    var sotT = "Your destination may not be reachable without song of time. The use of Song of Time is not considered by default. To enable Song of Time toggle it in the options menu";
                    foreach (var i in Utility.SeperateStringByMeasurement(LBPathFinder, sotT, "")) { LBPathFinder.Items.Add(i); }
                }
                LBPathFinder.Items.Add("");
                var ErrT = "If you believe this is an error try navigating to a different entrance close to your destination or try a different starting point.";
                foreach (var i in Utility.SeperateStringByMeasurement(LBPathFinder, ErrT, "")) { LBPathFinder.Items.Add(i); }

                return;
            }
            PrintPaths(-1, partition);
        }
        #endregion Buttons
        //Other---------------------------------------------------------------------------
        #region Other
        private void CHKShowAll_CheckedChanged(object sender, EventArgs e) { PrintToListBox(); }

        private void CMBStart_DropDown(object sender, EventArgs e) { PrintToComboBox(true); AdjustCMBWidth(sender); }

        private void CMBEnd_DropDown(object sender, EventArgs e) { PrintToComboBox(false); AdjustCMBWidth(sender); }
        #endregion Other
        #endregion Form Objects
        #region Functions
        //Context Menu Functions---------------------------------------------------------------------------
        #region Context Menu
        private void CreateMenu()
        {
            //LBValidLocations List Box
            ContextMenuStrip LocationContextMenu = new ContextMenuStrip();
            LocationContextMenu.Opening += LocationContextMenu_Opening;
            ToolStripItem WhatUnlcoked = LocationContextMenu.Items.Add("What Unlocked This?");
            WhatUnlcoked.Click += (sender, e) => { RunMenuItems(0, LBValidLocations); };
            if (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled && LogicObjects.MainTrackerInstance.Options.IncludeItemLocations)
            {
                ToolStripItem NavigateHere = LocationContextMenu.Items.Add("Navigate to this check");
                NavigateHere.Click += (sender, e) => { RunMenuItems(5, LBValidLocations); };
            }
            ToolStripItem Filter = LocationContextMenu.Items.Add("Filter at this Location");
            Filter.Click += (sender, e) => { RunMenuItems(3, LBValidLocations); };
            ToolStripItem GroupFilter = LocationContextMenu.Items.Add("Filter at Locations near this area");
            GroupFilter.Click += (sender, e) => { RunMenuItems(4, LBValidLocations); };
            ToolStripItem Check = LocationContextMenu.Items.Add("Check This Item");
            Check.Click += (sender, e) => { RunMenuItems(1, LBValidLocations); };
            ToolStripItem Mark = LocationContextMenu.Items.Add("Mark This Item");
            Mark.Click += (sender, e) => { RunMenuItems(2, LBValidLocations); };
            LBValidLocations.ContextMenuStrip = LocationContextMenu;

            //LBValidEntrances List Box
            ContextMenuStrip EntranceContextMenu = new ContextMenuStrip();
            EntranceContextMenu.Opening += EntranceContextMenu_Opening;
            ToolStripItem EWhatUnlcoked = EntranceContextMenu.Items.Add("What Unlocked This?");
            EWhatUnlcoked.Click += (sender, e) => { RunMenuItems(0, LBValidEntrances); };
            if (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled)
            {
                ToolStripItem ENavigateHere = EntranceContextMenu.Items.Add("Navigate to this entrance");
                ENavigateHere.Click += (sender, e) => { RunMenuItems(5, LBValidEntrances); };
            }
            ToolStripItem EFilter = EntranceContextMenu.Items.Add("Filter at this Location");
            EFilter.Click += (sender, e) => { RunMenuItems(3, LBValidEntrances); };
            ToolStripItem EGroupFilter = EntranceContextMenu.Items.Add("Filter at Locations near this area");
            EGroupFilter.Click += (sender, e) => { RunMenuItems(4, LBValidEntrances); };
            ToolStripItem ECheck = EntranceContextMenu.Items.Add("Check This Item");
            ECheck.Click += (sender, e) => { RunMenuItems(1, LBValidEntrances); };
            ToolStripItem EMark = EntranceContextMenu.Items.Add("Mark This Item");
            EMark.Click += (sender, e) => { RunMenuItems(2, LBValidEntrances); };
            LBValidEntrances.ContextMenuStrip = EntranceContextMenu;

            //Set Item Button
            ContextMenuStrip SetItemMenu = new ContextMenuStrip();
            ToolStripItem ItemSetAll = SetItemMenu.Items.Add("Set Only");
            ItemSetAll.Click += (sender, e) => { CheckItemSelected(LBValidLocations, false, 1); };
            ToolStripItem ItemUnSetAll = SetItemMenu.Items.Add("Unset Only");
            ItemUnSetAll.Click += (sender, e) => { CheckItemSelected(LBValidLocations, false, 2); };
            BTNSetItem.ContextMenuStrip = SetItemMenu;

            //Set Entrance Button
            ContextMenuStrip SetLocationMenu = new ContextMenuStrip();
            ToolStripItem EntranceSetAll = SetLocationMenu.Items.Add("Set Only");
            EntranceSetAll.Click += (sender, e) => { CheckItemSelected(LBValidEntrances, false, 1); };
            ToolStripItem EntranceUnSetAll = SetLocationMenu.Items.Add("Unset Only");
            EntranceUnSetAll.Click += (sender, e) => { CheckItemSelected(LBValidEntrances, false, 2); };
            BTNSetEntrance.ContextMenuStrip = SetLocationMenu;
        }

        private void EntranceContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var index = LBValidEntrances.IndexFromPoint(LBValidEntrances.PointToClient(Cursor.Position));
            if (index < 0) { e.Cancel = true; return; }
            if (!(LBValidEntrances.Items[index] is LogicObjects.LogicEntry)) { e.Cancel = true; }
        }

        private void LocationContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var index = LBValidLocations.IndexFromPoint(LBValidLocations.PointToClient(Cursor.Position));
            if (index < 0) { e.Cancel = true; return; }
            if (!(LBValidLocations.Items[index] is LogicObjects.LogicEntry)) { e.Cancel = true; }
        }

        private void RunMenuItems(int Function, ListBox ActiveListBox)
        {
            LogicObjects.LogicEntry ActiveItem;
            if ((ActiveListBox == LBValidLocations) && LBValidLocations.SelectedItem is LogicObjects.LogicEntry)
            { ActiveItem = LBValidLocations.SelectedItem as LogicObjects.LogicEntry; }
            else if ((ActiveListBox == LBValidEntrances) && LBValidEntrances.SelectedItem is LogicObjects.LogicEntry)
            { ActiveItem = LBValidEntrances.SelectedItem as LogicObjects.LogicEntry; }
            else { return; }

            if (Function == 0) { Tools.CurrentSelectedItem = ActiveItem; Tools.WhatUnlockedThis(); }
            if (Function == 1) { CheckItemSelected(ActiveListBox, true); }
            if (Function == 2) { CheckItemSelected(ActiveListBox, false); }
            if (Function == 3) { ((ActiveListBox == LBValidLocations) ? TXTLocSearch : TXTEntSearch).Text = "=#" + ActiveItem.LocationArea; }
            if (Function == 4)
            {
                TextBox SearchBox = (ActiveListBox == LBValidLocations) ? TXTLocSearch : TXTEntSearch;
                List<Map.LocationArea> LocationDic = new List<Map.LocationArea>();
                Map MapUtils = new Map();
                MapUtils.setLocationDic(LocationDic);
                foreach (var i in LocationDic)
                {
                    foreach (var j in i.SubAreas)
                    {
                        if (ActiveItem.LocationArea.ToLower() == j.Replace("#", "").ToLower())
                        {
                            SearchBox.Text = MapUtils.CreateFilter(0, i.SubAreas);
                            return;
                        }
                    }
                }
            }
            if (Function == 5)
            {
                CMBEnd.DataSource = new BindingSource(new Dictionary<int, string> { { ActiveItem.ID, ActiveItem.LocationName } }, null);
                CMBEnd.DisplayMember = "Value";
                CMBEnd.ValueMember = "key";
                CMBEnd.SelectedIndex = 0;
            }
        }
        #endregion Context Menu
        // List/combo Box Functions---------------------------------------------------------------------------
        #region List/combo Box
        public void PrintToListBox()
        {
            var lbLocTop = LBValidLocations.TopIndex;
            var lbEntTop = LBValidEntrances.TopIndex;
            var lbCheckTop = LBCheckedLocations.TopIndex;
            int TotalLoc = 0;
            int TotalEnt = 0;
            int totalchk = 0;
            List<LogicObjects.LogicEntry> logic = LogicObjects.MainTrackerInstance.Logic;
            List<LogicObjects.LogicEntry> ListBoxItems = new List<LogicObjects.LogicEntry>();
            Dictionary<string, int> Groups = new Dictionary<string, int>();
            Dictionary<int, int> ListBoxAssignments = new Dictionary<int, int>();

            if (File.Exists(@"Recources\Other Files\Categories.txt"))
            {
                Groups = File.ReadAllLines(@"Recources\Other Files\Categories.txt")
                    .Select(x => x.ToLower().Trim()).Distinct()
                    .Select((value, index) => new { value, index })
                    .ToDictionary(pair => pair.value, pair => pair.index);
            }

            foreach (var entry in LogicObjects.MainTrackerInstance.Logic)
            {
                if (!entry.AppearsInListbox()) { continue; }

                entry.DisplayName = entry.DictionaryName;
                if ((entry.Available || entry.HasRandomItem(false) || CHKShowAll.Checked) && (entry.LocationName != "" && entry.LocationName != null) && !entry.Checked)
                {
                    entry.DisplayName = entry.HasRandomItem(false) ? ($"{entry.LocationName}: {entry.RandomizedEntry(LogicObjects.MainTrackerInstance, true).ItemName}") : entry.LocationName;

                    if ((!entry.IsEntrance() || !LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled))
                    {
                        TotalLoc += 1;
                        if (Utility.FilterSearch(entry, TXTLocSearch.Text, entry.DisplayName, entry.RandomizedEntry(LogicObjects.MainTrackerInstance, true)))
                        {
                            ListBoxAssignments.Add(entry.ID, 0);
                            ListBoxItems.Add(entry);
                        }
                    }
                    else if ((entry.IsEntrance() && LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled))
                    {
                        TotalEnt += 1;
                        if (Utility.FilterSearch(entry, TXTEntSearch.Text, entry.DisplayName, entry.RandomizedEntry(LogicObjects.MainTrackerInstance, true)))
                        {
                            ListBoxAssignments.Add(entry.ID, 1);
                            ListBoxItems.Add(entry);
                        }
                    }
                }
                if (entry.Checked)
                {
                    entry.DisplayName = entry.HasRandomItem(false) ? $"{entry.RandomizedEntry(LogicObjects.MainTrackerInstance, true).ItemName}: {entry.LocationName}" : $"Nothing: {entry.LocationName}";
                    totalchk += 1;
                    if (Utility.FilterSearch(entry, TXTCheckedSearch.Text, entry.DisplayName, entry.RandomizedEntry(LogicObjects.MainTrackerInstance, true)))
                    {
                        ListBoxAssignments.Add(entry.ID, 2);
                        ListBoxItems.Add(entry);
                    }
                }
            }

            ListBoxItems = ListBoxItems
                .OrderBy(x => Utility.BoolToInt(x.IsEntrance()))
                .ThenBy(x => Utility.BoolToInt(x.HasRandomItem(false) && LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom))
                .ThenBy(x => (Groups.ContainsKey(x.LocationArea.ToLower().Trim()) ? Groups[x.LocationArea.ToLower().Trim()] : ListBoxItems.Count() + 1))
                .ThenBy(x => x.LocationArea)
                .ThenBy(x => x.DisplayName).ToList();

            var lastLocArea = "";
            var lastEntArea = "";
            var lastChkArea = "";

            int AvalableLocations = 0;
            int AvalableEntrances = 0;
            int CheckedLocations = 0;

            LBValidLocations.Items.Clear();
            LBValidEntrances.Items.Clear();
            LBCheckedLocations.Items.Clear();

            foreach (var entry in ListBoxItems)
            {
                if (!ListBoxAssignments.ContainsKey(entry.ID)) { continue; }
                if (ListBoxAssignments[entry.ID] == 0) { lastLocArea = WriteObject(entry, LBValidLocations, lastLocArea, entry.HasRandomItem(false)); AvalableLocations++; }
                if (ListBoxAssignments[entry.ID] == 1) { lastEntArea = WriteObject(entry, LBValidEntrances, lastEntArea, entry.HasRandomItem(false)); AvalableEntrances++; }
                if (ListBoxAssignments[entry.ID] == 2) { lastChkArea = WriteObject(entry, LBCheckedLocations, lastChkArea, false); CheckedLocations++; }
            }

            label1.Text = "Available Locations: " + ((AvalableLocations == TotalLoc) ? AvalableLocations.ToString() : (AvalableLocations.ToString() + "/" + TotalLoc.ToString()));
            label2.Text = "Checked locations: " + ((CheckedLocations == totalchk) ? CheckedLocations.ToString() : (CheckedLocations.ToString() + "/" + totalchk.ToString())); ;
            label3.Text = "Available Entrances: " + ((AvalableEntrances == TotalEnt) ? AvalableEntrances.ToString() : (AvalableEntrances.ToString() + "/" + TotalEnt.ToString())); ;
            LBValidLocations.TopIndex = lbLocTop;
            LBValidEntrances.TopIndex = lbEntTop;
            LBCheckedLocations.TopIndex = lbCheckTop;
        }

        private string WriteObject(LogicObjects.LogicEntry entry, ListBox lb, string lastArea, bool Marked)
        {
            bool ShowMarked = (Marked && LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom);
            string returnLastArea = lastArea;
            if (entry.LocationArea != returnLastArea)
            {
                if (returnLastArea != "") { lb.Items.Add(Utility.CreateDivider(lb)); }

                string Header = entry.LocationArea.ToUpper();
                if (ShowMarked && entry.IsEntrance()) { Header += " SET EXITS"; }
                else if (ShowMarked && !entry.IsEntrance()) { Header += " SET ITEMS"; }
                else if (entry.IsEntrance()) { Header += " ENTRANCES"; }
                lb.Items.Add(Header + ":");
                returnLastArea = entry.LocationArea;
            }
            lb.Items.Add(entry);
            return (returnLastArea);
        }

        private void PrintPaths(int PathToPrint, int partition)
        {
            LBPathFinder.Items.Clear();
            var sortedpaths = PathFinder.paths[partition].OrderBy(x => x.Count);

            if (PathToPrint == -1 || PathFinder.paths[partition].ElementAtOrDefault(PathToPrint) == null)
            {
                int counter = 1;
                foreach (var path in sortedpaths)
                {
                    var ListTitle = new LogicObjects.ListItem { DisplayName = "Path: " + counter + " (" + path.Count + " Steps)", ID = counter - 1, Identifier = partition };
                    LBPathFinder.Items.Add(ListTitle);
                    var firstStop = true;
                    foreach (var stop in path)
                    {
                        var start = (firstStop) ? PathFinder.SetSOTName(LogicObjects.MainTrackerInstance, stop) : LogicObjects.MainTrackerInstance.Logic[stop.EntranceToTake].LocationName;
                        var ListItem = new LogicObjects.ListItem
                        {
                            DisplayName = start + " => " + LogicObjects.MainTrackerInstance.Logic[stop.ResultingExit].ItemName,
                            ID = counter - 1,
                            Identifier = partition
                        };
                        LBPathFinder.Items.Add(ListItem); firstStop = false;
                    }
                    LBPathFinder.Items.Add(new LogicObjects.ListItem
                    {
                        DisplayName = "===============================",
                        ID = counter - 1,
                        Identifier = partition
                    });
                    counter++;
                }
            }
            else
            {
                var path = sortedpaths.ToArray()[PathToPrint];
                var ListTitle = new LogicObjects.ListItem { DisplayName = "Path: " + (PathToPrint + 1) + " (" + path.Count + " Steps)", ID = -1, Identifier = partition };
                LBPathFinder.Items.Add(ListTitle);
                var firstStop = true;
                foreach (var stop in path)
                {
                    var start = (firstStop) ? PathFinder.SetSOTName(LogicObjects.MainTrackerInstance, stop) : LogicObjects.MainTrackerInstance.Logic[stop.EntranceToTake].LocationName;
                    var ListItem = new LogicObjects.ListItem
                    {
                        DisplayName = start + " => " + LogicObjects.MainTrackerInstance.Logic[stop.ResultingExit].ItemName,
                        ID = -1,
                        Identifier = partition
                    };
                    LBPathFinder.Items.Add(ListItem);
                    firstStop = false;
                }
                LBPathFinder.Items.Add(new LogicObjects.ListItem
                {
                    DisplayName = "===============================",
                    ID = -1,
                    Identifier = partition
                });
            }
        }

        public void ShowtoolTip(MouseEventArgs e, ListBox lb)
        {
            if (!LogicObjects.MainTrackerInstance.Options.ShowEntryNameTooltip) { return; }
            int index = lb.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            if (!(lb.Items[index] is LogicObjects.LogicEntry || lb.Items[index] is LogicObjects.ListItem)) { return; }
            string DisplayName = lb.Items[index].ToString();
            if (toolTip1.GetToolTip(lb) == DisplayName) { return; }
            if (Utility.IsDivider(DisplayName)) { return; }
            toolTip1.SetToolTip(lb, DisplayName);
        }

        public void PrintToComboBox(bool start)
        {
            var UnsortedPathfinder = new Dictionary<int, string>();
            var UnsortedItemPathfinder = new Dictionary<int, string>();
            foreach (var entry in LogicObjects.MainTrackerInstance.Logic)
            {
                if (entry.IsEntrance())
                {
                    if ((start) ? entry.Aquired : entry.Available) { UnsortedPathfinder.Add(entry.ID, (start) ? entry.ItemName : entry.LocationName); }
                }
                if (LogicObjects.MainTrackerInstance.Options.IncludeItemLocations && entry.ItemSubType != "Entrance" && !entry.IsFake && entry.Available && !start)
                {
                    UnsortedItemPathfinder.Add(entry.ID, (entry.RandomizedItem > -1) ? entry.LocationName + ": " + LogicObjects.MainTrackerInstance.Logic[entry.RandomizedItem].ItemName : entry.LocationName);
                }
            }
            Dictionary<int, string> sortedPathfinder = UnsortedPathfinder.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

            if (LogicObjects.MainTrackerInstance.Options.IncludeItemLocations && !start)
            {
                Dictionary<int, string> ItemPathFinder = new Dictionary<int, string>();
                Dictionary<int, string> sortedItemPathfinder = UnsortedItemPathfinder.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                ItemPathFinder.Add(-2, "ENTRANCES =============================");
                foreach (KeyValuePair<int, string> p in sortedPathfinder)
                {
                    ItemPathFinder.Add(p.Key, p.Value);
                }
                ItemPathFinder.Add(-1, "ITEM LOCATIONS =============================");
                foreach (KeyValuePair<int, string> p in sortedItemPathfinder)
                {
                    ItemPathFinder.Add(p.Key, p.Value);
                }
                sortedPathfinder = ItemPathFinder;
            }

            ComboBox cmb = (start) ? CMBStart : CMBEnd;
            cmb.DataSource = new BindingSource(sortedPathfinder, null);
            cmb.DisplayMember = "Value";
            cmb.ValueMember = "key";
        }
        #endregion List/combo Box
        //Other Functions---------------------------------------------------------------------------
        #region Other Functions
        public void AdjustCMBWidth(object sender)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            int width = senderComboBox.DropDownWidth;
            Graphics g = senderComboBox.CreateGraphics();
            Font font = senderComboBox.Font;
            int vertScrollBarWidth = (senderComboBox.Items.Count > senderComboBox.MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;

            int newWidth;

            foreach (KeyValuePair<int, string> dictionary in senderComboBox.Items)
            {
                newWidth = (int)g.MeasureString(dictionary.Value, font).Width + vertScrollBarWidth;
                if (width < newWidth) { width = newWidth; }
            }
            senderComboBox.DropDownWidth = width;
        }

        public void CheckItemSelected(ListBox LB, bool FullCheck, int SetFunction = 0)
        {
            //Set Function: 0 = none, 1 = Always Set, 2 = Always Unset
            if (TXTLocSearch.Text.ToLower() == "enabledev" && LB == LBValidLocations && !FullCheck)
            {
                Debugging.ISDebugging = !Debugging.ISDebugging;
                FormatMenuItems();
                TXTLocSearch.Clear();
                return;
            }
            var Templogic = Utility.CloneLogicList(LogicObjects.MainTrackerInstance.Logic); //We want to save logic at this point but don't want to comit to a full save state
            bool ChangesMade = false;
            foreach (var i in LB.SelectedItems)
            {
                if ((i is LogicObjects.LogicEntry))
                {
                    if (FullCheck) 
                    { 
                        if ((LB == LBValidLocations || LB == LBValidEntrances) && (i as LogicObjects.LogicEntry).Checked) { continue; }
                        if (LB == LBCheckedLocations && !(i as LogicObjects.LogicEntry).Checked) { continue; }
                        if (!LogicEditing.CheckObject(i as LogicObjects.LogicEntry, LogicObjects.MainTrackerInstance)) { continue; }
                        ChangesMade = true;
                    }
                    else 
                    { 
                        if (SetFunction != 0)
                        {
                            bool set = (SetFunction == 1) ? true : false;
                            if ((i as LogicObjects.LogicEntry).RandomizedItem > -1 && set) { continue; }
                            if ((i as LogicObjects.LogicEntry).RandomizedItem < 0 && !set) { continue; }
                        }

                        if (!LogicEditing.MarkObject(i as LogicObjects.LogicEntry)) { continue; }
                        ChangesMade = true;
                    }
                }
            }
            if (!ChangesMade) { return; }
            Tools.SaveState(LogicObjects.MainTrackerInstance, Templogic); //Now that we have successfully checked/Marked an object we can commit to a full save state
            LogicObjects.MainTrackerInstance.UnsavedChanges = true;
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);

            FireEvents(false);

            int TopIndex = LB.TopIndex;
            PrintToListBox();
            LB.TopIndex = TopIndex;
        }

        public void FormatMenuItems()
        {
            importSpoilerLogToolStripMenuItem.Text = (Utility.CheckforSpoilerLog(LogicObjects.MainTrackerInstance.Logic)) ? "Remove Spoiler Log" : "Import Spoiler Log";
            useSongOfTimeInPathfinderToolStripMenuItem.Text = (LogicObjects.MainTrackerInstance.Options.UseSongOfTime) ? "Disable Song of Time in pathfinder" : "Enable Song of Time in pathfinder";
            stricterLogicHandelingToolStripMenuItem.Text = (LogicObjects.MainTrackerInstance.Options.StrictLogicHandeling) ? "Disable Stricter Logic Handeling" : "Enable Stricter Logic Handeling";
            showEntryNameToolTipToolStripMenuItem.Text = (LogicObjects.MainTrackerInstance.Options.ShowEntryNameTooltip) ? "Disable Entry Name ToolTip" : "Show Entry Name ToolTip";
            includeItemLocationsAsDestinationToolStripMenuItem.Text = (LogicObjects.MainTrackerInstance.Options.IncludeItemLocations) ? "Exclude Item Locations As Destinations" : "Include Item Locations As Destinations";
            entranceRandoToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.EntranceRando;
            optionsToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            undoToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            redoToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            saveToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            seedCheckerToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            generatePlaythroughToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            whatUnlockedThisToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            updateLogicToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            popoutPathfinderToolStripMenuItem.Visible = (LogicObjects.MainTrackerInstance.EntranceRando);
            if (!LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable) 
            {
                LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled = LogicObjects.MainTrackerInstance.EntranceRando; 
            }
            useSongOfTimeInPathfinderToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            includeItemLocationsAsDestinationToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            coupleEntrancesToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            toggleEntranceRandoFeaturesToolStripMenuItem.Text = (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled) ? "Disable Entrance Rando Features" : "Enable Entrance Rando Features";
            coupleEntrancesToolStripMenuItem.Text = (LogicObjects.MainTrackerInstance.Options.CoupleEntrances) ? "Uncouple Entrances" : "Couple Entrances";
            devToolStripMenuItem.Visible = Debugging.ISDebugging;
            seperateMarkedItemsToolStripMenuItem.Text = (LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom) ? "Don't Seperate Marked Items" : "Seperate Marked Items";

            CreateMenu();

            //MM specific Controls
            importSpoilerLogToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.IsMM();
            useSongOfTimeInPathfinderToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.IsMM() && LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            includeItemLocationsAsDestinationToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.IsMM() && LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            coupleEntrancesToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.IsMM() && LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled;
            generatePlaythroughToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.IsMM() && (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            seedCheckerToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.IsMM() && (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            whatUnlockedThisToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.IsMM() && (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            FilterMapToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.IsMM() && (LogicObjects.MainTrackerInstance.LogicVersion > 0);
            itemTrackerToolStripMenuItem.Visible = LogicObjects.MainTrackerInstance.IsMM() && (LogicObjects.MainTrackerInstance.LogicVersion > 0);

        }

        private void ResizeObject()
        {
            var UpperLeftLBL = label1;
            var UpperRightLBL = label3;
            var LowerLeftLBL = label2;
            var LowerRightLBL = label4;
            var LowerRight2LBL = label5;
            var LowerRight3LBL = label6;
            var Menuhieght = menuStrip1.Height;
            var FormHeight = this.Height - 40 - Menuhieght;
            var FormWidth = this.Width - 18;
            var FormHalfHeight = FormHeight / 2;
            var FormHalfWidth = FormWidth / 2;

            var locX = 2;
            var locY = 2 + Menuhieght;

            if (LogicObjects.MainTrackerInstance.LogicVersion == 0)
            {
                SetObjectVisibility(false, false);
            }
            else if (LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled)
            {
                SetObjectVisibility(true, true);
                UpperLeftLBL.Location = new Point(locX, locY + 2);
                BTNSetItem.Location = new Point(FormHalfWidth - BTNSetItem.Width, Menuhieght + 1);
                TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                TXTLocSearch.Width = FormHalfWidth - 2;
                LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                LBValidLocations.Width = FormHalfWidth - 2;
                LBValidLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                UpperRightLBL.Location = new Point(FormHalfWidth + locX, locY + 2);
                BTNSetEntrance.Location = new Point(FormWidth - BTNSetEntrance.Width, Menuhieght + 1);
                TXTEntSearch.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + 6);
                TXTEntSearch.Width = FormHalfWidth - 2;
                LBValidEntrances.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + TXTEntSearch.Height + 8);
                LBValidEntrances.Width = FormHalfWidth - 2;
                LBValidEntrances.Height = FormHalfHeight - UpperRightLBL.Height - TXTEntSearch.Height - 14;

                LowerLeftLBL.Location = new Point(locX, FormHalfHeight + locY - 2);
                CHKShowAll.Location = new Point(FormHalfWidth - CHKShowAll.Width, Menuhieght + FormHalfHeight - 2);
                TXTCheckedSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 2 + FormHalfHeight);
                TXTCheckedSearch.Width = FormHalfWidth - 2;
                LBCheckedLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTCheckedSearch.Height + 4 + FormHalfHeight);
                LBCheckedLocations.Width = FormHalfWidth - 2;
                LBCheckedLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTCheckedSearch.Height - 8;

                LowerRightLBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY - 2);
                BTNFindPath.Location = new Point(FormWidth - BTNFindPath.Width, Menuhieght + FormHalfHeight - 3);
                LowerRight2LBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY + LowerRightLBL.Height + 6);
                LowerRight3LBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY + LowerRightLBL.Height + 7 + CMBStart.Height);
                CMBStart.Location = new Point(FormHalfWidth + locX + LowerRight3LBL.Width + 2, FormHalfHeight + locY + LowerRightLBL.Height + 2);
                CMBEnd.Location = new Point(FormHalfWidth + locX + LowerRight3LBL.Width + 2, FormHalfHeight + locY + LowerRightLBL.Height + CMBStart.Height + 5);
                CMBStart.Width = FormHalfWidth - LowerRight3LBL.Width - 4;
                CMBEnd.Width = FormHalfWidth - LowerRight3LBL.Width - 4;
                LBPathFinder.Location = new Point(locX + FormHalfWidth, FormHalfHeight + locY + 8 + LowerRightLBL.Height + CMBStart.Height + CMBEnd.Height);
                LBPathFinder.Width = FormHalfWidth - 2;
                LBPathFinder.Height = LBCheckedLocations.Height - CMBEnd.Height - 4;
            }
            else
            {
                SetObjectVisibility(true, false);
                UpperLeftLBL.Location = new Point(locX, locY + 2);
                BTNSetItem.Location = new Point(FormWidth - BTNSetItem.Width, Menuhieght + 1);
                TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                TXTLocSearch.Width = FormWidth - 2;
                LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                LBValidLocations.Width = FormWidth - 2;
                LBValidLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                LowerLeftLBL.Location = new Point(locX, FormHalfHeight + locY - 2);
                CHKShowAll.Location = new Point(FormWidth - CHKShowAll.Width, Menuhieght + FormHalfHeight - 2);
                TXTCheckedSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 2 + FormHalfHeight);
                TXTCheckedSearch.Width = FormWidth - 2;
                LBCheckedLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTCheckedSearch.Height + 4 + FormHalfHeight);
                LBCheckedLocations.Width = FormWidth - 2;
                LBCheckedLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTCheckedSearch.Height - 8;
            }
            PrintToListBox();
            this.Refresh();

        }

        public void SetObjectVisibility(bool item, bool location)
        {
            var UpperLeftLBL = label1;
            var UpperRightLBL = label3;
            var LowerLeftLBL = label2;
            var LowerRightLBL = label4;
            var LowerRight2LBL = label5;
            var LowerRight3LBL = label6;

            UpperLeftLBL.Visible = item;
            BTNSetItem.Visible = item;
            TXTLocSearch.Visible = item;
            LBValidLocations.Visible = item;
            LowerLeftLBL.Visible = item;
            CHKShowAll.Visible = item;
            TXTCheckedSearch.Visible = item;
            LBCheckedLocations.Visible = item;

            UpperRightLBL.Visible = location;
            BTNSetEntrance.Visible = location;
            TXTEntSearch.Visible = location;
            LBValidEntrances.Visible = location;
            LowerRightLBL.Visible = location;
            BTNFindPath.Visible = location;
            LowerRight2LBL.Visible = location;
            LowerRight3LBL.Visible = location;
            CMBStart.Visible = location;
            CMBEnd.Visible = location;
            LBPathFinder.Visible = location;
        }

        private static void FireEvents(bool TrackerUpdated = true, bool LocationCheck = true)
        {
            if (LocationCheck) { LocationChecked(null, null); }
            if (TrackerUpdated) { TrackerUpdate(null, null); }
        }
        #endregion Other Functions

        #endregion Functions
    }
}
