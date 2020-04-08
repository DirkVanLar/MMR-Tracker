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
using Octokit;

namespace MMR_Tracker_V2
{
    public partial class FRMTracker : Form
    {
        public FRMTracker()
        {
            InitializeComponent();
        }

        //Object Events
        //Form Events---------------------------------------------------------------------------
        private void FRMTracker_Load(object sender, EventArgs e)
        {
            Debugging.ISDebugging = (Control.ModifierKeys == Keys.Control) ? (!Debugger.IsAttached) : (Debugger.IsAttached);
            Utility.CheckforOptionsFile();
            if (VersionHandeling.GetLatestTrackerVersion()) { this.Close(); }
            ResizeObject();
            FormatMenuItems();
        }

        private void FRMTracker_ResizeEnd(object sender, EventArgs e) { ResizeObject(); }

        private void FRMTracker_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !Utility.PromptSave();
        }

        //Menu Strip---------------------------------------------------------------------------

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.Redo();
            PrintToListBox();
            FormatMenuItems();
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.Undo();
            PrintToListBox();
            FormatMenuItems();
        }

        //Menu Strip => File---------------------------------------------------------------------------

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.SaveInstance();
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Utility.PromptSave()) { return; }
            string file = Utility.FileSelect("Select A Save File", "MMR Tracker Save (*.MMRTSAV)|*.MMRTSAV");
            if (file == "") { return; }
            Utility.ResetInstance();
            Utility.LoadInstance(file);
            LogicEditing.CreateDicNameToID(LogicObjects.DicNameToID, LogicObjects.Logic);
            LogicEditing.CreatedEntrancepairDcitionary(LogicObjects.EntrancePairs, LogicObjects.DicNameToID);
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Utility.PromptSave()) { return; }
            string file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSET)|*.txt;*.MMRTSET");
            if (file == "") { return; }

            bool SettingsFile = file.EndsWith(".MMRTSET");
            var lines = (SettingsFile) ? File.ReadAllLines(file).Skip(2) : File.ReadAllLines(file);

            CreateTrackerInstance(lines.ToArray());

            if (SettingsFile)
            {
                RandomizeOptions.UpdateRandomOptionsFromFile(File.ReadAllLines(file));
                VersionHandeling.entranceRadnoEnabled = Utility.CheckForRandomEntrances(LogicObjects.Logic);
                VersionHandeling.OverRideAutoEntranceRandoEnable = (VersionHandeling.entranceRadnoEnabled != VersionHandeling.IsEntranceRando());
            }

            if (VersionHandeling.IsEntranceRando() && !SettingsFile && Utility.UnradnomizeEntranesOnStartup)
            {
                foreach (var item in LogicObjects.Logic)
                {
                    if (item.IsEntrance()) { item.Options = 1; }
                    VersionHandeling.entranceRadnoEnabled = false;
                    VersionHandeling.OverRideAutoEntranceRandoEnable = true;
                }
            }
            LogicEditing.CalculateItems(LogicObjects.Logic);
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }

        //Menu Strip => File => New---------------------------------------------------------------------------

        private void CasualLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Utility.PromptSave()) { return; }
            System.Net.WebClient wc = new System.Net.WebClient();
            string webData = wc.DownloadString("https://raw.githubusercontent.com/ZoeyZolotova/mm-rando/dev/MMR.Randomizer/Resources/REQ_CASUAL.txt");
            string[] Lines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            CreateTrackerInstance(Lines);
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }

        private void GlitchedLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Utility.PromptSave()) { return; }
            System.Net.WebClient wc = new System.Net.WebClient();
            string webData = wc.DownloadString("https://raw.githubusercontent.com/ZoeyZolotova/mm-rando/dev/MMR.Randomizer/Resources/REQ_GLITCH.txt");
            string[] Lines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            CreateTrackerInstance(Lines);
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }

        //Menu Strip => Options---------------------------------------------------------------------------

        //Menu Strip => Options => Logic Options---------------------------------------------------------------------------

        private void EditRadnomizationOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Utility.PromptSave()) { return; }
            bool EntrancesRandoBefore = Utility.CheckForRandomEntrances(LogicObjects.Logic);
            RandomizeOptions RandoOptionScreen = new RandomizeOptions();
            RandoOptionScreen.ShowDialog();
            bool EntrancesRandoAfter = Utility.CheckForRandomEntrances(LogicObjects.Logic);

            if (!VersionHandeling.OverRideAutoEntranceRandoEnable || (EntrancesRandoBefore != EntrancesRandoAfter))
            {
                VersionHandeling.entranceRadnoEnabled = Utility.CheckForRandomEntrances(LogicObjects.Logic);
                VersionHandeling.OverRideAutoEntranceRandoEnable = (VersionHandeling.entranceRadnoEnabled != VersionHandeling.IsEntranceRando());
            }

            LogicEditing.CalculateItems(LogicObjects.Logic);
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }

        private void ImportSpoilerLogToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (Utility.CheckforSpoilerLog(LogicObjects.Logic))
            {
                foreach (var entry in LogicObjects.Logic) { entry.SpoilerRandom = -2; }
            }
            else
            {
                string file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt;*html)|*.txt;*html");
                if (file == "") { return; }
                LogicEditing.WriteSpoilerLogToLogic(LogicObjects.Logic, file);
                if (!Utility.CheckforSpoilerLog(LogicObjects.Logic)) { MessageBox.Show("No spoiler data found!"); return; }
                if (!Utility.CheckforSpoilerLog(LogicObjects.Logic, true)) { MessageBox.Show("Not all checks have been assigned spoiler data!"); }
                bool EntrancesRandoBefore = Utility.CheckForRandomEntrances(LogicObjects.Logic);
                foreach (var i in LogicObjects.Logic)
                {
                    if (i.SpoilerRandom != i.ID && i.SpoilerRandom > -1 && (i.Options == 1 || i.Options == 2)) { i.Options = 0; }
                }
                bool EntrancesRandoAfter = Utility.CheckForRandomEntrances(LogicObjects.Logic);
                if (!VersionHandeling.OverRideAutoEntranceRandoEnable || (EntrancesRandoBefore != EntrancesRandoAfter))
                {
                    VersionHandeling.entranceRadnoEnabled = Utility.CheckForRandomEntrances(LogicObjects.Logic);
                    VersionHandeling.OverRideAutoEntranceRandoEnable = (VersionHandeling.entranceRadnoEnabled != VersionHandeling.IsEntranceRando());
                }
                LogicEditing.CalculateItems(LogicObjects.Logic);
            }
            FormatMenuItems();
            ResizeObject();
            PrintToListBox();
        }

        private void StricterLogicHandelingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicEditing.StrictLogicHandeling = !LogicEditing.StrictLogicHandeling;
            FormatMenuItems();
        }

        //Menu Strip => Options => Entrance Rando---------------------------------------------------------------------------

        private void UseSongOfTimeInPathfinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pathfinding.UseSongOfTime = !Pathfinding.UseSongOfTime;
            FormatMenuItems();
        }

        private void ToggleEntranceRandoFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VersionHandeling.entranceRadnoEnabled = !VersionHandeling.entranceRadnoEnabled;
            VersionHandeling.OverRideAutoEntranceRandoEnable = (VersionHandeling.entranceRadnoEnabled != VersionHandeling.IsEntranceRando());
            ResizeObject();
            PrintToListBox();
            FormatMenuItems();
        }

        private void IncludeItemLocationsAsDestinationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pathfinding.IncludeItemLocations = !Pathfinding.IncludeItemLocations;
            FormatMenuItems();
        }

        private void CoupleEntrancesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!LogicEditing.CoupleEntrances) { if (!Utility.PromptSave()) { return; } }
            LogicEditing.CoupleEntrances = !LogicEditing.CoupleEntrances;
            if (!LogicEditing.CoupleEntrances) { MessageBox.Show("Entrances will not uncouple automatically."); }
            if (LogicEditing.CoupleEntrances)
            {
                Utility.UnsavedChanges = true;
                Utility.SaveState(Utility.CloneLogicList(LogicObjects.Logic));
                foreach (var entry in LogicObjects.Logic)
                {
                    if (entry.Checked && entry.RandomizedItem > -1)
                    {
                        LogicEditing.CheckEntrancePair(LogicObjects.Logic[entry.ID], LogicObjects.Logic, true);
                    }
                }
                LogicEditing.CalculateItems(LogicObjects.Logic, true);
                PrintToListBox();
            }
            FormatMenuItems();
        }

        //Menu Strip => Options => Dev---------------------------------------------------------------------------

        private void CreateDictionaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.CreateDictionary();
        }

        private void PrintLogicObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InformationDisplay DebugScreen = new InformationDisplay();
            Debugging.PrintLogicObject(LogicObjects.Logic);
            DebugScreen.DebugFunction = 1;
            DebugScreen.Show();
        }

        private void UpdateDisplayNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Utility.PromptSave()) { return; }
            Utility.UpdateNames(LogicObjects.Logic);
            PrintToListBox();
        }

        private void DumbStuffToolStripMenuItem_Click(object sender, EventArgs e) { Debugging.TestDumbStuff(); }

        private void CreateOOTFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OOT_Support.CreateOOTFiles();
        }

        //Menu Strip => Options => MISC Options---------------------------------------------------------------------------

        private void ShowEntryNameToolTipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowEntryNameTooltip = !Utility.ShowEntryNameTooltip;
            FormatMenuItems();
        }

        //Menu Strip => Tools---------------------------------------------------------------------------

        private void SeedCheckerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SeedChecker SeedCheckerForm = new SeedChecker();
            SeedCheckerForm.ShowDialog();
        }

        private void GeneratePlaythroughToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Debugging.GeneratePlaythrough(LogicObjects.Logic);
        }

        private void WhatUnlockedThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine(this.ActiveControl == LBValidLocations);
            Console.WriteLine(LBValidLocations.SelectedItem is LogicObjects.LogicEntry);

            if ((this.ActiveControl == LBValidLocations) && LBValidLocations.SelectedItem is LogicObjects.LogicEntry)
            {
                LogicObjects.CurrentSelectedItem = LBValidLocations.SelectedItem as LogicObjects.LogicEntry;
            }
            else if ((this.ActiveControl == LBValidEntrances) && LBValidEntrances.SelectedItem is LogicObjects.LogicEntry)
            {
                LogicObjects.CurrentSelectedItem = LBValidEntrances.SelectedItem as LogicObjects.LogicEntry;
            }
            else
            {
                ItemSelect ItemSelectForm = new ItemSelect();
                ItemSelect.Function = 3;
                var dialogResult = ItemSelectForm.ShowDialog();
                if (dialogResult != DialogResult.OK) { LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry(); return; }
            }
            var Requirements = LogicEditing.FindRequirements(LogicObjects.CurrentSelectedItem, LogicObjects.Logic);
            string message = "";
            foreach (var i in Requirements) { message = message + LogicObjects.Logic[i].ItemName + "\n"; }
            MessageBox.Show(message, LogicObjects.CurrentSelectedItem.LocationName +  " Was Unlocked with:");
            LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry();
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
            if (!Utility.PromptSave()) { return; }
            LogicEditing.RecreateLogic();
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }

        //Menu strip => Info

        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InformationDisplay DebugScreen = new InformationDisplay();
            Debugging.PrintLogicObject(LogicObjects.Logic);
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
            var text = (Utility.BomberCode == "") ? "Enter your bombers code below." : "Bomber code: \n" + Utility.BomberCode + "\nEnter a new code to change it.";
            string name = Interaction.InputBox(text, "Bomber Code", "");
            if (name != "") { Utility.BomberCode = name; }
        }

        private void TimedEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var text = (Utility.LotteryNumber == "") ? "Enter your Lottery Number(s) below." : "Lottery Number(s): \n" + Utility.LotteryNumber + "\nEnter Lottery Number(s) to change it.";
            string name = Interaction.InputBox(text, "Lottery Number(s)", "");
            if (name != "") { Utility.LotteryNumber = name; }
        }

        private void OcarinaSongsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form
            {
                BackgroundImage = Bitmap.FromFile(@"Recources\Ocarina Songs.PNG"),
                Width = 500,
                Height = 500,
                BackgroundImageLayout = ImageLayout.Stretch
            };
            form.Show();
        }

        //Text Boxes---------------------------------------------------------------------------
        private void TXTLocSearch_TextChanged(object sender, EventArgs e) { PrintToListBox(); }

        private void TXTEntSearch_TextChanged(object sender, EventArgs e) { PrintToListBox(); }

        private void TXTCheckedSearch_TextChanged(object sender, EventArgs e) { PrintToListBox(); }

        //List Boxes---------------------------------------------------------------------------
        private void LBValidLocations_DoubleClick(object sender, EventArgs e) { CheckItemSelected(LBValidLocations, true); }

        private void LBValidEntrances_DoubleClick(object sender, EventArgs e) { CheckItemSelected(LBValidEntrances, true); }

        private void LBCheckedLocations_DoubleClick(object sender, EventArgs e) { CheckItemSelected(LBCheckedLocations, true); }

        private void LBPathFinder_DoubleClick(object sender, EventArgs e)
        {
            if (LBPathFinder.SelectedItem is LogicObjects.ListItem)
            {
                var item = LBPathFinder.SelectedItem as LogicObjects.ListItem;
                PrintPaths(item.ID);
            }
            else { PrintPaths(-1); }
        }

        private void LBValidLocations_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBValidLocations); }

        private void LBValidEntrances_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBValidEntrances); }

        private void LBCheckedLocations_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBCheckedLocations); }

        private void LBPathFinder_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBPathFinder); }

        //Buttons---------------------------------------------------------------------------
        private void BTNSetItem_Click(object sender, EventArgs e) { CheckItemSelected(LBValidLocations, false); }

        private void BTNSetEntrance_Click(object sender, EventArgs e) { CheckItemSelected(LBValidEntrances, false); }

        private void BTNFindPath_Click(object sender, EventArgs e)
        {

            LBPathFinder.Items.Clear();

            if (!(CMBStart.SelectedItem is KeyValuePair<int, string>) || !(CMBEnd.SelectedItem is KeyValuePair<int, string>)) { return; }
            var Startindex = Int32.Parse(CMBStart.SelectedValue.ToString());
            var DestIndex = Int32.Parse(CMBEnd.SelectedValue.ToString());
            if (Startindex < 0 || DestIndex < 0) { return; }
            var startinglocation = LogicObjects.Logic[Startindex];
            var destination = LogicObjects.Logic[DestIndex];
            LBPathFinder.Items.Add("Finding Path.....");
            LBPathFinder.Items.Add("Please Wait");
            LBPathFinder.Items.Add("");
            LBPathFinder.Items.Add("This could take up to 20");
            LBPathFinder.Items.Add("seconds depending on how");
            LBPathFinder.Items.Add("complete your map is and");
            LBPathFinder.Items.Add("the speed of your PC!");
            LBPathFinder.Refresh();
            Console.WriteLine(Startindex);
            Console.WriteLine(DestIndex);
            var maps = Pathfinding.FindLogicalEntranceConnections(LogicObjects.Logic);
            var Fullmap = maps[0]; //A map of all available exit from each entrance
            var ResultMap = maps[1]; //A map of all available entrances from each exit as long as the result of that entrance is known
            Pathfinding.Findpath(ResultMap, Fullmap, startinglocation.ID, destination.ID, new List<int>(), new List<int>(), new List<LogicObjects.MapPoint>(), true);
            LBPathFinder.Items.Clear();

            if (Pathfinding.paths.Count == 0)
            {
                LBPathFinder.Items.Add("No Path Found!");
                LBPathFinder.Items.Add("");
                LBPathFinder.Items.Add("This path finder is still in beta");
                LBPathFinder.Items.Add("And can be buggy.");
                LBPathFinder.Items.Add("If you believe this is an error");
                LBPathFinder.Items.Add("try navigating to a different entrance");
                LBPathFinder.Items.Add("close to your destination or try a");
                LBPathFinder.Items.Add("different starting point.");
                if (!Pathfinding.UseSongOfTime)
                {
                    LBPathFinder.Items.Add("");
                    LBPathFinder.Items.Add("It may also be the case that the only");
                    LBPathFinder.Items.Add("path to your destination is by use of");
                    LBPathFinder.Items.Add("the Song of Time.");
                    LBPathFinder.Items.Add("By default Song of Time is ignored in");
                    LBPathFinder.Items.Add("the path finder.");
                    LBPathFinder.Items.Add("You can enable the use of Song of");
                    LBPathFinder.Items.Add("Time in entrance rando options.");
                }
                return;
            }
            PrintPaths(-1);
        }

        //Other---------------------------------------------------------------------------
        private void CHKShowAll_CheckedChanged(object sender, EventArgs e) { PrintToListBox(); }

        private void CMBStart_DropDown(object sender, EventArgs e) { PrintToComboBox(true); AdjustCMBWidth(sender); }

        private void CMBEnd_DropDown(object sender, EventArgs e) { PrintToComboBox(false); AdjustCMBWidth(sender); }

        //Functions---------------------------------------------------------------------------
        public void AdjustCMBWidth(object sender)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            int width = senderComboBox.DropDownWidth;
            Graphics g = senderComboBox.CreateGraphics();
            Font font = senderComboBox.Font;
            int vertScrollBarWidth =
                (senderComboBox.Items.Count > senderComboBox.MaxDropDownItems)
                ? SystemInformation.VerticalScrollBarWidth : 0;

            int newWidth;

            foreach (KeyValuePair<int, string> dictionary in senderComboBox.Items)
            {
                newWidth = (int)g.MeasureString(dictionary.Value, font).Width
                    + vertScrollBarWidth;
                if (width < newWidth)
                {
                    width = newWidth;
                }
            }
            senderComboBox.DropDownWidth = width;
        }

        public void CheckItemSelected(ListBox LB, bool FullCheck)
        {
            if (TXTLocSearch.Text.ToLower() == "enabledev" && LB == LBValidLocations && !FullCheck)
            {
                Debugging.ISDebugging = !Debugging.ISDebugging;
                FormatMenuItems();
                TXTLocSearch.Clear();
                return;
            }
            //var selectedIndex = LB.SelectedIndex;
            //We want to save logic at this point but don't want to comit to a full save state
            var TempState = Utility.CloneLogicList(LogicObjects.Logic);
            bool ChangesMade = false;
            foreach (var i in LB.SelectedItems)
            {
                if ((i is LogicObjects.LogicEntry))
                {
                    if (FullCheck) 
                    { 
                        if ((LB == LBValidLocations || LB == LBValidEntrances) && (i as LogicObjects.LogicEntry).Checked) { continue; }
                        if (LB == LBCheckedLocations && !(i as LogicObjects.LogicEntry).Checked) { continue; }
                        if (!LogicEditing.CheckObject(i as LogicObjects.LogicEntry)) { continue; }
                        ChangesMade = true;
                    }
                    else 
                    { 
                        if (!LogicEditing.MarkObject(i as LogicObjects.LogicEntry)) { continue; }
                        ChangesMade = true;
                    }
                }
            }
            if (!ChangesMade) { return; }
            Utility.UnsavedChanges = true;
            Utility.SaveState(TempState); //Now that we have successfully checked/Marked an object we can commit to a full save state
            LogicEditing.CalculateItems(LogicObjects.Logic);

            int TopIndex = LB.TopIndex;
            PrintToListBox();
            LB.TopIndex = TopIndex;
        }

        public void CreateTrackerInstance(string[] Logic)
        {
            Utility.ResetInstance();
            LogicEditing.CreateLogic(LogicObjects.Logic, Logic);
            LogicEditing.CalculateItems(LogicObjects.Logic);

            if (OOT_Support.isOOT)
            {
                DialogResult dialogResult = MessageBox.Show("Support for the Ocarina of Time Randomizer is Limited. Many features will be disabled and core features might not work as intended. Do you wish to continue?", "OOT BETA", MessageBoxButtons.YesNo);
                if (dialogResult != DialogResult.Yes) { Utility.ResetInstance(); }
            }
            else if (!VersionHandeling.ValidVersions.Contains(VersionHandeling.Version))
            {
                DialogResult dialogResult = MessageBox.Show("This version of logic is not supported. Only official releases of versions 1.8 and up are supported. This may result in the tracker not funtioning Correctly. If you are using an official release and are seeing this message, Please update your tracker. Do you wish to continue?", "Unsupported Version", MessageBoxButtons.YesNo);
                if (dialogResult != DialogResult.Yes) { Utility.ResetInstance(); }
            }
            Utility.SaveState(LogicObjects.Logic);
        }

        public void FormatMenuItems()
        {
            importSpoilerLogToolStripMenuItem.Text = (Utility.CheckforSpoilerLog(LogicObjects.Logic)) ? "Remove Spoiler Log" : "Import Spoiler Log";
            useSongOfTimeInPathfinderToolStripMenuItem.Text = (Pathfinding.UseSongOfTime) ? "Disable Song of Time in pathfinder" : "Enable Song of Time in pathfinder";
            stricterLogicHandelingToolStripMenuItem.Text = (LogicEditing.StrictLogicHandeling) ? "Disable Stricter Logic Handeling" : "Enable Stricter Logic Handeling";
            showEntryNameToolTipToolStripMenuItem.Text = (Utility.ShowEntryNameTooltip) ? "Disable Entry Name ToolTip" : "Show Entry Name ToolTip";
            includeItemLocationsAsDestinationToolStripMenuItem.Text = (Pathfinding.IncludeItemLocations) ? "Exclude Item Locations As Destinations" : "Include Item Locations As Destinations";
            entranceRandoToolStripMenuItem.Visible = VersionHandeling.IsEntranceRando();
            optionsToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            undoToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            redoToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            saveToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            seedCheckerToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            generatePlaythroughToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            whatUnlockedThisToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            updateLogicToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            if (!VersionHandeling.OverRideAutoEntranceRandoEnable) { VersionHandeling.entranceRadnoEnabled = VersionHandeling.IsEntranceRando(); }
            useSongOfTimeInPathfinderToolStripMenuItem.Visible = VersionHandeling.entranceRadnoEnabled;
            includeItemLocationsAsDestinationToolStripMenuItem.Visible = VersionHandeling.entranceRadnoEnabled;
            coupleEntrancesToolStripMenuItem.Visible = VersionHandeling.entranceRadnoEnabled;
            toggleEntranceRandoFeaturesToolStripMenuItem.Text = (VersionHandeling.entranceRadnoEnabled) ? "Disable Entrance Rando Features" : "Enable Entrance Rando Features";
            coupleEntrancesToolStripMenuItem.Text = (LogicEditing.CoupleEntrances) ? "Uncouple Entrances" : "Couple Entrances";
            devToolStripMenuItem.Visible = Debugging.ISDebugging;
            seperateMarkedItemsToolStripMenuItem.Text = (Utility.MoveMarkedToBottom) ? "Don't Seperate Marked Items" : "Seperate Marked Items";


            //OOT Handeling
            importSpoilerLogToolStripMenuItem.Visible = !OOT_Support.isOOT;
            useSongOfTimeInPathfinderToolStripMenuItem.Visible = !OOT_Support.isOOT && VersionHandeling.entranceRadnoEnabled;
            includeItemLocationsAsDestinationToolStripMenuItem.Visible = !OOT_Support.isOOT && VersionHandeling.entranceRadnoEnabled;
            coupleEntrancesToolStripMenuItem.Visible = !OOT_Support.isOOT && VersionHandeling.entranceRadnoEnabled;
            generatePlaythroughToolStripMenuItem.Visible = !OOT_Support.isOOT && (VersionHandeling.Version > 0);
            seedCheckerToolStripMenuItem.Visible = !OOT_Support.isOOT && (VersionHandeling.Version > 0);
            whatUnlockedThisToolStripMenuItem.Visible = !OOT_Support.isOOT && (VersionHandeling.Version > 0);

        }

        private void PrintPaths(int PathToPrint)
        {
            LBPathFinder.Items.Clear();
            var sortedpaths = Pathfinding.paths.OrderBy(x => x.Count);

            if (PathToPrint == -1 || Pathfinding.paths.ElementAtOrDefault(PathToPrint) == null)
            {
                int counter = 1;
                foreach (var path in sortedpaths)
                {
                    LBPathFinder.Items.Add(new LogicObjects.ListItem { DisplayName = "Path: " + counter + " (" + path.Count + " Steps)", ID = counter - 1 });
                    var firstStop = true;
                    foreach (var stop in path)
                    {
                        var start = (firstStop) ? Pathfinding.SetSOTName(stop) : LogicObjects.Logic[stop.EntranceToTake].LocationName;
                        var ListItem = new LogicObjects.ListItem { DisplayName = start + " => " + LogicObjects.Logic[stop.ResultingExit].ItemName, ID = counter - 1 };
                        LBPathFinder.Items.Add(ListItem); firstStop = false;
                    }
                    LBPathFinder.Items.Add(new LogicObjects.ListItem { DisplayName = "===============================", ID = counter - 1 });
                    counter++;
                }
            }
            else
            {
                var path = sortedpaths.ToArray()[PathToPrint];
                var ListTitle = new LogicObjects.ListItem { DisplayName = "Path: " + (PathToPrint + 1) + " (" + path.Count + " Steps)", ID = -1 };
                LBPathFinder.Items.Add(ListTitle);
                foreach (var stop in path)
                {
                    var ThisIsStupid = new LogicObjects.ListItem
                    {
                        DisplayName =
                        ((LogicObjects.Logic[stop.EntranceToTake].DictionaryName == "EntranceSouthClockTownFromClockTowerInterior") ?
                            "Song of Time" :
                            LogicObjects.Logic[stop.EntranceToTake].LocationName
                        ) + " => " + LogicObjects.Logic[stop.ResultingExit].ItemName,
                        ID = -1
                    };
                    LBPathFinder.Items.Add(ThisIsStupid);
                }
                LBPathFinder.Items.Add("===============================");
            }
        }

        public void PrintToComboBox(bool start)
        {
            var UnsortedPathfinder = new Dictionary<int, string>();
            var UnsortedItemPathfinder = new Dictionary<int, string>();
            foreach (var entry in LogicObjects.Logic)
            {
                if (entry.IsEntrance())
                {
                    if ((start) ? entry.Aquired : entry.Available) { UnsortedPathfinder.Add(entry.ID, (start) ? entry.ItemName : entry.LocationName); }
                }
                if (Pathfinding.IncludeItemLocations && entry.ItemSubType != "Entrance" && !entry.IsFake && entry.Available && !start)
                {
                    UnsortedItemPathfinder.Add(entry.ID, (entry.RandomizedItem > -1) ? entry.LocationName + ": " + LogicObjects.Logic[entry.RandomizedItem].ItemName : entry.LocationName);
                }
            }
            Dictionary<int, string> sortedPathfinder = UnsortedPathfinder.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

            if (Pathfinding.IncludeItemLocations && !start)
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

        public void PrintToListBox()
        {
            var lbLocTop = LBValidLocations.TopIndex;
            var lbEntTop = LBValidEntrances.TopIndex;
            var lbCheckTop = LBCheckedLocations.TopIndex;
            int TotalLoc = 0;
            int TotalEnt = 0;
            int totalchk = 0;
            List<LogicObjects.LogicEntry> logic = LogicObjects.Logic;
            List<LogicObjects.LogicEntry> ListBoxItems = new List<LogicObjects.LogicEntry>();
            Dictionary<string,int> Groups = new Dictionary<string, int>();
            Dictionary<int, int> ListBoxAssignments = new Dictionary<int, int>();

            if (File.Exists(@"Recources\Categories.txt"))
            {
                Groups = File.ReadAllLines(@"Recources\Categories.txt")
                    .Select(x => x.ToLower().Trim()).Distinct()
                    .Select((value, index) => new { value, index })
                    .ToDictionary(pair => pair.value, pair => pair.index);
            }

            foreach (var entry in LogicObjects.Logic)
            {
                if (entry.IsFake || entry.Unrandomized() || entry.ForceJunk()) { continue; }

                entry.DisplayName = entry.DictionaryName;
                if ((entry.Available || entry.HasRandomItem() || CHKShowAll.Checked) && (entry.LocationName != "" && entry.LocationName != null) && !entry.Checked)
                {
                    entry.DisplayName = entry.HasRandomItem() ? ($"{entry.LocationName}: {entry.RandomizedEntry(true).ItemName}") : entry.LocationName;

                    if ((!entry.IsEntrance() || !VersionHandeling.entranceRadnoEnabled))
                    {
                        TotalLoc += 1;
                        if (Utility.FilterSearch(entry, TXTLocSearch.Text, entry.DisplayName, entry.RandomizedEntry(true)))
                        {
                            ListBoxAssignments.Add(entry.ID, 0);
                            ListBoxItems.Add(entry);
                        }
                    }
                    else if ((entry.IsEntrance() && VersionHandeling.entranceRadnoEnabled))
                    {
                        TotalEnt += 1;
                        if (Utility.FilterSearch(entry, TXTEntSearch.Text, entry.DisplayName, entry.RandomizedEntry(true)))
                        {
                            ListBoxAssignments.Add(entry.ID, 1);
                            ListBoxItems.Add(entry);
                        }
                    }
                }
                if (entry.Checked)
                {
                    entry.DisplayName = entry.HasRandomItem() ? $"{entry.RandomizedEntry(true).ItemName}: {entry.LocationName}" : $"Nothing: {entry.LocationName}";
                    totalchk += 1;
                    if (Utility.FilterSearch(entry, TXTCheckedSearch.Text, entry.DisplayName, entry.RandomizedEntry(true)))
                    {
                        ListBoxAssignments.Add(entry.ID, 2);
                        ListBoxItems.Add(entry); 
                    }
                }
            }

            ListBoxItems = ListBoxItems
                .OrderBy(x => Utility.BoolToInt(x.IsEntrance()))
                .ThenBy(x => Utility.BoolToInt(x.HasRandomItem() && Utility.MoveMarkedToBottom))
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
                if (ListBoxAssignments[entry.ID] == 0) { lastLocArea = WriteObject(entry, LBValidLocations, lastLocArea, entry.HasRandomItem()); AvalableLocations++; }
                if (ListBoxAssignments[entry.ID] == 1) { lastEntArea = WriteObject(entry, LBValidEntrances, lastEntArea, entry.HasRandomItem()); AvalableEntrances++; }
                if (ListBoxAssignments[entry.ID] == 2) { lastChkArea = WriteObject(entry, LBCheckedLocations, lastChkArea, false); CheckedLocations++; }
            }

            label1.Text = "Available Locations: " + ((AvalableLocations == TotalLoc) ? AvalableLocations.ToString() : (AvalableLocations.ToString() + "/" + TotalLoc.ToString()));
            label2.Text = "Checked locations: " + ((CheckedLocations == totalchk) ? CheckedLocations.ToString() : (CheckedLocations.ToString() + "/" + totalchk.ToString())); ;
            label3.Text = "Available Entrances: " + ((AvalableEntrances == TotalEnt) ? AvalableEntrances.ToString() : (AvalableEntrances.ToString() + "/" + TotalEnt.ToString())); ;
            LBValidLocations.TopIndex = lbLocTop;
            LBValidEntrances.TopIndex = lbEntTop;
            LBCheckedLocations.TopIndex = lbCheckTop;
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

            if (VersionHandeling.Version == 0)
            {
                SetObjectVisibility(false, false);
            }
            else if (VersionHandeling.entranceRadnoEnabled)
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

        public void ShowtoolTip(MouseEventArgs e, ListBox lb)
        {
            if (!Utility.ShowEntryNameTooltip) { return; }
            int index = lb.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            if (!(lb.Items[index] is LogicObjects.LogicEntry || lb.Items[index] is LogicObjects.ListItem)) { return; }
            string DisplayName = lb.Items[index].ToString();
            if (toolTip1.GetToolTip(lb) == DisplayName) { return; }
            if (Utility.IsDivider(DisplayName)) { return; }
            toolTip1.SetToolTip(lb, DisplayName);
        }

        private string WriteObject(LogicObjects.LogicEntry entry, ListBox lb, string lastArea, bool Marked)
        {
            string returnLastArea = lastArea;
            if (entry.LocationArea != returnLastArea)
            {
                if (returnLastArea != "") { lb.Items.Add(Utility.CreateDivider(lb)); }

                string Header = entry.LocationArea.ToUpper();
                if (Marked && entry.IsEntrance()) { Header += " SET EXITS"; }
                else if (Marked && !entry.IsEntrance()) { Header += " SET ITEMS"; }
                else if (entry.IsEntrance()) { Header += " ENTRANCES"; }
                lb.Items.Add(Header + ":");
                returnLastArea = entry.LocationArea;
            }
            lb.Items.Add(entry);
            return (returnLastArea);
        }

        private void verifyCustomRandoCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Code " + (Debugging.VerifyCustomRandoCode() ? "Worked" : "Faled"));
        }

        private void seperateMarkedItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.MoveMarkedToBottom = !Utility.MoveMarkedToBottom;
            FormatMenuItems();
            PrintToListBox();
        }
    }
}
