using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MMR_Tracker_V2;

namespace MMR_Tracker
{
    
    public partial class Map : Form
    {
        public Map()
        {
            InitializeComponent();
            MainInterface.LogicStateUpdated += LogicEditing_LogicChanged;
        }

        private void LogicEditing_LogicChanged(object sender, EventArgs e)
        {
            Debugging.Log("Trigger");
            Map_Load(sender, e);
        }

        private void Map_Load(object sender, EventArgs e)
        {
            clearContextMenuStrips();
            setLocationDic(LocationDic);
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
            this.entrances.Visible = LogicObjects.MainTrackerInstance.EntranceRando;
            OriginalSize = this.Size;
        }

        public class LocationArea
        {
            public string locationArea { get; set; }
            public string[] SubAreas { get; set; }
            public Button Btn { get; set; }
        }

        public MainInterface MainInterfaceInstance = MainInterface.CurrentProgram;
        private ContextMenuStrip btnRClick;
        public List<LocationArea> LocationDic = new List<LocationArea>();
        public void setLocationDic(List<LocationArea> Result)
        {
            Result.Clear();
            Result.Add(new LocationArea { locationArea = "ClockTown", Btn = clockTown, SubAreas = new string[] 
            { "#North Clock Town", "#South Clock Town", "#East Clock Town", "#West Clock Town", "#Stock Pot Inn", "#Laundry Pool", "#Beneath Clocktown" } });
            Result.Add(new LocationArea { locationArea = "Termina", Btn = Termina, SubAreas = new string[] 
            { "#Termina Field", "#Astral Observatory" } });
            Result.Add(new LocationArea { locationArea = "Ranch", Btn = Ranch, SubAreas = new string[] 
            { "#Milk Road", "#Romani Ranch" } });
            Result.Add(new LocationArea { locationArea = "Swamp", Btn = Swamp, SubAreas = new string[] 
            { "#Road to Southern Swamp", "#Southern Swamp", "#Swamp Skull House", "#Deku Palace" }});
            Result.Add(new LocationArea { locationArea = "Woodfall", Btn = Woodfall, SubAreas = new string[] 
            { "#Woodfall", "#Woodfall Temple", "#Woodfall Temple Fairies", "#Dungeon Entrance, Woodfall" } });
            Result.Add(new LocationArea { locationArea = "Ikana", Btn = Ikana, SubAreas = new string[] 
            { "#Road to Ikana", "#Ikana Canyon", "#Secret Shrine", "#Beneath the Well", "#Ikana Castle", "#Ikana Graveyard" } });
            Result.Add(new LocationArea { locationArea = "StoneTower", Btn = StoneTower, SubAreas = new string[] 
            { "#Stone Tower", "#Stone Tower Temple", "#Inverted Stone Tower Temple", "#Stone Tower Temple Fairies", "#Dungeon Entrance, Stone Tower" } });
            Result.Add(new LocationArea { locationArea = "Mountain", Btn = Mountain, SubAreas = new string[] 
            { "#Path to Mountain Village", "#Mountain Village", "#Twin Islands", "#Goron Village" } });
            Result.Add(new LocationArea { locationArea = "Snowhead", Btn = Snowhead, SubAreas = new string[] 
            { "#Path to Snowhead", "#Snowhead", "#Snowhead Temple", "#Snowhead Temple Fairies", "#Dungeon Entrance, Snowhead" } });
            Result.Add(new LocationArea { locationArea = "Moon", Btn = Moon, SubAreas = new string[] 
            { "#The Moon" } });
            Result.Add(new LocationArea { locationArea = "Coast", Btn = Coast, SubAreas = new string[] 
            { "#Zora Cape", "#Zora Hall", "#Great Bay Cape", "#Great Bay Temple", "#Great Bay Temple Fairies", "#Dungeon Entrance, Great Bay" }});
            Result.Add(new LocationArea { locationArea = "Misc", Btn = Misc, SubAreas = new string[] 
            { "#Misc" } });
            Result.Add(new LocationArea { locationArea = "GreatBay", Btn = GreatBay, SubAreas = new string[] 
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
            bool EntrancesInItemBox = !LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled && LogicObjects.MainTrackerInstance.EntranceRando;
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
            Debugging.Log(filter);
            return filter;
        }

        public ContextMenuStrip CreateMenu(string[] Filters, bool ClearMe = false, bool Return = false)
        {
            btnRClick = new ContextMenuStrip();
            //this.ContextMenuStrip = btnRClick;
            if (Return) { return btnRClick; }
            if (ClearMe)
            {
                for(var i = 0; i < Filters.Length; i++)
                {
                    if (!LogicObjects.MainTrackerInstance.EntranceRando && i == 1) { continue; }
                    ToolStripItem ContextMenui = btnRClick.Items.Add(Filters[i]);
                    bool EntrancesInItemBox = !LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled && LogicObjects.MainTrackerInstance.EntranceRando;
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

            if (File.Exists(VersionHandeling.BaseProgramPath + @"Recources\Other Files\Categories.txt"))
            {
                Groups = File.ReadAllLines(VersionHandeling.BaseProgramPath + @"Recources\Other Files\Categories.txt")
                    .Select(x => x.Trim())
                    .Select(x => "#" + x).ToList();
            }

            List<string> NewMisc = new List<string> { "#Misc" };
            foreach (var i in ValidLocations)
            {
                if (!Assignedlocations.Contains(i)) { NewMisc.Add(i); Debugging.Log(i); }
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

        private void AreaClicked(object sender, EventArgs e)
        {
            if (!(sender is Button)) { return; }
            Button area = sender as Button;
            if (area == clockTown) { ProcessFilters(LocationDic.Find(x => x.locationArea == "ClockTown").SubAreas); }
            if (area == Termina) { ProcessFilters(LocationDic.Find(x => x.locationArea == "Termina").SubAreas); }
            if (area == Ranch) { ProcessFilters(LocationDic.Find(x => x.locationArea == "Ranch").SubAreas); }
            if (area == Swamp) { ProcessFilters(LocationDic.Find(x => x.locationArea == "Swamp").SubAreas); }
            if (area == Woodfall) { ProcessFilters(LocationDic.Find(x => x.locationArea == "Woodfall").SubAreas); }
            if (area == Ikana) { ProcessFilters(LocationDic.Find(x => x.locationArea == "Ikana").SubAreas); }
            if (area == StoneTower) { ProcessFilters(LocationDic.Find(x => x.locationArea == "StoneTower").SubAreas); }
            if (area == Mountain) { ProcessFilters(LocationDic.Find(x => x.locationArea == "Mountain").SubAreas); }
            if (area == Snowhead) { ProcessFilters(LocationDic.Find(x => x.locationArea == "Snowhead").SubAreas); }
            if (area == GreatBay) { ProcessFilters(LocationDic.Find(x => x.locationArea == "GreatBay").SubAreas); }
            if (area == Moon) { ProcessFilters(LocationDic.Find(x => x.locationArea == "Moon").SubAreas); }
            if (area == Coast) { ProcessFilters(LocationDic.Find(x => x.locationArea == "Coast").SubAreas); }
            if (area == Misc) { ProcessFilters(LocationDic.Find(x => x.locationArea == "Misc").SubAreas); }
            if (area == Clear)
            {
                bool EntrancesInItemBox = !LogicObjects.MainTrackerInstance.Options.EntranceRadnoEnabled && LogicObjects.MainTrackerInstance.EntranceRando;
                if (checkedLocations.Checked) { MainInterfaceInstance.TXTCheckedSearch.Text = ""; }
                if (entrances.Checked && EntrancesInItemBox) { MainInterfaceInstance.TXTLocSearch.Text = ""; }
                else if (entrances.Checked && !EntrancesInItemBox) { MainInterfaceInstance.TXTEntSearch.Text = ""; }
                if (locations.Checked) { MainInterfaceInstance.TXTLocSearch.Text = ""; }
            }
        }

        public Size OriginalSize;

        public void ResizeButton(Rectangle originalSize, Button btn)
        {
            float xRatio = (float)(this.Width - 8) / (float)(OriginalSize.Width - 8);
            float yRatio = (float)(this.Height - 39) / (float)(OriginalSize.Height - 39);

            int newX = (int)(originalSize.X * xRatio);
            int newY = (int)(originalSize.Y * yRatio);
            int newWidth = (int)(originalSize.Width * yRatio);
            int newHeight = (int)(originalSize.Height * yRatio);

            Debugging.Log((this.Width - 8).ToString());
            Debugging.Log((this.Height - 39).ToString());

            btn.Location = new Point(newX, newY);
            btn.Size = new Size(newWidth, newHeight);
        }

        private void Map_Resize(object sender, EventArgs e)
        {
            foreach (Button ctrl in LocationDic.Select(x => x.Btn))
            {
                ResizeButton(new Rectangle(ctrl.Location.X, ctrl.Location.Y, ctrl.Width, ctrl.Height), ctrl);
            }
            ResizeButton(new Rectangle(Clear.Location.X, Clear.Location.Y, Clear.Width, Clear.Height), Clear);
            OriginalSize = this.Size;
        }
    }
}
