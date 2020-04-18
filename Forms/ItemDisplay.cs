using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MMR_Tracker.Forms
{
    public partial class ItemDisplay : Form
    {
        public static Dictionary<string, Bitmap> Images = new Dictionary<string, Bitmap>();
        public ItemDisplay()
        {
            InitializeComponent();
        }
        public void SetItemImages()
        {
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
            Images.Add("SkullToken", GetImage(4, 22));

        }
        public static Bitmap GetImage(int Column, int Row)
        {
            Bitmap source = new Bitmap(@"Recources\Nintendo 64 - The Legend of Zelda Majoras Mask - Item Icons.png");
            return source.Clone(new System.Drawing.Rectangle(Column * 32, Row * 32, 32, 32), source.PixelFormat);
        }

        private void ItemDisplay_Load(object sender, EventArgs e)
        {
            SetItemImages();

        }
    }
}
