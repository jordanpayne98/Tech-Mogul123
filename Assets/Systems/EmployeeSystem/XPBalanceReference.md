# Employee XP Balance & Training vs Hiring Strategy

## Overview
Employee skill growth through contracts is now balanced to create a strategic choice: train existing employees through contracts OR hire better employees at higher cost.

## XP System Mechanics

### How XP Works
1. **Contract completion** awards XP based on `baseXPReward` from contract template
2. **XP is distributed** proportionally to the required skills
3. **Skills increase directly** by the XP amount (capped at 100)

Example:
```
Contract requires: Dev 60, Design 30, Marketing 10 (total: 100)
Base XP: 6
Distribution:
- Dev XP: 6 × (60/100) = 3.6
- Design XP: 6 × (30/100) = 1.8
- Marketing XP: 6 × (10/100) = 0.6

If employee has Dev 50, after contract: Dev 53.6
```

## Updated XP Rewards (Reduced ~50%)

| Contract Type | Old XP | New XP | Reduction |
|--------------|--------|--------|-----------|
| QuickWebsite | 5 | **3** | -40% |
| MobileApp | 12 | **6** | -50% |
| SoftwareIntegration | 10 | **5** | -50% |
| EnterpriseSystem | 15 | **7** | -53% |
| CustomSolution | 12 | **6** | -50% |

## Skill Growth Rate Comparison

### Training Through Contracts (Slow, Cheap)

**Early Game (0-100 rep):**
```
Contract: QuickWebsite (Easy)
XP: 3 total
Typical distribution: Dev +1.8, Design +0.9, Marketing +0.3
Time: 15-20 days
Cost: Only employee salary during contract

Result: Small skill gains over weeks
```

**Mid Game (250 rep):**
```
Contract: SoftwareIntegration (Medium)
XP: 5 total
Typical distribution: Dev +2.5, Design +1.5, Marketing +1.0
Time: 25-32 days
Cost: Employee salary (~$6,000-8,000/month)

Result: Moderate skill gains
```

**Late Game (450 rep):**
```
Contract: EnterpriseSystem (Hard)
XP: 7 total
Typical distribution: Dev +3.5, Design +2.5, Marketing +1.0
Time: 35-45 days
Cost: Elite employee salary (~$10,000-15,000/month)

Result: Good skill gains but expensive salaries
```

### Hiring Better Employees (Fast, Expensive)

**Early Game (0-100 rep):**
```
Current employee: Dev 20, Design 10, Marketing 6
Fire & Hire new: Dev 38, Design 19, Marketing 12

Skill gain: +18 Dev, +9 Design, +6 Marketing (instant!)
Cost: Firing penalty + signing bonus + higher salary
```

**Mid Game (250 rep):**
```
Current employee: Dev 50, Design 25, Marketing 15
Fire & Hire new: Dev 64, Design 32, Marketing 21

Skill gain: +14 Dev, +7 Design, +6 Marketing (instant!)
Cost: Significant upfront cost + ~20% higher salary
```

**Late Game (450 rep):**
```
Current employee: Dev 85, Design 45, Marketing 30
Fire & Hire new: Dev 100, Design 51, Marketing 34

Skill gain: +15 Dev, +6 Design, +4 Marketing (instant!)
Cost: Large upfront + much higher ongoing salary
```

## Strategic Comparison

### Training Strategy (Contract XP)
**Pros:**
- ✅ No upfront cost
- ✅ Builds team loyalty (implied future feature)
- ✅ Gradual predictable growth
- ✅ Learn while earning (contracts still pay)

**Cons:**
- ❌ VERY slow (3-7 XP per contract)
- ❌ Requires many contracts to see results
- ❌ Employees may cap out at low skills
- ❌ Still paying salary during training

**Time to grow +10 Dev skill:**
- Early contracts (3 XP): ~6 contracts × 15 days = **90 days**
- Mid contracts (5 XP): ~4 contracts × 25 days = **100 days**
- Late contracts (7 XP): ~3 contracts × 35 days = **105 days**

### Hiring Strategy (Fire & Replace)
**Pros:**
- ✅ INSTANT skill upgrade
- ✅ Get exactly the skills you need
- ✅ No waiting for growth
- ✅ Can target specific roles

**Cons:**
- ❌ Large upfront cost (signing bonus)
- ❌ Higher ongoing salary (~20-40% more)
- ❌ Firing penalty for existing employee
- ❌ Risk of bad hire (RNG skills)

**Upfront cost example at 100 rep:**
```
Fire employee: -$3,000 penalty
Hire new: -$4,000 signing bonus
Higher salary: -$1,000/month extra
Total first month: -$8,000

Benefit: +18 Dev skill IMMEDIATELY
```

## Recommended Balance

### When to Train
- **Early game** when cash is tight
- **Decent employees** already hired (don't want to fire)
- **Long-term planning** (building core team)
- **Between hiring waves** (gradual improvement)

### When to Hire
- **Mid-game** when you have cash flow
- **Need immediate skill boost** for hard contracts
- **Stuck with low-skill employees** (cap problem)
- **Expanding team** (new roles needed)

## Example Scenarios

### Scenario 1: Patient Builder (Training Focus)
```
Day 1: Hire 4 employees (Dev 19, 15, 15, 12) - $12K upfront
Month 1-3: Complete 6 Easy contracts (+18 XP distributed)
After 90 days: Best Dev now ~35 skill
Investment: $12K initial + ~$30K salaries = $42K
Result: Decent team, slow growth
```

### Scenario 2: Aggressive Scaler (Hiring Focus)
```
Day 1: Hire 3 employees (Dev 19, 15, 15) - $9K upfront
Day 30: Fire worst, hire better (Dev 38) - $8K cost
Day 60: Fire another, hire better (Dev 38) - $8K cost
After 90 days: Top team (Dev 38, 38, 35)
Investment: $9K + $16K upgrades + $35K salaries = $60K
Result: Strong team, high cost
```

### Scenario 3: Balanced Approach (Hybrid)
```
Day 1: Hire 4 employees (Dev 19, 15, 15, 12) - $12K
Month 1-2: Complete 4 contracts (+12 XP) while earning
Day 60: Fire worst employee, hire upgrade - $7K
After 90 days: Mixed team (Dev 38, 23, 23, 21)
Investment: $12K + $7K + $32K salaries = $51K
Result: Good balance of cost and power
```

## Math Summary

### ROI on Training
```
Contract XP: 3-7 per contract
Days needed for +10 skill: 90-105 days
Cost: Salary only (~$3K-5K/month)

ROI: Moderate - slow growth but earning while learning
```

### ROI on Hiring
```
Instant skill gain: +10-20 immediately
Upfront cost: $6K-10K
Ongoing cost: +20-40% salary

ROI: High immediate, expensive long-term
```

## Design Philosophy

The system creates meaningful choices:
1. **Early game**: Train because you can't afford hiring
2. **Mid game**: Mix of both - upgrade key roles, train others
3. **Late game**: Hiring is more viable (higher cash flow)

**Target**: Training takes ~3 months to equal one hiring upgrade
**Result**: Both strategies are viable depending on playstyle and budget

## Future Considerations

Potential systems to enhance choice:
- Employee loyalty reducing cost of long-term employees
- Training bonuses for employees with high morale
- Skill degradation if not practicing (encourages hiring)
- Mentorship system (senior employees train juniors faster)
