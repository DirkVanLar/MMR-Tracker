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

        private void DefaultOptionSelect_Load(object sender, EventArgs e)
        {
            chkHorizontal.Checked = Options.HorizontalLayout;
            chkSeperate.Checked = Options.Seperatemarked;
            chkShowTooltips.Checked = Options.ToolTips;
            chkUpdates.Checked = Options.CheckForUpdates;
            cmbMiddle.Text = Options.MiddleClickFunction;
            btnApply.Visible = LogicObjects.MainTrackerInstance.Logic.Any();
        }

        private void applyOptions(bool ApplyToTracker = false)
        {
            Options.HorizontalLayout = chkHorizontal.Checked;
            Options.Seperatemarked = chkSeperate.Checked;
            Options.ToolTips = chkShowTooltips.Checked;
            Options.CheckForUpdates = chkUpdates.Checked;
            Options.MiddleClickFunction = cmbMiddle.Text;
            if (ApplyToTracker)
            {
                LogicObjects.MainTrackerInstance.Options.HorizontalLayout = Options.HorizontalLayout;
                LogicObjects.MainTrackerInstance.Options.MoveMarkedToBottom = Options.Seperatemarked;
                LogicObjects.MainTrackerInstance.Options.ShowEntryNameTooltip = Options.ToolTips;
                LogicObjects.MainTrackerInstance.Options.CheckForUpdate = Options.CheckForUpdates;
                LogicObjects.MainTrackerInstance.Options.MiddleClickStarNotMark = Options.MiddleClickFunction == "Star";
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
    }
}
