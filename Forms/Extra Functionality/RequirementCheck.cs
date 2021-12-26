using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms.Sub_Forms;
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

namespace MMR_Tracker.Forms.Extra_Functionality
{
    public partial class RequirementCheck : Form
    {
        public RequirementCheck()
        {
            InitializeComponent();
        }

        public LogicObjects.LogicEntry entry;
        public LogicObjects.TrackerInstance Instance;

        public List<LogicObjects.LogicEntry> GoBackList = new List<LogicObjects.LogicEntry>();

        private void listBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) { return; }
            e.DrawBackground();

            Font F = LogicObjects.MainTrackerInstance.Options.FormFont;
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;

            if (listBox2.Items[e.Index] is LogicEditor.RequiementConditional)
            {
                var StringLength = e.Graphics.MeasureString("", F).Width;
                var ActualStringLength = e.Graphics.MeasureString("", F).Width;
                LogicEditor.RequiementConditional entry = listBox2.Items[e.Index] as LogicEditor.RequiementConditional;
                var drawComma = false;
                foreach (var i in entry.ItemIDs)
                {
                    bool EntryAvailable = i.ItemUseable(Instance) && (!i.IsTrick || (i.IsTrick && i.TrickEnabled));
                    F = new Font(F.FontFamily, F.Size, EntryAvailable ? FontStyle.Bold : FontStyle.Regular);

                    if (drawComma)
                    {
                        e.Graphics.DrawString(", ", F, brush, StringLength, e.Bounds.Y, StringFormat.GenericDefault);
                        StringLength += e.Graphics.MeasureString(", ", F).Width;
                        ActualStringLength += e.Graphics.MeasureString(", ", F).Width;
                    }

                    var Printname = i.DictionaryName;
                    if (i.ItemName != null && !string.IsNullOrWhiteSpace(i.ItemName)) { Printname = i.ItemName; }
                    e.Graphics.DrawString(Printname, F, brush, StringLength, e.Bounds.Y, StringFormat.GenericDefault);
                    StringLength += e.Graphics.MeasureString(Printname, F).Width - e.Graphics.MeasureString(" ", F).Width;
                    ActualStringLength += e.Graphics.MeasureString(Printname, F).Width;

                    drawComma = true;
                }

                if (ActualStringLength > listBox2.Width && (int)ActualStringLength + 2 > listBox2.HorizontalExtent) { listBox2.HorizontalExtent = (int)ActualStringLength + 2; }
            }
            else
            {
                e.Graphics.DrawString("Error", e.Font, brush, e.Bounds, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();

        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) { return; }
            e.DrawBackground();

            Font F = LogicObjects.MainTrackerInstance.Options.FormFont;
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;

            if (listBox1.Items[e.Index] is LogicObjects.LogicEntry)
            {
                LogicObjects.LogicEntry entry = listBox1.Items[e.Index] as LogicObjects.LogicEntry;

                bool EntryAvailable = true;
                if (!entry.ItemUseable(Instance)) { EntryAvailable = false; }
                if (entry.IsTrick && !entry.TrickEnabled) { EntryAvailable = false; }

                if (EntryAvailable)  { F = new Font(F.FontFamily, F.Size, FontStyle.Bold); }
                var Printname = entry.DictionaryName;
                if (entry.ItemName != null && !string.IsNullOrWhiteSpace(entry.ItemName)) { Printname = entry.ItemName; }

                e.Graphics.DrawString(Printname, F, brush, e.Bounds, StringFormat.GenericDefault);
            }
            else
            {
                e.Graphics.DrawString("Error", e.Font, brush, e.Bounds, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();
        }

        private void RequirementCheck_Load(object sender, EventArgs e)
        {
            if (Instance.Logic.All(x => x.TimeSetup == 0 && x.AvailableOn == 0)) { HideTimeData(); }
            WriteEntry();
        }

        private void HideTimeData()
        {
            ND1.Visible = false;
            ND2.Visible = false;
            ND3.Visible = false;
            NN1.Visible = false;
            NN2.Visible = false;
            NN3.Visible = false;
            label3.Visible = false;
            SD1.Visible = false;
            SD2.Visible = false;
            SD3.Visible = false;
            SN1.Visible = false;
            SN2.Visible = false;
            SN3.Visible = false;
            label6.Visible = false;

            int buttonOldY = button1.Location.Y;
            button1.Location = new Point { X = button1.Location.X, Y = listBox2.Location.Y + listBox2.Height - button1.Height + 1 };
            button2.Location = new Point { X = button2.Location.X, Y = listBox2.Location.Y + listBox2.Height - button2.Height + 1 };
            int buttonNewY = button1.Location.Y;

            listBox1.Height += (buttonNewY - buttonOldY);
        }

        public void WriteEntry()
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox1.ItemHeight = Convert.ToInt32(LogicObjects.MainTrackerInstance.Options.FormFont.Size * 1.7);
            listBox2.ItemHeight = Convert.ToInt32(LogicObjects.MainTrackerInstance.Options.FormFont.Size * 1.7);
            listBox2.HorizontalExtent = 0;
            this.Text = $"Requirements for {entry.LocationName ?? entry.DictionaryName}";

            var NewEntry = new LogicObjects.LogicEntry() { ID = entry.ID, DictionaryName = entry.DictionaryName, IsFake = entry.IsFake, Price = entry.Price, Required = entry.Required, Conditionals = entry.Conditionals };
            NewEntry = LogicEditing.PerformLogicEdits(NewEntry, Instance)??NewEntry;
            if (NewEntry.LogicWasEdited)
            {
                LogicEditor.CleanLogicEntry(NewEntry, Instance);
            }
            chkShowUnaltered.Visible = NewEntry.LogicWasEdited;

            if (chkShowUnaltered.Checked)
            {
                NewEntry.Required = entry.Required;
                NewEntry.Conditionals = entry.Conditionals;
            }

            foreach (var i in NewEntry.Required ?? new int[0])
            {
                var ReqEntry = Instance.Logic[i];
                ReqEntry.DisplayName = ReqEntry.DisplayName ?? ReqEntry.DictionaryName;
                listBox1.Items.Add(ReqEntry);
            }

            if (NewEntry.Conditionals != null && NewEntry.Conditionals.Count() > 10000)
            {
                listBox2.Items.Add("To many conditionals to display");
            }
            else
            {

                foreach (var j in NewEntry.Conditionals ?? new int[0][])
                {
                    var CondEntry = new LogicEditor.RequiementConditional { ItemIDs = new List<LogicObjects.LogicEntry>() };
                    foreach (var i in j ?? new int[0])
                    {
                        var ReqEntry = Instance.Logic[i];
                        CondEntry.DisplayName = ReqEntry.DisplayName ?? ReqEntry.DictionaryName;
                        CondEntry.ItemIDs.Add(ReqEntry);
                    }
                    listBox2.Items.Add(CondEntry);
                }
            }

            ND1.Checked = (((entry.AvailableOn >> 0) & 1) == 1);
            ND2.Checked = (((entry.AvailableOn >> 2) & 1) == 1);
            ND3.Checked = (((entry.AvailableOn >> 4) & 1) == 1);
            NN1.Checked = (((entry.AvailableOn >> 1) & 1) == 1);
            NN2.Checked = (((entry.AvailableOn >> 3) & 1) == 1);
            NN3.Checked = (((entry.AvailableOn >> 5) & 1) == 1);
            SD1.Checked = (((entry.TimeSetup >> 0) & 1) == 1);
            SD2.Checked = (((entry.TimeSetup >> 2) & 1) == 1);
            SD3.Checked = (((entry.TimeSetup >> 4) & 1) == 1);
            SN1.Checked = (((entry.TimeSetup >> 1) & 1) == 1);
            SN2.Checked = (((entry.TimeSetup >> 3) & 1) == 1);
            SN3.Checked = (((entry.TimeSetup >> 5) & 1) == 1);

        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            ND1.Checked = (((entry.AvailableOn >> 0) & 1) == 1);
            ND2.Checked = (((entry.AvailableOn >> 2) & 1) == 1);
            ND3.Checked = (((entry.AvailableOn >> 4) & 1) == 1);
            NN1.Checked = (((entry.AvailableOn >> 1) & 1) == 1);
            NN2.Checked = (((entry.AvailableOn >> 3) & 1) == 1);
            NN3.Checked = (((entry.AvailableOn >> 5) & 1) == 1);
            SD1.Checked = (((entry.TimeSetup >> 0) & 1) == 1);
            SD2.Checked = (((entry.TimeSetup >> 2) & 1) == 1);
            SD3.Checked = (((entry.TimeSetup >> 4) & 1) == 1);
            SN1.Checked = (((entry.TimeSetup >> 1) & 1) == 1);
            SN2.Checked = (((entry.TimeSetup >> 3) & 1) == 1);
            SN3.Checked = (((entry.TimeSetup >> 5) & 1) == 1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MiscSingleItemSelect Selector = new MiscSingleItemSelect
            {
                Text = "Go to item",
                ListContent = LogicObjects.MainTrackerInstance.Logic,
                Display = 6
            };
            Selector.ShowDialog();
            if (Selector.DialogResult != DialogResult.OK) { return; }
            //GoBackList.Add(currentEntry.ID);
            try
            {
                GoBackList.Add(entry);
                entry = Selector.SelectedObject;
                WriteEntry();
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!GoBackList.Any()) { return; }
            entry = GoBackList[GoBackList.Count() - 1];
            GoBackList.RemoveAt(GoBackList.Count() - 1);
            WriteEntry();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is LogicObjects.LogicEntry)
            {
                GoBackList.Add(entry);
                entry = listBox1.SelectedItem as LogicObjects.LogicEntry;
                WriteEntry();
            }
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem is LogicEditor.RequiementConditional)
            {
                var item = (listBox2.SelectedItem as LogicEditor.RequiementConditional);
                if (item.ItemIDs.Count < 2)
                {
                    GoBackList.Add(entry);
                    entry = item.ItemIDs[0];
                    WriteEntry();
                    return;
                }
                MiscSingleItemSelect Selector = new MiscSingleItemSelect
                {
                    Text = "Go to item",
                    Display = 6,
                    ListContent = item.ItemIDs
                };
                Selector.ShowDialog();
                if (Selector.DialogResult != DialogResult.OK) { return; }
                GoBackList.Add(entry);
                entry = Selector.SelectedObject;
                WriteEntry();
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void chkShowUnaltered_CheckedChanged(object sender, EventArgs e)
        {
            WriteEntry();
        }
    }
}
