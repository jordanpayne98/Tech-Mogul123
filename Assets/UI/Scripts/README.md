# UI Scripts

This folder contains all UI component scripts for the Tech Mogul UI system.

## ✅ Scripts Created

### Core Dashboard Components
- **CashDisplay.cs** - Displays current cash with color-coded feedback
- **DateDisplay.cs** - Shows current game date (Month Day, Year)
- **TimeControls.cs** - Time speed control buttons (Pause, 1x, 2x, 4x, 8x)
- **SpeedIndicator.cs** - Visual indicator showing current time speed

### System Management
- **PanelManager.cs** - Manages panel visibility and navigation
- **NotificationManager.cs** - Handles toast notifications for game events
- **NotificationToast.cs** - Individual notification display with animations

## Features Implemented

### Event-Driven Architecture
✅ All UI components use EventBus (no direct system references)
✅ Subscribe in OnEnable, unsubscribe in OnDisable
✅ Zero simulation logic in UI

### Visual Feedback
✅ Cash display color-codes based on amount (green/orange/red)
✅ Speed indicator shows current speed with color
✅ Time control buttons highlight active speed
✅ Animated cash value changes
✅ Notification system with fade in/out animations

### Panel Management
✅ Dashboard always visible
✅ Only one additional panel active at a time
✅ Button highlighting for active panel
✅ Toggle panels on/off

## Usage

### 1. Create Main Canvas

```
Hierarchy:
Canvas (Screen Space - Overlay)
├── DashboardPanel
│   ├── Header
│   │   ├── TitleText
│   │   ├── CashDisplay (add CashDisplay.cs)
│   │   └── DateDisplay (add DateDisplay.cs)
│   ├── TimeControls (add TimeControls.cs)
│   │   ├── PauseButton
│   │   ├── NormalButton
│   │   ├── FastButton
│   │   └── FasterButton
│   └── Navigation
│       ├── EmployeesButton
│       ├── ProductsButton
│       ├── ContractsButton
│       └── MarketButton
├── EmployeePanel (placeholder, inactive)
├── ProductPanel (placeholder, inactive)
├── ContractPanel (placeholder, inactive)
├── MarketPanel (placeholder, inactive)
├── NotificationContainer (add NotificationManager.cs)
└── PanelManager (add PanelManager.cs to Canvas root)
```

### 2. Setup Component References

**CashDisplay:**
- Assign TextMeshProUGUI reference
- Enable/disable animation as desired

**DateDisplay:**
- Assign TextMeshProUGUI reference
- Toggle showYear on/off

**TimeControls:**
- Assign all speed button references
- Configure colors and active scale

**PanelManager:**
- Assign all panel GameObjects
- Assign all navigation buttons
- Configure button colors

**NotificationManager:**
- Create notification toast prefab (simple panel with TextMeshProUGUI)
- Assign notification prefab
- Assign container Transform

### 3. Test in Play Mode

1. Add GameManager, TimeSystem to scene
2. Enter Play Mode
3. Start new game or set time speed
4. Verify UI updates automatically
5. Test panel navigation

## Events Subscribed To

### CashDisplay
- `OnCashChangedEvent` - Updates cash display

### DateDisplay
- `OnDayTickEvent` - Updates date display

### TimeControls & SpeedIndicator
- `OnSpeedChangedEvent` - Updates button highlights and speed text
- Publishes `RequestChangeSpeedEvent`

### NotificationManager
- `OnGameStartedEvent` - "Game Started"
- `OnBankruptcyEvent` - "Bankruptcy"
- `OnInsufficientCashEvent` - Cash warning
- `OnMonthTickEvent` - New month notification

## Customization

### Colors

Edit in scripts or expose in Inspector:
- Success: Green (#2ECC71)
- Warning: Orange (#F39C12)
- Error: Red (#E74C3C)
- Info: Blue (#3498DB)

### Animations

**CashDisplay:**
- `useAnimation` - Enable/disable smooth cash changes
- `animationDuration` - How long to animate (default: 0.5s)

**NotificationToast:**
- `fadeInDuration` - Fade in time (default: 0.3s)
- `fadeOutDuration` - Fade out time (default: 0.5s)

### Panel Behavior

**PanelManager:**
- Toggle same panel to close it
- Only one panel active at a time
- Dashboard always visible

## Next Steps

1. **Create Canvas in scene** - Follow hierarchy above
2. **Create notification toast prefab** - Simple panel with text
3. **Assign component references** - Connect UI elements in Inspector
4. **Test dashboard** - Verify cash, date, time controls work
5. **Build panel content** - Add employee, product, contract panels

## Documentation

- [UI Guidelines](/Pages/UI Guidelines.md) - Complete UI architecture
- [Architecture](/Pages/Architecture.md) - Event-driven design
- [Phase 1 Roadmap](/Pages/Phase 1 Roadmap.md) - Implementation checklist

All UI scripts follow the zero-simulation-logic principle and are fully event-driven! 🎨
