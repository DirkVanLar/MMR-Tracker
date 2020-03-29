using MMR_Tracker.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{

    public partial class ItemSelect : Form
    {
        public ItemSelect()
        {
            InitializeComponent();
        }

        public static int Function = 0;
        public static bool Updating = false;
        public static List<int> CheckedItems = new List<int>();
        public static List<LogicObjects.LogicEntry> UsedLogic = new List<LogicObjects.LogicEntry>();
        public bool ItemsReturned = false;

        //Form Items

        private void ItemSelect_Load(object sender, EventArgs e)
        {
            this.ActiveControl = TXTSearch;
            lbCheckItems.Visible = false;
            BTNJunk.Width = LBItemSelect.Width;
            if (Function != 7)
            {
                chkAddSeperate.Visible = false;
                TXTSearch.Width = LBItemSelect.Width;
            }
            RunFunction();
        }

        private void LbCheckItems_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (Updating) { return; }
            var NewItem = lbCheckItems.Items[e.Index] as LogicObjects.LogicEntry;
            if (e.NewValue == CheckState.Checked)
            {
                CheckedItems.Add(NewItem.ID);
            }
            else
            {
                CheckedItems.RemoveAt(CheckedItems.IndexOf(NewItem.ID));
            }
        }

        private void LBItemSelect_DoubleClick(object sender, EventArgs e)
        {
            ReturnItems();
        }

        private void TXTSearch_TextChanged(object sender, EventArgs e)
        {
            RunFunction();
        }

        private void BTNJunk_Click(object sender, EventArgs e)
        {
            ReturnItems();
        }

        //List Populating

        private void ShowUnusedRealAsItem()
        {
            LBItemSelect.Items.Clear();
            List<string> Duplicates = new List<string>();
            for (var i = 0; i < UsedLogic.Count; i++)
            {
                if (!UsedLogic[i].Aquired
                    && (!UsedLogic[i].IsFake)
                    && !Duplicates.Contains(UsedLogic[i].ItemName)
                    && UsedLogic[i].ItemName != null
                    && Utility.FilterSearch(UsedLogic[i], TXTSearch.Text, UsedLogic[i].DisplayName)
                    && (LogicObjects.CurrentSelectedItem.ItemSubType == UsedLogic[i].ItemSubType || LogicObjects.CurrentSelectedItem.ItemSubType == "ALL"))
                {
                    UsedLogic[i].DisplayName = UsedLogic[i].ItemName;
                    LBItemSelect.Items.Add(UsedLogic[i]);
                    Duplicates.Add(UsedLogic[i].ItemName);
                }
            }
        }

        private void ShowRealAsLocation()
        {
            lbCheckItems.Items.Clear();
            for (var i = 0; i < UsedLogic.Count; i++)
            {
                UsedLogic[i].DisplayName = UsedLogic[i].LocationName ?? UsedLogic[i].DictionaryName;
                if (Utility.FilterSearch(UsedLogic[i], TXTSearch.Text, UsedLogic[i].DisplayName) && !UsedLogic[i].IsFake)
                {
                    lbCheckItems.Items.Add(UsedLogic[i]);
                }
            }
        }

        private void ShowAvailableAsLocation()
        {
            LBItemSelect.Sorted = false;
            LBItemSelect.Items.Clear();
            for (var i = 0; i < UsedLogic.Count; i++)
            {
                UsedLogic[i].DisplayName = UsedLogic[i].LocationName ?? UsedLogic[i].DictionaryName;
                if (Utility.FilterSearch(UsedLogic[i], TXTSearch.Text, UsedLogic[i].DisplayName) && UsedLogic[i].Available && !UsedLogic[i].IsFake)
                {
                    LBItemSelect.Items.Add(UsedLogic[i]);
                }
            }
        }

        private void ShowAllAsLocation()
        {
            LBItemSelect.Items.Clear();
            for (var i = 0; i < LogicEditor.LogicList.Count; i++)
            {
                UsedLogic[i].DisplayName = UsedLogic[i].LocationName ?? UsedLogic[i].DictionaryName;
                UsedLogic[i].DisplayName = (LogicEditor.UseSpoilerInDisplay) ? (UsedLogic[i].SpoilerLocation ?? UsedLogic[i].DisplayName) : UsedLogic[i].DisplayName;
                UsedLogic[i].DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? UsedLogic[i].DictionaryName : UsedLogic[i].DisplayName;
                if (Utility.FilterSearch(UsedLogic[i], TXTSearch.Text, UsedLogic[i].DisplayName))
                {
                    LBItemSelect.Items.Add(UsedLogic[i]);
                }
            }
        }

        private void ShowAllAsItem()
        {
            lbCheckItems.Items.Clear();
            for (var i = 0; i < UsedLogic.Count; i++)
            {
                UsedLogic[i].DisplayName = UsedLogic[i].ItemName ?? UsedLogic[i].DictionaryName;
                UsedLogic[i].DisplayName = (LogicEditor.UseSpoilerInDisplay) ? (UsedLogic[i].SpoilerItem ?? UsedLogic[i].DisplayName) : UsedLogic[i].DisplayName;
                UsedLogic[i].DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? UsedLogic[i].DictionaryName : UsedLogic[i].DisplayName;
                if (Utility.FilterSearch(UsedLogic[i], TXTSearch.Text, UsedLogic[i].DisplayName))
                {
                    lbCheckItems.Items.Add(UsedLogic[i]);
                }
            }
        }

        private void ShowAllCheckedItem()
        {
            lbCheckItems.Items.Clear();
            for (var i = 0; i < CheckedItems.Count; i++)
            {
                var item = UsedLogic[CheckedItems[i]];
                item.DisplayName = item.LocationName ?? item.DictionaryName;
                item.DisplayName = (LogicEditor.UseSpoilerInDisplay) ? (item.SpoilerLocation ?? item.DisplayName) : item.DisplayName;
                item.DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? item.DictionaryName : item.DisplayName;
                if (Utility.FilterSearch(item, TXTSearch.Text, item.DisplayName))
                {
                    LBItemSelect.Items.Add(item);
                }
            }
        }

        //Other Functions

        private void ReturnItems()
        {
            ItemsReturned = true;
            if (LBItemSelect.Visible)
            {
                if (!(LBItemSelect.SelectedItem is LogicObjects.LogicEntry)) { return; }
                LogicObjects.CurrentSelectedItem = LBItemSelect.SelectedItem as LogicObjects.LogicEntry;
                CheckedItems = new List<int>();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                foreach (var i in CheckedItems)
                {
                    var item = UsedLogic[i];
                    LogicObjects.selectedItems.Add(item);
                }
                CheckedItems = new List<int>();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void RunFunction()
        {
            Updating = true;
            if (Function == 0) //Main Tracker select Item
            {
                UsedLogic = LogicObjects.Logic;
                BTNJunk.Text = "Junk";
                LBItemSelect.SelectionMode = SelectionMode.One;
                ShowUnusedRealAsItem(); 
                this.Text = "Item at " + LogicObjects.CurrentSelectedItem.LocationName;
                LBItemSelect.Sorted = true;
            }
            if (Function == 1) //Seed Checker ignored Real Location Select
            {
                UsedLogic = LogicObjects.Logic;
                BTNJunk.Text = "Select";
                UseCheckBox();
                ShowRealAsLocation();
                RecheckItems();
                this.Text = "Select a location";
                LBItemSelect.Sorted = true;
            }
            if (Function == 2) //Seed Checker Item Select
            {
                UsedLogic = LogicObjects.Logic;
                BTNJunk.Text = "Select";
                UseCheckBox();
                ShowAllAsItem();
                RecheckItems();
                this.Text = "Select an item";
                LBItemSelect.Sorted = true;
            }
            if (Function == 3) //What Unlocked This? Show Available Real Locations
            {
                UsedLogic = LogicObjects.Logic;
                BTNJunk.Visible = false;
                this.Height = this.Height - BTNJunk.Height;
                LBItemSelect.SelectionMode = SelectionMode.One;
                ShowAvailableAsLocation(); 
                this.Text = "Select a location";
                LBItemSelect.Sorted = true;
            }
            if (Function == 4) //Logic Editor Go to Check
            {
                UsedLogic = LogicEditor.LogicList;
                BTNJunk.Text = "Select";
                LBItemSelect.SelectionMode = SelectionMode.One;
                ShowAllAsLocation(); 
                this.Text = "Select an item";
            }
            if (Function == 5) //Logic Editor Select Items
            {
                UsedLogic = LogicEditor.LogicList;
                BTNJunk.Text = "Select";
                UseCheckBox();
                ShowAllAsItem();
                RecheckItems();
                this.Text = "Select a location";
                LBItemSelect.Sorted = true;
            }
            if (Function == 6) //Logic Editor Go to List Item
            {
                UsedLogic = LogicEditor.LogicList;
                BTNJunk.Text = "Select";
                LBItemSelect.SelectionMode = SelectionMode.One;
                ShowAllCheckedItem();
                this.Text = "Select a location";
                LBItemSelect.Sorted = true;
            }
            if (Function == 7) //Logic Editor Select Items (Conditionals)
            {
                UsedLogic = LogicEditor.LogicList;
                BTNJunk.Text = "Select";
                chkAddSeperate.Checked = LogicEditor.AddCondSeperatly;
                UseCheckBox();
                ShowAllAsItem();
                RecheckItems();
                this.Text = "Select a location";
                LBItemSelect.Sorted = true;
            }
            Updating = false;
        }

        private void UseCheckBox()
        {
            LBItemSelect.Visible = false;
            lbCheckItems.Visible = true;
            lbCheckItems.Location = LBItemSelect.Location;
            lbCheckItems.Height = LBItemSelect.Height;
            lbCheckItems.Width = LBItemSelect.Width;
        }

        private void RecheckItems()
        {
            for (var i = 0; i < lbCheckItems.Items.Count; i++)
            {
                var item = lbCheckItems.Items[i] as LogicObjects.LogicEntry;
                if (CheckedItems.Contains(item.ID)) { lbCheckItems.SetItemChecked(i, true); }
            }
        }

        private void chkAddSeperate_CheckedChanged(object sender, EventArgs e)
        {
            LogicEditor.AddCondSeperatly = chkAddSeperate.Checked;
            Console.WriteLine(LogicEditor.AddCondSeperatly);
        }

        private void ItemSelect_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ItemsReturned) { LogicEditor.AddCondSeperatly = false; }
            Updating = false;
            UsedLogic = new List<LogicObjects.LogicEntry>();
            Function = 0;
            CheckedItems = new List<int>();
            ItemsReturned = false;
        }
    }
}
