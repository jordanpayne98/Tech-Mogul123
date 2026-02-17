# EmployeeSystem

✅ **IMPLEMENTED** - Complete employee management system for Tech Mogul.

## Overview

The EmployeeSystem manages all aspects of employees including hiring, firing, skills, morale, burnout, and salary payments.

## Implemented Components

### ✅ Employee.cs

Core employee data class containing:
- **Identity**: ID, name, role reference
- **Skills**: Development, Design, Marketing (0-100 scale)
- **Well-being**: Morale (0-100), Burnout (0-100)
- **Work**: Monthly salary, current assignment tracking, availability status
- **Experience**: Days employed, total projects/contracts completed

**Key Features:**
- Skill generation with variance from role templates
- Effective skill calculation (modified by morale/burnout)
- Work assignment tracking
- Burnout accumulation and recovery
- Morale management
- Skill progression system

### ✅ EmployeeSystem.cs

Manager MonoBehaviour handling:
- Employee hiring from role templates
- Employee firing (with assignment checks)
- Daily burnout recovery and morale updates
- Monthly automatic salary payments
- Low morale warnings
- Random name generation

**Active in Scene:** Added to `_GameSystems` GameObject

## Usage Examples

### Hire Employee
```csharp
EventBus.Publish(new RequestHireEmployeeEvent 
{ 
    RoleTemplate = developerRoleSO,
    EmployeeName = "Alex Chen" // Optional
});
```

### Fire Employee
```csharp
EventBus.Publish(new RequestFireEmployeeEvent 
{ 
    EmployeeId = employee.employeeId 
});
```

### Access Employees
```csharp
var all = EmployeeSystem.Instance.Employees;
var available = EmployeeSystem.Instance.GetAvailableEmployees();
var devs = EmployeeSystem.Instance.GetEmployeesByRole(devRole);
```

## Testing

**Context Menu Commands (Editor Only):**
1. Right-click EmployeeSystem component
2. Choose:
   - **Hire Random Developer**
   - **Hire Random Designer**
   - **Log All Employees**

## Events Integration

**Subscribes:** `OnDayTickEvent`, `OnMonthTickEvent`, `RequestHireEmployeeEvent`, `RequestFireEmployeeEvent`

**Publishes:** `OnEmployeeHiredEvent`, `OnEmployeeFiredEvent`, `OnEmployeeLowMoraleEvent`, `RequestDeductCashEvent`

## Mechanics

- **Effective Skill** = Base Skill × (1 + Morale Modifier + Burnout Modifier)
- **Burnout Recovery**: 1 point/day when available
- **Morale Changes**: Daily adjustments based on burnout and work status
- **Salary Payment**: Automatic monthly deduction via GameManager

## Next Steps

- [ ] Create Employee UI Panel
- [ ] Implement hire dialog
- [ ] Add employee detail view
- [ ] Build employee list display

See [Employee System](/Pages/Systems/Employee System.md) for complete documentation.

## Dependencies

- TimeSystem (for monthly salary payments)
- EventBus (for event communication)
- Role ScriptableObjects (in `/Assets/Data/Roles/`)

## Status

- [ ] Not yet implemented
