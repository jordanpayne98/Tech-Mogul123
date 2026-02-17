# ContractSystem

This folder contains contract management, goal tracking, and performance evaluation logic.

## What Goes Here

### Core Scripts
- `ContractSystem.cs` - Main system managing contracts
- `ContractData.cs` - Contract data structure (goals, deadlines, payouts, etc.)
- `ContractEvents.cs` - All contract-related events
- `ContractGoal.cs` - Goal definition and tracking

### UI Subfolder
- `UI/ContractPanel.cs` - Main contract panel (available + active)
- `UI/ContractCard.cs` - Individual contract display
- `UI/ContractDetailView.cs` - Detailed contract information
- `UI/CompletionReportDialog.cs` - Contract completion report

## Documentation

See [Contract System](/Pages/Systems/Contract System.md) for full implementation details.

## Dependencies

- TimeSystem (for daily progress and deadline tracking)
- EmployeeSystem (for employee productivity and effects)
- EventBus (for event communication)
- ContractTemplate ScriptableObjects (in `/Assets/Data/ContractTemplates/`)

## Status

- [ ] Not yet implemented
