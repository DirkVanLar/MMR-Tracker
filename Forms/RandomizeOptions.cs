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
            string settingString = "";
            foreach (var item in LogicObjects.MainTrackerInstance.Logic)
            {
                if (item.IsFake) { continue; }

                int Setting = item.Options;

                settingString += Setting.ToString();
            }
            var logictext = LogicEditing.WriteLogicToArray(LogicObjects.MainTrackerInstance);
            string[] Options = new string[2 + logictext.Length];
            Options[0] = settingString;
            Options[1] = LogicObjects.MainTrackerInstance.LogicVersion.ToString();
            var count = 2;
            foreach (var i in logictext)
            {
                Options[count] = i;
                count++;
            }
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "MMR Tracker Settings (*.MMRTSET)|*.MMRTSET", FilterIndex = 1 };
            if (saveDialog.ShowDialog() == DialogResult.OK) { File.WriteAllLines(saveDialog.FileName, Options); }

        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            string file = Utility.FileSelect("Select A Settings File", "MMR Tracker Settings (*.MMRTSET)|*.MMRTSET");
            if (file == "") { return; }
            string[] options = File.ReadAllLines(file);
            UpdateRandomOptionsFromFile(options, LogicObjects.MainTrackerInstance);
            WriteToListVeiw();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e) { WriteToListVeiw(); }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (updating) { return; }
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

                if (option == 4)
                {
                    if (entry.StartingItem()) { entry.Options -= 4; }
                    else { entry.Options += 4; }
                }
                else
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
                    string[] row = { entry.DictionaryName, randomizedOptions[entry.RandomizedState()], entry.StartingItem().ToString() };
                    ListViewItem listViewItem = new ListViewItem(row) { Tag = entry.ID };
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
            updating = false;
        }

        public static void UpdateRandomOptionsFromFile(string[] options, LogicObjects.TrackerInstance Instance)
        {

            int Version = Int32.Parse(options[1]);

            if (Instance.LogicVersion != Version)
            {
                MessageBox.Show("This settings file was not made using the current logic version. Please resave your settings in the current logic version.");
                return;
            }
            int counter = 0;
            foreach (var item in Instance.Logic)
            {
                if (item.IsFake) { continue; }
                int setting = Int32.Parse(options[0][counter].ToString());
                item.Options = setting;
                counter++;
            }
        }

        private void txtSearch_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { txtSearch.Clear(); }
        }
    }
}
