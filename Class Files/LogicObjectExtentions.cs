using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker.Class_Files
{
    public static class LogicObjectExtentions
    {
        //Logic Entry Extentions
        public static bool IsEntrance(this LogicObjects.LogicEntry entry)
        {
            return entry.ItemSubType == "Entrance";
        }
        public static bool IsGossipStone(this LogicObjects.LogicEntry entry)
        {
            return entry.ItemSubType.StartsWith("Gossip");
        }
        public static LogicObjects.LogicEntry RandomizedEntry(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool ReturnJunkAsItem = false)
        {
            if (ReturnJunkAsItem && entry.HasRandomItem(false) && !entry.HasRandomItem(true))
            { return new LogicObjects.LogicEntry { ID = -1, DictionaryName = "Junk", DisplayName = "Junk", LocationName = "Junk", ItemName = "Junk" }; }
            if (!entry.HasRandomItem(true) || entry.RandomizedItem >= Instance.Logic.Count) { return null; }
            return Instance.Logic[entry.RandomizedItem];
        }
        public static LogicObjects.LogicEntry PairedEntry(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool RandomizedItem = false)
        {
            var Pairs = Instance.EntrancePairs;
            int ID = (RandomizedItem) ? entry.RandomizedItem : entry.ID;
            if (Pairs.ContainsKey(ID) && Pairs[ID] < Instance.Logic.Count) { return Instance.Logic[Pairs[ID]]; }
            return null;
        }
        public static LogicObjects.LogicEntry ClearRandomizedDungeonInThisArea(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            //Finds the area clear related to the dungeon that is randomized to the current area.
            //If woodfall entrance leads to snowhead and you pass this function woodfall clear it will return snowhead clear.
            if (!Instance.EntranceAreaDic.ContainsKey(entry.ID)) { return null; }
            var templeEntrance = Instance.EntranceAreaDic[entry.ID];//What is the dungeon entrance in this area
            var RandTempleEntrance = Instance.Logic[templeEntrance].RandomizedItem;//What dungeon does this areas dungeon entrance lead to
            var RandAreaClear = RandTempleEntrance < 0 ? -1 : Instance.EntranceAreaDic.FirstOrDefault(x => x.Value == RandTempleEntrance).Key;//What is the Area clear Value For That Dungeon
            var RandClearLogic = RandAreaClear == -1 ? null : Instance.Logic[RandAreaClear]; //Get the full logic data for the area clear that we want to check the availability of.
            return RandClearLogic;
        }
        public static bool HasRandomItem(this LogicObjects.LogicEntry entry, bool RealItem)
        {
            return (RealItem) ? entry.RandomizedItem > -1 : entry.RandomizedItem > -2;
        }
        public static bool Unrandomized(this LogicObjects.LogicEntry entry, int UnRand0Manual1Either2 = 0)
        {
            if (UnRand0Manual1Either2 == 0) { return entry.RandomizedState() == 1; }
            if (UnRand0Manual1Either2 == 1) { return entry.RandomizedState() == 2; }
            if (UnRand0Manual1Either2 == 2) { return entry.RandomizedState() == 1 || entry.RandomizedState() == 2; }
            return false;
        }
        public static bool Randomized(this LogicObjects.LogicEntry entry)
        {
            return entry.RandomizedState() == 0;
        }
        public static int RandomizedState(this LogicObjects.LogicEntry entry)
        {
            int option = entry.Options;
            return (entry.Options > 3) ? option - 4 : option;
        }
        public static bool StartingItem(this LogicObjects.LogicEntry entry)
        {
            return (entry.Options > 3);
        }
        public static bool IsWarpSong(this LogicObjects.LogicEntry entry)
        {
            return (entry.LocationArea == "Owl Warp");
        }
        public static bool AppearsInListbox(this LogicObjects.LogicEntry entry)
        {
            return (entry.Randomized() || entry.Unrandomized(1)) && !entry.IsFake && !string.IsNullOrWhiteSpace(entry.LocationName);
        }
        public static bool LogicItemAquired(this LogicObjects.LogicEntry entry)
        {
            return (entry.Aquired || entry.StartingItem() || (entry.Unrandomized() && entry.Available));
        }
        public static LogicObjects.LogicEntry GetItemsNewLocation(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> Logic)
        {
            return Logic.Find(x => x.RandomizedItem == entry.ID);
        }
        public static LogicObjects.LogicEntry GetItemsSpoilerLocation(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> Logic)
        {
            return Logic.Find(x => x.SpoilerRandom == entry.ID);
        }
        public static bool ItemHasBeenPlaced(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> Logic)
        {
            return Logic.Where(x => x.RandomizedItem == entry.ID || x.SpoilerRandom == entry.ID).Any();
        }
        public static bool UserCreatedFakeItem(this LogicObjects.LogicEntry entry, List<LogicObjects.LogicEntry> Logic)
        {
            int lastRealItem = -1;
            foreach (var i in Logic)
            {
                if (!i.IsFake) { lastRealItem = i.ID; }
            }
            return (entry.ID > lastRealItem);
        }
        public static bool ItemBelongsToMe(this LogicObjects.LogicEntry entry)
        {
            if (!LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { return true; }
            if (entry.IsEntrance()) { return true; }
            return entry.PlayerData.ItemBelongedToPlayer == -1 || entry.PlayerData.ItemBelongedToPlayer == LogicObjects.MainTrackerInstance.Options.MyPlayerID;
        }
        public static bool IsProgressiveItem(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            return entry.ProgressiveItemSet(Instance) != null;
        }
        public static string ProgressiveItemName(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            if (entry.IsProgressiveItem(Instance) && Instance.Options.ProgressiveItems && Instance.IsMM())
            {
                return (entry.SpoilerItem.Count() > 1) ? entry.SpoilerItem[1] : entry.ItemName ?? entry.DictionaryName;
            }
            return entry.ItemName ?? entry.DictionaryName;
        }
        public static int ProgressiveItemsNeeded(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool IndexValue = false)
        {
            var set = entry.ProgressiveItemSet(Instance);
            if (set == null) { return 0; }
            int offset = (IndexValue) ? 0 : 1;
            return set.IndexOf(entry) + offset;
        }
        public static int ProgressiveItemsAquired(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool Unique = true)
        {
            var set = entry.ProgressiveItemSet(Instance).Where(x => x.LogicItemAquired()).ToList();
            var setIDs = set.Select(x => x.ID);
            if (Unique) { return set.Where(x => x.LogicItemAquired()).Count(); }
            return Instance.Logic.Where(x => setIDs.Contains(x.RandomizedItem) && x.Checked).Count();
        }
        public static List<LogicObjects.LogicEntry> ProgressiveItemSet(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            if (!Instance.Options.ProgressiveItems || !Instance.IsMM()) { return null; }
            List<List<LogicObjects.LogicEntry>> ProgressiveItemSets = Utility.GetProgressiveItemSets(Instance);
            var set = ProgressiveItemSets.Find(x => x.Contains(entry));
            return set;
        }
        public static bool ItemUseable(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, List<int> usedItems = null)
        {
            if (usedItems == null) { usedItems = new List<int>(); }
            var Set = entry.ProgressiveItemSet(Instance);
            if (Set == null) { return NonProgressiveItemUseable(); }

            var AquiredSet = Set.Where(x => x.LogicItemAquired()).ToList();
            var ItemsNeeded = entry.ProgressiveItemsNeeded(Instance);
            if (AquiredSet.Count() >= ItemsNeeded)
            {
                for (var i = 0; i < ItemsNeeded; i++) { usedItems.Add(AquiredSet[i].ID); }
                return true;
            }
            return false;

            bool NonProgressiveItemUseable()
            {
                if (entry.LogicItemAquired())
                {
                    usedItems.Add(entry.ID);
                    return true;
                }
                else { return false; }
            }
        }
        public static bool CheckAvailability(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, List<int> usedItems = null, bool FromScratch = true, bool ForceStrictLogicHendeling = false)
        {
            var logic = Instance.Logic;
            var DicID = Instance.DicNameToID;
            var UselessLogicEntries = Utility.uselessLogicItems();
            var BYOAData = Utility.BYOAmmoData();
            usedItems = usedItems ?? new List<int>();
            if (string.IsNullOrWhiteSpace(entry.LocationName) && !entry.IsFake) { return false; }

            //Check for a "Combinations" Entry
            if (entry.Required != null && entry.Conditionals != null && entry.Required.Where(x => logic[x].DictionaryName.StartsWith("MMRTCombinations")).Any())
            {
                return LogicEditing.HandleMMRTCombinationLogic(entry, Instance, usedItems);
            }
            //Check for a "Check contains Item" Entry
            else if (entry.Required != null && entry.Conditionals != null && entry.Required.Where(x => logic[x].DictionaryName == "MMRTCheckContains").Any())
            {
                return LogicEditing.HandleMMRTCheckContainsItemLogic(entry, Instance, usedItems);
            }
            //Check for a MMR Dungeon clear Entry
            else if (Instance.IsMM() && entry.IsFake && Instance.EntranceAreaDic.Count > 0 && Instance.EntranceAreaDic.ContainsKey(entry.ID))
            {
                Console.WriteLine(entry.DictionaryName + " Was Dungeon Clear");
                return LogicEditing.HandleMMRTDungeonClearLogic(entry, Instance, usedItems);
            }
            //If a check was assigned a custom price, Change wallet logic entries to ensure the item is purchasable.
            else if (entry.Price > -1)
            {
                Console.WriteLine(entry.DictionaryName + " needed Price adjustment");
                return LogicEditing.HandleMMRTrandomPriceLogic(entry, Instance, usedItems);
            }
            //Removes logic entries that are only neccesary during randomization and don't actually represent the items requirements
            //An example is the pendant of memeories and letter to kafei being required for old lady and big bomb bag purchase check
            else if (Instance.Options.RemoveUselessLogic && UselessLogicEntries.ContainsKey(entry.DictionaryName) && entry.Required != null)
            {
                Console.WriteLine(entry.DictionaryName + " Contained Useless Logic");
                int[] NewReq = entry.Required;
                foreach (var i in UselessLogicEntries[entry.DictionaryName])
                {
                    if (DicID.ContainsKey(i))
                    {
                        NewReq = LogicEditing.removeItemFromRequirement(NewReq, new int[] { DicID[i] });
                    }
                }

                return LogicEditing.RequirementsMet(NewReq, Instance, usedItems) &&
                        LogicEditing.CondtionalsMet(entry.Conditionals, Instance, usedItems);
            }
            //If bring your own ammo is enabled, add required items to logic.
            else if (Instance.Options.BringYourOwnAmmo && BYOAData.ContainsKey(entry.DictionaryName))
            {
                Console.WriteLine(entry.DictionaryName + " Was effected by BYOAmmo");
                int[][] NewCond = entry.Conditionals;
                if (!BYOAData[entry.DictionaryName].Where(x => !DicID.ContainsKey(x)).Any())
                {
                    NewCond = LogicEditing.AddConditionalAsRequirement(NewCond, BYOAData[entry.DictionaryName].Select(x => DicID[x]).ToArray());
                }

                return LogicEditing.RequirementsMet(entry.Required, Instance, usedItems) &&
                        LogicEditing.CondtionalsMet(NewCond, Instance, usedItems);

            }
            //Check availability the standard way
            else
            {
                //Disable skipping entry if Strictlogic is enable or logic is being calculated from scratch such as firsy run
                if (FromScratch == false && ForceStrictLogicHendeling == false && Instance.Options.StrictLogicHandeling == false)
                {
                    bool shouldupdate = false;
                    if (entry.Required != null) { foreach (var i in entry.Required) { if (LogicEditing.LastUpdated.Contains(i)) { shouldupdate = true; } } }
                    if (entry.Conditionals != null) { foreach (var k in entry.Conditionals) { foreach (var i in k) { if (LogicEditing.LastUpdated.Contains(i)) { shouldupdate = true; } } } }
                    
                    if (!shouldupdate) { return entry.Available; }
                }
                return LogicEditing.RequirementsMet(entry.Required, Instance, usedItems) &&
                        LogicEditing.CondtionalsMet(entry.Conditionals, Instance, usedItems);
            }

        }
        public static bool FakeItemStatusChange(this LogicObjects.LogicEntry entry)
        {
            if (entry.Aquired != entry.Available)
            {
                LogicEditing.LastUpdated.Add(entry.ID);
                if (entry.IsFake)
                {
                    entry.Aquired = entry.Available;
                    return true;
                }
            }
            return false;
        }
        //TODO make this list a config file
        public static bool CanBeStartingItem(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            if (!Instance.IsMM()) { return true; }
            List<string> StartingItems = new List<string>
            {
                "MaskDeku",
                "ItemBow",
                "ItemFireArrow",
                "ItemIceArrow",
                "ItemLightArrow",
                "ItemBombBag",
                "ItemMagicBean",
                "ItemPowderKeg",
                "ItemPictobox",
                "ItemLens",
                "ItemHookshot",
                "FairyMagic",
                "FairySpinAttack",
                "FairyDoubleMagic",
                "FairyDoubleDefense",
                "ItemFairySword",
                "ItemBottleWitch",
                "ItemBottleAliens",
                "ItemBottleGoronRace",
                "ItemBottleBeavers",
                "ItemBottleDampe",
                "ItemBottleMadameAroma",
                "ItemNotebook",
                "UpgradeRazorSword",
                "UpgradeGildedSword",
                "UpgradeMirrorShield",
                "UpgradeBigQuiver",
                "UpgradeBiggestQuiver",
                "UpgradeBigBombBag",
                "UpgradeBiggestBombBag",
                "UpgradeAdultWallet",
                "UpgradeGiantWallet",
                "MaskPostmanHat",
                "MaskAllNight",
                "MaskBlast",
                "MaskStone",
                "MaskGreatFairy",
                "MaskKeaton",
                "MaskBremen",
                "MaskBunnyHood",
                "MaskDonGero",
                "MaskScents",
                "MaskRomani",
                "MaskCircusLeader",
                "MaskKafei",
                "MaskCouple",
                "MaskTruth",
                "MaskKamaro",
                "MaskGibdo",
                "MaskGaro",
                "MaskCaptainHat",
                "MaskGiant",
                "MaskGoron",
                "MaskZora",
                "ItemOcarina",
                "SongTime",
                "SongHealing",
                "SongSoaring",
                "SongEpona",
                "SongStorms",
                "SongSonata",
                "SongLullaby",
                "SongNewWaveBossaNova",
                "SongElegy",
                "SongOath",
                "ItemWoodfallMap",
                "ItemWoodfallCompass",
                "ItemSnowheadMap",
                "ItemSnowheadCompass",
                "ItemGreatBayMap",
                "ItemGreatBayCompass",
                "ItemStoneTowerMap",
                "ItemStoneTowerCompass",
                //"ShopItemTradingPostShield", //For Some reason these can't be starting items???
                //"ShopItemZoraShield",
                "ChestInvertedStoneTowerBean",
                "ItemTingleMapTown",
                "ItemTingleMapWoodfall",
                "ItemTingleMapSnowhead",
                "ItemTingleMapRanch",
                "ItemTingleMapGreatBay",
                "ItemTingleMapStoneTower",
                "MaskFierceDeity",
                "StartingSword",
                "StartingShield",
                "ShopItemBusinessScrubMagicBean",
                "RemainsOdolwa",
                "RemainsGoht",
                "RemainsGyorg",
                "RemainsTwinmold",

            };
            return StartingItems.Contains(entry.DictionaryName);
        }
        //Logic Instance Extentions
        public static void RefreshFakeItems(this LogicObjects.TrackerInstance Instance)
        {
            foreach(var i in Instance.Logic.Where(j => j.IsFake)) { i.Aquired = false; i.Available = false; }
        }
        public static bool IsEntranceRando(this LogicObjects.TrackerInstance Instance)
        {
            return (Instance.Logic.Where(x => x.IsEntrance()).Count() > 0);
        }
        public static bool IsMM(this LogicObjects.TrackerInstance Instance)
        {
            return Instance.GameCode == "MMR";
        }
        public static bool ItemInRange(this LogicObjects.TrackerInstance Instance, int Item)
        {
            return Item > -1 && Item < Instance.Logic.Count;
        }
        public static void GetWalletsFromConfigFile(this LogicObjects.TrackerInstance Instance)
        {
            Dictionary<string, int> Wallets = new Dictionary<string, int>();
            if (File.Exists(@"Recources\Other Files\WalletValues.txt"))
            {
                //Console.WriteLine("Wallet Config Found");
                bool AtGame = true;
                List<int> UsedItemsForLargestWallet = new List<int>();
                foreach (var i in File.ReadAllLines(@"Recources\Other Files\WalletValues.txt"))
                {
                    var x = i.Trim();
                    if (string.IsNullOrWhiteSpace(x) || x.StartsWith("//")) { continue; }
                    if (x.ToLower().StartsWith("#gamecodestart:"))
                    {
                        AtGame = x.ToLower().Replace("#gamecodestart:", "").Trim().Split(',')
                            .Select(y => y.Trim()).Contains(Instance.GameCode.ToLower());
                        continue;
                    }
                    if (x.ToLower().StartsWith("#gamecodeend:")) { AtGame = true; continue; }
                    if (!AtGame) { continue; }


                    var info = x.Split(':').Select(y => y.Trim()).ToArray();
                    if (info.Count() != 2) { continue; }
                    string Wallet = info[0];
                    int capacity = 0;
                    try { capacity = int.Parse(info[1]); } catch { continue; }

                    if (!Wallets.ContainsKey(Wallet)) { Wallets.Add(Wallet, capacity); }
                }
            }
            Instance.WalletDictionary = Wallets;
        }
        public static LogicObjects.LogicEntry GetLogicObjectFromDicName(this LogicObjects.TrackerInstance Instance, string DicName)
        {
            if (Instance.DicNameToID.ContainsKey(DicName)) { return Instance.Logic[Instance.DicNameToID[DicName]]; }
            else { return null; }
        }
    }
}
