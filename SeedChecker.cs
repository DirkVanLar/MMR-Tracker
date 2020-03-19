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

namespace MMR_Tracker
{
    public partial class SeedChecker : Form
    {
        public SeedChecker()
        {
            InitializeComponent();
        }

        private void btnAddNeeded_Click(object sender, EventArgs e)
        {
            ItemSelect.Function = 2;
            ItemSelect ItemSelectForm = new ItemSelect(); var dialogResult = ItemSelectForm.ShowDialog();
            if (dialogResult != DialogResult.OK) { LogicObjects.selectedItems = new List<LogicObjects.LogicEntry>(); return; }
            foreach(var item in LBNeededItems.Items)
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

        private void btnAddIgnored_Click(object sender, EventArgs e)
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

        private void btnCheckSeed_Click(object sender, EventArgs e)
        {
            if (!Utility.CheckforSpoilerLog(LogicObjects.Logic)) { MessageBox.Show("You have not imported a spoiler Log"); return; }
            LBResult.Items.Clear();
            List<int> Ignored = new List<int>();
            foreach(var item in LBIgnoredChecks.Items)
            {
                Ignored.Add((item as LogicObjects.ListItem).ID);
            }
            var logicCopy = Utility.CloneLogicList(LogicObjects.Logic);
            LogicEditing.CheckSeed(logicCopy, true, Ignored);
            foreach(var item in LBNeededItems.Items)
            {
                var ListItem = item as LogicObjects.ListItem;
                if (logicCopy[ListItem.ID].Aquired) { LBResult.Items.Add(ListItem.DisplayName + ": Obtainable"); }
                else { LBResult.Items.Add(ListItem.DisplayName + ": Unobtainable"); }
            }
        }
    }
}
