﻿using MMR_Tracker.Class_Files;
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
        public static List<LogicObjects.LogicEntry> ItemStringLogic;
        public RandomizeOptions()
        {
            InitializeComponent();
        }

        //Form Objects

        public static bool IsInMMRItemList(LogicObjects.LogicEntry x, bool IncludeOwl = false)
        {
            return 
                (
                    (
                        x.ItemSubType == "Bottle" ||
                        x.ItemSubType == "Item" ||
                        x.ItemSubType == "Entrance" ||
                        (x.ItemSubType == "Owl Statue" && IncludeOwl)
                    ) &&
                    x.DictionaryName != "Ice Trap"
                );
        }

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
            btnLoadMMRSet.Enabled = LogicObjects.MainTrackerInstance.IsMM();
            txtRandEntString.Enabled = LogicObjects.MainTrackerInstance.IsEntranceRando();

            ItemStringLogic = LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsFake && (IsInMMRItemList(x) || !LogicObjects.MainTrackerInstance.IsMM())).ToList();

            EnableButtons(false, false);
            WriteToListVeiw();
        }

        private void CHK_CheckedChanged(object sender, EventArgs e) { WriteToListVeiw(); }

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

        private void btnLoadMMRSet_Click(object sender, EventArgs e)
        {
            string file = Utility.FileSelect("Select MMR Settings File", "MMR Settings File (*.json)|*.json");
            if (file == "") { return; }
            LogicObjects.Configuration SettingFile = new LogicObjects.Configuration();
            try { SettingFile = JsonConvert.DeserializeObject<LogicObjects.Configuration>(File.ReadAllText(file)); }
            catch { MessageBox.Show("Options file inavlid!"); return; }
            var Settings = SettingFile.GameplaySettings;
            ApplyRandomizerSettings(Settings);
            WriteToListVeiw();
        }

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

        private void btnToggleTricks_Click(object sender, EventArgs e) { UpdateRandomOption(5); }

        private void btnApplyString_Click(object sender, EventArgs e)
        {
            ItemStringLogic = LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsFake && (IsInMMRItemList(x) || !LogicObjects.MainTrackerInstance.IsMM())).ToList();
            var ItemLogic = ItemStringLogic.Where(x => !x.IsEntrance()).ToList();
            var ItemGroupCount = (int)Math.Ceiling(ItemLogic.Count / 32.0);

            var CustomItemList = Tools.ParseLocationAndJunkSettingString(txtCustomItemString.Text, ItemGroupCount, "Item");
            var ForceJunkList = Tools.ParseLocationAndJunkSettingString(txtJunkItemString.Text, ItemGroupCount, "Junk");
            var EntranceList = Tools.ParseEntranceandStartingString(txtRandEntString.Text, ItemStringLogic.Where(x => x.IsEntrance()).ToList(), "Entrance");

            var StartingStringLogic = LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsFake && (IsInMMRItemList(x, true) || !LogicObjects.MainTrackerInstance.IsMM())).ToList();
            //Should be obsolete with the new Dictionary based starting item system since it marks the actual items as starting items.
            //if (LogicObjects.MainTrackerInstance.LogicFormat == "txt" || LogicObjects.MainTrackerInstance.LogicFormat == "entrance") 
            //{ RemoveDuplicateStartingItems(StartingStringLogic, LogicObjects.MainTrackerInstance); }
            var StartingList = Tools.ParseEntranceandStartingString(txtStartingitemString.Text, StartingStringLogic.Where(x => x.CanBeStartingItem(LogicObjects.MainTrackerInstance)).ToList(), "Starting item");

            label3.Text = "Custom Item String";
            if (CustomItemList == null) { label3.Text = "Custom Item String (INVALID!)"; }
            else if (CustomItemList.Count > 0)
            {
                var Counter = 0;
                var MI = LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsEntrance());
                foreach (var i in MI)
                {
                    if (!i.IsFake && i.ItemSubType != "Dungeon Entrance")
                    {
                        if (CustomItemList.Contains(Counter))
                        {
                            i.SetRandomized();
                        }
                        else
                        {
                            i.SetUnRandomized();
                        }
                        Counter++;
                    }
                }
            }
            label4.Text = "Force Junk String";
            if (ForceJunkList == null) { label4.Text = "Force Junk String (INVALID!)"; }
            else if (ForceJunkList.Count > 0)
            {
                var Counter = 0;
                var MI = LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsEntrance());
                foreach (var i in MI)
                {
                    if (!i.IsFake && i.ItemSubType != "Dungeon Entrance")
                    {
                        if (ForceJunkList.Contains(Counter) && i.Randomized())
                        {
                            i.SetJunk();
                        }
                        Counter++;
                    }
                }
            }
            label6.Text = "Randomized Entrance String";
            if (EntranceList == null) { label6.Text = "Entrance String (INVALID!)"; }
            else if (EntranceList.Count > 0)
            {
                var MI = LogicObjects.MainTrackerInstance.Logic.Where(x => x.IsEntrance());
                foreach (var i in MI)
                {
                    if (EntranceList.Contains(i))
                    {
                        i.SetRandomized();
                    }
                    else
                    {
                        i.SetUnRandomized();
                    }
                }
            }
            label6.Text = "Starting Item String";
            if (StartingList == null) { label6.Text = ("Starting Item String (INVALID!)"); }
            else if (StartingList.Count > 0)
            {
                var MI = StartingStringLogic;
                foreach (var i in MI)
                {
                    if (StartingList.Contains(i))
                    {
                        i.ToggleStartingItem(true);
                    }
                    else
                    {
                        i.ToggleStartingItem(false);
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
                    entry.ToggleStartingItem();
                }
                else if (option < 4 && !entry.IsFake)
                {
                    entry.Options = entry.isStartingItem() ? option + 4 : option;
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

            List<ListViewItem> TempList = new List<ListViewItem>();

            if (ShowingRandOptions())
            {
                ListViewItem RandomITemHeader = new ListViewItem("RANDOMIZED ITEMS ===============================================================================") { Tag = -1 };
                TempList.Add(RandomITemHeader);
            }

            bool isValid(LogicObjects.LogicEntry x, string Displayname)
            {
                if (x.IsFake 
                    || x.LocationArea == "%Settings%" 
                    || x.LocationArea == "Hidden" 
                    || x.ItemSubType.Contains("Setting") 
                    || string.IsNullOrWhiteSpace(x.ItemSubType)) { return false; }
                if (!Utility.FilterSearch(x, txtSearch.Text, Displayname)) { return false; }
                if (x.isStartingItem() && chkShowStartingItems.Checked) { return true; }
                if (x.RandomizedState() == 0 && !chkShowRandom.Checked) { return false; }
                if (x.RandomizedState() == 1 && !chkShowUnrand.Checked) { return false; }
                if (x.RandomizedState() == 2 && !chkShowUnrandMan.Checked) { return false; }
                if (x.RandomizedState() == 3 && !chkShowJunk.Checked) { return false; }
                return true;
            }


            foreach (var entry in logic)
            {
                var Disname = entry.DictionaryName;
                if (!string.IsNullOrWhiteSpace(entry.LocationName))
                {
                    Disname = chkShowLogicName.Checked ? $"{Disname} ({entry.LocationName})" : entry.LocationName;
                    if (!string.IsNullOrWhiteSpace(entry.ItemName)) { Disname += $" ({entry.ItemName})"; }
                }
                if (!isValid(entry, Disname)) { continue; }
                string[] row = { Disname, randomizedOptions[entry.RandomizedState()], entry.isStartingItem().ToString(), "" };
                ListViewItem listViewItem = new ListViewItem(row) { Tag = entry.ID };
                TempList.Add(listViewItem);
            }
            if (ShowingTrickOptions())
            {
                ListViewItem TrickHeader = new ListViewItem("TRICKS ===============================================================================") { Tag = -1 };
                TempList.Add(TrickHeader);
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
                    TempList.Add(listViewItem);
                }
            }
            listView1.Items.AddRange(TempList.ToArray());

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

            if (ItemStringLogic != null)
            {
                CreateItemString();
                CreateJunkItemString();
                CreateEntranceString();
                CreateStartingItemString();
            }

            var CountRand = logic.Where(x => !x.IsFake && x.RandomizedState() == 0).Count();
            var CountUnRand = logic.Where(x => !x.IsFake && x.RandomizedState() == 1).Count();
            var CountUnRandMan = logic.Where(x => !x.IsFake && x.RandomizedState() == 2).Count();
            var CountJunk = logic.Where(x => !x.IsFake && x.RandomizedState() == 3).Count();
            var CountStarting = logic.Where(x => !x.IsFake && x.isStartingItem()).Count();

            var CountEnabledTricks = logic.Where(x => x.IsFake && x.IsTrick && x.TrickEnabled).Count();
            var CountDisabledTricks = logic.Where(x => x.IsFake && x.IsTrick && !x.TrickEnabled).Count();

            var CountAllLogic = logic.Where(x => !x.IsFake).Count();
            var CountAllTrick = logic.Where(x => x.IsFake && x.IsTrick).Count();

            chkShowRandom.Text = $"Show Randomized ({CountRand})";
            chkShowUnrand.Text = $"Show UnRandomized ({CountUnRand})";
            chkShowUnrandMan.Text = $"Show UnRando (Man) ({CountUnRandMan})";
            chkShowJunk.Text = $"Show Forced Junk ({CountJunk})";
            chkShowStartingItems.Text = $"Show Starting Items ({CountStarting})";
            chkShowEnabledTricks.Text = $"Show Enabled Tricks ({CountEnabledTricks})";
            chkShowDisabledTricks.Text = $"Show Disabled Tricks ({CountDisabledTricks})";
            chkTricks.Text = $"({CountAllTrick})";
            chkRandomState.Text = $"({CountAllLogic})";

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
                if (entry.isStartingItem() && chkShowStartingItems.Checked) { chkValid = true; }

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

        public void ApplyRandomizerSettings(LogicObjects.GameplaySettings Settings)
        {

            //Apply custom item strings
            if (Settings.UseCustomItemList) { txtCustomItemString.Text = Settings.CustomItemListString; }
            if (LogicObjects.MainTrackerInstance.IsEntranceRando())
            {
                txtRandEntString.Text = Settings.RandomizedEntrancesString;
                LogicObjects.MainTrackerInstance.Options.CoupleEntrances = !Settings.DecoupleEntrances;
            }
            txtJunkItemString.Text = Settings.CustomJunkLocationsString;
            txtStartingitemString.Text = Settings.CustomStartingItemListString;
            btnApplyString_Click(null, null);

            LogicObjects.MainTrackerInstance.Options.ProgressiveItems = Settings.ProgressiveUpgrades;
            LogicObjects.MainTrackerInstance.Options.BringYourOwnAmmo = Settings.ByoAmmo;
            LogicObjects.MainTrackerInstance.Options.Keysy["SmallKey"] = false;
            LogicObjects.MainTrackerInstance.Options.Keysy["BossKey"] = false;
            if (Settings.SmallKeyMode.Contains("DoorsOpen"))
            {
                LogicObjects.MainTrackerInstance.Options.Keysy["SmallKey"] = true;
            }
            if (Settings.BossKeyMode.Contains("DoorsOpen"))
            {
                LogicObjects.MainTrackerInstance.Options.Keysy["BossKey"] = true;
            }

            for(var i =0; i < Settings.EnabledTricks.Count(); i++)
            {
                int number2 = -2;
                var numString = Settings.EnabledTricks[i];
                bool canConvert = int.TryParse(numString, out number2);
                if (canConvert && LogicObjects.MainTrackerInstance.ItemInRange(number2))
                {
                    Settings.EnabledTricks[i] = LogicObjects.MainTrackerInstance.Logic[number2].DictionaryName;
                }
            }

            //Apply tricks
            foreach (var i in LogicObjects.MainTrackerInstance.Logic.Where(x => x.IsTrick))
            {
                i.TrickEnabled = Settings.EnabledTricks.Contains(i.DictionaryName);
            }

            //Apply Items Settings
            //The new 1.14 setting always uses custom item list, the UseCustomItemList option is gone so it will always be true
            if (!Settings.UseCustomItemList)
            {
                //The CategoriesRandomized array only exits in 1.14. So if it was found in settings use the 1.14 Category List
                if (Settings.CategoriesRandomized == null)
                {
                    #region OldRandoCategories
                    //Dungeon Items
                    SetRange("Woodfall Map", "Stone Tower Key 4 - death armos maze", Settings.AddDungeonItems);
                    //Shop Items
                    SetRange("Trading Post Red Potion", "Zora Shop Red Potion", Settings.AddShopItems);
                    SetRange("Bomb Bag (20)", "Bomb Bag (20)", Settings.AddShopItems);
                    SetRange("Town Bomb Bag (30)", "Town Bomb Bag (30)", Settings.AddShopItems);
                    SetRange("Milk Bar Chateau", "Milk Bar Milk", Settings.AddShopItems);
                    SetRange("Swamp Scrub Magic Bean", "Canyon Scrub Blue Potion", Settings.AddShopItems);
                    SetRange("Gorman Bros Purchase Milk", "Gorman Bros Purchase Milk", Settings.AddShopItems);
                    //Misc
                    SetRange("Lens Cave 20r", "Ikana Scrub 200r", Settings.AddOther);
                    //Bottle Catch
                    SetRange("Bottle: Fairy", "Bottle: Mushroom", Settings.RandomizeBottleCatchContents);
                    //Moon
                    SetRange("Deku Trial HP", "Link Trial 10 Bombchu", Settings.AddMoonItems);
                    //Fairy Rewards
                    SetRange("Great Fairy Magic Meter", "Great Fairy's Sword", Settings.AddFairyRewards);
                    SetRange("Great Fairy's Mask", "Great Fairy's Mask", Settings.AddFairyRewards);
                    //Pre Clocktown
                    SetRange("Pre-Clocktown 10 Deku Nuts", "Pre-Clocktown 10 Deku Nuts", Settings.AddNutChest);
                    //Starting Items
                    SetRange("Starting Sword", "Starting Heart 2", Settings.CrazyStartingItems);
                    //Cows
                    SetRange("Ranch Cow #1 Milk", "Great Bay Coast Grotto Cow #2 Milk", Settings.AddCowMilk);
                    //Skulls
                    SetRange("Swamp Skulltula Main Room Near Ceiling", "Ocean Skulltula 2nd Room Behind Skull 2", Settings.AddSkulltulaTokens);
                    //Fairies
                    SetRange("Clock Town Stray Fairy", "Stone Tower Lava Room Ledge", Settings.AddStrayFairies);
                    //Mundane
                    SetRange("Lottery 50r", "Seahorse", Settings.AddMundaneRewards);
                    //Preserve Soaring
                    SetRange("Song of Soaring", "Song of Soaring", !Settings.ExcludeSongOfSoaring);
                    #endregion OldRandoCategories
                }
                else //Original 1.14 categories. Never really used expcept in like 2 betas
                {
                    #region Temp114Categories
                    //Categories are now group by item so handle setting by item name.
                    SetRangebyItem("GreenRupees", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Green Rupee"));
                    SetRangebyItem("RedRupees", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Red Rupee"));
                    SetRangebyItem("Ammo", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x =>
                        x.ItemName.Contains("Bombs") ||
                        x.ItemName.Contains("Arrows") ||
                        x.ItemName.Contains("Deku Nuts") ||
                        x.ItemName == "Deku Stick" ||
                        x.ItemName.Contains("Magic Bean")
                    ));
                    SetRangebyItem("Bombchu", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName.Contains("Bombchu")));
                    SetRangebyItem("MagicJars", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Small Magic Jar" || x.ItemName == "Large Magic Jar"));
                    SetRangebyItem("RecoveryHearts", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Recovery Heart"));
                    SetRangebyItem("BlueRupees", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Blue Rupee"));
                    SetRangebyItem("PurpleRupees", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Purple Rupee"));
                    SetRangebyItem("GoldRupees", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Gold Rupee"));
                    SetRangebyItem("SilverRupees", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Silver Rupee"));
                    SetRangebyItem("PiecesOfHeart", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Piece of Heart"));
                    SetRangebyItem("DungeonKeys", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName.Contains("Small Key") || x.ItemName.Contains("Boss Key")));
                    SetRangebyItem("StrayFairies", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Stray Fairy"));
                    SetRangebyItem("SkulltulaTokens", LogicObjects.MainTrackerInstance.Logic.Where(x => x.ItemName != null).Where(x => x.ItemName == "Skulltula Spirit"));
                    //Use the old method for categories that existed in the old version
                    SetRange("Song of Soaring", "Song of Soaring", Settings.CategoriesRandomized.Contains("SongOfSoaring"));
                    SetRange("Starting Sword", "Starting Heart 2", Settings.CategoriesRandomized.Contains("CrazyStartingItems"));
                    SetRange("Seahorse", "Seahorse", Settings.CategoriesRandomized.Contains("Misc"));
                    SetRange("Great Fairy Magic Meter", "Great Fairy's Sword", Settings.CategoriesRandomized.Contains("GreatFairyRewards"));
                    SetRange("Trading Post Red Potion", "Zora Shop Red Potion", Settings.CategoriesRandomized.Contains("ShopItems"));
                    SetRange("Bomb Bag (20)", "Bomb Bag (20)", Settings.CategoriesRandomized.Contains("ShopItems"));
                    SetRange("Town Bomb Bag (30)", "Town Bomb Bag (30)", Settings.CategoriesRandomized.Contains("ShopItems"));
                    SetRange("Milk Bar Chateau", "Milk Bar Milk", Settings.CategoriesRandomized.Contains("ShopItems"));
                    SetRange("Swamp Scrub Magic Bean", "Canyon Scrub Blue Potion", Settings.CategoriesRandomized.Contains("ShopItems"));
                    SetRange("Gorman Bros Purchase Milk", "Gorman Bros Purchase Milk", Settings.CategoriesRandomized.Contains("ShopItems"));
                    SetRange("Ranch Cow #1 Milk", "Great Bay Coast Grotto Cow #2 Milk", Settings.CategoriesRandomized.Contains("CowMilk"));
                    SetRange("Bottle: Fairy", "Bottle: Mushroom", Settings.CategoriesRandomized.Contains("CaughtBottleContents"));
                    //Do Glitch Checks last since their items exist in other categories but we want to override these specific checks
                    SetRange("Deku Palace Out of Bounds Item", "Deku Palace Out of Bounds Item", Settings.CategoriesRandomized.Contains("GlitchesRequired"));
                    SetRange("Pre-Clocktown 10 Deku Nuts", "Pre-Clocktown 10 Deku Nuts", Settings.CategoriesRandomized.Contains("GlitchesRequired"));

                    #endregion Temp114Categories
                }
            }

            //Set Dungeon randomizations
            //The dungeons aren't in the custom item list so the always need to be set based on the setting.
            //Pre LogicNameChanges
            SetRange("Woodfall Temple access", "Woodfall Temple access", Settings.RandomizeDungeonEntrances);
            SetRange("Snowhead Temple access", "Snowhead Temple access", Settings.RandomizeDungeonEntrances);
            SetRange("Great Bay Temple access", "Great Bay Temple access", Settings.RandomizeDungeonEntrances);
            SetRange("Inverted Stone Tower Temple access", "Inverted Stone Tower Temple access", Settings.RandomizeDungeonEntrances);
            //Post Logic Name Changes
            SetRange("AreaWoodFallTempleAccess", "AreaWoodFallTempleAccess", Settings.RandomizeDungeonEntrances);
            SetRange("AreaSnowheadTempleAccess", "AreaSnowheadTempleAccess", Settings.RandomizeDungeonEntrances);
            SetRange("AreaGreatBayTempleAccess", "AreaGreatBayTempleAccess", Settings.RandomizeDungeonEntrances);
            SetRange("AreaInvertedStoneTowerTempleAccess", "AreaInvertedStoneTowerTempleAccess", Settings.RandomizeDungeonEntrances);

            //Junk Starting Items
            if (Settings.NoStartingItems || Settings.StartingItemMode == "None")
            {
                List<string> StartingItems = new List<string>
                {
                    "Starting Sword",
                    "Starting Shield",
                    "Starting Heart 1",
                    "Starting Heart 2",
                    "Deku Mask",
                    "StartingSword",
                    "StartingShield",
                    "StartingHeartContainer1",
                    "StartingHeartContainer2",
                    "MaskDeku",
                };
                if (Settings.AddSongs) { StartingItems.Add("Song of Healing"); StartingItems.Add("SongHealing"); }
                foreach (var i in StartingItems)
                {
                    var item = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == i);
                    if (item == null) { continue; }
                    bool Starting = item.isStartingItem();
                    bool Radnomized = item.Randomized();
                    if (Radnomized) { item.SetJunk(); }
                }

            }

            //If the gossip stone items exist in logic, Enable them only if they are set to contain hints (Not "Default")
            if (LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == "GossipTerminaSouth") != null && LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == "GossipTerminaGossipDrums") != null)
            {
                SetGossipRange("GossipTerminaSouth", "GossipTerminaGossipDrums", Settings.GossipHintStyle != "Default");
            }

            void SetRange(string start, string end, bool Randomized)
            {
                var StartingItem = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == start);
                var EndingItem = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == end);
                if (StartingItem == null || EndingItem == null) { return; }

                for (var i = StartingItem.ID; i <= EndingItem.ID; i++)
                {
                    int O = (Randomized) ? 0 : 1;
                    O = (LogicObjects.MainTrackerInstance.Logic[i].isStartingItem()) ? O + 4 : O;
                    LogicObjects.MainTrackerInstance.Logic[i].Options = O;
                }

            }

            void SetGossipRange(string start, string end, bool Randomized)
            {
                var StartingItem = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == start);
                var EndingItem = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == end);
                if (StartingItem == null || EndingItem == null) { return; }

                for (var i = StartingItem.ID; i <= EndingItem.ID; i++)
                {
                    LogicObjects.MainTrackerInstance.Logic[i].Options = (Randomized) ? 2 : 1;
                }

            }

            void SetRangebyItem(string CategoryName, IEnumerable<LogicObjects.LogicEntry> LogicSet)
            {
                bool SettingActive = Settings.CategoriesRandomized.Contains(CategoryName);
                foreach (var i in LogicSet)
                {
                    int O = (SettingActive) ? 0 : 1;
                    O = (i.isStartingItem()) ? O + 4 : O;
                    i.Options = O;
                }
            }
        }

        private void CreateJunkItemString()
        {
            var ItemLogic = ItemStringLogic.Where(x => !x.IsEntrance()).ToList();
            var ItemGroupCount = (int)Math.Ceiling(ItemLogic.Count / 32.0);

            int[] n = new int[ItemGroupCount];
            string[] ns = new string[ItemGroupCount];
            foreach (var item in ItemLogic.Where(x => x.RandomizedState() == 3))
            {
                var i = ItemLogic.ToList().IndexOf(item);
                int j = i / 32;
                int k = i % 32;
                n[j] |= (int)(1 << k);
                ns[j] = Convert.ToString(n[j], 16);
            }
            txtJunkItemString.Text = string.Join("-", ns.Reverse());
        }

        private void CreateItemString()
        {
            var ItemLogic = ItemStringLogic.Where(x => !x.IsEntrance()).ToList();
            var ItemGroupCount = (int)Math.Ceiling(ItemLogic.Count / 32.0);

            int[] n = new int[ItemGroupCount];
            string[] ns = new string[ItemGroupCount];

            foreach (var item in ItemLogic.Where(x => x.RandomizedState() != 1 && x.RandomizedState() != 2))
            {
                var i = ItemLogic.ToList().IndexOf(item);
                int j = i / 32;
                int k = i % 32;
                try
                {
                    n[j] |= (int)(1 << k);
                    ns[j] = Convert.ToString(n[j], 16);
                }
                catch { }
            }
            txtCustomItemString.Text = string.Join("-", ns.Reverse());
        }

        private void CreateEntranceString()
        {
            var EntranceLogic = ItemStringLogic.Where(x => x.IsEntrance()).ToList();
            var EntranceGroupCount = (int)Math.Ceiling(EntranceLogic.Count / 32.0);

            int[] n = new int[EntranceGroupCount];
            string[] ns = new string[EntranceGroupCount];
            foreach (var item in EntranceLogic.Where(x => x.RandomizedState() == 0))
            {
                var i = EntranceLogic.ToList().IndexOf(item);
                int j = i / 32;
                int k = i % 32;
                try
                {
                    n[j] |= (int)(1 << k);
                    ns[j] = Convert.ToString(n[j], 16);
                }
                catch { }
            }
            txtRandEntString.Text = string.Join("-", ns.Reverse());
        }

        private void CreateStartingItemString()
        {
            var StartingStringLogic = LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsFake && (IsInMMRItemList(x, true) || !LogicObjects.MainTrackerInstance.IsMM())).ToList();

            RemoveDuplicateStartingItems(StartingStringLogic, LogicObjects.MainTrackerInstance);

            var EntranceLogic = StartingStringLogic.Where(x => x.CanBeStartingItem(LogicObjects.MainTrackerInstance)).ToList();
            var EntranceGroupCount = (int)Math.Ceiling(EntranceLogic.Count / 32.0);

            int[] n = new int[EntranceGroupCount];
            string[] ns = new string[EntranceGroupCount];
            foreach (var item in EntranceLogic.Where(x => x.isStartingItem()))
            {
                var i = EntranceLogic.ToList().IndexOf(item);
                int j = i / 32;
                int k = i % 32;
                try
                {
                    n[j] |= (int)(1 << k);
                    ns[j] = Convert.ToString(n[j], 16);
                }
                catch { }
            }
            txtStartingitemString.Text = string.Join("-", ns.Reverse());
        }

        private void RemoveDuplicateStartingItems(List<LogicObjects.LogicEntry> StartingItems, LogicObjects.TrackerInstance Instance)
        {
            if (!Instance.IsMM()) { return; }
            List<string> usedItems = new List<string>();
            for (var i = StartingItems.Count() - 1; i >= 0; i--)
            {
                if (usedItems.Contains(StartingItems[i].ItemName))
                {
                    StartingItems.RemoveAt(i);
                }
                usedItems.Add(StartingItems[i].ItemName);
            }
        }
    }
}
