# Balance Testing Guide

## Quick Setup

1. **Add Debug Component**:
   - Select `_GameSystems` GameObject in Hierarchy
   - Add `ContractBalanceDebugger` component
   - Right-click component → Context Menu for debug commands

2. **Enable Console Logs**:
   - Window → General → Console
   - Enable "Collapse" and "Log" messages
   - Look for `[Contract Scaling]` and `[Employee Gen]` logs

---

## Testing Scenarios

### Scenario 1: Day 1 (0 Reputation) ⭐

**Expected Behavior:**
- Need to hire 4-5 employees immediately
- Contracts require ~20-40 skill
- Single employee (Dev 20) cannot solo
- Team of 3-4 can complete Easy contracts

**How to Test:**
1. Start new game (0 reputation)
2. Open Hire dialog, check available employees
   - Should see Dev ~15-20 primary skill
3. Generate contracts (should be only Easy)
   - Check required skills (~20-40 range)
4. Hire 4 employees
5. Assign 3-4 to one contract
6. Verify contract is completable but challenging

**Debug Command:**
- Right-click `ContractBalanceDebugger` → "Debug: Set Reputation to 0"
- Right-click → "Test: Show Balance at Current Reputation"

**Console Verification:**
```
[Contract Scaling] Rep: 0/500 (0%), Employee Max: 15, Multiplier: 2.00×
[Employee Gen] Reputation: 0/500 (0%), Employee Max: 15
Contract requirement: 26-44 skill
```

---

### Scenario 2: First Growth Phase (100 Reputation) ⭐⭐

**Expected Behavior:**
- Contracts require 3-4 employees
- Employee skills: Dev ~30-39 primary
- XP gains feel slow (~3 per Easy contract)
- 6 contracts needed to gain +10 skill

**How to Test:**
1. Set reputation to 100 (debug command)
2. Check employee pool
   - Primary skills should be ~35-40
3. Generate contracts (70% Easy, 30% Medium)
   - Easy: ~46-78 skill
   - Medium: ~78-130 skill (rare)
4. Complete 3 Easy contracts
5. Check XP gains in console (should be ~3 per contract)
6. Calculate: 3 XP × 6 contracts = 18 XP = ~90 days for +10 skill

**Debug Command:**
- "Debug: Set Reputation to 100"
- "Test: Compare Training vs Hiring Cost"

**Console Verification:**
```
[Contract Scaling] Rep: 100/500 (20%), Employee Max: 30, Multiplier: 1.80×
📈 Alice gained XP - Dev: 1.8, Design: 0.9, Marketing: 0.3
```

---

### Scenario 3: Strategic Choice Phase (250 Reputation) ⭐⭐⭐

**Expected Behavior:**
- Contracts require 3 employees consistently
- Employee skills: Dev ~51-66 primary
- Choice becomes real: train OR hire?
- Should have enough cash to consider hiring

**How to Test:**
1. Set reputation to 250
2. Check employee costs
   - Firing penalty: ~$5,000
   - Signing bonus: ~$6,000
   - Total upgrade cost: ~$11,000
3. Check contract payouts (~$10,000-20,000)
4. Complete 2-3 contracts
5. Calculate if you can afford to hire upgrade
6. Test both paths:
   - Path A: Train 3 employees over 3 months
   - Path B: Hire 1 upgrade immediately

**Debug Command:**
- "Debug: Set Reputation to 250"
- "Test: Compare Training vs Hiring Cost"

**Expected Outcome:**
- Training: Slow but steady progress
- Hiring: Immediate boost but expensive
- Both viable depending on playstyle

---

### Scenario 4: Elite Phase (400 Reputation) ⭐⭐⭐⭐

**Expected Behavior:**
- Contracts require 2-3 elite employees
- Employee skills: Dev ~73-95 primary
- Can start seeing 95 skill employees
- Contracts mostly Hard difficulty

**How to Test:**
1. Set reputation to 400
2. Generate contracts (10% Easy, 40% Medium, 50% Hard)
3. Check skill requirements (should be capped at ~100)
4. Hire employees - should see Dev ~90-95
5. Assign 2-3 to Hard contract
6. Verify quality bonuses are significant (overflow)

**Debug Command:**
- "Debug: Set Reputation to 400"

**Console Verification:**
```
[Contract Scaling] Rep: 400/500 (80%), Employee Max: 73, Multiplier: 1.59×
[Employee Gen] Developer created: Dev 93, Design 47, Marketing 31
```

---

### Scenario 5: Master Phase (500 Reputation) ⭐⭐⭐⭐⭐

**Expected Behavior:**
- Contracts require 2-3 employees
- Employee skills: Max 100 (capped)
- Quality bonuses are huge (2-3× overflow)
- Both training and hiring viable

**How to Test:**
1. Set reputation to 500
2. Generate contracts (all Hard)
3. Hire employees - should see Dev 100
4. Assign 2 max employees to contract
5. Check quality bonus (should be 100%+)
6. Verify payouts are massive

**Debug Command:**
- "Debug: Set Reputation to 500"

---

## Training vs Hiring Test

**Test the strategic choice at each phase:**

### At 0 Reputation
- Training: ONLY option (can't afford hiring)
- Expected: Players train because broke

### At 100 Reputation  
- Training: Still cheap but slow
- Hiring: Starting to be affordable
- Expected: Mix (train main team, hire 1 upgrade)

### At 250 Reputation
- Training: Viable with good cash flow
- Hiring: Clear strategic choice
- Expected: Players choose based on strategy

### At 400+ Reputation
- Training: Good for fine-tuning
- Hiring: Fast to build elite team
- Expected: Both used depending on needs

---

## Console Commands Reference

Right-click `ContractBalanceDebugger` component:

### Information
- **"Test: Show Balance at Current Reputation"** - Shows current state
- **"Test: Simulate All Reputation Levels"** - Prints full progression table
- **"Test: Compare Training vs Hiring Cost"** - Shows strategic comparison

### Reputation Shortcuts
- **"Debug: Set Reputation to 0"** - Test Day 1
- **"Debug: Set Reputation to 100"** - Test early game
- **"Debug: Set Reputation to 250"** - Test mid game
- **"Debug: Set Reputation to 400"** - Test late game
- **"Debug: Set Reputation to 500"** - Test endgame

---

## What to Look For

### ✅ Good Signs
- Contracts always require 2+ employees
- Team size decreases as reputation increases
- XP gains feel meaningful but slow
- Hiring feels like a tough choice (expensive but worth it)
- No soloing contracts except maybe Easy at 200+ rep

### ❌ Red Flags
- Contracts completable with 1 employee (TOO EASY)
- Team sizes increase with reputation (BACKWARDS)
- XP gains too fast (10+ per contract)
- Hiring feels mandatory (training worthless)
- Contracts impossible even with 5 employees (TOO HARD)

---

## Expected Logs During Play

### Contract Generation
```
[Contract Scaling] Rep: 250/500 (50%), Employee Max: 51, Multiplier: 1.65×, Contract Scaling: 0.84×
[Contract Gen] Medium contract: Required Dev 78, Design 45, Marketing 30
```

### Employee Hiring
```
[Employee Gen] Reputation: 250/500 (50%), Employee Max: 51
[Employee Gen] Developer created: Dev 64, Design 32, Marketing 21
```

### XP Gains
```
📈 Alice gained XP - Dev: 2.5, Design: 1.5, Marketing: 1.0
```

---

## Quick Verification Checklist

**Early Game (0-100 rep):**
- [ ] Need 4-5 employees for first contracts
- [ ] XP gains are ~3 per contract
- [ ] Hiring is too expensive
- [ ] Training is only viable option

**Mid Game (200-300 rep):**
- [ ] Need 3 employees for contracts
- [ ] XP gains are ~5-6 per contract
- [ ] Hiring costs ~$10K upfront
- [ ] Both training and hiring feel viable

**Late Game (400-500 rep):**
- [ ] Need 2-3 elite employees
- [ ] XP gains are ~7 per contract
- [ ] Can see 95-100 skill employees
- [ ] Quality bonuses are significant

---

## Automation Test (Advanced)

If you want to automate testing:

1. Right-click `ContractBalanceDebugger`
2. Select "Test: Simulate All Reputation Levels"
3. Check Console output for progression table
4. Verify team sizes decrease smoothly

Expected output:
```
Rep 0/500 (0%) ☆☆☆☆☆ | Employee: 20 | Contract: 17 | Team: 4-5
Rep 100/500 (20%) ★☆☆☆☆ | Employee: 39 | Contract: 31 | Team: 2-3
Rep 250/500 (50%) ★★★☆☆ | Employee: 66 | Contract: 54 | Team: 2-3
Rep 400/500 (80%) ★★★★☆ | Employee: 95 | Contract: 77 | Team: 2-3
Rep 500/500 (100%) ★★★★★ | Employee: 100 | Contract: 90 | Team: 2-3
```

---

## Tips for Manual Testing

1. **Use Debug Commands** - Don't manually play to 500 rep, use shortcuts
2. **Watch Console** - All the math is logged, verify it matches expectations
3. **Test Edge Cases** - Try to solo contracts, try 10 employees on Easy, etc.
4. **Compare Strategies** - Play one save training, another hiring, see which feels better
5. **Feel the Choice** - The decision between train/hire should feel meaningful

---

**Happy Testing!** 🎯

If you find any issues, check:
1. Contract template values (should be 75-180 range)
2. XP rewards (should be 3-7)
3. Reputation scaling formulas in ReputationSystem.cs
4. Contract multiplier in ContractData.cs
