using Newtonsoft.Json;
using System.Collections.Generic;

namespace MMR_Tracker_V2
{
    class Pathfinding
    {
        public static List<List<LogicObjects.Map>> paths = new List<List<LogicObjects.Map>>();

        public static bool UseSongOfTime = false;

        public static bool IncludeItemLocations = false;

        public static List<List<LogicObjects.Map>> FindLogicalEntranceConnections(List<LogicObjects.LogicEntry> logic)
        {
            var result = new List<List<LogicObjects.Map>> { new List<LogicObjects.Map>(), new List<LogicObjects.Map>() };

            var logicTemplate = Utility.CloneLogicList(logic);

            foreach (LogicObjects.LogicEntry entry in logicTemplate)
            {
                if (entry.ItemSubType == "Entrance")
                {
                    entry.Aquired = false;
                    if (entry.LocationArea != "Owl Warp") { entry.Available = false; }
                }
            }

            foreach (LogicObjects.LogicEntry entry in logic)
            {
                if (entry.RandomizedItem > -1 && entry.ItemSubType == "Entrance" && entry.Checked)
                {
                    var dummyLogic = Utility.CloneLogicList(logicTemplate);
                    var ExitToCheck = dummyLogic[entry.RandomizedItem];
                    ExitToCheck.Aquired = true;
                    LogicEditing.CalculateItems(dummyLogic, true);
                    foreach (var dummyEntry in dummyLogic)
                    {
                        if (dummyEntry.Available &&
                            !dummyEntry.IsFake &&
                            (dummyEntry.ItemSubType == "Entrance" || IncludeItemLocations) &&
                            CheckSOT(ExitToCheck, dummyEntry))
                        {
                            var newEntry = new LogicObjects.Map
                            {
                                CurrentExit = ExitToCheck.ID,
                                Entrance = dummyEntry.ID,
                                ResultingExit = (dummyEntry.RandomizedItem > -1) ? dummyEntry.RandomizedItem : -2,
                                IsOwlWarp = (dummyEntry.LocationArea == "Owl Warp")
                            };
                            result[0].Add(newEntry);
                            if (newEntry.ResultingExit > -1 && dummyEntry.ItemSubType == "Entrance") { result[1].Add(newEntry); }
                        }
                    }
                }

            }
            return result;
        }

        public static void Findpath(
            List<LogicObjects.Map> map, //A map of all available entrances from each exit as long as the result of that entrance is known
            List<LogicObjects.Map> FullMap, //A map of all available exit from each entrance
            int startinglocation, //The ID of the last exit you came from
            int destination, //The ID of the entrance you want to reach
            List<int> ExitsSeen, //The IDs of each entrance you have seen in the current area as well as all surrounding areas
            List<int> ExitsSeenOriginal, //The same as exits seen but does not conatin entrance found during this execution of the function
            List<LogicObjects.Map> Path, //A list of exits you have taken to get to your current point
            bool InitialRun //Is this code being run from the original source (True) or from it's self (False)
            )
        {
            if (InitialRun) { paths = new List<List<LogicObjects.Map>>(); }
            //There are no logical exits from majoras lair, this however is used to lock inaccesable exits
            if (LogicObjects.Logic[startinglocation].DictionaryName == "EntranceMajorasLairFromTheMoon") { return; }
            //Make a copy to edit and pass to the next funtion
            var ExitsSeenCopy = JsonConvert.DeserializeObject<List<int>>(JsonConvert.SerializeObject(ExitsSeen));
            //The path finder will use this to ignore seen entrances in areas with new entrances and seen entrances
            var ExitsSeenOriginalCopy = JsonConvert.DeserializeObject<List<int>>(JsonConvert.SerializeObject(ExitsSeen));
            //A list of exits available to take
            var validExits = new List<LogicObjects.Map>();

            foreach (var point in FullMap)
            {
                if (point.CurrentExit == startinglocation && point.Entrance == destination) { paths.Add(Path); }
            }
            //For each point in our trimmed map 
            foreach (var point in map)
            {
                if (point.CurrentExit == startinglocation && (!ExitsSeenOriginal.Contains(point.Entrance)))
                {
                    if (CheckExitValid(map, point.ResultingExit, ExitsSeenCopy))
                    {
                        validExits.Add(point);
                        ExitsSeen.Add(point.Entrance);
                    }
                }
            }
            //Pick the first entrance in the valid exits list, rerun the function with the resulting exit as the starting location
            foreach (var exit in validExits)
            {
                var UpdatedPath = JsonConvert.DeserializeObject<List<LogicObjects.Map>>(JsonConvert.SerializeObject(Path));
                UpdatedPath.Add(exit);
                Findpath(map, FullMap, exit.ResultingExit, destination, ExitsSeenCopy, ExitsSeenOriginalCopy, UpdatedPath, false);
            }
        }

        public static bool CheckExitValid(List<LogicObjects.Map> map, int startinglocation, List<int> ExitsSeen)
        {
            /* This checks all of the exits in the area we would go to if we took this exit. If we have seen all of these exits
            Before we have already been to or seen this area earlier in our path so there is no need to take this exit.

            TODO If there is only one new exit that we can see from this entrance, the pathfinder needs to know to only 
            check that exit in the future and skip the ones we've already seen. */
            var good = false;
            foreach (var point in map)
            {
                if (point.CurrentExit == startinglocation && !ExitsSeen.Contains(point.Entrance))
                {
                    good = true;
                    ExitsSeen.Add(point.Entrance);
                }
            }
            return good;
        }

        private static bool CheckSOT(LogicObjects.LogicEntry EntranceToCheck, LogicObjects.LogicEntry dummyEntry)
        {
            var songOfTime = (UseSongOfTime) ? "" : "EntranceSouthClockTownFromClockTowerInterior";
            if (dummyEntry.DictionaryName == songOfTime)
            {
                if (EntranceToCheck.DictionaryName == "EntranceClockTowerInteriorFromBeforethePortaltoTermina" || EntranceToCheck.DictionaryName == "EntranceClockTowerInteriorFromSouthClockTown")
                {
                    return true;
                }
                else { return false; }
            }
            return true;
        }
        public static string SetSOTName(LogicObjects.Map stop)
        {
            var StartName = LogicObjects.Logic[stop.CurrentExit].DictionaryName;
            var entName = LogicObjects.Logic[stop.Entrance].DictionaryName;
            if (entName == "EntranceSouthClockTownFromClockTowerInterior")
            {
                if (StartName == "EntranceClockTowerInteriorFromBeforethePortaltoTermina" || StartName == "EntranceClockTowerInteriorFromSouthClockTown")
                {
                    return LogicObjects.Logic[stop.Entrance].LocationName;
                }
                else { return "Song of Time"; }
            }
            return LogicObjects.Logic[stop.Entrance].LocationName; ;
        }
    }
}
