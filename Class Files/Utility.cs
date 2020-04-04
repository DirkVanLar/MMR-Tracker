using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    class Utility
    {

        public static List<List<LogicObjects.LogicEntry>> UndoList = new List<List<LogicObjects.LogicEntry>>();
        public static List<List<LogicObjects.LogicEntry>> RedoList = new List<List<LogicObjects.LogicEntry>>();
        public static bool UnsavedChanges = false;
        public static bool ShowEntryNameTooltip = true;
        public static bool UnradnomizeEntranesOnStartup = true;
        public static string BomberCode = "";
        public static string LotteryNumber = "";

        public static string ConvertCsvFileToJsonObject(string path)
        {
            var csv = new List<string[]>();
            var lines = File.ReadAllLines(path);

            foreach (string line in lines)
                csv.Add(line.Split(','));

            var properties = lines[0].Split(',');

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j], csv[i][j]);

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }
        public static void CreateDictionary()
        {
            string file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt)|*.txt");
            if (file == "") { return; }

            int counter = 0;
            List<LogicObjects.LogicEntry> CDLogic = new List<LogicObjects.LogicEntry>();
            //Create a logic list out of the logic file with only ID and dictionary name. Set IsFake to true.
            int LogicVersion = 0;
            foreach (var line in File.ReadAllLines(file))
            {
                if (line.Contains("-version"))
                { LogicVersion = Int32.Parse(line.Replace("-version ", "")); }
                else if (line.StartsWith("-"))
                {
                    CDLogic.Add(new LogicObjects.LogicEntry { ID = counter, DictionaryName = line.Substring(2), IsFake = true });
                    counter++;
                }
            }
            bool isEntRand = LogicVersion < VersionHandeling.EntranceRandoVersion;

            List<LogicObjects.SpoilerData> SpoilerLog = ReadHTMLSpoilerLog("", isEntRand);
            if (SpoilerLog.Count == 0) { return; }

            //For each entry in your logic list, check each entry in your spoiler log to find the rest of the data
            foreach (LogicObjects.LogicEntry entry in CDLogic)
            {
                foreach (LogicObjects.SpoilerData spoiler in SpoilerLog)
                {
                    if (spoiler.LocationID == entry.ID)
                    {
                        entry.IsFake = false;
                        entry.LocationName = spoiler.LocationName;
                        entry.SpoilerLocation = spoiler.LocationName;
                        entry.LocationArea = spoiler.LocationArea;
                        entry.ItemSubType = "Item";
                        if (entry.DictionaryName.Contains("Bottle:")) { entry.ItemSubType = "Bottle"; }
                        if (entry.DictionaryName.StartsWith("Entrance")) { entry.ItemSubType = "Entrance"; }

                        if (isEntRand)
                        {
                            if (entry.DictionaryName == "Woodfall Temple access")
                            { entry.LocationArea = "Dungeon Entrance"; entry.ItemSubType = "Dungeon Entrance"; }
                            if (entry.DictionaryName == "Snowhead Temple access")
                            { entry.LocationArea = "Dungeon Entrance"; entry.ItemSubType = "Dungeon Entrance"; }
                            if (entry.DictionaryName == "Great Bay Temple access")
                            { entry.LocationArea = "Dungeon Entrance"; entry.ItemSubType = "Dungeon Entrance"; }
                            if (entry.DictionaryName == "Inverted Stone Tower Temple access")
                            { entry.LocationArea = "Dungeon Entrance"; entry.ItemSubType = "Dungeon Entrance"; }
                        } //Dungeon Entrance Rando is dumb
                    }
                    if (spoiler.ItemID == entry.ID)
                    {
                        entry.IsFake = false; //Not necessary, might cause problem but also might fix them ¯\_(ツ)_/¯
                        entry.ItemName = spoiler.ItemName;
                        entry.SpoilerItem = spoiler.ItemName;
                    }
                }
            }

            List<string> csv = new List<string> { "DictionaryName,LocationName,ItemName,LocationArea,ItemSubType,SpoilerLocation,SpoilerItem" };
            //Write this data to list of strings formated as lines of csv and write that to a text file
            foreach (LogicObjects.LogicEntry entry in CDLogic)
            {
                if (!entry.IsFake)
                {
                    csv.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                         entry.DictionaryName, entry.LocationName, entry.ItemName, entry.LocationArea,
                         entry.ItemSubType, entry.SpoilerLocation, entry.SpoilerItem));
                }
            }

            SaveFileDialog saveDic = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                Title = "Save Dictionary File",
                FileName = "MMRDICTIONARYV" + LogicVersion + ".csv"
            };
            saveDic.ShowDialog();
            File.WriteAllLines(saveDic.FileName, csv);
        }
        public static List<LogicObjects.LogicEntry> CloneLogicList(List<LogicObjects.LogicEntry> logic)
        {
            //Create a deep copy of a logic object by converting it to a json and coverting it back.
            //I have no idea why this works and it seems silly but whatever.
            return JsonConvert.DeserializeObject<List<LogicObjects.LogicEntry>>(JsonConvert.SerializeObject(logic));
        }
        public static List<LogicObjects.SpoilerData> ReadHTMLSpoilerLog(string Path, bool isEntRand)
        {
            List<LogicObjects.SpoilerData> SpoilerData = new List<LogicObjects.SpoilerData>();

            if (Path == "")
            {
                OpenFileDialog SpoilerFile = new OpenFileDialog
                {
                    Title = "Select an HTML Spoiler Log",
                    Filter = "HTML Spoiler Log (*.html)|*.html",
                    FilterIndex = 1,
                    Multiselect = false
                };
                if (SpoilerFile.ShowDialog() != DialogResult.OK) { return SpoilerData; }
                Path = SpoilerFile.FileName;
            }

            string Region = "";
            LogicObjects.SpoilerData entry = new LogicObjects.SpoilerData();
            foreach (string line in File.ReadAllLines(Path))
            {
                if (line.Contains("<tr class=\"region\">"))
                {
                    Region = line.Trim();
                    Region = Region.Replace("<tr class=\"region\"><td colspan=\"3\">", "");
                    Region = Region.Replace("</td></tr>", "");
                }
                if (line.Contains("data-newlocationid="))
                {
                    var X = line.Trim().Split('"');
                    entry.LocationID = Int32.Parse(X[3]);
                    entry.ItemID = Int32.Parse(X[1]);
                }
                if (line.Contains("<td class=\"newlocation\">"))
                {
                    var X = line.Trim();
                    X = X.Replace("<td class=\"newlocation\">", "");
                    X = X.Replace("</td>", "");
                    entry.LocationName = X;
                }
                if (line.Contains("<td class=\"spoiler itemname\"><span data-content=\"") || line.Contains("<td class=\"spoiler itemname\"> <span data-content=\""))
                {
                    var X = line.Trim();
                    X = X.Replace("<td class=\"spoiler itemname\"> <span data-content=\"", "");
                    X = X.Replace("<td class=\"spoiler itemname\"><span data-content=\"", "");
                    X = X.Replace("\"></span></td>", "");
                    entry.ItemName = X;
                    entry.LocationArea = Region;
                    SpoilerData.Add(entry);
                    entry = new LogicObjects.SpoilerData();
                }
                if (line.Contains("<h2>Item Locations</h2>")) { break; }
            }

            if (isEntRand) { return SpoilerData; }

            //Fix Dungeon Entrances
            Dictionary<string, int> EntIDMatch = new Dictionary<string, int>();
            var entranceIDs = VersionHandeling.AreaClearDictionary();
            foreach (LogicObjects.SpoilerData Thing in SpoilerData)
            {
                if (entranceIDs.ContainsValue(Thing.ItemID)) { EntIDMatch.Add(Thing.ItemName, Thing.ItemID); }
            }
            foreach (LogicObjects.SpoilerData Thing in SpoilerData)
            {
                if (EntIDMatch.ContainsKey(Thing.LocationName)) { Thing.LocationID = EntIDMatch[Thing.LocationName]; }
            }

            return SpoilerData;
        }
        public static List<LogicObjects.SpoilerData> ReadTextSpoilerlog(string Path)
        {

            List<LogicObjects.SpoilerData> SpoilerData = new List<LogicObjects.SpoilerData>();

            if (Path == "")
            {
                OpenFileDialog SpoilerFile = new OpenFileDialog
                {
                    Title = "Select A Logic File",
                    Filter = "Text Spoiler Log (*.txt)|*.txt",
                    FilterIndex = 1,
                    Multiselect = false
                };
                if (SpoilerFile.ShowDialog() != DialogResult.OK) { return SpoilerData; }
                Path = SpoilerFile.FileName;
            }
            List<int> usedId = new List<int>();
            foreach (string line in File.ReadLines(Path))
            {
                LogicObjects.SpoilerData entry = new LogicObjects.SpoilerData();
                if (line.Contains("->"))
                {
                    var linedata = line.Split(new string[] { "->" }, StringSplitOptions.None);
                    linedata[0] = linedata[0].Replace("*", "");//Not sure if this is neccassary but I'm to
                    linedata[1] = linedata[1].Replace("*", "");//lazy to check and it's not hurting anything
                    entry.LocationName = linedata[0].Trim();
                    entry.ItemName = linedata[1].Trim();
                    entry.LocationID = -2;
                    entry.ItemID = -2;
                    bool itemfound = false;
                    foreach (LogicObjects.LogicEntry X in LogicObjects.Logic)
                    {
                        if (X.SpoilerLocation == entry.LocationName) { entry.LocationID = X.ID; }
                        if (X.SpoilerItem == entry.ItemName && !usedId.Contains(X.ID) && !itemfound)
                        { entry.ItemID = X.ID; usedId.Add(X.ID); itemfound = true; }
                    }
                    SpoilerData.Add(entry);
                }
            }
            return SpoilerData;
        }
        public static bool SaveInstance()
        {
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "MMR Tracker Save (*.MMRTSAV)|*.MMRTSAV", FilterIndex = 1 };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return false; }
            string[] Options = new string[12];
            Options[0] = JsonConvert.SerializeObject(LogicObjects.Logic);
            Options[1] = "version:" + VersionHandeling.Version.ToString();
            Options[2] = "UseSOT:" + ((Pathfinding.UseSongOfTime) ? "1" : "0");
            Options[3] = "IncludeItems:" + ((Pathfinding.IncludeItemLocations) ? "1" : "0");
            Options[4] = "EntranceCouple:" + ((LogicEditing.CoupleEntrances) ? "1" : "0");
            Options[5] = "StrictLogic:" + ((LogicEditing.StrictLogicHandeling) ? "1" : "0");
            Options[6] = "ShowToolTip:" + ((Utility.ShowEntryNameTooltip) ? "1" : "0");
            Options[7] = "EntRadno:" + ((VersionHandeling.entranceRadnoEnabled) ? "1" : "0");
            Options[8] = "AutoEntRand:" + ((VersionHandeling.OverRideAutoEntranceRandoEnable) ? "1" : "0");
            Options[9] = "OOTSave:" + ((OOT_Support.isOOT) ? "1" : "0");
            Options[10] = "BomberCode:" + Utility.BomberCode;
            Options[11] = "LotteryNumber:" + Utility.BomberCode;
            File.WriteAllLines(saveDialog.FileName, Options);
            UnsavedChanges = false;
            return true;
        }
        public static bool LoadInstance(string LogicFile)
        {
            string[] options = File.ReadAllLines(LogicFile);
            LogicObjects.Logic = JsonConvert.DeserializeObject<List<LogicObjects.LogicEntry>>(options[0]);
            if (options.Length > 1) { VersionHandeling.Version = Int32.Parse(options[1].Replace("version:","")); }
            if (options.Length > 2) { Pathfinding.UseSongOfTime = (options[2] == "UseSOT:1"); }
            if (options.Length > 3) { Pathfinding.IncludeItemLocations = (options[3] == "IncludeItems:1"); }
            if (options.Length > 4) { LogicEditing.CoupleEntrances = (options[4] == "EntranceCouple:1"); }
            if (options.Length > 5) { LogicEditing.StrictLogicHandeling = (options[5] == "StrictLogic:1"); }
            if (options.Length > 6) { Utility.ShowEntryNameTooltip = (options[6] == "ShowToolTip:1"); }
            if (options.Length > 7) { VersionHandeling.entranceRadnoEnabled = (options[7] == "EntRadno:1"); }
            if (options.Length > 8) { VersionHandeling.OverRideAutoEntranceRandoEnable = (options[8] == "AutoEntRand:1"); }
            if (options.Length > 9) { OOT_Support.isOOT = (options[9] == "OOTSave:1"); }
            if (options.Length > 10) { Utility.BomberCode = (options[9].Replace("BomberCode:", "")); }
            if (options.Length > 11) { Utility.BomberCode = (options[9].Replace("LotteryNumber:", "")); }
            return true;
        }
        public static void SaveState(List<LogicObjects.LogicEntry> logic)
        {
            UndoList.Add(CloneLogicList(logic));
            RedoList = new List<List<LogicObjects.LogicEntry>>();
        }
        public static void Undo()
        {
            if (UndoList.Any())
            {
                UnsavedChanges = true;
                var lastItem = UndoList.Count - 1;
                RedoList.Add(CloneLogicList(LogicObjects.Logic));
                LogicObjects.Logic = CloneLogicList(UndoList[lastItem]);
                UndoList.RemoveAt(lastItem);
            }
        }
        public static void Redo()
        {
            if (RedoList.Any())
            {
                UnsavedChanges = true;
                var lastItem = RedoList.Count - 1;
                UndoList.Add(CloneLogicList(LogicObjects.Logic));
                LogicObjects.Logic = CloneLogicList(RedoList[lastItem]);
                RedoList.RemoveAt(lastItem);
            }
        }
        public static bool PromptSave(bool OnlyIfUnsaved = true)
        {
            if (UnsavedChanges || !OnlyIfUnsaved)
            {
                DialogResult result = MessageBox.Show("Would you like to save?", "You have unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel) { return false; }
                if (result == DialogResult.Yes)
                {
                    if (!SaveInstance()) { return false; }
                }
            }
            return true;
        }
        public static void ResetInstance()
        {
            UnsavedChanges = false;
            VersionHandeling.entranceRadnoEnabled = false;
            UndoList = new List<List<LogicObjects.LogicEntry>>();
            RedoList = new List<List<LogicObjects.LogicEntry>>();
            VersionHandeling.Version = 0;
            LogicObjects.Logic = new List<LogicObjects.LogicEntry>();
            LogicObjects.DicNameToID = new Dictionary<string, int>();
            LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry();
            LogicObjects.MMRDictionary = new List<LogicObjects.LogicDictionaryEntry>();
            LogicObjects.EntrancePairs = new Dictionary<int, int>();
        }
        public static string FileSelect(string title, string filter)
        {
            OpenFileDialog SelectedFile = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                FilterIndex = 1,
                Multiselect = false
            };
            if (SelectedFile.ShowDialog() != DialogResult.OK) { return ""; }
            return SelectedFile.FileName;
        }
        public static void UpdateNames(List<LogicObjects.LogicEntry> Logic)
        {
            LogicObjects.MMRDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDictionaryEntry>>(Utility.ConvertCsvFileToJsonObject(VersionHandeling.SwitchDictionary(VersionHandeling.Version, OOT_Support.isOOT)[0]));
            foreach (var entry in Logic)
            {
                foreach (var dicent in LogicObjects.MMRDictionary)
                {
                    if (entry.DictionaryName == dicent.DictionaryName)
                    {
                        entry.LocationName = dicent.LocationName;
                        entry.ItemName = dicent.ItemName;
                        entry.LocationArea = dicent.LocationArea;
                        entry.ItemSubType = dicent.ItemSubType;
                        entry.SpoilerItem = dicent.SpoilerItem;
                        entry.SpoilerLocation = dicent.SpoilerLocation;
                        break;
                    }
                }
            }
        }
        public static bool FilterSearch(LogicObjects.LogicEntry logic, string searchTerm, string NameToCompare, LogicObjects.LogicEntry RandomizedItem = null)
        {
            if (searchTerm == "") { return true; }
            string[] searchTerms = searchTerm.Split('|');
            foreach (string term in searchTerms)
            {
                string[] subTerms = term.Split(',');
                bool valid = true;
                foreach (string subterm in subTerms)
                {
                    if (subterm == "") { continue; }
                    if (subterm[0] == '#')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (!logic.LocationArea.ToLower().Contains(subterm.Substring(1).ToLower())) { valid = false; }
                    }
                    else if (subterm[0] == '@')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (!logic.ItemSubType.ToLower().Contains(subterm.Substring(1).ToLower())) { valid = false; }
                    }
                    else if (subterm[0] == '$')
                    {
                        if (subterm.Substring(1) == "" || logic.ItemName == null) { continue; }
                        if (!logic.ItemName.ToLower().Contains(subterm.Substring(1).ToLower())) { valid = false; }
                    }
                    else if (subterm[0] == '%')
                    {
                        if (subterm.Substring(1) == "" || logic.LocationName == null) { continue; }
                        if (!logic.LocationName.ToLower().Contains(subterm.Substring(1).ToLower())) { valid = false; }
                    }
                    else if (subterm[0] == '&')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (RandomizedItem == null) { valid = false; }
                        else if (!RandomizedItem.ItemName.ToLower().Contains(subterm.Substring(1).ToLower())) { valid = false; }
                    }
                    else
                    {
                        if (!NameToCompare.ToLower().Contains(subterm.ToLower())) { valid = false; }
                    }
                }
                if (valid) { return true; }
            }
            return false;
        }
        public static bool CheckforSpoilerLog(List<LogicObjects.LogicEntry> Logic, bool full = false)
        {
            bool fullLog = true;
            bool Spoiler = false;
            foreach (var i in Logic)
            {
                if (i.SpoilerRandom > -1) 
                { 
                    Spoiler = true; 
                }
                if (i.SpoilerRandom < 0 && !i.IsFake) 
                { 
                    fullLog = false;
                    if (full) { Console.WriteLine(i.DictionaryName + " Does not have SpoilerData"); }
                }
            }
            return (full) ? fullLog : Spoiler;
        }
        public static bool IsDivider(string text)
        {
            if (text == null || text == "") { return false; }
            int occurences = 0;
            foreach (var i in text)
            {
                if (i == '=') { occurences++; }
            }
            return (occurences >= 5);
        }
        public static bool CheckForRandomEntrances(List<LogicObjects.LogicEntry> logic, int validEntranceCount = 6)
        {
            if (!VersionHandeling.IsEntranceRando()) { return false; }
            int count = 0;
            foreach (var i in logic)
            {
                if (i.IsEntrance() && (i.Options == 0 || i.Options == 2)) { count += 1; }
                if (count >= validEntranceCount) { return true; }
            }
            return false;
        }
        public static Dictionary<int, int> CreateRandItemDic(List<LogicObjects.LogicEntry> logic, bool Spoiler = false)
        {
            var spoilerDic = new Dictionary<int, int>();
            foreach (var i in logic)
            {
                var value = (Spoiler) ? i.SpoilerRandom : i.RandomizedItem;
                if (value > -2 && !spoilerDic.ContainsKey(value))
                {
                    spoilerDic.Add(value, i.ID);
                }
            }
            return spoilerDic;
        }
        public static int BoolToInt(bool Bool, bool FalseFirst = true)
        {
            return Bool ? (FalseFirst ? 1 : 0 ) : (FalseFirst ? 0 : 1);
        } 
        public static List<string> SeperateStringByMeasurement(ListBox container, string Measure, string indent = "    ")
        {
            Font font = container.Font;
            int width = container.Width;
            Graphics g = container.CreateGraphics();

            if (IsDivider(Measure) || (int)g.MeasureString(Measure, font).Width < width) { return new List<string> { Measure }; }

            List<string> ShortenedStrings = new List<string>();
            string[] words = Measure.Split(' ');
            string ShortenedString = "";
            foreach (string word in words)
            {
                string testString = ShortenedString + word + " ";
                int Stringwidth = (int)g.MeasureString(testString, font).Width;
                if (Stringwidth > width)
                {
                    ShortenedStrings.Add(ShortenedString);
                    ShortenedString = indent + word + " ";
                }
                else
                {
                    ShortenedString = ShortenedString + word + " ";
                }
            }
            ShortenedStrings.Add(ShortenedString);
            return ShortenedStrings;
        }
        public static string CreateDivider(ListBox container, string DividerText = "")
        {
            Font font = container.Font;
            int width = container.Width;
            Graphics g = container.CreateGraphics();

            int marks = 1;
            string Divider = "";
            while((int)g.MeasureString(Divider, font).Width <= width)
            {
                string Section = "";
                for (var i = 0; i < marks; i++)
                {
                    Section += "=";
                }
                Divider = Section + DividerText + Section;
                marks++;
            }
            return Divider;
        }
        public static void CheckforOptionsFile()
        {
            if (!File.Exists("options.txt"))
            {
                var file = File.Create("options.txt");
                if (!Debugging.ISDebugging || (Control.ModifierKeys == Keys.Shift))
                {
                    var firsttime = MessageBox.Show("Welcome to the Majoras Mask Randomizer Tracker by thedrummonger! It looks like this is your first time running the tracker. If that is the case select Yes, otherwise select No.", "First Time Setup", MessageBoxButtons.YesNo);
                    if (firsttime == DialogResult.Yes)
                    {
                        MessageBox.Show("Please Take this opportunity to familliarize yourself with how to use this tracker. There are many features that are not obvious or explained anywhere outside of the about page. This information can be accessed at any time by selecting 'Info' -> 'About'. Click OK to show the About Page. Once you have read through the information, close the window to return to setup.", "How to Use", MessageBoxButtons.OK);
                        DebugScreen DebugScreen = new DebugScreen();
                        Debugging.PrintLogicObject(LogicObjects.Logic);
                        DebugScreen.DebugFunction = 2;
                        DebugScreen.ShowDialog();
                    }

                    var DefaultSetting = MessageBox.Show("If you would like to change the default options, press Yes. Otherwise, press No. Selecting Cancel at an option prompt will leave it default. These can be changed later in the Options text document that will be created in your tracker folder. The can also be changed per instance in the options tab", "Default Setting", MessageBoxButtons.YesNo);
                    if (DefaultSetting != DialogResult.Yes) { return; }

                    var ShowToolTips = MessageBox.Show("Would you like to see tooltips that display the full name of an item when you mouse over it?", "Show Tool Tips", MessageBoxButtons.YesNoCancel);
                    if (ShowToolTips == DialogResult.No) { Utility.ShowEntryNameTooltip = false; }

                    var DisableEntrances = MessageBox.Show("Would you like the tracker to automatically mark entrances as unrandomized when creating an instance? This is usefull if you don't plan to use entrance randomizer often.", "Start with Entrance Rando", MessageBoxButtons.YesNoCancel);
                    if (DisableEntrances == DialogResult.No) { Utility.UnradnomizeEntranesOnStartup = false; }

                    var UpdateCheck = MessageBox.Show("Would you like the tracker to notify you when a newer version has been released?", "Check For Updates", MessageBoxButtons.YesNoCancel);
                    if (UpdateCheck == DialogResult.No) { VersionHandeling.CheckForUpdate = false; }
                }

                if (Debugging.ISDebugging) { Utility.UnradnomizeEntranesOnStartup = false; }

                string[] options = new string[] { "ToolTips:" + ((Utility.ShowEntryNameTooltip) ? 1 : 0), "DisableEntrancesOnStartup:" + ((Utility.UnradnomizeEntranesOnStartup) ? 1 : 0), "CheckForUpdates:" + ((VersionHandeling.CheckForUpdate) ? 1 : 0) };
                file.Close();
                File.WriteAllLines("options.txt", options);
            }
            else
            {
                foreach (var line in File.ReadAllLines("options.txt"))
                {
                    if (line.StartsWith("ToolTips:")) { Utility.ShowEntryNameTooltip = line.Contains("ToolTips:1"); }
                    if (line.StartsWith("DisableEntrancesOnStartup:")) { Utility.UnradnomizeEntranesOnStartup = line.Contains("DisableEntrancesOnStartup:1"); }
                    if (line.StartsWith("CheckForUpdates:")) { VersionHandeling.CheckForUpdate = line.Contains("CheckForUpdates:1"); }
                    if (line.StartsWith("Dev:")) { Debugging.ISDebugging = line.Contains("Dev:1"); }
                }
            }

        }
    }
}
