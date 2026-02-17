# Core Systems

This folder contains foundational managers and utilities used throughout the game.

## What's Here

### ✓ Completed
- `GameManager.cs` - Game state, cash management, bankruptcy detection
- `EventBus.cs` - Central event communication system
- `SaveManager.cs` - Save/load functionality (Phase 1 placeholder)

### Potential Future Additions
- `AudioManager.cs` - Sound and music management
- `InputManager.cs` - Input handling and keyboard shortcuts
- `ConfigManager.cs` - Game configuration and settings
- `NotificationManager.cs` - Toast notification system
- `DebugConsole.cs` - In-game debug console

## Key Principles

### EventBus Pattern
All systems communicate via EventBus:
```csharp
// Publish event
EventBus.Publish(new OnCashChangedEvent { NewCash = 50000f });

// Subscribe to event
EventBus.Subscribe<OnCashChangedEvent>(HandleCashChanged);

// Unsubscribe
EventBus.Unsubscribe<OnCashChangedEvent>(HandleCashChanged);
```

### GameManager Responsibilities
- Central game state tracking
- Cash management
- Game flow (new game, game over, etc.)
- Coordinating system initialization

### SaveManager Pattern
Phase 1: Placeholder implementation
Phase 2+: Full save/load with JSON serialization

## Documentation

See [Architecture](/Pages/Architecture.md) for detailed system architecture.

## Status

- [x] Core managers implemented
- [x] EventBus tested and working
- [ ] Additional utilities as needed
