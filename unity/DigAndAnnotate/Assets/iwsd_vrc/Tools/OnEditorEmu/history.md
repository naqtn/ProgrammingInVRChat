
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
- Fix and improve player collider
    - Add player collider selection feature to quick menu
        - you can select from (1) LocalPlayer layer (default), (2) Player layer to test as remote player, (3) None
    - Fix default active collider. In previous release Player layer is activated by mistake.
    - Fix Emu_Player prefab PlayerLayer collider IsTrigger to be true
- Add Interact() method support.
    - It enables OnInteract trigger from Animation Event and uGUI Event.
- Support reference camera of VRC_SceneDescriptor
    - Including post processing version 2
