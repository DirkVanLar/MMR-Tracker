using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Forms.Other_Games
{
    class WindWakerTools
    {

        public static string[] HandleWWRSpoilerLog(string[] Log = null)
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
                    Filter = "WWR Spoiler Log (*.txt)|*.txt",
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
            SpoilerData.Add("Converted WWR");
            string header = "";
            var FileContent = Log.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            foreach (var line in FileContent)
            {
                if (line.Contains("All item locations:"))
                {
                    AtItems = true;
                }
                if (line.Contains("Starting island:"))
                {
                    AtItems = false;
                }
                if (line.Contains("Entrances:"))
                {
                    AtEntrances = true;
                }
                if (line.Contains("Charts:"))
                {
                    break;
                }
                if (AtItems || AtEntrances)
                {
                    var Parts = line.Split(':');
                    if (string.IsNullOrWhiteSpace(Parts[1])) { header = Parts[0].Trim() + " "; continue; }
                    if (AtEntrances) { header = ""; }
                    if (Parts.Length < 2) { continue; }
                    Parts[0] = rgx.Replace(Parts[0].Replace(" -", "").Replace("-", ""), "");
                    SpoilerData.Add($"{header}{Parts[0].Trim()}->{Parts[1].Trim().Replace(" -", "")}");
                }
            }
            string Settings = "";
            for (var i = 0; i < FileContent.Count(); i++)
            {
                if (FileContent[i] == "Options selected:")
                {
                    Settings = FileContent[i + 1];
                }
            }

            if (Settings.Contains("sword_mode: Start with Sword"))
            { SpoilerData.Add("SettingSwordMode->SettingSwordModeStartWith"); }
            else if (Settings.Contains("sword_mode: Randomized Sword"))
            { SpoilerData.Add("SettingSwordMode->SettingSwordModeRandomized"); }
            else
            { SpoilerData.Add("SettingSwordMode->SettingSwordModeSwordless"); }

            if (Settings.Contains("skip_rematch_bosses"))
            { SpoilerData.Add("SettingRemachBossesSkipped->SettingRemachBossesSkippedTrue"); }
            else
            { SpoilerData.Add("SettingRemachBossesSkipped->SettingRemachBossesSkippedFalse"); }

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

        public static void CreateWWRLogicFile()
        {
            var lines = File.ReadAllLines("Recources\\Dictionaries\\WWRDICTIONARYV170.csv");
            List<LogicObjects.LogicDictionaryEntry> LogicDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDictionaryEntry>>(Utility.ConvertCsvFileToJsonObject(lines));
            List<string> log = new List<string>();
            log.Add("-versionWWR 170");
            foreach (var i in LogicDictionary)
            {
                log.Add("- " + i.DictionaryName);
                log.Add("");
                log.Add("");
                log.Add("0");
                log.Add("0");
                log.Add("");
            }
            SaveFileDialog saveDic = new SaveFileDialog
            {
                Filter = "TXT File (*.txt)|*.txt",
                Title = "Save WWR Logic File",
                FileName = "WWR Logic.txt"
            };
            saveDic.ShowDialog();
            File.WriteAllLines(saveDic.FileName, log);
        }

        public static void CreateDictionary()
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            System.Net.WebClient wc = new System.Net.WebClient();
            string webData = wc.DownloadString("https://raw.githubusercontent.com/LagoLunatic/wwrando/9f4752b6defbc5849ef52044422109b36f2ad790/logic/item_locations.txt");
            string[] Lines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            List<string> DictionaryLines = new List<string> { "DictionaryName,LocationName,ItemName,LocationArea,ItemSubType,SpoilerLocation,SpoilerItem,EntrancePair" };
            LogicObjects.LogicDictionaryEntry Item = new LogicObjects.LogicDictionaryEntry();

            foreach (var i in Lines)
            {
                string Line = i.Trim();
                //Debugging.Log(Line);
                bool Unimplimented = false;
                if (Line.StartsWith("#"))
                {
                    Line.Replace("#", "");
                    Unimplimented = true;
                }
                if (Line.Contains(" -") && !Line.StartsWith("-") && !Line.Contains("\"") && Line.Contains(":") && !Line.Contains("Original item:") && !Line.Contains("Note:") && !Line.Contains("/"))
                {
                    if (!string.IsNullOrWhiteSpace(Item.DictionaryName))
                    {
                        string DictionaryLine = $"{Item.DictionaryName},{Item.LocationName},{Item.ItemName},{Item.LocationArea},{Item.ItemSubType},{Item.SpoilerLocation},{Item.SpoilerItem},";
                        DictionaryLines.Add(DictionaryLine.Replace("  ", " "));
                    }
                    Item = new LogicObjects.LogicDictionaryEntry();
                    var AllParts = Line.Split(new string[] { " -" }, StringSplitOptions.None);

                    string CombineParts = "";
                    for (var j = 1; j < AllParts.Length; j++)
                    {
                        string Seperater = (j == AllParts.Length - 1) ? "" : " -";
                        CombineParts = CombineParts + AllParts[j] + Seperater;
                    }

                    string[] Parts = new string[2] { AllParts[0], CombineParts };


                    Item.ItemSubType = "Item";
                    Item.LocationArea = Parts[0].Trim().Replace("#", "");
                    Item.LocationName = Parts[1].Substring(0, Parts[1].IndexOf(":")).Trim();
                    Item.SpoilerLocation = new string[] { Item.LocationName };
                    Item.DictionaryName = Item.LocationArea + " " + rgx.Replace(Parts[1].Trim(), "").Replace("-", "");
                    Item.LocationName = (Unimplimented) ? "" : Item.LocationName;
                }
                if (Line.Contains("Original item:"))
                {
                    Item.ItemName = Line.Split(':')[1].Trim();
                    Item.SpoilerItem = new string[] { Item.ItemName };
                }
            }
            SaveFileDialog saveDic = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                Title = "Save Dictionary File",
                FileName = "WWRDICTIONARYV" + "1.7.0" + ".csv"
            };
            saveDic.ShowDialog();
            File.WriteAllLines(saveDic.FileName, DictionaryLines);
        }
    }
}
