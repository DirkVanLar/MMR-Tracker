using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms;
using MMR_Tracker.Forms.Sub_Forms;
using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Other_Games
{
    class OcarinaOfTimeRando
    {
        public static string[] IgnoredJunk = new string[]
            {
                "Rupees (",
                "Rupee (",
                "Deku Seeds (",
                "Deku Nuts (",
                "Buy Bombs (",
                "Bombs (",
                "Buy Arrows (",
                "Arrows (",
                "Deku Stick (",
                "Deku Sticks (",
                "Ice Trap",
                "Buy Fairy",
                "Buy Green Potion",
                "Buy Heart",

            };
        public static List<string> allBottles = new List<string>() 
        { 
            "Bottle with Red Potion", 
            "Bottle with Green Potion", 
            "Bottle with Blue Potion", 
            "Bottle with Fairy", 
            "Bottle with Fish", 
            "Bottle with Blue Fire", 
            "Bottle with Bugs", 
            "Bottle with Big Poe", 
            "Bottle with Poe", 
            "Bottle", 
            "Bottle with Milk" 
        };
        public static List<string> Dungeons = new List<string>()
            {
                "Bottom of the Well",
                "Deku Tree",
                "Dodongos Cavern",
                "Fire Temple",
                "Forest Temple",
                "Ganons Castle",
                "Gerudo Training Ground",
                "Ice Cavern",
                "Jabu Jabus Belly",
                "Shadow Temple",
                "Spirit Temple",
                "Water Temple"
            };
        public class OOTRLogicObject
        {
            public string region_name { get; set; } = "";
            public string Dungeon { get; set; } = "";
            public bool MQ { get; set; } = false;
            public Dictionary<string, string> events { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> locations { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> exits { get; set; } = new Dictionary<string, string>();
        }

        public class SpoilerLog
        {
            public Dictionary<string, dynamic> settings = new Dictionary<string, dynamic>();
            public Dictionary<string, dynamic> randomized_settings = new Dictionary<string, dynamic>();
            public Dictionary<string, string> dungeons = new Dictionary<string, string>();
            public Dictionary<string, dynamic> entrances = new Dictionary<string, dynamic>();
            public Dictionary<string, dynamic> locations = new Dictionary<string, dynamic>();
            public Dictionary<string, int> item_pool = new Dictionary<string, int>();
            public Dictionary<string, string> trials = new Dictionary<string, string>();
            public Dictionary<string, int> starting_items = new Dictionary<string, int>();
            public Dictionary<string, OOTRGossip> gossip_stones = new Dictionary<string, OOTRGossip>();
        }

        public class OOTRGossip
        {
            public string text = "";
            public string[] colors = null;
        }

        public class RegionExit
        {
            public string region = "";
            public string from = "";
        }
        public class SpoilerLogPriceData
        {
            public string item = "";
            public int price = 0;
        }

        //Create Function
        public static void CreateOOTRLogic()
        {
            LogicObjects.LogicFile Logic = ReadOotrLogic();
            LogicObjects.LogicDictionary dict = ReadOOTRRefSheet();
            AddMissingItems(dict, Logic);
            AddExtraItems(dict, Logic);
            CleanIllegalChar(Logic);
            AddTricks(Logic);
            AddReverseOptionEntries(Logic);
            AddTempleLayoutTracking(dict, Logic);
            AddRandoOptions(dict, Logic);
            AddEndgameRequirementOptions(dict, Logic);
            AddXofYentries(dict, Logic);

            //FindAmbiguosLogicNames(dict, Logic);
            ConvertItemnamesinLogicToDictname(dict, Logic);
            //CheckFormissingLogicItems(dict, Logic);

            //Ganons castle boss key and gerudo keys are not part of kysy since they are their own setting.
            dict.LogicDictionaryList.Find(x => x.DictionaryName == "Ganons Tower Boss Key Chest").KeyType = null;
            var GerudoKeys = dict.LogicDictionaryList.Where(x => x.ItemName == "Small Key Thieves Hideout");
            foreach(var i in GerudoKeys)
            {
                i.KeyType = null;
            }

            //Convert the Master sword pedestal entry to a real item
            //It is already initialized as a fake item in the dictionary, so logic knows to map "Time_Travel" logic entries to it
            //Its always unrandomized, but we ned it to be a real item so once we have it (along with the ability to change ages)
            //The pathfinder will always consider it available when finding entrance connections.
            //Otherwise we would only be able to find entrance paths that our starting age could access.
            var MasterSwordItem = dict.LogicDictionaryList.Find(x => x.DictionaryName == "Master Sword Pedestal");
            MasterSwordItem.LocationName = "Master Sword Pedestal";
            MasterSwordItem.ItemName = "Master Sword";
            MasterSwordItem.LocationArea = "Temple of Time";
            MasterSwordItem.ItemSubType = "Master Sword Pedestal";
            MasterSwordItem.FakeItem = false;

            //FinalLogicCheck(Logic);
            JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };



            string[] LogicText = JsonConvert.SerializeObject(Logic, _jsonSerializerOptions).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            LogicObjects.MainTrackerInstance.RawLogicFile = LogicText;
            LogicObjects.MainTrackerInstance.LogicDictionary = dict;
            LogicEditing.PopulateTrackerInstance(LogicObjects.MainTrackerInstance);
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);

            MainInterface.CurrentProgram.FormatMenuItems();
            MainInterface.CurrentProgram.ResizeObject();
            MainInterface.CurrentProgram.PrintToListBox();
            Tools.UpdateTrackerTitle();
            Tools.SaveFilePath = "";

            foreach(var i in LogicObjects.MainTrackerInstance.Logic)
            {
                LogicEditor.CleanLogicEntry(i, LogicObjects.MainTrackerInstance);
                i.Required = LogicEditing.removeItemFromRequirement(i.Required, new int[] { LogicObjects.MainTrackerInstance.DicNameToID["True"] });
            }

            DoFinalLogicCleanup(LogicObjects.MainTrackerInstance.Logic);


            File.WriteAllLines("Ocarina of Time Rando (Beta).txt", LogicEditing.WriteLogicToJson(LogicObjects.MainTrackerInstance));
            File.WriteAllText("OOTR V1 json Logic Dictionary.json", JsonConvert.SerializeObject(LogicObjects.MainTrackerInstance.LogicDictionary, _jsonSerializerOptions));
            Process.Start(Directory.GetCurrentDirectory());


            return;


            Console.WriteLine($"Logic File contained {Logic.Logic.Count()} Entries");

        }

        //Read Data off the OOTR Github
        public static LogicObjects.LogicFile ReadOotrLogic()
        {
            var Logic = new List<OOTRLogicObject>();

            GetDataFromWeb(Logic, "Roman971/OoT-Randomizer/Dev-R");

            var ParsedLogic = new List<OOTRLogicObject>();

            Dictionary<string, List<string>> RegionLogic = new Dictionary<string, List<string>>();

            foreach (var i in Logic)
            {
                if (!RegionLogic.ContainsKey(i.region_name)) { RegionLogic.Add(i.region_name, new List<string>()); }
                var DungeonState = i.MQ ? "Master Quest" : "Vanilla";
                OOTRLogicObject NewLogicEntry = new OOTRLogicObject() { region_name = i.region_name };
                foreach (var j in i.locations)
                {
                    string LogicLine = CleanLogicEntry(j.Value).Trim();
                    LogicLine = i.region_name + $" & ({LogicLine})";
                    if (i.Dungeon != "") { LogicLine = $"{i.Dungeon} {DungeonState} & ({LogicLine})"; }
                    NewLogicEntry.locations.Add(j.Key, LogicLine);
                }
                foreach (var j in i.events)
                {
                    string[] BadLogicEntries = new string[] { "can_play(song)", "can_use(item)", "_is_magic_item(item)", "_is_adult_item(item)", "_is_child_item(item)", "_is_magic_arrow(item)", "has_projectile(for_age)", "guarantee_hint", "guarantee_trade_path" };
                    if (BadLogicEntries.Contains(j.Key)) { continue; }
                    string LogicLine = CleanLogicEntry(j.Value).Trim();
                    LogicLine = (i.region_name == "Logic Helper") ? LogicLine : i.region_name + $" & ({LogicLine})";
                    if (i.Dungeon != "") { LogicLine = $"{i.Dungeon} {DungeonState} & ({LogicLine})"; }
                    NewLogicEntry.events.Add(j.Key.Replace(" & ", " "), LogicLine);
                }
                foreach (var j in i.exits)
                {
                    string LogicLine = CleanLogicEntry(j.Value).Trim();
                    LogicLine = i.region_name + $" & ({LogicLine})";
                    if (i.Dungeon != "") { LogicLine = $"{i.Dungeon} {DungeonState} & ({LogicLine})"; }
                    NewLogicEntry.exits.Add($"{i.region_name} -> {j.Key}", LogicLine);
                }
                if (!NewLogicEntry.locations.Any()) { NewLogicEntry.locations = null; }
                if (!NewLogicEntry.events.Any()) { NewLogicEntry.events = null; }
                if (!NewLogicEntry.exits.Any()) { NewLogicEntry.exits = null; }
                ParsedLogic.Add(NewLogicEntry);
            }

            Dictionary<string, string> MasterLogic = new Dictionary<string, string>();

            foreach (var i in ParsedLogic)
            {
                foreach (var j in i.locations ?? new Dictionary<string, string>())
                {
                    if (!MasterLogic.ContainsKey(j.Key)) { MasterLogic.Add(j.Key, $"({j.Value})"); }
                    else { MasterLogic[j.Key] += $" | ({j.Value})"; }
                }
                foreach (var j in i.events ?? new Dictionary<string, string>())
                {
                    if (!MasterLogic.ContainsKey(j.Key)) { MasterLogic.Add(j.Key, $"({j.Value})"); }
                    else { MasterLogic[j.Key] += $" | ({j.Value})"; }
                }
                foreach (var j in i.exits ?? new Dictionary<string, string>())
                {
                    if (!MasterLogic.ContainsKey(j.Key)) { MasterLogic.Add(j.Key, $"({j.Value})"); }
                    else { MasterLogic[j.Key] += $" | ({j.Value})"; }
                }
            }

            foreach (var Region in RegionLogic.Keys.ToArray())
            {
                foreach (var i in MasterLogic)
                {
                    if (i.Key.Contains("->"))
                    {
                        var Data = i.Key.Split(new string[] { "->" }, StringSplitOptions.None);
                        if (Data[1].Trim() == Region && !RegionLogic[Region].Contains(i.Key)) { RegionLogic[Region].Add(i.Key); }
                    }
                }
            }

            foreach (var Region in RegionLogic)
            {
                MasterLogic.Add(Region.Key, string.Join(" | ", Region.Value));
            }

            //Add logic that is only needed for seed generation and can be ignored in the tracker
            MasterLogic.Add("True", "");
            MasterLogic.Add("at_dampe_time", "");
            MasterLogic.Add("at_day", "");
            MasterLogic.Add("at_night", "");
            MasterLogic.Add("had_night_start", "");
            MasterLogic.Add("disable_trade_revert", "");
            MasterLogic.Add("age == starting_age", "");
            MasterLogic.Add("guarantee_trade_path", "");
            MasterLogic.Add("not entrance_shuffle", "");

            LogicObjects.LogicFile FormatedLogic = new LogicObjects.LogicFile();
            FormatedLogic.GameCode = "OOTR";
            FormatedLogic.Version = 1;
            FormatedLogic.Logic = new List<LogicObjects.JsonFormatLogicItem>();

            LogicParser Parser = new LogicParser();

            foreach (var i in MasterLogic)
            {
                FormatedLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem() { Id = i.Key, ConditionalItems = Parser.ConvertLogicToConditionalString(i.Value) });
            }

            return FormatedLogic;
            //File.WriteAllText("OOTRLogic.json", JsonConvert.SerializeObject(FormatedLogic, new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }));

        }

        public static LogicObjects.LogicDictionary ReadOOTRRefSheet()
        {
            List<string[]> Entrances = new List<string[]>();
            List<string[]> Locations = new List<string[]>();
            List<string> GossipStones = new List<string>();
            List<string[]> FakeITems = new List<string[]>();


            string CurrentSection = "";
            string[] RefFile = File.ReadAllLines(@"lib\Other Games\OOTR Data.txt");
            foreach (var i in RefFile)
            {
                var Line = i.Trim();
                if (Line.StartsWith("SECTION:"))
                {
                    CurrentSection = Line.Replace("SECTION:", "").Trim();
                    continue;
                }
                if (string.IsNullOrWhiteSpace(Line) || Line.StartsWith("//") || Line.StartsWith(@"\\") || Line.StartsWith(@"#")) { continue; }
                var Data = Line.Split(',').Select(x => x.Trim().Replace("'", "").Replace(")", "").Replace("(", "").Replace("\"", ""));
                Data = Data.Where(x => !string.IsNullOrWhiteSpace(x));
                if (!Data.Any()) { continue; }

                if (CurrentSection == "Entrances") { Entrances.Add(Data.ToArray()); }
                if (CurrentSection == "Locations") { Locations.Add(Data.ToArray()); }
                if (CurrentSection == "GOSSIP") { GossipStones.Add(Data.ToArray()[0]); }
                if (CurrentSection == "FAKE") { FakeITems.Add(Data.ToArray()); }
            }

            LogicObjects.LogicDictionary OOTRDict = new LogicObjects.LogicDictionary()
            {
                DefaultWalletCapacity = 99,
                GameCode = "OOTR",
                LogicFormat = "json",
                LogicVersion = 1,
                LogicDictionaryList = new List<LogicObjects.LogicDictionaryEntry>()
            };
            foreach (var i in Entrances)
            {
                var Data = i[0].Split(new string[] { "->" }, StringSplitOptions.None);
                LogicObjects.LogicDictionaryEntry entry = new LogicObjects.LogicDictionaryEntry()
                {
                    DictionaryName = i[0],
                    EntrancePair = i[1] == "OneWay" ? null : i[1],
                    FakeItem = false,
                    GameClearDungeonEntrance = null,
                    GossipItem = null,
                    GossipLocation = null,
                    ItemName = $"{Data[1]} <- {Data[0]}",
                    ItemSubType = "Entrance",
                    ProgressiveItemData = null,
                    KeyType = null,
                    LocationArea = i[2],
                    LocationName = i[2] == "inaccessible" ? null : i[0],
                    RandoOnlyRequiredLogic = null,
                    ValidRandomizerStartingItem = false,
                    WalletCapacity = null,
                    SpoilerItem = new string[] { $"{Data[1]} <- {Data[0]}" },
                    SpoilerLocation = new string[] { i[0] }
                };
                OOTRDict.LogicDictionaryList.Add(entry);
            }
            foreach (var i in Locations)
            {
                var Data = i[0].Split(new string[] { "->" }, StringSplitOptions.None);
                LogicObjects.LogicDictionaryEntry entry = new LogicObjects.LogicDictionaryEntry()
                {
                    DictionaryName = i[0],
                    EntrancePair = null,
                    FakeItem = false,
                    GameClearDungeonEntrance = null,
                    GossipItem = null,
                    GossipLocation = null,
                    ItemName = i[1],
                    ItemSubType = "Item",
                    ProgressiveItemData = null,
                    KeyType = i[1].StartsWith("Small Key") ? "small" : (i[1].StartsWith("Boss Key") ? "boss" : null),
                    LocationArea = i[2],
                    LocationName = i[0],
                    RandoOnlyRequiredLogic = null,
                    ValidRandomizerStartingItem = false,
                    WalletCapacity = null,
                    SpoilerItem = new string[] { i[1], i[1].Replace(" ", "_") },
                    SpoilerLocation = new string[] { i[0] }
                };

                var result = Regex.Match(entry.ItemName, @"\d+$");
                if (result.Success)
                {
                    entry.SpoilerItem = entry.SpoilerItem.Concat(new string[] { entry.ItemName.Replace(result.Groups[0].Value, $"({result.Groups[0].Value})") }).ToArray();
                }
                if (entry.ItemName.StartsWith("Bombchus")) { entry.SpoilerItem = entry.SpoilerItem.Concat(new string[] { "Bombchus" }).ToArray(); }

                if (entry.ItemName.Contains("Treasure Chest Game"))
                {
                    entry.SpoilerItem = entry.SpoilerItem.Concat(new string[] { entry.ItemName.Replace("Treasure Chest Game", "(Treasure Chest Game)") }).ToArray();
                }

                OOTRDict.LogicDictionaryList.Add(entry);
            }
            foreach (var i in GossipStones)
            {
                LogicObjects.LogicDictionaryEntry entry = new LogicObjects.LogicDictionaryEntry()
                {
                    DictionaryName = i,
                    EntrancePair = null,
                    FakeItem = false,
                    GameClearDungeonEntrance = null,
                    GossipItem = null,
                    GossipLocation = null,
                    ItemName = i,
                    ItemSubType = "Gossip " + i,
                    ProgressiveItemData = null,
                    KeyType = null,
                    LocationArea = "Gossip Stones",
                    LocationName = i,
                    RandoOnlyRequiredLogic = null,
                    ValidRandomizerStartingItem = false,
                    WalletCapacity = null,
                    SpoilerItem = null,
                    SpoilerLocation = null
                };
                OOTRDict.LogicDictionaryList.Add(entry);
            }
            foreach (var i in FakeITems)
            {
                LogicObjects.LogicDictionaryEntry entry = new LogicObjects.LogicDictionaryEntry()
                {
                    DictionaryName = i[0],
                    EntrancePair = null,
                    FakeItem = true,
                    GameClearDungeonEntrance = null,
                    GossipItem = null,
                    GossipLocation = null,
                    ItemName = i[1].Replace(" ", "_"),
                    ItemSubType = null,
                    ProgressiveItemData = null,
                    KeyType = null,
                    LocationArea = null,
                    LocationName = null,
                    RandoOnlyRequiredLogic = null,
                    ValidRandomizerStartingItem = false,
                    WalletCapacity = null,
                    SpoilerItem = null,
                    SpoilerLocation = null
                };
                OOTRDict.LogicDictionaryList.Add(entry);
            }
            return OOTRDict;
            File.WriteAllText("OOTRDictionary.json", JsonConvert.SerializeObject(OOTRDict, new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }));

        }

        public static void GetDataFromWeb(List<OOTRLogicObject> Logic, string Branch = "TestRunnerSRL/OoT-Randomizer/Dev")
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            List<string> LogicFiles = new List<string>
            {
                $"https://raw.githubusercontent.com/{Branch}/data/World/Overworld.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Bottom%20of%20the%20Well.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Deku%20Tree.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Dodongos%20Cavern.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Fire%20Temple.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Forest%20Temple.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Ganons%20Castle.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Gerudo%20Training%20Ground.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Ice%20Cavern.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Jabu%20Jabus%20Belly.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Shadow%20Temple.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Spirit%20Temple.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Water%20Temple.json"

            };

            List<string> MQLogicFiles = new List<string>
            {
                $"https://raw.githubusercontent.com/{Branch}/data/World/Bottom%20of%20the%20Well%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Deku%20Tree%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Dodongos%20Cavern%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Fire%20Temple%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Forest%20Temple%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Ganons%20Castle%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Gerudo%20Training%20Ground%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Ice%20Cavern%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Jabu%20Jabus%20Belly%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Shadow%20Temple%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Spirit%20Temple%20MQ.json",
                $"https://raw.githubusercontent.com/{Branch}/data/World/Water%20Temple%20MQ.json"

            };


            GetLogicDataFromFile(LogicFiles, false);
            GetLogicDataFromFile(MQLogicFiles, true);

            void GetLogicDataFromFile(List<string> LogicFileUrls, bool MQData)
            {
                foreach (var i in LogicFileUrls)
                {
                    Console.WriteLine(i);
                    string ItemData = wc.DownloadString(i);
                    string[] ItemDataLines = ItemData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    var CleanedLogic = RemoveCommentsFromJSON(ItemDataLines);
                    var tempLogic = JsonConvert.DeserializeObject<List<OOTRLogicObject>>(CleanedLogic, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });

                    if (MQData) { foreach (var t in tempLogic) { t.MQ = true; } }
                    Logic.AddRange(tempLogic);
                }
            }

            string RawHelperLogic = wc.DownloadString($"https://raw.githubusercontent.com/{Branch}/data/LogicHelpers.json");
            string[] RawHelperLogicLines = RawHelperLogic.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var CleanedHelperLogic = RemoveCommentsFromJSON(RawHelperLogicLines);

            OOTRLogicObject HelperLogic = new OOTRLogicObject() { region_name = "Logic Helper", events = JsonConvert.DeserializeObject<Dictionary<string, string>>(CleanedHelperLogic) };

            Dictionary<string, string> CleanedEvents = new Dictionary<string, string>();
            foreach (var i in HelperLogic.events)
            {
                CleanedEvents.Add(i.Key, i.Value.Replace("'Bugs'", "Item_Bugs").Replace("'Fish'", "Item_Fish").Replace("'Fairy'", "Item_Fairy"));
            }
            HelperLogic.events = CleanedEvents;

            Logic.Add(HelperLogic);
        }

        //Add Items to logic and dictionary

        public static void GetAverageItemAmountsFromSpoilerLog(Dictionary<string, int> ItemAmontAverages)
        {
            string LogFolder = @"C:\Users\drumm\Downloads\OOTR Logs";

            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            foreach (var i in Directory.GetFiles(LogFolder))
            {
                var Log = JsonConvert.DeserializeObject<SpoilerLog>(File.ReadAllText(i), jsonSerializerSettings);
                foreach (var j in Log.item_pool.Keys.Where(ky => !IgnoredJunk.Where(ig => ky.StartsWith(ig)).Any()))
                {
                    string Cleaned = j;

                    if (ItemAmontAverages.ContainsKey(Cleaned))
                    {
                        if (ItemAmontAverages[Cleaned] < Log.item_pool[Cleaned]) { ItemAmontAverages[Cleaned] = Log.item_pool[Cleaned]; }
                    }
                    else { ItemAmontAverages.Add(Cleaned, Log.item_pool[Cleaned]); }
                }
            }
        }

        public static void AddExtraItems(LogicObjects.LogicDictionary dict, LogicObjects.LogicFile Logic)
        {
            Dictionary<string, int> ItemAmontAverages = new Dictionary<string, int>();

            string CurrentSection = "";
            string[] RefFile = File.ReadAllLines(@"lib\Other Games\OOTR Data.txt");
            foreach (var i in RefFile)
            {
                var Line = i.Trim();
                if (Line.StartsWith("SECTION:"))
                {
                    CurrentSection = Line.Replace("SECTION:", "").Trim();
                    continue;
                }
                if (string.IsNullOrWhiteSpace(Line) || Line.StartsWith("//") || Line.StartsWith(@"\\") || Line.StartsWith(@"#")) { continue; }
                var Data = Line.Split(',').Select(x => x.Trim().Replace("'", "").Replace(")", "").Replace("(", "").Replace("\"", ""));
                Data = Data.Where(x => !string.IsNullOrWhiteSpace(x));
                if (!Data.Any()) { continue; }

                if (CurrentSection == "AVERAGEAMOUNT") { ItemAmontAverages.Add(Data.ToArray()[0], Int32.Parse(Data.ToArray()[1])); }
            }

            foreach (var i in ItemAmontAverages.OrderBy(x => x.Key))
            {
                int CountInVanilla = dict.LogicDictionaryList.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains(i.Key)).Count();
                if (i.Value + 1 > CountInVanilla && !IgnoredJunk.Where(x => i.Key.StartsWith(x)).Any())
                {
                    Console.WriteLine($"Missing {(i.Value - CountInVanilla) + 2} {i.Key}. {CountInVanilla}/{i.Value + 2} ");
                    var ammounttoadd = (i.Value) >= 5 ? Math.Ceiling((Decimal)(i.Value - CountInVanilla) / 5) * 5 : (i.Value - CountInVanilla) + 2;
                    Console.WriteLine($"Adding {(int)ammounttoadd} {i.Key}");
                    Console.WriteLine($"======================================");

                    for (var j = 0; j < ammounttoadd; j++)
                    {
                        dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                        {
                            DictionaryName = $"Extra {i.Key} {j}",
                            FakeItem = false,
                            ItemName = i.Key,
                            ItemSubType = "Item",
                            SpoilerItem = new string[] { i.Key }
                        });
                        Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
                        {
                            Id = $"Extra {i.Key} {j}"
                        });
                    }
                }
            }
        }

        public static void AddMissingItems(LogicObjects.LogicDictionary dict, LogicObjects.LogicFile Logic)
        {
            List<string> TradeItems = new List<string>()
            {
                "Pocket Egg",
                "Pocket Cucco",
                "Cojiro",
                "Odd Mushroom",
                "Odd Potion",
                "Poachers Saw",
                "Broken Sword",
                "Prescription",
                "Eyeball Frog",
                "Eyedrops",
                "Claim Check"
            };

            //Add the trade items. Their original location is not randomized, but one of these items is randomly added to the pool as the starting trade item
            foreach (var i in TradeItems)
            {
                dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = $"Trade Item {i}",
                    FakeItem = false,
                    ItemName = i,
                    ItemSubType = "Item",
                    SpoilerItem = new string[] { i, i.Replace(" ", "_") }
                });
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
                {
                    Id = $"Trade Item {i}"
                });
            }

            //Add the mising has_bottle logic object, not sure where this is defined in the randomizer but it should just be any one bottle in the item list

            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"has_bottle",
                ConditionalItems = allBottles.Select(x => new List<string>() { x }).ToList()
            });

            //In the randomizer, the bottles you ind are filled with a random item. These items don't exist in the vanilla item list so add them as possibilities
            //Also add some extra real bottles since if you start with bottles they are all empty bottles
            foreach (var i in allBottles)
            {
                for (var j = 1; j < 4; j++)
                {
                    dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                    {
                        DictionaryName = $"Bottled Item {i} {j}",
                        FakeItem = false,
                        ItemName = i,
                        ItemSubType = "Item",
                        SpoilerItem = new string[] { i, i.Replace(" ", "_") }
                    });
                    Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
                    {
                        Id = $"Bottled Item {i} {j}"
                    });
                }
            }

            Dictionary<string, string> UsablilityLogicItems = new Dictionary<string, string>()
            {
                {"can_play_Bolero_of_Fire", "Ocarina and Bolero_of_Fire"},
                {"can_play_Eponas_Song", "Ocarina and Eponas_Song"},
                {"can_play_Minuet_of_Forest", "Ocarina and Minuet_of_Forest"},
                {"can_play_Nocturne_of_Shadow", "Ocarina and Nocturne_of_Shadow"},
                {"can_play_Prelude_of_Light", "Ocarina and Prelude_of_Light"},
                {"can_play_Requiem_of_Spirit", "Ocarina and Requiem_of_Spirit"},
                {"can_play_Sarias_Song", "Ocarina and Sarias_Song"},
                {"can_play_Scarecrow_Song", "Ocarina and Scarecrow_Song"},
                {"can_play_Serenade_of_Water", "Ocarina and Serenade_of_Water"},
                {"can_play_Song_of_Storms", "Ocarina and Song_of_Storms"},
                {"can_play_Song_of_Time", "Ocarina and Song_of_Time"},
                {"can_play_Suns_Song", "Ocarina and Suns_Song"},
                {"can_play_Zeldas_Lullaby", "Ocarina and Zeldas_Lullaby"},
                {"can_use_Boomerang", "is_child and Boomerang"},
                {"can_use_Bow", "is_adult and Bow"},
                {"can_use_Dins_Fire", "Dins_Fire and Magic_Meter"},
                {"can_use_Distant_Scarecrow", "is_adult and Distant_Scarecrow"},
                {"can_use_Farores_Wind", "Farores_Wind and Magic_Meter"},
                {"can_use_Fire_Arrows", "is_adult and Fire_Arrows and Bow and Magic_Meter"},
                {"can_use_Golden_Gauntlets", "is_adult and Golden_Gauntlets"},
                {"can_use_Goron_Tunic", "is_adult and Goron_Tunic"},
                {"can_use_Hookshot", "is_adult and Hookshot"},
                {"can_use_Hover_Boots", "is_adult and Hover_Boots"},
                {"can_use_Iron_Boots", "is_adult and Iron_Boots"},
                {"can_use_Kokiri_Sword", "is_child and Kokiri_Sword"},
                {"can_use_Lens_of_Truth", "Lens_of_Truth and Magic_Meter"},
                {"can_use_Light_Arrows", "is_adult and Light_Arrows and Bow and Magic_Meter"},
                {"can_use_Longshot", "is_adult and Longshot"},
                {"can_use_Megaton_Hammer", "is_adult and Megaton_Hammer"},
                {"can_use_Nayrus_Love", "Nayrus_Love and Magic_Meter"},
                {"can_use_Scarecrow", "is_adult and Scarecrow"},
                {"can_use_Silver_Gauntlets", "is_adult and Silver_Gauntlets"},
                {"can_use_Slingshot", "is_child and Slingshot"},
                {"can_use_Sticks", "is_child and Sticks"},
                {"can_use_Zora_Tunic", "is_adult and Zora_Tunic"}
            };

            LogicParser parser = new LogicParser();

            //The randomizer used a can_play() and can_use() macro which allows passing an item to see if its usable
            //We can't do that kind of advced functionality in the tracker, so we parse down all the references to can can_play() and can_use() when reading logic
            //and add them as entries here
            foreach (var i in UsablilityLogicItems)
            {
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
                {
                    Id = i.Key,
                    ConditionalItems = parser.ConvertLogicToConditionalString(i.Value.Replace(" and ", " & "))
                });
            }

            //The randomizer keeps track of and changes these values as its placing items. We can't really do that so we'll repurpose it to a 
            // "Can be age" type value which will work for the tracker
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"age == adult",
                ConditionalItems = parser.ConvertLogicToConditionalString("Starting Age == adult | Time_Travel")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"age == child",
                ConditionalItems = parser.ConvertLogicToConditionalString("Starting Age == child | Time_Travel")
            });

            //More of the Can_use style logic, but these are mush easier to define
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"has_projectile_adult",
                ConditionalItems = parser.ConvertLogicToConditionalString("Bow | Hookshot")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"has_projectile_child",
                ConditionalItems = parser.ConvertLogicToConditionalString("Slingshot | Boomerang")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"has_projectile_both",
                ConditionalItems = parser.ConvertLogicToConditionalString("has_projectile_adult & has_projectile_child")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"has_projectile_either",
                ConditionalItems = parser.ConvertLogicToConditionalString("has_projectile_adult | has_projectile_child")
            });

            //Since wallets are always progressive we can't really assign a price value to a specific wallet
            //Here we make fake items that only exist to be added to price sanity checks
            //Luckily the randomizer doesn have any wallets in any of the logic by default, it just adds it dynamically
            //so we don't have to worry about removing any wallets from logic
            dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
            {
                DictionaryName = $"AdultWallet",
                FakeItem = true,
                WalletCapacity = 200
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"AdultWallet",
                ConditionalItems = new List<List<string>> { new List<string> { "Progressive Wallet x1" } }
            });
            dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
            {
                DictionaryName = $"GiantWallet",
                FakeItem = true,
                WalletCapacity = 500
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"GiantWallet",
                ConditionalItems = new List<List<string>> { new List<string> { "Progressive Wallet x2" } }
            });
            dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
            {
                DictionaryName = $"TycoonWallet",
                FakeItem = true,
                WalletCapacity = 999
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"TycoonWallet",
                ConditionalItems = new List<List<string>> { new List<string> { "Progressive Wallet x3" } }
            });

            //The number of Gerudo fortress keys in the game is not listed under the item pool in he spoiler log
            //so the "AddExtraItems" function misses these.
            for (var i = 1; i < 3; i++)
            {
                dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = $"Extra_Small_Key_Thieves_Hideout {i}",
                    FakeItem = false,
                    ItemName = "Small Key Thieves Hideout",
                    ItemSubType = "Item",
                    SpoilerItem = new string[] { "Small Key Thieves Hideout", "Small_Key_Thieves_Hideout" }
                });
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
                {
                    Id = $"Extra_Small_Key_Thieves_Hideout {i}"
                });
            }

        }

        public static void AddTricks(LogicObjects.LogicFile Logic)
        {
            List<string> AddedTricks = new List<string>();
            foreach (var Entry in Logic.Logic)
            {
                if (Entry.ConditionalItems == null) { continue; }
                foreach (var cond in Entry.ConditionalItems)
                {
                    foreach (var i in cond.Where(x => x.StartsWith("logic_")))
                    {
                        var LogicEntries = Logic.Logic.Where(x => x.Id == i);
                        if (!LogicEntries.Any() && !AddedTricks.Contains(i))
                        {
                            AddedTricks.Add(i);
                        }
                    }
                }
            }
            foreach(var i in AddedTricks)
            {
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
                {
                    Id = i,
                    IsTrick = true,
                    TrickTooltip = i
                });
            }
        }

        public static void AddReverseOptionEntries(LogicObjects.LogicFile Logic)
        {
            LogicParser parser = new LogicParser();
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"damage_multiplier != ohko",
                ConditionalItems = parser.ConvertLogicToConditionalString("damage_multiplier == half | damage_multiplier == normal | damage_multiplier == double | damage_multiplier == quadruple")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"damage_multiplier != quadruple",
                ConditionalItems = parser.ConvertLogicToConditionalString("damage_multiplier == half | damage_multiplier == normal | damage_multiplier == double | damage_multiplier == ohko")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"gerudo_fortress != fast",
                ConditionalItems = parser.ConvertLogicToConditionalString("gerudo_fortress == normal | gerudo_fortress == open")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"gerudo_fortress != normal",
                ConditionalItems = parser.ConvertLogicToConditionalString("gerudo_fortress == fast | gerudo_fortress == open")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"open_forest != closed",
                ConditionalItems = parser.ConvertLogicToConditionalString("open_forest == closed_deku | open_forest == open")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"shuffle_ganon_bosskey != dungeons",
                ConditionalItems = parser.ConvertLogicToConditionalString("shuffle_ganon_bosskey == other | shuffle_ganon_bosskey == medallions | shuffle_ganon_bosskey == stones | shuffle_ganon_bosskey == tokens")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"shuffle_ganon_bosskey != medallions",
                ConditionalItems = parser.ConvertLogicToConditionalString("shuffle_ganon_bosskey == other | shuffle_ganon_bosskey == dungeons | shuffle_ganon_bosskey == stones | shuffle_ganon_bosskey == tokens")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"shuffle_ganon_bosskey != stones",
                ConditionalItems = parser.ConvertLogicToConditionalString("shuffle_ganon_bosskey == other | shuffle_ganon_bosskey == dungeons | shuffle_ganon_bosskey == medallions | shuffle_ganon_bosskey == tokens")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"shuffle_ganon_bosskey != tokens",
                ConditionalItems = parser.ConvertLogicToConditionalString("shuffle_ganon_bosskey == other | shuffle_ganon_bosskey == dungeons | shuffle_ganon_bosskey == medallions | shuffle_ganon_bosskey == stones")
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"zora_fountain != open",
                ConditionalItems = parser.ConvertLogicToConditionalString("zora_fountain == closed | zora_fountain == adult")
            });

        }

        public static void AddTempleLayoutTracking(LogicObjects.LogicDictionary dict, LogicObjects.LogicFile Logic)
        {
            

            List<string> DungeonLogicReq = new List<string>()
            {
                "Bottom of the Well",
                "Deku Tree Lobby",
                "Dodongos Cavern Beginning",
                "Fire Temple Lower",
                "Forest Temple Lobby",
                "Ganons Castle Lobby",
                "Gerudo Training Ground Lobby",
                "Ice Cavern Beginning",
                "Jabu Jabus Belly Beginning",
                "Shadow Temple Entryway",
                "Spirit Temple Lobby",
                "Water Temple Lobby"
            };

            int counter = 0;
            foreach (var d in Dungeons)
            {
                dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = d + " Vanilla",
                    LocationName = d + " Layout",
                    ItemName = "Vanilla",
                    ItemSubType = $"Option{d.Replace(" ", "")}Layout",
                    LocationArea = "%Temple Layouts%"
                });
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = d + " Vanilla", RequiredItems = new List<string>() { DungeonLogicReq[counter] } });
                dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = d + " Master Quest",
                    ItemName = "Master Quest",
                    ItemSubType = $"Option{d.Replace(" ", "")}Layout",
                });
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = d + " Master Quest" });
                counter++;
            }
        }

        public static void AddRandoOptions(LogicObjects.LogicDictionary dict, LogicObjects.LogicFile Logic)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            StageOption("bridge", "vanilla", new string[] { "medallions", "open", "stones", "tokens", "other", "dungeons" });
            StageOption("damage_multiplier", "normal", new string[] { "double", "half", "ohko", "quadruple" });
            StageOption("gerudo_fortress", "normal", new string[] { "fast", "open" });
            StageOption("lacs_condition", "vanilla", new string[] { "medallions", "stones", "tokens", "other", "dungeons" });
            StageOption("open_forest", "closed", new string[] { "closed_deku", "open" });
            StageOption("open_kakariko", "closed", new string[] { "zelda", "open" });
            StageOption("shuffle_ganon_bosskey", "other", new string[] { "medallions", "stones", "tokens", "dungeons", "remove" });
            StageOption("shuffle_scrubs", "off", new string[] { "on" });
            StageOption("Starting Age", "child", new string[] { "adult" });
            StageOption("zora_fountain", "closed", new string[] { "adult", "open" });

            void StageOption(string DictNameBase, string DefaultOption, string[] OtherOptions)
            {
                CommitOption(DictNameBase, DefaultOption, textInfo.ToTitleCase(DictNameBase.Replace("_", "")));
                foreach (var i in OtherOptions)
                {
                    CommitOption(DictNameBase, i);
                }
            }
            void CommitOption(string DictNameBase, string Currentoption, string LocationName = null)
            {
                dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = $"{DictNameBase} == {Currentoption}",
                    LocationName = LocationName,
                    ItemName = Currentoption,
                    ItemSubType = $"Option_{DictNameBase.Replace(" ", "_")}",
                    LocationArea = "%Randomizer Options%"
                });
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = $"{DictNameBase} == {Currentoption}" });
            }

            AddNotoptions("bombchus_in_logic");
            AddNotoptions("free_scarecrow");
            AddNotoptions("keysanity");
            AddNotoptions("shuffle_weird_egg");
            AddNotoptions("complete_mask_quest");
            AddNotoptions("open_door_of_time");
            AddNotoptions("shuffle_dungeon_entrances");
            AddNotoptions("shuffle_overworld_entrances");
            AddNotoptions("skip_child_zelda");

            void AddNotoptions(string DictNameBase)
            {
                dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = DictNameBase,
                    LocationName = DictNameBase.Replace("_", " "),
                    ItemName = "True",
                    ItemSubType = $"Option_{DictNameBase.Replace(" ", "_")}",
                    LocationArea = "%Randomizer Options%"
                });
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = DictNameBase });
                dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = "not " + DictNameBase,
                    ItemName = "False",
                    ItemSubType = $"Option_{DictNameBase.Replace(" ", "_")}"
                });
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = "not " + DictNameBase });
            }


            AddTrialoptions("Fire");
            AddTrialoptions("Forest");
            AddTrialoptions("Light");
            AddTrialoptions("Shadow");
            AddTrialoptions("Spirit");
            AddTrialoptions("Water");
            void AddTrialoptions(string DictNameBase)
            {
                dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = $"skipped_trials[{DictNameBase}]",
                    ItemName = "Inactive",
                    ItemSubType = $"Option_{DictNameBase}_trial_status"
                });
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = $"skipped_trials[{DictNameBase}]" });
                dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = $"not skipped_trials[{DictNameBase}]",
                    LocationName = $"{DictNameBase} Trial Status",
                    ItemName = "Active",
                    ItemSubType = $"Option_{DictNameBase}_trial_status",
                    LocationArea = "%Ganons Trials Status%"
                });
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = "not " + $"skipped_trials[{DictNameBase}]", RequiredItems = new List<string> { "Ganons Castle Lobby" } });
            }
        }

        public static void AddEndgameRequirementOptions(LogicObjects.LogicDictionary dict, LogicObjects.LogicFile Logic)
        {
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = "MMRTCombinationsDynamic" });
            string[] Stones = new string[]
            {
                "Queen Gohma",
                "King Dodongo",
                "Barinade"
            };
            string[] Medallions = new string[]
            {
                "Links Pocket",
                "Phantom Ganon",
                "Volvagia",
                "Morpha",
                "Bongo Bongo",
                "Twinrova"
            };
            var Skulltokens = dict.LogicDictionaryList.Where(x => x.ItemName == "Gold Skulltula Token").Select(x => x.DictionaryName);
            var Dungones = Stones.Concat(Medallions);

            var countOptions = new string[]
            {
                "MedallionsNeededForRainbowBridge",
                "MedallionsNeededForGannonsBossKey",
                "MedallionsNeededForLightArrowCutscene",
                "StonesNeededForRainbowBridge",
                "StonesNeededForGannonsBossKey",
                "StonesNeededForLightArrowCutscene",
                "DungeonsNeededForRainbowBridge",
                "DungeonsNeededForGannonsBossKey",
                "DungeonsNeededForLightArrowCutscene",
                "SkullTokensNeededForRainbowBridge",
                "SkullTokensNeededForGannonsBossKey",
                "SkullTokensNeededForLightArrowCutscene",
            };
            var NeededOptions = new string[]
            {
                "has_medallions_bridge_medallions",
                "has_medallions_ganon_bosskey_medallions",
                "has_medallions_lacs_medallions",
                "has_stones_bridge_stones",
                "has_stones_ganon_bosskey_stones",
                "has_stones_lacs_stones",
                "has_dungeon_rewards_bridge_rewards",
                "has_dungeon_rewards_ganon_bosskey_rewards",
                "has_dungeon_rewards_lacs_rewards",
                "Gold_Skulltula_Token, bridge_tokens",
                "Gold_Skulltula_Token, ganon_bosskey_tokens",
                "Gold_Skulltula_Token, lacs_tokens"
            };

            for (var i = 0; i < countOptions.Count(); i++)
            {
                dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = countOptions[i],
                    LocationName = Regex.Replace(countOptions[i], "([a-z])([A-Z])", "$1 $2"),
                    ItemSubType = $"MMRTCountCheck",
                    LocationArea = "%End Game Requirements%"
                });
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = countOptions[i] });

                List<List<string>> Conditionals = null;
                if (countOptions[i].StartsWith("MedallionsNeeded"))
                {
                    Conditionals = Medallions.Select(x => new List<string>() { x }).ToList();
                }
                if (countOptions[i].StartsWith("StonesNeeded"))
                {
                    Conditionals = Stones.Select(x => new List<string>() { x }).ToList();
                }
                if (countOptions[i].StartsWith("DungeonsNeeded"))
                {
                    Conditionals = Dungones.Select(x => new List<string>() { x }).ToList();
                }
                if (countOptions[i].StartsWith("SkullTokensNeeded"))
                {
                    Conditionals = Skulltokens.Select(x => new List<string>() { x }).ToList();
                }

                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
                {
                    Id = NeededOptions[i],
                    RequiredItems = new List<string>() { "MMRTCombinationsDynamic", countOptions[i] },
                    ConditionalItems = Conditionals
                });

            }

            //Add Sepcial Requirements for Count Items so they only appear if they are relevent

            Logic.Logic.Find(x => x.Id == "MedallionsNeededForRainbowBridge").RequiredItems.Add("bridge == medallions");
            Logic.Logic.Find(x => x.Id == "MedallionsNeededForGannonsBossKey").RequiredItems.Add("shuffle_ganon_bosskey == medallions");
            Logic.Logic.Find(x => x.Id == "MedallionsNeededForLightArrowCutscene").RequiredItems.Add("lacs_condition == medallions");
            Logic.Logic.Find(x => x.Id == "StonesNeededForRainbowBridge").RequiredItems.Add("bridge == stones");
            Logic.Logic.Find(x => x.Id == "StonesNeededForGannonsBossKey").RequiredItems.Add("shuffle_ganon_bosskey == stones");
            Logic.Logic.Find(x => x.Id == "StonesNeededForLightArrowCutscene").RequiredItems.Add("lacs_condition == stones");
            Logic.Logic.Find(x => x.Id == "DungeonsNeededForRainbowBridge").RequiredItems.Add("bridge == dungeons");
            Logic.Logic.Find(x => x.Id == "DungeonsNeededForGannonsBossKey").RequiredItems.Add("shuffle_ganon_bosskey == dungeons");
            Logic.Logic.Find(x => x.Id == "DungeonsNeededForLightArrowCutscene").RequiredItems.Add("lacs_condition == dungeons");
            Logic.Logic.Find(x => x.Id == "SkullTokensNeededForRainbowBridge").RequiredItems.Add("bridge == tokens");
            Logic.Logic.Find(x => x.Id == "SkullTokensNeededForGannonsBossKey").RequiredItems.Add("shuffle_ganon_bosskey == tokens");
            Logic.Logic.Find(x => x.Id == "SkullTokensNeededForLightArrowCutscene").RequiredItems.Add("lacs_condition == tokens");

            //Deal with big poe here since it's more convenient
            dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
            {
                DictionaryName = "PoesNeededForPoeHutReward",
                LocationName = "Big Poe Count",
                ItemSubType = $"MMRTCountCheck",
                LocationArea = "%Randomizer Options%"
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = "PoesNeededForPoeHutReward" });

            var ChecksitemNameOccurences = dict.LogicDictionaryList.Where(x => x.ItemName == "Bottle with Big Poe").Select(x => x.DictionaryName);
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = "Bottle_with_Big_Poe, big_poe_count",
                RequiredItems = new List<string>() { "MMRTCombinationsDynamic", "PoesNeededForPoeHutReward" },
                ConditionalItems = ChecksitemNameOccurences.Select(x => new List<string>() { x }).ToList()
            });

        }

        public static void AddXofYentries(LogicObjects.LogicDictionary dict, LogicObjects.LogicFile Logic)
        {
            List<LogicObjects.JsonFormatLogicItem> Additions = new List<LogicObjects.JsonFormatLogicItem>();
            foreach (var entry in Logic.Logic)
            {
                if (entry.ConditionalItems == null || !entry.ConditionalItems.Any()) { continue; }
                foreach (var cond in entry.ConditionalItems)
                {
                    foreach (var i in cond)
                    {
                        var match = Regex.Match(i, @"x\d+");
                        if (match.Success)
                        {
                            if (Additions.Where(x => x.Id == i).Any()) { continue; }
                            var ItemNeeded = i.Replace(match.Groups[0].ToString(), "").Trim();
                            var AmmountNeeded = match.Groups[0].ToString().Replace("x", "").Trim();

                            var PossibleItemnames = new List<string> { ItemNeeded, ItemNeeded.Replace(" ", "_"), ItemNeeded.Replace("_", " ") };

                            var ChecksitemNameOccurences = dict.LogicDictionaryList.Where(x => PossibleItemnames.Contains(x.ItemName)).Select(x => x.DictionaryName);
                            if (!ChecksitemNameOccurences.Any()) { Console.WriteLine($"{ItemNeeded} Not Found in Item list"); continue; }
                            if (ChecksitemNameOccurences.Count() < Int32.Parse(AmmountNeeded)) { Console.WriteLine("Not enough Items found"); continue; }


                            LogicObjects.JsonFormatLogicItem NewLogicItem = new LogicObjects.JsonFormatLogicItem();
                            NewLogicItem.Id = i;
                            NewLogicItem.ConditionalItems = ChecksitemNameOccurences.Select(x => new List<string>() { x }).ToList();
                            if (Int32.Parse(AmmountNeeded) > 1)
                            {
                                if (!Additions.Where(x => x.Id == $"MMRTCombinations{AmmountNeeded}").Any())
                                {
                                    Additions.Add(new LogicObjects.JsonFormatLogicItem() { Id = $"MMRTCombinations{AmmountNeeded}" });
                                }
                                NewLogicItem.RequiredItems = new List<string> { $"MMRTCombinations{AmmountNeeded}" };
                            }
                            Additions.Add(NewLogicItem);
                        }
                    }
                }
            }

            foreach (var i in Additions) { Logic.Logic.Add(i); }

        }

        //Clean and parse
        public static string CleanLogicEntry(string entry)
        {
            entry = entry.Replace(" & ", " "); //If any logic has & in it by default it will mess things up

            entry = entry.Replace(System.Environment.NewLine, " "); //Replace newlines with a space
            entry = Regex.Replace(entry, @"\s+", " "); //Trim segments of multiple spaces to a single space
            entry = entry.Trim();//Trim the front and end of the line

            //For the most part, logic items can't contain spaces. Some entries however contain spaces and are grouped using '
            //Remove the ' from these entries and change the spaces to _ so it's in a familiar format
            var SpacedEntries = Regex.Matches(entry, @"\'(.*?)\'");
            foreach (Match match in SpacedEntries)
            {
                var newValue = match.Groups[1].Value.Replace("'", "");//.Replace(" ", "_");
                entry = entry.Replace(match.Groups[0].Value, $"{newValue}");
            }

            entry = entry.Replace(" and ", " & ").Replace(" or ", " | "); //change "and" and "or" logic operators to "&" and "|" which is what the tracker is able to parse


            //change any x of y entries "ex: (item, 4)" to a more familiar format (item x4)
            //No real reason to do this but since SSR logic uses the latter format and we already have code to parse it it make life a bit easier.
            var Countmatch = Regex.Matches(entry, @", \d+");
            foreach (Match match in Countmatch)
            {
                var NewValue = match.Groups[0].Value.Replace(", ", " x");
                entry = entry.Replace(match.Groups[0].Value, NewValue);
            }

            //Remove the parentheses from entries such as "can_use" and "can_play" so the logic parser can parse it properly
            //"can_use(item)" becomes "can_use_item"
            string[] CanPrefixs = new string[] { "can_play", "can_use", "has_projectile", "has_stones", "has_medallions", "has_dungeon_rewards" };

            foreach (var i in CanPrefixs)
            {
                var Matches = Regex.Matches(entry, i + @"\((.*?)\)");
                foreach (Match match in Matches) { entry = entry.Replace(match.Groups[0].Value, $"{i}_{match.Groups[1].Value}"); }
            }

            //Some entries use "at(location, item)" to mean "can use this item at this location". Since the tracker logic is not capable of that complex of logic parsing
            //Replace it with an entry of "(location & item)" which is similar enough for the trackers sake
            var AtEntries = Regex.Matches(entry, @"at\((.*?)\)");
            foreach (Match match in AtEntries)
            {
                var newValue = match.Groups[1].Value.Replace("'", "").Replace(",", " &").Replace("and True", "");
                entry = entry.Replace(match.Groups[0].Value, $"({newValue})");
            }

            //Similar to "at(location, item)" but only take the item as an argument and uses the current location.
            //Since we always require the current location anyway we can simply change this from "here(item)" to "(item)"
            var HereEntries = Regex.Matches(entry, @"here\((.*?)\)");
            foreach (Match match in HereEntries)
            {
                entry = entry.Replace(match.Groups[0].Value, "(" + match.Groups[1].Value + ")");
            }

            return entry;
        }

        public static string RemoveCommentsFromJSON(string[] ItemDataLines)
        {
            string CleanedLogic = "";
            foreach (var i in ItemDataLines)
            {
                var CleanedLine = Utility.RemoveCommentLines(i);
                if (!string.IsNullOrWhiteSpace(CleanedLine)) { CleanedLogic += CleanedLine; }
            }
            return CleanedLogic;
        }

        public static void CleanIllegalChar(LogicObjects.LogicFile Logic)
        {
            foreach(var i in Logic.Logic)
            {
                i.Id = i.Id.Replace(")", "").Replace("(", "");
            }
        }

        public static void ConvertItemnamesinLogicToDictname(LogicObjects.LogicDictionary dict, LogicObjects.LogicFile Logic)
        {
            foreach (var DictName in Logic.Logic.Select(x => x.Id).ToList())
            {
                var Entry = Logic.Logic.Find(x => x.Id == DictName);

                List<List<string>> newCondictionalSet = new List<List<string>>();
                if (Entry.ConditionalItems == null)
                {
                    continue;
                }
                foreach (var cond in Entry.ConditionalItems)
                {
                    List<string> newCondictional = new List<string>();
                    foreach (var i in cond)
                    {
                        if (DoStuff(i)) { continue; }
                        if (DoStuff(i.Replace(" ", "_"))) { continue; }
                        if (DoStuff(i.Replace("_", " "))) { continue; }

                        newCondictional.Add(i);

                        Console.WriteLine($"Could not find any valid Dictionary name to assign to {i}");

                        bool DoStuff(string name)
                        {
                            var LogicEntries = Logic.Logic.Find(x => x.Id == name);
                            if (LogicEntries != null) { newCondictional.Add(LogicEntries.Id); return true; }

                            var ItemNameEntries = dict.LogicDictionaryList.Where(x => x.ItemName == name).Select(x => x.DictionaryName).ToList();
                            if (AddByNameEntry(ItemNameEntries)) { return true; }
                            var ItemSpoilerNameEntries = dict.LogicDictionaryList.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains(name)).Select(x => x.DictionaryName).ToList();
                            if (AddByNameEntry(ItemSpoilerNameEntries)) { return true;  }


                            bool AddByNameEntry(List<string> entries)
                            {
                                if (entries.Any())
                                {
                                    var validItemNameEntries = Logic.Logic.Where(x => entries.Contains(x.Id));
                                    if (validItemNameEntries.Any())
                                    {
                                        string NewLine = $"({string.Join(" | ", validItemNameEntries.Select(x => x.Id))})";
                                        newCondictional.Add(NewLine); return true;
                                    }
                                }
                                return false;
                            }
                            return false;
                        }
                    }
                    newCondictionalSet.Add(newCondictional);
                }
                if (false)
                {
                    Entry.ConditionalItems = newCondictionalSet;
                }
                else if (newCondictionalSet.Any())
                {
                    LogicParser Parser = new LogicParser();
                    var NewCondString = $"({string.Join(") | (", newCondictionalSet.Select(x => string.Join(" & ", x)))})";
                    Console.WriteLine("===========================================================");
                    Console.WriteLine(Entry.Id);
                    Console.WriteLine(NewCondString);
                    Entry.ConditionalItems = Parser.ConvertLogicToConditionalString(NewCondString).Select(i => i.Where(x => x != "2").ToList()).ToList();
                }
                else
                {
                    Entry.ConditionalItems = null;
                }
            }
        }

        private static void DoFinalLogicCleanup(List<LogicObjects.LogicEntry> logic)
        {
            List<LogicObjects.LogicEntry> GanonsTowerChecks = new List<LogicObjects.LogicEntry>()
            {
                logic.Find(x => x.DictionaryName == "Ganons Tower Boss Key Chest"),
                logic.Find(x => x.DictionaryName == "Ganondorf Hint"),
                logic.Find(x => x.DictionaryName == "Ganon"),
            };

            //The ganons tower checks are given the ganons castle dungeon tag but don't have a MQ varient, so onyl the Vanilla tag gets added. Neither are needed.
            var GCVanillaEntry = logic.Find(x => x.DictionaryName == "Ganons Castle Vanilla");
            foreach (var i in GanonsTowerChecks)
            {
                i.Required = LogicEditing.removeItemFromRequirement(i.Required, new int[] { GCVanillaEntry.ID });
            }

            //Ganons boss key is not handle via normal boss keysy. We need to add the option for ganons boss key being removed as an alt conitional
            //to having the boss key in all checks its used in. (only two)
            var GanonBossKysy = logic.Find(x => x.DictionaryName == "shuffle_ganon_bosskey == remove");
            GanonsTowerChecks[1].Required = LogicEditing.removeItemFromRequirement(GanonsTowerChecks[1].Required, new int[] { GanonsTowerChecks[0].ID });
            GanonsTowerChecks[1].Conditionals = LogicEditing.AddConditional(GanonsTowerChecks[1].Conditionals, new int[] { GanonsTowerChecks[0].ID });
            GanonsTowerChecks[1].Conditionals = LogicEditing.AddConditional(GanonsTowerChecks[1].Conditionals, new int[] { GanonBossKysy.ID });
            GanonsTowerChecks[2].Required = LogicEditing.removeItemFromRequirement(GanonsTowerChecks[2].Required, new int[] { GanonsTowerChecks[0].ID });
            GanonsTowerChecks[2].Conditionals = LogicEditing.AddConditional(GanonsTowerChecks[2].Conditionals, new int[] { GanonsTowerChecks[0].ID });
            GanonsTowerChecks[2].Conditionals = LogicEditing.AddConditional(GanonsTowerChecks[2].Conditionals, new int[] { GanonBossKysy.ID });

            //For some reason logic has can recieve ganon boss key as available if the boss key is shuffled somewhere that isn't "Gift from Sages".
            //The only use for the can_receive_ganon_bosskey logic entry is in "Gift from Sages" ans since we don't want to see that check if it doesn't contain
            //Ganons boss key (since thats the onyl thing it can contain) remove the conditional that make can_receive_ganon_bosskey achievable if ganons
            //Boss key is not on "Gift from Sages"
            List<LogicObjects.LogicEntry> can_receive_ganon_bosskey_bad_conditionals = new List<LogicObjects.LogicEntry>()
            {
                logic.Find(x => x.DictionaryName == "shuffle_ganon_bosskey != stones"),
                logic.Find(x => x.DictionaryName == "shuffle_ganon_bosskey != medallions"),
                logic.Find(x => x.DictionaryName == "shuffle_ganon_bosskey != dungeons"),
                logic.Find(x => x.DictionaryName == "shuffle_ganon_bosskey != tokens")
            };
            var can_receive_ganon_bosskey = logic.Find(x => x.DictionaryName == "can_receive_ganon_bosskey");
            can_receive_ganon_bosskey.Conditionals = LogicEditing.removeItemFromConditionals(can_receive_ganon_bosskey.Conditionals, can_receive_ganon_bosskey_bad_conditionals.Select(x => x.ID).ToArray(), false);

        }

        //Misc Checks
        public static void FindAmbiguosLogicNames(LogicObjects.LogicDictionary dict, LogicObjects.LogicFile Logic)
        {
            List<string> ExactNames = new List<string>();
            List<string> Similarnames = new List<string>();
            List<string> ItemAndDict = new List<string>();

            foreach (var i in Logic.Logic)
            {
                var PossibleItemnames = new List<string> { i.Id, i.Id.Replace(" ", "_"), i.Id.Replace("_", " ") };
                if (Logic.Logic.Where(x => x.Id == i.Id).Count() > 1)
                {
                    ExactNames.Add(i.Id);
                }
                else
                {
                    if (Logic.Logic.Where(x => PossibleItemnames.Contains(x.Id)).Count() > 1)
                    {
                        Similarnames.Add(i.Id);
                    }
                }
                var AmbiguousItemnames = dict.LogicDictionaryList.Where(x => PossibleItemnames.Contains(x.ItemName) || (x.SpoilerItem != null && x.SpoilerItem.Where(c => PossibleItemnames.Contains(c)).Any())).Where(x => !x.DictionaryName.Contains("Gossip Stone") && !x.DictionaryName.Contains("Ganondorf Hint"));
                if (AmbiguousItemnames.Any())
                {
                    ItemAndDict.Add(i.Id);
                }


            }

            Console.WriteLine($"The following name had exact duplicates in logic\n{string.Join("\n", ExactNames.Distinct())}\n");
            Console.WriteLine($"The following name had an ambiguos name in logic\n{string.Join("\n", Similarnames.Distinct())}\n");
            Console.WriteLine($"The following name had items with the same name\n{string.Join("\n", ItemAndDict.Distinct())}\n");

        }

        public static void FinalLogicCheck(LogicObjects.LogicFile Logic)
        {
            Dictionary<string, int> LogicNametoId = new Dictionary<string, int>();
            int DicCounter = 0;
            foreach (var i in Logic.Logic)
            {
                Console.WriteLine(i.Id + ": " + DicCounter);
                LogicNametoId.Add(i.Id, DicCounter);
                DicCounter++;
            }
            foreach (var i in Logic.Logic)
            {
                Console.WriteLine(i.Id);
                bool Found = i.ConditionalItems == null ? true : i.ConditionalItems.Select(x => x.Select(y => LogicNametoId.ContainsKey(y))).Any();
                if (!Found) { Console.WriteLine("Error"); throw new Exception($"{i.Id} had Bad Conditional"); }
            }
        }

        public static void CheckFormissingLogicItems(LogicObjects.LogicDictionary dict, LogicObjects.LogicFile Logic)
        {
            List<string> MissingEntries = new List<string>();
            List<string> CircularReq = new List<string>();
            foreach (var Entry in Logic.Logic)
            {
                if (Entry.ConditionalItems == null)
                {
                    continue;
                }
                foreach (var cond in Entry.ConditionalItems)
                {
                    foreach (var i in cond)
                    {
                        var PossibleItemnames = new List<string> { i };

                        var LogicEntries = Logic.Logic.Where(x => PossibleItemnames.Contains(x.Id));
                        var itemEntries = dict.LogicDictionaryList.Where(x => PossibleItemnames.Contains(x.ItemName) ||
                                (x.SpoilerItem != null && x.SpoilerItem.Where(y => PossibleItemnames.Contains(y)).Any())
                            );

                        if (!LogicEntries.Any() && !itemEntries.Any())
                        {
                            if (!MissingEntries.Contains(i)) { MissingEntries.Add(i); }
                        }
                        if (i == Entry.Id)
                        {
                            if (!CircularReq.Contains(Entry.Id)) { CircularReq.Add(Entry.Id); }
                        }
                    }
                }
            }
            if (MissingEntries.Any())
            {
                Console.WriteLine($"\nThe following Entries could not be found in logic or item list\n{string.Join(Environment.NewLine, MissingEntries.OrderBy(x => x))}");
            }
            if (CircularReq.Any())
            {
                Console.WriteLine($"\nThe following Entries contained its self in its logic\n{string.Join(Environment.NewLine, CircularReq.OrderBy(x => x))}");
            }
        }

        //Spoiler Log Importing
        public static bool HandleOOTRSpoilerLog(LogicObjects.TrackerInstance Instance, string[] Spoiler)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            List<int> UsedItems = new List<int>();

            var Log = JsonConvert.DeserializeObject<SpoilerLog>(string.Join(" ", Spoiler), jsonSerializerSettings);

            //Prep Logic
            foreach (var i in Instance.Logic)
            {
                if (!i.IsFake)
                {
                    i.Options = 0;
                }
                else if (i.IsTrick)
                {
                    i.TrickEnabled = false;
                }
            }

            #region OptionPrep
            //Define Option Entries
            var bombchus_in_logic = Instance.Logic.Find(x => x.DictionaryName == $"bombchus_in_logic");
            var not_bombchus_in_logic = Instance.Logic.Find(x => x.DictionaryName == $"not bombchus_in_logic");
            var bridgeCheck = Instance.Logic.Find(x => x.DictionaryName == "bridge == vanilla");
            var bridgeOther = Instance.Logic.Find(x => x.DictionaryName == "bridge == other");
            var complete_mask_quest = Instance.Logic.Find(x => x.DictionaryName == $"complete_mask_quest");
            var not_complete_mask_quest = Instance.Logic.Find(x => x.DictionaryName == $"not complete_mask_quest");
            var damage_multiplier = Instance.Logic.Find(x => x.DictionaryName == "damage_multiplier == normal");
            var free_scarecrow = Instance.Logic.Find(x => x.DictionaryName == $"free_scarecrow");
            var not_free_scarecrow = Instance.Logic.Find(x => x.DictionaryName == $"not free_scarecrow");
            var gerudo_fortress = Instance.Logic.Find(x => x.DictionaryName == "gerudo_fortress == normal");
            var shuffle_smallkeys = Instance.Logic.Find(x => x.DictionaryName == "keysanity");
            var not_shuffle_smallkeys = Instance.Logic.Find(x => x.DictionaryName == "not keysanity");
            var lacs_conditionCheck = Instance.Logic.Find(x => x.DictionaryName == "lacs_condition == vanilla");
            var lacs_conditionOther = Instance.Logic.Find(x => x.DictionaryName == "lacs_condition == other");
            var open_door_of_time = Instance.Logic.Find(x => x.DictionaryName == $"open_door_of_time");
            var not_open_door_of_time = Instance.Logic.Find(x => x.DictionaryName == $"not open_door_of_time");
            var open_forest = Instance.Logic.Find(x => x.DictionaryName == "open_forest == closed");
            var open_kakariko = Instance.Logic.Find(x => x.DictionaryName == "open_kakariko == closed");
            var shuffle_dungeon_entrances = Instance.Logic.Find(x => x.DictionaryName == $"shuffle_dungeon_entrances");
            var not_shuffle_dungeon_entrances = Instance.Logic.Find(x => x.DictionaryName == $"not shuffle_dungeon_entrances");
            var shuffle_overworld_entrances = Instance.Logic.Find(x => x.DictionaryName == $"shuffle_overworld_entrances");
            var not_shuffle_overworld_entrances = Instance.Logic.Find(x => x.DictionaryName == $"not shuffle_overworld_entrances");
            var shuffle_weird_egg = Instance.Logic.Find(x => x.DictionaryName == $"shuffle_weird_egg");
            var not_shuffle_weird_egg = Instance.Logic.Find(x => x.DictionaryName == $"not shuffle_weird_egg");
            var shuffle_scrubs = Instance.Logic.Find(x => x.DictionaryName == $"shuffle_scrubs == on");
            var not_shuffle_scrubs = Instance.Logic.Find(x => x.DictionaryName == $"shuffle_scrubs == off");
            var shuffle_ganon_bosskey = Instance.Logic.Find(x => x.DictionaryName == "shuffle_ganon_bosskey == other");
            var GanonBossKey = Instance.Logic.Find(x => x.DictionaryName == $"Ganons Tower Boss Key Chest");
            var skip_child_zelda = Instance.Logic.Find(x => x.DictionaryName == $"skip_child_zelda");
            var not_skip_child_zelda = Instance.Logic.Find(x => x.DictionaryName == $"not skip_child_zelda");
            var zora_fountain = Instance.Logic.Find(x => x.DictionaryName == "zora_fountain == closed");
            var starting_age = Instance.Logic.Find(x => x.DictionaryName == "Starting Age == child");
            var Master_Sword_Pedestal = Instance.Logic.Find(x => x.DictionaryName == "Master Sword Pedestal");

            //Set Defaults for some settings that don't always appear in the Spoiler
            starting_age.SpoilerRandom = starting_age.ID;
            skip_child_zelda.SpoilerRandom = not_skip_child_zelda.ID;

            //This check is nevr randomized and is only needed for the pathfinder to function
            Master_Sword_Pedestal.SetUnRandomized();
            UsedItems.Add(Master_Sword_Pedestal.ID);

            #endregion OptionPrep

            #region Radnomizer Settings
            //Read Standard options
            foreach (var i in Log.settings)
            {
                ReadSettings(i);
            }

            //If an option is randomized, the data we want is located here.
            foreach (var i in Log.randomized_settings)
            {
                ReadSettings(i);
            }


            void ReadSettings(KeyValuePair<string, dynamic> i)
            {
                switch (i.Key)
                {
                    case "big_poe_count":
                        Instance.Logic.Find(x => x.DictionaryName == $"PoesNeededForPoeHutReward").CountCheckData = $"${i.Value}";
                        break;
                    case "bombchus_in_logic":
                        bombchus_in_logic.SpoilerRandom = i.Value ? bombchus_in_logic.ID : not_bombchus_in_logic.ID;
                        break;
                    case "bridge":
                        bridgeCheck.SpoilerRandom = bridgeOther.ID;
                        if (i.Value == "vanilla" || i.Value == "medallions" || i.Value == "open" || i.Value == "stones" || i.Value == "tokens" || i.Value == "dungeons")
                        {
                            bridgeCheck.SpoilerRandom = Instance.Logic.Find(x => x.DictionaryName == $"bridge == {i.Value}").ID;
                        }
                        break;
                    case "complete_mask_quest":
                        complete_mask_quest.SpoilerRandom = i.Value ? complete_mask_quest.ID : not_complete_mask_quest.ID;
                        break;
                    case "damage_multiplier":
                        damage_multiplier.SpoilerRandom = damage_multiplier.ID;
                        if (i.Value == "double" || i.Value == "half" || i.Value == "ohko" || i.Value == "quadruple")
                        {
                            damage_multiplier.SpoilerRandom = Instance.Logic.Find(x => x.DictionaryName == $"damage_multiplier == {i.Value}").ID;
                        }
                        break;
                    case "free_scarecrow":
                        free_scarecrow.SpoilerRandom = i.Value ? free_scarecrow.ID : not_free_scarecrow.ID;
                        break;
                    case "gerudo_fortress":
                        gerudo_fortress.SpoilerRandom = gerudo_fortress.ID;
                        if (i.Value == "fast" || i.Value == "open")
                        {
                            gerudo_fortress.SpoilerRandom = Instance.Logic.Find(x => x.DictionaryName == $"gerudo_fortress == {i.Value}").ID;
                        }
                        break;
                    case "shuffle_smallkeys":
                        shuffle_smallkeys.SpoilerRandom = i.Value == "vanilla" ? not_shuffle_smallkeys.ID : shuffle_smallkeys.ID;
                        Instance.Options.Keysy["SmallKey"] = i.Value == "remove";
                        break;
                    case "shuffle_bosskeys":
                        Instance.Options.Keysy["BossKey"] = i.Value == "remove";
                        break;
                    case "lacs_condition":
                        lacs_conditionCheck.SpoilerRandom = lacs_conditionOther.ID;
                        if (i.Value == "vanilla" || i.Value == "medallions" || i.Value == "stones" || i.Value == "tokens" || i.Value == "dungeons")
                        {
                            lacs_conditionCheck.SpoilerRandom = Instance.Logic.Find(x => x.DictionaryName == $"lacs_condition == {i.Value}").ID;
                        }
                        break;
                    case "open_door_of_time":
                        open_door_of_time.SpoilerRandom = i.Value ? open_door_of_time.ID : not_open_door_of_time.ID;
                        break;
                    case "open_forest":
                        open_forest.SpoilerRandom = open_forest.ID;
                        if (i.Value == "closed_deku" || i.Value == "open")
                        {
                            open_forest.SpoilerRandom = Instance.Logic.Find(x => x.DictionaryName == $"open_forest == {i.Value}").ID;
                        }
                        break;
                    case "open_kakariko":
                        open_kakariko.SpoilerRandom = open_kakariko.ID;
                        if (i.Value == "zelda" || i.Value == "open")
                        {
                            open_kakariko.SpoilerRandom = Instance.Logic.Find(x => x.DictionaryName == $"open_kakariko == {i.Value}").ID;
                        }
                        break;
                    case "shuffle_dungeon_entrances":
                        shuffle_dungeon_entrances.SpoilerRandom = i.Value ? shuffle_dungeon_entrances.ID : not_shuffle_dungeon_entrances.ID;
                        break;
                    case "shuffle_overworld_entrances":
                        shuffle_overworld_entrances.SpoilerRandom = i.Value ? shuffle_overworld_entrances.ID : not_shuffle_overworld_entrances.ID;
                        break;
                    case "shuffle_weird_egg":
                        shuffle_weird_egg.SpoilerRandom = i.Value ? shuffle_weird_egg.ID : not_shuffle_weird_egg.ID;
                        break;
                    case "shuffle_ganon_bosskey":
                        shuffle_ganon_bosskey.SpoilerRandom = shuffle_ganon_bosskey.ID;
                        if (i.Value == "medallions" || i.Value == "stones" || i.Value == "tokens" || i.Value == "dungeons" || i.Value == "remove")
                        {
                            shuffle_ganon_bosskey.SpoilerRandom = Instance.Logic.Find(x => x.DictionaryName == $"shuffle_ganon_bosskey == {i.Value}").ID;
                            if (i.Value != "remove")
                            {
                                Instance.Logic.Find(x => x.DictionaryName == $"Gift from Sages").SpoilerRandom = GanonBossKey.ID;
                                UsedItems.Add(GanonBossKey.ID);
                            }
                        }
                        break;
                    case "shuffle_scrubs":
                        not_shuffle_scrubs.SpoilerRandom = i.Value == "off" ? not_shuffle_scrubs.ID : shuffle_scrubs.ID;
                        break;
                    case "skip_child_zelda": //Can't select the option in the randomizer so not sure what the actual option text is
                        skip_child_zelda.SpoilerRandom = i.Value ? skip_child_zelda.ID : not_skip_child_zelda.ID;
                        break;
                    case "starting_age":
                        if (i.Value == "adult")
                        {
                            starting_age.SpoilerRandom = Instance.Logic.Find(x => x.DictionaryName == $"Starting Age == adult").ID;
                        }
                        break;
                    case "zora_fountain":
                        zora_fountain.SpoilerRandom = zora_fountain.ID;
                        if (i.Value == "adult" || i.Value == "open")
                        {
                            zora_fountain.SpoilerRandom = Instance.Logic.Find(x => x.DictionaryName == $"zora_fountain == {i.Value}").ID;
                        }
                        break;
                    case "bridge_medallions":
                        Instance.Logic.Find(x => x.DictionaryName == $"MedallionsNeededForRainbowBridge").CountCheckData = $"${i.Value}";
                        break;
                    case "ganon_bosskey_medallions":
                        Instance.Logic.Find(x => x.DictionaryName == $"MedallionsNeededForGannonsBossKey").CountCheckData = $"${i.Value}";
                        break;
                    case "bridge_rewards":
                        Instance.Logic.Find(x => x.DictionaryName == $"DungeonsNeededForRainbowBridge").CountCheckData = $"${i.Value}";
                        break;
                    case "ganon_bosskey_rewards":
                        Instance.Logic.Find(x => x.DictionaryName == $"DungeonsNeededForGannonsBossKey").CountCheckData = $"${i.Value}";
                        break;
                    case "bridge_tokens":
                        Instance.Logic.Find(x => x.DictionaryName == $"SkullTokensNeededForRainbowBridge").CountCheckData = $"${i.Value}";
                        break;
                    case "ganon_bosskey_tokens":
                        Instance.Logic.Find(x => x.DictionaryName == $"SkullTokensNeededForGannonsBossKey").CountCheckData = $"${i.Value}";
                        break;
                    case "bridge_stones":
                        Instance.Logic.Find(x => x.DictionaryName == $"StonesNeededForRainbowBridge").CountCheckData = $"${i.Value}";
                        break;
                    case "ganon_bosskey_stones":
                        Instance.Logic.Find(x => x.DictionaryName == $"StonesNeededForGannonsBossKey").CountCheckData = $"${i.Value}";
                        break;
                    case "disabled_locations":
                        foreach(string entry in i.Value)
                        {
                            var CurItem = Instance.Logic.Find(x => x.DictionaryName == entry);
                            CurItem.SetJunk();
                        }
                        break;
                    case "allowed_tricks":
                        foreach (string entry in i.Value)
                        {
                            var CurItem = Instance.Logic.Find(x => x.DictionaryName == entry);
                            if (CurItem.IsTrick) { CurItem.TrickEnabled = true; }
                        }
                        break;
                }
            }
            #endregion Radnomizer Settings

            #region Dungeon Layouts and Trial Status
            foreach (var i in Log.dungeons)
            {
                var OptionVanilla = Instance.Logic.Find(x => x.DictionaryName == $"{i.Key} Vanilla");
                var OptionmasterQuest = Instance.Logic.Find(x => x.DictionaryName == $"{i.Key} Master Quest");
                if (i.Value == "vanilla")
                {
                    OptionVanilla.SpoilerRandom = OptionVanilla.ID;
                }
                else
                {
                    OptionVanilla.SpoilerRandom = OptionmasterQuest.ID;
                }

                Console.WriteLine($"{OptionVanilla.LocationName} is {Instance.Logic[OptionVanilla.SpoilerRandom].ItemName}");

            }

            foreach (var i in Log.trials)
            {
                var OptionSkiped = Instance.Logic.Find(x => x.DictionaryName == $"skipped_trials[{i.Key}]");
                var OptionNotSkiped = Instance.Logic.Find(x => x.DictionaryName == $"not skipped_trials[{i.Key}]");
                if (i.Value == "active")
                {
                    OptionNotSkiped.SpoilerRandom = OptionNotSkiped.ID;
                }
                else
                {
                    OptionNotSkiped.SpoilerRandom = OptionSkiped.ID;
                }

                Console.WriteLine($"{OptionNotSkiped.LocationName} is {Instance.Logic[OptionNotSkiped.SpoilerRandom].ItemName}");

            }
            #endregion Dungeon Layouts and Trial Status

            #region StartingItems
            foreach (var i in Log.starting_items)
            {
                var Item = i.Key;

                if (IgnoredJunk.Select(x => x.Replace("(", "").Trim()).Contains(Item))
                {
                    continue;
                }

                for (var n = 0; n < i.Value; n++)
                {
                    var ValidUnusedItems = Instance.Logic.Find(x => x.SpoilerItem != null && x.SpoilerItem.Contains(Item) && !UsedItems.Contains(x.ID));
                    if (ValidUnusedItems == null && Item.Contains("(") && Item.Contains(")"))
                    {
                        ValidUnusedItems = Instance.Logic.Find(x => x.SpoilerItem != null && x.SpoilerItem.Contains(Item.Replace("(", "").Replace(")", "")) && !UsedItems.Contains(x.ID));
                    }
                    if (ValidUnusedItems != null)
                    {
                        UsedItems.Add(ValidUnusedItems.ID);
                        ValidUnusedItems.ToggleStartingItem(true);
                    }
                    else { Console.WriteLine($"Item: {Item} Was not found in logic"); }
                }
            }
            #endregion StartingItems

            #region Items

            foreach (var j in Log.locations)
            {
                string Locations = j.Key;
                string Item = "";
                int Price = -1;

                Type T = j.Value.GetType();
                if (T == typeof(string)) { Item = j.Value.ToString(); }
                else
                {
                    SpoilerLogPriceData Data = j.Value.ToObject<SpoilerLogPriceData>();
                    Item = Data.item;
                    Price = Data.price;
                }

                if (Item.Contains("["))
                {
                    Console.WriteLine($"{Item} Was Changed to {Item.Substring(0, Item.IndexOf("[")).Trim()}");
                    Item = Item.Substring(0, Item.IndexOf("[")).Trim();
                }

                var ValidLocation = Instance.Logic.Find(x => x.SpoilerLocation != null && x.SpoilerLocation.Contains(Locations));
                var ValidUnusedItems = Instance.Logic.Find(x => x.SpoilerItem != null && x.SpoilerItem.Contains(Item) && !UsedItems.Contains(x.ID));

                if (ValidLocation == null && Locations.Contains("(") && Locations.Contains(")"))
                {
                    ValidLocation = Instance.Logic.Find(x => x.SpoilerLocation != null && x.SpoilerLocation.Contains(Locations.Replace("(", "").Replace(")", "")));
                }
                if (ValidUnusedItems == null && Item.Contains("(") && Item.Contains(")"))
                {
                    ValidUnusedItems = Instance.Logic.Find(x => x.SpoilerItem != null && x.SpoilerItem.Contains(Item.Replace("(", "").Replace(")", "")) && !UsedItems.Contains(x.ID));
                }

                if (ValidLocation != null)
                {
                    if (Price > -1) { ValidLocation.Price = Price; }
                    if (ValidUnusedItems != null)
                    {
                        UsedItems.Add(ValidUnusedItems.ID);
                        ValidLocation.SpoilerRandom = ValidUnusedItems.ID;
                    }
                    else if (IgnoredJunk.Where(x => Item.StartsWith(x)).Any()) //There are way more junk items (ruppes, ammo etc) than what exist in the dictionary.
                    {
                        ValidLocation.SpoilerRandom = -1;
                        ValidLocation.JunkItemType = Item;
                    }
                    else { Console.WriteLine($"Item: {Item} Was not found in logic"); }
                }
                else { Console.WriteLine($"Location: {Locations} Was not found in logic"); }


            }

            foreach(var item in Instance.Logic.Where(x => x.ItemSubType == "Item" && x.AppearsInListbox()))
            {
                if (item.SpoilerRandom < -1) //If a location was not given spoiler data
                {
                    Console.WriteLine("=======================================================");
                    if (!string.IsNullOrWhiteSpace(item.ItemName))
                    {
                        if (!UsedItems.Contains(item.ID)) //Set the locations randomized item to its self if its item wasn't already assigned
                        {
                            UsedItems.Add(item.ID);
                            item.SpoilerRandom = item.ID;
                            Console.WriteLine($"Setting {item.LocationName ?? item.DictionaryName} to Vanilla Item {item.ItemName ?? item.DictionaryName}");
                        }
                        else //If this entrys item was already assigned, try to find another unused item with the same name
                        {
                            var ValidUnusedItems = Instance.Logic.Find(x => x.SpoilerItem != null && x.SpoilerItem.Contains(item.ItemName) && !UsedItems.Contains(x.ID));
                            if (ValidUnusedItems != null) 
                            { 
                                item.SpoilerRandom = ValidUnusedItems.ID;
                                Console.WriteLine($"Setting {item.LocationName ?? item.DictionaryName} to (French) Vanilla Item {ValidUnusedItems.ItemName ?? ValidUnusedItems.DictionaryName}");
                            }
                            else
                            {
                                item.SpoilerRandom = -1;
                                if (IgnoredJunk.Where(j => item.ItemName.StartsWith(j.Replace("(", "").Trim())).Any())
                                {
                                    item.JunkItemType = item.ItemName;
                                    Console.WriteLine($"Placing Masked Junk Item {item.ItemName} at {item.LocationName ?? item.DictionaryName}");
                                }
                                else
                                {
                                    item.JunkItemType = $"no unused {item.ItemName} Available";
                                    Console.WriteLine($"no unused {item.ItemName} Available to place at {item.LocationName ?? item.DictionaryName}. Setting to errored Junk");
                                }
                            }
                        }
                    }
                    else
                    {
                        item.SpoilerRandom = -1;
                        item.JunkItemType = $"No Item Available";
                        Console.WriteLine($"{item.LocationName??item.DictionaryName} Did not have a vanilla item. Setting to errored Junk");
                    }
                }
            }
            #endregion Items

            #region Entrances

            List<int> Exitsused = new List<int>();
            Dictionary<LogicObjects.LogicEntry, LogicObjects.LogicEntry> OneWayData = new Dictionary<LogicObjects.LogicEntry, LogicObjects.LogicEntry>();

            foreach (var j in Log.entrances)
            {
                var entrance = j.Key;
                string exit = null;

                string sValue = j.Value as string;
                if (sValue != null)
                {
                    //If an area only has one exit/entrance the spoiler log doesn't the "from" region meaning the full item name is not present
                    //We need to "Guess" the from region by looking for entrances whos item name start with what we are given
                    //Since these only have one valid entrance/exit anyway the guess should always be correct
                    var GuessedExitname = Instance.Logic.Where(x =>
                        x.IsEntrance() && !string.IsNullOrWhiteSpace(x.ItemName) &&
                        x.ItemName.Split(new string[] { "<-" }, StringSplitOptions.None)[0].Trim() == sValue).ToArray();

                    if (GuessedExitname.Any())
                    {
                        exit = GuessedExitname[GuessedExitname.Count() - 1].ItemName.Trim();
                    }
                }
                else
                {
                    RegionExit R = j.Value.ToObject<RegionExit>();
                    exit = $"{ R.region } <- { R.from }";
                }

                if (exit == null) { Console.WriteLine($"No valid exit found at {j.Key}"); continue; }

                var ValidEntrances = Instance.Logic.Where(x => x.IsEntrance() && x.SpoilerLocation != null && x.SpoilerLocation.Contains(entrance));
                var ValidExits = Instance.Logic.Where(x => x.IsEntrance() && x.SpoilerItem != null && x.SpoilerItem.Select(z => z.Trim()).Contains(exit));

                if (!ValidEntrances.Any() || !ValidExits.Any())
                {
                    //Console.WriteLine($"=======================================");
                    //Console.WriteLine($"Line {entrance} -> {exit} Not valid");
                    //Console.WriteLine($"Valid entrance Count {ValidEntrances.Count()}");
                    //Console.WriteLine($"Valid Exit Count {ValidExits.Count()}");
                    continue;
                }

                var StashedEntrance = ValidEntrances.ToArray()[0];
                var StashedExit = ValidExits.ToArray()[0];

                ValidEntrances = ValidEntrances.Where(x => !x.IsOneWayEntrance(Instance));
                ValidExits = ValidExits.Where(x => !x.IsOneWayEntrance(Instance));

                //Since one way entrances lead to exits that exist elsewere, stash the entrance and exit data to apply at the end
                //This is so we don't confuse the function that tries to fill in missing data based on entrance pairs
                if (!ValidEntrances.Any() || !ValidExits.Any())
                {
                    //Console.WriteLine($"=======================================");
                    //Console.WriteLine($"Skipping One Way entrance {StashedEntrance.LocationName}");
                    //Console.WriteLine($"Entrance One Way {!ValidEntrances.Where(x => !x.IsOneWayEntrance(Instance)).Any()}");
                    //Console.WriteLine($"Exit One Way {!ValidExits.Where(x => !x.IsOneWayEntrance(Instance)).Any()}");
                    OneWayData.Add(StashedEntrance, StashedExit);
                    continue;
                }

                var ExitToUse = ValidExits.ToList()[ValidExits.Count() - 1];

                var UnusedValidExits = ValidExits.Where(x => !Exitsused.Contains(x.ID));
                if (UnusedValidExits.Any())
                {
                    ExitToUse = UnusedValidExits.ToList()[UnusedValidExits.Count() - 1];
                }
                else { Console.WriteLine($"Warning Exit {ExitToUse.ItemName} Was used multiple times"); }

                var EntranceTouse = ValidEntrances.ToList()[ValidEntrances.Count() - 1];

                EntranceTouse.SpoilerRandom = ExitToUse.ID;

            }

            //If an area only has one entrance/exit the entrance is not listed in the spoiler log unless entrances are uncouple
            //If entrances are couple however we can find where that entrance leads by going down an entrance pair rabbit hole
            //And looking at what exit leads to the entrance we are trying to find
            foreach(var e in Instance.Logic.Where(x => x.IsEntrance()))
            {
                if (e.SpoilerRandom < 0)
                {
                    if (Instance.Options.CoupleEntrances)
                    {
                        var PairedEntry = e.PairedEntry(Instance);
                        if (PairedEntry != null && PairedEntry.SpoilerRandom > -1)
                        {
                            //Console.WriteLine($"=======================================================================");
                            //Console.WriteLine($"[{e.LocationName}] Did not have spoiler data but entrances were couple");
                            //Console.WriteLine($"[{e.LocationName}] Coupled Entrance was [{PairedEntry.LocationName}] or [{PairedEntry.ItemName}]");
                            var PairedEntryLocation = PairedEntry.GetItemsSpoilerLocation(Instance.Logic);
                            if (PairedEntryLocation != null)
                            {
                                //Console.WriteLine($"[{PairedEntry.ItemName}] Was Located at [{PairedEntryLocation.LocationName}]");
                                var PairedEntryLocationPairedEntry = PairedEntryLocation.PairedEntry(Instance);
                                if (PairedEntryLocationPairedEntry != null)
                                {
                                    //Console.WriteLine($"[{PairedEntryLocation.LocationName}] paired entry was [{PairedEntryLocationPairedEntry.LocationName}] or [{PairedEntryLocationPairedEntry.ItemName}]");
                                    //Console.WriteLine($"[{e.LocationName}] Leads to [{PairedEntryLocationPairedEntry.ItemName}]");
                                    e.SpoilerRandom = PairedEntryLocationPairedEntry.ID;
                                }
                            }
                        }
                    }
                }
            }

            //We can now commit one way entrances
            foreach (var i in OneWayData)
            {
                //Console.WriteLine("Commiting One Way data");
                //Console.WriteLine($"[{i.Key.LocationName}] leads to [{i.Value.ItemName}]");
                i.Key.SpoilerRandom = i.Value.ID;
            }

            //unrandomized entrances don't show up in the spoiler log. If an entrance doesn't have data at this point we can assume it's unrandomized.
            foreach (var e in Instance.Logic.Where(x => x.IsEntrance() && x.AppearsInListbox()))
            {
                if (e.SpoilerRandom < 0)
                {
                    e.SpoilerRandom = e.ID;
                    e.SetUnRandomized();
                }
            }

            #endregion Entrances


            foreach(var i in Instance.Logic.Where(x => x.AppearsInListbox() && !x.IsCountCheck() && !x.IsGossipStone()))
            {
                if (i.SpoilerRandom < -1 && i.Randomized())
                {
                    Console.WriteLine($"{i.LocationName ?? i.DictionaryName} Needs Spoiler Data");
                }
            }

            Tools.CheckAllItemsByLocation("%Randomizer Options%");
            Tools.CheckAllItemsByLocation("%End Game Requirements%");

            return true;
        }


    }
}
