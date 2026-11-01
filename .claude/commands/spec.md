---
description: Specyfikacja mechaniki lub elementu projektu — wywiad z autorem sekcja po sekcji, Redaktor, akceptacja (etap 1)
argument-hint: <temat lub ID, np. GDD-CMB-001>
---
Etap 1 cyklu dla: $ARGUMENTS

0. **Gałąź najpierw.** `git branch --show-current` — jeśli `main`, przed czymkolwiek innym `wydawca` (krok „gałąź”, dokładna nazwa `feature/<NN>-<nazwa>`).
   Hook blokuje zapis w `Documentation/` na `main`.
1. **Zwiad zamiast czytania.** Jedno wywołanie `zwiadowca` z pytaniami: co robi teraz kod dotknięty zadaniem (pliki, metody, linie),
   które elementy dokumentacji już to opisują (ID, linie), w jakim pliku i z jakim ID zapisać nowy element (wg rejestru dokumentacji;
   brak pasującego folderu/prefiksu → wniosek §7.8). Sam nie czytasz kodu, rejestru ani wytycznych promotora.
   Wynik zwiadu przekażesz później Architektowi w zleceniu (nie każ mu szukać tego od nowa).
2. **Szablon** — pola według typu (pełne wytyczne czyta tylko Redaktor):
   - mechanika (§3a): Opis funkcjonalny · Warunki uruchomienia · Zasady działania · Ograniczenia · Parametry · Wzory · Interakcje · Sytuacje wyjątkowe · Informacja zwrotna · Implementacja · Historia zmian;
   - architektura `ARC` (§3b): Cel · Odpowiedzialność · Elementy · Kontrakt · Zasady użycia · Parametry i ograniczenia · Interakcje · Sytuacje wyjątkowe · Decyzje projektowe · Implementacja · Historia;
   - optymalizacja `OPT` (§3c): Problem · Hipoteza · Zmiana · Scenariusz · Wyniki (z plikiem źródłowym) · Wnioski · Implementacja · Historia.
   Precyzja: każda liczba z jednostką, bez „szybko / dużo / czasem”; parametry w tabelach; co najmniej jedna sytuacja wyjątkowa.
3. Zapisz **szkielet** w `Documentation/`: nagłówek `## <ID> <nazwa>`, status *Projekt*, wszystkie pola szablonu, puste treści.
4. **Sekcja po sekcji:** pytania partiami po 2–4 z rekomendacjami → zgoda autora → sekcja od razu zapisana w pliku.
   Brak wartości → 2–3 opcje z uzasadnieniem; decyzja autora. Kanon w `Vision/Content/` — oprzyj się na nim (pytaj zwiadowcę, nie czytaj sam).
5. `redaktor` (konkretne ID, „nowa specyfikacja”) → popraw braki z autorem.
6. ✋ Autor zatwierdza (przy zadaniu M razem z planem) → status *Zatwierdzona*, wpis w historii zmian specyfikacji, punkt kontrolny.
   Przy M/L po tej bramce: plan zapisany, punkt kontrolny gotowy do wznowienia → zaproponuj `/clear` (styl Prowadzącego, „Podział sesji”).
