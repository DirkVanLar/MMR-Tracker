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
            System.Net.WebClient wc = new System.Net.WebClient();
            string ItemData = wc.DownloadString("https://raw.githubusercontent.com/lepelog/sslib/master/SS%20Rando%20Logic%20-%20Item%20Location.yaml");
            string[] ItemDataLines = ItemData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            string MacroData = wc.DownloadString("https://raw.githubusercontent.com/lepelog/sslib/master/SS%20Rando%20Logic%20-%20Macros.yaml");
            string[] MacroDataLines = MacroData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

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
                Filter = "",
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
                    if (EntriesInLogic.Contains(LogicName) && !SSInstance.Logic.Any(x=>x.DictionaryName == LogicName))
                    {
                        Console.WriteLine($"{LogicName} was created");
                        SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = LogicName, IsFake = true, IsTrick = false, 
                            Conditionals = LogicEditor.CreatePermiations(ItemsWithThisName.Select(x => x.ID).ToArray(), j) });
                    }
                }

                //Create Entries for items that are reffered to in logic, add all available of this item as conditionals
                if (EntriesInLogic.Contains(i.ItemName) && !SSInstance.Logic.Any(x => x.DictionaryName == i.ItemName) && i.DictionaryName != i.ItemName)
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

        public static string FixMisspellings(string Logic, string Dic)
        {
            string NewLogic = Logic.Replace("Clashots", "Clawshots");
            NewLogic = NewLogic.Replace("Baby Rattle", "Baby's Rattle");
            NewLogic = NewLogic.Replace("?", "");

            return NewLogic;
        }

        public static void AddAdditionalEntries(LogicObjects.TrackerInstance SSInstance)
        {
            //Add extra fake item entries and adjust logic

            //For some reason any bottle is refered to as "Empty bottle". This is probaly a mistake but we'll add the entry anyway
            List<int[]> Bottles = new List<int[]>();
            foreach (var i in SSInstance.Logic.Where(x => x.ItemName == "Bottle"))
            {
                Bottles.Add(new int[] { i.ID });
            }
            int[][] BottlesCond = Bottles.ToArray();

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Empty Bottle", IsFake = true, IsTrick = false, Conditionals = BottlesCond });

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

            //Add MMRTGAmeClear entry for the playthrough generator
            SSInstance.Logic.Add(new LogicObjects.LogicEntry  { ID = SSInstance.Logic.Count(), DictionaryName = "MMRTGameClear", IsFake = true, IsTrick = false, 
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Reach and Defeat Demise").ID } 
            });

            //Create Sword Entries. It's a little wierd since starting with a sword technically give you two upgrades, so better to do it manually

            var allSwords = SSInstance.Logic.Where(x => x.ItemName == "Progressive Sword").Select(x=>x.ID).ToList();
            allSwords.Add(SSInstance.Logic.Count());
            allSwords.Add(SSInstance.Logic.Count());

            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "Option \"Swordless\" Disabled",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "SettingStartingSwordTrue").ID }
            });

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

            //Addentries for settings in logic
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "Option \"skip-skykeep\" Enabled",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "SettingSkipSkykeepTrue").ID }
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "Option \"closed-thunderhead\" Disabled",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "SettingClosedThunderHeadFalse").ID }
            });

            CreateRequiredDungeonLogic(SSInstance);

        }

        public static void CreateRequiredDungeonLogic(LogicObjects.TrackerInstance SSInstance)
        {

            //Add Required Dungeon Logic
            var RequiredDungeonNoneEntries = SSInstance.Logic.Where(x => x.ItemName == "None" && x.ItemSubType == "SettingRequiredDungeon");
            var RequiredDungeonNoneEntriesIDS = RequiredDungeonNoneEntries.Select(x => x.ID).ToArray();

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "0DungeonsRequired", IsFake = true, IsTrick = false, Required = RequiredDungeonNoneEntriesIDS });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "1DungeonsRequired",
                IsFake = true,
                IsTrick = false,
                Conditionals = LogicEditor.CreatePermiations(RequiredDungeonNoneEntriesIDS, 5)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "2DungeonsRequired",
                IsFake = true,
                IsTrick = false,
                Conditionals = LogicEditor.CreatePermiations(RequiredDungeonNoneEntriesIDS, 4)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "3DungeonsRequired",
                IsFake = true,
                IsTrick = false,
                Conditionals = LogicEditor.CreatePermiations(RequiredDungeonNoneEntriesIDS, 3)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "4DungeonsRequired",
                IsFake = true,
                IsTrick = false,
                Conditionals = LogicEditor.CreatePermiations(RequiredDungeonNoneEntriesIDS, 2)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "5DungeonsRequired",
                IsFake = true,
                IsTrick = false,
                Conditionals = RequiredDungeonNoneEntriesIDS.Select(x => new int[] { x }).ToArray()
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "6DungeonsRequired",
                IsFake = true,
                IsTrick = false
            });

            var AquiredAndBeatableList = new List<int>();

            AquiredAndBeatableList.Add(SSInstance.Logic.Count());
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "SVRequiredAndCompletable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Skyview").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonSW").ID }
            });
            AquiredAndBeatableList.Add(SSInstance.Logic.Count());
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "ETRequiredAndCompletable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Earth Temple").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonET").ID }
            });
            AquiredAndBeatableList.Add(SSInstance.Logic.Count());
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "MFRequiredAndCompletable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Lanayru Mining Facility").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonMF").ID }
            });
            AquiredAndBeatableList.Add(SSInstance.Logic.Count());
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "ACRequiredAndCompletable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Ancient Cistern").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonAC").ID }
            });
            AquiredAndBeatableList.Add(SSInstance.Logic.Count());
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "SSRequiredAndCompletable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Sandship").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonSS").ID }
            });
            AquiredAndBeatableList.Add(SSInstance.Logic.Count());
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "FSRequiredAndCompletable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "Can Beat Fire Sanctuary").ID, SSInstance.Logic.Find(x => x.DictionaryName == "RequiredDungeonFS").ID }
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "0DungeonsRequiredAndBeatable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "0DungeonsRequired").ID }
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "1DungeonsRequiredAndBeatable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "1DungeonsRequired").ID },
                Conditionals = LogicEditor.CreatePermiations(AquiredAndBeatableList.ToArray(), 1)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "2DungeonsRequiredAndBeatable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "2DungeonsRequired").ID },
                Conditionals = LogicEditor.CreatePermiations(AquiredAndBeatableList.ToArray(), 2)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "3DungeonsRequiredAndBeatable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "3DungeonsRequired").ID },
                Conditionals = LogicEditor.CreatePermiations(AquiredAndBeatableList.ToArray(), 3)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "4DungeonsRequiredAndBeatable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "4DungeonsRequired").ID },
                Conditionals = LogicEditor.CreatePermiations(AquiredAndBeatableList.ToArray(), 4)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "5DungeonsRequiredAndBeatable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "5DungeonsRequired").ID },
                Conditionals = LogicEditor.CreatePermiations(AquiredAndBeatableList.ToArray(), 5)
            });
            SSInstance.Logic.Add(new LogicObjects.LogicEntry
            {
                ID = SSInstance.Logic.Count(),
                DictionaryName = "6DungeonsRequiredAndBeatable",
                IsFake = true,
                IsTrick = false,
                Required = new int[] { SSInstance.Logic.Find(x => x.DictionaryName == "6DungeonsRequired").ID },
                Conditionals = LogicEditor.CreatePermiations(AquiredAndBeatableList.ToArray(), 6)
            });

            SSInstance.Logic.Add(new LogicObjects.LogicEntry { ID = SSInstance.Logic.Count(), DictionaryName = "Can Access Past", IsFake = true, IsTrick = false });
            var CanAccessPast = SSInstance.Logic[SSInstance.Logic.Count() - 1];

            CanAccessPast.Required = new int[]
            {
                SSInstance.Logic.Find(x => x.DictionaryName == "Master Sword").ID,
                SSInstance.Logic.Find(x => x.DictionaryName == "Can Access Sealed Temple").ID
            };

            CanAccessPast.Conditionals = new int[][]
            {
                new int[]{ SSInstance.Logic.Find(x => x.DictionaryName == "0DungeonsRequiredAndBeatable").ID },
                new int[]{ SSInstance.Logic.Find(x => x.DictionaryName == "1DungeonsRequiredAndBeatable").ID },
                new int[]{ SSInstance.Logic.Find(x => x.DictionaryName == "2DungeonsRequiredAndBeatable").ID },
                new int[]{ SSInstance.Logic.Find(x => x.DictionaryName == "3DungeonsRequiredAndBeatable").ID },
                new int[]{ SSInstance.Logic.Find(x => x.DictionaryName == "4DungeonsRequiredAndBeatable").ID },
                new int[]{ SSInstance.Logic.Find(x => x.DictionaryName == "5DungeonsRequiredAndBeatable").ID },
                new int[]{ SSInstance.Logic.Find(x => x.DictionaryName == "6DungeonsRequiredAndBeatable").ID }
            };
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
                    CurrentEntry.Logic = FixMisspellings(Logic.Trim(), CurrentEntry.DictionaryName) ;
                    MakeGodesscubesTrackable(CurrentEntry);
                    SSData.Add(CurrentEntry);
                    CurrentEntry = new SSLocation { DictionaryName = "", isFake = true };
                    Logic = "";
                }
            }

            void MakeGodesscubesTrackable(SSLocation entry)
            {
                if (!IsGoddessCubeAccessEntry(entry.DictionaryName)) { return; }
                entry.isFake = false;
                Console.WriteLine(entry.Logic.ToLower());
                entry.LocationArea = "Goddess Cubes";
                entry.LocationName = entry.DictionaryName;
                if (entry.Logic.ToLower().Contains("woods")) { entry.LocationName = "Faron Woods - " + entry.DictionaryName; }
                if (entry.Logic.ToLower().Contains("skyview")) { entry.LocationName = "Skyview Spring - " + entry.DictionaryName; }
                if (entry.Logic.ToLower().Contains("floria")) { entry.LocationName = "Lake Floria - " + entry.DictionaryName; }
                if (entry.Logic.ToLower().Contains("eldin")) { entry.LocationName = "Eldin Volcano - " + entry.DictionaryName; }
                if (entry.Logic.ToLower().Contains("summit")) { entry.LocationName = "Volcano Summit - " + entry.DictionaryName; }
                if (entry.Logic.ToLower().Contains("lanayru")) { entry.LocationName = "Lanayru - " + entry.DictionaryName; }
                if (entry.Logic.ToLower().Contains("sea")) { entry.LocationName = "Lanayru Sand Sea - " + entry.DictionaryName; }
                if (entry.Logic.ToLower().Contains("gorge")) { entry.LocationName = "Lanayru Gorge - " + entry.DictionaryName; }
                entry.ItemSubType = entry.DictionaryName;
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

            //Create items that either don't exist is the current Item List

            Dictionary<string, string> MissingItems = new Dictionary<string, string>
            {
                //These entries are not actual items but flags in the vanilla game so they don't have a check listed in the item list.
                { "Skyloft - Emerald Tablet", "Emerald Tablet" },
                { "Skyloft - Spiral Charge", "Spiral Charge" },
                //The original check for the following items are currently not reachable and are commented out in the item list. 
                //The items however are either still in the pool or used in logic, so they need to be added.
                { "Flooded Faron Woods - Faron Soth Part event", "Faron Song of the Hero Part" },
                { "Fire Dragon Room - Eldin Soth part event", "Eldin Song of the Hero Part" },
                { "Lanayru Gorge - Lanayru Soth Part event", "Lanayru Song of the Hero Part" },
                { "Lanayru Gorge - Life Tree Seedling", "Life Tree Seedling" },
                { "Lanayru Gorge - Boss rush hylian shield", "Hylian Shield" }, 
                { "Lanayru Gorge - Boss rush heart piece", "Heart Piece" }, 
                //The "Goddess Cube in Lanayru Gorge" that unlocks this currently can't be reached.
                { "Thunderhead - Mogma Mitts Island Quiver", "Small Quiver" },
                //Beetles heart piece was added to the randomization pool despite it being still available at his shop.
                { "Skyloft - Beetle Shop Heart Piece", "Heart Piece" },
                //Extra junk that I assume is just to fill the item pool.
                { "Extra Treasure 1", "Rare Treasure" }, 
                { "Extra Treasure 2", "Rare Treasure" },
                { "Extra Treasure 3", "Rare Treasure" },
                { "Extra Treasure 4", "Rare Treasure" },
                { "Extra Treasure 5", "Rare Treasure" },
                { "Extra Treasure 6", "Rare Treasure" },
                { "Extra Treasure 7", "Rare Treasure" },
                { "Extra Treasure 8", "Rare Treasure" }
            };
            foreach (var i in MissingItems)
            {
                SSData.Add(new SSLocation { DictionaryName = $"{i.Key}", isFake = false, LocationName = "", ItemName = i.Value, ItemSubType = "Item", SpoilerItem = i.Value });
            }

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
            SSData.Add(new SSLocation { DictionaryName = "SettingClosedThunderHeadTrue", isFake = false, LocationName = "", ItemName = "Enabled", ItemSubType = "SettingClosedThunderHead", LocationArea = "%Settings%", SpoilerLocation = "SettingClosedThunderHeadTrue", SpoilerItem = "SettingClosedThunderHeadTrue" });
            SSData.Add(new SSLocation { DictionaryName = "SettingClosedThunderHeadFalse", isFake = false, LocationName = "", ItemName = "Disabled", ItemSubType = "SettingClosedThunderHead", LocationArea = "%Settings%", SpoilerLocation = "SettingClosedThunderHeadFalse", SpoilerItem = "SettingClosedThunderHeadFalse" });

            //Add "Skip Skykeep" setting entrys
            SSData.Add(new SSLocation { DictionaryName = "SettingSkipSkykeep", isFake = false, LocationName = "Skip Skykeep", ItemName = "", ItemSubType = "SettingSkipSkykeep", LocationArea = "%Settings%", SpoilerLocation = "SettingSkipSkykeep", SpoilerItem = "SettingSkipSkykeep" });
            SSData.Add(new SSLocation { DictionaryName = "SettingSkipSkykeepTrue", isFake = false, LocationName = "", ItemName = "Enabled", ItemSubType = "SettingSkipSkykeep", LocationArea = "%Settings%", SpoilerLocation = "SettingSkipSkykeepTrue", SpoilerItem = "SettingSkipSkykeepTrue" });
            SSData.Add(new SSLocation { DictionaryName = "SettingSkipSkykeepFalse", isFake = false, LocationName = "", ItemName = "Disabled", ItemSubType = "SettingSkipSkykeep", LocationArea = "%Settings%", SpoilerLocation = "SettingSkipSkykeepFalse", SpoilerItem = "SettingSkipSkykeepFalse" });

            //Add "Required Dungeons" settings
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeon1", isFake = false, LocationName = "Required Dungeon 1", ItemName = "", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "RequiredDungeon1", SpoilerLocation = $"RequiredDungeon1" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeon2", isFake = false, LocationName = "Required Dungeon 2", ItemName = "", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "RequiredDungeon2", SpoilerLocation = $"RequiredDungeon2" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeon3", isFake = false, LocationName = "Required Dungeon 3", ItemName = "", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "RequiredDungeon3", SpoilerLocation = $"RequiredDungeon3" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeon4", isFake = false, LocationName = "Required Dungeon 4", ItemName = "", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "RequiredDungeon4", SpoilerLocation = $"RequiredDungeon4" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeon5", isFake = false, LocationName = "Required Dungeon 5", ItemName = "", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "RequiredDungeon5", SpoilerLocation = $"RequiredDungeon5" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeon6", isFake = false, LocationName = "Required Dungeon 6", ItemName = "", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "RequiredDungeon6", SpoilerLocation = $"RequiredDungeon6" });

            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonSW", isFake = false, LocationName = "", ItemName = "Req Skyview", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Skyview", SpoilerLocation = $"RequiredDungeonSW" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonET", isFake = false, LocationName = "", ItemName = "Req Earth Temple", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Earth Temple", SpoilerLocation = $"RequiredDungeonET" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonMF", isFake = false, LocationName = "", ItemName = "Req Lanayru Mining Facility", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Lanayru Mining Facility", SpoilerLocation = $"RequiredDungeonMF" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonAC", isFake = false, LocationName = "", ItemName = "Req Ancient Cistern", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Ancient Cistern", SpoilerLocation = $"RequiredDungeonAC" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonSS", isFake = false, LocationName = "", ItemName = "Req Sandship", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Sandship", SpoilerLocation = $"RequiredDungeonSS" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonFS", isFake = false, LocationName = "", ItemName = "Req Fire Sanctuary", ItemSubType = "SettingRequiredDungeon", LocationArea = "", SpoilerItem = "Required Dungeon Fire Sanctuary", SpoilerLocation = $"RequiredDungeonFS" });

            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonNone1", isFake = false, LocationName = "", ItemName = "None", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "Required Dungeon None1", SpoilerLocation = $"RequiredDungeonNone1" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonNone2", isFake = false, LocationName = "", ItemName = "None", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "Required Dungeon None2", SpoilerLocation = $"RequiredDungeonNone2" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonNone3", isFake = false, LocationName = "", ItemName = "None", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "Required Dungeon None3", SpoilerLocation = $"RequiredDungeonNone3" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonNone4", isFake = false, LocationName = "", ItemName = "None", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "Required Dungeon None4", SpoilerLocation = $"RequiredDungeonNone4" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonNone5", isFake = false, LocationName = "", ItemName = "None", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "Required Dungeon None5", SpoilerLocation = $"RequiredDungeonNone5" });
            SSData.Add(new SSLocation { DictionaryName = $"RequiredDungeonNone6", isFake = false, LocationName = "", ItemName = "None", ItemSubType = "SettingRequiredDungeon", LocationArea = "%Required Dungeon%", SpoilerItem = "Required Dungeon None6", SpoilerLocation = $"RequiredDungeonNone6" });

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
                        CurrentEntry.SpoilerLocation = line.Replace(":", "").Replace("+", "").Trim();
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
                    if (AtLogic)
                    {
                        AtLogic = false;
                        CurrentEntry.Logic = FixMisspellings(LogicString, CurrentEntry.DictionaryName);
                        LogicString = "";
                    }

                    CurrentEntry.ItemName = Utility.GetTextAfter(line, "original item:").Trim();
                    CurrentEntry.SpoilerItem = Utility.GetTextAfter(line, "original item:").Trim();
                    if (CurrentEntry.ItemName == "Gratitude Crystal") 
                    { 
                        CurrentEntry.LocationArea = "Single Gratitude Crystal"; 
                        CurrentEntry.ItemSubType = $"Gratitude Crystal {CurrentEntry.DictionaryName}";
                    }
                    if (CurrentEntry.ItemName == "Revitalizing Potion")
                    {
                        CurrentEntry.ItemName = "Bottle";
                        CurrentEntry.SpoilerItem = "Bottle";
                    }
                }
                else if (line.Contains("type:"))
                {
                    if (AtLogic)
                    {
                        AtLogic = false;
                        CurrentEntry.Logic = FixMisspellings(LogicString, CurrentEntry.DictionaryName);
                        LogicString = "";
                    }
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
            bool SkipSkykeep = false;
            bool Swordless = false;
            bool RandomizeSailcloth = false;
            List<string> SpoilerData = new List<string>();
            SpoilerData.Add("Converted SSR");
            string header = "";
            var FileContent = Log.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            Dictionary<string, string> RequiredDungeons = new Dictionary<string, string>();
            foreach (var line in FileContent)
            {
                if (line.Contains("Options selected:"))
                {
                    AtOptions = true;
                }


                if (AtOptions)
                {
                    if (line.Contains("randomize-tablets")) { RandomizeTablets = true; }
                    if (line.Contains("closed-thunderhead")) { ClosedThunderhead = true; }
                    if (line.Contains("swordless")) { Swordless = true; }
                    if (line.Contains("randomize-sailcloth")) { RandomizeSailcloth = true; }
                    if (line.Contains("skip-skykeep")) { SkipSkykeep = true; }

                    for (var i = 1; i < 7; i++)
                    {
                        if (line.Contains($"Required Dungeon {i}:"))
                        {
                            if (RequiredDungeons.ContainsKey($"RequiredDungeon{i}")) { continue; }
                            RequiredDungeons.Add($"RequiredDungeon{i}", line.Replace($"Required Dungeon {i}:", "").Trim());
                            break;
                        }
                    }
                }


                if (line.Contains("All item locations:"))
                {
                    AtOptions = false;
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
                }
            }

            for (var i = 1; i < 7; i++)
            {
                if (!RequiredDungeons.ContainsKey($"RequiredDungeon{i}"))
                {
                    RequiredDungeons.Add($"RequiredDungeon{i}", $"None{i}");
                }
                SpoilerData.Add($"RequiredDungeon{i}->Required Dungeon " + RequiredDungeons[$"RequiredDungeon{i}"]);
            }

            if (!RandomizeTablets) { SpoilerData.Add($"SettingSuffleTablet->SettingSuffleTabletFalse"); }
            else { SpoilerData.Add($"SettingSuffleTablet->SettingSuffleTabletTrue"); }

            if (!Swordless) { SpoilerData.Add($"SettingStartingSword->SettingStartingSwordTrue"); }
            else { SpoilerData.Add($"SettingStartingSword->SettingStartingSwordFalse"); }

            if (ClosedThunderhead) { SpoilerData.Add($"SettingClosedThunderHead->SettingClosedThunderHeadTrue"); }
            else { SpoilerData.Add($"SettingClosedThunderHead->SettingClosedThunderHeadFalse"); }

            if (SkipSkykeep) { SpoilerData.Add($"SettingSkipSkykeep->SettingSkipSkykeepTrue"); }
            else { SpoilerData.Add($"SettingSkipSkykeep->SettingSkipSkykeepFalse"); }

            //if (RandomizeSailcloth) { SpoilerData.Add($"SettingRandomizeSailcloth->SettingRandomizeSailclothTrue"); }
            //else { SpoilerData.Add($"SettingRandomizeSailcloth->SettingRandomizeSailclothFalse"); }

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
