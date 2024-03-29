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
    public partial class LogicEditorAddPermutations : Form
    {
        public LogicEditorAddPermutations()
        {
            InitializeComponent();
        }

        public LogicObjects.TrackerInstance UsedInstance = LogicEditor.EditorInstance;
        public List<LogicObjects.LogicEntry> ListContent = new List<LogicObjects.LogicEntry>();
        public List<int> CheckedItems = new List<int>();
        public int Display = 0;
        public int Function = 0;
        public List<LogicObjects.LogicEntry> SelectedItems = new List<LogicObjects.LogicEntry>();
        private bool Updating = false;

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            WriteToListBox();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var i in CheckedItems.Where(x => UsedInstance.ItemInRange(x)))
            {
                SelectedItems.Add(UsedInstance.Logic[i]);
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
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
            EnforceNeededLimit();
        }

        private void LogicEditorAddPermutations_Load(object sender, EventArgs e)
        {
            WriteToListBox();
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
                        ListItem.DisplayName = i.GetDistinctItemName(LogicEditor.EditorInstance);
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
                        ListItem.DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? i.DictionaryName : ListItem.DisplayName;
                        break;
                    case 7:
                        ListItem.DisplayName = i.GetDistinctItemName(LogicEditor.EditorInstance);
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

        private void EnforceNeededLimit()
        {
            Updating = true;
            if (CheckedItems.Count() == 0) { numericUpDown1.Value = 0; }
            if ((int)numericUpDown1.Value < 1) { numericUpDown1.Value = 1; }
            if ((int)numericUpDown1.Value > CheckedItems.Count()) { numericUpDown1.Value = CheckedItems.Count(); }
            Updating = false;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (Updating) { return; }
            EnforceNeededLimit();
        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { textBox1.Clear(); }
        }

        private void LogicEditorAddPermutations_Shown(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox1.Focus();
        }
    }
}
