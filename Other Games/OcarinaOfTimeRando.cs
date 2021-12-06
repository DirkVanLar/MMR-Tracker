using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms;
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

namespace MMR_Tracker.Other_Games
{
    class OcarinaOfTimeRando
    {
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
            public Dictionary<string, string> dungeons = new Dictionary<string, string>();
            public Dictionary<string, dynamic> entrances = new Dictionary<string, dynamic>();
            public Dictionary<string, dynamic> locations = new Dictionary<string, dynamic>();
            public Dictionary<string, int> item_pool = new Dictionary<string, int>();
        }

        public class RegionExit
        {
            public string region = "";
            public string from = "";
        }

        //New Attempt
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


            File.WriteAllLines("OOTRLogic.json", LogicEditing.WriteLogicToJson(LogicObjects.MainTrackerInstance));
            File.WriteAllText("OOTRDcitionary.json", JsonConvert.SerializeObject(LogicObjects.MainTrackerInstance.LogicDictionary, _jsonSerializerOptions));
            Process.Start(Directory.GetCurrentDirectory());

            return;


            Console.WriteLine($"Logic File contained {Logic.Logic.Count()} Entries");

        }

        private static void DoFinalLogicCleanup(List<LogicObjects.LogicEntry> logic)
        {
            List<LogicObjects.LogicEntry> GanonsTowerChecks = new List<LogicObjects.LogicEntry>()
            {
                logic.Find(x => x.DictionaryName == "Ganons Tower Boss Key Chest"),
                logic.Find(x => x.DictionaryName == "Ganondorf Hint"),
                logic.Find(x => x.DictionaryName == "Ganon"),
            };

            var GCVanillaEntry = logic.Find(x => x.DictionaryName == "Ganons Castle Vanilla");

            foreach (var i in GanonsTowerChecks)
            {
                //The ganons tower checks are given the ganons castle dungeon tag but don't have a MQ varient, so onyl the Vanilla tag gets added. Neither are needed.
                i.Required = LogicEditing.removeItemFromRequirement(i.Required, new int[] { GCVanillaEntry.ID }); 
            }

            //Ganons boss 
            var GanonBossKysy = logic.Find(x => x.DictionaryName == "shuffle_ganon_bosskey == remove");
            GanonsTowerChecks[1].Required = LogicEditing.removeItemFromRequirement(GanonsTowerChecks[1].Required, new int[] { GanonsTowerChecks[0].ID });
            GanonsTowerChecks[1].Conditionals = LogicEditing.AddConditional(GanonsTowerChecks[1].Conditionals, new int[] { GanonsTowerChecks[0].ID });
            GanonsTowerChecks[1].Conditionals = LogicEditing.AddConditional(GanonsTowerChecks[1].Conditionals, new int[] { GanonBossKysy.ID });
            GanonsTowerChecks[2].Required = LogicEditing.removeItemFromRequirement(GanonsTowerChecks[2].Required, new int[] { GanonsTowerChecks[0].ID });
            GanonsTowerChecks[2].Conditionals = LogicEditing.AddConditional(GanonsTowerChecks[2].Conditionals, new int[] { GanonsTowerChecks[0].ID });
            GanonsTowerChecks[2].Conditionals = LogicEditing.AddConditional(GanonsTowerChecks[2].Conditionals, new int[] { GanonBossKysy.ID });
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
                foreach(var cond in Entry.ConditionalItems)
                {
                    foreach(var i in cond)
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

        public static LogicObjects.LogicFile ReadOotrLogic()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            List<string> LogicFiles = new List<string>
            {
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Overworld.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Bottom%20of%20the%20Well.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Deku%20Tree.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Dodongos%20Cavern.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Fire%20Temple.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Forest%20Temple.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Ganons%20Castle.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Gerudo%20Training%20Ground.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Ice%20Cavern.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Jabu%20Jabus%20Belly.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Shadow%20Temple.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Spirit%20Temple.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Water%20Temple.json"

            };

            List<string> MQLogicFiles = new List<string>
            {
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Bottom%20of%20the%20Well%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Deku%20Tree%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Dodongos%20Cavern%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Fire%20Temple%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Forest%20Temple%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Ganons%20Castle%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Gerudo%20Training%20Ground%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Ice%20Cavern%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Jabu%20Jabus%20Belly%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Shadow%20Temple%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Spirit%20Temple%20MQ.json",
                "https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/World/Water%20Temple%20MQ.json"

            };

            var Logic = new List<OOTRLogicObject>();
            var ParsedLogic = new List<OOTRLogicObject>();

            Dictionary<string, List<string>> RegionLogic = new Dictionary<string, List<string>>();

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

            string RawHelperLogic = wc.DownloadString("https://raw.githubusercontent.com/TestRunnerSRL/OoT-Randomizer/Dev/data/LogicHelpers.json");
            string[] RawHelperLogicLines = RawHelperLogic.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var CleanedHelperLogic = RemoveCommentsFromJSON(RawHelperLogicLines);

            OOTRLogicObject HelperLogic = new OOTRLogicObject() { region_name = "Logic Helper", events = JsonConvert.DeserializeObject<Dictionary<string, string>>(CleanedHelperLogic) };

            Dictionary<string, string> CleanedEvents = new Dictionary<string, string>();
            foreach(var i in HelperLogic.events)
            {
                CleanedEvents.Add(i.Key, i.Value.Replace("'Bugs'", "Item_Bugs").Replace("'Fish'", "Item_Fish").Replace("'Fairy'", "Item_Fairy"));
            }
            HelperLogic.events = CleanedEvents;

            Logic.Add(HelperLogic);

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

            foreach(var Region in RegionLogic.Keys.ToArray())
            {
                foreach(var i in MasterLogic)
                {
                    if (i.Key.Contains("->"))
                    {
                        var Data = i.Key.Split(new string[] { "->" }, StringSplitOptions.None);
                        if (Data[1].Trim() == Region && !RegionLogic[Region].Contains(i.Key)) { RegionLogic[Region].Add(i.Key); }
                    }
                }
            }

            foreach(var Region in RegionLogic)
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
            foreach(var i in RefFile)
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
                LogicFormat = "JSON",
                LogicVersion = 1,
                LogicDictionaryList = new List<LogicObjects.LogicDictionaryEntry>()
            };
            foreach(var i in Entrances)
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
                    SpoilerItem = new string[] { i[1] , i[1].Replace(" ", "_") },
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

        public static void AddExtraItems(LogicObjects.LogicDictionary dict, LogicObjects.LogicFile Logic)
        {
            string LogFolder = @"C:\Users\drumm\Downloads\OOTR Logs";

            Dictionary<string, int> ItemAmontAverages = new Dictionary<string, int>();
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            foreach (var i in Directory.GetFiles(LogFolder))
            {
                var Log = JsonConvert.DeserializeObject<SpoilerLog>(File.ReadAllText(i), jsonSerializerSettings);
                foreach (var j in Log.item_pool.Keys)
                {
                    if (ItemAmontAverages.ContainsKey(j))
                    {
                        if (ItemAmontAverages[j] < Log.item_pool[j]) { ItemAmontAverages[j] = Log.item_pool[j]; }
                    }
                    else { ItemAmontAverages.Add(j, Log.item_pool[j]); }
                }
            }

            string[] IgnoredJunk = new string[]
            {
                "Rupees (",
                "Deku Seeds (",
                "Deku Nuts (",
                "Buy Bombs (",
                "Bombs (",
                "Buy Arrows (",
                "Arrows (",
                "Deku Stick ("
            };

            foreach (var i in ItemAmontAverages.OrderBy(x => x.Key))
            {
                int CountInVanilla = dict.LogicDictionaryList.Where(x => x.SpoilerItem!=null && x.SpoilerItem.Contains(i.Key)).Count();
                if (i.Value + 1 > CountInVanilla && !IgnoredJunk.Where(x => i.Key.StartsWith(x)).Any())
                {
                    Console.WriteLine($"Missing {i.Value - CountInVanilla} {i.Key}. {CountInVanilla}/{i.Value} ");
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

            List<string> allBottles = new List<string>() { "Bottle with Red Potion", "Bottle with Green Potion", "Bottle with Blue Potion", "Bottle with Fairy", "Bottle with Fish", "Bottle with Blue Fire", "Bottle with Bugs", "Bottle with Big Poe", "Bottle with Poe", "Bottle", "Bottle with Milk" };

            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"has_bottle",
                ConditionalItems = allBottles.Select(x => new List<string>() { x }).ToList()
            });

            foreach (var i in allBottles.Where(x => x != "Bottle"))
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

            foreach (var i in UsablilityLogicItems)
            {
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
                {
                    Id = i.Key,
                    ConditionalItems = parser.ConvertLogicToConditionalString(i.Value.Replace(" and ", " & "))
                });
            }

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
            dict.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
            {
                DictionaryName = $"AdultWallet",
                FakeItem = true,
                WalletCapacity = 200
            });
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem
            {
                Id = $"AdultWallet",
                ConditionalItems = new List<List<string>> { new List<string> { "Progressive Wallet x2" } }
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
                ConditionalItems = new List<List<string>> { new List<string> { "Progressive Wallet x3" } }
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
                ConditionalItems = new List<List<string>> { new List<string> { "Progressive Wallet x4" } }
            });



        }

        public static void AddReverseOptionEntries(LogicObjects.LogicFile Logic)
        {
            LogicParser parser = new LogicParser();
            Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem 
            { 
                Id = $"damage_multiplier != ohko" ,
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
                ConditionalItems = parser.ConvertLogicToConditionalString("open_forest == deku | open_forest == open")
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
            List<string> Dungeons = new List<string>()
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
                    ItemSubType = $"Option{d.Replace(" ","")}Layout",
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
            StageOption("open_forest", "closed", new string[] { "deku", "open" });
            StageOption("open_kakariko", "closed", new string[] { "zelda", "open" });
            StageOption("shuffle_ganon_bosskey", "other", new string[] { "medallions", "stones", "tokens", "dungeons", "remove" });
            StageOption("shuffle_scrubs", "off", new string[] { "on" });
            StageOption("Starting Age", "child", new string[] { "adult" });
            StageOption("zora_fountain", "closed", new string[] { "adult", "open" });

            void StageOption(string DictNameBase, string DefaultOption, string[] OtherOptions)
            {
                CommitOption(DictNameBase, DefaultOption, textInfo.ToTitleCase(DictNameBase.Replace("_", "")) );
                foreach(var i in OtherOptions)
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
                Logic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = $"skipped_trials[{DictNameBase}]"});
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

            for(var i = 0; i < countOptions.Count(); i++)
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
                foreach(var cond in entry.ConditionalItems)
                {
                    foreach(var i in cond)
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

            foreach(var i in Additions) { Logic.Logic.Add(i); }

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


        //Spoiler=====================================================================================================================================================

        public static void ReadDynamicValue(Dictionary<string, dynamic> Dict)
        {
            foreach (KeyValuePair<string, dynamic> pair in Dict)
            {
                Type T = pair.Value.GetType();
                Console.WriteLine(T.GetGenericArguments()[0].ToString());
            }
        }

        public static void ReadSpoiler()
        {
            string SpoilerLogFile = "";
            List<string> SpoilerLogFileLocations =  new List<string> 
            { 
                @"D:\Games\Emulated Games\Emulator\Wii\Dolphin-x64 Ocarina of Time Randomizer\Roms\OoT_D98CE_JKI93B07ON_Spoiler.json",
                @"C:\CodeTest\OoT_D98CE_JKI93B07ON_Spoiler.json"
            };
            foreach (var i in SpoilerLogFileLocations)
            {
                if (File.Exists(i))
                {
                    SpoilerLogFile = i;
                    break;
                }
            }

            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            var Log = JsonConvert.DeserializeObject<SpoilerLog>(File.ReadAllText(SpoilerLogFile), jsonSerializerSettings);

            var Entrances = Log.entrances.Keys.ToList();

            foreach (var i in Log.entrances)
            {
                string sValue = i.Value as string;
                if (sValue != null)
                {
                    string FullExitValue = sValue;
                    foreach(var j in Entrances)
                    {
                        var data = j.Split(new string[] { " -> " }, StringSplitOptions.None);
                        if (data[0].Trim() == sValue)
                        {
                            FullExitValue = (data[0] + " -> " + data[1] + "," + data[0] + " -> " + data[1] + "," + data[1] + " <- " + data[0] + "," + data[1] + " -> " + data[0]);
                            break;
                        }
                    }
                    //Console.WriteLine(FullExitValue);
                }
                else
                {
                    RegionExit R = i.Value.ToObject<RegionExit>();
                    //Console.WriteLine(R.region + " -> " + R.from + "," + R.region + " -> " + R.from + "," + R.from + " <- " + R.region + "," + R.from + " -> " + R.region);
                }
            }

            var VanillaLocations = JsonConvert.DeserializeObject<SpoilerLog>(File.ReadAllText(@"C:\CodeTest\Vanilla_Locations.json"), jsonSerializerSettings);
            
            //Console.WriteLine("================================================================");
            //Console.WriteLine("Vanilla Lcoation Not In Spoiler");
            foreach(var i in VanillaLocations.locations)
            {
                if (Log.locations.Where(x => i.Key == x.Key).Count() == 0)
                {
                    //Console.WriteLine(i.Key);
                }
            }
            //Console.WriteLine("================================================================");
            //Console.WriteLine("Spoiler Lcoations Not in Vanilla list");
            foreach(var i in Log.locations)
            {
                if (VanillaLocations.locations.Where(x => i.Key == x.Key).Count() == 0)
                {
                    //Console.WriteLine(i.Key);
                }
            }

            foreach(var i in Log.item_pool)
            {
                int CountInVanilla = VanillaLocations.locations.Where(x => x.Value == i.Key).Count();
                if (i.Value > CountInVanilla)
                {
                    Console.WriteLine($"{i.Value} {i.Key} In Spoiler, Missing {i.Value - CountInVanilla}");
                }
            }

            string[] allowed_tricks = Log.settings["allowed_tricks"].ToObject<string[]>();

        }

    }
}
