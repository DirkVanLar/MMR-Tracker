# This project is no longer being worked on

## The last version of MMR compatible with this tracker is version 1.13. The 600+ freestanding item added in 1.14 will not work at. The limited support for WWR, SSR and OOTR were already outdated and will most definitely not work, aside from maybe WWR. 
## This project may be revisited in the future when I find the time and motivation to redo it from the ground up. Much of this was a learning project and a large amount of the code from early on is poorly written and not feasibly maintainable for one person. If you would like to continue this project in the meantime feel free.

# MMR-Tracker
An Item, Location and Entrance Tracker for the Majoras Mask Randomizer

## Basic Functionality:
You will supply this tracker the logic file used to generate your seed and it will show you every location that is available to check based on the logic and items you have obtained.
Double clicking a location will ask you to select what item was found there, or you can import your html spoiler log to have this done automatically.
If you know what item a location contains, you can set the location to display what item is located there without actually checking it.
All checked locations will be displayed in a separate box displaying the location and item that was found there.

## Item Settings:
You can customize what locations are Randomized, unrandomized or junk. Unrandomized items will be automatically obtained when they are available and junk locations will be hidden from the tracker.
You can set items as starting items to have the tracker always consider them obtained.
These settings can be imported directly from the Majoras Mask Randomizer using either the settings.json or custom item strings.

## Entrance Randomizer Features:
When a version of the randomizer that includes entrance rando is used, the tracker will have a seperate box to keep track of available entrances.
When checking an entrance, the tracker will automatically mark the entrance in reverse assuming you have the option enabled.
You can use the pathfinder to show you available paths from one entrance to another.

## Online Play:
You can sync your tracker with your friends online to share progress between your trackers.
The tracker can be set to automatically mark items that others have found in coop seeds so you will know where that item is located.
The tracker can be set to automatically check items obtained by your friends in an online world to mark that you have recieved it as well.

## Extra Features: 
The tracker conatins a number of not exactly tracker related tools for use with the randomzier.

#### Seed Checker
You can use the Seed Checker to check if your seed can obtain a number of selected items based on your logic.
You can tell the seed checker to ignore certain checks you want to avoid.

#### Playthrough Generator
You can use the playthrough generator to create a guide that will tell you what items you need to obtain, where they are, and in what order you need to obtain them to beat the game. You can define what constitutes "Beating the game" by adding a fake item to your logic called MMRTGameClear and setting it to require what is neccesary to beat the game.

#### Visual Item Tracker
A visual display of what items you have obtained.

#### Map filter
An interactable map that will filter the tracker based on the area of the map you click on.

#### Advanced Logic Editor
The tracker is equipped a feature complete Logic Editor.
This logic editor features everything included with the editor found in the MM Randomizer plus a number of new features Including:

Undo/ Redo functionality.

The ability to go back to the last entry after using the "Go To" button.

Displaying item and location names along with the logic name.

A larger Conditionals window for viewing complex conditionals.

Copy/Pasting entries.

The ability to reorder fake items while auto updating values in other entries.

The ability to rename fake items.
