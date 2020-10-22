using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms;
using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Other_Games
{
    class SkywardSwordTools
    {
        public class SSLocations
        {
            public string DictionaryName { get; set; } = "";
            public string LocationName { get; set; } = "";
            public string ItemName { get; set; } = "";
            public string LocationArea { get; set; } = "";
            public string Logic { get; set; } = "";
            public bool isFake { get; set; } = false;
            public string SpoilerLocation { get; set; } = "";
            public string SpoilerItem { get; set; } = "";
            public string ItemSubType { get; set; } = "";
        }
        public static void CreateData()
        {
            //System.Net.WebClient wc = new System.Net.WebClient();
            //string ItemData = wc.DownloadString("https://raw.githubusercontent.com/lepelog/sslib/master/SS%20Rando%20Logic%20-%20Item%20Location.yaml");
            //string[] ItemDataLines = ItemData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            //string MacroData = wc.DownloadString("https://raw.githubusercontent.com/lepelog/sslib/master/SS%20Rando%20Logic%20-%20Macros.yaml");
            //string[] MacroDataLines = MacroData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            string[] ItemDataLines = File.ReadAllLines(@"D:\Emulated Games\Emulator\Dolphin\Dolphin-x64 Skyward Sword Randomizer\Seed Testing\SS Rando Logic - Item Location.txt");
            string[] MacroDataLines = File.ReadAllLines(@"D:\Emulated Games\Emulator\Dolphin\Dolphin-x64 Skyward Sword Randomizer\Seed Testing\SS Rando Logic - Macros.txt");

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
            AddAdditionalEntries(SSInstance);
            CreateLogicHelpers(SSData, SSInstance);

            LogicEditor.EditorInstance = SSInstance;
            ApplyLogic(SSData, SSInstance);
            LogicEditor.EditorInstance = backupEditorInstance;

            var Save = true;

            if (!Save)
            {
                LogicObjects.MainTrackerInstance = SSInstance;
                LogicEditing.CalculateItems(SSInstance);
                MainInterface.CurrentProgram.FormatMenuItems();
                MainInterface.CurrentProgram.ResizeObject();
            }
            else
            {
                SaveDictionary(SSInstance);
                SaveLogic(SSInstance);
            }




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

        public static void SaveDictionary(LogicObjects.TrackerInstance SSInstance)
        {
            List<string> csv = new List<string> { "DictionaryName,LocationName,ItemName,LocationArea,ItemSubType,SpoilerLocation,SpoilerItem" };
            foreach (LogicObjects.LogicDictionaryEntry entry in SSInstance.LogicDictionary)
            {
                csv.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                     entry.DictionaryName, entry.LocationName, entry.ItemName, entry.LocationArea,
                     entry.ItemSubType, entry.SpoilerLocation, entry.SpoilerItem));
            }
            SaveFileDialog saveDic = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                Title = "Save Dictionary File",
                FileName = "SSRDICTIONARYV1.csv"
            };
            saveDic.ShowDialog();
            File.WriteAllLines(saveDic.FileName, csv);
        }

        public static void SaveLogic(LogicObjects.TrackerInstance SSInstance)
        {
            var csv = LogicEditing.WriteLogicToArray(SSInstance);
            SaveFileDialog saveDic = new SaveFileDialog
            {
                Filter = "Text File (*.txt)|*.txt",
                Title = "Save Logic File",
                FileName = "SSRLogic.txt"
            };
            saveDic.ShowDialog();
            File.WriteAllLines(saveDic.FileName, csv);
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

        public static void AddAdditionalEntries(LogicObjects.TrackerInstance SSInstance)
        {
            //Create gratitude crystal logic manually using MMRTCombinations entries
            List<int[]> GratitudeCrystals = new List<int[]>();
            foreach(var i in SSInstance.Logic.Where(x => x.ItemName == "Gratitude Crystal" || x.ItemName == "5 Gratitude Crystals"))
            {
                if (i.ItemName == "Gratitude Crystal") { GratitudeCrystals.Add(new int[] { i.ID }); }
                //Add the 5 pack gratiude crystals 5 times because having 1 is worth 5 crystals
                else if (i.ItemName == "5 Gratitude Crystals") { for (var g = 0; g < 5; g++) { GratitudeCrystals.Add(new int[] { i.ID }); } }
            }
            int[][] GratitudeCond = GratitudeCrystals.ToArray();

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations5", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x5", IsFake = true, IsTrick = false, 
                Required = new int[] { SSInstance.Logic.Count() - 1 }, 
                Conditionals = GratitudeCond });

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

            //For some reason any bottle is refered to as "Empty bottle". This is probaly a mistake but we'll add the entry anyway
            List<int[]> Bottles = new List<int[]>();
            foreach (var i in SSInstance.Logic.Where(x => x.ItemName == "Bottle"))
            {
                Bottles.Add(new int[] { i.ID });
            }
            int[][] BottlesCond = Bottles.ToArray();

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Empty Bottle", IsFake = true, IsTrick = false, Conditionals = BottlesCond });

            //For some reason the baby rattle is spelled differently in some places. This is probaly a mistake but we'll add a fake item to refer it to the correct entry.
            var rattleentry = SSInstance.Logic.Find(x => x.ItemName == "Baby's Rattle");
            if (rattleentry != null)
            {
                SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Baby Rattle", IsFake = true, IsTrick = false, Required = new int[] { rattleentry.ID } });
            }

            //For some reason Song of the hero is shortened to SOTH in some places, I have no idea how the randomizer code is able to handle this but whatever. Same deal as above
            var LSOTH = SSInstance.Logic.Find(x => x.ItemName == "Lanayru Soth part");
            if (rattleentry != null)
            {
                SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Lanayru Song of the Hero Part", IsFake = true, IsTrick = false, Required = new int[] { LSOTH.ID } });
            }
            var FSOTH = SSInstance.Logic.Find(x => x.ItemName == "Faron Soth Part");
            if (rattleentry != null)
            {
                SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Faron Song of the Hero Part", IsFake = true, IsTrick = false, Required = new int[] { FSOTH.ID } });
            }
            var ESOTH = SSInstance.Logic.Find(x => x.ItemName == "Eldin Soth part");
            if (rattleentry != null)
            {
                SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Eldin Song of the Hero Part", IsFake = true, IsTrick = false, Required = new int[] { ESOTH.ID } });
            }

            //These are not supposed to be a thing so just make it require it's self to hide it
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Boko Base", IsFake = true, IsTrick = false, Required= new[] { SSInstance.Logic.Count() } });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Flooded Faron Woods", IsFake = true, IsTrick = false, Required = new int[] { SSInstance.Logic.Count() } });

            //Fix Dungeon Rando Entries
            var AccessSW = SSInstance.Logic.Find(x => x.DictionaryName == "Can Access Dungeon Entrance In Deep Woods");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Skyview", IsFake = true, IsTrick = false, Required = new int[] { AccessSW.ID } });
            var AccessET = SSInstance.Logic.Find(x => x.DictionaryName == "Can Access Dungeon Entrance In Eldin Volcano");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Earth Temple", IsFake = true, IsTrick = false, Required = new int[] { AccessET.ID } });
            var AccessLD = SSInstance.Logic.Find(x => x.DictionaryName == "Can Access Dungeon Entrance In Lanayru Desert");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Lanayru Mining Facility", IsFake = true, IsTrick = false, Required = new int[] { AccessLD.ID } });
            var AccessLF = SSInstance.Logic.Find(x => x.DictionaryName == "Can Access Dungeon Entrance In Lake Floria");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Ancient Cistern", IsFake = true, IsTrick = false, Required = new int[] { AccessLF.ID } });
            var AccessSS = SSInstance.Logic.Find(x => x.DictionaryName == "Can Access Dungeon Entrance In Sand Sea");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Sandship", IsFake = true, IsTrick = false, Required = new int[] { AccessSS.ID } });
            var AccessFS = SSInstance.Logic.Find(x => x.DictionaryName == "Can Access Dungeon Entrance In Volcano Summit");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Fire Sanctuary", IsFake = true, IsTrick = false, Required = new int[] { AccessFS.ID } });
            var AccessSK = SSInstance.Logic.Find(x => x.DictionaryName == "Can Access Dungeon Entrance On Skyloft");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Skykeep", IsFake = true, IsTrick = false, Required = new int[] { AccessSK.ID } });

            //Manually fix some wierd key related entries, I think this is because of some error in my code but whatever.
            var SWBossKey = SSInstance.Logic.Find(x => x.ItemName == "Skyview Boss Key");
            if (SWBossKey != null)
            {
                SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "SW Boss Key", IsFake = true, IsTrick = false, Required = new int[] { SWBossKey.ID } });
            }
            var LMFKey = SSInstance.Logic.Find(x => x.ItemName == "LMF Small Key");
            if (LMFKey != null)
            {
                SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "LMF Small Key x1", IsFake = true, IsTrick = false, Required = new int[] { LMFKey.ID } });
            }
            var SKSmallKey = SSInstance.Logic.Find(x => x.ItemName == "Skykeep Small Key");
            if (SKSmallKey != null)
            {
                SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Skykeep Small Key x1", IsFake = true, IsTrick = false, Required = new int[] { SKSmallKey.ID } });
                SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "SK Small Key x1", IsFake = true, IsTrick = false, Required = new int[] { SKSmallKey.ID } });
            }
            var LCSmallKey = SSInstance.Logic.Find(x => x.ItemName == "LanayruCaves Small Key");
            if (LCSmallKey != null)
            {
                SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "LanayruCaves Small Key x1", IsFake = true, IsTrick = false, Required = new int[] { LCSmallKey.ID } });
            }

            //Add new entries for the tablets so they can be aquired by either the tablet it's self or if "SuffleTablet" setting is off. 
            var RubyTablet = SSInstance.Logic.Find(x => x.ItemName == "Ruby Tablet");
            var EmerladTablet = SSInstance.Logic.Find(x => x.ItemName == "Emerald Tablet");
            var AmberTablet = SSInstance.Logic.Find(x => x.ItemName == "Amber Tablet");
            var HaveTablets = SSInstance.Logic.Find(x => x.DictionaryName == "SettingSuffleTabletFalse");
            List<int[]> RubyCond = new List<int[]> { new int[] { RubyTablet.ID }, new int[] { HaveTablets.ID } };
            List<int[]> EmeraldCond = new List<int[]> { new int[] { EmerladTablet.ID }, new int[] { HaveTablets.ID } };
            List<int[]> AmberCond = new List<int[]> { new int[] { AmberTablet.ID }, new int[] { HaveTablets.ID } };
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Ruby Tablet", IsFake = true, IsTrick = false, Conditionals = RubyCond.ToArray() });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Emerald Tablet", IsFake = true, IsTrick = false, Conditionals = EmeraldCond.ToArray() });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Amber Tablet", IsFake = true, IsTrick = false, Conditionals = AmberCond.ToArray() });


        }

        public static void ParseHelpers(List<SSLocations> SSData, string[] MacroDataLines)
        {

            SSLocations CurrentEntry = new SSLocations { DictionaryName = "", isFake = true };
            string Logic = "";
            foreach (var i in MacroDataLines)
            {
                string line = i;

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
                    MakeDungeonEntranceRealLocation(CurrentEntry);
                    CurrentEntry.Logic = Logic.Trim().Replace("?","");
                    SSData.Add(CurrentEntry);
                    CurrentEntry = new SSLocations { DictionaryName = "", isFake = true };
                    Logic = "";
                }
            }

            void MakeDungeonEntranceRealLocation(SSLocations Entry)
            {
                switch (Entry.DictionaryName)
                {
                    case "Can Access Dungeon Entrance In Deep Woods":
                        Entry.isFake = false;
                        Entry.LocationName = "Dungeon Entrance In Deep Woods";
                        Entry.ItemName = "Skyview";
                        Entry.LocationArea = "Faron Woods";
                        Entry.ItemSubType = "DungeonEntrance";
                        Entry.SpoilerLocation = CurrentEntry.LocationName;
                        Entry.SpoilerItem = CurrentEntry.ItemName;
                        break;
                    case "Can Access Dungeon Entrance In Eldin Volcano":
                        Entry.isFake = false;
                        Entry.LocationName = "Dungeon Entrance In Eldin Volcano";
                        Entry.ItemName = "Earth Temple";
                        Entry.LocationArea = "Eldin Volcano";
                        Entry.ItemSubType = "DungeonEntrance";
                        Entry.SpoilerLocation = CurrentEntry.LocationName;
                        Entry.SpoilerItem = CurrentEntry.ItemName;
                        break;
                    case "Can Access Dungeon Entrance In Lanayru Desert":
                        Entry.isFake = false;
                        Entry.LocationName = "Dungeon Entrance In Lanayru Desert";
                        Entry.ItemName = "Lanayru Mining Facility";
                        Entry.LocationArea = "Lanayru";
                        Entry.ItemSubType = "DungeonEntrance";
                        Entry.SpoilerLocation = CurrentEntry.LocationName;
                        Entry.SpoilerItem = CurrentEntry.ItemName;
                        break;
                    case "Can Access Dungeon Entrance In Lake Floria":
                        Entry.isFake = false;
                        Entry.LocationName = "Dungeon Entrance In Lake Floria";
                        Entry.ItemName = "Ancient Cistern";
                        Entry.LocationArea = "Lake Floria";
                        Entry.ItemSubType = "DungeonEntrance";
                        Entry.SpoilerLocation = CurrentEntry.LocationName;
                        Entry.SpoilerItem = CurrentEntry.ItemName;
                        break;
                    case "Can Access Dungeon Entrance In Sand Sea":
                        Entry.isFake = false;
                        Entry.LocationName = "Dungeon Entrance In Sand Sea";
                        Entry.ItemName = "Sandship";
                        Entry.LocationArea = "Lanayru Sand Sea";
                        Entry.ItemSubType = "DungeonEntrance";
                        Entry.SpoilerLocation = CurrentEntry.LocationName;
                        Entry.SpoilerItem = CurrentEntry.ItemName;
                        break;
                    case "Can Access Dungeon Entrance In Volcano Summit":
                        Entry.isFake = false;
                        Entry.LocationName = "Dungeon Entrance In Volcano Summit";
                        Entry.ItemName = "Fire Sanctuary";
                        Entry.LocationArea = "Eldin Volcano";
                        Entry.ItemSubType = "DungeonEntrance";
                        Entry.SpoilerLocation = CurrentEntry.LocationName;
                        Entry.SpoilerItem = CurrentEntry.ItemName;
                        break;
                    case "Can Access Dungeon Entrance On Skyloft":
                        Entry.isFake = false;
                        Entry.LocationName = "Dungeon Entrance On Skyloft";
                        Entry.ItemName = "Skykeep";
                        Entry.LocationArea = "Skyloft";
                        Entry.ItemSubType = "DungeonEntrance";
                        Entry.SpoilerLocation = CurrentEntry.LocationName;
                        Entry.SpoilerItem = CurrentEntry.ItemName;
                        break;

                }
            }

        }

        public static List<LogicObjects.LogicDictionaryEntry> CreateSSDictionary(List<SSLocations> SSData)
        {
            List<LogicObjects.LogicDictionaryEntry> ssDictionary = new List<LogicObjects.LogicDictionaryEntry>();
            foreach(var i in SSData.Where(x=>!x.isFake))
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

        static void CreateCustomData(List<SSLocations> SSData)
        {
            SSData.Add(new SSLocations { DictionaryName = "Item Emerald Tablet", isFake = false, LocationName = "", ItemName = "Emerald Tablet", ItemSubType = "Item", SpoilerItem = "Emerald Tablet" });
            SSData.Add(new SSLocations { DictionaryName = "SettingSuffleTablet", isFake = false, LocationName = "Shuffled Tablets", ItemName = "", ItemSubType = "SettingSuffleTablet", LocationArea = "%Settings%", SpoilerLocation = "SettingSuffleTablet", SpoilerItem = "SettingSuffleTablet" });
            SSData.Add(new SSLocations { DictionaryName = "SettingSuffleTabletTrue", isFake = false, LocationName = "", ItemName = "True", ItemSubType = "SettingSuffleTablet", LocationArea = "%Settings%", SpoilerLocation = "SettingSuffleTabletTrue", SpoilerItem = "SettingSuffleTabletTrue" });
            SSData.Add(new SSLocations { DictionaryName = "SettingSuffleTabletFalse", isFake = false, LocationName = "", ItemName = "False", ItemSubType = "SettingSuffleTablet", LocationArea = "%Settings%", SpoilerLocation = "SettingSuffleTabletFalse", SpoilerItem = "SettingSuffleTabletFalse" });
            SSData.Add(new SSLocations { DictionaryName = "SettingStartingSword", isFake = false, LocationName = "Start With Sword", ItemName = "", ItemSubType = "SettingStartingSword", LocationArea = "%Settings%", SpoilerLocation = "SettingStartingSword", SpoilerItem = "SettingStartingSword" });
            SSData.Add(new SSLocations { DictionaryName = "SettingStartingSwordTrue", isFake = false, LocationName = "", ItemName = "Progressive Sword", ItemSubType = "SettingStartingSword", LocationArea = "%Settings%", SpoilerLocation = "SettingStartingSwordTrue", SpoilerItem = "SettingStartingSwordTrue" });
            SSData.Add(new SSLocations { DictionaryName = "SettingStartingSwordFalse", isFake = false, LocationName = "", ItemName = "None", ItemSubType = "SettingStartingSword", LocationArea = "%Settings%", SpoilerLocation = "SettingStartingSwordFalse", SpoilerItem = "SettingStartingSwordFalse" });
            //Extra Junk items to fill the pool
            for (var i = 0; i < 10; i++) 
            { 
                SSData.Add(new SSLocations { DictionaryName = $"ExtraTreasure{i}", isFake = false, LocationName = "", ItemName = "Rare Treasure", ItemSubType = "Item", LocationArea = "", SpoilerItem = "Rare Treasure", SpoilerLocation = $"ExtraTreasure{i}" }); 
            }
            //Not sure why these weren't in the item data but whatever
            SSData.Add(new SSLocations { DictionaryName = $"ExtraHP1", isFake = false, LocationName = "", ItemName = "Heart Piece", ItemSubType = "Item", LocationArea = "", SpoilerItem = "Heart Piece", SpoilerLocation = $"ExtraHP1" });
            SSData.Add(new SSLocations { DictionaryName = $"ExtraHP2", isFake = false, LocationName = "", ItemName = "Heart Piece", ItemSubType = "Item", LocationArea = "", SpoilerItem = "Heart Piece", SpoilerLocation = $"ExtraHP2" });
            SSData.Add(new SSLocations { DictionaryName = $"ExtraBottle1", isFake = false, LocationName = "", ItemName = "Bottle", ItemSubType = "Item", LocationArea = "", SpoilerItem = "Bottle|Empty Bottle", SpoilerLocation = $"ExtraBottle1" });

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

                line = i;

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

            foreach (var i in SSDictionary) 
            { 
                if (i.Logic.Trim() == "Nothing") { i.Logic = ""; }
                FixSpoilerNames(i);
            }

            CreateCustomData(SSDictionary);

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

        public static void FixSpoilerNames(SSLocations SSData)
        {
            switch (SSData.SpoilerItem)
            {
                case "Lanayru Soth part":
                    SSData.SpoilerItem += "|Lanayru Song of the Hero Part";
                    break;
                case "Bottle":
                    SSData.SpoilerItem += "|Empty Bottle";
                    break;
                case "Baby's Rattle":
                    SSData.SpoilerItem += "|Baby Rattle";
                    break;
                case "Faron Soth Part":
                    SSData.SpoilerItem += "|Faron Song of the Hero Part";
                    break;
                case "Skyview Map":
                    SSData.SpoilerItem += "|SW Map";
                    break;
                case "Skyview Boss Key":
                    SSData.SpoilerItem += "|SW Boss Key";
                    break;
                case "Eldin Soth part":
                    SSData.SpoilerItem += "|Eldin Song of the Hero Part";
                    break;
                case "Sandship Map":
                    SSData.SpoilerItem += "|SS Map";
                    break;
                case "hylian shield":
                    SSData.SpoilerItem += "|Hylian Shield";
                    break;
                case "Skykeep Small Key":
                    SSData.SpoilerItem += "|SK Small Key";
                    break;
                case "Skykeep Map":
                    SSData.SpoilerItem += "|SK Map";
                    break;
            }
        }

        public static string[] HandleSSRSpoilerLog(string[] Log = null)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");

            //CreateWWRLogicFile(); return;

            bool ManualConvert = (Log == null);
            string filename = "";
            if (ManualConvert)
            {
                OpenFileDialog SelectedFile = new OpenFileDialog
                {
                    Title = $"Select WWR Spoiler Log",
                    Filter = "OOTR Spoiler Log (*.txt)|*.txt",
                    FilterIndex = 1,
                    Multiselect = false
                };
                if (SelectedFile.ShowDialog() != DialogResult.OK) { return null; }
                filename = SelectedFile.FileName;
                Log = File.ReadAllLines(SelectedFile.FileName);
            }


            bool AtItems = false;
            bool AtEntrances = false;
            List<string> SpoilerData = new List<string>();
            SpoilerData.Add("Converted SSR");
            string header = "";
            var FileContent = Log.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            foreach (var line in FileContent)
            {
                if (line.Contains("All item locations:"))
                {
                    AtItems = true;
                }
                if (line.Contains("Entrances:"))
                {
                    AtItems = false;
                    AtEntrances = true;
                }
                if (AtItems || AtEntrances)
                {
                    var Parts = line.Split(':');
                    if (string.IsNullOrWhiteSpace(Parts[1])) { header = Parts[0].Trim() + " - "; continue; }
                    if (AtEntrances) { header = ""; }
                    if (Parts.Length < 2) { continue; }
                    SpoilerData.Add($"{header}{Parts[0].Trim()}->{Parts[1].Trim()}");
                    Console.WriteLine($"{header}{Parts[0].Trim()}->{Parts[1].Trim()}");
                }
            }

            var StartWithTablets = SpoilerData.Find(x => x.Contains("->Ruby Tablet")) == null;
            if (StartWithTablets) { SpoilerData.Add($"SettingSuffleTablet->SettingSuffleTabletFalse"); }
            else { SpoilerData.Add($"SettingSuffleTablet->SettingSuffleTabletTrue"); }

            var StartWithSword = true; //I assume this will be togglable Later
            if (StartWithSword) { SpoilerData.Add($"SettingStartingSword->SettingStartingSwordTrue"); }
            else { SpoilerData.Add($"SettingStartingSword->SettingStartingSwordFalse"); }

            if (ManualConvert)
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Spoiler Log (*.txt)|*.txt",
                    FilterIndex = 1,
                    FileName = filename.Replace(".txt", " MMRT Converted")
                };
                if (saveDialog.ShowDialog() != DialogResult.OK) { return null; }
                File.WriteAllLines(saveDialog.FileName, SpoilerData);
            }
            return SpoilerData.ToArray();

        }

        public static void TestSpoilerLog()
        {

            string[] Spoiler = File.ReadAllLines(@"D:\Emulated Games\Emulator\Dolphin\Dolphin-x64 Skyward Sword Randomizer\Seed Testing\SS Random Tablet - Spoiler Log.txt");
            var SpoilerData = SkywardSwordTools.HandleSSRSpoilerLog(Spoiler);


            Tools.ReadTextSpoilerlog(LogicObjects.MainTrackerInstance, SpoilerData);

        }
    }
}
