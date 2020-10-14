using MMR_Tracker.Class_Files;
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
    public partial class MiscSingleItemSelect : Form
    {
        public MiscSingleItemSelect()
        {
            InitializeComponent();
        }

        public LogicObjects.TrackerInstance UsedInstance = LogicObjects.MainTrackerInstance;
        public int Function = 0;
        public int Display = 0;
        public List<LogicObjects.LogicEntry> ListContent = new List<LogicObjects.LogicEntry>();
        public LogicObjects.LogicEntry SelectedObject = null;

        private void MiscSingleItemSelect_Load(object sender, EventArgs e)
        {
            WriteToListBox();
        }

        private void WriteToListBox()
        {
            listBox1.Items.Clear();
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
                }
                if (string.IsNullOrWhiteSpace(ListItem.DisplayName)) { ListItem.DisplayName = i.DictionaryName + "DidError"; }
                if (Utility.FilterSearch(ListItem.LocationEntry, textBox1.Text, ListItem.DisplayName)) { listBox1.Items.Add(ListItem); }
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (!(listBox1.SelectedItem is LogicObjects.ListItem)) { return; }
            SelectedObject = (listBox1.SelectedItem as LogicObjects.ListItem).LocationEntry;
            switch (Function)
            {
                case 0:// General Select
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                case 1:// What Unlocked This
                    Tools.WhatUnlockedThis(SelectedObject);
                    break;
                case 2:// Spoiler Log Lookup
                    if (!(listBox1.SelectedItem is LogicObjects.ListItem)) { return; }
                    var item = (listBox1.SelectedItem as LogicObjects.ListItem).LocationEntry;
                    var itemLocation = item.GetItemsSpoilerLocation(UsedInstance.Logic);
                    if (itemLocation == null) { MessageBox.Show($"{item.DictionaryName} Was not found in spoiler data"); }
                    MessageBox.Show($"{item.ItemName??item.DictionaryName} is found at {itemLocation.LocationName ?? itemLocation.DictionaryName}", $"{ item.DictionaryName} Item Location: ");
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            WriteToListBox();
        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { textBox1.Clear(); }
        }
    }
}
