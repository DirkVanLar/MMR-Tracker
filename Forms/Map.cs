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
        public bool EntrancesInItemBox = !LogicObjects.MainTrackerInstance.Options.entranceRadnoEnabled && LogicObjects.MainTrackerInstance.IsEntranceRando();
        private ContextMenuStrip btnRClick;
        public Dictionary<string, string[]> TLocations = new Dictionary<string, string[]>(){
            {"ClockTown", new string[] {"#North Clock Town", "#South Clock Town", "#East Clock Town", "#West Clock Town" } },
            {"Termina", new string[] {"#Termina"} }
        };
    public Map()
        {
            InitializeComponent();
        }

        /* For adding a new area
         * Create a new String array that contains each area label in the area your want to create
         * Run the ProcessFilters function and pass the String array
         */
        #region Static Function
        public void WriteFilter(string FilterTest, TextBox txt)
        {
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            { txt.Text = FilterTest; }
            else
            { txt.Text += "|" + FilterTest; }
        }

        public void ProcessFilters(string[] Filters)
        {
            string filters = CreateFilter(0, Filters);
            string filtersOnlyEntrance = CreateFilter(1, Filters);
            string filtersOnlyItem = CreateFilter(2, Filters);
            var useFilter = "";
            if (EntrancesInItemBox && entrances.Checked && !locations.Checked) { useFilter = filtersOnlyEntrance; }
            else if (EntrancesInItemBox && !entrances.Checked && locations.Checked) { useFilter = filtersOnlyItem; }
            else { useFilter = filters; }

            if (checkedLocations.Checked) { WriteFilter(filters, MainInterface.TXTCheckedSearch); }
            if (entrances.Checked && EntrancesInItemBox) { WriteFilter(useFilter, MainInterface.TXTLocSearch); }
            else if (entrances.Checked && !EntrancesInItemBox) { WriteFilter(useFilter, MainInterface.TXTEntSearch); }
            if (locations.Checked) { WriteFilter(useFilter, MainInterface.TXTLocSearch); }
        }

        public string CreateFilter(int Both0Entrance1Item2, string[] Filters)
        {
            string filter = "";
            string Seperator = "";
            foreach(var i in Filters)
            {
                string CurFilter = filter;
                if (Both0Entrance1Item2 == 0) { filter += Seperator + i; }
                if (Both0Entrance1Item2 == 1) { filter += Seperator + i + ",@Entrance"; }
                if (Both0Entrance1Item2 == 2) { filter += Seperator + i + ",!@Entrance"; }
                Seperator = "|";
            }
            Console.WriteLine(filter);
            return filter;
        }
        public void CreateMenu(string[] Filters)
        {
            btnRClick = new ContextMenuStrip();
            this.ContextMenuStrip = btnRClick;
            foreach (string i in Filters)
            {
                string[] j = new string[] { i };
                ToolStripItem ContextMenui = btnRClick.Items.Add(i);
                ContextMenui.Click += (sender, e) =>
                {
                    ProcessFilters(j);
                    btnRClick.Hide();
                    btnRClick.Items.Clear();
                };
            }
            //btnRClick.Show(Cursor.Position.X, Cursor.Position.Y);
        }

        #endregion Static Function

        private void ClockTown_MouseClick(object sender, MouseEventArgs e)
        {
        switch (e.Button) {
            case MouseButtons.Left:
                ProcessFilters(TLocations["ClockTown"]);
                break;
            case MouseButtons.Right:
                CreateMenu(TLocations["ClockTown"]);
                break;
            }
        }

        private void Termina_Click(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(TLocations["Termina"]);
                    break;
                case MouseButtons.Right:
                    CreateMenu(TLocations["Termina"]);
                    break;
            }
        }

        private void Ranch_Click(object sender, MouseEventArgs e)
        {
            string[] Filters = new string[]
            {
                "#Milk Road",
                "#Romani Ranch"
            };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void Swamp_Click(object sender, MouseEventArgs e)
        {
            string[] Filters = new string[]
            {
                "#Road to Southern Swamp",
                "#Southern Swamp",
                "#Swamp Skull House",
                "#Deku Palace"
            };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void Woodfall_Click(object sender, MouseEventArgs e)
        {
            string[] Filters = new string[]
            {
                "#Woodfall",
                "#Woodfall Temple",
                "#Woodfall Temple Fairies",
                "#Dungeon Entrance, Woodfall"
            };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void Ikana_Click(object sender, MouseEventArgs e)
        {
            string[] Filters = new string[]
            {
                "#Road to Ikana",
                "#Ikana Canyon",
                "#Secret Shrine",
                "#Beneath the Well",
                "#Ikana Castle",
                "#Ikana Graveyard"
            };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void StoneTower_Click(object sender, MouseEventArgs e)
        {
            string[] Filters = new string[]
            {
                "#Stone Tower",
                "#Stone Tower Temple",
                "#Inverted Stone Tower Temple",
                "#Stone Tower Temple Fairies",
                "#Dungeon Entrance, Stone Tower"
            };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void Mountain_Click(object sender, MouseEventArgs e)
        {
            string[] Filters = new string[]
            {
                "#Path to Mountain Village",
                "#Mountain Village",
                "#Twin Islands",
                "#Goron Village"
            };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void Snowhead_Click(object sender, MouseEventArgs e)
        {
            string[] Filters = new string[]
            {
                "#Path to Snowhead",
                "#Snowhead",
                "#Snowhead Temple",
                "#Snowhead Temple Fairies",
                "#Dungeon Entrance, Snowhead"
            };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void GreatBay_Click(object sender, MouseEventArgs e)
        {
            string[] Filters = new string[]
            {
                "#Great Bay Coast",
                "#Pinnacle Rock",
                "#Ocean Skull House",
                "#Pirate Fortress",
                "#Pirates' Fortress Exterior",
                "#Pirates' Fortress Interior",
                "#Pirates' Fortress Sewer"
            };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void Moon_Click(object sender, MouseEventArgs e)
        {
        string[] Filters = new string[]
        {
            "#The Moon"
        };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void Coast_Click(object sender, MouseEventArgs e)
        {
            string[] Filters = new string[]
            {
                "#Zora Cape",
                "#Zora Hall",
                "#Great Bay Cape",
                "#Great Bay Temple",
                "#Great Bay Temple Fairies",
                "#Dungeon Entrance, Great Bay"
            };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void Misc_Click(object sender, MouseEventArgs e)
        {
            string[] Filters = new string[]
            {
                "#Misc"
            };
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessFilters(Filters);
                    break;
                case MouseButtons.Right:
                    CreateMenu(Filters);
                    break;
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            if (checkedLocations.Checked) { MainInterface.TXTCheckedSearch.Text = ""; }
            if (entrances.Checked && EntrancesInItemBox) { MainInterface.TXTLocSearch.Text = ""; }
            else if (entrances.Checked && !EntrancesInItemBox) { MainInterface.TXTEntSearch.Text = ""; }
            if (locations.Checked) { MainInterface.TXTLocSearch.Text = ""; }
        }

        private void Map_Load(object sender, EventArgs e)
        {
            if (EntrancesInItemBox) { this.entrances.Visible = true; }
            else { this.entrances.Visible = false; }
        }
    }
}
