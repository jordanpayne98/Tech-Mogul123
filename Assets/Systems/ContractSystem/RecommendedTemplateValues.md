# Recommended Contract Template Values

## Current Problem
Base template values (30-70) result in contracts that are too easy after reputation scaling.

## Solution
Increase template base values by ~2.5× to account for the new scaling system.

## Recommended Values by Contract Type

### Easy Contracts (QuickWebsite, Simple Projects)
**Old Values:**
- devSkillMin: 30
- devSkillMax: 70
- designSkillMin: 20
- designSkillMax: 60
- marketingSkillMin: 10
- marketingSkillMax: 50

**NEW RECOMMENDED VALUES:**
```
devSkillMin: 80
devSkillMax: 120
designSkillMin: 50
designSkillMax: 100
marketingSkillMin: 30
marketingSkillMax: 80
```

**Result at 0 rep:**
- Template: 100 (random)
- Easy mod: 100 × 0.5 = 50
- Rep scaling: 50 × 0.345 = 17 skill
- Team needed: 2 employees ✓

### Medium Difficulty (MobileApp, SoftwareIntegration)
**OLD:**
- devSkillMin: 40
- devSkillMax: 80

**NEW RECOMMENDED:**
```
devSkillMin: 100
devSkillMax: 150
designSkillMin: 70
designSkillMax: 120
marketingSkillMin: 40
marketingSkillMax: 90
```

**Result at 100 rep:**
- Template: 125 (random)
- Medium mod: 125 × 1.0 = 125
- Rep scaling: 125 × 0.621 = 78 skill
- Team needed: 3 employees ✓

### Hard Difficulty (EnterpriseSystem, CustomSolution)
**OLD:**
- devSkillMin: 50
- devSkillMax: 90

**NEW RECOMMENDED:**
```
devSkillMin: 120
devSkillMax: 180
designSkillMin: 80
designSkillMax: 140
marketingSkillMin: 50
marketingSkillMax: 100
```

**Result at 250 rep:**
- Template: 150 (random)
- Hard mod: 150 × 1.4 = 210
- Rep scaling: 210 × 0.966 = 203 skill (capped at 100)
- Team needed: 3-4 employees ✓

## Verification Table

| Rep | Difficulty | Template | After Mods | After Scaling | Employees | Team Size | Status |
|-----|-----------|----------|------------|---------------|-----------|-----------|---------|
| 0 | Easy | 100 | 50 | 17 | 20 primary | 2 | ✓ Balanced |
| 50 | Easy | 100 | 50 | 27 | 29 primary | 2-3 | ✓ Good |
| 100 | Easy | 100 | 50 | 31 | 39 primary | 2 | ✓ Good |
| 100 | Medium | 125 | 125 | 78 | 39 primary | 3-4 | ✓ Challenging |
| 250 | Medium | 125 | 125 | 121 | 66 primary | 3 | ✓ Balanced |
| 250 | Hard | 150 | 210 | 203→100 | 66 primary | 3 | ✓ Challenging |
| 500 | Hard | 150 | 210 | 290→100 | 100 primary | 2-3 | ✓ Good |

## Implementation

Update each contract template asset:
1. Open `/Assets/Data/ContractTemplates/[TemplateName].asset`
2. Update skill min/max values per above recommendations
3. Test in-game at various reputation levels

## Quick Reference: Multiplier

To convert old values to new:
```
NEW = OLD × 2.5

Examples:
30 → 75
50 → 125
70 → 175
90 → 225
```

Adjust as needed based on desired difficulty per contract type.
