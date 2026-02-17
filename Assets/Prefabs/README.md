# Shared Prefabs

This folder contains non-UI prefabs used across the game.

## What Goes Here

### System Prefabs
- `_GameSystems.prefab` - Complete game systems setup
  - GameManager
  - SaveManager
  - TimeSystem
  - EmployeeSystem
  - ProductSystem
  - ContractSystem
  - RivalSystem

### Effect Prefabs
- Particle effects (if any)
- Animation prefabs
- Visual feedback elements

### Audio Prefabs
- Sound effect prefabs (if using prefab pattern)
- Music controller (if applicable)

## Usage

### Game Systems Prefab

Once all systems are implemented, create a `_GameSystems.prefab`:

1. Create empty GameObject `_GameSystems`
2. Add all system components
3. Configure references and settings
4. Save as prefab
5. Use this prefab in all game scenes

This ensures consistent system setup across scenes.

## Documentation

See [Architecture](/Pages/Architecture.md) for system initialization order.

## Status

- [ ] _GameSystems prefab not yet created (waiting for all systems to be implemented)
- [ ] Other prefabs as needed
