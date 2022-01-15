using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using MMR_Tracker.Class_Files;
using System.Drawing;
using MMR_Tracker.Forms.Sub_Forms;

namespace MMR_Tracker.Forms
{
    public partial class LogicEditor : Form
    {
        public LogicEditor()
        {
            InitializeComponent();
        }

        //Main Lists
        public static LogicObjects.TrackerInstance EditorInstance = new LogicObjects.TrackerInstance();

        public static LogicObjects.UndoRedoData EditorInstanceUndoRedoData = new LogicObjects.UndoRedoData();

        //Utility Lists
        public static List<LogicObjects.LogicEntry> CopiedRequirement = new List<LogicObjects.LogicEntry>();
        public static List<RequiementConditional> CopiedConditional = new List<RequiementConditional>();

        //Entry management
        public static List<int> GoBackList = new List<int>();
        public static LogicObjects.LogicEntry currentEntry = new LogicObjects.LogicEntry();
        public static ListBox LastSelectedListBox;

        //Other Variables
        public static bool PrintingItem = false;
        public static bool UseDictionaryNameInSearch = false;
        public static bool AddCondSeperatly = false;
        public static bool NudUpdateing = false;
        public static bool CreatingNewItem = false;
        public static int MaxEntries = 10000;

        public static LogicEditor EditorForm = null;

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
            if (LogicObjects.MainTrackerInstance.LogicVersion > 0)
            {
                EditorInstance = Utility.CloneTrackerInstance(LogicObjects.MainTrackerInstance);
                FormatForm();
            }
            else
            {
                EditorInstance = new LogicObjects.TrackerInstance();
                FormatForm();
            }
            useLocationItemNamesToolStripMenuItem.Checked = (!UseDictionaryNameInSearch);
            CreateContextMenus();
        }

        public void FormatForm(int StartAt = 0)
        {
            this.Text = "Logic Editor";
            bool enabled = (EditorInstance.Logic.Count > 0);
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
            chkSetDay1.Enabled = enabled;
            chkSetDay2.Enabled = enabled;
            chkSetDay3.Enabled = enabled;
            chkSetNight1.Enabled = enabled;
            chkSetNight2.Enabled = enabled;
            chkSetNight3.Enabled = enabled;
            groupBox1.Enabled = enabled;
            groupBox2.Enabled = enabled;
            groupBox3.Enabled = enabled;
            chkIsTrick.Enabled = enabled;
            btnEditSelected.Enabled = enabled;
            btnUp.Enabled = enabled;
            btnDown.Enabled = enabled;

            undoToolStripMenuItem.Visible = enabled;
            redoToolStripMenuItem.Visible = enabled;
            renameCurrentItemToolStripMenuItem.Visible = enabled;
            reorderLogicToolStripMenuItem.Visible = enabled;
            whatIsThisUsedInToolStripMenuItem.Visible = enabled;
            lblTrickToolTip.Visible = enabled;
            lblCategory.Visible = enabled;
            deleteCurrentItemToolStripMenuItem.Visible = enabled;
            setTrickToolTipToolStripMenuItem.Visible = enabled;
            setTrickCategoryToolStripMenuItem.Visible = enabled;
            cleanLogicEntryToolStripMenuItem.Visible = enabled;
            showAllFakeToolStripMenuItem.Visible = enabled;
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
            else { EditorForm = null; }
        }

        //Button

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            LoadLogic();
        }

        public void LoadLogic(string[] Lines = null)
        {
            if (!PromptSave()) { return; }
            if (Lines == null)
            {
                var file = Utility.FileSelect("Select A Logic File", "Logic File (*.txt;*.MMRTSET;*.json)|*.txt;*.MMRTSET;*.json");
                if (file == "") { return; }
                bool SettingsFile = file.EndsWith(".MMRTSET");
                Lines = (SettingsFile) ? File.ReadAllLines(file).Skip(2).ToArray() : File.ReadAllLines(file).ToArray();
            }

            if (Tools.TestForTextSpoiler(Lines))
            {
                LogicObjects.GameplaySettings SettingFile = null;
                foreach (var line in Lines)
                {
                    if (line.StartsWith("Settings:"))
                    {
                        var Newline = line.Replace("Settings:", "\"GameplaySettings\":");
                        Newline = "{" + Newline + "}";
                        try { SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(Newline).GameplaySettings; }
                        catch
                        {
                            SettingFile = Tools.ParseSpoilerLogSettingsWithLineBreak(Lines, text: true);
                            if (SettingFile == null)
                            {
                                MessageBox.Show("File could nto be read. Please select a logic file");
                                return;
                            }
                        }
                        break;
                    }
                }
                if (SettingFile == null)
                {
                    MessageBox.Show("File could nto be read. Please select a logic file");
                    return;
                }
                else
                {
                    Lines = Tools.GetLogicFileFromSettings(SettingFile);
                    if (Lines == null) { return; }
                }
            }

            GoBackList = new List<int>();
            EditorInstance = new LogicObjects.TrackerInstance();
            EditorInstance.RawLogicFile = Lines;
            LogicEditing.PopulateTrackerInstance(EditorInstance);

            if (EditorInstance.Logic.Count < Convert.ToInt32(nudIndex.Value)) { nudIndex.Value = EditorInstance.Logic.Count - 1; }
            FormatForm(Convert.ToInt32(nudIndex.Value));
        }

        private void BtnGoTo_Click(object sender, EventArgs e)
        {
            MiscSingleItemSelect Selector = new MiscSingleItemSelect
            {
                Text = "Go to item",
                ListContent = LogicEditor.EditorInstance.Logic,
                Display = 6
            };
            Selector.ShowDialog();
            if (Selector.DialogResult != DialogResult.OK) { return; }
            //GoBackList.Add(currentEntry.ID);
            try
            {
                nudIndex.Value = Selector.SelectedObject.ID;
                WriteCurentItem(Selector.SelectedObject.ID);
            }
            catch { }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (Tools.PromptSave(LogicObjects.MainTrackerInstance))
            {
                if (EditorInstance.LogicFormat == "json")
                {
                    LogicEditing.RecreateLogic(LogicObjects.MainTrackerInstance, LogicEditing.WriteLogicToJson(EditorInstance));
                }
                else if (EditorInstance.LogicFormat == "txt" || EditorInstance.LogicFormat == "entrance")
                {
                    LogicEditing.RecreateLogic(LogicObjects.MainTrackerInstance, LogicEditing.WriteLogicToArray(EditorInstance));
                }
                else
                {
                    MessageBox.Show("Logic type was incorrect or not supported");
                    return;
                }
            }
            MainInterface.CurrentProgram.PrintToListBox();
            MainInterface.CurrentProgram.ResizeObject();
            MainInterface.CurrentProgram.FormatMenuItems();
        }

        private void BtnAddReq_Click(object sender, EventArgs e)
        {
            EditorInstance.UnsavedChanges = true;
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            MiscMultiItemSelect NeededSelect = new MiscMultiItemSelect
            {
                UsedInstance = EditorInstance,
                Display = 7,
                ListContent = EditorInstance.Logic
            };
            if (NeededSelect.ShowDialog() != DialogResult.OK) { return; }
            foreach (var i in NeededSelect.SelectedItems)
            {
                if (LBRequired.Items.Contains(i)) { continue; }
                i.DisplayName = i.ItemName ?? i.DictionaryName;
                i.DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? i.DictionaryName : i.DisplayName;
                LBRequired.Items.Add(i);
            }
            UpdateReqAndCond();
            WriteCurentItem((int)nudIndex.Value);
        }

        public void ContextMenuAddPermutations(object sender, EventArgs e)
        {
            LogicEditorAddPermutations Selector = new LogicEditorAddPermutations();
            Selector.UsedInstance = EditorInstance;
            Selector.Display = 2;
            Selector.ListContent = EditorInstance.Logic;
            Selector.ShowDialog();
            if (Selector.DialogResult != DialogResult.OK) {  return; }
            if (Selector.SelectedItems.Count < 1) {  return; }

            Console.WriteLine($"{(int)Selector.numericUpDown1.Value} OF {Selector.SelectedItems.Count} Items");

            var UniqueCombinations = Utility.CountUniqueCombinations(Selector.SelectedItems.Count(), (int)Selector.numericUpDown1.Value);

            if (UniqueCombinations > MaxEntries || (int)Selector.numericUpDown1.Value > Selector.SelectedItems.Count || UniqueCombinations < MaxEntries*-1)
            {
                MessageBox.Show($"{UniqueCombinations} Entries would be created with your selected parameters. This is greater than the max number of entries ({MaxEntries}) this process can easily handle, the entry will not be created.", "To many combinations exist!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EditorInstance.UnsavedChanges = true;
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);

            Debugging.Log($"Begin create {UniqueCombinations} permutatios");
            try
            {
                var NewConditionals = CreatePermiations(Selector.SelectedItems.Select(x=>x.ID).ToArray(), (int)Selector.numericUpDown1.Value);

                Debugging.Log("Finish create permutatios");
                if (currentEntry.Conditionals == null)
                {
                    currentEntry.Conditionals = NewConditionals;
                }
                else
                {
                    currentEntry.Conditionals = currentEntry.Conditionals.Concat(NewConditionals).ToArray();
                }

                Debugging.Log("Finish add to entry");
            }
            catch { }

            WriteCurentItem((int)nudIndex.Value);
            Debugging.Log("Finish Write Items");
        }

        private void BtnRemoveReq_Click(object sender, EventArgs e)
        {
            EditorInstance.UnsavedChanges = true;
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            LBRequired.BeginUpdate();
            for (int x = LBRequired.SelectedIndices.Count - 1; x >= 0; x--)
            {
                int idx = LBRequired.SelectedIndices[x];
                LBRequired.Items.RemoveAt(idx);
            }
            LBRequired.EndUpdate();
            UpdateReqAndCond();
        }

        private void BtnAddCond_Click(object sender, EventArgs e)
        {
            LogicEditorConditional ConditionalSelect = new LogicEditorConditional
            {
                Text = "Create Logic Conditional",
                UsedInstance = EditorInstance,
                Display = 7,
                ListContent = EditorInstance.Logic
            };
            ConditionalSelect.ShowDialog();
        }

        private void BtnRemoveCond_Click(object sender, EventArgs e)
        {
            EditorInstance.UnsavedChanges = true;
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            LBConditional.BeginUpdate();
            for (int x = LBConditional.SelectedIndices.Count - 1; x >= 0; x--)
            {
                if (!(LBConditional.Items[x] is RequiementConditional)) { continue; }
                int idx = LBConditional.SelectedIndices[x];
                LBConditional.Items.RemoveAt(idx);
            }
            LBConditional.EndUpdate();
            UpdateReqAndCond();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (!GoBackList.Any()) { return; }
            NudUpdateing = true;
            nudIndex.Value = (GoBackList[GoBackList.Count - 1]);
            WriteCurentItem(GoBackList[GoBackList.Count - 1]);
            GoBackList.RemoveAt(GoBackList.Count - 1);
            NudUpdateing = false;
        }

        public void BtnNewLogic_Click(object sender, EventArgs e)
        {
            if (!PromptSave()) { return; }
            EditorInstance = new LogicObjects.TrackerInstance();
            GoBackList = new List<int>();
            nudIndex.Value = 0;
            LBRequired.Items.Clear();
            LBConditional.Items.Clear();
            FormatForm();
        }

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            Tools.Undo(EditorInstance, EditorInstanceUndoRedoData);
            WriteCurentItem(currentEntry.ID);
        }

        private void BtnRedo_Click(object sender, EventArgs e)
        {
            Tools.Redo(EditorInstance, EditorInstanceUndoRedoData);
            WriteCurentItem(currentEntry.ID);
        }

        private void BtnNewItem_Click(object sender, EventArgs e)
        {
            EditorInstance.UnsavedChanges = true;
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            string name = Interaction.InputBox("Input New Item Name", "New Item", "");
            if (name == "") { return; }
            CreatingNewItem = true;
            GoBackList.Add(currentEntry.ID);
            LogicObjects.LogicEntry newEntry = new LogicObjects.LogicEntry { ID = EditorInstance.Logic.Count, DictionaryName = name, IsFake = true, Required = null, Conditionals = null, AvailableOn = 0, NeededBy = 0 };
            EditorInstance.Logic.Add(newEntry);
            FormatForm(EditorInstance.Logic.Count - 1);
            //nudIndex.Value = (EditorInstance.Logic.Count - 1);
            //WriteCurentItem(EditorInstance.Logic.Count - 1);
            CreatingNewItem = false;
        }

        private void BtnEditSelected_Click(object sender, EventArgs e)
        {
            EditorInstance.UnsavedChanges = true;
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            for (var n = 0; n < LBConditional.Items.Count; n++)
            {
                if (LBConditional.GetSelected(n))
                {
                    var temp = LBConditional.Items[n] as RequiementConditional;
                    MiscMultiItemSelect Selector = new MiscMultiItemSelect
                    {
                        Text = "Edit items in this conditional",
                        UsedInstance = EditorInstance,
                        Display = 7,
                        ListContent = EditorInstance.Logic,
                        CheckedItems = temp.ItemIDs.Select(x => x.ID).ToList()
                    };
                    if (Selector.ShowDialog() != DialogResult.OK) { return; }
                    RequiementConditional entry = new RequiementConditional { ItemIDs = new List<LogicObjects.LogicEntry>() };
                    string Display = "";
                    string addComma = "";
                    foreach (var i in Selector.SelectedItems)
                    {
                        string ItemName = i.ItemName ?? i.DictionaryName;
                        ItemName = (LogicEditor.UseDictionaryNameInSearch) ? i.DictionaryName : ItemName;
                        Display = Display + addComma + ItemName;
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
                    LBConditional.Refresh();
                    UpdateReqAndCond();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            this.fileToolStripMenuItem.HideDropDown();
            SaveInstance();
        }

        private void UseLocationItemNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UseDictionaryNameInSearch = !UseDictionaryNameInSearch;
            useLocationItemNamesToolStripMenuItem.Checked = !UseDictionaryNameInSearch;
            WriteCurentItem(currentEntry.ID);
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            MoveItem(-1);
        }

        private void BtnDown_Click(object sender, EventArgs e)
        {
            MoveItem(1);
        }

        private void ReorderLogicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicEditorReorder Selector = new LogicEditorReorder();
            Selector.ListContent = Utility.CloneLogicList(EditorInstance.Logic);
            Selector.ShowDialog();
            if (Selector.DialogResult != DialogResult.OK) { return; }
            EditorInstance.UnsavedChanges = true;
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            int counter = 0;
            Dictionary<int, int> newOrder = new Dictionary<int, int>();
            foreach (var i in Selector.SelectedItems)
            {
                if (i.ID != counter)
                {
                    newOrder.Add(i.ID, counter);
                }
                counter++;
            }
            foreach (var i in Selector.SelectedItems)
            {
                if (newOrder.ContainsKey(i.ID))
                {
                    Debugging.Log("Item ID " + i.ID + " Became " + newOrder[i.ID]);
                    i.ID = newOrder[i.ID];
                }
                if (i.Required != null)
                {
                    for (var j = 0; j < i.Required.Length; j++)
                    {
                        if (newOrder.ContainsKey(i.Required[j]))
                        {
                            Debugging.Log("Requirment " + i.Required[j] + " Became " + newOrder[i.Required[j]]);
                            i.Required[j] = newOrder[i.Required[j]];
                        }
                    }
                }
                if (i.Conditionals != null)
                {
                    for (var j = 0; j < i.Conditionals.Length; j++)
                    {
                        for (var k = 0; k < i.Conditionals[j].Length; k++)
                        {
                            if (newOrder.ContainsKey(i.Conditionals[j][k]))
                            {
                                Debugging.Log("Conditional " + i.Conditionals[j][k] + " Became " + newOrder[i.Conditionals[j][k]]);
                                i.Conditionals[j][k] = newOrder[i.Conditionals[j][k]];
                            }
                        }
                    }
                }
            }
            EditorInstance.Logic = Utility.CloneLogicList(Selector.SelectedItems);
            WriteCurentItem(currentEntry.ID);
        }

        private void RenameCurrentItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!currentEntry.IsFake) { MessageBox.Show("Only fake Items Can be Renamed"); return; }
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            string name = Interaction.InputBox("Input New Item Name", "New Item", currentEntry.DictionaryName);
            if (name == "") { return; }
            EditorInstance.UnsavedChanges = true;
            currentEntry.DictionaryName = name;
            WriteCurentItem(currentEntry.ID);
        }

        private void setTrickToolTipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string CurTT = (currentEntry.TrickToolTip == "No Tooltip Available") ? "" : currentEntry.TrickToolTip;
            var text = (CurTT == "") ? "No tooltip set, enter tooltip below." : "Current Tooltip: \n\"" + CurTT + "\"\nEnter a new tooltip below to change it.";
            string name = Interaction.InputBox(text, $"{currentEntry.DictionaryName} Trick Tooltip", CurTT);
            if (name != "") { currentEntry.TrickToolTip = name; }
        }

        private void deleteCurrentItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<LogicObjects.LogicEntry> Inuse = new List<LogicObjects.LogicEntry>();
            foreach (var i in EditorInstance.Logic)
            {
                if (i.Required != null)
                {
                    for (var j = 0; j < i.Required.Length; j++)
                    {
                        if (i.Required[j] == currentEntry.ID) { Inuse.Add(i); }
                    }
                }
                if (i.Conditionals != null)
                {
                    for (var j = 0; j < i.Conditionals.Length; j++)
                    {
                        for (var k = 0; k < i.Conditionals[j].Length; k++)
                        {
                            if (i.Conditionals[j][k] == currentEntry.ID && !Array.Exists(Inuse.ToArray(), x => x.ID == i.ID)) { Inuse.Add(i); }
                        }
                    }
                }
            }

            if (Inuse.Count() > 0)
            {
                string Usage = $"Unable to delete {currentEntry.DictionaryName} Because it is a requirement or conditional in the following entries:\n\n";
                foreach (var i in Inuse) { Usage = Usage + $"{i.DictionaryName}\n"; }
                MessageBox.Show(Usage, "Item In Use", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var Confirm = MessageBox.Show($"Are you sure you wish to delete the {currentEntry.DictionaryName} entry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (Confirm == DialogResult.No) { return; }

            EditorInstance.Logic.RemoveAt(currentEntry.ID);
            int counter = 0;
            Dictionary<int, int> newOrder = new Dictionary<int, int>();
            foreach (var i in EditorInstance.Logic)
            {
                if (i.ID != counter)
                {
                    newOrder.Add(i.ID, counter);
                }
                counter++;
            }
            foreach (var i in EditorInstance.Logic)
            {
                if (newOrder.ContainsKey(i.ID))
                {
                    Debugging.Log("Item ID " + i.ID + " Became " + newOrder[i.ID]);
                    i.ID = newOrder[i.ID];
                }
                if (i.Required != null)
                {
                    for (var j = 0; j < i.Required.Length; j++)
                    {
                        if (newOrder.ContainsKey(i.Required[j]))
                        {
                            Debugging.Log("Requirment " + i.Required[j] + " Became " + newOrder[i.Required[j]]);
                            i.Required[j] = newOrder[i.Required[j]];
                        }
                    }
                }
                if (i.Conditionals != null)
                {
                    for (var j = 0; j < i.Conditionals.Length; j++)
                    {
                        for (var k = 0; k < i.Conditionals[j].Length; k++)
                        {
                            if (newOrder.ContainsKey(i.Conditionals[j][k]))
                            {
                                Debugging.Log("Conditional " + i.Conditionals[j][k] + " Became " + newOrder[i.Conditionals[j][k]]);
                                i.Conditionals[j][k] = newOrder[i.Conditionals[j][k]];
                            }
                        }
                    }
                }
            }
            WriteCurentItem((currentEntry.ID >= EditorInstance.Logic.Count) ? EditorInstance.Logic.Count - 1 : currentEntry.ID);
        }

        private void saveLogicWothoutTrickDataLegacyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.fileToolStripMenuItem.HideDropDown();
            SaveInstance(false);
        }

        private void whatIsThisUsedInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<LogicObjects.LogicEntry> Inuse = new List<LogicObjects.LogicEntry>();
            foreach (var i in EditorInstance.Logic)
            {
                if (i.Required != null)
                {
                    for (var j = 0; j < i.Required.Length; j++)
                    {
                        if (i.Required[j] == currentEntry.ID) { Inuse.Add(i); }
                    }
                }
                if (i.Conditionals != null)
                {
                    for (var j = 0; j < i.Conditionals.Length; j++)
                    {
                        for (var k = 0; k < i.Conditionals[j].Length; k++)
                        {
                            if (i.Conditionals[j][k] == currentEntry.ID && !Array.Exists(Inuse.ToArray(), x => x.ID == i.ID)) { Inuse.Add(i); }
                        }
                    }
                }
            }

            if (Inuse.Count() > 0)
            {
                try
                {
                    MiscSingleItemSelect Selector = new MiscSingleItemSelect
                    {
                        ListContent = Inuse,
                        Display = 6,
                        Text = $"{currentEntry.DictionaryName} Is used in the following entries:\n\n"
                    };
                    Selector.ShowDialog();
                    if (Selector.DialogResult != DialogResult.OK) { return; }
                    var index = Selector.SelectedObject.ID;
                    //GoBackList.Add(currentEntry.ID);
                    nudIndex.Value = index;
                    WriteCurentItem(index);
                    return;
                }
                catch { }
            }
            MessageBox.Show($"{currentEntry.DictionaryName} Is not used in any entries", "No entries found", MessageBoxButtons.OK);
        }

        private void clearLogicDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearLogicData();
        }

        private void cleanLogicEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cleanLogicEntryToolStripMenuItem.HideDropDown();
            CleanLogicEntry(currentEntry, EditorInstance);
            WriteCurentItem(currentEntry.ID);
        }

        private void extractRequiredItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            CleanLogicEntry(currentEntry, EditorInstance, false);
            WriteCurentItem(currentEntry.ID);
        }

        private void removeRedundantConditionalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            CleanLogicEntry(currentEntry, EditorInstance, true, false);
            WriteCurentItem(currentEntry.ID);
        }

        private void showAllFakeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<LogicObjects.LogicEntry> Fake = new List<LogicObjects.LogicEntry>();
            foreach (var i in EditorInstance.Logic)
            {
                if (i.IsFake) { Fake.Add(i); }
            }

            if (Fake.Count() > 0)
            {
                try
                {
                    MiscSingleItemSelect Selector = new MiscSingleItemSelect
                    {
                        ListContent = Fake,
                        Display = 6,
                        Text = $"Fake Items:\n\n"
                    };
                    Selector.ShowDialog();
                    if (Selector.DialogResult != DialogResult.OK) { return; }
                    var index = Selector.SelectedObject.ID;
                    //GoBackList.Add(currentEntry.ID);
                    nudIndex.Value = index;
                    WriteCurentItem(index);
                    return;
                }
                catch { }
            }
            MessageBox.Show($"No fake items found", "No entries found", MessageBoxButtons.OK);
        }

        private void setTrickCategoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string CurTT = (currentEntry.TrickCategory == "No Category Available") ? "" : currentEntry.TrickCategory;
            var text = (CurTT == "") ? "No Category set, enter Category below." : "Current Category: \n\"" + CurTT + "\"\nEnter a new Category below to change it.";
            string name = Interaction.InputBox(text, $"{currentEntry.DictionaryName} Trick Category", CurTT);
            if (name != "") { currentEntry.TrickCategory = name; }
        }

        //Other

        private void NudIndex_ValueChanged(object sender, EventArgs e)
        {
            if (nudIndex.Value > EditorInstance.Logic.Count - 1) 
            {
                try { nudIndex.Value = EditorInstance.Logic.Count - 1; }
                catch { nudIndex.Value = 0; }
                return; 
            }
            if (nudIndex.Value < 0) { nudIndex.Value = 0; return; }
            if (currentEntry.ID + 1 != (int)nudIndex.Value && currentEntry.ID - 1 != (int)nudIndex.Value && !NudUpdateing && currentEntry.ID != (int)nudIndex.Value)  
            {
                Debugging.Log("Value Manually Entered");
                GoBackList.Add(currentEntry.ID); 
            }

            WriteCurentItem((int)nudIndex.Value);
        }

        private void TimeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (PrintingItem) { return; }
            EditorInstance.UnsavedChanges = true;
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            WriteTimeDependecies(currentEntry);
        }

        private void LBRequired_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (LBRequired.SelectedItem is LogicObjects.LogicEntry)
                {
                    var index = (LBRequired.SelectedItem as LogicObjects.LogicEntry).ID;
                    //GoBackList.Add(currentEntry.ID);
                    nudIndex.Value = index;
                    WriteCurentItem(index);
                }
            }
            catch { }
        }

        private void LBConditional_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (LBConditional.SelectedItem is RequiementConditional)
                {
                    var item = (LBConditional.SelectedItem as RequiementConditional);
                    if (item.ItemIDs.Count < 2)
                    {
                        //GoBackList.Add(currentEntry.ID);
                        nudIndex.Value = item.ItemIDs[0].ID;
                        WriteCurentItem(item.ItemIDs[0].ID);
                        return;
                    }
                    MiscSingleItemSelect Selector = new MiscSingleItemSelect
                    {
                        Text = "Go to item",
                        Display = 6,
                        ListContent = item.ItemIDs
                    };
                    Selector.ShowDialog();
                    if (Selector.DialogResult != DialogResult.OK) { return; }
                    var index = Selector.SelectedObject.ID;
                    //GoBackList.Add(currentEntry.ID);
                    nudIndex.Value = index;
                    WriteCurentItem(index);
                }
            }
            catch { }
        }

        private void LBConditional_SelectedIndexChanged(object sender, EventArgs e)
        {
            LastSelectedListBox = LBConditional;
        }

        private void LBRequired_SelectedIndexChanged(object sender, EventArgs e)
        {
            LastSelectedListBox = LBRequired;
        }

        private void LBConditional_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && Control.ModifierKeys == Keys.Control)
            {
                if (LBConditional.SelectedItems.Count < 1) { return; }
                CopiedConditional = new List<RequiementConditional>();
                foreach (RequiementConditional i in LBConditional.SelectedItems)
                {
                    CopiedConditional.Add(i);
                }
                Clipboard.SetText(JsonConvert.SerializeObject(CopiedConditional));
            }

            if (e.KeyCode == Keys.V && Control.ModifierKeys == Keys.Control)
            {
                if (CopiedConditional.Count < 1) { return; }
                EditorInstance.UnsavedChanges = true;
                foreach (RequiementConditional i in CopiedConditional)
                {
                    LBConditional.Items.Add(i);
                }
                UpdateReqAndCond();
            }
        }

        private void LBRequired_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && Control.ModifierKeys == Keys.Control)
            {
                if (LBRequired.SelectedItems.Count < 1) { return; }
                CopiedRequirement = new List<LogicObjects.LogicEntry>();
                foreach (LogicObjects.LogicEntry i in LBRequired.SelectedItems)
                {
                    CopiedRequirement.Add(i);
                }
                Clipboard.SetText(JsonConvert.SerializeObject(CopiedRequirement));
            }

            if (e.KeyCode == Keys.V && Control.ModifierKeys == Keys.Control)
            {
                if (CopiedRequirement.Count < 1) { return; }
                EditorInstance.UnsavedChanges = true;
                foreach (LogicObjects.LogicEntry i in CopiedRequirement)
                {
                    LBRequired.Items.Add(i);
                }
                UpdateReqAndCond();
            }
        }

        private void preventKeyShortcuts(object sender, KeyPressEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control) { e.Handled = true; }
        }

        private void LogicEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.PageUp && nudIndex.Value + 1 < EditorInstance.Logic.Count()) { nudIndex.Value += 1; }
            if (e.KeyCode == Keys.PageDown && nudIndex.Value > 0) { nudIndex.Value -= 1; }
        }

        private void chkIsTrick_CheckedChanged(object sender, EventArgs e)
        {
            if (PrintingItem) { return; }
            EditorInstance.UnsavedChanges = true;
            Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
            currentEntry.IsTrick = chkIsTrick.Checked;
            setTrickToolTipToolStripMenuItem.Visible = currentEntry.IsTrick;
            setTrickCategoryToolStripMenuItem.Visible = currentEntry.IsTrick;
            WriteCurentItem(currentEntry.ID);
        }

        public void RunLogicParser()
        {
            LogicParser PArser = new LogicParser();
            var result = PArser.ShowDialog();
            if (result == DialogResult.OK)
            {
                EditorInstance.UnsavedChanges = true;
                Tools.SaveState(EditorInstance, new LogicObjects.SaveState() { Logic = EditorInstance.Logic }, EditorInstanceUndoRedoData);
                if (currentEntry.Conditionals == null)
                {
                    currentEntry.Conditionals = LogicParser.Conditionals;
                }
                else
                {
                    currentEntry.Conditionals = currentEntry.Conditionals.Concat(LogicParser.Conditionals).ToArray();
                }
            }
            LogicParser.Conditionals = null;
            WriteCurentItem((int)nudIndex.Value);
        }

        public void ClearLogicData(bool YOLO = false)
        {
            if (!YOLO)
            {
                var Clear = MessageBox.Show("WARNING, this will clear all requirements and conditionals from your logic data, are you sure you wish to continue?", "Clear Logic Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (Clear != DialogResult.Yes) { return; }
            }

            foreach (var i in EditorInstance.Logic)
            {
                i.Required = null;
                i.Conditionals = null;
            }
            WriteCurentItem(currentEntry.ID);
        }

        private void LBRequired_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = LogicObjects.MainTrackerInstance.Options.FormFont;
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;
            e.Graphics.DrawString(LBRequired.Items[e.Index].ToString(), F, brush, e.Bounds);
            e.DrawFocusRectangle();
        }

        private void LBConditional_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = LogicObjects.MainTrackerInstance.Options.FormFont;
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;
            e.Graphics.DrawString(LBConditional.Items[e.Index].ToString(), F, brush, e.Bounds);
            e.DrawFocusRectangle();

            var Len = e.Graphics.MeasureString(LBConditional.Items[e.Index].ToString(), F).Width;
            if (Len > LBConditional.Width && (int)Len + 2 > LBConditional.HorizontalExtent) { LBConditional.HorizontalExtent = (int)Len + 2; }

        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            UserSettings.HandleUserPreset(sender, e);
        }

        private void lblDicName_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(lblDicName.Text);
        }

        private void lblItemName_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(lblItemName.Text);
        }

        private void lblLocName_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(lblLocName.Text);
        }

        private void lblTrickToolTip_MouseMove(object sender, MouseEventArgs e)
        {
            if (!currentEntry.IsTrick) { return; }
            var ToolTip = string.IsNullOrWhiteSpace(currentEntry.TrickToolTip) ? "No Tool Available\n\n(Double Click to set)" : currentEntry.TrickToolTip;
            var CurrentToolTipText = toolTip1.GetToolTip(lblTrickToolTip);
            if (CurrentToolTipText == ToolTip) { return; }
            toolTip1.SetToolTip(lblTrickToolTip, ToolTip);
        }

        private void lblCategory_MouseMove(object sender, MouseEventArgs e)
        {
            if (!currentEntry.IsTrick) { return; }
            var ToolTip = string.IsNullOrWhiteSpace(currentEntry.TrickCategory) ? "No Category Available\n\n(Double Click to set)" : currentEntry.TrickCategory;
            var CurrentToolTipText = toolTip1.GetToolTip(lblCategory);
            if (CurrentToolTipText == ToolTip) { return; }
            toolTip1.SetToolTip(lblCategory, ToolTip);
        }

        //Functions

        public void WriteCurentItem(int Index)
        {
            PrintingItem = true;
            LBRequired.ItemHeight = Convert.ToInt32(LogicObjects.MainTrackerInstance.Options.FormFont.Size * 1.7);
            LBConditional.ItemHeight = Convert.ToInt32(LogicObjects.MainTrackerInstance.Options.FormFont.Size * 1.7);
            LBRequired.Items.Clear();
            LBConditional.Items.Clear();
            LBConditional.HorizontalExtent = 0;
            LogicObjects.LogicEntry entry;
            try
            {
                entry = EditorInstance.Logic[Index];
            }
            catch
            {
                this.Text = "Logic Editor";
                FormatForm();
                return;
            }

            currentEntry = entry;
            renameCurrentItemToolStripMenuItem.Visible = currentEntry.UserCreatedFakeItem();
            deleteCurrentItemToolStripMenuItem.Visible = currentEntry.UserCreatedFakeItem();
            setTrickToolTipToolStripMenuItem.Visible = currentEntry.UserCreatedFakeItem() && currentEntry.IsTrick;
            setTrickCategoryToolStripMenuItem.Visible = currentEntry.UserCreatedFakeItem() && currentEntry.IsTrick;

            LBRequired.BeginUpdate();
            LBConditional.BeginUpdate();

            foreach (var i in entry.Required ?? new int[0])
            {
                var ReqEntry = EditorInstance.Logic[i];

                ReqEntry.DisplayName = ReqEntry.GetDistinctItemName(EditorInstance);
                ReqEntry.DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? ReqEntry.DictionaryName : ReqEntry.DisplayName;
                LBRequired.Items.Add(ReqEntry);
            }

            if (entry.Conditionals != null && entry.Conditionals.Count() > MaxEntries)
            {
                LBConditional.Items.Add("To many conditionals to display");
            }
            else
            {

                foreach (var j in entry.Conditionals ?? new int[0][])
                {
                    var CondEntry = new RequiementConditional { ItemIDs = new List<LogicObjects.LogicEntry>() };
                    string Display = "";
                    string addComma = "";
                    foreach (var i in j ?? new int[0])
                    {
                        var ReqEntry = EditorInstance.Logic[i];

                        string disName = ReqEntry.GetDistinctItemName(EditorInstance);
                        disName = (LogicEditor.UseDictionaryNameInSearch) ? ReqEntry.DictionaryName : disName;

                        Display = Display + addComma + disName;
                        addComma = ", ";
                        CondEntry.ItemIDs.Add(ReqEntry);

                    }
                    CondEntry.DisplayName = Display;
                    LBConditional.Items.Add(CondEntry);
                }
            }

            LBRequired.EndUpdate();
            LBConditional.EndUpdate();

            string DictionaryName = entry.DictionaryName.ToString();
            string LocationName = entry.LocationName ?? "Fake Location";
            string ItemName = entry.ItemName ?? "Fake Item";

            lblDicName.Text = DictionaryName;
            this.Text = (String.IsNullOrWhiteSpace(lblDicName.Text)) ? "Logic Editor" : $"Logic Editor: {lblDicName.Text}";
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
            chkSetDay1.Checked = (((entry.TimeSetup >> 0) & 1) == 1);
            chkSetDay2.Checked = (((entry.TimeSetup >> 2) & 1) == 1);
            chkSetDay3.Checked = (((entry.TimeSetup >> 4) & 1) == 1);
            chkSetNight1.Checked = (((entry.TimeSetup >> 1) & 1) == 1);
            chkSetNight2.Checked = (((entry.TimeSetup >> 3) & 1) == 1);
            chkSetNight3.Checked = (((entry.TimeSetup >> 5) & 1) == 1);

            chkIsTrick.Checked = entry.IsTrick;
            chkIsTrick.Enabled = entry.IsFake;

            nudIndex.Value = currentEntry.ID;

            lblTrickToolTip.Visible = entry.IsTrick;
            lblCategory.Visible = entry.IsTrick;

            PrintingItem = false;
        }

        public void WriteTimeDependecies(LogicObjects.LogicEntry entry)
        {
            entry.AvailableOn = 0;
            entry.NeededBy = 0;
            entry.TimeSetup = 0;
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
            if (chkSetDay1.Checked) { entry.TimeSetup += 1; };
            if (chkSetNight1.Checked) { entry.TimeSetup += 2; };
            if (chkSetDay2.Checked) { entry.TimeSetup += 4; };
            if (chkSetNight2.Checked) { entry.TimeSetup += 8; };
            if (chkSetDay3.Checked) { entry.TimeSetup += 16; };
            if (chkSetNight3.Checked) { entry.TimeSetup += 32; };
        }

        public void UpdateReqAndCond()
        {
            List<int> req = new List<int>();
            foreach (var i in LBRequired.Items) { req.Add((i as LogicObjects.LogicEntry).ID); }
            currentEntry.Required = req.ToArray();

            if (LBConditional.Items.Count > 0 && !(LBConditional.Items[0] is RequiementConditional))
            {
                Debugging.Log("Skipped Data"); return;
            }

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

        public bool PromptSave(bool OnlyIfUnsaved = true)
        {
            if (EditorInstance.UnsavedChanges || !OnlyIfUnsaved)
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

        public bool SaveInstance(bool UseTrickData = true, bool UseJson = false)
        {
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Logic File (*.txt)|*.txt", FilterIndex = 1 };
            if (saveDialog.ShowDialog() != DialogResult.OK) { return false; }
            List<string> logicText = new List<string>();
            if (UseJson)
            {
                logicText = LogicEditing.WriteLogicToJson(EditorInstance).ToList();
            }
            else
            {
                logicText = LogicEditing.WriteLogicToArray(EditorInstance, UseTrickData).ToList();
            }
            StreamWriter LogicFile = new StreamWriter(File.Open(saveDialog.FileName, FileMode.Create));
            for (int i = 0; i < logicText.Count; i++)
            {
                if (i == logicText.Count - 1) { LogicFile.Write(logicText[i]); break; }
                LogicFile.WriteLine(logicText[i]);
            }
            LogicFile.Close();
            EditorInstance.UnsavedChanges = false;
            return true;
        }

        private void saveLogicInJSONFormatBetaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.fileToolStripMenuItem.HideDropDown();
            SaveInstance(UseJson: true);
        }

        public void MoveItem(int direction)
        {
            if (LastSelectedListBox.SelectedItem == null || LastSelectedListBox.SelectedIndex < 0 || LastSelectedListBox.SelectedItems.Count > 1) { return; }
            int newIndex = LastSelectedListBox.SelectedIndex + direction;
            if (newIndex < 0 || newIndex >= LastSelectedListBox.Items.Count) { return; }
            object selected = LastSelectedListBox.SelectedItem;
            LastSelectedListBox.Items.Remove(selected);
            LastSelectedListBox.Items.Insert(newIndex, selected);
            LastSelectedListBox.SetSelected(newIndex, true);
            UpdateReqAndCond();
        }

        public void CreateContextMenus()
        {
            ContextMenuStrip AddConditionalMenu = new ContextMenuStrip();
            ToolStripItem AddPermutations = AddConditionalMenu.Items.Add("Create (Any X of A,B,C,D)");
            AddPermutations.Click += (sender, e) => { ContextMenuAddPermutations(sender, e); };
            ToolStripItem ParseExpression = AddConditionalMenu.Items.Add("Parse A logical Expression");
            ParseExpression.Click += (sender, e) =>  { RunLogicParser(); };
            btnAddCond.ContextMenuStrip = AddConditionalMenu;
        }

        //Static Functions

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

                if (LogicEntry1.SpoilerItem != null && LogicEntry1.SpoilerItem.Any()) 
                {
                    originalName = LogicEntry1.SpoilerItem[0];
                    while (usedSpoilerNames.Contains(LogicEntry1.SpoilerItem[0]))
                    {
                        LogicEntry1.SpoilerItem[0] = originalName + " (" + number.ToString() + ")";
                        number += 1;
                    }
                    usedSpoilerNames.Add(LogicEntry1.SpoilerItem[0]);
                }
            }
        }

        public static void CleanLogicEntry(LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool RemoveRedundant = true, bool Extractreq = true)
        {
            Utility.nullEmptyLogicItems(Instance.Logic);
            if (entry.Required == null && entry.Conditionals == null) { return; }
            var l = Instance.Logic;
            if (entry.Required != null &&
                (entry.Required.Where(x => l[x].DictionaryName.StartsWith("MMRTCombinations") || l[x].DictionaryName.StartsWith("MMRTCheckContains")).Any()))
            { return; }

            if (Extractreq && RemoveRedundant) { MoveRequirementsToConditionals(entry); }
            if (RemoveRedundant) { RemoveRedundantConditionals(entry); }
            if (Extractreq) { MakeCommonConditionalsRequirements(entry); }
        }

        public static void MoveRequirementsToConditionals(LogicObjects.LogicEntry entry)
        {
            if (entry.Required == null) { return; }
            if (entry.Conditionals == null) { entry.Conditionals = new int[][] { entry.Required }; }
            else
            {
                var NewConditionals = entry.Conditionals.Select(x => x.ToList()).ToArray();
                foreach (var i in NewConditionals)
                {
                    i.AddRange(entry.Required.ToList());
                }
                entry.Conditionals = NewConditionals.Select(x => x.ToArray()).ToArray();
            }
            entry.Required = null;
        }

        public static bool RemoveRedundantConditionals(LogicObjects.LogicEntry entry)
        {
            bool ChangesMade = false;
            if (entry.Conditionals == null) { return ChangesMade; }
            var cleanedConditionals = entry.Conditionals.Select(x => x.Distinct().ToList()).ToList();

            bool Clear = false;
            while (!Clear)
            {
                var test = cleanedConditionals.Where(i => IsRedundant(i, cleanedConditionals)).ToArray();
                if (test.Any())
                {
                    ChangesMade = true;
                    var TempCond = cleanedConditionals;
                    TempCond.Remove(test[0]);
                    cleanedConditionals = TempCond;
                }
                else { Clear = true; }
            }

            List<List<int>> TempConditionals = cleanedConditionals.Select(x => x.ToList()).ToList(); ;
            if (entry.Required != null)
            {
                var NewConditionals = cleanedConditionals.Select(x => x.ToList());
                foreach (var i in NewConditionals)
                {
                    if (i.Where(x => entry.Required.Contains(x)).Any()) { ChangesMade = true; }
                    i.RemoveAll(x => entry.Required.Contains(x));
                }
                TempConditionals = NewConditionals.ToList();
                TempConditionals.RemoveAll(x => !x.Any());
            }
            entry.Conditionals = (TempConditionals.Any()) ? TempConditionals.Select(x => x.ToArray()).ToArray() : null;

            return ChangesMade;

            bool IsRedundant(List<int> FocusedList, List<List<int>> CheckingList)
            {
                foreach (var i in CheckingList)
                {
                    if (!i.Equals(FocusedList) && i.All(j => FocusedList.Contains(j)))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static bool MakeCommonConditionalsRequirements(LogicObjects.LogicEntry entry)
        {
            bool ChangesMade = false;
            if (entry.Conditionals == null) { return ChangesMade; }
            List<int> ConsistantConditionals =
                entry.Conditionals.SelectMany(x => x).Distinct().Where(i => entry.Conditionals.All(x => x.Contains(i))).ToList();

            bool changesMade = ConsistantConditionals.Any();

            var NewRequirements = (entry.Required ?? new List<int>().ToArray()).ToList();
            NewRequirements.AddRange(ConsistantConditionals);
            entry.Required = (NewRequirements.Any()) ? NewRequirements.Distinct().ToArray() : null;

            var NewConditionals = entry.Conditionals.Select(x => x.ToList()).ToList();
            foreach (var i in NewConditionals)
            {
                i.RemoveAll(x => ConsistantConditionals.Contains(x));
            }
            NewConditionals.RemoveAll(x => !x.Any());
            entry.Conditionals = (NewConditionals.Any()) ? NewConditionals.Select(x => x.ToArray()).ToArray() : null;
            return ChangesMade;
        }

        public static int[][] CreatePermiations(int[] List, int numb)
        {
            var EnumList = List.Select(x => x);
            IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> items, int count)
            {
                int i = 0;
                foreach (var item in items)
                {
                    if (count == 1)
                        yield return new T[] { item };
                    else
                    {
                        foreach (var result in GetPermutations(items.Skip(i + 1), count - 1))
                            yield return new T[] { item }.Concat(result);
                    }
                    ++i;
                }
            }
            return GetPermutations(List, numb).Select(x => x.ToArray()).ToArray();
        }
    }
}
