using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Forms.Other_Games
{
    class MInishCapTools
    {

        public static void CreateMinishLogDic()
        {
            var MinishLogicFile = File.ReadAllLines(@"D:\Emulated Games\Emulator\mGBA-0.7.3-win64\MinishRandomizer.v0.6.1a\MinishRandomizer.v0.6.1a\default.logic");

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

                Entry.DictionaryName = Data[0];

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


                if (MinishLogic.Logic.Where(x => x.DictionaryName == Entry.DictionaryName).Any()) { continue; }
                Console.WriteLine(Entry.DictionaryName);

                Entry.IsFake = Data[1] == "Helper";
                Entry.Checked = false;
                Entry.ItemSubType = "Item";
                Entry.RandomizedItem = -2;
                Entry.SpoilerRandom = -2;
                if (!Entry.IsFake)
                {
                    Dic.DictionaryName = Entry.DictionaryName;
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

        public static void FillMinishLogic()
        {
            var MinishLogicFile = File.ReadAllLines(@"D:\Emulated Games\Emulator\mGBA-0.7.3-win64\MinishRandomizer.v0.6.1a\MinishRandomizer.v0.6.1a\default.logic");
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

        public static void PrintMinishLogic()
        {
            var MinishLogicFile = File.ReadAllLines(@"D:\Emulated Games\Emulator\mGBA-0.7.3-win64\MinishRandomizer.v0.6.1a\MinishRandomizer.v0.6.1a\default.logic");
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

                Console.WriteLine("Logic Entry" + Data[0]);
                Console.WriteLine(Data[3]);
                Console.WriteLine("==================================================");

            }
        }

        public static int[][] ConvertMinishLogic(string Input)
        {
            if (string.IsNullOrWhiteSpace(Input)) { return null; }
            var OrderedLogic = Utility.CloneLogicList(LogicObjects.MainTrackerInstance.Logic);
            var OrderedLogicSpoiler = Utility.CloneLogicList(LogicObjects.MainTrackerInstance.Logic)
                    .Where(x => x.SpoilerItem != null && x.SpoilerItem.Count() > 0 && !string.IsNullOrWhiteSpace(x.SpoilerItem[0]));
            Input = Input.Replace("Items.", "").Replace("Helpers.", "").Replace("Locations.", "");

            var InputList = Input.ToArray();

            List<char> Actions = new List<char> { '&' };

            try
            {
                for (var i = 0; i < Input.Length; i++)
                {
                    if (Input[i] == '|') { Actions.Add('|'); InputList[i] = ' '; }
                    if (Input[i] == '&') { Actions.Add('&'); InputList[i] = ' '; }
                    if (Input[i] == ')' && Actions.Count > 1) { Actions.RemoveAt(Actions.Count() - 1); }
                    if (Input[i] == ',') { InputList[i] = Actions[Actions.Count() - 1]; }
                }
            }
            catch { }

            string output = new string(InputList);

            LogicParser MinishParser = new LogicParser();

            int[][] Conditional = null;
            try
            {
                Dictionary<string, string> ReplacerList = new Dictionary<string, string>();
                foreach (var i in OrderedLogicSpoiler)
                {
                    if (!ReplacerList.ContainsKey(i.SpoilerItem[0]))
                    {
                        ReplacerList.Add(i.SpoilerItem[0], i.ID.ToString());
                    }
                }

                foreach (var i in OrderedLogic)
                {
                    if (!ReplacerList.ContainsKey(i.DictionaryName))
                    {
                        ReplacerList.Add(i.DictionaryName, i.ID.ToString());
                    }
                }

                foreach (var i in ReplacerList.OrderBy(x => x.Key.Count()).Reverse())
                {
                    if (output.Contains(i.Key))
                    {
                        output = output.Replace(i.Key, i.Value);
                    }
                }
                Conditional = MinishParser.ConvertLogicToConditional(output);
            }
            catch
            {
                Conditional = new int[1][] { new int[] { -1 } };
            }
            return Conditional;
        }

        public static string ConvertMinishLogicString(string Input)
        {
            var OrderedLogic = Utility.CloneLogicList(LogicObjects.MainTrackerInstance.Logic).OrderBy(x => x.DictionaryName.Count()).Reverse();
            Input = Input.Replace("Items.", "").Replace("Helpers.", "").Replace("Locations.", "");

            var InputList = Input.ToArray();

            List<char> Actions = new List<char> { '&' };

            try
            {
                for (var i = 0; i < Input.Length; i++)
                {
                    if (Input[i] == '|') { Actions.Add('|'); InputList[i] = ' '; }
                    if (Input[i] == '&') { Actions.Add('&'); InputList[i] = ' '; }
                    if (Input[i] == ')' && Actions.Count > 1) { Actions.RemoveAt(Actions.Count() - 1); }
                    if (Input[i] == ',') { InputList[i] = Actions[Actions.Count() - 1]; }
                }
            }
            catch { }

            string output = new string(InputList);
            return output;
        }

        public static string[] ExtractNames(string x)
        {
            List<string> Sets = new List<string>();
            string Word = "";
            bool InCountObject = false;
            foreach (var i in x)
            {
                if (i == '+') { InCountObject = true; }
                if (i == ')') { InCountObject = false; }

                if (AddChar(i)) { Word += i; }
                else
                {
                    if (Word != "")
                    {
                        Sets.Add(Word);
                        Word = "";
                    }
                }
            }
            if (Word != "") { Sets.Add(Word); }
            return Sets.Select(y => y).Distinct().ToArray();

            bool AddChar(char i)
            {
                return i != '|' && i != '&' && i != '(' && i != ')' && (i != ' ' || InCountObject) && (i != ',' || InCountObject);
            }
        }

    }
}
