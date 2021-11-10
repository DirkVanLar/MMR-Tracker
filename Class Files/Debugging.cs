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
            int Startingcount = 0;
            foreach (var i in LogicObjects.MainTrackerInstance.Logic)
            {
                if (i.CanBeStartingItem(LogicObjects.MainTrackerInstance))
                {
                    Startingcount++;
                    Console.WriteLine($"\"{i.DictionaryName}\"," );
                }
            }
            Console.WriteLine(Startingcount + " Starting items Found");

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
