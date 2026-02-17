# Save/Load System Implementation

## Overview

The save/load system is fully functional and integrated with all game systems. It uses JSON serialization to save game state to disk.

## Architecture

### Core Components

**SaveManager** (`/Assets/_Core/SaveManager.cs`)
- Singleton manager handling save/load operations
- Saves to `Application.persistentDataPath/savegame.json`
- Uses Unity's `JsonUtility` for serialization
- Publishes events on save/load completion

**SaveData** (`/Assets/_Core/SaveData.cs`)
- Contains all serializable game state classes
- Converts runtime data to/from JSON-compatible format
- Handles ScriptableObject references via asset paths

### Data Structure

```
SaveData
├── GameManagerData (cash, running state)
├── TimeSystemData (date, speed)
├── EmployeeSystemData (employee list, counter)
├── ProductSystemData (product list)
├── ContractSystemData (contract list)
└── ReputationSystemData (current reputation)
```

## Event Flow

### Save Game
1. UI/User triggers `RequestSaveGameEvent`
2. SaveManager gathers data from all systems
3. Converts to serializable format
4. Writes JSON to disk
5. Publishes `OnGameSavedEvent` with success status

### Load Game
1. UI/User triggers `RequestLoadGameEvent`
2. SaveManager reads JSON from disk
3. Publishes system-specific load events:
   - `RequestSetCashEvent` → GameManager
   - `RequestSetDateEvent` → TimeSystem
   - `RequestLoadEmployeesEvent` → EmployeeSystem
   - `RequestLoadProductsEvent` → ProductSystem
   - `RequestLoadContractsEvent` → ContractSystem
   - `RequestSetReputationEvent` → ReputationSystem
4. Systems restore their state
5. Publishes `OnGameLoadedEvent` with success status

### New Game
1. UI/User triggers `RequestStartNewGameEvent`
2. GameManager resets cash and publishes `OnGameStartedEvent`
3. All systems subscribe to `OnGameStartedEvent` and reset:
   - TimeSystem resets to starting date
   - EmployeeSystem clears roster
   - ProductSystem clears products
   - ContractSystem clears and generates initial contracts
   - ReputationSystem resets to starting value
4. Game begins in normal state

## System Integration

Each system implements three event handlers:

```csharp
void HandleGameStarted(OnGameStartedEvent evt)
{
    // Reset system to initial state
    _data.Clear();
    _counter = 0;
}

void HandleLoadData(RequestLoadDataEvent evt)
{
    // Restore system state from serialized data
    _data.Clear();
    foreach (var item in evt.Items)
    {
        _data.Add(item.ToRuntimeData());
    }
}
```

### Implemented Systems

✅ **GameManager**
- Saves: current cash, running state
- Loads: restores cash via `RequestSetCashEvent`
- New Game: resets to starting cash

✅ **TimeSystem**
- Saves: current date (year, month, day), time speed
- Loads: restores date and speed
- New Game: resets to starting date (2024-01-01)

✅ **EmployeeSystem**
- Saves: all employee data including skills, morale, assignments
- Loads: recreates employee roster with full state
- New Game: clears all employees

✅ **ProductSystem**
- Saves: all products with progress, revenue, assignments
- Loads: recreates products with full state
- New Game: clears all products

✅ **ContractSystem**
- Saves: all contracts with progress, goals, assignments
- Loads: recreates contracts with full state
- New Game: clears and generates fresh contracts

✅ **ReputationSystem**
- Saves: current reputation value
- Loads: restores reputation
- New Game: resets to starting value (0)

## ScriptableObject Handling

ScriptableObjects (roles, templates, categories) are saved by asset path:

```csharp
// Save
string path = AssetDatabase.GetAssetPath(scriptableObject);

// Load (Editor)
var so = AssetDatabase.LoadAssetAtPath<T>(path);

// Load (Build)
var so = Resources.Load<T>(path);
```

**Note**: For runtime builds, ensure ScriptableObjects are in a Resources folder or use Addressables.

## Usage

### Via UI (when UI buttons exist)
Add these buttons to MainUI.uxml:
```xml
<Button name="new-game-btn" text="New Game" />
<Button name="save-game-btn" text="Save" />
<Button name="load-game-btn" text="Load" />
```

The `MainUIController` automatically binds these buttons if they exist.

### Via Code
```csharp
// Start new game
EventBus.Publish(new RequestStartNewGameEvent());

// Save game
EventBus.Publish(new RequestSaveGameEvent());

// Load game
EventBus.Publish(new RequestLoadGameEvent());
```

### Via Debug Menu (Editor Only)
1. Select `GameManager` GameObject in hierarchy
2. Right-click on `GameManager` component in Inspector
3. Choose from debug menu:
   - "Debug: Start New Game"
   - "Debug: Save Game"
   - "Debug: Load Game"
   - "Debug: Add $10,000"

SaveManager also has debug options:
   - "Debug: Show Save Path"
   - "Debug: Delete Save File"

## Save File Location

**Editor & Standalone Builds**:
- Windows: `%userprofile%\AppData\LocalLow\[CompanyName]\[ProductName]\savegame.json`
- macOS: `~/Library/Application Support/[CompanyName]/[ProductName]/savegame.json`
- Linux: `~/.config/unity3d/[CompanyName]/[ProductName]/savegame.json`

Use `Application.persistentDataPath` to find the exact location for your project.

## Save Data Structure Example

```json
{
  "saveVersion": 1,
  "saveName": "Auto Save",
  "saveTimestamp": "2024-01-15 14:30:22",
  "gameManager": {
    "currentCash": 45000,
    "isGameRunning": true
  },
  "timeSystem": {
    "year": 2024,
    "month": 3,
    "day": 15,
    "currentSpeed": 1
  },
  "employeeSystem": {
    "employees": [
      {
        "employeeId": "guid-1234",
        "employeeName": "Alex Smith",
        "roleAssetPath": "Assets/Data/Roles/DeveloperRole.asset",
        "devSkill": 65,
        "designSkill": 30,
        "marketingSkill": 20,
        "morale": 75,
        "burnout": 15,
        "monthlySalary": 3500,
        "currentAssignment": "Contract_5678",
        "isAvailable": false
      }
    ],
    "employeeCounter": 1
  }
}
```

## Events Reference

### Save/Load Events
```csharp
// Requests
RequestSaveGameEvent          // Trigger save operation
RequestLoadGameEvent          // Trigger load operation
RequestStartNewGameEvent      // Trigger new game

// Results
OnGameSavedEvent             // { Success: bool }
OnGameLoadedEvent            // { Success: bool }
OnGameStartedEvent           // Fired when new game starts

// Lifecycle
OnBeforeLoadGameEvent        // Before load begins
OnAfterLoadGameEvent         // After load completes

// System-specific load requests
RequestSetCashEvent          // { Amount: float }
RequestSetDateEvent          // { Year, Month, Day }
RequestLoadEmployeesEvent    // { Employees: List<SerializableEmployee> }
RequestLoadProductsEvent     // { Products: List<SerializableProduct> }
RequestLoadContractsEvent    // { Contracts: List<SerializableContract> }
RequestSetReputationEvent    // { Reputation: float }
```

## Testing Workflow

1. **Start a new game**:
   - GameManager → Debug: Start New Game
   - Verify all systems reset
   - Time starts, initial contracts generated

2. **Play for a while**:
   - Hire employees
   - Accept contracts
   - Complete work
   - Advance time

3. **Save the game**:
   - GameManager → Debug: Save Game
   - Check console for "Game saved successfully"
   - Verify save file exists

4. **Make changes**:
   - Hire more employees
   - Modify game state

5. **Load the save**:
   - GameManager → Debug: Load Game
   - Verify game state restored to save point
   - Check all systems have correct data

## Future Enhancements

### Phase 2+ Features
- Multiple save slots
- Auto-save on interval
- Save game naming and metadata
- Cloud save support
- Save file versioning/migration
- Compressed save files
- Encrypted save files

### Recommended Additions
- Save game UI dialog for naming saves
- Load game UI with save slot list
- "Are you sure?" confirmation dialogs
- Save/load progress indicators
- Corrupted save file handling

## Notes

- Save operations are synchronous (blocking)
- No validation of loaded data yet
- ScriptableObject references may break if assets move
- Employee skill history is preserved
- Contract goal states are preserved
- Product development progress is preserved

## Troubleshooting

**Save file not found**:
- Check `Application.persistentDataPath` location
- Verify write permissions
- Check console for errors

**Load fails**:
- Check JSON is valid
- Verify ScriptableObject asset paths exist
- Check save file version matches

**Data not restoring**:
- Verify system subscribes to load events
- Check event handlers are called
- Ensure proper event unsubscribe in OnDisable

**ScriptableObject references null**:
- Verify asset paths in save file
- Ensure assets haven't moved
- For builds, ensure assets in Resources folder
