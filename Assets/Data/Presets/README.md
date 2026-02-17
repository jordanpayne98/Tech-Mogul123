# Difficulty Preset ScriptableObjects

This folder contains game difficulty presets that modify starting conditions and game balance.

## What Goes Here

ScriptableObject assets defining difficulty settings for Easy, Normal, and Hard modes.

### Phase 1 Difficulty Presets

Create these three preset assets:

**EasyPreset.asset**
- Preset Name: Easy
- Description: Recommended for first-time players
- Starting Cash: $75,000
- Starting Employees: 3
- Salary Multiplier: 0.8x (cheaper)
- Revenue Multiplier: 1.2x (more income)
- Contract Payout Multiplier: 1.2x (more income)
- Burnout Rate Multiplier: 0.7x (slower)
- Morale Decay Multiplier: 0.8x (slower)
- Skill Growth Multiplier: 1.2x (faster)
- Rival Aggressiveness: 0.3 (low)

**NormalPreset.asset**
- Preset Name: Normal
- Description: Balanced experience for most players
- Starting Cash: $50,000
- Starting Employees: 2
- All Multipliers: 1.0x (baseline)
- Rival Aggressiveness: 0.5 (medium)

**HardPreset.asset**
- Preset Name: Hard
- Description: For experienced players seeking a challenge
- Starting Cash: $30,000
- Starting Employees: 1
- Salary Multiplier: 1.2x (more expensive)
- Revenue Multiplier: 0.8x (less income)
- Contract Payout Multiplier: 0.8x (less income)
- Burnout Rate Multiplier: 1.3x (faster)
- Morale Decay Multiplier: 1.2x (faster)
- Skill Growth Multiplier: 0.8x (slower)
- Rival Aggressiveness: 0.8 (high)

## How to Create

1. Create `DifficultyPresetSO.cs` script (see Data Design documentation)
2. Right-click in this folder → Create → TechMogul → Difficulty Preset
3. Name the asset (e.g., `NormalPreset`)
4. Configure values in Inspector

## Documentation

See [Data Design](/Pages/Data Design.md) for DifficultyPresetSO script structure and detailed examples.

## Usage

GameManager references the selected preset on new game start to configure:
- Starting cash
- Initial employee count
- Economic multipliers
- Balance tuning

## Status

- [x] DifficultyPresetSO script created ✓
- [ ] EasyPreset - Ready to create (Right-click → Create → TechMogul → Difficulty Preset)
- [ ] NormalPreset - Ready to create (Right-click → Create → TechMogul → Difficulty Preset)
- [ ] HardPreset - Ready to create (Right-click → Create → TechMogul → Difficulty Preset)

See [ScriptableObject Assets Guide](/Pages/ScriptableObject Assets Guide.md) for detailed creation instructions.
