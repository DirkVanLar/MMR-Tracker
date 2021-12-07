using MMR_Tracker_V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker.Other_Games
{
    class GlobalHandeler
    {
        public static bool HandleOtherGameSpoilerLog(LogicObjects.TrackerInstance instance, string[] Spoiler)
        {
            if (instance.GameCode == "WWR" && Spoiler[0].Contains("Wind Waker Randomizer"))
            {
                return false;
            }
            else if (instance.GameCode == "OOTR" && Spoiler[1].Contains(":version"))
            {
                return OcarinaOfTimeRando.HandleOOTRSpoilerLog(instance, Spoiler);
            }
            else if (instance.GameCode == "SSR" && Spoiler[0].Contains("Skyward Sword Randomizer"))
            {
                return SkywardSwordRando.HandleSSRSpoilerLog(instance, Spoiler);
            }
            return false;
        }
    }
}
