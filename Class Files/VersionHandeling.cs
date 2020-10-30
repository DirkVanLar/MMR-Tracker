using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MMR_Tracker.Class_Files;

namespace MMR_Tracker_V2
{
    class VersionHandeling
    {
        //Logic Version Handeling
        public static string trackerVersion = "V1.11";

        public static Dictionary<int, int> AreaClearDictionary(LogicObjects.TrackerInstance Instance)
        {
            //Rando Version 1.5 = Logic Version 3
            //Rando Version 1.6 = Logic Version 5
            //Rando Version 1.7 = Logic Version 6
            //Rando Version 1.8 = Logic Version 8
            //Rando Version 1.9 - 1.11 = Logic Version 13
            //Entrance Rando Dev Build 1.11.0.2 = Logic Version 16 (Used to test entrance rando features)
            var EntAreaDict = new Dictionary<int, int>();

            if (!Instance.IsMM()) { return EntAreaDict; }

            var WoodfallClear = Instance.Logic.Find(x => x.DictionaryName == "Woodfall clear");
            var WoodfallAccess = Instance.Logic.Find(x => x.DictionaryName == "Woodfall Temple access" && !x.IsFake);
            if (WoodfallAccess == null || WoodfallClear == null) { return new Dictionary<int, int>(); }
            EntAreaDict.Add(WoodfallClear.ID, WoodfallAccess.ID);

            var SnowheadClear = Instance.Logic.Find(x => x.DictionaryName == "Snowhead clear");
            var SnowheadAccess = Instance.Logic.Find(x => x.DictionaryName == "Snowhead Temple access" && !x.IsFake);
            if (SnowheadAccess == null || SnowheadClear == null) { return new Dictionary<int, int>(); }
            EntAreaDict.Add(SnowheadClear.ID, SnowheadAccess.ID);

            var GreatBayClear = Instance.Logic.Find(x => x.DictionaryName == "Great Bay clear");
            var GreatBayAccess = Instance.Logic.Find(x => x.DictionaryName == "Great Bay Temple access" && !x.IsFake);
            if (GreatBayAccess == null || GreatBayClear == null) { return new Dictionary<int, int>(); }
            EntAreaDict.Add(GreatBayClear.ID, GreatBayAccess.ID);

            var StoneTowerClear = Instance.Logic.Find(x => x.DictionaryName == "Ikana clear");
            var StoneTowerAccess = Instance.Logic.Find(x => x.DictionaryName == "Inverted Stone Tower Temple access" && !x.IsFake);
            if (StoneTowerAccess == null || StoneTowerClear == null) { return new Dictionary<int, int>(); }
            EntAreaDict.Add(StoneTowerClear.ID, StoneTowerAccess.ID);

            return EntAreaDict;
        }

        public static string GetDictionaryPath(LogicObjects.TrackerInstance Instance)
        {
            var Currentversion = Instance.LogicVersion;
            //Get the dictionary
            Dictionary<int, string> dictionaries = new Dictionary<int, string>();//< Int (Version),String (Path to the that dictionary)>
            var dic = Instance.GameCode + "DICTIONARY";
            foreach (var i in Directory.GetFiles(@"Recources\Dictionaries").Where(x => x.Contains(dic)).ToArray())
            {
                var entry = i.Replace("Recources\\Dictionaries\\" + dic + "V", "").Replace(".csv", "");
                int version = 0;
                try { version = Int32.Parse(entry); }
                catch { continue; }
                dictionaries.Add(version, i);
            }

            string currentdictionary;
            if (!dictionaries.Any()) { currentdictionary = ""; }
            else
            {
                var index = 0;
                if (dictionaries.ContainsKey(Currentversion)) { index = Currentversion; }
                else //If we are using a logic version that doesn't have a dictionary, use the dictioary with the closest version
                { index = dictionaries.Keys.Aggregate((x, y) => Math.Abs(x - Currentversion) < Math.Abs(y - Currentversion) ? x : y); }
                currentdictionary = dictionaries[index];
            }

            Debugging.Log(currentdictionary);
            return currentdictionary;
        }

        public static LogicObjects.VersionInfo GetVersionDataFromLogicFile(string[] LogicFile)
        {
            LogicObjects.VersionInfo versionData = new LogicObjects.VersionInfo { Version = 0, Gamecode = "MMR" };
            if (LogicFile[0].Contains("-version"))//Ensure the first line of this file has version data
            {
                if (!LogicFile[0].Contains("-version "))//Check if the version line has game code data after "-version"
                {
                    var i = LogicFile[0].Split(' ');
                    versionData.Gamecode = i[0].Replace("-version", "");
                }
                var j = LogicFile[0].Split(' ');
                if (j.Count() > 1 && int.TryParse(j[1], out int Ver))
                {
                    versionData.Version = Ver;
                }
            }
            return versionData;
        }

        //Tracker Version Handeling
        public static bool GetLatestTrackerVersion()
        {
            var CheckForUpdate = File.Exists("options.txt") && File.ReadAllLines("options.txt").Any(x => x.Contains("CheckForUpdates:1"));
            if (!CheckForUpdate && (Control.ModifierKeys != Keys.Shift)) { return false; }

            var client = new GitHubClient(new ProductHeaderValue("MMR-Tracker"));
            var lateset = client.Repository.Release.GetLatest("Thedrummonger", "MMR-Tracker").Result;

            Debugging.Log($"Latest Version: { lateset.TagName } Current Version { trackerVersion }");

            if (VersionHandeling.CompareVersions(lateset.TagName, trackerVersion))
            {
                if (Debugging.ISDebugging && (Control.ModifierKeys != Keys.Shift)) { Debugging.Log($"Tracker Out of Date. Latest Version: { lateset.TagName } Current Version { trackerVersion }"); }
                else
                {
                    var Download = MessageBox.Show($"Your tracker version { trackerVersion } is out of Date. Would you like to download the latest version { lateset.TagName }?", "Tracker Out of Date", MessageBoxButtons.YesNo);
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
