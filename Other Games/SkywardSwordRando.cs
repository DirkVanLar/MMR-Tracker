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
using MMR_Tracker.Class_Files;
using System.Windows.Forms;

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
        public static void SkywardSwordTesting(bool Applylive, bool CreateDict, string LogicURL, string LogicFileName = "")
        {
            

            LogicParser Parser = new LogicParser();

            System.Net.WebClient wc = new System.Net.WebClient();
            Dictionary<string, SSRCheck> Checks = new Dictionary<string, SSRCheck>();
            Checks = JsonConvert.DeserializeObject<Dictionary<string, SSRCheck>>(YamlToJson(wc.DownloadString("https://raw.githubusercontent.com/ssrando/ssrando/master/checks.yaml")));

            Dictionary<string, string> Logic = new Dictionary<string, string>();
            Logic = JsonConvert.DeserializeObject<Dictionary<string, string>>(YamlToJson(wc.DownloadString(LogicURL)));

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

            //Fix the one option with an "Is not" Operator which is not as easily handled
            Logic.Add("Option \"shop-mode\" Is Not \"Vanilla\"", "Option \"shop-mode\" Is \"Randomized\" | Option \"shop-mode\" Is \"Always Junk\"");

            //Make the Enabled options logic entries match the other option entries in formatting
            Logic.Add("Option \"hero-mode\" Enabled", "Option \"hero-mode\" Is \"Enabled\"");
            Logic.Add("Option \"skip-skykeep\" Enabled", "Option \"skip-skykeep\" Is \"Enabled\"");
            Logic.Add("Option \"randomize-trials\" Disabled", "Option \"randomize-trials\" Is \"Disabled\"");

            //Create Can access Past Logic.
            Logic.Add("Can Beat Required Dungeons",
                "MMRTCombinations6 & (" +
                "(Option \"RequiredDungeon1\" Is \"Skyview\" & Can Beat Skyview) |" +
                "(Option \"RequiredDungeon2\" Is \"Earth Temple\" & Can Beat Earth Temple) |" +
                "(Option \"RequiredDungeon3\" Is \"Sandship\" & Can Beat Sandship) |" +
                "(Option \"RequiredDungeon4\" Is \"Ancient Cistern\" & Can Beat Ancient Cistern) |" +
                "(Option \"RequiredDungeon5\" Is \"Lanayru Mining Facility\" & Can Beat Lanayru Mining Facility) |" +
                "(Option \"RequiredDungeon6\" Is \"Fire Sanctuary\" & Can Beat Fire Sanctuary) |" +
                "(Option \"RequiredDungeon1\" Is \"None\") |" +
                "(Option \"RequiredDungeon2\" Is \"None\") |" +
                "(Option \"RequiredDungeon3\" Is \"None\") |" +
                "(Option \"RequiredDungeon4\" Is \"None\") |" +
                "(Option \"RequiredDungeon5\" Is \"None\") |" +
                "(Option \"RequiredDungeon6\" Is \"None\")" +
            ")");
            Logic.Add("Can Access Past", "Can Beat Required Dungeons & Can Access Sealed Temple & Meets Gate of Time Sword Requirement & Can Raise Gate of Time");

            //The item Names for the got-sword-requirement options are ambiguous with the actual sword items which breaks logic. This is dumb but it fixed it
            Logic.Add("Option \"got-sword-requirement\" Is \"Goddess Longsword\"", "Option \"got-sword-requirement\" Is \"Requires Goddess Longsword\"");
            Logic.Add("Option \"got-sword-requirement\" Is \"Goddess Whitesword\"", "Option \"got-sword-requirement\" Is \"Requires Goddess Whitesword\"");
            Logic.Add("Option \"got-sword-requirement\" Is \"Goddess Sword\"", "Option \"got-sword-requirement\" Is \"Requires Goddess Sword\"");
            Logic.Add("Option \"got-sword-requirement\" Is \"Master Sword\"", "Option \"got-sword-requirement\" Is \"Requires Master Sword\"");
            Logic.Add("Option \"got-sword-requirement\" Is \"True Master Sword\"", "Option \"got-sword-requirement\" Is \"Requires True Master Sword\"");

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

            //Convert Logic to MMR Json Logic
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

            //Convert Item List to an MMRTracker Logic Dictionary
            foreach (var i in Checks)
            {
                var entry = new LogicObjects.LogicDictionaryEntry()
                {
                    DictionaryName = i.Key,
                    LocationName = i.Key,
                    ItemName = i.Value.original_item,
                    ItemSubType = IsGoddessCubeAccessEntry(i.Key) ? "Goddess Cube" : "Item",
                    FakeItem = false,
                    KeyType = i.Value.original_item.EndsWith("Boss Key") ? "boss" : (i.Value.original_item.EndsWith("Small Key") ? "small" : null),
                    SpoilerItem = new string[] { i.Value.original_item },
                    SpoilerLocation = new string[] { i.Key },
                    LocationArea = i.Value.type.Split(',')[0]
                };
                MasterDictionary.LogicDictionaryList.Add(entry);

            }

            //Add trick entries to logic as tricks
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

            //Add Item only checks that don't appear in the Check list but are needed in logic
            List<string> MissingItems = new List<string>() {
                "Faron Song of the Hero Part",
                "Eldin Song of the Hero Part",
                "Lanayru Song of the Hero Part",
                "Emerald Tablet", 
                "Spiral Charge",
                "Goddess Cube in Lanayru Gorge",
                "Hylian Shield"
            };
            foreach (var i in MissingItems) { AddMissiongrealItem(i); }
            AddMissiongrealItem("Extra Bottle", "Empty Bottle");
            AddMissiongrealItem("Extra Quiver", "Small Quiver");
            AddMissiongrealItem("Extra HP", "Heart Piece");

            //Add Dungeon entrances
            //The randomizer defines logic items as "Can access dungeon entrance on *Location Name*" 
            //And looks for logic items called "Can access *Dungeon Name*" 
            //It renames the "dungeon entrance on *Location Name*" to the randomized "*Dungeon Name*" during randomization
            //However, the "Can access dungeon entrance on *Location Name*" is also refered to in logic specifically for lanayru mining facility
            //Due to differences in how the randomizer and the tracker handle this, We are adding a seperate new entry using the "Can access *Dungeon Name*"
            //WHich will require the "Can access dungeon entrance on *Location Name*" entry as its only requirement,
            //This allows the "Can access dungeon entrance on *Location Name*" entry to stay the same for the one instance in lanayru mining facility
            //while allowing the "Can access *Dungeon Name*" entries to be randomized like real items
            AddDungeonEntrance("Can Access Ancient Cistern", "Dungeon Entrance in Lake Floria", "Ancient Cistern", "Can Access Dungeon Entrance in Lake Floria");
            AddDungeonEntrance("Can Access Earth Temple", "Dungeon Entrance in Eldin Volcano", "Earth Temple", "Can Access Dungeon Entrance in Eldin Volcano");
            AddDungeonEntrance("Can Access Fire Sanctuary", "Dungeon Entrance in Volcano Summit", "Fire Sanctuary", "Can Access Dungeon Entrance in Volcano Summit");
            AddDungeonEntrance("Can Access Lanayru Mining Facility", "Dungeon Entrance in Lanayru Desert", "Lanayru Mining Facility", "Can Access Dungeon Entrance in Lanayru Desert");
            AddDungeonEntrance("Can Access Sandship", "Dungeon Entrance in Sand Sea", "Sandship", "Can Access Dungeon Entrance in Sand Sea");
            AddDungeonEntrance("Can Access Sky Keep", "Dungeon Entrance on Skyloft", "Sky Keep", "Can Access Dungeon Entrance on Skyloft");
            AddDungeonEntrance("Can Access Skyview", "Dungeon Entrance in Deep Woods", "Skyview", "Can Access Dungeon Entrance in Deep Woods");

            //Same story as above the naming is reversed. "can beat *Dungeon name* is being kept the same while "Can access dungeon entrance on *Location Name*"
            //is being created and used as the "Area access clear" entry. This allows this system to mimic MMRs dungeon rando logic system so it works in the tracker
            AddDungeonClears("Can Beat Dungeon Entrance in Lake Floria", "Can Beat Ancient Cistern", "Can Access Ancient Cistern");
            AddDungeonClears("Can Beat Dungeon Entrance in Eldin Volcano", "Can Beat Earth Temple", "Can Access Earth Temple");
            AddDungeonClears("Can Beat Dungeon Entrance in Volcano Summit", "Can Beat Fire Sanctuary", "Can Access Fire Sanctuary");
            AddDungeonClears("Can Beat Dungeon Entrance in Lanayru Desert", "Can Beat Lanayru Mining Facility", "Can Access Lanayru Mining Facility");
            AddDungeonClears("Can Beat Dungeon Entrance in Sand Sea", "Can Beat Sandship", "Can Access Sandship");
            AddDungeonClears("Can Beat Dungeon Entrance on Skyloft", "Can Beat Sky Keep", "Can Access Sky Keep");
            AddDungeonClears("Can Beat Dungeon Entrance in Deep Woods", "Can Beat Skyview", "Can Access Skyview");

            //Same as above but these don't have any area clear logic or entries that use the original "Can access *Location* Silent realm"
            //But we add it just like a dungeon entrance for consistency.
            AddDungeonEntrance("Can Access Skyloft Silent Realm", "Trial Gate on Skyloft", "Skyloft Silent Realm", "Can Open Trial Gate on Skyloft");
            AddDungeonEntrance("Can Access Lanayru Silent Realm", "Trial Gate in Lanayru Desert", "Lanayru Silent Realm", "Can Open Trial Gate in Lanayru Desert");
            AddDungeonEntrance("Can Access Faron Silent Realm", "Trial Gate in Faron Woods", "Faron Silent Realm", "Can Open Trial Gate in Faron Woods");
            AddDungeonEntrance("Can Access Eldin Silent Realm", "Trial Gate in Eldin Volcano", "Eldin Silent Realm", "Can Open Trial Gate in Eldin Volcano");


            AddOptionEntry("got-sword-requirement", "Requires Goddess Longsword", new string[] { "Requires Goddess Sword", "Requires Goddess Whitesword", "Requires Master Sword", "Requires True Master Sword" }, "Gate of Time Sword Requirement");
            AddOptionEntry("hero-mode", "Enabled", new string[] { "Disabled" }, "Hero Mode");
            AddOptionEntry("open-lmf", "Nodes", new string[] { "Open" }, "Lanayru Mining Facility Accessibility");
            AddOptionEntry("open-thunderhead", "Ballad", new string[] { "Open" }, "Thunder Head Accessibility");
            AddOptionEntry("RequiredDungeon1", "Skyview", new string[] { "None" }, "Required Dungeon 1", "%Required Dungeons%", "Option Required Dungeon");
            AddOptionEntry("RequiredDungeon2", "Earth Temple", new string[] { "None" }, "Required Dungeon 2", "%Required Dungeons%", "Option Required Dungeon");
            AddOptionEntry("RequiredDungeon3", "Sandship", new string[] { "None" }, "Required Dungeon 3", "%Required Dungeons%", "Option Required Dungeon");
            AddOptionEntry("RequiredDungeon4", "Ancient Cistern", new string[] { "None" }, "Required Dungeon 4", "%Required Dungeons%", "Option Required Dungeon");
            AddOptionEntry("RequiredDungeon5", "Lanayru Mining Facility", new string[] { "None" }, "Required Dungeon 5", "%Required Dungeons%", "Option Required Dungeon");
            AddOptionEntry("RequiredDungeon6", "Fire Sanctuary", new string[] { "None" }, "Required Dungeon 6", "%Required Dungeons%", "Option Required Dungeon");
            AddOptionEntry("shop-mode", "Randomized", new string[] { "Vanilla", "Always Junk" }, "Shop Mode");
            AddOptionEntry("skip-skykeep", "Enabled", new string[] { "Disabled" }, "Skip Skykeep");
            AddOptionEntry("randomize-trials", "Enabled", new string[] { "Disabled" }, "Randomize Trials");

            //Convert all item names in logic to dictionary entries, since thats what the tracker uses.
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

            //Print out all items used in logic that were not found
            Console.WriteLine($"Undefiend Logic Requirements ================================");
            Console.WriteLine($"=================================================");
            foreach (var i in InvalidItems.Distinct().OrderBy(x => x))
            {
                Console.WriteLine(i);
            }

            //move all entries that exist in every conditional to the requiremnts
            foreach (var Logicentry in MasterLogic)
            {
                ExtractRequirements(Logicentry);
            }

            bool LogicNotComplete = false;

            foreach (var entry in MasterLogic)
            {
                if (entry.RequiredItems != null)
                {
                    foreach (var i in entry.RequiredItems)
                    {
                        if (!MasterLogic.Where(x => x.Id == i).Any())
                        {
                            Console.WriteLine(i + " Was missing from logic");
                            LogicNotComplete = true;
                        }
                    }
                }
                if (entry.ConditionalItems != null)
                {
                    foreach (var cond in entry.ConditionalItems)
                    {
                        foreach (var i in cond)
                        {
                            if (!MasterLogic.Where(x => x.Id == i).Any())
                            {
                                Console.WriteLine(i + " Was missing from logic");
                                LogicNotComplete = true;
                            }
                        }
                    }
                }
            }

            if (LogicNotComplete) { MessageBox.Show("A logic entry requires an item that does not exist in logic"); return; }

            JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            LogicObjects.LogicFile LogicFile = new LogicObjects.LogicFile() { Logic = MasterLogic, Version = 1, GameCode = "SSR" };

            if (Applylive) { goto MakeTracker; }

            string FilePath = "";
            SaveFileDialog saveDialog = new SaveFileDialog();
            if (CreateDict)
            {
                saveDialog = new SaveFileDialog
                {
                    Filter = "MMR Tracker Dictionary (*.json)|*.json",
                    FilterIndex = 1,
                    FileName = $"{MasterDictionary.GameCode} V{MasterDictionary.LogicVersion} {MasterDictionary.LogicFormat} Logic Dictionary"
                };
                if (saveDialog.ShowDialog() != DialogResult.OK) { return; }
                FilePath = saveDialog.FileName;
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(MasterDictionary, _jsonSerializerOptions));
            }

            saveDialog = new SaveFileDialog
            {
                Filter = "Logic File (*.txt)|*.txt",
                FilterIndex = 1,
                FileName = LogicFileName == "" ? $"{MasterDictionary.GameCode} V{MasterDictionary.LogicVersion} {MasterDictionary.LogicFormat} Logic" : LogicFileName
            };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return; }
            FilePath = saveDialog.FileName;
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(LogicFile, _jsonSerializerOptions));

            return;

        MakeTracker:

            string[] LogicText = JsonConvert.SerializeObject(LogicFile, _jsonSerializerOptions).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            LogicObjects.MainTrackerInstance.RawLogicFile = LogicText;
            LogicObjects.MainTrackerInstance.LogicDictionary = MasterDictionary;
            LogicEditing.PopulateTrackerInstance(LogicObjects.MainTrackerInstance);
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);

            MainInterface.CurrentProgram.FormatMenuItems();
            MainInterface.CurrentProgram.ResizeObject();
            MainInterface.CurrentProgram.PrintToListBox();
            Tools.UpdateTrackerTitle();
            Tools.SaveFilePath = "";

            //PrintLogic(MasterLogic);

            void AddMissiongrealItem(string Name, string ItemName = "")
            {
                if (ItemName == "") { ItemName = Name; }
                MasterDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = Name,
                    ItemName = ItemName,
                    SpoilerItem = new string[] { ItemName },
                    ItemSubType = "Item"
                });
                MasterLogic.Add(new LogicObjects.JsonFormatLogicItem { Id = Name });
            }

            void AddDungeonEntrance(string LogicName, string LocationName, string Itemname, string EntranceRequirment, string ItemSubType = "Dungeon Entrance")
            {
                MasterDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = LogicName,
                    LocationName = LocationName,
                    ItemName = Itemname,
                    SpoilerLocation = new string[] { LocationName },
                    SpoilerItem = new string[] { Itemname },
                    ItemSubType = ItemSubType
                });
                MasterLogic.Add(new LogicObjects.JsonFormatLogicItem { Id = LogicName, RequiredItems = new List<string>() { EntranceRequirment } });
            }


            void AddDungeonClears(string LogicName, string ClearRequirment, string DungeonEntranceItem)
            {
                MasterDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = LogicName,
                    FakeItem = true,
                    GameClearDungeonEntrance = DungeonEntranceItem
                });
                MasterLogic.Add(new LogicObjects.JsonFormatLogicItem { Id = LogicName, RequiredItems = new List<string>() { ClearRequirment } });
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

            void AddOptionEntry(string OptionText, string Defaultvalue, string[] Othervalues, string FriendlyLocationName = "", string Category = "%Options%", string Grouping = "")
            {
                if (Grouping == "") { Grouping = $"Option {OptionText}"; }
                if (FriendlyLocationName == "") { FriendlyLocationName = Defaultvalue; }
                CommitOptionEntry(OptionText, Defaultvalue, FriendlyLocationName, Category, Grouping, $"Option {OptionText}", $"Option {OptionText} Is {Defaultvalue}");
                foreach (var i in Othervalues)
                {
                    CommitOptionEntry(OptionText, i, null, Category, Grouping, null, $"Option {OptionText} Is {i}");
                }

            }

            void CommitOptionEntry(string OptionText, string Defaultvalue, string FriendlyLocationName, string Category, string Grouping, string SpoilerLocation, string SpoilerItem)
            {
                MasterDictionary.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry
                {
                    DictionaryName = $"Option \"{OptionText}\" Is \"{Defaultvalue}\"",
                    LocationName = FriendlyLocationName,
                    ItemName = Defaultvalue,
                    SpoilerLocation = SpoilerLocation == null ? null : new string[] { SpoilerLocation },
                    SpoilerItem = new string[] { SpoilerItem },
                    ItemSubType = Grouping,
                    LocationArea = Category
                });
                MasterLogic.Add(new LogicObjects.JsonFormatLogicItem { Id = $"Option \"{OptionText}\" Is \"{Defaultvalue}\"" });
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

        public static bool IsGoddessCubeAccessEntry(string Name)
        {
            return Name.StartsWith("Goddess Cube ") || Name == "Initial Goddess Cube";
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

        public static bool HandleSSRSpoilerLog(LogicObjects.TrackerInstance Instance, string[] Spoiler)
        {
            List<int> LocationsWithItems = new List<int>();
            List<int> ItemsWithLocations = new List<int>();
            List<string> StartingItems = new List<string>();
            string CurrentSection = "start";
            string Region = "";

            Dictionary<string, LogicObjects.LogicEntry> ReqDungeons = SetuprequiredDungeons(Instance);
            SetBinaryOptionsDefault(Instance);

            foreach (var i in Spoiler)
            {
                if (string.IsNullOrWhiteSpace(i)) { continue; }
                if (i == "Options selected:") { CurrentSection = "options"; continue; }
                if (i == "Cosmetic Options:") { CurrentSection = "cosmetic"; continue; }
                if (i == "Starting items:") { CurrentSection = "starting"; continue; }
                if (i.StartsWith("Required Dungeon 1:")) { CurrentSection = "req1"; }
                if (i.StartsWith("Required Dungeon 2:")) { CurrentSection = "req2"; }
                if (i.StartsWith("Required Dungeon 3:")) { CurrentSection = "req3"; }
                if (i.StartsWith("Required Dungeon 4:")) { CurrentSection = "req4"; }
                if (i.StartsWith("Required Dungeon 5:")) { CurrentSection = "req5"; }
                if (i.StartsWith("Required Dungeon 6:")) { CurrentSection = "req6"; }
                if (i == "SotS:") { CurrentSection = "SotS"; continue; }
                if (i == "Barren Regions:") { CurrentSection = "Barren Regions"; continue; }
                if (i == "Playthrough:") { CurrentSection = "Nonprogress Regions"; continue; }
                if (i == "All item locations:") { CurrentSection = "items"; continue; }
                if (i == "Entrances:") { CurrentSection = "entrances"; continue; }
                if (i == "Trial Gates:") { CurrentSection = "trials"; continue; }
                if (i == "Hints:") { CurrentSection = "hints"; continue; }

                if (CurrentSection == "starting")
                {
                    var item = i.Trim();
                    StartingItems.Add(item);
                }

                if (CurrentSection == "options")
                {
                    string Line = i.Trim();
                    if (Line == "hero-mode") { Line = "hero-mode:Enabled"; }
                    if (Line == "skip-skykeep") { Line = "skip-skykeep:Enabled"; }
                    if (Line == "randomize-trials") { Line = "randomize-trials:Enabled"; }
                    var Parts = Line.Split(':').Select(x => x.Trim()).ToArray();
                    if (Parts.Length < 2) { continue; }

                    string OptionName = $"Option {Parts[0]}";
                    string Selectedoptionsname = $"Option {Parts[0]} Is {Parts[1]}";
                    string SelectedoptionsnameAlt = $"Option {Parts[0]} Is Requires {Parts[1]}";

                    Console.WriteLine($"====================================================");
                    Console.WriteLine($"looking for setting with name: {OptionName}");
                    Console.WriteLine($"To assign Option Data: {Selectedoptionsname} OR {SelectedoptionsnameAlt}");

                    var OptionEntry = Instance.Logic.Find(x => x.SpoilerLocation != null && x.SpoilerLocation.Contains(OptionName));
                    var SelectedOptionEntry = Instance.Logic.Find(x => x.SpoilerItem != null && 
                                                                (x.SpoilerItem.Contains(Selectedoptionsname) || x.SpoilerItem.Contains(SelectedoptionsnameAlt)));

                    if (OptionEntry == null || SelectedOptionEntry == null) { continue; }
                    OptionEntry.SpoilerRandom = SelectedOptionEntry.ID;
                }

                if (CurrentSection.StartsWith("req"))
                {
                    var data = i.Split(':').Select(x => x.Trim()).ToArray();
                    if (CurrentSection == "req1") { ReqDungeons["Skyview"].SpoilerRandom = ReqDungeons[data[1]].ID; }
                    if (CurrentSection == "req2") { ReqDungeons["Earth Temple"].SpoilerRandom = ReqDungeons[data[1]].ID;}
                    if (CurrentSection == "req3") { ReqDungeons["Lanayru Mining Facility"].SpoilerRandom = ReqDungeons[data[1]].ID; }
                    if (CurrentSection == "req4") { ReqDungeons["Ancient Cistern"].SpoilerRandom = ReqDungeons[data[1]].ID;}
                    if (CurrentSection == "req5") { ReqDungeons["Sandship"].SpoilerRandom = ReqDungeons[data[1]].ID; }
                    if (CurrentSection == "req6") { ReqDungeons["Fire Sanctuary"].SpoilerRandom = ReqDungeons[data[1]].ID;}
                }

                if (CurrentSection == "items" || CurrentSection == "entrances" || CurrentSection == "trials")
                {
                    var Parts = i.Split(':').Select(x => x.Trim()).ToArray();
                    if (string.IsNullOrWhiteSpace(Parts[1])) { Region = Parts[0].Trim() + " - "; continue; }
                    if (CurrentSection != "items") { Region = ""; }
                    if (Parts.Length < 2) { continue; }

                    string LocationWithregion = Region + Parts[0];

                    var Locations = Instance.Logic.Where(x => x.SpoilerLocation != null && x.SpoilerLocation.Contains(LocationWithregion)).ToList();
                    var Items = Instance.Logic.Where(x => x.SpoilerItem != null && x.SpoilerItem.Contains(Parts[1])).ToList();
                    if (!Locations.Any() || !Items.Any()) 
                    { Console.WriteLine($"Could not find logic data for line \n[{LocationWithregion}]:[{Parts[1]}] \nLoc: {Locations.Any()} item: {Items.Any()}"); continue; }

                    var UnusedLocation = Locations.Find(x => !LocationsWithItems.Contains(x.ID));
                    var UnusedItem = Items.Find(x => !ItemsWithLocations.Contains(x.ID));
                    if (UnusedLocation == null) 
                    { 
                        Console.WriteLine($"[{LocationWithregion}] Has already been assigned spoiler data"); 
                        continue; 
                    }
                    if (UnusedItem == null)
                    {
                        if (Parts[1].EndsWith("Treasure") || Parts[1].EndsWith("Rupoor") || Parts[1].EndsWith("Rupee"))
                        {
                            UnusedItem = new LogicObjects.LogicEntry() { ID = -1 };
                        }
                        else
                        {
                            Console.WriteLine($"All items with spoiler name [{Parts[1]}] have already been assigned to locations");
                            continue;
                        }
                    }

                    LocationsWithItems.Add(UnusedLocation.ID);
                    ItemsWithLocations.Add(UnusedItem.ID);

                    UnusedLocation.SpoilerRandom = UnusedItem.ID;
                }
            }

            foreach(var i in StartingItems)
            {
                Console.WriteLine(i + " Was Marked as a starting Item");
                var Item = Instance.Logic.Find(x => x.SpoilerItem.Contains(i) && x.GetItemsSpoilerLocation(Instance.Logic) == null && !x.StartingItem());
                if (Item != null)
                {
                    Console.WriteLine(Item.DictionaryName + " Was not randomized to a location, setting as starting Item");
                    if (Item.Options < 4) { Item.Options += 4; }
                }
            }

            foreach(var i in UnrandomizedSingleGratitudeCrystals())
            {
                var SingleCrystalLocation = Instance.Logic.Find(x => x.DictionaryName == i);
                if (SingleCrystalLocation != null) { SingleCrystalLocation.Options = 2; }
            }

            return true;
        }

        public static List<string> UnrandomizedSingleGratitudeCrystals()
        {
            return new List<string>()
            {
                "Knight Academy - Crystal in Link's Room",
                "Knight Academy - Crystal in Knight Academy Plant",
                "Knight Academy - Crystal in Zelda's Room",
                "Knight Academy - Crystal in Sparring Hall",
                "Central Skyloft - Crystal between Wooden Planks",
                "Central Skyloft - Crystal on West Cliff",
                "Central Skyloft - Crystal in Orielle and Parrow's House",
                "Central Skyloft - Crystal on Light Tower",
                "Skyloft Village - Crystal near Pumpkin Patch",
                "Central Skyloft - Crystal after Waterfall Cave",
                "Central Skyloft - Crystal in Loftwing Prison",
                "Central Skyloft - Crystal on Waterfall Island",
                "Sky - Crystal outside Lumpy Pumpkin",
                "Sky - Crystal inside Lumpy Pumpkin",
                "Sky - Crystal on Beedle's Ship"
            };
        }

        public static Dictionary<string, LogicObjects.LogicEntry> SetuprequiredDungeons(LogicObjects.TrackerInstance Instance)
        {
            Dictionary<string, LogicObjects.LogicEntry> ReqDungeons = new Dictionary<string, LogicObjects.LogicEntry>()
                    {
                        { "Skyview", Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon1\" Is \"Skyview\"")},
                        { "Earth Temple", Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon2\" Is \"Earth Temple\"")},
                        { "Sandship", Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon3\" Is \"Sandship\"")},
                        { "Ancient Cistern", Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon4\" Is \"Ancient Cistern\"")},
                        { "Lanayru Mining Facility", Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon5\" Is \"Lanayru Mining Facility\"")},
                        { "Fire Sanctuary", Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon6\" Is \"Fire Sanctuary\"")}
                    };
            var ReqDungeon1None = Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon1\" Is \"None\"");
            var ReqDungeon2None = Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon2\" Is \"None\"");
            var ReqDungeon3None = Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon3\" Is \"None\"");
            var ReqDungeon4None = Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon4\" Is \"None\"");
            var ReqDungeon5None = Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon5\" Is \"None\"");
            var ReqDungeon6None = Instance.Logic.Find(x => x.DictionaryName == "Option \"RequiredDungeon6\" Is \"None\"");

            ReqDungeons["Skyview"].SpoilerRandom = ReqDungeon1None.ID;
            ReqDungeons["Earth Temple"].SpoilerRandom = ReqDungeon2None.ID;
            ReqDungeons["Sandship"].SpoilerRandom = ReqDungeon3None.ID;
            ReqDungeons["Ancient Cistern"].SpoilerRandom = ReqDungeon4None.ID;
            ReqDungeons["Lanayru Mining Facility"].SpoilerRandom = ReqDungeon5None.ID;
            ReqDungeons["Fire Sanctuary"].SpoilerRandom = ReqDungeon6None.ID;
            return ReqDungeons;
        }

        public static void SetBinaryOptionsDefault(LogicObjects.TrackerInstance Instance)
        {
            var HeroModeEntry = Instance.Logic.Find(x => x.DictionaryName == "Option \"hero-mode\" Is \"Enabled\"");
            var HeroModeDefault = Instance.Logic.Find(x => x.DictionaryName == "Option \"hero-mode\" Is \"Disabled\"");
            var skipskykeepEntry = Instance.Logic.Find(x => x.DictionaryName == "Option \"skip-skykeep\" Is \"Enabled\"");
            var skipskykeepDefault = Instance.Logic.Find(x => x.DictionaryName == "Option \"skip-skykeep\" Is \"Disabled\"");
            var RandomTrialsEntry = Instance.Logic.Find(x => x.DictionaryName == "Option \"randomize-trials\" Is \"Enabled\"");
            var RandomTrialsDefault = Instance.Logic.Find(x => x.DictionaryName == "Option \"randomize-trials\" Is \"Disabled\"");
            HeroModeEntry.SpoilerRandom = HeroModeDefault.ID;
            skipskykeepEntry.SpoilerRandom = skipskykeepDefault.ID;
            RandomTrialsEntry.SpoilerRandom = RandomTrialsDefault.ID;

        }
    }
}
