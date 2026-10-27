---
description: Specyfikacja mechaniki lub elementu projektu — wywiad z autorem, szkic, Redaktor, akceptacja (etap 1)
argument-hint: <temat lub ID, np. GDD-CMB-001>
---
Etap 1 cyklu dla: $ARGUMENTS

0. Jeśli zadanie nie ma jeszcze gałęzi — `wydawca` tworzy ją teraz (wymaga czystego drzewa; przy zmianach — zatrzymaj się i zapytaj autora).

1. Ustal plik i ID wg `.claude/zasady/dokumentacja-rejestr.md` (nowy element → kolejny numer w prefiksie; brak pasującego folderu/prefiksu → wniosek §7.8).
2. Wybierz szablon: §3a (mechanika gry), §3b (architektura), §3c (optymalizacja) z `.claude/rules/dokumentacja-promotor.md`.
3. Wywiad z autorem pole po polu, partiami po 2–4 pytania. Brak wartości → zaproponuj 2–3 opcje z uzasadnieniem; decyzja autora.
   Jeśli element ma kanon w `Vision/Content/` — oprzyj się na nim.
4. Zapisz szkic w `Documentation/` ze statusem *Projekt*.
5. `redaktor` → popraw braki z autorem → przedstaw całość.
6. ✋ Autor zatwierdza → status *Zatwierdzona*, wpis w STATUS (etap 1 → 2), w historii zmian specyfikacji.
