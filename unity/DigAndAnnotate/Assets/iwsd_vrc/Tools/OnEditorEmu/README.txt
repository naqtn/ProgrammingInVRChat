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


## Note


### Reproduce defect: OnDestroy trriger doesn't work

Currently (version 2018.3.3), original client doesn't run OnDestroy at all.
So this emulator omit this feature.

### OnDestroy trigger cause NullReferenceException

If using OnDestroy trigger, you'll see following error message on Unity Console window.
Please ignore this.

    NullReferenceException: Object reference not set to an instance of an object
    VRCSDK2.VRC_Trigger.ExecuteTriggerType (TriggerType triggerType)
    VRCSDK2.VRC_Trigger.OnDestroy ()

<!--
Original VRC_Trigger removes itself at runtime.
It is not initialized properly in Unity editor environment.
That is reason of this error, maybe.
-->


## Licence etc.

MIT licence
by naqtn (https://twitter.com/naqtn)

Hosted at https://github.com/naqtn/ProgrammingInVRChat

---
end
