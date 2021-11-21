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
using MMR_Tracker.Forms.Other_Games;
using MMR_Tracker.Other_Games;
using MMR_Tracker.Forms.Extra_Functionality;
using static MMR_Tracker.Class_Files.MMR_Code_Reference.items;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using MMR_Tracker.Class_Files.MMR_Code_Reference;

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
                Formatting = Formatting.Indented
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
            if (!Directory.Exists(@"Recources\Logs")) { Directory.CreateDirectory(@"Recources\Logs"); }
            if (!File.Exists(Debugging.LogFile)) { File.Create(Debugging.LogFile); }
            try
            {
                File.AppendAllText(LogFile, Data + Environment.NewLine);
            }
            catch (Exception e) { Console.WriteLine($"Could not write to log file {e}"); }
        }

        public static void TestDumbStuff()
        {
            CreateJsonLogicDic();

            void CreateJsonLogicDic()
            {
                LogicObjects.LogicDictionary NewLogicDic = new LogicObjects.LogicDictionary()
                {
                    LogicFormat = LogicObjects.MainTrackerInstance.LogicFormat,
                    LogicVersion = LogicObjects.MainTrackerInstance.LogicVersion,
                    GameCode = LogicObjects.MainTrackerInstance.GameCode,
                    DefaultWalletCapacity = 99,
                    LogicDictionaryList = new List<LogicObjects.LogicDictionaryEntry>()
                };

                var priceData = TempReadPRiceFile();
                var entareadic = LogicObjects.MainTrackerInstance.EntranceAreaDic;

                foreach (var i in LogicObjects.MainTrackerInstance.Logic.Where(x => x.ID < 1143)) 
                {
                    var DicEntry = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == i.DictionaryName);
                    var itemPool = Enum.GetValues(typeof(Item)).Cast<Item>().ToArray();
                    var item = itemPool.FirstOrDefault(x => x.ToString() == i.DictionaryName);
                    var gossipLocations = item.HasAttribute<Definitions.GossipLocationHintAttribute>() ? item.GetAttribute<Definitions.GossipLocationHintAttribute>().Values : null;
                    var GossipItems = item.HasAttribute<Definitions.GossipItemHintAttribute>() ? item.GetAttribute<Definitions.GossipItemHintAttribute>().Values : null;
                    var locationarea = item.HasAttribute<Definitions.RegionNameAttribute>() ? item.GetAttribute<Definitions.RegionNameAttribute>().Name : null;
                    var locationname = item.HasAttribute<Definitions.LocationNameAttribute>() ? item.GetAttribute<Definitions.LocationNameAttribute>().Name : null;
                    var itemname = item.HasAttribute<Definitions.ItemNameAttribute>() ? item.GetAttribute<Definitions.ItemNameAttribute>().Name : null;
                    int? WalletCapacity = null;
                    List<string> BossKeynames = new List<string>() { "ItemWoodfallBossKey", "ItemSnowheadBossKey", "ItemGreatBayBossKey", "ItemStoneTowerBossKey" };
                    List<string> SmallKeynames = new List<string>()
                    { "ItemWoodfallKey1", "ItemGreatBayKey1", "ItemSnowheadKey1", "ItemSnowheadKey2", "ItemSnowheadKey3", 
                        "ItemStoneTowerKey1", "ItemStoneTowerKey2", "ItemStoneTowerKey3", "ItemStoneTowerKey4" };

                    if (!itemPool.Where(x => x.ToString() == i.DictionaryName).Any()) { Console.WriteLine($"{i.DictionaryName} Was not found in MMR files"); }

                    if (i.DictionaryName == "UpgradeAdultWallet") { WalletCapacity = 200; }
                    else if (i.DictionaryName == "UpgradeGiantWallet") { WalletCapacity = 500; }

                    NewLogicDic.LogicDictionaryList.Add(new LogicObjects.LogicDictionaryEntry()
                    {
                        DictionaryName = i.DictionaryName,
                        LocationName = locationname != null ? locationname : i.LocationName,
                        ItemName = itemname != null ? itemname : i.ItemName,
                        LocationArea = locationarea != null ? locationarea : (string.IsNullOrWhiteSpace(i.LocationArea) ? null : i.LocationArea),
                        ItemSubType = (string.IsNullOrWhiteSpace(i.ItemSubType) ? null : i.ItemSubType),
                        FakeItem = DicEntry == null,
                        SpoilerLocation = locationname != null ? locationname : (DicEntry == null ? null : DicEntry.SpoilerLocation),
                        SpoilerItem = itemname != null ? itemname : (DicEntry == null ? null : DicEntry.SpoilerItem),
                        GossipLocation = gossipLocations == null ? null : string.Join("|", gossipLocations),
                        GossipItem = GossipItems == null ? null : string.Join("|", GossipItems),
                        WalletCapacity = WalletCapacity,
                        SpoilerPriceName = priceData.ContainsKey(i.DictionaryName) ? priceData[i.DictionaryName] : null,
                        GameClearDungeonEntrance = entareadic.ContainsKey(i.ID) ? LogicObjects.MainTrackerInstance.Logic[entareadic[i.ID]].DictionaryName : null,
                        EntrancePair = null,
                        KeyType = BossKeynames.Contains(i.DictionaryName) ? "boss" : (SmallKeynames.Contains(i.DictionaryName) ? "small" : null)
                    }) ;
                }

                string FilePath = "";
                SaveFileDialog saveDialog = new SaveFileDialog 
                { 
                    Filter = "MMR Tracker Save (*.json)|*.json", 
                    FilterIndex = 1, 
                    FileName = $"{NewLogicDic.GameCode} V{NewLogicDic.LogicVersion} Logic Dictionary" 
                };
                if (saveDialog.ShowDialog() != DialogResult.OK) { return; }
                FilePath = saveDialog.FileName;
                JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                };
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(NewLogicDic, _jsonSerializerOptions));
            }

            Dictionary<string,string> TempReadPRiceFile()
            {
                var finaldic = new Dictionary<string, string>();
                var SpoilerPriceDic = new Dictionary<string, int>();
                var TextFile = File.ReadAllLines(@"Recources\Other Files\SpoilerLogPriceLogicMap.txt");

                bool AtGame = true;
                foreach (var line in TextFile)
                {
                    var x = line.Trim();
                    x = Regex.Replace(x, @"\s+", " ");
                    if (string.IsNullOrWhiteSpace(x) || x.StartsWith("//")) { continue; }
                    if (x.Contains("//")) { x = x.Substring(0, x.IndexOf("//")); }
                    if (x.ToLower().StartsWith("#gamecodestart:"))
                    {
                        AtGame = x.ToLower().Replace("#gamecodestart:", "").Trim().Split(',').Select(y => y.Trim()).Contains(LogicObjects.MainTrackerInstance.GameCode.ToLower());
                        continue;
                    }
                    if (x.ToLower().StartsWith("#gamecodeend:")) { AtGame = true; continue; }
                    if (!AtGame) { continue; }

                    var RestrictionSplit = x.Split('!');

                    var DicAndPriceData = RestrictionSplit[0].Split('|');
                    if (DicAndPriceData.Count() < 2) { continue; }
                    finaldic.Add(DicAndPriceData[0].Trim(), DicAndPriceData[1].Trim());
                }
                return finaldic;
            }


            //FixLogicSavedWithoutSetupTime();
            void FixLogicSavedWithoutSetupTime()
            {
                var Currentfile = Utility.FileSelect("Select your broken Logic File", "MMR Tracker Save (*.txt)|*.txt");
                if (Currentfile == "") { return; }

                var Basefile = Utility.FileSelect("Select a Logic File with Time Setup Data", "MMR Tracker Save (*.txt)|*.txt");
                if (Basefile == "") { return; }

                var BrokeLogic = LogicObjects.LogicFile.FromJson(File.ReadAllText(Currentfile));
                var baseLogic = LogicObjects.LogicFile.FromJson(File.ReadAllText(Basefile));

                foreach(var i in BrokeLogic.Logic)
                {
                    var entryinbase = baseLogic.Logic.Find(x => x.Id == i.Id);
                    if (entryinbase == null) { continue; }
                    i.TimeSetup = entryinbase.TimeSetup;
                }

                var fixedLogic = BrokeLogic.ToString();

                SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Logic File (*.txt)|*.txt", FilterIndex = 1 };
                if (saveDialog.ShowDialog() != DialogResult.OK) { return; }

                StreamWriter LogicFile = new StreamWriter(File.Open(saveDialog.FileName, FileMode.Create));
                LogicFile.WriteLine(fixedLogic);
                LogicFile.Close();

            }


            //OcarinaOfTimeRando.GenerateDictionary();

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

            void FullRecreateInstanceBackup()
            {
                List<LogicObjects.LogicEntry> BackupData = Utility.CloneLogicList(LogicObjects.MainTrackerInstance.Logic);
                Clipboard.SetText(JsonConvert.SerializeObject(BackupData));

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
            LogicEditor.AssignUniqueItemnames(LogicEditor.EditorInstance.Logic);
            LogicEditor.EditorForm.CreateContextMenus();

        }

    }
}
