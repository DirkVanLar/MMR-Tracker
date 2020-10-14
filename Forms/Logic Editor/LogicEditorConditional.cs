﻿using MMR_Tracker.Class_Files;
using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMR_Tracker.Forms.Sub_Forms
{
    public partial class LogicEditorConditional : Form
    {
        public LogicEditorConditional()
        {
            InitializeComponent();
        }

        public LogicObjects.TrackerInstance UsedInstance = LogicObjects.MainTrackerInstance;
        public List<LogicObjects.LogicEntry> ListContent = new List<LogicObjects.LogicEntry>();
        public List<int> CheckedItems = new List<int>();
        public List<int> CheckedTemplate = new List<int>();
        public int Display = 0;
        public int Function = 0;
        public List<LogicObjects.LogicEntry> SelectedItems = new List<LogicObjects.LogicEntry>();
        private bool Updating = false;
        private bool AddCondSeperately = false;
        public LogicEditor EditorForm = null;

        private void LogicEditorConditional_Load(object sender, EventArgs e)
        {
            if (EditorForm == null) { return; }
            WriteToListBox();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            WriteToListBox();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            AddCondSeperately = checkBox1.Checked;
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (Updating) { return; }
            var NewItem = e.Item.Tag as LogicObjects.LogicEntry;
            if (e.Item.Checked)
            {
                CheckedItems.Add(NewItem.ID);
            }
            else
            {
                CheckedItems.RemoveAt(CheckedItems.IndexOf(NewItem.ID));
            }
        }
        private void WriteToListBox()
        {
            listView1.BeginUpdate();
            Updating = true;
            listView1.Items.Clear();
            foreach (var i in ListContent)
            {
                LogicObjects.ListItem ListItem = new LogicObjects.ListItem();
                ListItem.LocationEntry = i;
                switch (Display)
                {
                    case 0:
                        ListItem.DisplayName = i.DictionaryName;
                        break;
                    case 1:
                        ListItem.DisplayName = i.LocationName ?? i.DictionaryName;
                        break;
                    case 2:
                        ListItem.DisplayName = i.ItemName ?? i.DictionaryName;
                        break;
                    case 3:
                        ListItem.DisplayName = i.SpoilerLocation[0] ?? i.LocationName ?? i.DictionaryName;
                        break;
                    case 4:
                        ListItem.DisplayName = i.SpoilerItem[0] ?? i.LocationName ?? i.DictionaryName;
                        break;
                    case 5:
                        ListItem.DisplayName = i.ProgressiveItemName(UsedInstance);
                        break;
                    case 6:
                        ListItem.DisplayName = i.LocationName ?? i.DictionaryName;
                        ListItem.DisplayName = (LogicEditor.UseSpoilerInDisplay) ? (i.SpoilerLocation[0] ?? ListItem.DisplayName) : ListItem.DisplayName;
                        ListItem.DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? i.DictionaryName : ListItem.DisplayName;
                        break;
                    case 7:
                        ListItem.DisplayName = i.ItemName ?? i.DictionaryName;
                        ListItem.DisplayName = (LogicEditor.UseSpoilerInDisplay) ? (i.SpoilerItem[0] ?? ListItem.DisplayName) : ListItem.DisplayName;
                        ListItem.DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? i.DictionaryName : ListItem.DisplayName;
                        break;
                }
                if (string.IsNullOrWhiteSpace(ListItem.DisplayName)) { ListItem.DisplayName = i.DictionaryName + "DidError"; }
                if (Utility.FilterSearch(ListItem.LocationEntry, textBox1.Text, ListItem.DisplayName))
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = ListItem.DisplayName;
                    item.Tag = ListItem.LocationEntry;
                    listView1.Items.Add(item);
                }
                RecheckItems();
            }
            Updating = false;
            listView1.EndUpdate();
        }
        private void RecheckItems()
        {
            for (var i = 0; i < listView1.Items.Count; i++)
            {
                var item = listView1.Items[i].Tag as LogicObjects.LogicEntry;
                if (CheckedItems.Contains(item.ID)) { listView1.Items[i].Checked = true; }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var i in CheckedItems.Where(x => UsedInstance.ItemInRange(x)))
            {
                SelectedItems.Add(UsedInstance.Logic[i]);
            }
            addconditional();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (var i in CheckedItems.Where(x => UsedInstance.ItemInRange(x)))
            {
                SelectedItems.Add(UsedInstance.Logic[i]);
            }
            addconditional();
            checkBox1.Checked = false;
            SelectedItems.Clear();
            CheckedItems.Clear();
            foreach(var i in CheckedTemplate) { CheckedItems.Add(i); }
            WriteToListBox();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CheckedTemplate.Clear();
            foreach (var i in CheckedItems) { CheckedTemplate.Add(i); listBox1.Items.Add(UsedInstance.Logic[i].DictionaryName); }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CheckedTemplate.Clear();
            listBox1.Items.Clear();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            EditorForm.RunLogicParser();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            EditorForm.ContextMenuAddPermutations(sender, e);
        }

        private void addconditional()
        {
            LogicEditor.EditorInstance.UnsavedChanges = true;
            Tools.SaveState(LogicEditor.EditorInstance);
            if (SelectedItems.Count < 1) { return; }
            if (AddCondSeperately)
            {
                foreach (var i in SelectedItems)
                {
                    var entry = new LogicEditor.RequiementConditional { DisplayName = (i.ItemName ?? i.DictionaryName), ItemIDs = new List<LogicObjects.LogicEntry> { i } };
                    if (EditorForm.LBConditional.Items.Contains(entry)) { continue; }
                    EditorForm.LBConditional.Items.Add(entry);
                }
            }
            else
            {
                LogicEditor.RequiementConditional entry = new LogicEditor.RequiementConditional { ItemIDs = new List<LogicObjects.LogicEntry>() };
                string Display = "";
                string addComma = "";
                foreach (var i in SelectedItems)
                {
                    Display = Display + addComma + (i.ItemName ?? i.DictionaryName);
                    addComma = ", ";
                    entry.ItemIDs.Add(i);
                }
                entry.DisplayName = Display;
                if (entry.DisplayName == "" || entry.ItemIDs.Count < 1) { return; }
                if (!EditorForm.LBConditional.Items.Contains(entry)) { EditorForm.LBConditional.Items.Add(entry); }

            }
            EditorForm.UpdateReqAndCond();
            EditorForm.WriteCurentItem((int)EditorForm.nudIndex.Value);
        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { textBox1.Clear(); }
        }
    }
}