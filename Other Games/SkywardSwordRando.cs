using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using MMR_Tracker.Forms;
using System.Text.RegularExpressions;
using MMR_Tracker_V2;

namespace MMR_Tracker.Other_Games
{
    class SSRCheck 
    {
        [JsonProperty("original item")]
        public string original_item { get; set; }
        public string type { get; set; }
    }

    class SkywardSwordRando
    {
        public static void SkywardSwordTesting()
        {
            Dictionary<string, string> SilentRealmReplacements = new Dictionary<string, string>()
            {
                { "Can Open Trial Gate in Faron Woods", "Can Access Eldin Silent Realm" },
                { "Can Open Trial Gate in Eldin Volcano", "Can Access Faron Silent Realm" },
                { "Can Open Trial Gate in Lanayru Desert", "Can Access Lanayru Silent Realm" },
                { "Can Open Trial Gate on Skyloft", "Can Access Skyloft Silent Realm" }
            };

            //Key needs to be static, Value needs to be real item
            Dictionary<string, string> DungeonEntranceReplacements = new Dictionary<string, string>() 
            {
                { "Can Access Dungeon Entrance in Lake Floria", "Can Access Ancient Cistern" },
                { "Can Access Dungeon Entrance in Eldin Volcano", "Can Access Earth Temple" },
                { "Can Access Dungeon Entrance in Volcano Summit", "Can Access Fire Sanctuary" },
                { "Can Access Dungeon Entrance in Lanayru Desert", "Can Access Lanayru Mining Facility" },
                { "Can Access Dungeon Entrance in Sand Sea", "Can Access Sandship" },
                { "Can Access Dungeon Entrance on Skyloft", "Can Access Sky Keep" },
                { "Can Access Dungeon Entrance in Deep Woods", "Can Access Skyview" }
            };

            //Key needs to be static, Value is dungeon clear item that will have it's logic swapped
            Dictionary<string, string> DungeonClearReplacements = new Dictionary<string, string>()
            {
                { "Can Beat Ancient Cistern", "Can Beat Dungeon Entrance in Lake Floria" },
                { "Can Beat Earth Temple", "Can Beat Dungeon Entrance in Eldin Volcano" },
                { "Can Beat Fire Sanctuary", "Can Beat Dungeon Entrance in Volcano Summit" },
                { "Can Beat Lanayru Mining Facility", "Can Beat Dungeon Entrance in Lanayru Desert" },
                { "Can Beat Sandship", "Can Beat Dungeon Entrance in Sand Sea" },
                { "Can Beat Sky Keep", "Can Beat Dungeon Entrance in Skyloft" },
                { "Can Beat Skyview", "Can Beat Dungeon Entrance in Deep Woods" }
            };

            LogicParser Parser = new LogicParser();

            System.Net.WebClient wc = new System.Net.WebClient();
            Dictionary<string, SSRCheck> Checks = new Dictionary<string, SSRCheck>();
            Checks = JsonConvert.DeserializeObject<Dictionary<string, SSRCheck>>(YamlToJson(wc.DownloadString("https://raw.githubusercontent.com/ssrando/ssrando/master/checks.yaml")));
            Dictionary<string, string> Logic = new Dictionary<string, string>();
            Logic = JsonConvert.DeserializeObject<Dictionary<string, string>>(YamlToJson(wc.DownloadString("https://raw.githubusercontent.com/ssrando/ssrando/master/SS%20Rando%20Logic%20-%20Glitchless%20Requirements.yaml")));

            List<LogicObjects.JsonFormatLogicItem> MasterLogic = new List<LogicObjects.JsonFormatLogicItem>();
            LogicObjects.LogicDictionary MasterDictionary = new LogicObjects.LogicDictionary()
            {
                DefaultWalletCapacity = 300,
                GameCode = "SSR",
                LogicFormat = "json",
                LogicVersion = 1,
                LogicDictionaryList = new List<LogicObjects.LogicDictionaryEntry>()
            };

            //Inaccesable
            Logic.Remove("Thunderhead - Second Goddess Chest on Mogma Mitts Island");
            //Fix Misspellings in Item List
            foreach (var i in Checks)
            {
                if (i.Value.original_item == "Bottle") { i.Value.original_item = "Empty Bottle"; }
                if (i.Value.original_item == "Baby's Rattle") { i.Value.original_item = "Baby Rattle"; }
            }

            //Add "X amount of Y Item" Logic Entries. Add neccesary MMRTCombinations entries.
            foreach (var i in Logic.Values.ToArray())
            {
                var entries = LogicParser.GetEntries(i).ToArray();
                foreach (var entry in entries)
                {
                    var CleanEntry = entry.Trim();
                    if (Logic.ContainsKey(CleanEntry)) { continue; }
                    var match = Regex.Match(entry, @"x\d+");
                    if (match.Success)
                    {
                        var ItemNeeded = CleanEntry.Replace(match.Groups[0].ToString(), "").Trim();
                        var AmmountNeeded = match.Groups[0].ToString().Replace("x", "").Trim();

                        var ChecksitemNameOccurences = Checks.Where(x => x.Value.original_item == ItemNeeded).ToDictionary(x => x.Key, x => x.Value);
                        if (!ChecksitemNameOccurences.Any()) { Console.WriteLine($"{ItemNeeded} Not Found in Item list"); continue; }
                        if (ChecksitemNameOccurences.Count() < Int32.Parse(AmmountNeeded)) { Console.WriteLine("Not enough Items found"); continue; }

                        var ItemList = ChecksitemNameOccurences.Select(x => x.Key).Select(x => x.Trim()).ToArray();
                        string newLogic = "";

                        if (ChecksitemNameOccurences.Count() > 1)
                        {
                            if (Int32.Parse(AmmountNeeded) > 1)
                            {
                                if (!Logic.ContainsKey($"MMRTCombinations{AmmountNeeded}")) { Logic.Add($"MMRTCombinations{AmmountNeeded}", "Nothing"); }
                                newLogic = $"MMRTCombinations{AmmountNeeded} & ({string.Join("|", ItemList)})";
                            }
                            else
                            {
                                newLogic = $"{string.Join("|", ItemList)}";
                            }
                        }
                        else
                        {
                            newLogic = $"{ItemList[0]}";
                        }


                        Logic.Add(CleanEntry, newLogic);
                        Console.WriteLine($"Added: ({CleanEntry}): [{newLogic}]");

                    }
                }
            }

            foreach (var i in Logic)
            {
                Console.WriteLine(i.Key);
                LogicObjects.JsonFormatLogicItem Logicentry = new LogicObjects.JsonFormatLogicItem();

                Logicentry.Id = i.Key;
                Logicentry.IsTrick = false;

                var Parsedlogic = Parser.ConvertLogicToConditionalString(i.Value);

                Logicentry.ConditionalItems = Parsedlogic == null ? new List<List<string>>() : Parsedlogic.Select(x => x.Select(y => y.Trim()).ToList()).ToList();
                Logicentry.RequiredItems = null;

                MasterLogic.Add(Logicentry);
            }

            foreach(var i in Checks)
            {
                var entry = new LogicObjects.LogicDictionaryEntry()
                {
                    DictionaryName = i.Key,
                    LocationName = i.Key,
                    ItemName = i.Value.original_item,
                    ItemSubType = "Item",
                    FakeItem = false,
                    KeyType = i.Value.original_item.EndsWith("Boss Key") ? "boss" : (i.Value.original_item.EndsWith("Small Key") ? "small" : null),
                    SpoilerItem = new string[] { i.Value.original_item },
                    SpoilerLocation = new string[] { i.Key },
                    LocationArea = i.Value.type.Split(',')[0]
                };
                MasterDictionary.LogicDictionaryList.Add(entry);

            }

            foreach (var i in Logic.Values.ToArray())
            {
                var entries = LogicParser.GetEntries(i).ToArray();
                foreach (var entry in entries)
                {
                    var CleanEntry = entry.Trim();
                    if (MasterLogic.Where(x => x.Id == CleanEntry).Any()) { continue; }
                    if (CleanEntry.EndsWith(" Trick"))
                    {
                        MasterLogic.Add(new LogicObjects.JsonFormatLogicItem() { Id = CleanEntry, IsTrick = true });
                        Console.WriteLine($"trick added {MasterLogic[MasterLogic.Count() - 1].Id}, {MasterLogic[MasterLogic.Count() - 1].IsTrick}");
                    }
                }
            }

            List<string> MissingItems = new List<string>()
            {
                "Faron Song of the Hero Part",
                "Eldin Song of the Hero Part",
                "Lanayru Song of the Hero Part",
                "Emerald Tablet",
                "Spiral Charge",
                "Goddess Cube in Lanayru Gorge"
            };

            foreach(var i in MissingItems) { AddMissiongrealItem(i); }

            List<string> InvalidItems = new List<string>();
            foreach (var k in MasterLogic)
            {
                if (k.ConditionalItems == null || !k.ConditionalItems.Any()) { continue; }

                Console.WriteLine($"Parsing Fixing Logic for {k.Id}");
                Console.WriteLine($"---------------------------------------");

                var LogicString = string.Join(" | ", k.ConditionalItems.Select(x => string.Join(" & ", x)));

                Console.WriteLine($"Old Logic\n[{LogicString}]");
                Console.WriteLine($"---------------------------------------");

                var entries = LogicParser.GetEntries(LogicString);

                for (var i = 0; i < entries.Count(); i++)
                {
                    if (string.IsNullOrEmpty(entries[i]) || LogicParser.ISLogicChar(entries[i][0]) || LogicParser.ISComment(entries[i])) { continue; }
                    string CleanEntry = entries[i].Trim();

                    if (string.IsNullOrWhiteSpace(CleanEntry)) { continue; }

                    var ChecksitemNameOccurences = MasterDictionary.LogicDictionaryList.Where(x => x.ItemName == CleanEntry).ToList();
                    var ChecksDictionarynameOccurences = MasterLogic.Where(x => x.Id == CleanEntry).ToList();

                    if (ChecksitemNameOccurences.Count() == 1)
                    {
                        entries[i] = ChecksitemNameOccurences[0].DictionaryName;
                    }
                    else if (ChecksitemNameOccurences.Count() > 1)
                    {
                        Console.WriteLine("Multiple Item Occurences");
                        entries[i] = "(" + string.Join("|", ChecksitemNameOccurences.Select(x => x.DictionaryName)) + ")";
                    }
                    else if (ChecksDictionarynameOccurences.Count() > 0)
                    {
                        entries[i] = CleanEntry;
                    }
                    else if (CleanEntry == ("Nothing"))
                    {
                        entries[i] = "";
                    }
                    else
                    {
                        InvalidItems.Add(CleanEntry);
                        entries[i] = CleanEntry;
                    }
                }
                var NewLogic = string.Join(" ", entries);
                Console.WriteLine($"Name Corrected logic");
                Console.WriteLine($"([{NewLogic}])");
                Console.WriteLine($"---------------------------------------");
                var Parsedlogic = Parser.ConvertLogicToConditionalString(NewLogic);

                if (Parsedlogic != null)
                {
                    Console.WriteLine($"name Corrected Parsed Logic \n{string.Join(" |\n", Parsedlogic.Select(x => string.Join(" & ", x)))}");
                    Console.WriteLine($"============================");
                }

                k.ConditionalItems = Parsedlogic == null ? new List<List<string>>() : Parsedlogic.Select(x => x.Select(y => y.Trim()).ToList()).ToList();
            }

            Console.WriteLine($"Undefiend Logic Requirements ================================");
            Console.WriteLine($"=================================================");
            foreach (var i in InvalidItems.Distinct().OrderBy(x => x))
            {
                Console.WriteLine(i);
            }

            foreach (var Logicentry in MasterLogic)
            {
                ExtractRequirements(Logicentry);
            }



            PrintLogic(MasterLogic);

            void AddMissiongrealItem(string Name)
            {
                MasterDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = Name,
                    ItemName = Name,
                    SpoilerItem = new string[] { Name },
                    ItemSubType = "Item"
                });
                MasterLogic.Add(new LogicObjects.JsonFormatLogicItem { Id = Name });
            }

            void PrintLogic(List<LogicObjects.JsonFormatLogicItem> PrintedLogic)
            {
                foreach (var Logicentry in PrintedLogic)
                {
                    Console.WriteLine("New Entry=======================");
                    Console.WriteLine($"{Logicentry.Id}:");
                    Console.WriteLine($"Is Trick: {Logicentry.IsTrick}");
                    if (Logicentry.RequiredItems != null)
                    {
                        Console.WriteLine("-Required:");
                        Console.WriteLine($"{string.Join(", ", Logicentry.RequiredItems)}:");
                    }
                    if (Logicentry.ConditionalItems != null)
                    {
                        Console.WriteLine("-Conditionals:");
                        foreach (var Cond in Logicentry.ConditionalItems)
                        {
                            Console.WriteLine($"{string.Join(", ", Cond)}:");
                        }
                    }
                    Console.WriteLine("================================");
                }
            }


            return;


            //Add Dungeon Entrance Dungeon Clear and Silent Realm Entries
            foreach (var i in DungeonEntranceReplacements) 
            {
                Checks.Add(i.Value, new SSRCheck() { original_item = i.Value, type = "Dungeon Entrance" });
                Logic.Add(i.Value, i.Key); 
            }
            foreach (var i in DungeonClearReplacements) { Logic.Add(i.Value, i.Key); }
            foreach (var i in SilentRealmReplacements)
            {
                Checks.Add(i.Value, new SSRCheck() { original_item = i.Value, type = "Silent realm" });
                Logic.Add(i.Value, i.Key); 
            }

            //Add Tricks as logic Items
            foreach (var i in Logic.Values.ToArray())
            {
                var entries = LogicParser.GetEntries(i).ToArray();
                foreach (var entry in entries)
                {
                    var CleanEntry = entry.Trim();
                    if (Logic.ContainsKey(CleanEntry)) { continue; }
                    if (CleanEntry.EndsWith(" Trick"))
                    {
                        Logic.Add(CleanEntry, "Nothing");
                    }
                }
            }

            


        }

        public static bool ExtractRequirements(LogicObjects.JsonFormatLogicItem entry)
        {
            bool ChangesMade = false;
            if (entry.ConditionalItems == null) { return ChangesMade; }
            List<string> ConsistantConditionals =
                entry.ConditionalItems.SelectMany(x => x).Distinct().Where(i => entry.ConditionalItems.All(x => x.Contains(i))).ToList();

            bool changesMade = ConsistantConditionals.Any();

            var NewRequirements = (entry.RequiredItems ?? new List<string>()).ToList();
            NewRequirements.AddRange(ConsistantConditionals);
            entry.RequiredItems = (NewRequirements.Any()) ? NewRequirements.Distinct().ToList() : null;

            var NewConditionals = entry.ConditionalItems.Select(x => x.ToList()).ToList();
            foreach (var i in NewConditionals)
            {
                i.RemoveAll(x => ConsistantConditionals.Contains(x));
            }
            NewConditionals.RemoveAll(x => !x.Any());
            entry.ConditionalItems = (NewConditionals.Any()) ? NewConditionals.Select(x => x.ToList()).ToList() : null;
            return ChangesMade;
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
    }
}
