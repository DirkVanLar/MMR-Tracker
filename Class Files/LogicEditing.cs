using MMR_Tracker.Class_Files;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Console.WriteLine("Populating Logic File");
            LogicObjects.VersionInfo version = VersionHandeling.GetVersionFromLogicFile(instance.RawLogicFile); //Returns [0] The logic Version, [1] The game this logic file is for
            instance.LogicVersion = version.Version;
            instance.GameCode = version.Gamecode;
            string[] VersionData = VersionHandeling.GetDictionaryPath(instance); //Returns [0] Path To Dictionary, [1] path to Entrance Pairs
            int SubCounter = 0;
            int idCounter = 0;
            LogicObjects.LogicEntry LogicEntry1 = new LogicObjects.LogicEntry();
            if (VersionData.Count() > 0 && VersionData[0] != "")
            {
                instance.LogicDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDictionaryEntry>>(Utility.ConvertCsvFileToJsonObject(VersionData[0]));
            }
            foreach (string line in instance.RawLogicFile)
            {
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
                        LogicEntry1.ItemName = (DicEntry.ItemName == "") ? null : DicEntry.ItemName;
                        LogicEntry1.LocationName = (DicEntry.LocationName == "") ? null : DicEntry.LocationName;
                        LogicEntry1.LocationArea = (DicEntry.LocationArea == "") ? "Misc" : DicEntry.LocationArea;
                        LogicEntry1.ItemSubType = (DicEntry.ItemSubType == "") ? "Item" : DicEntry.ItemSubType;
                        LogicEntry1.SpoilerLocation = (DicEntry.SpoilerLocation == "") ? LogicEntry1.LocationName : DicEntry.SpoilerLocation;
                        LogicEntry1.SpoilerItem = (DicEntry.SpoilerItem == "") ? LogicEntry1.ItemName : DicEntry.SpoilerItem;
                        break;
                    case 1:
                        if (line == null || line == "") { LogicEntry1.Required = null; break; }
                        string[] req = line.Split(',');
                        LogicEntry1.Required = Array.ConvertAll(req, s => int.Parse(s));
                        break;
                    case 2:
                        if (line == null || line == "") { LogicEntry1.Conditionals = null; break; }
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
                        //Push Data to the instance
                        instance.Logic.Add(LogicEntry1);
                        LogicEntry1 = new LogicObjects.LogicEntry();
                        idCounter++;
                        break;
                }
                SubCounter++;
            }

            instance.EntranceRando = instance.IsEntranceRando();
            instance.EntranceAreaDic = VersionHandeling.AreaClearDictionary(instance);
            CreateDicNameToID(instance.DicNameToID, instance.Logic);
            if (VersionData.Count() > 1 && VersionData[1] != "") { CreatedEntrancepairDcitionary(instance.EntrancePairs, instance.DicNameToID, VersionData); }

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
            foreach (var entry in logic)
            {
                if (entry.IsFake)
                {
                    entry.Available = false;
                    entry.Aquired = false;
                }
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

        public static bool CheckObject(LogicObjects.LogicEntry CheckedObject, LogicObjects.TrackerInstance Instance)
        {
            if (CheckedObject.Checked && CheckedObject.RandomizedItem > -2)
            {
                CheckedObject.Checked = false;
                if (CheckedObject.RandomizedItem > -1 && CheckedObject.RandomizedItem < Instance.Logic.Count && !Tools.SameItemMultipleChecks(CheckedObject.RandomizedItem, Instance))
                {
                    Instance.Logic[CheckedObject.RandomizedItem].Aquired = false;
                    CheckEntrancePair(CheckedObject, Instance, false);
                }
                CheckedObject.RandomizedItem = -2; 
                return true;
            }
            if (CheckedObject.SpoilerRandom > -2 || CheckedObject.RandomizedItem > -2 || CheckedObject.Options == 2)
            {
                CheckedObject.Checked = true;
                if (CheckedObject.Options == 2) { CheckedObject.RandomizedItem = CheckedObject.ID; }
                if (CheckedObject.SpoilerRandom > -2) { CheckedObject.RandomizedItem = CheckedObject.SpoilerRandom; }
                if (CheckedObject.RandomizedItem < 0) { CheckedObject.RandomizedItem = -1; return true; }
                Instance.Logic[CheckedObject.RandomizedItem].Aquired = true;
                CheckEntrancePair(CheckedObject, Instance, true);
                return true;
            }
            Tools.CurrentSelectedItem = CheckedObject; //Set the global CurrentSelectedItem to the Location selected in the list box
            ItemSelect ItemSelectForm = new ItemSelect(); var dialogResult = ItemSelectForm.ShowDialog();
            if (dialogResult != DialogResult.OK) { Tools.CurrentSelectedItem = new LogicObjects.LogicEntry(); return false; }
            CheckedObject.Checked = true;
            if (Tools.CurrentSelectedItem.ID < 0) //At this point CurrentSelectedItem has been changed to the selected item
            { CheckedObject.RandomizedItem = -1; Tools.CurrentSelectedItem = new LogicObjects.LogicEntry(); return true; }
            CheckedObject.RandomizedItem = Tools.CurrentSelectedItem.ID;
            Instance.Logic[Tools.CurrentSelectedItem.ID].Aquired = true;
            Tools.CurrentSelectedItem = new LogicObjects.LogicEntry();
            CheckEntrancePair(CheckedObject, Instance, true);

            return true;
        }

        public static bool MarkObject(LogicObjects.LogicEntry CheckedObject)
        {
            if (CheckedObject.RandomizedItem > -2) { CheckedObject.RandomizedItem = -2; return true; }
            if (CheckedObject.SpoilerRandom > -2) { CheckedObject.RandomizedItem = CheckedObject.SpoilerRandom; return true; }
            if (CheckedObject.Options == 2) { CheckedObject.RandomizedItem = CheckedObject.ID; return true; }
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
                if (data.LocationID > -1 && data.ItemID > -2)
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
            if (!Instance.Options.CoupleEntrances || !Location.HasRandomItem(true) || !Location.IsEntrance()) { return; }
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
            bool SettingsFile = false;
            string file = "";
            if (LogicFile == null)
            {
                file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSET)|*.txt;*.MMRTSET");
                if (file == "") { return; }

                SettingsFile = file.EndsWith(".MMRTSET");
                LogicFile = (SettingsFile) ? File.ReadAllLines(file).Skip(2).ToArray() : File.ReadAllLines(file);
            }

            var OldLogic = Utility.CloneLogicList(Instance.Logic);
            var Backup = Utility.CloneTrackerInstance(Instance);
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
                    logicEntry.Options = entry.Options;
                }
            }
            if (SettingsFile)
            {
                RandomizeOptions.UpdateRandomOptionsFromFile(File.ReadAllLines(file), Instance);
            }
            Instance.Options.EntranceRadnoEnabled = Utility.CheckForRandomEntrances(Instance);
            Instance.Options.OverRideAutoEntranceRandoEnable = (Instance.Options.EntranceRadnoEnabled != Instance.EntranceRando);
            CalculateItems(Instance, true);
            Tools.SaveState(Instance);
        }

        public static void CreatedEntrancepairDcitionary(Dictionary<int, int> entrancePairs, Dictionary<string, int> NameToID, string[] VersionData)
        {
            foreach (var i in File.ReadAllLines(VersionData[1]))
            {
                var j = i.Split(',');
                if (NameToID.ContainsKey(j[0]) && NameToID.ContainsKey(j[1]))
                {
                    entrancePairs.Add(NameToID[j[0]], NameToID[j[1]]);
                }
            }
        }

        public static void CreateDicNameToID(Dictionary<string, int> NameToID, List<LogicObjects.LogicEntry> logic)
        {
            foreach (var LogicEntry1 in logic)
            {
                if (!NameToID.ContainsKey(LogicEntry1.DictionaryName) && !LogicEntry1.IsFake)
                { NameToID.Add(LogicEntry1.DictionaryName, LogicEntry1.ID); }
            }
        }

        public static string[] WriteLogicToArray(LogicObjects.TrackerInstance Instance)
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
            }
            return lines.ToArray();
        }
    }
}
