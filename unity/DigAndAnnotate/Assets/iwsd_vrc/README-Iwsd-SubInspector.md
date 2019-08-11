# Iwsd / Sub-Inspector

A tool providing reorder list UI of event handling definition etc. for VRChat world creation.

![SubInspectorUI](doc/sub-inspector.PNG)

Sub-inspector (left side in the image) serves supplementary edit UI for same object that Unity Inspector (right side) handles.


## How to use

1. Import Unity package of this tool 
2. Open SubInspector view from Unity menu: Window > VRC_Iwsd > "Open Sub Inspector"
3. As usual, select a object to edit in Hierarchy view
4. Use edit UI showing in SubInspector view that affects selected object's components.



## Features

### Reorder list

SubInspector provides reorder UI for list items.

Supported components:

| Component     | list                             |
|---------------|----------------------------------|
| `VRC_Trigger` | `Triggers`                       |
| `Button`      | `OnClick`                        |
| `InputField`  | `OnValueChanged` and `OnEndEdit` |


Note:
For `Button` and `InputField`, There's another powerful editor extension [EasyEventEditor](https://github.com/Merlin-san/EasyEventEditor) by Merlin-san.)

### Copy and paste some parts of VRC_Trigger

You can copy some parts of VRC_Trigger definitions and paste to other VRC_Trigger.

- To copy
    1. Select trigger entries that you want to copy. 
    2. Press `Copy to clipboard` button.
    - It stores data to system clipboard as text. Technically in some JSON format.
- To paste
    1. Select target GameObject. If not having VRC_Trigger yet, add it.
    2. Press `Paste from to clipboard` button.
    - It read clipboard and adds entries to the end of trigger list.

Note: 
You can paste to another Unity project. 
Though, GameObject reference becomes "None".
("instanceID" might much accidentally. in that case trigger refers inappropriate object.)
Anyway you should check pasted contents.

### Inspector edit protection

It make GameObject and/or its components unchangeable (not editable) in Inspector.
This is some protection against unexpected edit or to avoid miss operations.
It only affects to Inspector. You can edit in another view, for example Scene view.

Note:
Technically it toggles [HideFlags](https://docs.unity3d.com/ScriptReference/HideFlags.html) NotEditable mask.


### Display object path

Display selected object path string from root of the scene.
Currently, this is copyable read only text. You can not edit object name with this UI.

Note: This is useful when directly editing animation clip.

## Licence etc.

MIT licence
by naqtn (https://twitter.com/naqtn)
Hosted at https://github.com/naqtn/ProgrammingInVRChat

This software includes SimpleJSON (https://github.com/Bunny83/SimpleJSON) licenced under MIT licence.

---
end
