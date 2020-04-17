using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MMR_Tracker_V2;

namespace MMR_Tracker
{
    
    public partial class Map : Form
    {
        public Map()
        {
            InitializeComponent();
        }

        public FRMTracker MainInterface;
        private ContextMenuStrip btnRClick;
        public Dictionary<string, string[]> LocationDic = new Dictionary<string, string[]>(){
            {"ClockTown", new string[] {"#North Clock Town", "#South Clock Town", "#East Clock Town", "#West Clock Town", "#Stock Pot Inn", "#Laundry Pool", "#Beneath Clocktown" } },
            {"Termina", new string[] { "#Termina Field", "#Astral Observatory" } },
            {"Ranch", new string[] { "#Milk Road", "#Romani Ranch" } },
            {"Swamp", new string[] { "#Road to Southern Swamp", "#Southern Swamp", "#Swamp Skull House", "#Deku Palace" } },
            {"Woodfall",new string[] { "#Woodfall", "#Woodfall Temple", "#Woodfall Temple Fairies", "#Dungeon Entrance, Woodfall" } },
            {"Ikana",new string[]{ "#Road to Ikana", "#Ikana Canyon", "#Secret Shrine", "#Beneath the Well", "#Ikana Castle", "#Ikana Graveyard" } },
            {"StoneTower",new string[]{ "#Stone Tower", "#Stone Tower Temple", "#Inverted Stone Tower Temple", "#Stone Tower Temple Fairies", "#Dungeon Entrance, Stone Tower" } },
            {"Mountain",new string[]{ "#Path to Mountain Village", "#Mountain Village", "#Twin Islands", "#Goron Village" } },
            {"Snowhead",new string[]{ "#Path to Snowhead", "#Snowhead", "#Snowhead Temple", "#Snowhead Temple Fairies", "#Dungeon Entrance, Snowhead" } },
            {"GreatBay",new string[]{ "#Great Bay Coast", "#Pinnacle Rock", "#Ocean Skull House", "#Pirate Fortress", "#Pirates' Fortress Exterior", "#Pirates' Fortress Interior", "#Pirates' Fortress Sewer" } },
            {"Moon",new string[]{ "#The Moon" } },
            {"Coast",new string[]{ "#Zora Cape", "#Zora Hall", "#Great Bay Cape", "#Great Bay Temple", "#Great Bay Temple Fairies", "#Dungeon Entrance, Great Bay" } },
            {"Misc",new string[]{ "#Misc" } }
        };

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
            bool EntrancesInItemBox = !LogicObjects.MainTrackerInstance.Options.entranceRadnoEnabled && LogicObjects.MainTrackerInstance.IsEntranceRando();
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
                if (Both0Entrance1Item2 == 0) { filter += Seperator + "=" + i; }
                if (Both0Entrance1Item2 == 1) { filter += Seperator + "=" + i + ",@Entrance"; }
                if (Both0Entrance1Item2 == 2) { filter += Seperator + "=" + i + ",!@Entrance"; }
                Seperator = "|";
            }
            Console.WriteLine(filter);
            return filter;
        }

        public ContextMenuStrip CreateMenu(string[] Filters, bool ClearMe = false, bool Return = false)
        {
            btnRClick = new ContextMenuStrip();
            this.ContextMenuStrip = btnRClick;
            if (Return) { return btnRClick; }
            if (ClearMe)
            {
                for(var i = 0; i < Filters.Length; i++)
                {
                    if (!LogicObjects.MainTrackerInstance.IsEntranceRando() && i == 1) { continue; }
                    ToolStripItem ContextMenui = btnRClick.Items.Add(Filters[i]);
                    bool EntrancesInItemBox = !LogicObjects.MainTrackerInstance.Options.entranceRadnoEnabled && LogicObjects.MainTrackerInstance.IsEntranceRando();
                    if (i == 2) { ContextMenui.Click += (sender, e) => { MainInterface.TXTCheckedSearch.Text = ""; }; }
                    if (i == 1 && EntrancesInItemBox) { ContextMenui.Click += (sender, e) => { MainInterface.TXTLocSearch.Text = ""; }; }
                    else if (i == 1 && !EntrancesInItemBox) { ContextMenui.Click += (sender, e) => { MainInterface.TXTEntSearch.Text = ""; }; }
                    if (i == 0) { ContextMenui.Click += (sender, e) => { MainInterface.TXTLocSearch.Text = ""; }; }
                }
            }
            else
            {
                foreach (string i in Filters)
                {
                    string[] j = new string[] { i };
                    ToolStripItem ContextMenui = btnRClick.Items.Add(i.Replace("#", ""));
                    ContextMenui.Click += (sender, e) => { ProcessFilters(new string[] { i }); };
                }
            }
            return btnRClick;
        }

        #endregion Static Function

        private void ClockTown_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["ClockTown"]);
        }

        private void Termina_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["Termina"]);
        }

        private void Ranch_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["Ranch"]);
        }

        private void Swamp_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["Swamp"]);
        }

        private void Woodfall_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["Woodfall"]);
        }

        private void Ikana_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["Ikana"]);
        }

        private void StoneTower_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["StoneTower"]);
        }

        private void Mountain_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["Mountain"]);
        }

        private void Snowhead_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["Snowhead"]);
        }

        private void GreatBay_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["GreatBay"]);
        }

        private void Moon_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["Moon"]);
        }

        private void Coast_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["Coast"]);
        }

        private void Misc_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic["Misc"]);
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            bool EntrancesInItemBox = !LogicObjects.MainTrackerInstance.Options.entranceRadnoEnabled && LogicObjects.MainTrackerInstance.IsEntranceRando();
            if (checkedLocations.Checked) { MainInterface.TXTCheckedSearch.Text = ""; }
            if (entrances.Checked && EntrancesInItemBox) { MainInterface.TXTLocSearch.Text = ""; }
            else if (entrances.Checked && !EntrancesInItemBox) { MainInterface.TXTEntSearch.Text = ""; }
            if (locations.Checked) { MainInterface.TXTLocSearch.Text = ""; }
        }

        private void Map_Load(object sender, EventArgs e)
        {
            AddExtraToMisc();
            foreach (var i in LocationDic)
            {
                try 
                { 
                    string name = i.Key;
                    Button btn = this.Controls.Find(name, false).FirstOrDefault() as Button;
                    btn.ContextMenuStrip = CreateMenu(i.Value);
                }
                catch { }
            }
            this.Clear.ContextMenuStrip = CreateMenu(new string[] { "Locations", "Entrances", "Checked Items" }, true);
            this.ContextMenuStrip = CreateMenu(new string[] { "" }, true, true);
            this.entrances.Visible = LogicObjects.MainTrackerInstance.IsEntranceRando();
        }

        public void AddExtraToMisc()
        {
            List<string> Groups = new List<string>();
            List<string> Assignedlocations = LocationDic.SelectMany(x => x.Value).ToList();
            List<string> ValidLocations = LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsFake).Select(x => x.LocationArea).Distinct().Select(x => "#" + x).ToList();

            if (File.Exists(@"Recources\Categories.txt"))
            {
                Groups = File.ReadAllLines(@"Recources\Categories.txt")
                    .Select(x => x.Trim())
                    .Select(x => "#" + x).ToList();
            }

            List<string> NewMisc = new List<string> { "#Misc" };
            foreach (var i in Groups)
            {
                if (!Assignedlocations.Contains(i) && ValidLocations.Contains(i)) { NewMisc.Add(i); Console.WriteLine(i); }
            }
            LocationDic["Misc"] = NewMisc.ToArray();
        }
    }
}
