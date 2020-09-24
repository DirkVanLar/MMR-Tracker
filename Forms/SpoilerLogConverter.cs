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
            //comboBox1.Items.Add("Wind Waker Rando");
            //comboBox1.Items.Add("Twilight Princess Rando");
            //comboBox1.Items.Add("Link to the past Rando");
            //comboBox1.Items.Add("Minish Cap Rando");
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 1) { HandleOOTRSpoilerLog(); }
        }

        private void HandleOOTRSpoilerLog()
        {
            var dic = LogicObjects.MainTrackerInstance.LogicDictionary;

            if (dic.Count < 1 || LogicObjects.MainTrackerInstance.GameCode != "OOTR")
            {
                MessageBox.Show("You must first import an OOTR Logic File");
                return;
            }

            OpenFileDialog SelectedFile = new OpenFileDialog
            {
                Title = $"Select OOTR Spoiler Log",
                Filter = "OOTR Spoiler Log (*.json)|*.json",
                FilterIndex = 1,
                Multiselect = false
            };
            if (SelectedFile.ShowDialog() != DialogResult.OK) { return; }

            //Handle Entraces
            List<entranceTable> SpoilerEntranceTable = new List<entranceTable>();
            dynamic array = JsonConvert.DeserializeObject(File.ReadAllText(SelectedFile.FileName));
            foreach (dynamic item in array.entrances)
            {
                var entry = new entranceTable();
                string line = item.ToString();
                line = line.Replace(',', ':');
                line = line.Replace('"', '{');
                line = line.Replace("{", "");
                line = line.Replace("}", "");
                var lines = line.Split(':').Select(x => x.Trim()).ToArray();
                var front = lines[0].Split(new string[] { "->" }, StringSplitOptions.None);
                entry.Entrance = front[0].Trim();
                entry.To = front[1].Trim();
                entry.Exit = (lines[1].Contains("region")) ? lines[2].Trim() : lines[1].Trim();
                entry.From = (lines[1].Contains("region")) ? lines[4].Trim() : "";
                SpoilerEntranceTable.Add(entry);
            }

            //Check the MQ entry in the spoiler log and convert the entries of any dungeon that are MQ to the MQ entry
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
                foreach (var i in SpoilerEntranceTable)
                {
                    if (i.Entrance == Entrance)
                    {
                        Console.WriteLine($"Changing {i.Entrance} to MQ {i.Entrance}");
                        i.Entrance = "MQ " + i.Entrance;
                    }
                    if (i.Exit == exit)
                    {
                        i.Exit = "MQ " + i.Exit;
                    }
                }
            }

            List<string> FileContent = new List<string>();

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
            foreach (var i in LogicObjects.MainTrackerInstance.LogicDictionary.Where(x => x.ItemSubType == "Entrance"))
            {
                var e = FileContent.Find(x => (x.Split(new string[] { "->" }, StringSplitOptions.None)[0]) == i.SpoilerLocation);
                if (e == null)
                {
                    Console.WriteLine($"===========================================================");
                    Console.WriteLine($"{i.SpoilerLocation} Was not found");
                    var reverse = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == i.EntrancePair);
                    if (reverse == null) 
                    {
                        Console.WriteLine($"{i.SpoilerLocation} Did not have a pair. Setting it vanilla.");
                        FileContent.Add(i.SpoilerLocation + "->" + i.SpoilerItem); 
                    }
                    else
                    {
                        var f = FileContent.Find(x => (x.Split(new string[] { "->" }, StringSplitOptions.None)[1]) == reverse.SpoilerItem && !IsOneWay(x));
                        if (f != null)
                        {
                            Console.WriteLine($"{i.SpoilerLocation} Reverse Data found at {f}");
                            var g = f.Split(new string[] { "->" }, StringSplitOptions.None);
                            var h0 = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerLocation == g[0] && !x.SpoilerLocation.Contains("Spawn"));
                            var j0 = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerItem == g[1] && !x.SpoilerLocation.Contains("Spawn"));
                            var h = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == h0.EntrancePair);
                            var j = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == j0.EntrancePair);

                            if (h == null || j == null) { Console.WriteLine($"{f} Did not have reverse Data! This is an error!"); continue; }

                            FileContent.Add(j.SpoilerLocation + "->" + h.SpoilerItem);
                        }
                        else
                        {
                            Console.WriteLine($"{i.SpoilerLocation} Did not have reverse Data. Setting it Vanilla.");
                            FileContent.Add(i.SpoilerLocation + "->" + i.SpoilerItem);
                        }
                    }
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
                    ItemNames.Add(lines[0].Trim(), lines[2].Trim());
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

            bool GanonKeyOnLACS = false;
            bool StartAsChild = false;
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
            foreach (dynamic item in array.settings)
            {
                string line = item.ToString();
                if (line.Contains("shuffle_ganon_bosskey") && line.Contains("lacs")) { GanonKeyOnLACS = true; }
                if (line.Contains("starting_age") && line.Contains("child")) { StartAsChild = true; }
                if (line.Contains("open_forest") && line.Contains("closed_deku")) { Forest = 1; }
                if (line.Contains("open_forest") && line.Contains("closed_forest")) { Forest = 2; }
                if (line.Contains("open_kakariko") && line.Contains("zelda")) { Kakariko = 1; }
                if (line.Contains("open_kakariko") && line.Contains("closed")) { Kakariko = 2; }
                if (line.Contains("open_door_of_time") && line.Contains("closed")) { OpenDOT = false; }
                if (line.Contains("zora_fountain") && line.Contains("adult")) { Zora = 1; }
                if (line.Contains("zora_fountain") && line.Contains("closed")) { Zora = 2; }
                if (line.Contains("gerudo_fortress") && line.Contains("one")) { Gerudo = 1; }
                if (line.Contains("gerudo_fortress") && line.Contains("open")) { Gerudo = 2; }
                if (line.Contains("bridge") && line.Contains("vanilla")) { bridge = 1; }
                if (line.Contains("bridge") && line.Contains("stone")) { bridge = 2; }
                if (line.Contains("bridge") && line.Contains("medallion")) { bridge = 3; }
                if (line.Contains("bridge") && line.Contains("dungeon")) { bridge = 4; }
                if (line.Contains("bridge") && line.Contains("skull")) { bridge = 5; }
                if (line.Contains("bombchus_in_logic") && line.Contains("true")) { Chu = true; }
                if (line.Contains("logic_no_night_tokens_without_suns_song") && line.Contains("true")) { SunSong = true; }
                if (line.Contains("complete_mask_quest") && line.Contains("true")) { mask = true; }
                if (line.Contains("free_scarecrow") && line.Contains("true")) { mask = true; }

            }

            foreach (var i in LogicObjects.MainTrackerInstance.LogicDictionary.Where(x => x.ItemSubType == "Item" || x.ItemSubType == "Boss Token" || x.ItemSubType == "AgeIndicator" || x.ItemSubType.Contains("Setting")))
            {
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
                        string age = (StartAsChild) ? "SettingStartingAgeChild" : "SettingStartingAgeAdult";
                        FileContent.Add($"SettingStartingAge->{ age}");
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

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Spoiler Log (*.txt)|*.txt",
                FilterIndex = 1,
                FileName = SelectedFile.FileName.Replace(".json", " MMRT Converted")
            };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return; }

            File.WriteAllLines(saveDialog.FileName, FileContent);
        }

    }
}
