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

        private void BtnAddNeeded_Click(object sender, EventArgs e)
        {
            var TempInstance = Utility.CloneTrackerInstance(LogicObjects.MainTrackerInstance);
            MiscMultiItemSelect NeededSelect = new MiscMultiItemSelect();
            NeededSelect.UsedInstance = TempInstance;
            NeededSelect.Display = 2;
            NeededSelect.ListContent = TempInstance.Logic;

            var GameClearEntry = TempInstance.Logic.Find(x => x.DictionaryName == "MMRTGameClear");
            if (GameClearEntry != null)
            {
                GameClearEntry.ItemName = (TempInstance.IsMM()) ? "Defeat Majora" : "Beat the Game";
            }
            else if (LogicObjects.MainTrackerInstance.IsMM())
            {
                Console.WriteLine("Adding MMRTGameClear");
                int GameClearID = PlaythroughGenerator.GetGameClearEntry(NeededSelect.ListContent, LogicObjects.MainTrackerInstance.IsEntranceRando());
                if (GameClearID > -1 && GameClearID < NeededSelect.ListContent.Count()) { NeededSelect.ListContent[GameClearID].ItemName = "Defeat Majora"; }
            }

            if (NeededSelect.ShowDialog() != DialogResult.OK) { return; }
            foreach (var i in NeededSelect.SelectedItems)
            {
                LBNeededItems.Items.Add(new LogicObjects.ListItem { DisplayName = i.ItemName ?? i.DictionaryName, PathID = i.ID });
            }
        }

        private void LBNeededItems_DoubleClick(object sender, EventArgs e)
        {
            if (LBNeededItems.SelectedIndex == -1) { return; }
            LBNeededItems.Items.RemoveAt(LBNeededItems.SelectedIndex);
        }

        private void BtnAddIgnored_Click(object sender, EventArgs e)
        {
            MiscMultiItemSelect NeededSelect = new MiscMultiItemSelect();
            NeededSelect.Display = 1;
            NeededSelect.ListContent = LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsFake && !string.IsNullOrWhiteSpace(x.LocationName)).ToList();
            if (NeededSelect.ShowDialog() != DialogResult.OK) { return; }
            foreach (var i in NeededSelect.SelectedItems)
            {
                LBIgnoredChecks.Items.Add(new LogicObjects.ListItem { DisplayName = i.LocationName ?? i.DictionaryName, PathID = i.ID });
            }
        }

        private void LBIgnoredChecks_DoubleClick(object sender, EventArgs e)
        {
            if (LBIgnoredChecks.SelectedIndex == -1) { return; }
            LBIgnoredChecks.Items.RemoveAt(LBIgnoredChecks.SelectedIndex);
        }

        private void BtnCheckSeed_Click(object sender, EventArgs e)
        {
            var logicCopy = Utility.CloneTrackerInstance(LogicObjects.MainTrackerInstance);
            foreach (var i in logicCopy.Logic)
            {
                i.Available = false;
                i.Checked = false;
                i.Aquired = false;
                i.Options = 0;
            }
            if (!Utility.CheckforSpoilerLog(logicCopy.Logic))
            {
                var file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt;*html)|*.txt;*html");
                if (file == "") { return; }
                LogicEditing.WriteSpoilerLogToLogic(logicCopy, file);
                if (!Utility.CheckforSpoilerLog(logicCopy.Logic, true))
                { MessageBox.Show("Not all items have spoiler data. Your results may be incorrect."); }
            }
            else if (!Utility.CheckforSpoilerLog(logicCopy.Logic, true))
            { MessageBox.Show("Not all items have spoiler data. Your results may be incorrect."); }

            foreach (var entry in logicCopy.Logic) { if (entry.SpoilerRandom > -1) { entry.RandomizedItem = entry.SpoilerRandom; } }

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

        private void BtnClear_Click(object sender, EventArgs e)
        {
            LBIgnoredChecks.Items.Clear();
            LBNeededItems.Items.Clear();
            LBResult.Items.Clear();
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
            var TempInstance = Utility.CloneTrackerInstance(LogicObjects.MainTrackerInstance);
            var GameClearEntry = TempInstance.Logic.Find(x => x.DictionaryName == "MMRTGameClear");
            if (GameClearEntry != null)
            {
                string GameClearName = (LogicObjects.MainTrackerInstance.IsMM()) ? "Defeat Majora" : "Beat the Game";
                LBNeededItems.Items.Add(new LogicObjects.ListItem { DisplayName = GameClearName, PathID = GameClearEntry.ID });
            }
            else if (LogicObjects.MainTrackerInstance.IsMM())
            {
                Console.WriteLine("Adding MMRTGameClear");
                int GamClearID = PlaythroughGenerator.GetGameClearEntry(TempInstance.Logic, LogicObjects.MainTrackerInstance.IsEntranceRando());
                LBNeededItems.Items.Add(new LogicObjects.ListItem { DisplayName = "Defeat Majora", PathID = GamClearID });
            }
        }
    }
}
