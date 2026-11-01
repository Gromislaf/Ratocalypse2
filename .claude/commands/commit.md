---
description: Akceptacja i commit — skrypt kontroli, potem commit i scalenie do main po zgodzie autora (etapy 6+7)
---
Warunek: etap 5 zakończony (wynik `kronikarz`) i testy z etapu 4 bez FAIL. Jeśli nie — powiedz, czego brakuje, i zatrzymaj się.

1. Zaktualizuj STATUS (punkt kontrolny: etap 7; linia w dzienniku; przy XS/S to druga i ostatnia aktualizacja przed pushem) — **przed** kontrolą, żeby trafił do tego samego commita.
2. Uruchom skrypt kontroli narzędziem Bash: `pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/narzedzia/kontrola-commita.ps1`.
3. Pokaż autorowi wynik skryptu bez skracania + komunikat `<ID>: <opis>` (w treści: specyfikacje, numery zadań).
   ❌ → najpierw naprawa właściwym agentem, potem ponownie krok 2. ⚠️ „plik spoza planu” → zapytaj: przywrócić czy dopisać do planu.
4. ✋ „akceptuję, commit” → jedno wywołanie `wydawca` (krok „commit”) z dokładną listą plików ze skryptu i komunikatem.
5. Przypomnij, że push to osobny krok: `/push`.
