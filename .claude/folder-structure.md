# Ratocalypse 2.5 — Folder Structure & Asset Conventions

## Assets root

```
Assets/
├── Cartoon Heroes/          ← third-party asset pack — NIE MODYFIKUJ
├── Modular Industrial Set/  ← third-party asset pack — NIE MODYFIKUJ
├── Plugins/FMOD/            ← external audio plugin — NIE MODYFIKUJ
├── Ratocalypse/             ← cały kod i assety gry (jedyne miejsce do pracy)
├── Resources/               ← Unity built-in
├── Settings/                ← URP renderer assets (PC + Mobile)
└── TextMesh Pro/            ← bundled z Unity
```

## Assets/Ratocalypse/ — szczegółowo

```
Ratocalypse/
├── Animations/
│   └── EnemyAnimator.controller   ← aktualnie pusty (brak klatek animacji wroga)
│
├── Data/                          ← wszystkie ScriptableObject assets
│   ├── Enemies/
│   │   └── CommonRat.asset        ← EnemyData
│   ├── Items/
│   │   └── sword.asset            ← ItemData
│   └── Player/
│       └── PlayerStats.asset      ← PlayerStats (singleton asset)
│
├── Editor/
│   └── EnemyAnimatorCreator.cs    ← narzędzie edytora
│
├── Scenes/
│   ├── Building.unity             ← główna scena gry (aktywna)
│   ├── Main Menu/
│   │   └── MainMenu.unity
│   └── [inne sceny]
│
├── Scripts/
│   ├── Core/                      ← EventBus, GameManager, QuestManager, GameEvents, GameState, interfejsy
│   ├── Enemy/                     ← EnemyAI, EnemyData
│   ├── Interactable/              ← ItemPickup
│   ├── Inventory/                 ← InventorySystem, ItemData
│   ├── NPC/                       ← NPCInteraction
│   ├── Player/                    ← PlayerController, PlayerHealthComponent, PlayerCombat,
│   │                                 PlayerStats, XPSystem, SkillTreeManager,
│   │                                 CameraController, CameraZoom, OcclusionFader
│   └── UI/                        ← PlayerHUD, InventoryUI, InventorySlotUI, XPBarUI,
│                                     SkillTreeUI, SkillNodeUI, QuestDialogUI,
│                                     MinimapController, MinimapMarker, ButtonHoverEffect
│
├── Textures/                      ← tekstury projektu
│
└── UI/                            ← assety UI (tylko art, bez skryptów)
    ├── Backgrounds/
    ├── Fonts/
    ├── Icons/
    └── Sprites/
```

## Konwencje nazewnictwa

| Typ | Konwencja | Przykład |
|---|---|---|
| Foldery własne | PascalCase, bez spacji | `Scripts/`, `Data/`, `Enemies/` |
| Skrypty `.cs` | PascalCase = nazwa klasy | `PlayerController.cs` |
| ScriptableObject assets | PascalCase, opisowo | `CommonRat.asset`, `Sword4Short.asset` |
| Sceny | PascalCase | `Building.unity`, `MainMenu.unity` |
| Third-party | bez zmian (nie ruszamy) | `Cartoon Heroes/`, `Modular Industrial Set/` |

## Gdzie tworzyć nowe assety

| Asset | Lokalizacja |
|---|---|
| Nowy EnemyData (ScriptableObject) | `Assets/Ratocalypse/Data/Enemies/` |
| Nowy ItemData (ScriptableObject) | `Assets/Ratocalypse/Data/Items/` |
| Nowy QuestData (ScriptableObject) | `Assets/Ratocalypse/Data/Quests/` *(utwórz folder)* |
| Nowy Animator Controller | `Assets/Ratocalypse/Animations/` |
| Nowy skrypt | `Assets/Ratocalypse/Scripts/[odpowiedni podkatalog]/` |
| Nowy skrypt UI | `Assets/Ratocalypse/Scripts/UI/` |
| Nowy skrypt edytora | `Assets/Ratocalypse/Editor/` |

## Build Settings

| Scena | Index | Status |
|---|---|---|
| `SampleScene.unity` | 0 | disabled (nieużywana) |
| `MainMenu.unity` | 1 | ✅ enabled |
| `Building.unity` | 2 | ✅ enabled |

## Znane TODO / Planowane ulepszenia

**OcclusionFader — fade shader**  
Aktualnie: instant hide przez `renderer.enabled`.  
Plan: `Shader Graphs/MIS_Base 1` → Surface Type: Transparent → właściwość `_Alpha` → Alpha output → w OcclusionFader użyć `MaterialPropertyBlock` zamiast `renderer.enabled`.  
⚠️ Nie zaczynaj bez dyskusji — logika OcclusionFader wymagała 7+ iteracji.

**Enemy Animator**  
`EnemyAnimator.controller` istnieje ale jest pusty. Wymaga animacji (Idle, Walk, Attack, Stun, Dead) gdy będą dostępne.

**SaveSystem**  
Format do potwierdzenia z użytkownikiem. `SkillTreeManager.RestoreFromSave()` już gotowe.
