---
description: Wysłanie gałęzi funkcji i main na GitHuba po osobnej zgodzie autora (etap 8)
---
Warunek: commit i scalenie z `/commit` wykonane. Jeśli nie — powiedz to i zatrzymaj się.

1. Sam sprawdź, co zostanie wysłane: `git log --oneline origin/main..main` i `git status -sb`. Pokaż autorowi.
2. ✋ „tak” → jedno wywołanie `wydawca` (krok „push”, dokładnie: `git push -u origin <gałąź zadania>` i `git push origin main`).
3. Zamknij zadanie w STATUS: dziennik bramek → na koniec planu `.claude/plans/<ID>.md` (sekcja „Dziennik bramek”), wyczyść dziennik w STATUS,
   wpis w „Zamknięte” (hash scalenia), punkt kontrolny „brak aktywnego zadania” + 3 następne z kolejki.
4. Zaproponuj następne zadanie i `/clear` przed jego rozpoczęciem.
