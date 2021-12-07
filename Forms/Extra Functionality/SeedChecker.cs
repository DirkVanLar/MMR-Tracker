using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms.Sub_Forms;
using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MMR_Tracker
{
    public partial class SeedChecker : Form
    {
        public SeedChecker()
        {
            InitializeComponent();
        }

        private LogicObjects.TrackerInstance CheckerInstance = null;

        private void BtnAddNeeded_Click(object sender, EventArgs e)
        {
            MiscMultiItemSelect NeededSelect = new MiscMultiItemSelect();
            NeededSelect.UsedInstance = CheckerInstance;
            NeededSelect.Display = 2;
            NeededSelect.ListContent = CheckerInstance.Logic;

            if (NeededSelect.ShowDialog() != DialogResult.OK) { return; }
            foreach (var i in NeededSelect.SelectedItems)
            {
                LBNeededItems.Items.Add(new LogicObjects.ListItem { DisplayName = i.ItemName ?? i.DictionaryName, PathID = i.ID });
            }
            BtnCheckSeed_Click(sender, e);
        }

        private void LBNeededItems_DoubleClick(object sender, EventArgs e)
        {
            if (LBNeededItems.SelectedIndex == -1) { return; }
            LBNeededItems.Items.RemoveAt(LBNeededItems.SelectedIndex);
            BtnCheckSeed_Click(sender, e);
        }

        private void BtnAddIgnored_Click(object sender, EventArgs e)
        {
            MiscMultiItemSelect NeededSelect = new MiscMultiItemSelect();
            NeededSelect.Display = 1;
            NeededSelect.ListContent = CheckerInstance.Logic.Where(x => !x.IsFake && !string.IsNullOrWhiteSpace(x.LocationName)).ToList();
            if (NeededSelect.ShowDialog() != DialogResult.OK) { return; }
            foreach (var i in NeededSelect.SelectedItems)
            {
                LBIgnoredChecks.Items.Add(new LogicObjects.ListItem { DisplayName = i.LocationName ?? i.DictionaryName, PathID = i.ID });
            }
            BtnCheckSeed_Click(sender, e);
        }

        private void LBIgnoredChecks_DoubleClick(object sender, EventArgs e)
        {
            if (LBIgnoredChecks.SelectedIndex == -1) { return; }
            LBIgnoredChecks.Items.RemoveAt(LBIgnoredChecks.SelectedIndex);
            BtnCheckSeed_Click(sender, e);
        }

        private void BtnCheckSeed_Click(object sender, EventArgs e)
        {
            if (!chkShowObtainable.Checked && !chkShowUnobtainable.Checked)
            {
                LBResult.Items.Clear();
                return;
            }

            var logicCopy = Utility.CloneTrackerInstance(CheckerInstance);

            foreach (var entry in logicCopy.Logic)
            {
                entry.Available = false;
                entry.Checked = false;
                entry.Aquired = false;
                if (entry.SpoilerRandom > -1) { entry.RandomizedItem = entry.SpoilerRandom; }//Make the items randomized item its spoiler item, just for consitancy sake
                else if (entry.RandomizedItem > -1) { entry.SpoilerRandom = entry.RandomizedItem; }//If the item doesn't have spoiler data, but does have a randomized item. set it's spoiler data to the randomized item
                else if (entry.Unrandomized(2)) { entry.SpoilerRandom = entry.ID; entry.RandomizedItem = entry.ID; }//If the item doesn't have spoiler data or a randomized item and is unrandomized (manual), set it's spoiler item to it's self 
            }

            LBResult.Items.Clear();
            List<int> Ignored = new List<int>();
            foreach (var item in LBIgnoredChecks.Items)
            {
                Ignored.Add((item as LogicObjects.ListItem).PathID);
            }

            var GameClearEntry = logicCopy.Logic.Find(x => x.DictionaryName == "MMRTGameClear");
            if (GameClearEntry != null)
            {
                GameClearEntry.ItemName = (LogicObjects.MainTrackerInstance.IsMM()) ? "Defeat Majora" : "Beat the Game";
            }
            else if (LogicObjects.MainTrackerInstance.IsMM())
            {
                int GameClearID = PlaythroughGenerator.GetGameClearEntry(logicCopy.Logic, LogicObjects.MainTrackerInstance.IsEntranceRando());
                logicCopy.Logic[GameClearID].ItemName = "Defeat Majora";
            }

            CheckSeed(logicCopy, true, Ignored);
            List<string> obtainable = new List<string>();
            List<string> unobtainable = new List<string>();
            foreach (var item in LBNeededItems.Items)
            {
                bool Spoil = false;
                var ListItem = item as LogicObjects.ListItem;
                var iteminLogic = logicCopy.Logic[ListItem.PathID];
                string ItemName = iteminLogic.ItemName ?? iteminLogic.DictionaryName;
                var ItemsLocation = iteminLogic.GetItemsNewLocation(logicCopy.Logic);
                string LocationFoundAt = (ItemsLocation != null) ? ItemsLocation.LocationName ?? ItemsLocation.DictionaryName : "";
                string DisplayName = (Spoil) ? ItemName + ": " + LocationFoundAt : ItemName;

                Debugging.Log(logicCopy.Logic[ListItem.PathID].DictionaryName + " " + logicCopy.Logic[ListItem.PathID].Aquired);
                if (logicCopy.Logic[ListItem.PathID].Aquired) { obtainable.Add(DisplayName); }
                else { unobtainable.Add(DisplayName); }
            }
            if (unobtainable.Count > 0 && chkShowUnobtainable.Checked)
            {
                LBResult.Items.Add("Unobtainable ==============================");
                foreach (var i in unobtainable) { LBResult.Items.Add(i); }
            }
            if (obtainable.Count > 0 && chkShowObtainable.Checked)
            {
                LBResult.Items.Add("Obtainable ==============================");
                foreach (var i in obtainable) { LBResult.Items.Add(i); }
            }
        }

        public static void CheckSeed(LogicObjects.TrackerInstance Instance, bool InitialRun, List<int> Ignored)
        {
            if (InitialRun) { Instance.RefreshFakeItems(); }
            bool recalculate = false;
            foreach (var item in Instance.Logic)
            {
                item.Available = item.CheckAvailability(Instance);
                if (item.FakeItemStatusChange()) { recalculate = true; }

                if (!item.IsFake && item.RandomizedItem > -1 && item.Available && !Instance.Logic[item.RandomizedItem].Aquired && !Ignored.Contains(item.ID))
                {
                    Instance.Logic[item.RandomizedItem].Aquired = item.Available;
                    recalculate = true;
                }
            }
            if (recalculate) { CheckSeed(Instance, false, Ignored); }
        }

        private void SeedChecker_Load(object sender, EventArgs e)
        {
            CheckerInstance = Utility.CloneTrackerInstance(LogicObjects.MainTrackerInstance);

            if (!Utility.CheckforSpoilerLog(CheckerInstance.Logic))
            {
                var file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt;*html)|*.txt;*html");
                if (file == "") { return; }
                LogicEditing.WriteSpoilerLogToLogic(CheckerInstance, file);
                if (!Utility.CheckforSpoilerLog(CheckerInstance.Logic, true))
                { MessageBox.Show("Not all items have spoiler data. Your results may be incorrect."); }
            }
            else if (!Utility.CheckforSpoilerLog(CheckerInstance.Logic, true, Log: true))
            { MessageBox.Show("Not all items have spoiler data. Your results may be incorrect."); }


            var GameClearEntry = CheckerInstance.Logic.Find(x => x.DictionaryName == "MMRTGameClear");

            int GameclearID = -1;

            if (GameClearEntry != null)
            {
                GameClearEntry.DictionaryName = (CheckerInstance.IsMM()) ? "Defeat Majora" : "Beat the Game";
                LBNeededItems.Items.Add(new LogicObjects.ListItem { DisplayName = GameClearEntry.DictionaryName, PathID = GameClearEntry.ID });
            }
            else if (CheckerInstance.IsMM())
            {
                Console.WriteLine("Adding MMRTGameClear");
                GameclearID = PlaythroughGenerator.GetGameClearEntry(CheckerInstance.Logic, CheckerInstance.IsEntranceRando());
                if (!CheckerInstance.ItemInRange(GameclearID)) { return; }
                CheckerInstance.Logic[GameclearID].DictionaryName = "Defeat Majora";
                LBNeededItems.Items.Add(new LogicObjects.ListItem { DisplayName = "Defeat Majora", PathID = GameclearID });
            }

            listBox2.DataSource = CheckerInstance.Logic.Select(x => x.LocationName ?? x.DictionaryName).ToList();

            GameClearEntry = CheckerInstance.Logic.Find(x => x.DictionaryName == "MMRTGameClear" || x.DictionaryName == "Beat the Game" || x.DictionaryName == "Defeat Majora");

            if (GameClearEntry != null)
            {
                listBox2.SelectedIndex = GameClearEntry.ID;
            }
            else
            {
                listBox2.SelectedIndex = 0;
            }
            WriteSpoilerItemsToBox();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var Playthrough = PlaythroughGenerator.GeneratePlaythrough(CheckerInstance, listBox2.SelectedIndex);
            if (Playthrough.GameClearItem == null)
            {
                MessageBox.Show("The selected Game Clear Item can not be obtained in this seed. A playthrough can not be generated.");
                return;
            }
            PlaythroughGenerator.DisplayPlaythrough(Playthrough.ImportantPlaythrough, Playthrough.PlaythroughInstance, Playthrough.GameClearItem.Check.ID, CheckerInstance.Logic);
        }

        private void spoilerLogLookupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(listBox1.SelectedItem is LogicObjects.ListItem)) { return; }
            var Location = (listBox1.SelectedItem as LogicObjects.ListItem).LocationEntry;
            var Item = (listBox1.SelectedItem as LogicObjects.ListItem).ItemEntry;
            MessageBox.Show($"{Item.ItemName ?? Item.DictionaryName} is found at {Location.LocationName ?? Location.DictionaryName}", $"{ Item.DictionaryName} Item Location: ");

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            for(var i = 0; i < listBox2.Items.Count; i++)
            {
                if (listBox2.Items[i].ToString().ToLower().Contains(textBox2.Text.ToLower()))
                {
                    listBox2.TopIndex = i;
                    break;
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            WriteSpoilerItemsToBox();
        }

        private void WriteSpoilerItemsToBox()
        {
            listBox1.Items.Clear();
            listBox1.Items.Add("Real Items ==========");
            foreach (var i in CheckerInstance.Logic.Where(x => !x.IsFake))
            {
                var SPOILERlOCATION = i.GetItemsSpoilerLocation(CheckerInstance.Logic);

                if (Utility.FilterSearch(i, textBox1.Text, i.ItemName ?? i.DictionaryName))
                {
                    var LBDisplayName = i.ItemName ?? i.DictionaryName;
                    if (CheckerInstance.Logic.Where(x => x.ItemName == i.ItemName).Count() > 1 && !string.IsNullOrWhiteSpace(i.ItemName) && !string.IsNullOrWhiteSpace(i.LocationName))
                    {
                        LBDisplayName = $"{i.ItemName} (From: {i.LocationName})";
                    }
                    if (SPOILERlOCATION != null)
                    {
                        var ListItem = new LogicObjects.ListItem
                        {
                            ItemEntry = i,
                            LocationEntry = SPOILERlOCATION,
                            PathID = SPOILERlOCATION.ID,
                            DisplayName = LBDisplayName
                        };
                        listBox1.Items.Add(ListItem);
                    }
                }
            }
            listBox1.Items.Add("Fake Items ==========");
            foreach (var i in CheckerInstance.Logic.Where(x => x.IsFake))
            {
                if (Utility.FilterSearch(i, textBox1.Text, i.ItemName ?? i.DictionaryName))
                {
                    var ListItem = new LogicObjects.ListItem
                    {
                        ItemEntry = i,
                        LocationEntry = i,
                        PathID = i.ID,
                        DisplayName = i.ItemName ?? i.DictionaryName
                    };
                    listBox1.Items.Add(ListItem);
                }
            }

            if (listBox1.SelectedIndex < 0)
            {
                btnLocationLookup.Enabled = false;
                btnShpereLookup.Enabled = false;
                btnAreaLookup.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!(listBox1.SelectedItem is LogicObjects.ListItem)) { return; }
            var Location = (listBox1.SelectedItem as LogicObjects.ListItem).LocationEntry;
            var Item = (listBox1.SelectedItem as LogicObjects.ListItem).ItemEntry;
            var Playthrough = PlaythroughGenerator.GeneratePlaythrough(CheckerInstance, Location.ID, true);

            var LocationInPlayThrough = Playthrough.Playthrough.Find(x => x.Check.ID == Location.ID);

            if (LocationInPlayThrough == null)
            {
                MessageBox.Show($"{Item.ItemName ?? Item.DictionaryName} Can not be obtained in this seed");
                return;
            }
            MessageBox.Show($"{Item.ItemName ?? Item.DictionaryName} Can be obtained in Sphere {LocationInPlayThrough.SphereNumber}");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnLocationLookup.Enabled = true;
            btnShpereLookup.Enabled = true;
            btnAreaLookup.Enabled = true;

            if (!(listBox1.SelectedItem is LogicObjects.ListItem))
            {
                btnLocationLookup.Enabled = false;
                btnShpereLookup.Enabled = false;
                btnAreaLookup.Enabled = false;
                return; 
            }
            var Item = (listBox1.SelectedItem as LogicObjects.ListItem).ItemEntry;
            btnLocationLookup.Enabled = !Item.IsFake;
            btnAreaLookup.Enabled = !Item.IsFake;

            if (listBox1.SelectedIndex < 0)
            {
                btnLocationLookup.Enabled = false;
                btnShpereLookup.Enabled = false;
                btnAreaLookup.Enabled = false;
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            string[] GameClearNames = new string[] { "Defeat Majora", "Beat the Game", "MMRTGameClear" };
            var GameClearEntry = CheckerInstance.Logic.Find(x => GameClearNames.Contains(x.DictionaryName));
            var StunMajora = CheckerInstance.Logic.Find(x => x.DictionaryName == "MMRTStunMajora");
            var DamageMajora = CheckerInstance.Logic.Find(x => x.DictionaryName == "MMRTDamageMajora");

            var LogictoPrint = new List<LogicObjects.LogicEntry>() { GameClearEntry };
            if (StunMajora != null) { LogictoPrint.Add(StunMajora); }
            if (DamageMajora != null) { LogictoPrint.Add(DamageMajora); }

            string gameclearEntryLogic = "";
            foreach(var q in LogictoPrint)
            {
                gameclearEntryLogic += $"\n{q.DictionaryName}";
                gameclearEntryLogic += "\nRequired:";
                if (q.Required == null) { gameclearEntryLogic += "\n   -None"; }
                else
                {
                    foreach (var i in q.Required)
                    {
                        gameclearEntryLogic += $"\n    -{CheckerInstance.Logic[i].ItemName ?? CheckerInstance.Logic[i].DictionaryName}";
                    }
                }
                gameclearEntryLogic += $"\nConditionals:";
                if (q.Conditionals == null) { gameclearEntryLogic += "\n    -None"; }
                else
                {
                    foreach (var conditional in q.Conditionals ?? new int[0][])
                    {
                        string ConditionalLine = "  -";
                        foreach (var i in conditional)
                        {
                            ConditionalLine += $"{CheckerInstance.Logic[i].ItemName ?? CheckerInstance.Logic[i].DictionaryName},";
                        }
                        gameclearEntryLogic += $"\n    {ConditionalLine}";
                    }
                }
                gameclearEntryLogic += "\n";
            }

            MessageBox.Show("This will generate a playthrough up to the point that you can obtain the item " +
                "selected in the list box below.\n\nThe playthrough is split into sections called \"Spheres\" " +
                "And contains each item you need to obtain, where that item was and what items you would use to check it.\n\n" +
                "The default selection \"Defeat Majora\" is automatically generated by the randomizer, but can be customized by adding an entry called \n\"MMRTGameClear\"" +
                "(No Quotes, Case sensitive)\nto your logic. \n\n The \"Defeat Majora\" entry created by the randomizer does not accurately represent what is actually " +
                "required to defeat majora. It is more meant to represent what a casual player might want for the fight. " +
                "\n\n The current logic for the \"Defeat Majora\" (MMRTGameClear) entry is:\n\n" +
                gameclearEntryLogic);
        }

        private void label7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The seed checker will use the current logic to see if the items in the list below are obtainable.\n\nSelect the \"Show obtainable\" Check box" +
                " to show all the items in the list below that are obtainable, and select the \"Show unobtainable\" Check box to show all the items in the list below that " +
                "are not obtainable.\n\nThis does not give any information about what checks contain what items, so this can be used without spoiling your seed.");
        }

        private void label8_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Add a check to this list to have the seed checker ignore it when checking if an item is obtainable. \n\nFor example, if you add the woodfall great " +
                "fairy reward to this list and that check contains the goron mask, any items in the above list that are in a location that requires the goron mask " +
                "will be marked as unavailable.\n\n This is usefull to check if you can beat a seed without doing a specific check.");
        }

        private void btnAreaLookup_Click(object sender, EventArgs e)
        {
            if (!(listBox1.SelectedItem is LogicObjects.ListItem)) { return; }
            var Location = (listBox1.SelectedItem as LogicObjects.ListItem).LocationEntry;
            var Item = (listBox1.SelectedItem as LogicObjects.ListItem).ItemEntry;
            MessageBox.Show($"{Item.ItemName ?? Item.DictionaryName} is found in {Location.LocationArea ?? "Unknown"}", $"{ Item.DictionaryName} Found at: ");
        }
    }
}
