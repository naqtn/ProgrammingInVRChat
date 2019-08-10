# Iwsd / Sub Inspector

A tool providing reorder list UI of event handling definition etc. for VRChat world creation.


## How to use

1. Import Unity package of this tool 
2. Open SubInspector view from Unity menu: Window > VRC_Iwsd > "Open Sub Inspector"
3. As usual, select a object to edit in Hierarchy view
4. Use edit UI showing in SubInspector view that affects selected object's components.

Drag and drop list item showing in SubInspector view to reorder

## Function

- Reorder list UI of :
    - VRC_Trigger  Triggers
    - Button  OnClick
    - InputField  OnValueChanged
- Display object path
    - Currently, this is copyable read only text. You can not edit object name with this UI.
- Copy and paste VRC_Trigger trigger definitions.
    - To copy, select entries and press "Copy to clipboard" button.
    - It stores data to system clipboard as text
    - To paste, press "Paste from to clipboard" button. It read clipboard and adds entries to the end of trigger list.
    - You can paste to another Unity project. Though, GameObject reference becomes "None". So you should check pasted contents.
    ("instanceID" might much accidentally. in that case trigger refers inappropriate object.)


## Licence etc.

MIT licence
by naqtn (https://twitter.com/naqtn)
Hosted at https://github.com/naqtn/ProgrammingInVRChat

This software includes SimpleJSON (https://github.com/Bunny83/SimpleJSON) licenced under MIT licence.

---
end
