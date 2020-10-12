using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    public partial class InformationDisplay : Form
    {

        public int LBX;
        public int LBY;

        public static List<string> Playthrough = new List<string>();

        public int DebugFunction = 0;
        public InformationDisplay()
        {
            InitializeComponent();
        }

        private void DebugScreen_Load(object sender, EventArgs e)
        {
            LBX = listBox1.Location.X;
            LBY = listBox1.Location.Y;
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
                case 4:
                    WhatUnlockedThis();
                    break;
                case 5:
                    ShowIPHelp();
                    break;
            }
            ResizeObject();
        }

        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (this.Text == "Info")
            {
                if (listBox1.SelectedItem.ToString().Contains("https://github.com/Thedrummonger/MMR-Tracker")) { System.Diagnostics.Process.Start("https://github.com/Thedrummonger/MMR-Tracker"); }
                if (listBox1.SelectedItem.ToString().Contains("https://github.com/ZoeyZolotova/mm-rando")) { System.Diagnostics.Process.Start("https://github.com/ZoeyZolotova/mm-rando"); }
                if (listBox1.SelectedItem.ToString().Contains("https://discord.gg/TJZ4uCP")) { System.Diagnostics.Process.Start("https://discord.gg/TJZ4uCP"); }
                if (listBox1.SelectedItem.ToString().Contains("bigmuffthedrummonger@gmail.com"))  { System.Diagnostics.Process.Start("mailto:bigmuffthedrummonger@gmail.com"); }
                if (listBox1.SelectedItem.ToString().Contains("(Click here for latest release)")) { System.Diagnostics.Process.Start("https://github.com/Thedrummonger/MMR-Tracker/releases"); }
                if (listBox1.SelectedItem.ToString().Contains("(Click here to open the online play help page)"))
                {
                    InformationDisplay DebugScreen = new InformationDisplay();
                    DebugScreen.DebugFunction = 5;
                    DebugScreen.Show();
                }

            }
            if (this.Text == "Online Play Help")
            {
                if (listBox1.SelectedItem.ToString().Contains("https://www.noip.com/support/knowledgebase/general-port-forwarding-guide/")) { System.Diagnostics.Process.Start("https://www.noip.com/support/knowledgebase/general-port-forwarding-guide/"); }
                if (listBox1.SelectedItem.ToString().Contains("https://github.com/kaklakariada/portmapper")) { System.Diagnostics.Process.Start("https://github.com/kaklakariada/portmapper"); }
                if (listBox1.SelectedItem.ToString().Contains("https://kb.netgear.com/20878/Finding-your-IP-address-without-using-the-command-prompt")) { System.Diagnostics.Process.Start("https://kb.netgear.com/20878/Finding-your-IP-address-without-using-the-command-prompt"); }
                if (listBox1.SelectedItem.ToString().Contains("(Click here to open the about page)"))
                {
                    InformationDisplay DebugScreen = new InformationDisplay();
                    DebugScreen.DebugFunction = 2;
                    DebugScreen.Show();
                }
            }
        }

        private void DebugScreen_ResizeEnd(object sender, EventArgs e)
        {
            ResizeObject();
            if (this.Text == "Info") { PrintInfo(); }
            if (this.Text == "Online Play Help") { ShowIPHelp(); }
        }

        public void PrintLogicToListBox(LogicObjects.TrackerInstance Instance)
        {
            this.Text = "Logic Object";
            List<LogicObjects.LogicEntry> Logic = Instance.Logic;
            listBox1.BeginUpdate();
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
                listBox1.Items.Add("Spoiler Log Location name: " + string.Join(",", Logic[i].SpoilerLocation));
                listBox1.Items.Add("Spoiler Log Item name: " + string.Join(",", Logic[i].SpoilerItem));
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
            listBox1.EndUpdate();
        }

        public void PrintInfo()
        {
            var instance = LogicObjects.MainTrackerInstance;
            listBox1.Items.Clear();
            List<string> Lines = new List<string>();
            this.Text = "Info";
            Lines.Add(Utility.CreateDivider(listBox1, "Majoras Mask Randomizer Tracker"));
            Lines.Add("Tracker Github: https://github.com/Thedrummonger/MMR-Tracker");
            Lines.Add("Version: " + VersionHandeling.trackerVersion + " (Click here for latest release)");
            Lines.Add("By: Thedrummonger");
            Lines.Add("Contact: bigmuffthedrummonger@gmail.com");
            Lines.Add(Utility.CreateDivider(listBox1, "For use with the Majoras Mask Randomizer"));
            Lines.Add("Majoras Mask Randomizer By: ZoeyZolotova");
            Lines.Add("Randomizer Github: https://github.com/ZoeyZolotova/mm-rando");
            Lines.Add("Randomizer Discord: https://discord.gg/TJZ4uCP");
            Lines.Add(Utility.CreateDivider(listBox1, "Credit to:"));
            Lines.Add("Xy172: Map Filter and bug testing");
            Lines.Add("ZoeyZolotova and the Majoras mask rando team: The majoras mask randomizer");
            Lines.Add("ColbyDude: Ripping A majority of the item tracker Icons");
            Lines.Add("(Double click links to open them.)");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Importing Logic:");
            Lines.Add("You will start by importing the logic you used to generate your rom.");
            Lines.Add("You can import a logic file or use the default Causal or Glitched Logic as they are found in the dev branch of the github repository.");
            Lines.Add("You can also select a Save file (.MMRSAV) from a previous run. This will apply the logic used in that saved file and give you the option to import it's settings. (Progress will not be imported)");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Using The Tracker:");
            Lines.Add("The tracker will show you all available locations based on your logic and obtained items.");
            Lines.Add("Double clicking a location will bring up the item select list.");
            Lines.Add("From here you will select the item you found at this location. (This is done for you if a spoiler log is imported)");
            Lines.Add("This will mark that item as obtained and recalculate what locations are available.");
            Lines.Add("The location will be moved to \"Checked locations\" and display what item it contained.");
            Lines.Add("Double Clicking an item in \"Checked locations\" will uncheck that location and mark the corrisponding item as unobtained");
            Lines.Add(Utility.CreateDivider(listBox1));
            if ((instance.EntranceRando && instance.Options.EntranceRadnoEnabled) || instance.LogicVersion == 0)
            {
                Lines.Add("Pathfinder:");
                if (instance.LogicVersion == 0) { Lines.Add("(This is only available if entrances are randomized.)"); }
                Lines.Add("The path finder will show you the path from one entrance to another.");
                Lines.Add("You will enter the last exit you came out of (or exit closest to you) along with the entrance you want to end up in front of.");
                Lines.Add("It will generate a path that will put you in an area from which you can access the destination Entrance");
                Lines.Add(Utility.CreateDivider(listBox1));
            }
            Lines.Add("List Filtering:");
            Lines.Add("Typing in the text box above a list will filter the items in the list.");
            Lines.Add("You can use certain symbols to further refine your search. Only one symbol can be used per search term.");
            Lines.Add("You can filter by area by typing # at the beggining of your filter.");
            Lines.Add("You can filter by logic name (The name used in the logic file) by typing ~ at the beggining of your filter.");
            Lines.Add("You can filter by Item Name by typing % at the beggining of your filter.");
            Lines.Add("You can filter by Location Name by typing $ at the beggining of your filter.");
            Lines.Add("You can filter by Item Type (Bottle, Entrance, item etc) by typing @ at the beggining of your filter.");
            Lines.Add("You can filter by The Item Name of the randomized Item found in a location by typing _ at the beggining of your filter.");
            Lines.Add("Typing * at the beggining of your search will show things that match your search and are starred. Typing ** as your search will show you all starred entries.");
            Lines.Add("Typing ! at the beggining of your search will invert the filter, showing only things that don't match the search.");
            Lines.Add("Typing = at the beggining of your search will show only things that match your search exactly.");
            Lines.Add("Unlike other symbols ! and = can be used alongside other symbols as long as they are at the beggining of a term. ! and = do nothing when used together.");
            Lines.Add("Adding ^ to the begging of your search string will cause the corrisopnding list box to show all checks reguardless of whether they are available as long as they match the filter.");
            Lines.Add("You can filter multiple things at once by seperating them with a |. (Pipe, located below the backspace key on most keyboards)");
            Lines.Add("For example typing \"Clock|Wood\" will show all checks that contain the word \"Clock\" as well as all checks that contain \"Wood\".");
            Lines.Add("You can also filter by multiple words by seperating them with a &.");
            Lines.Add("For example typing \"Clock&Wood\" will show only checks that contain both. the word \"clock\" and the word \"wood\".");
            Lines.Add("These methods can be combined in the same search.");
            Lines.Add("For example typing \"#Clock|Wood&fairy\" in the search box will show all entries who area contains the word \"Clock\" or display name contains both the words \"Wood\" and \"Fairy\".");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Setting a Location vs Checking a Location:");
            Lines.Add("The set item and set entrance button will mark an item/entrance as being at a location without actually marking that item/entrance as being obtained.");
            Lines.Add("This is usefull when you know what item is in a location but haven't actually obtianed it such as if you see it in a shop or read about it in a hint.");
            Lines.Add("Set items (Also reffered to as Marked Items) will always appear in your list even if they aren't available. If they aren't available, the text will have a strike through.");
            Lines.Add("You can middle click an item to set it quickly as long as the middle click function is set to set item in Options -> Misc Options.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Starring an item:");
            Lines.Add("Starring an item is only used to indicate the entry should be kept in mind, it has no effect on the entry logically.");
            Lines.Add("Starred items will be noted with a * after the text and will be bolded.");
            Lines.Add("Starred items will always appear in your list even if they aren't available. If they aren't available, the text will have a strike through.");
            Lines.Add("You can middle click an item to Star it quickly as long as the middle click function is set to star item in Options -> Misc Options.");
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
            Lines.Add("Radnomization options are save along with your progress in each save file.");
            Lines.Add("Loading a save file in the Radnomization options page will import the setting from that save file without importing progress.");
            Lines.Add("You can save a template save file that will only conatin your current radnomization options.");
            Lines.Add("You can import the settings json file created by the randomizer to import those settings into the tracker.");
            Lines.Add("You can also import setting strings for randomized locations/entrances and junk locations from the randomizer (Starting item setting string not currently supported).");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Importing Spoiler log:");
            Lines.Add("This will allow you to import the spoiler log generated with your rom.");
            Lines.Add("After you have imoprted the spoiler log checking/marking a location will automatically Fill in the appropriate item based on your spoiler log.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Strict Logic Handeling:");
            Lines.Add("This option might make your logic calculations a bit slower, but will prevent rare bugs that occur involvolving circular dependencies in logic.");
            Lines.Add("You should amost never need to enable this, but it's worth a try if logic is being buggy.");
            Lines.Add(Utility.CreateDivider(listBox1));
            if ((instance.EntranceRando && instance.Options.EntranceRadnoEnabled) || instance.LogicVersion == 0)
            {
                Lines.Add("ENTRANCE RADNO:");
                if (instance.LogicVersion == 0) { Lines.Add("(These options are only available if entrances are randomized.)"); }
                Lines.Add(Utility.CreateDivider(listBox1));
                Lines.Add("Use Song Of Time In Path finder:");
                Lines.Add("By default using song of time is not considered in the pathfinder. Clicking this will toggle Using Song Of time in the Pathfinder");
                Lines.Add(Utility.CreateDivider(listBox1));
                Lines.Add("Include Item locations as destination:");
                Lines.Add("This option will add Item locations to the destination Combo box.");
                Lines.Add("By selecting an item location as your destination the pathfinder will attempt to find a path that will garantee you access to the chosen Check.");
                Lines.Add(Utility.CreateDivider(listBox1));
                Lines.Add("Toggle Entrance Rando Features:");
                Lines.Add("This will toggle the available entrances and path finder lists");
                Lines.Add("If this Feature is off, entrances will show up in the available items list.");
                Lines.Add("The pathfinder can still be accessed using the popout pathfinder.");
                Lines.Add(Utility.CreateDivider(listBox1));
                Lines.Add("Couple Entrances:");
                Lines.Add("This option will make the tracker assume that an entrance is the same both ways");
                Lines.Add("For example if (Ikana > Sakon) Leads to (NCT < Mayor) Then it will assume (NCT > Mayor) Leads to (Ikana < Sakon)");
                Lines.Add("If this option is enabled when you mark an entrance it will automatcially mark the entrance in reverse as well assuming it's not a one way.");
                Lines.Add(Utility.CreateDivider(listBox1));
            }
            Lines.Add("MISC OPTIONS:");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Show Entry Name Tooltip:");
            Lines.Add("This will toggle the tooltip that displays the full name of an item when you mouse over an item in a list.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Seperate marked items:");
            Lines.Add("If this option is enabled, set/marked items will be moved to the bottom of the list box.");
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
            Lines.Add("The playthrough generator will use the logic entry \"MMRTGameClear\" to determine what constitutes beating the game. If this entry does not exist in your logic it will be created using default rquirements");
            Lines.Add("This will spoil your seed! only use this if you already know where items are or don't care to be spoiled.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("What Unlocked This:");
            Lines.Add("This will show what items you have obtained that made this check available in your tracker.");
            Lines.Add("It will attempt to show data for the last item you selected in the avalable item/entrances list box.");
            Lines.Add("If it can't find that information it will you ask you to imput an item from a list of avalable items.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Logic Editor:");
            Lines.Add("This is an advanced version of the logic editor found in the majoras mask randomizer.");
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
            Lines.Add("This will let you swap the logic you're currently using.");
            Lines.Add("The tracker will attempt to preserve your check location/entrances and what they contained, however this may not always work if the changes in logic are to drastic.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Filter Map:");
            Lines.Add("This will display a map of termina that can be clicked on to filter the tracker by the selected area.");
            Lines.Add("Right clicking an area will allow you to filter by specific sub areas in that location.");
            Lines.Add("Holding control will add the selected filter to your current filter instead of overwriting it.");
            Lines.Add("Use the Locations, entrances and Checked check boxes to determine which list boxes will be filtered .");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Item Tracker:");
            Lines.Add("The item tracker will give you a visual display of what items you have obtained.");
            Lines.Add("Items will be greyed out until they are obtained. Some items such as boss remains will become active once they become available.");
            Lines.Add("The moon icon in the bottom left will become active when the Game is beatable (Go mode). This is determined the same way the pathfinder determined game clear.");
            Lines.Add("NOTE: you may notice some slight slowdown when using this feature.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Popout Pathfinder:");
            Lines.Add("This will open a new window containing the pathfinder.");
            Lines.Add("Multiple of these pathfinders can be open at once allowing you to keep track of multiple paths.");
            Lines.Add(Utility.CreateDivider(listBox1));
            Lines.Add("Online Play:");
            Lines.Add("Online play allows you to sync items you check and mark in your tracker with others over the internet.");
            Lines.Add("This is usefull for co-op or online playthroughs where items and knowlege are shared.");
            Lines.Add("More information can be found in the Online play help page.");
            Lines.Add("(Click here to open the online play help page)");
            Lines.Add(Utility.CreateDivider(listBox1));

            foreach (var i in Lines)
            {
                foreach (var j in Utility.WrapStringInListBox(listBox1, i))
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

        public void WhatUnlockedThis()
        {
            foreach (var i in Playthrough) { listBox1.Items.Add(i); }
        }

        public void ResizeObject()
        {
            Debugging.Log(this.Text);
            if (this.Text == "Playthrough")
            {
                listBox1.Height = this.Height - 60 - menuStrip1.Height;
                listBox1.Width = this.Width - 40;
            }
            else
            {
                listBox1.Location = new Point(LBX, LBY - menuStrip1.Height);
                menuStrip1.Visible = false;
                listBox1.Height = this.Height - 60;
                listBox1.Width = this.Width - 40;
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Text == "Playthrough") 
            {
                var text = new List<string>();
                SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Playthrough (*.txt)|*.txt", FilterIndex = 1 };
                if (saveDialog.ShowDialog() != DialogResult.OK) { return; }
                foreach(string i in listBox1.Items)
                {
                    text.Add(i);
                }
                File.WriteAllLines(saveDialog.FileName, text);
            }
        }

        public void ShowIPHelp()
        {
            this.Text = "Online Play Help";
            listBox1.Items.Clear();
            List<string> Lines = new List<string>
            {
                Utility.CreateDivider(listBox1, "Port Forwarding"),
                "Here I will do my best to outline the basic information you will need to setup online play. You will however still be expected to have at least a basic understanding of the terms and concepts detailed below.",
                "",
                "In order for your tracker to recieve data, you must have the port you selected under \"Your port\" forwarded to your PC in your router.",
                "For information on how to accomplish this visit this link https://www.noip.com/support/knowledgebase/general-port-forwarding-guide/.",
                "",
                "If your router supports UPNP, other external programs can be used acomplish this.",
                "We recomend https://github.com/kaklakariada/portmapper as it was used to test this application.",
                "",
                Utility.CreateDivider(listBox1, "Local Play"),
                "In order to play with people on you local network, you will need to use their local ip. This is different than the IP listed in \"Your IP\". Information on how to find this can be found here\"https://kb.netgear.com/20878/Finding-your-IP-address-without-using-the-command-prompt\".",
                "",
                "If you want to add an external player to your group, you will need everyone on your local network to assign a different port to their tracker. Each of these ports will then need to be forwarded to each corresponding computer. The external player can then use your public IP address with the corresponding port for each person.",
                "",
                Utility.CreateDivider(listBox1, "Adding IP Addresses"),
                "You will need to add the Public IP address of everyone you want to send data to. Their public IP address will be listed in the \"Your IP\" Field of the Online play form.",
                "",
                "They will need to provide you this address along with the port they have selected in \"Your port\". You will enter this data in the \"Ipaddress\" and \"Port\" field and click \"Add IP\"",
                "",
                "By default you will be able to recieve data from any IP address that has you in their send list. You can select the \"Only accept data from sending list\" option to only allow data from those in your send list.",
                "",
                Utility.CreateDivider(listBox1, "Sending and Recieving data"),
                "Whenever you update an entry in your tracker by checking or marking it, you tracker information will be sent to everyone in your send list as long as they have \"Listen for data\" selected and you have \"Send data\" selected",
                "",
                "By only selecting listen for data, you will recieve updates from others, but will not send updates of your own.",
                "By only selecting Send, you will send updates to others, but will not recieve updates from anyone.",
                "",
                Utility.CreateDivider(listBox1, "Other options"),
                "By default, net data will mirror the state (Checked or marked) of the data you receive. If you only want net data to mark entries regardless of if they were full checked in the net data, you can select \"Disallow Full Check\" in options. This is a recommended setting for co-op runs since you don't actually obtain the item the other player receives, but want to keep track of it. Net data will never be able to uncheck or unmark an item regardless of any option, this is by design.",
                "",
                "Selecting \"Auto Add Incoming IPs\" will add any IP you receive data from to your sending list if it's not there already. This option is useless if used in conjunction with \"Only accept data from sending list\", which is pretty self explanatory. This setting could potentially be dangerous as it could allow an unknown third party to add themselves to your group. It is recommended that you only enable this during setup.",
                "",
                Utility.CreateDivider(listBox1, "DISCLAIMER!"),
                "This application does not send any information to any device other those you select. As always you can check the source code or email the developers with any questions or concerns. Links to this information can be found in the about page",
                "(Click here to open the about page)"
            };
            foreach (var i in Lines)
            {
                foreach (var j in Utility.WrapStringInListBox(listBox1, i))
                {
                    listBox1.Items.Add(j);
                }
            }
        }
    }
}
