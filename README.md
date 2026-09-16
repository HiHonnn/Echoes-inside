# Echoes Inside

Echoes Inside is a small 2D narrative puzzle game developed as a Unity learning project. The player explores memory-themed rooms, interacts with family members, and completes a different gameplay challenge in each chapter.

The project focuses on gameplay programming, scene flow, state management, and applying object-oriented design in a complete game prototype.

## Gameplay

The current build contains eight configured scenes and three chapter-specific gameplay loops:

- An inventory and picture assembly puzzle.
- A maze with collectible objectives, waypoint patrol enemies, player detection, and respawning.
- A timed cooking challenge with ingredient collection, recipes, and four cooking stations.
- Dialogue, static cutscenes, scene fades, background music, and sound effect coordination.

## Technical Highlights

- 2D player movement with `Rigidbody2D`, directional animation, and sprite flipping.
- Camera boundary management with `CinemachineConfiner2D`.
- Session-level progression, inventory, puzzle state, and player-position restoration across scenes.
- Reusable puzzle flow through `IPuzzleStrategy` and `PuzzleController`.
- Event-driven completion flow using C# events.
- Ingredient composition implemented with the Decorator pattern.
- Chapter-specific environment, puzzle, enemy, and audio creation through Abstract Factory implementations.
- Play Mode coverage for scene transitions, camera behavior, dialogue, cutscenes, UI fades, and audio routing.

## Technology

- Unity 6000.2.9f1
- C#
- Universal Render Pipeline 2D
- Unity 2D Physics
- Cinemachine
- TextMesh Pro
- Unity Test Framework 1.6.0

## Project Structure

```text
Assets/
├── Scenes/                         # Playable scenes
├── _EchoesInside/
│   ├── Scripts/
│   │   ├── Core/                   # Player movement, game state, map transitions
│   │   ├── Interaction/            # Shared interaction components
│   │   ├── Chapter1_Grandma/       # Inventory and picture puzzle
│   │   ├── Chapter2_Father/        # Room and maze gameplay
│   │   ├── Chapter3_Mom/           # Cooking gameplay
│   │   ├── DreamSystem/            # Puzzle strategies and factories
│   │   └── Narrative/              # Dialogue, cutscenes, fades, audio, tests
│   ├── Art/
│   ├── Animations/
│   └── Prefabs/
├── Settings/
└── TextMesh Pro/
```

## Opening the Project

1. Install Unity 6000.2.9f1 through Unity Hub.
2. Clone the repository.
3. Add the repository directory as a Unity project.
4. Open `Assets/Scenes/TitleScreen.unity`.
5. Enter Play Mode.

## Tests

Open **Window > General > Test Runner**, select **PlayMode**, and run the test assembly under:

```text
Assets/_EchoesInside/Scripts/Narrative/Tests/PlayMode
```

The repository currently contains 42 Play Mode test cases.

## Asset Notice

Third-party audio source packs are intentionally excluded from this portfolio repository until their redistribution terms are verified. A clean clone may therefore contain missing audio references; the gameplay code, scenes, and test sources remain available for review.

Before publishing, review [ASSET_AUDIT.md](ASSET_AUDIT.md) and confirm that every included visual asset can be redistributed.

## Repository Purpose

This repository is shared for portfolio and recruitment review. No open-source license has been granted for the original source code or project assets.
