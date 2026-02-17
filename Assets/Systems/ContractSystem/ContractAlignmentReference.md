# Contract-Employee Alignment Reference

## Overview
Contracts dynamically scale based on available employee quality at each reputation level. The system reads `GetEmployeeQualityMultiplier()` from ReputationSystem to ensure contracts are always challenging but achievable with proper team building.

**Updated: Progressive difficulty scaling** - Early game requires larger teams (2.0× multiplier), gradually reducing to 1.56× at high reputation as employees become more capable.

## Alignment Formula

```csharp
employeeMaxSkill = ReputationSystem.GetEmployeeQualityMultiplier()

// Dynamic multiplier based on reputation percentage
if (reputationPercent < 20%)  multiplier = 2.0 to 1.8
if (reputationPercent < 50%)  multiplier = 1.8 to 1.65
if (reputationPercent >= 50%) multiplier = 1.65 to 1.56

contractScaling = (employeeMaxSkill × multiplier) / 100
contractRequirement = baseSkillRequirement × contractScaling × variance(0.85-1.15)
```

**Key Design Goal:** Early game requires large teams (4-5 employees), late game still needs coordination (2-3 employees) but more efficient.

## Detailed Alignment by Reputation

### At 0 Reputation (0% - 0 Stars)
**Employees Available:**
- Max Skill: 15
- Primary (1.3×): 20
- Typical Dev: 19, Design: 10, Marketing: 6

**Contracts Generated:**
- Multiplier: 2.0×
- Scaling: 0.26-0.44×
- Easy Contracts: 26-44 skill requirement
- **Team Needed**: 4-5 employees
- **Strategy**: Must build full starting roster immediately

---

### At 50 Reputation (10% - 1 Star)
**Employees Available:**
- Max Skill: 22
- Primary (1.3×): 29
- Typical Dev: 28, Design: 14, Marketing: 9

**Contracts Generated:**
- Multiplier: 1.9×
- Scaling: 0.36-0.62×
- Easy Contracts: 36-62 skill requirement
- **Team Needed**: 3-4 employees
- **Strategy**: Still need large teams for Easy contracts

---

### At 100 Reputation (20% - 1 Star)
**Employees Available:**
- Max Skill: 30
- Primary (1.3×): 39
- Typical Dev: 38, Design: 19, Marketing: 12

**Contracts Generated:**
- Multiplier: 1.8×
- Scaling: 0.46-0.78×
- Easy Contracts: 46-78 skill requirement
- Medium Contracts: Rare
- **Team Needed**: 3-4 employees
- **Strategy**: Teams starting to become efficient

---

### At 150 Reputation (30% - 2 Stars)
**Employees Available:**
- Max Skill: 37
- Primary (1.3×): 48
- Typical Dev: 47, Design: 24, Marketing: 15

**Contracts Generated:**
- Multiplier: 1.75×
- Scaling: 0.55-0.94×
- Easy Contracts: 55-94 skill requirement
- Medium Contracts: Emerging
- **Team Needed**: 3-4 employees
- **Strategy**: Balanced teams becoming viable

---

### At 200 Reputation (40% - 2 Stars)
**Employees Available:**
- Max Skill: 44
- Primary (1.3×): 57
- Typical Dev: 55, Design: 28, Marketing: 18

**Contracts Generated:**
- Multiplier: 1.7×
- Scaling: 0.64-1.09×
- Easy Contracts: 64-100 skill requirement
- Medium Contracts: 30% of offers
- **Team Needed**: 3 employees
- **Strategy**: Specialists with support roles

---

### At 250 Reputation (50% - 3 Stars)
**Employees Available:**
- Max Skill: 51
- Primary (1.3×): 66
- Typical Dev: 64, Design: 32, Marketing: 21

**Contracts Generated:**
- Multiplier: 1.65×
- Scaling: 0.71-1.22×
- Easy Contracts: 71-100 skill requirement
- Medium Contracts: 50% of offers
- Hard Contracts: Starting to appear (20%)
- **Team Needed**: 3 employees
- **Strategy**: Efficient coordinated teams

---

### At 300 Reputation (60% - 3 Stars)
**Employees Available:**
- Max Skill: 58
- Primary (1.3×): 76
- Typical Dev: 74, Design: 37, Marketing: 24

**Contracts Generated:**
- Multiplier: 1.63×
- Scaling: 0.81-1.38×
- Easy Contracts: Rare (30%)
- Medium Contracts: 50% of offers
- Hard Contracts: 20% of offers
- **Team Needed**: 3 employees
- **Strategy**: Quality specialists needed

---

### At 350 Reputation (70% - 4 Stars)
**Employees Available:**
- Max Skill: 66
- Primary (1.3×): 86
- Typical Dev: 84, Design: 42, Marketing: 28

**Contracts Generated:**
- Multiplier: 1.61×
- Scaling: 0.90-1.54×
- Easy Contracts: Rare (30%)
- Medium Contracts: 50%
- Hard Contracts: 20%
- **Team Needed**: 2-3 employees
- **Strategy**: Elite employees more efficient

---

### At 400 Reputation (80% - 4 Stars) ⭐
**Employees Available:**
- Max Skill: 73
- Primary (1.3×): 95
- Typical Dev: 93, Design: 47, Marketing: 31

**Contracts Generated:**
- Multiplier: 1.59×
- Scaling: 0.99-1.69×
- Easy Contracts: Rare (10%)
- Medium Contracts: 40%
- Hard Contracts: 50%
- **Team Needed**: 2-3 skilled employees
- **Strategy**: Precision team composition

---

### At 450 Reputation (90% - 5 Stars) ⭐
**Employees Available:**
- Max Skill: 80
- Primary (1.3×): 100
- Typical Dev: 100, Design: 51, Marketing: 34

**Contracts Generated:**
- Multiplier: 1.58×
- Scaling: 1.07-1.83×
- Easy Contracts: Very rare (10%)
- Medium Contracts: 40%
- Hard Contracts: 50%
- **Team Needed**: 2-3 employees
- **Strategy**: Master-level coordination

---

### At 500 Reputation (100% - 5 Stars)
**Employees Available:**
- Max Skill: 88
- Primary (1.3×): 100
- Typical Dev: 100, Design: 56, Marketing: 37

**Contracts Generated:**
- Multiplier: 1.56×
- Scaling: 1.17-1.99×
- Easy Contracts: Very rare (10%)
- Medium Contracts: 40%
- Hard Contracts: 50%
- **Team Needed**: 2-3 employees
- **Strategy**: Peak efficiency with elite teams

## Team Composition Examples

### Very Early Game (0-50 rep)
**Easy Contract (~40 skill requirement):**
- Option A: 3 Devs (19 + 19 + 10) = 48 ✓
- Option B: 2 Devs (19 + 19) + 1 Designer (10) + 1 Marketer (6) = 54 ✓
- **Overflow**: 8-14 skill margin
- **Strategy**: Need 4-5 employees minimum, hire immediately

### Early Game (100-200 rep)
**Easy Contract (~65 skill requirement):**
- Option A: 2 Devs (38 + 38) + 1 Designer (19) = 95 ✓
- Option B: 1 Dev (55) + 1 Designer (28) + support (18) = 101 ✓
- **Overflow**: 30-36 skill margin
- **Strategy**: 3-4 employees with role diversity

### Mid Game (250-300 rep)
**Medium Contract (~95 skill requirement):**
- Option A: 2 Devs (64 + 64) + 1 Designer (32) = 160 ✓
- Option B: 1 Dev (74) + 1 Designer (37) + 1 support (24) = 135 ✓
- **Overflow**: 40-65 skill margin (quality bonus)
- **Strategy**: 3 skilled employees ideal

### Late-Mid Game (350-400 rep)
**Hard Contract (~100 skill requirement):**
- Option A: 1 Dev (93) + 1 Designer (47) + 1 support (31) = 171 ✓
- Option B: 2 Devs (84 + 84) + 1 Designer (42) = 210 ✓
- **Overflow**: 71-110 skill margin (large quality bonus)
- **Strategy**: 2-3 elite employees

### End Game (450-500 rep)
**Hard Contract (~100 skill requirement):**
- Option A: 1 Dev (100) + 1 Designer (51) + 1 support (34) = 185 ✓
- Option B: 2 Devs (100 + 100) + 1 Designer (56) = 256 ✓
- **Overflow**: 85-156 skill margin (massive quality bonus)
- **Strategy**: 2-3 max-skilled employees dominate

## Contract Difficulty Distribution

| Reputation % | Easy | Medium | Hard |
|-------------|------|--------|------|
| 0-20% | 100% | 0% | 0% |
| 20-50% | 70% | 30% | 0% |
| 50-75% | 30% | 50% | 20% |
| 75-100% | 10% | 40% | 50% |

**Progressive Unlocking:**
- Medium contracts start appearing at 20% (100/500 rep)
- Hard contracts start appearing at 50% (250/500 rep)
- Easy contracts become rare at 75% (375/500 rep)

## Skill Variance Impact

Contracts have ±15% variance from base scaling:
- **Low roll (0.85×)**: Easier contracts, higher success rate
- **Average (1.0×)**: Standard difficulty
- **High roll (1.15×)**: Challenging contracts, better rewards

**Example at 300 reputation:**
- Base scaling: 0.95×
- Low roll: 0.81× (easier)
- High roll: 1.10× (harder)

## Quality Bonus System

Exceeding contract requirements increases payout:
- **0-10 overflow**: Standard payout
- **10-25 overflow**: +10-20% quality bonus
- **25-50 overflow**: +20-40% quality bonus
- **50+ overflow**: +40%+ quality bonus

**Strategic Implication:** Over-assigning skilled employees = higher profits

## Alignment Verification

To verify alignment in-game, check Console logs:

```
[Employee Gen] Reputation: 250/500 (50%), Employee Max: 51
[Employee Gen] Developer created: Dev 64, Design 32, Marketing 21
[Contract Scaling] Rep: 250/500 (50%), Employee Max: 51, Contract Scaling: 0.71x
[Contract Gen] Medium contract: Required Dev 78, Design 45, Marketing 30
```

**Verification:**
- Employee best: Dev 64
- Contract requires: Dev 78
- Need: 64 + support OR 2 employees ✓

## Dynamic Adjustment Benefits

### Automatically Scales
- Works with ANY max reputation setting
- No hardcoded values
- Always challenging but fair

### Reputation-Driven
- Early game: Accessible contracts
- Mid game: Growing challenge
- Late game: Elite difficulty

### Team Building Enforced
- Can't solo contracts with one employee
- Must build diverse teams
- Encourages strategic hiring

## Summary

✅ **Contracts are fully aligned** with employee skills at every reputation level
✅ **Dynamic scaling** reads actual employee max skills
✅ **1.3× multiplier** ensures teamwork is required
✅ **Smooth progression** from 0-500 reputation
✅ **No sudden jumps** - consistent challenge scaling

The contract system automatically adjusts to your new skill progression, maintaining balanced difficulty throughout the entire game! 🎯
