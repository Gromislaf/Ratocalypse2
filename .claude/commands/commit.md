---
description: Commit na gałęzi funkcji, potem scalenie do main — każdy krok po zgodzie autora (etap 7)
---
Warunek: etap 6 zakończony („ok” autora) dla aktywnego zadania. Jeśli nie — powiedz to i zatrzymaj się.

1. Zleć `wydawca`: propozycja commita (lista plików, kontrole `.meta`/wykluczeń/konfliktów, komunikat).
2. Przedstaw autorowi. ✋ „tak” → `wydawca` wykonuje commit na gałęzi funkcji.
3. Zapytaj o scalenie do `main`. ✋ „tak” → `wydawca` scala (`--no-ff`).
4. Zaktualizuj STATUS (etap 7). Przypomnij, że push to osobny krok: `/push`.
