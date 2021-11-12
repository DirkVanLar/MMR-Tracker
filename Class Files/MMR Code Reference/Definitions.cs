using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker.Class_Files.MMR_Code_Reference.items;
using static MMR_Tracker.Class_Files.MMR_Code_Reference.MMRTGameplaySettings;
using static MMR_Tracker.Class_Files.MMR_Code_Reference.OtherObjects;

namespace MMR_Tracker.Class_Files.MMR_Code_Reference
{
    class Definitions
    {
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class StartingItemAttribute : Attribute
        {
            public int Address { get; }
            public byte Value { get; }
            public bool IsAdditional { get; }

            public StartingItemAttribute(int address, byte value, bool isAdditional = false)
            {
                Address = address;
                Value = value;
                IsAdditional = isAdditional;
            }
        }
        public class ItemNameAttribute : Attribute
        {
            public string Name { get; private set; }

            public ItemNameAttribute(string name)
            {
                Name = name;
            }
        }
        public class LocationNameAttribute : Attribute
        {
            public string Name { get; private set; }

            public LocationNameAttribute(string name)
            {
                Name = name;
            }
        }
        public class RegionAttribute : Attribute
        {
            public Region Region { get; private set; }

            public RegionAttribute(Region region)
            {
                Region = region;
            }
        }
        public class RegionNameAttribute : Attribute
        {
            public string Name { get; private set; }

            public RegionNameAttribute(string name)
            {
                Name = name;
            }
        }
        public class GossipLocationHintAttribute : Attribute
        {
            public string[] Values { get; }

            public GossipLocationHintAttribute(string value, params string[] values)
            {
                var list = values.ToList();
                list.Add(value);
                Values = list.ToArray();
            }
        }
        public class GossipItemHintAttribute : Attribute
        {
            public string[] Values { get; }

            public GossipItemHintAttribute(string value, params string[] values)
            {
                var list = values.ToList();
                list.Add(value);
                Values = list.ToArray();
            }
        }
        public class ShopTextAttribute : Attribute
        {
            public string Default { get; private set; }
            public string WitchShop { get; private set; }
            public string TradingPostMain { get; private set; }
            public string TradingPostPartTimer { get; private set; }
            public string CuriosityShop { get; private set; }
            public string BombShop { get; private set; }
            public string ZoraShop { get; private set; }
            public string GoronShop { get; private set; }
            public string GoronShopSpring { get; private set; }

            public bool IsMultiple { get; private set; }
            public bool IsDefinite { get; private set; }

            public ShopTextAttribute(string defaultText,
                string witchShop = null,
                string tradingPostMain = null,
                string tradingPostPartTimer = null,
                string curiosityShop = null,
                string bombShop = null,
                string zoraShop = null,
                string goronShop = null,
                string goronSpringShop = null,
                bool isMultiple = false,
                bool isDefinite = false)
            {
                Default = defaultText;
                WitchShop = witchShop;
                TradingPostMain = tradingPostMain;
                TradingPostPartTimer = tradingPostPartTimer;
                CuriosityShop = curiosityShop;
                BombShop = bombShop;
                ZoraShop = zoraShop;
                GoronShop = goronShop;
                GoronShopSpring = goronSpringShop;
                IsMultiple = isMultiple;
                IsDefinite = isDefinite;
            }
        }
        public class ChestTypeAttribute : Attribute
        {
            public ChestType Type { get; private set; }

            public ChestTypeAttribute(ChestType type)
            {
                Type = type;
            }

            public enum ChestType
            {
                SmallWooden = 0,
                SmallGold = 1,
                LargeGold = 2,
                BossKey = 3,
            }
        }
        public class GetItemIndexAttribute : Attribute
        {
            public ushort Index { get; }

            public GetItemIndexAttribute(ushort index)
            {
                Index = index;
            }
        }
        public class ItemPoolAttribute : Attribute
        {
            public ItemCategory ItemCategory { get; }
            public LocationCategory LocationCategory { get; }

            public ItemPoolAttribute(ItemCategory itemCategory, LocationCategory locationCategory)
            {
                ItemCategory = itemCategory;
                LocationCategory = locationCategory;
            }
        }
        public class ProgressiveAttribute : Attribute
        {
        }
        public class PurchaseableAttribute : Attribute
        {
        }
        public class VisibleAttribute : Attribute
        {
        }
        public class ChestAttribute : Attribute
        {
            public int[] Addresses { get; private set; }
            public AppearanceType Type { get; private set; }

            public ChestAttribute(int address, AppearanceType type = AppearanceType.Normal, params int[] additionalAddresses)
            {
                Addresses = additionalAddresses.Concat(new[] { address }).ToArray();
                Type = type;
            }

            public enum AppearanceType
            {
                Normal = 0,
                Invisible = 1,
                AppearsClear = 2,
                AppearsSwitch = 3,
            }

            public static byte GetType(ChestTypeAttribute.ChestType chestType, AppearanceType appearanceType)
            {
                var type = (byte)chestType;
                type <<= 2;
                type += (byte)appearanceType;
                return type;
            }
        }

        public class GrottoChestAttribute : Attribute
        {
            public int[] Addresses { get; private set; }

            public GrottoChestAttribute(params int[] addresses)
            {
                Addresses = addresses;
            }
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class ShopInventoryAttribute : Attribute
        {
            private const int BaseShopInventoryDataAddress = 0x00CDDC60;

            public int ShopItemAddress { get; private set; }
            public ShopKeeper Keeper { get; private set; }

            public ShopInventoryAttribute(ShopKeeper shopKeeper, int shopItemIndex)
            {
                ShopItemAddress = BaseShopInventoryDataAddress + (int)shopKeeper + (shopItemIndex * 0x20);
                Keeper = shopKeeper;
            }

            public enum ShopKeeper
            {
                WitchShop = 0x11E0,
                TradingPostMain = 0x1240,
                TradingPostPartTimer = 0x1340,
                CuriosityShop = 0x1440,
                BombShop = 0x14C0,
                ZoraShop = 0x1540,
                GoronShop = 0x15A0,
                GoronShopSpring = 0x1600,
            }
        }
        public class RepeatableAttribute : Attribute
        {
        }
        public class TemporaryAttribute : Attribute
        {
        }
        public class StartingItemIdAttribute : Attribute
        {
            public byte ItemId { get; }

            public StartingItemIdAttribute(byte itemId)
            {
                ItemId = itemId;
            }
        }
        public class HackContentAttribute : Attribute
        {
            public byte[] HackContent { get; }
            public bool ApplyOnlyIfItemIsDifferent { get; }

            public HackContentAttribute(string modResourcePropertyName, bool applyOnlyIfItemIsDifferent = true)
            {

            }
        }
        public class GossipCompetitiveHintAttribute : Attribute
        {
            public int Priority { get; private set; }
            public Func<MMR_Tracker_V2.LogicObjects.GameplaySettings, bool> Condition { get; private set; }

            public GossipCompetitiveHintAttribute(int priority = 0)
            {
                Priority = priority;
            }

            public GossipCompetitiveHintAttribute(int priority, string settingProperty, object settingValue)
            {
                Priority = priority;

                if (settingProperty != null)
                {

                }
            }

            public GossipCompetitiveHintAttribute(int priority, ItemCategory itemCategory, bool doesContain)
            {

            }

            public GossipCompetitiveHintAttribute(int priority, ItemCategory itemCategory, bool doesContain, string settingFlagProperty, int flagValue, bool hasFlag)
            {

            }
            public class ShortenCutsceneSettings
            {
                public ShortenCutsceneGeneral General { get; set; } =
                    ShortenCutsceneGeneral.BlastMaskThief
                    | ShortenCutsceneGeneral.BoatArchery
                    | ShortenCutsceneGeneral.FishermanGame
                    | ShortenCutsceneGeneral.MilkBarPerformance
                    | ShortenCutsceneGeneral.HungryGoron
                    | ShortenCutsceneGeneral.TatlInterrupts
                    | ShortenCutsceneGeneral.FasterBankText
                    | ShortenCutsceneGeneral.GoronVillageOwl
                    | ShortenCutsceneGeneral.AutomaticCredits
                    | ShortenCutsceneGeneral.EverythingElse
                    ;

                public ShortenCutsceneBossIntro BossIntros { get; set; } =
                    ShortenCutsceneBossIntro.Odolwa
                    | ShortenCutsceneBossIntro.Goht
                    | ShortenCutsceneBossIntro.Gyorg
                    | ShortenCutsceneBossIntro.Majora
                    | ShortenCutsceneBossIntro.IgosDuIkana
                    | ShortenCutsceneBossIntro.Gomess;
            }

        }
        public class OverwritableAttribute : Attribute
        {
        }
        public class GossipCombineOrderAttribute : Attribute
        {
            public int Order { get; }

            public GossipCombineOrderAttribute(int order)
            {
                Order = order;
            }
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class GossipCombineAttribute : Attribute
        {
            public Item OtherItem { get; }
            public string CombinedName { get; }

            public GossipCombineAttribute(Item otherItem, string combinedName = null)
            {
                OtherItem = otherItem;
                CombinedName = combinedName;
            }
        }
        public class RupeeRepeatableAttribute : Attribute
        {
        }
        public class DowngradableAttribute : Attribute
        {
        }
        public class MultiLocationAttribute : Attribute
        {
            public Item[] Locations { get; }

            public MultiLocationAttribute(params Item[] locations)
            {
                Locations = locations;
            }
        }
        public class EntranceNameAttribute : Attribute
        {
            public string Name { get; private set; }

            public EntranceNameAttribute(string name)
            {
                Name = name;
            }
        }
        public class DungeonEntranceAttribute : Attribute
        {
            public DungeonEntrance Entrance { get; }
            public DungeonEntrance? Pair { get; }

            public DungeonEntranceAttribute(DungeonEntrance entrance)
            {
                Entrance = entrance;
            }

            public DungeonEntranceAttribute(DungeonEntrance entrance, DungeonEntrance pair)
            {
                Entrance = entrance;
                Pair = pair;
            }
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class ExitAttribute : Attribute
        {
            public int SceneId { get; private set; }
            public byte ExitIndex { get; private set; }

            public ExitAttribute(Scene scene, byte exitIndex)
            {
            }
        }
        public class SpawnAttribute : Attribute
        {
            public ushort SpawnId { get; private set; }
            public SpawnAttribute(Scene scene, byte spawnIndex)
            {
                SpawnId = (ushort)(((byte)scene << 9) + (spawnIndex << 4));
            }
        }
        public class SceneInternalIdAttribute : Attribute
        {
            public byte InternalId { get; private set; }

            public SceneInternalIdAttribute(byte internalId)
            {
                InternalId = internalId;
            }
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class ExitAddressAttribute : Attribute
        {
            public int Address { get; private set; }
            public ExitAddressAttribute(int address)
            {
                Address = address;
            }

            public enum BaseAddress
            {
                SongOfSoaring = 0xF587AC,
            }
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class ExitCutsceneAttribute : Attribute
        {
            public int SceneId { get; private set; }
            public byte SetupIndex { get; private set; }
            public byte CutsceneIndex { get; private set; }

            public ExitCutsceneAttribute(Scene scene, byte setupIndex, byte cutsceneIndex)
            {
            }
        }
        public class ReturnableAttribute : Attribute
        {
            public Func<GameplaySettings, bool> Condition { get; private set; }

            public ReturnableAttribute(string settingFlagProperty, int flagValue, bool hasFlag)
            {
                var parameter = Expression.Parameter(typeof(GameplaySettings));

                // settings => (((int)settings[settingFlagProperty] & flagValue) != 0) == hasFlag
                var flagExpression = Expression.Equal(
                    Expression.NotEqual(
                        Expression.And(
                            Expression.Convert(Expression.Property(parameter, settingFlagProperty), typeof(int)),
                            Expression.Constant(flagValue)
                            ),
                        Expression.Constant(0)
                        ),
                    Expression.Constant(hasFlag)
                );
                Condition = Expression.Lambda<Func<GameplaySettings, bool>>(flagExpression, parameter).Compile();
            }
        }
        public class GetBottleItemIndicesAttribute : Attribute
        {
            public int[] Indices { get; private set; }

            public GetBottleItemIndicesAttribute(params int[] indices)
            {
                Indices = indices;
            }
        }
        [AttributeUsage(AttributeTargets.Field)]
        public class StartingTingleMapAttribute : Attribute
        {
            public TingleMap TingleMap { get; }

            public StartingTingleMapAttribute(TingleMap tingleMap)
            {
                TingleMap = tingleMap;
            }
        }
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class CollectableIndexAttribute : Attribute
        {
            public ushort Index { get; }

            public CollectableIndexAttribute(ushort index)
            {
                Index = index;
            }
        }
        public class NullableItemAttribute : Attribute
        {
        }
        public class MainLocationAttribute : Attribute
        {
            public Item Location { get; }

            public MainLocationAttribute(Item location)
            {
                Location = location;
            }
        }
        public class ExclusiveItemMessageAttribute : Attribute
        {
            public ushort Id { get; private set; }
            public string Message { get; private set; }

            public ExclusiveItemMessageAttribute(ushort id, string message = null)
            {
                this.Id = id;
                this.Message = message;
            }
        }
        public class ExclusiveItemAttribute : Attribute
        {
            public byte Item { get; private set; }
            public byte Flags { get; private set; }
            public byte Type { get; private set; }

            public ExclusiveItemAttribute(byte item, byte flags = 0, byte type = 0)
            {
                this.Item = item;
                this.Flags = flags;
                this.Type = type;
            }
        }
        public class ExclusiveItemGraphicAttribute : Attribute
        {
            public byte Graphic { get; private set; }
            public ushort Object { get; private set; }

            public ExclusiveItemGraphicAttribute(byte graphic, ushort obj)
            {
                this.Graphic = graphic;
                this.Object = obj;
            }
        }
    }
}
