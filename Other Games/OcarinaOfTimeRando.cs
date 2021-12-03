using MMR_Tracker.Forms;
using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public static string CleanLogicEntry(string entry)
        {
            entry = entry.Replace(" and ", " & ").Replace(" or ", " | "); //change "and" and "or" logic operators to "&" and "|" which is what the tracker is able to parse
            entry = entry.Replace(System.Environment.NewLine, " "); //Replace newlines with a space
            entry = Regex.Replace(entry, @"\s+", " "); //Trim segments of multiple spaces to a single space
            entry = entry.Trim();//Trim the front and end of the line

            //For the most part, logic items can't contain spaces. Some entries however contain spaces and are grouped using '
            //Remove the ' from these entries and change the spaces to _ so it's in a familiar format
            var SpacedEntries = Regex.Matches(entry, @"\'(.*?)\'");
            foreach (Match match in SpacedEntries)
            {
                var newValue = match.Groups[1].Value.Replace("'", "").Replace(" ", "_");
                entry = entry.Replace(match.Groups[0].Value, $"{newValue}");
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

            //change any x of y entries "ex: (item, 4)" to a more familiar format (item x4)
            //No real reason to do this but since SSR logic uses the latter format and we already have code to parse it it make life a bit easier.
            var Countmatch = Regex.Matches(entry, @", \d+");
            foreach (Match match in Countmatch)
            {
                var NewValue = match.Groups[0].Value.Replace(", ", " x");
                entry = entry.Replace(match.Groups[0].Value, NewValue);
            }

            return entry;
        }

        public static void ReadOotrLogic()
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
            Logic.Add(HelperLogic);

            foreach (var i in Logic)
            {
                var DungeonState = i.MQ ? "Master Quest" : "Vanilla";
                Console.WriteLine();
                Console.WriteLine($"====================");
                Console.WriteLine($"Region: ({i.region_name})");
                Console.WriteLine($"--------------------");
                Console.WriteLine($"Locations:");
                OOTRLogicObject NewLogicEntry = new OOTRLogicObject() { region_name = i.region_name };
                foreach (var j in i.locations)
                {
                    string LogicLine = CleanLogicEntry(j.Value).Trim();
                    LogicLine = i.region_name + $" & ({LogicLine})";
                    if (i.Dungeon != "") { LogicLine = $"{i.Dungeon} {DungeonState} & ({LogicLine})"; }
                    Console.WriteLine($"Logic for {j.Key}");
                    Console.WriteLine($"[{LogicLine}]");
                    NewLogicEntry.locations.Add(j.Key, LogicLine);
                }
                Console.WriteLine($"--------------------");
                Console.WriteLine($"Events:");
                foreach (var j in i.events)
                {
                    string[] BadLogicEntries = new string[] { "can_play(song)", "can_use(item)", "_is_magic_item(item)", "_is_adult_item(item)", "_is_child_item(item)", "_is_magic_arrow(item)", "has_projectile(for_age)" };
                    if (BadLogicEntries.Contains(j.Key)) { continue; }
                    string LogicLine = CleanLogicEntry(j.Value).Trim();
                    LogicLine = (i.region_name == "Logic Helper") ? LogicLine : i.region_name + $" & ({LogicLine})";
                    if (i.Dungeon != "") { LogicLine = $"{i.Dungeon} {DungeonState} & ({LogicLine})"; }
                    Console.WriteLine($"Logic for {j.Key}");
                    Console.WriteLine($"[{LogicLine}]");
                    NewLogicEntry.events.Add(j.Key, LogicLine);
                }
                Console.WriteLine($"--------------------");
                Console.WriteLine($"Entrances:");
                foreach (var j in i.exits)
                {
                    string LogicLine = CleanLogicEntry(j.Value).Trim();
                    LogicLine = i.region_name + $" & ({LogicLine})";
                    if (i.Dungeon != "") { LogicLine = $"{i.Dungeon} {DungeonState} & ({LogicLine})"; }
                    Console.WriteLine($"Logic for {i.region_name} -> {j.Key}");
                    Console.WriteLine($"[{LogicLine}]");
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

            LogicObjects.LogicFile FormatedLogic = new LogicObjects.LogicFile();
            FormatedLogic.GameCode = "OOTR";
            FormatedLogic.Version = 1;
            FormatedLogic.Logic = new List<LogicObjects.JsonFormatLogicItem>();

            LogicParser Parser = new LogicParser();

            foreach (var i in MasterLogic)
            {
                FormatedLogic.Logic.Add(new LogicObjects.JsonFormatLogicItem() { Id = i.Key, ConditionalItems = Parser.ConvertLogicToConditionalString(i.Value) });
            }

            File.WriteAllText("OOTRLogic.json", JsonConvert.SerializeObject(FormatedLogic, new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }));

        }

        public static void ReadOOTRRefSheet()
        {

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


        //OldStuff

        public static void GenerateDictionary()
        {
            //DictionaryName,LocationName,ItemName,LocationArea,ItemSubType,SpoilerLocation,SpoilerItem,EntrancePair
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            var VanillaLocations = JsonConvert.DeserializeObject<SpoilerLog>(File.ReadAllText(@"lib\Misc\OOTRTestFiles\Vanilla_Locations.json"), jsonSerializerSettings);
            Dictionary<string, dynamic> EntranceList = new Dictionary<string, dynamic>();

            //Print all Locations and items from the Vanilla Locations List
            foreach (var i in VanillaLocations.locations)
            {
                Console.WriteLine($"{i.Key},{i.Key},{i.Value},,Item,{i.Key},{i.Value},");
            }

            //Search through all Test spoiler logs for entrances and combine them into one file
            foreach (var i in Directory.GetFiles(@"lib\Misc\OOTRTestFiles\Spoiler Logs"))
            {
                var Log = JsonConvert.DeserializeObject<SpoilerLog>(File.ReadAllText(i), jsonSerializerSettings);
                foreach (var j in Log.entrances)
                {
                    if (!EntranceList.ContainsKey(j.Key))
                    {
                        EntranceList.Add(j.Key, j.Value);
                    }
                }
            }
            var Entrances = EntranceList.Keys.ToList();
            //Print All Entrances
            foreach (var i in Entrances)
            {
                var data = i.Split(new string[] { " -> " }, StringSplitOptions.None);
                var From = data[0];
                var To = data[1];

                string CoupledEntrance = "";
                if (Entrances.Contains($"{To} -> {From}")) { CoupledEntrance = $"{To} -> {From}"; }

                Console.WriteLine(
                    $"{From} -> {To}," +
                    $"{From} -> {To}," +
                    $"{To} <- {From}," +
                    $",Entrance," +
                    $"{From} -> {To}," +
                    $"{To} <- {From}," +
                    CoupledEntrance
                );
            }

        }

        public static void GetItemAmounts()
        {
            Dictionary<string, int> ItemAmontAverages = new Dictionary<string, int>();
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            var VanillaLocations = JsonConvert.DeserializeObject<SpoilerLog>(File.ReadAllText(@"C:\CodeTest\Vanilla_Locations.json"), jsonSerializerSettings);

            foreach (var i in Directory.GetFiles(@"C:\CodeTest\SpoilerLogs"))
            {
                var Log = JsonConvert.DeserializeObject<SpoilerLog>(File.ReadAllText(i), jsonSerializerSettings);
                foreach(var j in Log.item_pool.Keys)
                {
                    if (ItemAmontAverages.ContainsKey(j))
                    {
                        if (ItemAmontAverages[j] < Log.item_pool[j]) { ItemAmontAverages[j] = Log.item_pool[j];}
                    }
                    else { ItemAmontAverages.Add(j, Log.item_pool[j]); }
                }
            }


            foreach (var i in ItemAmontAverages.OrderBy(x => x.Key))
            {
                int CountInVanilla = VanillaLocations.locations.Where(x => x.Value == i.Key).Count();
                if (i.Value > CountInVanilla)
                {
                    Console.WriteLine($"Missing {i.Value - CountInVanilla} {i.Key}. {CountInVanilla}/{i.Value} ");
                    var ammounttoadd = (i.Value) >= 5 ? Math.Ceiling((Decimal)(i.Value - CountInVanilla + 5) / 5) * 5 : (i.Value - CountInVanilla);
                    Console.WriteLine($"Adding {(int)ammounttoadd} {i.Key}");
                    Console.WriteLine($"======================================");
                }
            }
        }

        public static void CheckMissingItemLocations()
        {
            Dictionary<string, string> Locations = new Dictionary<string, string>();
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            var VanillaLocations = JsonConvert.DeserializeObject<SpoilerLog>(File.ReadAllText(@"C:\CodeTest\Vanilla_Locations.json"), jsonSerializerSettings);

            foreach (var i in Directory.GetFiles(@"C:\CodeTest\SpoilerLogs"))
            {
                var Log = JsonConvert.DeserializeObject<SpoilerLog>(File.ReadAllText(i), jsonSerializerSettings);
                foreach (var j in Log.locations)
                {
                    if (!Locations.ContainsKey(j.Key))
                    {
                        Locations.Add(j.Key, "");
                    }
                }
            }

            Console.WriteLine("================================================================");
            Console.WriteLine("Vanilla Lcoation Not In Spoiler");
            foreach (var i in VanillaLocations.locations)
            {
                if (Locations.Where(x => i.Key == x.Key).Count() == 0)
                {
                    Console.WriteLine(i.Key);
                }
            }
            Console.WriteLine("================================================================");
            Console.WriteLine("Spoiler Lcoations Not in Vanilla list");
            foreach (var i in Locations)
            {
                if (VanillaLocations.locations.Where(x => i.Key == x.Key).Count() == 0)
                {
                    Console.WriteLine(i.Key);
                }
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



        //LOGIC=====================================================================================================================================================

        public static void OLDReadOotrLogic()
        {
            IDictionary<string, string> LogicDictionary = new Dictionary<string, string>();
            List<string> NeededLogicEntries = new List<string>();
            List<string> EventChecks = new List<string>();
            List<string> ItemChecks = new List<string>();
            List<string> ExitChecks = new List<string>();
            System.Net.WebClient wc = new System.Net.WebClient();

            List<string> LogicFiles = new List<string>
            {
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Overworld.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Bottom%20of%20the%20Well.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Deku%20Tree.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Dodongos%20Cavern.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Fire%20Temple.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Forest%20Temple.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Ganons%20Castle.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Gerudo%20Training%20Ground.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Ice%20Cavern.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Jabu%20Jabus%20Belly.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Shadow%20Temple.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Spirit%20Temple.json",
                "https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Water%20Temple.json"
            };

            var Logic = new List<OOTRLogicObject>();

            foreach (var i in LogicFiles)
            {
                string ItemData = wc.DownloadString(i);
                string[] ItemDataLines = ItemData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                var CleanedLogic = RemoveCommentsFromJSON(ItemDataLines);

                var jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                var tempLogic = JsonConvert.DeserializeObject<List<OOTRLogicObject>>(CleanedLogic, jsonSerializerSettings);

                Console.WriteLine("Parsing Logic: " + i);

                Logic.AddRange(tempLogic);
            }

            CleanLogic(Logic);
            ExportLogicToDictionary(Logic, LogicDictionary);
            LogicDictionary = ParseAtLogicEntries(LogicDictionary);
            GetRequiredLogingEntries(Logic, NeededLogicEntries, LogicDictionary, EventChecks, ItemChecks, ExitChecks);

            foreach (var i in LogicDictionary)
            {
                //Console.WriteLine("=====================");
                //Console.WriteLine(i.Key);
                //Console.WriteLine(i.Value);
            }

            Console.WriteLine("=========================");
            Console.WriteLine("Events");
            Console.WriteLine("=========================");
            foreach (var i in EventChecks.Distinct()) { Console.WriteLine(i); }

            Console.WriteLine("=========================");
            Console.WriteLine("Items");
            Console.WriteLine("=========================");
            foreach (var i in ItemChecks.Distinct()) { Console.WriteLine(i); }

            Console.WriteLine("=========================");
            Console.WriteLine("exits");
            Console.WriteLine("=========================");
            foreach (var i in ExitChecks.Distinct()) { Console.WriteLine(i); }

            Console.WriteLine("=========================");
            Console.WriteLine("Required Logic Items");
            Console.WriteLine("=========================");
            foreach (var i in NeededLogicEntries.Distinct().OrderBy(x => x).Where(x => !x.Contains("->"))) { Console.WriteLine(i); }
            foreach (var i in NeededLogicEntries.Distinct().OrderBy(x => x).Where(x => x.Contains("->"))) { Console.WriteLine(i); }
        }

        public static void GetRequiredLogingEntries(List<OOTRLogicObject> Logic, List<string> NeededLogicEntries, IDictionary<string, string> LogicDictionary, List<string> EventChecks, List<string> ItemChecks, List<string> ExitChecks)
        {
            foreach (var i in LogicDictionary)
            {
                List<string> LogicItems = GetLogicItems(i.Value);
                foreach (var j in LogicItems)
                {
                    string CleanedSection = j;
                    if (!string.IsNullOrWhiteSpace(CleanedSection) && !LogicParser.ISLogicChar(CleanedSection[0]))
                    {
                        NeededLogicEntries.Add(CleanedSection.Trim());
                    }
                }
            }
            foreach (var i in Logic)
            {
                foreach (var key in i.events.Keys.ToList())
                {
                    EventChecks.Add(key);
                }
                foreach (var key in i.locations.Keys.ToList())
                {
                    ItemChecks.Add(key);
                }
                foreach (var key in i.exits.Keys.ToList())
                {
                    ExitChecks.Add(key);
                }
            }
        }

        public static IDictionary<string, string> ParseAtLogicEntries(IDictionary<string, string> LogicDictionary)
        {
            Dictionary<string, string> NewLogicDictionary = new Dictionary<string, string>();

            foreach (var i in LogicDictionary)
            {
                string NewLogicString = "";
                List<string> LogicItems = GetLogicItems(i.Value);
                foreach (var j in LogicItems)
                {
                    string CleanedSection = j;
                    if (!string.IsNullOrWhiteSpace(CleanedSection) && !LogicParser.ISLogicChar(CleanedSection[0]))
                    {
                        if (CleanedSection.Trim().StartsWith("at("))
                        {
                            //Console.WriteLine("Found At line " + CleanedSection);
                            CleanedSection = CleanedSection.Replace("at(", "(").Replace(",", " &");
                        }
                        if (CleanedSection.Trim().StartsWith("here("))
                        {
                            //Console.WriteLine("Found At line " + CleanedSection);
                            CleanedSection = CleanedSection.Replace("here(", "(");
                        }
                    }
                    NewLogicString += CleanedSection;
                }
                NewLogicDictionary.Add(i.Key, NewLogicString);
            }
            return NewLogicDictionary;
        }

        public static void ExportLogicToDictionary(List<OOTRLogicObject> Logic, IDictionary<string, string> LogicDictionary)
        {
            foreach (var i in Logic)
            {
                foreach (var j in i.locations)
                {
                    if (LogicDictionary.ContainsKey(j.Key))
                    {
                        LogicDictionary[j.Key] += " | " + "( " + j.Value + " )";
                    }
                    else
                    {
                        LogicDictionary.Add(j.Key, "( " + j.Value + " )");
                    }
                }

            }
            foreach (var i in Logic)
            {
                foreach (var j in i.exits)
                {
                    LogicDictionary.Add(j.Key, j.Value);
                }

            }
            foreach (var i in Logic)
            {
                foreach (var j in i.events)
                {
                    if (LogicDictionary.ContainsKey(j.Key))
                    {
                        LogicDictionary[j.Key] += " | " + "( " + j.Value + " )";
                    }
                    else
                    {
                        LogicDictionary.Add(j.Key, "( " + j.Value + " )");
                    }
                }
            }

            foreach (var i in Logic)
            {
                string AccessObject = i.region_name;
                string AccessLogic = "";

                foreach (var j in Logic)
                {
                    foreach (var k in j.exits)
                    {
                        if (k.Key.Split(new string[] { "->" }, StringSplitOptions.None)[1].Trim() == i.region_name)
                        {
                            AccessLogic += k.Key + " | ";
                        }
                    }
                }
                if (AccessLogic != "")
                {
                    AccessLogic = AccessLogic.Substring(0, AccessLogic.Length - 3);
                }
                LogicDictionary.Add(AccessObject, AccessLogic);
            }
        }

        public static void CleanLogic(List<OOTRLogicObject> Logic)
        {
            foreach (var i in Logic)
            {
                foreach (var key in i.events.Keys.ToList())
                {
                    i.events[key] = Regex.Replace(i.events[key], @"\s+", " ").Trim().Replace(" and ", " & ").Replace(" or ", " | ").Replace("'", "");
                    i.events[key] = i.region_name + " & " + i.events[key];
                }
                foreach (var key in i.locations.Keys.ToList())
                {
                    i.locations[key] = Regex.Replace(i.locations[key], @"\s+", " ").Trim().Replace(" and ", " & ").Replace(" or ", " | ").Replace("'", "");
                    i.locations[key] = i.region_name + " & " + i.locations[key];
                }
                foreach (var key in i.exits.Keys.ToList())
                {
                    i.exits[key] = Regex.Replace(i.exits[key], @"\s+", " ").Trim().Replace(" and ", " & ").Replace(" or ", " | ").Replace("'", "");
                    i.exits[key] = i.region_name + " & " + i.exits[key];
                }

                Dictionary<string, string> NewEntranceDic = new Dictionary<string, string>();
                foreach (var j in i.exits)
                {
                    string NewKey = i.region_name.Trim() + " -> " + j.Key.Trim();
                    NewEntranceDic.Add(NewKey, j.Value);
                }
                i.exits = NewEntranceDic;
            }
        }

        public static List<string> GetLogicItems(string i)
        {
            string[] SpecialCases = { " at", "can_use", "can_play", "has_projectile", "_is_magic_item", "_is_adult_item", "_is_child_item", "_is_magic_arrow", " here" };
            bool InSpecialEntry = false;
            int SpecialCaseDepth = 0;
            List<string> LogicItems = new List<string>();
            string CurrentEntry = "";
            foreach (var j in i)
            {
                if (InSpecialEntry)
                {
                    CurrentEntry += j;
                    if (j == ')' && SpecialCaseDepth == 1)
                    {
                        InSpecialEntry = false;
                    }
                    else
                    {
                        if (j == '(')
                        {
                            SpecialCaseDepth++;
                        }
                        if (j == ')')
                        {
                            SpecialCaseDepth--;
                        }
                    }
                }
                else if (LogicParser.ISLogicChar(j))
                {
                    if (j == '(' && SpecialCases.Contains(CurrentEntry.Trim()))
                    {
                        InSpecialEntry = true;
                        SpecialCaseDepth = 1;
                        CurrentEntry += j;
                    }
                    else
                    {
                        if (CurrentEntry != "")
                        {
                            LogicItems.Add(CurrentEntry);
                            CurrentEntry = "";
                        }
                        LogicItems.Add(j.ToString());
                    }
                }
                else
                {
                    CurrentEntry += j;
                }
            }
            if (CurrentEntry != "")
            {
                LogicItems.Add(CurrentEntry);
            }
            return LogicItems;
        }
    }
}
