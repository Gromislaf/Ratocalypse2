# Struktura projektu — kod i assety

> Właściciel: `architekt`. Zmiana (nowy folder, przeniesienie, nowa kategoria) — wyłącznie przez wniosek (Z13), jak wyjątki w dokumentacji.
> Bez wniosku wolno: dodać plik w istniejącym folderze, zgodnie z tabelą „Gdzie tworzyć”.
> Zastępuje dawny `.claude/folder-structure.md` (archiwum: `.claude/_archiwum/`).

---

## Katalog główny

```
Ratocalypse 2.5/
├── Assets/              kod i assety (niżej)
├── Documentation/       specyfikacja dla promotora (.claude/rules/dokumentacja-promotor.md)
├── Vision/              wizja, filary, roadmapa, świat, warsztat treści
├── Packages/            manifest pakietów — tylko autor (Z11)
├── ProjectSettings/     ustawienia projektu — tylko autor (Z11)
├── Tools/               narzędzia autora (poza zakresem agentów, chyba że zlecenie mówi inaczej)
└── .claude/             konfiguracja agentów
```

Pliki archiwalne w katalogu głównym (`GameDesignDoc.html`, `docs_mechanics.html`, `Dokumentacja.pdf`, `zrzut1.png`) — do decyzji autora
(zadanie 27): przeniesienie do `Vision/_ref/` lub usunięcie z repozytorium.

## Assets

```
Assets/
├── Cartoon Heroes/          zewnętrzna — nie modyfikujemy (Z12)
├── Modular Industrial Set/  zewnętrzna — nie modyfikujemy (Z12)
├── Plugins/FMOD/            zewnętrzna — nie modyfikujemy (Z12)
├── TextMesh Pro/            zewnętrzna — nie modyfikujemy (Z12)
├── Resources/               Unity — nie używamy do ładowania danych gry (Z9)
├── Settings/                URP renderer assets (PC + Mobile) — tylko autor
└── Ratocalypse/             jedyne miejsce pracy
```

## Assets/Ratocalypse

```
Ratocalypse/
├── Animations/              Animator Controllers (EnemyAnimator; PlayerAnimator po zadaniu 24)
├── Data/                    wszystkie definicje ScriptableObject
│   ├── Enemies/             EnemyData
│   ├── Items/               ItemData
│   ├── Player/              PlayerStats
│   ├── Quests/              QuestData            (do utworzenia — zadanie 27)
│   ├── Skills/              definicje skilli     (zadanie 12)
│   └── Audio/               FMODEvents           (jeśli przeniesiony z obecnej lokalizacji — decyzja w zadaniu)
├── Editor/                  skrypty edytora
├── Input/                   plik akcji Input Systemu (zadanie 25)
├── Prefabs/
├── Scenes/
│   ├── Building.unity       główna scena gry
│   ├── Benchmark.unity      scena pomiarowa (ochrona czasowa — Z12)
│   ├── Main Menu/MainMenu.unity
│   └── Building/            dane sceny (NavMesh)
├── Scripts/
│   ├── Core/                EventBus, GameEvents, GameManager, GameState, interfejsy, AudioManager, FMODEvents,
│   │                        QuestManager, QuestData, SaveSystem, SaveData, SkillDefinition, Benchmark*, PerformanceMonitor
│   ├── Enemy/               EnemyAI, EnemyData
│   ├── Interactable/        ItemPickup, SavePoint
│   ├── Inventory/           InventorySystem, ItemData
│   ├── NPC/                 NPCInteraction
│   ├── Player/              PlayerController, PlayerHealthComponent, PlayerCombat, PlayerStats, XPSystem,
│   │                        SkillTreeManager, CameraController, CameraZoom, OcclusionFader
│   └── UI/                  PlayerHUD, InventoryUI, InventorySlotUI, XPBarUI, SkillTreeUI, SkillNodeUI,
│                            QuestDialogUI, MinimapController, MinimapMarker, ButtonHoverEffect
├── Tests/                   testy EditMode/PlayMode (zadanie 26) — pisze wyłącznie `tester`
├── Textures/
└── UI/                      assety UI (tylko grafika, bez skryptów): Backgrounds, Fonts, Icons, Sprites
```

**Znany dług strukturalny** (w kolejce): `Scripts/Core/` jako worek na wszystko (QuestManager, SaveSystem, Benchmark, SkillDefinition) — porządek przez wniosek Architekta;
assety w złym miejscu: `Scenes/NewEnemyData.asset`, `Scenes/NewItem.asset`, `Scenes/Game/New Quest Data.asset` (zadanie 27).

## Gdzie tworzyć

| Co | Gdzie |
|---|---|
| Nowa definicja ScriptableObject | `Assets/Ratocalypse/Data/<kategoria>/` |
| Nowy skrypt rozgrywki | `Assets/Ratocalypse/Scripts/<moduł>/` (moduł = folder funkcji, Z1.4) |
| Nowy skrypt UI | `Assets/Ratocalypse/Scripts/UI/` |
| Nowy skrypt edytora | `Assets/Ratocalypse/Editor/` |
| Test | `Assets/Ratocalypse/Tests/` (po zadaniu 26) |
| Animator Controller | `Assets/Ratocalypse/Animations/` |
| Prefab | `Assets/Ratocalypse/Prefabs/` |

## Nazewnictwo

| Typ | Konwencja | Przykład |
|---|---|---|
| Foldery własne | PascalCase, bez spacji | `Scripts/`, `Enemies/` |
| Skrypty `.cs` | nazwa pliku = nazwa klasy | `PlayerController.cs` |
| Assety ScriptableObject | PascalCase, opisowo | `CommonRat.asset` |
| Sceny | PascalCase | `Building.unity` |

## Build Settings

| Scena | Index | Status |
|---|---|---|
| `SampleScene.unity` | 0 | wyłączona |
| `MainMenu.unity` | 1 | włączona |
| `Building.unity` | 2 | włączona |

`Benchmark.unity` — poza Build Settings gry; do buildów pomiarowych dodaje autor (Z10.4).
