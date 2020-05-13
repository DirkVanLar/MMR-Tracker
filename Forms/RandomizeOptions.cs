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
        public static bool PauseListview = false;
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
            EnableButtons(false, false);
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
                if (!CheckedItems.Contains(item)) { CheckedItems.Add(item); }
                
            }
            else
            {
                if (CheckedItems.IndexOf(item) > -1) { CheckedItems.RemoveAt(CheckedItems.IndexOf(item)); }
            }
            bool EnableRO = false;
            bool EnableTR = false;
            foreach (var i in CheckedItems)
            {
                if (i.IsTrick && i.IsFake) { EnableTR = true; }
                if (!i.IsFake) { EnableRO = true; }
            }
            EnableButtons(EnableTR, EnableRO);
        }

        private void EnableButtons(bool TR, bool RO)
        {
            BTNRandomized.Enabled = RO;
            BTNUnrando.Enabled = RO;
            BTNUnrandMan.Enabled = RO;
            BTNJunk.Enabled = RO;
            BTNStarting.Enabled = RO;
            btnToggleTricks.Enabled = TR;
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
            else if (CustomItemList.Count > 0)
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
            else if (ForceJunkList.Count > 0)
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

        private void chkRandomState_CheckedChanged(object sender, EventArgs e)
        {
            if (updating) { return; }
            PauseListview = true;
            chkShowRandom.Checked = chkRandomState.Checked;
            chkShowUnrand.Checked = chkRandomState.Checked;
            chkShowUnrandMan.Checked = chkRandomState.Checked;
            chkShowJunk.Checked = chkRandomState.Checked;
            chkShowStartingItems.Checked = chkRandomState.Checked;
            PauseListview = false;
            WriteToListVeiw();
        }

        private void chkTricks_CheckedChanged(object sender, EventArgs e)
        {
            if (updating) { return; }
            PauseListview = true;
            chkShowDisabledTricks.Checked = chkTricks.Checked;
            chkShowEnabledTricks.Checked = chkTricks.Checked;
            PauseListview = false;
            WriteToListVeiw();
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
                else if (option < 4 && !entry.IsFake)
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
            if (PauseListview) { return; }
            updating = true;
            listView1.Items.Clear();
            CheckfullchkState();
            var logic = LogicObjects.MainTrackerInstance.Logic;
            List<string> randomizedOptions = new List<string> { "Randomized", "Unrandomized", "Unrandomized (Manual)", "Forced Junk" };
            listView1.FullRowSelect = true;

            

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
            bool EnableRO = false;
            bool EnableTR = false;
            foreach (var i in CheckedItems)
            {
                if (i.IsTrick && i.IsFake) { EnableTR = true; }
                if (!i.IsFake) { EnableRO = true; }
            }

            EnableButtons(EnableTR, EnableRO);

            updating = false;
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

        private void CheckfullchkState()
        {
            bool allcheck = (chkShowRandom.Checked && chkShowUnrand.Checked && chkShowUnrandMan.Checked && chkShowJunk.Checked && chkShowStartingItems.Checked);
            bool someChecked = (chkShowRandom.Checked || chkShowUnrand.Checked || chkShowUnrandMan.Checked || chkShowJunk.Checked || chkShowStartingItems.Checked);
            if (allcheck)
            {
                chkRandomState.CheckState = CheckState.Checked;
            }
            else if (someChecked)
            {
                chkRandomState.CheckState = CheckState.Indeterminate;
            }
            else
            {
                chkRandomState.CheckState = CheckState.Unchecked;
            }
            allcheck = (chkShowEnabledTricks.Checked && chkShowDisabledTricks.Checked);
            someChecked = (chkShowEnabledTricks.Checked || chkShowDisabledTricks.Checked);
            if (allcheck)
            {
                chkTricks.CheckState = CheckState.Checked;
            }
            else if (someChecked)
            {
                chkTricks.CheckState = CheckState.Indeterminate;
            }
            else
            {
                chkTricks.CheckState = CheckState.Unchecked;
            }
        }

        private void btnLoadMMRSet_Click(object sender, EventArgs e)
        {
            string file = Utility.FileSelect("Select MMR Settings File", "MMR Settings File (*.json)|*.json");
            if (file == "") { return; }
            LogicObjects.Configuration SettingFile = new LogicObjects.Configuration();
            try { SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(File.ReadAllText(file)); }
            catch { Console.WriteLine("Can't Read"); }
            var Settings = SettingFile.GameplaySettings;

            //Apply custom item strings
            if (Settings.UseCustomItemList) { txtCustomItemString.Text = Settings.CustomItemListString; }
            txtJunkItemString.Text = Settings.CustomJunkLocationsString;
            btnApplyString_Click(null, null);

            //Apply tricks
            foreach (var i in LogicObjects.MainTrackerInstance.Logic.Where(x => x.IsTrick))
            {
                i.TrickEnabled = Settings.EnabledTricks.Contains(i.ID);
            }

            //Apply Items Settings
            if (!Settings.UseCustomItemList)
            {
                //Dungeon Items
                EditRange("Woodfall Map", "Stone Tower Key 4 - death armos maze", Settings.AddDungeonItems);
                //Shop Items
                EditRange("Trading Post Red Potion", "Zora Shop Red Potion", Settings.AddShopItems);
                EditRange("Bomb Bag (20)", "Bomb Bag (20)", Settings.AddShopItems);
                EditRange("Town Bomb Bag (30)", "Town Bomb Bag (30)", Settings.AddShopItems);
                EditRange("Milk Bar Chateau", "Milk Bar Milk", Settings.AddShopItems);
                EditRange("Swamp Scrub Magic Bean", "Canyon Scrub Blue Potion", Settings.AddShopItems);
                EditRange("Gorman Bros Purchase Milk", "Gorman Bros Purchase Milk", Settings.AddShopItems);
                //Misc
                EditRange("Lens Cave 20r", "Ikana Scrub 200r", Settings.AddOther);
                //Bottle Catch
                EditRange("Bottle: Fairy", "Bottle: Mushroom", Settings.RandomizeBottleCatchContents);
                //Moon
                EditRange("Deku Trial HP", "Link Trial 10 Bombchu", Settings.AddMoonItems);
                //Fairy Rewards
                EditRange("Great Fairy Magic Meter", "Great Fairy's Sword", Settings.AddFairyRewards);
                EditRange("Great Fairy's Mask", "Great Fairy's Mask", Settings.AddFairyRewards);
                //Pre Clocktown
                EditRange("Pre-Clocktown 10 Deku Nuts", "Pre-Clocktown 10 Deku Nuts", Settings.AddNutChest);
                //Starting Items
                EditRange("Starting Sword", "Starting Heart 2", Settings.CrazyStartingItems);
                //Cows
                EditRange("Ranch Cow #1 Milk", "Great Bay Coast Grotto Cow #2 Milk", Settings.AddCowMilk);
                //Skulls
                EditRange("Swamp Skulltula Main Room Near Ceiling", "Ocean Skulltula 2nd Room Behind Skull 2", Settings.AddSkulltulaTokens);
                //Fairies
                EditRange("Clock Town Stray Fairy", "Stone Tower Lava Room Ledge", Settings.AddStrayFairies);
                //Mundane
                EditRange("Lottery 50r", "Seahorse", Settings.AddMundaneRewards);
                //Preserve Soaring
                EditRange("Song of Soaring", "Song of Soaring", !Settings.ExcludeSongOfSoaring);
                //Dungeon Entrances
                EditRange("Woodfall Temple access", "Woodfall Temple access", Settings.RandomizeDungeonEntrances);
                EditRange("Snowhead Temple access", "Snowhead Temple access", Settings.RandomizeDungeonEntrances);
                EditRange("Great Bay Temple access", "Great Bay Temple access", Settings.RandomizeDungeonEntrances);
                EditRange("Inverted Stone Tower Temple access", "Inverted Stone Tower Temple access", Settings.RandomizeDungeonEntrances);
            }

            //Junk Starting Items
            if (Settings.NoStartingItems)
            {
                List<string> StartingItems = new List<string> 
                {
                    "Starting Sword",
                    "Starting Shield",
                    "Starting Heart 1",
                    "Starting Heart 2",
                    "Deku Mask"
                };
                if (Settings.AddSongs) { StartingItems.Add("Song of Healing"); }
                foreach(var i in StartingItems)
                {
                    var item = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == i);
                    if (item == null) { continue; }
                    bool Starting = item.StartingItem();
                    bool Radnomized = item.Randomized();
                    if (Radnomized) { item.Options = (Starting) ? 7 : 3; }
                }

            }

            WriteToListVeiw();
        }

        public void EditRange(string start, string end, bool Randomized)
        {
            var StartingItem = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == start);
            var EndingItem = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == end);
            if (StartingItem == null || EndingItem == null) { return; }

            for (var i = StartingItem.ID; i <= EndingItem.ID; i++)
            {
                int O = (Randomized) ? 0 : 1;
                O = (LogicObjects.MainTrackerInstance.Logic[i].StartingItem()) ? O + 4 : O;
                LogicObjects.MainTrackerInstance.Logic[i].Options = O;
            }

        }
    }
}
