using MMR_Tracker.Forms;
using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            List<LogicObjects.SpoilerData> SpoilerLog = Tools.ReadHTMLSpoilerLog("", CDLogic);
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
        public static List<LogicObjects.SpoilerData> ReadTextSpoilerlog(LogicObjects.TrackerInstance instance, string[] Spoiler = null)
        {
            List<LogicObjects.SpoilerData> SpoilerData = new List<LogicObjects.SpoilerData>();

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
                    LogicObjects.GameplaySettings SettingFile = new LogicObjects.GameplaySettings();
                    try
                    {
                        SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(line).GameplaySettings;
                        RandomizeOptions ApplySettings = new RandomizeOptions();
                        ApplySettings.ApplyRandomizerSettings(SettingFile);
                        Debugging.Log("Settings Applied");
                    }
                    catch { Debugging.Log(line); }
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
                        if (ExtractPlayerData.Count() > 1 && int.TryParse(ExtractPlayerData[1], out int test))
                        {
                            LineCopy = ExtractPlayerData[0];
                            entry.BelongsTo = test;
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
                        }
                        else
                        {
                            ItemsNotFound.Add(entry.ItemName);
                        }
                        if (ItemExists && NoAvailableItemsEntry != null) 
                        { 
                            Item = NoAvailableItemsEntry; 
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
                        SpoilerData.Add(entry);
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
        
        public static List<LogicObjects.SpoilerData> ReadHTMLSpoilerLog(string Path, LogicObjects.TrackerInstance Instance)
        {
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
                if (SpoilerFile.ShowDialog() != DialogResult.OK) { return SpoilerData; }
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
                    LogicObjects.GameplaySettings SettingFile = new LogicObjects.GameplaySettings();
                    try
                    {
                        SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(newLine).GameplaySettings;
                        RandomizeOptions ApplySettings = new RandomizeOptions();
                        ApplySettings.ApplyRandomizerSettings(SettingFile);
                        Debugging.Log("Settings Applied");
                    }
                    catch { Debugging.Log(newLine); }
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

            if (Instance.EntranceRando) { return SpoilerData; }

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

            return SpoilerData;
        }
        public static bool SaveInstance(LogicObjects.TrackerInstance Instance, bool SetPath = false , string FilePath = "")
        {
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
            Debugging.Log("Clear undo/redo");
            SaveInstance.UndoList.Clear();
            SaveInstance.RedoList.Clear();
            Debugging.Log("Write Data");
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(SaveInstance));
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
            string Gamecode = (string.IsNullOrWhiteSpace(LogicObjects.MainTrackerInstance.GameCode)) ? "MMR" : LogicObjects.MainTrackerInstance.GameCode;
            if (LogicObjects.MainTrackerInstance.UnsavedChanges) { MainInterface.CurrentProgram.Text = $"{Gamecode} Tracker*"; }
            else { MainInterface.CurrentProgram.Text = $"{Gamecode} Tracker"; }
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
        public static void SaveState(LogicObjects.TrackerInstance Instance, List<LogicObjects.LogicEntry> Logic = null )
        {
            int MaxUndoCount = (int)Math.Floor(Math.Pow(((double)5000 / (double)Instance.Logic.Count()), 1.5)); //Reduce the max count based on the size of the logic.
            if (MaxUndoCount < 3) { MaxUndoCount = 3; }
            if (Logic == null) { Logic = Instance.Logic; }
            Instance.UndoList.Add(Utility.CloneLogicList(Logic));
            if (Instance.UndoList.Count() > MaxUndoCount) { Instance.UndoList.RemoveAt(0); }
            Instance.RedoList = new List<List<LogicObjects.LogicEntry>>();
            MainInterface.CurrentProgram.Tools_StateListChanged();
        }
        public static void Undo(LogicObjects.TrackerInstance Instance)
        {
            if (Instance.UndoList.Any())
            {
                SetUnsavedChanges(Instance);
                var lastItem = Instance.UndoList.Count - 1;
                Instance.RedoList.Add(Utility.CloneLogicList(Instance.Logic));
                Instance.Logic = Utility.CloneLogicList(Instance.UndoList[lastItem]);
                Instance.UndoList.RemoveAt(lastItem);
                MainInterface.CurrentProgram.Tools_StateListChanged();
            }
        }
        public static void Redo(LogicObjects.TrackerInstance Instance)
        {
            if (Instance.RedoList.Any())
            {
                SetUnsavedChanges(Instance);
                var lastItem = Instance.RedoList.Count - 1;
                Instance.UndoList.Add(Utility.CloneLogicList(Instance.Logic));
                Instance.Logic = Utility.CloneLogicList(Instance.RedoList[lastItem]);
                Instance.RedoList.RemoveAt(lastItem);
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
        public static void CreateOptionsFile()
        {
            if (!File.Exists("options.txt"))
            {
                List<string> options = new List<string>();
                try
                {
                    var file = File.Create("options.txt");
                    file.Close();
                }
                catch
                {
                    MessageBox.Show("Access Denied. Ensure you have proper access to the folder you are running the tracker from.");
                    System.Windows.Forms.Application.Exit();
                }

                if (!Debugging.ISDebugging || (Control.ModifierKeys == Keys.Shift))
                {
                    var firsttime = MessageBox.Show("It looks like this is your first time running this tracker. If so select Yes. Otherwise, select No.", "First Time Setup", MessageBoxButtons.YesNo);
                    if (firsttime == DialogResult.Yes)
                    {
                        MessageBox.Show("Please Take this opportunity to familliarize yourself with how to use this tracker. There are many features that are not obvious or explained anywhere outside of the about page. This information can be accessed at any time by selecting 'Info' -> 'About'. Click OK to show the About Page. Once you have read through the information, close the window to return to setup.", "How to Use", MessageBoxButtons.OK);
                        InformationDisplay DebugScreen = new InformationDisplay { DebugFunction = 2 };
                        DebugScreen.ShowDialog();
                    }

                    var DefaultSetting = MessageBox.Show("If you would like to change the default options, press Yes. Otherwise, press No. Selecting Cancel at an option prompt will leave it default. These can be changed later in the Options text document that will be created in your tracker folder. The can also be changed per instance in the options tab", "Default Setting", MessageBoxButtons.YesNo);
                    if (DefaultSetting == DialogResult.Yes) 
                    {
                        var ShowToolTips = MessageBox.Show("Would you like to see tooltips that display the full name of an item when you mouse over it?", "Show Tool Tips", MessageBoxButtons.YesNoCancel);
                        if (ShowToolTips == DialogResult.No) { options.Add("ToolTips:0"); }
                        else { options.Add("ToolTips:1"); }

                        var SeperateMArkedItems = MessageBox.Show("Would you like Marked locations to be moved to the bottom of the list?", "Show Tool Tips", MessageBoxButtons.YesNoCancel);
                        if (SeperateMArkedItems == DialogResult.Yes) { options.Add("SeperateMarked:1"); }
                        else { options.Add("SeperateMarked:0"); }

                        var MiddleClickFunction = MessageBox.Show("Would you like Middle Click to star a location instead of Setting it? Staring a location will allow you to mark it as important so you can easily refrence it later. Setting a location will add what item or entrance is placed at that location. (Yes: Star, No: Set)", "Star or Set", MessageBoxButtons.YesNoCancel);
                        if (MiddleClickFunction == DialogResult.Yes) { options.Add("MiddleClickFunction:1"); }
                        else { options.Add("MiddleClickFunction:0"); }

                        var DisableEntrances = MessageBox.Show("Would you like the tracker to automatically mark entrances as unrandomized when creating an instance? This is usefull if you don't plan to use entrance randomizer often.", "Start with Entrance Rando", MessageBoxButtons.YesNoCancel);
                        if (DisableEntrances == DialogResult.No) { options.Add("DisableEntrancesOnStartup:0"); }
                        else { options.Add("DisableEntrancesOnStartup:1"); }

                        var UpdateCheck = MessageBox.Show("Would you like the tracker to notify you when a newer version has been released?", "Check For Updates", MessageBoxButtons.YesNoCancel);
                        if (UpdateCheck == DialogResult.No) { options.Add("CheckForUpdates:0"); }
                        else { options.Add("CheckForUpdates:1"); }
                    }
                }

                SetDefault();
                File.WriteAllLines("options.txt", options);

                void SetDefault()
                {
                    if (!options.Any(x => x.Contains("ToolTips"))) { options.Add("ToolTips:1"); }
                    if (!options.Any(x => x.Contains("SeperateMarked"))) { options.Add("SeperateMarked:0"); }
                    if (!options.Any(x => x.Contains("MiddleClickFunction"))) { options.Add("MiddleClickFunction:0"); }
                    if (!options.Any(x => x.Contains("DisableEntrancesOnStartup"))) { options.Add("DisableEntrancesOnStartup:0"); }
                    if (!options.Any(x => x.Contains("CheckForUpdates"))) { options.Add("CheckForUpdates:1"); }
                }

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
            Tools.SaveFilePath = "";
            Instance.RawLogicFile = RawLogic;
            LogicEditing.PopulateTrackerInstance(Instance);
            LogicEditing.CalculateItems(Instance);

            if (!Instance.IsMM() && (!File.Exists("options.txt") || File.ReadAllLines("options.txt").ToList().Find(x => x.Contains("OtherLogicOK")) == null) && !Debugging.ISDebugging)
            {
                DialogResult dialogResult = MessageBox.Show("This logic file was NOT created for the Majoras Mask Randomizer. While this tracker can support other games, support is very Limited. Many features will be disabled and core features might not work as intended. Do you wish to continue? If so this prompt will no longer display.", "Other Randomizer", MessageBoxButtons.YesNo);
                if (dialogResult != DialogResult.Yes) { Instance = new LogicObjects.TrackerInstance(); return; }
                try { using (StreamWriter w = File.AppendText("options.txt")) { w.WriteLine("OtherLogicOK"); } }
                catch { }

            }
            else if (Instance.LogicVersion < 8 && Instance.IsMM())
            {
                DialogResult dialogResult = MessageBox.Show("You are using a version of logic that is not supported by this tracker. Any logic version lower than version 8 (Randomizer version 1.8) may not work as intended. Do you wish to continue?", "Unsupported Version", MessageBoxButtons.YesNo);
                if (dialogResult != DialogResult.Yes) { Instance = new LogicObjects.TrackerInstance(); return; }
            }

            if (File.Exists("options.txt"))
            {
                Instance.Options.ShowEntryNameTooltip = true;
                Instance.Options.MoveMarkedToBottom = false;
                Instance.Options.UnradnomizeEntranesOnStartup = true;
                Instance.Options.CheckForUpdate = true;
                Instance.Options.MiddleClickStarNotMark = false;
                foreach (var i in File.ReadAllLines("options.txt"))
                {
                    if (i.Contains("ToolTips:0")) { Instance.Options.ShowEntryNameTooltip = false; }
                    if (i.Contains("SeperateMarked:1")) { Instance.Options.MoveMarkedToBottom = true; }
                    if (i.Contains("DisableEntrancesOnStartup:0")) { Instance.Options.UnradnomizeEntranesOnStartup = false; }
                    if (i.Contains("CheckForUpdates:0")) { Instance.Options.CheckForUpdate = false; }
                    if (i.Contains("MiddleClickFunction:1")) { Instance.Options.MiddleClickStarNotMark = true; }
                }
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
            foreach (var i in ItemsUsed) { message = message + (LogicObjects.MainTrackerInstance.Logic[i].ItemName ?? LogicObjects.MainTrackerInstance.Logic[i].DictionaryName) + "\n"; }
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
        public static List<int> ParseLocationAndJunkSettingString(string c)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(c))
            {
                return result;
            }
            try
            {
                result.Clear();
                string[] Sections = c.Split('-');
                int[] NewSections = new int[13];
                if (Sections.Length != NewSections.Length) { return null; }
                for (int i = 0; i < 13; i++)
                {
                    if (Sections[12 - i] != "") { NewSections[i] = Convert.ToInt32(Sections[12 - i], 16); }
                }
                for (int i = 0; i < 32 * 13; i++)
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
        public static List<LogicObjects.LogicEntry> ParseEntranceandStartingString(LogicObjects.TrackerInstance Instance, string c)
        {
            if (string.IsNullOrWhiteSpace(c))
            {
                return new List<LogicObjects.LogicEntry>();
            }
            var Entrances = Instance.Logic.Where(x => x.IsEntrance()).ToList();
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
                RawLogicFile = SaveFileTemplate.RawLogicFile;
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
                        catch { MessageBox.Show("Not a valid HTML Spoiler Log!"); return false; }
                        break;
                    }
                }
                if (!setLogicFile()) { return false; }

            }
            else if (TextLog)
            {
                foreach (var line in RawLogicFile)
                {
                    if (line.StartsWith("Settings:"))
                    {
                        var Newline = line.Replace("Settings:", "\"GameplaySettings\":");
                        Newline = "{" + Newline + "}";
                        SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(Newline).GameplaySettings;
                        break;
                    }
                }
                if (!setLogicFile()) { return false; }
            }

            bool setLogicFile()
            {
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
                    if (!File.Exists(SettingFile.UserLogicFileName)) { return false; }
                    RawLogicFile = File.ReadAllLines(SettingFile.UserLogicFileName);
                }
                return true;
            }

            LogicObjects.MainTrackerInstance = new LogicObjects.TrackerInstance();

            Tools.CreateTrackerInstance(LogicObjects.MainTrackerInstance, RawLogicFile.ToArray());

            if (saveFile)
            {
                LogicObjects.MainTrackerInstance.Options = SaveFileTemplate.Options;
                foreach (var i in LogicObjects.MainTrackerInstance.Logic)
                {
                    var TemplateData = SaveFileTemplate.Logic.Find(x => x.DictionaryName == i.DictionaryName);
                    if (TemplateData != null)
                    {
                        i.Options = TemplateData.Options;
                        i.TrickEnabled = TemplateData.TrickEnabled;
                    }
                }
            }
            else if (HTMLLog || TextLog)
            {
                LogicEditing.WriteSpoilerLogToLogic(LogicObjects.MainTrackerInstance, file);
                if (!Utility.CheckforSpoilerLog(LogicObjects.MainTrackerInstance.Logic)) { MessageBox.Show("No spoiler data found!"); }
                else if (!Utility.CheckforSpoilerLog(LogicObjects.MainTrackerInstance.Logic, true)) { MessageBox.Show("Not all checks have been assigned spoiler data!"); }
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
        public static bool TestForTextSpoiler(string[] RawLogicFile)
        {
            foreach (var line in RawLogicFile)
            {
                if (line.StartsWith("Settings:"))
                {
                    var Newline = line.Replace("Settings:", "\"GameplaySettings\":");
                    Newline = "{" + Newline + "}";
                    try { LogicObjects.GameplaySettings SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(Newline).GameplaySettings; }
                    catch { return false; }
                    return true;
                }
            }
            return false;
        }
        public static void HandleUserPreset(object sender, EventArgs e)
        {
            MainInterface.CurrentProgram.presetsToolStripMenuItem.DropDownItems.Clear();
            MainInterface.CurrentProgram.changeLogicToolStripMenuItem.DropDownItems.Clear();
            if (LogicEditor.EditorForm != null)
            {
                LogicEditor.EditorForm.templatesToolStripMenuItem.DropDownItems.Clear();
            }
            List<ToolStripMenuItem> NewPresets = new List<ToolStripMenuItem>();
            List<ToolStripMenuItem> RecreatePresets = new List<ToolStripMenuItem>();
            List<ToolStripMenuItem> LogicEditorPresets = new List<ToolStripMenuItem>();
            int counter = 0;
            if (!Directory.Exists(@"Recources\Other Files\Custom Logic Presets"))
            {
                try
                {
                    Directory.CreateDirectory((@"Recources\Other Files\Custom Logic Presets"));
                }
                catch { }
            }
            else
            {
                foreach (var i in Directory.GetFiles(@"Recources\Other Files\Custom Logic Presets").Where(x => x.EndsWith(".txt") && !x.Contains("Web Presets.txt")))
                {
                    ToolStripMenuItem CustomLogicPreset = new ToolStripMenuItem
                    {
                        Name = $"PresetNewLogic{counter}",
                        Size = new System.Drawing.Size(180, 22),
                        Text = Path.GetFileName(i).Replace(".txt", "")
                    };
                    counter++;
                    CustomLogicPreset.Click += (s, ee) => MainInterface.CurrentProgram.LoadLogicPreset(i, "", s, ee);
                    NewPresets.Add(CustomLogicPreset);

                    ToolStripMenuItem CustomLogicPresetRecreate = new ToolStripMenuItem
                    {
                        Name = $"PresetChangeLogic{counter}",
                        Size = new System.Drawing.Size(180, 22),
                        Text = Path.GetFileName(i).Replace(".txt", "")
                    };
                    counter++;
                    CustomLogicPresetRecreate.Click += (s, ee) => MainInterface.CurrentProgram.LoadLogicPreset(i, "", s, ee, false);
                    RecreatePresets.Add(CustomLogicPresetRecreate);

                    ToolStripMenuItem CustomLogicPresetLogicEditor = new ToolStripMenuItem
                    {
                        Name = $"PresetLogicEditor{counter}",
                        Size = new System.Drawing.Size(180, 22),
                        Text = Path.GetFileName(i).Replace(".txt", "")
                    };
                    counter++;
                    CustomLogicPresetLogicEditor.Click += (s, ee) => LogicEditor.LoadLogicPreset(i, "");
                    LogicEditorPresets.Add(CustomLogicPresetLogicEditor);
                }
            }
            AddDevPresets();
            if (File.Exists(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt"))
            {
                var TextFile = File.ReadAllLines(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt");
                AddPersonalPresets(TextFile);
                ToolStripMenuItem CustomLogicPreset = new ToolStripMenuItem();
                ToolStripMenuItem CustomLogicPresetRecreate = new ToolStripMenuItem();
                ToolStripMenuItem CustomLogicPresetEditor = new ToolStripMenuItem();
                foreach (var i in TextFile)
                {
                    if (i.StartsWith("Name:"))
                    {
                        CustomLogicPreset.Name = $"PresetNewLogic{counter}";
                        CustomLogicPreset.Text = Regex.Replace(i, "Name:", "", RegexOptions.IgnoreCase).Trim();

                        CustomLogicPresetRecreate.Name = $"PresetChangeLogic{counter}";
                        CustomLogicPresetRecreate.Text = Regex.Replace(i, "Name:", "", RegexOptions.IgnoreCase).Trim();

                        CustomLogicPresetEditor.Name = $"PresetChangeLogic{counter}";
                        CustomLogicPresetEditor.Text = Regex.Replace(i, "Name:", "", RegexOptions.IgnoreCase).Trim();
                    }
                    else if (i.StartsWith("Address:"))
                    {
                        CustomLogicPreset.Click += (s, ee) => MainInterface.CurrentProgram.LoadLogicPreset("", Regex.Replace(i, "Address:", "", RegexOptions.IgnoreCase).Trim(), s, ee);
                        NewPresets.Add(CustomLogicPreset);
                        CustomLogicPreset = new ToolStripMenuItem();

                        CustomLogicPresetRecreate.Click += (s, ee) => MainInterface.CurrentProgram.LoadLogicPreset("", Regex.Replace(i, "Address:", "", RegexOptions.IgnoreCase).Trim(), s, ee, false);
                        RecreatePresets.Add(CustomLogicPresetRecreate);
                        CustomLogicPresetRecreate = new ToolStripMenuItem();

                        CustomLogicPresetEditor.Click += (s, ee) => LogicEditor.LoadLogicPreset("", Regex.Replace(i, "Address:", "", RegexOptions.IgnoreCase).Trim());
                        LogicEditorPresets.Add(CustomLogicPresetEditor);
                        CustomLogicPresetEditor = new ToolStripMenuItem();
                    }
                    counter++;
                }
            }
            if (NewPresets.Count() < 1)
            {
                ToolStripMenuItem CustomLogicPreset = new ToolStripMenuItem
                {
                    Name = "newToolStripMenuItem",
                    Size = new System.Drawing.Size(180, 22),
                    Text = "No Presets Found (Open Folder)"
                };
                CustomLogicPreset.Click += (s, ee) => MainInterface.CurrentProgram.presetsToolStripMenuItem_Click(s, ee);
                MainInterface.CurrentProgram.presetsToolStripMenuItem.DropDownItems.Add(CustomLogicPreset);
            }
            else
            {
                foreach (var i in NewPresets.OrderBy(x => x.Text))
                {
                    MainInterface.CurrentProgram.presetsToolStripMenuItem.DropDownItems.Add(i);
                }
                foreach (var i in RecreatePresets.OrderBy(x => x.Text))
                {
                    MainInterface.CurrentProgram.changeLogicToolStripMenuItem.DropDownItems.Add(i);
                }
                if (LogicEditor.EditorForm != null)
                {
                    foreach (var i in LogicEditorPresets.OrderBy(x => x.Text))
                    {
                        LogicEditor.EditorForm.templatesToolStripMenuItem.DropDownItems.Add(i);
                    }
                }
                ToolStripMenuItem newRecreatePreset = new ToolStripMenuItem
                {
                    Name = $"PresetChangeLogic{counter + 1}",
                    Size = new System.Drawing.Size(180, 22),
                    Text = "Browse"
                };
                newRecreatePreset.Click += (s, ee) => LogicEditing.RecreateLogic(LogicObjects.MainTrackerInstance);
                MainInterface.CurrentProgram.changeLogicToolStripMenuItem.DropDownItems.Add(newRecreatePreset);
            }

            void AddPersonalPresets(string[] TextFile)
            {
                if (Debugging.ISDebugging || Environment.MachineName == "TIMOTHY-PC")
                {
                    if (!TextFile.Contains("Name: Thedrummonger Glitched Logic"))
                    {
                        File.AppendAllText(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt", "\nName: Thedrummonger Glitched Logic");
                        File.AppendAllText(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt", "\nAddress: https://raw.githubusercontent.com/Thedrummonger/MMR-Logic/master/Logic%20File.txt");
                    }
                    if (!TextFile.Contains("Name: Thedrummonger Entrance Rando"))
                    {
                        File.AppendAllText(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt", "\nName: Thedrummonger Entrance Rando");
                        File.AppendAllText(@"Recources\Other Files\Custom Logic Presets\Web Presets.txt", "\nAddress: https://raw.githubusercontent.com/Thedrummonger/MMR-Logic/Entrance-Radno-Logic/Logic%20File.txt");
                    }
                }
            }
            void AddDevPresets()
            {
                if (!Debugging.ISDebugging || !Directory.Exists(@"Recources\Other Files\Other Game Premade Logic")) { return; }
                foreach (var i in Directory.GetFiles(@"Recources\Other Files\Other Game Premade Logic").Where(x => x.EndsWith(".txt") && !x.Contains("Web Presets.txt")))
                {
                    ToolStripMenuItem CustomLogicPreset = new ToolStripMenuItem
                    {
                        Name = $"PresetNewLogic{counter}",
                        Size = new System.Drawing.Size(180, 22),
                        Text = Path.GetFileName(i).Replace(".txt", "") + " DEV"
                    };
                    counter++;
                    CustomLogicPreset.Click += (s, ee) => MainInterface.CurrentProgram.LoadLogicPreset(i, "", s, ee);
                    NewPresets.Add(CustomLogicPreset);

                    ToolStripMenuItem CustomLogicPresetRecreate = new ToolStripMenuItem
                    {
                        Name = $"PresetChangeLogic{counter}",
                        Size = new System.Drawing.Size(180, 22),
                        Text = Path.GetFileName(i).Replace(".txt", "") + " DEV"
                    };
                    counter++;
                    CustomLogicPresetRecreate.Click += (s, ee) => MainInterface.CurrentProgram.LoadLogicPreset(i, "", s, ee, false);
                    RecreatePresets.Add(CustomLogicPresetRecreate);

                    ToolStripMenuItem CustomLogicPresetEditor = new ToolStripMenuItem
                    {
                        Name = $"PresetChangeLogic{counter}",
                        Size = new System.Drawing.Size(180, 22),
                        Text = Path.GetFileName(i).Replace(".txt", "") + " DEV"
                    };
                    counter++;
                    CustomLogicPresetEditor.Click += (s, ee) => LogicEditor.LoadLogicPreset(i, "");
                    LogicEditorPresets.Add(CustomLogicPresetEditor);
                }
            }
        }

    }
}
