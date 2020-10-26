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
        public class SSLocation
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

            //string[] ItemDataLines = File.ReadAllLines(@"D:\Emulated Games\Emulator\Dolphin\Dolphin-x64 Skyward Sword Randomizer\Seed Testing\SS Rando Logic - Item Location.yaml");
            //string[] MacroDataLines = File.ReadAllLines(@"D:\Emulated Games\Emulator\Dolphin\Dolphin-x64 Skyward Sword Randomizer\Seed Testing\SS Rando Logic - Macros.yaml");

            string[] ItemDataLines = File.ReadAllLines(@"C:\Users\ttalbot\Documents\VS CODE STUFF\SS Rando Logic - Item Location\SS Rando Logic - Item Location.yaml");

            string[] MacroDataLines = File.ReadAllLines(@"C:\Users\ttalbot\Documents\VS CODE STUFF\SS Rando Logic - Item Location\SS Rando Logic - Macros.yaml");

            var backupEditorInstance = Utility.CloneTrackerInstance(LogicEditor.EditorInstance);

            var SSData = ParseData(ItemDataLines, MacroDataLines);

            LogicObjects.TrackerInstance SSInstance = new LogicObjects.TrackerInstance();
            SSInstance.LogicDictionary = CreateSSDictionary(SSData);
            SSInstance.RawLogicFile = CreateSSLogic(SSData);

            LogicEditing.PopulateTrackerInstance(SSInstance);
            AddAdditionalEntries(SSInstance);
            CreateLogicHelpers(SSData, SSInstance);

            LogicEditor.EditorInstance = SSInstance;
            ApplyLogic(SSData, SSInstance);
            LogicEditor.EditorInstance = backupEditorInstance;

            foreach(var i in SSInstance.Logic) { LogicEditor.CleanLogicEntry(i, SSInstance); }

            var Save = false;

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

        public static void ApplyLogic(List<SSLocation> SSData, LogicObjects.TrackerInstance SSInstance)
        {
            List<string> Failed = new List<string>();
            foreach(var i in SSInstance.Logic.Where(x=> x.Conditionals == null))
            {

                var dataentry = SSData.Find(x => x.DictionaryName == i.DictionaryName);
                if (dataentry == null)
                {
                   // Console.WriteLine($"Could not find {i.DictionaryName} in Data File");
                    continue; 
                }
                if (string.IsNullOrWhiteSpace(dataentry.Logic) || dataentry.Logic.Trim() == "Nothing")
                {
                    //Console.WriteLine($"{i.DictionaryName} Does not have Logic");
                    continue; 
                }

                //Console.WriteLine("========================================================");
                //Console.WriteLine($"Creating Logic for {i.DictionaryName}");

                LogicParser SSParser = new LogicParser();
                //Console.WriteLine($"{dataentry.Logic}\nWas converted to:");
                string ConvertedLogic = LogicParser.ConvertDicNameToID(dataentry.Logic);
                //Console.WriteLine($"{ConvertedLogic}");
                try
                {
                    i.Conditionals = SSParser.ConvertLogicToConditional(ConvertedLogic);
                    //Console.WriteLine($"Successufully Created Logic for {i.DictionaryName}");
                }
                catch
                {
                    Failed.Add(i.DictionaryName + "\n" + dataentry.Logic + "\n" + ConvertedLogic + "\n==============================");
                    //Console.WriteLine($"Failed to convert Logic for {i.DictionaryName}");
                }

            }
            Console.WriteLine("");
            if (Failed.Count == 0)
            {
                Console.WriteLine("All Entries were created Successfully");
            }
            else
            {
                Console.WriteLine("The following entries failed");
                foreach (var i in Failed) { Console.WriteLine(i); }
            }
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

        public static void CreateLogicHelpers(List<SSLocation> SSData, LogicObjects.TrackerInstance SSInstance)
        {
            List<string> ItemNamesProcessed = new List<string>();

            var EntriesInLogic = GetItemsUsedInLogic(SSData);

            foreach (var i in SSData.Where(x=>!x.isFake && !string.IsNullOrWhiteSpace(x.ItemName)))
            {
                if (ItemNamesProcessed.Contains(i.ItemName)) { continue; }
                ItemNamesProcessed.Add(i.ItemName);
                var ItemsWithThisName = SSInstance.Logic.Where(x => x.ItemName == i.ItemName);
                //Console.WriteLine($"{i.ItemName} is in the pool {ItemsWithThisName.Count()} times");

                //Create Entries for any x of a,b,c entries
                for (var j = 1; j <= ItemsWithThisName.Count(); j++)
                {
                    string LogicName = $"{i.ItemName} x{j}";
                    if (EntriesInLogic.Contains(LogicName) && !SSInstance.Logic.Where(x=>x.DictionaryName == LogicName).Any())
                    {
                        Console.WriteLine($"{LogicName} was created");
                        SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = LogicName, IsFake = true, IsTrick = false, 
                            Conditionals = LogicEditor.CreatePermiations(ItemsWithThisName.Select(x => x.ID).ToArray(), j) });
                    }
                }

                //Create Entries for items that are reffered to in logic, add all available of this item as conditionals
                if (EntriesInLogic.Contains(i.ItemName) && !SSInstance.Logic.Where(x => x.DictionaryName == i.ItemName).Any())
                {
                    Console.WriteLine($"{i.ItemName} was created");
                    SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = i.ItemName, IsFake = true, IsTrick = false, Conditionals = ItemsWithThisName.Select(x => new int[] { x.ID }).ToArray() });
                }
            }
                
        }

        public static List<string> GetItemsUsedInLogic(List<SSLocation> SSData)
        {
            List<string> AllEntries = new List<string>(); ;
            foreach(var i in SSData.Where(x => !string.IsNullOrWhiteSpace(x.Logic)))
            {
                foreach(var j in LogicParser.GetEntries(i.Logic).Where(x => x.Length > 0 && !LogicParser.ISLogicChar(x[0]) && !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct())
                {
                    AllEntries.Add(j);
                }
            }
            return AllEntries;
        }

        public static void AddAdditionalEntries(LogicObjects.TrackerInstance SSInstance)
        {
            //Add extra fake item entries and adjust logic

            //Add MMRTCombinations Entries for use later
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations2", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations5", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations10", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations20", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations30", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations40", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations50", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations60", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations70", IsFake = true, IsTrick = false });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTCombinations80", IsFake = true, IsTrick = false });


            //Create gratitude crystal logic manually using MMRTCombinations entries
            List<int[]> GratitudeCrystals = new List<int[]>();
            foreach(var i in SSInstance.Logic.Where(x => x.ItemName == "Gratitude Crystal" || x.ItemName == "5 Gratitude Crystals"))
            {
                if (i.ItemName == "Gratitude Crystal") { GratitudeCrystals.Add(new int[] { i.ID }); }
                //Add the 5 pack gratiude crystals 5 times because having 1 is worth 5 crystals
                else if (i.ItemName == "5 Gratitude Crystals") { for (var g = 0; g < 5; g++) { GratitudeCrystals.Add(new int[] { i.ID }); } }
            }
            int[][] GratitudeCond = GratitudeCrystals.ToArray();

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x5", IsFake = true, IsTrick = false, 
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "MMRTCombinations5").ID }, 
                Conditionals = GratitudeCond });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x10", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "MMRTCombinations10").ID },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x20", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "MMRTCombinations20").ID },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x30", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "MMRTCombinations30").ID },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x40", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "MMRTCombinations40").ID },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x50", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "MMRTCombinations50").ID },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x60", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "MMRTCombinations60").ID },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x70", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "MMRTCombinations70").ID },
                Conditionals = GratitudeCond
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Gratitude Crystal x80", IsFake = true, IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "MMRTCombinations80").ID },
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
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Baby Rattle", IsFake = true, IsTrick = false, Required = new int[] { rattleentry.ID } });

            //For some reason Song of the hero is shortened to SOTH in some places, I have no idea how the randomizer code is able to handle this but whatever. Same deal as above
            var LSOTH = SSInstance.Logic.Find(x => x.ItemName == "Lanayru Soth part");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Lanayru Song of the Hero Part", IsFake = true, IsTrick = false, Required = new int[] { LSOTH.ID } });

            var FSOTH = SSInstance.Logic.Find(x => x.ItemName == "Faron Soth Part");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Faron Song of the Hero Part", IsFake = true, IsTrick = false, Required = new int[] { FSOTH.ID } });

            var ESOTH = SSInstance.Logic.Find(x => x.ItemName == "Eldin Soth part");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Eldin Song of the Hero Part", IsFake = true, IsTrick = false, Required = new int[] { ESOTH.ID } });

            //These are not supposed to be a thing so just make it require it's self to hide it
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Boko Base", IsFake = true, IsTrick = false, Required= new[] { SSInstance.Logic.Count() } });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Flooded Faron Woods", IsFake = true, IsTrick = false, Required = new int[] { SSInstance.Logic.Count() } });

            //Setup Dungeon Rando Entries
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

            //Manually fix some weird key related entries.
            var SWBossKey = SSInstance.Logic.Find(x => x.ItemName == "Skyview Boss Key");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "SW Boss Key", IsFake = true, IsTrick = false, Required = new int[] { SWBossKey.ID } });

            var LMFKey = SSInstance.Logic.Find(x => x.ItemName == "LMF Small Key");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "LMF Small Key x1", IsFake = true, IsTrick = false, Required = new int[] { LMFKey.ID } });

            var SKSmallKey = SSInstance.Logic.Find(x => x.ItemName == "Skykeep Small Key");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Skykeep Small Key x1", IsFake = true, IsTrick = false, Required = new int[] { SKSmallKey.ID } });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "SK Small Key x1", IsFake = true, IsTrick = false, Required = new int[] { SKSmallKey.ID } });

            var LCSmallKey = SSInstance.Logic.Find(x => x.ItemName == "LanayruCaves Small Key");
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "LanayruCaves Small Key x1", IsFake = true, IsTrick = false, Required = new int[] { LCSmallKey.ID } });

            //Add new entries for the tablets so they can be aquired by either the tablet it's self or by the "SuffleTablet" setting being off.
            var HaveTablets = SSInstance.Logic.Find(x => x.DictionaryName == "SettingSuffleTabletFalse");
            int[][] RubyCond = new int[][] { new int[] { SSInstance.Logic.Find(x => x.ItemName == "Ruby Tablet").ID }, new int[] { HaveTablets.ID } };
            int[][] EmeraldCond = new int[][] { new int[] { SSInstance.Logic.Find(x => x.ItemName == "Emerald Tablet").ID }, new int[] { HaveTablets.ID } };
            int[][] AmberCond = new int[][] { new int[] { SSInstance.Logic.Find(x => x.ItemName == "Amber Tablet").ID }, new int[] { HaveTablets.ID } };

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Ruby Tablet", IsFake = true, IsTrick = false, Conditionals = RubyCond });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Emerald Tablet", IsFake = true, IsTrick = false, Conditionals = EmeraldCond });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Amber Tablet", IsFake = true, IsTrick = false, Conditionals = AmberCond });

            //Add Game clear logic to the "Can Access Past" entry based on what 2 dungeons are required
            var CanAccessPast = SSInstance.Logic.Find(x => x.DictionaryName == "Can Access Past");

            CanAccessPast.Required = new int[]
            {
                SSInstance.Logic.Find(x => x.DictionaryName == "MMRTCombinations2").ID,
                SSInstance.Logic.Find(x => x.DictionaryName == "Master Sword").ID, 
                SSInstance.Logic.Find(x => x.DictionaryName == "Can Access Sealed Temple").ID
            };

            CanAccessPast.Conditionals = new int[][]
            {
                new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Skyview").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonSW").ID },
                new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Earth Temple").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonET").ID },
                new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Lanayru Mining Facility").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonMF").ID },
                new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Ancient Cistern").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonAC").ID },
                new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Sandship").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonSS").ID },
                new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Fire Sanctuary").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonFS").ID },
                new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Skykeep").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonSK").ID },
            };

            //Add MMRTGAmeClear entry for the playthrough generator
            SSInstance.Logic.Add(new LogicObjects.LogicEntry  { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTGameClear", IsFake = true, IsTrick = false, 
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Reach and Defeat Demise").ID } 
            });

            //Create Sword Entries. It's a little wierd since starting with a sword technically give you two upgrades, so better to do it manually
            var StartingSwordTrueSetting = SSInstance.Logic.Find(x => x.DictionaryName == "SettingStartingSwordTrue").ID;
            var allSwords = SSInstance.Logic.Where(x => x.ItemName == "Progressive Sword").Select(x=>x.ID).ToList();
            allSwords.Add(StartingSwordTrueSetting);
            allSwords.Add(StartingSwordTrueSetting);

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Progressive Sword x1", IsFake = true, IsTrick = false, 
                Conditionals = LogicEditor.CreatePermiations(allSwords.ToArray(), 1)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Progressive Sword x2", IsFake = true, IsTrick = false,
                Conditionals = LogicEditor.CreatePermiations(allSwords.ToArray(), 2)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Progressive Sword x3", IsFake = true, IsTrick = false,
                Conditionals = LogicEditor.CreatePermiations(allSwords.ToArray(), 3)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Progressive Sword x4", IsFake = true, IsTrick = false,
                Conditionals = LogicEditor.CreatePermiations(allSwords.ToArray(), 4)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Progressive Sword x5", IsFake = true, IsTrick = false,
                Conditionals = LogicEditor.CreatePermiations(allSwords.ToArray(), 5)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Progressive Sword x6", IsFake = true, IsTrick = false,
                Conditionals = LogicEditor.CreatePermiations(allSwords.ToArray(), 6)
            });

        }

        public static bool IsGoddessCubeAccessEntry(string Name)
        {
            return Name.StartsWith("Goddess Cube ")|| Name == "Initial Goddess Cube";
        }

        public static void ParseHelpers(List<SSLocation> SSData, string[] MacroDataLines)
        {

            SSLocation CurrentEntry = new SSLocation { DictionaryName = "", isFake = true };
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
                    MakeGodesscubesTrackable(CurrentEntry);
                    CurrentEntry.Logic = Logic.Trim().Replace("?","");
                    SSData.Add(CurrentEntry);
                    CurrentEntry = new SSLocation { DictionaryName = "", isFake = true };
                    Logic = "";
                }
            }

            void MakeGodesscubesTrackable(SSLocation entry)
            {
                if (!IsGoddessCubeAccessEntry(entry.DictionaryName)) { return; }
                entry.isFake = false;
                entry.LocationArea = "Goddess Cube tracking";
                entry.ItemSubType = entry.DictionaryName;
                entry.LocationName = entry.DictionaryName;
                entry.ItemName = entry.DictionaryName;
                entry.SpoilerItem = entry.DictionaryName;
                entry.SpoilerLocation = entry.DictionaryName;
            }

            void MakeDungeonEntranceRealLocation(SSLocation Entry)
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

        public static List<LogicObjects.LogicDictionaryEntry> CreateSSDictionary(List<SSLocation> SSData)
        {
            List<LogicObjects.LogicDictionaryEntry> ssDictionary = new List<LogicObjects.LogicDictionaryEntry>();
            foreach(var i in SSData.Where(x=>!x.isFake))
            {
                ssDictionary.Add(new LogicObjects.LogicDictionaryEntry { DictionaryName = i.DictionaryName, ItemName = i.ItemName, ItemSubType = i.ItemSubType, LocationArea = i.LocationArea, LocationName = i.LocationName, SpoilerItem = i.SpoilerItem, SpoilerLocation = i.SpoilerLocation });
            }

            return ssDictionary;
        }

        public static string[] CreateSSLogic(List<SSLocation> SSData)
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

        static void CreateCustomData(List<SSLocation> SSData)
        {
            //Add extra real item entries

            //The emerald tablet does not have a location associated with it and is not in the data files, so add the item as an item only entry.
            SSData.Add(new SSLocation { DictionaryName = "Item Emerald Tablet", isFake = false, LocationName = "", ItemName = "Emerald Tablet", ItemSubType = "Item", SpoilerItem = "Emerald Tablet" });

            //Add "Shuffle Tablets" setting entrys
            SSData.Add(new SSLocation { DictionaryName = "SettingSuffleTablet", isFake = false, LocationName = "Shuffled Tablets", ItemName = "", ItemSubType = "SettingSuffleTablet", LocationArea = "%Settings%", SpoilerLocation = "SettingSuffleTablet", SpoilerItem = "SettingSuffleTablet" });
            SSData.Add(new SSLocation { DictionaryName = "SettingSuffleTabletTrue", isFake = false, LocationName = "", ItemName = "Enabled", ItemSubType = "SettingSuffleTablet", LocationArea = "%Settings%", SpoilerLocation = "SettingSuffleTabletTrue", SpoilerItem = "SettingSuffleTabletTrue" });
            SSData.Add(new SSLocation { DictionaryName = "SettingSuffleTabletFalse", isFake = false, LocationName = "", ItemName = "Disabled", ItemSubType = "SettingSuffleTablet", LocationArea = "%Settings%", SpoilerLocation = "SettingSuffleTabletFalse", SpoilerItem = "SettingSuffleTabletFalse" });

            //Add "Swordless" setting entrys
            SSData.Add(new SSLocation { DictionaryName = "SettingStartingSword", isFake = false, LocationName = "Swordless", ItemName = "", ItemSubType = "SettingStartingSword", LocationArea = "%Settings%", SpoilerLocation = "SettingStartingSword", SpoilerItem = "SettingStartingSword" });
            SSData.Add(new SSLocation { DictionaryName = "SettingStartingSwordTrue", isFake = false, LocationName = "", ItemName = "Disabled", ItemSubType = "SettingStartingSword", LocationArea = "%Settings%", SpoilerLocation = "SettingStartingSwordTrue", SpoilerItem = "SettingStartingSwordTrue" });
            SSData.Add(new SSLocation { DictionaryName = "SettingStartingSwordFalse", isFake = false, LocationName = "", ItemName = "Enabled", ItemSubType = "SettingStartingSword", LocationArea = "%Settings%", SpoilerLocation = "SettingStartingSwordFalse", SpoilerItem = "SettingStartingSwordFalse" });

            //Add "Closed Thunderhead" setting entrys
            SSData.Add(new SSLocation { DictionaryName = "SettingClosedThunderHead", isFake = false, LocationName = "Closed Thunderhead", ItemName = "", ItemSubType = "SettingClosedThunderHead", LocationArea = "%Settings%", SpoilerLocation = "SettingClosedThunderHead", SpoilerItem = "SettingClosedThunderHead" });
            SSData.Add(new SSLocation { DictionaryName = "Option \"closed-thunderhead\" Enabled", isFake = false, LocationName = "", ItemName = "Enabled", ItemSubType = "SettingClosedThunderHead", LocationArea = "%Settings%", SpoilerLocation = "SettingClosedThunderHeadTrue", SpoilerItem = "SettingClosedThunderHeadTrue" });
            SSData.Add(new SSLocation { DictionaryName = "Option \"closed-thunderhead\" Disabled", isFake = false, LocationName = "", ItemName = "Disabled", ItemSubType = "SettingClosedThunderHead", LocationArea = "%Settings%", SpoilerLocation = "SettingClosedThunderHeadFalse", SpoilerItem = "SettingClosedThunderHeadFalse" });

            //Add some extra "Rare Treasure" items to fill the pool
            for (var i = 0; i < 10; i++) 
            { 
                SSData.Add(new SSLocation { DictionaryName = $"ExtraTreasure{i}", isFake = false, LocationName = "", ItemName = "Rare Treasure", ItemSubType = "Item", LocationArea = "", SpoilerItem = "Rare Treasure", SpoilerLocation = $"ExtraTreasure{i}" }); 
            }

            //Not sure why these weren't in the item data but whatever, add them as item only entries so the spoiler log is happy
            SSData.Add(new SSLocation { DictionaryName = $"ExtraHP1", isFake = false, LocationName = "", ItemName = "Heart Piece", ItemSubType = "Item", LocationArea = "", SpoilerItem = "Heart Piece", SpoilerLocation = $"ExtraHP1" });
            SSData.Add(new SSLocation { DictionaryName = $"ExtraHP2", isFake = false, LocationName = "", ItemName = "Heart Piece", ItemSubType = "Item", LocationArea = "", SpoilerItem = "Heart Piece", SpoilerLocation = $"ExtraHP2" });
            SSData.Add(new SSLocation { DictionaryName = $"ExtraBottle1", isFake = false, LocationName = "", ItemName = "Bottle", ItemSubType = "Item", LocationArea = "", SpoilerItem = "Bottle|Empty Bottle", SpoilerLocation = $"ExtraBottle1" });

            //Add "Required Dungeons" settings
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeon1", isFake = false, LocationName = "Required Dungeon 1", ItemName = "", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "RequiredDungeon1", SpoilerLocation = $"RequiredDungeon1" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeon2", isFake = false, LocationName = "Required Dungeon 2", ItemName = "", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "RequiredDungeon1", SpoilerLocation = $"RequiredDungeon2" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonSW", isFake = false, LocationName = "", ItemName = "Req Skyview", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Skyview", SpoilerLocation = $"RequiredDungeonSW" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonET", isFake = false, LocationName = "", ItemName = "Req Earth Temple", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Earth Temple", SpoilerLocation = $"RequiredDungeonET" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonMF", isFake = false, LocationName = "", ItemName = "Req Lanayru Mining Facility", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Lanayru Mining Facility", SpoilerLocation = $"RequiredDungeonMF" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonAC", isFake = false, LocationName = "", ItemName = "Req Ancient Cistern", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Ancient Cistern", SpoilerLocation = $"RequiredDungeonAC" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonSS", isFake = false, LocationName = "", ItemName = "Req Sandship", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Sandship", SpoilerLocation = $"RequiredDungeonSS" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonFS", isFake = false, LocationName = "", ItemName = "Req Fire Sanctuary", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Fire Sanctuary", SpoilerLocation = $"RequiredDungeonFS" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonSK", isFake = false, LocationName = "", ItemName = "Req Skykeep", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Skykeep", SpoilerLocation = $"RequiredDungeonSK" });

        }

        static bool isNewEntry(string line, bool Alternate)
        {
            string Tell = (Alternate) ? ":" : "-";
            return (!line.StartsWith(" ") && line.Contains(Tell)) ;
        }

        public static List<SSLocation> ParseData(string[] ItemDataLines, string[] MacroDataLines)
        {
            List<SSLocation> SSDictionary = new List<SSLocation>();

            SSLocation CurrentEntry = new SSLocation { DictionaryName = "" };

            bool AtLogic = false;
            string LogicString = "";

            foreach (var i in ItemDataLines)
            {
                var line = "";

                line = i;

                if (line.Trim().StartsWith("#")) { continue; }
                if (line.Contains("#")) { line = line.Substring(0, line.IndexOf("#")); }


                if (isNewEntry(line, false))
                {
                    CreateNewEntry();
                    CurrentEntry.DictionaryName = line.Replace(":", "").Trim();

                    bool Unimplimented = false;
                    if (CurrentEntry.DictionaryName.EndsWith("+"))
                    {
                        CurrentEntry.DictionaryName = CurrentEntry.DictionaryName.Replace("+", "");
                        Unimplimented = true;
                    }

                    CurrentEntry.ItemSubType = "Item";
                    var Data = line.Split('-').Select(x => x.Trim()).ToArray();
                    if (Data.Count() > 1)
                    {
                        CurrentEntry.LocationName = (Unimplimented) ? "" : line.Replace(":", "").Trim();
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
                    AtLogic = true;
                }
                else if (line.Contains("original item:"))
                {
                    AtLogic = false;
                    CurrentEntry.Logic = LogicString;
                    LogicString = "";

                    CurrentEntry.ItemName = Utility.GetTextAfter(line, "original item:").Trim();
                    CurrentEntry.SpoilerItem = Utility.GetTextAfter(line, "original item:").Trim();
                    if (CurrentEntry.ItemName == "Gratitude Crystal") { CurrentEntry.LocationArea += " Gratitude Crystal"; CurrentEntry.ItemSubType += " Gratitude Crystal"; }
                }

                if (AtLogic)
                {
                    if (line.Contains("Need:"))
                    {
                        LogicString += Utility.GetTextAfter(line, "Need:").Trim().Replace("?", "");
                    }
                    else
                    {
                        LogicString += line.Trim().Replace("?", "");
                    }
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
                    CurrentEntry = new SSLocation { DictionaryName = "" };
                }
            }

        }

        public static void FixSpoilerNames(SSLocation SSData)
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
                    Title = $"Select SSR Spoiler Log",
                    Filter = "SSR Spoiler Log (*.txt)|*.txt",
                    FilterIndex = 1,
                    Multiselect = false
                };
                if (SelectedFile.ShowDialog() != DialogResult.OK) { return null; }
                filename = SelectedFile.FileName;
                Log = File.ReadAllLines(SelectedFile.FileName);
            }


            bool AtItems = false;
            bool AtEntrances = false;
            bool AtOptions = false;
            bool RandomizeTablets = false;
            bool ClosedThunderhead = false;
            bool Swordless = false;
            bool RandomizeSailcloth = false;
            List<string> SpoilerData = new List<string>();
            SpoilerData.Add("Converted SSR");
            string header = "";
            var FileContent = Log.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            foreach (var line in FileContent)
            {
                if (line.Contains("Options selected:"))
                {
                    AtOptions = true;
                }
                if (line.Contains("Required Dungeon 1:"))
                {
                    AtOptions = false;
                    SpoilerData.Add($"RequiredDungeon1->Required Dungeon " + line.Replace("Required Dungeon 1:", "").Trim());
                }
                if (line.Contains("Required Dungeon 2:"))
                {
                    SpoilerData.Add($"RequiredDungeon2->Required Dungeon " + line.Replace("Required Dungeon 2:", "").Trim());
                }

                if (AtOptions)
                {
                    if (line.Contains("randomize-tablets")) { RandomizeTablets = true; }
                    if (line.Contains("closed-thunderhead")) { ClosedThunderhead = true; }
                    if (line.Contains("swordless")) { Swordless = true; }
                    if (line.Contains("randomize-sailcloth")) { RandomizeSailcloth = true; }
                }


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

            if (!RandomizeTablets) { SpoilerData.Add($"SettingSuffleTablet->SettingSuffleTabletFalse"); }
            else { SpoilerData.Add($"SettingSuffleTablet->SettingSuffleTabletTrue"); }

            if (!Swordless) { SpoilerData.Add($"SettingStartingSword->SettingStartingSwordTrue"); }
            else { SpoilerData.Add($"SettingStartingSword->SettingStartingSwordFalse"); }

            if (ClosedThunderhead) { SpoilerData.Add($"SettingClosedThunderHead->SettingClosedThunderHeadTrue"); }
            else { SpoilerData.Add($"SettingClosedThunderHead->SettingClosedThunderHeadFalse"); }

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
