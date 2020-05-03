using MMR_Tracker.Class_Files;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    public partial class RandomizeOptions : Form
    {
        public static bool updating = false;
        public List<LogicObjects.LogicEntry> CheckedItems = new List<LogicObjects.LogicEntry>();
        public RandomizeOptions()
        {
            InitializeComponent();
        }

        //Form Objects

        private void RandomizeOptions_Load(object sender, EventArgs e)
        {
            chkShowRandom.Checked = true;
            chkShowUnrand.Checked = true;
            chkShowUnrandMan.Checked = true;
            chkShowJunk.Checked = true;
            chkShowStartingItems.Checked = true;
            chkShowDisabledTricks.Checked = true;
            chkShowEnabledTricks.Checked = true;
            listView1.Columns[0].Width = 400;
            listView1.Columns[1].Width = 100;
            listView1.Columns[2].Width = 85;
            listView1.Columns[3].Width = 80;
            txtSearch.Text = "";
            WriteToListVeiw();
        }

        private void CHKShowRandom_CheckedChanged(object sender, EventArgs e) { WriteToListVeiw(); }

        private void ChkShowUnrand_CheckedChanged(object sender, EventArgs e) { WriteToListVeiw(); }

        private void ChkShowUnrandMan_CheckedChanged(object sender, EventArgs e) { WriteToListVeiw(); }

        private void ChkJunk_CheckedChanged(object sender, EventArgs e) { WriteToListVeiw(); }

        private void ChkStartingItems_CheckedChanged(object sender, EventArgs e) { WriteToListVeiw(); }

        private void BTNRandomized_Click(object sender, EventArgs e) { UpdateRandomOption(0); }

        private void BTNUnrando_Click(object sender, EventArgs e) { UpdateRandomOption(1); }

        private void BTNUnrandMan_Click(object sender, EventArgs e) { UpdateRandomOption(2); }

        private void BTNJunk_Click(object sender, EventArgs e) { UpdateRandomOption(3); }

        private void BTNStarting_Click(object sender, EventArgs e) { UpdateRandomOption(4); }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var template = new LogicObjects.TrackerInstance();
            template.RawLogicFile = LogicObjects.MainTrackerInstance.RawLogicFile;
            LogicEditing.PopulateTrackerInstance(template);
            foreach (var i in template.Logic)
            {
                var main = LogicObjects.MainTrackerInstance.Logic[i.ID];
                i.Options = main.Options;
                i.TrickEnabled = main.TrickEnabled;
            }
            Tools.SaveInstance(template);
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            string file = Utility.FileSelect("Select A Save File", "MMR Tracker Save (*.MMRTSAV)|*.MMRTSAV");
            if (file == "") { return; }
            var template = new LogicObjects.TrackerInstance();
            try { template = JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(File.ReadAllText(file)); }
            catch
            {
                try
                {
                    string[] options = File.ReadAllLines(file);
                    template.Logic = JsonConvert.DeserializeObject<List<LogicObjects.LogicEntry>>(options[0]);
                    if (options.Length > 1) { template.LogicVersion = Int32.Parse(options[1].Replace("version:", "")); }
                    else
                    {
                        MessageBox.Show("Save File Invalid!");
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Save File Invalid!");
                    return;
                }
            }

            foreach(var i in LogicObjects.MainTrackerInstance.Logic)
            {
                var TemplateData = template.Logic.Find(x => x.DictionaryName == i.DictionaryName);
                if (TemplateData != null)
                {
                    i.Options = TemplateData.Options;
                    i.TrickEnabled = TemplateData.TrickEnabled;
                }
            }
            WriteToListVeiw();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e) { WriteToListVeiw(); }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (updating) { return; }
            if (Int32.Parse(e.Item.Tag.ToString()) == -1) { e.Item.Checked = false; return; }
            var item = LogicObjects.MainTrackerInstance.Logic[Int32.Parse(e.Item.Tag.ToString())];
            if (e.Item.Checked)
            {
                if (CheckedItems.Contains(item)) { return; }
                CheckedItems.Add(item);
            }
            else
            {
                if (CheckedItems.IndexOf(item) < 0) { return; }
                CheckedItems.RemoveAt(CheckedItems.IndexOf(item));
            }
        }

        //Functions

        public void UpdateRandomOption(int option)
        {
            foreach (var selection in CheckedItems)
            {
                var entry = selection as LogicObjects.LogicEntry;
                if (option == 5 && entry.IsTrick && entry.IsFake)
                {
                    entry.TrickEnabled = !entry.TrickEnabled;
                }
                else if (option == 4 && !entry.IsFake)
                {
                    if (entry.StartingItem()) { entry.Options -= 4; }
                    else { entry.Options += 4; }
                }
                else if (!entry.IsFake)
                {
                    if (entry.StartingItem()) { entry.Options = option + 4; }
                    else { entry.Options = option; }
                }
            }
            CheckedItems = new List<LogicObjects.LogicEntry>();
            WriteToListVeiw();
        }

        public void WriteToListVeiw()
        {
            updating = true;
            listView1.Items.Clear();
            var logic = LogicObjects.MainTrackerInstance.Logic;
            List<string> randomizedOptions = new List<string> { "Randomized", "Unrandomized", "Unrandomized (Manual)", "Forced Junk" };
            listView1.FullRowSelect = true;
            Console.WriteLine("========================================================================================");

            

            if (ShowingRandOptions())
            {
                ListViewItem RandomITemHeader = new ListViewItem("RANDOMIZED ITEMS ===============================================================================") { Tag = -1 };
                listView1.Items.Add(RandomITemHeader);
            }

            foreach (var entry in logic)
            {
                bool chkValid = false;
                if (entry.RandomizedState() == 0 && chkShowRandom.Checked) { chkValid = true; }
                if (entry.RandomizedState() == 1 && chkShowUnrand.Checked) { chkValid = true; }
                if (entry.RandomizedState() == 2 && chkShowUnrandMan.Checked) { chkValid = true; }
                if (entry.RandomizedState() == 3 && chkShowJunk.Checked) { chkValid = true; }
                if (entry.StartingItem() && chkShowStartingItems.Checked) { chkValid = true; }

                if (!entry.IsFake && chkValid && Utility.FilterSearch(entry, txtSearch.Text, entry.DictionaryName))
                {
                    string[] row = { 
                        entry.DictionaryName, randomizedOptions[entry.RandomizedState()], entry.StartingItem().ToString(), "" };
                    ListViewItem listViewItem = new ListViewItem(row) { Tag = entry.ID };
                    listView1.Items.Add(listViewItem);
                }
            }
            if (ShowingTrickOptions())
            {
                ListViewItem TrickHeader = new ListViewItem("TRICKS ===============================================================================") { Tag = -1 };
                listView1.Items.Add(TrickHeader);
            }

            foreach (var entry in logic.Where(x => x.IsFake && x.IsTrick))
            {
                bool chkValid = false;
                if (entry.TrickEnabled && chkShowEnabledTricks.Checked) { chkValid = true; }
                if (!entry.TrickEnabled && chkShowDisabledTricks.Checked) { chkValid = true; }

                if (chkValid && Utility.FilterSearch(entry, txtSearch.Text, entry.DictionaryName))
                {
                    string[] row = {
                        entry.DictionaryName, "", "", entry.TrickEnabled.ToString() };
                    ListViewItem listViewItem = new ListViewItem(row) { Tag = entry.ID };
                    listViewItem.ToolTipText = entry.TrickToolTip;
                    listView1.Items.Add(listViewItem);
                }
            }
            var checkedItems = CheckedItems.Cast<LogicObjects.LogicEntry>().ToList();
            foreach (ListViewItem i in listView1.Items)
            {
                if (checkedItems.Any(p => p.ID == Int32.Parse(i.Tag.ToString())))
                {
                    i.Checked = true;
                }
            }

            if (ShowingRandOptions() && !ShowingTrickOptions())
            {
                listView1.Columns[0].Width = 480;
                listView1.Columns[1].Width = 100;
                listView1.Columns[2].Width = 85;
                listView1.Columns[3].Width = 0;
            }
            else if (!ShowingRandOptions() && ShowingTrickOptions())
            {
                listView1.Columns[0].Width = 585;
                listView1.Columns[1].Width = 0;
                listView1.Columns[2].Width = 0;
                listView1.Columns[3].Width = 80;
            }
            else
            {
                listView1.Columns[0].Width = 400;
                listView1.Columns[1].Width = 100;
                listView1.Columns[2].Width = 85;
                listView1.Columns[3].Width = 80;
            }

            updating = false;
        }

        private void txtSearch_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { txtSearch.Clear(); }
        }

        private void chkShowEnabledTricks_CheckedChanged(object sender, EventArgs e)
        {
            WriteToListVeiw();
        }

        private void chkShowDisabledTricks_CheckedChanged(object sender, EventArgs e)
        {
            WriteToListVeiw();
        }

        private void btnToggleTricks_Click(object sender, EventArgs e)
        {
            UpdateRandomOption(5);
        }

        private void btnApplyString_Click(object sender, EventArgs e)
        {
            var CustomItemList = Tools.ParseSettingString(txtCustomItemString.Text);
            var ForceJunkList = Tools.ParseSettingString(txtJunkItemString.Text);

            if (CustomItemList == null) { label3.Text = "Custom Item String (INVALID!)"; }
            else
            {
                label3.Text = "Custom Item String";
                var Counter = 0;
                var MI = LogicObjects.MainTrackerInstance.Logic;
                foreach (var i in MI)
                {
                    if (!i.IsFake && i.ItemSubType != "Dungeon Entrance")
                    {
                        if (CustomItemList.Contains(Counter))
                        {
                            i.Options = (i.StartingItem()) ? 4 : 0;
                        }
                        else
                        {
                            i.Options = (i.StartingItem()) ? 5 : 1;
                        }
                        Counter++;
                    }
                }
            }
            if (ForceJunkList == null) { label4.Text = "Force Junk String (INVALID!)"; }
            else
            {
                label4.Text = "Force Junk String";
                var Counter = 0;
                var MI = LogicObjects.MainTrackerInstance.Logic;
                foreach (var i in MI)
                {
                    if (!i.IsFake && i.ItemSubType != "Dungeon Entrance")
                    {
                        if (ForceJunkList.Contains(Counter) && i.Randomized())
                        {
                            i.Options = (i.StartingItem()) ? 7 : 3;
                        }
                        Counter++;
                    }
                }
            }
            WriteToListVeiw();
        }

        private bool ShowingRandOptions()
        {
            if(!(chkShowRandom.Checked || chkShowUnrand.Checked || chkShowUnrandMan.Checked || chkShowJunk.Checked || chkShowStartingItems.Checked)) { return false; }

            var logic = LogicObjects.MainTrackerInstance.Logic;
            foreach (var entry in logic)
            {
                bool chkValid = false;
                if (entry.RandomizedState() == 0 && chkShowRandom.Checked) { chkValid = true; }
                if (entry.RandomizedState() == 1 && chkShowUnrand.Checked) { chkValid = true; }
                if (entry.RandomizedState() == 2 && chkShowUnrandMan.Checked) { chkValid = true; }
                if (entry.RandomizedState() == 3 && chkShowJunk.Checked) { chkValid = true; }
                if (entry.StartingItem() && chkShowStartingItems.Checked) { chkValid = true; }

                if (!entry.IsFake && chkValid && Utility.FilterSearch(entry, txtSearch.Text, entry.DictionaryName))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ShowingTrickOptions()
        {
            if (!(chkShowEnabledTricks.Checked || chkShowDisabledTricks.Checked)) { return false; }

            var logic = LogicObjects.MainTrackerInstance.Logic;
            foreach (var entry in logic.Where(x => x.IsFake && x.IsTrick))
            {
                bool chkValid = false;
                if (entry.TrickEnabled && chkShowEnabledTricks.Checked) { chkValid = true; }
                if (!entry.TrickEnabled && chkShowDisabledTricks.Checked) { chkValid = true; }

                if (chkValid && Utility.FilterSearch(entry, txtSearch.Text, entry.DictionaryName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
