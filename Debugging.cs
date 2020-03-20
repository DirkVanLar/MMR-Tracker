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
        
    }
}
