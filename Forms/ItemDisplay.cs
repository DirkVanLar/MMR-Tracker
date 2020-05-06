using MMR_Tracker.Class_Files;
using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace MMR_Tracker.Forms
{
    public partial class ItemDisplay : Form
    {
        public static Dictionary<string, Bitmap> Images = new Dictionary<string, Bitmap>();
        public MainInterface MainInterfaceInstance;
        public int ItemsOnScreen = 8;
        public int[] Position = new int[] { 0, 0 };
        public bool DebugShowAll = false;

        //Form Functions
        private void ItemDisplay_Load(object sender, EventArgs e)
        {
            GetImageFromImageSheet();
            UpdateScreen();
            this.Height = ((this.Width - 16) / ItemsOnScreen) * Position[0] + 38;
        }
        private void ItemDisplay_Resize(object sender, EventArgs e)
        {
            UpdateScreen();
            this.Height = ((this.Width - 16) / ItemsOnScreen)* Position[0] + 38;
        }
        public ItemDisplay()
        {
            InitializeComponent();
            MainInterface.LocationChecked += MainInterface_LocationChecked;
            Tools.NetDataProcessed += MainInterface_LocationChecked;
        }

        //Picture Utils
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
                    inv = Color.FromArgb(inv.A, (255 - inv.R), (255 - inv.G), (255 - inv.B));
                    pic.SetPixel(x, y, inv);
                }
            }
            return pic;
        }
        private Bitmap ReshadeImage(Bitmap pic, int R, int G, int B)
        {
            pic = pic.Clone() as Bitmap;
            for (int y = 0; (y <= (pic.Height - 1)); y++)
            {
                for (int x = 0; (x <= (pic.Width - 1)); x++)
                {
                    Color inv = pic.GetPixel(x, y);
                    if (inv.R == 0 && inv.G == 0 && inv.B == 0) { continue; }
                    int NewR = (inv.R + R > 255) ? 255 : ((inv.R + R < 0) ? 0 : inv.R + R);
                    int NewB = (inv.B + B > 255) ? 255 : ((inv.B + B < 0) ? 0 : inv.B + B);
                    int NewG = (inv.G + G > 255) ? 255 : ((inv.G + G < 0) ? 0 : inv.G + G);
                    inv = Color.FromArgb(inv.A, NewR, NewG, NewB);
                    pic.SetPixel(x, y, inv);
                }
            }
            return pic;
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
        public void GetImageFromImageSheet()
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
            Images.Add("AdultWallet", GetImage(5, 3));
            Images.Add("GiantWallet", GetImage(5, 4));
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
            Images.Add("SmallPoe", GetImage(5, 10));
            Images.Add("BigPoe", GetImage(0, 11));
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
            Images.Add("OceanSkullToken", ReshadeImage(GetImage(4, 22), -200, -40, 80));
            Images.Add("SwampSkullToken", ReshadeImage(GetImage(4, 22), -100, 0, -40));
            Images.Add("ClockTownFairy", GetImage(5, 22));
            Images.Add("StoneTowerFairy", GetImage(2, 12));
            Images.Add("GreatBayFairy", GetImage(3, 12));
            Images.Add("SnowheadFairy", GetImage(4, 12));
            Images.Add("WoodfallFairy", GetImage(5, 12));
            Images.Add("Song", GetImage(3, 24));
            Images.Add("SongTime", ReshadeImage(GetImage(3, 24), -200, -20, 100));
            Images.Add("SongHealing", ReshadeImage(GetImage(3, 24), 100, 0, 50));
            Images.Add("SongEpona", ReshadeImage(GetImage(3, 24), 100, 20, -60));
            Images.Add("SongStorms", ReshadeImage(GetImage(3, 24), 64, 64, 128));
            Images.Add("SongSoaring", ReshadeImage(GetImage(3, 24), 32, 0, 0));
            Images.Add("Sonata", ReshadeImage(GetImage(3, 24), -50, 32, -50));
            Images.Add("Lullaby", ReshadeImage(GetImage(3, 24), 64, -64, -32));
            Images.Add("BossaNova", ReshadeImage(GetImage(3, 24), -200, 50, 100));
            Images.Add("Elegy", ReshadeImage(GetImage(3, 24), 100, 80, 0));
            Images.Add("Oath", ReshadeImage(GetImage(3, 24), 20, -50, 100));

            Images.Add("Error", GetImage(2, 24));
            Images.Add("Moon", new Bitmap(@"Recources\Images\Moon.ico"));

        }

        //Picture Placing
        public void DisplayImages()
        {
            Controls.Clear();

            int Spacing = (this.Width - 16) / ItemsOnScreen;
            Position[0] = 0;
            Position[1] = 0;

            var start = System.DateTime.Now.Ticks;

            DrawItem("Ocarina", "Ocarina of Time", Spacing);
            DrawProgressiveItem("Bow", new Dictionary<string, string> { { "Hero's Bow", "30" }, { "Town Archery Quiver (40)", "40" }, { "Swamp Archery Quiver (50)", "50" } }, Spacing);
            DrawItem("FireArrow", "Fire Arrow", Spacing);
            DrawItem("IceArrow", "Ice Arrow", Spacing);
            DrawItem("LightArrow", "Light Arrow", Spacing);
            DrawItem("SongTime", "Song of Time", Spacing, "Time");
            DrawItem("Sonata", "Sonata of Awakening", Spacing, "Sonata");
            DrawItem("MoonTear", "Moon's Tear", Spacing);

            DrawProgressiveItem("Bombs", new Dictionary<string, string> { { "Bomb Bag (20)", "20" }, { "Town Bomb Bag (30)", "30" }, { "Mountain Bomb Bag (40)", "40" } }, Spacing);
            DrawCountableItem("Bombchus", new List<string> { "10 Bombchu", "5 Bombchu", "Bombchu" }, Spacing);
            DrawCountableItem("DekuSticks", new List<string> { "Deku Stick" }, Spacing);
            DrawCountableItem("DekuNuts", new List<string> { "10 Deku Nuts", "Deku Nuts" }, Spacing);
            DrawCountableItem("MagicBeans", new List<string> { "Magic Bean", "Any Magic Bean" }, Spacing);
            DrawItem("SongHealing", "Song of Healing", Spacing, "Healing");
            DrawItem("Lullaby", "Goron Lullaby", Spacing, "Lullaby");
            DrawItem("LandDeed", "Land Title Deed", Spacing);

            DrawCountableItem("PowderKeg", new List<string> { "Powder Keg" }, Spacing);
            DrawItem("PictoBox", "Pictobox", Spacing);
            DrawItem("LensOfTruth", "Lens of Truth", Spacing);
            DrawItem("HookShot", "Hookshot", Spacing);
            DrawItem("GreatFairySword", "Great Fairy's Sword", Spacing);
            DrawItem("SongEpona", "Epona's Song", Spacing, "Epona");
            DrawItem("BossaNova", "New Wave Bossa Nova", Spacing, "Bossanova");
            DrawItem("SwampDeed", "Swamp Title Deed", Spacing);

            DrawCountableItem("Bottle", new List<string> { "Bottle with Red Potion", "Bottle with Milk", "Bottle with Gold Dust", "Empty Bottle", "Bottle with Chateau Romani" }, Spacing, true);
            DrawCountableItem("Milk", new List<string> { "Milk", "Bottle with Milk" }, Spacing);
            DrawItem("GoldDust", "Goron Race Bottle", Spacing);
            DrawItem("SeaHorse", "Seahorse", Spacing);
            DrawCountableItem("Chateau", new List<string> { "Chateau Romani", "Bottle with Chateau Romani" }, Spacing);
            DrawItem("SongSoaring", "Song of Soaring", Spacing, "Soaring");
            DrawItem("Elegy", "Elegy of Emptiness", Spacing, "Elegy");
            DrawItem("MountainDeed", "Mountain Title Deed", Spacing);

            DrawItem("Fairy", "Bottle: Fairy", Spacing);
            DrawItem("DekuPrincess", "Bottle: Deku Princess", Spacing);
            DrawItem("Fish", "Bottle: Fish", Spacing);
            DrawItem("Bug", "Bottle: Bug", Spacing);
            DrawItem("SmallPoe", "Bottle: Poe", Spacing);
            DrawItem("SongStorms", "Song of Storms", Spacing, "Storms");
            DrawItem("Oath", "Oath to Order", Spacing, "Oath");
            DrawItem("OceanDeed", "Ocean Title Deed", Spacing);


            DrawItem("BigPoe", "Bottle: Big Poe", Spacing);
            DrawItem("Water", "Bottle: Spring Water", Spacing);
            DrawItem("HotSpringWater", "Bottle: Hot Spring Water", Spacing);
            DrawItem("ZoraEgg", "Bottle: Zora Egg", Spacing);
            DrawItem("Mushroom", "Bottle: Mushroom", Spacing);
            DrawCountableItem("HeartPiece", new List<string> { "Piece of Heart" }, Spacing, true);
            DrawCountableItem("HeartContainer", new List<string> { "Heart Container" }, Spacing, true);
            DrawItem("RoomKey", "Room Key", Spacing);

            DrawItem("PostmansHat", "Postman's Hat", Spacing);
            DrawItem("AllNightMask", "All Night Mask", Spacing);
            DrawItem("BlastMask", "Blast Mask", Spacing);
            DrawItem("StoneMask", "Stone Mask", Spacing);
            DrawItem("GreatFairyMask", "Great Fairy's Mask", Spacing);
            DrawItem("DekuMask", "Deku Mask", Spacing);
            DrawCountableItem("RedPotion", new List<string> { "Red Potion" }, Spacing);
            DrawItem("MamaLetter", "Letter to Mama", Spacing);



            DrawItem("Keatonmask", "Keaton Mask", Spacing);
            DrawItem("BremonMask", "Bremen Mask", Spacing);
            DrawItem("BunnyHood", "Bunny Hood", Spacing);
            DrawItem("DonGeroMask", "Don Gero's Mask", Spacing);
            DrawItem("MaskOfScents", "Mask of Scents", Spacing);
            DrawItem("GoronMask", "Goron Mask", Spacing);
            DrawCountableItem("GreenPotion", new List<string> { "Green Potion" }, Spacing);
            DrawItem("KafeiLetter", "Letter to Kafei", Spacing);

            DrawItem("RomaniMask", "Romani Mask", Spacing);
            DrawItem("CircusLeadersMask", "Circus Leader's Mask", Spacing);
            DrawItem("KafeisMask", "Kafei's Mask", Spacing);
            DrawItem("CouplesMask", "Couple's Mask", Spacing);
            DrawItem("MaskOftruth", "Mask of Truth", Spacing);
            DrawItem("ZoraMask", "Zora Mask", Spacing);
            DrawCountableItem("BluePotion", new List<string> { "Blue Potion" }, Spacing);
            DrawItem("Pendant", "Pendant of Memories", Spacing);

            DrawItem("KamaroMask", "Kamaro's Mask", Spacing);
            DrawItem("GibdoMask", "Gibdo Mask", Spacing);
            DrawItem("GaroMask", "Garo Mask", Spacing);
            DrawItem("CaptainsHat", "Captain's Hat", Spacing);
            DrawItem("GiantsMask", "Giant's Mask", Spacing);
            DrawItem("FierceDeityMask", "Fierce Deity's Mask", Spacing);
            DrawItem("ClockTownFairy", "Clock Town Stray Fairy", Spacing);
            DrawItem("BombersNotebook", "Bombers' Notebook", Spacing);

            DrawProgressiveItem("KokiriSword", new Dictionary<string, string> { { "Starting Sword", "null" }, { "Razor Sword", "null" }, { "Gilded Sword", "null" } }, Spacing, new List<string> { "KokiriSword", "RazorSword", "GildedSword" });
            DrawItem("OdolwasRemains", "Woodfall clear", Spacing, "", true);
            DrawItem("Map", "Woodfall Map", Spacing);
            DrawItem("Compass", "Woodfall Compass", Spacing);
            DrawItem("BossKey", "Woodfall Boss Key", Spacing);
            DrawItem("SmallKey", "Woodfall Key 1", Spacing);
            DrawCountableItem("WoodfallFairy", new List<string> { "Woodfall Stray Fairy" }, Spacing, true);
            DrawCountableItem("SwampSkullToken", new List<string> { "Swamp Skulltula Spirit" }, Spacing, true);

            DrawProgressiveItem("HeroShield", new Dictionary<string, string> { { "Trading Post Shield", "null" }, { "Zora Shop Shield", "null" }, { "Starting Shield", "null" }, { "Mirror Shield", "null" } }, Spacing, new List<string> { "HeroShield", "HeroShield", "HeroShield", "MirrorShield" });
            DrawItem("GohtsRemains", "Snowhead clear", Spacing, "", true);
            DrawItem("Map", "Snowhead Map", Spacing);
            DrawItem("Compass", "Snowhead Compass", Spacing);
            DrawItem("BossKey", "Snowhead Boss Key", Spacing);
            DrawCountableItem("SmallKey", new List<string> { "Snowhead Small Key" }, Spacing, true);
            DrawCountableItem("SnowheadFairy", new List<string> { "Snowhead Stray Fairy" }, Spacing, true);
            DrawCountableItem("Magic", new List<string> { "Magic Power", "Extended Magic Power" }, Spacing);

            DrawProgressiveItem("AdultWallet", new Dictionary<string, string> { { "Town Wallet (200)", "null" }, { "Ocean Wallet (500)", "null" } }, Spacing, new List<string> { "AdultWallet", "GiantWallet" });
            DrawItem("GyorgsRemains", "Great Bay clear", Spacing, "", true);
            DrawItem("Map", "Great Bay Map", Spacing);
            DrawItem("Compass", "Great Bay Compass", Spacing);
            DrawItem("BossKey", "Great Bay Boss Key", Spacing);
            DrawItem("SmallKey", "Great Bay Key 1", Spacing);
            DrawCountableItem("GreatBayFairy", new List<string> { "Great Bay Stray Fairy" }, Spacing, true);
            DrawCountableItem("OceanSkullToken", new List<string> { "Ocean Skulltula Spirit" }, Spacing, true);

            DrawItem("Moon", "Moon Access", Spacing, "", true);
            DrawItem("TwimoldsRemains", "Ikana clear", Spacing, "", true);
            DrawItem("Map", "Stone Tower Map", Spacing);
            DrawItem("Compass", "Stone Tower Compass", Spacing);
            DrawItem("BossKey", "Stone Tower Boss Key", Spacing);
            DrawCountableItem("SmallKey", new List<string> { "Stone Tower Small Key" }, Spacing, true);
            DrawCountableItem("StoneTowerFairy", new List<string> { "Stone Tower Stray Fairy" }, Spacing, true);
            DrawItem("DoubleDefence", "Great Fairy Double Defense", Spacing);

            var End = System.DateTime.Now.Ticks;
            var total = End - start;
            Console.WriteLine("Execution took " + total.ToString() + " ticks");

        }
        public void Increaseposition()
        {
            int row = Position[0];
            int colomn = Position[1];

            colomn++;
            if (colomn >= ItemsOnScreen)
            {
                colomn = 0;
                row++;
            }
            Position[0] = row;
            Position[1] = colomn;

        }
        public void DrawItem(string Image, string Logicname, int Spacing, string Text = "", bool FakeItem = false)
        {
            int row = Position[0];
            int colomn = Position[1];
            Increaseposition();
            Bitmap CurentImage;
            if (Images.ContainsKey(Image))
            {
                CurentImage = Images[Image];
            }
            else
            {
                CurentImage = Images["Error"];
            }

            LogicObjects.LogicEntry Entry = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == Logicname);
            if (Entry != null && ((!FakeItem && !Entry.Useable()) || (FakeItem && !Entry.Available)) && !DebugShowAll) { CurentImage = new Bitmap(GreyImage(CurentImage)); }

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
            PB.Click += (s, ee) => FilterRegularItem(Entry);

            string display = "";
            if (Entry == null)
            {
                display = Text;
            }
            else
            {
                if (Entry.GetItemsNewLocation(LogicObjects.MainTrackerInstance.Logic) != null && !Entry.Aquired) { display = (Text == "") ? "X" : "X " + Text; }
                else { display = Text; }
            }

            Label lb = new Label();
            lb.Location = new Point(PostionItem(row, colomn, Spacing).X + 2, PostionItem(row, colomn, Spacing).Y + 2);
            lb.BackColor = Color.Black;
            lb.ForeColor = Color.White;
            lb.Text = display;
            lb.AutoSize = true;
            lb.Font = new Font("Arial", Spacing / 6);
            Controls.Add(lb);

            while (lb.Width > PB.Width - 4) { lb.Font = new Font("Arial", lb.Font.SizeInPoints - (float)0.1); }

            lb.Click += (s, ee) => FilterRegularItem(Entry);
            lb.BringToFront();
        }
        public void DrawProgressiveItem(string Image, Dictionary<string, string> Logicnames, int Spacing, List<string> ProgressiveImages = null)
        {
            int row = Position[0];
            int colomn = Position[1];
            Increaseposition();
            Bitmap CurentImage;
            if (Images.ContainsKey(Image))
            {
                CurentImage = Images[Image];
            }
            else
            {
                CurentImage = Images["Error"];
            }
            string CountNumber = "";
            List<LogicObjects.LogicEntry> AllEntries = new List<LogicObjects.LogicEntry>();
            bool Itemmarked = false;
            int count = 0;
            foreach (KeyValuePair<string, string> i in Logicnames)
            {
                LogicObjects.LogicEntry Entry = LogicObjects.MainTrackerInstance.Logic.Find(x => x.DictionaryName == i.Key);
                if (Entry != null && Entry.Useable()) { CountNumber = i.Value; }
                if (Entry != null) { AllEntries.Add(Entry); }
                if (Entry != null && Entry.GetItemsNewLocation(LogicObjects.MainTrackerInstance.Logic) != null & !Entry.Aquired) { Itemmarked = true; }
                if (Entry != null && Entry.Useable() && ProgressiveImages != null && count < ProgressiveImages.Count())
                {
                    if (Images.ContainsKey(ProgressiveImages[count]))
                    {
                        CurentImage = Images[ProgressiveImages[count]];
                    }
                }
                count++;
            }
            if (CountNumber == "" && !DebugShowAll) { CurentImage = new Bitmap(GreyImage(CurentImage)); }
            if (CountNumber == "" && Itemmarked) { CountNumber = "X"; }
            if (CountNumber == "null") { CountNumber = ""; }
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
            while (lb.Width > PB.Width - 4) { lb.Font = new Font("Arial", lb.Font.SizeInPoints - (float)0.1); }
            lb.Click += (s, ee) => FilterProgressiveItem(AllEntries);
            lb.BringToFront();
        }
        public void DrawCountableItem(string Image, List<string> ItemNames, int Spacing, bool ShowCount = false)
        {
            int row = Position[0];
            int colomn = Position[1];
            Increaseposition();
            Bitmap CurentImage;
            if (Images.ContainsKey(Image))
            {
                CurentImage = Images[Image];
            }
            else
            {
                CurentImage = Images["Error"];
            }
            var Instance = LogicObjects.MainTrackerInstance;
            var log = Instance.Logic;
            int Obtained = log.Where(x => ItemNames.Contains(x.ItemName) && x.Useable()).Count();
            int Seen = log.Where(x => x.HasRandomItem(true) && ItemNames.Contains(x.RandomizedEntry(Instance).ItemName) && !x.Checked).Count();
            int TotalNumber = Seen + Obtained;
            string Display = "";

            if (Obtained < 1 && !DebugShowAll) { CurentImage = GreyImage(CurentImage); }
            if (ShowCount && TotalNumber > 0)
            {
                Display = (TotalNumber > Obtained) ? $"{Obtained.ToString()}/{TotalNumber.ToString()}" : TotalNumber.ToString();
            }
            else if (Seen > 0 && Obtained < 1)
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
            while (lb.Width > PB.Width - 4) { lb.Font = new Font("Arial", lb.Font.SizeInPoints - (float)0.1); }
            lb.Click += (s, ee) => FilterMultipleItems(ItemNames);
            lb.BringToFront();
        }

        //External Actions
        public void FilterRegularItem(LogicObjects.LogicEntry e)
        {
            if (e == null) { return; }
            var itemLocation = e.GetItemsNewLocation(LogicObjects.MainTrackerInstance.Logic);
            if (itemLocation == null) { return; }
            if (itemLocation.Checked && e.Aquired)
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { MainInterfaceInstance.TXTCheckedSearch.Text = $"_{e.ItemName}"; }
                else { MainInterfaceInstance.TXTCheckedSearch.Text += (MainInterfaceInstance.TXTCheckedSearch.Text == "" ? "" : "|") + $"_{e.ItemName}"; }
            }
            else
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { MainInterfaceInstance.TXTLocSearch.Text = $"_{e.ItemName}"; }
                else { MainInterfaceInstance.TXTLocSearch.Text += (MainInterfaceInstance.TXTLocSearch.Text == "" ? "" : "|") + $"_{e.ItemName}"; }

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
                var itemLocation = i.GetItemsNewLocation(LogicObjects.MainTrackerInstance.Logic);
                if (itemLocation != null)
                {
                    if (itemLocation.Checked && i.Aquired) { CheckedListFilter += (CheckedListFilter == "" ? "" : "|") + $"_{i.ItemName}"; }
                    else { ItemListFilter += (ItemListFilter == "" ? "" : "|") + $"_{i.ItemName}"; }
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
                List<string> Used = new List<string>();
                foreach (var i in Marked)
                {
                    if (Used.Contains(i.RandomizedEntry(log).ItemName)) { continue; }
                    itemFilter += (itemFilter == "" ? "" : "|") + $"_{i.RandomizedEntry(log).ItemName}";
                    Used.Add(i.RandomizedEntry(log).ItemName);
                }
                if ((ModifierKeys & Keys.Control) != Keys.Control) { MainInterfaceInstance.TXTLocSearch.Text = itemFilter; }
                else { MainInterfaceInstance.TXTLocSearch.Text += (MainInterfaceInstance.TXTLocSearch.Text == "" ? "" : "|") + itemFilter; }
            }
            if (Checked.Count() > 0)
            {
                List<string> Used = new List<string>();
                foreach (var i in Checked)
                {
                    if (Used.Contains(i.RandomizedEntry(log).ItemName)) { continue; }
                    CheckedFilter += (CheckedFilter == "" ? "" : "|") + $"_{i.RandomizedEntry(log).ItemName}";
                    Used.Add(i.RandomizedEntry(log).ItemName);
                }
                if ((ModifierKeys & Keys.Control) != Keys.Control) { MainInterfaceInstance.TXTCheckedSearch.Text = CheckedFilter; }
                else { MainInterfaceInstance.TXTCheckedSearch.Text += (MainInterfaceInstance.TXTCheckedSearch.Text == "" ? "" : "|") + CheckedFilter; }
            }
        }
        private void MainInterface_LocationChecked(object sender, EventArgs e)
        {
            UpdateScreen();
        }

        private void UpdateScreen()
        {
           DisplayImages();
        }
    }
}
