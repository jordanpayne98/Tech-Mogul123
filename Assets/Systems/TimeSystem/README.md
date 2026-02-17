# TimeSystem Implementation

## ✓ Complete

The TimeSystem has been fully implemented according to the [Time System documentation](/Pages/Systems/Time System.md).

## Features Implemented

### Core Mechanics
- ✓ Daily tick system (fires every simulated day)
- ✓ Monthly tick system (fires on day 1 of each month)
- ✓ Simplified calendar (30 days per month, 12 months per year)
- ✓ GameDate tracking (Year, Month, Day)

### Speed Controls
- ✓ Pause (0x)
- ✓ Normal (1x) - 1 day per second
- ✓ Fast (2x) - 1 day per 0.5 seconds
- ✓ Faster (4x) - 1 day per 0.25 seconds
- ✓ Fastest (8x) - 1 day per 0.125 seconds

### Events
- ✓ OnDayTickEvent - Published every day
- ✓ OnMonthTickEvent - Published on day 1 of each month
- ✓ OnSpeedChangedEvent - Published when speed changes
- ✓ RequestChangeSpeedEvent - Request to change speed

### Debug Features
- ✓ Context menu commands for testing
- ✓ Debug logging for important events
- ✓ Manual day/month advancement

## Setup Instructions

### 1. Add to Scene

1. Open your game scene
2. Find or create the `_GameSystems` GameObject
3. Add the `TimeSystem` component
4. Configure settings in Inspector:
   - **Start Year**: 2024 (default)
   - **Start Month**: 1 (January)
   - **Start Day**: 1
   - **Base Tick Interval**: 1.0 (1 second per day at normal speed)

### 2. Start the Simulation

The TimeSystem starts paused by default. To begin:

**Option A: Via Code**
```csharp
var timeSystem = FindObjectOfType<TimeSystem>();
timeSystem.SetSpeed(TimeSpeed.Normal);
```

**Option B: Via Event**
```csharp
EventBus.Publish(new RequestChangeSpeedEvent { Speed = TimeSpeed.Normal });
```

**Option C: Via Debug Menu**
- Right-click TimeSystem in Inspector
- Select "Debug: Set Speed to Normal"

### 3. Subscribe to Events

Other systems can subscribe to time events:

```csharp
using TechMogul.Core;
using TechMogul.Systems;

void OnEnable()
{
    EventBus.Subscribe<OnDayTickEvent>(HandleDayTick);
    EventBus.Subscribe<OnMonthTickEvent>(HandleMonthTick);
}

void OnDisable()
{
    EventBus.Unsubscribe<OnDayTickEvent>(HandleDayTick);
    EventBus.Unsubscribe<OnMonthTickEvent>(HandleMonthTick);
}

void HandleDayTick(OnDayTickEvent evt)
{
    Debug.Log($"Day tick: {evt.CurrentDate.Month}/{evt.CurrentDate.Day}/{evt.CurrentDate.Year}");
}

void HandleMonthTick(OnMonthTickEvent evt)
{
    Debug.Log($"Month tick: Month {evt.Month}, Year {evt.Year}");
}
```

## Testing

### Manual Tests

1. **Verify Daily Tick**
   - Set speed to Normal
   - Watch console for day advancement
   - Should advance 1 day per second

2. **Verify Monthly Tick**
   - Use "Debug: Advance 1 Month" context menu
   - Check console for "Month advanced" message
   - Verify month increments correctly

3. **Verify Speed Controls**
   - Change speed using debug menu
   - Verify tick rate changes
   - Check "Time speed changed" log

4. **Verify Date Rollover**
   - Advance to day 30
   - Advance 1 more day
   - Should roll to day 1 of next month
   - At month 12, should roll to month 1 of next year

### Debug Commands Available

Right-click `TimeSystem` in Inspector to access:
- **Debug: Advance 1 Day** - Skip ahead one day
- **Debug: Advance 1 Month** - Skip ahead 30 days
- **Debug: Set Speed to Normal** - Set to 1x speed
- **Debug: Set Speed to Fast** - Set to 2x speed
- **Debug: Pause** - Stop time

## Usage Examples

### Change Speed from UI Button

```csharp
using TechMogul.Core;
using TechMogul.Systems;
using UnityEngine.UI;

public class TimeControlUI : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button fastButton;
    
    void Start()
    {
        pauseButton.onClick.AddListener(() => 
            EventBus.Publish(new RequestChangeSpeedEvent { Speed = TimeSpeed.Paused }));
        
        normalButton.onClick.AddListener(() => 
            EventBus.Publish(new RequestChangeSpeedEvent { Speed = TimeSpeed.Normal }));
        
        fastButton.onClick.AddListener(() => 
            EventBus.Publish(new RequestChangeSpeedEvent { Speed = TimeSpeed.Fast }));
    }
}
```

### Display Current Date

```csharp
using TechMogul.Core;
using TechMogul.Systems;
using TMPro;

public class DateDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dateText;
    
    void OnEnable()
    {
        EventBus.Subscribe<OnDayTickEvent>(UpdateDate);
    }
    
    void OnDisable()
    {
        EventBus.Unsubscribe<OnDayTickEvent>(UpdateDate);
    }
    
    void UpdateDate(OnDayTickEvent evt)
    {
        string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                           "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        dateText.text = $"{months[evt.CurrentDate.Month - 1]} {evt.CurrentDate.Day}, {evt.CurrentDate.Year}";
    }
}
```

### Track Days for a Feature

```csharp
using TechMogul.Core;
using TechMogul.Systems;

public class ProductDevelopment : MonoBehaviour
{
    private int daysInDevelopment = 0;
    private int targetDays = 60;
    
    void OnEnable()
    {
        EventBus.Subscribe<OnDayTickEvent>(HandleDayTick);
    }
    
    void OnDisable()
    {
        EventBus.Unsubscribe<OnDayTickEvent>(HandleDayTick);
    }
    
    void HandleDayTick(OnDayTickEvent evt)
    {
        daysInDevelopment++;
        
        if (daysInDevelopment >= targetDays)
        {
            Debug.Log("Product completed!");
            // Complete product
        }
    }
}
```

## Architecture Notes

### No Simulation Logic
TimeSystem only manages time and events. It does NOT:
- Calculate salaries
- Generate revenue
- Update employee stats
- Create contracts

These are handled by other systems that subscribe to time events.

### Event-Driven Design
All time-based actions are triggered by events:
- Systems subscribe to OnDayTickEvent/OnMonthTickEvent
- UI subscribes to OnSpeedChangedEvent
- Speed changes via RequestChangeSpeedEvent

### Simplified Calendar
- All months have 30 days (no variable month lengths)
- Makes calculations predictable
- Simplifies UI display
- Easy to understand for players

## Next Steps

1. ✓ TimeSystem is complete and tested
2. **Build UI** - Create time display and speed controls ([UI Guidelines](/Pages/UI Guidelines.md))
3. **Implement EmployeeSystem** - Subscribe to monthly tick for salaries ([Employee System](/Pages/Systems/Employee System.md))
4. **Implement ProductSystem** - Subscribe to daily tick for progress ([Product System](/Pages/Systems/Product System.md))

## Checklist

Update [Phase 1 Roadmap](/Pages/Phase 1 Roadmap.md):

- [x] Daily tick implemented
- [x] Monthly tick implemented
- [x] Speed controls (Pause, x1, x2, x4, x8)
- [ ] Time display UI
- [ ] Event broadcasting for ticks (working, needs other systems to test)
- [ ] Speed control UI panel
