# Employee Skill Scaling Reference (Max Reputation: 500)

## Overview
Employee skills scale dynamically based on reputation percentage. **Slower progression** - you won't see 95+ skills until 400+ reputation, and 100 skills are reserved for 450-500 reputation.

## Key Thresholds

| Reputation | Percentage | Stars | Max Skill | Primary (1.3×) | Skill Jump |
|-----------|------------|-------|-----------|----------------|------------|
| 0/500 | 0% | ☆☆☆☆☆ | 15 | 20 | - |
| 100/500 | 20% | ★☆☆☆☆ | 30 | 39 | +19 |
| 150/500 | 30% | ★★☆☆☆ | 37 | 48 | +9 |
| 200/500 | 40% | ★★☆☆☆ | 44 | 57 | +9 |
| 250/500 | 50% | ★★★☆☆ | 51 | 66 | +9 |
| 300/500 | 60% | ★★★☆☆ | 58 | 76 | +10 |
| 350/500 | 70% | ★★★★☆ | 66 | 86 | +10 |
| 400/500 | 80% | ★★★★☆ | 73 | **95** | +9 ✓ |
| 450/500 | 90% | ★★★★★ | 80 | **100** | +5 ✓ |
| 500/500 | 100% | ★★★★★ | 88 | 100 (capped) | 0 |

## Detailed Skill Ranges by Reputation

### At 0 Reputation (0% - 0 Stars)
- **Reputation**: 0/500
- **Min Skill**: 0
- **Max Skill**: 15
- **Primary Skill (1.3× boost)**: up to 20
- **Expected Range**: 1-20 skills
- **Example**: Dev: 19, Design: 10, Marketing: 6

### At 50 Reputation (10% - 1 Star)
- **Reputation**: 50/500
- **Min Skill**: 1.5
- **Max Skill**: 22
- **Primary Skill (1.3× boost)**: up to 29
- **Expected Range**: 2-29 skills
- **Example**: Dev: 28, Design: 14, Marketing: 9

### At 100 Reputation (20% - 1 Star)
- **Reputation**: 100/500
- **Min Skill**: 3
- **Max Skill**: 30
- **Primary Skill (1.3× boost)**: up to 39
- **Expected Range**: 3-39 skills
- **Example**: Dev: 38, Design: 19, Marketing: 12

### At 125 Reputation (25% - 2 Stars)
- **Reputation**: 125/500
- **Min Skill**: 3.75
- **Max Skill**: 33
- **Primary Skill (1.3× boost)**: up to 43
- **Expected Range**: 4-43 skills
- **Example**: Dev: 42, Design: 21, Marketing: 14

### At 250 Reputation (50% - 3 Stars)
- **Reputation**: 250/500
- **Min Skill**: 7.5
- **Max Skill**: 51
- **Primary Skill (1.3× boost)**: up to 66
- **Expected Range**: 8-66 skills
- **Example**: Dev: 64, Design: 32, Marketing: 21

### At 300 Reputation (60% - 3 Stars)
- **Reputation**: 300/500
- **Min Skill**: 9
- **Max Skill**: 58.5
- **Primary Skill (1.3× boost)**: up to 76
- **Expected Range**: 9-76 skills
- **Example**: Dev: 74, Design: 37, Marketing: 24
- **Progress**: +10 skill from 250 rep

### At 350 Reputation (70% - 4 Stars)
- **Reputation**: 350/500
- **Min Skill**: 10.5
- **Max Skill**: 66
- **Primary Skill (1.3× boost)**: up to 86
- **Expected Range**: 11-86 skills
- **Example**: Dev: 84, Design: 42, Marketing: 28
- **Progress**: +10 skill from 300 rep

### At 400 Reputation (80% - 4 Stars) ⭐ 95 Skill Threshold
- **Reputation**: 400/500
- **Min Skill**: 12
- **Max Skill**: 73
- **Primary Skill (1.3× boost)**: up to **95**
- **Expected Range**: 12-95 skills
- **Example**: Dev: 93, Design: 47, Marketing: 31
- **Progress**: +9 skill from 350 rep

### At 450 Reputation (90% - 5 Stars) ⭐ 100 Skill Threshold
- **Reputation**: 450/500
- **Min Skill**: 13.5
- **Max Skill**: 80
- **Primary Skill (1.3× boost)**: up to **100** (capped)
- **Expected Range**: 14-100 skills
- **Example**: Dev: 100, Design: 51, Marketing: 34

### At 500 Reputation (100% - 5 Stars)
- **Reputation**: 500/500
- **Min Skill**: 15
- **Max Skill**: 88
- **Primary Skill (1.3× boost)**: up to 114 (capped at 100)
- **Expected Range**: 15-100 skills
- **Example**: Dev: 100, Design: 56, Marketing: 37

## Contract Alignment

Contracts are now dynamically scaled to match available employee quality:

### Contract Difficulty Formula
- Contracts require ~1.3× single employee max skill
- Variance: 0.85-1.15× (±15% difficulty variation)
- Encourages team building (need 2-3 employees per contract)

### Examples at Different Reputation Levels

**At 100/500 Reputation (20%):**
- Employee Max: 32 (primary: 42)
- Contract Requirements: 42-55 skill range
- **Need 2-3 employees** to complete

**At 250/500 Reputation (50%):**
- Employee Max: 54 (primary: 70)
- Contract Requirements: 70-91 skill range
- **Need 1-2 good employees** to complete

**At 500/500 Reputation (100%):**
- Employee Max: 90 (primary: 117)
- Contract Requirements: 95-152 skill range (capped at 100)
- **Need skilled team** to complete

### Contract Difficulty Distribution by Reputation

| Reputation % | Easy | Medium | Hard |
|-------------|------|--------|------|
| 0-20% | 100% | 0% | 0% |
| 20-50% | 70% | 30% | 0% |
| 50-75% | 30% | 50% | 20% |
| 75-100% | 10% | 40% | 50% |

## Skill Distribution

### Primary Skill (Role Specialty)
- Base: Random between minSkill and maxSkill
- **Boost: 1.3× multiplier**
- Example: Developer's Dev skill gets 30% boost
- Capped at maxSkill × 1.3 (or 100, whichever is lower)

### Secondary Skills
- Multiplier: 40-70% of primary skill base value
- No boost applied
- Capped at maxSkill (no 1.3× boost)

### Tertiary Skills
- Multiplier: 30-60% of primary skill base value
- No boost applied
- Capped at maxSkill (no 1.3× boost)

## Formulas

```csharp
// Calculate reputation percentage
reputationPercent = (currentReputation / maxReputation) × 100

// Calculate max skill
maxSkill = 20 + (reputationPercent × 0.8)

// Calculate min skill
minSkill = reputationPercent × 0.2

// Primary skill (with boost)
primarySkill = Random(minSkill, maxSkill) × 1.3
primarySkill = Clamp(primarySkill, 1, min(maxSkill × 1.3, 100))

// Secondary skills (no boost)
secondarySkill = primaryBase × Random(0.4, 0.7)
secondarySkill = Clamp(secondarySkill, 1, min(maxSkill, 100))

// Tertiary skills (no boost)
tertiarySkill = primaryBase × Random(0.3, 0.6)
tertiarySkill = Clamp(tertiarySkill, 1, min(maxSkill, 100))
```

## Star Rating Thresholds

With Max Reputation = 500:
- **0 Stars**: 0-49 reputation (0-9%)
- **1 Star**: 50-124 reputation (10-24%)
- **2 Stars**: 125-224 reputation (25-44%)
- **3 Stars**: 225-349 reputation (45-69%)
- **4 Stars**: 350-449 reputation (70-89%)
- **5 Stars**: 450-500 reputation (90-100%)

## Progression Philosophy

### Early Game (0-100 reputation, 0-20%)
- **Max Skills**: 15-30 (primary: 20-39)
- **Focus**: Learn basics, build small teams
- **Contracts**: Easy only, require teamwork

### Mid Game (100-300 reputation, 20-60%)
- **Max Skills**: 30-58 (primary: 39-76)
- **Focus**: Grow business, tackle medium contracts
- **Contracts**: Mix of Easy/Medium, some challenge

### Late-Mid Game (300-400 reputation, 60-80%)
- **Max Skills**: 58-73 (primary: 76-95)
- **Focus**: Build quality teams, compete
- **Contracts**: Medium/Hard mix, significant challenge

### End Game (400-500 reputation, 80-100%)
- **Max Skills**: 73-88 (primary: 95-100)
- **Focus**: Elite employees, hardest contracts
- **Contracts**: Mostly Hard, maximum difficulty

## Why This Design?

### Reserved High Skills
- **95+ skills don't appear until 400+ reputation** (80%)
- Maintains challenge throughout majority of game
- Makes reaching late game feel rewarding

### Gradual Power Growth
- Slower curve means each reputation point matters more
- Can't overpower early/mid game with lucky hires
- Skill progression feels earned

### Team Building Required
- Even at max reputation, contracts challenge you
- Single employee can't carry (need 1.3× their max)
- Encourages diverse hiring and specialization

## Testing

To verify employee generation at different reputation levels:

1. Set reputation via Inspector: `ReputationSystem → Starting Reputation`
2. Start new game
3. Open Hire Employee dialog
4. Check skill ranges match the table above
5. Console shows: `[Employee Gen] Reputation: X, Min: Y, Max: Z`

## Customization

To adjust skill scaling, modify in `ReputationSystem.cs`:

```csharp
// Current: 20-100 skill range
float maxSkill = 20f + (reputationPercent * 0.8f);

// Wider range (10-120):
float maxSkill = 10f + (reputationPercent * 1.1f);

// Narrower range (30-80):
float maxSkill = 30f + (reputationPercent * 0.5f);
```
