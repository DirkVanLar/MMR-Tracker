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
using static MMR_Tracker.Forms.SpoilerLogConverter;

namespace MMR_Tracker.Forms.Other_Games
{
    class OcarinaOfTimeTools
    {
        public class entranceTable
        {
            public string Entrance { get; set; }
            public string To { get; set; }
            public string Exit { get; set; }
            public string From { get; set; }
        }

        //Get Settings
        public static bool GanonKeyOnLACS = false;
        public static bool StartAsChild = true;
        public static bool Chu = false;
        public static bool SunSong = false;
        public static bool mask = false;
        public static bool Scarecrow = false;
        public static bool CoupledEntrances = true;
        public static bool OpenDOT = true;
        public static int Forest = 0; // 0 = Open, 1 = Closed Deku, 2 = Default
        public static int Kakariko = 0; // 0 = Open, 1 = Letter opens, 2 = Default
        public static int Zora = 0; // 0 = open, 1 = Open as Adult, 2 = Default
        public static int Gerudo = 0; // 0 = Default, 1 = One Carpenter, 2 = open
        public static int bridge = 0; // 0 = open, 1 = Vanilla, 2 = stone, 3 = Meddalion, 4 = All, 5 = Skulls
        public static int DamageMode = 0; // 0 = Other, 1 = 4x, 2 = OHKO
        public static int LACSTrigger = 0; // 0 = Vanilla, 1 = stone, 2 = Meddalion, 3 = All,
        public static bool isMulti = false;
        public static int worlCount = 1;
        public static List<string> FileContent = new List<string>();

        public static dynamic SettingsArray;
        public static dynamic EntranceArray;
        public static dynamic DungeonArray;
        public static dynamic ItemsArray;

        public static string[] HandleOOTRSpoilerLog(string Log = "")
        {

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
            FileContent.Add("Converted OOTR");

            SettingsArray = array.settings;
            EntranceArray = array.entrances;
            DungeonArray = array.dungeons;
            ItemsArray = array.locations;

            ConvertMultiWorldLog();
            ReadSettings();
            HandleEntrances();
            ConvertMQEntrances();
            HandleItems();
            AddSettings();

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
            return FileContent.ToArray();

            #region DoStuffIDontWantToLookAt
            
            #endregion DoStuffIDontWantToLookAt
        }

        public static void CreateOOTRLogicFile()
        {
            var Lines = File.ReadAllLines("Recources\\Dictionaries\\OOTRDICTIONARYV5.csv");
            List<LogicObjects.LogicDictionaryEntry> LogicDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDictionaryEntry>>(Utility.ConvertCsvFileToJsonObject(Lines));
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

        public static void ReadSettings()
        {
            foreach (dynamic item in SettingsArray)
            {
                string line = item.ToString();
                if (line.Contains("shuffle_ganon_bosskey"))
                {
                    if (line.Contains("lacs")) { GanonKeyOnLACS = true; }

                    if (line.Contains("lacs_stones")) { LACSTrigger = 1; }
                    else if (line.Contains("lacs_medallions")) { LACSTrigger = 2; }
                    else if (line.Contains("lacs_dungeons")) { LACSTrigger = 3; }
                }
                if (line.Contains("starting_age") && line.Contains("adult")) { StartAsChild = false; }
                if (line.Contains("open_door_of_time") && line.Contains("false")) { OpenDOT = false; }
                if (line.Contains("bombchus_in_logic") && line.Contains("true")) { Chu = true; }
                if (line.Contains("logic_no_night_tokens_without_suns_song") && line.Contains("true")) { SunSong = true; }
                if (line.Contains("complete_mask_quest") && line.Contains("true")) { mask = true; }
                if (line.Contains("free_scarecrow") && line.Contains("true")) { Scarecrow = true; }
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
                if (line.Contains("damage_multiplier"))
                {
                    if (line.Contains("quadruple")) { DamageMode = 1; }
                    else if (line.Contains("ohko")) { DamageMode = 2; }
                }
            }
        }

        public static void HandleEntrances()
        {//Handle Entraces
            List<entranceTable> SpoilerEntranceTable = new List<entranceTable>();
            foreach (dynamic item in EntranceArray)
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


            foreach (var i in SpoilerEntranceTable)
            {
                FileContent.Add($"{i.Entrance}>{i.To}->{i.Exit}" + (i.From == "" ? "" : $"<{i.From}"));
            }

            bool IsOneWay(string i)
            {
                var j = i.Split(new string[] { "->" }, StringSplitOptions.None)[0];
                var k = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerLocation.Split('|').Contains(j));
                return string.IsNullOrWhiteSpace(k.EntrancePair) || i.Contains("Adult Spawn") || i.Contains("Child Spawn");
            }

            //Attempt to add Entrances that were left out of the spoiler log
            foreach (var DictionaryItem in LogicObjects.MainTrackerInstance.LogicDictionary.Where(x => x.ItemSubType == "Entrance"))
            {

                if (DictionaryItem.SpoilerItem.Split('|').Where(x => x.Contains("=>")).Any() || DictionaryItem.SpoilerLocation.Split('|').Where(x => x.Contains("=>")).Any()) { continue; }
                var SpoilerLogLine = FileContent.Find(x => DictionaryItem.SpoilerLocation.Split('|').Contains((x.Split(new string[] { "->" }, StringSplitOptions.None)[0])));
                if (SpoilerLogLine == null)
                {
                    Debugging.Log($"===========================================================");
                    Debugging.Log($"{DictionaryItem.SpoilerLocation.Split('|')[0]} Was not found");
                    var EntrancePair = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == DictionaryItem.EntrancePair);
                    if (EntrancePair == null || !CoupledEntrances)
                    {
                        Debugging.Log($"{DictionaryItem.SpoilerLocation.Split('|')[0]} Did not have a pair. Setting it vanilla.");
                        FileContent.Add(DictionaryItem.SpoilerLocation.Split('|')[0] + "->" + DictionaryItem.SpoilerItem.Split('|')[0]);
                    }
                    else
                    {
                        var EntrancePairSpoilerLogEntry =
                            FileContent.Where(x => x.Contains("->")).ToList()
                            .Find(x => EntrancePair.SpoilerItem.Split('|').Contains(x.Split(new string[] { "->" }, StringSplitOptions.None)[1]) && !IsOneWay(x));
                        if (EntrancePairSpoilerLogEntry != null)
                        {
                            Debugging.Log($"{DictionaryItem.SpoilerLocation.Split('|')[0]} Reverse Data found at {EntrancePairSpoilerLogEntry}");
                            var g = EntrancePairSpoilerLogEntry.Split(new string[] { "->" }, StringSplitOptions.None);
                            var h0 = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerLocation.Split('|').Contains(g[0])
                                && !x.SpoilerLocation.Split('|').Where(o => o.Contains("Spawn")).Any());
                            var j0 = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.SpoilerItem.Split('|').Contains(g[1])
                                && !x.SpoilerLocation.Split('|').Where(o => o.Contains("Spawn")).Any());
                            var h = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == h0.EntrancePair);
                            var j = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == j0.EntrancePair);

                            if (h == null || j == null) { Debugging.Log($"{EntrancePairSpoilerLogEntry} Did not have reverse Data! This is an error!"); continue; }

                            Debugging.Log($"New Entry Created: {j.SpoilerLocation.Split('|')[0] + "->" + h.SpoilerItem.Split('|')[0]}");

                            FileContent.Add(j.SpoilerLocation.Split('|')[0] + "->" + h.SpoilerItem.Split('|')[0]);
                        }
                        else
                        {
                            Debugging.Log($"{DictionaryItem.SpoilerLocation.Split('|')[0]} Did not have reverse Data. Setting it Vanilla.");
                            FileContent.Add(DictionaryItem.SpoilerLocation.Split('|')[0] + "->" + DictionaryItem.SpoilerItem.Split('|')[0]);
                        }
                    }
                }
            }
        }

        public static void ConvertMQEntrances()
        {//Check the MQ entry in the spoiler log and convert the entries of any dungeon that are MQ to the MQ entry
            foreach (dynamic i in DungeonArray)
            {
                string line = i.ToString();
                //Debugging.Log(line);
                var LineSplit = line.Split(':').Select(x => x.Replace("\"", "").Trim()).ToArray();
                if (line.Length < 2) { continue; }
                //Debugging.Log($"{LineSplit[0]}: {LineSplit[1] == "mq"}");

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
                        //Debugging.Log($"Changing {i.Entrance} to MQ {i.Entrance}");
                        i.Entrance = "MQ " + i.Entrance;
                    }
                    if (i.Exit == exit)
                    {
                        i.Exit = "MQ " + i.Exit;
                    }
                    FileContent[k] = ($"{i.Entrance}>{i.To}->{i.Exit}" + (i.From == "" ? "" : $"<{i.From}"));
                }
            }
        }

        public static void HandleItems()
        {//Handle Items
            Dictionary<string, string> ItemNames = new Dictionary<string, string>();
            foreach (dynamic item in ItemsArray)
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
                    if (isMulti && lines.Count() > 4) { Name = Name + "$" + lines[4]; }
                    FileContent.Add($"{lines[0].Trim()}->{Name}");
                }
                else { FileContent.Add($"{lines[0].Trim()}->{lines[1].Trim()}"); }

            }

            foreach (var i in LogicObjects.MainTrackerInstance.LogicDictionary.Where(x => x.ItemSubType == "Item"
            || x.ItemSubType == "Boss Token"
            || x.ItemSubType == "AgeIndicator"
            || x.DictionaryName == "Temple of Time Light Arrow Cutscene"))
            {
                if (string.IsNullOrWhiteSpace(i.LocationName)) { continue; }
                var e = FileContent.Find(x => i.SpoilerLocation.Split('|').Contains(x.Split(new string[] { "->" }, StringSplitOptions.None)[0]));
                if (e == null)
                {
                    if (i.DictionaryName == "Temple of Time Light Arrow Cutscene" && GanonKeyOnLACS)
                    {
                        FileContent.Add("ToT Light Arrows Cutscene->Boss Key (Ganons Castle)");
                    }
                    else
                    {
                        FileContent.Add($"{i.SpoilerLocation.Split('|')[0]}->{i.SpoilerItem.Split('|')[0]}");
                    }
                }
            }
        }

        public static void AddSettings()
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
            switch (OpenDOT)
            {
                case true:
                    FileContent.Add($"SettingDoorOfTime->SettingDoorOfTimeOpen");
                    break;
                case false:
                    FileContent.Add($"SettingDoorOfTime->SettingDoorOfTimeClosed");
                    break;
            }
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
            switch (Chu)
            {
                case true:
                    FileContent.Add($"SettingBombchusInLogic->SettingBombchusInLogicTrue");
                    break;
                case false:
                    FileContent.Add($"SettingBombchusInLogic->SettingBombchusInLogicFalse");
                    break;
            }
            switch (SunSong)
            {
                case true:
                    FileContent.Add($"SettingNighSkullSunSong->SettingNighSkullSunSongTrue");
                    break;
                case false:
                    FileContent.Add($"SettingNighSkullSunSong->SettingNighSkullSunSongFalse");
                    break;
            }
            switch (mask)
            {
                case true:
                    FileContent.Add($"SettingCompleteMaskQuest->SettingCompleteMaskQuestTrue");
                    break;
                case false:
                    FileContent.Add($"SettingCompleteMaskQuest->SettingCompleteMaskQuestFalse");
                    break;
            }
            switch (Scarecrow)
            {
                case true:
                    FileContent.Add($"SettingFreeScarecrow->SettingFreeScarecrowTrue");
                    break;
                case false:
                    FileContent.Add($"SettingFreeScarecrow->SettingFreeScarecrowFalse");
                    break;
            }
            switch (DamageMode)
            {
                case 0:
                    FileContent.Add($"SettingDamageMode->SettingDamageModeOther");
                    break;
                case 1:
                    FileContent.Add($"SettingDamageMode->SettingDamageMode4X");
                    break;
                case 2:
                    FileContent.Add($"SettingDamageMode->SettingDamageModeOHKO");
                    break;
            }
            switch (LACSTrigger)
            {
                case 0:
                    FileContent.Add($"SettingLACSTrigger->SettingLACSTriggerVanilla");
                    break;
                case 1:
                    FileContent.Add($"SettingLACSTrigger->SettingLACSTriggerStones");
                    break;
                case 2:
                    FileContent.Add($"SettingLACSTrigger->SettingLACSTriggerMedallions");
                    break;
                case 3:
                    FileContent.Add($"SettingLACSTrigger->SettingLACSTriggerDungeons");
                    break;
            }
        }

        public static void AddShopItemPrices()
        {//Add Shop Prices
            string[] ShopChecks = new string[]
            {
                    "KF Shop Item 1", "KF Shop Item 2", "KF Shop Item 3", "KF Shop Item 4", "KF Shop Item 5", "KF Shop Item 6", "KF Shop Item 7", "KF Shop Item 8", "Market Potion Shop Item 1", "Market Potion Shop Item 2", "Market Potion Shop Item 3", "Market Potion Shop Item 4", "Market Potion Shop Item 5", "Market Potion Shop Item 6", "Market Potion Shop Item 7", "Market Potion Shop Item 8", "Market Bombchu Shop Item 1", "Market Bombchu Shop Item 2", "Market Bombchu Shop Item 3", "Market Bombchu Shop Item 4", "Market Bombchu Shop Item 5", "Market Bombchu Shop Item 6", "Market Bombchu Shop Item 7", "Market Bombchu Shop Item 8", "Kak Potion Shop Item 1", "Kak Potion Shop Item 2", "Kak Potion Shop Item 3", "Kak Potion Shop Item 4", "Kak Potion Shop Item 5", "Kak Potion Shop Item 6", "Kak Potion Shop Item 7", "Kak Potion Shop Item 8", "GC Shop Item 1", "GC Shop Item 2", "GC Shop Item 3", "GC Shop Item 4", "GC Shop Item 5", "GC Shop Item 6", "GC Shop Item 7", "GC Shop Item 8", "ZD Shop Item 1", "ZD Shop Item 2", "ZD Shop Item 3", "ZD Shop Item 4", "ZD Shop Item 5", "ZD Shop Item 6", "ZD Shop Item 7", "ZD Shop Item 8", "LW Deku Scrub Near Bridge", "LW Deku Scrub Near Deku Theater Right", "LW Deku Scrub Near Deku Theater Left", "LW Deku Scrub Grotto Rear", "LW Deku Scrub Grotto Front", "SFM Deku Scrub Grotto Rear", "SFM Deku Scrub Grotto Front", "LLR Deku Scrub Grotto Left", "LLR Deku Scrub Grotto Right", "LLR Deku Scrub Grotto Center", "GC Deku Scrub Grotto Left", "GC Deku Scrub Grotto Right", "GC Deku Scrub Grotto Center", "DMC Deku Scrub Grotto Left", "DMC Deku Scrub Grotto Right", "DMC Deku Scrub Grotto Center", "ZR Deku Scrub Grotto Rear", "ZR Deku Scrub Grotto Front", "LH Deku Scrub Grotto Left", "LH Deku Scrub Grotto Right", "LH Deku Scrub Grotto Center", "Colossus Deku Scrub Grotto Rear", "Colossus Deku Scrub Grotto Front", "GV Deku Scrub Grotto Rear", "GV Deku Scrub Grotto Front", "Dodongos Cavern Deku Scrub Side Room Near Dodongos", "Dodongos Cavern Deku Scrub Lobby", "Dodongos Cavern Deku Scrub Near Bomb Bag Right", "Dodongos Cavern Deku Scrub Near Bomb Bag Left", "Jabu Jabus Belly Deku Scrub", "Ganons Castle Deku Scrub Center-Left", "Ganons Castle Deku Scrub Center-Right", "Ganons Castle Deku Scrub Right", "Ganons Castle Deku Scrub Left", "Dodongos Cavern MQ Deku Scrub Lobby Rear", "Dodongos Cavern MQ Deku Scrub Lobby Front", "Dodongos Cavern MQ Deku Scrub Staircase", "Dodongos Cavern MQ Deku Scrub Side Room Near Lower Lizalfos", "Ganons Castle MQ Deku Scrub Center-Left", "Ganons Castle MQ Deku Scrub Center", "Ganons Castle MQ Deku Scrub Center-Right", "Ganons Castle MQ Deku Scrub Left", "Ganons Castle MQ Deku Scrub Right", "Ganons Castle MQ Forest Trial Eye Switch Chest"
            };

            List<string> LocationList = new List<string>();
            foreach (var i in ItemsArray) { LocationList.Add(i.ToString()); }

            foreach (var i in ShopChecks)
            {
                var ShopLine = LocationList.Find(x => x.Contains(i));
                if (ShopLine == null) { continue; }
                try
                {
                    int PriceRange = GetPriceRange(Int32.Parse(ShopLine.Split(',')[1].Split(':')[1].Replace("}", "").Trim()));
                    FileContent.Add($"{i} Price->{i} Price {PriceRange}");
                }
                catch { }
            }

            int GetPriceRange(int price)
            {
                if (price < 100) { return 1; }
                if (price < 201) { return 2; }
                if (price < 501) { return 3; }
                return 4;
            }
        }

        public static void ConvertMultiWorldLog()
        {
            int MyplayerID = LogicObjects.MainTrackerInstance.Options.MyPlayerID;
            foreach (dynamic item in SettingsArray)
            {
                string line = item.ToString();
                if (line.Contains("\"world_count\":") && int.TryParse(line.Replace("\"world_count\":", "").Replace(",", "").Trim(), out worlCount) && worlCount > 1)
                {
                    isMulti = true;
                    break;
                }
            }
            Debugging.Log($"Multiworld {isMulti}, World Count: {worlCount}");

            if (isMulti)
            {
                if (MyplayerID < 0 || MyplayerID > worlCount || !LogicObjects.MainTrackerInstance.Options.IsMultiWorld)
                {
                    MessageBox.Show("The selected logic file is a multiworld Logic file. Multiworld is either not enabled in your tracker or your player ID was not found in the spoiler log. Multiworld data will not be imported", "Multiworld Invalid!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    MyplayerID = 1;
                    isMulti = false;
                }
                string entranceLine = "";
                foreach (dynamic i in EntranceArray)
                {
                    string j = i.ToString();
                    if (j.Trim().StartsWith($"\"World {MyplayerID}\"")) { entranceLine = j.Replace(System.Environment.NewLine, "").Replace($"\"World {MyplayerID}\":", ""); }
                }
                if (entranceLine != "") { EntranceArray = JsonConvert.DeserializeObject(entranceLine); }
                string DungeonLine = "";
                foreach (dynamic i in DungeonArray)
                {
                    string j = i.ToString();
                    if (j.Trim().StartsWith($"\"World {MyplayerID}\"")) { DungeonLine = j.Replace(System.Environment.NewLine, "").Replace($"\"World {MyplayerID}\":", ""); }
                }
                if (DungeonLine != "") { DungeonArray = JsonConvert.DeserializeObject(DungeonLine); }
                string ItemLine = "";
                foreach (dynamic i in ItemsArray)
                {
                    string j = i.ToString();
                    if (j.Trim().StartsWith($"\"World {MyplayerID}\"")) { ItemLine = j.Replace(System.Environment.NewLine, "").Replace($"\"World {MyplayerID}\":", ""); }
                }
                if (ItemLine != "") { ItemsArray = JsonConvert.DeserializeObject(ItemLine); }
            }

        }
    }

    class OcarinaOfTimeToolsLogicCreation
    {

        public class RegionData
        {
            public string Region { get; set; } = "";
            public Dictionary<string, string> Events { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> Locations { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> Exits { get; set; } = new Dictionary<string, string>();
        }

        public static void Testing()
        {
            //var DataPath = @"D:\Visual Studio Code Stuff\MMR TRACKER V2\MMR Tracker V2\Recources\Other Junk\OOTR Logic Data\World";
            var DataPath = @"C:\Users\ttalbot\Downloads\data\data";

            var WorldData = DataPath + @"\World";

            var MainWorldFiles = Directory.GetFiles(WorldData, "*.json").Where(x => !x.Contains("MQ")).SelectMany(f => File.ReadLines(f).Concat(new[] { Environment.NewLine }));

            var MQWorldFiles = Directory.GetFiles(WorldData, "*.json").Where(x => x.Contains("MQ")).SelectMany(f => File.ReadLines(f).Concat(new[] { Environment.NewLine }));

            var LogiHelperFile = File.ReadAllLines(DataPath + @"\LogicHelpers.json");

            var AllRegionData = new List<RegionData>();

            RegionData CurrentRegion = null;

            bool InRegion = false;
            bool inLocations = false;
            bool InExits = false;
            bool AtEndORegion = false;
            bool InEvents = false;

            string CurrentEventLine = "";
            string CurrentLogicLine = "";
            string CurrentExitLine = "";

            List<string> UsedNames = new List<string>();
            List<string> DuplicateName = new List<string>();

            HAndleMianFiles();
            HandleMQFiles();
            HandleLogicHelpers();
            //PrintData();
            //PrintDuplicate();
            PRintLogicItems();

            void PRintLogicItems()
            {
                foreach(var i in GetItemsUsedInLogic(AllRegionData).Distinct()) { Console.WriteLine(i); }
            }

            void HAndleMianFiles()
            {
                foreach (var i in MainWorldFiles)
                {
                    if (TestPosition(i)) { continue; }

                    string CleanedLine = RemoveComments(i);

                    if (InEvents)
                    {
                        if (CleanedLine.Contains(":") && CurrentEventLine != "")
                        {
                            var logiclinesplit = CleanLine(CurrentEventLine, CurrentRegion.Region).Split(':');
                            if (logiclinesplit.Count() > 1) { CurrentRegion.Events.Add(logiclinesplit[0].Trim(), "(" + logiclinesplit[1].Trim()); }
                            CurrentEventLine = "";
                        }
                        CurrentEventLine += CleanedLine;
                    }

                    if (inLocations)
                    {
                        if (CleanedLine.Contains(":") && CurrentLogicLine != "")
                        {
                            var logiclinesplit = CleanLine(CurrentLogicLine, CurrentRegion.Region).Split(':');
                            if (logiclinesplit.Count() > 1) { CurrentRegion.Locations.Add(logiclinesplit[0].Trim(), "(" + logiclinesplit[1].Trim()); }
                            CurrentLogicLine = "";
                        }
                        CurrentLogicLine += CleanedLine;
                    }

                    if (InExits)
                    {
                        if (CleanedLine.Contains(":") && CurrentExitLine != "")
                        {
                            var logiclinesplit = CleanLine(CurrentExitLine, CurrentRegion.Region, true).Split(':');
                            if (logiclinesplit.Count() > 1) { CurrentRegion.Exits.Add($"{CurrentRegion.Region}>{logiclinesplit[0].Trim()}", "(" + logiclinesplit[1].Trim()); }
                            CurrentExitLine = "";
                        }
                        CurrentExitLine += CleanedLine;
                    }

                    if (AtEndORegion)
                    {
                        if (CurrentRegion != null)
                        {
                            AllRegionData.Add(CurrentRegion);
                            CurrentRegion = null;
                        }
                    }
                }
                InRegion = false;
                inLocations = false;
                InExits = false;
                AtEndORegion = false;
                InEvents = false;
                CurrentEventLine = "";
                CurrentLogicLine = "";
                CurrentExitLine = "";
                CurrentRegion = null;
            }
            
            void HandleMQFiles()
            {
                foreach (var i in MQWorldFiles)
                {
                    if (TestPosition(i, true)) { continue; }

                    string CleanedLine = RemoveComments(i);

                    if (InEvents)
                    {
                        if (CleanedLine.Contains(":") && CurrentEventLine != "")
                        {
                            var logiclinesplit = CleanLine(CurrentEventLine, CurrentRegion.Region).Split(':');
                            if (logiclinesplit.Count() > 1) { CurrentRegion.Events.Add(logiclinesplit[0].Trim(), "(" + logiclinesplit[1].Trim()); }
                            CurrentEventLine = "";
                        }
                        CurrentEventLine += CleanedLine;
                    }

                    if (inLocations)
                    {
                        if (CleanedLine.Contains(":") && CurrentLogicLine != "")
                        {
                            var logiclinesplit = CleanLine(CurrentLogicLine, CurrentRegion.Region).Split(':');
                            if (logiclinesplit.Count() > 1) { CurrentRegion.Locations.Add(logiclinesplit[0].Trim(), "(" + logiclinesplit[1].Trim()); }
                            CurrentLogicLine = "";
                        }
                        CurrentLogicLine += CleanedLine;
                    }

                    if (InExits)
                    {
                        if (CleanedLine.Contains(":") && CurrentExitLine != "")
                        {
                            var logiclinesplit = CleanLine(CurrentExitLine, CurrentRegion.Region, true).Split(':');
                            if (logiclinesplit.Count() > 1) { CurrentRegion.Exits.Add($"{CurrentRegion.Region}>MQ {logiclinesplit[0].Trim()}", "(" + logiclinesplit[1].Trim()); }
                            CurrentExitLine = "";
                        }
                        CurrentExitLine += CleanedLine;
                    }

                    if (AtEndORegion)
                    {
                        if (CurrentRegion != null)
                        {
                            AllRegionData.Add(CurrentRegion);
                            CurrentRegion = null;
                        }
                    }
                }
                InRegion = false;
                inLocations = false;
                InExits = false;
                AtEndORegion = false;
                InEvents = false;
                CurrentEventLine = "";
                CurrentLogicLine = "";
                CurrentExitLine = "";
                CurrentRegion = null;
            }

            string RemoveComments(string i)
            {
                if (i.Contains("#"))
                {
                    return i.Split('#')[0];
                }
                return i;
            }

            void HandleLogicHelpers()
            {
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                CurrentRegion = new RegionData { Region = "Helpers" };
                string LogicLine = "";
                foreach (var i in LogiHelperFile)
                {
                    var CleanedLine = RemoveComments(i);

                    if (CleanedLine.Contains(":") && LogicLine != "")
                    {
                        var HelperData = LogicLine.Split(':');

                        if (HelperData.Count() > 1)
                        {
                            HelperData[0] = regex.Replace(HelperData[0], " ");
                            HelperData[0] = HelperData[0].Replace("\"", "").Replace(",", "").Replace("'", "").Trim();
                            HelperData[1] = regex.Replace(HelperData[1], " ");
                            HelperData[1] = HelperData[1].Replace("\"", "").Replace(",", "").Replace("'", "").Trim();
                            if (!HelperData[1].Contains("==") && !HelperData[1].Contains("!="))
                            {
                                CurrentRegion.Events.Add(HelperData[0], HelperData[1]);
                            }
                        }
                        LogicLine = "";
                    }

                    LogicLine += CleanedLine;

                }
                AllRegionData.Add(CurrentRegion);
            }

            string CleanLine(string Line, string Region, bool EntranceLine = false)
            {
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                Line = regex.Replace(Line, " ");
                Line = Line.Replace("\"", "").Replace(",", "").Replace("'", "").Trim();


                Line = $"{Line}) and {Region}";

                return Line.Trim();
            }

            void PrintData()
            {
                foreach (var i in AllRegionData)
                {
                    Console.WriteLine("============");
                    Console.WriteLine($"Region: {i.Region}");

                    if (i.Locations.Any())
                    {
                        Console.WriteLine($"Location Logic:");
                        foreach (var j in i.Locations)
                        {
                            Console.WriteLine($"  {j}");
                            if (UsedNames.Contains(j.Key)) { DuplicateName.Add(j.Key); }
                            else { UsedNames.Add(j.Key); }
                        }
                    }
                    if (i.Exits.Any())
                    {
                        Console.WriteLine($"Exit Logic:");
                        foreach (var j in i.Exits)
                        {
                            Console.WriteLine($"  {j}");
                            if (UsedNames.Contains(j.Key)) { DuplicateName.Add(j.Key); }
                            else { UsedNames.Add(j.Key); }
                        }
                    }
                    if (i.Events.Any())
                    {
                        Console.WriteLine($"Event Logic:");
                        foreach (var j in i.Events)
                        {
                            Console.WriteLine($"  {j}");
                            if (UsedNames.Contains(j.Key)) { DuplicateName.Add(j.Key); }
                            else { UsedNames.Add(j.Key); }
                        }
                    }
                }
            }

            void PrintDuplicate()
            {
                Dictionary<string, string> CombinedDuplicates = new Dictionary<string, string>();
                foreach (var i in DuplicateName.Distinct())
                {
                    CombinedDuplicates.Add(i, "");
                    foreach (var j in AllRegionData)
                    {
                        foreach (var k in j.Events)
                        {
                            AddDupeData(k, i);
                        }
                        foreach (var k in j.Exits)
                        {
                            AddDupeData(k, i);
                        }
                        foreach (var k in j.Locations)
                        {
                            AddDupeData(k, i);
                        }
                    }
                }
                Console.WriteLine($"=========");
                Console.WriteLine($"Combined Duplicates");

                foreach (var i in CombinedDuplicates)
                {
                    Console.WriteLine(i);
                }


                void AddDupeData(KeyValuePair<string, string> k, string i)
                {
                    if (k.Key == i)
                    {
                        if (CombinedDuplicates[i] == "")
                        {
                            CombinedDuplicates[i] = $"({k.Value})";
                        }
                        else
                        {
                            CombinedDuplicates[i] += $" or ({k.Value})";
                        }
                    }
                }
            }

            bool TestPosition(string i, bool IsMQ = false)
            {
                if (i.Trim().StartsWith("#")) { return true; }

                if (i.Contains("\"region_name\":"))
                {
                    InRegion = true;
                    AtEndORegion = false;
                    CurrentRegion = new RegionData();
                    CurrentRegion.Region = (IsMQ ? "MQ " : "") + i.Replace("\"region_name\":", "").Replace("\"", "").Replace(",", "").Trim();
                    return true;
                }
                if (i.Contains("\"events\":") && InRegion)
                {
                    InEvents = true;
                    return true;
                }
                if (i.Contains("\"locations\":") && InRegion)
                {
                    inLocations = true;
                    return true;
                }
                if (i.Contains("\"exits\":") && InRegion)
                {
                    InExits = true;
                    return true;
                }
                if (i.Contains("}"))
                {
                    if (InEvents) 
                    { 
                        InEvents = false;
                        if (CurrentEventLine != "")
                        {
                            var logiclinesplit = CleanLine(CurrentEventLine, CurrentRegion.Region).Split(':');
                            if (logiclinesplit.Count() > 1) { CurrentRegion.Events.Add(logiclinesplit[0].Trim(), "(" + logiclinesplit[1].Trim()); }
                            CurrentEventLine = "";
                        }
                    }
                    else if (inLocations) 
                    { 
                        inLocations = false;
                        if (CurrentLogicLine != "")
                        {
                            var logiclinesplit = CleanLine(CurrentLogicLine, CurrentRegion.Region).Split(':');
                            if (logiclinesplit.Count() > 1) { CurrentRegion.Locations.Add(logiclinesplit[0].Trim(), "(" + logiclinesplit[1].Trim()); }
                            CurrentLogicLine = "";
                        }
                    }
                    else if (InExits) 
                    { 
                        InExits = false;
                        if (CurrentExitLine != "")
                        {
                            string MQRoom = IsMQ ? "MQ " : "";

                            var logiclinesplit = CleanLine(CurrentExitLine, CurrentRegion.Region, true).Split(':');
                            if (logiclinesplit.Count() > 1) { CurrentRegion.Exits.Add($"{CurrentRegion.Region}>{MQRoom}{logiclinesplit[0].Trim()}", "(" + logiclinesplit[1].Trim()); }
                            CurrentExitLine = "";
                        }
                    }
                    else if (InRegion) 
                    { 
                        InRegion = false;
                    }

                    if (!InRegion) { AtEndORegion = true; }
                    return true;
                }
                return false;
            }
        }

        public static List<string> GetItemsUsedInLogic(List<RegionData> RegionData)
        {
            List<string> AllEntries = new List<string>();

            foreach(var h in RegionData)
            {
                foreach(var g in h.Locations)
                {
                    var parsedString = g.Value.Replace(" and ", "&").Replace(" or ", "|").Replace("can_use(", "(");
                    foreach (var j in LogicParser.GetEntries(parsedString).Where(x => x.Length > 0 && !LogicParser.ISLogicChar(x[0]) && !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct())
                    {
                        AllEntries.Add(j + ": " + h.Region);
                    }
                }
                foreach (var g in h.Exits)
                {
                    var parsedString = g.Value.Replace(" and ", "&").Replace(" or ", "|").Replace("can_use(", "(");
                    foreach (var j in LogicParser.GetEntries(parsedString).Where(x => x.Length > 0 && !LogicParser.ISLogicChar(x[0]) && !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct())
                    {
                        AllEntries.Add(j + ": " + h.Region);
                    }
                }
                foreach (var g in h.Events)
                {
                    var parsedString = g.Value.Replace(" and ", "&").Replace(" or ", "|").Replace("can_use(", "(");
                    foreach (var j in LogicParser.GetEntries(parsedString).Where(x => x.Length > 0 && !LogicParser.ISLogicChar(x[0]) && !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct())
                    {
                        AllEntries.Add(j + ": " + h.Region);
                    }
                }
            }
            return AllEntries.OrderBy(x => x).ToList();
        }
    }
}
