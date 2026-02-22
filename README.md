# Merge Game Demo

A Unity-based merge puzzle game with cats.

## Overview

This is a merge-two puzzle game where players combine identical items to create higher-tier items. The goal is to collect final-tier items by merging through a progression chain. The game features a grid-based system, drag-and-drop mechanics, item spawning, and collection tracking.

It is a very small project by design, for educational and portfolio purposes.

## Setup Instructions

### Requirements
- Unity 2022.3 LTS or newer
- TextMeshPro package (included)
- Universal Render Pipeline (URP)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/delilah/MergeGameDemo.git
```

2. Open the project in Unity Hub

3. Open the Main scene: `Assets/Scenes/Main.unity`

4. Press Play to run the game

### Configuration

Game settings can be adjusted via ScriptableObjects:

- **GameConfig** (`Assets/ScriptableObjects/GameConfig.asset`):
  - Items to win threshold
  - Audio clips and volumes
  - Visual settings
  - TO BE EXPANDED

- **Item Data** (`Assets/ScriptableObjects/Items/`):
  - Item names and sprites
  - Merge progression chains
  - Final item flags

- **Spawner Data** (`Assets/ScriptableObjects/Spawners/`):
  - Spawnable item pools
  - Spawn cooldown duration

## How to Play

1. **Start Game**: Click the start button on the intro screen
2. **Select Spawner**: Click on the spawner (little house)
3. **Spawn Items**: Click again to spawn a random item on an empty tile
4. **Merge Items**: Drag an item onto another identical item to merge them
5. **Collect**: Click on final-tier items to collect them
6. **Win**: Collect the required number of items to complete the game
7. **Restart**: Click "Play Again" to reset and start over

## Technologies Used

- **Unity 2022.3 LTS**: Game engine
- **C#**: Programming language
- **Universal Render Pipeline (URP)**: Rendering pipeline
- **TextMeshPro**: UI text rendering
- **NUnit**: Unit testing framework
- **Unity Event System**: Input handling

## Development Practices

- **Version Control**: Git with .gitignore for Unity projects
- **Testing**: Unit tests for core gameplay mechanics
- **Documentation**: XML comments and inline documentation
- **Code Style**: Consistent naming conventions and formatting
- **Modular Design**: Reusable components and systems

## ROADMAP

- Cosmetic/VFX Improvements
- Sound Improvements
- UI Improvements
- Multiple Spawners
- Levels
- More Tests
