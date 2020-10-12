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

namespace MMR_Tracker_V2
{
    class Debugging
    {
        public static bool ISDebugging = false;

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
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("ID: " + Logic[i].ID);
                Console.WriteLine("Name: " + Logic[i].DictionaryName);
                Console.WriteLine("Location: " + Logic[i].LocationName);
                Console.WriteLine("Item: " + Logic[i].ItemName);
                Console.WriteLine("Location area: " + Logic[i].LocationArea);
                Console.WriteLine("Item Sub Type: " + Logic[i].ItemSubType);
                Console.WriteLine("Available: " + Logic[i].Available);
                Console.WriteLine("Aquired: " + Logic[i].Aquired);
                Console.WriteLine("Checked: " + Logic[i].Checked);
                Console.WriteLine("Fake Item: " + Logic[i].IsFake);
                Console.WriteLine("Random Item: " + Logic[i].RandomizedItem);
                Console.WriteLine("Spoiler Log Location name: " + string.Join(",", Logic[i].SpoilerLocation));
                Console.WriteLine("Spoiler Log Item name: " + string.Join(",", Logic[i].SpoilerItem));
                Console.WriteLine("Spoiler Log Randomized Item: " + Logic[i].SpoilerRandom);
                if (Logic[i].RandomizedState() == 0) { Console.WriteLine("Randomized State: Randomized"); }
                if (Logic[i].RandomizedState() == 1) { Console.WriteLine("Randomized State: Unrandomized"); }
                if (Logic[i].RandomizedState() == 2) { Console.WriteLine("Randomized State: Forced Fake"); }
                if (Logic[i].RandomizedState() == 3) { Console.WriteLine("Randomized State: Forced Junk"); }

                Console.WriteLine("Starting Item: " + Logic[i].StartingItem());

                string av = "Available On: ";
                if (((Logic[i].AvailableOn >> 0) & 1) == 1) { av += "Day 1, "; }
                if (((Logic[i].AvailableOn >> 2) & 1) == 1) { av += "Day 2, "; }
                if (((Logic[i].AvailableOn >> 4) & 1) == 1) { av += "Day 3, "; }
                if (((Logic[i].AvailableOn >> 1) & 1) == 1) { av += "Night 1, "; }
                if (((Logic[i].AvailableOn >> 3) & 1) == 1) { av += "Night 2, "; }
                if (((Logic[i].AvailableOn >> 5) & 1) == 1) { av += "Night 3, "; }
                Console.WriteLine(av);
                av = "Needed By: ";
                if (((Logic[i].NeededBy >> 0) & 1) == 1) { av += "Day 1, "; }
                if (((Logic[i].NeededBy >> 2) & 1) == 1) { av += "Day 2, "; }
                if (((Logic[i].NeededBy >> 4) & 1) == 1) { av += "Day 3, "; }
                if (((Logic[i].NeededBy >> 1) & 1) == 1) { av += "Night 1, "; }
                if (((Logic[i].NeededBy >> 3) & 1) == 1) { av += "Night 2, "; }
                if (((Logic[i].NeededBy >> 5) & 1) == 1) { av += "Night 3, "; }
                Console.WriteLine(av);

                var test2 = Logic[i].Required;
                if (test2 == null) { Console.WriteLine("NO REQUIREMENTS"); }
                else
                {
                    Console.WriteLine("Required");
                    for (int j = 0; j < test2.Length; j++)
                    {
                        Console.WriteLine(Logic[test2[j]].ItemName ?? Logic[test2[j]].DictionaryName);
                    }
                }
                var test3 = Logic[i].Conditionals;
                if (test3 == null) { Console.WriteLine("NO CONDITIONALS"); }
                else
                {
                    for (int j = 0; j < test3.Length; j++)
                    {
                        Console.WriteLine("Conditional " + j);
                        for (int k = 0; k < test3[j].Length; k++)
                        {
                            Console.WriteLine(Logic[test3[j][k]].ItemName ?? Logic[test3[j][k]].DictionaryName);
                        }
                    }
                }
            }
        }

        public static void TestDumbStuff()
        {
            //TestEncryption();
            //SetTestMultiworldData();
            //GenerateBigData();
            //GetAllUniqueCombos();
            //TestProgressive();
            //CreatePAcketData();
            PromptBackup();

            void TestEncryption()
            {
                var EncryptedString = Crypto.EncryptStringAES("This is a test String", "MMRTNET");
                Console.WriteLine(EncryptedString);
                Console.WriteLine(Crypto.DecryptStringAES(EncryptedString, "MMRTNET"));
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

                var aquired = Tools.ProgressiveItemAquired(LogicObjects.MainTrackerInstance.Logic, BigBombBag, UsedItems);

                Console.WriteLine(aquired);
                Console.WriteLine("");
                foreach (var i in UsedItems) { Console.WriteLine($"Final: Entry {i} was used"); }
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

    }
}
