using Newtonsoft.Json;
using YamlDotNet.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MMR_Tracker_V2;
using System.Text.RegularExpressions;
using MMR_Tracker.Forms;
using MMR_Tracker.Class_Files;
using System.Diagnostics;
using System.Windows.Forms;

namespace MMR_Tracker.Other_Games
{
    class WWRCheck
    {
        [JsonProperty("original item")]
        public string original_item { get; set; }
        public string types { get; set; }
        public string Need { get; set; }
    }

    class WindWakerRando
    {
        public static Dictionary<string, string> TempleShorthand = new Dictionary<string, string>()
        {
            {"DRC"  ,   "Dragon Roost Cavern"},
            {"FW"   ,   "Forbidden Woods"},
            {"ET"   ,   "Earth Temple"},
            {"TotG" ,   "Tower of the Gods"},
            {"WT"   ,   "Wind Temple"},
            {"FF"   ,   "Forsaken Fortress"}
        };

        public static string[] Ignoredjunk = new string[]
        {
            "Pear",
            "Rupee",
            "Jelly",
            "Bait",
            "Crest",
            "Necklace"
        };

        public static Dictionary<string, string> CategoryOptionDict = new Dictionary<string, string>()
        {
            {"Dungeon",                "progression_dungeons"},
            {"Great Fairy",            "progression_great_fairies"},
            {"Puzzle Secret Cave",     "progression_puzzle_secret_caves"},
            {"Combat Secret Cave",     "progression_combat_secret_caves"},
            {"Short Sidequest",        "progression_short_sidequests"},
            {"Long Sidequest",         "progression_long_sidequests"},
            {"Spoils Trading",         "progression_spoils_trading"},
            {"Minigame",               "progression_minigames"},
            {"Free Gift",              "progression_free_gifts"},
            {"Mail",                   "progression_mail"},
            {"Raft",                   "progression_platforms_rafts"},
            {"Platform",               "progression_platforms_rafts"},
            {"Submarine",              "progression_submarines"},
            {"Eye Reef Chest",         "progression_eye_reef_chests"},
            {"Gunboat",                "progression_big_octos_gunboats"},
            {"Big Octo",               "progression_big_octos_gunboats"},
            {"Expensive Purchase",     "progression_expensive_purchases"},
            {"Island Puzzle",          "progression_island_puzzles"},
            {"Other Chest",            "progression_misc"},
            {"Misc",                   "progression_misc"},
            {"Tingle Chest",           "progression_tingle_chests"},
            {"Battlesquid",            "progression_battlesquid"},
            {"Savage Labyrinth",       "progression_savage_labyrinth"},
            {"Sunken Treasure Triforce", "progression_triforce_charts"},
            {"Sunken Treasure",         "progression_treasure_charts"},
            {"Consumables only",        "Never_enabled"},
            {"No progression",          "Never_enabled" }
        };

        public static void TestWWR()
        {
            LogicParser Parser = new LogicParser();
            System.Net.WebClient wc = new System.Net.WebClient();
            string Item_Location_file = wc.DownloadString("https://raw.githubusercontent.com/LagoLunatic/wwrando/master/logic/item_locations.txt");

            Dictionary<string, WWRCheck> Checks = new Dictionary<string, WWRCheck>();
            Checks = JsonConvert.DeserializeObject<Dictionary<string, WWRCheck>>(YamlToJson(wc.DownloadString("https://raw.githubusercontent.com/LagoLunatic/wwrando/master/logic/item_locations.txt")));

            Dictionary<string, string> Helpers = new Dictionary<string, string>();
            Helpers = JsonConvert.DeserializeObject<Dictionary<string, string>>(YamlToJson(wc.DownloadString("https://raw.githubusercontent.com/LagoLunatic/wwrando/master/logic/macros.txt")));

            char[] delims = new[] { '\r', '\n' };
            string[] Lines = Item_Location_file.Split(delims,StringSplitOptions.RemoveEmptyEntries);

            //Get all the entrys that were commented out, their locations are not randomized but the items are still in the pool
            string StoredLocation = "";
            foreach (var i in Lines)
            {
                if (i.Length < 1 || i[0] != '#') { continue; }
                var CleanLine = i.Substring(1);
                CleanLine = Utility.RemoveCommentLines(CleanLine).Trim();

                if (CleanLine.Contains(" - ") && CleanLine.EndsWith(":"))
                {
                    StoredLocation = CleanLine;
                }
                if (CleanLine.StartsWith("Original item: ") && StoredLocation != "" && !CleanLine.EndsWith("got deciphered"))
                {
                    Checks.Add(StoredLocation.Replace(":", ""), new WWRCheck { Need = "", original_item = CleanLine.Replace("Original item: ", "").Trim().Split(',')[0], types = "ItemOnly" });
                    StoredLocation = "";
                }
            }

            LogicObjects.LogicDictionary WWRDictionary = new LogicObjects.LogicDictionary
            {
                DefaultWalletCapacity = 200,
                GameCode = "WWR",
                LogicFormat = "json",
                LogicVersion = 1,
                LogicDictionaryList = new List<LogicObjects.LogicDictionaryEntry>()
            };
            LogicObjects.LogicFile WWRLogic = new LogicObjects.LogicFile
            {
                GameCode = "WWR",
                Version = 1,
                Logic = new List<LogicObjects.JsonFormatLogicItem>()
            };


            //Parse the "Can Access Other Locations" Entries. They basically look for if you meet the requirements for the listed location
            //We can just replace it with the requirements for the listed location
            foreach (var i in Checks.Keys.ToArray())
            {
                string Logic = Checks[i].Need;
                var AtEntries = Regex.Matches(Logic, "Can Access Other Location \".*?\"");
                foreach (Match r in AtEntries)
                {
                    Console.WriteLine("=========================");
                    var LocationToMatchLogic = r.Value.Replace("Can Access Other Location \"", "").Replace("\"", "");
                    Console.WriteLine(r.Groups[0] + " Was at other location entry");
                    if (Checks.ContainsKey(LocationToMatchLogic))
                    {
                        Console.WriteLine(LocationToMatchLogic + " Was replaced by");
                        Console.WriteLine($"({Checks[LocationToMatchLogic].Need})");
                        Logic = Logic.Replace(r.Groups[0].Value, $"({Checks[LocationToMatchLogic].Need})");
                    }
                }
                Checks[i].Need = Logic;
            }

            foreach (var i in Helpers.Keys.ToArray())
            {
                string Logic = Helpers[i];
                var AtEntries = Regex.Matches(Logic, "Can Access Other Location \".*?\"");
                foreach (Match r in AtEntries)
                {
                    Console.WriteLine("=========================");
                    var LocationToMatchLogic = r.Value.Replace("Can Access Other Location \"", "").Replace("\"", "");
                    Console.WriteLine(r.Groups[0] + " Was at other location entry");
                    if (Checks.ContainsKey(LocationToMatchLogic))
                    {
                        Console.WriteLine(LocationToMatchLogic + " Was replaced by");
                        Console.WriteLine($"({Checks[LocationToMatchLogic].Need})");
                        Logic = Logic.Replace(r.Groups[0].Value, $"({Checks[LocationToMatchLogic].Need})");
                    }
                }
                Helpers[i] = Logic;
            }

            //Read the data from the checks list and helpers list and parse it to a logic object and dictionary object
            foreach (var i in Checks)
            {
                string Logicname = (i.Value.original_item + "From" + i.Key);
                Logicname = Regex.Replace(Logicname, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);

                var dictentry = new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = Logicname,
                    FakeItem = false,
                    ItemSubType = "Item",
                };

                var LocationData = Regex.Split(i.Key, " - ");
                var Area = LocationData[0];

                if (i.Value.original_item.Contains("Small Key") || i.Value.original_item.Contains("Big Key") || i.Value.original_item.Contains("Dungeon Map") || i.Value.original_item.Contains("Compass")) 
                { 
                    dictentry.ItemName = $"{Area} {i.Value.original_item}";
                    dictentry.SpoilerItem = new string[] { $"{Area} {i.Value.original_item}", $"{TempleShorthand.FirstOrDefault(x => x.Value == Area).Key} {i.Value.original_item}" };
                }
                else
                {
                    dictentry.ItemName = i.Value.original_item;
                    dictentry.SpoilerItem = new string[] { i.Value.original_item };
                }

                if (i.Value.types != "ItemOnly")
                {
                    dictentry.LocationArea = Area.Trim();
                    dictentry.LocationName = i.Key.Trim();
                    dictentry.SpoilerLocation = new string[] { i.Key.Trim() };
                    dictentry.LocationCategory = i.Value.types.Split(',').Select(x => x.Trim()).ToArray();
                }

                WWRDictionary.LogicDictionaryList.Add(dictentry);

                var LogicEntry = new LogicObjects.JsonFormatLogicItem();
                LogicEntry.Id = Logicname;
                if (i.Value.types != "ItemOnly" && !string.IsNullOrWhiteSpace(i.Value.Need) && i.Value.Need.Trim() != "Nothing")
                {

                    LogicEntry.ConditionalItems = Parser.ConvertLogicToConditional(i.Value.Need).Select(v => v.Where(x => x != "2").ToList()).ToList();
                }

                WWRLogic.Logic.Add(LogicEntry);
            }

            foreach (var i in Helpers)
            {

                var LogicEntry = new LogicObjects.JsonFormatLogicItem();
                LogicEntry.Id = i.Key;
                if (!string.IsNullOrWhiteSpace(i.Value) && i.Value.Trim() != "Nothing")
                {
                    string Logic = i.Value;

                    LogicEntry.ConditionalItems = Parser.ConvertLogicToConditional(Logic).Select(v => v.Where(x => x != "2").ToList()).ToList();
                }
                WWRLogic.Logic.Add(LogicEntry);
            }

            //Add all the entries that ask for x of an item
            AddProgressiveItemEntries(WWRDictionary, WWRLogic);

            //Add all the entries that ask for x of a dungeon key
            AddDungeonKeyEntries(WWRDictionary, WWRLogic);

            //add option entries that effect logic as starting items
            CreateOptions(WWRDictionary, WWRLogic);

            //Turn the "Can access *entrance*" logic helpers into real items for entrance rando
            CreateEntranceChecks(WWRDictionary);

            //Make the "Chart for Island *"checks real items since they can be randomized
            CreateTreasureChartChecks(WWRDictionary);

            //Make an entry that is impossible to aquire. Not sure how the tracker will handle this
            WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = "Impossible", RequiredItems = new List<string> { "Impossible" } });

            //Default logic uses item names in some cases. Convert those over to the proper dictionary names
            List<string> MissingItems = new List<string>();
            ParseLogicEntryItemNamestoDictNames(WWRDictionary, WWRLogic, MissingItems);
            Console.WriteLine("");
            Console.WriteLine("The following logic items could not be parsed");
            foreach (var i in MissingItems.Distinct().OrderBy(x => x)) { Console.WriteLine(i); }

            foreach (var Logicentry in WWRLogic.Logic)
            {
                SkywardSwordRando.ExtractRequirements(Logicentry);
            }


            JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            string[] LogicText = JsonConvert.SerializeObject(WWRLogic, _jsonSerializerOptions).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            LogicObjects.MainTrackerInstance.RawLogicFile = LogicText;
            LogicObjects.MainTrackerInstance.LogicDictionary = WWRDictionary;
            LogicEditing.PopulateTrackerInstance(LogicObjects.MainTrackerInstance);
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);

            MainInterface.CurrentProgram.FormatMenuItems();
            MainInterface.CurrentProgram.ResizeObject();
            MainInterface.CurrentProgram.PrintToListBox();
            Tools.UpdateTrackerTitle();
            Tools.SaveFilePath = "";

            File.WriteAllLines("Wind Waker Rando (Beta).txt", LogicEditing.WriteLogicToJson(LogicObjects.MainTrackerInstance));
            File.WriteAllText("WWR V1 json Logic Dictionary.json", JsonConvert.SerializeObject(LogicObjects.MainTrackerInstance.LogicDictionary, _jsonSerializerOptions));
            Process.Start(Directory.GetCurrentDirectory());

            HandleWWRSpoilerLog(LogicObjects.MainTrackerInstance, File.ReadAllLines(@"C:\Users\drumm\Downloads\WWR Logs\WW Random LushRoundWriting - Spoiler Log.txt"));
        }

        public static void PrintDictObject(LogicObjects.LogicDictionary WWRDictionary)
        {
            foreach (var i in WWRDictionary.LogicDictionaryList)
            {
                Console.WriteLine("===================");
                Console.WriteLine(i.DictionaryName);
                Console.WriteLine(i.LocationName);
                Console.WriteLine(i.LocationArea);
                Console.WriteLine(i.ItemName);
            }
        }

        public static void PrintLogicObject(LogicObjects.LogicFile WWRLogic)
        {
            int LargestConditional = -1;
            for (var i = 0; i < WWRLogic.Logic.Count(); i++)
            {
                var entry = WWRLogic.Logic[i];
                Console.WriteLine("================");
                Console.WriteLine($"ID: {entry.Id}");
                if (entry.RequiredItems != null && entry.RequiredItems.Any()) { Console.WriteLine($"Required:\n  {string.Join(", ", entry.RequiredItems)}"); }
                if (entry.ConditionalItems != null && entry.ConditionalItems.Any())
                {
                    Console.WriteLine("Conditionals:\n  " + string.Join("\n  ", entry.ConditionalItems.Select(x => string.Join(", ", x))));
                    Console.WriteLine($"Conditional Length: {entry.ConditionalItems.Count()}");
                    if (entry.ConditionalItems.Count() > LargestConditional) { LargestConditional = entry.ConditionalItems.Count(); }
                }
            }
            Console.WriteLine("Largest Conditional was: " + LargestConditional);
        }

        public static void CreateOptions(LogicObjects.LogicDictionary WWRDictionary, LogicObjects.LogicFile WWRLogic)
        {
            WWRDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
            {
                DictionaryName = "Option \"skip_rematch_bosses\" Enabled",
                LocationName = "Skip Boss Rematches",
                ItemName = "Enabled",
                ItemSubType = $"Option_skip_rematch_bosses",
                LocationArea = "%Randomizer Options%"
            });
            WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = $"Option \"skip_rematch_bosses\" Enabled" });
            WWRDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
            {
                DictionaryName = "Option \"skip_rematch_bosses\" Disabled",
                ItemName = "Disabled",
                ItemSubType = $"Option_skip_rematch_bosses"
            });
            WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = $"Option \"skip_rematch_bosses\" Disabled" });

            WWRDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
            {
                DictionaryName = "Option \"sword_mode\" Is \"start_with\"",
                LocationName = "Sword Mode",
                ItemName = "Start With Hero's Sword",
                ItemSubType = $"Option_sword_mode",
                LocationArea = "%Randomizer Options%"
            });
            WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = $"Option \"sword_mode\" Is \"start_with\"" });
            WWRDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
            {
                DictionaryName = "Option \"sword_mode\" Is \"None\"",
                ItemName = "None",
                ItemSubType = $"Option_sword_mode",
            });
            WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = $"Option \"sword_mode\" Is \"None\"" });
            WWRDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
            {
                DictionaryName = "Option \"sword_mode\" Is \"Swordless\"",
                ItemName = "Swordless",
                ItemSubType = $"Option_sword_mode",
            });
            WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = $"Option \"sword_mode\" Is \"Swordless\"" });

            WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem 
            {
                Id = $"Option \"sword_mode\" Is Not \"Swordless\"" , 
                ConditionalItems =  new List<List<string>> { new List<string> { $"Option \"sword_mode\" Is \"None\"" }, new List<string> { $"Option \"sword_mode\" Is \"start_with\"" } }
            });
        }

        public static void ParseLogicEntryItemNamestoDictNames(LogicObjects.LogicDictionary WWRDictionary, LogicObjects.LogicFile WWRLogic, List<string> MissingItems)
        {
            LogicParser Parser = new LogicParser();
            for (var i = 0; i < WWRLogic.Logic.Count(); i++)
            {
                var entry = WWRLogic.Logic[i];

                if (entry.RequiredItems.Any())
                {
                    for (var j = 0; j < entry.RequiredItems.Count(); j++)
                    {
                        var req = entry.RequiredItems[j];
                        var MatchingLogicNames = WWRLogic.Logic.Where(x => x.Id == req);
                        if (!MatchingLogicNames.Any())
                        {
                            MissingItems.Add(req);
                        }
                    }
                }

                if (entry.ConditionalItems.Any())
                {
                    bool RecompileConditionals = false;
                    string ActionLog = "";
                    for (var j = 0; j < entry.ConditionalItems.Count(); j++)
                    {
                        var set = entry.ConditionalItems[j];
                        for (var k = 0; k < set.Count(); k++)
                        {
                            var cond = set[k];
                            var MatchingLogicNames = WWRLogic.Logic.Where(x => x.Id == cond);
                            if (!MatchingLogicNames.Any())
                            {
                                var MatchingItemNames = WWRDictionary.LogicDictionaryList.Where(x => x.ItemName == cond);
                                if (!MatchingItemNames.Any())
                                {
                                    MissingItems.Add(cond);
                                }
                                else if (MatchingItemNames.Count() > 1)
                                {
                                    string ItemGroup = $"({string.Join(" | ", MatchingItemNames.Select(x => x.DictionaryName))})";
                                    ActionLog = ActionLog + ($"\nConditional Edited [{cond}] -> [{ItemGroup}]");
                                    entry.ConditionalItems[j][k] = ItemGroup;
                                    RecompileConditionals = true;
                                }
                                else
                                {
                                    ActionLog = ActionLog + ($"\nConditional Edited [{cond}] -> [{MatchingItemNames.ToList()[0].DictionaryName}]");
                                    entry.ConditionalItems[j][k] = MatchingItemNames.ToList()[0].DictionaryName;
                                }
                            }
                        }
                    }
                    if (RecompileConditionals)
                    {
                        string ConditionalToString = $"({string.Join(") | (", WWRLogic.Logic[i].ConditionalItems.Select(x => string.Join(" & ", x)))})";
                        WWRLogic.Logic[i].ConditionalItems = Parser.ConvertLogicToConditional(ConditionalToString);
                        ActionLog = ActionLog + ($"\nRecompiling Conditional [{ConditionalToString}]");
                    }
                    if (ActionLog != "")
                    {
                        Console.WriteLine($"=========================\nChecking Contional for: [{entry.Id}]{ActionLog}");
                    }

                }
            }
        }

        public static void AddProgressiveItemEntries(LogicObjects.LogicDictionary WWRDictionary, LogicObjects.LogicFile WWRLogic)
        {
            Dictionary<string, List<string>> ProgressiveItemData = new Dictionary<string, List<string>>();

            //Get potential prgressive items based on item name, A bit to loose for my liking but it seems to get what it needs
            foreach (var i in WWRDictionary.LogicDictionaryList)
            {
                if (i.ItemName.ToLower().Contains("bomb bag"))
                {
                    if (!ProgressiveItemData.ContainsKey("Progressive Bomb Bag")) { ProgressiveItemData.Add("Progressive Bomb Bag", new List<string>()); }
                    ProgressiveItemData["Progressive Bomb Bag"].Add(i.DictionaryName);
                }
                if (i.ItemName.ToLower().Contains("bow") || i.ItemName.ToLower().Contains("fire and ice arrows") || i.ItemName.ToLower().Contains("light arrow"))
                {
                    if (!ProgressiveItemData.ContainsKey("Progressive Bow")) { ProgressiveItemData.Add("Progressive Bow", new List<string>()); }
                    ProgressiveItemData["Progressive Bow"].Add(i.DictionaryName);
                }
                if (i.ItemName.ToLower().Contains("picto"))
                {
                    if (!ProgressiveItemData.ContainsKey("Progressive Picto Box")) { ProgressiveItemData.Add("Progressive Picto Box", new List<string>()); }
                    ProgressiveItemData["Progressive Picto Box"].Add(i.DictionaryName);
                }
                if (i.ItemName.ToLower().Contains("quiver"))
                {
                    if (!ProgressiveItemData.ContainsKey("Progressive Quiver")) { ProgressiveItemData.Add("Progressive Quiver", new List<string>()); }
                    ProgressiveItemData["Progressive Quiver"].Add(i.DictionaryName);
                }
                if (i.ItemName.ToLower().Contains("shield"))
                {
                    if (!ProgressiveItemData.ContainsKey("Progressive Shield")) { ProgressiveItemData.Add("Progressive Shield", new List<string>()); }
                    ProgressiveItemData["Progressive Shield"].Add(i.DictionaryName);
                }
                if (i.ItemName.ToLower().Contains("sword"))
                {
                    if (!ProgressiveItemData.ContainsKey("Progressive Sword")) { ProgressiveItemData.Add("Progressive Sword", new List<string>()); }
                    ProgressiveItemData["Progressive Sword"].Add(i.DictionaryName);
                }
                if (i.ItemName.ToLower().Contains("wallet"))
                {
                    if (!ProgressiveItemData.ContainsKey("Progressive Wallet")) { ProgressiveItemData.Add("Progressive Wallet", new List<string>()); }
                    ProgressiveItemData["Progressive Wallet"].Add(i.DictionaryName);
                }
            }
            //Convert item names to their progressive item names since that what the rando uses internally. Add the progressive item name to the Spoiler item list
            foreach (var i in WWRDictionary.LogicDictionaryList)
            {
                var Pdata = ProgressiveItemData.Where(d => d.Value.Contains(i.DictionaryName)).ToArray();
                if (Pdata.Any())
                {
                    WWRDictionary.LogicDictionaryList.Find(x => x.DictionaryName == i.DictionaryName).ItemName = Pdata[0].Key;
                    WWRDictionary.LogicDictionaryList.Find(x => x.DictionaryName == i.DictionaryName).SpoilerItem = i.SpoilerItem.Concat(new string[] { Pdata[0].Key }).ToArray();
                }
            }
            //Add the x of y item entries. add an entry for each item based on the number of items found. Creates unused entries but it's easier
            foreach(var i in ProgressiveItemData)
            {
                var CountOfItems = i.Value.Count();
                for (var c = 1; c < CountOfItems+1; c++)
                {
                    string CombinationEntry = $"MMRTCombinations{c}";
                    if (WWRLogic.Logic.Find(x => x.Id == CombinationEntry) == null && c > 1 && c != CountOfItems)
                    {
                        WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = CombinationEntry });
                    }
                    WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem { 
                        Id = $"{i.Key} x{c}",
                        RequiredItems = c == 1 ? new List<string>() : (c == CountOfItems ? i.Value : new List<string>() { CombinationEntry }),
                        ConditionalItems = c == CountOfItems ? new List<List<string>>() : i.Value.Select(x => new List<string> { x }).ToList()
                    });
                }
            }


        }

        public static void AddDungeonKeyEntries(LogicObjects.LogicDictionary WWRDictionary, LogicObjects.LogicFile WWRLogic)
        {
            var KeyDict = new Dictionary<string, int>
                {
                    { "DRC", 4 },
                    { "ET", 3 },
                    { "FW", 1 },
                    { "TotG", 2 },
                    { "WT", 2 },
                };

            foreach (var i in KeyDict)
            {
                for (var j = 1; j < i.Value + 1; j++)
                {
                    string CombinationEntry = $"MMRTCombinations{j}";
                    if (WWRLogic.Logic.Find(x => x.Id == CombinationEntry) == null && j > 1 && j != i.Value)
                    {
                        WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem { Id = CombinationEntry });
                    }

                    var Keys = WWRDictionary.LogicDictionaryList.Where(x => x.ItemName == $"{TempleShorthand[i.Key]} Small Key");

                    WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem
                    {
                        Id = $"{i.Key} Small Key x{j}",
                        RequiredItems = (j == 1) ? new List<string>() : (j == i.Value ? Keys.Select(x => x.DictionaryName).ToList() : new List<string>() { CombinationEntry }) ,
                        ConditionalItems = j == i.Value ? new List<List<string>>() : Keys.Select(x => new List<string> { x.DictionaryName }).ToList()
                    });
                }
                var BigKeys = WWRDictionary.LogicDictionaryList.Where(x => x.ItemName == $"{TempleShorthand[i.Key]} Big Key");

                WWRLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem
                {
                    Id = $"{i.Key} Big Key",
                    ConditionalItems = BigKeys.Select(x => new List<string> { x.DictionaryName }).ToList()
                });
            }

        }

        public static void CreateEntranceChecks(LogicObjects.LogicDictionary WWRDictionary)
        {
            Dictionary<string, string> EntranceMap = new Dictionary<string, string>
            {
                {"Secret Cave Entrance on Outset Island","Savage Labyrinth"},
                {"Secret Cave Entrance on Dragon Roost Island","Dragon Roost Island Secret Cave"},
                {"Secret Cave Entrance on Fire Mountain","Fire Mountain Secret Cave"},
                {"Secret Cave Entrance on Ice Ring Isle","Ice Ring Isle Secret Cave"},
                {"Secret Cave Entrance on Private Oasis","Cabana Labyrinth"},
                {"Secret Cave Entrance on Needle Rock Isle","Needle Rock Isle Secret Cave"},
                {"Secret Cave Entrance on Angular Isles","Angular Isles Secret Cave"},
                {"Secret Cave Entrance on Boating Course","Boating Course Secret Cave"},
                {"Secret Cave Entrance on Stone Watcher Island","Stone Watcher Island Secret Cave"},
                {"Secret Cave Entrance on Overlook Island","Overlook Island Secret Cave"},
                {"Secret Cave Entrance on Bird's Peak Rock","Bird's Peak Rock Secret Cave"},
                {"Secret Cave Entrance on Pawprint Isle","Pawprint Isle Chuchu Cave"},
                {"Secret Cave Entrance on Pawprint Isle Side Isle","Pawprint Isle Wizzrobe Cave"},
                {"Secret Cave Entrance on Diamond Steppe Island","Diamond Steppe Island Warp Maze Cave"},
                {"Secret Cave Entrance on Bomb Island","Bomb Island Secret Cave"},
                {"Secret Cave Entrance on Rock Spire Isle","Rock Spire Isle Secret Cave"},
                {"Secret Cave Entrance on Shark Island","Shark Island Secret Cave"},
                {"Secret Cave Entrance on Cliff Plateau Isles","Cliff Plateau Isles Secret Cave"},
                {"Secret Cave Entrance on Horseshoe Island","Horseshoe Island Secret Cave"},
                {"Secret Cave Entrance on Star Island","Star Island Secret Cave"},
                {"Dungeon Entrance On Dragon Roost Island","Dragon Roost Cavern"},
                {"Dungeon Entrance In Forest Haven Sector","Forbidden Woods"},
                {"Dungeon Entrance In Tower of the Gods Sector","Tower of the Gods"},
                {"Dungeon Entrance On Headstone Island","Earth Temple"},
                {"Dungeon Entrance On Gale Isle","Wind Temple"}
            };
            foreach(var entrance in EntranceMap)
            {
                WWRDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = $"Can Access {entrance.Key}",
                    LocationName = entrance.Key,
                    ItemName = entrance.Value,
                    ItemSubType = "Entrance",
                    LocationArea = entrance.Key.Contains("Secret Cave Entrance") ? "Secret Cave Entrances" : "Dungeon Entrances",
                    SpoilerLocation = new string[] { entrance.Key },
                    SpoilerItem = new string[] { entrance.Value }
                });
            }

        }

        public static void CreateTreasureChartChecks(LogicObjects.LogicDictionary WWRDictionary)
        {
            Dictionary<string, string> Chartdata = new Dictionary<string, string>()
            {
                {"Treasure Chart 25", "Forsaken Fortress"},
                {"Treasure Chart 7" , "Star Island"},
                {"Treasure Chart 24", "Northern Fairy Island"},
                {"Triforce Chart 2" , "Gale Isle"},
                {"Treasure Chart 11", "Crescent Moon Island"},
                {"Triforce Chart 7" , "Seven-Star Isles"},
                {"Treasure Chart 13", "Overlook Island"},
                {"Treasure Chart 41", "Four-Eye Reef"},
                {"Treasure Chart 29", "Mother and Child Isles"},
                {"Treasure Chart 22", "Spectacle Island"},
                {"Treasure Chart 18", "Windfall Island"},
                {"Treasure Chart 30", "Pawprint Isle"},
                {"Treasure Chart 39", "Dragon Roost Island"},
                {"Treasure Chart 19", "Flight Control Platform"},
                {"Treasure Chart 8" , "Western Fairy Island"},
                {"Treasure Chart 2" , "Rock Spire Isle"},
                {"Treasure Chart 10", "Tingle Island"},
                {"Treasure Chart 26", "Northern Triangle Island"},
                {"Treasure Chart 3" , "Eastern Fairy Island"},
                {"Treasure Chart 37", "Fire Mountain"},
                {"Treasure Chart 27", "Star Belt Archipelago"},
                {"Treasure Chart 38", "Three-Eye Reef"},
                {"Treasure Chart 1" , "Private Oasis"},
                {"Treasure Chart 21", "Cyclops Reef"},
                {"Treasure Chart 6" , "Six-Eye Reef"},
                {"Treasure Chart 14", "Tower of the Gods"},
                {"Treasure Chart 34", "Eastern Triangle Island"},
                {"Treasure Chart 5" , "Thorned Fairy Island"},
                {"Treasure Chart 28", "Needle Rock Isle"},
                {"Treasure Chart 35", "Islet of Steel"},
                {"Triforce Chart 3" , "Stone Watcher Island"},
                {"Triforce Chart 6" , "Southern Triangle Island"},
                {"Triforce Chart 1" , "Greatfish Isle"},
                {"Treasure Chart 20", "Bomb Island"},
                {"Treasure Chart 36", "Bird's Peak Rock"},
                {"Treasure Chart 23", "Diamond Steppe Island"},
                {"Treasure Chart 12", "Five-Eye Reef"},
                {"Treasure Chart 16", "Shark Island"},
                {"Treasure Chart 4" , "Southern Fairy Island"},
                {"Treasure Chart 17", "Ice Ring Isle"},
                {"Treasure Chart 31", "Forest Haven"},
                {"Triforce Chart 5" , "Cliff Plateau Isles"},
                {"Treasure Chart 9" , "Horseshoe Island"},
                {"Triforce Chart 4" , "Outset Island"},
                {"Treasure Chart 40", "Headstone Island"},
                {"Triforce Chart 8" , "Two-Eye Reef"},
                {"Treasure Chart 15", "Angular Isles"},
                {"Treasure Chart 32", "Boating Course"},
                {"Treasure Chart 33", "Five-Star Isles"}
            };

            int Counter = 1;
            foreach(var i in Chartdata)
            {
                WWRDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = $"Chart for Island {Counter}",
                    LocationName = i.Key + " Location",
                    ItemName = $"Sunken Treasure on {i.Value}",
                    ItemSubType = "Bottle",
                    LocationArea = "Treasure Chart Locations",
                    SpoilerLocation = new string[] { i.Key },
                    SpoilerItem = new string[] { i.Value }
                });
                Counter++;
            }
        }

        public static string YamlToJson(string Input)
        {
            // convert string/file to YAML object
            string ItemData = Input;
            var r = new StringReader(ItemData);
            var deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(r);
            JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            return JsonConvert.SerializeObject(yamlObject, _jsonSerializerOptions);
        }

        public static bool HandleWWRSpoilerLog(LogicObjects.TrackerInstance Instance, string[] Spoiler)
        {
            int LocationsStart = Array.IndexOf(Spoiler, "All item locations:") + 1;
            int EntrancesStart = Array.IndexOf(Spoiler, "Entrances:") + 1;
            int ChartsStart = Array.IndexOf(Spoiler, "Charts:") + 1;
            int OptionLine = Array.IndexOf(Spoiler, "Options selected:") + 1;
            var pattern = @"\,(?![^[]*\])";
            var query = Spoiler[OptionLine];
            var OptionLines = Regex.Split(query, pattern);
            List<int> UsedLocations = new List<int>();
            List<int> UsedItems = new List<int>();

            string CurrentArea = "";

            var WindWakerItem = Instance.Logic[Instance.DicNameToID["WindWakerFromDragonRoostIslandGiftfromKingofRedLions"]];
            var WindsRequiemItem = Instance.Logic[Instance.DicNameToID["WindsRequiemFromDragonRoostIslandWindShrine"]];
            var SongofPassingItem = Instance.Logic[Instance.DicNameToID["SongofPassingFromWindfallIslandTottTeachRhythm"]];
            var BalladofGalesItem = Instance.Logic[Instance.DicNameToID["BalladofGalesFromTheGreatSeaCyclos"]];
            var EnableSkipBossRematches = Instance.Logic[Instance.DicNameToID["Option \"skip_rematch_bosses\" Enabled"]];
            var DisableSkipBossRematches = Instance.Logic[Instance.DicNameToID["Option \"skip_rematch_bosses\" Disabled"]];
            var SwordModeStartWith = Instance.Logic[Instance.DicNameToID["Option \"sword_mode\" Is \"start_with\""]];
            var SwordModeNone = Instance.Logic[Instance.DicNameToID["Option \"sword_mode\" Is \"None\""]];
            var SwordModeSwordless = Instance.Logic[Instance.DicNameToID["Option \"sword_mode\" Is \"Swordless\""]];

            //Static starting Items
            WindWakerItem.ToggleStartingItem(true);
            WindsRequiemItem.ToggleStartingItem(true);
            SongofPassingItem.ToggleStartingItem(true);
            BalladofGalesItem.ToggleStartingItem(true);

            //Options that don't appear in the spoiler log unless enabled
            EnableSkipBossRematches.SpoilerRandom = DisableSkipBossRematches.ID;

            foreach(var i in Instance.LogicDictionary.LogicDictionaryList)
            {
                if (string.IsNullOrWhiteSpace(i.LocationName) || !Instance.DicNameToID.ContainsKey(i.DictionaryName)) { continue; }
                var LogicEntry = Instance.Logic[Instance.DicNameToID[i.DictionaryName]];
                if (i.LocationCategory != null && i.LocationCategory.Any())
                {
                    foreach(var c in i.LocationCategory) 
                    {
                        var Category = c;
                        if (c == "Sunken Treasure" && i.ItemName.StartsWith("Triforce Shard "))
                        {
                            Category = c + " Triforce";
                        }
                        if (!OptionLines.Select(x => x.Trim()).Contains(CategoryOptionDict[Category])) 
                        { 
                            LogicEntry.SetJunk(); 
                        } 

                    }
                }
            }
            if (!OptionLines.Select(x => x.Trim()).Contains("randomize_charts")) 
            {
                foreach (var entry in Instance.Logic.Where(x => x.DictionaryName.StartsWith("Chart for Island "))) 
                {
                    entry.SetUnRandomized();
                }
            }


            Console.WriteLine("Options===========================");
            foreach (var i in OptionLines)
            {
                Console.WriteLine(i.Trim());

                if (i.Trim().StartsWith("starting_gear"))
                {
                    var data = Regex.Match(i.Trim(), @"\[([^)]*)\]").Groups[1].Value.Replace("'", "").Split(',').Select(x => x.Trim());
                    foreach(var d in data)
                    {
                        MakeStartingItem(Instance.Logic.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains(d)));
                    }
                }
                if (i.Trim().StartsWith("num_starting_triforce_shards"))
                {
                    var data = i.Trim().Split(':');
                    int Count = Int32.Parse(data[1]);
                    for (var t = 0; t < Count; t++)
                    {
                        MakeStartingItem(Instance.Logic.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains($"Triforce Shard {t + 1}")));
                    }
                }
                if (i.Trim().StartsWith("starting_pohs"))
                {
                    var data = i.Trim().Split(':');
                    int Count = Int32.Parse(data[1]);
                    for (var t = 0; t < Count; t++)
                    {
                        MakeStartingItem(Instance.Logic.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains($"Piece of Heart")));
                    }
                }
                if (i.Trim().StartsWith("starting_hcs"))
                {
                    var data = i.Trim().Split(':');
                    int Count = Int32.Parse(data[1]);
                    for (var t = 0; t < Count; t++)
                    {
                        MakeStartingItem(Instance.Logic.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains($"Heart Container")));
                    }
                }
                if (i.Trim() == "skip_rematch_bosses")
                {
                    EnableSkipBossRematches.SpoilerRandom = EnableSkipBossRematches.ID;
                }
                if (i.Trim().StartsWith("sword_mode"))
                {
                    var data = i.Trim().Split(':');
                    switch (data[1].Trim())
                    {
                        case "No Starting Sword":
                            SwordModeStartWith.SpoilerRandom = SwordModeNone.ID; break;
                        case "Swordless":
                            SwordModeStartWith.SpoilerRandom = SwordModeSwordless.ID; break;
                        default:
                            SwordModeStartWith.SpoilerRandom = SwordModeStartWith.ID;
                            MakeStartingItem(Instance.Logic.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains($"Progressive Sword")));
                            break;
                    }
                }
                if (i.Trim().StartsWith("randomize_entrances"))
                {
                    if (!i.Trim().Contains("Dungeons"))
                    {
                        foreach(var entry in Instance.Logic.Where(x => x.IsEntrance() && x.DictionaryName.StartsWith("Can Access Dungeon Entrance"))) { entry.SetUnRandomized(); }
                    }
                    else if (!i.Trim().Contains("Secret Caves"))
                    {
                        foreach (var entry in Instance.Logic.Where(x => x.IsEntrance() && x.DictionaryName.StartsWith("Can Access Secret Cave Entrance"))) { entry.SetUnRandomized(); }
                    }
                }

            }

            void MakeStartingItem(IEnumerable<LogicObjects.LogicEntry> ValidItem)
            {
                var ValidUnusedItem = ValidItem.Where(x => !UsedItems.Contains(x.ID));
                if (ValidUnusedItem.Any())
                {
                    var validItem = ValidUnusedItem.ToArray()[0];
                    validItem.ToggleStartingItem(true);
                    UsedItems.Add(validItem.ID);
                }
            }

            Console.WriteLine("Items===========================");
            foreach (var i in Spoiler.Skip(LocationsStart).Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                if (i.StartsWith("Starting island:")) { break; }

                var data = i.Split(':').Select(x => x.Trim()).ToArray();
                if (string.IsNullOrWhiteSpace(data[1])) { CurrentArea = data[0]; continue; }

                Console.WriteLine($"-----------------------------------------");
                Console.WriteLine($"Location: {CurrentArea} - {data[0]}");
                Console.WriteLine($"Item: {data[1]}");

                bool JunkItem = Ignoredjunk.Where(x => data[1].EndsWith(x)).Any();

                var ValidLocation = Instance.Logic.Where(x => x.SpoilerLocation != null && x.SpoilerLocation.Contains($"{CurrentArea} - {data[0]}"));
                var ValidItem = Instance.Logic.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains($"{data[1]}"));

                var ValidUnusedLocation = ValidLocation.Where(x => !UsedLocations.Contains(x.ID));
                var ValidUnusedItem = ValidItem.Where(x => !UsedItems.Contains(x.ID));

                Console.WriteLine($"Location Valid {ValidLocation.Any()}");
                Console.WriteLine($"Item Valid {ValidItem.Any() || JunkItem}");
                Console.WriteLine($"Unused Locations {ValidUnusedLocation.Any()}");
                Console.WriteLine($"Unused Items {ValidUnusedItem.Any() || JunkItem}");

                if (ValidUnusedLocation.Any() && ValidUnusedItem.Any())
                {
                    ValidUnusedLocation.ToArray()[0].SpoilerRandom = ValidUnusedItem.ToArray()[0].ID;
                    UsedLocations.Add(ValidUnusedLocation.ToArray()[0].ID);
                    UsedItems.Add(ValidUnusedItem.ToArray()[0].ID);
                }
                else if (ValidUnusedLocation.Any() && JunkItem)
                {
                    ValidUnusedLocation.ToArray()[0].SpoilerRandom = -1;
                    ValidUnusedLocation.ToArray()[0].JunkItemType = data[1];
                    UsedLocations.Add(ValidUnusedLocation.ToArray()[0].ID);
                    Console.WriteLine($"Item {data[1]} was masked as junk");
                }


            }
            Console.WriteLine("Entrances===========================");
            foreach (var i in Spoiler.Skip(EntrancesStart).Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                if (i.StartsWith("Charts:")) { break; }
                var data = i.Split(':').Select(x => x.Trim()).ToArray();

                Console.WriteLine($"-----------------------------------------");
                Console.WriteLine($"Entrance: {data[0]}");
                Console.WriteLine($"Exit: {data[1]}");

                var ValidLocation = Instance.Logic.Where(x => x.SpoilerLocation != null && x.SpoilerLocation.Contains($"{data[0]}"));
                var ValidItem = Instance.Logic.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains($"{data[1]}"));

                var ValidUnusedLocation = ValidLocation.Where(x => !UsedLocations.Contains(x.ID));
                var ValidUnusedItem = ValidItem.Where(x => !UsedItems.Contains(x.ID));

                Console.WriteLine($"Location Valid {ValidLocation.Any()}");
                Console.WriteLine($"Item Valid {ValidItem.Any()}");
                Console.WriteLine($"Unused Locations {ValidUnusedLocation.Any()}");
                Console.WriteLine($"Unused Items {ValidUnusedItem.Any()}");

                if (ValidUnusedLocation.Any() && ValidUnusedItem.Any())
                {
                    ValidUnusedLocation.ToArray()[0].SpoilerRandom = ValidUnusedItem.ToArray()[0].ID;
                    UsedLocations.Add(ValidUnusedLocation.ToArray()[0].ID);
                    UsedItems.Add(ValidUnusedItem.ToArray()[0].ID);
                }

            }
            Console.WriteLine("Charts===========================");
            foreach (var i in Spoiler.Skip(ChartsStart).Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var data = i.Split(':').Select(x => x.Trim()).ToArray();
                if (string.IsNullOrWhiteSpace(data[1])) { CurrentArea = data[0]; continue; }

                Console.WriteLine($"-----------------------------------------");
                Console.WriteLine($"Chart: {data[0]}");
                Console.WriteLine($"Sector: {data[1]}");

                var ValidLocation = Instance.Logic.Where(x => x.SpoilerLocation != null && x.SpoilerLocation.Contains($"{data[0]}"));
                var ValidItem = Instance.Logic.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains($"{data[1]}"));

                var ValidUnusedLocation = ValidLocation.Where(x => !UsedLocations.Contains(x.ID));
                var ValidUnusedItem = ValidItem.Where(x => !UsedItems.Contains(x.ID));

                Console.WriteLine($"Location Valid {ValidLocation.Any()}");
                Console.WriteLine($"Item Valid {ValidItem.Any()}");
                Console.WriteLine($"Unused Locations {ValidUnusedLocation.Any()}");
                Console.WriteLine($"Unused Items {ValidUnusedItem.Any()}");

                if (ValidUnusedLocation.Any() && ValidUnusedItem.Any())
                {
                    ValidUnusedLocation.ToArray()[0].SpoilerRandom = ValidUnusedItem.ToArray()[0].ID;
                    UsedLocations.Add(ValidUnusedLocation.ToArray()[0].ID);
                    UsedItems.Add(ValidUnusedItem.ToArray()[0].ID);
                }
            }

            Tools.CheckAllItemsByLocation("%Randomizer Options%");

            return true;
        }
    }
}
