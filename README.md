# 王权 · Crown: The Gilded Cage

> You sit upon the highest throne, yet you are the most powerless person in this palace.

An AI-Driven 2D Throne Simulator — Graduate Game Design Project, 2026

---

## Overview

Crown: The Gilded Cage is a 2D visual novel survival game powered by real-time AI interaction. The player takes the role of a sixteen-year-old King who has just inherited the crown — the most powerless person in the palace. The true ruler is the Regent (his uncle), who watches every word from the shadows.

Every word spoken may tighten the noose.

## Core Mechanics · 核心机制

- **Free-form language interaction** — player types freely; Gemini AI parses intent and returns structured JSON in real time
- **Four-resource survival system** — Treasury, Popularity, Church, Military; any resource reaching 0 or 100 triggers a death ending
- **Hidden suspicion metric** — Uncle's Suspicion accumulates with aggressive play, decays slightly each round; reaching 100 triggers the Tower ending
- **22-event random system** — JSON-driven events interrupt between rounds; weighted by current resource danger zones and NPC affinity extremes
- **11 distinct endings** — resource extremes (8), suspicion (1), AI-triggered specials (2)
- **The Mediocrity Principle** — extremes kill; the only path is maintaining balance across all five dimensions simultaneously

## Tech Stack · 技术栈

| Component | Technology |
|-----------|------------|
| Engine | Unity 2022 LTS |
| AI | Gemini API (Google) |
| AI Prompt | `Crown/Assets/StreamingAssets/prompt_v1.txt` |
| Event Data | `Crown/Assets/StreamingAssets/events.json` (22 events) |
| Version Control | Git / GitHub |
| Resolution | 1920×1080 FullScreenWindow (auto-locked, zero Inspector config) |

## Project Structure · 项目结构

```
Design-and-Graphics-Programming-for-Game-2026S-/
├── Crown/                          # Unity project root
│   └── Assets/
│       ├── Scenes/                 # Unity scenes (MainMenu, GameScene, EndingScene)
│       ├── Scripts/                # C# runtime scripts
│       │   ├── GameStateManager.cs     # Resource values, suspicion, round tracking
│       │   ├── APIManager.cs           # Gemini API calls, JSON parsing, memory chain
│       │   ├── DialogueSystem.cs       # NPC turn flow, triggerEvent handling, coroutines
│       │   ├── UIManager.cs            # UI display, typewriter, loading states
│       │   ├── EventManager.cs         # JSON-driven random event system
│       │   ├── AudioManager.cs         # BGM and SFX management
│       │   ├── EndingManager.cs        # Ending detection and scene transition
│       │   ├── ResolutionManager.cs    # 1920×1080 resolution lock (zero-attachment)
│       │   └── UI/
│       │       └── ResourceBarUI.cs    # Per-resource bar component
│       ├── StreamingAssets/        # Runtime data (auto-bundled in all builds)
│       │   ├── prompt_v1.txt       # AI system prompt (active)
│       │   └── events.json         # Random events definition (22 events)
│       ├── Sprites/
│       │   ├── Backgrounds/        # Scene backgrounds
│       │   ├── Characters/         # NPC portrait sprites
│       │   └── UI/                 # Interface assets
│       ├── Audio/
│       │   ├── Music/              # BGM tracks
│       │   └── SFX/                # Sound effects
│       └── Fonts/                  # TextMeshPro fonts
├── Docs/
│   ├── GDD.md                      # Game Design Document
│   ├── storyline_v1.md             # 12-round narrative framework (bilingual)
│   ├── mechanics_v1.1.md           # Mechanics specification
│   ├── endings_v1.md               # Endings design (11 endings)
│   ├── random_events_v1.md         # Random events system (22 events)
│   ├── asset_naming.md             # Asset naming convention
│   └── Prompts/
│       ├── prompt_v1.txt           # AI system prompt (source of truth)
│       └── prompt_template_v1.1.md # Prompt module documentation
├── Tools/
│   └── test_prompt.py              # Prompt testing script
└── README.md
```

## AI Declarations · AI使用声明

| Use | Tool |
|-----|------|
| Visual Art | Seendance 5.1 |
| Music | Mureka V9 |
| Voice / Audio | ElevenLabs |
| In-game NPC Dialogue | Gemini API (Google) |

## License

This project is for academic purposes only.
