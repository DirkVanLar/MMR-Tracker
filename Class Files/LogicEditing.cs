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
            LogicObjects.RawLogicText = LogicFile.ToList<string>();
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
                    if (line.Contains("-versionOOT"))
                    {
                        OOT_Support.isOOT = true;
                        curLine = line.Replace("versionOOT", "version");
                    }
                    VersionHandeling.Version = Int32.Parse(curLine.Replace("-version ", ""));
                    VersionData = VersionHandeling.SwitchDictionary();
                    LogicObjects.MMRDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDic>>(Utility.ConvertCsvFileToJsonObject(VersionData[0]));
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
                    case 4:
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
                bool aquired = (item.Aquired || (item.RandomizedState == 1 && item.Available) || item.StartingItem);
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

        public static int SetAreaClear(LogicObjects.LogicEntry item, List<LogicObjects.LogicEntry> logic)
        {
            //0 = Do nothing, 1 = Skip Fake Item logic ,2 = Skip Fake Item logic and recalculate logic
            int recalculate = 0;

            Dictionary<int, int> EntAreaDict = VersionHandeling.AreaClearDictionary();

            if (EntAreaDict.ContainsKey(item.ID) && !VersionHandeling.IsEntranceRando())
            {
                recalculate = 1;
                var templeEntrance = EntAreaDict[item.ID];//What is the dungeon entrance in this area
                var RandTempleEntrance = logic[templeEntrance].RandomizedItem;//What dungeon does this areas dungeon entrance lead to
                var RandAreaClear = RandTempleEntrance < 0 ? -1 : EntAreaDict.FirstOrDefault(x => x.Value == RandTempleEntrance).Key;//What is the Area clear Value For That Dungeon
                var RandClearLogic = RandAreaClear == -1 ? new LogicObjects.LogicEntry { ID = -1 } : logic[RandAreaClear]; //Get the full logic data for the area clear that we want to check the availability of.

                //Set this areas clear value to the available value of the area we are cheking
                if (RandClearLogic.ID > -1 && item.Aquired != RandClearLogic.Available)
                {
                    item.Aquired = RandClearLogic.Available;
                    recalculate = 2;
                }
                //If the temple data for an area is removed after the area clear is set, the clear needs to be set to false.
                if (RandClearLogic.ID < 0 && item.Aquired)
                {
                    recalculate = 2;
                    item.Aquired = false;
                }
            }
            return recalculate;
        }

        public static bool CheckObject(LogicObjects.LogicEntry CheckedObject)
        {
            if (CheckedObject.Checked && CheckedObject.RandomizedItem > -2)
            {
                CheckedObject.Checked = false;
                if (CheckedObject.RandomizedItem > -1 && CheckedObject.RandomizedItem < LogicObjects.Logic.Count)
                {
                    LogicObjects.Logic[CheckedObject.RandomizedItem].Aquired = false;
                    CheckEntrancePair(CheckedObject, LogicObjects.Logic, false);
                }
                CheckedObject.RandomizedItem = -2;
                return true;
            }
            if (CheckedObject.SpoilerRandom > -2 || CheckedObject.RandomizedItem > -2 || CheckedObject.RandomizedState == 2)
            {
                CheckedObject.Checked = true;
                if (CheckedObject.RandomizedState == 2) { CheckedObject.RandomizedItem = CheckedObject.ID; }
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
            if (CheckedObject.RandomizedState == 2) { CheckedObject.RandomizedItem = CheckedObject.ID; return true; }
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
            if (!CoupleEntrances || Location.RandomizedItem < 0) { return; }
            var item = logic[Location.RandomizedItem];
            if (!LogicObjects.EntrancePairs.ContainsKey(Location.ID) || !LogicObjects.EntrancePairs.ContainsKey(item.ID)) { return; }
            var reverseLocation = LogicObjects.EntrancePairs[item.ID];
            var reverseItem = LogicObjects.EntrancePairs[Location.ID];
            //This is checking if the reverse entrance seems to have already been cheked and randomized to something
            if ((logic[reverseLocation].Checked || (logic[reverseLocation].RandomizedItem > -1 && logic[reverseLocation].RandomizedItem != reverseItem) || logic[reverseItem].Aquired) && Checking)
            { return; }
            //This checkes to see if the spoiler log conflicts with what the reverse check is trying to do
            if (logic[reverseLocation].SpoilerRandom != reverseItem && logic[reverseLocation].SpoilerRandom > -1) { return; }
            logic[reverseLocation].Checked = Checking;
            logic[reverseLocation].RandomizedItem = (Checking) ? reverseItem : -2;
            logic[reverseItem].Aquired = Checking;
        }

        public static void RecreateLogic()
        {
            string file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt)|*.txt");
            if (file == "") { return; }

            var OldLogic = Utility.CloneLogicList(LogicObjects.Logic);
            LogicObjects.Logic = new List<LogicObjects.LogicEntry>();
            LogicObjects.DicNameToID = new Dictionary<string, int>();
            LogicObjects.EntrancePairs = new Dictionary<int, int>();
            LogicObjects.RawLogicText = new List<string>();
            VersionHandeling.Version = 0;

            CreateLogic(LogicObjects.Logic, File.ReadAllLines(file));

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
                    logicEntry.RandomizedState = entry.RandomizedState;
                    logicEntry.StartingItem = entry.StartingItem;
                }
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
            var VersionData = VersionHandeling.SwitchDictionary();
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
            List<LogicObjects.Sphere> playthrough = new List<LogicObjects.Sphere>();
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
            List<int> NeededItems = Debugging.ResolveFakeToRealItems(new LogicObjects.Sphere { SphereNumber = 0, Check = ItemCopy, ItemsUsed = UsedItems }, playthrough, logic);
            NeededItems = NeededItems.Distinct().ToList();
            return NeededItems;
        }
    }
}
