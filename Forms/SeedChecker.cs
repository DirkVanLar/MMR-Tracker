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
                if (ListItem.ID == Tools.CurrentSelectedItem.ID) { return; }
            }
            foreach (var i in Tools.CurrentselectedItems)
            {
                LBNeededItems.Items.Add(new LogicObjects.ListItem { DisplayName = i.DisplayName, ID = i.ID });
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
                if (ListItem.ID == Tools.CurrentSelectedItem.ID) { return; }
            }
            foreach (var i in Tools.CurrentselectedItems)
            {
                LBIgnoredChecks.Items.Add(new LogicObjects.ListItem { DisplayName = i.DisplayName, ID = i.ID });
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
            var logicCopy = Utility.CloneLogicInstance(LogicObjects.MainTrackerInstance);
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
                Ignored.Add((item as LogicObjects.ListItem).ID);
            }
            CheckSeed(logicCopy, true, Ignored);
            List<string> obtainable = new List<string>();
            List<string> unobtainable = new List<string>();
            foreach (var item in LBNeededItems.Items)
            {
                var ListItem = item as LogicObjects.ListItem;
                Console.WriteLine(logicCopy.Logic[ListItem.ID].DictionaryName + " " + logicCopy.Logic[ListItem.ID].Aquired);
                if (logicCopy.Logic[ListItem.ID].Aquired) { obtainable.Add(ListItem.DisplayName); }
                else { unobtainable.Add(ListItem.DisplayName); }
            }
            if (unobtainable.Count > 0)
            {
                LBResult.Items.Add("Unobtainable ==============================");
                foreach (var i in unobtainable) { LBResult.Items.Add(i); }
            }
            if (obtainable.Count > 0)
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
            if (InitialRun) { LogicEditing.ForceFreshCalculation(Instance.Logic); }
            bool recalculate = false;
            foreach (var item in Instance.Logic)
            {
                item.Available = LogicEditing.RequirementsMet(item.Required, Instance.Logic) && LogicEditing.CondtionalsMet(item.Conditionals, Instance.Logic);
                Console.WriteLine($"{item.DictionaryName} Avalable {item.Available}");
                int Special = LogicEditing.SetAreaClear(item, Instance);
                if (Special == 2) { recalculate = true; }

                if (item.Aquired != item.Available && Special == 0 && item.IsFake)
                {
                    item.Aquired = item.Available;
                    recalculate = true;
                }
                if (!item.IsFake && item.RandomizedItem > -1 && item.Available != Instance.Logic[item.RandomizedItem].Aquired && !Ignored.Contains(item.ID))
                {
                    Instance.Logic[item.RandomizedItem].Aquired = item.Available;
                    recalculate = true;
                }
            }
            if (recalculate) { CheckSeed(Instance, false, Ignored); }
        }
    }
}
