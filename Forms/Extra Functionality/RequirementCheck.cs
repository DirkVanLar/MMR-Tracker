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

        private void listBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            var graphics = this.CreateGraphics();
            if (e.Index < 0) { return; }
            e.DrawBackground();

            Font F = e.Font;
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;

            if (listBox2.Items[e.Index] is LogicEditor.RequiementConditional)
            {
                var test = graphics.MeasureString("", F).Width;
                LogicEditor.RequiementConditional entry = listBox2.Items[e.Index] as LogicEditor.RequiementConditional;
                var drawComma = false;
                foreach (var i in entry.ItemIDs)
                {

                    if (drawComma)
                    {
                        e.Graphics.DrawString(",", F, brush, test, e.Bounds.Y, StringFormat.GenericDefault);
                        test += graphics.MeasureString(",", F).Width;
                    }


                    if (i.ItemUseable()) { F = new Font(F.FontFamily, F.Size, FontStyle.Bold); }
                    var Printname = i.DictionaryName;
                    if (i.ItemName != null && !string.IsNullOrWhiteSpace(i.ItemName)) { Printname = i.ItemName; }
                    e.Graphics.DrawString(Printname, F, brush, test, e.Bounds.Y, StringFormat.GenericDefault);
                    test += graphics.MeasureString(Printname, F).Width - graphics.MeasureString(" ", F).Width;
                    F = new Font(F.FontFamily, F.Size, FontStyle.Regular);
                    
                    drawComma = true;
                }
            }
            else
            {
                e.Graphics.DrawString("Error", e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) { return; }
            e.DrawBackground();

            Font F = e.Font;

            if (listBox1.Items[e.Index] is LogicObjects.LogicEntry)
            {
                LogicObjects.LogicEntry entry = listBox1.Items[e.Index] as LogicObjects.LogicEntry;

                if (entry.ItemUseable()) { F = new Font(F.FontFamily, F.Size, FontStyle.Bold); }
                var Printname = entry.DictionaryName;
                if (entry.ItemName != null && !string.IsNullOrWhiteSpace(entry.ItemName)) { Printname = entry.ItemName; }

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                { e.Graphics.DrawString(Printname, F, Brushes.White, e.Bounds, StringFormat.GenericDefault); }
                else
                { e.Graphics.DrawString(Printname, F, Brushes.Black, e.Bounds, StringFormat.GenericDefault); }
            }
            else
            {
                e.Graphics.DrawString("Error", e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();
        }

        private void RequirementCheck_Load(object sender, EventArgs e)
        {
            WriteEntry();
        }

        public void WriteEntry()
        {
            this.Text = $"Requirements for {entry.LocationName ?? entry.DictionaryName}";
            foreach (var i in entry.Required ?? new int[0])
            {
                var ReqEntry = Instance.Logic[i];
                ReqEntry.DisplayName = ReqEntry.DisplayName ?? ReqEntry.DictionaryName;
                listBox1.Items.Add(ReqEntry);
            }

            if (entry.Conditionals != null && entry.Conditionals.Count() > 10000)
            {
                listBox2.Items.Add("To many conditionals to display");
            }
            else
            {

                foreach (var j in entry.Conditionals ?? new int[0][])
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
        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            ND1.Checked = (((entry.AvailableOn >> 0) & 1) == 1);
            ND2.Checked = (((entry.AvailableOn >> 2) & 1) == 1);
            ND3.Checked = (((entry.AvailableOn >> 4) & 1) == 1);
            NN1.Checked = (((entry.AvailableOn >> 1) & 1) == 1);
            NN2.Checked = (((entry.AvailableOn >> 3) & 1) == 1);
            NN3.Checked = (((entry.AvailableOn >> 5) & 1) == 1);
        }
    }
}
