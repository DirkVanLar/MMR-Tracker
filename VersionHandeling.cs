using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MMR_Tracker_V2
{
    class VersionHandeling
    {
        public static int Version = 0; // The current verion of logic being used
        public static int EntranceRandoVersion = 16; // The version of logic that entrance randomizer was implimented
        public static bool entranceRadnoEnabled = false; // Whether or not entrances should be seperated into their own colum
        public static List<int> ValidVersions = new List<int> { 3, 5, 6, 8, 13, 16 }; // Versions of logic used in main releases

        public static Dictionary<int, int> AreaClearDictionary()
        {
            //Rando Version 1.5 = Logic Version 3
            //Rando Version 1.6 = Logic Version 5
            //Rando Version 1.7 = Logic Version 6
            //Rando Version 1.8 = Logic Version 8
            //Rando Version 1.9 = Logic Version 13
            //Rando Version 1.10 = Logic Version 13
            var EntAreaDict = new Dictionary<int, int>();
            switch (Version)
            {
                case 3:
                    EntAreaDict.Add(100, 99); //Woodfall Clear, Woodfall Entrance
                    EntAreaDict.Add(103, 102); //Snowhead Clear, Snowhead Entrance
                    EntAreaDict.Add(108, 107); //GreatBay Clear, GreatBay Entrance
                    EntAreaDict.Add(113, 112); //Ikana Clear, StoneTower Entrance
                    break;
                case 5: case 6:
                    EntAreaDict.Add(101, 100); //Woodfall Clear, Woodfall Entrance
                    EntAreaDict.Add(104, 103); //Snowhead Clear, Snowhead Entrance
                    EntAreaDict.Add(109, 108); //GreatBay Clear, GreatBay Entrance
                    EntAreaDict.Add(114, 113); //Ikana Clear, StoneTower Entrance
                    break;
                case 8: case 13:
                    EntAreaDict.Add(105, 104); //Woodfall Clear, Woodfall Entrance
                    EntAreaDict.Add(108, 107); //Snowhead Clear, Snowhead Entrance
                    EntAreaDict.Add(113, 112); //GreatBay Clear, GreatBay Entrance
                    EntAreaDict.Add(118, 117); //Ikana Clear, StoneTower Entrance
                    break;
            }
            return EntAreaDict;
        }

        public static bool isEntranceRando()
        {
            return Version >= EntranceRandoVersion;
        }

        public static string SwitchDictionary()
        {
            string[] files = Directory.GetFiles(@"Dictionaries");
            Dictionary<int,string> dictionaries = new Dictionary<int, string>();//< Int (Version),String (Path to the that dictionary)>
            int smallestentry = 0;
            int largestentry = 0;
            foreach (var i in files)
            {
                var entry = i.Replace("Dictionaries\\MMRDICTIONARYV", "");
                entry = entry.Replace(".csv", "");
                int version = 0;
                try { version = Int32.Parse(entry); }
                catch { break; }
                dictionaries.Add(version, i);
                if (version > largestentry) { largestentry = version; }
                if (smallestentry == 0) { smallestentry = largestentry; }
                if (version < smallestentry) { smallestentry = version; }
            }

            string currentdictionary = "";

            if (dictionaries.ContainsKey(VersionHandeling.Version))
            {
                LogicObjects.MMRDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDic>>(Utility.ConvertCsvFileToJsonObject(dictionaries[Version]));
                currentdictionary = dictionaries[Version];
            }
            else //If we are using a logic version that doesn't have a dictionary, use the dictioary with the closest version
            {
                int closest = dictionaries.Keys.Aggregate((x, y) => Math.Abs(x - Version) < Math.Abs(y - Version) ? x : y);
                LogicObjects.MMRDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDic>>(Utility.ConvertCsvFileToJsonObject(dictionaries[closest]));
                currentdictionary = dictionaries[closest];
            }
            Console.WriteLine(currentdictionary);
            return currentdictionary;
        }
    }
}
