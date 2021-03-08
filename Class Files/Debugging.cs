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

namespace MMR_Tracker_V2
{
    class Debugging
    {
        public static bool ISDebugging = false;
        public static bool ViewAsUserMode = false;

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
            //TestExtractNames();
            //MInishCapTools.FillMinishLogic();
            //MInishCapTools.PrintMinishLogic();
            //GetAllLocations();

            SStesting();

            void OOTRTesting()
            {
                OcarinaOfTimeToolsLogicCreation.Testing();
            }

            void SStesting()
            {
                SkywardSwordTools.CreateData();
            }

            void VersionHandleTesting()
            {
                List<string> TestVersionNumbers = new List<string>
                {
                    "1",
                    "15",
                    "12.0.5",
                    "12.1.62",
                    "6.2a",
                    "7.1.2.3"
                };
                foreach (var i in TestVersionNumbers)
                {
                    Console.WriteLine(ParseVersion(i));
                }

                Version ParseVersion(string ver)
                {
                    ver = string.Join("", ver.Where(x => char.IsDigit(x) || x == '.'));
                    if (!ver.Contains(".")) { ver += ".0"; }
                    return new Version(ver);
                }
            }

            void TestExtractNames()
            {
                var L1 = "Locations.AccessWilds, (|Helpers.HasBow, Items.RocsCape, Items.Flippers), Items.KinstoneX.YellowTotemProng::3, Items.BombBag";
                var L2 = "(+28, Items.PieceOfHeart, Items.HeartContainer::4), Helpers.HasSword, Locations.WavebladeHeartPiece";
                var L3 = "Items.Wallet::3, (+980, Items.Rupee1, Items.Rupee5::5, Items.Rupee20::20, Items.Rupee50::50, Items.Rupee100::100, Items.Rupee200::200)";
                var L4 = "Locations.AccessDroplets, Items.BigKey`TOD_SET`, (|Helpers.DropletsBottomJump, (&Items.GustJar, Items.Flippers, Items.SmallKey`TOD_SET`:4)), Helpers.CanSplit2";

                L1 = L1.Replace("Items.", "").Replace("Helpers.", "").Replace("Locations.", "");
                L2 = L2.Replace("Items.", "").Replace("Helpers.", "").Replace("Locations.", "");
                L3 = L3.Replace("Items.", "").Replace("Helpers.", "").Replace("Locations.", "");
                L4 = L4.Replace("Items.", "").Replace("Helpers.", "").Replace("Locations.", "");

                var L1Names = MInishCapTools.ExtractNames(L1);
                var L2Names = MInishCapTools.ExtractNames(L2);
                var L3Names = MInishCapTools.ExtractNames(L3);
                var L4Names = MInishCapTools.ExtractNames(L4);
                Console.WriteLine("================");
                foreach (var i in L1Names) { Console.WriteLine(i); }
                Console.WriteLine("================");
                foreach (var i in L2Names) { Console.WriteLine(i); }
                Console.WriteLine("================");
                foreach (var i in L3Names) { Console.WriteLine(i); }
                Console.WriteLine("================");
                foreach (var i in L4Names) { Console.WriteLine(i); }
            }

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
        }

        public static void GetAllLocations()
        {
            foreach (var i in LogicObjects.MainTrackerInstance.Logic.Select(x => x.LocationArea).Distinct().OrderBy(x => x))
            {
                Console.WriteLine(i);
            }
        }

    }
}
