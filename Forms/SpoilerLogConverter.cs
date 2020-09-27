using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Forms
{
    public partial class SpoilerLogConverter : Form
    {
        public SpoilerLogConverter()
        {
            InitializeComponent();
        }

        public class entranceTable
        {
            public string Entrance { get; set; }
            public string To { get; set; }
            public string Exit { get; set; }
            public string From { get; set; }
        }

        private void SpoilerLogConverter_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("Select a Spoiler Log");
            comboBox1.Items.Add("Ocarina Of Time Rando");
            comboBox1.Items.Add("Wind Waker Rando");
            comboBox1.Items.Add("Twilight Princess Rando");
            comboBox1.Items.Add("Link to the past Rando");
            comboBox1.Items.Add("Minish Cap Rando");
            if (LogicObjects.MainTrackerInstance.GameCode == "OOTR") { comboBox1.SelectedIndex = 1; }
            else if (LogicObjects.MainTrackerInstance.GameCode == "WWR") { comboBox1.SelectedIndex = 2; }
            else if (LogicObjects.MainTrackerInstance.GameCode == "TPR") { comboBox1.SelectedIndex = 3; }
            else if (LogicObjects.MainTrackerInstance.GameCode == "LTTPR") { comboBox1.SelectedIndex = 4; }
            else if (LogicObjects.MainTrackerInstance.GameCode == "MCR") { comboBox1.SelectedIndex = 5; }
            else { comboBox1.SelectedIndex = 0; }
            
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    MessageBox.Show("Please Select A Game!");
                    break;
                case 1:
                    HandleOOTRSpoilerLog();
                    break;
                case 2:
                    HandleWWRSpoilerLog();
                    break;
                case 3:
                    MessageBox.Show("NYI!");
                    break;
                case 4:
                    MessageBox.Show("NYI!");
                    break;
                case 5:
                    MessageBox.Show("NYI!");
                    break;
            }
        }

        public static string[] HandleOOTRSpoilerLog(string Log = "")
        {
            void CreateOOTRLogicFile()
            {
                List<LogicObjects.LogicDictionaryEntry> LogicDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDictionaryEntry>>(Utility.ConvertCsvFileToJsonObject("Recources\\Dictionaries\\OOTRDICTIONARYV5.csv"));
                List<string> log = new List<string>();
                log.Add("-versionOOTR 5");
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
                    Title = "Save OOT Logic File",
                    FileName = "OOTR Logic.txt"
                };
                saveDic.ShowDialog();
                File.WriteAllLines(saveDic.FileName, log);
            }

            bool ManualConvert = (Log == "");
            string filename = "";

            var dic = LogicObjects.MainTrackerInstance.LogicDictionary;

            if (dic.Count < 1 || LogicObjects.MainTrackerInstance.GameCode != "OOTR")
            {
                if (!Debugging.ISDebugging) { MessageBox.Show("You must first import an OOTR Logic File"); }
                else { CreateOOTRLogicFile(); }
                return null;
            }

            if (ManualConvert)
            {
                OpenFileDialog SelectedFile = new OpenFileDialog
                {
                    Title = $"Select OOTR Spoiler Log",
                    Filter = "OOTR Spoiler Log (*.json)|*.json",
                    FilterIndex = 1,
                    Multiselect = false
                };
                if (SelectedFile.ShowDialog() != DialogResult.OK) { return null; }
                filename = SelectedFile.FileName;
                Log = File.ReadAllText(SelectedFile.FileName);
            }

            dynamic array = JsonConvert.DeserializeObject(Log);

            //Get Settings
            bool GanonKeyOnLACS = false;
            bool StartAsChild = true;
            int Forest = 0;
            int Kakariko = 0;
            bool OpenDOT = true;
            int Zora = 0;
            int Gerudo = 0;
            int bridge = 0;
            bool Chu = false;
            bool SunSong = false;
            bool mask = false;
            bool Scarecrow = false;
            bool CoupledEntrances = true;
            foreach (dynamic item in array.settings)
            {
                string line = item.ToString();
                if (line.Contains("shuffle_ganon_bosskey") && line.Contains("lacs")) { GanonKeyOnLACS = true; }
                if (line.Contains("starting_age") && line.Contains("adult")) { StartAsChild = false; }
                if (line.Contains("open_door_of_time") && line.Contains("closed")) { OpenDOT = false; }
                if (line.Contains("bombchus_in_logic") && line.Contains("true")) { Chu = true; }
                if (line.Contains("logic_no_night_tokens_without_suns_song") && line.Contains("true")) { SunSong = true; }
                if (line.Contains("complete_mask_quest") && line.Contains("true")) { mask = true; }
                if (line.Contains("free_scarecrow") && line.Contains("true")) { mask = true; }
                if (line.Contains("decouple_entrances") && line.Contains("true")) { CoupledEntrances = false; }
                if (line.Contains("open_forest"))
                {
                    if (line.Contains("closed_deku")) { Forest = 1; }
                    else if (line.Contains("closed")) { Forest = 2; }
                }
                if (line.Contains("open_kakariko"))
                {
                    if (line.Contains("zelda")) { Kakariko = 1; }
                    else if (line.Contains("closed")) { Kakariko = 2; }
                }
                if (line.Contains("zora_fountain"))
                {
                    if (line.Contains("adult")) { Zora = 1; }
                    else if (line.Contains("open")) { Zora = 2; }
                }
                if (line.Contains("gerudo_fortress"))
                {
                    if (line.Contains("one")) { Gerudo = 1; }
                    else if (line.Contains("open")) { Gerudo = 2; }
                }
                if (line.Contains("bridge"))
                {
                    if (line.Contains("vanilla")) { bridge = 1; }
                    else if (line.Contains("stone")) { bridge = 2; }
                    else if (line.Contains("medallion")) { bridge = 3; }
                    else if (line.Contains("dungeon")) { bridge = 4; }
                    else if (line.Contains("skull")) { bridge = 5; }
                }
            }

            //Handle Entraces
            List<entranceTable> SpoilerEntranceTable = new List<entranceTable>();
            foreach (dynamic item in array.entrances)
            {
                var entry = new entranceTable();
                string line = item.ToString();
                line = line.Replace(',', ':').Replace('"', '{').Replace("{", "").Replace("}", "");
                var lines = line.Split(':').Select(x => x.Trim()).ToArray();
                var front = lines[0].Split(new string[] { "->" }, StringSplitOptions.None);
                entry.Entrance = front[0].Trim();
                entry.To = front[1].Trim();
                entry.Exit = (lines[1].Contains("region")) ? lines[2].Trim() : lines[1].Trim();
                entry.From = (lines[1].Contains("region")) ? lines[4].Trim() : "";
                SpoilerEntranceTable.Add(entry);
            }

            //Check the MQ entry in the spoiler log and convert the entries of any dungeon that are MQ to the MQ entry
            

            List<string> FileContent = new List<string>();
            FileContent.Add("Converted OOTR");

            foreach (var i in SpoilerEntranceTable)
            {
                FileContent.Add($"{i.Entrance}>{i.To}->{i.Exit}" + (i.From == "" ? "" : $"<{i.From}"));
            }

            bool IsOneWay(string i)
            {
                var j = i.Split(new string[] { "->" }, StringSplitOptions.None)[0];
                var k = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerLocation == j);
                return string.IsNullOrWhiteSpace(k.EntrancePair) || i.Contains("Adult Spawn") || i.Contains("Child Spawn");
            }

            //Attempt to add Entrances that were left out of the spoiler log
            foreach (var DictionaryItem in LogicObjects.MainTrackerInstance.LogicDictionary.Where(x => x.ItemSubType == "Entrance"))
            {
                if (DictionaryItem.SpoilerItem.Contains("=>") || DictionaryItem.SpoilerLocation.Contains("=>")) { continue; }
                var SpoilerLogLine = FileContent.Find(x => (x.Split(new string[] { "->" }, StringSplitOptions.None)[0]) == DictionaryItem.SpoilerLocation);
                if (SpoilerLogLine == null)
                {
                    Console.WriteLine($"===========================================================");
                    Console.WriteLine($"{DictionaryItem.SpoilerLocation} Was not found");
                    var EntrancePair = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == DictionaryItem.EntrancePair);
                    if (EntrancePair == null || !CoupledEntrances) 
                    {
                        Console.WriteLine($"{DictionaryItem.SpoilerLocation} Did not have a pair. Setting it vanilla.");
                        FileContent.Add(DictionaryItem.SpoilerLocation + "->" + DictionaryItem.SpoilerItem); 
                    }
                    else
                    {
                        var EntrancePairSpoilerLogEntry = FileContent.Find(x => (x.Split(new string[] { "->" }, StringSplitOptions.None)[1]) == EntrancePair.SpoilerItem && !IsOneWay(x));
                        if (EntrancePairSpoilerLogEntry != null)
                        {
                            Console.WriteLine($"{DictionaryItem.SpoilerLocation} Reverse Data found at {EntrancePairSpoilerLogEntry}");
                            var g = EntrancePairSpoilerLogEntry.Split(new string[] { "->" }, StringSplitOptions.None);
                            var h0 = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerLocation == g[0] && !x.SpoilerLocation.Contains("Spawn"));
                            var j0 = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerItem == g[1] && !x.SpoilerLocation.Contains("Spawn"));
                            var h = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == h0.EntrancePair);
                            var j = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == j0.EntrancePair);

                            if (h == null || j == null) { Console.WriteLine($"{EntrancePairSpoilerLogEntry} Did not have reverse Data! This is an error!"); continue; }

                            FileContent.Add(j.SpoilerLocation + "->" + h.SpoilerItem);
                        }
                        else
                        {
                            Console.WriteLine($"{DictionaryItem.SpoilerLocation} Did not have reverse Data. Setting it Vanilla.");
                            FileContent.Add(DictionaryItem.SpoilerLocation + "->" + DictionaryItem.SpoilerItem);
                        }
                    }
                }
            }

            foreach (dynamic i in array.dungeons)
            {
                string line = i.ToString();
                Console.WriteLine(line);
                var LineSplit = line.Split(':').Select(x => x.Replace("\"", "").Trim()).ToArray();
                if (line.Length < 2) { continue; }
                Console.WriteLine($"{LineSplit[0]}: {LineSplit[1] == "mq"}");

                if (LineSplit[1] == "mq")
                {
                    switch (LineSplit[0])
                    {
                        case "Deku Tree":
                            ConvertToMQ("Deku Tree Lobby", "Deku Tree Lobby");
                            break;
                        case "Dodongos Cavern":
                            ConvertToMQ("Dodongos Cavern Beginning", "Dodongos Cavern Beginning");
                            break;
                        case "Jabu Jabus Belly":
                            ConvertToMQ("Jabu Jabus Belly Beginning", "Jabu Jabus Belly Beginning");
                            break;
                        case "Bottom of the Well":
                            ConvertToMQ("Bottom of the Well", "Bottom of the Well");
                            break;
                        case "Ice Cavern":
                            ConvertToMQ("Ice Cavern Beginning", "Ice Cavern Beginning");
                            break;
                        case "Gerudo Training Grounds":
                            ConvertToMQ("Gerudo Training Grounds Lobby", "Gerudo Training Grounds Lobby");
                            break;
                        case "Forest Temple":
                            ConvertToMQ("Forest Temple Lobby", "Forest Temple Lobby");
                            break;
                        case "Fire Temple":
                            ConvertToMQ("Fire Temple Lower", "Fire Temple Lower");
                            break;
                        case "Water Temple":
                            ConvertToMQ("Water Temple Lobby", "Water Temple Lobby");
                            break;
                        case "Spirit Temple":
                            ConvertToMQ("Spirit Temple Lobby", "Spirit Temple Lobby");
                            break;
                        case "Shadow Temple":
                            ConvertToMQ("Shadow Temple Entryway", "Shadow Temple Entryway");
                            break;
                        case "Ganons Castle":
                            ConvertToMQ("Ganons Castle", "Ganons Castle");
                            break;
                    }
                }

            }

            void ConvertToMQ(string Entrance, string exit)
            {
                for (var k = 0; k < FileContent.Count(); k++)
                {
                    var Line = FileContent[k];
                    var i = new entranceTable();
                    if (Line.Split(new string[] { "->" }, StringSplitOptions.None).Length < 2) { continue; }
                    var Front = Line.Split(new string[] { "->" }, StringSplitOptions.None)[0];
                    var back = Line.Split(new string[] { "->" }, StringSplitOptions.None)[1];

                    i.Entrance = Front.Split('>')[0];
                    i.To = Front.Split('>')[1];
                    i.Exit = back.Split('<')[0];
                    i.From = (back.Split('<').Length > 1) ? back.Split('<')[1] : "";

                    if (i.Entrance == Entrance)
                    {
                        Console.WriteLine($"Changing {i.Entrance} to MQ {i.Entrance}");
                        i.Entrance = "MQ " + i.Entrance;
                    }
                    if (i.Exit == exit)
                    {
                        i.Exit = "MQ " + i.Exit;
                    }
                    FileContent[k] = ($"{i.Entrance}>{i.To}->{i.Exit}" + (i.From == "" ? "" : $"<{i.From}"));
                }
            }

            //Handle Items
            Dictionary<string, string> ItemNames = new Dictionary<string, string>();
            foreach (dynamic item in array.locations)
            {
                string line = item.ToString();
                line = line.Replace(',', ':');
                line = line.Replace('"', '{');
                line = line.Replace("{", "");
                line = line.Replace("}", "");
                var lines = line.Split(':');
                for (var i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Trim();
                }

                if (lines[1].Contains("item"))
                {
                    var Name = lines[2].Trim();

                    if (Name.Contains("["))
                    {
                        var ind = Name.IndexOf("[");
                        Name = Name.Substring(0, ind).Trim();
                    }

                    ItemNames.Add(lines[0].Trim(), Name);
                }
                else
                {
                    ItemNames.Add(lines[0].Trim(), lines[1].Trim());
                }

            }

            foreach (var i in ItemNames)
            {
                FileContent.Add($"{i.Key}->{i.Value}");
            }

            

            foreach (var i in LogicObjects.MainTrackerInstance.LogicDictionary.Where(x => x.ItemSubType == "Item" 
                                                                                        || x.ItemSubType == "Boss Token" 
                                                                                        || x.ItemSubType == "AgeIndicator" 
                                                                                        || x.ItemSubType.Contains("Setting")))
            {
                if (string.IsNullOrWhiteSpace(i.LocationName)) { continue; }
                var e = FileContent.Find(x => (x.Split(new string[] { "->" }, StringSplitOptions.None)[0]) == i.SpoilerLocation);
                if (e == null)
                {
                    #region ApplySettings
                    if (i.DictionaryName == "Temple of Time Light Arrow Cutscene" && GanonKeyOnLACS)
                    {
                        FileContent.Add("ToT Light Arrows Cutscene->Boss Key (Ganons Castle)");
                    }
                    else if (i.DictionaryName == "SettingStartingAge")
                    {
                        switch (StartAsChild)
                        {
                            case true:
                                FileContent.Add($"SettingStartingAge->SettingStartingAgeChild");
                                break;
                            case false:
                                FileContent.Add($"SettingStartingAge->SettingStartingAgeAdult");
                                break;
                        }
                    }
                    else if (i.DictionaryName == "SettingForest")
                    {
                        switch (Forest)
                        {
                            case 0:
                                FileContent.Add($"SettingForest->SettingForestOpenForest");
                                break;
                            case 1:
                                FileContent.Add($"SettingForest->SettingForestClosedDeku");
                                break;
                            case 2:
                                FileContent.Add($"SettingForest->SettingForestClosedForest");
                                break;
                        }
                    }
                    else if (i.DictionaryName == "SettingDoorOfTime")
                    {
                        switch (OpenDOT)
                        {
                            case true:
                                FileContent.Add($"SettingDoorOfTime->SettingDoorOfTimeOpen");
                                break;
                            case false:
                                FileContent.Add($"SettingDoorOfTime->SettingDoorOfTimeClosed");
                                break;
                        }
                    }
                    else if (i.DictionaryName == "SettingKakarikoGate")
                    {
                        switch (Kakariko)
                        {
                            case 0:
                                FileContent.Add($"SettingKakarikoGate->SettingKakarikoGateOpenGate");
                                break;
                            case 1:
                                FileContent.Add($"SettingKakarikoGate->SettingKakarikoGateZeldasLetter");
                                break;
                            case 2:
                                FileContent.Add($"SettingKakarikoGate->SettingKakarikoGateClosedGate");
                                break;
                        }
                    }
                    else if (i.DictionaryName == "SettingZorasFountain")
                    {
                        switch (Zora)
                        {
                            case 0:
                                FileContent.Add($"SettingZorasFountain->SettingZorasFountainClosed");
                                break;
                            case 1:
                                FileContent.Add($"SettingZorasFountain->SettingZorasFountainAdult");
                                break;
                            case 2:
                                FileContent.Add($"SettingZorasFountain->SettingZorasFountainOpen");
                                break;
                        }
                    }
                    else if (i.DictionaryName == "SettingGerudoFortress")
                    {
                        switch (Gerudo)
                        {
                            case 0:
                                FileContent.Add($"SettingGerudoFortress->SettingGerudoFortressDefaultBehavior");
                                break;
                            case 1:
                                FileContent.Add($"SettingGerudoFortress->SettingGerudoFortressRescueOne");
                                break;
                            case 2:
                                FileContent.Add($"SettingGerudoFortress->SettingGerudoFortressOpen");
                                break;
                        }
                    }
                    else if (i.DictionaryName == "SettingRainbowBridge")
                    {
                        switch (bridge)
                        {
                            case 0:
                                FileContent.Add($"SettingRainbowBridge->SettingRainbowBridgeAlwaysOpen");
                                break;
                            case 1:
                                FileContent.Add($"SettingRainbowBridge->SettingRainbowBridgeVanilla");
                                break;
                            case 2:
                                FileContent.Add($"SettingRainbowBridge->SettingRainbowBridgeStone");
                                break;
                            case 3:
                                FileContent.Add($"SettingRainbowBridge->SettingRainbowBridgeMedallions");
                                break;
                            case 4:
                                FileContent.Add($"SettingRainbowBridge->SettingRainbowBridgeAllDungeons");
                                break;
                            case 5:
                                FileContent.Add($"SettingRainbowBridge->SettingRainbowBridgeSkullTokens");
                                break;
                        }
                    }
                    else if (i.DictionaryName == "SettingBombchusInLogic")
                    {
                        switch (Chu)
                        {
                            case true:
                                FileContent.Add($"SettingBombchusInLogic->SettingBombchusInLogicTrue");
                                break;
                            case false:
                                FileContent.Add($"SettingBombchusInLogic->SettingBombchusInLogicFalse");
                                break;
                        }
                    }
                    else if (i.DictionaryName == "SettingNighSkullSunSong")
                    {
                        switch (SunSong)
                        {
                            case true:
                                FileContent.Add($"SettingNighSkullSunSong->SettingNighSkullSunSongTrue");
                                break;
                            case false:
                                FileContent.Add($"SettingNighSkullSunSong->SettingNighSkullSunSongFalse");
                                break;
                        }
                    }
                    else if (i.DictionaryName == "SettingCompleteMaskQuest")
                    {
                        switch (mask)
                        {
                            case true:
                                FileContent.Add($"SettingCompleteMaskQuest->SettingCompleteMaskQuestTrue");
                                break;
                            case false:
                                FileContent.Add($"SettingCompleteMaskQuest->SettingCompleteMaskQuestFalse");
                                break;
                        }
                    }
                    else if (i.DictionaryName == "SettingFreeScarecrow")
                    {
                        switch (Scarecrow)
                        {
                            case true:
                                FileContent.Add($"SettingFreeScarecrow->SettingFreeScarecrowTrue");
                                break;
                            case false:
                                FileContent.Add($"SettingFreeScarecrow->SettingFreeScarecrowFalse");
                                break;
                        }
                    }
                    else
                    {
                        FileContent.Add($"{i.SpoilerLocation}->{i.SpoilerItem}");
                    }
                    #endregion ApplySettings
                }
            }

            if (ManualConvert)
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Spoiler Log (*.txt)|*.txt",
                    FilterIndex = 1,
                    FileName = filename.Replace(".json", " MMRT Converted")
                };
                if (saveDialog.ShowDialog() != DialogResult.OK) { return null; }

                File.WriteAllLines(saveDialog.FileName, FileContent);
            }
            return (ManualConvert) ? null : FileContent.ToArray();

        }

        public static string[] HandleWWRSpoilerLog(string[] Log = null)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            void CreateDictionary()
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                string webData = wc.DownloadString("https://raw.githubusercontent.com/LagoLunatic/wwrando/9f4752b6defbc5849ef52044422109b36f2ad790/logic/item_locations.txt");
                string[] Lines = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                List<string> DictionaryLines = new List<string> { "DictionaryName,LocationName,ItemName,LocationArea,ItemSubType,SpoilerLocation,SpoilerItem,EntrancePair" };
                LogicObjects.LogicDictionaryEntry Item = new LogicObjects.LogicDictionaryEntry();

                foreach (var i in Lines)
                {
                    string Line = i.Trim();
                    //Console.WriteLine(Line);
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
                        Item.SpoilerLocation = Item.LocationName;
                        Item.DictionaryName = Item.LocationArea + " " + rgx.Replace(Parts[1].Trim(), "").Replace("-", "");
                        Item.LocationName = (Unimplimented) ? "" : Item.LocationName;
                    }
                    if (Line.Contains("Original item:"))
                    {
                        Item.ItemName = Line.Split(':')[1].Trim();
                        Item.SpoilerItem = Item.ItemName;
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

            void CreateWWRLogicFile()
            {
                List<LogicObjects.LogicDictionaryEntry> LogicDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDictionaryEntry>>(Utility.ConvertCsvFileToJsonObject("Recources\\Dictionaries\\WWRDICTIONARYV170.csv"));
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

            //CreateWWRLogicFile(); return;

            bool ManualConvert = (Log == null);
            string filename = "";
            if (ManualConvert)
            {
                OpenFileDialog SelectedFile = new OpenFileDialog
                {
                    Title = $"Select WWR Spoiler Log",
                    Filter = "OOTR Spoiler Log (*.txt)|*.txt",
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
            for (var i = 0; i< FileContent.Count(); i++)
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
            return (ManualConvert) ? null : SpoilerData.ToArray();

        }

    }
}
