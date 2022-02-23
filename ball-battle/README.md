# Ball Battle

This is a small Unity Project built by me :D

## Technical Information
- Unity 2019.4.13f1
- AR Foundation 3.1.3
- ARCore XR Plugin 3.1.3
- ARKit XR Plugin 3.1.3
- TextMeshPro 2.1.1
- Universal RP 7.3.1

## Scripts structure
- [ARManager.cs](Assets/Scripts/ARManager.cs): All AR-related functions
- [Ball.cs](Assets/Scripts/Ball.cs): Well, to *move* the ball
- [DetectionArea.cs](Assets/Scripts/DetectionArea.cs): Control how the Detection Area (of Defender Soldiers) works
- [EnergyPoint.cs](Assets/Scripts/EnergyPoint.cs): Supplemental function for setting Energy value (This could be done in GameUICtrl, but I prefer a separated file)
- [GameManager.cs](Assets/Scripts/GameManager.cs): The main Game Manager, in charge for handling most game logics
- [GameUICtrl.cs](Assets/Scripts/GameUICtrl.cs): Game UI controller, this and GameManager works together, one for game logics, one for sending those logics to UI
- [Globals.cs](Assets/Scripts/Globals.cs): ScriptableObject used to store global variables that need to be consistence between scenes
- [MainMenu.cs](Assets/Scripts/MainMenu.cs): handle what appears on the main menu
- [OnscreenDebeugOutput.cs](Assets/Scripts/OnscreenDebeugOutput.cs): Custom function for debugging AR (as I stated in the next section)
- [Params.cs](Assets/Scripts/Params.cs): Parameters to balance the game
- [PenaltyCtrl.cs](Assets/Scripts/PenaltyCtrl.cs): For Penalty scene
- [Soldier.cs](Assets/Scripts/Soldier.cs): Soldier-related functions (including some setters and collision checking)
- [SoldierAtk.cs](Assets/Scripts/SoldierAtk.cs): for Soldiers that are Attackers
- [SoldierDef.cs](Assets/Scripts/SoldierDef.cs): for Soldiers that are Defenders

## Random thoughts

### How do I feel about this project
This is the first time I've made an AR-ready project. Some AR challenges include:
- Implement AR core elements
- Positioning, transforming & scaling objects
- Make sure everything is consistent between AR-ready and non-AR gameplay
- ...

### Estimated time for each section
Total: approx. 7 days, including
- Core gameplay: approx. 2 days
- UI: approx. 1 day
- AR: approx. 2+ days
- Building, Testing, etc: approx. 2 days

Of course I don't do those separately. i.e I tweak UI while waiting for AR file to be built. Why? Read the next part pls

### The most challenging problem
I don't have an Android phone (luckily my iPhone supports AR). Unity Editor & Unity Remote doesn't support debugging with AR enabled. Android Studio does (with Virtual camera enabled for Android Virtual Device - or AVD), but the Emulator itself doesn't work with Unity APK builds (AR supports x86 emulators, while Unity runs with arm64 ._.). I tried Nox, but it doesn't work either.

In the end, I had to change the platform of this project to iOS, export xCode project, use a VM to build it and install to my phone.
I also used [Runtime Inspector & Hierachy](https://github.com/yasirkula/UnityRuntimeInspector) to *debug* AR and write some codes to *print to screen*.
The whole process (from exporting to installing) costs ~ 15 mins total (u can say it's a huge pain everytime trying to modify something in the code and wait for its result). To save time while waiting for the VM to build, I modify UI, Gameplay, etc.

### What could be improved
- Better UI (I'm not good at designing tho)
- Some extensive AR functions. i.e. using two fingers to scale the field, swipe to rotate it, etc.
- Better character animation
- Code cleaning & optimizing

### About the maze
I came up with an algorithm to create the maze.
Let the field be an array of m (rows) x n (cols).

Move randomly between cells and call that *1st-path*. Mark visited cells. If we can't move further, go back and continue with *2nd-path*, etc. Save visited cells into orders. The array after finishing traversal is something like

```python
# Incomplete. All zeroes should be filled
[0, 0, 1, 2, 2]
[0, 1, 1, 2, 0]
[0, 1, 2, 2, 0]
[1, 1, 2, 0, 0]
[1, 1, 1, 1, 0]
[1, 1, 1, 1, 0]
[0, 0, 0, 0, 0]
[0, 0, 0, 0, 0]
[0, 0, 0, 0, 0]
[0, 0, 0, 0, 0]
```

And the *paths* are something like (describe in python dict)

```python
{
    1: [(2, 0), (2, 1), (2, 2), ..., (2, 4), (1, 4)],
    2: [(2, 3), (2, 2), ...],
    ...
}
```

After that, we will create walls base on those.
