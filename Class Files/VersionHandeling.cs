using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    class VersionHandeling
    {
        //Logic Version Handeling
        public static int EntranceRandoVersion = 16; // The version of logic that entrance randomizer was implimented
        public static List<int> ValidVersions = new List<int> { 8, 13, 14, 16 }; // Versions of logic used in main releases

        public static string trackerVersion = "V1.8";

        public static Dictionary<int, int> AreaClearDictionary(int LogicVer)
        {
            //Rando Version 1.5 = Logic Version 3
            //Rando Version 1.6 = Logic Version 5
            //Rando Version 1.7 = Logic Version 6
            //Rando Version 1.8 = Logic Version 8
            //Rando Version 1.9 - 1.11 = Logic Version 13
            //Entrance Rando Dev Build 1.11.0.2 = Logic Version 16 (Used to test entrance rando features)
            var EntAreaDict = new Dictionary<int, int>();

            var AreaDicVersion = 0;

            if (!ValidVersions.Contains(LogicVer))
            { AreaDicVersion = ValidVersions.Aggregate((x, y) => Math.Abs(x - LogicVer) < Math.Abs(y - LogicVer) ? x : y); }
            else { AreaDicVersion = LogicVer; }

            switch (AreaDicVersion)
            {
                case 3:
                    EntAreaDict.Add(100, 99); //Woodfall Clear, Woodfall Entrance
                    EntAreaDict.Add(103, 102); //Snowhead Clear, Snowhead Entrance
                    EntAreaDict.Add(108, 107); //GreatBay Clear, GreatBay Entrance
                    EntAreaDict.Add(113, 112); //Ikana Clear, StoneTower Entrance
                    break;
                case 5:
                case 6:
                    EntAreaDict.Add(101, 100); //Woodfall Clear, Woodfall Entrance
                    EntAreaDict.Add(104, 103); //Snowhead Clear, Snowhead Entrance
                    EntAreaDict.Add(109, 108); //GreatBay Clear, GreatBay Entrance
                    EntAreaDict.Add(114, 113); //Ikana Clear, StoneTower Entrance
                    break;
                case 8:
                case 13:
                    EntAreaDict.Add(105, 104); //Woodfall Clear, Woodfall Entrance
                    EntAreaDict.Add(108, 107); //Snowhead Clear, Snowhead Entrance
                    EntAreaDict.Add(113, 112); //GreatBay Clear, GreatBay Entrance
                    EntAreaDict.Add(118, 117); //Ikana Clear, StoneTower Entrance
                    break;
                case 14:
                    EntAreaDict.Add(107, 106); //Woodfall Clear, Woodfall Entrance
                    EntAreaDict.Add(110, 109); //Snowhead Clear, Snowhead Entrance
                    EntAreaDict.Add(115, 114); //GreatBay Clear, GreatBay Entrance
                    EntAreaDict.Add(120, 119); //Ikana Clear, StoneTower Entrance
                    break;
            }
            return EntAreaDict;
        }

        public static string[] SwitchDictionary(LogicObjects.TrackerInstance Instance)
        {
            var Game = Instance.Game;
            var Currentversion = Instance.Version;

            string[] files = Directory.GetFiles(@"Recources");
            Dictionary<int, string> dictionaries = new Dictionary<int, string>();//< Int (Version),String (Path to the that dictionary)>
            Dictionary<int, string> Pairs = new Dictionary<int, string>();//< Int (Version),String (Path to the that dictionary)>
            int smallestDicEntry = 0;
            int largestDicEntry = 0;
            int smallestPairEntry = 0;
            int largestPairEntry = 0;
            foreach (var i in files)
            {
                var dic = "";
                if (Instance.IsMM()) { dic = "MMRDICTIONARY"; }
                if (Instance.IsOOT()) { dic = "OOTRDICTIONARY"; }
                if (i.Contains(dic))
                {
                    var entry = i.Replace("Recources\\" + dic + "V", "");
                    entry = entry.Replace(".csv", "");
                    int version = 0;
                    try { version = Int32.Parse(entry); }
                    catch { continue; }
                    dictionaries.Add(version, i);
                    if (version > largestDicEntry) { largestDicEntry = version; }
                    if (smallestDicEntry == 0) { smallestDicEntry = largestDicEntry; }
                    if (version < smallestDicEntry) { smallestDicEntry = version; }
                }
                if (i.Contains("ENTRANCEPAIRS"))
                {
                    var entry = i.Replace("Recources\\ENTRANCEPAIRSV", "");
                    entry = entry.Replace(".csv", "");
                    int version = 0;
                    try { version = Int32.Parse(entry); }
                    catch { continue; }
                    Pairs.Add(version, i);
                    if (version > largestPairEntry) { largestPairEntry = version; }
                    if (smallestPairEntry == 0) { smallestPairEntry = largestPairEntry; }
                    if (version < smallestPairEntry) { smallestPairEntry = version; }
                }

            }

            string[] currentdictionary = new string[2];
            var index = 0;

            if (dictionaries.ContainsKey(Currentversion)) { index = Currentversion; }
            else //If we are using a logic version that doesn't have a dictionary, use the dictioary with the closest version
            { index = dictionaries.Keys.Aggregate((x, y) => Math.Abs(x - Currentversion) < Math.Abs(y - Currentversion) ? x : y); }

            currentdictionary[0] = dictionaries[index];

            Console.WriteLine(currentdictionary[0]);

            if (Pairs.ContainsKey(Currentversion))
            { index = Currentversion; }
            else //If we are using a logic version that doesn't have a Pair List, use the pair List with the closest version
            { index = Pairs.Keys.Aggregate((x, y) => Math.Abs(x - Currentversion) < Math.Abs(y - Currentversion) ? x : y); }

            currentdictionary[1] = Pairs[index];

            Console.WriteLine(currentdictionary[1]);

            return currentdictionary;
        }

        public static int[] GetVersionFromLogicFile(string[] LogicFile)
        {
            //[0] Version, [1] Game (0 = MM, 1 = OOT)
            int[] version = new int[] { 0, 0 };
            if (LogicFile[0].Contains("-version"))
            {
                string line = LogicFile[0];
                if (line.Contains("-versionOOT")) { version[1] = 1; line = line.Replace("versionOOT", "version"); }
                if (line.Contains("-versionWW")) { version[1] = 2; line = line.Replace("versionWW", "version"); } //These are just examples
                if (line.Contains("-versionTP")) { version[1] = 3; line = line.Replace("versionTP", "version"); } //These games are not supported
                version[0] = Int32.Parse(line.Replace("-version ", ""));
            }
            return version;
        }

        //Tracker Version Handeling
        public static bool GetLatestTrackerVersion()
        {
            var CheckForUpdate = false;
            if (File.Exists("options.txt"))
            {
                foreach (var file in File.ReadAllLines("options.txt"))
                {
                    if (file.Contains("CheckForUpdates:1")) { CheckForUpdate = true; }
                }
            }
            if (!CheckForUpdate && (Control.ModifierKeys != Keys.Shift)) { return false; }

            var client = new GitHubClient(new ProductHeaderValue("MMR-Tracker"));
            var releases = client.Repository.Release.GetAll("Thedrummonger", "MMR-Tracker");
            var lateset = releases.Result[0];

            Console.WriteLine($"Latest Version: { lateset.TagName } Current Version { trackerVersion }");

            if (VersionHandeling.CompareVersions(lateset.TagName, trackerVersion))
            {
                if (Debugging.ISDebugging && (Control.ModifierKeys != Keys.Shift)) { Console.WriteLine($"Tracker Out of Date. Latest Version: { lateset.TagName } Current Version { trackerVersion }"); }
                else
                {
                    var Download = MessageBox.Show($"Your tracker version V{ trackerVersion } is out of Date! Would you like to download the latest version { lateset.TagName } ?", "Tracker Out of Date", MessageBoxButtons.YesNo);
                    if (Download == DialogResult.Yes) { { Process.Start(lateset.HtmlUrl); return true; } }
                }
            }
            return false;
        }

        public static bool CompareVersions(string V1, string V2)
        {
            List<int> Version1;
            List<int> Version2;
            try
            {
                Version1 = V1.Replace("V", "").Split('.').Select(x => Convert.ToInt32(x)).ToList();
                Version2 = V2.Replace("V", "").Split('.').Select(x => Convert.ToInt32(x)).ToList();
            }
            catch { return false; }

            for (var i = 0; i < Version1.Count(); i++)
            {
                if (i >= Version2.Count()) { Version2.Add(0); }
                if (Version1[i] > Version2[i]) { return true; }
                if (Version1[i] < Version2[i]) { return false; }
            }
            return false;
        }
    }
}
