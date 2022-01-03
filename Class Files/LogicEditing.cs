using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    class LogicEditing
    {
        public static List<int> LastUpdated = new List<int>();
        public static bool PopulateTrackerInstance(LogicObjects.TrackerInstance instance)
        {
            Console.WriteLine($"Determining Logic Type");
            LogicObjects.LogicFile NewformatLogicFile = null;
            try
            {
                NewformatLogicFile = LogicObjects.LogicFile.FromJson(string.Join("", instance.RawLogicFile)); 
            }
            catch
            {
                Console.WriteLine($"JSON Parse Failed, assuming old Logic Format");
            }

            if (NewformatLogicFile == null)
            {
                Console.WriteLine($"Logic Type Was txt, Parsing...");
                instance.LogicFormat = "txt";
                return PopulateTXTTrackerInstance(instance);
            }
            else
            {
                Console.WriteLine($"Logic Type Was json, Parsing...");
                instance.LogicFormat = "json";
                return PopulateJSONTrackerInstance(instance);
            }

        }

        public static bool PopulateJSONTrackerInstance(LogicObjects.TrackerInstance instance)
        {
            LogicObjects.LogicFile NewformatLogicFile = LogicObjects.LogicFile.FromJson(string.Join("", instance.RawLogicFile));
            instance.Logic.Clear();
            instance.DicNameToID.Clear();
            instance.EntrancePairs.Clear();
            instance.LogicVersion = NewformatLogicFile.Version;
            instance.GameCode = NewformatLogicFile.GameCode ?? "MMR";

            LogicObjects.LogicDictionary MasterDic = null;

            if (instance.LogicDictionary == null || instance.LogicDictionary.LogicDictionaryList.Count < 1)
            {
                Console.WriteLine($"Attempting to Find Dictionary matching \nGamecode:{instance.GameCode}\nLogic Version: {instance.LogicVersion}");
                string DictionaryPath = VersionHandeling.GetJSONDictionaryPath(instance);
                if (!string.IsNullOrWhiteSpace(DictionaryPath))
                {
                    try
                    {
                        Console.WriteLine($"Attempting to Parse Dictionary at {DictionaryPath}");
                        MasterDic = JsonConvert.DeserializeObject<LogicObjects.LogicDictionary>(File.ReadAllText(DictionaryPath));
                        instance.LogicDictionary = MasterDic;
                        Console.WriteLine($"Dictionary Parsed");
                    }
                    catch { MessageBox.Show($"The Dictionary File \"{DictionaryPath}\" has been corrupted. The tracker will not function correctly."); }
                }
                else { MessageBox.Show($"A valid dictionary file could not be found for this logic. The tracker will not function correctly."); }
            }

            Dictionary<string, int> LogicNametoId = new Dictionary<string, int>();
            int DicCounter = 0;
            foreach (var i in NewformatLogicFile.Logic)
            {
                LogicNametoId.Add(i.Id, DicCounter);
                DicCounter++;
            }

            foreach (var i in NewformatLogicFile.Logic)
            {
                LogicObjects.LogicEntry LogicEntry1 = new LogicObjects.LogicEntry
                {
                    ID = NewformatLogicFile.Logic.IndexOf(i),
                    DictionaryName = i.Id,
                    Checked = false,
                    RandomizedItem = -2,
                    RandomizerStaticFakeItem = false,
                    SpoilerRandom = -2,
                    Required = i.RequiredItems?.Select(x => LogicNametoId[x]).ToArray(),
                    Conditionals = i.ConditionalItems?.Select(x => x.Select(y => LogicNametoId[y]).ToArray()).ToArray(),
                    NeededBy = (int)i.TimeNeeded,
                    AvailableOn = (int)i.TimeAvailable,
                    TimeSetup = (int)i.TimeSetup,
                    IsTrick = i.IsTrick,
                    TrickEnabled = i.IsTrick,
                    TrickToolTip = i.TrickTooltip
                };

                var DicEntry = instance.LogicDictionary.LogicDictionaryList.Find(x => x.DictionaryName == LogicEntry1.DictionaryName);
                if (DicEntry != null)
                {
                    LogicEntry1.IsFake = DicEntry.FakeItem;
                    LogicEntry1.RandomizerStaticFakeItem = DicEntry.FakeItem;
                    LogicEntry1.SpoilerPriceName = DicEntry.SpoilerPriceLocations?.ToList();

                    LogicEntry1.ItemName = (string.IsNullOrWhiteSpace(DicEntry.ItemName)) ? null : DicEntry.ItemName;
                    LogicEntry1.LocationName = (string.IsNullOrWhiteSpace(DicEntry.LocationName)) ? null : DicEntry.LocationName;
                    LogicEntry1.ProgressiveItemData = DicEntry.ProgressiveItemData;

                    if (!LogicEntry1.IsFake)
                    {
                        LogicEntry1.LocationArea = (string.IsNullOrWhiteSpace(DicEntry.LocationArea)) ? "Misc" : DicEntry.LocationArea;
                        LogicEntry1.ItemSubType = (string.IsNullOrWhiteSpace(DicEntry.ItemSubType)) ? "Item" : DicEntry.ItemSubType;
                        LogicEntry1.SpoilerLocation = DicEntry.SpoilerLocation == null ? null : DicEntry.SpoilerLocation.ToList();
                        LogicEntry1.SpoilerItem = DicEntry.SpoilerItem == null ? null : DicEntry.SpoilerItem.ToList();
                    }
                }

                //Push Data to the instance
                instance.Logic.Add(LogicEntry1);
            }

            instance.EntranceRando = instance.IsEntranceRando();
            instance.CreateAreaClearDictionary(instance.LogicDictionary.LogicDictionaryList);
            instance.CreateWalletDictionary(instance.LogicDictionary);
            instance.CreateDicNameToID();
            instance.Keys["SmallKeys"] = instance.CreateKeyDictionary(instance.LogicDictionary.LogicDictionaryList, "small");
            instance.Keys["BossKeys"] = instance.CreateKeyDictionary(instance.LogicDictionary.LogicDictionaryList, "boss");
            instance.Keys["ChecksNeedingKeys"] = instance.GetChecksNeedingKeys();
            instance.RandoOnlyLogicRequirements = instance.GetUselessLogicItems();
            if (instance.EntranceRando) { CreatedEntrancepairDcitionary(instance); }
            MarkUniqeItemsUnrandomizedManual(instance);
            Utility.nullEmptyLogicItems(instance.Logic);

            if (instance.GameCode != "MMR") { instance.Options.StrictLogicHandeling = true; }

            return true;

        }

        public static bool PopulateTXTTrackerInstance(LogicObjects.TrackerInstance instance)
        {
            /* Sets the Values of the follwing using the data in instance.RawLogicFile
             * Version
             * Game
             * Entrance Area Dictionary
             * Logic
             * LogicDictionary
             * Name to ID Dictionary
             * Entrance pair Dictionary
             */
            instance.Logic.Clear();
            instance.DicNameToID.Clear();
            instance.EntrancePairs.Clear();
            LogicObjects.VersionInfo versionData = VersionHandeling.GetVersionDataFromLogicFile(instance.RawLogicFile);
            instance.LogicVersion = versionData.Version;
            instance.GameCode = versionData.Gamecode;
            int SubCounter = 0;
            int idCounter = 0;

            foreach(var i in instance.RawLogicFile)
            {
                if (i.Trim() == "- EntranceMajorasLairFromTheMoon")
                {
                    instance.LogicFormat = "entrance";
                    break;
                }
            }

            LogicObjects.LogicDictionary MasterDic = null;

            if (instance.LogicDictionary == null || instance.LogicDictionary.LogicDictionaryList.Count < 1)
            {
                string DictionaryPath = VersionHandeling.GetJSONDictionaryPath(instance);
                if (!string.IsNullOrWhiteSpace(DictionaryPath))
                {
                    try
                    {
                        MasterDic = JsonConvert.DeserializeObject<LogicObjects.LogicDictionary>(File.ReadAllText(DictionaryPath));
                        instance.LogicDictionary = MasterDic;
                    }
                    catch { MessageBox.Show($"The Dictionary File \"{DictionaryPath}\" has been corrupted. The tracker will not function correctly."); }
                }
                else { MessageBox.Show($"A valid dictionary file could not be found for this logic. The tracker will not function correctly."); }
            }

            LogicObjects.LogicEntry LogicEntry1 = new LogicObjects.LogicEntry();
            var NextLine = 1;
            foreach (string line in instance.RawLogicFile)
            {
                if (NextLine == 1) { NextLine++; continue; }
                if (line.StartsWith("-")) { SubCounter = 0; }
                switch (SubCounter)
                {
                    case 0:
                        LogicEntry1.ID = idCounter;
                        LogicEntry1.DictionaryName = line.Substring(2);
                        LogicEntry1.Checked = false;
                        LogicEntry1.RandomizedItem = -2;
                        LogicEntry1.IsFake = true;
                        LogicEntry1.SpoilerRandom = -2;

                        var DicEntry = instance.LogicDictionary.LogicDictionaryList.Find(x => x.DictionaryName == LogicEntry1.DictionaryName);
                        if (DicEntry == null) { break; }

                        LogicEntry1.IsFake = DicEntry.FakeItem;
                        LogicEntry1.RandomizerStaticFakeItem = DicEntry.FakeItem;
                        LogicEntry1.IsTrick = false;
                        LogicEntry1.TrickEnabled = true;
                        LogicEntry1.TrickToolTip = "";
                        LogicEntry1.ItemName = (string.IsNullOrWhiteSpace(DicEntry.ItemName)) ? null : DicEntry.ItemName;
                        LogicEntry1.LocationName = (string.IsNullOrWhiteSpace(DicEntry.LocationName)) ? null : DicEntry.LocationName;
                        LogicEntry1.LocationArea = (string.IsNullOrWhiteSpace(DicEntry.LocationArea)) ? "Misc" : DicEntry.LocationArea;
                        LogicEntry1.ItemSubType = (string.IsNullOrWhiteSpace(DicEntry.ItemSubType)) ? "Item" : DicEntry.ItemSubType;
                        LogicEntry1.SpoilerLocation = DicEntry.SpoilerLocation == null ? null : DicEntry.SpoilerLocation.ToList();
                        LogicEntry1.SpoilerItem = DicEntry.SpoilerItem == null ? null : DicEntry.SpoilerItem.ToList();
                        break;
                    case 1:
                        if (string.IsNullOrWhiteSpace(line)) { LogicEntry1.Required = null; break; }
                        LogicEntry1.Required = line.Split(',').Select(y => int.Parse(y)).ToArray();
                        break;
                    case 2:
                        if (string.IsNullOrWhiteSpace(line)) { LogicEntry1.Conditionals = null; break; }
                        LogicEntry1.Conditionals = line.Split(';').Select(x => x.Split(',').Select(y => int.Parse(y)).ToArray()).ToArray();
                        break;
                    case 3:
                        LogicEntry1.NeededBy = Convert.ToInt32(line);
                        break;
                    case 4:
                        LogicEntry1.AvailableOn = Convert.ToInt32(line);
                        break;
                    case 5:
                        LogicEntry1.IsTrick = (line.StartsWith(";"));
                        LogicEntry1.TrickEnabled = true;
                        LogicEntry1.TrickToolTip = (line.Length > 1) ? line.Substring(1) : "No Tooltip Available";
                        //if (LogicEntry1.IsTrick) { Debugging.Log($"Trick {LogicEntry1.DictionaryName} Found. ToolTip =  { LogicEntry1.TrickToolTip }"); }
                        break;
                }
                if ((NextLine) >= instance.RawLogicFile.Count() || instance.RawLogicFile[NextLine].StartsWith("-"))
                {
                    //Push Data to the instance
                    instance.Logic.Add(LogicEntry1);
                    LogicEntry1 = new LogicObjects.LogicEntry();
                    idCounter++;
                }
                NextLine++;
                SubCounter++;
            }

            instance.EntranceRando = instance.IsEntranceRando();
            instance.CreateAreaClearDictionary(instance.LogicDictionary.LogicDictionaryList);
            instance.CreateDicNameToID();
            if (instance.EntranceRando) { CreatedEntrancepairDcitionary(instance); }
            MarkUniqeItemsUnrandomizedManual(instance);
            Utility.nullEmptyLogicItems(instance.Logic);

            return true;
        }

        public static void MarkUniqeItemsUnrandomizedManual(LogicObjects.TrackerInstance Instance)
        {
            foreach (var i in Instance.Logic) { if (Instance.Logic.Where(x => x.ItemSubType == i.ItemSubType).Count() < 2) { i.SetUnRandomizedManual(); } }
        }

        public static bool RequirementsMet(int[] DefaultItemlist, LogicObjects.TrackerInstance logic, List<int> usedItems = null)
        {
            usedItems = usedItems ?? new List<int>();
            if (DefaultItemlist == null || DefaultItemlist.Count() < 1) { return true; }
            foreach (var i in DefaultItemlist)
            {
                if (logic.Logic[i].IsTrick && !logic.Logic[i].TrickEnabled) { return false; }//There should never be tricks in the requirements, but this is a good fallback
                if (!logic.Logic[i].ItemUseable(logic, usedItems)) { return false; }
            }
            return true;
        }

        public static bool CondtionalsMet(int[][] list, LogicObjects.TrackerInstance logic, List<int> usedItems = null)
        {
            usedItems = usedItems ?? new List<int>();
            if (list == null || !list.Any()) { return true; }
            //Remove any lines from the conditional that contain disabled tricks
            var ValidListEntries = list.Where(x => !x.Where(y => logic.Logic[y].IsTrick && !logic.Logic[y].TrickEnabled).Any());
            if (!ValidListEntries.Any()) { return false; ; } //See: Conditional with only tricks
            foreach (var i in ValidListEntries)
            {
                List<int> UsedItemsSet = new List<int>();
                if (RequirementsMet(i, logic, UsedItemsSet))
                {
                    foreach (var set in UsedItemsSet) { usedItems.Add(set); }
                    return true;
                }
            }
            return false;

            #region Conditional with only tricks
            /*This function will return false if there are no conditional entries left after disabled tricks have been removed.
            The randomizer does the opposite, returning true if the conditional only contained tricks and all those tricks were removed.
            This works for the randomizer because it requires there be at least one entry with no tricks to fall back on.
            In the randomizer, if a check is unobtainable it is given moon access as a requirement, or conditional if tricks can make it obtainable.
            It has to do this since, while moon access does make sure the location can't contain something needed to beat the game, it is eventually obtainable.
            The randomizer errors if any check is unobtainable, even if the check is not needed to beat the game. The tracker doesn't care.
            By having the tracker return false if all conditionals are inavlid tricks it gives the tracker an option to make checks unobtainable.
            Since the randomizer expects logic will always have at least one conditional with no tricks to fall back on, 
            this will only cause inconsistancy if logic was used that is invalid in the randomizer anyway.*/
            #endregion

        }

        public static void CalculateItems(LogicObjects.TrackerInstance Instance, bool ForceStrictLogicHendeling = false, bool InitialRun = true, bool fromScratch = true)
        {
            if (InitialRun)
            {
                Utility.nullEmptyLogicItems(Instance.Logic);
                if ((Instance.Options.StrictLogicHandeling || ForceStrictLogicHendeling)) { Instance.RefreshFakeItems(); }
            }
            CalculateFakeItems(Instance, ForceStrictLogicHendeling, fromScratch);
            //Once all the fake items are unlocked, see what real items are available.
            foreach (var item in Instance.Logic.Where(x => !x.IsFake && !x.Unrandomized()))
            {
                item.Available = item.CheckAvailability(Instance, FromScratch: fromScratch, ForceStrictLogicHendeling: ForceStrictLogicHendeling);
            }
        }

        public static void CalculateFakeItems(LogicObjects.TrackerInstance Instance, bool ForceStrictLogicHendeling = false, bool fromScratch = true)
        {
            bool recalculate = false;
            //Calculate all fake items. If the fake item is available, set it to aquired
            foreach (var item in Instance.Logic.Where(x => x.IsFake || x.Unrandomized()))
            {
                item.Available = item.CheckAvailability(Instance, FromScratch: fromScratch, ForceStrictLogicHendeling: ForceStrictLogicHendeling);
                if (item.FakeItemStatusChange()) { recalculate = true; }
            }
            //If any fake items are unlocked run this funtion again to check if any fake items were unlocked as a result.
            if (recalculate) { CalculateFakeItems(Instance, ForceStrictLogicHendeling, fromScratch); } 
        }

        public static void WriteSpoilerLogToLogic(LogicObjects.TrackerInstance Instance, string path, bool ApplySetting = true)
        {
            List<LogicObjects.SpoilerData> SpoilerData = new List<LogicObjects.SpoilerData>();
            LogicObjects.GameplaySettings SettingsData = null;
            Dictionary<string, int> Pricedata = new Dictionary<string, int>();
            Dictionary<string, string> Hintdata = new Dictionary<string, string>();
            if (path.Contains(".txt") || path.Contains(".json"))
            {
                bool TXTOverride = false;
                if (Instance.IsMM())
                {
                    var txs = MessageBox.Show("If possible, the HTML spoiler log should always be imported in place of the text spoiler log.\n\n The text spoiler log will " +
                        "work most of the time but may be inconsistent.\n\nWould you like to select an HTML spoiler log instead?", "Text log used", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (txs == DialogResult.Yes)
                    {
                        var HTMLPath = Utility.FileSelect("Select an HTML Spoiler Log", "Spoiler Log (*html)|*html");
                        if (HTMLPath != "")
                        {
                            TXTOverride = true;
                            LogicObjects.SpoilerLogData SPLD = Tools.ReadHTMLSpoilerLog(HTMLPath, Instance);
                            SpoilerData = SPLD.SpoilerDatas;
                            Pricedata = SPLD.Pricedata;
                            SettingsData = SPLD.SettingString;
                            Hintdata = SPLD.GossipHints;
                        }
                    }
                }
                if (!TXTOverride) 
                {
                    var LogData = Tools.ReadTextSpoilerlog(Instance, File.ReadAllLines(path));
                    if (LogData == null) { return; }
                    SpoilerData = LogData.SpoilerDatas;
                    SettingsData = LogData.SettingString;
                }
            }
            else if (path.Contains(".html"))
            {
                LogicObjects.SpoilerLogData SPLD = Tools.ReadHTMLSpoilerLog(path, Instance);
                SpoilerData = SPLD.SpoilerDatas;
                Pricedata = SPLD.Pricedata;
                SettingsData = SPLD.SettingString;
                Hintdata = SPLD.GossipHints;
            }
            else { MessageBox.Show("This Spoiler log is not valid. Please use an HTML file."); return; }

            if (SettingsData != null && ApplySetting)
            {
                RandomizeOptions ApplySettings = new RandomizeOptions();
                ApplySettings.ApplyRandomizerSettings(SettingsData);
                Debugging.Log("Settings Applied");
            }

            foreach (LogicObjects.SpoilerData data in SpoilerData)
            {
                if (data.LocationID > -1 && data.ItemID > -2 && data.LocationID < Instance.Logic.Count && data.ItemID < Instance.Logic.Count)
                {
                    Instance.Logic[data.LocationID].SpoilerRandom = data.ItemID;
                    if (data.ItemID == -1) { Instance.Logic[data.LocationID].JunkItemType = data.JunkItemType; }
                    if (data.BelongsTo > -1) { Instance.Logic[data.LocationID].PlayerData.ItemBelongedToPlayer = data.BelongsTo; }
                }
            }

            var SpoilerPriceLogicMap = Utility.ReadSpoilerLogPriceLogicMap(Instance, Pricedata);
            foreach(var i in SpoilerPriceLogicMap) { Instance.GetLogicObjectFromDicName(i.Key).Price = i.Value; }

            foreach (var data in Hintdata)
            {
                var GStone = Instance.Logic.Find(x => x.DictionaryName == "Gossip" + data.Key);
                if (GStone != null)
                {
                    GStone.GossipHint = "$" + data.Value;
                }
            }

            var entranceIDs = Instance.EntranceAreaDic;
            foreach (var i in Instance.Logic.Where(x => x.ItemSubType == "Dungeon Entrance" && entranceIDs.ContainsValue(x.ID) && x.SpoilerRandom < 0))
            {
                i.SpoilerRandom = i.ID;
            }

            Instance.CurrentSpoilerLog.type = Path.GetExtension(path);
            Instance.CurrentSpoilerLog.Log = File.ReadAllLines(path);

        }

        public static void CheckEntrancePair(LogicObjects.LogicEntry Location, LogicObjects.TrackerInstance Instance, bool Checking)
        {
            if (Location.ID < 0 || !Instance.Options.CoupleEntrances || !Location.HasRandomItem(true) || !Location.IsEntrance()) { return; }
            var reverseLocation = Location.PairedEntry(Instance, true);
            var reverseItem = Location.PairedEntry(Instance);
            if (reverseItem == null || reverseLocation == null) return;
            LastUpdated.Add(reverseItem.ID);
            LastUpdated.Add(reverseLocation.ID);
            //is the reverse entrance already checked and randomized to something
            if ((reverseLocation.Checked || (reverseLocation.HasRandomItem(true) && reverseLocation.RandomizedEntry(Instance) != reverseItem) || reverseItem.Aquired) && Checking) { return; }
            //Does the spoiler log conflict with what the reverse check is trying to do
            if (reverseLocation.SpoilerRandom != reverseItem.ID && reverseLocation.SpoilerRandom > -1 && Checking) { return; }
            reverseLocation.Checked = Checking;
            reverseLocation.RandomizedItem = (Checking) ? reverseItem.ID : -2;
            reverseItem.Aquired = Checking;
        }

        public static void RecreateLogic(LogicObjects.TrackerInstance Instance, string[] LogicData = null)
        {
            var LogicFile = LogicData;
            var saveFile = false;
            LogicObjects.TrackerInstance template = null;
            if (LogicFile == null)
            {
                string file = "";
                file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSAV)|*.txt;*.MMRTSAV");
                if (file == "") { return; }

                saveFile = file.EndsWith(".MMRTSAV");
                string[] SaveFileRawLogicFile = null;
                if (saveFile)
                {
                    try { template = JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(File.ReadAllText(file)); }
                    catch
                    {
                        MessageBox.Show("Save File Not Valid.");
                        return;
                    }
                    SaveFileRawLogicFile = template.RawLogicFile;
                }

                LogicFile = (saveFile) ? SaveFileRawLogicFile : File.ReadAllLines(file);
            }

            Tools.SaveState(Instance, new LogicObjects.SaveState() { trackerInstance = Instance }, LogicObjects.MaintrackerInstanceUndoRedoData);

            var OldLogic = Utility.CloneLogicList(Instance.Logic);
            Instance.RawLogicFile = LogicFile;
            Instance.LogicDictionary = null;
            LogicEditing.PopulateTrackerInstance(Instance);

            var logic = Instance.Logic;
            foreach (var entry in OldLogic)
            {
                var logicEntry = logic.Find(x => x.DictionaryName == entry.DictionaryName);
                if (logicEntry == null) { continue; }
                logicEntry.Aquired = entry.Aquired;
                logicEntry.Checked = entry.Checked;
                logicEntry.RandomizedItem = entry.RandomizedItem;
                logicEntry.Starred = entry.Starred;
                logicEntry.Options = entry.Options;
                logicEntry.TrickEnabled = entry.TrickEnabled;
                logicEntry.PlayerData = entry.PlayerData;
                logicEntry.CountCheckData = entry.CountCheckData;
                logicEntry.JunkItemType = entry.JunkItemType;
                logicEntry.Price = entry.Price;
            }

            if (Instance.CurrentSpoilerLog.Log != null)
            {
                var path = Path.GetTempPath();
                var fileName = Guid.NewGuid().ToString() + Instance.CurrentSpoilerLog.type;
                var fullPath = Path.Combine(path, fileName);

                File.WriteAllLines(fullPath, Instance.CurrentSpoilerLog.Log);
                WriteSpoilerLogToLogic(Instance, fullPath, false);
                try { if (File.Exists(fullPath)) { File.Delete(fullPath); } }
                catch (Exception ex) { Console.WriteLine("Error deleteing TEMP file: " + ex.Message); }
            }
            else
            {
                foreach (var entry in OldLogic)
                {
                    var logicEntry = logic.Find(x => x.DictionaryName == entry.DictionaryName);
                    if (logicEntry == null) { continue; }
                    logicEntry.SpoilerRandom = entry.SpoilerRandom;
                    logicEntry.Price = entry.Price;
                    logicEntry.GossipHint = entry.GossipHint;
                }
            }

            if (saveFile)
            {
                var Options = MessageBox.Show("Would you like to import the general tracker options from this save file?", "Options", MessageBoxButtons.YesNo);
                if (Options == DialogResult.Yes) { LogicObjects.MainTrackerInstance.Options = template.Options; }
                var RandOptions = MessageBox.Show("Would you like to import the Item Randomization options from this save file?", "Randomization Options", MessageBoxButtons.YesNo);
                if (RandOptions == DialogResult.Yes)
                {
                    foreach (var i in LogicObjects.MainTrackerInstance.Logic)
                    {
                        var TemplateData = template.Logic.Find(x => x.DictionaryName == i.DictionaryName);
                        if (TemplateData != null)
                        {
                            i.Options = TemplateData.Options;
                            i.TrickEnabled = TemplateData.TrickEnabled;
                        }
                    }
                }
            }
            Instance.Options.EntranceRadnoEnabled = Utility.CheckForRandomEntrances(Instance);
            Instance.Options.OverRideAutoEntranceRandoEnable = (Instance.Options.EntranceRadnoEnabled != Instance.EntranceRando);
            CalculateItems(Instance, true);
        }

        public static void CreatedEntrancepairDcitionary(LogicObjects.TrackerInstance instance)
        {
            foreach (var i in instance.Logic.Where(x => x.IsEntrance()))
            {
                var Pair = instance.LogicDictionary.LogicDictionaryList.Find(x => x.EntrancePair == i.DictionaryName);
                if (Pair == null || !instance.DicNameToID.ContainsKey(Pair.DictionaryName)) { continue; }
                instance.EntrancePairs.Add(i.ID, instance.DicNameToID[Pair.DictionaryName]);
            }
        }


        public static string[] WriteLogicToArray(LogicObjects.TrackerInstance Instance, bool IncludeTrickData = true)
        {
            List<string> lines = new List<string>
            {
                (Instance.IsMM()) ? "-version " + Instance.LogicVersion : "-version" + Instance.GameCode + " " + Instance.LogicVersion
            };
            foreach (var line in Instance.Logic)
            {
                lines.Add("- " + line.DictionaryName);
                string Req = "";
                string Comma = "";
                foreach (var i in line.Required ?? new int[0])
                {
                    Req = Req + Comma + i.ToString();
                    Comma = ",";
                }
                lines.Add(Req);
                string cond = "";
                string colon = "";
                foreach (var j in line.Conditionals ?? new int[0][])
                {
                    Req = "";
                    Comma = "";
                    foreach (var i in j ?? new int[0])
                    {
                        Req = Req + Comma + i.ToString();
                        Comma = ",";
                    }
                    cond = cond + colon + Req;
                    colon = ";";
                }
                lines.Add(cond);
                lines.Add(line.NeededBy.ToString());
                lines.Add(line.AvailableOn.ToString());
                if (IncludeTrickData)
                {
                    string trickLine = "";
                    if (line.IsTrick)
                    {
                        trickLine = ";";
                        if (line.TrickToolTip != "No Tooltip Available") { trickLine += line.TrickToolTip; }
                    }
                    lines.Add(trickLine);
                }
            }
            return lines.ToArray();
        }

        public static string[] WriteLogicToJson(LogicObjects.TrackerInstance Instance)
        {
            LogicObjects.LogicFile LogicFile = new LogicObjects.LogicFile
            {
                Logic = new List<LogicObjects.JsonFormatLogicItem>(),
                Version = Instance.LogicVersion,
                GameCode = Instance.GameCode == "MMR" ? null : Instance.GameCode
            };
            foreach (var i in Instance.Logic)
            {
                LogicObjects.JsonFormatLogicItem Newentry = new LogicObjects.JsonFormatLogicItem();
                Newentry.Id = i.DictionaryName;
                Newentry.RequiredItems = i.Required == null ? new List<string>() : i.Required.Select(x => Instance.Logic[x].DictionaryName).ToList();
                Newentry.ConditionalItems = i.Conditionals == null ? new List<List<string>>() : i.Conditionals.Select(x => x.Select(y => Instance.Logic[y].DictionaryName).ToList()).ToList();
                Newentry.TimeAvailable = (LogicObjects.TimeOfDay)i.AvailableOn;
                Newentry.TimeNeeded = (LogicObjects.TimeOfDay)i.NeededBy;
                Newentry.TimeSetup = (LogicObjects.TimeOfDay)i.TimeSetup;
                Newentry.IsTrick = i.IsTrick;
                Newentry.TrickTooltip = i.TrickToolTip == "No Tooltip Available" ? null : i.TrickToolTip;
                LogicFile.Logic.Add(Newentry);
            }
            return new string[] { LogicFile.ToString() };
        }

        public static Dictionary<int, int> CreateAreaClearDictionary(LogicObjects.TrackerInstance Instance)
        {
            var EntAreaDict = new Dictionary<int, int>();

            if (!Instance.IsMM()) { return EntAreaDict; }

            var WoodfallClear = Instance.Logic.Find(x => x.DictionaryName == "Woodfall clear" || x.DictionaryName == "AreaWoodFallTempleClear");
            var WoodfallAccess = Instance.Logic.Find(x => (x.DictionaryName == "Woodfall Temple access" || x.DictionaryName == "AreaWoodFallTempleAccess") && !x.IsFake);
            if (WoodfallAccess == null || WoodfallClear == null)
            {
                Console.WriteLine($"Coul not find Woodfall Data. Access found {WoodfallAccess != null}. Clear found {WoodfallClear != null}.");
                return new Dictionary<int, int>();
            }
            EntAreaDict.Add(WoodfallClear.ID, WoodfallAccess.ID);

            var SnowheadClear = Instance.Logic.Find(x => x.DictionaryName == "Snowhead clear" || x.DictionaryName == "AreaSnowheadTempleClear");
            var SnowheadAccess = Instance.Logic.Find(x => (x.DictionaryName == "Snowhead Temple access" || x.DictionaryName == "AreaSnowheadTempleAccess") && !x.IsFake);
            if (SnowheadAccess == null || SnowheadClear == null)
            {
                Console.WriteLine($"Coul not find Snowhead Data. Access found {SnowheadAccess != null} Clear {SnowheadClear != null}");
                return new Dictionary<int, int>();
            }
            EntAreaDict.Add(SnowheadClear.ID, SnowheadAccess.ID);

            var GreatBayClear = Instance.Logic.Find(x => x.DictionaryName == "Great Bay clear" || x.DictionaryName == "AreaGreatBayTempleClear");
            var GreatBayAccess = Instance.Logic.Find(x => (x.DictionaryName == "Great Bay Temple access" || x.DictionaryName == "AreaGreatBayTempleAccess") && !x.IsFake);
            if (GreatBayAccess == null || GreatBayClear == null)
            {
                Console.WriteLine($"Coul not find Great Bay Data. Access found {GreatBayAccess != null} Clear {GreatBayClear != null}");
                return new Dictionary<int, int>();
            }
            EntAreaDict.Add(GreatBayClear.ID, GreatBayAccess.ID);

            var StoneTowerClear = Instance.Logic.Find(x => x.DictionaryName == "Ikana clear" || x.DictionaryName == "AreaStoneTowerClear");
            var StoneTowerAccess = Instance.Logic.Find(x => (x.DictionaryName == "Inverted Stone Tower Temple access" || x.DictionaryName == "AreaInvertedStoneTowerTempleAccess") && !x.IsFake);
            if (StoneTowerAccess == null || StoneTowerClear == null)
            {
                Console.WriteLine($"Coul not find Ikana Data. Access found {StoneTowerAccess != null} Clear {StoneTowerClear != null}");
                return new Dictionary<int, int>();
            }
            EntAreaDict.Add(StoneTowerClear.ID, StoneTowerAccess.ID);

            return EntAreaDict;
        }

        public static bool HandleMMRTCombinationLogic(LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, List<int> usedItems = null)
        {
            bool AllBossKeys = true;
            bool AllSmallKeys = true;
            foreach (var cond in entry.Conditionals)
            {
                foreach(var i in cond)
                {
                    if (!Instance.Keys["SmallKeys"].Contains(i)) { AllSmallKeys = false; }
                    if (!Instance.Keys["BossKeys"].Contains(i)) { AllBossKeys = false; }
                }
            }

            if (AllBossKeys && Instance.Options.Keysy["BossKey"]) { return true; }
            if (AllSmallKeys && Instance.Options.Keysy["SmallKey"]) { return true; }

            var logic = Instance.Logic;
            List<int> CondItemsUsed = new List<int>();
            int ComboEntry = entry.Required.ToList().Find(x => logic[x].DictionaryName.StartsWith("MMRTCombinations"));
            var Required = entry.Required.Where(x => !logic[x].DictionaryName.StartsWith("MMRTCombinations") && logic[x].CountCheckData == null).ToArray();
            var DynamicComboData = entry.Required.Where(x => logic[x].CountCheckData != null).ToArray();
            int ConditionalsAquired = 0;
            int ConditionalsNeeded = -1;
            bool WasDynamicCount = logic[ComboEntry].DictionaryName == "MMRTCombinationsDynamic" && DynamicComboData.Any();
            bool WasStaticCount = int.TryParse(logic[ComboEntry].DictionaryName.Replace("MMRTCombinations", ""), out ConditionalsNeeded);

            if (WasDynamicCount)
            {
                bool DynamicDataValid = int.TryParse(logic[DynamicComboData[0]].CountCheckData.Replace("$", ""), out ConditionalsNeeded);
                if (!DynamicDataValid) { return false; }
            }
            else if (!WasStaticCount || ConditionalsNeeded < 0)
            {
                return false;
            }

            if (!Required.Any() || LogicEditing.RequirementsMet(Required, Instance, CondItemsUsed))
            {
                foreach (var i in entry.Conditionals)
                {
                    List<int> ReqItemsUsed = new List<int>();
                    if (LogicEditing.RequirementsMet(i, Instance, ReqItemsUsed))
                    {
                        foreach (var q in ReqItemsUsed) { CondItemsUsed.Add(q); }
                        ConditionalsAquired++;
                    }
                    if (ConditionalsAquired >= ConditionalsNeeded)
                    {
                        foreach (var q in CondItemsUsed) { usedItems.Add(q); }
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HandleMMRTCheckContainsItemLogic(LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, List<int> usedItems = null)
        {
            var logic = Instance.Logic;
            var Checks = entry.Required.Where(x => logic[x].DictionaryName != "MMRTCheckContains").ToArray();
            if (!Checks.Any()) { return false; }
            var entries = Math.Min(Checks.Count(), entry.Conditionals.Count());
            for (var i = 0; i < entries; i++)
            {
                var Check = logic[entry.Required[i]].RandomizedItem;
                var Items = entry.Conditionals[i];
                foreach (var j in Items)
                {
                    if (Check == j) { usedItems.Add(j); return true; }
                }
            }
            return false;
        }

        public static LogicObjects.LogicEntry HandleMMRTDungeonClearLogic(LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, List<int> usedItems = null)
        {
            var dungeonEntranceObject = Instance.Logic[Instance.EntranceAreaDic[entry.ID]];
            var RandClearLogic = entry.ClearRandomizedDungeonInThisArea(Instance);
            if (dungeonEntranceObject.Unrandomized(2)) { RandClearLogic = entry; }
            if (RandClearLogic == null) 
            { 
               // Console.WriteLine($"Logic for {entry.DictionaryName} was unknown because {dungeonEntranceObject.LocationName} is not checked"); 
                return null; 
            }

            //Console.WriteLine($"Assigning logic from {RandClearLogic.DictionaryName} to {entry.DictionaryName}");
            entry.Required = RandClearLogic.Required;
            entry.Conditionals = RandClearLogic.Conditionals;
            return entry;
        }

        public static LogicObjects.LogicEntry HandleMMRTrandomPriceLogic(LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {

            int DefaultCapacity = 0;
            if (Instance.WalletDictionary.ContainsKey("MMRTDefault"))
            {
                DefaultCapacity = Instance.WalletDictionary["MMRTDefault"];
            }
            bool NoWalletNeed = entry.Price <= DefaultCapacity;

            var ValidWallets = Instance.WalletDictionary.Where(x => x.Value >= entry.Price).ToDictionary(x => x.Key, x => x.Value).Keys;
            var ValidWalletObjects = ValidWallets.Select(x => Instance.GetLogicObjectFromDicName(x)).Where(x => x != null);
            var ValidWalletIDs = ValidWalletObjects.Select(x => x.ID).ToArray();
            if (ValidWallets.Count() < 1 ||  ValidWalletIDs == null)
            {
                Console.WriteLine("Critical error there are no wallets big enough to buy this item!");
                return entry;
            }
            int[] NewRequiredArray = null;
            int[][] NewConditionalsArray = null;

            List<int> AnyWalletFakeItems = new List<int>();
            foreach (var i in entry.Required ?? new int[0])
            {
                bool AllWallets = true;
                var RequiremtnItem = Instance.Logic[i];
                if (!RequiremtnItem.IsFake) { continue; }
                if (RequiremtnItem.Required != null) { continue; }
                if (RequiremtnItem.Conditionals == null) { AllWallets = false; continue; }
                foreach (var Cond in RequiremtnItem.Conditionals ?? new int[0][])
                {
                    foreach (var j in Cond)
                    {
                        var RequiremtnItem2 = Instance.Logic[j];
                        if (!Instance.WalletDictionary.ContainsKey(RequiremtnItem2.DictionaryName)) { AllWallets = false; }
                    }
                }
                if (AllWallets) { AnyWalletFakeItems.Add(i); }
            }

            NewRequiredArray = removeItemFromRequirement(entry.Required, ValidWalletIDs);
            NewRequiredArray = removeItemFromRequirement(NewRequiredArray, AnyWalletFakeItems.ToArray());
            NewConditionalsArray = removeItemFromConditionals(entry.Conditionals, ValidWalletIDs, false);
            if (!NoWalletNeed) { NewConditionalsArray = AddConditionalAsRequirement(NewConditionalsArray, ValidWalletIDs); }

            return new LogicObjects.LogicEntry() { DictionaryName = entry.DictionaryName, ID = -1, Required = NewRequiredArray, Conditionals = NewConditionalsArray , Price = entry.Price};
        }

        public static LogicObjects.LogicEntry PerformLogicEdits(LogicObjects.LogicEntry NewEntry, LogicObjects.TrackerInstance Instance)
        {
            var DicID = Instance.DicNameToID;
            var UselessLogicEntries = Instance.RandoOnlyLogicRequirements;
            var BYOAData = Utility.BYOAmmoData();

            //Check for a MMR Dungeon clear Entry
            if (NewEntry.IsFake && Instance.EntranceAreaDic.Count > 0 && Instance.EntranceAreaDic.ContainsKey(NewEntry.ID))
            {
                NewEntry = LogicEditing.HandleMMRTDungeonClearLogic(NewEntry, Instance);
                if (NewEntry == null) { return null; }
                //Console.WriteLine($"{NewEntry.DictionaryName} Had it's Dungeon Logic Swapped");
                NewEntry.LogicWasEdited = true;
            }
            //If a check was assigned a custom price, Change wallet logic entries to ensure the item is purchasable.
            if (NewEntry.Price > -1)
            {
                //Console.WriteLine(entry.DictionaryName + " needed Price adjustment");
                NewEntry = LogicEditing.HandleMMRTrandomPriceLogic(NewEntry, Instance);
                //Console.WriteLine($"{NewEntry.DictionaryName} Had it's Wallet Logic Swapped");
                NewEntry.LogicWasEdited = true;
            }
            //Removes logic entries that are only neccesary during randomization and don't actually represent the items requirements
            //An example is the pendant of memeories and letter to kafei being required for old lady and big bomb bag purchase check
            if (Instance.Options.RemoveUselessLogic && UselessLogicEntries.ContainsKey(NewEntry.DictionaryName) && NewEntry.Required != null)
            {
                //Console.WriteLine(entry.DictionaryName + " Contained Useless Logic");
                foreach (var i in UselessLogicEntries[NewEntry.DictionaryName])
                {
                    if (DicID.ContainsKey(i)) 
                    { 
                        NewEntry.Required = LogicEditing.removeItemFromRequirement(NewEntry.Required, new int[] { DicID[i] });
                        //Console.WriteLine($"{NewEntry.DictionaryName} Had Useless Item {i} Removed");
                        NewEntry.LogicWasEdited = true;
                    }
                }
            }
            //If bring your own ammo is enabled, add required items to logic.
            if (Instance.Options.BringYourOwnAmmo && BYOAData.ContainsKey(NewEntry.DictionaryName))
            {
                //Console.WriteLine(entry.DictionaryName + " Was effected by BYOAmmo");
                if (!BYOAData[NewEntry.DictionaryName].Where(x => !DicID.ContainsKey(x)).Any())
                {
                    //Console.WriteLine($"Adding the following to {entry.DictionaryName}");
                    NewEntry.Conditionals = LogicEditing.AddConditionalAsRequirement(NewEntry.Conditionals, BYOAData[NewEntry.DictionaryName].Select(x => DicID[x]).ToArray());
                    //Console.WriteLine($"{NewEntry.DictionaryName} Had Ammo Logic added");
                    NewEntry.LogicWasEdited = true;
                }
            }
            if (Instance.Options.Keysy["SmallKey"] && (Instance.Keys["ChecksNeedingKeys"].Contains(NewEntry.ID) || NewEntry.LogicWasEdited)) //If logic was editied, run it anyway to be safe.
            {
                NewEntry.Required = removeItemFromRequirement(NewEntry.Required, Instance.Keys["SmallKeys"].ToArray());
                NewEntry.Conditionals = removeItemFromConditionals(NewEntry.Conditionals, Instance.Keys["SmallKeys"].ToArray(), true);
                //Console.WriteLine($"{NewEntry.DictionaryName} Had Small Keys removed");
                NewEntry.LogicWasEdited = true;
            }
            if (Instance.Options.Keysy["BossKey"] && (Instance.Keys["ChecksNeedingKeys"].Contains(NewEntry.ID) || NewEntry.LogicWasEdited)) //If logic was editied, run it anyway to be safe.
            {
                NewEntry.Required = removeItemFromRequirement(NewEntry.Required, Instance.Keys["BossKeys"].ToArray());
                NewEntry.Conditionals = removeItemFromConditionals(NewEntry.Conditionals, Instance.Keys["BossKeys"].ToArray(), true);
                //Console.WriteLine($"{NewEntry.DictionaryName} Had Boss Keys removed");
                NewEntry.LogicWasEdited = true;
            }
            return NewEntry;
        }
        public static int[] AddRequirement(int[] entry, int[] Requirements)
        {
            List<int> NewRequirements = Requirements.ToList();
            if (entry == null) { NewRequirements.ToArray(); }
            foreach (var i in entry) { NewRequirements.Add(i); }
            return NewRequirements.ToArray();
        }
        public static int[][] AddConditional(int[][] entry, int[] Conditional)
        {
            List<int[]> NewRequirements = new List<int[]> { Conditional };
            if (entry == null) { return NewRequirements.ToArray(); }
            foreach (var i in entry) { NewRequirements.Add(i); }
            return NewRequirements.ToArray();
        }
        public static int[][] AddConditionalAsRequirement(int[][] entry, int[] Conditional)
        {
            List<List<int>> NewConditonals = new List<List<int>>();
            foreach (var item in Conditional)
            {
                if (entry == null) { NewConditonals.Add(new List<int> { item }); continue; }
                foreach (var Conitional in entry)
                {
                    List<int> NewCondtitional = new List<int>() { item };
                    foreach (var i in Conitional) { NewCondtitional.Add(i); }
                    NewConditonals.Add(NewCondtitional);
                }
            }
            return NewConditonals.Select(x => x.ToArray()).ToArray();
        }
        public static int[] removeItemFromRequirement(int[] entry, int[] Requirements)
        {
            if (Requirements == null || entry == null) { return null; }
            List<int> NewRequirements = new List<int>();
            var reqWithoutItem = entry.Where(x => !Requirements.Contains(x));
            if (!reqWithoutItem.Any()) { return null; }
            foreach(var i in reqWithoutItem) { NewRequirements.Add(i); }
            return NewRequirements.ToArray();
        }
        public static int[][] removeItemFromConditionals(int[][] entry, int[] Conditional, bool RemovedItemIsAlwaysAvailable)
        {
            if (Conditional == null || entry == null) { return null; }
            List<List<int>> NewConditionals = new List<List<int>>();
            foreach (var conditional in entry)
            {
                List<int> NewCondtitional = new List<int>();
                foreach (var i in conditional.Where(x => !Conditional.Contains(x))) { NewCondtitional.Add(i); }
                if (NewCondtitional.Any()) { NewConditionals.Add(NewCondtitional); }
                else if (RemovedItemIsAlwaysAvailable) { return null; }
                //If RemovedItemIsAlwaysAvailable is false, the items are being removed from conditionals because it should not be considered as a valid option for completing the check,
                //Meaning other conditionals still apply.
                //If it's true, since that item is always available and if it was the only item in a conditional the conditional set can never be false, so set it to null
            }
            if (!NewConditionals.Any()) { return null; }
            return NewConditionals.Select(x => x.ToArray()).ToArray();
        }
    }
}
