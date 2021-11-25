using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using MMR_Tracker.Class_Files;

namespace MMR_Tracker.Forms
{
    public partial class PathFinder : Form
    {
        public List<List<LogicObjects.MapPoint>> InstancePaths = new List<List<LogicObjects.MapPoint>>();

        public PathFinder()
        {
            InitializeComponent();
        }

        //Form Handeling

        public void BTNFindPath_Click(object sender, EventArgs e)
        {
            GetAndPrintPaths(LBPathFinder, CMBStart, CMBEnd);
        }

        public async void GetAndPrintPaths(ListBox LB, ComboBox cmbStart, ComboBox cmbEnd, bool UseSOT = false)
        {
            //Find grab the ID of the last path currently in the list. TODO delete that path from the "paths" to free up junk space.
            if (LB.Items.Count > 1)
            {
                var item = LB.Items[1] as LogicObjects.ListItem;
            }
            Console.WriteLine($"Use SOT {UseSOT}");

            LB.Items.Clear();

            if (!(cmbStart.SelectedItem is KeyValuePair<int, string>) || !(cmbEnd.SelectedItem is KeyValuePair<int, string>)) { return; }
            var Startindex = Int32.Parse(cmbStart.SelectedValue.ToString());
            var DestIndex = Int32.Parse(cmbEnd.SelectedValue.ToString());
            if (Startindex < 0 || DestIndex < 0) { return; }
            LB.Items.Add("Finding Path.....");
            LB.Items.Add("Please Wait");
            LB.Refresh();

            bool DestinationAtStarting = await Task.Run(() => Calculatepath(Startindex, DestIndex, UseSOT));

            LB.Items.Clear();

            if (DestinationAtStarting)
            {
                foreach (var i in Utility.WrapStringInListBox(LB, "Your destination is available from your starting area.", "")) { LB.Items.Add(i); }
                return;
            }

            if (InstancePaths.Count == 0)
            {
                if (!UseSOT) 
                {
                    Console.WriteLine("No path found, attempting with Song of Time");
                    GetAndPrintPaths(LB, cmbStart, cmbEnd, true);
                    return;
                }

                foreach (var i in Utility.WrapStringInListBox(LB, "The path finder could not valid path to your destination!", "")) { LB.Items.Add(i); }
                LB.Items.Add("");
                foreach (var i in Utility.WrapStringInListBox(LB, "This path finder is still in beta and may not always work as intended.", "")) { LB.Items.Add(i); }
                var ErrT = "If you believe this is an error try navigating to a different entrance close to your destination or try a different starting point.";
                foreach (var i in Utility.WrapStringInListBox(LB, ErrT, "")) { LB.Items.Add(i); }

                return;
            }
            PrintPaths(-1, LB, UseSOT);
        }

        public bool Calculatepath(int Startindex, int DestIndex, bool UseSOT)
        {
            var startinglocation = LogicObjects.MainTrackerInstance.Logic[Startindex];
            var destination = LogicObjects.MainTrackerInstance.Logic[DestIndex];
            var maps = FindLogicalEntranceConnections(LogicObjects.MainTrackerInstance, startinglocation, UseSOT);
            var Fullmap = maps[0]; //A map of all available exit from each entrance
            var ResultMap = maps[1]; //A map of all available entrances from each exit as long as the result of that entrance is known

            if (Fullmap.Find(x => x.CurrentExit == startinglocation.ID && x.EntranceToTake == destination.ID) != null) { return true; }

            Findpath(LogicObjects.MainTrackerInstance, ResultMap, Fullmap, startinglocation.ID, destination.ID, new List<int>(), new List<int>(), new List<LogicObjects.MapPoint>(),true);
            return false;
        }

        private void CMBStart_DropDown(object sender, EventArgs e)
        {
            PrintToComboBox(true); AdjustCMBWidth(sender);
        }

        private void CMBEnd_DropDown(object sender, EventArgs e)
        {
            PrintToComboBox(false); AdjustCMBWidth(sender);
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
                if (LogicObjects.MainTrackerInstance.Options.IncludeItemLocations && !entry.IsEntrance() && !entry.IsFake && entry.Available && !start)
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

            if (sortedPathfinder.Values.Count < 1) { return; }
            ComboBox cmb = (start) ? CMBStart : CMBEnd;
            cmb.DataSource = new BindingSource(sortedPathfinder, null);
            cmb.DisplayMember = "Value";
            cmb.ValueMember = "key";
        }

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

            if (senderComboBox.Items.Count < 1) { return; }

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

        //Pathfinding

        public void PrintPaths(int PathToPrint, ListBox ListBox, bool SOTUsed = false)
        {
            if (!(ListBox is ListBox)) { return; }
            var listbox = ListBox as ListBox;
            listbox.Items.Clear();

            if (SOTUsed)
            {
                foreach (var i in Utility.WrapStringInListBox(ListBox, "A valid path could not be found without the use of Song of Time!", "")) 
                {
                    listbox.Items.Add(new LogicObjects.ListItem { DisplayName = i, PathID = -1 });
                }
                listbox.Items.Add(new LogicObjects.ListItem { DisplayName = "===============================", PathID = -1 });
            }

            var sortedpaths = InstancePaths.OrderBy(x => x.Count);

            if (PathToPrint == -1 || InstancePaths.ElementAtOrDefault(PathToPrint) == null)
            {
                int counter = 1;
                foreach (var path in sortedpaths)
                {
                    var ListTitle = new LogicObjects.ListItem { DisplayName = "Path: " + counter + " (" + path.Count + " Steps)", PathID = counter - 1 };
                    listbox.Items.Add(ListTitle);
                    var firstStop = true;
                    foreach (var stop in path)
                    {
                        var start = (firstStop) ? SetSOTName(LogicObjects.MainTrackerInstance, stop) : LogicObjects.MainTrackerInstance.Logic[stop.EntranceToTake].LocationName;
                        var ListItem = new LogicObjects.ListItem
                        {
                            DisplayName = start + " => " + LogicObjects.MainTrackerInstance.Logic[stop.ResultingExit].ItemName,
                            PathID = counter - 1
                        };
                        listbox.Items.Add(ListItem); firstStop = false;
                    }
                    listbox.Items.Add(new LogicObjects.ListItem
                    {
                        DisplayName = "===============================",
                        PathID = counter - 1
                    });
                    counter++;
                }
            }
            else
            {
                var path = sortedpaths.ToArray()[PathToPrint];
                var ListTitle = new LogicObjects.ListItem { DisplayName = "Path: " + (PathToPrint + 1) + " (" + path.Count + " Steps)", PathID = -1 };
                listbox.Items.Add(ListTitle);
                var firstStop = true;
                foreach (var stop in path)
                {
                    var start = (firstStop) ? SetSOTName(LogicObjects.MainTrackerInstance, stop) : LogicObjects.MainTrackerInstance.Logic[stop.EntranceToTake].LocationName;
                    var ListItem = new LogicObjects.ListItem
                    {
                        DisplayName = start + " => " + LogicObjects.MainTrackerInstance.Logic[stop.ResultingExit].ItemName,
                        PathID = -1
                    };
                    listbox.Items.Add(ListItem);
                    firstStop = false;
                }
                listbox.Items.Add(new LogicObjects.ListItem
                {
                    DisplayName = "===============================",
                    PathID = -1
                });
            }
        }

        public void LBPathFinder_DoubleClick(object sender, EventArgs e)
        {
            ListBox LB = sender as ListBox;
            if (LB.SelectedItem is LogicObjects.ListItem)
            {
                var item = LB.SelectedItem as LogicObjects.ListItem;
                PrintPaths(item.PathID, LB);
            }
            else { return; }
        }

        public List<List<LogicObjects.MapPoint>> FindLogicalEntranceConnections(LogicObjects.TrackerInstance Instance, LogicObjects.LogicEntry startinglocation, bool useSOT)
        {
            //Result[0] = A map of all available exit from each entrance, Result[1] = A map of all available entrances from each exit as long as the result of that entrance is known.
            var result = new List<List<LogicObjects.MapPoint>> { new List<LogicObjects.MapPoint>(), new List<LogicObjects.MapPoint>() };

            var logicTemplate = new LogicObjects.TrackerInstance
            {
                Logic = Utility.CloneLogicList(Instance.Logic),
                Options = Instance.Options
            };
            UnmarkEntrances(logicTemplate.Logic);

            AddConditionalsToInsideClockTower(logicTemplate, useSOT);

            //Add all available owl warps as valid exits from our starting point.
            foreach (LogicObjects.LogicEntry OwlEntry in Instance.Logic.Where(x => x.IsWarpSong() && x.Available))
            {
                var newEntry = new LogicObjects.MapPoint
                {
                    CurrentExit = startinglocation.ID,
                    EntranceToTake = OwlEntry.ID,
                    ResultingExit = OwlEntry.RandomizedItem
                };
                result[0].Add(newEntry);
                if (newEntry.ResultingExit > -1) { result[1].Add(newEntry); }
            }

            //For each entrance, mark it Aquired and check what entrances (and item locations if they are included) become avalable.
            foreach (LogicObjects.LogicEntry entry in Instance.Logic.Where(x => x.RandomizedItem > -1 && x.IsEntrance() && x.Checked))
            {
                var ExitToCheck = logicTemplate.Logic[entry.RandomizedItem];
                //There are no valid exits from majoras lair. Logic uses it to make sure innaccessable exits don't lock something needed to beat the game.
                if (ExitToCheck.DictionaryName == "EntranceMajorasLairFromTheMoon") { continue; }
                ExitToCheck.Aquired = true;
                LogicEditing.CalculateItems(logicTemplate);
                foreach (var dummyEntry in logicTemplate.Logic.Where(x => EntranceConnectionValid(x, ExitToCheck, logicTemplate)))
                {
                    var newEntry = new LogicObjects.MapPoint
                    {
                        CurrentExit = ExitToCheck.ID,
                        EntranceToTake = dummyEntry.ID,
                        ResultingExit = dummyEntry.RandomizedItem
                    };
                    result[0].Add(newEntry);
                    if (newEntry.ResultingExit > -1) { result[1].Add(newEntry); }
                }
                UnmarkEntrances(logicTemplate.Logic);
            }
            return result;
        }

        public void UnmarkEntrances(List<LogicObjects.LogicEntry> Logic)
        {
            foreach (LogicObjects.LogicEntry Entry in Logic)
            {
                if (Entry.IsEntrance() || (Entry.IsFake && !NotAreaAccess(Entry, Logic)))
                {
                    Entry.Aquired = false;
                    Entry.Available = false;
                }
            }
        }

        public bool EntranceConnectionValid(LogicObjects.LogicEntry x, LogicObjects.LogicEntry ExitToCheck, LogicObjects.TrackerInstance lt)
        {
            return (x.Available && !x.IsFake && !x.IsWarpSong() && (x.IsEntrance() || lt.Options.IncludeItemLocations));
        }

        public void Findpath(LogicObjects.TrackerInstance Instance, List<LogicObjects.MapPoint> map, List<LogicObjects.MapPoint> FullMap, int startinglocation, int destination, List<int> ExitsKnown, List<int> ExitsVisited, List<LogicObjects.MapPoint> Path, bool InitialRun = false)
        {
            #region
            //map               :A map of all available entrances from each exit as long as the result of that entrance is known
            //FullMap           :A map of all available exit from each entrance
            //startinglocation  :The ID of the last exit you came from
            //destination       :The ID of the entrance you want to reach
            //ExitsKnown        :The IDs of each entrance we've seen from areas we've been to and areas we've scanned using the NewExitsInResultingArea function
            //ExitsVisited      :The IDs of each entrance we've seen from area we have actually been to or could have been to earlier in the path
            //Path              :A list of exits you have taken to get to your current point
            //InitialRun        :Is this code being run from the original source (True) or from it's self (False)
            #endregion
            if (InitialRun) { InstancePaths = new List<List<LogicObjects.MapPoint>>(); }
            //Make a copy to edit and pass to the next funtion
            var ExitsKnownCopy = JsonConvert.DeserializeObject<List<int>>(JsonConvert.SerializeObject(ExitsKnown));
            var ExitsVisitedCopy = JsonConvert.DeserializeObject<List<int>>(JsonConvert.SerializeObject(ExitsKnown));
            //A list of exits available to take
            var validExits = new List<LogicObjects.MapPoint>();

            //If we can reach our destination from this point, add the path we took to get here to the valid paths list.
            if (FullMap.Find(x => x.CurrentExit == startinglocation && x.EntranceToTake == destination) != null) { InstancePaths.Add(Path); }

            //For each exit from our current location
            foreach (var point in map.Where(x => x.CurrentExit == startinglocation))
            {
                #region
                /*Check that the exit we are taking has not been accessable from a previous exit we have actually been to or seen earlier in the path.
                Then Check to see if taking this exit contains exits we haven't seen before.*/
                #endregion
                if (!ExitsVisited.Contains(point.EntranceToTake) && ScanExitResults(map, point.ResultingExit, ExitsKnownCopy))
                {
                    validExits.Add(point);
                    ExitsKnownCopy.Add(point.EntranceToTake);
                    ExitsVisitedCopy.Add(point.EntranceToTake);
                }
            }

            //Pick the first entrance in the valid exits list, rerun the function with the resulting exit as the starting location
            foreach (var exit in validExits)
            {
                var UpdatedPath = JsonConvert.DeserializeObject<List<LogicObjects.MapPoint>>(JsonConvert.SerializeObject(Path));
                UpdatedPath.Add(exit);
                Findpath(Instance, map, FullMap, exit.ResultingExit, destination, ExitsKnownCopy, ExitsVisitedCopy, UpdatedPath);
            }
        }

        public bool ScanExitResults(List<LogicObjects.MapPoint> map, int startinglocation, List<int> ExitsSeen)
        {
            // This checks all of the exits in the area we would go to if we took this exit. If we have seen all of these exits before there is no need to take this exit.
            var Newexits = false;
            foreach (var point in map.Where(x => x.CurrentExit == startinglocation))
            {
                if (!ExitsSeen.Contains(point.EntranceToTake))
                {
                    Newexits = true;
                    ExitsSeen.Add(point.EntranceToTake);
                }
            }
            return Newexits;
        }

        private void AddConditionalsToInsideClockTower(LogicObjects.TrackerInstance Instance, bool useSOT)
        {
            var ClockTowerToClockTown = Instance.Logic.Find(x => x.DictionaryName == "EntranceSouthClockTownFromClockTowerInterior");
            var ClockTownToClockTower = Instance.Logic.Find(x => x.DictionaryName == "EntranceClockTowerInteriorFromSouthClockTown");
            var LostWoodsToTermina = Instance.Logic.Find(x => x.DictionaryName == "EntranceClockTowerInteriorFromBeforethePortaltoTermina");
            if (ClockTowerToClockTown == null || ClockTowerToClockTown == null || ClockTowerToClockTown == null || useSOT) { Console.WriteLine("Skip COnd SOT used"); return; }
            Console.WriteLine("Conditionals were added to Beneath CT");
            var NewConditionals = ClockTowerToClockTown.Conditionals;
            NewConditionals = LogicEditing.AddConditional(NewConditionals, new int[] { ClockTownToClockTower.ID });
            NewConditionals = LogicEditing.AddConditional(NewConditionals, new int[] { LostWoodsToTermina.ID });
            ClockTowerToClockTown.Conditionals = NewConditionals;
        }

        public string SetSOTName(LogicObjects.TrackerInstance Instance, LogicObjects.MapPoint stop)
        {
            var StartName = Instance.Logic[stop.CurrentExit].DictionaryName;
            var entName = Instance.Logic[stop.EntranceToTake].DictionaryName;
            if (entName == "EntranceSouthClockTownFromClockTowerInterior")
            {
                if (StartName == "EntranceClockTowerInteriorFromBeforethePortaltoTermina" || StartName == "EntranceClockTowerInteriorFromSouthClockTown")
                {
                    return Instance.Logic[stop.EntranceToTake].LocationName;
                }
                else { return "Song of Time"; }
            }
            return Instance.Logic[stop.EntranceToTake].LocationName; ;
        }

        public bool NotAreaAccess(LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> Logic)
        {
            //Attempts to check if a fake item details area access. If the fake item can be unlocked with only real item entries it should represent access to an area
            //And shouldn't need to be unaquired for the pathfinder
            var Reqdef = new int[0];
            var Condef = new int[0][];
            foreach (var i in entry.Required ?? Reqdef)
            {
                var reqEntry = Logic[i];
                if (reqEntry.IsFake || reqEntry.IsEntrance()) { return false; }
            }
            foreach (var h in entry.Conditionals ?? Condef)
            {
                var RealEntry = true;
                foreach (var i in h)
                {
                    var CondEntry = Logic[i];
                    if (CondEntry.IsFake || CondEntry.IsEntrance()) { RealEntry = false; }
                }
                if (RealEntry) { return true; }
            }
            return false;
        }

        private void lblSwapPathfinder_Click(object sender, EventArgs e)
        {
            LogicObjects.LogicEntry newStart = null;
            LogicObjects.LogicEntry newDest = null;
            try
            {
                int DestIndex = Int32.Parse(CMBEnd.SelectedValue.ToString());
                newStart = LogicObjects.MainTrackerInstance.Logic[DestIndex].PairedEntry(LogicObjects.MainTrackerInstance);
            }
            catch { }
            try
            {
                int StartIndex = Int32.Parse(CMBStart.SelectedValue.ToString());
                newDest = LogicObjects.MainTrackerInstance.Logic[StartIndex].PairedEntry(LogicObjects.MainTrackerInstance);
            }
            catch { }

            if (newStart == null || newDest == null) { return; }

            try
            {
                CMBStart.DataSource = new BindingSource(new Dictionary<int, string> { { newStart.ID, newStart.ItemName } }, null);
                CMBStart.DisplayMember = "Value";
                CMBStart.ValueMember = "key";
                CMBStart.SelectedIndex = 0;
            }
            catch { }
            try
            {
                CMBEnd.DataSource = new BindingSource(new Dictionary<int, string> { { newDest.ID, newDest.LocationName } }, null);
                CMBEnd.DisplayMember = "Value";
                CMBEnd.ValueMember = "key";
                CMBEnd.SelectedIndex = 0;
            }
            catch { }
        }
    }
}
