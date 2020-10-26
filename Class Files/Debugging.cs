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

namespace MMR_Tracker_V2
{
    class Debugging
    {
        public static bool ISDebugging = false;

        public static string LogFile = "";

        public static void PrintLogicObject(List<LogicObjects.LogicEntry> Logic, int start = -1, int end = -1)
        {
            start -= 1;

            if (start < 0) { start = 0; }
            if (end == -1) { end = Logic.Count; }
            if (end < start) { end = start + 1; }
            if (end > Logic.Count) { end = Logic.Count; }

            if (start < 0) { start = 0; }
            for (int i = start; i < end; i++)
            {
                Debugging.Log("---------------------------------------");
                Debugging.Log("ID: " + Logic[i].ID);
                Debugging.Log("Name: " + Logic[i].DictionaryName);
                Debugging.Log("Location: " + Logic[i].LocationName);
                Debugging.Log("Item: " + Logic[i].ItemName);
                Debugging.Log("Location area: " + Logic[i].LocationArea);
                Debugging.Log("Item Sub Type: " + Logic[i].ItemSubType);
                Debugging.Log("Available: " + Logic[i].Available);
                Debugging.Log("Aquired: " + Logic[i].Aquired);
                Debugging.Log("Checked: " + Logic[i].Checked);
                Debugging.Log("Fake Item: " + Logic[i].IsFake);
                Debugging.Log("Random Item: " + Logic[i].RandomizedItem);
                Debugging.Log("Spoiler Log Location name: " + string.Join(",", Logic[i].SpoilerLocation));
                Debugging.Log("Spoiler Log Item name: " + string.Join(",", Logic[i].SpoilerItem));
                Debugging.Log("Spoiler Log Randomized Item: " + Logic[i].SpoilerRandom);
                if (Logic[i].RandomizedState() == 0) { Debugging.Log("Randomized State: Randomized"); }
                if (Logic[i].RandomizedState() == 1) { Debugging.Log("Randomized State: Unrandomized"); }
                if (Logic[i].RandomizedState() == 2) { Debugging.Log("Randomized State: Forced Fake"); }
                if (Logic[i].RandomizedState() == 3) { Debugging.Log("Randomized State: Forced Junk"); }

                Debugging.Log("Starting Item: " + Logic[i].StartingItem());

                string av = "Available On: ";
                if (((Logic[i].AvailableOn >> 0) & 1) == 1) { av += "Day 1, "; }
                if (((Logic[i].AvailableOn >> 2) & 1) == 1) { av += "Day 2, "; }
                if (((Logic[i].AvailableOn >> 4) & 1) == 1) { av += "Day 3, "; }
                if (((Logic[i].AvailableOn >> 1) & 1) == 1) { av += "Night 1, "; }
                if (((Logic[i].AvailableOn >> 3) & 1) == 1) { av += "Night 2, "; }
                if (((Logic[i].AvailableOn >> 5) & 1) == 1) { av += "Night 3, "; }
                Debugging.Log(av);
                av = "Needed By: ";
                if (((Logic[i].NeededBy >> 0) & 1) == 1) { av += "Day 1, "; }
                if (((Logic[i].NeededBy >> 2) & 1) == 1) { av += "Day 2, "; }
                if (((Logic[i].NeededBy >> 4) & 1) == 1) { av += "Day 3, "; }
                if (((Logic[i].NeededBy >> 1) & 1) == 1) { av += "Night 1, "; }
                if (((Logic[i].NeededBy >> 3) & 1) == 1) { av += "Night 2, "; }
                if (((Logic[i].NeededBy >> 5) & 1) == 1) { av += "Night 3, "; }
                Debugging.Log(av);

                var test2 = Logic[i].Required;
                if (test2 == null) { Debugging.Log("NO REQUIREMENTS"); }
                else
                {
                    Debugging.Log("Required");
                    for (int j = 0; j < test2.Length; j++)
                    {
                        Debugging.Log(Logic[test2[j]].ItemName ?? Logic[test2[j]].DictionaryName);
                    }
                }
                var test3 = Logic[i].Conditionals;
                if (test3 == null) { Debugging.Log("NO CONDITIONALS"); }
                else
                {
                    for (int j = 0; j < test3.Length; j++)
                    {
                        Debugging.Log("Conditional " + j);
                        for (int k = 0; k < test3[j].Length; k++)
                        {
                            Debugging.Log(Logic[test3[j][k]].ItemName ?? Logic[test3[j][k]].DictionaryName);
                        }
                    }
                }
            }
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
            //TestEncryption();
            //SetTestMultiworldData();
            //GenerateBigData();
            //GetAllUniqueCombos();
            //TestProgressive();
            //CreatePAcketData();
            //PromptBackup();
            FillMinishLogic();

            void TestEncryption()
            {
                var EncryptedString = Crypto.EncryptStringAES("This is a test String", "MMRTNET");
                Debugging.Log(EncryptedString);
                Debugging.Log(Crypto.DecryptStringAES(EncryptedString, "MMRTNET"));
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

            void GenerateBigData()
            {
                string[] ShopChecks = new string[]
            {
                "KF Shop Item 1",
                "KF Shop Item 2",
                "KF Shop Item 3",
                "KF Shop Item 4",
                "KF Shop Item 5",
                "KF Shop Item 6",
                "KF Shop Item 7",
                "KF Shop Item 8",
                "Market Potion Shop Item 1",
                "Market Potion Shop Item 2",
                "Market Potion Shop Item 3",
                "Market Potion Shop Item 4",
                "Market Potion Shop Item 5",
                "Market Potion Shop Item 6",
                "Market Potion Shop Item 7",
                "Market Potion Shop Item 8",
                "Market Bombchu Shop Item 1",
                "Market Bombchu Shop Item 2",
                "Market Bombchu Shop Item 3",
                "Market Bombchu Shop Item 4",
                "Market Bombchu Shop Item 5",
                "Market Bombchu Shop Item 6",
                "Market Bombchu Shop Item 7",
                "Market Bombchu Shop Item 8",
                "Kak Potion Shop Item 1",
                "Kak Potion Shop Item 2",
                "Kak Potion Shop Item 3",
                "Kak Potion Shop Item 4",
                "Kak Potion Shop Item 5",
                "Kak Potion Shop Item 6",
                "Kak Potion Shop Item 7",
                "Kak Potion Shop Item 8",
                "GC Shop Item 1",
                "GC Shop Item 2",
                "GC Shop Item 3",
                "GC Shop Item 4",
                "GC Shop Item 5",
                "GC Shop Item 6",
                "GC Shop Item 7",
                "GC Shop Item 8",
                "ZD Shop Item 1",
                "ZD Shop Item 2",
                "ZD Shop Item 3",
                "ZD Shop Item 4",
                "ZD Shop Item 5",
                "ZD Shop Item 6",
                "ZD Shop Item 7",
                "ZD Shop Item 8",
                "LW Deku Scrub Near Bridge",
                "LW Deku Scrub Near Deku Theater Right",
                "LW Deku Scrub Near Deku Theater Left",
                "LW Deku Scrub Grotto Rear",
                "LW Deku Scrub Grotto Front",
                "SFM Deku Scrub Grotto Rear",
                "SFM Deku Scrub Grotto Front",
                "LLR Deku Scrub Grotto Left",
                "LLR Deku Scrub Grotto Right",
                "LLR Deku Scrub Grotto Center",
                "GC Deku Scrub Grotto Left",
                "GC Deku Scrub Grotto Right",
                "GC Deku Scrub Grotto Center",
                "DMC Deku Scrub Grotto Left",
                "DMC Deku Scrub Grotto Right",
                "DMC Deku Scrub Grotto Center",
                "ZR Deku Scrub Grotto Rear",
                "ZR Deku Scrub Grotto Front",
                "LH Deku Scrub Grotto Left",
                "LH Deku Scrub Grotto Right",
                "LH Deku Scrub Grotto Center",
                "Colossus Deku Scrub Grotto Rear",
                "Colossus Deku Scrub Grotto Front",
                "GV Deku Scrub Grotto Rear",
                "GV Deku Scrub Grotto Front",
                "Dodongos Cavern Deku Scrub Side Room Near Dodongos",
                "Dodongos Cavern Deku Scrub Lobby",
                "Dodongos Cavern Deku Scrub Near Bomb Bag Right",
                "Dodongos Cavern Deku Scrub Near Bomb Bag Left",
                "Jabu Jabus Belly Deku Scrub",
                "Ganons Castle Deku Scrub Center-Left",
                "Ganons Castle Deku Scrub Center-Right",
                "Ganons Castle Deku Scrub Right",
                "Ganons Castle Deku Scrub Left",
                "Dodongos Cavern MQ Deku Scrub Lobby Rear",
                "Dodongos Cavern MQ Deku Scrub Lobby Front",
                "Dodongos Cavern MQ Deku Scrub Staircase",
                "Dodongos Cavern MQ Deku Scrub Side Room Near Lower Lizalfos",
                "Ganons Castle MQ Deku Scrub Center-Left",
                "Ganons Castle MQ Deku Scrub Center",
                "Ganons Castle MQ Deku Scrub Center-Right",
                "Ganons Castle MQ Deku Scrub Left",
                "Ganons Castle MQ Deku Scrub Right",
                "Ganons Castle MQ Forest Trial Eye Switch Chest"
            };

                Console.WriteLine("#");
                foreach (var i in ShopChecks)
                {
                    Console.WriteLine(i + " Price");
                    Console.WriteLine(i + " Price 1");
                    Console.WriteLine(i + " Price 2");
                    Console.WriteLine(i + " Price 3");
                }
                Console.WriteLine("#");
                foreach (var i in ShopChecks)
                {
                    Console.WriteLine(i + " Price Range");
                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.WriteLine("");
                }
                Console.WriteLine("#");
                foreach (var i in ShopChecks)
                {
                    Console.WriteLine("");
                    Console.WriteLine("0 <> 99");
                    Console.WriteLine("100 <> 200");
                    Console.WriteLine("201 <> 500");
                }
                Console.WriteLine("#");
                foreach (var i in ShopChecks)
                {
                    Console.WriteLine(i + " Price Range");
                    Console.WriteLine(i + " Price Range");
                    Console.WriteLine(i + " Price Range");
                    Console.WriteLine(i + " Price Range");
                }
                Console.WriteLine("#");

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

            void TestProgressive()
            {
                var BigBombBag = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == "Town Bomb Bag (30)");

                List<int> UsedItems = new List<int>();

                var aquired = BigBombBag.ItemUseable();

                Debugging.Log(aquired.ToString());
                Debugging.Log("");
                foreach (var i in UsedItems) { Debugging.Log($"Final: Entry {i} was used"); }
            }

            void CreatePAcketData()
            {
                string NetDataString = "{\"PlayerID\":2,\"IPData\":{\"IP\":\"99.145.3.193\",\"PORT\":2113,\"DisplayName\":null},\"RequestingUpdate\":0,\"LogicData\":[{\"ID\":2,\"PI\":1,\"RI\":0,\"Ch\":true},{\"ID\":3,\"PI\":1,\"RI\":0,\"Ch\":true},{\"ID\":6,\"PI\":1,\"RI\":1,\"Ch\":true}]}";

                LogicObjects.MMRTpacket NetData = new LogicObjects.MMRTpacket();
                NetData = JsonConvert.DeserializeObject<LogicObjects.MMRTpacket>(NetDataString);
                OnlinePlay.ManageNetData(NetData);
            }

            void PromptBackup()
            {
                var function = MessageBox.Show("Restore Data (Yes), Backup Data (No)", "", MessageBoxButtons.YesNoCancel);
                if (function == DialogResult.Yes) { FullRecreateInstanceRestore(); }
                else if (function == DialogResult.No) { FullRecreateInstanceBackup(); }
            }

            void FullRecreateInstanceBackup()
            {
                List<LogicObjects.LogicEntry> BackupData = Utility.CloneLogicList(LogicObjects.MainTrackerInstance.Logic);
                Clipboard.SetText(JsonConvert.SerializeObject(BackupData));

            }

            void FullRecreateInstanceRestore()
            {
                List<LogicObjects.LogicEntry> RestorData = JsonConvert.DeserializeObject<List<LogicObjects.LogicEntry>>(Clipboard.GetText());
                ListBox CheckedItems = new ListBox();
                foreach(var i in RestorData)
                {
                    var NewLoc = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == i.DictionaryName);
                    if (NewLoc == null) { continue; }
                    NewLoc.Options = i.Options;
                    if (i.Checked)
                    {
                        CheckedItems.Items.Add(NewLoc);
                    }
                    else if (i.HasRandomItem(false))
                    {
                        NewLoc.RandomizedItem = NewLoc.SpoilerRandom;
                    }
                    else if (i.IsTrick)
                    {
                        NewLoc.TrickEnabled = i.TrickEnabled;
                    }
                }
                for (int i = 0; i < CheckedItems.Items.Count; i++)
                {
                    CheckedItems.SetSelected(i, true);
                }

                MainInterface.CurrentProgram.CheckItemSelected(CheckedItems,true);
                LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance, true);
                MainInterface.CurrentProgram.PrintToListBox();
            }

            void CreateMinishLogDic()
            {
                var MinishLogicFile = File.ReadAllLines(@"C:\Users\ttalbot\Documents\VS CODE STUFF\MCR\Resources\default.logic");

                bool AtLogic = false;
                bool IsInIf = false;
                bool IsInElse = false;
                bool atKinstones = false;
                bool atRupeeMania = false;

                LogicObjects.TrackerInstance MinishLogic = new LogicObjects.TrackerInstance
                {
                    Logic = new List<LogicObjects.LogicEntry>(),
                    LogicDictionary = new List<LogicObjects.LogicDictionaryEntry>(),
                    GameCode = "MCR",
                    LogicVersion = 1
                };


                string CurrentLocation = "Overworld";
                foreach (var i in MinishLogicFile)
                {
                    if (i.Trim().StartsWith("#Kinstone")) { atKinstones = true; }
                    if (i.Trim().StartsWith("HearthPot;")) { atKinstones = false; }
                    if (i.Trim().StartsWith("!ifdef - RUPEEMANIA")) { atRupeeMania = true; }
                    if (i.Trim() == "# Item Macro Helpers") { AtLogic = true; }
                    if (i.Trim() == "#Unrandomized locations") { AtLogic = false; }
                    if (!AtLogic) { continue; }
                    if (string.IsNullOrWhiteSpace(i)) { continue; }
                    if (i.Trim().StartsWith("#")) { continue; }

                    if (i.Contains("!ifdef") || i.Contains("!ifndef")) { IsInIf = true; continue; }
                    if (i.Contains("!else")) { IsInElse = true; IsInIf = false; continue; }
                    if (i.Contains("!endif")) { IsInElse = false; IsInIf = false; continue; }

                    //if (IsInElse) { continue; }

                    string CleanedLine = i.Replace("Items.", "").Replace("Helpers.", "").Replace("Locations.", "");

                    var Data = CleanedLine.Split(';').Select(x => x.Trim()).ToArray();

                    if (Data.Count() < 4) { continue; }
                    LogicObjects.LogicEntry Entry = new LogicObjects.LogicEntry();
                    LogicObjects.LogicDictionaryEntry Dic = new LogicObjects.LogicDictionaryEntry();

                    if (Data[0].Contains(":"))
                    {
                        Data[0] = Data[0].Substring(0, Data[0].IndexOf(":"));
                    }

                    CurrentLocation = "Overworld";

                    var LogicData = Data[3].Split(',').Select(x => x.Replace(")", "").Replace("(", "").Replace("|", "").Replace("&", "")).ToList();
                    var AreaData = LogicData.Find(x => x.Contains("Access"));
                    if (AreaData != null)
                    {
                        string Loc = string.Concat(AreaData.Replace("Access", "").Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                        CurrentLocation = Loc;
                    }

                    Entry.DictionaryName = Data[0];

                    if (MinishLogic.Logic.Where(x => x.DictionaryName == Entry.DictionaryName).Any()) { continue; }
                    Console.WriteLine(Entry.DictionaryName);

                    Entry.IsFake = Data[1] == "Helper";
                    Entry.Checked = false;
                    Entry.ItemSubType = "Item";
                    Entry.RandomizedItem = -2;
                    Entry.SpoilerRandom = -2;
                    if (!Entry.IsFake)
                    {
                        Dic.DictionaryName = Data[0];
                        Dic.LocationArea = CurrentLocation;
                        Dic.LocationName = string.Concat(Data[0].Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                        Dic.SpoilerLocation = Data[0];

                        string ItemName = "";
                        string DicItemName = "";
                        if (Data.Count() > 4)
                        {
                            DicItemName = Data[4];
                            if (DicItemName.Contains("#"))
                            {
                                DicItemName = DicItemName.Substring(0, DicItemName.IndexOf("#")).Trim();
                            }
                            if (DicItemName.Contains("'"))
                            {
                                DicItemName = DicItemName.Substring(0, DicItemName.IndexOf("'")).Trim();
                            }
                            if (string.IsNullOrWhiteSpace(DicItemName)) { DicItemName = ""; }
                            ItemName = string.Concat(DicItemName.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                        }

                        Dic.SpoilerItem = DicItemName;
                        Dic.ItemName = ItemName;
                        Dic.ItemSubType = "Item";
                        MinishLogic.LogicDictionary.Add(Dic);

                        Entry.LocationArea = CurrentLocation;
                        Entry.LocationName = string.Concat(Data[0].Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                        Entry.ItemName = ItemName;
                        Console.WriteLine(ItemName);
                    }

                    MinishLogic.Logic.Add(Entry);
                    Console.WriteLine("================");
                }

                LogicObjects.MainTrackerInstance = MinishLogic;
                MainInterface.CurrentProgram.PrintToListBox();
                MainInterface.CurrentProgram.ResizeObject();
                MainInterface.CurrentProgram.FormatMenuItems();

                List<string> csv = new List<string> { "DictionaryName,LocationName,ItemName,LocationArea,ItemSubType,SpoilerLocation,SpoilerItem" };
                foreach (LogicObjects.LogicDictionaryEntry entry in MinishLogic.LogicDictionary)
                {
                    csv.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                         entry.DictionaryName, entry.LocationName, entry.ItemName, entry.LocationArea,
                         entry.ItemSubType, entry.SpoilerLocation, entry.SpoilerItem));
                }

                SaveFileDialog saveDic = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    Title = "Save Dictionary File",
                    FileName = "MCRDICTIONARYV6.csv"
                };
                saveDic.ShowDialog();
                File.WriteAllLines(saveDic.FileName, csv);

                SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Logic File (*.txt)|*.txt", FilterIndex = 1 };
                if (saveDialog.ShowDialog() != DialogResult.OK) { return; }
                var logicText = LogicEditing.WriteLogicToArray(MinishLogic, true).ToList();
                StreamWriter LogicFile = new StreamWriter(File.Open(saveDialog.FileName, FileMode.Create));
                for (int i = 0; i < logicText.Count; i++)
                {
                    if (i == logicText.Count - 1) { LogicFile.Write(logicText[i]); break; }
                    LogicFile.WriteLine(logicText[i]);
                }
                LogicFile.Close();

                return;
            }

            void FillMinishLogic()
            {
                var MinishLogicFile = File.ReadAllLines(@"C:\Users\ttalbot\Documents\VS CODE STUFF\MCR\Resources\default.logic");
                bool AtLogic = false;
                bool IsInIf = false;
                bool IsInElse = false;
                foreach (var i in MinishLogicFile)
                {
                    if (i.Trim() == "# Item Macro Helpers") { AtLogic = true; }
                    if (i.Trim() == "#Unrandomized locations") { AtLogic = false; }
                    if (!AtLogic) { continue; }
                    if (string.IsNullOrWhiteSpace(i)) { continue; }
                    if (i.Trim().StartsWith("#")) { continue; }

                    if (i.Contains("!ifdef") || i.Contains("!ifndef")) { IsInIf = true; continue; }
                    if (i.Contains("!else")) { IsInElse = true; IsInIf = false; continue; }
                    if (i.Contains("!endif")) { IsInElse = false; IsInIf = false; continue; }

                    //if (IsInElse) { continue; }

                    string CleanedLine = i.Replace("Items.", "").Replace("Helpers.", "").Replace("Locations.", "");

                    var Data = CleanedLine.Split(';').Select(x => x.Trim()).ToArray();

                    if (Data.Count() < 4) { continue; }

                    var LogicData = ConvertMinishLogic(Data[3]);

                    var Logicentry = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == Data[0]);

                    if (Logicentry == null)
                    {
                        Console.WriteLine($"Failed to find {Data[0]} in logic");
                        continue;
                    }

                    if (LogicData == null)
                    {
                        Console.WriteLine($"{Logicentry.DictionaryName} Had no logic");
                        continue;
                    }

                    if (LogicData[0][0] == -1)
                    {
                        Console.WriteLine($"Failed to Parse Logic for {Logicentry.DictionaryName}");
                        continue;
                    }

                    Console.WriteLine($"Successfully Created Logic for {Logicentry.DictionaryName}");
                    Logicentry.Conditionals = LogicData;

                }

                
            }

            int[][] ConvertMinishLogic(string Input)
            {
                Input = Input.Replace("Items.", "").Replace("Helpers.", "").Replace("Locations.", "");

                var InputList = Input.ToArray();

                List<char> Actions = new List<char> { '&' };

                try
                {
                    for (var i = 0; i < Input.Length; i++)
                    {
                        if (Input[i] == '|') { Actions.Add('|'); InputList[i] = ' '; }
                        if (Input[i] == '&') { Actions.Add('&'); InputList[i] = ' '; }
                        if (Input[i] == ')') { Actions.RemoveAt(Actions.Count() - 1); }
                        if (Input[i] == ',') { InputList[i] = Actions[Actions.Count() - 1]; }
                    }
                }
                catch { }

                string output = new string(InputList);

                LogicParser MinishParser = new LogicParser();

                int[][] Conditional = null;
                try
                {
                    Debugging.Log(output);
                    var OrderedLogic = Utility.CloneLogicList(LogicObjects.MainTrackerInstance.Logic).OrderBy(x => x.DictionaryName.Count()).Reverse();
                    foreach (var i in OrderedLogic)
                    {
                        if (i.SpoilerItem != null && i.SpoilerItem.Count() > 0 && !string.IsNullOrWhiteSpace(i.SpoilerItem[0]) && output.Contains(i.SpoilerItem[0]))
                        {
                            output = output.Replace(i.SpoilerItem[0], i.ID.ToString());
                        }
                    }
                    foreach (var i in OrderedLogic)
                    {
                        if (output.Contains(i.DictionaryName))
                        {
                            output = output.Replace(i.DictionaryName, i.ID.ToString());
                        }
                    }
                    Debugging.Log(output);
                    Conditional = MinishParser.ConvertLogicToConditional(output);
                }
                catch
                {
                    Conditional = new int[1][] { new int[] { -1 } };
                }
                return Conditional;
            }
        }

        public static void CleanLogicEntry(LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            if (entry.Required == null && entry.Conditionals == null) { return; }
            var l = Instance.Logic;
            if (entry.Required.Where(x => l[x].DictionaryName.StartsWith("MMRTCombinations") || l[x].DictionaryName.StartsWith("MMRTCheckContains")).Any())
            { return;}
            MoveRequirementsToConditionals(entry);
            RemoveRedundantConditionals(entry);
            MakeCommonConditionalsRequirements(entry);
        }

        public static void MoveRequirementsToConditionals(LogicObjects.LogicEntry entry)
        {
            if (entry.Required == null) { return; }
            if (entry.Conditionals == null)
            {
                List<int> NewConditionals = new List<int>();
                foreach(var i in entry.Required)
                {
                    NewConditionals.Add(i);
                }
                entry.Conditionals = new int[][] { NewConditionals.ToArray() };
            }
            else
            {
                var NewConditionals = entry.Conditionals.Select(x => x.ToList()).ToArray();
                foreach (var i in NewConditionals)
                {
                    i.AddRange(entry.Required.ToList());
                }
                entry.Conditionals = NewConditionals.Select(x => x.ToArray()).ToArray();
            }
            entry.Required = null;
        }

        public static void RemoveRedundantConditionals(LogicObjects.LogicEntry entry)
        {
            if (entry.Conditionals == null) { return; }
            var cleanedConditionals = entry.Conditionals.Select(x => x.Distinct().ToArray()).ToArray();
            var NewConditionals = cleanedConditionals.Where(i => !IsRedundant(i, cleanedConditionals));

            entry.Conditionals = NewConditionals.ToArray();

            bool IsRedundant(int[] FocusedList, int[][] CheckingList)
            {   
                foreach(var i in CheckingList)
                {
                    if (!i.Equals(FocusedList) && i.All(j => FocusedList.Contains(j)))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static void MakeCommonConditionalsRequirements(LogicObjects.LogicEntry entry)
        {
            if (entry.Conditionals == null) { return; }
            List<int> ConsistantConditionals = 
                entry.Conditionals.SelectMany(x => x).Distinct().Where(i => entry.Conditionals.All(x => x.Contains(i))).ToList();

            var NewRequirements = (entry.Required ?? new List<int>().ToArray()).ToList();
            NewRequirements.AddRange(ConsistantConditionals);
            entry.Required = (NewRequirements.Any()) ? NewRequirements.Distinct().ToArray() : null;

            var NewConditionals = entry.Conditionals.Select(x => x.ToList()).ToList();
            foreach (var i in NewConditionals) 
            {
                i.RemoveAll(x => ConsistantConditionals.Contains(x));
            }
            NewConditionals.RemoveAll(x => !x.Any());
            entry.Conditionals = (NewConditionals.Any()) ? NewConditionals.Select(x => x.ToArray()).ToArray() : null;
        }

    }
}
