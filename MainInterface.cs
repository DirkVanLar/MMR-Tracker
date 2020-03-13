using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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
            stricterLogicHandelingToolStripMenuItem.Text = (LogicEditing.StrictLogicHandeling) ? "Disable Stricter Logic Handeling" : "Enable Stricter Logic Handeling";
            toggleEntranceRandoFeaturesToolStripMenuItem.Text = (VersionHandeling.entranceRadnoEnabled) ? "Disable Entrance Rando Features" : "Enable Entrance Rando Features";
            useSongOfTimeInPathfinderToolStripMenuItem.Text = (Pathfinding.UseSongOfTime) ? "Disable Song of Time in pathfinder" : "Enable Song of Time in pathfinder";
            includeItemLocationsAsDestinationToolStripMenuItem.Text = (Pathfinding.IncludeItemLocations) ? "Exclude Item Locations As Destinations" : "Include Item Locations As Destinations";
            ResizeObject();
            entranceRandoToolStripMenuItem.Visible = false;
            devToolStripMenuItem.Visible = true;
            optionsToolStripMenuItem.Visible = false;
            undoToolStripMenuItem.Visible = false;
            redoToolStripMenuItem.Visible = false;
            saveToolStripMenuItem.Visible = false;
        }

        private void FRMTracker_ResizeEnd(object sender, EventArgs e)
        {
            ResizeObject();
        }

        //Menu Strip---------------------------------------------------------------------------
        private void CreateDictionaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.CreateDictionary();
        }

        private void EditRadnomizationOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomizeOptions RandoOptionScreen = new RandomizeOptions();
            RandoOptionScreen.ShowDialog();
            LogicEditing.CalculateItems(LogicObjects.Logic, true, false);
            PrintToListBox();
        }

        private void ImportSpoilerLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var spoilerLogImported = false;
            foreach (var entry in LogicObjects.Logic) { if (entry.SpoilerRandom > -2) { spoilerLogImported = true; break; } }

            if (spoilerLogImported)
            {
                foreach (var entry in LogicObjects.Logic) { entry.SpoilerRandom = -2; }
            }
            else
            {
                string file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt;*html)|*.txt;*html");
                if (file == "") { return; }
                LogicEditing.WriteSpoilerLogToLogic(LogicObjects.Logic, file);
            }
            importSpoilerLogToolStripMenuItem.Text = (spoilerLogImported) ? "Import Spoiler Log" : "Remove Spoiler Log";
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Utility.PromptSave()) { return; }
            string file = Utility.FileSelect("Select A Save File", "MMR Tracker Save (*.MMRTSAV)|*.MMRTSAV");
            if (file == "") { return; }
            Utility.ResetInstance();
            Utility.LoadInstance(file);
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Utility.PromptSave()) { return; }
            string file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt)|*.txt");
            if (file == "") { return; }
            Utility.ResetInstance();
            LogicEditing.CreateLogic(LogicObjects.Logic, file, LogicObjects.DicNameToID);
            LogicEditing.CalculateItems(LogicObjects.Logic, true, false);
            if (!VersionHandeling.ValidVersions.Contains(VersionHandeling.Version))
            {
                DialogResult dialogResult = MessageBox.Show("This version of logic is not supported. Only official releases of versions 1.5 and up are supported. This may result in the tracker not funtioning Correctly. If you are using an official release and are seeing this message, Please update your tracker. Do you wish to continue?", "Unsupported Version", MessageBoxButtons.YesNo);
                if (dialogResult != DialogResult.Yes)
                {
                    Utility.ResetInstance();
                }
            }
            Utility.SaveState(LogicObjects.Logic);
            PrintToListBox();
            ResizeObject();
            FormatMenuItems();
        }

        private void PrintLogicObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DebugScreen DebugScreen = new DebugScreen();
            Debugging.PrintLogicObject(LogicObjects.Logic);
            DebugScreen.DebugFunction = 1;
            DebugScreen.Show();
        }

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
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.SaveInstance();
        }

        private void stricterLogicHandelingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicEditing.StrictLogicHandeling = !LogicEditing.StrictLogicHandeling;
            stricterLogicHandelingToolStripMenuItem.Text = (LogicEditing.StrictLogicHandeling) ? "Disable Stricter Logic Handeling" : "Enable Stricter Logic Handeling";
        }

        private void showEntryNameToolTipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowEntryNameTooltip = !Utility.ShowEntryNameTooltip;
            showEntryNameToolTipToolStripMenuItem.Text = (Utility.ShowEntryNameTooltip) ? "Disable Entry Name ToolTip" : "Show Entry Name ToolTip";
        }

        private void ToggleEntranceRandoFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VersionHandeling.entranceRadnoEnabled = !VersionHandeling.entranceRadnoEnabled;
            toggleEntranceRandoFeaturesToolStripMenuItem.Text = (VersionHandeling.entranceRadnoEnabled) ? "Disable Entrance Rando Features" : "Enable Entrance Rando Features";
            ResizeObject();
            PrintToListBox();
            Console.WriteLine("Entrance rando is " + VersionHandeling.entranceRadnoEnabled);
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.Undo();
            PrintToListBox();
        }

        private void useSongOfTimeInPathfinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pathfinding.UseSongOfTime = !Pathfinding.UseSongOfTime;
            useSongOfTimeInPathfinderToolStripMenuItem.Text = (Pathfinding.UseSongOfTime) ? "Disable Song of Time in pathfinder" : "Enable Song of Time in pathfinder";
        }

        private void includeItemLocationsAsDestinationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pathfinding.IncludeItemLocations = !Pathfinding.IncludeItemLocations;
            includeItemLocationsAsDestinationToolStripMenuItem.Text = (Pathfinding.IncludeItemLocations) ? "Exclude Item Locations As Destinations" : "Include Item Locations As Destinations";
        }

        private void UpdateDisplayNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.UpdateNames(LogicObjects.Logic);
            PrintToListBox();
        }

        //Text Boxes---------------------------------------------------------------------------
        private void TXTLocSearch_TextChanged(object sender, EventArgs e)
        {
            PrintToListBox();
        }

        private void TXTEntSearch_TextChanged(object sender, EventArgs e)
        {
            PrintToListBox();
        }

        private void TXTCheckedSearch_TextChanged(object sender, EventArgs e)
        {
            PrintToListBox();
        }

        //List Boxes---------------------------------------------------------------------------
        private void LBValidLocations_DoubleClick(object sender, EventArgs e) { CheckItemSelected(LBValidLocations, true); }

        private void LBValidEntrances_DoubleClick(object sender, EventArgs e) { CheckItemSelected(LBValidEntrances, true); }

        private void LBCheckedLocations_DoubleClick(object sender, EventArgs e) { CheckItemSelected(LBCheckedLocations, true); }

        private void LBValidLocations_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBValidLocations); }

        private void LBValidEntrances_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBValidEntrances); }

        private void LBCheckedLocations_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBCheckedLocations); }

        private void LBPathFinder_MouseMove(object sender, MouseEventArgs e) { ShowtoolTip(e, LBPathFinder); }

        //Buttons---------------------------------------------------------------------------
        private void BTNSetItem_Click(object sender, EventArgs e)
        {
            if (TXTLocSearch.Text.ToLower() == "enabledev")
            {
                devToolStripMenuItem.Visible = !devToolStripMenuItem.Visible;
                TXTLocSearch.Clear();
                return;
            }
            CheckItemSelected(LBValidLocations, false);
        }

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

            //Print Paths
            var sortedpaths = Pathfinding.paths.OrderBy(x => x.Count);
            int counter = 1;
            foreach (var path in sortedpaths)
            {
                LBPathFinder.Items.Add("Path: " + counter + " (" + path.Count + " Steps)");
                foreach (var stop in path)
                {
                    var ThisIsStupid = new LogicObjects.LogicEntry
                    { DisplayName = 
                        ((LogicObjects.Logic[stop.Entrance].DictionaryName == "EntranceSouthClockTownFromClockTowerInterior")?
                        "Song of Time" : LogicObjects.Logic[stop.Entrance].LocationName) 
                    + " => " + LogicObjects.Logic[stop.ResultingExit].ItemName };
                    LBPathFinder.Items.Add(ThisIsStupid);
                }
                counter++;
                LBPathFinder.Items.Add("===============================");
            }
        }

        //Other---------------------------------------------------------------------------
        private void CHKShowAll_CheckedChanged(object sender, EventArgs e) { PrintToListBox(); }

        private void CMBStart_DropDown(object sender, EventArgs e) { PrintToComboBox(true); adjustCMBWidth(sender); }

        private void CMBEnd_DropDown(object sender, EventArgs e) { PrintToComboBox(false); adjustCMBWidth(sender); }

        //Functions---------------------------------------------------------------------------
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

        public void PrintToListBox()
        {
            LBValidLocations.Items.Clear();
            LBValidEntrances.Items.Clear();
            LBCheckedLocations.Items.Clear();
            var logic = LogicObjects.Logic;

            var Unsortedlogic = new List<LogicObjects.LogicEntry>();

            foreach (var entry in LogicObjects.Logic)
            {
                entry.ListGroup = -1;
                entry.DisplayName = "ERROR";
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
                        entry.ListGroup = 0;
                        Unsortedlogic.Add(entry);
                    }
                    else if ((entry.ItemSubType == "Entrance" && VersionHandeling.entranceRadnoEnabled) &&
                        Utility.FilterSearch(entry, TXTEntSearch.Text, entry.DisplayName))
                    {
                        entry.ListGroup = 1;
                        Unsortedlogic.Add(entry);
                    }
                }
                if (entry.Checked && !entry.IsFake && (entry.RandomizedState == 0 || entry.RandomizedState == 2))
                {
                    entry.DisplayName = (entry.RandomizedItem > -1) ? logic[entry.RandomizedItem].ItemName + ": " + entry.LocationName : "Junk: " + entry.LocationName;

                    entry.ListGroup = 2;
                    if (Utility.FilterSearch(entry, TXTCheckedSearch.Text, entry.DisplayName)) { Unsortedlogic.Add(entry); }
                }

            }

            var sortedlogic = Unsortedlogic.OrderBy(x => x.LocationArea).ThenBy(x => x.DisplayName);

            var lastLocArea = "";
            var lastEntArea = "";
            var lastChkArea = "";

            foreach (var entry in sortedlogic)
            {
                if (entry.ListGroup == 0) { lastLocArea = WriteObject(entry, LBValidLocations, lastLocArea); }
                if (entry.ListGroup == 1) { lastEntArea = WriteObject(entry, LBValidEntrances, lastEntArea); }
                if (entry.ListGroup == 2) { lastChkArea = WriteObject(entry, LBCheckedLocations, lastChkArea); }
            }


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

        public void PrintToComboBox(bool start)
        {
            var UnsortedPathfinder = new Dictionary<int, string>();
            var UnsortedItemPathfinder = new Dictionary<int, string>();
            foreach (var entry in LogicObjects.Logic)
            {
                if (entry.ItemSubType == "Entrance")
                {
                    if (entry.Aquired && start)
                    { UnsortedPathfinder.Add(entry.ID, entry.ItemName ); }
                    if (entry.Available && !start)
                    { UnsortedPathfinder.Add( entry.ID,entry.LocationName ); }
                }
                if (Pathfinding.IncludeItemLocations && entry.ItemSubType != "Entrance" && !entry.IsFake && entry.Available && !start)
                {
                    UnsortedItemPathfinder.Add(entry.ID, (entry.RandomizedItem > -1) ? entry.LocationName + ": " + LogicObjects.Logic[entry.RandomizedItem].ItemName : entry.LocationName  );
                }
            }
            Dictionary<int,string> sortedPathfinder = UnsortedPathfinder.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

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
                foreach (KeyValuePair<int,string> p in sortedItemPathfinder)
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

        public void CheckItemSelected(ListBox LB, bool FullCheck)
        {
            if ((LB.SelectedItem is LogicObjects.LogicEntry))
            {
                var selectedIndex = LB.SelectedIndex;
                //We want to save logic at this point but don't want to comit to a full save state
                var TempState = Utility.CloneLogicList(LogicObjects.Logic);
                if (FullCheck)
                {
                    if (!LogicEditing.CheckObject(LB.SelectedItem as LogicObjects.LogicEntry)) { return; }
                }
                else
                {
                    if (!LogicEditing.MarkObject(LB.SelectedItem as LogicObjects.LogicEntry)) { return; }
                }
                Utility.UnsavedChanges = true;
                //Now that we have successfully checked/Marked an object we can commit to a full save state
                Utility.SaveState(TempState);
                PrintToListBox();
                if (selectedIndex < LB.Items.Count) { LB.SelectedIndex = selectedIndex; }
                else { LB.SelectedIndex = LB.Items.Count - 1; }
            }
        }

        public void ShowtoolTip(MouseEventArgs e, ListBox lb)
        {
            if (!Utility.ShowEntryNameTooltip) { return; }
            int index = lb.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            string tip = lb.Items[index].ToString();
            if (toolTip1.GetToolTip(lb) != tip && (lb.Items[index] is LogicObjects.LogicEntry))
            { toolTip1.SetToolTip(lb, tip); }
        }

        public void adjustCMBWidth(object sender)
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

        public void FormatMenuItems()
        {
            var spoilerLogImported = false;
            foreach (var entry in LogicObjects.Logic) { if (entry.SpoilerRandom > -2) { spoilerLogImported = true; break; } }
            importSpoilerLogToolStripMenuItem.Text = (spoilerLogImported) ? "Remove Spoiler Log" : "Import Spoiler Log";
            useSongOfTimeInPathfinderToolStripMenuItem.Text = (Pathfinding.UseSongOfTime) ? "Disable Song of Time in pathfinder" : "Enable Song of Time in pathfinder";
            stricterLogicHandelingToolStripMenuItem.Text = (LogicEditing.StrictLogicHandeling) ? "Disable Stricter Logic Handeling" : "Enable Stricter Logic Handeling";
            showEntryNameToolTipToolStripMenuItem.Text = (Utility.ShowEntryNameTooltip) ? "Disable Entry Name ToolTip" : "Show Entry Name ToolTip";
            includeItemLocationsAsDestinationToolStripMenuItem.Text = (Pathfinding.IncludeItemLocations) ? "Exclude Item Locations As Destinations" : "Include Item Locations As Destinations";
            entranceRandoToolStripMenuItem.Visible = VersionHandeling.isEntranceRando();
            optionsToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            undoToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            redoToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            saveToolStripMenuItem.Visible = (VersionHandeling.Version > 0);
            VersionHandeling.entranceRadnoEnabled = (VersionHandeling.isEntranceRando());
            toggleEntranceRandoFeaturesToolStripMenuItem.Text = (VersionHandeling.entranceRadnoEnabled) ? "Disable Entrance Rando Features" : "Enable Entrance Rando Features";
        }

        
    }
}
