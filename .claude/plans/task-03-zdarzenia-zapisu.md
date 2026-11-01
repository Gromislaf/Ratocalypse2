# Plan: #3 — Naprawa przepływu zdarzeń SavePoint / GameManager

**Rozmiar:** S  
**Zasady:** Z1  
**Gałąź:** feature/03-zdarzenia-zapisu

## Problem

Dwa zdarzenia zapisu były niespójne:
- `OnCheckpointReached` — subskrybent: `GameManager`, brak publishera
- `OnGameSaved` — publisher: `SaveSystem`, brak subskrybenta
- `SaveSystem.md` SAV-001 opisywał zły sygnał informacji zwrotnej

## Decyzja autora

`SavePoint.Interact()` publikuje `OnCheckpointReached` (powiadomienie: gracz dotarł do checkpointu), potem wywołuje `SaveSystem.Save()`. `GameManager` zostaje z samym `Debug.Log` — reakcje (audio, UI, punkt odrodzenia) to osobne zadania z kolejki.

## Zmiany w kodzie

| Plik | Zmiana |
|---|---|
| `Assets/Ratocalypse/Scripts/Interactable/SavePoint.cs` | `Interact()`: dodano `EventBus.Publish(new OnCheckpointReached { checkpointId = savePointId })` przed `Save()`; zaktualizowano komentarz nagłówkowy |
| `Assets/Ratocalypse/Scripts/Core/GameManager.cs` | `HandleCheckpointReached`: usunięto zakomentowaną linię `SaveSystem.Instance.Save(...)`, dodano komentarz `// powiadomienie; zapis wykonuje SavePoint.Interact` |

## Dokumentacja

`Documentation/Systems/SaveSystem.md` SAV-001 — naprawiony opis przepływu, wersja 0.2, status Zaimplementowana.

## Dziennik bramek

| Data | Etap | Zdarzenie |
|---|---|---|
| 2026-11-01 | 0 | Zadanie aktywne, rozmiar S zatwierdzony |
| 2026-11-01 | 2 | Plan zatwierdzony przez autora |
| 2026-11-01 | 3a | Programista — implementacja |
| 2026-11-01 | 4 | Inspektor PASS; Tester T1+T2 PASS |
| 2026-11-01 | 5 | Kronikarz — SAV-001 v0.2 |
| 2026-11-01 | 6+7 | Commit `c74957a` → merge `496902a` |
| 2026-11-01 | 8 | Push `main` — OK |
