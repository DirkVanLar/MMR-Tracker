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
                var file = Utility.FileSelect("Select A Spoiler Log", "Spoiler Log (*.txt;*html)|*.txt;*html");
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
                if (i.IsFake) { i.SpoilerRandom = i.ID; i.RandomizedItem = i.ID; i.LocationName = i.DictionaryName; i.ItemName = i.DictionaryName; }
                if (i.Unrandomized() && i.ID == i.SpoilerRandom) { i.IsFake = true; }//If the item is unrandomized treat it as a fake item
                if (i.SpoilerRandom > -1) { i.RandomizedItem = i.SpoilerRandom; }//Make the items randomized item its spoiler item, just for consitancy sake
                else if (i.RandomizedItem > -1) { i.SpoilerRandom = i.RandomizedItem; }//If the item doesn't have spoiler data, but does have a randomized item. set it's spoiler data to the randomized item
                else if (i.Unrandomized(1)) { i.SpoilerRandom = i.ID; i.RandomizedItem = i.ID; }//If the item doesn't have spoiler data or a randomized item and is unrandomized (manual), set it's spoiler item to it's self
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
                container.Playthrough = Playthrough.OrderBy(x => x.SphereNumber).ThenBy(x => x.Check.ItemSubType).ThenBy(x => x.Check.LocationArea).ThenBy(x => x.Check.LocationName).ToList();
                return container;
            }

            container.GameClearItem = GameClearPlaythroughItem;

            importantItems.Add(GameClearPlaythroughItem.Check.ID);
            FindImportantItems(GameClearPlaythroughItem, importantItems, Playthrough, SpoilerToID);

            container.Playthrough = Playthrough.OrderBy(x => x.SphereNumber).ThenBy(x => x.Check.ItemSubType).ThenBy(x => x.Check.LocationArea).ThenBy(x => x.Check.LocationName).ToList();

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

        

        public static void DisplayPlaythrough(List<LogicObjects.PlaythroughItem> Playthrough, LogicObjects.TrackerInstance CopyInstance, int GameclearItem)
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
                if (i.SphereNumber != lastSphere)
                {
                    PlaythroughString.Add("Sphere: " + i.SphereNumber + " ====================================="); lastSphere = i.SphereNumber;
                }
                if (i.Check.ID == GameclearItem) { PlaythroughString.Add(FinalTask); }
                else
                {
                    PlaythroughString.Add("Check \"" + i.Check.LocationName + "\" to obtain \"" + CopyInstance.Logic[i.Check.RandomizedItem].ItemName + "\"");
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


        public static void CalculatePlaythrough(LogicObjects.TrackerInstance Instance, List<LogicObjects.PlaythroughItem> Playthrough, int sphere, List<int> ImportantItems)
        {
            var logic = Instance.Logic;
            bool RealItemObtained = false;
            bool recalculate = false;
            List<LogicObjects.LogicEntry> itemCheckList = new List<LogicObjects.LogicEntry>();


            foreach (var item in logic)
            {
                List<int> UsedItems = new List<int>();
                item.Available = item.CheckAvailability(Instance, UsedItems);

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

            if (recalculate) { CalculatePlaythrough(Instance, Playthrough, NewSphere, ImportantItems); }
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
            try
            {
                int StunMajora = playLogic.Count();
                playLogic.Add(new LogicObjects.LogicEntry
                {
                    ID = StunMajora,
                    DictionaryName = "MMRTStunMajora",
                    IsFake = true,
                    Conditionals = new int[][]
                    {
                        new int[] { playLogic.Find(x => x.DictionaryName == "Town Archery Quiver (40)").ID },
                        new int[] { playLogic.Find(x => x.DictionaryName == "Swamp Archery Quiver (50)").ID },
                        new int[] { playLogic.Find(x => x.DictionaryName == "Hero's Bow").ID },
                        new int[] { playLogic.Find(x => x.DictionaryName == "Zora Mask").ID }
                    }
                });

                int DamageMajora = playLogic.Count();
                playLogic.Add(new LogicObjects.LogicEntry
                {
                    ID = DamageMajora,
                    DictionaryName = "MMRTDamageMajora",
                    IsFake = true,
                    Conditionals = new int[][]
                    {
                        new int[] { playLogic.Find(x => x.DictionaryName == "Starting Sword").ID },
                        new int[] { playLogic.Find(x => x.DictionaryName == "Razor Sword").ID },
                        new int[] { playLogic.Find(x => x.DictionaryName == "Gilded Sword").ID },
                        new int[] { playLogic.Find(x => x.DictionaryName == "Great Fairy's Sword").ID }
                    }
                });

                GameClear = playLogic.Count();
                playLogic.Add(new LogicObjects.LogicEntry
                {
                    ID = GameClear,
                    DictionaryName = "MMRTGameClear",
                    IsFake = true,
                    Required = (!EntranceRadno) ?
                        new int[] { playLogic.Find(x => x.DictionaryName == "Moon Access").ID } :
                        new int[] { playLogic.Find(x => x.DictionaryName == "EntranceMajorasLairFromTheMoon").ID },
                    Conditionals = new int[][]
                    {
                        new int[] { StunMajora, DamageMajora },
                        new int[] { playLogic.Find(x => x.DictionaryName == "Fierce Deity's Mask").ID, playLogic.Find(x => x.DictionaryName == "Magic Meter").ID }
                    }
                });
            }
            catch { GameClear = -1; }
            return GameClear;
        }

        public static void MarkAreaClearAsEntry(LogicObjects.TrackerInstance instance)
        {

            var EntAreaDict = instance.EntranceAreaDic;
            LogicObjects.LogicEntry Default = new LogicObjects.LogicEntry();
            var WoodFallClear = instance.Logic.Find(x => x.DictionaryName == "Woodfall clear") ?? Default;
            var SnowheadClear = instance.Logic.Find(x => x.DictionaryName == "Snowhead clear") ?? Default;
            var GreatBayClear = instance.Logic.Find(x => x.DictionaryName == "Great Bay clear") ?? Default;
            var IkanaClear = instance.Logic.Find(x => x.DictionaryName == "Ikana clear") ?? Default;

            WoodFallClear.IsFake = false;
            SnowheadClear.IsFake = false;
            GreatBayClear.IsFake = false;
            IkanaClear.IsFake = false;
            //Set the area clear name to their defualt
            WoodFallClear.LocationName = "Defeat Odolwa";
            WoodFallClear.ItemName = "Odolwas Remians";
            SnowheadClear.LocationName = "Defeat Goht";
            SnowheadClear.ItemName = "Gohts Remians";
            GreatBayClear.LocationName = "Defeat Gyrog";
            GreatBayClear.ItemName = "Gyrogs Remians";
            IkanaClear.LocationName = "Defeat Twinmold";
            IkanaClear.ItemName = "Twinmolds Remians";
            //Find the name of the randomized area clear
            var newWoodfallLocation = (WoodFallClear.ClearRandomizedDungeonInThisArea(instance) ?? WoodFallClear).LocationName;
            var newWoodfallItem = (WoodFallClear.ClearRandomizedDungeonInThisArea(instance) ?? WoodFallClear).ItemName;
            var newSnowheadLocation = (SnowheadClear.ClearRandomizedDungeonInThisArea(instance) ?? SnowheadClear).LocationName;
            var newSnowheadItem = (SnowheadClear.ClearRandomizedDungeonInThisArea(instance) ?? SnowheadClear).ItemName;
            var newGreatBayLocation = (GreatBayClear.ClearRandomizedDungeonInThisArea(instance) ?? GreatBayClear).LocationName;
            var newGreatBayItem = (GreatBayClear.ClearRandomizedDungeonInThisArea(instance) ?? GreatBayClear).ItemName;
            var newIkanaLocation = (IkanaClear.ClearRandomizedDungeonInThisArea(instance) ?? IkanaClear).LocationName;
            var newIkanaItem = (IkanaClear.ClearRandomizedDungeonInThisArea(instance) ?? IkanaClear).ItemName;
            //Set the randomized area clear name to the original area clear
            WoodFallClear.LocationName = newWoodfallLocation;
            WoodFallClear.ItemName = newWoodfallItem;
            SnowheadClear.LocationName = newSnowheadLocation;
            SnowheadClear.ItemName = newSnowheadItem;
            GreatBayClear.LocationName = newGreatBayLocation;
            GreatBayClear.ItemName = newGreatBayItem;
            IkanaClear.LocationName = newIkanaLocation;
            IkanaClear.ItemName = newIkanaItem;
        }

        public static void SwapAreaClearLogic(LogicObjects.TrackerInstance Instance)
        {
            var areaClearData = Instance.EntranceAreaDic;
            var ReferenceLogic = Utility.CloneLogicList(Instance.Logic);
            foreach (var i in Instance.Logic)
            {
                if (areaClearData.ContainsKey(i.ID))
                {
                    var Dungeon = Instance.Logic[areaClearData[i.ID]];
                    if (Dungeon.RandomizedItem < 0) { return; }
                    var DungoneRandItem = Dungeon.RandomizedItem;
                    var RandomClear = areaClearData.FirstOrDefault(x => x.Value == DungoneRandItem).Key;
                    Instance.Logic[i.ID].Required = ReferenceLogic[RandomClear].Required;
                    Instance.Logic[i.ID].Conditionals = ReferenceLogic[RandomClear].Conditionals;
                }
            }
        }
    }
}
