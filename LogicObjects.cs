using Newtonsoft.Json;
using System.Collections.Generic;

namespace MMR_Tracker_V2
{
    class LogicObjects
    {
        public static List<LogicObjects.LogicDic> MMRDictionary = new List<LogicDic>();
        public static Dictionary<int, int> EntrancePairs = new Dictionary<int, int>();
        public static Dictionary<string, int> DicNameToID = new Dictionary<string, int>();
        public static List<LogicObjects.LogicEntry> Logic = new List<LogicObjects.LogicEntry>();
        public static LogicObjects.LogicEntry CurrentSelectedItem = new LogicObjects.LogicEntry();
        public static List<string> RawLogicText = new List<string>();
        public static List<LogicObjects.LogicEntry> selectedItems = new List<LogicObjects.LogicEntry>();

        public class LogicDic
        {
            public string DictionaryName { get; set; } //The name the logic file uses for the item
            public string LocationName { get; set; } //The name that will be displayed as the location you check
            public string ItemName { get; set; } //The name that will be displayed as the item you recieve
            public string LocationArea { get; set; } //The General Area the location is in
            public string ItemSubType { get; set; } //The type of item it is
            public string SpoilerLocation { get; set; } //The name of this location in the spoiler Log
            public string SpoilerItem { get; set; } //The name of this item in the spoiler log
            public string DisplayName { get; set; } //The value that is displayed if this object is displayed as a string
            public override string ToString()
            {
                return DisplayName;
            }
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
            public int RandomizedState { get; set; } //Whether or not the location is randomized, unrandomized or forced Junk
            public bool StartingItem { get; set; } //Whether or not the item is given at the start of the game
            public string LocationArea { get; set; } //The General Area the location is in
            public string ItemSubType { get; set; } //The type of item it is
            public string SpoilerLocation { get; set; } //The name of this location in the spoiler Log
            public string SpoilerItem { get; set; } //The name of this item in the spoiler log
            public int SpoilerRandom { get; set; } //The item the spoiler log says is in this location
            public string DisplayName { get; set; } //The value that is displayed if this object is displayed as a string
            public override string ToString()
            {
                return DisplayName;
            }
        }

        public class Map
        {
            public int CurrentExit { get; set; } //The exit you are curretly at
            public int Entrance { get; set; } //The entrance you can go through
            public int ResultingExit { get; set; } //The resulting exit you will end up at
            public bool isOwlWarp { get; set; } //Is the entrance an owl warp
            public string DisplayName { get; set; } //The value that is displayed if this object is displayed as a string
            public override string ToString()
            {
                return DisplayName;
            }
        }
        public class SpoilerData
        {
            public int LocationID { get; set; } 
            public string LocationName { get; set; } 
            public int ItemID { get; set; } 
            public string ItemName { get; set; } 
            public string LocationArea { get; set; } 
            public string DisplayName { get; set; } 
            public override string ToString()
            {
                return DisplayName;
            }
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
        public class sphere
        {
            public int sphereNumber { get; set; }
            public LogicEntry Check { get; set; }
            public List<int> ItemsUsed { get; set; }
        }
    }
}
