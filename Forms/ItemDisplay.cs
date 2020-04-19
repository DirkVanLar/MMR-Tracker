using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace MMR_Tracker.Forms
{
    public partial class ItemDisplay : Form
    {
        public static Dictionary<string, Bitmap> Images = new Dictionary<string, Bitmap>();
        public MainInterface MainInterfaceInstance;
        public ItemDisplay()
        {
            InitializeComponent();
            MainInterface.LocationChecked += MainInterface_LocationChecked;
        }

        private void MainInterface_LocationChecked(object sender, EventArgs e)
        {
            DisplayImages();
        }
        public void SetItemImages()
        {
            Images.Clear();
            //Equip Items
            Images.Add("Ocarina", GetImage(0, 2));
            Images.Add("Bow", GetImage(1, 2));
            Images.Add("FireArrow", GetImage(2, 2));
            Images.Add("IceArrow", GetImage(3, 2));
            Images.Add("LightArrow", GetImage(4, 2));
            Images.Add("Bombs", GetImage(0, 3));
            Images.Add("Bombchus", GetImage(1, 3));
            Images.Add("DekuSticks", GetImage(2, 3));
            Images.Add("DekuNuts", GetImage(3, 3));
            Images.Add("MagicBeans", GetImage(4, 3));
            Images.Add("PowderKeg", GetImage(0, 4));
            Images.Add("PictoBox", GetImage(1, 4));
            Images.Add("LensOfTruth", GetImage(2, 4));
            Images.Add("HookShot", GetImage(3, 4));
            Images.Add("GreatFairySword", GetImage(4, 4));
            //Trade Items
            Images.Add("MoonTear", GetImage(0, 6));
            Images.Add("LandDeed", GetImage(1, 6));
            Images.Add("SwampDeed", GetImage(2, 6));
            Images.Add("MountainDeed", GetImage(3, 6));
            Images.Add("OceanDeed", GetImage(4, 6));
            Images.Add("RoomKey", GetImage(5, 6));
            Images.Add("MamaLetter", GetImage(0, 7));
            Images.Add("KafeiLetter", GetImage(1, 7));
            Images.Add("Pendant", GetImage(2, 7));
            Images.Add("Magic", GetImage(3, 7));
            Images.Add("HeartPiece", GetImage(4, 7));
            Images.Add("HeartContainer", GetImage(5, 7));
            Images.Add("DoubleDefence", InvertImage(GetImage(5, 7)));
            //Bottled Stuff
            Images.Add("Bottle", GetImage(0, 9));
            Images.Add("RedPotion", GetImage(1, 9));
            Images.Add("GreenPotion", GetImage(2, 9));
            Images.Add("BluePotion", GetImage(3, 9));
            Images.Add("Fairy", GetImage(4, 9));
            Images.Add("DekuPrincess", GetImage(5, 9));
            Images.Add("Milk", GetImage(0, 10));
            Images.Add("Fish", GetImage(2, 10));
            Images.Add("Bug", GetImage(3, 10));
            Images.Add("BigPoe", GetImage(5, 10));
            Images.Add("SmallPoe", GetImage(0, 11));
            Images.Add("Water", GetImage(1, 11));
            Images.Add("HotSpringWater", GetImage(2, 11));
            Images.Add("ZoraEgg", GetImage(3, 11));
            Images.Add("GoldDust", GetImage(4, 11));
            Images.Add("Mushroom", GetImage(5, 11));
            Images.Add("SeaHorse", GetImage(0, 12));
            Images.Add("Chateau", GetImage(1, 12));
            //Masks
            Images.Add("PostmansHat", GetImage(0, 14));
            Images.Add("AllNightMask", GetImage(1, 14));
            Images.Add("BlastMask", GetImage(2, 14));
            Images.Add("StoneMask", GetImage(3, 14));
            Images.Add("GreatFairyMask", GetImage(4, 14));
            Images.Add("DekuMask", GetImage(5, 14));
            Images.Add("Keatonmask", GetImage(0, 15));
            Images.Add("BremonMask", GetImage(1, 15));
            Images.Add("BunnyHood", GetImage(2, 15));
            Images.Add("DonGeroMask", GetImage(3, 15));
            Images.Add("MaskOfScents", GetImage(4, 15));
            Images.Add("GoronMask", GetImage(5, 15));
            Images.Add("RomaniMask", GetImage(0, 16));
            Images.Add("CircusLeadersMask", GetImage(1, 16));
            Images.Add("KafeisMask", GetImage(2, 16));
            Images.Add("CouplesMask", GetImage(3, 16));
            Images.Add("MaskOftruth", GetImage(4, 16));
            Images.Add("ZoraMask", GetImage(5, 16));
            Images.Add("KamaroMask", GetImage(0, 17));
            Images.Add("GibdoMask", GetImage(1, 17));
            Images.Add("GaroMask", GetImage(2, 17));
            Images.Add("CaptainsHat", GetImage(3, 17));
            Images.Add("GiantsMask", GetImage(4, 17));
            Images.Add("FierceDeityMask", GetImage(5, 17));
            //Quest Items
            Images.Add("KokiriSword", GetImage(0, 20));
            Images.Add("RazorSword", GetImage(1, 20));
            Images.Add("GildedSword", GetImage(2, 20));
            Images.Add("HeroShield", GetImage(4, 20));
            Images.Add("MirrorShield", GetImage(5, 20));
            Images.Add("OdolwasRemains", GetImage(0, 21));
            Images.Add("GohtsRemains", GetImage(1, 21));
            Images.Add("GyorgsRemains", GetImage(2, 21));
            Images.Add("TwimoldsRemains", GetImage(3, 21));
            Images.Add("BombersNotebook", GetImage(5, 21));
            Images.Add("Map", GetImage(0, 22));
            Images.Add("Compass", GetImage(1, 22));
            Images.Add("BossKey", GetImage(2, 22));
            Images.Add("SmallKey", GetImage(3, 22));
            //Fairies/Skulls
            Images.Add("SkullToken", GetImage(4, 22));
            Images.Add("OceanSkullToken", GetImage(4, 25));
            Images.Add("SwampSkullToken", GetImage(5, 25));
            Images.Add("ClockTownFairy", GetImage(5, 22));
            Images.Add("StoneTowerFairy", GetImage(2, 12));
            Images.Add("GreatBayFairy", GetImage(3, 12));
            Images.Add("SnowheadFairy", GetImage(4, 12));
            Images.Add("WoodfallFairy", GetImage(5, 12));
            Images.Add("Song", GetImage(3, 24));

        }
        public void DisplayImages()
        {
            Controls.Clear();

            int ItemsOnScreen = 6;

            int Spacing = (this.Width - 16) / ItemsOnScreen;

            DrawItem("Ocarina", "Ocarina of Time", 0, 0, Spacing);
            DrawProgressiveItem("Bow", new Dictionary<string, string> { { "Hero's Bow", "30" }, { "Town Archery Quiver (40)", "40" }, { "Swamp Archery Quiver (50)", "50" } }, 0, 1, Spacing);
            DrawItem("FireArrow", "Fire Arrow", 0, 2, Spacing);
            DrawItem("IceArrow", "Ice Arrow", 0, 3, Spacing);
            DrawItem("LightArrow", "Light Arrow", 0, 4, Spacing);
            DrawProgressiveItem("Bombs", new Dictionary<string, string> { { "Bomb Bag (20)", "20" }, { "Town Bomb Bag (30)", "30" }, { "Mountain Bomb Bag (40)", "40" } }, 1, 0, Spacing);
            DrawCountableItem("Bombchus", new List<string> { "10 Bombchu", "5 Bombchu", "Bombchu" }, 1, 1, Spacing);
            DrawCountableItem("DekuSticks", new List<string> { "Deku Stick" }, 1, 2, Spacing);
            DrawCountableItem("DekuNuts", new List<string> { "10 Deku Nuts", "Deku Nuts" }, 1, 3, Spacing);
            DrawCountableItem("MagicBeans", new List<string> { "Magic Bean", "Any Magic Bean" }, 1, 4, Spacing);
            DrawCountableItem("PowderKeg", new List<string> { "Powder Keg" }, 2, 0, Spacing);
            DrawItem("PictoBox", "Pictobox", 2, 1, Spacing);
            DrawItem("LensOfTruth", "Lens of Truth", 2, 2, Spacing);
            DrawItem("HookShot", "Hookshot", 2, 3, Spacing);
            DrawItem("GreatFairySword", "Great Fairy's Sword", 2, 4, Spacing);
            DrawCountableItem("Bottle", new List<string> { "Bottle with Red Potion", "Bottle with Milk", "Bottle with Gold Dust", "Empty Bottle" , "Bottle with Chateau Romani" }, 0, 5, Spacing, true);
            DrawProgressiveItem("Song", new Dictionary<string, string> { { "Song of Soaring", "Soaring" } }, 3, 3, Spacing);
        }

        public void DrawItem(string Image, string Logicname, int row, int colomn, int Spacing)
        {
            var CurentImage = Images[Image];
            LogicObjects.LogicEntry Entry = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == Logicname);
            if (Entry != null && !Entry.Useable()) { CurentImage = new Bitmap(GreyImage(CurentImage)); }
            var PB = new PictureBox
            {
                BorderStyle = BorderStyle.Fixed3D,
                Image = CurentImage,
                Width = Spacing,
                Height = Spacing,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = PostionItem(row, colomn, Spacing),
                BackColor = Color.Transparent
            };
            Controls.Add(PB);
            PB.Click += (s, ee) => filterRegularItem(Entry);

            if (Entry == null || Entry.GetItemLocation(LogicObjects.MainTrackerInstance.Logic) == null || Entry.Aquired) { return; }

            Label lb = new Label();
            lb.Location = new Point(PostionItem(row, colomn, Spacing).X + 2, PostionItem(row, colomn, Spacing).Y + 2);
            lb.BackColor = Color.Black;
            lb.ForeColor = Color.White;
            lb.Text = "X";
            lb.AutoSize = true;
            lb.Font = new Font("Arial", Spacing / 6);
            Controls.Add(lb);
            lb.Click += (s, ee) => filterRegularItem(Entry);
            lb.BringToFront();
        }
        public void DrawProgressiveItem(string Image, Dictionary<string, string> Logicnames, int row, int colomn, int Spacing)
        {
            var CurentImage = Images[Image];
            string CountNumber = "";
            List<LogicObjects.LogicEntry> AllEntries = new List<LogicObjects.LogicEntry>();
            bool Itemmarked = false;
            foreach (KeyValuePair<string, string> i in Logicnames)
            {
                LogicObjects.LogicEntry Entry = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == i.Key);
                if (Entry != null && Entry.Useable()) { CountNumber = i.Value; }
                if (Entry != null) { AllEntries.Add(Entry); }
                if (Entry != null && Entry.GetItemLocation(LogicObjects.MainTrackerInstance.Logic ) != null & !Entry.Aquired) { Itemmarked = true; }
            }
            Console.WriteLine(Itemmarked);
            if (CountNumber == "") { CurentImage = new Bitmap(GreyImage(CurentImage)); }
            if (CountNumber == "" && Itemmarked) { CountNumber = "X"; }
            var PB = new PictureBox
            {
                BorderStyle = BorderStyle.Fixed3D,
                Image = CurentImage,
                Width = Spacing,
                Height = Spacing,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = PostionItem(row, colomn, Spacing),
                BackColor = Color.Transparent
            };
            Controls.Add(PB);
            PB.Click += (s, ee) => FilterProgressiveItem(AllEntries);

            Label lb = new Label();
            lb.Location = new Point(PostionItem(row, colomn, Spacing).X + 2, PostionItem(row, colomn, Spacing).Y + 2);
            lb.BackColor = Color.Black;
            lb.ForeColor = Color.White;
            lb.Text = CountNumber;
            lb.AutoSize = true;
            lb.Font = new Font("Arial", Spacing / 6);
            Controls.Add(lb);
            lb.Click += (s, ee) => FilterProgressiveItem(AllEntries);
            lb.BringToFront();
        }
        public void DrawCountableItem(string Image, List<string> ItemNames, int row, int colomn, int Spacing, bool ShowCount = false)
        {
            var CurentImage = Images[Image];
            var Instance = LogicObjects.MainTrackerInstance;
            var log = Instance.Logic;
            int Obtained = log.Where(x => ItemNames.Contains(x.ItemName) && x.Useable()).Count();
            int Seen = log.Where(x => x.HasRandomItem(true) && ItemNames.Contains(x.RandomizedEntry(Instance).ItemName) && !x.Checked).Count();
            int TotalNumber = Seen + Obtained;
            string Display = "";

            Console.WriteLine(Obtained);

            if (Obtained < 1) { CurentImage = GreyImage(CurentImage); }
            if (ShowCount && TotalNumber > 0) 
            {
                Display = (TotalNumber > Obtained) ? $"{Obtained.ToString()}/{TotalNumber.ToString()}" : TotalNumber.ToString();
            }
            else if(Seen > 0 && Obtained < 1)
            {
                Display = "X";
            }

            var PB = new PictureBox
            {
                BorderStyle = BorderStyle.Fixed3D,
                Image = CurentImage,
                Width = Spacing,
                Height = Spacing,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = PostionItem(row, colomn, Spacing),
                BackColor = Color.Transparent
            };
            Controls.Add(PB);
            PB.Click += (s, ee) => FilterMultipleItems(ItemNames);

            Label lb = new Label();
            lb.Location = new Point(PostionItem(row, colomn, Spacing).X + 2, PostionItem(row, colomn, Spacing).Y + 2);
            lb.BackColor = Color.Black;
            lb.ForeColor = Color.White;
            lb.Text = Display;
            lb.AutoSize = true;
            lb.Font = new Font("Arial", Spacing / 6);
            Controls.Add(lb);
            lb.Click += (s, ee) => FilterMultipleItems(ItemNames);
            lb.BringToFront();
        }

        public void filterRegularItem(LogicObjects.LogicEntry e)
        {
            if (e == null) { return; }
            var itemLocation = e.GetItemLocation(LogicObjects.MainTrackerInstance.Logic);
            if (itemLocation == null) { return; }
            if (itemLocation.Checked && e.Aquired)
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { MainInterfaceInstance.TXTCheckedSearch.Text = $"&{e.ItemName}"; }
                else { MainInterfaceInstance.TXTCheckedSearch.Text += (MainInterfaceInstance.TXTCheckedSearch.Text == "" ? "" : "|") + $"&{e.ItemName}"; }
            }
            else
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { MainInterfaceInstance.TXTLocSearch.Text = $"&{e.ItemName}"; }
                else { MainInterfaceInstance.TXTLocSearch.Text += (MainInterfaceInstance.TXTLocSearch.Text == "" ? "" : "|") + $"&{e.ItemName}"; }

            }
        }
        public void FilterProgressiveItem(List<LogicObjects.LogicEntry> f)
        {
            if (f == null) { return; }
            string ItemListFilter = "";
            string CheckedListFilter = "";
            if (f.Count == 0) { return; }
            foreach (var i in f)
            {
                var itemLocation = i.GetItemLocation(LogicObjects.MainTrackerInstance.Logic);
                if (itemLocation != null)
                {
                    if (itemLocation.Checked && i.Aquired) { CheckedListFilter += (CheckedListFilter == "" ? "" : "|") + $"&{i.ItemName}"; }
                    else { ItemListFilter += (ItemListFilter == "" ? "" : "|") + $"&{i.ItemName}"; }
                }
            }
            if (ItemListFilter != "")
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { MainInterfaceInstance.TXTLocSearch.Text = ItemListFilter; }
                else { MainInterfaceInstance.TXTLocSearch.Text += (MainInterfaceInstance.TXTLocSearch.Text == "" ? "" : "|") + ItemListFilter; }
            }
            if (CheckedListFilter != "")
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { MainInterfaceInstance.TXTCheckedSearch.Text = CheckedListFilter; }
                else { MainInterfaceInstance.TXTCheckedSearch.Text += (MainInterfaceInstance.TXTCheckedSearch.Text == "" ? "" : "|") + CheckedListFilter; }
            }
        }
        public void FilterMultipleItems(List<string> h)
        {
            if (h.Count() == 0) { return; }
            var log = LogicObjects.MainTrackerInstance;
            var Checked = log.Logic.Where(x => x.HasRandomItem(true) && h.Contains(x.RandomizedEntry(log).ItemName) && x.Checked);
            var Marked = log.Logic.Where(x => x.HasRandomItem(true) && h.Contains(x.RandomizedEntry(log).ItemName) && !x.Checked);
            var itemFilter = "";
            var CheckedFilter = "";
            if (Marked.Count() > 0)
            {
                foreach(var i in Marked)
                {
                    itemFilter += (itemFilter == "" ? "" : "|") + $"&{i.RandomizedEntry(log).ItemName}";
                }
                if ((ModifierKeys & Keys.Control) != Keys.Control) { MainInterfaceInstance.TXTLocSearch.Text = itemFilter; }
                else { MainInterfaceInstance.TXTLocSearch.Text += (MainInterfaceInstance.TXTLocSearch.Text == "" ? "" : "|") + itemFilter; }
            }
            if (Checked.Count() > 0)
            {
                foreach (var i in Checked)
                {
                    CheckedFilter += (CheckedFilter == "" ? "" : "|") + $"&{i.RandomizedEntry(log).ItemName}";
                }
                if ((ModifierKeys & Keys.Control) != Keys.Control) { MainInterfaceInstance.TXTCheckedSearch.Text = CheckedFilter; }
                else { MainInterfaceInstance.TXTCheckedSearch.Text += (MainInterfaceInstance.TXTCheckedSearch.Text == "" ? "" : "|") + CheckedFilter; }
            }
        }

        public static Bitmap GetImage(int Column, int Row)
        {
            Bitmap source = new Bitmap(@"Recources\Images\Nintendo 64 - The Legend of Zelda Majoras Mask - Item Icons.png");
            return source.Clone(new System.Drawing.Rectangle(Column * 32, Row * 32, 32, 32), source.PixelFormat);
        }
        public Point PostionItem(int Row, int Columb, int Spacing)
        {
            return new Point(Columb * Spacing, Row * Spacing);
        }
        public Bitmap GreyImage(Bitmap image)
        {
            Bitmap grayScale = new Bitmap(image.Width, image.Height);

            for (Int32 y = 0; y < grayScale.Height; y++)
                for (Int32 x = 0; x < grayScale.Width; x++)
                {
                    Color c = image.GetPixel(x, y);

                    Int32 gs = (Int32)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                    grayScale.SetPixel(x, y, Color.FromArgb(gs, gs, gs));
                }
            return grayScale;
        }

        public Bitmap InvertImage(Bitmap pic)
        {
            for (int y = 0; (y <= (pic.Height - 1)); y++)
            {
                for (int x = 0; (x <= (pic.Width - 1)); x++)
                {
                    Color inv = pic.GetPixel(x, y);
                    if (inv.R == 0 && inv.G == 0 && inv.B == 0) { continue; }
                    inv = Color.FromArgb(255, (255 - inv.R), (255 - inv.G), (255 - inv.B));
                    pic.SetPixel(x, y, inv);
                }
            }
            return pic;
        }
        private void ItemDisplay_Load(object sender, EventArgs e)
        {
            SetItemImages();
            DisplayImages();
        }

        private void ItemDisplay_Resize(object sender, EventArgs e)
        {
            DisplayImages();
        }
    }
}
