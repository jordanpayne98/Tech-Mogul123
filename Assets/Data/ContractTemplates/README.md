# Contract Template ScriptableObjects

This folder contains contract type definitions used to generate available contracts.

## What Goes Here

ScriptableObject assets defining different contract types with goals, deadlines, and payouts.

### Phase 1 Contract Templates

Create these three contract template assets:

**QuickWebsite.asset**
- Difficulty: Easy
- Possible Goals: Quality 60, Complete 3 features
- Min/Max Goals: 1-2
- Deadline: 10 days
- Payout: $2,000 - $5,000
- Skill Weights: 40% Dev, 40% Design, 20% Marketing
- Burnout Impact: 10
- XP Reward: 5

**SoftwareIntegration.asset**
- Difficulty: Medium
- Possible Goals: Quality 75, Complete 5 features, Meet deadline
- Min/Max Goals: 2-3
- Deadline: 20 days
- Payout: $5,000 - $12,000
- Skill Weights: 60% Dev, 25% Design, 15% Marketing
- Burnout Impact: 15
- XP Reward: 8

**CustomSolution.asset**
- Difficulty: Hard
- Possible Goals: Quality 85, Complete 8 features, Meet tight deadline
- Min/Max Goals: 3-4
- Deadline: 30 days
- Payout: $10,000 - $25,000
- Skill Weights: 50% Dev, 30% Design, 20% Marketing
- Burnout Impact: 20
- XP Reward: 12

## How to Create

1. Create `ContractTemplateSO.cs` script (see Data Design documentation)
2. Right-click in this folder → Create → TechMogul → Contract Template
3. Name the asset (e.g., `QuickWebsite`)
4. Configure values in Inspector

## Documentation

See [Data Design](/Pages/Data Design.md) for ContractTemplateSO script structure and detailed examples.

## Usage

ContractSystem uses these templates to generate available contracts each month.

## Status

- [x] ContractTemplateSO script created ✓
- [ ] QuickWebsite template - Ready to create (Right-click → Create → TechMogul → Contract Template)
- [ ] SoftwareIntegration template - Ready to create (Right-click → Create → TechMogul → Contract Template)
- [ ] CustomSolution template - Ready to create (Right-click → Create → TechMogul → Contract Template)

See [ScriptableObject Assets Guide](/Pages/ScriptableObject Assets Guide.md) for detailed creation instructions.
