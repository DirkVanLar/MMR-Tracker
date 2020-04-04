using System.Collections.Generic;
using System.Linq;

namespace MMR_Tracker_V2
{
    public class LogicObjects
    {
        public static List<LogicObjects.LogicDictionaryEntry> MMRDictionary = new List<LogicDictionaryEntry>();
        public static Dictionary<int, int> EntrancePairs = new Dictionary<int, int>();
        public static Dictionary<string, int> DicNameToID = new Dictionary<string, int>();
        public static List<LogicObjects.LogicEntry> Logic = new List<LogicObjects.LogicEntry>();
        public static LogicObjects.LogicEntry CurrentSelectedItem = new LogicObjects.LogicEntry();
        public static List<LogicObjects.LogicEntry> selectedItems = new List<LogicObjects.LogicEntry>();

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
    }

    public static class Extentions
    {
        public static bool IsEntrance(this LogicObjects.LogicEntry entry)
        {
            return entry.ItemSubType == "Entrance";
        }
        public static LogicObjects.LogicEntry RandomizedEntry(this LogicObjects.LogicEntry entry, bool ReturnJunkAsItem = false, List<LogicObjects.LogicEntry> logic = null)
        {
            if (logic == null) { logic = LogicObjects.Logic; }
            if (ReturnJunkAsItem && entry.HasJunkRandomItem()) { return new LogicObjects.LogicEntry { ID = -1, DictionaryName = "Junk", DisplayName = "Junk", LocationName = "Junk", ItemName = "Junk" }; }
            if (!entry.HasRealRandomItem() || entry.RandomizedItem >= logic.Count) { return null; }
            return logic[entry.RandomizedItem];
        }
        public static LogicObjects.LogicEntry PairedEntry(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> logic, bool RandomizedItem = false, Dictionary<int, int> Pairs = null)
        {
            if (Pairs == null) { Pairs = LogicObjects.EntrancePairs; }
            int ID = (RandomizedItem) ? entry.RandomizedItem : entry.ID;
            if (Pairs.ContainsKey(ID) && Pairs[ID] < logic.Count) { return logic[Pairs[ID]]; }
            return null;
        }
        public static LogicObjects.LogicEntry RandomizedAreaClear(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> logic, Dictionary<int, int> EntAreaDict)
        {
            if (!EntAreaDict.ContainsKey(entry.ID)) { return null; }
            var templeEntrance = EntAreaDict[entry.ID];//What is the dungeon entrance in this area
            var RandTempleEntrance = logic[templeEntrance].RandomizedItem;//What dungeon does this areas dungeon entrance lead to
            var RandAreaClear = RandTempleEntrance < 0 ? -1 : EntAreaDict.FirstOrDefault(x => x.Value == RandTempleEntrance).Key;//What is the Area clear Value For That Dungeon
            var RandClearLogic = RandAreaClear == -1 ? null : logic[RandAreaClear]; //Get the full logic data for the area clear that we want to check the availability of.
            return RandClearLogic;
        }
        public static bool HasRandomItem(this LogicObjects.LogicEntry entry)
        {
            return entry.RandomizedItem > -2;
        }
        public static bool HasRealRandomItem(this LogicObjects.LogicEntry entry)
        {
            return entry.RandomizedItem > -1;
        }
        public static bool HasJunkRandomItem(this LogicObjects.LogicEntry entry)
        {
            return entry.RandomizedItem == -1;
        }
        public static bool Unrandomized(this LogicObjects.LogicEntry entry, int UnRand0Manual1Either2 = 0)
        {
            if (UnRand0Manual1Either2 == 0) { return entry.Options == 1; }
            if (UnRand0Manual1Either2 == 1) { return entry.Options == 2; }
            if (UnRand0Manual1Either2 == 0) { return entry.Options == 1 || entry.Options == 2; }
            return false;
        }
        public static bool Randomized(this LogicObjects.LogicEntry entry)
        {
            return entry.Options == 0;
        }
        public static bool ForceJunk(this LogicObjects.LogicEntry entry)
        {
            return entry.Options == 3;
        }
        public static int RandomizedState(this LogicObjects.LogicEntry entry)
        {
            return (entry.Options > 3) ? entry.Options - 4 : entry.Options;
        }
        public static bool StartingItem(this LogicObjects.LogicEntry entry)
        {
            return (entry.Options > 3);
        }
    }
}
