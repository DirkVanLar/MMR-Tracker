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
            if (dialogResult != DialogResult.OK) { LogicObjects.selectedItems = new List<LogicObjects.LogicEntry>(); return; }
            foreach (var item in LBNeededItems.Items)
            {
                var ListItem = item as LogicObjects.ListItem;
                if (ListItem.ID == LogicObjects.CurrentSelectedItem.ID) { return; }
            }
            foreach (var i in LogicObjects.selectedItems)
            {
                LBNeededItems.Items.Add(new LogicObjects.ListItem { DisplayName = i.DisplayName, ID = i.ID });
            }
            LogicObjects.selectedItems = new List<LogicObjects.LogicEntry>();
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
            if (dialogResult != DialogResult.OK) { LogicObjects.selectedItems = new List<LogicObjects.LogicEntry>(); return; }
            foreach (var item in LBIgnoredChecks.Items)
            {
                var ListItem = item as LogicObjects.ListItem;
                if (ListItem.ID == LogicObjects.CurrentSelectedItem.ID) { return; }
            }
            foreach (var i in LogicObjects.selectedItems)
            {
                LBIgnoredChecks.Items.Add(new LogicObjects.ListItem { DisplayName = i.DisplayName, ID = i.ID });
            }
            LogicObjects.selectedItems = new List<LogicObjects.LogicEntry>();
            ItemSelect.Function = 0;
        }

        private void LBIgnoredChecks_DoubleClick(object sender, EventArgs e)
        {
            if (LBIgnoredChecks.SelectedIndex == -1) { return; }
            LBIgnoredChecks.Items.RemoveAt(LBIgnoredChecks.SelectedIndex);
        }

        private void BtnCheckSeed_Click(object sender, EventArgs e)
        {
            var logicCopy = Utility.CloneLogicList(LogicObjects.Logic);
            foreach (var i in logicCopy)
            {
                i.Available = false;
                i.Checked = false;
                i.Aquired = false;
                i.Options = 0;
            }
            if (!Utility.CheckforSpoilerLog(LogicObjects.Logic))
            {
                var file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt;*html)|*.txt;*html");
                if (file == "") { return; }
                LogicEditing.WriteSpoilerLogToLogic(logicCopy, file);
                if (!Utility.CheckforSpoilerLog(logicCopy, true))
                { MessageBox.Show("Not all items have spoiler data. Your results may be incorrect."); }
            }
            else if (!Utility.CheckforSpoilerLog(LogicObjects.Logic, true))
            { MessageBox.Show("Not all items have spoiler data. Your results may be incorrect."); }

            foreach (var entry in logicCopy) { if (entry.SpoilerRandom > -1) { entry.RandomizedItem = entry.SpoilerRandom; } }

            LBResult.Items.Clear();
            List<int> Ignored = new List<int>();
            foreach (var item in LBIgnoredChecks.Items)
            {
                Ignored.Add((item as LogicObjects.ListItem).ID);
            }
            LogicEditing.CheckSeed(logicCopy, true, Ignored);
            List<string> obtainable = new List<string>();
            List<string> unobtainable = new List<string>();
            foreach (var item in LBNeededItems.Items)
            {
                var ListItem = item as LogicObjects.ListItem;
                Console.WriteLine(logicCopy[ListItem.ID].DictionaryName + " " + logicCopy[ListItem.ID].Aquired);
                if (logicCopy[ListItem.ID].Aquired) { obtainable.Add(ListItem.DisplayName); }
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
    }
}
