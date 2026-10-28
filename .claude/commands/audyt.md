---
description: Pełny przegląd projektu — Redaktor (dokumentacja), Inspektor (kod vs zasady), Audytor (spójność). Na koniec fazy roadmapy i przed spotkaniem z promotorem
argument-hint: [zakres — domyślnie cały projekt]
---
Przegląd w zakresie: $ARGUMENTS (puste = cały projekt).

0. Odśwież mapę kodu: `pwsh -NoProfile -File .claude/narzedzia/mapa-kodu.ps1` — wszyscy trzej zaczynają od niej.
1. `redaktor` — `Documentation/` folder po folderze.
2. `inspektor` — tryb `/audyt`: `Assets/Ratocalypse/Scripts/` względem Z1–Z14 i konwencji (od mapy kodu).
3. `audytor` — spójność między źródłami (kod przez mapę kodu).
4. Zbierz wyniki w jedno zestawienie: 🔴/🟠/🟡, bez duplikatów, z proponowanym rozmiarem (S/M/L) dla każdej pozycji.
5. Przedstaw autorowi; zapytaj, co dopisać do kolejki i w jakiej kolejności. Dopisz do STATUS jednym zapisem.
