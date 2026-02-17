# UI Prefabs

This folder contains reusable UI prefab assets for panels, dialogs, and components.

## What Goes Here

### Panel Prefabs
- `DashboardPanel.prefab` - Main UI header with cash, date, navigation
- `EmployeePanel.prefab` - Employee list and management panel
- `ProductPanel.prefab` - Product development and revenue panel
- `ContractPanel.prefab` - Contract browsing and tracking panel
- `MarketPanel.prefab` - Market overview and rival companies

### Dialog Prefabs
- `HireDialog.prefab` - Employee hiring dialog
- `NewProductDialog.prefab` - Start new product dialog
- `ContractDetailDialog.prefab` - Contract details dialog
- `CompletionReportDialog.prefab` - Contract completion report
- `ConfirmDialog.prefab` - Generic confirmation dialog

### Component Prefabs
- `EmployeeListItem.prefab` - Individual employee row in list
- `ProductCard.prefab` - Individual product display card
- `ContractCard.prefab` - Individual contract display card
- `RivalCard.prefab` - Individual rival company display
- `NotificationToast.prefab` - Notification popup

## Documentation

See [UI Guidelines](/Pages/UI Guidelines.md) for UI architecture and patterns.

## Key Principles

- **Zero Simulation Logic** - UI only displays data and sends requests
- **Event-Driven** - Subscribe to events, don't poll
- **Display Data Classes** - Use lightweight data classes for UI
- **UGUI Only** - All UI built with Unity's UGUI system

## Status

- [ ] Canvas setup not yet created
- [ ] Panel prefabs not yet created
- [ ] Dialog prefabs not yet created
- [ ] Component prefabs not yet created
