using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MMR_Tracker_V2;

namespace MMR_Tracker
{
    public partial class Map : Form
    {
        public FRMTracker MainInterface;
        public Map()
        {
            InitializeComponent();
        }
        public void clockTown_Click(object sender, EventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {
                if (locations.Checked == true)
                {
                    MainInterface.TXTLocSearch.Text = "#North Clock Town|#South Clock Town|#East Clock Town|#West Clock Town"; 
                }
                if (entrances.Checked == true)
                { MainInterface.TXTEntSearch.Text = "#North Clock Town|#South Clock Town|#East Clock Town|#West Clock Town"; }
                if (checkedLocations.Checked == true)
                { MainInterface.TXTCheckedSearch.Text = "#North Clock Town|#South Clock Town|#East Clock Town|#West Clock Town"; }
            }
            else
            {
                if (locations.Checked == true)
                { MainInterface.TXTLocSearch.Text += ",#North Clock Town|#South Clock Town|#East Clock Town|#West Clock Town"; }
                if (entrances.Checked == true)
                { MainInterface.TXTEntSearch.Text += ",#North Clock Town|#South Clock Town|#East Clock Town|#West Clock Town"; }
                if (checkedLocations.Checked == true)
                { MainInterface.TXTCheckedSearch.Text += ",#North Clock Town|#South Clock Town|#East Clock Town|#West Clock Town"; }

            }
        }
    }
}
