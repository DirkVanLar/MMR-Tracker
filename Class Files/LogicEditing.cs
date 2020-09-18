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

        public static bool PopulateTrackerInstance(LogicObjects.TrackerInstance instance)
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
            string DictionaryPath = VersionHandeling.GetDictionaryPath(instance);
            int SubCounter = 0;
            int idCounter = 0;
            LogicObjects.LogicEntry LogicEntry1 = new LogicObjects.LogicEntry();
            if (!string.IsNullOrWhiteSpace(DictionaryPath))
            {
                try
                {
                    instance.LogicDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDictionaryEntry>>(Utility.ConvertCsvFileToJsonObject(DictionaryPath));
                }
                catch { MessageBox.Show($"The Dictionary File \"{DictionaryPath}\" has been corrupted. The tracker will not function correctly."); }
            }
            else { MessageBox.Show($"A valid dictionary file could not be found for this logic. The tracker will not function correctly."); }

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

                        var DicEntry = instance.LogicDictionary.Find(x => x.DictionaryName == LogicEntry1.DictionaryName);
                        if (DicEntry == null) { break; }

                        LogicEntry1.IsFake = false;
                        LogicEntry1.IsTrick = false;
                        LogicEntry1.TrickEnabled = true;
                        LogicEntry1.TrickToolTip = "";
                        LogicEntry1.ItemName = (string.IsNullOrWhiteSpace(DicEntry.ItemName)) ? null : DicEntry.ItemName;
                        LogicEntry1.LocationName = (string.IsNullOrWhiteSpace(DicEntry.LocationName)) ? null : DicEntry.LocationName;
                        LogicEntry1.LocationArea = (string.IsNullOrWhiteSpace(DicEntry.LocationArea)) ? "Misc" : DicEntry.LocationArea;
                        LogicEntry1.ItemSubType = (string.IsNullOrWhiteSpace(DicEntry.ItemSubType)) ? "Item" : DicEntry.ItemSubType;
                        LogicEntry1.SpoilerLocation = (string.IsNullOrWhiteSpace(DicEntry.SpoilerLocation)) ? LogicEntry1.LocationName : DicEntry.SpoilerLocation;
                        LogicEntry1.SpoilerItem = (string.IsNullOrWhiteSpace(DicEntry.SpoilerItem)) ? LogicEntry1.ItemName : DicEntry.SpoilerItem;
                        break;
                    case 1:
                        if (string.IsNullOrWhiteSpace(line)) { LogicEntry1.Required = null; break; }
                        string[] req = line.Split(',');
                        LogicEntry1.Required = Array.ConvertAll(req, s => int.Parse(s));
                        break;
                    case 2:
                        if (string.IsNullOrWhiteSpace(line)) { LogicEntry1.Conditionals = null; break; }
                        string[] ConditionalSets = line.Split(';');
                        int[][] Conditionals = new int[ConditionalSets.Length][];
                        for (int j = 0; j < ConditionalSets.Length; j++)
                        {
                            string[] condtional = ConditionalSets[j].Split(',');
                            Conditionals[j] = Array.ConvertAll(condtional, s => int.Parse(s));
                        }
                        LogicEntry1.Conditionals = Conditionals;
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
                        if (LogicEntry1.IsTrick) { Console.WriteLine($"Trick {LogicEntry1.DictionaryName} Found. ToolTip =  { LogicEntry1.TrickToolTip }"); }
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
            instance.EntranceAreaDic = VersionHandeling.AreaClearDictionary(instance);
            CreateDicNameToID(instance);
            if (instance.EntranceRando) { CreatedEntrancepairDcitionary(instance); }

            return true;
        }

        public static bool RequirementsMet(int[] list, List<LogicObjects.LogicEntry> logic, List<int> usedItems = null)
        {
            usedItems = usedItems ?? new List<int>();
            if (list == null) { return true; }
            foreach(var i in list)
            {
                usedItems.Add(i);
                if (!logic[i].Useable()) { return false; }
            }
            return true;
        }

        public static bool CondtionalsMet(int[][] list, List<LogicObjects.LogicEntry> logic, List<int> usedItems = null)
        {
            usedItems = usedItems ?? new List<int>();
            if (list == null) { return true; }
            foreach(var i in list)
            {
                if (Array.Exists(i, x => !logic[x].TrickEnabled && logic[x].IsTrick)) { continue; } //Ignore lines with disabled tricks
                List<int> UsedItemsSet = new List<int>();
                if (RequirementsMet(i, logic, UsedItemsSet))
                {
                    foreach (var set in UsedItemsSet) { usedItems.Add(set); }
                    return true;
                }
            }
            return false;
        }

        public static void ForceFreshCalculation(List<LogicObjects.LogicEntry> logic)
        {
            //This makes logic calculate fake items from scratch. This is used to prevent a bug where two fake items
            //can be unlocked by each other. In this case they will never change from availabe to unavailabe
            //even if they are actually unavailable. This is only used in the pathfinder but the option to use it all the
            //time is availbe through a toggle in the options menu.
            foreach (var entry in logic.Where(x => x.IsFake))
            {
                entry.Available = false;
                entry.Aquired = false;
            }
        }

        public static void CalculateItems(LogicObjects.TrackerInstance Instance, bool ForceStrictLogicHendeling = false, bool InitialRun = true)
        {
            if (InitialRun && (Instance.Options.StrictLogicHandeling || ForceStrictLogicHendeling)) { ForceFreshCalculation(Instance.Logic); }
            bool recalculate = false;
            foreach (var item in Instance.Logic)
            {
                item.Available = RequirementsMet(item.Required, Instance.Logic) && CondtionalsMet(item.Conditionals, Instance.Logic);

                int Special = SetAreaClear(item, Instance);
                if (Special == 2) { recalculate = true; }

                if (item.Aquired != item.Available && item.IsFake && Special == 0)
                {
                    item.Aquired = item.Available;
                    recalculate = true;
                }
            }
            if (recalculate) { CalculateItems(Instance, false, false); }
        }

        public static int SetAreaClear(LogicObjects.LogicEntry ClearLogic, LogicObjects.TrackerInstance Instance)
        {
            //0 = do nothing, 1 = Skip Fake item calculation, 2 = Skip Fake item calculation and recalculate logic
            Dictionary<int, int> EntAreaDict = Instance.EntranceAreaDic;
            if (EntAreaDict.Count == 0 || !EntAreaDict.ContainsKey(ClearLogic.ID)) { return 0; }
            var RandClearLogic = ClearLogic.RandomizedAreaClear(Instance);
            if (RandClearLogic == null && ClearLogic.Aquired) { ClearLogic.Aquired = false; return 2; }
            if (RandClearLogic == null) { return 1; }
            if (ClearLogic.Aquired != RandClearLogic.Available) { ClearLogic.Aquired = RandClearLogic.Available; return 2; }
            return 1;
        }

        public static bool CheckObject(LogicObjects.LogicEntry CheckedObject, LogicObjects.TrackerInstance Instance, int FromNetPlayer = -1)
        {
            if (CheckedObject.ID < -1) { return false; }
            if (CheckedObject.Checked && CheckedObject.RandomizedItem > -2)
            {
                if (CheckedObject.RandomizedItem > -1 && CheckedObject.RandomizedItem < Instance.Logic.Count && !Tools.SameItemMultipleChecks(CheckedObject.RandomizedItem, Instance) && (!LogicObjects.MainTrackerInstance.Options.IsMultiWorld || CheckedObject.ItemBelongsToMe()))
                {
                    Instance.Logic[CheckedObject.RandomizedItem].Aquired = false;
                    CheckEntrancePair(CheckedObject, Instance, false);
                }
                CheckedObject.Checked = false;
                CheckedObject.RandomizedItem = -2; 
                return true;
            }
            if (CheckedObject.SpoilerRandom > -2 || CheckedObject.RandomizedItem > -2 || CheckedObject.RandomizedState() == 2)
            {
                CheckedObject.Checked = true;
                if (CheckedObject.RandomizedState() == 2) { CheckedObject.RandomizedItem = CheckedObject.ID; }
                if (CheckedObject.SpoilerRandom > -2) { CheckedObject.RandomizedItem = CheckedObject.SpoilerRandom; }
                if (CheckedObject.RandomizedItem < 0) { CheckedObject.RandomizedItem = -1; return true; }
                if (!LogicObjects.MainTrackerInstance.Options.IsMultiWorld || CheckedObject.ItemBelongsToMe())
                {
                    Instance.Logic[CheckedObject.RandomizedItem].Aquired = true;
                }
                Instance.Logic[CheckedObject.RandomizedItem].PlayerData.ItemCameFromPlayer = FromNetPlayer;
                CheckEntrancePair(CheckedObject, Instance, true);
                return true;
            }
            Tools.CurrentSelectedItem = CheckedObject; //Set the global CurrentSelectedItem to the Location selected in the list box
            ItemSelect ItemSelectForm = new ItemSelect(); var dialogResult = ItemSelectForm.ShowDialog();
            if (dialogResult != DialogResult.OK) { Tools.CurrentSelectedItem = new LogicObjects.LogicEntry(); return false; }
            CheckedObject.Checked = true;
            if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { CheckedObject.PlayerData.ItemBelongedToPlayer = Tools.CurrentSelectedItem.PlayerData.ItemBelongedToPlayer; }
            if (Tools.CurrentSelectedItem.ID < 0) //At this point CurrentSelectedItem has been changed to the selected item
            { CheckedObject.RandomizedItem = -1; Tools.CurrentSelectedItem = new LogicObjects.LogicEntry(); return true; }
            CheckedObject.RandomizedItem = Tools.CurrentSelectedItem.ID;
            if (!LogicObjects.MainTrackerInstance.Options.IsMultiWorld || CheckedObject.ItemBelongsToMe())
            {
                Instance.Logic[Tools.CurrentSelectedItem.ID].Aquired = true;
            }
            Tools.CurrentSelectedItem = new LogicObjects.LogicEntry();
            CheckEntrancePair(CheckedObject, Instance, true);

            return true;
        }

        public static bool MarkObject(LogicObjects.LogicEntry CheckedObject)
        {
            if (CheckedObject.RandomizedItem > -2) { CheckedObject.RandomizedItem = -2; return true; }
            if (CheckedObject.SpoilerRandom > -2) { CheckedObject.RandomizedItem = CheckedObject.SpoilerRandom; return true; }
            if (CheckedObject.RandomizedState() == 2) { CheckedObject.RandomizedItem = CheckedObject.ID; return true; }
            Tools.CurrentSelectedItem = CheckedObject;
            ItemSelect ItemSelectForm = new ItemSelect(); var dialogResult = ItemSelectForm.ShowDialog();
            if (dialogResult != DialogResult.OK) { Tools.CurrentSelectedItem = new LogicObjects.LogicEntry(); return false; }
            if (Tools.CurrentSelectedItem.ID < 0)
            { CheckedObject.RandomizedItem = -1; Tools.CurrentSelectedItem = new LogicObjects.LogicEntry(); return true; }
            CheckedObject.RandomizedItem = Tools.CurrentSelectedItem.ID;
            Tools.CurrentSelectedItem = new LogicObjects.LogicEntry();
            return true;
        }

        public static void WriteSpoilerLogToLogic(LogicObjects.TrackerInstance Instance, string path)
        {
            List<LogicObjects.SpoilerData> SpoilerData = new List<LogicObjects.SpoilerData>();
            if (path.Contains(".txt")) { SpoilerData = Tools.ReadTextSpoilerlog(path, Instance); }
            else if (path.Contains(".html")) { SpoilerData = Tools.ReadHTMLSpoilerLog(path, Instance); }
            else { MessageBox.Show("This Spoiler log is not valid. Please use either an HTML or TXT file."); return;  }
            foreach (LogicObjects.SpoilerData data in SpoilerData)
            {
                if (data.LocationID > -1 && data.ItemID > -2 && data.LocationID < Instance.Logic.Count && data.ItemID < Instance.Logic.Count)
                    Instance.Logic[data.LocationID].SpoilerRandom = data.ItemID;
            }

            var entranceIDs = Instance.EntranceAreaDic;
            foreach (var i in Instance.Logic.Where(x => x.ItemSubType == "Dungeon Entrance" && entranceIDs.ContainsValue(x.ID) && x.SpoilerRandom < 0))
            {
                i.SpoilerRandom = i.ID;
            }
        }

        public static void CheckEntrancePair(LogicObjects.LogicEntry Location, LogicObjects.TrackerInstance Instance, bool Checking)
        {
            if (Location.ID < 0 || !Instance.Options.CoupleEntrances || !Location.HasRandomItem(true) || !Location.IsEntrance()) { return; }
            var reverseLocation = Location.PairedEntry(Instance, true);
            var reverseItem = Location.PairedEntry(Instance);
            if (reverseItem == null || reverseLocation == null) return;
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

            var OldLogic = Utility.CloneLogicList(Instance.Logic);
            Instance.RawLogicFile = LogicFile;
            LogicEditing.PopulateTrackerInstance(Instance);

            var logic = Instance.Logic;
            foreach (var entry in OldLogic)
            {
                if (Instance.DicNameToID.ContainsKey(entry.DictionaryName))
                {
                    var logicEntry = logic[Instance.DicNameToID[entry.DictionaryName]];

                    logicEntry.Aquired = entry.Aquired;
                    logicEntry.Checked = entry.Checked;
                    logicEntry.RandomizedItem = entry.RandomizedItem;
                    logicEntry.SpoilerRandom = entry.SpoilerRandom;
                    logicEntry.Starred = entry.Starred;
                    logicEntry.Options = entry.Options;
                    logicEntry.TrickEnabled = entry.TrickEnabled;
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
            Tools.SaveState(Instance);
        }

        public static void CreatedEntrancepairDcitionary(LogicObjects.TrackerInstance instance)
        {
            foreach(var i in instance.Logic.Where(x => x.IsEntrance()))
            {
                var Pair = instance.LogicDictionary.Find(x => x.EntrancePair == i.DictionaryName);
                if (Pair == null || !instance.DicNameToID.ContainsKey(Pair.DictionaryName)) { continue; }
                instance.EntrancePairs.Add(i.ID, instance.DicNameToID[Pair.DictionaryName]);
            }   
        }

        public static void CreateDicNameToID(LogicObjects.TrackerInstance instance)
        {
            foreach (var LogicEntry1 in instance.Logic)
            {
                if (!instance.DicNameToID.ContainsKey(LogicEntry1.DictionaryName) && !LogicEntry1.IsFake)
                { instance.DicNameToID.Add(LogicEntry1.DictionaryName, LogicEntry1.ID); }
            }
        }

        public static string[] WriteLogicToArray(LogicObjects.TrackerInstance Instance, bool IncludeTrickData = true)
        {
            List<string> lines = new List<string>();
            lines.Add((Instance.IsMM()) ? "-version " + Instance.LogicVersion : "-version" + Instance.GameCode + " " + Instance.LogicVersion);
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
                        if (line.TrickToolTip != "No Tooltip Available") { trickLine = trickLine + line.TrickToolTip; }
                    }
                    lines.Add(trickLine);
                }
            }
            return lines.ToArray();
        }
    }
}
