using MMR_Tracker.Forms;
using MMR_Tracker.Forms.Core_Tracker;
using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MMR_Tracker.Class_Files
{
    class Tools
    {
        public static string SaveFilePath = "";

        public static LogicObjects.ItemUnlockData FindRequirements(LogicObjects.LogicEntry Item, LogicObjects.TrackerInstance Instance)
        {
            List<int> ImportantItems = new List<int>();
            List<LogicObjects.PlaythroughItem> playthrough = new List<LogicObjects.PlaythroughItem>();
            var LogicCopy = Utility.CloneTrackerInstance(Instance);

            var RandomizedEntry = LogicCopy.Logic[Item.ID].RandomizedEntry(LogicCopy);
            if (RandomizedEntry != null && RandomizedEntry.Aquired)
            {
                RandomizedEntry.Aquired = false;
            }

            foreach (var i in LogicCopy.Logic)
            {
                if (i.Unrandomized()) { i.IsFake = true; }
            }
            var ItemCopy = LogicCopy.Logic[Item.ID];
            LogicCopy.RefreshFakeItems();
            foreach (var i in LogicCopy.Logic)
            {
                ImportantItems.Add(i.ID);
                if (i.IsFake) { i.SpoilerRandom = i.ID; }
            }
            PlaythroughGenerator.UnlockAllFake(LogicCopy, ImportantItems, 0, playthrough);
            List<int> UsedItems = new List<int>();
            bool isAvailable = ItemCopy.CheckAvailability(Instance, UsedItems);

            if (!isAvailable) { return new LogicObjects.ItemUnlockData(); }
            List<int> NeededItems = Tools.ResolveFakeToRealItems(new LogicObjects.PlaythroughItem { SphereNumber = 0, Check = ItemCopy, ItemsUsed = UsedItems }, playthrough, LogicCopy.Logic);
            List<int> FakeItems = Tools.FindAllFakeItems(new LogicObjects.PlaythroughItem { SphereNumber = 0, Check = ItemCopy, ItemsUsed = UsedItems }, playthrough, LogicCopy.Logic);
            NeededItems = NeededItems.Distinct().ToList();
            return new LogicObjects.ItemUnlockData {Playthrough = playthrough, FakeItems = FakeItems, ResolvedRealItems = NeededItems, UsedItems =UsedItems };
        }

        public static void CreateDictionary()
        {
            string file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt)|*.txt");
            if (file == "") { return; }

            LogicObjects.TrackerInstance CDLogic = new LogicObjects.TrackerInstance();
            int LogicVersion = VersionHandeling.GetVersionDataFromLogicFile(File.ReadAllLines(file)).Version;
            LogicEditing.PopulateTrackerInstance(CDLogic);

            List<LogicObjects.SpoilerData> SpoilerLog = Tools.ReadHTMLSpoilerLog("", CDLogic).SpoilerDatas;
            if (SpoilerLog.Count == 0) { return; }

            //For each entry in your logic list, check each entry in your spoiler log to find the rest of the data
            foreach (LogicObjects.LogicEntry entry in CDLogic.Logic)
            {
                foreach (LogicObjects.SpoilerData spoiler in SpoilerLog)
                {
                    if (spoiler.LocationID == entry.ID)
                    {
                        entry.IsFake = false;
                        entry.LocationName = spoiler.LocationName;
                        entry.SpoilerLocation = new List<string> { spoiler.LocationName };
                        entry.LocationArea = spoiler.LocationArea;
                        entry.ItemSubType = "Item";
                        if (entry.DictionaryName.Contains("Bottle:")) { entry.ItemSubType = "Bottle"; }
                        if (entry.DictionaryName.StartsWith("Entrance")) { entry.ItemSubType = "Entrance"; }

                        if (!CDLogic.EntranceRando)
                        {
                            if (entry.DictionaryName == "Woodfall Temple access")
                            { entry.LocationArea = "Dungeon Entrance"; entry.ItemSubType = "Dungeon Entrance"; }
                            if (entry.DictionaryName == "Snowhead Temple access")
                            { entry.LocationArea = "Dungeon Entrance"; entry.ItemSubType = "Dungeon Entrance"; }
                            if (entry.DictionaryName == "Great Bay Temple access")
                            { entry.LocationArea = "Dungeon Entrance"; entry.ItemSubType = "Dungeon Entrance"; }
                            if (entry.DictionaryName == "Inverted Stone Tower Temple access")
                            { entry.LocationArea = "Dungeon Entrance"; entry.ItemSubType = "Dungeon Entrance"; }
                        } //Dungeon Entrance Rando is dumb
                    }
                    if (spoiler.ItemID == entry.ID)
                    {
                        entry.IsFake = false; //Not necessary, might cause problem but also might fix them ¯\_(ツ)_/¯
                        entry.ItemName = spoiler.ItemName;
                        entry.SpoilerItem = new List<string> { spoiler.ItemName };
                    }
                }
            }

            List<string> csv = new List<string> { "DictionaryName,LocationName,ItemName,LocationArea,ItemSubType,SpoilerLocation,SpoilerItem" };
            //Write this data to list of strings formated as lines of csv and write that to a text file
            foreach (LogicObjects.LogicEntry entry in CDLogic.Logic)
            {
                if (!entry.IsFake)
                {
                    csv.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                         entry.DictionaryName, entry.LocationName, entry.ItemName, entry.LocationArea,
                         entry.ItemSubType, entry.SpoilerLocation, entry.SpoilerItem));
                }
            }

            SaveFileDialog saveDic = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                Title = "Save Dictionary File",
                FileName = "MMRDICTIONARYV" + LogicVersion + ".csv"
            };
            saveDic.ShowDialog();
            File.WriteAllLines(saveDic.FileName, csv);
        }

        public static LogicObjects.SpoilerLogData ReadTextSpoilerlog(LogicObjects.TrackerInstance instance, string[] Spoiler = null)
        {
            LogicObjects.SpoilerLogData SpoilerData = new LogicObjects.SpoilerLogData();

            if (Spoiler == null)
            {
                OpenFileDialog SpoilerFile = new OpenFileDialog
                {
                    Title = "Select A Logic File",
                    Filter = "Text Spoiler Log (*.txt)|*.txt",
                    FilterIndex = 1,
                    Multiselect = false
                };
                if (SpoilerFile.ShowDialog() != DialogResult.OK) { return SpoilerData; }
                Spoiler = File.ReadLines(SpoilerFile.FileName).ToArray();
            }

            Spoiler = SpoilerLogConverter.AutoConverter(instance, Spoiler);

            Dictionary<int, List<int>> usedId = new Dictionary<int, List<int>>();
            int PlayerID = instance.Options.MyPlayerID;

            var IceTrapEntry = instance.Logic.Find(x => x.DictionaryName == "Ice Trap");
            var NoAvailableItemsEntry = instance.Logic.Find(x => x.DictionaryName == "MMRTSpoilerLogErrorCatcher");

            //Debugging
            List<string> LocationsNotFound = new List<string>();
            List<string> LocationsFound = new List<string>();
            List<string> ItemsNotFound = new List<string>();
            List<string> ItemsIceTrap = new List<string>();
            List<string> ItemsNoneAvailable = new List<string>();
            List<string> ItemsFound = new List<string>();
            //Debugging

            foreach (string i in Spoiler)
            {
                //Debugging
                bool ItemHandled = false;
                //Debugging

                var line = i;

                if (line.StartsWith("Settings:") && instance.IsMM())
                {
                    line = line.Replace("Settings:", "\"GameplaySettings\":");
                    line = "{" + line + "}";
                    try
                    {
                        SpoilerData.SettingString = JsonConvert.DeserializeObject<LogicObjects.Configuration>(line).GameplaySettings;
                    }
                    catch {
                        SpoilerData.SettingString  = ParseSpoilerLogSettingsWithLineBreak(Spoiler, text: true);
                    }
                }

                if (line.Contains("Gossip Stone ") && line.Contains("Message")) { break; }
                if (line.Contains("->"))
                {
                    LogicObjects.SpoilerData entry = new LogicObjects.SpoilerData();
                    string LineCopy = line;
                    entry.BelongsTo = PlayerID;

                    if (LineCopy.Contains("$")) 
                    {
                        var ExtractPlayerData = LineCopy.Split('$');
                        if (ExtractPlayerData.Count() > 1 && int.TryParse(ExtractPlayerData[1], out int NewPlayer))
                        {
                            LineCopy = ExtractPlayerData[0];
                            entry.BelongsTo = NewPlayer;
                        }
                    }

                    if (!usedId.ContainsKey(entry.BelongsTo)) { usedId.Add(entry.BelongsTo, new List<int>()); }

                    var linedata = LineCopy.Split(new string[] { "->" }, StringSplitOptions.None);
                    linedata[0] = linedata[0].Replace("*", "");
                    linedata[1] = linedata[1].Replace("*", "");

                    entry.LocationName = linedata[0].Trim();
                    entry.ItemName = linedata[1].Trim();
                    entry.LocationID = -2;
                    entry.ItemID = -2;


                    var location = instance.Logic.Find(x => x.SpoilerLocation.Select(j => j ?? "".Trim()).Contains(entry.LocationName.Trim()));

                    var Item = instance.Logic.Find(x => x.SpoilerItem.Select(j => j ?? "".Trim()).Contains(entry.ItemName.Trim()) && !usedId[entry.BelongsTo].Contains(x.ID));

                    if (location == null)
                    {
                        LocationsNotFound.Add(entry.LocationName);
                    }
                    else
                    {
                        LocationsFound.Add(entry.LocationName);
                    }

                    //Handle Ice traps
                    if (entry.ItemName.Contains("Ice Trap") || entry.ItemName == "Ice Trap")
                    {
                        ItemsIceTrap.Add(entry.ItemName);
                        ItemHandled = true;
                        if (IceTrapEntry == null) { Item = new LogicObjects.LogicEntry { ID = -1 }; }
                        else { Item = IceTrapEntry; }
                    }

                    //If an item exists in the spoiler log, but doesn't have any unused items to assign, attempt to use a special error entry
                    if (Item == null)
                    {
                        var ItemExists = instance.Logic.Find(x => x.SpoilerItem.Select(j => j ?? "".Trim()).Contains(entry.ItemName.Trim())) != null;
                        if (ItemExists)
                        {
                            ItemsNoneAvailable.Add(entry.ItemName);
                            if (NoAvailableItemsEntry != null)
                            {
                                Item = NoAvailableItemsEntry;
                            }
                        }
                        else
                        {
                            ItemsNotFound.Add(entry.ItemName);
                        }
                    }
                    else if (!ItemHandled)
                    {
                        ItemsFound.Add(entry.ItemName);
                    }

                    if (Item != null && location != null)
                    {
                        entry.ItemID = Item.ID;
                        entry.LocationID = location.ID;
                        if (instance.IsMM() || !Item.IsEntrance()) usedId[entry.BelongsTo].Add(Item.ID);
                        SpoilerData.SpoilerDatas.Add(entry);
                    }
                }
            }

            bool DebugStatus = true;
            if (DebugStatus)
            {
                Console.WriteLine($"\nThe Following locations were not found in the logic");
                foreach (var i in LocationsNotFound)
                {
                    Console.WriteLine($"-{i}");
                }
                Console.WriteLine($"\nThe Following Items were not found in the logic");
                foreach (var i in ItemsNotFound)
                {
                    Console.WriteLine($"-{i}");
                }
                Console.WriteLine($"\nThe Following Items were found in the dictionary but all were already used");
                foreach (var i in ItemsNoneAvailable)
                {
                    Console.WriteLine($"-{i}");
                }
                Console.WriteLine($"\nThe Following Items were ice traps");
                foreach (var i in ItemsIceTrap)
                {
                    Console.WriteLine($"-{i}");
                }
                Console.WriteLine($"\nThe Following Locations were found in the dictionary");
                foreach (var i in LocationsFound)
                {
                    //Console.WriteLine($"-{i}");
                }
                Console.WriteLine($"\nThe Following Items were found in the dictionary");
                foreach (var i in ItemsFound)
                {
                    //Console.WriteLine($"-{i}");
                }
            }


            //foreach(var i in usedId[PlayerID].OrderBy(x => x)) { Debugging.Log(i); }
            return SpoilerData;
        }
        
        public static LogicObjects.SpoilerLogData ReadHTMLSpoilerLog(string Path, LogicObjects.TrackerInstance Instance)
        {
            var ReturnData = new LogicObjects.SpoilerLogData();

            List<LogicObjects.SpoilerData> SpoilerData = new List<LogicObjects.SpoilerData>();

            if (Path == "")
            {
                OpenFileDialog SpoilerFile = new OpenFileDialog
                {
                    Title = "Select an HTML Spoiler Log",
                    Filter = "HTML Spoiler Log (*.html)|*.html",
                    FilterIndex = 1,
                    Multiselect = false
                };
                if (SpoilerFile.ShowDialog() != DialogResult.OK) { return new LogicObjects.SpoilerLogData { SpoilerDatas = SpoilerData }; }
                Path = SpoilerFile.FileName;
            }

            string Region = "";
            LogicObjects.SpoilerData entry = new LogicObjects.SpoilerData();
            foreach (string line in File.ReadAllLines(Path))
            {
                if (line.StartsWith("<label><b>Settings: </b></label><code style=\"word-break: break-all;\">") && Instance.IsMM())
                {
                    var newLine = line.Replace("<label><b>Settings: </b></label><code style=\"word-break: break-all;\">", "{\"GameplaySettings\":");
                    newLine = newLine.Replace("</code><br/>", "}");
                    LogicObjects.GameplaySettings SettingFile = null;
                    try
                    {
                        SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(newLine).GameplaySettings;
                    }
                    catch 
                    {
                        //In 1.15 the setting file in the spoiler log is no longer a single line. New function to read multiline settings.
                        SettingFile = ParseSpoilerLogSettingsWithLineBreak(File.ReadAllLines(Path));
                    }
                    ReturnData.SettingString = SettingFile;
                }

                if (line.Contains("<tr class=\"region\">"))
                {
                    Region = line.Trim();
                    Region = Region.Replace("<tr class=\"region\"><td colspan=\"3\">", "");
                    Region = Region.Replace("</td></tr>", "");
                }
                if (line.Contains("data-newlocationid="))
                {
                    var X = line.Trim().Split('"');
                    entry.LocationID = Int32.Parse(X[3]);
                    entry.ItemID = Int32.Parse(X[1]);
                }
                if (line.Contains("<td class=\"newlocation\">"))
                {
                    var X = line.Trim();
                    X = X.Replace("<td class=\"newlocation\">", "");
                    X = X.Replace("</td>", "");
                    entry.LocationName = X;
                }
                if (line.Contains("<td class=\"spoiler itemname\"><span data-content=\"") || line.Contains("<td class=\"spoiler itemname\"> <span data-content=\""))
                {
                    var X = line.Trim();
                    X = X.Replace("<td class=\"spoiler itemname\"> <span data-content=\"", "");
                    X = X.Replace("<td class=\"spoiler itemname\"><span data-content=\"", "");
                    X = X.Replace("\"></span></td>", "");
                    entry.ItemName = X;
                    entry.LocationArea = Region;

                    if (entry.ItemName.Contains("Ice Trap"))
                    {
                        entry.ItemID = -1;
                    }

                    SpoilerData.Add(entry);
                    entry = new LogicObjects.SpoilerData();
                }
                if (line.Contains("<h2>Item Locations</h2>")) { break; }
            }

            Dictionary<string, int> Pricedata = new Dictionary<string, int>();
            bool AtPricedata = false;
            string PriceCheck = "";
            int Pricenumb = 0;
            foreach (string line in File.ReadAllLines(Path))
            {
                if (line.Trim().StartsWith("<h2>Gossip Stone Hints</h2>")) { break; }
                if (line.Trim().StartsWith("<h2>Randomized Prices</h2>") && Instance.IsMM()) { AtPricedata = true; }
                if (AtPricedata)
                {
                    if (line.Trim().StartsWith("<td>"))
                    {
                        var X = line.Trim();
                        X = X.Replace("<td>", "");
                        X = X.Replace("</td>", "");
                        PriceCheck = X;
                    }
                    if (line.Trim().StartsWith("<td class=\"spoiler\"><span data-content="))
                    {
                        var X = line.Trim();
                        X = X.Replace("<td class=\"spoiler\"><span data-content=\"", "");
                        X = X.Replace("\"></span></td>", "");
                        try { Pricenumb = int.Parse(X); }
                        catch { Pricenumb = 69; Console.WriteLine("Could not parse Line \n" + line); }

                        int AppendCounter = 0;
                        string CurrentKey = PriceCheck;
                        while (Pricedata.ContainsKey(CurrentKey))
                        {
                            AppendCounter += 1;
                            CurrentKey = PriceCheck + " " + AppendCounter.ToString();
                        }

                        Pricedata.Add(CurrentKey, Pricenumb);
                    }
                }
            }

            Dictionary<string, string> GossipData = new Dictionary<string, string>();
            bool AtGossipdata = false;
            string GossipStone = "";
            string Hint = "";
            foreach (string line in File.ReadAllLines(Path))
            {
                if (line.Trim().StartsWith("<script>")) { break; }
                if (line.Trim().StartsWith("<h2>Gossip Stone Hints</h2>") && Instance.IsMM()) { AtGossipdata = true; }
                if (AtGossipdata)
                {
                    if (line.Trim().StartsWith("<td>"))
                    {
                        var X = line.Trim();
                        X = X.Replace("<td>", "");
                        X = X.Replace("</td>", "");
                        GossipStone = X;
                    }
                    if (line.Trim().StartsWith("<td class=\"spoiler\"><span data-content="))
                    {
                        var X = line.Trim();
                        X = X.Replace("<td class=\"spoiler\"><span data-content=\"", "");
                        X = X.Replace("\"></span></td>", "");
                        Hint = X;
                        if (!GossipData.ContainsKey(GossipStone)) { GossipData.Add(GossipStone, Hint); }
                    }
                }
            }

            ReturnData.SpoilerDatas = SpoilerData;
            ReturnData.Pricedata = Pricedata;
            ReturnData.GossipHints = GossipData;


            if (Instance.EntranceRando) { return ReturnData; }

            //Fix Dungeon Entrances
            Dictionary<string, int> EntIDMatch = new Dictionary<string, int>();
            var entranceIDs = Instance.EntranceAreaDic;
            foreach (LogicObjects.SpoilerData Thing in SpoilerData)
            {
                if (entranceIDs.ContainsValue(Thing.ItemID)) { EntIDMatch.Add(Thing.ItemName, Thing.ItemID); }
            }
            foreach (LogicObjects.SpoilerData Thing in SpoilerData)
            {
                if (EntIDMatch.ContainsKey(Thing.LocationName)) { Thing.LocationID = EntIDMatch[Thing.LocationName]; }
            }

            return ReturnData;
        }

        public static bool SaveInstance(LogicObjects.TrackerInstance Instance, bool SetPath = false , string FilePath = "")
        {
            JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            Debugging.Log("Begin Save");
            if (FilePath == "" || !File.Exists(FilePath))
            {
                SaveFileDialog saveDialog = new SaveFileDialog { Filter = "MMR Tracker Save (*.MMRTSAV)|*.MMRTSAV", FilterIndex = 1 };
                if (saveDialog.ShowDialog() != DialogResult.OK) { return false; }
                FilePath = saveDialog.FileName;
            }
            //Clear the undo and redo list because otherwise the save file is massive
            Instance.UnsavedChanges = false;
            Debugging.Log("Start Clone");
            var SaveInstance = Utility.CloneTrackerInstance(Instance);
            Debugging.Log("Write Data");
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(SaveInstance, _jsonSerializerOptions));
            Debugging.Log("Format tracker");
            if (SetPath) { Tools.SaveFilePath = FilePath; }
            UpdateTrackerTitle();
            return true;
        }

        public static void SetUnsavedChanges(LogicObjects.TrackerInstance Instance)
        {
            Instance.UnsavedChanges = true;
            UpdateTrackerTitle();
        }

        public static void UpdateTrackerTitle()
        {
            MainInterface.CurrentProgram.Text = (string.IsNullOrWhiteSpace(LogicObjects.MainTrackerInstance.GameCode)) ? "MMR" : LogicObjects.MainTrackerInstance.GameCode;
            MainInterface.CurrentProgram.Text += " Tracker";
            MainInterface.CurrentProgram.Text += (LogicObjects.MainTrackerInstance.UnsavedChanges) ? "*" : "";

            if (VersionHandeling.TrackerVersionStatus == -1) { MainInterface.CurrentProgram.Text += $" (OUTDATED)"; }
            else if (VersionHandeling.TrackerVersionStatus == 1) { MainInterface.CurrentProgram.Text += $" (DEV)"; }
        }

        public static void LoadInstance(string file = "")
        {
            if (file == "")
            {
                if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return; }
                file = Utility.FileSelect("Select A Save File", "MMR Tracker Save (*.MMRTSAV)|*.MMRTSAV");
                if (file == "") { return; }
            }
            var backup = Utility.CloneTrackerInstance(LogicObjects.MainTrackerInstance);
            LogicObjects.MainTrackerInstance = new LogicObjects.TrackerInstance();
            //Try to load the save file with the new system. If that fails try wth the old system. If that fails restore the current instance and show an error.
            try { LogicObjects.MainTrackerInstance = JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(File.ReadAllText(file)); }
            catch
            {
                bool OldSave = TryLoadOldSaveFile(file);
                if (!OldSave)
                {
                    MessageBox.Show($"Save File Invalid!");
                    LogicObjects.MainTrackerInstance = backup;
                    return;
                }
            }
            //Extra saftey checks for older save files
            LogicObjects.MainTrackerInstance.EntranceRando = LogicObjects.MainTrackerInstance.IsEntranceRando();
            if (LogicObjects.MainTrackerInstance.LogicVersion == 0)
            {
                LogicObjects.MainTrackerInstance.LogicVersion = VersionHandeling.GetVersionDataFromLogicFile(LogicObjects.MainTrackerInstance.RawLogicFile).Version;
            }
            Tools.SaveFilePath = file;
            return;
        }

        public static bool TryLoadOldSaveFile(string file)
        {
            //Pre Spoiler Location/Spoiler Item Changes
            try
            {
                LogicObjects.MainTrackerInstance = JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(File.ReadAllText(file), new JsonSerializerSettings
                {
                    Error = HandleDeserializationError
                });
                void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
                {
                    errorArgs.ErrorContext.Handled = true;
                }
                foreach (var i in LogicObjects.MainTrackerInstance.Logic)
                {
                    var dicentry = LogicObjects.MainTrackerInstance.LogicDictionary.Find(x => x.DictionaryName == i.DictionaryName);
                    if (dicentry == null) { continue; }
                    i.SpoilerLocation = (string.IsNullOrWhiteSpace(dicentry.SpoilerLocation))
                            ? new List<string> { i.LocationName } : dicentry.SpoilerLocation.Split('|').ToList();
                    i.SpoilerItem = (string.IsNullOrWhiteSpace(dicentry.SpoilerItem))
                        ? new List<string> { i.ItemName } : dicentry.SpoilerItem.Split('|').ToList();
                }
                return true;
            } catch { }
            //Pre Tracker Instance Addition
            try
            {
                string[] options = File.ReadAllLines(file);
                LogicObjects.MainTrackerInstance.Logic = JsonConvert.DeserializeObject<List<LogicObjects.LogicEntry>>(options[0]);
                if (options.Length > 1) { LogicObjects.MainTrackerInstance.LogicVersion = Int32.Parse(options[1].Replace("version:", "")); return true; }
            }
            catch
            { }
            return false;
        }

        public static void SaveState(LogicObjects.TrackerInstance Instance, LogicObjects.SaveState Data, LogicObjects.UndoRedoData UndoredoData, bool CloneData = true)
        {
            if (CloneData)
            {
                if (Data.trackerInstance != null)
                {
                    Data.trackerInstance = Utility.CloneTrackerInstance(Data.trackerInstance);
                }
                else if (Data.Logic != null)
                {
                    Data.Logic = Utility.CloneLogicList(Data.Logic);
                }
                else if (Data.SingleItems != null)
                {
                    Data.SingleItems = Utility.CloneLogicList(Data.SingleItems);
                }
            }
            int MaxUndoCount = (int)Math.Floor(Math.Pow(((double)5000 / (double)Instance.Logic.Count()), 1.5)); //Reduce the max count based on the size of the logic.
            if (MaxUndoCount < 3) { MaxUndoCount = 3; }
            UndoredoData.UndoList.Add(Data);
            if (UndoredoData.UndoList.Count() > MaxUndoCount) { UndoredoData.UndoList.RemoveAt(0); }
            UndoredoData.RedoList = new List<LogicObjects.SaveState>();
            MainInterface.CurrentProgram.Tools_StateListChanged();

        }

        public static void Undo(LogicObjects.TrackerInstance Instance, LogicObjects.UndoRedoData UndoredoData)
        {
            if (UndoredoData.UndoList.Any())
            {
                SetUnsavedChanges(Instance);
                var lastItem = UndoredoData.UndoList.Count - 1;

                if (UndoredoData.UndoList[lastItem] == null) { return; }

                if (UndoredoData.UndoList[lastItem].trackerInstance != null)
                {
                    UndoredoData.RedoList.Add(new LogicObjects.SaveState() { trackerInstance = Utility.CloneTrackerInstance(Instance) });
                    var NewInstance = Utility.CloneTrackerInstance(UndoredoData.UndoList[lastItem].trackerInstance);
                    Instance = NewInstance;
                }
                else if (UndoredoData.UndoList[lastItem].Logic != null)
                {
                    UndoredoData.RedoList.Add(new LogicObjects.SaveState() { Logic = Utility.CloneLogicList(Instance.Logic) });
                    Instance.Logic = Utility.CloneLogicList(UndoredoData.UndoList[lastItem].Logic);
                }
                else if (UndoredoData.UndoList[lastItem].SingleItems != null)
                {
                    UndoredoData.RedoList.Add(new LogicObjects.SaveState() { Logic = Utility.CloneLogicList(Instance.Logic) });
                    foreach (var i in UndoredoData.UndoList[lastItem].SingleItems)
                    {
                        Instance.Logic[i.ID] = Utility.CloneLogicObject(i);
                    }
                }

                UndoredoData.UndoList.RemoveAt(lastItem);
                MainInterface.CurrentProgram.Tools_StateListChanged();
            }
        }

        public static void Redo(LogicObjects.TrackerInstance Instance, LogicObjects.UndoRedoData UndoredoData)
        {
            if (UndoredoData.RedoList.Any())
            {
                SetUnsavedChanges(Instance);
                var lastItem = UndoredoData.RedoList.Count - 1;

                if (UndoredoData.RedoList[lastItem].trackerInstance != null)
                {
                    UndoredoData.UndoList.Add(new LogicObjects.SaveState() { trackerInstance = Utility.CloneTrackerInstance(Instance) });
                    var NewInstance = Utility.CloneTrackerInstance(UndoredoData.RedoList[lastItem].trackerInstance);
                    Instance = NewInstance;
                }
                else if (UndoredoData.RedoList[lastItem].Logic != null)
                {
                    UndoredoData.UndoList.Add(new LogicObjects.SaveState() { Logic = Utility.CloneLogicList(Instance.Logic) });
                    Instance.Logic = Utility.CloneLogicList(UndoredoData.RedoList[lastItem].Logic);
                }
                else if (UndoredoData.RedoList[lastItem].SingleItems != null)
                {
                    UndoredoData.UndoList.Add(new LogicObjects.SaveState() { Logic = Utility.CloneLogicList(Instance.Logic) });
                    foreach (var i in UndoredoData.RedoList[lastItem].SingleItems)
                    {
                        Instance.Logic[i.ID] = Utility.CloneLogicObject(i);
                    }
                }

                UndoredoData.RedoList.RemoveAt(lastItem);
                MainInterface.CurrentProgram.Tools_StateListChanged();

            }
        }

        public static bool PromptSave(LogicObjects.TrackerInstance Instance, bool OnlyIfUnsaved = true)
        {
            if (Instance.UnsavedChanges || !OnlyIfUnsaved)
            {
                if (Instance.Options.AutoSave && File.Exists(Tools.SaveFilePath))
                {
                    Tools.SaveInstance(LogicObjects.MainTrackerInstance, true, Tools.SaveFilePath);
                    return true;
                }
                DialogResult result = MessageBox.Show("Would you like to save?", "You have unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel) { return false; }
                if (result == DialogResult.Yes)
                {
                    if (!Tools.SaveInstance(LogicObjects.MainTrackerInstance, true, Tools.SaveFilePath)) { return false; }
                }
            }
            return true;
        }

        public static Dictionary<int, int> CreateRandItemDic(List<LogicObjects.LogicEntry> logic, bool Spoiler = false)
        {
            var spoilerDic = new Dictionary<int, int>();
            foreach (var i in logic)
            {
                var value = (Spoiler) ? i.SpoilerRandom : i.RandomizedItem;
                if (value > -2 && !spoilerDic.ContainsKey(value))
                {
                    spoilerDic.Add(value, i.ID);
                }
            }
            return spoilerDic;
        }

        public static void CreateOptionsFile(bool Recreate = false)
        {
            var options = new LogicObjects.DefaultTrackerOption();

            JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            if (!File.Exists("options.txt"))
            {
                try
                {
                    var file = File.Create("options.txt");
                    file.Close();
                }
                catch
                {
                    MessageBox.Show("Access Denied. Ensure you have proper access to the folder you are running the tracker from.");
                    if (Recreate) { return; }
                    else { System.Windows.Forms.Application.Exit(); }
                }

                if ((!Debugging.ISDebugging || (Control.ModifierKeys == Keys.Shift)) && !Recreate)
                {
                    var firsttime = MessageBox.Show("It looks like this is your first time running this tracker, would you like to see first time use information?", "First Time Setup", MessageBoxButtons.YesNo);
                    if (firsttime == DialogResult.Yes)
                    {
                        MessageBox.Show("Please Take this opportunity to familliarize yourself with how to use this tracker. \n\nClick OK to display the About page detailing how the tracker functions. \n\nThis information can be accessed at any time by selecting 'Info' -> 'About'.", "How to Use", MessageBoxButtons.OK);
                        InformationDisplay DebugScreen = new InformationDisplay { DebugFunction = 2 };
                        DebugScreen.ShowDialog();
                    }

                    var DefaultSetting = MessageBox.Show("Would you like to edit the default options?.\n\nThese can be changed later by selecting\nOptions -> Misc options -> Change Default settings\nor by editing the options.txt that will be created in your tracker folder. \nThese settings can also be changed per instance in the options tab", "Default Setting", MessageBoxButtons.YesNo);
                    if (DefaultSetting == DialogResult.Yes) { PromptForUserOptions(); }
                }

                File.WriteAllText("options.txt", JsonConvert.SerializeObject(options, _jsonSerializerOptions));
            }
            else if (Recreate)
            {
                try { options = JsonConvert.DeserializeObject<LogicObjects.DefaultTrackerOption>(File.ReadAllText("options.txt"), _jsonSerializerOptions); }
                catch { Console.WriteLine("could not parse options.txt"); }
                PromptForUserOptions();
                File.WriteAllText("options.txt", JsonConvert.SerializeObject(options, _jsonSerializerOptions));
            }

            void PromptForUserOptions()
            {
                DefaultOptionSelect SelectForm = new DefaultOptionSelect();
                SelectForm.Options = options;
                SelectForm.ShowDialog();
            }
        }

        public static List<int> ResolveFakeToRealItems(LogicObjects.PlaythroughItem item, List<LogicObjects.PlaythroughItem> Playthrough, List<LogicObjects.LogicEntry> logic)
        {
            //Find all the real items that were used to unlock this fake item
            var RealItems = new List<int>();
            var New = new LogicObjects.PlaythroughItem();
            foreach (var j in item.ItemsUsed)
            {
                if (!logic[j].IsFake) { RealItems.Add(j); }
                else
                {
                    var NewItem = Playthrough.Find(i => i.Check.ID == j);
                    if (NewItem == null) { continue; }
                    foreach (var k in ResolveFakeToRealItems(NewItem, Playthrough, logic)) { RealItems.Add(k); }
                }
            }
            return RealItems;
        }

        public static List<int> FindAllFakeItems(LogicObjects.PlaythroughItem item, List<LogicObjects.PlaythroughItem> Playthrough, List<LogicObjects.LogicEntry> logic)
        {
            //Find all fake items used to unlock the given item
            var FakeItems = new List<int>();
            var New = new LogicObjects.PlaythroughItem();
            foreach (var j in item.ItemsUsed)
            {
                if (logic[j].IsFake)
                {
                    FakeItems.Add(j);
                    var NewItem = Playthrough.Find(i => i.Check.ID == j);
                    foreach (var k in FindAllFakeItems(NewItem, Playthrough, logic)) { FakeItems.Add(k); }
                }
            }
            return FakeItems;
        }

        public static bool SameItemMultipleChecks(int item, LogicObjects.TrackerInstance Instance)
        {
            if (item < 0 || (!Instance.Options.StrictLogicHandeling && !LogicObjects.MainTrackerInstance.Options.IsMultiWorld)) { return false; }
            int count = 0;
            foreach (var entry in Instance.Logic)
            {
                if (entry.RandomizedItem == item && entry.Checked && entry.ItemBelongsToMe()) { count += 1; }
            }
            return count > 1;
        }

        public static void CreateTrackerInstance(LogicObjects.TrackerInstance Instance, string[] RawLogic)
        {
            JsonSerializerSettings _jsonSerializerOptions = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            Tools.SaveFilePath = "";
            Instance.RawLogicFile = RawLogic;
            LogicEditing.PopulateTrackerInstance(Instance);
            LogicEditing.CalculateItems(Instance);

            LogicObjects.DefaultTrackerOption TrackerDefaultOptions = new LogicObjects.DefaultTrackerOption();
            if (File.Exists("options.txt"))
            {
                try { TrackerDefaultOptions = JsonConvert.DeserializeObject<LogicObjects.DefaultTrackerOption>(File.ReadAllText("options.txt"), _jsonSerializerOptions); }
                catch { Console.WriteLine("could not parse options.txt"); }
                Instance.Options.ShowEntryNameTooltip = TrackerDefaultOptions.ToolTips;
                Instance.Options.MoveMarkedToBottom = TrackerDefaultOptions.Seperatemarked;
                Instance.Options.UnradnomizeEntranesOnStartup = TrackerDefaultOptions.DisableEntrancesOnStartup;
                Instance.Options.CheckForUpdate = TrackerDefaultOptions.CheckForUpdates;
                Instance.Options.HorizontalLayout = TrackerDefaultOptions.HorizontalLayout;
                Instance.Options.MiddleClickStarNotMark = TrackerDefaultOptions.MiddleClickFunction == "Star";
                Instance.Options.FormFont = TrackerDefaultOptions.FormFont;
            }

            if (!Instance.IsMM() && !TrackerDefaultOptions.OtherGamesOK && !Debugging.ISDebugging)
            {
                DialogResult dialogResult = MessageBox.Show("This logic file was NOT created for the Majoras Mask Randomizer. While this tracker can support other games, support is very Limited. Many features will be disabled and core features might not work as intended. Do you wish to continue? If so this prompt will no longer display.", "Other Randomizer", MessageBoxButtons.YesNo);
                if (dialogResult != DialogResult.Yes) { Instance = new LogicObjects.TrackerInstance(); return; }
                TrackerDefaultOptions.OtherGamesOK = true;
                try 
                {
                    if (File.Exists("options.txt")) { File.WriteAllText("options.txt", JsonConvert.SerializeObject(TrackerDefaultOptions, _jsonSerializerOptions)); }
                }
                catch { }

            }
            else if (Instance.LogicVersion < 8 && Instance.IsMM() && !Instance.JsonLogic)
            {
                DialogResult dialogResult = MessageBox.Show("You are using a version of logic that is not supported by this tracker. Any logic version lower than version 8 (Randomizer version 1.8) may not work as intended. Do you wish to continue?", "Unsupported Version", MessageBoxButtons.YesNo);
                if (dialogResult != DialogResult.Yes) { Instance = new LogicObjects.TrackerInstance(); return; }
            }
        }

        public static void WhatUnlockedThis(LogicObjects.LogicEntry Entry)
        {
            if (!Entry.Available)
            {
                MessageBox.Show(Entry.LocationName + " Is not available with the currently obtained items", Entry.LocationName + " Is not available");
                return;
            }
            var UnlockData = Tools.FindRequirements(Entry, LogicObjects.MainTrackerInstance);
            var Requirements = UnlockData.ResolvedRealItems;
            var FakeItems = UnlockData.FakeItems.Distinct().ToList();
            var Playthrough = UnlockData.Playthrough;
            var ItemsUsed = UnlockData.UsedItems;
            if (Requirements.Count == 0)
            {
                MessageBox.Show("Nothing is needed to check this location.", Entry.LocationName + " Has No Requirements");
                return;
            }
            string message = "Logic Entries used:\n";
            foreach (var i in ItemsUsed.Distinct()) { message = message + (LogicObjects.MainTrackerInstance.Logic[i].ItemName ?? LogicObjects.MainTrackerInstance.Logic[i].DictionaryName) + "\n"; }
            message += "\nReal items used:\n";
            foreach (var i in Requirements) { message = message + LogicObjects.MainTrackerInstance.Logic[i].ItemName + "\n"; }
            message += "\nFake Items Breakdown:\n";
            foreach (var i in FakeItems.OrderBy(x => LogicObjects.MainTrackerInstance.Logic[x].DictionaryName)) 
            {
                var ItemInPlaythrough = Playthrough.Find(x => x.Check.ID == i) ?? new LogicObjects.PlaythroughItem { ItemsUsed = new List<int>() };
                message = message + LogicObjects.MainTrackerInstance.Logic[i].DictionaryName + "\n";

                var check = Playthrough.Find(x => x.Check.ID == i);
                foreach (var j in check.ItemsUsed.OrderBy(x => LogicObjects.MainTrackerInstance.Logic[x].DictionaryName))
                {
                    var L = LogicObjects.MainTrackerInstance.Logic[j];
                    message = message + ">>" + ((L.IsFake) ? L.DictionaryName : L.ItemName ?? L.DictionaryName) + "\n";
                }
            }
            InformationDisplay Display = new InformationDisplay
            {
                Text = Entry.LocationName + " Was Unlocked with:",
                DebugFunction = 4
            };
            InformationDisplay.Playthrough = message.Split(new[] { "\n" }, StringSplitOptions.None).ToList();
            Display.Show();
        }

        public static List<int> ParseLocationAndJunkSettingString(string c, int ItemCount)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(c))
            {
                return result;
            }

            result.Clear();
            string[] Sections = c.Split('-');
            int[] NewSections = new int[ItemCount];
            if (Sections.Length != NewSections.Length) { Console.WriteLine($"Didin't match {Sections.Length}, {NewSections.Length}"); return null; }

            try
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (Sections[(ItemCount - 1) - i] != "") { NewSections[i] = Convert.ToInt32(Sections[(ItemCount - 1) - i], 16); }
                }
                for (int i = 0; i < 32 * ItemCount; i++)
                {
                    int j = i / 32;
                    int k = i % 32;
                    if (((NewSections[j] >> k) & 1) > 0) { result.Add(i); }
                }
            }
            catch
            {
                return null;
            }
            return result;
        }

        public static List<LogicObjects.LogicEntry> ParseEntranceandStartingString(string c, List<LogicObjects.LogicEntry> Subsection )
        {
            if (string.IsNullOrWhiteSpace(c))
            {
                return new List<LogicObjects.LogicEntry>();
            }
            var Entrances = Subsection;
            if (Entrances.Count < 1) { return new List<LogicObjects.LogicEntry>(); }
            var sectionCount = (int)Math.Ceiling(Entrances.Count / 32.0);
            var result = new List<LogicObjects.LogicEntry>();
            if (string.IsNullOrWhiteSpace(c))
            {
                return result;
            }
            try
            {
                string[] v = c.Split('-');
                int[] vi = new int[sectionCount];
                if (v.Length != vi.Length)
                {
                    return null;
                }
                for (int i = 0; i < sectionCount; i++)
                {
                    if (v[sectionCount - 1 - i] != "")
                    {
                        vi[i] = Convert.ToInt32(v[sectionCount - 1 - i], 16);
                    }
                }
                for (int i = 0; i < 32 * sectionCount; i++)
                {
                    int j = i / 32;
                    int k = i % 32;
                    if (((vi[j] >> k) & 1) > 0)
                    {
                        if (i < Entrances.Count)
                        {
                            result.Add(Entrances[i]);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return result;
        }

        public static bool ParseLogicFile(string file = "")
        {
            if (file == "")
            {
                if (!Tools.PromptSave(LogicObjects.MainTrackerInstance)) { return false; }
                file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSAV;*.html)|*.txt;*.MMRTSAV;*.html");
                if (file == "") { return false; }
            }

            var saveFile = file.EndsWith(".MMRTSAV");
            var HTMLLog = file.EndsWith(".html");
            var TextLog = file.EndsWith(".txt") && TestForTextSpoiler(File.ReadAllLines(file));

            string[] RawLogicFile = File.ReadAllLines(file);
            LogicObjects.TrackerInstance SaveFileTemplate = null;
            LogicObjects.GameplaySettings SettingFile = null;

            if (saveFile)
            {
                try { SaveFileTemplate = JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(File.ReadAllText(file)); }
                catch
                {
                    MessageBox.Show("Save File Not Valid.");
                    return false;
                }
                LogicObjects.MainTrackerInstance = SaveFileTemplate;
                return true;
            }
            else if (HTMLLog)
            {
                foreach (var line in RawLogicFile)
                {
                    if (line.StartsWith("<label><b>Settings: </b></label><code style=\"word-break: break-all;\">"))
                    {
                        var newLine = line.Replace("<label><b>Settings: </b></label><code style=\"word-break: break-all;\">", "{\"GameplaySettings\":");
                        newLine = newLine.Replace("</code><br/>", "}");
                        try { SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(newLine).GameplaySettings; }
                        catch  (Exception e)
                        {
                            Console.WriteLine(e);
                            //In 1.15 the setting file in the spoiler log is no longer a single line. New function to read multiline settings.
                            SettingFile = ParseSpoilerLogSettingsWithLineBreak(RawLogicFile);
                            if (SettingFile == null)
                            {
                                MessageBox.Show("HTML Spoiler log had incorrect data!");
                                return false;
                            }
                        }
                        Console.WriteLine(SettingFile.CustomItemListString);
                        break;
                    }
                }
                RawLogicFile = GetLogicFileFromSettings(SettingFile);
                if (RawLogicFile == null) { return false; }

            }
            else if (TextLog)
            {
                foreach (var line in RawLogicFile)
                {
                    if (line.StartsWith("Settings:"))
                    {
                        var Newline = line.Replace("Settings:", "\"GameplaySettings\":");
                        Newline = "{" + Newline + "}";
                        try { SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(Newline).GameplaySettings; }
                        catch 
                        {
                            SettingFile = ParseSpoilerLogSettingsWithLineBreak(RawLogicFile, text: true);
                            if (SettingFile == null)
                            {
                                MessageBox.Show("Text Spoiler log had incorrect data!");
                                return false;
                            }
                        }
                        break;
                    }
                }
                RawLogicFile = GetLogicFileFromSettings(SettingFile);
                if (RawLogicFile == null) { return false; }
            }

            LogicObjects.MainTrackerInstance = new LogicObjects.TrackerInstance();

            Tools.CreateTrackerInstance(LogicObjects.MainTrackerInstance, RawLogicFile.ToArray());

            if (HTMLLog || TextLog)
            {
                LogicEditing.WriteSpoilerLogToLogic(LogicObjects.MainTrackerInstance, file);
                if (!Utility.CheckforSpoilerLog(LogicObjects.MainTrackerInstance.Logic)) { MessageBox.Show("No spoiler data found!"); }
                else if (!Utility.CheckforSpoilerLog(LogicObjects.MainTrackerInstance.Logic, true, Log: true)) { MessageBox.Show("Not all checks have been assigned spoiler data!"); }
            }
            else if (LogicObjects.MainTrackerInstance.EntranceRando && LogicObjects.MainTrackerInstance.Options.UnradnomizeEntranesOnStartup)
            {
                LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled = false;
                LogicObjects.MainTrackerInstance.Options.OverRideAutoEntranceRandoEnable = true;
                foreach (var item in LogicObjects.MainTrackerInstance.Logic)
                {
                    if (item.IsEntrance()) { item.Options = 1; }
                }
            }
            LogicEditing.CalculateItems(LogicObjects.MainTrackerInstance);
            return true;
        }

        public static string[] GetLogicFileFromSettings(LogicObjects.GameplaySettings SettingFile)
        {
            string[] RawLogicFile;
            if (SettingFile.LogicMode == "Casual")
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                string webData = wc.DownloadString("https://raw.githubusercontent.com/ZoeyZolotova/mm-rando/dev/MMR.Randomizer/Resources/REQ_CASUAL.txt");
                RawLogicFile = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }
            else if (SettingFile.LogicMode == "Glitched")
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                string webData = wc.DownloadString("https://raw.githubusercontent.com/ZoeyZolotova/mm-rando/dev/MMR.Randomizer/Resources/REQ_GLITCH.txt");
                RawLogicFile = webData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }
            else
            {
                if (!File.Exists(SettingFile.UserLogicFileName))
                {
                    MessageBox.Show("The logic file used to create this seed could not be found! Ensure it is in the same location and has the same name as when the seed whas generated!");
                    return null;
                }

                if (SettingFile.UserLogicFileName.EndsWith(".json"))
                {
                    LogicObjects.GameplaySettings SettingJSONfromSpoilerLog = null;
                    try { SettingJSONfromSpoilerLog = JsonConvert.DeserializeObject<LogicObjects.Configuration>(File.ReadAllText(SettingFile.UserLogicFileName)).GameplaySettings; }
                    catch { MessageBox.Show("Spoiler Log did not have Usable Logic Data!"); return null; }
                    if (SettingJSONfromSpoilerLog.Logic == "") { MessageBox.Show("Logic not found!"); return null; }
                    RawLogicFile = SettingJSONfromSpoilerLog.Logic.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                }
                else
                {
                    RawLogicFile = File.ReadAllLines(SettingFile.UserLogicFileName);
                }
            }
            return RawLogicFile;
        }

        public static bool TestForTextSpoiler(string[] RawLogicFile)
        {
            foreach (var line in RawLogicFile)
            {
                if (line.StartsWith("Settings:"))
                {
                    var Newline = line.Replace("Settings:", "\"GameplaySettings\":");
                    Newline = "{" + Newline + "}";
                    try { LogicObjects.GameplaySettings SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(Newline).GameplaySettings; }
                    catch { LogicObjects.GameplaySettings SettingFile = ParseSpoilerLogSettingsWithLineBreak(RawLogicFile, text: true); if (SettingFile == null){ return false; }}
                    return true;
                }
            }
            return false;
        }

        public static LogicObjects.GameplaySettings ParseSpoilerLogSettingsWithLineBreak(string[] RawLogicFile, bool text = false)
        {
            string SettingStart = text ? "Settings:" : "<label><b>Settings:";
            string SettingEnd = text ? "Seed:" : "}</code><br/>";

            LogicObjects.GameplaySettings SettingFile = null;
            bool inSettings = false;
            string SettingString = "{\"GameplaySettings\":{";
            foreach (var line in RawLogicFile)
            {
                if (line.StartsWith(SettingEnd) && inSettings)
                {
                    SettingString += text ? "}" : "}}";
                    break;
                }
                if (inSettings)
                {
                    SettingString += line;
                }
                if (line.StartsWith(SettingStart)) { inSettings = true; }
            }


            try { SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(SettingString).GameplaySettings; }
            catch (Exception e)
            {
                Console.WriteLine(e); 
                return null; 
            }
            return SettingFile;
        }

        public static void GetWhatchanged(List<LogicObjects.LogicEntry> Logic, ListBox LB, bool Adding = false)
        {
            if (!Adding) { LogicEditing.LastUpdated = new List<int>(); }
            foreach (var lbi in LB.SelectedItems)
            {
                var i = (lbi is LogicObjects.ListItem) ? (lbi as LogicObjects.ListItem).LocationEntry : lbi;
                if (!(i is LogicObjects.LogicEntry)) { continue; }
                var Item = i as LogicObjects.LogicEntry;
                if (Item.ID < 0) { continue; }
                if (Logic[Item.ID].RandomizedItem < 0) { continue; }
                LogicEditing.LastUpdated.Add(Logic[Item.ID].RandomizedItem);
            }
        }

        public static List<LogicObjects.LogicEntry> PopulateUndoList(List<LogicObjects.LogicEntry> Logic, ListBox LB)
        {
            List<LogicObjects.LogicEntry> UndoList = new List<LogicObjects.LogicEntry>();
            foreach (var lbi in LB.SelectedItems)
            {
                var i = (lbi is LogicObjects.ListItem) ? (lbi as LogicObjects.ListItem).LocationEntry : lbi;
                if (!(i is LogicObjects.LogicEntry)) { continue; }
                var Item = i as LogicObjects.LogicEntry;
                if (Item.ID < 0) { continue; }
                UndoList.Add(Utility.CloneLogicObject(Item));
                if (Logic[Item.ID].RandomizedItem < 0) { continue; }
                UndoList.Add(Utility.CloneLogicObject(Logic[Logic[Item.ID].RandomizedItem]));
            }
            return UndoList;
        }

    }
}
