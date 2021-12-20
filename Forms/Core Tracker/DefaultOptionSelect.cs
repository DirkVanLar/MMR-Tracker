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

namespace MMR_Tracker.Forms.Core_Tracker
{
    public partial class DefaultOptionSelect : Form
    {
        public DefaultOptionSelect()
        {
            InitializeComponent();
        }

        public LogicObjects.DefaultTrackerOption Options = new LogicObjects.DefaultTrackerOption();
        private bool ValuesUpdating = false;
        private Size ExampleCurrentSize;
        private Point ExampleCurrentLocation;

        private void DefaultOptionSelect_Load(object sender, EventArgs e)
        {
            ValuesUpdating = true;
            chkHorizontal.Checked = Options.HorizontalLayout;
            chkSeperate.Checked = Options.Seperatemarked;
            chkShowTooltips.Checked = Options.ToolTips;
            chkUpdates.Checked = Options.CheckForUpdates;
            chkAdditionalStats.Checked = Options.ShowAdditionalStats;
            cmbMiddle.Text = Options.MiddleClickFunction;
            int counter = 0;
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                cmbFontStyle.Items.Add(font.Name);
                if (font.Name == Options.FormFont.FontFamily.Name) { cmbFontStyle.SelectedIndex = counter; }
                counter++;
            }
            nudFontSize.Value = (decimal)Options.FormFont.Size;
            btnApply.Visible = LogicObjects.MainTrackerInstance.Logic.Any();
            textBox1.Text = "Example";
            textBox1.SendToBack();
            ExampleCurrentSize = textBox1.Size;
            ExampleCurrentLocation = textBox1.Location;
            ValuesUpdating = false;
            UpdatefontExample();
        }

        private void applyOptions(bool ApplyToTracker = false)
        {
            Options.HorizontalLayout = chkHorizontal.Checked;
            Options.Seperatemarked = chkSeperate.Checked;
            Options.ToolTips = chkShowTooltips.Checked;
            Options.CheckForUpdates = chkUpdates.Checked;
            Options.ShowAdditionalStats = chkAdditionalStats.Checked;
            Options.MiddleClickFunction = cmbMiddle.Text;
            Options.FormFont = new Font(familyName: cmbFontStyle.SelectedItem.ToString(), (float)nudFontSize.Value, FontStyle.Regular);
            if (ApplyToTracker)
            {
                LogicObjects.MainTrackerInstance.Options.HorizontalLayout = Options.HorizontalLayout;
                LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom = Options.Seperatemarked;
                LogicObjects.MainTrackerInstance.Options.ShowEntryNameTooltip = Options.ToolTips;
                LogicObjects.MainTrackerInstance.Options.CheckForUpdate = Options.CheckForUpdates;
                LogicObjects.MainTrackerInstance.Options.ShowAdditionalStats = Options.ShowAdditionalStats;
                LogicObjects.MainTrackerInstance.Options.MiddleClickStarNotMark = Options.MiddleClickFunction == "Star";
                LogicObjects.MainTrackerInstance.Options.FormFont = Options.FormFont;
            }
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            applyOptions();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            applyOptions(true);
        }

        private void UpdatefontExample()
        {
            if (ValuesUpdating) { return; }
            textBox1.Font = new Font(familyName: cmbFontStyle.SelectedItem.ToString(), (float)nudFontSize.Value, FontStyle.Regular);
            if (textBox1.Height != ExampleCurrentSize.Height)
            {
                var change = textBox1.Height - ExampleCurrentSize.Height;
                textBox1.Location = new Point(ExampleCurrentLocation.X, ExampleCurrentLocation.Y - (int)(change / 2));
            }
        }

        private void nudFontSize_ValueChanged(object sender, EventArgs e)
        {
            UpdatefontExample();
        }

        private void cmbFontStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatefontExample();
        }
    }
}
