using MMR_Tracker.Forms;
using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker.Other_Games
{
    class SkywardSwordTools
    {
        public class SSLocations
        {
            public string DictionaryName { get; set; }
            public string LocationName { get; set; }
            public string ItemName { get; set; }
            public string LocationArea { get; set; }
            public string Logic { get; set; }
            public bool isFake { get; set; } = false;
            public string SpoilerLocation { get; set; }
            public string SpoilerItem { get; set; }
            public string ItemSubType { get; set; }
        }
        public static void CreateData()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            string ItemData = wc.DownloadString("https://raw.githubusercontent.com/lepelog/sslib/master/SS%20Rando%20Logic%20-%20Item%20Location.yaml");
            string[] ItemDataLines = ItemData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            string MacroData = wc.DownloadString("https://raw.githubusercontent.com/lepelog/sslib/master/SS%20Rando%20Logic%20-%20Macros.yaml");
            string[] MacroDataLines = MacroData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var backupEditorInstance = Utility.CloneTrackerInstance(LogicEditor.EditorInstance);

            var SSData = ParseData(ItemDataLines, MacroDataLines);

            foreach (var i in SSData)
            {
                Console.WriteLine("=============");
                Console.WriteLine(i.DictionaryName);
                Console.WriteLine((i.isFake) ? "Fake Entry" : i.LocationName);
                Console.WriteLine((i.isFake) ? "Fake Entry" : i.ItemName);
                Console.WriteLine(i.Logic);
                Console.WriteLine((i.isFake) ? "Fake Entry" : i.LocationArea);
            }
            Console.WriteLine("=============");


            LogicObjects.TrackerInstance SSInstance = new LogicObjects.TrackerInstance();
            SSInstance.LogicDictionary = CreateSSDictionary(SSData);
            SSInstance.RawLogicFile = CreateSSLogic(SSData);

            LogicEditing.PopulateTrackerInstance(SSInstance);
            AddGRatitudeCrystalEntries(SSInstance);
            CreateLogicHelpers(SSData, SSInstance);

            LogicEditor.EditorInstance = SSInstance;
            ApplyLogic(SSData, SSInstance);
            LogicEditor.EditorInstance = backupEditorInstance;

            LogicObjects.MainTrackerInstance = SSInstance;
            LogicEditing.CalculateItems(SSInstance);
            MainInterface.CurrentProgram.FormatMenuItems();
            MainInterface.CurrentProgram.ResizeObject();




        }

        public static void ApplyLogic(List<SSLocations> SSData, LogicObjects.TrackerInstance SSInstance)
        {
            List<string> Failed = new List<string>();
            foreach(var i in SSInstance.Logic.Where(x=> x.Conditionals == null))
            {
                Console.WriteLine("========================================================");
                Console.WriteLine($"Creating Logic for {i.DictionaryName}");

                var dataentry = SSData.Find(x => x.DictionaryName == i.DictionaryName);
                if (dataentry == null)
                {
                    Console.WriteLine($"Could not find {i.DictionaryName} in Data File");
                    continue; 
                }
                if (string.IsNullOrWhiteSpace(dataentry.Logic) || dataentry.Logic.Trim() == "Nothing")
                {
                    Console.WriteLine($"{i.DictionaryName} Does not have Logic");
                    continue; 
                }

                LogicParser SSParser = new LogicParser();
                Console.WriteLine($"{dataentry.Logic}\nWas converted to:");
                string ConvertedLogic = LogicParser.ConvertDicNameToID(dataentry.Logic);
                Console.WriteLine($"{ConvertedLogic}");
                try
                {
                    i.Conditionals = SSParser.ConvertLogicToConditional(ConvertedLogic);
                    Console.WriteLine($"Successufully Created Logic for {i.DictionaryName}");
                }
                catch
                {
                    Failed.Add(i.DictionaryName + "\n" + dataentry.Logic + "\n" + ConvertedLogic + "\n==============================");
                    Console.WriteLine($"Failed to convert Logic for {i.DictionaryName}");
                }

            }
            Console.WriteLine("");
            Console.WriteLine("The following entries failed");

            foreach (var i in Failed) { Console.WriteLine(i); }
        }

        public static string textTotheRight(string Input, string RightOf)
        {
            int Loc = Input.IndexOf(RightOf);
            if (Loc < 0) { return ""; }
            int Rightof = Loc + RightOf.Count();
            if (Rightof >= Input.Count()) { return ""; }
            return Input.Substring(Rightof);
        }

        public static void CreateLogicHelpers(List<SSLocations> SSData, LogicObjects.TrackerInstance SSInstance)
        {
            List<string> WasCounted = new List<string>();

            foreach (var i in SSData.Where(x=>!x.isFake && !string.IsNullOrWhiteSpace(x.ItemName)))
            {
                if (WasCounted.Contains(i.ItemName)) { continue; }

                //Create Entries for standard items taht exist in the item pool only once, this is a bit easier than converting item names to dictionary names in the logic
                var EntryNumber = SSInstance.Logic.Where(x => x.ItemName == i.ItemName);
                if (EntryNumber.Count() < 2)
                {
                    var usedin3 = SSData.Find(x => x.Logic.Contains(i.ItemName));
                    if (usedin3 != null && !SSInstance.Logic.Where(x => x.DictionaryName == i.ItemName).Any())
                    {
                        var requirement = SSInstance.Logic.Find(x => x.ItemName == i.ItemName);

                        if (requirement == null) { continue; }

                        Console.WriteLine($"{i.ItemName} was created");

                        SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = i.ItemName, IsFake = true, IsTrick = false, Required = new int[] { requirement.ID } });
                    }
                    continue; 
                }

                //Create Entries for any x of a,b,c entries
                Console.WriteLine($"{i.ItemName} is in the pool {EntryNumber.Count()} times");
                WasCounted.Add(i.ItemName);
                bool entryCreated = false;
                for (var j = 1; j <= EntryNumber.Count(); j++)
                {
                    string LogicName = $"{i.ItemName} x{j}";
                    var usedin = SSData.Find(x => x.Logic.Contains(LogicName));
                    if (usedin != null && !SSInstance.Logic.Where(x=>x.DictionaryName == LogicName).Any())
                    {
                        string TexttotheRight = textTotheRight(usedin.Logic, LogicName);
                        char ChartotheRight = (TexttotheRight.Count() > 0) ? TexttotheRight.ToCharArray()[0] : 'x';
                        if (!char.IsDigit(ChartotheRight))
                        {
                            entryCreated = true;
                            Console.WriteLine($"{LogicName} was created");

                            string Input = "";
                            bool drawcomma = false;
                            foreach (var l in EntryNumber)
                            {
                                if (drawcomma) { Input += (";" + l.ID.ToString()); }
                                else { Input += l.ID.ToString(); drawcomma = true; }
                            }

                            var Combos = LogicEditor.CreatePermiations(Input, j)
                                .Split(';').Select(x => x
                            .Split(',').Select(y => Int32.Parse(y)).ToArray()).ToArray(); ;

                            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = LogicName, IsFake = true, IsTrick = false, Conditionals = Combos });
                        }
                    }
                }
                if (entryCreated) { continue; }

                //Create Entries for any single item that exists in the item pool twice
                var usedin2 = SSData.Find(x => x.Logic.Contains(i.ItemName));
                if (usedin2 != null && !SSInstance.Logic.Where(x => x.DictionaryName == i.ItemName).Any())
                {
                    var Options = SSInstance.Logic.Where(x => x.ItemName == i.ItemName);
                    if (Options.Count() < 1) { continue; }

                    Console.WriteLine($"{i.ItemName} was created");

                    List<int[]> NewCond = new List<int[]>();
                    foreach (var k in Options) { NewCond.Add(new int[] { k.ID }); }

                    SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = i.ItemName, IsFake = true, IsTrick = false, Conditionals = NewCond.ToArray() });
                }
            }

        }

        public static void AddGRatitudeCrystalEntries(LogicObjects.TrackerInstance SSInstance)
        {
            List<int[]> GratitudeCrystals = new List<int[]>();
            foreach(var i in SSInstance.Logic.Where(x => x.ItemName == "Gratitude Crystal"))
            {
                GratitudeCrystals.Add(new int[] { i.ID });
            }
            foreach (var i in SSInstance.Logic.Where(x => x.ItemName == "5 Gratitude Crystals"))
            {
                GratitudeCrystals.Add(new int[] { i.ID });
                GratitudeCrystals.Add(new int[] { i.ID });
                GratitudeCrystals.Add(new int[] { i.ID });
                GratitudeCrystals.Add(new int[] { i.ID });
                GratitudeCrystals.Add(new int[] { i.ID });
            }
            int[][] GratitudeCond = GratitudeCrystals.ToArray();

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations5", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x5", IsFake = true, IsTrick = false, 
                Required = new int[] { SSInstance.Logic.Count() - 1 }, Conditionals = GratitudeCond });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations10", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x10", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Count() - 1 },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations20", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x20", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Count() - 1 },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations30", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x30", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Count() - 1 },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations40", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x40", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Count() - 1 },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations50", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x50", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Count() - 1 },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations60", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x60", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Count() - 1 },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations70", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x70", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Count() - 1 },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations80", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x80", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Count() - 1 },
                Conditionals = GratitudeCond
            });

            List<int[]> Bottles = new List<int[]>();
            foreach (var i in SSInstance.Logic.Where(x => x.ItemName == "Bottle"))
            {
                Bottles.Add(new int[] { i.ID });
            }
            int[][] BottlesCond = Bottles.ToArray();

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Empty Bottle", IsFake = true, IsTrick = false, Conditionals = BottlesCond });

            var rattleentry = SSInstance.Logic.Find(x => x.ItemName == "Baby's Rattle");
            if (rattleentry != null)
            {
                SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Baby Rattle", IsFake = true, IsTrick = false, Required = new int[] { rattleentry.ID } });
            }
        }

        public static void ParseHelpers(List<SSLocations> SSData, string[] MacroDataLines)
        {

            SSLocations CurrentEntry = new SSLocations { DictionaryName = "", isFake = true };
            string Logic = "";
            foreach (var i in MacroDataLines)
            {
                string line = i;

                if (i.StartsWith("# ")) { line = i.Substring(2); }
                else if (i.StartsWith("#")) { line = i.Substring(1); }
                if (line.Trim().StartsWith("#")) { continue; }
                if (line.Contains("#")) { line = line.Substring(0, line.IndexOf("#")); }

                if (isNewEntry(line, true))
                {
                    CreateNewEntry();
                    CurrentEntry.DictionaryName = line.Replace(":", "").Trim();
                }
                else if (line.StartsWith(" "))
                {
                    Logic += line;
                }
            }
            CreateNewEntry();

            void CreateNewEntry()
            {
                if (CurrentEntry.DictionaryName != "")
                {
                    CurrentEntry.Logic = Logic.Trim().Replace("?","");
                    SSData.Add(CurrentEntry);
                    CurrentEntry = new SSLocations { DictionaryName = "", isFake = true };
                    Logic = "";
                }
            }

        }

        public static List<LogicObjects.LogicDictionaryEntry> CreateSSDictionary(List<SSLocations> SSData)
        {
            List<LogicObjects.LogicDictionaryEntry> ssDictionary = new List<LogicObjects.LogicDictionaryEntry>();
            foreach(var i in SSData)
            {
                ssDictionary.Add(new LogicObjects.LogicDictionaryEntry { DictionaryName = i.DictionaryName, ItemName = i.ItemName, ItemSubType = i.ItemSubType, LocationArea = i.LocationArea, LocationName = i.LocationName, SpoilerItem = i.SpoilerItem, SpoilerLocation = i.SpoilerLocation });
            }
            return ssDictionary;
        }

        public static string[] CreateSSLogic(List<SSLocations> SSData)
        {
            List<string> ssDictionary = new List<string> { "-versionSSR 1" };
            foreach (var i in SSData)
            {
                ssDictionary.Add($"- {i.DictionaryName}");
                ssDictionary.Add($"");
                ssDictionary.Add($"");
                ssDictionary.Add($"0");
                ssDictionary.Add($"0");
                ssDictionary.Add($"");
            }
            return ssDictionary.ToArray();
        }

        static bool isNewEntry(string line, bool Alternate)
        {
            string Tell = (Alternate) ? ":" : "-";
            return (!line.StartsWith(" ") && line.Contains(Tell)) ;
        }

        public static List<SSLocations> ParseData(string[] ItemDataLines, string[] MacroDataLines)
        {
            List<SSLocations> SSDictionary = new List<SSLocations>();

            SSLocations CurrentEntry = new SSLocations { DictionaryName = "" };

            foreach (var i in ItemDataLines)
            {
                string DicNameAppend = "";
                var line = "";

                if (i.StartsWith("# "))
                {
                    line = i.Substring(2);
                    DicNameAppend = " UNUSED";
                }
                else if(i.StartsWith("#"))
                {
                    line = i.Substring(1);
                    DicNameAppend = " UNUSED";
                }
                else
                {
                    line = i;
                }

                if (line.Trim().StartsWith("#")) { continue; }
                if (line.Contains("#")) { line = line.Substring(0, line.IndexOf("#")); }

                if (isNewEntry(line, false))
                {
                    CreateNewEntry();
                    CurrentEntry.DictionaryName = line.Replace(":", "").Trim();
                    CurrentEntry.ItemSubType = "Item";
                    var Data = line.Split('-').Select(x => x.Trim()).ToArray();
                    if (Data.Count() > 1)
                    {
                        CurrentEntry.LocationName = line.Replace(":", "").Trim();
                        CurrentEntry.SpoilerLocation = line.Replace(":", "").Trim();
                        CurrentEntry.LocationArea = Data[0].Trim();
                    }
                    else
                    {
                        CurrentEntry.LocationName = line.Replace(":", "").Trim();
                        CurrentEntry.SpoilerLocation = line.Replace(":", "").Trim();
                        CurrentEntry.LocationArea = "Misc";
                    }

                }
                else if (line.Contains("Need:"))
                {
                    CurrentEntry.Logic = line.Trim().Substring(line.IndexOf(":")).Trim().Replace("?","");
                }
                else if (line.Contains("original item:"))
                {
                    CurrentEntry.ItemName = line.Substring(line.IndexOf(":") + 1).Trim();
                    CurrentEntry.SpoilerItem = line.Substring(line.IndexOf(":") + 1).Trim();
                }

            }
            CreateNewEntry();

            ParseHelpers(SSDictionary, MacroDataLines);

            foreach (var i in SSDictionary) { if (i.Logic.Trim() == "Nothing") { i.Logic = ""; } }

            return SSDictionary;

            


            void CreateNewEntry()
            {
                if (CurrentEntry.DictionaryName != "")
                {
                    SSDictionary.Add(CurrentEntry);
                    CurrentEntry = new SSLocations { DictionaryName = "" };
                }
            }

        }
    }
}
