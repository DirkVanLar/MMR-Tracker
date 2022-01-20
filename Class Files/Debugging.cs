using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MathNet.Numerics;
using MathNet.Symbolics;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.Logging;
using Microsoft.JScript;
using System.Windows.Documents;
using MMR_Tracker.Other_Games;
using MMR_Tracker.Forms.Extra_Functionality;
using static MMR_Tracker.Class_Files.MMR_Code_Reference.items;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using MMR_Tracker.Class_Files.MMR_Code_Reference;
using System.Xml;
using System.Data;

namespace MMR_Tracker_V2
{
    class Debugging
    {
        public static bool ISDebugging = false;
        public static bool ViewAsUserMode = false;

        public static string LogFile = "";

        

        public static void PrintLogicObject(List<LogicObjects.LogicEntry> Logic, int start = -1, int end = -1)
        {
            JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            string LogicPrint = JsonConvert.SerializeObject(Logic, _jsonSerializerOptions);

            Console.WriteLine(LogicPrint);
        }

        public static void PrintLogic(LogicObjects.LogicEntry Entry, int Whattoprint = 0)
        {
            Console.WriteLine($"Logic for {Entry.DictionaryName ?? "Unkown"}");
            if (Whattoprint == 0 || Whattoprint == 1)
            {
                if (Entry.Required == null) { Console.WriteLine("No requirements"); }
                else { Console.WriteLine("Requirements:"); foreach (var i in Entry.Required) { Console.WriteLine(i); } }
            }
            if (Whattoprint == 0 || Whattoprint == 2)
            {
                if (Entry.Conditionals == null) { Console.WriteLine("No Conditionals"); }
                else { Console.WriteLine("Conditionals:"); foreach (var i in Entry.Conditionals) { Console.Write("|"); foreach (var j in i) { Console.Write(j); } } }
            }
            Console.WriteLine($"");
            Console.WriteLine($"End write Logic");
        }

        public static void Log(string Data, int LogLevel = 0)
        {
            //0 = Print to Console, 1 = Print to LogFile, 2 = Print to Both
            if (LogLevel == 0 || LogLevel == 2) { Console.WriteLine(Data); }
            if (LogLevel == 0) { return; }
            if (!Directory.Exists(VersionHandeling.BaseProgramPath + @"Recources\Logs")) { Directory.CreateDirectory(VersionHandeling.BaseProgramPath + @"Recources\Logs"); }
            if (!File.Exists(Debugging.LogFile)) { File.Create(Debugging.LogFile); }
            try
            {
                File.AppendAllText(LogFile, Data + Environment.NewLine);
            }
            catch (Exception e) { Console.WriteLine($"Could not write to log file {e}"); }
        }

        public static void TestDumbStuff()
        {

            //SkywardSwordRando.SkywardSwordTesting(false, true, "https://raw.githubusercontent.com/ssrando/ssrando/master/SS%20Rando%20Logic%20-%20Glitchless%20Requirements.yaml", "Skyward Sword Rando Casual (Beta)");
            //SkywardSwordRando.SkywardSwordTesting(false, false, "https://raw.githubusercontent.com/ssrando/ssrando/master/SS%20Rando%20Logic%20-%20Glitched%20Requirements.yaml", "Skyward Sword Rando Glitched (Beta)");

            //WindWakerRando.TestWWR();
            //OcarinaOfTimeRando.CreateOOTRLogic();

            //OOT3DR.GetSpoilerLog();

            //CheckForLocationsHardRequireingTempItems();

            BackupRestoreChecks();

            void CheckForLocationsHardRequireingTempItems()
            {
                var itemPool = Enum.GetValues(typeof(Item)).Cast<Item>();
                var TestLogic = Utility.CloneTrackerInstance(LogicObjects.MainTrackerInstance);

                var TempItems = new List<int>();

                foreach (var i in TestLogic.Logic)
                {
                    var MMRItems = itemPool.Where(x => i.DictionaryName == x.ToString());
                    if (MMRItems.Any())
                    {
                        var MMRItem = MMRItems.First();

                        if (MMRItem.HasAttribute<Definitions.TemporaryAttribute>())
                        {
                            TempItems.Add(i.ID);
                        }
                    }
                }

                var ChecksRequireingSingleTempItem = new List<int>();
                foreach(var i in TempItems)
                {
                    var copyInstance = Utility.CloneTrackerInstance(LogicObjects.MainTrackerInstance);
                    foreach(var j in copyInstance.Logic)
                    {
                        if (j.ItemSubType == "Dungeon Entrance")
                        {
                            j.SetUnRandomized();
                        }
                        j.Available = false;
                        j.Aquired = !j.IsFake && j.ID != i;
                    }
                    LogicEditing.CalculateItems(copyInstance, true, true, true);
                    var ChecksHardRequireingASingleTempItem = copyInstance.Logic.Where(x => !x.Available && !x.IsFake && !string.IsNullOrWhiteSpace(x.LocationName) && x.AvailableOn > 0);
                    if (ChecksHardRequireingASingleTempItem.Any())
                    {
                        Console.WriteLine($"Checks Hard Requireing {copyInstance.Logic[i].ItemName ?? copyInstance.Logic[i].DictionaryName} ======================================");
                        foreach (var j in ChecksHardRequireingASingleTempItem)
                        {
                            Console.WriteLine($"{j.LocationName ?? j.DictionaryName}");
                            ChecksRequireingSingleTempItem.Add(j.ID);
                        }
                    }
                }
                var copyInstance2 = Utility.CloneTrackerInstance(LogicObjects.MainTrackerInstance);
                foreach (var j in copyInstance2.Logic)
                {
                    if (j.ItemSubType == "Dungeon Entrance")
                    {
                        j.SetUnRandomized();
                    }
                    j.Available = false;
                    j.Aquired = !j.IsFake && !TempItems.Contains(j.ID);
                }
                LogicEditing.CalculateItems(copyInstance2, true, true, true);
                var ChecksRequiringMultipleTempItems = copyInstance2.Logic.Where(x => !x.Available && !x.IsFake && !ChecksRequireingSingleTempItem.Contains(x.ID) && !string.IsNullOrWhiteSpace(x.LocationName) && x.AvailableOn > 0);
                if (ChecksRequiringMultipleTempItems.Any())
                {
                    Console.WriteLine($"Checks Hard Requireing a temp item ======================================");
                    foreach (var j in ChecksRequiringMultipleTempItems)
                    {
                        Console.WriteLine($"{j.LocationName ?? j.DictionaryName}");
                        ChecksRequireingSingleTempItem.Add(j.ID);
                    }
                }



            }

            bool ParseALogicalExpression()
            {
                //A function to parse a logic expression in the form of a string

                //This is an example string with static True/False Values
                string Expression = "T & (T | F | T) & (F | (T & (F | T | ( T & F | F & T))))";
                Console.WriteLine(Expression);

                //Break the expression into an array containing the individual logic items as well as the logic operators
                //We can now loop through each operator and replace it as needed.
                List<string> ExpandedExptression = GetEntries(Expression);

                for (var i = 0; i < ExpandedExptression.Count(); i++)
                {
                    switch (ExpandedExptression[i])
                    {
                        //Replace the Logical operatiors with Math operators. & operators translate to * and | operators translate to +
                        //This will convert the expression to a math problem which can then be solved
                        case "&":
                            ExpandedExptression[i] = "*";
                            break;
                        case "|":
                            ExpandedExptression[i] = "+";
                            break;
                        default:
                            //Convert the items in the logic string to 1s and 0s.
                            //In an actual use case the "PerformLogicCheck" function would determin whether the item meets certain conditions.
                            //If it does meet those conditions it is replaced in the string with a 1 and if not it is replaced with a 0.
                            ExpandedExptression[i] = PerformLogicCheck(ExpandedExptression[i]);
                            break;
                    }
                }
                //Join the array back into a string
                Expression = string.Join("", ExpandedExptression);
                Console.WriteLine(Expression);

                //Solve the newly created equation
                DataTable dt = new DataTable();
                var Solution = dt.Compute(Expression, "");

                //The equation should always return a number, so we can parse it to an int.
                int Result = (int)Solution;
                Console.WriteLine(Result);

                //If the result is 0, availablility is false. If its greater than zero, availablility is true 
                return Result > 0;

                string PerformLogicCheck(string i)
                {
                    //Code to determine if the item meets the necessary criteria will go here
                    if (i == "T")
                    {
                        return "1";
                    }
                    else if (i == "F")
                    {
                        return "0";
                    }
                    return i;
                }

                List<string> GetEntries(string input)
                {
                    List<string> BrokenString = new List<string>();
                    string currentItem = "";
                    foreach (var i in input)
                    {
                        if (char.IsWhiteSpace(i)) { continue; }
                        if (ISLogicChar(i))
                        {
                            if (currentItem != "")
                            {
                                BrokenString.Add(currentItem);
                                currentItem = "";
                            }
                            BrokenString.Add(i.ToString());
                        }
                        else { currentItem += i.ToString(); }
                    }
                    if (currentItem != "") { BrokenString.Add(currentItem); }
                    return BrokenString;
                }

                bool ISLogicChar(char i)
                {
                    switch (i)
                    {
                        case '&': case '|': case '+': case '*': case '(': case ')':
                            return true;
                        default:
                            return false;
                    }

                }
            }

            void BackupRestoreChecks()
            {
                string ClipBoardText = Clipboard.GetText();
                if (ClipBoardText.StartsWith("BackupData:"))
                {
                    int[] Checks = JsonConvert.DeserializeObject<int[]>(Clipboard.GetText().Substring(11));
                    Tools.CheckAllItemsByIDList(Checks);
                    Clipboard.Clear();
                }
                else
                {
                    int[] CheckedLocations = LogicObjects.MainTrackerInstance.Logic.Where(x => x.Checked).Select(x => x.ID).ToArray();
                    Clipboard.SetText($"BackupData:{JsonConvert.SerializeObject(CheckedLocations)}");
                    Console.WriteLine(Clipboard.GetText().Substring(11));
                }
            }


            void testCountEntry()
            {
                LogicObjects.MainTrackerInstance.Logic.Add(new LogicObjects.LogicEntry()
                {
                    ID = LogicObjects.MainTrackerInstance.Logic.Count(),
                    DictionaryName = "MMRTCombinationsDynamic",
                    IsFake = true,
                });

                LogicObjects.MainTrackerInstance.Logic.Add(new LogicObjects.LogicEntry()
                {
                    ID = LogicObjects.MainTrackerInstance.Logic.Count(),
                    DictionaryName = "CombinationCounterTest",
                    LocationName = "Skulltulla Needed",
                    LocationArea = "%Options%",
                    IsFake = false,
                    ItemSubType = "MMRTCountCheck"
                });

                var Skulltulas = LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName == "Swamp Skulltula Spirit");
                var hookshot = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == "ItemHookshot");

                LogicObjects.MainTrackerInstance.Logic.Add(new LogicObjects.LogicEntry()
                {
                    ID = LogicObjects.MainTrackerInstance.Logic.Count(),
                    DictionaryName = "CombinationCounterResult",
                    LocationName = "I WORK",
                    LocationArea = "%Options%",
                    IsFake = false,
                    ItemSubType = "Item",
                    Required = new int[] { LogicObjects.MainTrackerInstance.Logic.Count() - 1, LogicObjects.MainTrackerInstance.Logic.Count() - 2, hookshot.ID },
                    Conditionals = Skulltulas.Select(x => new int[] { x.ID }).ToArray()
                });

                LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
                MainInterface.CurrentProgram.FormatMenuItems();
                MainInterface.CurrentProgram.ResizeObject();
                MainInterface.CurrentProgram.PrintToListBox();
            }

            void SetTestMultiworldData()
            {
                LogicObjects.MainTrackerInstance.Logic[0].Aquired = true;
                LogicObjects.MainTrackerInstance.Logic[0].PlayerData.ItemCameFromPlayer = 9;

                LogicObjects.MainTrackerInstance.Logic[10].Aquired = true;

                LogicObjects.MainTrackerInstance.Logic[13].Checked = true;
                LogicObjects.MainTrackerInstance.Logic[13].RandomizedItem = 0;
                LogicObjects.MainTrackerInstance.Logic[13].PlayerData.ItemBelongedToPlayer = 5;
            }

            void GetAllUniqueCombos()
            {
                Form UniqueData = new Form();
                TextBox Data = new TextBox { Parent = UniqueData, Location = new System.Drawing.Point { X = 2, Y = 2, }, Width = 200 };
                NumericUpDown Combos = new NumericUpDown { Parent = UniqueData, Location = new System.Drawing.Point { X = 2, Y = Data.Height + 4, }, Width = 200 };
                Button ok = new Button { Parent = UniqueData, Location = new System.Drawing.Point { X = 2, Y = Combos.Location.Y + Combos.Height + 2, }, Text = "Copy", Width = 200 };

                ok.Click += Ok_Click;

                UniqueData.Controls.Add(Data);
                UniqueData.Controls.Add(Combos);
                UniqueData.Controls.Add(ok);

                UniqueData.Width = 220;
                UniqueData.Height = 110;

                UniqueData.Show();

                void Ok_Click(object sender, EventArgs e)
                {
                    var Line = Data.Text;
                    var num = (int)Combos.Value;
                    string Output = "";
                    bool drawcolon = false;
                    foreach (var i in GetPermutations(Line.Split(';'), num))
                    {
                        if (drawcolon) { Output += ";"; }
                        else { drawcolon = true; }
                        bool drawcomma = false;
                        foreach (var j in i)
                        {
                            if (drawcomma)
                            {
                                Output += ("," + j);
                            }
                            else
                            {
                                Output += j;
                                drawcomma = true;
                            }
                        }
                    }
                    Clipboard.SetText(Output);
                    IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> items, int count)
                    {
                        int i = 0;
                        foreach (var item in items)
                        {
                            if (count == 1)
                                yield return new T[] { item };
                            else
                            {
                                foreach (var result in GetPermutations(items.Skip(i + 1), count - 1))
                                    yield return new T[] { item }.Concat(result);
                            }
                            ++i;
                        }
                    }
                }
            }
        }

        public static void GetAllLocations()
        {
            foreach (var i in LogicObjects.MainTrackerInstance.Logic.Select(x => x.LocationArea).Distinct().OrderBy(x => x))
            {
                Console.WriteLine(i);
            }
        }

        public static void Get115IDs()
        {
            var file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSET)|*.txt;*.MMRTSET");
            if (file == "") { return; }
            bool SettingsFile = file.EndsWith(".MMRTSET");
            var Lines = (SettingsFile) ? File.ReadAllLines(file).Skip(2).ToArray() : File.ReadAllLines(file).ToArray();
            LogicObjects.LogicFile NewformatLogicFile = LogicObjects.LogicFile.FromJson(string.Join("", Lines));
            foreach(var i in NewformatLogicFile.Logic)
            {
                Console.WriteLine(i.Id);
            }
        }

        public static void Fix113EntrandoLogic()
        {
            LogicObjects.TrackerInstance Instance = new LogicObjects.TrackerInstance();
            string file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt)|*.txt");
            Instance.RawLogicFile = File.ReadAllLines(file);
            Instance.LogicDictionary = null;
            LogicEditing.PopulateTrackerInstance(Instance);

            bool PastGossips = false;
            bool BeginPushback = false;

            foreach (var i in Instance.Logic)
            {
                if (BeginPushback && !PastGossips)
                {
                    Instance.Logic[i.ID - 28].Required = i.Required;
                    Instance.Logic[i.ID - 28].Conditionals = i.Conditionals;
                    i.Required = null;
                    i.Conditionals = null;
                }

                if (i.DictionaryName == "GossipTerminaGossipDrums") { PastGossips = true; }
                if (i.DictionaryName == "EntranceGrottoPalaceStraightFromDekuPalaceA") { BeginPushback = true; }
            }

            foreach (var i in Instance.Logic)
            {
                
                if (i.Required != null)
                {
                    for (var r = 0; r < i.Required.Count(); r++)
                    {
                        if (EntryIsEntranceTest(i.Required[r]))
                        {
                            i.Required[r] = i.Required[r] - 28;
                        }
                    }
                }

                if (i.Conditionals != null)
                {
                    foreach (var c in i.Conditionals)
                    {
                        for (var r = 0; r < c.Count(); r++)
                        {
                            if (EntryIsEntranceTest(c[r]))
                            {
                                c[r] = c[r] - 28;
                            }
                        }
                    }
                }


            }

            bool EntryIsEntranceTest(int Entry)
            {
                return Entry > 432 && Entry < 742;
            }

            if (LogicEditor.EditorForm == null)
            {
                LogicEditor.EditorForm = new LogicEditor();
                LogicEditor.EditorForm.Show();
            }
            else
            {
                LogicEditor.EditorForm.Show();
                LogicEditor.EditorForm.Focus();
            }

            LogicEditor.EditorForm.nudIndex.Value = 0;
            LogicEditor.EditorInstance = Instance;
            LogicEditor.EditorForm.FormatForm();
            LogicEditor.EditorForm.CreateContextMenus();

        }

        public static bool BackupPopulatePre115TrackerInstance(LogicObjects.TrackerInstance instance)
        {
            /* Sets the Values of the follwing using the data in instance.RawLogicFile
             * Version
             * Game
             * Entrance Area Dictionary
             * Logic
             * LogicDictionary
             * Name to ID Dictionary
             * Entrance pair Dictionary
             */
            instance.Logic.Clear();
            instance.DicNameToID.Clear();
            instance.EntrancePairs.Clear();
            LogicObjects.VersionInfo versionData = VersionHandeling.GetVersionDataFromLogicFile(instance.RawLogicFile);
            instance.LogicVersion = versionData.Version;
            instance.GameCode = versionData.Gamecode;
            int SubCounter = 0;
            int idCounter = 0;
            LogicObjects.LogicDictionary MasterDic = null;

            if (instance.LogicDictionary == null || instance.LogicDictionary.LogicDictionaryList.Count < 1)
            {
                string DictionaryPath = VersionHandeling.GetDictionaryPath(instance);
                if (!string.IsNullOrWhiteSpace(DictionaryPath))
                {
                    try
                    {

                        MasterDic = JsonConvert.DeserializeObject<LogicObjects.LogicDictionary>(File.ReadAllText(DictionaryPath));
                        instance.LogicDictionary = MasterDic;
                    }
                    catch { MessageBox.Show($"The Dictionary File \"{DictionaryPath}\" has been corrupted. The tracker will not function correctly."); }
                }
                else { MessageBox.Show($"A valid dictionary file could not be found for this logic. The tracker will not function correctly."); }
            }

            LogicObjects.LogicEntry LogicEntry1 = new LogicObjects.LogicEntry();
            var NextLine = 1;
            foreach (string line in instance.RawLogicFile)
            {
                if (NextLine == 1) { NextLine++; continue; }
                if (line.StartsWith("-")) { SubCounter = 0; }
                switch (SubCounter)
                {
                    case 0:
                        LogicEntry1.ID = idCounter;
                        LogicEntry1.DictionaryName = line.Substring(2);
                        LogicEntry1.Checked = false;
                        LogicEntry1.RandomizedItem = -2;
                        LogicEntry1.IsFake = true;
                        LogicEntry1.SpoilerRandom = -2;

                        var DicEntry = instance.LogicDictionary.LogicDictionaryList.Find(x => x.DictionaryName == LogicEntry1.DictionaryName);
                        if (DicEntry == null) { break; }

                        LogicEntry1.IsFake = false;
                        LogicEntry1.IsTrick = false;
                        LogicEntry1.TrickEnabled = true;
                        LogicEntry1.TrickToolTip = "";
                        LogicEntry1.ItemName = (string.IsNullOrWhiteSpace(DicEntry.ItemName)) ? null : DicEntry.ItemName;
                        LogicEntry1.LocationName = (string.IsNullOrWhiteSpace(DicEntry.LocationName)) ? null : DicEntry.LocationName;
                        LogicEntry1.LocationArea = (string.IsNullOrWhiteSpace(DicEntry.LocationArea)) ? "Misc" : DicEntry.LocationArea;
                        LogicEntry1.ItemSubType = (string.IsNullOrWhiteSpace(DicEntry.ItemSubType)) ? "Item" : DicEntry.ItemSubType;
                        LogicEntry1.SpoilerLocation = DicEntry.SpoilerLocation.ToList();
                        LogicEntry1.SpoilerItem = DicEntry.SpoilerItem.ToList();
                        break;
                    case 1:
                        if (string.IsNullOrWhiteSpace(line)) { LogicEntry1.Required = null; break; }
                        LogicEntry1.Required = line.Split(',').Select(y => int.Parse(y)).ToArray();
                        break;
                    case 2:
                        if (string.IsNullOrWhiteSpace(line)) { LogicEntry1.Conditionals = null; break; }
                        LogicEntry1.Conditionals = line.Split(';').Select(x => x.Split(',').Select(y => int.Parse(y)).ToArray()).ToArray();
                        break;
                    case 3:
                        LogicEntry1.NeededBy = System.Convert.ToInt32(line);
                        break;
                    case 4:
                        LogicEntry1.AvailableOn = System.Convert.ToInt32(line);
                        break;
                    case 5:
                        LogicEntry1.IsTrick = (line.StartsWith(";"));
                        LogicEntry1.TrickEnabled = true;
                        LogicEntry1.TrickToolTip = (line.Length > 1) ? line.Substring(1) : "No Tooltip Available";
                        //if (LogicEntry1.IsTrick) { Debugging.Log($"Trick {LogicEntry1.DictionaryName} Found. ToolTip =  { LogicEntry1.TrickToolTip }"); }
                        break;
                }
                if ((NextLine) >= instance.RawLogicFile.Count() || instance.RawLogicFile[NextLine].StartsWith("-"))
                {
                    //Push Data to the instance
                    instance.Logic.Add(LogicEntry1);
                    LogicEntry1 = new LogicObjects.LogicEntry();
                    idCounter++;
                }
                NextLine++;
                SubCounter++;
            }

            instance.EntranceRando = instance.IsEntranceRando();
            instance.EntranceAreaDic = BackupCreateAreaClearDictionary(instance);
            instance.CreateDicNameToID();
            Utility.nullEmptyLogicItems(instance.Logic);

            return true;
        }

        public static Dictionary<int, int> BackupCreateAreaClearDictionary(LogicObjects.TrackerInstance Instance)
        {
            var EntAreaDict = new Dictionary<int, int>();

            if (!Instance.IsMM()) { return EntAreaDict; }

            var WoodfallClear = Instance.Logic.Find(x => x.DictionaryName == "Woodfall clear" || x.DictionaryName == "AreaWoodFallTempleClear");
            var WoodfallAccess = Instance.Logic.Find(x => (x.DictionaryName == "Woodfall Temple access" || x.DictionaryName == "AreaWoodFallTempleAccess") && !x.IsFake);
            if (WoodfallAccess == null || WoodfallClear == null)
            {
                Console.WriteLine($"Coul not find Woodfall Data. Access found {WoodfallAccess != null}. Clear found {WoodfallClear != null}.");
                return new Dictionary<int, int>();
            }
            EntAreaDict.Add(WoodfallClear.ID, WoodfallAccess.ID);

            var SnowheadClear = Instance.Logic.Find(x => x.DictionaryName == "Snowhead clear" || x.DictionaryName == "AreaSnowheadTempleClear");
            var SnowheadAccess = Instance.Logic.Find(x => (x.DictionaryName == "Snowhead Temple access" || x.DictionaryName == "AreaSnowheadTempleAccess") && !x.IsFake);
            if (SnowheadAccess == null || SnowheadClear == null)
            {
                Console.WriteLine($"Coul not find Snowhead Data. Access found {SnowheadAccess != null} Clear {SnowheadClear != null}");
                return new Dictionary<int, int>();
            }
            EntAreaDict.Add(SnowheadClear.ID, SnowheadAccess.ID);

            var GreatBayClear = Instance.Logic.Find(x => x.DictionaryName == "Great Bay clear" || x.DictionaryName == "AreaGreatBayTempleClear");
            var GreatBayAccess = Instance.Logic.Find(x => (x.DictionaryName == "Great Bay Temple access" || x.DictionaryName == "AreaGreatBayTempleAccess") && !x.IsFake);
            if (GreatBayAccess == null || GreatBayClear == null)
            {
                Console.WriteLine($"Coul not find Great Bay Data. Access found {GreatBayAccess != null} Clear {GreatBayClear != null}");
                return new Dictionary<int, int>();
            }
            EntAreaDict.Add(GreatBayClear.ID, GreatBayAccess.ID);

            var StoneTowerClear = Instance.Logic.Find(x => x.DictionaryName == "Ikana clear" || x.DictionaryName == "AreaStoneTowerClear");
            var StoneTowerAccess = Instance.Logic.Find(x => (x.DictionaryName == "Inverted Stone Tower Temple access" || x.DictionaryName == "AreaInvertedStoneTowerTempleAccess") && !x.IsFake);
            if (StoneTowerAccess == null || StoneTowerClear == null)
            {
                Console.WriteLine($"Coul not find Ikana Data. Access found {StoneTowerAccess != null} Clear {StoneTowerClear != null}");
                return new Dictionary<int, int>();
            }
            EntAreaDict.Add(StoneTowerClear.ID, StoneTowerAccess.ID);

            return EntAreaDict;
        }

        public static void BackupLoadLogic()
        {
            var file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSET)|*.txt;*.MMRTSET");
            if (file == "") { return; }
            bool SettingsFile = file.EndsWith(".MMRTSET");
            var Lines = (SettingsFile) ? File.ReadAllLines(file).Skip(2).ToArray() : File.ReadAllLines(file).ToArray();

            LogicObjects.MainTrackerInstance = new LogicObjects.TrackerInstance();

            LogicObjects.MainTrackerInstance.LogicFormat = "txt";

            LogicObjects.MainTrackerInstance.RawLogicFile = Lines;
            BackupPopulatePre115TrackerInstance(LogicObjects.MainTrackerInstance);
        }


    }
}
