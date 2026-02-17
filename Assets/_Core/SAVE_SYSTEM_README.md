# Multiple Save Slots System

## Summary

Implemented a full-featured save/load system with **3 save slots**, complete with save/load dialogs, file management, and save metadata display.

## Key Features

✅ **3 Independent Save Slots** - Multiple saves per game
✅ **Save/Load Dialog UI** - Visual slot selection  
✅ **Custom Save Names** - Name your saves
✅ **Save Metadata Display** - See cash, date, employees, reputation
✅ **Overwrite Support** - Replace existing saves
✅ **Delete Functionality** - Remove old saves
✅ **Empty Slot Detection** - Know which slots are free
✅ **Timestamp Tracking** - When each save was made

## How to Use

### Saving a Game
1. Press ESC (or click Save button)
2. Click "Save Game"
3. Enter save name (optional)
4. Click slot to save to
5. Done!

### Loading a Game
1. Press ESC (or click Load button)
2. Click "Load Game"
3. See all saves with info
4. Click "Load Game" on desired save
5. Done!

### Deleting a Save
1. Open save or load dialog
2. Click "Delete" button on any save
3. Confirmed!

## Save Location

Saves stored in: `Application.persistentDataPath/Saves/`
- Windows: `%USERPROFILE%\AppData\LocalLow\[Company]\[Game]\Saves\`
- macOS: `~/Library/Application Support/[Company]/[Game]/Saves/`
- Linux: `~/.config/unity3d/[Company]/[Game]/Saves/`

Files: `save_0.json`, `save_1.json`, `save_2.json`

## What's Saved

Each save file contains:
- Save name (custom)
- Save date/time
- Current cash
- Game date (year/month/day)
- All employees
- All products
- All contracts
- Current reputation
- Game state

## Configuration

Default: **3 slots**

To change:
- Select `SaveLoadDialog` GameObject
- Set `maxSaveSlots` to desired number

## Files Created

- `/Assets/UI/UIToolkit/SaveLoadDialogController.cs`
- `/Assets/UI/UIToolkit/UIDocuments/SaveLoadDialog.uxml`
- `/Assets/UI/UIToolkit/Styles/SaveLoadDialog.uss`
- Scene object: `SaveLoadDialog`

## Manual Setup Required

**Link USS file**:
1. Open `SaveLoadDialog.uxml` in UI Builder
2. Select root element
3. Add stylesheet: `SaveLoadDialog.uss`
4. Save

## Testing

1. Start game
2. Press ESC → Save Game
3. Save to all 3 slots with different names
4. Press ESC → Load Game
5. See all saves listed
6. Load different saves
7. Try deleting saves

## Debug Commands (SaveManager)

- Debug: Show Save Directory
- Debug: List All Saves
- Debug: Delete All Saves

Enjoy your new multi-slot save system! 🎮💾
