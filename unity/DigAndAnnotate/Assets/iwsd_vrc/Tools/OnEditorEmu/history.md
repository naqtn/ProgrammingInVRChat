
### v1.0 (2019-02-23)
- First release

### v1.1

- Change key to open quick menu
    - From ESC (or 'Q') to TAB
- Change mouse capture operation. 
    - ESC to escape from emulator. Click Game window to back to emulator.
    - (TAB is now for quick menu.)
- Add uGUI event support.
    - Replace VRC_Trigger reference to emulator Emu_Trigger at runtime.
    - Not supported when UI components are in newly spawned object.
- Add support delay of trigger execution
- Add player collider selection feature
    - Select one from LocalPlayer layer (default), Player layer, None at quick menu
- Add Interact() method support.
    - It enables OnInteract trigger from Animation Event and uGUI Event.
- Support reference camera of VRC_SceneDescriptor
    - including post processing version 2
    
