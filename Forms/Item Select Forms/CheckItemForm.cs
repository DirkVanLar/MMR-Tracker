using Microsoft.VisualBasic;
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
        public string GlobalJunkType = "Junk";
        public List<LogicObjects.LogicEntry> GossipChecksFound = new List<LogicObjects.LogicEntry>();

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
                if (Item.IsGossipStone())
                {
                    GossipStoneCheck(Item);
                    continue;
                }
                if (Item.ItemSubType == "MMRTCountCheck")
                {
                    CountCheck(Item);
                    continue;
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
            if (GossipChecksFound.Count() > 0)
            {
                MarkGossipEntries();
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
                    if (CheckedObject.RandomizedItem == -1 && CheckedObject.SpoilerRandom != -1 && CheckedObject.JunkItemType != "Junk") { CheckedObject.JunkItemType = "Junk"; }
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
                    if (CheckedObject.RandomizedItem == -1 && CheckedObject.SpoilerRandom != -1 && CheckedObject.JunkItemType != "Junk") { CheckedObject.JunkItemType = "Junk"; }
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

        public bool MarkGossipEntries()
        {
            bool ItemStateChanged = false;
            foreach (var CheckedObject in GossipChecksFound)
            {
                if (CheckedObject.ID < -1 || CheckedObject.SpoilerRandom < 0 || CheckedObject.RandomizedItem > -2 || CheckedObject.Checked) { continue; }
                CheckedObject.RandomizedItem = CheckedObject.SpoilerRandom;
                ItemStateChanged = true;
                continue;
            }
            return ItemStateChanged;
        }

        //Manual Checks

        private void CheckItemForm_Load(object sender, EventArgs e)
        {
            PositionItems();
            CreateJunkContextmenu();
            LBItemSelect.Sorted = chkSort.Checked;
            NextManualItem();
        }

        private void CreateJunkContextmenu()
        {
            ContextMenuStrip JunkMenu = new ContextMenuStrip();
            ToolStripItem Junk = JunkMenu.Items.Add("Junk");
            Junk.Click += (sender, e) => { HandleItemJunkType(sender, e, "Junk"); };
            ToolStripItem Rupees = JunkMenu.Items.Add("Rupees");
            Rupees.Click += (sender, e) => { HandleItemJunkType(sender, e, "Rupees"); };
            ToolStripItem Ammo = JunkMenu.Items.Add("Ammo");
            Ammo.Click += (sender, e) => { HandleItemJunkType(sender, e, "Ammo"); };
            ToolStripItem IceTrap = JunkMenu.Items.Add("Ice Trap");
            IceTrap.Click += (sender, e) => { HandleItemJunkType(sender, e, "Ice Trap"); };
            btnJunk.ContextMenuStrip = JunkMenu;
        }

        private void HandleItemJunkType(object sender, EventArgs e, string JunkType)
        {
            GlobalJunkType = JunkType;
            HandleSelectedItem(sender, e);
        }

        public void PositionItems()
        {

            LBItemSelect.Width = this.Width - 24;
            LBItemSelect.Height = this.Height - (LBItemSelect.Location.Y) - 42;

            if (LogicObjects.MainTrackerInstance.Options.IsMultiWorld)
            {
                TXTSearch.Width = LBItemSelect.Width - numericUpDown1.Width - 4;
            }
            else
            {
                label1.Visible = false;
                numericUpDown1.Visible = false;
                TXTSearch.Width = LBItemSelect.Width;
            }

            label2.Location = new Point { X = 4, Y = 6 };
            TXTSearch.Location = new Point { X = 4, Y = 12 + label2.Height };
            LBItemSelect.Location = new Point { X = 4, Y = 18 + label2.Height + TXTSearch.Height };

            btnJunk.Location = new Point { X = 4 + TXTSearch.Width - btnJunk.Width, Y = 4 };
            chkSort.Location = new Point { X = 4 + TXTSearch.Width - chkSort.Width - btnJunk.Width, Y = 6 };

            numericUpDown1.Location = new Point { X = LBItemSelect.Width + 4 - numericUpDown1.Width, Y = TXTSearch.Location.Y };
            label1.Location = new Point { X = numericUpDown1.Location.X, Y = label2.Location.Y };



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
            if (ManualSelect[0].ItemSubType.ToLower().Contains("setting"))
            {
                this.Text = $"Setting: {ManualSelect[0].LocationName ?? ManualSelect[0].DictionaryName}";
            }
            else if (ManualSelect[0].ItemSubType.ToLower().Contains("dungeon"))
            {
                this.Text = $"Dungeon at: {ManualSelect[0].LocationName ?? ManualSelect[0].DictionaryName}";
            }
            else if (ManualSelect[0].IsEntrance())
            {
                this.Text = $"Exit at: {ManualSelect[0].LocationName ?? ManualSelect[0].DictionaryName}";
            }
            else
            {
                this.Text = $"Item at: {ManualSelect[0].LocationName ?? ManualSelect[0].DictionaryName}";
            }
            TXTSearch.Clear();
            TXTSearch.Focus();
            WriteItems(ManualSelect[0]);
        }

        public void WriteItems(LogicObjects.LogicEntry entry)
        {
            LBItemSelect.ItemHeight = Convert.ToInt32(LogicObjects.MainTrackerInstance.Options.FormFont.Size * 1.7);
            var UsedLogic = Instance.Logic;
            var options = Instance.Options;
            LBItemSelect.Items.Clear();
            List<string> Duplicates = new List<string>();
            for (var i = 0; i < UsedLogic.Count; i++)
            {
                var ItemName = UsedLogic[i].ProgressiveItemName(LogicObjects.MainTrackerInstance);
                UsedLogic[i].DisplayName = ItemName;
                if ((!UsedLogic[i].IsFake)
                    && ((UsedLogic[i].GetItemsNewLocation(UsedLogic) == null && !UsedLogic[i].Aquired) || Instance.Options.IsMultiWorld || !options.RemoveObtainedItemsfromList)
                    && (!UsedLogic[i].Unrandomized(2))
                    && !Duplicates.Contains(ItemName)
                    && UsedLogic[i].ItemName != null
                    && Utility.FilterSearch(UsedLogic[i], TXTSearch.Text, UsedLogic[i].DisplayName)
                    && (entry.ItemSubType == UsedLogic[i].ItemSubType || (entry.IsEntrance() && UsedLogic[i].IsEntrance()) || entry.ItemSubType == "ALL"))
                {
                    LBItemSelect.Items.Add(UsedLogic[i]);
                    Duplicates.Add(ItemName);
                }
            }
        }

        private void HandleSelectedItem(object sender, EventArgs e)
        {
            bool ChangesMade;
            if (FullCheck) { ChangesMade = CheckManualItem(sender, e); }
            else { ChangesMade = MarkManualItem(sender, e); }
            if (ChangesMade) { ItemStateChanged = true; }
            GlobalJunkType = "Junk";

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
                if (SelectedItem.ID == -1) { CheckedObject.JunkItemType = GlobalJunkType; }
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
            if (SelectedItem.ID == -1) { CheckedObject.JunkItemType = GlobalJunkType; }
            CheckedObject.RandomizedItem = SelectedItem.ID;
            return true;
        }

        //Functions
        private void CountCheck(LogicObjects.LogicEntry Item)
        {
            if (Item.Checked)
            {
                Item.Checked = false;
                Item.RandomizedItem = -2;
                Item.JunkItemType = "Junk";
                if (string.IsNullOrWhiteSpace(Item.CountCheckData) || (Item.CountCheckData.Count() > 0 && Item.CountCheckData[0] != '$'))
                {
                    Item.CountCheckData = null;
                }
            }
            else if (Item.CountCheckData == null || Item.CountCheckData.Count() < 1)
            {
                Form fontSelect = new Form();
                fontSelect.FormBorderStyle = FormBorderStyle.FixedSingle;
                fontSelect.Text = "Count";
                fontSelect.Width = (220);
                fontSelect.Height = (112);
                try { fontSelect.Icon = Icon.FromHandle((Bitmap.FromFile(@"Recources\Images\Moon.ico") as Bitmap).GetHicon()); } catch { }
                //Font Size lable
                NumericUpDown lbSize = new NumericUpDown();
                lbSize.Value = 1;
                lbSize.Minimum = 1;
                lbSize.DecimalPlaces = 0;
                lbSize.Location = new Point(2, 2);
                lbSize.Width = (200);
                lbSize.Parent = fontSelect;
                fontSelect.Controls.Add(lbSize);
                Button Default = new Button();
                Default.Text = "Set Count";
                Default.Location = new Point(2, 2 + lbSize.Height);
                Default.Width = lbSize.Width;
                Default.Click += (s, ea) =>
                {
                    fontSelect.Close();
                };
                fontSelect.Height = (lbSize.Height *2 + Default.Height + 4 + SystemInformation.CaptionButtonSize.Height);
                fontSelect.Controls.Add(Default);
                fontSelect.ShowDialog();
                Item.Checked = true;
                Item.RandomizedItem = -1;
                Item.CountCheckData = lbSize.Value.ToString();
                Item.JunkItemType = Item.CountCheckData;
            }
            else if (Item.CountCheckData[0] == '$')
            {
                Item.Checked = true;
                Item.RandomizedItem = -1;
                Item.JunkItemType = Item.CountCheckData.Replace("$", "");
            }
            else
            {
                Item.Checked = true;
                Item.RandomizedItem = -1;
                Item.JunkItemType = Item.CountCheckData;
            }
            ItemStateChanged = true;
        }

        private void GossipStoneCheck(LogicObjects.LogicEntry Item)
        {
            if (Item.Checked)
            {
                if (!Item.GossipHint.StartsWith("$")) //Hints set via spoiler log contain a $ at the beggining of the string, don't erase those.
                {
                    Item.GossipHint = "";
                }
            }
            else
            {
                if (Item.GossipHint == "")
                {
                    string input = Interaction.InputBox("Enter Gossip Stone Hint.", "Enter Hint");
                    Item.GossipHint = input;
                }
            }

            Item.Checked = !Item.Checked;
            ItemStateChanged = true;

            if (Item.GossipHint != "" && Item.GossipHint.StartsWith("$") && Item.Checked)
            {
                var GossipLocationToCheck = GetGossipHintReferenceLocation(Item);
                if (GossipLocationToCheck != null) { GossipChecksFound.Add(GossipLocationToCheck); }
            }

        }

        public LogicObjects.LogicEntry GetGossipHintReferenceLocation(LogicObjects.LogicEntry Item)
        {
            var MidMessage = Class_Files.MMR_Code_Reference.Definitions.Gossip.MessageMidSentences.ToArray();
            var StartMessage = Class_Files.MMR_Code_Reference.Definitions.Gossip.MessageStartSentences.ToArray();
            string ParsedHint = Item.GossipHint;
            ParsedHint = ParsedHint.Replace("$", "");
            ParsedHint = ParsedHint.Replace(".", "");
            foreach (var i in MidMessage)
            {
                if (ParsedHint.Contains(i)) { ParsedHint = ParsedHint.Replace(i, "|"); }
            }
            foreach (var i in StartMessage)
            {
                if (ParsedHint.Contains(i)) { ParsedHint = ParsedHint.Replace(i, ""); }
            }

            Console.WriteLine($"------------------------------------------------");
            Console.WriteLine($"Parsing Gossip Hint{ParsedHint}");
            var messageSegments = ParsedHint.Split('|').Select(x => x.Trim()).ToArray();

            if (messageSegments.Count() == 2)
            {
                Console.WriteLine($"Hint Prased as [{messageSegments[0]}]: [{messageSegments[1]}]");
                var PossibleLocation = Instance.LogicDictionary.LogicDictionaryList
                    .Where(x => (x.GossipLocation != null && x.GossipLocation.Contains(messageSegments[0])) || (x.SpoilerLocation != null && x.SpoilerLocation.Contains(messageSegments[0])));
                var PossibleItems = Instance.LogicDictionary.LogicDictionaryList
                    .Where(x => (x.GossipItem != null && x.GossipItem.Contains(messageSegments[1])) || (x.SpoilerItem != null && x.SpoilerItem.Contains(messageSegments[1])));

                //Console.WriteLine($"From Dictionary: Found {PossibleLocation.Count()} Possible Locations and {PossibleLocation.Count()} possible items");

                if (!PossibleLocation.Any())
                {
                    //Console.WriteLine("Cleaning Symbols From Locations...");
                    PossibleLocation = Instance.LogicDictionary.LogicDictionaryList
                    .Where(x => (x.GossipLocation != null && x.GossipLocation.Select(y => y.Replace("#", "")).Contains(messageSegments[0]))
                    || (x.SpoilerLocation != null && x.SpoilerLocation.Select(y => y.Replace("#", "")).Contains(messageSegments[0])));
                    //Console.WriteLine($"Found {PossibleLocation.Count()} Possible Locations after Cleaning");
                }

                if (PossibleLocation.Any() && PossibleItems.Any())
                {
                    //Console.WriteLine($"From Logic: Found {PossibleLocation.Count()} Possible Locations and {PossibleLocation.Count()} possible items");
                    var ValidPossibleLocations = PossibleLocation.Where(x => Instance.DicNameToID.ContainsKey(x.DictionaryName))?
                        .Select(x => Instance.DicNameToID[x.DictionaryName]);
                    var ValidPossibleItems = PossibleItems.Where(x => Instance.DicNameToID.ContainsKey(x.DictionaryName))?
                        .Select(x => Instance.DicNameToID[x.DictionaryName]);


                    //Console.WriteLine($"From Logic: Found {ValidPossibleLocations.Count()} Possible Locations and {ValidPossibleItems.Count()} possible items");
                    foreach (var i in Instance.Logic)
                    {
                        if (ValidPossibleLocations.Contains(i.ID) && i.SpoilerRandom > -1 && ValidPossibleItems.Contains(i.SpoilerRandom) && !i.HasRandomItem(false))
                        {
                            Console.WriteLine($"Valid Location {i.DictionaryName} had valid item {Instance.Logic[i.SpoilerRandom].DictionaryName}");
                            Console.WriteLine($"------------------------------------------------");
                            return i;
                        }
                    }
                    //Console.WriteLine($"No valid Locations Containd a Valid Item");
                }
            }
            Console.WriteLine($"------------------------------------------------");
            return null;
        }

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

        private void CheckItemForm_Shown(object sender, EventArgs e)
        {
            TXTSearch.Clear();
            TXTSearch.Focus();
        }

        private void CheckItemForm_Resize(object sender, EventArgs e)
        {
            PositionItems();
        }

        private void LBItemSelect_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = LogicObjects.MainTrackerInstance.Options.FormFont;
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;
            e.Graphics.DrawString(LBItemSelect.Items[e.Index].ToString(), F, brush, e.Bounds);
            e.DrawFocusRectangle();
        }
    }
}
