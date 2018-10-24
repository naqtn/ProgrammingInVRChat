# OnEditorEmu: On Unity Editor VRChat Client Emulator

## CAUTION (Backup your project!)

This project is now at early alpha stage.
It might destroy your Unity project by implementation mistake.
Don't use with serious project withdout backup.


## How to use

* Press Unity Editor play button to start emulation.
* You can control player with Game window.
    * WASD keys to move
    * Use mouse to turn and look up and down.
    * Space key to jump
* TAB key to toggle mouse cursor lock
* You can interact with a object by centering it and press left mouse button.


## Status

* Currently only for single player
    * Doesn't care broadcast type at all.
* Implemented trigers and actions
    * almost all actions are implemented.
    * Triggers are limited. (only Interact, Custom)
    * Detail: see Features.txt (and/or Emu_Trigger.cs)
* ExecuteCustomTrigger function of VRC_Trigger component works with Unity animation trigger.
* Emulate VRC_Trigger commponent only.


## Benefit, Goal, Limitation etc.

* Easy to debug when creating VRChat world
* Less turn around time (No build time)
* Enable using tools available in Unity editor enviroment to improve (or optimize) your world
* Only emulate desktop client. VR priority is rather low.


## Licence etc.

MIT licence
by naqtn (https://twitter.com/naqtn)

Hosted at https://github.com/naqtn/ProgrammingInVRChat

---
end
