using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    class Debugging
    {
        public static void PrintDictionatry()
        {
            for (int i = 0; i < LogicObjects.MMRDictionary.Count; i++)
            {
                Console.WriteLine(i);
                Console.WriteLine("Logic Name: " + LogicObjects.MMRDictionary[i].DictionaryName);
                Console.WriteLine("Location Name: " + LogicObjects.MMRDictionary[i].LocationName);
                Console.WriteLine("Item Name: " + LogicObjects.MMRDictionary[i].ItemName);
                Console.WriteLine("Item Location: " + LogicObjects.MMRDictionary[i].LocationArea);
                Console.WriteLine("Item Type: " + LogicObjects.MMRDictionary[i].ItemSubType);
                Console.WriteLine("Spoiler Location: " + LogicObjects.MMRDictionary[i].SpoilerLocation);
                Console.WriteLine("Spoiler Name: " + LogicObjects.MMRDictionary[i].SpoilerItem);

            }
        }

        public static void PrintLogicObject(List<LogicObjects.LogicEntry> Logic)
        {
            for (int i = 0; i < Logic.Count; i++)
            {
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("ID: " + Logic[i].ID);
                Console.WriteLine("Name: " + Logic[i].DictionaryName);
                Console.WriteLine("Location: " + Logic[i].LocationName);
                Console.WriteLine("Item: " + Logic[i].ItemName);
                Console.WriteLine("Location area: " + Logic[i].LocationArea);
                Console.WriteLine("Item Sub Type: " + Logic[i].ItemSubType);
                Console.WriteLine("Available: " + Logic[i].Available);
                Console.WriteLine("Aquired: " + Logic[i].Aquired);
                Console.WriteLine("Checked: " + Logic[i].Checked);
                Console.WriteLine("Fake Item: " + Logic[i].IsFake);
                Console.WriteLine("Random Item: " + Logic[i].RandomizedItem);
                Console.WriteLine("Spoiler Log Location name: " + Logic[i].SpoilerLocation);
                Console.WriteLine("Spoiler Log Item name: " + Logic[i].SpoilerItem);
                Console.WriteLine("Spoiler Log Randomized Item: " + Logic[i].SpoilerRandom);
                if (Logic[i].RandomizedState == 0) { Console.WriteLine("Randomized State: Randomized"); }
                if (Logic[i].RandomizedState == 1) { Console.WriteLine("Randomized State: Unrandomized"); }
                if (Logic[i].RandomizedState == 2) { Console.WriteLine("Randomized State: Forced Fake"); }
                if (Logic[i].RandomizedState == 3) { Console.WriteLine("Randomized State: Forced Junk"); }

                var test2 = Logic[i].Required;
                if (test2 == null) { Console.WriteLine("NO REQUIREMENTS"); }
                else
                {
                    Console.WriteLine("Required");
                    for (int j = 0; j < test2.Length; j++)
                    {
                        Console.WriteLine(test2[j]);
                    }
                }
                var test3 = Logic[i].Conditionals;
                if (test3 == null) { Console.WriteLine("NO CONDITIONALS"); }
                else
                {
                    for (int j = 0; j < test3.Length; j++)
                    {
                        Console.WriteLine("Conditional " + j);
                        for (int k = 0; k < test3[j].Length; k++)
                        {
                            Console.WriteLine(test3[j][k]);
                        }
                    }
                }
            }
        }

        public static void GeneratePlaythrough(List<LogicObjects.LogicEntry> logic)
        {
            List<LogicObjects.sphere> Playthrough = new List<LogicObjects.sphere>();
            Dictionary<int, int> SpoilerToID = new Dictionary<int, int>();
            var playLogic = Utility.CloneLogicList(logic);

            if (!Utility.CheckforSpoilerLog(playLogic))
            {
                var file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt;*html)|*.txt;*html");
                if (file == "") { return; }
                LogicEditing.WriteSpoilerLogToLogic(playLogic, file);
            }
            if (!Utility.CheckforFullSpoilerLog(playLogic))
            { MessageBox.Show("Not all items have spoiler data. Playthrough can not be generated. Ensure you are using the same version of logic used to generate your selected spoiler log"); return; }

            List<int> importantItems = new List<int>();
            foreach(var i in playLogic)
            {
                i.Available = false;
                i.Checked = false;
                i.Aquired = false;
                i.RandomizedState = 0;
                if (i.IsFake) { i.SpoilerRandom = i.ID; i.RandomizedItem = i.ID; i.LocationName = i.DictionaryName; i.ItemName = i.DictionaryName; }
                if (i.SpoilerRandom > -1) { i.RandomizedItem = i.SpoilerRandom; }
                SpoilerToID.Add(i.SpoilerRandom, i.ID);
                //Check for all items mentioned in the logic file
                if (i.Required != null)
                {
                    foreach (var k in i.Required)
                    {
                        if (!importantItems.Contains(k)) { importantItems.Add(k); }
                    }
                }
                if (i.Conditionals != null)
                {
                    foreach (var j in i.Conditionals)
                    {
                        foreach (var k in j) { if (!importantItems.Contains(k)) { importantItems.Add(k); } }
                    }
                }
                if (i.DictionaryName == "Moon Access") { importantItems.Add(i.ID); }
            }

            SwapAreaClearLogic(playLogic);
            CalculatePlaythrough(playLogic, Playthrough, 0, importantItems);

            importantItems = new List<int>();
            bool MajoraReachable = false;
            foreach (var i in Playthrough)
            {
                if ((i.Check.DictionaryName == "Moon Access" && !VersionHandeling.isEntranceRando()) || 
                    playLogic[i.Check.RandomizedItem].DictionaryName == "EntranceMajorasLairFromTheMoon")
                {
                    FindImportantItems(i, importantItems, Playthrough, SpoilerToID);
                    MajoraReachable = true;
                    break;
                }
            }
            if (!MajoraReachable) { MessageBox.Show("Majora is not reachable in this seed! Playthrough could not be generated!"); return; }

            List<string> PlaythroughString = new List<string>();
            int lastSphere = -1;
            foreach (var i in Playthrough)
            {
                if (!importantItems.Contains(i.Check.ID) || i.Check.IsFake) { continue; }
                if (i.sphereNumber != lastSphere)
                {
                    PlaythroughString.Add("Sphere: " + i.sphereNumber + " ====================================="); lastSphere = i.sphereNumber;
                }
                PlaythroughString.Add("Check \"" + i.Check.LocationName + "\" to obtain \"" + playLogic[i.Check.RandomizedItem].ItemName+ "\"");
                string items = "    Using Items: ";
                foreach(var j in i.ItemsUsed) { items = items + playLogic[j].ItemName + ", "; }
                if (items != "    Using Items: ") { PlaythroughString.Add(items); }

            }
            DebugScreen DebugScreen = new DebugScreen();
            DebugScreen.Playthrough = PlaythroughString;
            DebugScreen.DebugFunction = 3;
            DebugScreen.Show();
            DebugScreen.Playthrough = new List<string>();
        }

        public static void CalculatePlaythrough(List<LogicObjects.LogicEntry> logic, List<LogicObjects.sphere> Playthrough, int sphere, List<int> ImportantItems)
        {
            bool RealItemObtained = false;
            bool recalculate = false;
            foreach (var item in logic)
            {
                List<int> UsedItems = new List<int>();
                item.Available = (LogicEditing.RequirementsMet(item.Required, logic, UsedItems) && LogicEditing.CondtionalsMet(item.Conditionals, logic, UsedItems));

                bool changed = false;
                if (!item.IsFake && item.SpoilerRandom > -1 && item.Available != logic[item.SpoilerRandom].Aquired)
                {
                    logic[item.SpoilerRandom].Aquired = item.Available;
                    recalculate = true;
                    changed = true;
                }
                if (changed && ImportantItems.Contains(item.SpoilerRandom) && item.Available)
                {
                    Playthrough.Add(new LogicObjects.sphere { sphereNumber = sphere, Check = item, ItemsUsed = UsedItems });
                    RealItemObtained = true;
                }
            }

            foreach (var item in logic)
            {
                List<int> UsedItems = new List<int>();
                item.Available = (LogicEditing.RequirementsMet(item.Required, logic, UsedItems) && LogicEditing.CondtionalsMet(item.Conditionals, logic, UsedItems));

                bool changed = false;
                if (item.Aquired != item.Available && item.IsFake)
                {
                    item.Aquired = item.Available;
                    recalculate = true;
                    changed = true;
                }
                if (changed && ImportantItems.Contains(item.SpoilerRandom) && item.Available)
                {
                    Playthrough.Add(new LogicObjects.sphere { sphereNumber = sphere, Check = item, ItemsUsed = UsedItems });
                    RealItemObtained = true;
                }
            }


            int NewSphere = (RealItemObtained) ? sphere + 1 : sphere;
            if (recalculate) { CalculatePlaythrough(logic, Playthrough, NewSphere, ImportantItems); }
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

        public static void FindImportantItems(LogicObjects.sphere EntryToCheck, List<int> importantItems, List<LogicObjects.sphere> Playthrough, Dictionary<int, int> SpoilerToID)
        {
            foreach(var i in EntryToCheck.ItemsUsed)
            {
                var locToCheck = SpoilerToID[i];
                if (importantItems.Contains(locToCheck)) { continue; }
                importantItems.Add(locToCheck);
                var NextLocation = new LogicObjects.sphere();
                foreach(var j in Playthrough)
                {
                    if (j.Check.ID == locToCheck) { NextLocation = j; break; }
                }
                FindImportantItems(NextLocation, importantItems, Playthrough, SpoilerToID);
            }
        }

    }
}
