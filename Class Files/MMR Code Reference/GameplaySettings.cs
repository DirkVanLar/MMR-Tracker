using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker.Class_Files.MMR_Code_Reference.Definitions.GossipCompetitiveHintAttribute;
using static MMR_Tracker.Class_Files.MMR_Code_Reference.items;
using static MMR_Tracker.Class_Files.MMR_Code_Reference.OtherObjects;

namespace MMR_Tracker.Class_Files.MMR_Code_Reference
{
    class MMRTGameplaySettings
    {

        public class GameplaySettings
        {
            #region General settings

            /// <summary>
            /// Filepath to the input logic file
            /// </summary>
            public string UserLogicFileName { get; set; } = "";

            public string Logic { get; set; }

            /// <summary>
            /// Options for the Asm <see cref="Patcher"/>.
            /// </summary>
            [JsonIgnore]
            //public AsmOptionsGameplay AsmOptions { get; set; } = new AsmOptionsGameplay();

            #endregion

            #region Asm Getters / Setters

            /// <summary>
            /// Whether or not to change Cow response behavior.
            /// </summary>
            public bool CloseCows
            {get;set;}

            /// <summary>
            /// Whether or not to enable cycling arrow types while using the bow.
            /// </summary>
            public bool ArrowCycling
            { get; set; }

            /// <summary>
            /// Whether or not to disable crit wiggle.
            /// </summary>
            public bool CritWiggleDisable
            { get; set; }

            /// <summary>
            /// Whether or not to draw hash icons on the file select screen.
            /// </summary>
            public bool DrawHash
            { get; set; }

            /// <summary>
            /// Whether or not to apply Elegy of Emptiness speedups.
            /// </summary>
            public bool ElegySpeedup
            { get; set; }

            /// <summary>
            /// Whether or not to enable faster pushing and pulling speeds.
            /// </summary>
            public bool FastPush
            { get; set; }

            /// <summary>
            /// Whether or not ice traps should behave slightly differently from other items in certain situations.
            /// </summary>
            public bool IceTrapQuirks
            { get; set; }

            /// <summary>
            /// Whether or not to enable freestanding models.
            /// </summary>
            public bool UpdateWorldModels
            { get; set; }

            /// <summary>
            /// Whether or not to allow using the ocarina underwater.
            /// </summary>
            public bool OcarinaUnderwater
            { get; set; }

            /// <summary>
            /// Whether or not to enable Quest Item Storage.
            /// </summary>
            public bool QuestItemStorage
            { get; set; }

            /// <summary>
            /// Whether or not to enable Continuous Deku Hopping.
            /// </summary>
            public bool ContinuousDekuHopping
            { get; set; }

            /// <summary>
            /// Updates shop models and text
            /// </summary>
            public bool UpdateShopAppearance
            { get; set; }

            /// <summary>
            /// Updates shop models and text
            /// </summary>
            public bool ProgressiveUpgrades
            { get; set; }

            public bool TargetHealthBar
            { get; set; }

            public bool ClimbMostSurfaces
            { get; set; }

            /// <summary>
            /// Whether or not to enable spawning scarecrow without Scarecrow's Song.
            /// </summary>
            public bool FreeScarecrow
            { get; set; }

            public bool DoubleArcheryRewards
            { get; set; }

            #endregion

            #region Random Elements

            /// <summary>
            /// Selected mode of logic (affects randomization rules)
            /// </summary>
            public LogicMode LogicMode { get; set; }

            public List<int> EnabledTricks { get; set; } = new List<int> { 1205, 1209, 1171, 1207, 1194, 1161, 1167, 1160, 1223, 1159 };

            /// <summary>
            /// Add songs to the randomization pool
            /// </summary>
            public bool AddSongs { get; set; }

            /// <summary>
            /// Randomize which dungeon you appear in when entering one
            /// </summary>
            public bool RandomizeDungeonEntrances { get; set; }

            /// <summary>
            /// (Beta) Randomize enemies
            /// </summary>
            public bool RandomizeEnemies { get; set; }

            /// <summary>
            /// Set how starting items are randomized
            /// </summary>
            public StartingItemMode StartingItemMode { get; set; }

            public SmallKeyMode SmallKeyMode { get; set; } = SmallKeyMode.DoorsOpen;

            public BossKeyMode BossKeyMode { get; set; }

            public StrayFairyMode StrayFairyMode { get; set; }

            public PriceMode PriceMode { get; set; }


            /// <summary>
            ///  Custom item list selections
            /// </summary>
            [JsonIgnore]
            public HashSet<Item> CustomItemList { get; set; } = new HashSet<Item>();

            public List<ItemCategory> ItemCategoriesRandomized { get; set; }

            public List<LocationCategory> LocationCategoriesRandomized { get; set; }

            /// <summary>
            ///  Custom item list string
            /// </summary>
            public string CustomItemListString { get; set; } = "--------------------206-40000000----10ffff-ffffffff-ffffffff-f8000000-3ddf77fd-3fffffff-f378ffff-ffffffff";

            /// <summary>
            ///  Custom starting item list selections
            /// </summary>
            [JsonIgnore]
            public List<Item> CustomStartingItemList { get; set; } = new List<Item>();

            /// <summary>
            ///  Custom starting item list string
            /// </summary>
            public string CustomStartingItemListString { get; set; } = "fdfe-2c00000-";

            /// <summary>
            /// List of locations that must be randomized to junk
            /// </summary>
            [JsonIgnore]
            public List<Item> CustomJunkLocations { get; set; } = new List<Item>();

            /// <summary>
            ///  Custom junk location string
            /// </summary>
            public string CustomJunkLocationsString { get; set; } = "-------------------------100000-----200000--f000";

            /// <summary>
            /// Defines number of ice traps.
            /// </summary>
            public IceTraps IceTraps { get; set; }

            /// <summary>
            /// Defines appearance pool for visible ice traps.
            /// </summary>
            public IceTrapAppearance IceTrapAppearance { get; set; }

            #endregion

            #region Gimmicks

            /// <summary>
            /// Modifies the damage value when Link is damaged
            /// </summary>
            public DamageMode DamageMode { get; set; }

            /// <summary>
            /// Adds an additional effect when Link is damaged
            /// </summary>
            public DamageEffect DamageEffect { get; set; }

            /// <summary>
            /// Modifies Link's movement
            /// </summary>
            public MovementMode MovementMode { get; set; }

            /// <summary>
            /// Sets the type of floor globally
            /// </summary>
            public FloorType FloorType { get; set; }

            public NutAndStickDrops NutandStickDrops { get; set; }

            /// <summary>
            /// Sets the clock speed.
            /// </summary>
            public ClockSpeed ClockSpeed { get; set; } = ClockSpeed.Default;

            /// <summary>
            /// Hides the clock UI.
            /// </summary>
            public bool HideClock { get; set; }

            /// <summary>
            /// Increases or decreases the cooldown of using the blast mask
            /// </summary>
            public BlastMaskCooldown BlastMaskCooldown { get; set; }

            /// <summary>
            /// Enables Sun's Song
            /// </summary>
            public bool EnableSunsSong { get; set; }

            /// <summary>
            /// Allow's using Fierce Deity's Mask anywhere
            /// </summary>
            public bool AllowFierceDeityAnywhere { get; set; }

            /// <summary>
            /// Arrows, Bombs, and Bombchu will not be provided. You must bring your own. Logic Modes other than No Logic will account for this.
            /// </summary>
            public bool ByoAmmo { get; set; }

            /// <summary>
            /// Dying causes the moon to crash, with all that that implies.
            /// </summary>
            public bool DeathMoonCrash { get; set; }

            public bool HookshotAnySurface { get; set; }

            #endregion

            #region Comfort / Cosmetics

            /// <summary>
            /// Certain cutscenes will play shorter, or will be skipped
            /// </summary>
            public ShortenCutsceneSettings ShortenCutsceneSettings { get; set; }

            /// <summary>
            /// Text is fast-forwarded
            /// </summary>
            public bool QuickTextEnabled { get; set; } = true;

            /// <summary>
            /// Replaces Link's default model
            /// </summary>
            public Character Character { get; set; }

            /// <summary>
            /// Method to write the gossip stone hints.
            /// </summary>
            public GossipHintStyle GossipHintStyle { get; set; } = GossipHintStyle.Competitive;

            /// <summary>
            /// FrEe HiNtS FoR WeEnIeS
            /// </summary>
            public bool FreeHints { get; set; } = true;

            /// <summary>
            /// Clear hints
            /// </summary>
            public bool ClearHints { get; set; } = true;

            /// <summary>
            /// Prevent downgrades
            /// </summary>
            public bool PreventDowngrades { get; set; } = true;

            /// <summary>
            /// Updates chest appearance to match contents
            /// </summary>
            public bool UpdateChests { get; set; }

            /// <summary>
            /// Change epona B button behavior to prevent player losing sword if they don't have a bow.
            /// </summary>
            public bool FixEponaSword { get; set; } = true;

            /// <summary>
            /// Enables Pictobox prompt text to display the picture subject depending on flags.
            /// </summary>
            public bool EnablePictoboxSubject { get; set; } = true;

            public bool LenientGoronSpikes { get; set; }

            #endregion

            #region Speedups

            /// <summary>
            /// Change beavers so the player doesn't have to race the younger beaver.
            /// </summary>
            public bool SpeedupBeavers { get; set; } = true;

            /// <summary>
            /// Change the dampe flames to always have 2 on ground floor and one up the ladder.
            /// </summary>
            public bool SpeedupDampe { get; set; } = true;

            /// <summary>
            /// Change dog race to make gold dog always win if the player has the Mask of Truth
            /// </summary>
            public bool SpeedupDogRace { get; set; } = true;

            /// <summary>
            /// Change the Lab Fish to only need to be fed one fish.
            /// </summary>
            public bool SpeedupLabFish { get; set; } = true;

            /// <summary>
            /// Change the Bank reward thresholds to 200/500/1000 instead of 200/1000/5000.
            /// </summary>
            public bool SpeedupBank { get; set; } = true;

            #endregion

            #region Functions

            public override string ToString()
            {
                return null;
            }

            public string Validate()
            {
                return null;
            }

            #endregion
        }
    }
}
