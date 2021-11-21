using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MMR_Tracker.Class_Files;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MMR_Tracker_V2
{
    class VersionHandeling
    {
        //Logic Version Handeling
        public static string trackerVersion = "V1.11";
        public static int TrackerVersionStatus = 0;

        //Rando Versions
        //Rando Version 1.5 = Logic Version 3
        //Rando Version 1.6 = Logic Version 5
        //Rando Version 1.7 = Logic Version 6
        //Rando Version 1.8 = Logic Version 8
        //Rando Version 1.9 - 1.11 = Logic Version 13
        //Entrance Rando Dev Build 1.11.0.2 = Logic Version 16 (Used to test entrance rando features)


        public static string GetDictionaryPath(LogicObjects.TrackerInstance Instance, bool JSON = false)
        {
            var Currentversion = Instance.LogicVersion;
            //Get the dictionary
            Dictionary<int, string> dictionaries = new Dictionary<int, string>();//< Int (Version),String (Path to the that dictionary)>
            var dic = Instance.GameCode + "DICTIONARY";

            if (JSON)
            {
                foreach (var i in Directory.GetFiles(@"Recources\Dictionaries").Where(x => x.Contains(dic)).ToArray())
                {
                    var entry = i.Replace("Recources\\Dictionaries\\" + dic + "V", "").Replace(".csv", "");
                    int version = 0;
                    try { version = Int32.Parse(entry); }
                    catch { continue; }
                    dictionaries.Add(version, i);
                }
            }
            else
            {
                foreach (var i in Directory.GetFiles(@"Recources\Dictionaries\Legacy and non MM").Where(x => x.Contains(dic)).ToArray())
                {
                    var entry = i.Replace("Recources\\Dictionaries\\Legacy and non MM\\" + dic + "V", "").Replace(".csv", "");
                    int version = 0;
                    try { version = Int32.Parse(entry); }
                    catch { continue; }
                    dictionaries.Add(version, i);
                }
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

        public static string GetJSONDictionaryPath(LogicObjects.TrackerInstance Instance)
        {
            string currentdictionary = "";
            int Versionoffset = -1;
            foreach (var i in Directory.GetFiles(@"Recources\Dictionaries").ToArray())
            {
                LogicObjects.LogicDictionary LogicDic = new LogicObjects.LogicDictionary();
                try 
                {
                    LogicDic = JsonConvert.DeserializeObject<LogicObjects.LogicDictionary>(File.ReadAllText(i));
                    if (Instance.GameCode == LogicDic.GameCode && Instance.LogicFormat == LogicDic.LogicFormat)
                    {
                        int offset = Math.Abs(Instance.LogicVersion - LogicDic.LogicVersion);
                        if (Versionoffset == -1 || Versionoffset > offset)
                        {
                            currentdictionary = i;
                            Versionoffset = offset;
                        }
                    }
                } 
                catch { continue; }
            }
            Debugging.Log("Json Dictionary " + currentdictionary);
            Debugging.Log($"Dictionary was {Versionoffset} versions off");
            return currentdictionary;
        }

        public static LogicObjects.VersionInfo GetVersionDataFromLogicFile(string[] LogicFile)
        {
            LogicObjects.VersionInfo versionData = new LogicObjects.VersionInfo { Version = 0, Gamecode = "MMR" };
            LogicObjects.LogicFile NewformatLogicFile = null;
            try { NewformatLogicFile = LogicObjects.LogicFile.FromJson(string.Join("", LogicFile)); }
            catch { }

            if (NewformatLogicFile != null)
            {
                versionData.Version = NewformatLogicFile.Version;
                return versionData;
            }

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

            var VersionSatus = VersionHandeling.CompareVersions(lateset.TagName, trackerVersion);

            Debugging.Log($"Latest Version: { lateset.TagName } Current Version { trackerVersion }");
            if (VersionSatus == 0) { Debugging.Log($"Using Current Version"); }
            else if (VersionSatus < 0) { Debugging.Log($"Using Unreleased Dev Version"); TrackerVersionStatus = 1; }
            else if (VersionSatus > 0)
            {
                if (Debugging.ISDebugging && (Control.ModifierKeys != Keys.Shift)) { Debugging.Log($"Using Outdated Version"); }
                else
                {
                    var Download = MessageBox.Show($"Your tracker version { trackerVersion } is out of Date. Would you like to download the latest version { lateset.TagName }?", "Tracker Out of Date", MessageBoxButtons.YesNo);
                    if (Download == DialogResult.Yes) { { Process.Start(lateset.HtmlUrl); return true; } }
                }
                TrackerVersionStatus = -1;
            }
            return false;
        }

        public static int CompareVersions(string V1, string V2)
        {
            if (!V1.Contains(".")) { V1 += ".0"; }
            if (!V2.Contains(".")) { V2 += ".0"; }
            var CleanedV1 = new Version(string.Join("", V1.Where(x => char.IsDigit(x) || x == '.')));
            var CleanedV2 = new Version(string.Join("", V2.Where(x => char.IsDigit(x) || x == '.')));
            return CleanedV1.CompareTo(CleanedV2);
        }
    }
}
