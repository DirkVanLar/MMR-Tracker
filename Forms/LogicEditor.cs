using MMR_Tracker_V2;
using MMR_Tracker.Forms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace MMR_Tracker.Forms
{
    public partial class LogicEditor : Form
    {
        public LogicEditor()
        {
            InitializeComponent();
        }

        public static List<LogicObjects.LogicEntry> LogicList = new List<LogicObjects.LogicEntry>();
        public static List<LogicObjects.LogicDic> EditorDictionary = new List<LogicObjects.LogicDic>();
        public static bool isOOT = false;
        public static bool GetOOTDictionary = false;
        public static int versionNumber = 0;
        public static LogicObjects.LogicEntry currentEntry = new LogicObjects.LogicEntry();
        public static bool PrintingItem = false;
        public static bool UsingTrackerLogic = false;
        public static bool UnsavedChanges = false;
        public static List<int> GoBackList = new List<int>();
        public static List<List<LogicObjects.LogicEntry>> UndoList = new List<List<LogicObjects.LogicEntry>>();
        public static List<List<LogicObjects.LogicEntry>> RedoList = new List<List<LogicObjects.LogicEntry>>();
        public static bool UseSpoilerInDisplay = false;
        public static bool UseDictionaryNameInSearch = false;

        public class RequiementConditional
        {
            public string DisplayName { get; set; }
            public List<LogicObjects.LogicEntry> ItemIDs { get; set; }
            public override string ToString()
            {
                return DisplayName;
            }
        }

        //Form

        private void LogicEditor_Load(object sender, EventArgs e)
        {
            nudIndex.Value = 0;
            if (VersionHandeling.Version > 0)
            {
                UsingTrackerLogic = true;
                versionNumber = VersionHandeling.Version;
                LogicList = Utility.CloneLogicList(LogicObjects.Logic);
                FormatForm();
            }
            else
            {
                LogicList = new List<LogicObjects.LogicEntry>();
                FormatForm();
            }
            useLocationItemNamesToolStripMenuItem.Text = (UseDictionaryNameInSearch) ? "Use Location/Item Name" : "Use Dictionary Name";
            displaySpoilerLogNamesToolStripMenuItem.Text = (UseSpoilerInDisplay) ? "Use Tracker names" : "Use Spoiler Log names";
            AssignUniqueItemnames(LogicList);
        }

        private void FormatForm(int StartAt = 0)
        {
            bool enabled = (LogicList.Count > 0);
            btnAddReq.Enabled = enabled;
            btnAddCond.Enabled = enabled;
            //button3.Enabled = false;
            saveLogicToolStripMenuItem.Visible = enabled;
            //button5.Enabled = false;
            applyToTrackerLogicToolStripMenuItem.Visible = enabled;
            btnRemoveReq.Enabled = enabled;
            btnRemoveCond.Enabled = enabled;
            newLogicToolStripMenuItem.Visible = enabled;
            btnGoTo.Enabled = enabled;
            btnBack.Enabled = enabled;
            nudIndex.Enabled = enabled;
            chkOnDay1.Enabled = enabled;
            chkOnDay2.Enabled = enabled;
            chkOnDay3.Enabled = enabled;
            chkOnNight1.Enabled = enabled;
            chkOnNight2.Enabled = enabled;
            chkOnNight3.Enabled = enabled;
            chkNeedDay1.Enabled = enabled;
            chkNeedDay2.Enabled = enabled;
            chkNeedDay3.Enabled = enabled;
            chkNeedNight1.Enabled = enabled;
            chkNeedNight2.Enabled = enabled;
            chkNeedNight3.Enabled = enabled;
            undoToolStripMenuItem.Visible = enabled;
            redoToolStripMenuItem.Visible = enabled;
            btnEditSelected.Enabled = enabled;
            lblDicName.Text = "";
            lblLocName.Text = "";
            lblItemName.Text = "";

            if (enabled)
            {
                WriteCurentItem(StartAt);
            }

        }

        private void LogicEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!PromptSave()) { e.Cancel = true; }
        }

        //Button

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            if (!PromptSave()) { return; }
            string file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSET)|*.txt;*.MMRTSET");
            if (file == "") { return; }
            GoBackList = new List<int>();
            UsingTrackerLogic = false;
            bool SettingsFile = file.EndsWith(".MMRTSET");
            var lines = (SettingsFile) ? File.ReadAllLines(file).Skip(2) : File.ReadAllLines(file);
            LogicList = new List<LogicObjects.LogicEntry>();
            ReadLogicFile(lines.ToArray());
            AssignUniqueItemnames(LogicList);
            if (LogicList.Count < Convert.ToInt32(nudIndex.Value)) { nudIndex.Value = LogicList.Count - 1; }
            FormatForm(Convert.ToInt32(nudIndex.Value));
        }

        private void BtnGoTo_Click(object sender, EventArgs e)
        {
            try
            {
                GoBackList.Add(currentEntry.ID);
                ItemSelect Selector = new ItemSelect();
                ItemSelect.Function = 4;
                Selector.ShowDialog();
                nudIndex.Value = LogicObjects.CurrentSelectedItem.ID;
                WriteCurentItem(LogicObjects.CurrentSelectedItem.ID);
                LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry();
                ItemSelect.Function = 0;
            }
            catch { }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (Utility.PromptSave())
            {
                LogicEditing.RecreateLogic(WriteLogicToArray());
            }
        }

        private void btnAddReq_Click(object sender, EventArgs e)
        {
            UnsavedChanges = true;
            SaveState();
            ItemSelect Selector = new ItemSelect();
            ItemSelect.Function = 5;
            Selector.ShowDialog();
            foreach (var i in LogicObjects.selectedItems)
            {
                LBRequired.Items.Add(i);
            }
            LogicObjects.selectedItems = new List<LogicObjects.LogicEntry>();
            ItemSelect.Function = 0;
            updateReqAndCond();
        }

        private void btnRemoveReq_Click(object sender, EventArgs e)
        {
            UnsavedChanges = true;
            SaveState();
            for (int x = LBRequired.SelectedIndices.Count - 1; x >= 0; x--)
            {
                int idx = LBRequired.SelectedIndices[x];
                LBRequired.Items.RemoveAt(idx);
            }
            updateReqAndCond();
        }

        private void btnAddCond_Click(object sender, EventArgs e)
        {
            UnsavedChanges = true;
            SaveState();
            ItemSelect Selector = new ItemSelect();
            ItemSelect.Function = 5;
            Selector.ShowDialog();
            RequiementConditional entry = new RequiementConditional { ItemIDs = new List<LogicObjects.LogicEntry>() };
            string Display = "";
            string addComma = "";
            foreach (var i in LogicObjects.selectedItems)
            {
                Display = Display + addComma + (i.ItemName ?? i.DictionaryName);
                addComma = ", ";
                entry.ItemIDs.Add(i);
            }
            entry.DisplayName = Display;
            LBConditional.Items.Add(entry);
            LogicObjects.selectedItems = new List<LogicObjects.LogicEntry>();
            ItemSelect.Function = 0;
            updateReqAndCond();
        }

        private void btnRemoveCond_Click(object sender, EventArgs e)
        {
            UnsavedChanges = true;
            SaveState();
            for (int x = LBConditional.SelectedIndices.Count - 1; x >= 0; x--)
            {
                int idx = LBConditional.SelectedIndices[x];
                LBConditional.Items.RemoveAt(idx);
            }
            updateReqAndCond();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (!GoBackList.Any()) { return; }
            WriteCurentItem(GoBackList[GoBackList.Count - 1]);
            GoBackList.RemoveAt(GoBackList.Count - 1);
        }

        private void btnNewLogic_Click(object sender, EventArgs e)
        {
            if (!PromptSave()) { return; }
            LogicList = new List<LogicObjects.LogicEntry>();
            GoBackList = new List<int>();
            nudIndex.Value = 0;
            LBRequired.Items.Clear();
            LBConditional.Items.Clear();
            FormatForm();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            Undo();
            WriteCurentItem(currentEntry.ID);
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            Redo();
            WriteCurentItem(currentEntry.ID);
        }

        private void btnNewItem_Click(object sender, EventArgs e)
        {
            UnsavedChanges = true;
            SaveState();
            string name = Interaction.InputBox("Input New Item Name", "New Item", "");
            if (name == "") { return; }
            LogicObjects.LogicEntry newEntry = new LogicObjects.LogicEntry { ID = LogicList.Count, DictionaryName = name, Required = null, Conditionals = null, AvailableOn = 0, NeededBy = 0 };
            LogicList.Add(newEntry);
            nudIndex.Value = (LogicList.Count - 1);
            WriteCurentItem(LogicList.Count - 1);
            FormatForm();
        }

        private void btnEditSelected_Click(object sender, EventArgs e)
        {
            UnsavedChanges = true;
            SaveState();
            for (var n = 0; n < LBConditional.Items.Count; n++)
            {
                if (LBConditional.GetSelected(n))
                {
                    var temp = LBConditional.Items[n] as RequiementConditional;
                    foreach (var j in temp.ItemIDs)
                    {
                        ItemSelect.CheckedItems.Add(j.ID);
                    }
                    ItemSelect Selector = new ItemSelect();
                    ItemSelect.Function = 5;
                    Selector.ShowDialog();
                    RequiementConditional entry = new RequiementConditional { ItemIDs = new List<LogicObjects.LogicEntry>() };
                    string Display = "";
                    string addComma = "";
                    foreach (var i in LogicObjects.selectedItems)
                    {
                        Display = Display + addComma + (i.ItemName ?? i.DictionaryName);
                        addComma = ", ";
                        entry.ItemIDs.Add(i);
                    }
                    entry.DisplayName = Display;
                    if (Display == "")
                    {
                        LBConditional.Items.RemoveAt(n);
                    }
                    else
                    {
                        LBConditional.Items[n] = (entry);
                    }
                    LogicObjects.selectedItems = new List<LogicObjects.LogicEntry>();
                    ItemSelect.Function = 0;
                    LBConditional.Refresh();
                    updateReqAndCond();
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (SaveInstance())
            {
                UnsavedChanges = false;
            }
        }

        private void useLocationItemNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UseDictionaryNameInSearch = !UseDictionaryNameInSearch;
            useLocationItemNamesToolStripMenuItem.Text = (UseDictionaryNameInSearch) ? "Use Location/Item Name" : "Use Dictionary Name";
            WriteCurentItem(currentEntry.ID);
        }

        private void displaySpoilerLogNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UseSpoilerInDisplay = !UseSpoilerInDisplay;
            displaySpoilerLogNamesToolStripMenuItem.Text = (UseSpoilerInDisplay) ? "Use Tracker names" : "Use Spoiler Log names";
            WriteCurentItem(currentEntry.ID);
        }

        //Other

        private void NudIndex_ValueChanged(object sender, EventArgs e)
        {
            if (nudIndex.Value > LogicList.Count - 1) { nudIndex.Value = LogicList.Count - 1; return; }
            if (nudIndex.Value < 0) { nudIndex.Value = 0; return; }
            WriteCurentItem((int)nudIndex.Value);
        }

        private void TimeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UnsavedChanges = true;
            SaveState();
            if (PrintingItem) { return; }
            WriteTimeDependecies(currentEntry);
        }

        private void LBRequired_DoubleClick(object sender, EventArgs e)
        {
            if (LBRequired.SelectedItem is LogicObjects.LogicEntry)
            {
                try
                {
                    var index = (LBRequired.SelectedItem as LogicObjects.LogicEntry).ID;
                    GoBackList.Add(currentEntry.ID);
                    nudIndex.Value = index;
                    WriteCurentItem(index);
                }
                catch { }
            }
        }

        private void LBConditional_DoubleClick(object sender, EventArgs e)
        {
            if (LBConditional.SelectedItem is RequiementConditional)
            {
                try
                {
                    var item = (LBConditional.SelectedItem as RequiementConditional);
                    if (item.ItemIDs.Count < 2)
                    {
                        GoBackList.Add(currentEntry.ID);
                        nudIndex.Value = item.ItemIDs[0].ID;
                        WriteCurentItem(item.ItemIDs[0].ID);
                        return;
                    }
                    ItemSelect.CheckedItems = new List<int>();
                    foreach (var i in item.ItemIDs)
                    {
                        ItemSelect.CheckedItems.Add(i.ID);
                    }
                    ItemSelect Selector = new ItemSelect();
                    ItemSelect.Function = 6;
                    Selector.ShowDialog();
                    var index = LogicObjects.CurrentSelectedItem.ID;
                    GoBackList.Add(currentEntry.ID);
                    nudIndex.Value = index;
                    WriteCurentItem(index);
                    LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry();
                    ItemSelect.Function = 0;
                }
                catch { }
            }
        }

        //Functions

        public void WriteCurentItem(int Index)
        {
            PrintingItem = true;
            LBRequired.Items.Clear();
            LBConditional.Items.Clear();
            var entry = new LogicObjects.LogicEntry();
            try
            {
                entry = LogicList[Index];
            }
            catch
            {
                FormatForm();
                return;
            }
            
            currentEntry = entry;
            foreach (var i in entry.Required ?? new int[0])
            {
                var ReqEntry = LogicList[i];

                ReqEntry.DisplayName = ReqEntry.ItemName ?? ReqEntry.DictionaryName;
                ReqEntry.DisplayName = (LogicEditor.UseSpoilerInDisplay) ? (ReqEntry.SpoilerItem ?? ReqEntry.DisplayName) : ReqEntry.DisplayName;
                ReqEntry.DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? ReqEntry.DictionaryName : ReqEntry.DisplayName;
                LBRequired.Items.Add(ReqEntry);
            }
            foreach (var j in entry.Conditionals ?? new int[0][])
            {
                var CondEntry = new RequiementConditional();
                CondEntry.ItemIDs = new List<LogicObjects.LogicEntry>();
                string Display = "";
                string addComma = "";
                foreach (var i in j ?? new int[0])
                {
                    var ReqEntry = LogicList[i]; 

                    string disName = ReqEntry.ItemName ?? ReqEntry.DictionaryName;
                    disName = (LogicEditor.UseSpoilerInDisplay) ? (ReqEntry.SpoilerItem ?? disName) : disName;
                    disName = (LogicEditor.UseDictionaryNameInSearch) ? ReqEntry.DictionaryName : disName;

                    Display = Display + addComma + disName;
                    addComma = ", ";
                    CondEntry.ItemIDs.Add(ReqEntry);

                }
                CondEntry.DisplayName = Display;
                LBConditional.Items.Add(CondEntry);
            }
            string DictionaryName = entry.DictionaryName.ToString();
            string LocationName = entry.LocationName ?? "Fake Location";
            string ItemName = entry.ItemName ?? "Fake Item";

            LocationName = (UseSpoilerInDisplay) ? (entry.SpoilerLocation ?? LocationName) : LocationName;
            ItemName = (UseSpoilerInDisplay) ? (entry.SpoilerItem ?? ItemName) : ItemName;

            lblDicName.Text = DictionaryName;
            lblLocName.Text = LocationName.ToString();
            lblItemName.Text = ItemName.ToString();

            chkOnDay1.Checked = (((entry.AvailableOn >> 0) & 1) == 1);
            chkOnDay2.Checked = (((entry.AvailableOn >> 2) & 1) == 1);
            chkOnDay3.Checked = (((entry.AvailableOn >> 4) & 1) == 1);
            chkOnNight1.Checked = (((entry.AvailableOn >> 1) & 1) == 1);
            chkOnNight2.Checked = (((entry.AvailableOn >> 3) & 1) == 1);
            chkOnNight3.Checked = (((entry.AvailableOn >> 5) & 1) == 1);
            chkNeedDay1.Checked = (((entry.NeededBy >> 0) & 1) == 1);
            chkNeedDay2.Checked = (((entry.NeededBy >> 2) & 1) == 1);
            chkNeedDay3.Checked = (((entry.NeededBy >> 4) & 1) == 1);
            chkNeedNight1.Checked = (((entry.NeededBy >> 1) & 1) == 1);
            chkNeedNight2.Checked = (((entry.NeededBy >> 3) & 1) == 1);
            chkNeedNight3.Checked = (((entry.NeededBy >> 5) & 1) == 1);
            PrintingItem = false;
        }

        public void WriteTimeDependecies(LogicObjects.LogicEntry entry)
        {
            entry.AvailableOn = 0;
            if (chkOnDay1.Checked) { entry.AvailableOn += 1; };
            if (chkOnNight1.Checked) { entry.AvailableOn += 2; };
            if (chkOnDay2.Checked) { entry.AvailableOn += 4; };
            if (chkOnNight2.Checked) { entry.AvailableOn += 8; };
            if (chkOnDay3.Checked) { entry.AvailableOn += 16; };
            if (chkOnNight3.Checked) { entry.AvailableOn += 32; };
            if (chkNeedDay1.Checked) { entry.NeededBy += 1; };
            if (chkNeedNight1.Checked) { entry.NeededBy += 2; };
            if (chkNeedDay2.Checked) { entry.NeededBy += 4; };
            if (chkNeedNight2.Checked) { entry.NeededBy += 8; };
            if (chkNeedDay3.Checked) { entry.NeededBy += 16; };
            if (chkNeedNight3.Checked) { entry.NeededBy += 32; };
        }

        public void updateReqAndCond()
        {
            List<int> req = new List<int>();
            foreach (var i in LBRequired.Items) { req.Add((i as LogicObjects.LogicEntry).ID); }
            currentEntry.Required = req.ToArray();

            List<int[]> cond = new List<int[]>();
            foreach (var i in LBConditional.Items)
            {
                req = new List<int>();
                var item = i as RequiementConditional;
                foreach (var j in item.ItemIDs) { req.Add(j.ID); }
                cond.Add(req.ToArray());
            }
            currentEntry.Conditionals = cond.ToArray();
        }

        private string[] WriteLogicToArray()
        {
            List<string> lines = new List<string>();
            lines.Add((isOOT) ? "-versionOOT " + versionNumber : "-version " + versionNumber);
            foreach (var line in LogicList)
            {
                lines.Add("- " + line.DictionaryName);
                string Req = "";
                string Comma = "";
                foreach (var i in line.Required ?? new int[0])
                {
                    Req = Req + Comma + i.ToString();
                    Comma = ",";
                }
                lines.Add(Req);
                string cond = "";
                string colon = "";
                foreach (var j in line.Conditionals ?? new int[0][])
                {
                    Req = "";
                    Comma = "";
                    foreach (var i in j ?? new int[0])
                    {
                        Req = Req + Comma + i.ToString();
                        Comma = ",";
                    }
                    cond = cond + colon + Req;
                    colon = ";";
                }
                lines.Add(cond);
                lines.Add(line.NeededBy.ToString());
                lines.Add(line.AvailableOn.ToString());
            }
            return lines.ToArray();
        }

        public bool PromptSave(bool OnlyIfUnsaved = true)
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

        public bool SaveInstance()
        {
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Logic File (*.txt)|*.txt", FilterIndex = 1 };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return false; }
            var logicText = WriteLogicToArray().ToList();
            StreamWriter LogicFile = new StreamWriter(File.Open(saveDialog.FileName, FileMode.Create));
            for (int i = 0; i < logicText.Count; i++)
            {
                if (i == logicText.Count - 1) { LogicFile.Write(logicText[i]); break; }
                LogicFile.WriteLine(logicText[i]);
            }
            LogicFile.Close();
            UnsavedChanges = false;
            return true;
        }

        //Static Functions

        public static void ReadLogicFile(string[] LogicFile)
        {
            int SubCounter = 0;
            int idCounter = 0;
            var VersionData = new string[2];
            LogicObjects.LogicEntry LogicEntry1 = new LogicObjects.LogicEntry();
            foreach (string line in LogicFile)
            {
                if (line.StartsWith("-")) { SubCounter = 0; }
                if (line.Contains("-version"))
                {
                    string curLine = line;
                    GetOOTDictionary = false;
                    isOOT = false;
                    if (line.Contains("-versionOOT"))
                    {
                        GetOOTDictionary = true;
                        isOOT = true;
                        curLine = line.Replace("versionOOT", "version");
                    }
                    versionNumber = Int32.Parse(curLine.Replace("-version ", ""));
                    Console.WriteLine(versionNumber);
                    VersionData = VersionHandeling.SwitchDictionary(versionNumber);
                    GetOOTDictionary = false;
                    EditorDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDic>>(Utility.ConvertCsvFileToJsonObject(VersionData[0]));
                }
                switch (SubCounter)
                {
                    case 0:
                        LogicEntry1.ID = idCounter;
                        LogicEntry1.DictionaryName = line.Substring(2);
                        LogicEntry1.Checked = false;
                        LogicEntry1.RandomizedItem = -2;
                        LogicEntry1.IsFake = true;
                        for (int i = 0; i < EditorDictionary.Count; i++)
                        {
                            if (EditorDictionary[i].DictionaryName == line.Substring(2))
                            {
                                LogicEntry1.IsFake = false;
                                var dicent = EditorDictionary[i];
                                LogicEntry1.ItemName = (dicent.ItemName == "") ? null : dicent.ItemName;
                                LogicEntry1.LocationName = (dicent.LocationName == "") ? null : dicent.LocationName;
                                break;
                            }
                        }
                        break;
                    case 1:
                        if (line == null || line == "") { LogicEntry1.Required = null; break; }
                        string[] req = line.Split(',');
                        LogicEntry1.Required = Array.ConvertAll(req, s => int.Parse(s));
                        break;
                    case 2:
                        if (line == null || line == "") { LogicEntry1.Conditionals = null; break; }
                        string[] ConditionalSets = line.Split(';');
                        int[][] Conditionals = new int[ConditionalSets.Length][];
                        for (int j = 0; j < ConditionalSets.Length; j++)
                        {
                            string[] condtional = ConditionalSets[j].Split(',');
                            Conditionals[j] = Array.ConvertAll(condtional, s => int.Parse(s));
                        }
                        LogicEntry1.Conditionals = Conditionals;
                        break;
                    case 3:
                        LogicEntry1.NeededBy = (line == "") ? 0 : Convert.ToInt32(line);
                        break;
                    case 4:
                        LogicEntry1.AvailableOn = (line == "") ? 0 : Convert.ToInt32(line);
                        LogicList.Add(LogicEntry1);
                        LogicEntry1 = new LogicObjects.LogicEntry();
                        idCounter++;
                        break;
                }
                SubCounter++;
            }
        }

        public static void AssignUniqueItemnames(List<LogicObjects.LogicEntry> Logic)
        {
            List<string> usedLocationNames = new List<string>();
            List<string> usedSpoilerNames = new List<string>();
            foreach (var LogicEntry1 in Logic)
            {
                if (LogicEntry1.ItemName == null) { continue; }
                int number = 1;
                string originalName = LogicEntry1.ItemName;
                while (usedLocationNames.Contains(LogicEntry1.ItemName))
                {
                    LogicEntry1.ItemName = originalName + " (" + number.ToString() + ")";
                    number += 1;
                }
                usedLocationNames.Add(LogicEntry1.ItemName);

                if (LogicEntry1.SpoilerItem != null) 
                {
                    originalName = LogicEntry1.SpoilerItem;
                    while (usedSpoilerNames.Contains(LogicEntry1.SpoilerItem))
                    {
                        LogicEntry1.SpoilerItem = originalName + " (" + number.ToString() + ")";
                        number += 1;
                    }
                    usedSpoilerNames.Add(LogicEntry1.SpoilerItem);
                }
            }
        }

        public static void Undo()
        {
            if (UndoList.Any())
            {
                UnsavedChanges = true;
                var lastItem = UndoList.Count - 1;
                RedoList.Add(Utility.CloneLogicList(LogicList));
                LogicList = Utility.CloneLogicList(UndoList[lastItem]);
                UndoList.RemoveAt(lastItem);
            }
        }

        public static void Redo()
        {
            if (RedoList.Any())
            {
                UnsavedChanges = true;
                var lastItem = RedoList.Count - 1;
                UndoList.Add(Utility.CloneLogicList(LogicList));
                LogicList = Utility.CloneLogicList(RedoList[lastItem]);
                RedoList.RemoveAt(lastItem);
            }
        }

        public static void SaveState()
        {
            UndoList.Add(Utility.CloneLogicList(LogicList));
            RedoList = new List<List<LogicObjects.LogicEntry>>();
        }
    }
}
