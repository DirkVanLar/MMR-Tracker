using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    class Debugging
    {
        public static bool ISDebugging = false;
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
            List<LogicObjects.Sphere> Playthrough = new List<LogicObjects.Sphere>();
            Dictionary<int, int> SpoilerToID = new Dictionary<int, int>();
            var playLogic = Utility.CloneLogicList(logic);

            if (!Utility.CheckforSpoilerLog(playLogic))
            {
                var file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt;*html)|*.txt;*html");
                if (file == "") { return; }
                LogicEditing.WriteSpoilerLogToLogic(playLogic, file);
            }
            if (!Utility.CheckforSpoilerLog(playLogic, true))
            { MessageBox.Show("Not all items have spoiler data. Playthrough can not be generated. Ensure you are using the same version of logic used to generate your selected spoiler log"); return; }

            List<int> importantItems = new List<int>();
            foreach (var i in playLogic)
            {
                i.Available = false;
                i.Checked = false;
                i.Aquired = false;
                i.RandomizedState = 0;
                if (i.IsFake) { i.SpoilerRandom = i.ID; i.RandomizedItem = i.ID; i.LocationName = i.DictionaryName; i.ItemName = i.DictionaryName; }
                if (i.RandomizedState == 1 && i.ID == i.SpoilerRandom) { i.IsFake = true; }
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

            LogicEditing.SwapAreaClearLogic(playLogic);
            CalculatePlaythrough(playLogic, Playthrough, 0, importantItems);

            importantItems = new List<int>();
            bool MajoraReachable = false;
            foreach (var i in Playthrough)
            {
                if ((i.Check.DictionaryName == "Moon Access" && !VersionHandeling.IsEntranceRando()) ||
                    playLogic[i.Check.RandomizedItem].DictionaryName == "EntranceMajorasLairFromTheMoon")
                {
                    importantItems.Add(i.Check.ID);
                    FindImportantItems(i, importantItems, Playthrough, SpoilerToID);
                    MajoraReachable = true;
                    break;
                }
            }
            if (!MajoraReachable) { MessageBox.Show("Majora is not reachable in this seed! Playthrough could not be generated!"); return; }

            Playthrough = Playthrough.OrderBy(x => x.SphereNumber).ThenBy(x => x.Check.ItemSubType).ThenBy(x => x.Check.LocationArea).ThenBy(x => x.Check.LocationName).ToList();

            foreach (var i in Playthrough) //Replace all fake items with the real items used to unlock those fake items
            {
                Console.WriteLine(i.Check.DictionaryName);
                if (i.Check.IsFake && importantItems.Contains(i.Check.ID)) { continue; }
                i.ItemsUsed = ResolveFakeToRealItems(i, Playthrough, logic);
                i.ItemsUsed = i.ItemsUsed.Distinct().ToList();
            }

            List<string> PlaythroughString = new List<string>();
            int lastSphere = -1;
            foreach (var i in Playthrough)
            {
                if (!importantItems.Contains(i.Check.ID) || i.Check.IsFake) { continue; }
                if (i.SphereNumber != lastSphere)
                {
                    PlaythroughString.Add("Sphere: " + i.SphereNumber + " ====================================="); lastSphere = i.SphereNumber;
                }
                PlaythroughString.Add("Check \"" + i.Check.LocationName + "\" to obtain \"" + playLogic[i.Check.RandomizedItem].ItemName + "\"");
                string items = "    Using Items: ";
                foreach (var j in i.ItemsUsed) { items = items + playLogic[j].ItemName + ", "; }
                if (items != "    Using Items: ") { PlaythroughString.Add(items); }

            }
            DebugScreen DebugScreen = new DebugScreen();
            DebugScreen.Playthrough = PlaythroughString;
            DebugScreen.DebugFunction = 3;
            DebugScreen.Show();
            DebugScreen.Playthrough = new List<string>();
        }

        public static void CalculatePlaythrough(List<LogicObjects.LogicEntry> logic, List<LogicObjects.Sphere> Playthrough, int sphere, List<int> ImportantItems)
        {
            Console.WriteLine("Shpere " + sphere);
            bool RealItemObtained = false;
            bool recalculate = false;
            List<LogicObjects.LogicEntry> itemCheckList = new List<LogicObjects.LogicEntry>();
            

            foreach (var item in logic)
            {
                List<int> UsedItems = new List<int>();
                item.Available = (LogicEditing.RequirementsMet(item.Required, logic, UsedItems) && LogicEditing.CondtionalsMet(item.Conditionals, logic, UsedItems));

                bool changed = false;
                if (!item.IsFake  && item.SpoilerRandom > -1 && item.Available != logic[item.SpoilerRandom].Aquired)
                {
                    itemCheckList.Add(item);
                    recalculate = true;
                    changed = true;
                }
                if (changed && ImportantItems.Contains(item.SpoilerRandom) && item.Available)
                {
                    Playthrough.Add(new LogicObjects.Sphere { SphereNumber = sphere, Check = item, ItemsUsed = UsedItems });
                    RealItemObtained = true;
                }
            }
            foreach (var item in itemCheckList)
            {
                logic[item.SpoilerRandom].Aquired = item.Available;
            }

            int NewSphere = (RealItemObtained) ? sphere + 1 : sphere;

            if (UnlockAllFake(logic, ImportantItems, NewSphere, Playthrough)) { recalculate = true; }

            if (recalculate) { CalculatePlaythrough(logic, Playthrough, NewSphere, ImportantItems); }
        }

        public static bool UnlockAllFake(List<LogicObjects.LogicEntry> logic, List<int> ImportantItems, int sphere, List<LogicObjects.Sphere> Playthrough)
        {
            var recalculate = false;
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
                    Playthrough.Add(new LogicObjects.Sphere { SphereNumber = sphere, Check = item, ItemsUsed = UsedItems });
                }
            }
            if (recalculate) { UnlockAllFake(logic, ImportantItems, sphere, Playthrough); }
            return recalculate;
        }

        public static void FindImportantItems(LogicObjects.Sphere EntryToCheck, List<int> importantItems, List<LogicObjects.Sphere> Playthrough, Dictionary<int, int> SpoilerToID)
        {
            foreach (var i in EntryToCheck.ItemsUsed)
            {
                var locToCheck = SpoilerToID[i];
                if (importantItems.Contains(locToCheck)) { continue; }
                importantItems.Add(locToCheck);
                var NextLocation = new LogicObjects.Sphere();
                foreach (var j in Playthrough)
                {
                    if (j.Check.ID == locToCheck) { NextLocation = j; break; }
                }
                FindImportantItems(NextLocation, importantItems, Playthrough, SpoilerToID);
            }
        }

        public static List<int> ResolveFakeToRealItems(LogicObjects.Sphere item, List<LogicObjects.Sphere> Playthrough, List<LogicObjects.LogicEntry> logic)
        {
            var RealItems = new List<int>();
            var New = new LogicObjects.Sphere();
            foreach (var j in item.ItemsUsed)
            {
                if (!logic[j].IsFake) { RealItems.Add(j); }
                else
                {
                    var NewItem = Playthrough.Where(i => i.Check.ID == j).FirstOrDefault();
                    foreach (var k in ResolveFakeToRealItems(NewItem, Playthrough, logic))
                    {
                        RealItems.Add(k);
                    }
                }
            }

            return RealItems;
        }
    }
}
