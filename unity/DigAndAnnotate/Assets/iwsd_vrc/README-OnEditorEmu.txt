# OnEditorEmu: On Unity Editor VRChat Client Emulator


## What's this?

This is a VRChat client emulation tool working on Unity editor environment.
It's aim is improving debugging process of VRChat world.
You can debug VRChat worlds with Unity editor functionality.


For example, to make complex gimmicks in a VRChat world, you may use Unity animation
as timeline control and state machine and use Animation Events to trigger VRChat components.

In such case, testing combination of Unity animations and VRChat components can not be done
on Unity editor because VRChat components don't work on Unity editor.
Checking animation internal state and trigger activation on VRChat client is stressful work.

With this emulator, you can check animation state on Unity editor as usual Unity application development.
(watching state change visually, inputting parameters manually, etc.)



## CAUTION (Backup your project!)

This software is now at early stage.
It might destroy your Unity project by implementation mistake.
Don't use with serious projects withdout backup.


## How to use

* Import released Unity package to your Unity project where you create VRChat world.
    * (Or copy files to your project Assets folder if you want use non-released latest version from GitHub)
* Select Unity menu Window > VRC_Iwsd > Emulator to open "Emulator Setting" window
    * mark "Enabled" checkbox.
    * Press Unity Editor play button to start emulation.
    * (There's shortcut button "Set enable and start")
* You can control player while Game window has focus.
    * WASD keys to move
    * mouse movement to turn and look up and down.
    * Space key to jump
* You can interact with a object by centering it and press left mouse button.
* Q key to open quick menu
    * (ESC also open quick menu like original VRChat client.
      But in Unity editor, ESC also activates mouse cursor automatically. You can avoid this behavior by using Q key.)
    * You can respawn player and Quit emulator via quick menu
* TAB key to toggle mouse cursor
    * While mouse cursor appears, you can operate Unity editor ordinally in play mode.
* You can also start and stop emulator by Unity play button.
* You must diable "Enabled" checkbox in "Emulator Setting" befor publish your world using "Build Control Panel" of VRChat SDK.
    * (Or publish button starts this emulator in inconsistent way)
    * (This happens because both SDK publishing process and this emulator are implemented as "game" in Unity)


## Development Status Digest

* Currently only for single player
    * This means it doesn't care broadcast type at all.
* trigers and actions implementation status :
    * almost all actions are implemented.
    * With the exception of Relay and VRC component dependent trigger (OnVideoStart etc.), almost all Triggers are implemented.
    * (See Features.txt (and/or Emu_Trigger.cs) for details.)
* ExecuteCustomTrigger function of VRC_Trigger component works with Unity animation trigger.
* Currently and basically emulate VRC_Trigger commponent only.
    * (But some VRC components seems work without tewaks. For example, VRC_AudioBank seems working via RPC)


## Benefit, Goal, Limitation etc.

* Make it more easy to debug when creating VRChat world
* Less turn around time to be able to start test/debug (No build time)
* Enable using tools available in Unity editor enviroment to improve (or optimize) your world
* Currently, only emulate desktop client. VR priority is rather low.


## Notes

### OnDestroy trigger cause NullReferenceException

If using OnDestroy trigger, you'll see following error message on Unity Console window.
Please ignore this.

    NullReferenceException: Object reference not set to an instance of an object
    VRCSDK2.VRC_Trigger.ExecuteTriggerType (TriggerType triggerType)
    VRCSDK2.VRC_Trigger.OnDestroy ()

<!--
Original VRC_Trigger removes itself at runtime.
But in Unity editor environment, it's not initialized properly.
That is reason of this error, maybe.
-->


## Licence etc.

MIT licence
by naqtn (https://twitter.com/naqtn)

Hosted at https://github.com/naqtn/ProgrammingInVRChat

If you have defect reports or feature requests, please post to GitHub issue (https://github.com/naqtn/ProgrammingInVRChat/issues)

---
end
