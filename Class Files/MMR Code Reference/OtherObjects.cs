using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker.Class_Files.MMR_Code_Reference.Definitions;


namespace MMR_Tracker.Class_Files.MMR_Code_Reference
{
    class OtherObjects
    {
        public enum Region
        {
            [RegionName("Misc")]
            Misc,

            [RegionName("Bottle Catch")]
            BottleCatch,

            [RegionName("Beneath Clocktown")]
            BeneathClocktown,

            [RegionName("Clock Tower Roof")]
            ClockTowerRoof,

            [RegionName("South Clock Town")]
            SouthClockTown,

            [RegionName("North Clock Town")]
            NorthClockTown,

            [RegionName("Deku Playground Items")]
            DekuPlaygroundItems,

            [RegionName("East Clock Town")]
            EastClockTown,

            [RegionName("Stock Pot Inn")]
            StockPotInn,

            [RegionName("West Clock Town")]
            WestClockTown,

            [RegionName("Laundry Pool")]
            LaundryPool,

            [RegionName("Termina Field")]
            TerminaField,

            [RegionName("Road to Southern Swamp")]
            RoadToSouthernSwamp,

            [RegionName("Southern Swamp")]
            SouthernSwamp,

            [RegionName("Swamp Spider House Items")]
            SwampSpiderHouseItems,

            [RegionName("Deku Palace")]
            DekuPalace,

            [RegionName("Butler Race Items")]
            ButlerRaceItems,

            [RegionName("Woodfall")]
            Woodfall,

            [RegionName("Woodfall Temple")]
            WoodfallTemple,

            [RegionName("Path to Mountain Village")]
            PathToMountainVillage,

            [RegionName("Mountain Village")]
            MountainVillage,

            [RegionName("Twin Islands")]
            TwinIslands,

            [RegionName("Goron Race Items")]
            GoronRaceItems,

            [RegionName("Goron Village")]
            GoronVillage,

            [RegionName("Path to Snowhead")]
            PathToSnowhead,

            [RegionName("Snowhead")]
            Snowhead,

            [RegionName("Snowhead Temple")]
            SnowheadTemple,

            [RegionName("Milk Road")]
            MilkRoad,

            [RegionName("Romani Ranch")]
            RomaniRanch,

            [RegionName("Great Bay Coast")]
            GreatBayCoast,

            [RegionName("Ocean Spider House Items")]
            OceanSpiderHouseItems,

            [RegionName("Zora Cape")]
            ZoraCape,

            [RegionName("Zora Hall")]
            ZoraHall,

            [RegionName("Pirates' Fortress Exterior")]
            PiratesFortressExterior,

            [RegionName("Pirates' Fortress Sewer")]
            PiratesFortressSewer,

            [RegionName("Pirates' Fortress Interior")]
            PiratesFortressInterior,

            [RegionName("Pinnacle Rock")]
            PinnacleRock,

            [RegionName("Great Bay Temple")]
            GreatBayTemple,

            [RegionName("Road to Ikana")]
            RoadToIkana,

            [RegionName("Ikana Graveyard")]
            IkanaGraveyard,

            [RegionName("Ikana Canyon")]
            IkanaCanyon,

            [RegionName("Beneath the Well")]
            BeneathTheWell,

            [RegionName("Ikana Castle")]
            IkanaCastle,

            [RegionName("Stone Tower")]
            StoneTower,

            [RegionName("Stone Tower Temple")]
            StoneTowerTemple,

            [RegionName("Secret Shrine")]
            SecretShrine,

            [RegionName("The Moon")]
            TheMoon,
        }
        public enum LogicMode
        {
            Casual,
            Glitched,
            Vanilla,
            UserLogic,
            NoLogic,
        }
        public enum BlastMaskCooldown
        {
            Default,
            Instant,
            VeryShort,
            Short,
            Long,
            VeryLong
        }
        public enum ClockSpeed
        {
            Default,
            VerySlow,
            Slow,
            Fast,
            VeryFast,
            SuperFast
        }
        public enum NutAndStickDrops
        {
            Default = 0,
            Light = 1,
            Medium = 2,
            Extra = 3,
            Mayhem = 4
        }
        public enum FloorType
        {
            Default,
            Sand,
            Ice,
            Snow,
            Random
        }
        public enum TingleMap : byte
        {
            None = 0,
            Town = 1,
            Swamp = 2,
            Mountain = 4,
            Ranch = 8,
            Ocean = 16,
            Canyon = 32,
        }
        public enum DungeonEntrance
        {
            [Region(Region.StoneTower)]
            [Exit(Scene.InvertedStoneTower, 0)]
            [Spawn(Scene.InvertedStoneTowerTemple, 0)]
            EntranceStoneTowerTempleInvertedFromStoneTowerInverted,

            [Region(Region.Woodfall)]
            [Exit(Scene.Woodfall, 1)]
            [Spawn(Scene.WoodfallTemple, 0)]
            EntranceWoodfallTempleFromWoodfall,

            [Region(Region.Snowhead)]
            [Exit(Scene.Snowhead, 1)]
            [Spawn(Scene.SnowheadTemple, 0)]
            EntranceSnowheadTempleFromSnowhead, // should use the No Intro version if shorten cutscenes is enabled

            [Region(Region.GreatBayTemple)]
            [Exit(Scene.GreatBayTemple, 0)]
            [ExitAddress(0xF155BA)]
            [Spawn(Scene.ZoraCape, 7)]
            EntranceZoraCapeFromGreatBayTemple,

            [Region(Region.WoodfallTemple)]
            [Exit(Scene.WoodfallTemple, 0)]
            [Spawn(Scene.Woodfall, 1)]
            EntranceWoodfallFromWoodfallTempleEntrance,

            [Region(Region.ZoraCape)]
            [ExitCutscene(Scene.ZoraCape, 0, 1)]
            [ExitCutscene(Scene.ZoraCape, 0, 2)]
            [ExitCutscene(Scene.ZoraCape, 1, 2)]
            [ExitCutscene(Scene.GreatBayCutscene, 0, 0)]
            [Spawn(Scene.GreatBayTemple, 0)]
            EntranceGreatBayTempleFromZoraCape,

            [Region(Region.StoneTower)]
            [Exit(Scene.InvertedStoneTowerTemple, 0)]
            [Spawn(Scene.InvertedStoneTower, 1)]
            EntranceStoneTowerInvertedFromStoneTowerTempleInverted,

            [Region(Region.SnowheadTemple)]
            [Exit(Scene.SnowheadTemple, 0)]
            [Spawn(Scene.Snowhead, 1)]
            EntranceSnowheadFromSnowheadTemple,

            [Region(Region.SnowheadTemple)]
            [Spawn(Scene.MountainVillage, 7)]
            EntranceMountainVillageFromSnowheadClear, // one way

            [Region(Region.GreatBayTemple)]
            [Spawn(Scene.ZoraCape, 8)]
            EntranceZoraCapeFromGreatBayTempleClear, // one way

            [Region(Region.StoneTowerTemple)]
            [Spawn(Scene.IkanaCanyon, 7)]
            EntranceIkanaCanyonFromIkanaClear, // one way

            [Region(Region.WoodfallTemple)]
            [Spawn(Scene.WoodfallTemple, 1)]
            EntranceWoodfallTemplePrisonFromOdolwasLair, // one way
        }
        public enum Scene
        {
            [SceneInternalId(0x12)]
            MayorsResidence = 0x00,

            [SceneInternalId(0x0B)]
            MajorasLair = 0x01,

            [SceneInternalId(0x0A)]
            PotionShop = 0x02,

            [SceneInternalId(0x10)]
            RanchBuildings = 0x03,

            [SceneInternalId(0x11)]
            HoneyDarling = 0x04,

            [SceneInternalId(0x0C)]
            BeneathGraveyard = 0x05,

            [SceneInternalId(0x00)]
            SouthernSwampClear = 0x06,

            [SceneInternalId(0x0D)]
            CuriosityShop = 0x07,

            // TestMap = 0x08,

            // Unused = 0x09,

            [SceneInternalId(0x07)]
            Grottos = 0x0A,

            // Unused = 0x0B,

            // Unused = 0x0C,

            // Unused = 0x0D,

            // CutsceneMap = 0x0E,

            // Unused = 0x0F,

            [SceneInternalId(0x13)]
            IkanaCanyon = 0x10,

            [SceneInternalId(0x14)]
            PiratesFortress = 0x11,

            [SceneInternalId(0x15)]
            MilkBar = 0x12,

            [SceneInternalId(0x16)]
            StoneTowerTemple = 0x13,

            [SceneInternalId(0x17)]
            TreasureChestShop = 0x14,

            [SceneInternalId(0x18)]
            InvertedStoneTowerTemple = 0x15,

            [SceneInternalId(0x19)]
            ClockTowerRoof = 0x16,

            [SceneInternalId(0x1A)]
            BeforeThePortalToTermina = 0x17,

            [SceneInternalId(0x1B)]
            WoodfallTemple = 0x18,

            [SceneInternalId(0x1C)]
            PathToMountainVillage = 0x19,

            [SceneInternalId(0x1D)]
            IkanaCastle = 0x1A,

            [SceneInternalId(0x1E)]
            DekuPlayground = 0x1B,

            [SceneInternalId(0x1F)]
            OdolwasLair = 0x1C,

            [SceneInternalId(0x20)]
            TownShootingGallery = 0x1D,

            [SceneInternalId(0x21)]
            SnowheadTemple = 0x1E,

            [SceneInternalId(0x22)]
            MilkRoad = 0x1F,

            [SceneInternalId(0x23)]
            PiratesFortressRooms = 0x20,

            [SceneInternalId(0x24)]
            SwampShootingGallery = 0x21,

            [SceneInternalId(0x25)]
            PinnacleRock = 0x22,

            [SceneInternalId(0x26)]
            FairyFountain = 0x23,

            [SceneInternalId(0x27)]
            SwampSpiderHouse = 0x24,

            [SceneInternalId(0x28)]
            OceanSpiderHouse = 0x25,

            [SceneInternalId(0x29)]
            AstralObservatory = 0x26,

            [SceneInternalId(0x2A)]
            DekuTrial = 0x27,

            [SceneInternalId(0x2B)]
            DekuPalace = 0x28,

            [SceneInternalId(0x2C)]
            MountainSmithy = 0x29,

            [SceneInternalId(0x2D)]
            TerminaField = 0x2A,

            [SceneInternalId(0x2E)]
            PostOffice = 0x2B,

            [SceneInternalId(0x2F)]
            MarineLab = 0x2C,

            [SceneInternalId(0x30)]
            DampesHouse = 0x2D,

            // Unused = 0x2E,

            [SceneInternalId(0x32)]
            GoronShrine = 0x2F,

            [SceneInternalId(0x33)]
            ZoraHall = 0x30,

            [SceneInternalId(0x34)]
            TradingPost = 0x31,

            [SceneInternalId(0x35)]
            RomaniRanch = 0x32,

            [SceneInternalId(0x36)]
            TwinmoldsLair = 0x33,

            [SceneInternalId(0x37)]
            GreatBayCoast = 0x34,

            [SceneInternalId(0x38)]
            ZoraCape = 0x35,

            [SceneInternalId(0x39)]
            LotteryShop = 0x36,

            // Unused = 0x37,

            [SceneInternalId(0x3B)]
            PiratesFortressExterior = 0x38,

            [SceneInternalId(0x3C)]
            FishermansHut = 0x39,

            [SceneInternalId(0x3D)]
            GoronShop = 0x3A,

            [SceneInternalId(0x3E)]
            DekuKingChamber = 0x3B,

            [SceneInternalId(0x3F)]
            GoronTrial = 0x3C,

            [SceneInternalId(0x40)]
            RoadToSouthernSwamp = 0x3D,

            [SceneInternalId(0x41)]
            DoggyRacetrack = 0x3E,

            [SceneInternalId(0x42)]
            CuccoShack = 0x3F,

            [SceneInternalId(0x43)]
            IkanaGraveyard = 0x40,

            [SceneInternalId(0x44)]
            GohtsLair = 0x41,

            [SceneInternalId(0x45)]
            SouthernSwamp = 0x42,

            [SceneInternalId(0x46)]
            Woodfall = 0x43,

            [SceneInternalId(0x47)]
            ZoraTrial = 0x44,

            [SceneInternalId(0x48)]
            GoronVillageSpring = 0x45,

            [SceneInternalId(0x49)]
            GreatBayTemple = 0x46,

            [SceneInternalId(0x4A)]
            WaterfallRapids = 0x47,

            [SceneInternalId(0x4B)]
            BeneathTheWell = 0x48,

            [SceneInternalId(0x4C)]
            ZoraHallRooms = 0x49,

            [SceneInternalId(0x4D)]
            GoronVillage = 0x4A,

            [SceneInternalId(0x4E)]
            GoronGrave = 0x4B,

            [SceneInternalId(0x4F)]
            SakonsHideout = 0x4C,

            [SceneInternalId(0x50)]
            MountainVillage = 0x4D,

            [SceneInternalId(0x51)]
            PoeHut = 0x4E,

            [SceneInternalId(0x52)]
            DekuShrine = 0x4F,

            [SceneInternalId(0x53)]
            RoadToIkana = 0x50,

            [SceneInternalId(0x54)]
            SwordsmansSchool = 0x51,

            [SceneInternalId(0x55)]
            MusicBoxHouse = 0x52,

            [SceneInternalId(0x56)]
            IgosDuIkanasLair = 0x53,

            [SceneInternalId(0x57)]
            TouristCenter = 0x54,

            [SceneInternalId(0x58)]
            StoneTower = 0x55,

            [SceneInternalId(0x59)]
            InvertedStoneTower = 0x56,

            [SceneInternalId(0x5A)]
            MountainVillageSpring = 0x57,

            [SceneInternalId(0x5B)]
            PathToSnowhead = 0x58,

            [SceneInternalId(0x5C)]
            Snowhead = 0x59,

            [SceneInternalId(0x5D)]
            TwinIslands = 0x5A,

            [SceneInternalId(0x5E)]
            TwinIslandsSpring = 0x5B,

            [SceneInternalId(0x5F)]
            GyorgsLair = 0x5C,

            [SceneInternalId(0x60)]
            SecretShrine = 0x5D,

            [SceneInternalId(0x61)]
            StockPotInn = 0x5E,

            [SceneInternalId(0x62)]
            GreatBayCutscene = 0x5F,

            [SceneInternalId(0x63)]
            ClockTowerInterior = 0x60,

            [SceneInternalId(0x64)]
            WoodsOfMystery = 0x61,

            // LostWoods = 0x62,

            [SceneInternalId(0x66)]
            LinkTrial = 0x63,

            [SceneInternalId(0x67)]
            TheMoon = 0x64,

            [SceneInternalId(0x68)]
            BombShop = 0x65,

            // GiantsChamber = 0x66,

            [SceneInternalId(0x6A)]
            GormanTrack = 0x67,

            [SceneInternalId(0x6B)]
            GoronRacetrack = 0x68,

            [SceneInternalId(0x6C)]
            EastClockTown = 0x69,

            [SceneInternalId(0x6D)]
            WestClockTown = 0x6A,

            [SceneInternalId(0x6E)]
            NorthClockTown = 0x6B,

            [SceneInternalId(0x6F)]
            SouthClockTown = 0x6C,

            [SceneInternalId(0x70)]
            LaundryPool = 0x6D,
        }
        public enum StartingItemMode
        {
            None,
            Random,
            AllowTemporaryItems,
        }
        public enum SmallKeyMode
        {
            Default,

            [Description("Small Key doors will always be open. Small Keys in the item pool will be replaced with other items.")]
            [HackContent(nameof(Resources.mods.key_small_open))]
            DoorsOpen = 1,

            [Description("Randomization algorithm will place any randomized Small Keys into a location within the same region, even if the Small Key has been replaced via another Small Key Mode.")]
            KeepWithinDungeon = 1 << 1,

            [Description("Small Keys will go back in time with Link. Any used Small Keys will return to Link's inventory.")]
            KeepThroughTime = 1 << 2,
        }
        public enum BossKeyMode
        {
            Default,

            [Description("Boss doors will always be open. Boss Keys in the item pool will be replaced with other items.")]
            [HackContent(nameof(Resources.mods.key_boss_open))]
            DoorsOpen = 1,

            [Description("Randomization algorithm will place any randomized Boss Keys into a location within the same region, even if the Boss Key has been replaced via another Boss Key Mode.")]
            KeepWithinDungeon = 1 << 1,

            [Description("Boss Keys will go back in time with Link.")]
            [HackContent(nameof(Resources.mods.key_boss_sot))]
            KeepThroughTime = 1 << 2,
        }
        public enum IceTraps
        {
            None,
            Normal,
            Extra,
            Mayhem,
            Onslaught,
        }
        public enum IceTrapAppearance
        {
            MajorItems,
            JunkItems,
            Anything,
        }
        public enum DamageMode
        {
            Default,
            Double,
            Quadruple,
            OHKO,
            Doom
        }
        public enum ShortenCutsceneGeneral
        {
            None = 0,

            [Description("You don't have to wait for Sakon to leave.")]
            BlastMaskThief = 1 << 0,

            [Description("The minigame will end as soon as you have the required 20 points.")]
            BoatArchery = 1 << 1,

            [Description("The minigame will end as soon as you have the required 20 points.")]
            FishermanGame = 1 << 2,

            [Description("Replaying of the music is shortened.")]
            MilkBarPerformance = 1 << 3,

            [Description("The Hungry Goron doesn't interrupt you when you approach, and you don't have to watch him roll away.")]
            [HackContent(nameof(Resources.mods.shorten_cutscene_don_gero))]
            HungryGoron = 1 << 4,

            [Description("Remove most instances of Tatl interrupting your gameplay.")]
            [HackContent(nameof(Resources.mods.remove_tatl_interrupts))]
            TatlInterrupts = 1 << 5,

            [Description("Skips the irrelevant bank text. Allows using Z/R to set deposit/withdraw amount to min/max.")]
            [HackContent(nameof(Resources.mods.faster_bank_text))]
            FasterBankText = 1 << 6,

            [Description("The owl in Goron Village will no longer trigger dialog when you approach. You must target it and talk to it if you want it to reveal the path.")]
            [HackContent(nameof(Resources.mods.shorten_cutscene_gv_owl))]
            GoronVillageOwl = 1 << 7,

            [Description("The dialog of the credits will proceed automatically.")]
            AutomaticCredits = 1 << 8,

            [Description("Various cutscenes are skipped or otherwise shortened.")]
            [HackContent(nameof(Resources.mods.short_cutscenes))]
            EverythingElse = 1 << 31,
        }
        public enum ShortenCutsceneBossIntro
        {
            None = 0,

            [Description("Odolwa is ready to fight you right away.")]
            [HackContent(nameof(Resources.mods.shorten_cutscene_odolwa_intro))]
            Odolwa = 1 << 0,

            [Description("Link doesn't look around in surprise when you enter Goht's room.")]
            [HackContent(nameof(Resources.mods.shorten_cutscene_goht_intro))]
            Goht = 1 << 1,

            [Description("Gyorg is ready to fight you right away.")]
            [HackContent(nameof(Resources.mods.shorten_cutscene_gyorg_intro))]
            Gyorg = 1 << 2,

            [Description("Twinmold is ready to fight you right away.")]
            [HackContent(nameof(Resources.mods.shorten_cutscene_twinmold_intro))]
            Twinmold = 1 << 3,

            [Description("Majora is ready to fight you right away.")]
            [HackContent(nameof(Resources.mods.shorten_cutscene_majora_intro))]
            Majora = 1 << 4,

            [Description("You don't have to look at Wart to get him down from the ceiling. It spawns on the ground.")]
            [HackContent(nameof(Resources.mods.shorten_cutscene_wart_intro))]
            Wart = 1 << 5,

            [Description("Igos du Ikana and his henchmen are ready to fight you right away.")]
            [HackContent(nameof(Resources.mods.shorten_cutscene_igos_intro))]
            IgosDuIkana = 1 << 6,

            [Description("Gomess and his bats are ready to fight you right away.")]
            [HackContent(nameof(Resources.mods.shorten_cutscene_gomess_intro))]
            Gomess = 1 << 7,
        }
        public enum Character
        {
            LinkMM,
            LinkOOT,
            AdultLink,
            Kafei
        }
        public enum GossipHintStyle
        {
            Default,
            Random,
            Relevant,
            Competitive,
        }
        public enum DamageEffect
        {
            Default,
            Fire,
            Ice,
            Shock,
            Knockdown,
            Random
        }
        public enum MovementMode
        {
            Default,
            HighSpeed,
            SuperLowGravity,
            LowGravity,
            HighGravity
        }
        public enum StrayFairyMode
        {
            Default,

            [Description("Stray Fairies in the item pool will be replaced with other items. Non-chest fairies (roaming, bubbles, beehives, etc.) are removed. Chests that ordinarily have a Stray Fairy will behave like normal chests.")]
            [HackContent(nameof(Resources.mods.fairies_chests_only))]
            ChestsOnly,

            [Description("Randomization algorithm will place any randomized Stray Fairies into a location within the same region, even if the Stray Fairy has been replaced via another Dungeon Fairy Mode.")]
            KeepWithinDungeon,
        }
        public enum PriceMode
        {
            None,

            [Description("Prices for item purchases will be randomized.")]
            Purchases = 1,

            [Description("Prices for minigames will be randomized.")]
            Minigames = 2,

            [Description("Prices for other miscellaneous spending will be randomized.")]
            Misc = 4,
        }
        public enum ItemCategory
        {
            Fake = -1,
            None,

            [Description("Randomize items on the main inventory screen other than trade items. Also includes Bombers' Notebook, Swords, Mirror Shield and Wallets.")]
            MainInventory,

            [Description("Randomize songs except Song of Soaring.")]
            Songs,

            [Description("Randomize the Song of Soaring.")]
            SongOfSoaring,

            [Description("Randomize Heart Containers.")]
            HeartContainers,

            [Description("Randomize Boss Remains.")]
            BossRemains,

            [Description("Randomize Pieces of Heart.")]
            PiecesOfHeart,

            [Description("Randomize Masks.")]
            Masks,

            [Description("Randomize Moon's Tear, Title Deeds, Letter to Kafei, Pendant of Memories, Room Key and Letter to Mama.")]
            TradeItems,

            [Description("Randomize Dungeon Maps/Compasses and overworld maps.")]
            Navigation,

            [Description("Randomize non-item Great Fairy rewards including Magic Power, Great Spin Attack, Extended Magic Power and Double Defense.")]
            MagicPowers,

            [Description("Randomize golden skulltula tokens. Tokens will not reset to 0 after Song of Time.")]
            SkulltulaTokens,

            [Description("Randomize stray fairies including the Clock Town stray fairy. Stray fairies will not reset to 0 after Song of Time.")]
            StrayFairies,

            [Description("Randomize small keys and boss keys.")]
            DungeonKeys,

            [Description("Randomize Gold Rupees.")]
            GoldRupees,

            [Description("Randomize Silver Rupees.")]
            SilverRupees,

            [Description("Randomize Purple Rupees.")]
            PurpleRupees,

            [Description("Randomize Red Rupees.")]
            RedRupees,

            [Description("Randomize Blue Rupees.")]
            BlueRupees,

            [Description("Randomize Green Rupees.")]
            GreenRupees,

            [Description("Randomize the recovery hearts in Pirates' Fortress.")]
            RecoveryHearts,

            [Description("Randomize large and small magic jars.")]
            MagicJars,

            [Description("Randomize Hero's Shields.")]
            Shields,

            [Description("Randomize Bombchu.")]
            Bombchu,

            [Description("Randomize Arrows.")]
            Arrows,

            [Description("Randomize Bombs.")]
            Bombs,

            [Description("Randomize Deku Nuts.")]
            DekuNuts,

            [Description("Randomize Deku Sticks.")]
            DekuSticks,

            [Description("Randomize Milk.")]
            Milk,

            [Description("Randomize Red Potions.")]
            RedPotions,

            [Description("Randomize Green Potions.")]
            GreenPotions,

            [Description("Randomize Blue Potions.")]
            BluePotions,

            [Description("Randomize the Chateau refill. Bottle with Chateau Romani is part of Main Inventory")]
            Chateau,

            [Description("Randomize the Seahorse.")]
            Seahorse,

            [Description("Randomize the Fairy purchase in the Trading Post.")]
            Fairy,

            [Description("Randomize bottle scoops.")]
            ScoopedItems,

            [Description("Randomize Ocarina and Song of Time.")]
            TimeTravel,
        }

        public enum LocationCategory
        {
            Fake = -1,
            None,

            [Description("Randomize chests that don't fit into the other categories.")]
            Chests,

            [Description("Randomize items rewarded by NPCs except Minigames.")]
            NpcRewards,

            [Description("Randomize freestanding items.")]
            Freestanding,

            [Description("Randomize purchases including shops, scrubs, tingle, bean man, milk bar and Gorman Bros.")]
            Purchases,

            [Description("Randomize starting items.")]
            StartingItems,

            [Description("Randomize items rewarded from minigames.")]
            Minigames,

            [Description("Randomize items earned by fighting bosses/minibosses.")]
            BossFights,

            [Description("Randomize items on The Moon.")]
            MoonItems,

            [Description("Randomize items spawned by enemies including freestanding Golden Skulltulas, enemies that normally spawn Stray Fairies and Takkuri.")]
            EnemySpawn,

            [Description("Randomize fixed dropped from grass. Only Keaton Grass and grass such as that near owl statues drop fixed items.")]
            Grass,

            [Description("Randomize fixed drops from jars including small jars and green jars.")]
            Jars,

            [Description("Randomize fixed drops from small and large crates.")]
            Crates,

            [Description("Randomize fixed drops from small snowballs.")]
            SmallSnowballs,

            [Description("Randomize fixed drops from large snowballs.")]
            LargeSnowballs,

            [Description("Randomize fixed drops from barrels. This includes items that already exist within barrels before they're destroyed.")]
            Barrels,

            [Description("Randomize fixed drops from beehives.")]
            Beehives,

            [Description("Randomize invisible items.")]
            InvisibleItems,

            [Description("Randomize items spawned by events including the Moon's Tear, the Sword School Gong, the Song Wall in Termina Field, the Telescope Guay and the Termina Field circling Guay.")]
            Events,

            [Description("Randomize items from soft soil.")]
            SoftSoil,

            [Description("Randomize items dropped by hitting specific spots in the game.")]
            HitSpots,

            [Description("Randomize fixed drops from rocks. Only rocks on walls drop fixed items. Also includes the item within the Red Rock in Mountain Village spring time.")]
            Rocks,

            [Description("Randomize bottle scoops.")]
            Scoops,

            [Description("Randomize items that require glitches to collect.")]
            GlitchesRequired,
        }
    }
}
