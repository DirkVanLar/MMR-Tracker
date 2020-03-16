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
        public static bool CreateLogic(List<LogicObjects.LogicEntry> LogicList, string LogicFileLocation, Dictionary<string, int> DicNameToID)
        {
            int SubCounter = 0;
            int idCounter = 0;
            var VersionData = new string[2];
            LogicObjects.LogicEntry LogicEntry1 = new LogicObjects.LogicEntry();
            foreach (string line in File.ReadAllLines(LogicFileLocation))
            {
                if (line.StartsWith("-")) { SubCounter = 0; }
                if (line.Contains("-version")) { 
                    VersionHandeling.Version = Int32.Parse(line.Replace("-version ", ""));
                    VersionData = VersionHandeling.SwitchDictionary();
                    LogicObjects.MMRDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDic>>(Utility.ConvertCsvFileToJsonObject(VersionData[0]));
                    if (VersionHandeling.isEntranceRando())
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
                        LogicEntry1.ListGroup = -1;
                        LogicEntry1.EntrancePair = -1;
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
                        if (!LogicObjects.DicNameToID.ContainsKey(LogicEntry1.DictionaryName) && !LogicEntry1.IsFake)
                        { LogicObjects.DicNameToID.Add(LogicEntry1.DictionaryName, LogicEntry1.ID); }
                        
                        LogicEntry1 = new LogicObjects.LogicEntry();
                        idCounter++;
                        break;
                }
                SubCounter++;
            }

            if (!VersionHandeling.isEntranceRando()) { return true; }

            foreach(var i in File.ReadAllLines(VersionData[1]))
            {
                var j = i.Split(',');
                if (LogicObjects.DicNameToID.ContainsKey(j[0]) && LogicObjects.DicNameToID.ContainsKey(j[1]))
                {
                    LogicObjects.EntrancePairs.Add(LogicObjects.DicNameToID[j[0]], LogicObjects.DicNameToID[j[1]]);
                }
            }

            foreach (var i in LogicObjects.Logic)
            {
                if (LogicObjects.EntrancePairs.ContainsKey(i.ID)) { i.EntrancePair = LogicObjects.EntrancePairs[i.ID]; }
            }

            return true;
        }

        public static bool RequirementsMet(int[] list, List<LogicObjects.LogicEntry> logic)
        {
            if (list == null) { return true; }
            for (var i = 0; i < list.Length; i++){
                var item = logic[list[i]];
                bool aquired = (
                    item.Aquired || 
                    (item.RandomizedState == 1 && item.Available) || 
                    item.StartingItem); // Is the item Aquired, Unrandomized and Available or a starting item.
                if (!aquired) { return false; }
            }
            return true;
        }

        public static bool CondtionalsMet(int[][] list, List<LogicObjects.LogicEntry> logic)
        {
            if (list == null) { return true; }
            for (var i = 0; i < list.Length; i++){ if (RequirementsMet(list[i], logic)) { return true; } }
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

        public static void CalculateItems(List<LogicObjects.LogicEntry> logic, bool InitialRun, bool ForceStrictLogicHandeling)
        {
            if (InitialRun && (StrictLogicHandeling || ForceStrictLogicHandeling)){ ForceFreshCalculation(logic); }
            bool recalculate = false;
            for (var i = 0; i < logic.Count; i++)
            {
                var item = logic[i];
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

            if (EntAreaDict.ContainsKey(item.ID) && !VersionHandeling.isEntranceRando())
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
                CheckEntrancePair(CheckedObject, LogicObjects.Logic[CheckedObject.RandomizedItem], LogicObjects.Logic, false);
                CheckedObject.Checked = false;
                if (CheckedObject.RandomizedItem > -1) { LogicObjects.Logic[CheckedObject.RandomizedItem].Aquired = false; }
                CheckedObject.RandomizedItem = -2;
                LogicEditing.CalculateItems(LogicObjects.Logic, true, false);
                return true;
            }
            if (CheckedObject.SpoilerRandom > -2 || CheckedObject.RandomizedItem > -2 || CheckedObject.RandomizedState == 2)
            {
                CheckedObject.Checked = true;
                if (CheckedObject.RandomizedState == 2) { CheckedObject.RandomizedItem = CheckedObject.ID; }
                if (CheckedObject.SpoilerRandom > -2) { CheckedObject.RandomizedItem = CheckedObject.SpoilerRandom; }
                if (CheckedObject.RandomizedItem < 0) { CheckedObject.RandomizedItem = -1; return true; }
                LogicObjects.Logic[CheckedObject.RandomizedItem].Aquired = true;
                CheckEntrancePair(CheckedObject, LogicObjects.Logic[CheckedObject.RandomizedItem], LogicObjects.Logic , true);
                LogicEditing.CalculateItems(LogicObjects.Logic, true, false);
                return true;
            }
            LogicObjects.CurrentSelectedItem = CheckedObject; //Set the global CheckedObject to the Location selected in the list box
            ItemSelect ItemSelectForm = new ItemSelect(); var dialogResult = ItemSelectForm.ShowDialog();
            if (dialogResult != DialogResult.OK) { LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry(); return false; }
            CheckedObject.Checked = true;
            if (LogicObjects.CurrentSelectedItem.ID < 0) //At this point Current selected item has been changed to the selected item
            { CheckedObject.RandomizedItem = -1; LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry(); return true; }
            CheckedObject.RandomizedItem = LogicObjects.CurrentSelectedItem.ID;
            LogicObjects.Logic[LogicObjects.CurrentSelectedItem.ID].Aquired = true;
            LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry();
            CheckEntrancePair(CheckedObject, LogicObjects.Logic[CheckedObject.RandomizedItem], LogicObjects.Logic, true);
            LogicEditing.CalculateItems(LogicObjects.Logic, true, false);

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
            else if (path.Contains(".html")){ SpoilerData = Utility.ReadHTMLSpoilerLog(path, VersionHandeling.isEntranceRando()); }
            else { MessageBox.Show("This Spoiler log is not valid. Please use either an HTML or TXT file.");return;}
            foreach(LogicObjects.SpoilerData data in SpoilerData)
            {
                if (data.LocationID > -1 && data.ItemID > -2)
                    Logic[data.LocationID].SpoilerRandom = data.ItemID;
            }
            
        }

        public static void CheckEntrancePair(LogicObjects.LogicEntry Location, LogicObjects.LogicEntry item, List<LogicObjects.LogicEntry> logic, bool Checking)
        {
            if (!CoupleEntrances || Location.RandomizedItem < 0 || Location.EntrancePair < 0 || item.EntrancePair < 0) { return; }
            var reverseLocation = item.EntrancePair;
            var reverseItem = Location.EntrancePair;
            //This is checking if the reverse entrance seems to have already been cheked and randomized to something
            if ((logic[reverseLocation].Checked || logic[reverseLocation].RandomizedItem > -1 || logic[reverseItem].Aquired) && Checking) { return; }
            //This checkes to see if the spoiler log conflicts with what the reverse check is trying to do
            if (logic[reverseLocation].SpoilerRandom != reverseItem && logic[reverseLocation].SpoilerRandom > -1) { return; }
            logic[reverseLocation].Checked = Checking;
            logic[reverseLocation].RandomizedItem = (Checking) ? reverseItem : -2;
            logic[reverseItem].Aquired = Checking;
        }
    }
}
