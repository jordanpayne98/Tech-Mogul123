# Monthly Report System

## Overview

Automatically tracks monthly performance metrics and displays expandable notifications with detailed statistics at the end of each month.

## Features

### Monthly Metrics Tracked
- **Contracts Completed** - Number of successful contracts
- **Contracts Failed** - Number of failed contracts
- **Products Released** - Products launched during the month
- **Average Morale** - Company-wide employee morale average
- **Money Earned** - Total revenue generated
- **Money Spent** - Total expenses paid
- **Profit/Loss** - Net change in cash

### Expandable Notifications
- Monthly reports appear as notifications in the notification panel
- Click to expand and see detailed breakdown
- Color-coded values (green = positive, red = negative)
- Financial summary with profit/loss calculation

## How It Works

### Data Collection
The `MonthlyReportSystem` subscribes to events throughout the month:
- `OnContractCompletedEvent` - Tracks completed/failed contracts
- `OnProductReleasedEvent` - Counts products released
- `OnCashChangedEvent` - Tracks money earned and spent
- `OnDayTickEvent` - Counts days in month

### Month End Report
When `OnMonthTickEvent` fires:
1. Calculate average employee morale
2. Calculate profit (end cash - start cash)
3. Publish `OnMonthlyReportEvent` with all metrics
4. Reset counters for next month
5. Notification system creates expandable notification

### Notification Display
- **Collapsed**: Shows "January 2026 Report" with info icon
- **Expanded**: Shows full metrics breakdown
- Click to toggle expansion
- Only one notification can be expanded at a time

## Integration

### Events Published
- `OnMonthlyReportEvent` - Contains full monthly report data

### Events Subscribed
- `OnMonthTickEvent` - Triggers report generation
- `OnDayTickEvent` - Counts days in month
- `OnContractCompletedEvent` - Tracks contract performance
- `OnProductReleasedEvent` - Tracks product launches
- `OnCashChangedEvent` - Tracks finances
- `OnGameStartedEvent` - Resets report data

## Usage

### View Monthly Reports
1. Play the game through at least one month
2. Check notification panel (top right)
3. Find month report notification (e.g., "February 2026 Report")
4. **Click the notification** to expand
5. View detailed metrics

### Example Report

```
February 2026 Report (▲ Expanded)
  Contracts Completed:    3
  Products Released:      1
  Average Morale:         82%
  ──────────────────────────
  Money Earned:    $28,500
  Money Spent:     $15,200
  Profit:          $13,300
```

## Configuration

The system automatically tracks all metrics. No configuration needed.

## Testing

### Quick Test
1. Enter Play mode
2. Start a new game (GameManager → Debug: Start New Game)
3. Advance time to fast speed (>>>)
4. Wait for month to end
5. Check notifications for monthly report
6. Click report to expand

### Debug Commands
No debug commands needed - reports generate automatically.

## Technical Details

### MonthlyReport Data Structure
```csharp
public class MonthlyReport
{
    public int DaysInMonth;
    public int ContractsCompleted;
    public int ContractsFailed;
    public int ProductsReleased;
    public float MoneyEarned;
    public float MoneySpent;
    public float Profit;
    public float AverageMorale;
}
```

### Event Flow
```
Month Ends
    ↓
OnMonthTickEvent published
    ↓
MonthlyReportSystem collects data
    ↓
Calculate average morale & profit
    ↓
Publish OnMonthlyReportEvent
    ↓
NotificationManager receives event
    ↓
Create expandable notification
    ↓
Reset counters for next month
```

## Styling

### CSS Classes
- `.notification-details` - Expanded content container
- `.monthly-report-details` - Report-specific styling
- `.report-row` - Each metric row
- `.report-value.positive` - Green values (profit, good morale)
- `.report-value.negative` - Red values (loss, expenses)
- `.report-separator` - Visual divider

### Customization
Edit `/Assets/UI/UIToolkit/Styles/NotificationPanel.uss` to customize colors, spacing, or layout.

## Future Enhancements

- Historical report archive
- Compare month-over-month performance
- Trend indicators (↑ improved, ↓ declined)
- Export reports to external file
- Charts/graphs in expanded view
- Achievements based on monthly performance
- Email-style report notifications
