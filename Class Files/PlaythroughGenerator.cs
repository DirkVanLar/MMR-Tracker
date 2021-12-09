using System;
using System.Collections.Generic;
using System.Linq;
using MMR_Tracker_V2;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MMR_Tracker.Class_Files
{
    class PlaythroughGenerator
    {
        public class PlaythroughContainer
        {
            public List<LogicObjects.PlaythroughItem> Playthrough { get; set; } = null;
            public List<LogicObjects.PlaythroughItem> RealItemPlaythrough { get; set; } = null;
            public List<LogicObjects.PlaythroughItem> ImportantPlaythrough { get; set; } = null;
            public LogicObjects.TrackerInstance PlaythroughInstance { get; set; } = null;
            public string ErrorMessage { get; set; } = "";
            public LogicObjects.PlaythroughItem GameClearItem { get; set; } = null;

        }

        public static PlaythroughContainer GeneratePlaythrough(LogicObjects.TrackerInstance Instance, int GameClear, bool fullplaythrough = false)
        {
            var container = new PlaythroughContainer();

            List<LogicObjects.PlaythroughItem> Playthrough = new List<LogicObjects.PlaythroughItem>();
            Dictionary<int, int> SpoilerToID = new Dictionary<int, int>();
            container.PlaythroughInstance = Utility.CloneTrackerInstance(Instance);

            if (GameClear < 0) { container.ErrorMessage = ("Could not find game clear requirements. Playthrough can not be generated."); return container; }

            if (!Utility.CheckforSpoilerLog(container.PlaythroughInstance.Logic))
            {
                var file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt)|*.txt");
                if (file == "") { return null; }
                LogicEditing.WriteSpoilerLogToLogic(container.PlaythroughInstance, file);
            }

            if (!Utility.CheckforSpoilerLog(container.PlaythroughInstance.Logic, true))
            { MessageBox.Show("Not all items have spoiler data. Results may be inconsistant. Ensure you are using the same version of logic used to generate your selected spoiler log"); }

            List<int> importantItems = new List<int>();
            foreach (var i in container.PlaythroughInstance.Logic)
            {
                i.Available = false;
                i.Checked = false;
                i.Aquired = false;
                if (!i.IsFake && i.Unrandomized(2) && i.SpoilerRandom < 0) { i.SpoilerRandom = i.ID; i.RandomizedItem = i.ID; }
                if (i.IsFake) { i.SpoilerRandom = i.ID; i.RandomizedItem = i.ID; i.LocationName = i.DictionaryName; i.ItemName = i.DictionaryName; }
                if (i.Unrandomized(2) && i.ID == i.SpoilerRandom) { i.SetRandomized(); }//If the item is unrandomized, Randomize it so it shows up in the correct sphere.
                if (i.SpoilerRandom > -1) { i.RandomizedItem = i.SpoilerRandom; }//Make the items randomized item its spoiler item, just for consitancy sake
                else if (i.RandomizedItem > -1) { i.SpoilerRandom = i.RandomizedItem; }//If the item doesn't have spoiler data, but does have a randomized item. set it's spoiler data to the randomized item
                if (SpoilerToID.ContainsKey(i.SpoilerRandom) || i.SpoilerRandom < 0) { continue; }
                SpoilerToID.Add(i.SpoilerRandom, i.ID);
                //Check for all items mentioned in the logic file
                if (i.Required != null)
                {
                    foreach (var k in i.Required)
                    {
                        if (!importantItems.Contains(k)) { importantItems.Add(k); }
                    }
                }
                if (i.Conditionals != null)
                {
                    foreach (var j in i.Conditionals)
                    {
                        foreach (var k in j) { if (!importantItems.Contains(k)) { importantItems.Add(k); } }
                    }
                }
                if (i.ID == GameClear) { importantItems.Add(i.ID); }

                if (!importantItems.Contains(i.ID) && fullplaythrough) { importantItems.Add(i.ID); }
            }

            SwapAreaClearLogic(container.PlaythroughInstance);
            MarkAreaClearAsEntry(container.PlaythroughInstance);
            CalculatePlaythrough(container.PlaythroughInstance, Playthrough, 0, importantItems);

            importantItems = new List<int>();
            var GameClearPlaythroughItem = Playthrough.Find(x => x.Check.ID == GameClear);
            if (GameClearPlaythroughItem == null)
            {
                container.Playthrough = Playthrough.OrderBy(x => x.SphereNumber).ThenByDescending(x => container.PlaythroughInstance.Logic[x.Check.RandomizedItem].StartingItem()).ThenBy(x => x.Check.ItemSubType).ThenBy(x => x.Check.LocationArea).ThenBy(x => x.Check.LocationName).ToList();
                return container;
            }


            container.GameClearItem = GameClearPlaythroughItem;

            importantItems.Add(GameClearPlaythroughItem.Check.ID);
            FindImportantItems(GameClearPlaythroughItem, importantItems, Playthrough, SpoilerToID);

            container.Playthrough = Playthrough.OrderBy(x => x.SphereNumber).ThenByDescending(x => container.PlaythroughInstance.Logic[x.Check.RandomizedItem].StartingItem()).ThenBy(x => x.Check.ItemSubType).ThenBy(x => x.Check.LocationArea).ThenBy(x => x.Check.LocationName).ToList();

            container.RealItemPlaythrough = JsonConvert.DeserializeObject<List<LogicObjects.PlaythroughItem>>(JsonConvert.SerializeObject(container.Playthrough));
            //Replace all fake items with the real items used to unlock those fake items
            foreach (var i in container.RealItemPlaythrough) { i.ItemsUsed = Tools.ResolveFakeToRealItems(i, Playthrough, container.PlaythroughInstance.Logic).Distinct().ToList(); }

            container.ImportantPlaythrough = container.RealItemPlaythrough.Where(i => (importantItems.Contains(i.Check.ID) && !i.Check.IsFake) || i.Check.ID == GameClear).ToList();

            //Convert Progressive Items Back to real items
            ConvertProgressiveItems(container.Playthrough, container.PlaythroughInstance);
            ConvertProgressiveItems(container.RealItemPlaythrough, container.PlaythroughInstance);
            ConvertProgressiveItems(container.ImportantPlaythrough, container.PlaythroughInstance);

            return container;
        }

        

        public static void DisplayPlaythrough(List<LogicObjects.PlaythroughItem> Playthrough, LogicObjects.TrackerInstance CopyInstance, int GameclearItem, List<LogicObjects.LogicEntry> MainLogic)
        {
            List<string> PlaythroughString = new List<string>();
            int lastSphere = -1;

            string FinalTask = CopyInstance.Logic[GameclearItem].DictionaryName;

            if (FinalTask == "MMRTGameClear")
            {
                FinalTask = (CopyInstance.IsMM()) ? "Defeat Majora" : "Beat the game";
            }
            foreach (var i in Playthrough)
            {
                if (i.Check.ItemSubType.StartsWith("Option ")) { continue; }
                if (i.SphereNumber != lastSphere)
                {
                    PlaythroughString.Add("Sphere: " + i.SphereNumber + " ====================================="); lastSphere = i.SphereNumber;
                }
                if (i.Check.ID == GameclearItem) { PlaythroughString.Add(FinalTask); }
                else
                {
                    var ObtainLine = "Check \"" + ( MainLogic[i.Check.RandomizedItem].StartingItem() ? "Starting items" : i.Check.LocationName) + "\" to obtain \"" + CopyInstance.Logic[i.Check.RandomizedItem].ItemName + "\"";
                    if (MainLogic.Count() > i.Check.ID && MainLogic[i.Check.ID].Checked) { ObtainLine += " ✅"; }
                    PlaythroughString.Add(ObtainLine);
                }
                string items = "    Using Items: ";
                foreach (var j in i.ItemsUsed) { items = items + CopyInstance.Logic[j].ItemName + ", "; }
                if (items != "    Using Items: ") { PlaythroughString.Add(items); }
            }

            InformationDisplay DisplayPlaythrough = new InformationDisplay();
            InformationDisplay.Playthrough = PlaythroughString;
            DisplayPlaythrough.DebugFunction = 3;
            DisplayPlaythrough.Show();
            InformationDisplay.Playthrough = new List<string>();
        }

        public static void ConvertProgressiveItems(List<LogicObjects.PlaythroughItem> Playthrough, LogicObjects.TrackerInstance Instance)
        {
            if (!LogicObjects.MainTrackerInstance.IsMM() || !LogicObjects.MainTrackerInstance.Options.ProgressiveItems) { return; }

            List<List<LogicObjects.LogicEntry>> ProgressiveItemSets = Utility.GetProgressiveItemSets(Instance);

            foreach (var i in ProgressiveItemSets) { if (i == null || !i.Any() || i.Where(x => x == null).Any()) { return; } }

            foreach (var ProgressiveItemSet in ProgressiveItemSets)
            {
                int TimesItemObtained = -1;
                foreach(var ObtainedItem in Playthrough)
                {
                    var RandomItem = Instance.Logic[ObtainedItem.Check.RandomizedItem];
                    if (ProgressiveItemSet.Contains(RandomItem))
                    {
                        TimesItemObtained++;
                        ObtainedItem.Check.RandomizedItem = ProgressiveItemSet[TimesItemObtained].ID;
                        if (TimesItemObtained >= ProgressiveItemSet.Count()) { TimesItemObtained = ProgressiveItemSet.Count() - 1; }
                    }

                    int TimesItemUsed = ObtainedItem.ItemsUsed.Where(x => ProgressiveItemSet.Where(y => y.ID == x).Any()).Count() - 1;
                    if (TimesItemUsed < 0) { continue; }
                    if (TimesItemUsed >= ProgressiveItemSet.Count()) { TimesItemUsed = ProgressiveItemSet.Count() - 1; }
                    //Debugging.Log($"{TimesItemUsed} Progressive Items where used to check {ObtainedItem.Check.LocationName}");
                    List<int> newItemsUsed = new List<int>();
                    bool ItemAdded = false;
                    foreach (var UsedItem in ObtainedItem.ItemsUsed)
                    {
                        if (ProgressiveItemSet.Where(y => y.ID == UsedItem).Any())
                        {
                            if (!ItemAdded)
                            {
                                //Debugging.Log($"{Instance.Logic[UsedItem].ItemName} was replace by {ProgressiveItemSet[TimesItemObtained].ItemName}");
                                newItemsUsed.Add(ProgressiveItemSet[TimesItemObtained].ID);
                                ItemAdded = true;
                            }
                        }
                        else
                        {
                            newItemsUsed.Add(UsedItem);
                        }
                    }
                    ObtainedItem.ItemsUsed = newItemsUsed.Distinct().ToList();
                }
            }
        }


        public static void CalculatePlaythrough(LogicObjects.TrackerInstance Instance, List<LogicObjects.PlaythroughItem> Playthrough, int sphere, List<int> ImportantItems, bool First = true)
        {
            var logic = Instance.Logic;
            bool RealItemObtained = false;
            bool recalculate = false;
            List<LogicObjects.LogicEntry> itemCheckList = new List<LogicObjects.LogicEntry>();


            foreach (var item in logic)
            {
                if (item.SpoilerRandom < 0) { continue; }
                List<int> UsedItems = new List<int>();
                if (Instance.Logic[item.SpoilerRandom].StartingItem()) 
                { 
                    item.Available = true; 
                }
                else
                {
                    item.Available = item.CheckAvailability(Instance, UsedItems);
                }

                if (!item.IsFake && item.SpoilerRandom > -1 && item.Available && !logic[item.SpoilerRandom].Aquired)
                {
                    itemCheckList.Add(item);
                    recalculate = true;
                    if (ImportantItems.Contains(item.SpoilerRandom) && item.Available)
                    {
                        Playthrough.Add(new LogicObjects.PlaythroughItem { SphereNumber = sphere, Check = item, ItemsUsed = UsedItems });
                        RealItemObtained = true;
                    }
                }
            }
            foreach (var item in itemCheckList)
            {
                logic[item.SpoilerRandom].Aquired = item.Available;
            }

            int NewSphere = (RealItemObtained) ? sphere + 1 : sphere;

            if (UnlockAllFake(Instance, ImportantItems, NewSphere, Playthrough)) { recalculate = true; }

            if (recalculate) { CalculatePlaythrough(Instance, Playthrough, NewSphere, ImportantItems, false); }
        }

        public static bool UnlockAllFake(LogicObjects.TrackerInstance Instance, List<int> ImportantItems, int sphere, List<LogicObjects.PlaythroughItem> Playthrough)
        {
            var logic = Instance.Logic;
            var recalculate = false;
            foreach (var item in logic)
            {
                List<int> UsedItems = new List<int>();

                item.Available = item.CheckAvailability(Instance, UsedItems);

                if (item.Aquired != item.Available && item.IsFake)
                {
                    item.Aquired = item.Available;
                    recalculate = true;
                    if (ImportantItems.Contains(item.SpoilerRandom) && item.Available)
                    {
                        Playthrough.Add(new LogicObjects.PlaythroughItem { SphereNumber = sphere, Check = item, ItemsUsed = UsedItems });
                    }
                }
            }
            if (recalculate) { UnlockAllFake(Instance, ImportantItems, sphere, Playthrough); }
            return recalculate;
        }

        public static void FindImportantItems(LogicObjects.PlaythroughItem EntryToCheck, List<int> importantItems, List<LogicObjects.PlaythroughItem> Playthrough, Dictionary<int, int> SpoilerToID)
        {
            foreach (var i in EntryToCheck.ItemsUsed ?? new List<int>())
            {
                if (!SpoilerToID.ContainsKey(i)) { continue; }
                var locToCheck = SpoilerToID[i];
                if (importantItems.Contains(locToCheck)) { continue; }
                importantItems.Add(locToCheck);
                var NextLocation =  Playthrough.Find(j => j.Check.ID == locToCheck) ?? new LogicObjects.PlaythroughItem();
                FindImportantItems(NextLocation, importantItems, Playthrough, SpoilerToID);
            }
        }

        public static int GetGameClearEntry(List<LogicObjects.LogicEntry> playLogic, bool EntranceRadno)
        {
            var MMRTGameClear = playLogic.Find(x => x.DictionaryName == "MMRTGameClear");
            if (MMRTGameClear != null) { return MMRTGameClear.ID; }

            int GameClear = -1;

            Dictionary<string, LogicObjects.LogicEntry> LogicItems = new Dictionary<string, LogicObjects.LogicEntry>()
            {
                { "BigQuiver", playLogic.Find(x => x.DictionaryName == "Town Archery Quiver (40)" || x.DictionaryName == "UpgradeBigQuiver")},
                { "BiggestQuiver",  playLogic.Find(x => x.DictionaryName == "Swamp Archery Quiver (50)" || x.DictionaryName == "UpgradeBiggestQuiver") },
                { "Bow", playLogic.Find(x => x.DictionaryName == "Hero's Bow" || x.DictionaryName == "ItemBow") },
                { "Zora", playLogic.Find(x => x.DictionaryName == "Zora Mask" || x.DictionaryName == "MaskZora") },
                { "StartingSword", playLogic.Find(x => x.DictionaryName == "Starting Sword" || x.DictionaryName == "StartingSword") },
                { "RazorSword", playLogic.Find(x => x.DictionaryName == "Razor Sword" || x.DictionaryName == "UpgradeRazorSword") },
                { "GildedSword", playLogic.Find(x => x.DictionaryName == "Gilded Sword" || x.DictionaryName == "UpgradeGildedSword") },
                { "FairySword", playLogic.Find(x => x.DictionaryName == "Great Fairy's Sword" || x.DictionaryName == "ItemFairySword") },
                { "MajorasLair", playLogic.Find(x => x.DictionaryName == "Moon Access" || x.DictionaryName == "AreaMoonAccess") },
                { "Deity", playLogic.Find(x => x.DictionaryName == "Fierce Deity's Mask" || x.DictionaryName == "MaskFierceDeity") },
                { "Magic", playLogic.Find(x => x.DictionaryName == "Magic Meter") }
            };
            //If any of the above entries are not found in logic, the game clear object can not be created.
            foreach(var i in LogicItems) { if (i.Value == null) { return -1; } }

            //If entrance rando is being used, the "EntranceMajorasLairFromTheMoon" should be used in place of Moon access, since just being on the moon no longer means access to majoras lair
            if (EntranceRadno)
            {
                if (playLogic.Find(x => x.DictionaryName == "EntranceMajorasLairFromTheMoon") == null) { return -1; }
                else { LogicItems["MajorasLair"] = playLogic.Find(x => x.DictionaryName == "EntranceMajorasLairFromTheMoon"); }
            }

            //If ocarina and song of time are in the item pool, they should be required for game completion.
            if (playLogic.Find(x => x.DictionaryName == "SongTime") != null && playLogic.Find(x => x.DictionaryName == "ItemOcarina") != null)
            {
                LogicItems.Add("SongTime", playLogic.Find(x => x.DictionaryName == "SongTime"));
                LogicItems.Add("ItemOcarina", playLogic.Find(x => x.DictionaryName == "ItemOcarina"));
            }

            try
            {
                //An entry that details the ability to stun majora. This is not expressly needed but IMO should be expected casually
                int StunMajora = playLogic.Count();
                playLogic.Add(new LogicObjects.LogicEntry
                {
                    ID = StunMajora,
                    DictionaryName = "MMRTStunMajora",
                    IsFake = true,
                    Conditionals = new int[][]
                    {
                        new int[] { LogicItems["Bow"].ID },
                        new int[] { LogicItems["BigQuiver"].ID },
                        new int[] { LogicItems["BiggestQuiver"].ID },
                        new int[] { LogicItems["Zora"].ID },
                        new int[] { LogicItems["Deity"].ID, LogicItems["Magic"].ID  }
                    }
                });

                //An entry that details the ability to Damage majora.
                int DamageMajora = playLogic.Count();
                playLogic.Add(new LogicObjects.LogicEntry
                {
                    ID = DamageMajora,
                    DictionaryName = "MMRTDamageMajora",
                    IsFake = true,
                    Conditionals = new int[][]
                    {
                        new int[] { LogicItems["StartingSword"].ID },
                        new int[] { LogicItems["RazorSword"].ID },
                        new int[] { LogicItems["GildedSword"].ID },
                        new int[] { LogicItems["FairySword"].ID },
                        new int[] { LogicItems["Deity"].ID }
                    }
                });

                //An entry that details the ability to reach and defeat Majora
                GameClear = playLogic.Count();
                playLogic.Add(new LogicObjects.LogicEntry
                {
                    ID = GameClear,
                    DictionaryName = "MMRTGameClear",
                    IsFake = true,
                    Required = new int[] { LogicItems["MajorasLair"].ID, StunMajora, DamageMajora },
                    Conditionals = null
                });

                //Add Ocarina and song of time to MMRTGameClear logic if they are in the item pool.
                if (LogicItems.ContainsKey("SongTime"))
                {
                    playLogic[GameClear].Required = playLogic[GameClear].Required.Concat(new int[] { LogicItems["SongTime"].ID, LogicItems["ItemOcarina"].ID }).ToArray();
                }
            }
            catch { GameClear = -1; }
            return GameClear;
        }

        public static void MarkAreaClearAsEntry(LogicObjects.TrackerInstance instance)
        {
            if (!instance.IsMM()) { return; }
            LogicObjects.LogicEntry Default = new LogicObjects.LogicEntry();
            var WoodFallClear = instance.Logic.Find(x => x.DictionaryName == "Woodfall clear" || x.DictionaryName == "AreaWoodFallTempleClear");
            var SnowheadClear = instance.Logic.Find(x => x.DictionaryName == "Snowhead clear" || x.DictionaryName == "AreaSnowheadTempleClear");
            var GreatBayClear = instance.Logic.Find(x => x.DictionaryName == "Great Bay clear" || x.DictionaryName == "AreaGreatBayTempleClear");
            var IkanaClear = instance.Logic.Find(x => x.DictionaryName == "Ikana clear" || x.DictionaryName == "AreaStoneTowerClear");

            WoodFallClear.LocationName = "Defeat Odolwa";
            SnowheadClear.LocationName = "Defeat Goht";
            GreatBayClear.LocationName = "Defeat Gyorg";
            IkanaClear.LocationName = "Defeat Twinmold";

            WoodFallClear.ItemName = "Woodfall Clear";
            SnowheadClear.ItemName = "Snowhead Clear";
            GreatBayClear.ItemName = "Great Bay Clear";
            IkanaClear.ItemName = "Ikana Clear";

            WoodFallClear.IsFake = false;
            SnowheadClear.IsFake = false;
            GreatBayClear.IsFake = false;
            IkanaClear.IsFake = false;

            if (instance.IsEntranceRando()) { return; }

            var WoodfallLocationName = WoodFallClear.ClearRandomizedDungeonInThisArea(instance).LocationName;
            var SnowheadLocationName = SnowheadClear.ClearRandomizedDungeonInThisArea(instance).LocationName;
            var GreatBayLocationName = GreatBayClear.ClearRandomizedDungeonInThisArea(instance).LocationName;
            var IkanaLocationName = IkanaClear.ClearRandomizedDungeonInThisArea(instance).LocationName;

            WoodFallClear.LocationName = WoodfallLocationName;
            SnowheadClear.LocationName = SnowheadLocationName;
            GreatBayClear.LocationName = GreatBayLocationName;
            IkanaClear.LocationName = IkanaLocationName;

        }

        public static void SwapAreaClearLogic(LogicObjects.TrackerInstance Instance)
        {
            //Since these checks are be converted to real items, the will not get parsed by the function in checkavailablility that usually swaps their logic
            //So here we do it manually.
            var EntAreaDict = Instance.EntranceAreaDic;
            if (!EntAreaDict.Any()) { return; }
            var ClearAreas = EntAreaDict.Keys.ToArray();

            var WoodFallClear = Instance.Logic[ClearAreas[0]];
            var SnowheadClear = Instance.Logic[ClearAreas[1]];
            var GreatBayClear = Instance.Logic[ClearAreas[2]];
            var IkanaClear = Instance.Logic[ClearAreas[3]];

            var newWoodfallReq = WoodFallClear.ClearRandomizedDungeonInThisArea(Instance).Required;
            var newWoodfallCond = WoodFallClear.ClearRandomizedDungeonInThisArea(Instance).Conditionals;
            var newSnowheadReq = SnowheadClear.ClearRandomizedDungeonInThisArea(Instance).Required;
            var newSnowheadCond = SnowheadClear.ClearRandomizedDungeonInThisArea(Instance).Conditionals;
            var newGreatBayreq = GreatBayClear.ClearRandomizedDungeonInThisArea(Instance).Required;
            var newgreatBayCond = GreatBayClear.ClearRandomizedDungeonInThisArea(Instance).Conditionals;
            var newIkanaReq = IkanaClear.ClearRandomizedDungeonInThisArea(Instance).Required;
            var newIkanaCond = IkanaClear.ClearRandomizedDungeonInThisArea(Instance).Conditionals;

            WoodFallClear.Required = newWoodfallReq;
            WoodFallClear.Conditionals = newWoodfallCond;
            SnowheadClear.Required = newSnowheadReq;
            SnowheadClear.Conditionals = newSnowheadCond;
            GreatBayClear.Required = newGreatBayreq;
            GreatBayClear.Conditionals = newgreatBayCond;
            IkanaClear.Required = newIkanaReq;
            IkanaClear.Conditionals = newIkanaCond;


        }
    }
}
