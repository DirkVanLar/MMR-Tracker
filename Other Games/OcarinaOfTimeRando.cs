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
        }

        public class RegionExit
        {
            public string region = "";
            public string from = "";
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
                    Console.WriteLine(FullExitValue);
                }
                else
                {
                    RegionExit R = i.Value.ToObject<RegionExit>();
                    Console.WriteLine(R.region + " -> " + R.from + "," + R.region + " -> " + R.from + "," + R.from + " <- " + R.region + "," + R.from + " -> " + R.region);
                }
            }

            string[] allowed_tricks = Log.settings["allowed_tricks"].ToObject<string[]>();

        }



        //LOGIC=====================================================================================================================================================

        public static void ReadOotrLogic()
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
    }
}
