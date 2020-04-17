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
using MMR_Tracker.Class_Files;
using MMR_Tracker.Forms;
using MMR_Tracker_V2;

namespace MMR_Tracker
{
    
    public partial class Map : Form
    {
        public Map()
        {
            InitializeComponent();
            MainInterface.TrackerUpdate += LogicEditing_LogicChanged;
        }

        private void LogicEditing_LogicChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Trigger");
            Map_Load(null, null);
        }

        private void Map_Load(object sender, EventArgs e)
        {
            clearContextMenuStrips();
            setLocationDic();
            AddUnusedToMisc();
            foreach (var i in LocationDic)
            {
                try
                {
                    Button btn = i.Btn;
                    btn.ContextMenuStrip = CreateMenu(i.SubAreas);
                }
                catch { }
            }
            this.Clear.ContextMenuStrip = CreateMenu(new string[] { "Locations", "Entrances", "Checked Items" }, true);
            this.ContextMenuStrip = CreateMenu(new string[] { "" }, true, true);
            this.entrances.Visible = LogicObjects.MainTrackerInstance.IsEntranceRando();
            Console.WriteLine(LogicObjects.MainTrackerInstance.IsEntranceRando());
        }

        public class LocationArea
        {
            public string locationArea { get; set; }
            public string[] SubAreas { get; set; }
            public Button Btn { get; set; }
        }

        public MainInterface MainInterfaceInstance;
        private ContextMenuStrip btnRClick;
        public List<LocationArea> LocationDic = new List<LocationArea>();
        public void setLocationDic()
        {
            LocationDic = new List<LocationArea>();
            LocationDic.Add(new LocationArea { locationArea = "ClockTown", Btn = clockTown, SubAreas = new string[] 
            { "#North Clock Town", "#South Clock Town", "#East Clock Town", "#West Clock Town", "#Stock Pot Inn", "#Laundry Pool", "#Beneath Clocktown" } });
            LocationDic.Add(new LocationArea { locationArea = "Termina", Btn = Termina, SubAreas = new string[] 
            { "#Termina Field", "#Astral Observatory" } });
            LocationDic.Add(new LocationArea { locationArea = "Ranch", Btn = Ranch, SubAreas = new string[] 
            { "#Milk Road", "#Romani Ranch" } });
            LocationDic.Add(new LocationArea { locationArea = "Swamp", Btn = Swamp, SubAreas = new string[] 
            { "#Road to Southern Swamp", "#Southern Swamp", "#Swamp Skull House", "#Deku Palace" }});
            LocationDic.Add(new LocationArea { locationArea = "Woodfall", Btn = Woodfall, SubAreas = new string[] 
            { "#Woodfall", "#Woodfall Temple", "#Woodfall Temple Fairies", "#Dungeon Entrance, Woodfall" } });
            LocationDic.Add(new LocationArea { locationArea = "Ikana", Btn = Ikana, SubAreas = new string[] 
            { "#Road to Ikana", "#Ikana Canyon", "#Secret Shrine", "#Beneath the Well", "#Ikana Castle", "#Ikana Graveyard" } });
            LocationDic.Add(new LocationArea { locationArea = "StoneTower", Btn = StoneTower, SubAreas = new string[] 
            { "#Stone Tower", "#Stone Tower Temple", "#Inverted Stone Tower Temple", "#Stone Tower Temple Fairies", "#Dungeon Entrance, Stone Tower" } });
            LocationDic.Add(new LocationArea { locationArea = "Mountain", Btn = Mountain, SubAreas = new string[] 
            { "#Path to Mountain Village", "#Mountain Village", "#Twin Islands", "#Goron Village" } });
            LocationDic.Add(new LocationArea { locationArea = "Snowhead", Btn = Snowhead, SubAreas = new string[] 
            { "#Path to Snowhead", "#Snowhead", "#Snowhead Temple", "#Snowhead Temple Fairies", "#Dungeon Entrance, Snowhead" } });
            LocationDic.Add(new LocationArea { locationArea = "Moon", Btn = Moon, SubAreas = new string[] 
            { "#The Moon" } });
            LocationDic.Add(new LocationArea { locationArea = "Coast", Btn = Coast, SubAreas = new string[] 
            { "#Zora Cape", "#Zora Hall", "#Great Bay Cape", "#Great Bay Temple", "#Great Bay Temple Fairies", "#Dungeon Entrance, Great Bay" }});
            LocationDic.Add(new LocationArea { locationArea = "Misc", Btn = Misc, SubAreas = new string[] 
            { "#Misc" } });
            LocationDic.Add(new LocationArea { locationArea = "GreatBay", Btn = GreatBay, SubAreas = new string[] 
            { "#Great Bay Coast", "#Pinnacle Rock", "#Ocean Skull House", "#Pirate Fortress", "#Pirates' Fortress Exterior", "#Pirates' Fortress Interior", "#Pirates' Fortress Sewer" } });
        }

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

            if (checkedLocations.Checked) { WriteFilter(filters, MainInterfaceInstance.TXTCheckedSearch); }
            if (entrances.Checked && EntrancesInItemBox) { WriteFilter(useFilter, MainInterfaceInstance.TXTLocSearch); }
            else if (entrances.Checked && !EntrancesInItemBox) { WriteFilter(useFilter, MainInterfaceInstance.TXTEntSearch); }
            if (locations.Checked) { WriteFilter(useFilter, MainInterfaceInstance.TXTLocSearch); }
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
                    if (i == 2) { ContextMenui.Click += (sender, e) => { MainInterfaceInstance.TXTCheckedSearch.Text = ""; }; }
                    if (i == 1 && EntrancesInItemBox) { ContextMenui.Click += (sender, e) => { MainInterfaceInstance.TXTLocSearch.Text = ""; }; }
                    else if (i == 1 && !EntrancesInItemBox) { ContextMenui.Click += (sender, e) => { MainInterfaceInstance.TXTEntSearch.Text = ""; }; }
                    if (i == 0) { ContextMenui.Click += (sender, e) => { MainInterfaceInstance.TXTLocSearch.Text = ""; }; }
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

        public void AddUnusedToMisc()
        {
            List<string> Groups = new List<string>();
            List<string> ValidLocations = LogicObjects.MainTrackerInstance.Logic.Where(x => !x.IsFake).Select(x => x.LocationArea).Distinct().Select(x => "#" + x).ToList();
            List<string> Assignedlocations = LocationDic.SelectMany(x => x.SubAreas).ToList();

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
            LocationDic.Find(x => x.locationArea == "Misc").SubAreas = NewMisc.ToArray();
        }

        public void clearContextMenuStrips()
        {
            foreach (var i in LocationDic)
            {
                i.Btn.ContextMenuStrip.Items.Clear();
            }
        }

        #endregion Static Function

        private void ClockTown_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "ClockTown").SubAreas);
        }

        private void Termina_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "Termina").SubAreas);
        }

        private void Ranch_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "Ranch").SubAreas);
        }

        private void Swamp_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "Swamp").SubAreas);
        }

        private void Woodfall_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "Woodfall").SubAreas);
        }

        private void Ikana_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "Ikana").SubAreas);
        }

        private void StoneTower_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "StoneTower").SubAreas);
        }

        private void Mountain_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "Mountain").SubAreas);
        }

        private void Snowhead_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "Snowhead").SubAreas);
        }

        private void GreatBay_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "GreatBay").SubAreas);
        }

        private void Moon_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "Moon").SubAreas);
        }

        private void Coast_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "Coast").SubAreas);
        }

        private void Misc_Click(object sender, EventArgs e)
        {
            ProcessFilters(LocationDic.Find(x => x.locationArea == "Misc").SubAreas);
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            bool EntrancesInItemBox = !LogicObjects.MainTrackerInstance.Options.entranceRadnoEnabled && LogicObjects.MainTrackerInstance.IsEntranceRando();
            if (checkedLocations.Checked) { MainInterfaceInstance.TXTCheckedSearch.Text = ""; }
            if (entrances.Checked && EntrancesInItemBox) { MainInterfaceInstance.TXTLocSearch.Text = ""; }
            else if (entrances.Checked && !EntrancesInItemBox) { MainInterfaceInstance.TXTEntSearch.Text = ""; }
            if (locations.Checked) { MainInterfaceInstance.TXTLocSearch.Text = ""; }
        }
    }
}
