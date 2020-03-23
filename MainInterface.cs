using MMR_Tracker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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
            ResizeObject();
            FormatMenuItems();
            devToolStripMenuItem.Visible = false;
        }

        private void FRMTracker_ResizeEnd(object sender, EventArgs e) { ResizeObject(); }

        //Menu Strip---------------------------------------------------------------------------

        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DebugScreen DebugScreen = new DebugScreen();
            Debugging.PrintLogicObject(LogicObjects.Logic);
            DebugScreen.DebugFunction = 2;
            DebugScreen.Show();
        }

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
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Utility.UnsavedChanges) { if (!Utility.PromptSave()) { return; } }
            string file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSET)|*.txt;*.MMRTSET");
            if (file == "") { return; }

            bool SettingsFile = file.EndsWith(".MMRTSET");
            var lines = (SettingsFile) ? File.ReadAllLines(file).Skip(2) : File.ReadAllLines(file);

            CreateTrackerInstance(lines.ToArray());

            if (SettingsFile) { 
                RandomizeOptions.UpdateRandomOptionsFromFile(File.ReadAllLines(file));
                VersionHandeling.entranceRadnoEnabled = Utility.CheckForRandomEntrances(LogicObjects.Logic);
                VersionHandeling.OverRideAutoEntranceRandoEnable = (VersionHandeling.entranceRadnoEnabled != VersionHandeling.isEntranceRando());
            }

            if (VersionHandeling.isEntranceRando() && !SettingsFile)
            {
                foreach (var item in LogicObjects.Logic)
                {
                    if (item.ItemSubType == "Entrance") { item.RandomizedState = 1; }
                    VersionHandeling.entranceRadnoEnabled = false;
                    VersionHandeling.OverRideAutoEntranceRandoEnable = true;
                }
                LogicEditing.CalculateItems(LogicObjects.Logic, true, true);
            }
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }

        //Menu Strip => File => New---------------------------------------------------------------------------

        private void CasualLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Utility.UnsavedChanges) { if (!Utility.PromptSave()) { return; } }
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
            if (Utility.UnsavedChanges) { if (!Utility.PromptSave()) { return; } }
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
            bool EntrancesRandoBefore = Utility.CheckForRandomEntrances(LogicObjects.Logic);
            RandomizeOptions RandoOptionScreen = new RandomizeOptions();
            RandoOptionScreen.ShowDialog();
            bool EntrancesRandoAfter = Utility.CheckForRandomEntrances(LogicObjects.Logic);

            if(!VersionHandeling.OverRideAutoEntranceRandoEnable || (EntrancesRandoBefore != EntrancesRandoAfter))
            {
                VersionHandeling.entranceRadnoEnabled = Utility.CheckForRandomEntrances(LogicObjects.Logic);
                VersionHandeling.OverRideAutoEntranceRandoEnable = (VersionHandeling.entranceRadnoEnabled != VersionHandeling.isEntranceRando());
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
                if (!Utility.CheckforFullSpoilerLog(LogicObjects.Logic)) { MessageBox.Show("Not all checks have been assigned spoiler data!"); }
            }
            FormatMenuItems();
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
            VersionHandeling.OverRideAutoEntranceRandoEnable = (VersionHandeling.entranceRadnoEnabled != VersionHandeling.isEntranceRando());
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
                LogicEditing.CalculateItems(LogicObjects.Logic, true, true);
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
            DebugScreen DebugScreen = new DebugScreen();
            Debugging.PrintLogicObject(LogicObjects.Logic);
            DebugScreen.DebugFunction = 1;
            DebugScreen.Show();
        }

        private void UpdateDisplayNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.UpdateNames(LogicObjects.Logic);
            PrintToListBox();
        }

        private void UpdateLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicEditing.RecreateLogic();
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }

        //Menu Strip => Options => MISC Options---------------------------------------------------------------------------

        private void ShowEntryNameToolTipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowEntryNameTooltip = !Utility.ShowEntryNameTooltip;
            FormatMenuItems();
        }

        private void seedCheckerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SeedChecker SeedCheckerForm = new SeedChecker();
            SeedCheckerForm.ShowDialog();
        }

        //Text Boxes---------------------------------------------------------------------------
        private void TXTLocSearch_TextChanged(object sender, EventArgs e) {  PrintToListBox();  }

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

            if (!(CMBStart.SelectedItem is KeyValuePair<int,string>) || !(CMBEnd.SelectedItem is KeyValuePair<int, string>)) { return; }
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
            Pathfinding.Findpath(ResultMap, Fullmap, startinglocation.ID, destination.ID, new List<int>(), new List<int>(), new List<LogicObjects.Map>(), true);
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

            foreach (var s in senderComboBox.Items)
            {
                var json = JsonConvert.SerializeObject(s);
                var dictionary = JsonConvert.DeserializeObject<KeyValuePair<int, string>>(json);

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
                devToolStripMenuItem.Visible = !devToolStripMenuItem.Visible;
                TXTLocSearch.Clear();
                return;
            }
            foreach (var i in LB.SelectedItems)
            {
                if ((i is LogicObjects.LogicEntry))
                {
                    //var selectedIndex = LB.SelectedIndex;
                    //We want to save logic at this point but don't want to comit to a full save state
                    var TempState = Utility.CloneLogicList(LogicObjects.Logic);
                    if (FullCheck)
                    {
                        if (!LogicEditing.CheckObject(i as LogicObjects.LogicEntry)) { return; }
                    }
                    else
                    {
                        if (!LogicEditing.MarkObject(i as LogicObjects.LogicEntry)) { return; }
                    }
                    Utility.UnsavedChanges = true;
                    //Now that we have successfully checked/Marked an object we can commit to a full save state
                    Utility.SaveState(TempState);
                }
                
            }
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
            if (!VersionHandeling.ValidVersions.Contains(VersionHandeling.Version))
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
            entranceRandoToolStripMenuItem.Visible = VersionHandeling.isEntranceRando();
            optionsToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            undoToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            redoToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            saveToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            if (!VersionHandeling.OverRideAutoEntranceRandoEnable) { VersionHandeling.entranceRadnoEnabled = (VersionHandeling.isEntranceRando()); }
            useSongOfTimeInPathfinderToolStripMenuItem.Visible = VersionHandeling.entranceRadnoEnabled;
            includeItemLocationsAsDestinationToolStripMenuItem.Visible = VersionHandeling.entranceRadnoEnabled;
            coupleEntrancesToolStripMenuItem.Visible = VersionHandeling.entranceRadnoEnabled;
            toggleEntranceRandoFeaturesToolStripMenuItem.Text = (VersionHandeling.entranceRadnoEnabled) ? "Disable Entrance Rando Features" : "Enable Entrance Rando Features";
            coupleEntrancesToolStripMenuItem.Text = (LogicEditing.CoupleEntrances) ? "Uncouple Entrances" : "Couple Entrances";
            LBValidEntrances.SelectionMode = (LogicEditing.CoupleEntrances) ? SelectionMode.One : SelectionMode.MultiExtended;
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
                        var start = (firstStop) ? Utility.CheckSOT(stop) : LogicObjects.Logic[stop.Entrance].LocationName;
                        var ListItem = new LogicObjects.ListItem{ DisplayName = start + " => " + LogicObjects.Logic[stop.ResultingExit].ItemName, ID = counter - 1 };
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
                        ((LogicObjects.Logic[stop.Entrance].DictionaryName == "EntranceSouthClockTownFromClockTowerInterior") ?
                            "Song of Time" :
                            LogicObjects.Logic[stop.Entrance].LocationName
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
                if (entry.ItemSubType == "Entrance")
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
            LBValidLocations.Items.Clear();
            LBValidEntrances.Items.Clear();
            LBCheckedLocations.Items.Clear();
            var logic = LogicObjects.Logic;

            var Unsortedlogic = new List<LogicObjects.LogicEntry>();
            Dictionary<int, int> listGroups = new Dictionary<int, int>();

            foreach (var entry in LogicObjects.Logic)
            {
                entry.DisplayName = entry.DictionaryName;
                if ((entry.Available || CHKShowAll.Checked) &&
                    !entry.IsFake &&
                    (entry.LocationName != "" && entry.LocationName != null) &&
                    !entry.Checked &&
                    (entry.RandomizedState == 0 || entry.RandomizedState == 2))
                {
                    entry.DisplayName = (entry.RandomizedItem > -2) ? (
                        (entry.RandomizedItem > -1) ?
                        entry.LocationName + ": " + logic[entry.RandomizedItem].ItemName :
                        entry.LocationName + ": JUNK") :
                    entry.LocationName;

                    if ((entry.ItemSubType != "Entrance" || !VersionHandeling.entranceRadnoEnabled) &&
                        Utility.FilterSearch(entry, TXTLocSearch.Text, entry.DisplayName))
                    {
                        listGroups.Add(entry.ID, 0);
                        Unsortedlogic.Add(entry);
                    }
                    else if ((entry.ItemSubType == "Entrance" && VersionHandeling.entranceRadnoEnabled) &&
                        Utility.FilterSearch(entry, TXTEntSearch.Text, entry.DisplayName))
                    {
                        listGroups.Add(entry.ID, 1);
                        Unsortedlogic.Add(entry);
                    }
                }
                if (entry.Checked && !entry.IsFake && (entry.RandomizedState == 0 || entry.RandomizedState == 2))
                {
                    entry.DisplayName = (entry.RandomizedItem > -1) ? logic[entry.RandomizedItem].ItemName + ": " + entry.LocationName : "Junk: " + entry.LocationName;

                    listGroups.Add(entry.ID, 2);
                    if (Utility.FilterSearch(entry, TXTCheckedSearch.Text, entry.DisplayName)) { Unsortedlogic.Add(entry); }
                }

            }

            var sortedlogic = Unsortedlogic.OrderBy(x => x.LocationArea).ThenBy(x => x.DisplayName);

            var lastLocArea = "";
            var lastEntArea = "";
            var lastChkArea = "";

            int AvalableLocations = 0;
            int AvalableEntrances = 0;
            int CheckedLocations = 0;

            foreach (var entry in sortedlogic)
            {
                if (!listGroups.ContainsKey(entry.ID)) { continue; }
                if (listGroups[entry.ID] == 0) { lastLocArea = WriteObject(entry, LBValidLocations, lastLocArea); AvalableLocations++; }
                if (listGroups[entry.ID] == 1) { lastEntArea = WriteObject(entry, LBValidEntrances, lastEntArea); AvalableEntrances++; }
                if (listGroups[entry.ID] == 2) { lastChkArea = WriteObject(entry, LBCheckedLocations, lastChkArea); CheckedLocations++; }
            }
            label1.Text = "Available Locations: " + AvalableLocations;
            label2.Text = "Checked locations: " + CheckedLocations;
            label3.Text = "Available Entrances: " + AvalableEntrances;
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
            string tip = lb.Items[index].ToString();
            if (toolTip1.GetToolTip(lb) == tip) { return; }
            if (Utility.IsDivider(tip)) { return; }
            toolTip1.SetToolTip(lb, tip);
        }

        private string WriteObject(LogicObjects.LogicEntry entry, ListBox lb, string lastArea)
        {
            string returnLastArea = lastArea;
            if (entry.LocationArea != returnLastArea)
            {
                if (returnLastArea != "") { lb.Items.Add("================================"); }
                lb.Items.Add(entry.LocationArea.ToUpper() + ":");
                returnLastArea = entry.LocationArea;
            }
            lb.Items.Add(entry);
            return (returnLastArea);
        }

        private void generatePlaythroughToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Debugging.GeneratePlaythrough(LogicObjects.Logic);
        }
    }
}
