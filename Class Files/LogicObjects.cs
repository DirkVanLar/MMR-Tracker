using MMR_Tracker.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Controls;

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
            public bool JsonLogic { get; set; } = false;
            public List<List<LogicEntry>> UndoList { get; set; } = new List<List<LogicEntry>>();
            public List<List<LogicEntry>> RedoList { get; set; } = new List<List<LogicEntry>>();
            public Dictionary<string, int> WalletDictionary { get; set; } = new Dictionary<string, int>();
            public SavedSpoilerLog CurrentSpoilerLog { get; set; } = new SavedSpoilerLog { Log = null, type = null };
        }

        public class Options
        {
            //Logic Options
            public bool StrictLogicHandeling { get; set; } = false;
            public bool RemoveObtainedItemsfromList { get; set; } = true;
            public bool ProgressiveItems { get; set; } = false;
            public bool BringYourOwnAmmo { get; set; } = false;
            public bool RemoveUselessLogic { get; set; } = true;    
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
            public bool HorizontalLayout { get; set; } = false;
            public string BomberCode { get; set; } = "";
            public string LotteryNumber { get; set; } = "";
            public Font FormFont { get; set; } = SystemFonts.DefaultFont;
            //Misc Options
            public bool CheckForUpdate { get; set; } = true;
            public bool AutoSave { get; set; } = false;
            //NetOptions
            public int MyPlayerID { get; set; } = -1;
            public bool IsMultiWorld { get; set; } = false;
            public bool AllowCheckingItems { get; set; } = false;
            public bool AutoAddIncomingConnections { get; set; } = false;
            public bool StrictIP { get; set; } = false;
            public int PortNumber { get; set; } = 2112;
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
            public List<string> SpoilerLocation { get; set; } = new List<string>(); //The name of this location in the spoiler Log
            public List<string> SpoilerItem { get; set; } = new List<string>(); //The name of this item in the spoiler log
            public int SpoilerRandom { get; set; } //The item the spoiler log says is in this location
            public int AvailableOn { get; set; } //When the Check is available
            public int NeededBy { get; set; } //When the item is Needed
            public int TimeSetup { get; set; } //Idk what this is, I think the randomizer uses it for advanced song of time/ocarina logic
            public bool IsTrick { get; set; } //Whether or not the entry is a trick
            public bool TrickEnabled { get; set; } //Whether or not the trick is enabled
            public string TrickToolTip { get; set; } //The tool tip describing what the trick is
            public string GossipHint { get; set; } = ""; //The text assigned to this gossip stone. Only applicable if the check is a gossip stone.
            public int Price { get; set; } = -1; //The price to purchase the item at a shop, used in Price Randomizer.
            public bool LogicWasEdited { get; set; } = false; //Used to track if edits were made to the logic of this item.
            public PlayerData PlayerData { get; set; } = new PlayerData(); //Data for multiworld
            public string DisplayName { get; set; } //The value that is displayed if this object is displayed as a string
            public override string ToString()
            {
                return DisplayName;
            }
        }

        public class PlayerData
        {
            public int ItemBelongedToPlayer { get; set; } = -1; //(Future proofing for multi world) What player the item at this check belonged to
            public int ItemCameFromPlayer { get; set; } = -1; //(Future proofing for multi world) What the player this item came from
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
            public string EntrancePair { get; set; } //The Paired entrance for this entry
        }

        public class Configuration
        {
            public GameplaySettings GameplaySettings { get; set; }
        }

        public class GameplaySettings
        {
            public bool UseCustomItemList { get; set; } = true;
            public string[] CategoriesRandomized { get; set; } = null;
            public bool AddDungeonItems { get; set; } = false;
            public bool AddShopItems { get; set; } = false;
            public bool AddMoonItems { get; set; } = false;
            public bool AddFairyRewards { get; set; } = false;
            public bool AddOther { get; set; } = false;
            public bool AddNutChest { get; set; } = false;
            public bool CrazyStartingItems { get; set; } = false;
            public bool AddCowMilk { get; set; } = false;
            public bool AddSkulltulaTokens { get; set; } = false;
            public bool AddStrayFairies { get; set; } = false;
            public bool AddMundaneRewards { get; set; } = false;
            public bool RandomizeBottleCatchContents { get; set; } = false;
            public bool ExcludeSongOfSoaring { get; set; } = false;
            public bool RandomizeDungeonEntrances { get; set; } = false;
            public bool NoStartingItems { get; set; } = false;
            public string StartingItemMode { get; set; } = "";
            public string Logic { get; set; } = "";
            public bool AddSongs { get; set; } = false;
            public bool ProgressiveUpgrades { get; set; } = false;
            public bool ByoAmmo { get; set; } = false;
            public bool DecoupleEntrances { get; set; } = false;
            public string LogicMode { get; set; } = "Casual";
            public string UserLogicFileName { get; set; } = "";
            public string CustomItemListString { get; set; } = "";
            public string RandomizedEntrancesString { get; set; } = "";
            public string CustomJunkLocationsString { get; set; } = "";
            public string CustomStartingItemListString { get; set; } = "";
            public string GossipHintStyle { get; set; } = "Default";
            public List<int> EnabledTricks { get; set; } = new List<int>();
        }
        public class MapPoint
        {
            public int CurrentExit { get; set; } //The exit you are curretly at
            public int EntranceToTake { get; set; } //The entrance you can go through
            public int ResultingExit { get; set; } //The resulting exit you will end up at
        }
        public class SavedSpoilerLog
        {
            public string type { get; set; } = null;
            public string[] Log { get; set; } = null;
        }
        public class SpoilerLogData
        {
            public List<LogicObjects.SpoilerData> SpoilerDatas { get; set; } = new List<SpoilerData>();
            public Dictionary<string, int> Pricedata { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, string> GossipHints { get; set; } = new Dictionary<string, string>();
            public LogicObjects.GameplaySettings SettingString { get; set; } = new LogicObjects.GameplaySettings();
        }
        public class SpoilerData
        {
            public int LocationID { get; set; }
            public string LocationName { get; set; }
            public int ItemID { get; set; }
            public string ItemName { get; set; }
            public string LocationArea { get; set; }
            public int BelongsTo { get; set; } = -1;
    }
        public class ListItem
        {
            public LogicObjects.LogicEntry LocationEntry { get; set; }
            public LogicObjects.LogicEntry ItemEntry { get; set; }
            public string Header { get; set; }
            public int PathID { get; set; }
            public int PathPartition { get; set; }
            public int Container { get; set; }
            public Font Font { get; set; }
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
        public class ItemUnlockData
        {
            public List<LogicObjects.PlaythroughItem> Playthrough { get; set; } = new List<LogicObjects.PlaythroughItem>();
            public List<int> ResolvedRealItems { get; set; } = new List<int>();
            public List<int> FakeItems { get; set; } = new List<int>();
            public List<int> UsedItems { get; set; } = new List<int>();
        }
        public class NetData
        {
            public int ID { get; set; } //Check ID
            public int PI { get; set; } //Player ID (Which player the item at this check belongs to)
            public int RI { get; set; } //Check Randomized Item
            public bool Ch { get; set; } //Whether the check is checked
        }
        public class IPDATA
        {
            public IPAddress IP { get; set; }
            public int PORT { get; set; }
            public string DisplayName { get; set; }
            public override string ToString()
            {
                return DisplayName;
            }
        }
        public class IPDATASerializable
        {
            public string IP { get; set; }
            public int PORT { get; set; }
            public string DisplayName { get; set; }
        }
        public class MMRTpacket
        {
            public int PlayerID { get; set; }
            public IPDATASerializable IPData { get; set; } = new IPDATASerializable();
            public int RequestingUpdate { get; set; } = 0; //0= Sending Only, 1= Requesting Only, 2 = Both
            public List<LogicObjects.NetData> LogicData { get; set; }

        }
        public class JsonFormatLogicItem
        {
            public string Id { get; set; }
            public List<string> RequiredItems { get; set; } = new List<string>();
            public List<List<string>> ConditionalItems { get; set; } = new List<List<string>>();
            public TimeOfDay TimeNeeded { get; set; }
            public TimeOfDay TimeAvailable { get; set; }
            public TimeOfDay TimeSetup { get; set; }
            public bool IsTrick { get; set; }

            private string _trickTooltip;
            public string TrickTooltip
            {
                get
                {
                    return IsTrick ? _trickTooltip : null;
                }
                set
                {
                    _trickTooltip = value;
                }
            }
        }

        [Flags]
        public enum TimeOfDay
        {
            None,
            Day1 = 1,
            Night1 = 2,
            Day2 = 4,
            Night2 = 8,
            Day3 = 16,
            Night3 = 32,
        }
        public class LogicFile
        {
            public int Version { get; set; }
            public List<JsonFormatLogicItem> Logic { get; set; }

            public override string ToString()
            {
                return JsonSerializer.Serialize(this, _jsonSerializerOptions);
            }

            public static LogicFile FromJson(string json)
            {
                return JsonSerializer.Deserialize<LogicFile>(json, _jsonSerializerOptions);
            }

            private readonly static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
                AllowTrailingCommas = true,
                Converters =
                {
                    new JsonColorConverter(),
                    new JsonStringEnumConverter(),
                }
            };
            public class JsonColorConverter : JsonConverter<Color>
            {
                public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    // TODO: Can optimize this further by using ReadOnlySpan<char> to split without allocations.
                    var text = reader.GetString();
                    var tokens = text.Split(new string[] { ", " }, StringSplitOptions.None);
                    if (tokens.Length == 3)
                    {
                        // Assume color values.
                        var values = tokens.Select(x => byte.Parse(x)).ToArray();
                        return Color.FromArgb(values[0], values[1], values[2]);
                    }
                    else
                    {
                        // Assume color name.
                        return Color.FromName(text);
                    }
                }

                public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
                {
                    writer.WriteStringValue(string.Format("{0}, {1}, {2}", value.R, value.G, value.B));
                }
            }
        }
    }

}
