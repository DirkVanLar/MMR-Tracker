using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MMR_Tracker_V2
{
    public class LogicObjects
    {
        public static TrackerInstance MainTrackerInstance = new TrackerInstance();

        public class TrackerInstance
        {
            public List<LogicEntry> Logic { get; set; } = new List<LogicEntry>();
            public List<LogicDictionaryEntry> LogicDictionary { get; set; } = new List<LogicDictionaryEntry>();
            public Dictionary<int, int> EntrancePairs { get; set; } = new Dictionary<int, int>();
            public Dictionary<string, int> DicNameToID { get; set; } = new Dictionary<string, int>();
            public Dictionary<int, int> EntranceAreaDic { get; set; } = new Dictionary<int, int>();
            public Options Options { get; set; } = new Options();
            public int LogicVersion { get; set; } = 0;
            public string GameCode { get; set; } = "MMR";
            public string[] RawLogicFile { get; set; }
            public bool UnsavedChanges { get; set; } = false;
            public bool EntranceRando { get; set; } = false;
            public List<List<LogicEntry>> UndoList { get; set; } = new List<List<LogicEntry>>();
            public List<List<LogicEntry>> RedoList { get; set; } = new List<List<LogicEntry>>();
        }

        public class Options
        {
            //Logic Options
            public bool StrictLogicHandeling { get; set; } = false;
            //Entrance rando Options
            public bool EntranceRadnoEnabled { get; set; } = false;
            public bool OverRideAutoEntranceRandoEnable { get; set; } = false;
            public bool CoupleEntrances { get; set; } = true;
            public bool UnradnomizeEntranesOnStartup { get; set; } = true;
            public bool UseSongOfTime { get; set; } = false;
            public bool IncludeItemLocations { get; set; } = false;
            //UI Options
            public bool MoveMarkedToBottom { get; set; } = false;
            public bool ShowEntryNameTooltip { get; set; } = true;
            public bool MiddleClickStarNotMark { get; set; } = false;
            public string BomberCode { get; set; } = "";
            public string LotteryNumber { get; set; } = "";
            public Font FormFont { get; set; } = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);
            //Misc Options
            public bool CheckForUpdate { get; set; } = true;
        }
        public class LogicDictionaryEntry
        {
            public string DictionaryName { get; set; } //The name the logic file uses for the item
            public string LocationName { get; set; } //The name that will be displayed as the location you check
            public string ItemName { get; set; } //The name that will be displayed as the item you recieve
            public string LocationArea { get; set; } //The General Area the location is in
            public string ItemSubType { get; set; } //The type of item it is
            public string SpoilerLocation { get; set; } //The name of this location in the spoiler Log
            public string SpoilerItem { get; set; } //The name of this item in the spoiler log
        }
        public class LogicEntry
        {
            public int ID { get; set; } //The id of the item. Will match the id used in the Logic file
            public string DictionaryName { get; set; } //The name the logic file uses for the item
            public string LocationName { get; set; } //The name that will be displayed as the location you check
            public string ItemName { get; set; } //The name that will be displayed as the item you recieve
            public int[] Required { get; set; } //An array of the items required to check this location
            public int[][] Conditionals { get; set; } //A 2d array of each set of conditionals required to check this location
            public bool Available { get; set; } //Whether or not the location is available to be checked
            public bool Aquired { get; set; } //Whether or not the item is aquired
            public bool Checked { get; set; } //Whether or not the location has been checked
            public int RandomizedItem { get; set; } //The random Item that was placed at the location
            public bool IsFake { get; set; } //Whether or not the entry is a logic shortcut aka "Fake Item"
            public int Options { get; set; } //Whether or not the location is randomized, unrandomized or forced Junk and whether or not it's a starting Item
            public bool Starred { get; set; } = false; //Whether the check has been starred
            public string LocationArea { get; set; } //The General Area the location is in
            public string ItemSubType { get; set; } //The type of item it is
            public string SpoilerLocation { get; set; } //The name of this location in the spoiler Log
            public string SpoilerItem { get; set; } //The name of this item in the spoiler log
            public int SpoilerRandom { get; set; } //The item the spoiler log says is in this location //The name of this location in the spoiler Log
            public int AvailableOn { get; set; } //When the Check is available
            public int NeededBy { get; set; } //When the item is Needed
            public string DisplayName { get; set; } //The value that is displayed if this object is displayed as a string
            public override string ToString()
            {
                return DisplayName;
            }
        }

        public class MapPoint
        {
            public int CurrentExit { get; set; } //The exit you are curretly at
            public int EntranceToTake { get; set; } //The entrance you can go through
            public int ResultingExit { get; set; } //The resulting exit you will end up at
        }
        public class SpoilerData
        {
            public int LocationID { get; set; }
            public string LocationName { get; set; }
            public int ItemID { get; set; }
            public string ItemName { get; set; }
            public string LocationArea { get; set; }
        }
        public class ListItem
        {
            public int ID { get; set; }
            public int Identifier { get; set; }
            public string DisplayName { get; set; }
            public override string ToString()
            {
                return DisplayName;
            }
        }
        public class PlaythroughItem
        {
            public int SphereNumber { get; set; }
            public LogicEntry Check { get; set; }
            public List<int> ItemsUsed { get; set; }
        }
        public class VersionInfo
        {
            public int Version { get; set; }
            public string Gamecode { get; set; }
        }
    }

    public static class Extentions
    {
        public static bool IsEntrance(this LogicObjects.LogicEntry entry)
        {
            return entry.ItemSubType == "Entrance";
        }
        public static LogicObjects.LogicEntry RandomizedEntry(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool ReturnJunkAsItem = false )
        {
            if (ReturnJunkAsItem && entry.HasRandomItem(false) && !entry.HasRandomItem(true)) 
            { return new LogicObjects.LogicEntry { ID = -1, DictionaryName = "Junk", DisplayName = "Junk", LocationName = "Junk", ItemName = "Junk" }; }
            if (!entry.HasRandomItem(true) || entry.RandomizedItem >= Instance.Logic.Count) { return null; }
            return Instance.Logic[entry.RandomizedItem];
        }
        public static LogicObjects.LogicEntry PairedEntry(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool RandomizedItem = false)
        {
            var Pairs = Instance.EntrancePairs;
            int ID = (RandomizedItem) ? entry.RandomizedItem : entry.ID;
            if (Pairs.ContainsKey(ID) && Pairs[ID] < Instance.Logic.Count) { return Instance.Logic[Pairs[ID]]; }
            return null;
        }
        public static LogicObjects.LogicEntry RandomizedAreaClear(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            //Finds the area clear related to the dungeon that is randomized to the current area.
            //If woodfall entrance leads to snowhead and you pass this function woodfall clear it will return snowhead clear.
            if (!Instance.EntranceAreaDic.ContainsKey(entry.ID)) { return null; }
            var templeEntrance = Instance.EntranceAreaDic[entry.ID];//What is the dungeon entrance in this area
            var RandTempleEntrance = Instance.Logic[templeEntrance].RandomizedItem;//What dungeon does this areas dungeon entrance lead to
            var RandAreaClear = RandTempleEntrance < 0 ? -1 : Instance.EntranceAreaDic.FirstOrDefault(x => x.Value == RandTempleEntrance).Key;//What is the Area clear Value For That Dungeon
            var RandClearLogic = RandAreaClear == -1 ? null : Instance.Logic[RandAreaClear]; //Get the full logic data for the area clear that we want to check the availability of.
            return RandClearLogic;
        }
        public static bool HasRandomItem(this LogicObjects.LogicEntry entry, bool RealItem)
        {
            return (RealItem) ? entry.RandomizedItem > -1 : entry.RandomizedItem > -2;
        }
        public static bool Unrandomized(this LogicObjects.LogicEntry entry, int UnRand0Manual1Either2 = 0)
        {
            if (UnRand0Manual1Either2 == 0) { return entry.RandomizedState() == 1; }
            if (UnRand0Manual1Either2 == 1) { return entry.RandomizedState() == 2; }
            if (UnRand0Manual1Either2 == 2) { return entry.RandomizedState() == 1 || entry.RandomizedState() == 2; }
            return false;
        }
        public static bool Randomized(this LogicObjects.LogicEntry entry)
        {
            return entry.RandomizedState() == 0;
        }
        public static int RandomizedState(this LogicObjects.LogicEntry entry)
        {
            int option = entry.Options;
            return (entry.Options > 3) ? option - 4 : option;
        }
        public static bool StartingItem(this LogicObjects.LogicEntry entry)
        {
            return (entry.Options > 3);
        }
        public static bool IsEntranceRando(this LogicObjects.TrackerInstance Instance)
        {
            return (Instance.Logic.Where(x => x.IsEntrance()).Count() > 0);
        }
        public static bool IsMM(this LogicObjects.TrackerInstance Instance)
        {
            return Instance.GameCode == "MMR";
        }
        public static bool IsWarpSong(this LogicObjects.LogicEntry entry)
        {
            return (entry.LocationArea == "Owl Warp");
        }
        public static bool AppearsInListbox(this LogicObjects.LogicEntry entry)
        {
            return (entry.Randomized() || entry.Unrandomized(1)) && !entry.IsFake;
        }
        public static bool Useable(this LogicObjects.LogicEntry entry)
        {
            return (entry.Aquired || entry.StartingItem() || (entry.Unrandomized() && entry.Available));
        }
        public static LogicObjects.LogicEntry GetItemsNewLocation(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> Logic)
        {
            return Logic.Find(x => x.RandomizedItem == entry.ID);
        }
    }
}
