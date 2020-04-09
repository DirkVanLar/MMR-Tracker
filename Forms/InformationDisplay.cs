using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    public partial class InformationDisplay : Form
    {

        public static List<string> Playthrough = new List<string>();

        public int DebugFunction = 0;
        public InformationDisplay()
        {
            InitializeComponent();
        }

        private void DebugScreen_Load(object sender, EventArgs e)
        {
            ResizeObject();
            this.Text = "Debug Screen";
            switch (DebugFunction)
            {
                case 0:
                    this.Close();
                    break;
                case 1:
                    PrintLogicToListBox(LogicObjects.MainTrackerInstance);
                    break;
                case 2:
                    PrintInfo();
                    break;
                case 3:
                    PrintPlaythrough();
                    break;
            }
        }

        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (DebugFunction == 2)
            {
                if (listBox1.SelectedItem.ToString().Contains("https://github.com/Thedrummonger/MMR-Tracker")) { System.Diagnostics.Process.Start("https://github.com/Thedrummonger/MMR-Tracker"); }
                if (listBox1.SelectedItem.ToString().Contains("https://github.com/ZoeyZolotova/mm-rando")) { System.Diagnostics.Process.Start("https://github.com/ZoeyZolotova/mm-rando"); }
                if (listBox1.SelectedItem.ToString().Contains("https://discord.gg/TJZ4uCP")) { System.Diagnostics.Process.Start("https://discord.gg/TJZ4uCP"); }
                if (listBox1.SelectedItem.ToString().Contains("bigmuffthedrummonger@gmail.com"))  { System.Diagnostics.Process.Start("mailto:bigmuffthedrummonger@gmail.com"); }
            }
        }

        private void DebugScreen_ResizeEnd(object sender, EventArgs e)
        {
            ResizeObject();
            Console.WriteLine(DebugFunction);
            switch (DebugFunction)
            {
                case 0:
                    this.Close();
                    break;
                case 1:
                    break;
                case 2:
                    PrintInfo();
                    break;
                case 3:
                    break;
            }
        }

        public void PrintLogicToListBox(LogicObjects.TrackerInstance Instance)
        {
            this.Text = "Logic Object";
            List<LogicObjects.LogicEntry> Logic = Instance.Logic;
            for (int i = 0; i < Logic.Count; i++)
            {
                listBox1.Items.Add("---------------------------------------");
                listBox1.Items.Add("ID: " + Logic[i].ID);
                listBox1.Items.Add("Name: " + Logic[i].DictionaryName);
                listBox1.Items.Add("Location: " + Logic[i].LocationName);
                listBox1.Items.Add("Item: " + Logic[i].ItemName);
                listBox1.Items.Add("Location area: " + Logic[i].LocationArea);
                listBox1.Items.Add("Item Sub Type: " + Logic[i].ItemSubType);
                listBox1.Items.Add("Available: " + Logic[i].Available);
                listBox1.Items.Add("Aquired: " + Logic[i].Aquired);
                listBox1.Items.Add("Checked: " + Logic[i].Checked);
                listBox1.Items.Add("Fake Item: " + Logic[i].IsFake);
                listBox1.Items.Add("Random Item: " + Logic[i].RandomizedItem);
                listBox1.Items.Add("Spoiler Log Location name: " + Logic[i].SpoilerLocation);
                listBox1.Items.Add("Spoiler Log Item name: " + Logic[i].SpoilerItem);
                listBox1.Items.Add("Spoiler Log Randomized Item: " + Logic[i].SpoilerRandom);
                if (Logic[i].RandomizedState() == 0) { listBox1.Items.Add("Randomized State: Randomized"); }
                if (Logic[i].RandomizedState() == 1) { listBox1.Items.Add("Randomized State: Unrandomized"); }
                if (Logic[i].RandomizedState() == 2) { listBox1.Items.Add("Randomized State: Forced Fake"); }
                if (Logic[i].RandomizedState() == 3) { listBox1.Items.Add("Randomized State: Forced Junk"); }

                listBox1.Items.Add("Starting Item: " + Logic[i].StartingItem());

                string av = "Available On: ";
                if (((Logic[i].AvailableOn >> 0) & 1) == 1) { av += "Day 1, "; }
                if (((Logic[i].AvailableOn >> 2) & 1) == 1) { av += "Day 2, "; }
                if (((Logic[i].AvailableOn >> 4) & 1) == 1) { av += "Day 3, "; }
                if (((Logic[i].AvailableOn >> 1) & 1) == 1) { av += "Night 1, "; }
                if (((Logic[i].AvailableOn >> 3) & 1) == 1) { av += "Night 2, "; }
                if (((Logic[i].AvailableOn >> 5) & 1) == 1) { av += "Night 3, "; }
                listBox1.Items.Add(av);
                av = "Needed By: ";
                if (((Logic[i].NeededBy >> 0) & 1) == 1) { av += "Day 1, "; }
                if (((Logic[i].NeededBy >> 2) & 1) == 1) { av += "Day 2, "; }
                if (((Logic[i].NeededBy >> 4) & 1) == 1) { av += "Day 3, "; }
                if (((Logic[i].NeededBy >> 1) & 1) == 1) { av += "Night 1, "; }
                if (((Logic[i].NeededBy >> 3) & 1) == 1) { av += "Night 2, "; }
                if (((Logic[i].NeededBy >> 5) & 1) == 1) { av += "Night 3, "; }
                listBox1.Items.Add(av);

                var test2 = Logic[i].Required;
                if (test2 == null) { listBox1.Items.Add("NO REQUIREMENTS"); }
                else
                {
                    listBox1.Items.Add("Required");
                    for (int j = 0; j < test2.Length; j++)
                    {
                        listBox1.Items.Add(Logic[test2[j]].ItemName ?? Logic[test2[j]].DictionaryName);
                    }
                }
                var test3 = Logic[i].Conditionals;
                if (test3 == null) { listBox1.Items.Add("NO CONDITIONALS"); }
                else
                {
                    for (int j = 0; j < test3.Length; j++)
                    {
                        listBox1.Items.Add("Conditional " + j);
                        for (int k = 0; k < test3[j].Length; k++)
                        {
                            listBox1.Items.Add(Logic[test3[j][k]].ItemName ?? Logic[test3[j][k]].DictionaryName);
                        }
                    }
                }
            }
        }

        public void PrintInfo()
        {
            var instance = LogicObjects.MainTrackerInstance;
            listBox1.Items.Clear();
            List<string> Lines = new List<string>();
            this.Text = "Info";
            Lines.Add("Majoras Mask Randomizer Tracker");
            Lines.Add("Tracker Github: https://github.com/Thedrummonger/MMR-Tracker");
            Lines.Add("Version: " + VersionHandeling.trackerVersion);
            Lines.Add("By: Thedrummonger");
            Lines.Add("Contact: bigmuffthedrummonger@gmail.com");
            Lines.Add(Utility.CreateDivider(listBox1, "For use with the Majoras Mask Randomizer"));
            Lines.Add("Majoras Mask Randomizer By: ZoeyZolotova");
            Lines.Add("Randomizer Github: https://github.com/ZoeyZolotova/mm-rando");
            Lines.Add("Randomizer Discord: https://discord.gg/TJZ4uCP");
            Lines.Add("(Double click links to open them.)");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Importing Logic:");
            Lines.Add("You will start by importing the logic you used to generate your rom.");
            Lines.Add("You can import a logic file or use the default Causal or Glitched Logic as they are found in the dev branch of the github repository.");
            Lines.Add("You can also select a settings file (.MMRTSET) generated by the Tracker. This will use the logic the tracker had when you saved the settings originally and will automatically apply the settings");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Using The Tracker:");
            Lines.Add("The tracker will show you all available locations based on your logic and obtained items.");
            Lines.Add("Double clicking a location will bring up the item select list.");
            Lines.Add("From here you will select the item you found at this location.");
            Lines.Add("This will mark that item as obtained and recalculate what locations are available.");
            Lines.Add("The location will be moved to \"Checked locations\" and display what item it contained.");
            Lines.Add("Double Clicking an item in \"Checked locations\" will uncheck that location and mark the corrisponding item as unobtained");
            Lines.Add(Utility.CreateDivider(listBox1));
            if ((instance.IsEntranceRando() && instance.Options.entranceRadnoEnabled) || instance.Version == 0)
            {
                Lines.Add("Pathfinder:");
                if (instance.Version == 0) { Lines.Add("(This is only available if entrances are randomized.)"); }
                Lines.Add("The path finder will show you the path from one entrance to another.");
                Lines.Add("You will enter the last exit you came out of (or exit closest to you) along with the entrance you want to end up in front of.");
                Lines.Add("It will generate a path that will put you in an area from which you can access the destination Entrance");
                Lines.Add(Utility.CreateDivider(listBox1));
            }
            Lines.Add("List Filtering:");
            Lines.Add("Typing in the text box above a list will filter the items in the list.");
            Lines.Add("You can filter by area by typing # at the beggining of your filter.");
            Lines.Add("You can filter by Item Type (Bottle, Entrance, item etc) by typing @ at the beggining of your filter.");
            Lines.Add("You can filter by Location Name by typing $ at the beggining of your filter.");
            Lines.Add("You can filter by Item Name by typing % at the beggining of your filter.");
            Lines.Add("You can filter by The Item Name of the randomized Item found in a location by typing & at the beggining of your filter.");
            Lines.Add("You can filter multiple things at once by seperating them with a |. (Pipe) (located below the backspace key on most keyboards)");
            Lines.Add("For example typing \"Clock|Wood\" will show all checks that contain the word \"Clock\" as well as all checks that contain \"Wood\".");
            Lines.Add("You can also filter by multiple words by seperating them with a ,. (Comma)");
            Lines.Add("For example typing \"Clock,Wood\" will show only checks that contain both. the word \"clock\" and the word \"wood\".");
            Lines.Add("These methods can be combined in the same search.");
            Lines.Add("For example typing \"#Clock|Wood,fairy\" in the search box will show all entries who area contains the word \"Clock\" or display name contains both the words \"Wood\" and \"Fairy\".");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Setting Item vs Marking Item:");
            Lines.Add("The set item and set entrance button will mark an item/entrance as being at a location without actually marking that item/entrance as being obtained.");
            Lines.Add("This is usefull when you know what item is in a location but haven't actually obtianed it such as if you see it in a shop or read about it in a hint.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("OPTIONS MENU:");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("LOGIC OPTIONS:");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Randomization options:");
            Lines.Add("There are 4 different states a location can be");
            Lines.Add("1. Randomized");
            Lines.Add("The location is randomized as normal. It will appear in the tracker and ask for the user to input what item is located there.");
            Lines.Add("2. Unrandomized");
            Lines.Add("The location is not randomized. It will not appear in the tracker and automatically be marked as obtained whenever it becomes available.");
            Lines.Add("3. Unrandomized (Manual)");
            Lines.Add("The location is not randomized but must be marked manually. It will still appear in the tracker, but must be double clicked to be marked as obtained.");
            Lines.Add("4. Forced Junk");
            Lines.Add("The location is randomized but will never contain an item usefull to logic The location will simply not appear in the tracker.");
            Lines.Add("An item can also be marked as a starting item.");
            Lines.Add("This will make the tracker always consider the item obtained when calculating logic.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Saving/Loading Randomization Options:");
            Lines.Add("You can save your options by clicking the save settings button");
            Lines.Add("This will create a .MMRTSET file that can be loaded by clicking Load Settings");
            Lines.Add("As mentioned above this .MMRTSET file can be use as logic as it will contain the the logic file the tracker is using when the setting file is created.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Importing Spoiler log:");
            Lines.Add("This will allow you to import the spoiler log generated with your rom.");
            Lines.Add("After you have imoprted the spoiler log checking/marking a location will automatically Fill in the appropriate item based on your spoiler log.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Strict Logic Handeling:");
            Lines.Add("This option might make your logic calculations a bit slower, but will prevent rare bugs that occur involvolving circular dependencies in logic.");
            Lines.Add("You should never need to enable this, but it's worth a try if logic is being buggy.");
            Lines.Add(Utility.CreateDivider(listBox1));
            if ((instance.IsEntranceRando() && instance.Options.entranceRadnoEnabled) || instance.Version == 0)
            {
                Lines.Add("ENTRANCE RADNO:");
                if (instance.Version == 0) { Lines.Add("(These options are only available if entrances are randomized.)"); }
                Lines.Add(Utility.CreateDivider(listBox1));
                Lines.Add("Use Song Of Time In Path finder:");
                Lines.Add("By default using song of time is not considered in the pathfinder");
                Lines.Add("Clicking this will toggle Using Song Of time in the Pathfinder");
                Lines.Add(Utility.CreateDivider(listBox1));
                Lines.Add("Include Item locations as destination:");
                Lines.Add("This option will add Item locations to the destination Combo box");
                Lines.Add("By selecting an item location as your destination the pathfinder will attempt to find a path that will garantee you access to the chosen Check.");
                Lines.Add(Utility.CreateDivider(listBox1));
                Lines.Add("Toggle Entrance Rando Features:");
                Lines.Add("This will toggle the available entrances and path finder lists");
                Lines.Add("If this Feature is off, entrances will show up in the available items list.");
                Lines.Add(Utility.CreateDivider(listBox1));
                Lines.Add("Couple Entrances:");
                Lines.Add("This option will assume that an entrance is the same both ways");
                Lines.Add("For example if (Ikana > Sakon) Leads to (NCT < Mayor) Then it will assume (NCT > Mayor) Leads to (Ikana < Sakon)");
                Lines.Add("If this option is enabled when you mark an entrance it will automatcially mark the entrance in reverse as well assuming it's not a one way.");
                Lines.Add(Utility.CreateDivider(listBox1));
            }
            Lines.Add("MISC OPTIONS:");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Show Entry Name Tooltip:");
            Lines.Add("This will toggle the tooltip that displays the full name of an item when you mouse over an item in a list.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Tools:");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Seed Checker:");
            Lines.Add("The seed checker will tell you if your seed can reach the selected items using your current logic.");
            Lines.Add("You will be asked to provide the spoiler log for the seed you want to check");
            Lines.Add("If a spoiler log is already imported into the tracker it will use that data.");
            Lines.Add("Adding a location to the Checks Ignored list will check if the selected items are obtainable assuming you never do the selected checks. ");
            Lines.Add("The seed checker will not reveal what items are on what checks.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Playthorugh Generator:");
            Lines.Add("The play through generator will show you all of the items you need to obtain to beat the game along with what order you need to obtain them.");
            Lines.Add("You will be asked to provide the spoiler log which will be used to generate the playthrough");
            Lines.Add("If a spoiler log is already imported into the tracker it will use that data.");
            Lines.Add("If the spoiler log is not able to assign all checks data, a playthrough will not be generated.");
            Lines.Add("This will spoil your seed! only use this if you already know where items are or don't care to be spoiled.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("What Unlocked This:");
            Lines.Add("This will show what items you have obtained that made this check available in your tracker.");
            Lines.Add("It will attempt to show data for the last item you selected in the avalable item/entrances list box.");
            Lines.Add("If it can't find that information it will you ask you to imput an item from a list of avalable items.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Logic Editor:");
            Lines.Add("This is and advanced version of the logic editor found in the majoras mask randomizer.");
            Lines.Add("It containes all of features present in that editor plus a number of new features including:");
            Lines.Add("-Undo / Redo functionality.");
            Lines.Add("-The ability to go back to the last entry after using the \"Go To\" button.");
            Lines.Add("-Displaying item and location names along with the logic name.");
            Lines.Add("-A larger Conditionals window for viewing complex conditionals.");
            Lines.Add("-Copy / Pasting entries.");
            Lines.Add("-The ability to reorder fake items while auto updating values in other entries.");
            Lines.Add("-The ability to rename fake items.");
            Lines.Add("When the logic editor loads it will automatically import the logic used by the tracker.");
            Lines.Add("If no Logic is present you can load logic from a logic file or create logic from scratch by adding new items.");
            Lines.Add("Once you've made your changes you can save the logic to a file or apply it directly to the tracker.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Update Logic:");
            Lines.Add("This will let you swamp the logic you're currently using.");
            Lines.Add("The tracker will attempt to preserve your check location/entrances and what they contained, however this may not always work if the changes in logic are to drastic.");
            Lines.Add(Utility.CreateDivider(listBox1));

            foreach (var i in Lines)
            {
                foreach (var j in Utility.SeperateStringByMeasurement(listBox1, i))
                {
                    listBox1.Items.Add(j);
                }
            }

        }

        public void PrintPlaythrough()
        {
            this.Text = "Playthrough";
            foreach (var i in Playthrough) { listBox1.Items.Add(i); }
        }

        public void ResizeObject()
        {
            listBox1.Height = this.Height - 60;
            listBox1.Width = this.Width - 40;
        }
    }
}
