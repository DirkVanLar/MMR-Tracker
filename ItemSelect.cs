using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    public partial class ItemSelect : Form
    {
        public ItemSelect()
        {
            InitializeComponent();
        }

        private void ItemSelect_Load(object sender, EventArgs e)
        {
            this.ActiveControl = TXTSearch;
            WriteToItemBox();
        }

        private void WriteToItemBox()
        {
            LBItemSelect.Items.Clear();
            List<string> Duplicates = new List<string>();
            for (var i = 0; i < LogicObjects.Logic.Count; i++)
            {
                if (!LogicObjects.Logic[i].Aquired
                    && !LogicObjects.Logic[i].IsFake
                    && !Duplicates.Contains(LogicObjects.Logic[i].ItemName)
                    && LogicObjects.Logic[i].ItemName != null 
                    && LogicObjects.Logic[i].ItemName.ToLower().Contains(TXTSearch.Text.ToLower())
                    && (LogicObjects.CurrentSelectedItem.ItemSubType == LogicObjects.Logic[i].ItemSubType))
                {
                    LogicObjects.Logic[i].DisplayName = LogicObjects.Logic[i].ItemName;
                    LBItemSelect.Items.Add(LogicObjects.Logic[i]);
                    Duplicates.Add(LogicObjects.Logic[i].ItemName);
                }
            }
        }

        private void LBItemSelect_DoubleClick(object sender, EventArgs e)
        {
            if (LBItemSelect.SelectedItem is LogicObjects.LogicEntry)
            {
                LogicObjects.CurrentSelectedItem = LBItemSelect.SelectedItem as LogicObjects.LogicEntry;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void TXTSearch_TextChanged(object sender, EventArgs e)
        {
            WriteToItemBox();
        }

        private void BTNJunk_Click(object sender, EventArgs e)
        {
            LogicObjects.CurrentSelectedItem = new LogicObjects.LogicEntry();
            LogicObjects.CurrentSelectedItem.ID = -1;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
