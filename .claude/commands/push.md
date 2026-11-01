---
description: Wysłanie gałęzi funkcji i main na GitHuba — wpisanie /push jest zgodą autora (etap 8)
---
Wpisanie `/push` przez autora **jest** zgodą na push — nie pytaj drugi raz. Warunek: commit i scalenie z `/commit` wykonane; jeśli nie — powiedz to i zatrzymaj się.

1. Jedno wywołanie `wydawca` (krok „push”, dokładna nazwa gałęzi zadania). Wydawca sam pokazuje, co wysyła (`git log --oneline origin/main..main`).
2. Zamknij zadanie skryptem (Bash, dokładnie w tej postaci; hashe z wyników Wydawcy):
   `pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/narzedzia/zamknij-zadanie.ps1 -Zadanie <NN> -Commit "<hash commita> → merge <hash scalenia> → push"`
   Skrypt przenosi dziennik do planu, czyści dziennik i punkt kontrolny, usuwa zadanie z kolejki, dopisuje do „Zamknięte”. **Nie edytujesz STATUS ręcznie.**
3. Pokaż autorowi wynik Wydawcy (commity, hashe) i wynik skryptu w 2–4 liniach, zaproponuj następne zadanie i `/clear` przed jego rozpoczęciem.
