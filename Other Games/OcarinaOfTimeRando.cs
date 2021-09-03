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
        class OOTRLogicObject
        {
            public string region_name { get; set; } = "";
            public Dictionary<string, string> events { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> locations { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> exits { get; set; } = new Dictionary<string, string>();
        }
        public static void ReadOotrLogic()
        {
            Dictionary<string, string> LogicDictionary = new Dictionary<string, string>();
            List<string> NeededLogicEntries = new List<string>();
            System.Net.WebClient wc = new System.Net.WebClient();
            string ItemData = wc.DownloadString("https://raw.githubusercontent.com/Roman971/OoT-Randomizer/Dev-R/data/World/Overworld.json");
            string[] ItemDataLines = ItemData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            string CleanedLogic = "";
            foreach(var i in ItemDataLines) 
            {
                var CleanedLine = Utility.RemoveCommentLines(i);
                if (!string.IsNullOrWhiteSpace(CleanedLine)) { CleanedLogic += CleanedLine; }
            }

            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            var Logic = JsonConvert.DeserializeObject<List<OOTRLogicObject>>(CleanedLogic, jsonSerializerSettings);
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
                    foreach(var k in j.exits)
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


            Dictionary<string, string> NewLogicDictionary = new Dictionary<string, string>();

            foreach (var i in LogicDictionary)
            {
                Console.WriteLine("=====================");
                Console.WriteLine("Items used in " + i.Key);
                string NewLogicString = "";
                List<string> LogicItems = GetLogicItems(i.Value);
                foreach (var j in LogicItems)
                {
                    string CleanedSection = j;
                    if (!string.IsNullOrWhiteSpace(CleanedSection) && !LogicParser.ISLogicChar(CleanedSection[0]))
                    {
                        if (CleanedSection.Trim().StartsWith("at("))
                        {
                            CleanedSection = CleanedSection.Replace("at(", "(").Replace(",", " &");
                        }
                    }
                    NewLogicString += CleanedSection;
                }
                NewLogicDictionary.Add(i.Key, NewLogicString);
            }

            LogicDictionary = NewLogicDictionary;
            foreach (var i in LogicDictionary)
            {
                Console.WriteLine("=====================");
                Console.WriteLine(i.Key);
                Console.WriteLine(i.Value);
            }
        }

        public static List<string> GetLogicItems(string i)
        {
            string[] SpecialCases = { "at", "can_use", "can_play" };
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
