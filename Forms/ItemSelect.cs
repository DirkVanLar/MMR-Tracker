using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        public string Title = "";
        public bool HeightSet = false;

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
            if (Function != 8)
            {
                btnDown.Visible = false;
                btnUp.Visible = false;
                TXTSearch.Width = LBItemSelect.Width;
            }
            RunFunction(true);
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
            if (Function == 0)
            {
                ItemsReturned = true;
                Tools.CurrentSelectedItem = new LogicObjects.LogicEntry { ID = -1 };
                if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { Tools.CurrentSelectedItem.PlayerData.ItemBelongedToPlayer = (int)nudForPlayer.Value; }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            ReturnItems();
        }

        private void lbCheckItems_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (Updating) { return; }
            var NewItem = e.Item.Tag as LogicObjects.LogicEntry;
            if (e.Item.Checked)
            {
                CheckedItems.Add(NewItem.ID);
            }
            else
            {
                CheckedItems.RemoveAt(CheckedItems.IndexOf(NewItem.ID));
            }
        }

        //List Populating

        private void ShowUnusedRealAsItem()
        {
            var options = LogicObjects.MainTrackerInstance.Options;
            LBItemSelect.Items.Clear();
            List<string> Duplicates = new List<string>();
            for (var i = 0; i < UsedLogic.Count; i++)
            {
                UsedLogic[i].DisplayName = UsedLogic[i].ItemName ?? UsedLogic[i].DictionaryName;
                if ((!UsedLogic[i].IsFake)
                    && ((UsedLogic[i].GetItemsNewLocation(UsedLogic) == null && !UsedLogic[i].Aquired) || LogicObjects.MainTrackerInstance.Options.IsMultiWorld || !options.RemoveObtainedItemsfromList)
                    && (!UsedLogic[i].Unrandomized(2))
                    && !Duplicates.Contains(UsedLogic[i].ItemName)
                    && UsedLogic[i].ItemName != null
                    && Utility.FilterSearch(UsedLogic[i], TXTSearch.Text, UsedLogic[i].DisplayName)
                    && (Tools.CurrentSelectedItem.ItemSubType == UsedLogic[i].ItemSubType || Tools.CurrentSelectedItem.ItemSubType == "ALL"))
                {
                    LBItemSelect.Items.Add(UsedLogic[i]);
                    Duplicates.Add(UsedLogic[i].ItemName);
                }
            }
        }

        private void ShowAllAsItemSelectOne()
        {
            LBItemSelect.Items.Clear();
            for (var i = 0; i < UsedLogic.Count; i++)
            {
                UsedLogic[i].DisplayName = UsedLogic[i].ItemName ?? UsedLogic[i].DictionaryName;
                if (Utility.FilterSearch(UsedLogic[i], TXTSearch.Text, UsedLogic[i].DisplayName))
                {
                    LBItemSelect.Items.Add(UsedLogic[i]);
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
                    ListViewItem item = new ListViewItem();
                    item.Text = UsedLogic[i].ToString();
                    item.Tag = UsedLogic[i];
                    lbCheckItems.Items.Add(item);
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
            for (var i = 0; i < LogicEditor.EditorInstance.Logic.Count; i++)
            {
                UsedLogic[i].DisplayName = UsedLogic[i].LocationName ?? UsedLogic[i].DictionaryName;
                UsedLogic[i].DisplayName = (LogicEditor.UseSpoilerInDisplay) ? (UsedLogic[i].SpoilerLocation[0] ?? UsedLogic[i].DisplayName) : UsedLogic[i].DisplayName;
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
                UsedLogic[i].DisplayName = (LogicEditor.UseSpoilerInDisplay) ? (UsedLogic[i].SpoilerItem[0] ?? UsedLogic[i].DisplayName) : UsedLogic[i].DisplayName;
                UsedLogic[i].DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? UsedLogic[i].DictionaryName : UsedLogic[i].DisplayName;
                if (Utility.FilterSearch(UsedLogic[i], TXTSearch.Text, UsedLogic[i].DisplayName))
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = UsedLogic[i].ToString();
                    item.Tag = UsedLogic[i];
                    lbCheckItems.Items.Add(item);
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
                item.DisplayName = (LogicEditor.UseSpoilerInDisplay) ? (item.SpoilerLocation[0] ?? item.DisplayName) : item.DisplayName;
                item.DisplayName = (LogicEditor.UseDictionaryNameInSearch) ? item.DictionaryName : item.DisplayName;
                if (Utility.FilterSearch(item, TXTSearch.Text, item.DisplayName))
                {
                    LBItemSelect.Items.Add(item);
                }
            }
        }

        private void ShowAllAsDictionary()
        {
            lbCheckItems.Items.Clear();
            for (var i = 0; i < UsedLogic.Count; i++)
            {
                if (!LogicEditor.EditorInstance.IsMM() || UsedLogic[i].UserCreatedFakeItem(UsedLogic))
                {
                    UsedLogic[i].DisplayName = UsedLogic[i].DictionaryName;
                    LBItemSelect.Items.Add(UsedLogic[i]);
                }
            }
        }

        private void ShowAllSpoilerAsItem()
        {
            LBItemSelect.Items.Clear();
            foreach(var i in UsedLogic.Where(x => x.SpoilerRandom > -1))
            {
                var spoilerDisplay = new LogicObjects.LogicEntry { ID = i.ID, DisplayName = UsedLogic[i.SpoilerRandom].ItemName?? UsedLogic[i.SpoilerRandom].DictionaryName };
                if (spoilerDisplay.DisplayName.ToLower().Contains(TXTSearch.Text.ToLower()))
                {
                    LBItemSelect.Items.Add(spoilerDisplay);
                }
                
            }
        }

        //Other Functions

        private void ReturnItems()
        {
            ItemsReturned = true;
            if (Function == 8)
            {
                foreach (LogicObjects.LogicEntry i in UsedLogic)
                {
                    if (!LogicEditor.EditorInstance.IsMM() || i.UserCreatedFakeItem(UsedLogic)) { break; }
                    Tools.CurrentselectedItems.Add(i);

                }
                foreach (LogicObjects.LogicEntry i in LBItemSelect.Items)
                {
                    Tools.CurrentselectedItems.Add(i);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (Function == 10)
            {
                if (!(LBItemSelect.SelectedItem is LogicObjects.LogicEntry)) { return; }
                var item = (LBItemSelect.SelectedItem as LogicObjects.LogicEntry).DisplayName;
                var Location = UsedLogic[(LBItemSelect.SelectedItem as LogicObjects.LogicEntry).ID].LocationName;
                MessageBox.Show($"{item} is found at {Location}");
            }
            else if (LBItemSelect.Visible)
            {
                if (!(LBItemSelect.SelectedItem is LogicObjects.LogicEntry)) { return; }
                Tools.CurrentSelectedItem = LBItemSelect.SelectedItem as LogicObjects.LogicEntry;
                if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { Tools.CurrentSelectedItem.PlayerData.ItemBelongedToPlayer = (int)nudForPlayer.Value; }
                CheckedItems = new List<int>();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                foreach (var i in CheckedItems)
                {
                    var item = UsedLogic[i];
                    Tools.CurrentselectedItems.Add(item);
                }
                CheckedItems = new List<int>();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void RunFunction(bool FormLoad = false)
        {
            Updating = true;

            switch (Function)
            {
                case 0:
                    UsedLogic = LogicObjects.MainTrackerInstance.Logic;
                    BTNJunk.Text = "Junk";
                    LBItemSelect.SelectionMode = SelectionMode.One;
                    ShowUnusedRealAsItem();
                    if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { SelectPlayer(); if (FormLoad) { nudForPlayer.Value = LogicObjects.MainTrackerInstance.Options.MyPlayerID; } };
                    this.Text = "Item at " + Tools.CurrentSelectedItem.LocationName;
                    LBItemSelect.Sorted = true;
                    break;
                case 1:
                    UsedLogic = LogicObjects.MainTrackerInstance.Logic;
                    BTNJunk.Text = "Select";
                    UseCheckBox();
                    ShowRealAsLocation();
                    RecheckItems();
                    this.Text = "Select a location";
                    LBItemSelect.Sorted = true;
                    break;
                case 2:
                    UsedLogic = LogicObjects.MainTrackerInstance.Logic;
                    BTNJunk.Text = "Select";
                    UseCheckBox();
                    ShowAllAsItem();
                    RecheckItems();
                    this.Text = "Select an item";
                    LBItemSelect.Sorted = true;
                    break;
                case 3:
                    UsedLogic = LogicObjects.MainTrackerInstance.Logic;
                    BTNJunk.Visible = false;
                    if (!HeightSet)
                    {
                        this.Height = this.Height - BTNJunk.Height;
                        HeightSet = true;
                    }
                    LBItemSelect.SelectionMode = SelectionMode.One;
                    ShowAvailableAsLocation();
                    this.Text = "Select a location";
                    LBItemSelect.Sorted = true;
                    break;
                case 4:
                    UsedLogic = LogicEditor.EditorInstance.Logic;
                    BTNJunk.Text = "Select";
                    LBItemSelect.SelectionMode = SelectionMode.One;
                    ShowAllAsLocation();
                    this.Text = "Select an item";
                    break;
                case 5:
                    UsedLogic = LogicEditor.EditorInstance.Logic;
                    BTNJunk.Text = "Select";
                    UseCheckBox();
                    ShowAllAsItem();
                    RecheckItems();
                    this.Text = "Select a location";
                    LBItemSelect.Sorted = true;
                    break;
                case 6:
                    UsedLogic = LogicEditor.EditorInstance.Logic;
                    BTNJunk.Text = "Select";
                    LBItemSelect.SelectionMode = SelectionMode.One;
                    ShowAllCheckedItem();
                    this.Text = "Select a location";
                    LBItemSelect.Sorted = true;
                    break;
                case 7:
                    UsedLogic = LogicEditor.EditorInstance.Logic;
                    BTNJunk.Text = "Select";
                    chkAddSeperate.Checked = LogicEditor.AddCondSeperatly;
                    UseCheckBox();
                    ShowAllAsItem();
                    RecheckItems();
                    this.Text = "Select a location";
                    LBItemSelect.Sorted = true;
                    break;
                case 8:
                    UsedLogic = LogicEditor.EditorInstance.Logic;
                    BTNJunk.Text = "Apply";
                    LBItemSelect.SelectionMode = SelectionMode.MultiExtended;
                    UseUpDown();
                    ShowAllAsDictionary();
                    this.Text = "Move an item to reorder it in the logic file";
                    break;
                case 9:
                    UsedLogic = LogicEditor.EditorInstance.Logic;
                    BTNJunk.Text = "Select";
                    LBItemSelect.SelectionMode = SelectionMode.One;
                    ShowAllCheckedItem();
                    this.Text = Title;
                    LBItemSelect.Sorted = true;
                    break;
                case 10:
                    UsedLogic = LogicObjects.MainTrackerInstance.Logic;
                    BTNJunk.Visible = false;
                    if (!HeightSet)
                    {
                        this.Height = this.Height - BTNJunk.Height;
                        HeightSet = true;
                    }
                    LBItemSelect.SelectionMode = SelectionMode.One;
                    ShowAllSpoilerAsItem();
                    this.Text = "Select an item to see it's location";
                    LBItemSelect.Sorted = true;
                    break;
                case 11:
                    UsedLogic = LogicEditor.EditorInstance.Logic;
                    BTNJunk.Text = "Select";
                    UseCheckBox();
                    ShowAllAsItem();
                    RecheckItems();
                    this.Text = "Select a pool of Items";
                    LBItemSelect.Sorted = true;
                    break;
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
            if (!HeightSet)
            {
                TXTSearch.Width = TXTSearch.Width - chkAddSeperate.Width - 10;
                HeightSet = true;
            }
        }

        private void SelectPlayer()
        {
            if (!HeightSet)
            {
                TXTSearch.Width = TXTSearch.Width - label1.Width - nudForPlayer.Width - 8;
                HeightSet = true;
            }
            label1.Visible = true;
            nudForPlayer.Visible = true;
        }

        private void UseUpDown()
        {
            TXTSearch.Visible = false;
            btnUp.Location = new Point(TXTSearch.Location.X, TXTSearch.Location.Y);
            btnUp.Height = TXTSearch.Height;
            btnUp.Width = (TXTSearch.Width / 2);
            btnDown.Location = new Point((TXTSearch.Location.X + TXTSearch.Width / 2), TXTSearch.Location.Y);
            btnDown.Height = TXTSearch.Height;
            btnDown.Width = (TXTSearch.Width / 2);
        }

        private void RecheckItems()
        {
            for (var i = 0; i < lbCheckItems.Items.Count; i++)
            {
                var item = lbCheckItems.Items[i].Tag as LogicObjects.LogicEntry;
                if (CheckedItems.Contains(item.ID)) { lbCheckItems.Items[i].Checked = true; }
            }
        }

        private void chkAddSeperate_CheckedChanged(object sender, EventArgs e)
        {
            LogicEditor.AddCondSeperatly = chkAddSeperate.Checked;
            Debugging.Log(LogicEditor.AddCondSeperatly.ToString());
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

        private void UpDownButtons(object sender, EventArgs e)
        {
            if (sender == btnUp) { MoveItem(-1); }
            else if (sender == btnDown) { MoveItem(1); }
        }

        public void MoveItem(int Direction)
        {
            ListBox LB = LBItemSelect;
            bool up = Direction < 0;
            int Decrementor = Direction * -1;
            if ((Control.ModifierKeys & Keys.Shift) != 0) { Direction *= 5; }
            if ((Control.ModifierKeys & Keys.Control) != 0) { Direction *= 10; }

            bool CanMove = TestMove();

            while (!CanMove)
            {
                Direction += Decrementor;
                if (Direction == 0) { return; }
                CanMove = TestMove();
            }

            if (up) { for (var i = 0; i < LB.SelectedIndices.Count; i++)      { MoveItem(i); } }
            else    { for (var i = LB.SelectedIndices.Count - 1; i >= 0; i--) { MoveItem(i); } }
            void MoveItem(int ind)
            {
                int newIndex = LB.SelectedIndices[ind] + Direction;
                object selected = LB.SelectedItems[ind];
                LB.Items.Remove(selected);
                LB.Items.Insert(newIndex, selected);
                LB.SetSelected(newIndex, true);
            }

            bool inBounds(int ind)
            {
                if (LB.SelectedItems[ind] == null || LB.SelectedIndices[ind] < 0) { return false; }
                int newIndex = LB.SelectedIndices[ind] + Direction;
                if (newIndex < 0 || newIndex >= LB.Items.Count) { return false; }
                return true;
            }
            bool TestMove()
            {
                if (up)  { for (var i = 0; i < LB.SelectedIndices.Count; i++)      { if (!inBounds(i)) { return false; } } }
                else     { for (var i = LB.SelectedIndices.Count - 1; i >= 0; i--) { if (!inBounds(i)) { return false; } } }
                return true;
            }
        }

        private void TXTSearch_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { TXTSearch.Clear(); }
        }
    }
}
