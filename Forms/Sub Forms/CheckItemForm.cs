using MMR_Tracker.Class_Files;
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

namespace MMR_Tracker.Forms.Sub_Forms
{
    public partial class CheckItemForm : Form
    {
        public List<LogicObjects.LogicEntry> ManualSelect = new List<LogicObjects.LogicEntry>();
        public List<LogicObjects.LogicEntry> AutoSelect = new List<LogicObjects.LogicEntry>();
        public LogicObjects.TrackerInstance Instance = new LogicObjects.TrackerInstance();
        public ListBox LB = new ListBox();
        public Dictionary<int, bool> ActionDic = new Dictionary<int, bool>();
        public int FromNetPlayer = -1;
        public int SetFunction = 0; //Set Function: 0 = none, 1 = Always Set, 2 = Always Unset
        public bool ItemStateChanged = false;
        public bool KeepChecked = false;
        public bool FullCheck = false;

        public CheckItemForm()
        {
            InitializeComponent();
        }

        public void BeginCheckItem()
        {
            List<LogicObjects.LogicEntry> ToCheck = new List<LogicObjects.LogicEntry>();
            foreach (var lbi in LB.SelectedItems)
            {
                var i = (lbi is LogicObjects.ListItem) ? (lbi as LogicObjects.ListItem).LocationEntry : lbi;
                if (!(i is LogicObjects.LogicEntry)) { continue; }
                var Item = i as LogicObjects.LogicEntry;
                if (SetFunction != 0 && !FullCheck)
                {
                    if (SetFunction == 1 && Item.HasRandomItem(false)) { continue; }
                    if (SetFunction == 2 && !Item.HasRandomItem(false)) { continue; }
                }
                if (!ActionDic.ContainsKey(Item.ID)) { ActionDic.Add(Item.ID, Item.Checked); }
                ToCheck.Add(Item);
            }
            SeperateLists(ToCheck);

            if (AutoSelect.Count() > 0)
            {
                if (FullCheck) { ItemStateChanged = CheckAutoSelectItems(); }
                else { ItemStateChanged = MarkAutoSelectItems(); }
            }
            if (ManualSelect.Count() > 0)
            {
                this.ShowDialog();
            }
        }

        //Automatic Checks

        public bool CheckAutoSelectItems()
        {
            bool ItemStateChanged = false;
            foreach (var CheckedObject in AutoSelect)
            {
                //Check if the items checked status has been changed, most likely by CheckEntrancePair.
                if (ActionDic.ContainsKey(CheckedObject.ID) && ActionDic[CheckedObject.ID] != CheckedObject.Checked && CheckedObject.IsEntrance()) { continue; } 
                if (CheckedObject.ID < -1) { continue; }
                if (CheckedObject.Checked && CheckedObject.RandomizedItem > -2)
                {
                    if (Instance.ItemInRange(CheckedObject.RandomizedItem) && !Tools.SameItemMultipleChecks(CheckedObject.RandomizedItem, Instance) && CheckedObject.ItemBelongsToMe())
                    {
                        Instance.Logic[CheckedObject.RandomizedItem].Aquired = false;
                        LogicEditing.CheckEntrancePair(CheckedObject, Instance, false);
                    }
                    CheckedObject.Checked = false;
                    if (!KeepChecked) { CheckedObject.RandomizedItem = -2; }
                    ItemStateChanged = true;
                    continue;
                }
                if (CheckedObject.SpoilerRandom > -2 || CheckedObject.RandomizedItem > -2 || CheckedObject.RandomizedState() == 2)
                {
                    CheckedObject.Checked = true;
                    if (CheckedObject.RandomizedState() == 2) { CheckedObject.RandomizedItem = CheckedObject.ID; }
                    if (CheckedObject.SpoilerRandom > -2) { CheckedObject.RandomizedItem = CheckedObject.SpoilerRandom; }
                    if (CheckedObject.RandomizedItem < 0)
                    {
                        CheckedObject.RandomizedItem = -1;
                        ItemStateChanged = true;
                        continue;
                    }
                    if (CheckedObject.ItemBelongsToMe())
                    {
                        Instance.Logic[CheckedObject.RandomizedItem].Aquired = true;
                    }
                    Instance.Logic[CheckedObject.RandomizedItem].PlayerData.ItemCameFromPlayer = FromNetPlayer;
                    LogicEditing.CheckEntrancePair(CheckedObject, Instance, true);
                    ItemStateChanged = true;
                    continue;
                }
            }
            return ItemStateChanged;
        }

        public bool MarkAutoSelectItems()
        {
            bool ItemStateChanged = false;
            foreach (var CheckedObject in AutoSelect)
            {
                if (CheckedObject.ID < -1) { continue; }
                if (CheckedObject.RandomizedItem > -2) 
                { 
                    CheckedObject.RandomizedItem = -2;
                    ItemStateChanged = true;
                    continue;
                }
                if (CheckedObject.SpoilerRandom > -2) 
                { 
                    CheckedObject.RandomizedItem = CheckedObject.SpoilerRandom;
                    ItemStateChanged = true;
                    continue;
                }
                if (CheckedObject.RandomizedState() == 2) 
                { 
                    CheckedObject.RandomizedItem = CheckedObject.ID;
                    ItemStateChanged = true;
                    continue;
                }
            }
            return ItemStateChanged;
        }

        //Manual Checks

        private void CheckItemForm_Load(object sender, EventArgs e)
        {
            if (!Instance.Options.IsMultiWorld) { btnJunk.Width = LBItemSelect.Width; numericUpDown1.Visible = false; label1.Visible = false; }
            else { numericUpDown1.Value = Instance.Options.MyPlayerID; }
            LBItemSelect.Sorted = chkSort.Checked;
            NextManualItem();
        }

        public void NextManualItem()
        {
            if (ManualSelect.Count() < 1)
            {
                this.Close();
                return;
            }
            //Check if the items checked status has been changed, most likely by CheckEntrancePair.
            if (ActionDic.ContainsKey(ManualSelect[0].ID) && ActionDic[ManualSelect[0].ID] != ManualSelect[0].Checked && ManualSelect[0].IsEntrance()) 
            {
                ManualSelect.RemoveAt(0);
                NextManualItem();
                return;
            }
            this.Text = $"Select item found at {ManualSelect[0].LocationName ?? ManualSelect[0].DictionaryName}";
            TXTSearch.Clear();
            TXTSearch.Focus();
            WriteItems(ManualSelect[0]);
        }

        public void WriteItems(LogicObjects.LogicEntry entry)
        {
            var UsedLogic = Instance.Logic;
            var options = Instance.Options;
            LBItemSelect.Items.Clear();
            List<string> Duplicates = new List<string>();
            for (var i = 0; i < UsedLogic.Count; i++)
            {
                UsedLogic[i].DisplayName = UsedLogic[i].ItemName ?? UsedLogic[i].DictionaryName;
                if ((!UsedLogic[i].IsFake)
                    && ((UsedLogic[i].GetItemsNewLocation(UsedLogic) == null && !UsedLogic[i].Aquired) || Instance.Options.IsMultiWorld || !options.RemoveObtainedItemsfromList)
                    && (!UsedLogic[i].Unrandomized(2))
                    && !Duplicates.Contains(UsedLogic[i].ItemName)
                    && UsedLogic[i].ItemName != null
                    && Utility.FilterSearch(UsedLogic[i], TXTSearch.Text, UsedLogic[i].DisplayName)
                    && (entry.ItemSubType == UsedLogic[i].ItemSubType || entry.ItemSubType == "ALL"))
                {
                    LBItemSelect.Items.Add(UsedLogic[i]);
                    Duplicates.Add(UsedLogic[i].ItemName);
                }
            }
        }

        private void HandleSelectedItem(object sender, EventArgs e)
        {
            bool ChangesMade;
            if (FullCheck) { ChangesMade = CheckManualItem(sender, e); }
            else { ChangesMade = MarkManualItem(sender, e); }
            if (ChangesMade) { ItemStateChanged = true; }

            ManualSelect.RemoveAt(0);
            NextManualItem();
        }

        private bool CheckManualItem(object sender, EventArgs e)
        {
            var CheckedObject = ManualSelect[0];
            LogicObjects.LogicEntry SelectedItem = new LogicObjects.LogicEntry { ID = -1 };
            if (sender == LBItemSelect)
            {
                if (!(LBItemSelect.SelectedItem is LogicObjects.LogicEntry)) { return false; }
                SelectedItem = LBItemSelect.SelectedItem as LogicObjects.LogicEntry;
            }

            CheckedObject.Checked = true;
            if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { CheckedObject.PlayerData.ItemBelongedToPlayer = (int)numericUpDown1.Value; }
            if (SelectedItem.ID < 0)
            {
                CheckedObject.RandomizedItem = -1;
                return true;
            }
            CheckedObject.RandomizedItem = SelectedItem.ID;
            if (!LogicObjects.MainTrackerInstance.Options.IsMultiWorld || CheckedObject.ItemBelongsToMe())
            {
                Instance.Logic[SelectedItem.ID].Aquired = true;
            }
            LogicEditing.CheckEntrancePair(CheckedObject, Instance, true);
            return true;
        }

        private bool MarkManualItem(object sender, EventArgs e)
        {
            var CheckedObject = ManualSelect[0];
            LogicObjects.LogicEntry SelectedItem = new LogicObjects.LogicEntry { ID = -1 };
            if (sender == LBItemSelect)
            {
                if (!(LBItemSelect.SelectedItem is LogicObjects.LogicEntry)) { return false; }
                SelectedItem = LBItemSelect.SelectedItem as LogicObjects.LogicEntry;
            }
            CheckedObject.RandomizedItem = SelectedItem.ID;
            return true;
        }

        //Functions

        public void SeperateLists(List<LogicObjects.LogicEntry> ToCheck)
        {
            AutoSelect = ToCheck.Where(x => !NeedsDialogBox(x)).ToList();
            ManualSelect = ToCheck.Where(x => NeedsDialogBox(x)).ToList();
        }

        public bool NeedsDialogBox(LogicObjects.LogicEntry entry)
        {
            return !(entry.Checked || entry.SpoilerRandom > -2 || entry.RandomizedItem > -2 || entry.RandomizedState() == 2);
        }

        private void TXTSearch_TextChanged(object sender, EventArgs e)
        {
            WriteItems(ManualSelect[0]);
        }

        private void TXTSearch_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) { TXTSearch.Clear(); }
        }

        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            LBItemSelect.Sorted = chkSort.Checked;
            if (!chkSort.Checked)
            {
                WriteItems(ManualSelect[0]);
            }
        }
    }
}
