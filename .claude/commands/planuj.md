---
description: Plan techniczny dla aktywnego zadania (etap 2, Architekt)
argument-hint: [ID zadania — domyślnie aktywne]
---
Etap 2 dla: $ARGUMENTS (puste = aktywne zadanie ze STATUS).

1. Warunek: specyfikacja ma status *Zatwierdzona* (ścieżka *refaktor* — wystarczy opis zadania). Jeśli nie — powiedz to i zatrzymaj się.
2. Zleć `architekt`: ID zadania, ID specyfikacji, ścieżka, pliki do przeczytania.
3. Zapisz zwrócony plan do `.claude/plans/<ID>.md`.
4. Przedstaw autorowi plan; wnioski Z13 — osobno i w całości.
5. ✋ Autor zatwierdza → STATUS (etap 2 → 3a) → `wydawca` tworzy gałąź → `programista`.
