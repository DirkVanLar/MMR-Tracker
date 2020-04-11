using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Newtonsoft.Json;

namespace MMR_Tracker.Forms
{
    public partial class PathFinder : Form
    {
        public static List<List<LogicObjects.MapPoint>> paths = new List<List<LogicObjects.MapPoint>>();

        public PathFinder()
        {
            InitializeComponent();
        }

        //Form Handeling

        private void BTNFindPath_Click(object sender, EventArgs e)
        {
            LBPathFinder.Items.Clear();

            if (!(CMBStart.SelectedItem is KeyValuePair<int, string>) || !(CMBEnd.SelectedItem is KeyValuePair<int, string>)) { return; }
            var Startindex = Int32.Parse(CMBStart.SelectedValue.ToString());
            var DestIndex = Int32.Parse(CMBEnd.SelectedValue.ToString());
            if (Startindex < 0 || DestIndex < 0) { return; }
            var startinglocation = LogicObjects.MainTrackerInstance.Logic[Startindex];
            var destination = LogicObjects.MainTrackerInstance.Logic[DestIndex];
            LBPathFinder.Items.Add("Finding Path.....");
            LBPathFinder.Items.Add("Please Wait");
            LBPathFinder.Items.Add("");
            LBPathFinder.Items.Add("This could take up to 20");
            LBPathFinder.Items.Add("seconds depending on how");
            LBPathFinder.Items.Add("complete your map is and");
            LBPathFinder.Items.Add("the speed of your PC!");
            LBPathFinder.Refresh();

            var maps = FindLogicalEntranceConnections(LogicObjects.MainTrackerInstance, startinglocation);
            var Fullmap = maps[0]; //A map of all available exit from each entrance
            var ResultMap = maps[1]; //A map of all available entrances from each exit as long as the result of that entrance is known
            Findpath(LogicObjects.MainTrackerInstance, ResultMap, Fullmap, startinglocation.ID, destination.ID, new List<int>(), new List<int>(), new List<LogicObjects.MapPoint>(), true);

            LBPathFinder.Items.Clear();

            if (PathFinder.paths.Count == 0)
            {
                LBPathFinder.Items.Add("No Path Found!");
                LBPathFinder.Items.Add("");
                LBPathFinder.Items.Add("This path finder is still in beta");
                LBPathFinder.Items.Add("And can be buggy.");
                LBPathFinder.Items.Add("If you believe this is an error");
                LBPathFinder.Items.Add("try navigating to a different entrance");
                LBPathFinder.Items.Add("close to your destination or try a");
                LBPathFinder.Items.Add("different starting point.");
                if (!LogicObjects.MainTrackerInstance.Options.UseSongOfTime)
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

        //Pathfinding

        private void PrintPaths(int PathToPrint)
        {
            LBPathFinder.Items.Clear();
            var sortedpaths = paths.OrderBy(x => x.Count);

            if (PathToPrint == -1 || paths.ElementAtOrDefault(PathToPrint) == null)
            {
                int counter = 1;
                foreach (var path in sortedpaths)
                {
                    LBPathFinder.Items.Add(new LogicObjects.ListItem { DisplayName = "Path: " + counter + " (" + path.Count + " Steps)", ID = counter - 1 });
                    var firstStop = true;
                    foreach (var stop in path)
                    {
                        var start = (firstStop) ? SetSOTName(LogicObjects.MainTrackerInstance, stop) : LogicObjects.MainTrackerInstance.Logic[stop.EntranceToTake].LocationName;
                        var ListItem = new LogicObjects.ListItem { DisplayName = start + " => " + LogicObjects.MainTrackerInstance.Logic[stop.ResultingExit].ItemName, ID = counter - 1 };
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
                        ((LogicObjects.MainTrackerInstance.Logic[stop.EntranceToTake].DictionaryName == "EntranceSouthClockTownFromClockTowerInterior") ?
                            "Song of Time" :
                            LogicObjects.MainTrackerInstance.Logic[stop.EntranceToTake].LocationName
                        ) + " => " + LogicObjects.MainTrackerInstance.Logic[stop.ResultingExit].ItemName,
                        ID = -1
                    };
                    LBPathFinder.Items.Add(ThisIsStupid);
                }
                LBPathFinder.Items.Add("===============================");
            }
        }

        private void LBPathFinder_DoubleClick(object sender, EventArgs e)
        {
            if (LBPathFinder.SelectedItem is LogicObjects.ListItem)
            {
                var item = LBPathFinder.SelectedItem as LogicObjects.ListItem;
                PrintPaths(item.ID);
            }
            else { PrintPaths(-1); }
        }

        public static List<List<LogicObjects.MapPoint>> FindLogicalEntranceConnections(LogicObjects.TrackerInstance Instance, LogicObjects.LogicEntry startinglocation)
        {
            //Result[0] = All connections that have been checked and have a radnomized item, Result[1] = All connections regardless of if they have been checked.
            var result = new List<List<LogicObjects.MapPoint>> { new List<LogicObjects.MapPoint>(), new List<LogicObjects.MapPoint>() };

            var logicTemplate = new LogicObjects.TrackerInstance
            {
                Logic = Utility.CloneLogicList(Instance.Logic),
                Options = Instance.Options
            };
            UnmarkEntrances(logicTemplate.Logic);

            //Add all available owl warps as valid exits from our starting point.
            foreach (LogicObjects.LogicEntry OwlEntry in Instance.Logic.Where(x => x.IsWarpSong() && x.Available))
            {
                var newEntry = new LogicObjects.MapPoint
                {
                    CurrentExit = startinglocation.ID,
                    EntranceToTake = OwlEntry.ID,
                    ResultingExit = (OwlEntry.RandomizedItem > -1) ? OwlEntry.RandomizedItem : -2
                };
                result[0].Add(newEntry);
                if (newEntry.ResultingExit > -1) { result[1].Add(newEntry); }
            }

            //For each entrance, mark it Aquired and check what entrances (and item locations if they are included) become avalable.
            foreach (LogicObjects.LogicEntry entry in Instance.Logic.Where(x => x.RandomizedItem > -1 && x.IsEntrance() && x.Checked))
            {
                var ExitToCheck = logicTemplate.Logic[entry.RandomizedItem];
                ExitToCheck.Aquired = true;
                LogicEditing.CalculateItems(logicTemplate, true);
                foreach (var dummyEntry in logicTemplate.Logic.Where(x => EntranceConnectionValid(x, ExitToCheck, logicTemplate)))
                {
                    var newEntry = new LogicObjects.MapPoint
                    {
                        CurrentExit = ExitToCheck.ID,
                        EntranceToTake = dummyEntry.ID,
                        ResultingExit = (dummyEntry.RandomizedItem > -1) ? dummyEntry.RandomizedItem : -2
                    };
                    result[0].Add(newEntry);
                    if (newEntry.ResultingExit > -1 && dummyEntry.IsEntrance()) { result[1].Add(newEntry); }
                }
                UnmarkEntrances(logicTemplate.Logic);
            }
            var l = Instance.Logic;
            foreach (var i in result[1])
            {
                Console.WriteLine($"{l[i.CurrentExit].ItemName} => {l[i.EntranceToTake].LocationName} => {l[i.ResultingExit].ItemName}");
            }
            return result;
        }

        public static void UnmarkEntrances(List<LogicObjects.LogicEntry> Logic)
        {
            foreach (LogicObjects.LogicEntry Entry in Logic)
            {
                if (Entry.IsEntrance())
                {
                    Entry.Aquired = false;
                    Entry.Available = false;
                }
            }
        }

        public static bool EntranceConnectionValid(LogicObjects.LogicEntry x, LogicObjects.LogicEntry ExitToCheck, LogicObjects.TrackerInstance lt)
        {
            return (x.Available && !x.IsFake && !x.IsWarpSong() && (x.IsEntrance() || lt.Options.IncludeItemLocations) && NotSOT(lt, ExitToCheck, x));
        }

        public static void Findpath(
            LogicObjects.TrackerInstance Instance,
            List<LogicObjects.MapPoint> map, //A map of all available entrances from each exit as long as the result of that entrance is known
            List<LogicObjects.MapPoint> FullMap, //A map of all available exit from each entrance
            int startinglocation, //The ID of the last exit you came from
            int destination, //The ID of the entrance you want to reach
            List<int> ExitsSeen, //The IDs of each entrance you have seen in the current area as well as all surrounding areas
            List<int> ExitsSeenOriginal, //The same as exits seen but does not conatain entrance found during this execution of the function through the CheckExitValid fucntion
            List<LogicObjects.MapPoint> Path, //A list of exits you have taken to get to your current point
            bool InitialRun = false //Is this code being run from the original source (True) or from it's self (False)
            )
        {
            if (InitialRun) { paths = new List<List<LogicObjects.MapPoint>>(); }
            //There are no available exits from majoras lair, this however is used to lock inaccesable exits
            if (Instance.Logic[startinglocation].DictionaryName == "EntranceMajorasLairFromTheMoon") { return; }
            //Make a copy to edit and pass to the next funtion
            var ExitsSeenCopy = JsonConvert.DeserializeObject<List<int>>(JsonConvert.SerializeObject(ExitsSeen));
            //The path finder will use this to ignore seen entrances in areas with new entrances and seen entrances
            var ExitsSeenOriginalCopy = JsonConvert.DeserializeObject<List<int>>(JsonConvert.SerializeObject(ExitsSeen));
            //A list of exits available to take
            var validExits = new List<LogicObjects.MapPoint>();

            //If we can reach our destination from this point, add the path we took to get here to the valid paths list.
            if (FullMap.Find(x => x.CurrentExit == startinglocation && x.EntranceToTake == destination) != null) { paths.Add(Path); }

            //For each point in our trimmed map 
            foreach (var point in map.Where(x => x.CurrentExit == startinglocation))
            {
                /*The ExitsSeenOriginal check ensures that the exit we are taking has not been accessable from 
                from a previous exit we have actually been to.
                Then Check to see if taking this exit contains exits we haven't seen before.
                 */
                if (!ExitsSeenOriginal.Contains(point.EntranceToTake) && CheckExitValid(map, point.ResultingExit, ExitsSeenCopy))
                {
                    validExits.Add(point);
                    ExitsSeen.Add(point.EntranceToTake);
                }
            }
            //Pick the first entrance in the valid exits list, rerun the function with the resulting exit as the starting location
            foreach (var exit in validExits)
            {
                var UpdatedPath = JsonConvert.DeserializeObject<List<LogicObjects.MapPoint>>(JsonConvert.SerializeObject(Path));
                UpdatedPath.Add(exit);
                Findpath(Instance, map, FullMap, exit.ResultingExit, destination, ExitsSeenCopy, ExitsSeenOriginalCopy, UpdatedPath);
            }
        }

        public static bool CheckExitValid(List<LogicObjects.MapPoint> map, int startinglocation, List<int> ExitsSeen)
        {
            /* This checks all of the exits in the area we would go to if we took this exit. If we have seen all of these exits
            Before or there is no need to take this exit.
            */
            var good = false;
            foreach (var point in map.Where(x => x.CurrentExit == startinglocation))
            {
                if (!ExitsSeen.Contains(point.EntranceToTake))
                {
                    good = true;
                    ExitsSeen.Add(point.EntranceToTake);
                }
            }
            return good;
        }

        private static bool NotSOT(LogicObjects.TrackerInstance Instance, LogicObjects.LogicEntry EntranceToCheck, LogicObjects.LogicEntry dummyEntry)
        {
            //true = Not Song of Time, false = Song of Time
            if (Instance.Options.UseSongOfTime) { return true; }
            var songOfTime = "EntranceSouthClockTownFromClockTowerInterior";
            if (dummyEntry.DictionaryName == songOfTime)
            {
                if (EntranceToCheck.DictionaryName == "EntranceClockTowerInteriorFromBeforethePortaltoTermina" || EntranceToCheck.DictionaryName == "EntranceClockTowerInteriorFromSouthClockTown")
                {
                    return true;
                }
                else { return false; }
            }
            return true;
        }

        public static string SetSOTName(LogicObjects.TrackerInstance Instance, LogicObjects.MapPoint stop)
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
    }
}
