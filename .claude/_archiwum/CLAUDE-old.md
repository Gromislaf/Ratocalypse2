# CLAUDE.md — Ratocalypse 2.5

## Project Overview
- **Genre:** RPG | **Engine:** Unity 6000.4.3f1 | **Language:** C# | **Pipeline:** URP 17.4.0
- **Migration:** ported from Unity 2022.3.4f1 (Build-In → URP, API Unity 6)
- **Renderer assets:** `Assets/Settings/` (osobne PC i Mobile)

## Build Order
1. ✅ `EventBus` — komunikacja między systemami
2. ✅ `PlayerStats` (ScriptableObject) + `PlayerHealthComponent`
3. ✅ `PlayerController` + `CameraController` — click-to-move NavMesh + kamera izometryczna
4. ✅ `PlayerCombat` — atak + hitbox przez Animation Events
5. ✅ `EnemyAI` — FSM + `EnemyData` (ScriptableObject)
6. ✅ `InventorySystem` + `InventoryUI` — plecak 20 slotów + ekwipunek 6 slotów, toggle `I`
7. ✅ `PlayerHUD` — pasek HP + Stamina, subskrybuje EventBus
8. ✅ `XP/LevelSystem` — XPSystem + XPBarUI + notyfikacja "LEVEL UP!"
9. ✅ `SkillTree` — 12 skilli, 3 gałęzie, toggle `C`
10. ✅ `QuestSystem` — QuestData + QuestManager + NPCInteraction + QuestDialogUI
11. ✅ `SaveSystem` — JSON (Newtonsoft 3.2.2), save pointy w scenie przez IInteractable

## Kluczowe pakiety
- Cinemachine 3.1.6, Input System 1.19.0, TextMeshPro (com.unity.ugui 2.0.0)
- **FMOD** (`Assets/Plugins/FMOD/`) — cały audio przez FMOD, nie `AudioSource`
- AI Navigation 2.0.12, Timeline 1.8.12, Visual Scripting 1.9.11

## Zasady (zawsze obowiązują)
- Unity 6 API — `FindFirstObjectByType`, nie `FindObjectOfType`
- Nie modyfikuj `Assets/Cartoon Heroes/`, `Assets/Modular Industrial Set/`, `Assets/Plugins/FMOD/`
- Nowe ScriptableObject assety → `Assets/Ratocalypse/Data/[podkategoria]/`
- Przy zmianach w walce lub inventory — opisz plan przed implementacją

---

## Kiedy używać którego agenta

| Zadanie | Agent | Dodatkowy plik |
|---|---|---|
| Nowy system / "jak zaprojektować X" | **Architekt** | `.claude/architecture.md` |
| "Napisz skrypt", bugfix | **Programista** | `.claude/conventions.md` + sekcja z `.claude/architecture.md` |
| "Podepnij do sceny", GO, komponenty | **Integrator** | `.claude/folder-structure.md` |
| "Sprawdź projekt", code review | **Inspektor** | — (tylko czyta) |
| Po zakończeniu systemu / po refaktorze | **Kronikarz** | wszystkie `.claude/*.md` |
| "Przetestuj X", weryfikacja po bugfixie | **Tester** | — |
| "Posprzątaj kod", po sprincie | **Refaktor** | `.claude/conventions.md` |

Pełne profile agentów (triggery, procesy, checklisty): **`.claude/agents.md`**

## Kiedy czytać dodatkowe pliki

| Sytuacja | Plik |
|---|---|
| Modyfikujesz lub projektujesz system | `.claude/architecture.md` |
| Piszesz nowy skrypt C# | `.claude/conventions.md` |
| Tworzysz / przenosisz pliki lub foldery | `.claude/folder-structure.md` |
