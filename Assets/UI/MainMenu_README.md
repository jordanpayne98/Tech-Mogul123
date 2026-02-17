# Main Menu System

## Overview

The Main Menu is the first screen players see when launching Tech Mogul. It provides options to start a new game, load a saved game, access settings, and quit.

## Structure

```
MainMenu Scene (/Assets/Scenes/MainMenu.unity)
├── Main Camera (Camera component)
└── MainMenuUI (UIDocument + MainMenuController)
```

## Files

- **Scene**: `/Assets/Scenes/MainMenu.unity`
- **UXML**: `/Assets/UI/UIToolkit/UIDocuments/MainMenu.uxml`
- **USS**: `/Assets/UI/UIToolkit/Styles/MainMenu.uss`
- **Controller**: `/Assets/UI/UIToolkit/MainMenuController.cs`
- **Background Image**: `/Assets/UI/Images/MainMenuBackground.png` (user-provided)

## Setup Instructions

### 1. Add Background Image

1. Save your background image to `/Assets/UI/Images/MainMenuBackground.png`
2. Select the image in the Project window
3. In the Inspector, ensure Texture Type is set to "Sprite (2D and UI)"
4. Click Apply

### 2. Configure Build Settings

1. Open File → Build Settings
2. Click "Add Open Scenes" to add MainMenu scene
3. Drag MainMenu scene to the top of the Scenes In Build list (index 0)
4. Ensure SampleScene is below it (index 1)
5. Close Build Settings

The game will now launch with the Main Menu first.

## Features

### New Game Button
- Publishes `RequestStartNewGameEvent` 
- Loads the SampleScene (main game scene)
- GameManager initializes game state with starting cash

### Load Game Button
- Publishes `RequestShowSaveLoadDialogEvent` with `IsLoading = true`
- Opens the Save/Load dialog in load mode
- Currently requires Save/Load dialog implementation

### Settings Button
- Placeholder for future settings menu
- Logs click event for now

### Quit Button
- In Editor: Stops play mode
- In Build: Quits application

## Customization

### Title Text
Edit in UXML: `MainMenu.uxml`
- Title: `<ui:Label text="TECH MOGUL" name="title" />`
- Subtitle: `<ui:Label text="Build Your Software Empire" name="subtitle" />`

### Button Labels
Edit button text attributes in UXML:
- `<ui:Button text="New Game" name="new-game-button" />`

### Styling
Edit USS file: `/Assets/UI/UIToolkit/Styles/MainMenu.uss`
- `.game-title`: Title styling (font size, color, shadow)
- `.menu-button`: Button styling (size, colors, hover effects)
- `.menu-button--primary`: Primary button styling (New Game)

### Background
Replace image at `/Assets/UI/Images/MainMenuBackground.png` or update the USS:

```css
.main-menu-background {
    background-image: url('project://database/Assets/UI/Images/YourImage.png');
}
```

## Event Flow

### Starting New Game

1. Player clicks "New Game" button
2. `MainMenuController.OnNewGameClicked()` fires
3. Publishes `RequestStartNewGameEvent`
4. Loads SampleScene
5. GameManager receives event in new scene
6. `GameManager.HandleStartNewGame()` processes
7. `GameManager.StartNewGame()` initializes state
8. Publishes `OnGameStartedEvent`
9. Game systems initialize

### Loading Game

1. Player clicks "Load Game" button
2. `MainMenuController.OnLoadGameClicked()` fires
3. Publishes `RequestShowSaveLoadDialogEvent { IsLoading = true }`
4. Save/Load dialog controller receives event and displays
5. Player selects save file
6. SaveSystem loads game data
7. Scene loads with restored state

## Technical Notes

- Uses Unity UI Toolkit (not Unity UI/Canvas)
- Controller inherits from MonoBehaviour
- All buttons use `RegisterCallback<ClickEvent>` pattern
- Callbacks unregistered in OnDisable() to prevent memory leaks
- Background overlay provides semi-transparent darkening (40% black)
- Game scene name configurable via `gameSceneName` serialized field

## Scene Configuration

The MainMenu scene should be:
1. Set as index 0 in Build Settings
2. Contain only Main Camera and MainMenuUI GameObject
3. Main Camera uses orthographic projection for 2D
4. UIDocument references PanelSettings asset at `/Assets/UI/UIToolkit/PanelSettings.asset`
