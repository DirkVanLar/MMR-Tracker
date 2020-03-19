using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                else if ( line.StartsWith("-"))
                {
                    CDLogic.Add(new LogicObjects.LogicEntry { ID = counter, DictionaryName = line.Substring(2), IsFake = true });
                    counter++;
                }
            }
            bool isEntRand = LogicVersion < VersionHandeling.EntranceRandoVersion;

            List<LogicObjects.SpoilerData> SpoilerLog = ReadHTMLSpoilerLog("", isEntRand) ;
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
                            { entry.LocationArea = "Woodfall"; entry.ItemSubType = "Entrance"; }
                            if (entry.DictionaryName == "Snowhead Temple access")
                            { entry.LocationArea = "Snowhead"; entry.ItemSubType = "Entrance"; }
                            if (entry.DictionaryName == "Great Bay Temple access")
                            { entry.LocationArea = "Great Bay Cape"; entry.ItemSubType = "Entrance"; }
                            if (entry.DictionaryName == "Inverted Stone Tower Temple access")
                            { entry.LocationArea = "Stone Tower"; entry.ItemSubType = "Entrance"; }
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

            List<string> csv = new List<string>{"DictionaryName,LocationName,ItemName,LocationArea,ItemSubType,SpoilerLocation,SpoilerItem"};
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

            SaveFileDialog saveDic = new SaveFileDialog();
            saveDic.Filter = "CSV File (*.csv)|*.csv";
            saveDic.Title = "Save Dictionary File";
            saveDic.FileName = "MMRDICTIONARYV" + LogicVersion + ".csv";
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
                OpenFileDialog SpoilerFile = new OpenFileDialog();
                SpoilerFile.Title = "Select an HTML Spoiler Log";
                SpoilerFile.Filter = "HTML Spoiler Log (*.html)|*.html";
                SpoilerFile.FilterIndex = 1;
                SpoilerFile.Multiselect = false;
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
                if (line.Contains("<td class=\"spoiler itemname\"><span data-content=\"")|| line.Contains("<td class=\"spoiler itemname\"> <span data-content=\""))
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
            List<string> entrance = new List<string>();
            List<int> id = new List<int>();

            foreach (LogicObjects.SpoilerData Thing in SpoilerData)
            {
                if (Thing.ItemName == "Woodfall"|| Thing.ItemName == "Inverted Stone Tower" || Thing.ItemName == "Snowhead" || Thing.ItemName == "Great Bay")
                {
                    entrance.Add(Thing.ItemName);
                    id.Add(Thing.ItemID);
                }
            }
            foreach (LogicObjects.SpoilerData Thing in SpoilerData)
            {
                int counter = 0;
                foreach(string X in entrance)
                {
                    if (X == Thing.LocationName){Thing.LocationID = id[counter];}
                    counter++;
                }
            }
            return SpoilerData;
        }
        public static List<LogicObjects.SpoilerData> ReadTextSpoilerlog(string Path)
        {

            List<LogicObjects.SpoilerData> SpoilerData = new List<LogicObjects.SpoilerData>();

            if (Path == "")
            {
                OpenFileDialog SpoilerFile = new OpenFileDialog();
                SpoilerFile.Title = "Select A Logic File";
                SpoilerFile.Filter = "Text Spoiler Log (*.txt)|*.txt";
                SpoilerFile.FilterIndex = 1;
                SpoilerFile.Multiselect = false;
                if (SpoilerFile.ShowDialog() != DialogResult.OK) { return SpoilerData; }
                Path = SpoilerFile.FileName;
            }
            List<int> usedId = new List<int>();
            foreach(string line in File.ReadLines(Path))
            {
                LogicObjects.SpoilerData entry = new LogicObjects.SpoilerData();
                if (line.Contains("->"))
                {
                    var linedata = line.Split(new string[] { "->" }, StringSplitOptions.None);
                    linedata[0] = linedata[0].Replace("*", "");
                    linedata[1] = linedata[1].Replace("*", "");
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
            string[] Options = new string[7];
            Options[0] = JsonConvert.SerializeObject(LogicObjects.Logic);
            Options[1] = VersionHandeling.Version.ToString();
            Options[2] = "UseSOT:" + ((Pathfinding.UseSongOfTime) ? "1" : "0");
            Options[3] = "IncludeItems:" + ((Pathfinding.IncludeItemLocations) ? "1" : "0");
            Options[4] = "EntranceCouple:" + ((LogicEditing.CoupleEntrances) ? "1" : "0");
            Options[5] = "StrictLogic:" + ((LogicEditing.StrictLogicHandeling) ? "1" : "0");
            Options[6] = "ShowToolTip:" + ((Utility.ShowEntryNameTooltip) ? "1" : "0");
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "MMR Tracker Save (*.MMRTSAV)|*.MMRTSAV", FilterIndex = 1 };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return false; }
            File.WriteAllLines(saveDialog.FileName, Options);
            return true;
        }
        public static bool LoadInstance(string LogicFile)
        {
            string[] options = File.ReadAllLines(LogicFile);
            LogicObjects.Logic = JsonConvert.DeserializeObject<List<LogicObjects.LogicEntry>>(options[0]);
            VersionHandeling.Version = Int32.Parse(options[1]);
            Pathfinding.UseSongOfTime = (options[2] == "UseSOT:1");
            Pathfinding.IncludeItemLocations = (options[3] == "IncludeItems:1");
            LogicEditing.CoupleEntrances = (options[4] == "EntranceCouple:1");
            LogicEditing.StrictLogicHandeling = (options[5] == "StrictLogic:1");
            Utility.ShowEntryNameTooltip = (options[6] == "ShowToolTip:1");
            VersionHandeling.entranceRadnoEnabled = (VersionHandeling.isEntranceRando());
            Console.WriteLine(options[1]);
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
        public static bool PromptSave()
        {
            if (UnsavedChanges)
            {
                DialogResult result = System.Windows.Forms.MessageBox.Show("Would you like to save?", "You have unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel) { return false; }
                if (result == DialogResult.Yes) {
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
            LogicObjects.MMRDictionary = new List<LogicObjects.LogicDic>();
            LogicObjects.EntrancePairs = new Dictionary<int, int>();
            LogicObjects.RawLogicText = new List<string>();
        }
        public static string FileSelect(string title, string filter)
        {
            OpenFileDialog SelectedFile = new OpenFileDialog();
            SelectedFile.Title = title;
            SelectedFile.Filter = filter;
            SelectedFile.FilterIndex = 1;
            SelectedFile.Multiselect = false;
            if (SelectedFile.ShowDialog() != DialogResult.OK) { return ""; }
            return SelectedFile.FileName;
        }
        public static void UpdateNames (List<LogicObjects.LogicEntry> Logic)
        {
            LogicObjects.MMRDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDic>>(Utility.ConvertCsvFileToJsonObject(VersionHandeling.SwitchDictionary()[0]));
            foreach (var entry in Logic)
            {
               foreach(var dicent in LogicObjects.MMRDictionary)
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
        public static bool FilterSearch (LogicObjects.LogicEntry logic, string searchTerm, string NameToCompare)
        {
            if (searchTerm == "") { return true; }
            string[] searchTerms = searchTerm.Split('|');
            foreach(string term in searchTerms)
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
                    else
                    {
                        if (!NameToCompare.ToLower().Contains(subterm.ToLower())) { valid = false; }
                    }
                }
                if (valid) { return true; }
            }
            return false;
        }

        public static bool CheckforSpoilerLog(List<LogicObjects.LogicEntry> Logic)
        {
            foreach(var i in Logic)
            {
                if (i.SpoilerRandom > -1) { return true; }
            }
            return false;
        }
    }
}
