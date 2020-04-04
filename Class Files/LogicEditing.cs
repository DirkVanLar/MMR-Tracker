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
        public static bool StrictLogicHandeling = false;

        public static bool CoupleEntrances = true;

        public static bool CreateLogic(List<LogicObjects.LogicEntry> LogicList, string[] LogicFile)
        {
            int SubCounter = 0;
            int idCounter = 0;
            var VersionData = new string[2];
            LogicObjects.LogicEntry LogicEntry1 = new LogicObjects.LogicEntry();
            foreach (string line in LogicFile)
            {
                if (line.StartsWith("-")) { SubCounter = 0; }
                if (line.Contains("-version"))
                {
                    string curLine = line;

                    OOT_Support.isOOT = false;
                    if (line.Contains("-versionOOT")) { OOT_Support.isOOT = true; curLine = line.Replace("versionOOT", "version"); }

                    VersionHandeling.Version = Int32.Parse(curLine.Replace("-version ", ""));
                    VersionData = VersionHandeling.SwitchDictionary(VersionHandeling.Version, OOT_Support.isOOT);
                    LogicObjects.MMRDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDictionaryEntry>>(Utility.ConvertCsvFileToJsonObject(VersionData[0]));
                    if (VersionHandeling.IsEntranceRando())
                    { VersionHandeling.entranceRadnoEnabled = true; }
                }
                switch (SubCounter)
                {
                    case 0:
                        LogicEntry1.ID = idCounter;
                        LogicEntry1.DictionaryName = line.Substring(2);
                        LogicEntry1.Checked = false;
                        LogicEntry1.RandomizedItem = -2;
                        LogicEntry1.IsFake = true;
                        LogicEntry1.SpoilerRandom = -2;
                        for (int i = 0; i < LogicObjects.MMRDictionary.Count; i++)
                        {
                            if (LogicObjects.MMRDictionary[i].DictionaryName == line.Substring(2))
                            {
                                LogicEntry1.IsFake = false;
                                var dicent = LogicObjects.MMRDictionary[i];
                                LogicEntry1.ItemName = (dicent.ItemName == "") ? null : dicent.ItemName;
                                LogicEntry1.LocationName = (dicent.LocationName == "") ? null : dicent.LocationName;
                                LogicEntry1.LocationArea = (dicent.LocationArea == "") ? "Misc" : dicent.LocationArea;
                                LogicEntry1.ItemSubType = (dicent.ItemSubType == "") ? "Item" : dicent.ItemSubType;
                                LogicEntry1.SpoilerLocation = (dicent.SpoilerLocation == "") ? LogicEntry1.LocationName : dicent.SpoilerLocation;
                                LogicEntry1.SpoilerItem = (dicent.SpoilerItem == "") ? LogicEntry1.ItemName : dicent.SpoilerItem;
                                break;
                            }
                        }
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
                        LogicList.Add(LogicEntry1);

                        LogicEntry1 = new LogicObjects.LogicEntry();
                        idCounter++;
                        break;
                }
                SubCounter++;
            }

            CreateDicNameToID(LogicObjects.DicNameToID, LogicObjects.Logic);
            CreatedEntrancepairDcitionary(LogicObjects.EntrancePairs, LogicObjects.DicNameToID);

            return true;
        }

        public static bool RequirementsMet(int[] list, List<LogicObjects.LogicEntry> logic, List<int> usedItems = null)
        {
            usedItems = usedItems ?? new List<int>();
            if (list == null) { return true; }
            for (var i = 0; i < list.Length; i++)
            {
                usedItems.Add(list[i]);
                var item = logic[list[i]];
                bool aquired = (item.Aquired || (item.Unrandomized() && item.Available) || item.StartingItem());
                if (!aquired) { return false; }
            }
            return true;
        }

        public static bool CondtionalsMet(int[][] list, List<LogicObjects.LogicEntry> logic, List<int> usedItems = null)
        {
            usedItems = usedItems ?? new List<int>();
            if (list == null) { return true; }
            for (var i = 0; i < list.Length; i++)
            {
                List<int> UsedItemsSet = new List<int>();
                if (RequirementsMet(list[i], logic, UsedItemsSet))
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
            //can be unlocked by each other. In this case they will never go from being availabe to unavailabe
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

        public static void CalculateItems(List<LogicObjects.LogicEntry> logic, bool ForceStrictLogicHandeling = false, bool InitialRun = true)
        {
            if (InitialRun && (StrictLogicHandeling || ForceStrictLogicHandeling)) { ForceFreshCalculation(logic); }
            bool recalculate = false;
            foreach (var item in logic)
            {
                item.Available = RequirementsMet(item.Required, logic) && CondtionalsMet(item.Conditionals, logic);

                int Special = SetAreaClear(item, logic);
                if (Special == 2) { recalculate = true; }

                if (item.Aquired != item.Available && item.IsFake && Special == 0)
                {
                    item.Aquired = item.Available;
                    recalculate = true;
                }
            }
            if (recalculate) { CalculateItems(logic, false, false); }
        }

        public static int SetAreaClear(LogicObjects.LogicEntry ClearLogic, List<LogicObjects.LogicEntry> logic)
        {
            //0 = do nothing, 1 = Skip Fake item calculation, 2 = Skip Fake item calculation and recalculate logic
            Dictionary<int, int> EntAreaDict = VersionHandeling.AreaClearDictionary();
            if (VersionHandeling.IsEntranceRando() || !EntAreaDict.ContainsKey(ClearLogic.ID)) { return 0; }
            var RandClearLogic = ClearLogic.RandomizedAreaClear(logic, EntAreaDict);
            if (RandClearLogic == null && ClearLogic.Aquired) { ClearLogic.Aquired = false; return 2; }
            if (RandClearLogic == null) { return 1; }
            if (ClearLogic.Aquired != RandClearLogic.Available) { ClearLogic.Aquired = RandClearLogic.Available; }
            return 2;
        }

        public static bool CheckObject(LogicObjects.LogicEntry CheckedObject)
        {
            if (CheckedObject.Checked && CheckedObject.RandomizedItem > -2)
            {
                CheckedObject.Checked = false;
                if (CheckedObject.RandomizedItem > -1 && CheckedObject.RandomizedItem < LogicObjects.Logic.Count && !SameItemMultipleChecks(CheckedObject.RandomizedItem))
                {
                    LogicObjects.Logic[CheckedObject.RandomizedItem].Aquired = false;
                    CheckEntrancePair(CheckedObject, LogicObjects.Logic, false);
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
                LogicObjects.Logic[CheckedObject.RandomizedItem].Aquired = true;
                CheckEntrancePair(CheckedObject, LogicObjects.Logic, true);
                return true;
            }
            LogicObjects.CurrentSelectedItem = CheckedObject; //Set the global CurrentSelectedItem to the Location selected in the list box
            ItemSelect ItemSelectForm = new ItemSelect(); var dialogResult = ItemSelectForm.ShowDialog();
            if (dialogResult != DialogResult.OK) { LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry(); return false; }
            CheckedObject.Checked = true;
            if (LogicObjects.CurrentSelectedItem.ID < 0) //At this point CurrentSelectedItem has been changed to the selected item
            { CheckedObject.RandomizedItem = -1; LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry(); return true; }
            CheckedObject.RandomizedItem = LogicObjects.CurrentSelectedItem.ID;
            LogicObjects.Logic[LogicObjects.CurrentSelectedItem.ID].Aquired = true;
            LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry();
            CheckEntrancePair(CheckedObject, LogicObjects.Logic, true);

            return true;
        }

        public static bool MarkObject(LogicObjects.LogicEntry CheckedObject)
        {
            if (CheckedObject.RandomizedItem > -2) { CheckedObject.RandomizedItem = -2; return true; }
            if (CheckedObject.SpoilerRandom > -2) { CheckedObject.RandomizedItem = CheckedObject.SpoilerRandom; return true; }
            if (CheckedObject.Options == 2) { CheckedObject.RandomizedItem = CheckedObject.ID; return true; }
            LogicObjects.CurrentSelectedItem = CheckedObject;
            ItemSelect ItemSelectForm = new ItemSelect(); var dialogResult = ItemSelectForm.ShowDialog();
            if (dialogResult != DialogResult.OK) { LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry(); return false; }
            if (LogicObjects.CurrentSelectedItem.ID < 0)
            { CheckedObject.RandomizedItem = -1; LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry(); return true; }
            CheckedObject.RandomizedItem = LogicObjects.CurrentSelectedItem.ID;
            LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry();
            return true;
        }

        public static void WriteSpoilerLogToLogic(List<LogicObjects.LogicEntry> Logic, string path)
        {
            List<LogicObjects.SpoilerData> SpoilerData = new List<LogicObjects.SpoilerData>();
            if (path.Contains(".txt")) { SpoilerData = Utility.ReadTextSpoilerlog(path); }
            else if (path.Contains(".html")) { SpoilerData = Utility.ReadHTMLSpoilerLog(path, VersionHandeling.IsEntranceRando()); }
            else { MessageBox.Show("This Spoiler log is not valid. Please use either an HTML or TXT file."); Console.WriteLine(SpoilerData); return;  }
            foreach (LogicObjects.SpoilerData data in SpoilerData)
            {
                if (data.LocationID > -1 && data.ItemID > -2)
                    Logic[data.LocationID].SpoilerRandom = data.ItemID;
            }

            if (!VersionHandeling.IsEntranceRando())//If dungeon entrances aren't randomized they don't show up in the spoiler log
            {
                var entranceIDs = VersionHandeling.AreaClearDictionary();
                foreach (var i in Logic)
                {
                    if (i.ItemSubType == "Dungeon Entrance" && entranceIDs.ContainsValue(i.ID) && i.SpoilerRandom < 0) { i.SpoilerRandom = i.ID; }
                }
            }
        }

        public static void CheckEntrancePair(LogicObjects.LogicEntry Location, List<LogicObjects.LogicEntry> logic, bool Checking)
        {
            if (!CoupleEntrances || !Location.HasRealRandomItem() || !Location.IsEntrance()) { return; }
            var reverseLocation = Location.PairedEntry(logic, true);
            var reverseItem = Location.PairedEntry(logic);
            if (reverseItem == null || reverseLocation == null) return;
            //is the reverse entrance already checked and randomized to something
            if ((reverseLocation.Checked || (reverseLocation.HasRealRandomItem() && reverseLocation.RandomizedEntry() != reverseItem) || reverseItem.Aquired) && Checking) { return; }
            //Does the spoiler log conflict with what the reverse check is trying to do
            if (reverseLocation.SpoilerRandom != reverseItem.ID && reverseLocation.SpoilerRandom > -1 && Checking) { return; }
            reverseLocation.Checked = Checking;
            reverseLocation.RandomizedItem = (Checking) ? reverseItem.ID : -2;
            reverseItem.Aquired = Checking;
        }

        public static void RecreateLogic(string[] LogicData = null)
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

            var OldLogic = Utility.CloneLogicList(LogicObjects.Logic);
            LogicObjects.Logic = new List<LogicObjects.LogicEntry>();
            LogicObjects.DicNameToID = new Dictionary<string, int>();
            LogicObjects.EntrancePairs = new Dictionary<int, int>();
            VersionHandeling.Version = 0;

            CreateLogic(LogicObjects.Logic, LogicFile);

            var logic = LogicObjects.Logic;
            foreach (var entry in OldLogic)
            {
                if (LogicObjects.DicNameToID.ContainsKey(entry.DictionaryName))
                {
                    var logicEntry = logic[LogicObjects.DicNameToID[entry.DictionaryName]];

                    logicEntry.Aquired = entry.Aquired;
                    logicEntry.Checked = entry.Checked;
                    logicEntry.RandomizedItem = entry.RandomizedItem;
                    logicEntry.SpoilerRandom = entry.SpoilerRandom;
                    logicEntry.Options = entry.Options;
                }
            }
            if (SettingsFile)
            {
                RandomizeOptions.UpdateRandomOptionsFromFile(File.ReadAllLines(file));
                VersionHandeling.entranceRadnoEnabled = Utility.CheckForRandomEntrances(LogicObjects.Logic);
                VersionHandeling.OverRideAutoEntranceRandoEnable = (VersionHandeling.entranceRadnoEnabled != VersionHandeling.IsEntranceRando());
            }
            CalculateItems(LogicObjects.Logic, true);
            Utility.SaveState(LogicObjects.Logic);
        }

        public static void CheckSeed(List<LogicObjects.LogicEntry> logic, bool InitialRun, List<int> Ignored)
        {
            if (InitialRun) { ForceFreshCalculation(logic); }
            bool recalculate = false;
            foreach (var item in logic)
            {
                item.Available = RequirementsMet(item.Required, logic) && CondtionalsMet(item.Conditionals, logic);

                int Special = SetAreaClear(item, logic);
                if (Special == 2) { recalculate = true; }

                if (item.Aquired != item.Available && Special == 0 && item.IsFake)
                {
                    item.Aquired = item.Available;
                    recalculate = true;
                }
                if (!item.IsFake && item.RandomizedItem > -1 && item.Available != logic[item.RandomizedItem].Aquired && !Ignored.Contains(item.ID))
                {
                    logic[item.RandomizedItem].Aquired = item.Available;
                    recalculate = true;
                }
            }
            if (recalculate) { CheckSeed(logic, false, Ignored); }
        }

        public static void CreatedEntrancepairDcitionary(Dictionary<int, int> entrancePairs, Dictionary<string, int> NameToID)
        {
            var VersionData = VersionHandeling.SwitchDictionary(VersionHandeling.Version);
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

        public static void SwapAreaClearLogic(List<LogicObjects.LogicEntry> logic)
        {
            var areaClearData = VersionHandeling.AreaClearDictionary();
            var ReferenceLogic = Utility.CloneLogicList(logic);
            foreach (var i in logic)
            {
                if (areaClearData.ContainsKey(i.ID))
                {
                    var Dungeon = logic[areaClearData[i.ID]];
                    if (Dungeon.RandomizedItem < 0) { return; }
                    var DungoneRandItem = Dungeon.RandomizedItem;
                    var RandomClear = areaClearData.FirstOrDefault(x => x.Value == DungoneRandItem).Key;
                    logic[i.ID].Required = ReferenceLogic[RandomClear].Required;
                    logic[i.ID].Conditionals = ReferenceLogic[RandomClear].Conditionals;
                }
            }
        }

        public static List<int> FindRequirements(LogicObjects.LogicEntry Item, List<LogicObjects.LogicEntry> logic)
        {
            List<int> ImportantItems = new List<int>();
            List<LogicObjects.PlaythroughItem> playthrough = new List<LogicObjects.PlaythroughItem>();
            var LogicCopy = Utility.CloneLogicList(logic);
            var ItemCopy = LogicCopy[Item.ID];
            ForceFreshCalculation(LogicCopy);
            foreach(var i in LogicCopy) 
            {
                ImportantItems.Add(i.ID);
                if (i.IsFake)  { i.SpoilerRandom = i.ID; } 
            }
            Debugging.UnlockAllFake(LogicCopy, ImportantItems, 0, playthrough);
            List<int> UsedItems = new List<int>();
            bool isAvailable = (RequirementsMet(ItemCopy.Required, logic, UsedItems) && CondtionalsMet(ItemCopy.Conditionals, logic, UsedItems));
            if (!isAvailable) { return new List<int>(); }
            List<int> NeededItems = Debugging.ResolveFakeToRealItems(new LogicObjects.PlaythroughItem { SphereNumber = 0, Check = ItemCopy, ItemsUsed = UsedItems }, playthrough, logic);
            NeededItems = NeededItems.Distinct().ToList();
            return NeededItems;
        }

        public static bool SameItemMultipleChecks(int item)
        {
            if (item < 0 || !StrictLogicHandeling) { return false; }
            int count = 0;
            foreach (var entry in LogicObjects.Logic)
            {
                if (entry.RandomizedItem == item && entry.Checked) { count += 1; }
            }
            Console.WriteLine(count);
            return count > 1;
        }

        public static string[] WriteLogicToArray(List<LogicObjects.LogicEntry> logic, int versionNumber, bool isOOT)
        {
            List<string> lines = new List<string>();
            lines.Add((isOOT) ? "-versionOOT " + versionNumber : "-version " + versionNumber);
            foreach (var line in logic)
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
