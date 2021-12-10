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
        public static bool IsOneWayEntrance(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            return entry.PairedEntry(Instance) == null;
        }
        public static bool IsGossipStone(this LogicObjects.LogicEntry entry)
        {
            return entry.ItemSubType.StartsWith("Gossip");
        }
        public static bool IsCountCheck(this LogicObjects.LogicEntry entry)
        {
            return entry.ItemSubType.StartsWith("MMRTCountCheck");
        }
        public static LogicObjects.LogicEntry RandomizedEntry(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, bool ReturnJunkAsItem = false)
        {
            if (ReturnJunkAsItem && entry.HasRandomItem(false) && !entry.HasRandomItem(true))
            { 
                return new LogicObjects.LogicEntry { 
                    ID = -1, 
                    DictionaryName = entry.JunkItemType, 
                    DisplayName = entry.JunkItemType, 
                    LocationName = entry.JunkItemType, 
                    ItemName = entry.JunkItemType 
                }; 
            }
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
        public static void ToggleStartingItem(this LogicObjects.LogicEntry entry, bool? Forcestate = null)
        {
            if (Forcestate == null)
            {
                entry.Options = entry.isStartingItem() ? entry.Options - 4 : entry.Options + 4;
            }
            else if ((bool)Forcestate && !entry.isStartingItem())
            {
                entry.Options += 4;
            }
            else if (!(bool)Forcestate && entry.isStartingItem())
            {
                entry.Options -= 4;
            }
        }
        public static void SetRandomized(this LogicObjects.LogicEntry entry)
        {
            entry.Options = entry.isStartingItem() ? 4 : 0;
        }
        public static void SetUnRandomized(this LogicObjects.LogicEntry entry)
        {
            entry.Options = entry.isStartingItem() ? 5 : 1;
        }
        public static void SetUnRandomizedManual(this LogicObjects.LogicEntry entry)
        {
            entry.Options = entry.isStartingItem() ? 6 : 2;
        }
        public static void SetJunk(this LogicObjects.LogicEntry entry)
        {
            entry.Options = entry.isStartingItem() ? 7 : 3;
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
            return (entry.isStartingItem()) ? option - 4 : option;
        }
        public static bool isStartingItem(this LogicObjects.LogicEntry entry)
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
        public static bool ActualItemAquired(this LogicObjects.LogicEntry entry)
        {
            return (entry.Aquired || entry.isStartingItem() || (entry.Unrandomized() && entry.Available));
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
            return entry.GetItemsNewLocation(Logic) != null || entry.GetItemsSpoilerLocation(Logic) != null;
        }
        public static bool UserCreatedFakeItem(this LogicObjects.LogicEntry entry)
        {
            return entry.IsFake && !entry.RandomizerStaticFakeItem;
        }
        public static bool ItemBelongsToMe(this LogicObjects.LogicEntry entry)
        {
            if (!LogicObjects.MainTrackerInstance.Options.IsMultiWorld) { return true; }
            if (entry.IsEntrance()) { return true; }
            return entry.PlayerData.ItemBelongedToPlayer == -1 || entry.PlayerData.ItemBelongedToPlayer == LogicObjects.MainTrackerInstance.Options.MyPlayerID;
        }
        public static string ProgressiveItemName(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {
            if (Instance.Options.ProgressiveItems && entry.ProgressiveItemData != null && entry.ProgressiveItemData.IsProgressiveItem)
            {
                return entry.ProgressiveItemData.ProgressiveItemName;
            }
            else
            {
                return entry.ItemName ?? entry.DictionaryName;
            }
        }
        public static bool ItemUseable(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, List<int> usedItems = null)
        {
            usedItems = usedItems ?? new List<int>();
            if (Instance.Options.ProgressiveItems && entry.ProgressiveItemData != null && entry.ProgressiveItemData.IsProgressiveItem) 
            { return entry.ProgressiveItemUsable(Instance, usedItems); }
            else 
            { return entry.NonProgressiveItemUseable(usedItems); }

        }

        public static bool ProgressiveItemUsable(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, List<int> usedItems = null)
        {
            var ItemSet = entry.ProgressiveItemData.ProgressiveItemSet;
            if (ItemSet == null) { return entry.NonProgressiveItemUseable(usedItems); }
            var ValidProggressiveItems = ItemSet.Where(x => Instance.DicNameToID.ContainsKey(x)).Select(x => Instance.Logic[Instance.DicNameToID[x]]);
            if (!ValidProggressiveItems.Any()) { return entry.NonProgressiveItemUseable(usedItems); }

            var ItemsNeeded = entry.ProgressiveItemData.CountNeededForItem;
            var AquiredItems = ValidProggressiveItems.Where(x => x.ActualItemAquired()).ToList();
            if (AquiredItems.Count() >= ItemsNeeded)
            {
                for (var i = 0; i < ItemsNeeded; i++) { usedItems.Add(AquiredItems[i].ID); }
                return true;
            }
            return false;
        }

        public static bool NonProgressiveItemUseable(this LogicObjects.LogicEntry entry, List<int> usedItems = null)
        {
            if (entry.ActualItemAquired())
            {
                usedItems.Add(entry.ID);
                return true;
            }
            else { return false; }
        }

        public static bool CheckAvailability(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance, List<int> usedItems = null, bool FromScratch = true, bool ForceStrictLogicHendeling = false)
        {
            var logic = Instance.Logic;
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
            //Check availability the standard way
            else
            {
                var NewEntry = new LogicObjects.LogicEntry() { ID = entry.ID, DictionaryName = entry.DictionaryName, IsFake = entry.IsFake, Price = entry.Price, Required = entry.Required, Conditionals = entry.Conditionals };
                NewEntry = LogicEditing.PerformLogicEdits(NewEntry, Instance);

                //Disable skipping entry if Strictlogic is enable or logic is being calculated from scratch such as firsy run

                if (FromScratch == false && ForceStrictLogicHendeling == false && Instance.Options.StrictLogicHandeling == false && !entry.LogicWasEdited)
                {
                    bool shouldupdate = false;
                    if (NewEntry.Required != null && LogicItemChanged(new List<int[]> { NewEntry.Required }.ToArray())) { shouldupdate = true; }
                    if (NewEntry.Conditionals != null && LogicItemChanged(NewEntry.Conditionals)) { shouldupdate = true; }

                    if (!shouldupdate) { return entry.Available; }
                }

                NewEntry.Conditionals = NewEntry.Conditionals == null ? NewEntry.Conditionals : NewEntry.Conditionals.OrderBy(x => x.Length).ToArray();

                return LogicEditing.RequirementsMet(NewEntry.Required, Instance, usedItems) &&
                        LogicEditing.CondtionalsMet(NewEntry.Conditionals, Instance, usedItems);
            }

            bool LogicItemChanged(int [][] Entry)
            {
                foreach (var k in Entry)
                {
                    foreach (var i in k)
                    {
                        if (LogicEditing.LastUpdated.Contains(i) || 
                            (Instance.Logic[i].ProgressiveItemData != null && Instance.Logic[i].ProgressiveItemData.IsProgressiveItem))
                        { return true; }
                    }
                }
                return false;
            }

        }
        public static bool FakeItemStatusChange(this LogicObjects.LogicEntry entry)
        {
            if (entry.Aquired != entry.Available && entry.IsFake)
            {
                LogicEditing.LastUpdated.Add(entry.ID);
                entry.Aquired = entry.Available;
                return true;
            }
            return false;
        }
        //TODO make this list a config file
        public static bool CanBeStartingItem(this LogicObjects.LogicEntry entry, LogicObjects.TrackerInstance Instance)
        {

            return Instance.LogicDictionary.LogicDictionaryList.Where(x => x.DictionaryName == entry.DictionaryName && x.ValidRandomizerStartingItem).Any();
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
        public static void CreateWalletDictionary(this LogicObjects.TrackerInstance Instance, LogicObjects.LogicDictionary Dictionary)
        {
            Instance.WalletDictionary = new Dictionary<string, int>();
            Instance.WalletDictionary.Add("MMRTDefault", Dictionary.DefaultWalletCapacity);
            Console.WriteLine($"Adding Wallet MMRTDefault: {Dictionary.DefaultWalletCapacity}");
            foreach (var i in Dictionary.LogicDictionaryList)
            {
                if (Instance.Logic.Find(x => x.DictionaryName == i.DictionaryName) != null && i.WalletCapacity != null && !Instance.WalletDictionary.ContainsKey(i.DictionaryName))
                {
                    Console.WriteLine($"Adding Wallet {i.DictionaryName}: {(int)i.WalletCapacity}");
                    Instance.WalletDictionary.Add(i.DictionaryName, (int)i.WalletCapacity);
                }
            }
        }
        public static void CreateAreaClearDictionary(this LogicObjects.TrackerInstance Instance, List<LogicObjects.LogicDictionaryEntry> Dictionary)
        {
            Instance.EntranceAreaDic = new Dictionary<int, int>();
            foreach (var i in Dictionary)
            {

                if (i.GameClearDungeonEntrance != null)
                {
                    var AreaClear = Instance.Logic.Find(x => x.DictionaryName == i.DictionaryName);
                    var DungeonEntrance = Instance.Logic.Find(x => x.DictionaryName == i.GameClearDungeonEntrance);
                    if (AreaClear != null && DungeonEntrance != null)
                    {
                        Console.WriteLine($"Adding Entrance Area Pair {AreaClear.DictionaryName}: {DungeonEntrance.DictionaryName}");
                        Instance.EntranceAreaDic.Add(AreaClear.ID, DungeonEntrance.ID);
                    }
                }
            }
        }
        public static LogicObjects.LogicEntry GetLogicObjectFromDicName(this LogicObjects.TrackerInstance Instance, string DicName)
        {
            if (Instance.DicNameToID.ContainsKey(DicName)) { return Instance.Logic[Instance.DicNameToID[DicName]]; }
            else { return null; }
        }

        public static List<int> CreateKeyDictionary(this LogicObjects.TrackerInstance Instance, List<LogicObjects.LogicDictionaryEntry> Dict, string KeyType)
        {
            List<int> KeyList = new List<int>();
            var Keys = Dict.Where(x => x.KeyType != null && x.KeyType == KeyType);
            if (!Keys.Any()) { return KeyList; }
            foreach (var i in Keys)
            {
                var LogicEntry = Instance.Logic.Find(x => x.DictionaryName == i.DictionaryName);
                if (LogicEntry != null)
                {
                    Console.WriteLine($"Found {KeyType} key: {LogicEntry.ItemName ?? LogicEntry.DictionaryName}");
                    KeyList.Add(LogicEntry.ID);
                }
            }
            return KeyList;
        }
        public static List<int> GetChecksNeedingKeys(this LogicObjects.TrackerInstance Instance)
        {
            var ChecksNeedingKeys = new List<int>();
            foreach(var NewEntry in Instance.Logic)
            {
                bool HasKeys = false;
                if (NewEntry.Required != null)
                {
                    foreach (var i in NewEntry.Required) { if (Instance.Keys["BossKeys"].Contains(i) || Instance.Keys["SmallKeys"].Contains(i)) { HasKeys = true; break; } }
                }
                if (NewEntry.Conditionals != null)
                {
                    foreach (var Cond in NewEntry.Conditionals) { foreach (var i in Cond) { if (Instance.Keys["BossKeys"].Contains(i) || Instance.Keys["SmallKeys"].Contains(i)) { HasKeys = true; break; } } }
                }
                if (HasKeys) { ChecksNeedingKeys.Add(NewEntry.ID); }
            }
            return ChecksNeedingKeys;
        }
        public static void CreateDicNameToID(this LogicObjects.TrackerInstance Instance)
        {
            foreach (var LogicEntry1 in Instance.Logic)
            {
                if (!Instance.DicNameToID.ContainsKey(LogicEntry1.DictionaryName))
                { Instance.DicNameToID.Add(LogicEntry1.DictionaryName, LogicEntry1.ID); }
            }
        }

        public static Dictionary<string, string[]> GetUselessLogicItems(this LogicObjects.TrackerInstance Instance)
        {
            var ChecksWithuselessLogic = Instance.LogicDictionary.LogicDictionaryList.Where(x => x.RandoOnlyRequiredLogic != null && Instance.DicNameToID.ContainsKey(x.DictionaryName));
            if (!ChecksWithuselessLogic.Any()) { return new Dictionary<string, string[]>(); }
            Dictionary<string, string[]> UselessLogicData = new Dictionary<string, string[]>();
            foreach (var i in ChecksWithuselessLogic)
            {
                Console.WriteLine($"Adding Useless Logic Data for {i.DictionaryName}: {string.Join(", ", i.RandoOnlyRequiredLogic)}");
                UselessLogicData.Add(i.DictionaryName, i.RandoOnlyRequiredLogic.Select(x => x.Trim()).ToArray());
            }
            return UselessLogicData;
        }
    }
}
