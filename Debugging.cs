using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

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
            var playLogic = Utility.CloneLogicList(logic);
            List<int> importantItems = new List<int>();
            foreach(var i in playLogic)
            {
                i.Available = false;
                i.Checked = false;
                i.Aquired = false;
                i.RandomizedState = 0;
                if (i.IsFake) { i.SpoilerRandom = i.ID; i.RandomizedItem = i.ID; i.LocationName = i.DictionaryName; i.ItemName = i.DictionaryName; }
                if (i.SpoilerRandom > -1) { i.RandomizedItem = i.SpoilerRandom; }
                if (i.DictionaryName == "Moon Access")  { importantItems.Add(i.ID);  }
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
            }

            CalculatePlaythrough(playLogic, Playthrough, 1, importantItems);

            int lastSphere = 0;
            foreach(var i in Playthrough)
            {
                if (i.sphereNumber != lastSphere) 
                { 
                    Console.WriteLine("Sphere: " + i.sphereNumber); lastSphere = i.sphereNumber;
                }
                Console.WriteLine(i.Check.LocationName + " Contained " + playLogic[i.Check.RandomizedItem].ItemName);
                if (playLogic[i.Check.RandomizedItem].DictionaryName == "Moon Access") { break; }
            }

        }

        public static void CalculatePlaythrough(List<LogicObjects.LogicEntry> logic, List<LogicObjects.sphere> Playthrough, int sphere, List<int> ImportantItems)
        {
            bool RealItemObtained = false;
            bool recalculate = false;
            foreach (var item in logic)
            {
                List<int> UsedItems = new List<int>();

                item.Available = (RequirementsMet(item.Required, logic, UsedItems) && CondtionalsMet(item.Conditionals, logic, UsedItems));

                bool changed = false;

                int Special = SetAreaClear(item, logic);
                if (Special == 2) { recalculate = true; changed = true; }

                if (item.Aquired != item.Available && Special == 0 && item.IsFake)
                {
                    item.Aquired = item.Available;
                    recalculate = true;
                    changed = true;
                }
                if (!item.IsFake && item.SpoilerRandom > -1 && item.Available != logic[item.SpoilerRandom].Aquired)
                {
                    logic[item.SpoilerRandom].Aquired = item.Available;
                    recalculate = true;
                    changed = true;
                }
                if (changed && ImportantItems.Contains(item.SpoilerRandom) && item.Available)
                {
                    Console.WriteLine(item.DictionaryName + " Was unlocked with:");
                    foreach (var n in UsedItems) { Console.WriteLine(logic[n].DictionaryName); }

                    Playthrough.Add(new LogicObjects.sphere { sphereNumber = sphere, Check = item, ItemsUsed = UsedItems });
                    RealItemObtained = true;
                }
            }
            int NewSphere = (RealItemObtained) ? sphere + 1 : sphere;
            if (recalculate) { CalculatePlaythrough(logic, Playthrough, NewSphere, ImportantItems); }
        }

        public static bool RequirementsMet(int[] list, List<LogicObjects.LogicEntry> logic, List<int> usedItems)
        {
            if (list == null) { return true; }
            for (var i = 0; i < list.Length; i++)
            {
                usedItems.Add(list[i]);
                var item = logic[list[i]];
                bool aquired = ( item.Aquired || item.StartingItem );
                if (!aquired) { return false; }
            }
            return true;
        }

        public static bool CondtionalsMet(int[][] list, List<LogicObjects.LogicEntry> logic, List<int> usedItems)
        {
            if (list == null) { return true; }
            for (var i = 0; i < list.Length; i++) 
            {
                List<int> UsedItemsSet = new List<int>();
                if (RequirementsMet(list[i], logic, UsedItemsSet)) 
                { 
                    foreach(var set in UsedItemsSet) { usedItems.Add(set); }
                    return true; 
                } 
            }
            return false;
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

    }
}
