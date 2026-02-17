# Role ScriptableObjects

This folder contains all employee role definitions.

## What Goes Here

ScriptableObject assets defining employee roles with their base stats and characteristics.

### Phase 1 Roles

Create these three role assets:

**DeveloperRole.asset**
- Base Dev Skill: 70
- Base Design Skill: 25
- Base Marketing Skill: 15
- Salary Range: $40,000 - $80,000
- Dev Growth Rate: 2.0x
- Design Growth Rate: 0.5x
- Marketing Growth Rate: 0.5x

**DesignerRole.asset**
- Base Dev Skill: 25
- Base Design Skill: 70
- Base Marketing Skill: 35
- Salary Range: $35,000 - $70,000
- Dev Growth Rate: 0.5x
- Design Growth Rate: 2.0x
- Marketing Growth Rate: 1.0x

**MarketerRole.asset**
- Base Dev Skill: 15
- Base Design Skill: 35
- Base Marketing Skill: 70
- Salary Range: $35,000 - $75,000
- Dev Growth Rate: 0.5x
- Design Growth Rate: 1.0x
- Marketing Growth Rate: 2.0x

## How to Create

1. Create `RoleSO.cs` script (see Data Design documentation)
2. Right-click in this folder → Create → TechMogul → Role
3. Name the asset (e.g., `DeveloperRole`)
4. Configure values in Inspector

## Documentation

See [Data Design](/Pages/Data Design.md) for RoleSO script structure and detailed examples.

## Usage

Reference these ScriptableObjects in EmployeeSystem to generate employees with role-specific stats.

## Status

- [x] RoleSO script created ✓
- [ ] DeveloperRole asset - Ready to create (Right-click → Create → TechMogul → Role)
- [ ] DesignerRole asset - Ready to create (Right-click → Create → TechMogul → Role)
- [ ] MarketerRole asset - Ready to create (Right-click → Create → TechMogul → Role)

See [ScriptableObject Assets Guide](/Pages/ScriptableObject Assets Guide.md) for detailed creation instructions.
