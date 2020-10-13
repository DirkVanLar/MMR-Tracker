using MMR_Tracker.Class_Files;
using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
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
            ItemSelect.Function = 2;
            ItemSelect ItemSelectForm = new ItemSelect(); var dialogResult = ItemSelectForm.ShowDialog();
            if (dialogResult != DialogResult.OK) { Tools.CurrentselectedItems = new List<LogicObjects.LogicEntry>(); return; }
            foreach (var item in LBNeededItems.Items)
            {
                var ListItem = item as LogicObjects.ListItem;
                if (ListItem.PathID == Tools.CurrentSelectedItem.ID) { return; }
            }
            foreach (var i in Tools.CurrentselectedItems)
            {
                LBNeededItems.Items.Add(new LogicObjects.ListItem { DisplayName = i.DisplayName, PathID = i.ID });
            }
            Tools.CurrentselectedItems = new List<LogicObjects.LogicEntry>();
            ItemSelect.Function = 0;
        }

        private void LBNeededItems_DoubleClick(object sender, EventArgs e)
        {
            if (LBNeededItems.SelectedIndex == -1) { return; }
            LBNeededItems.Items.RemoveAt(LBNeededItems.SelectedIndex);
        }

        private void BtnAddIgnored_Click(object sender, EventArgs e)
        {
            ItemSelect.Function = 1;
            ItemSelect ItemSelectForm = new ItemSelect(); var dialogResult = ItemSelectForm.ShowDialog();
            if (dialogResult != DialogResult.OK) { Tools.CurrentselectedItems = new List<LogicObjects.LogicEntry>(); return; }
            foreach (var item in LBIgnoredChecks.Items)
            {
                var ListItem = item as LogicObjects.ListItem;
                if (ListItem.PathID == Tools.CurrentSelectedItem.ID) { return; }
            }
            foreach (var i in Tools.CurrentselectedItems)
            {
                LBIgnoredChecks.Items.Add(new LogicObjects.ListItem { DisplayName = i.DisplayName, PathID = i.ID });
            }
            Tools.CurrentselectedItems = new List<LogicObjects.LogicEntry>();
            ItemSelect.Function = 0;
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
                if (item.AquireFakeItem()) { recalculate = true; }

                if (!item.IsFake && item.RandomizedItem > -1 && item.Available && !Instance.Logic[item.RandomizedItem].Aquired && !Ignored.Contains(item.ID))
                {
                    Instance.Logic[item.RandomizedItem].Aquired = item.Available;
                    recalculate = true;
                }
            }
            if (recalculate) { CheckSeed(Instance, false, Ignored); }
        }
    }
}
