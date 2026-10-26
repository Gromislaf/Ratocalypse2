# Ratocalypse 2.5 — Ogólna specyfikacja gry

## Podstawowe założenia

| Pole               | Wartość                                                      |
| ------------------ | ------------------------------------------------------------ |
| Tytuł              | Ratocalypse 2.5                                              |
| Gatunek            | Izometryczne RPG akcji                                       |
| Tryb               | Single player                                                |
| Perspektywa        | Kamera izometryczna, rzut ukośny                             |
| Platforma          | PC (Windows)                                                 |
| Silnik             | Unity 6000.4.3f1, URP 17.4.0                                 |
| Sterowanie         | Klawiatura i mysz (click-to-move)                            |
| Główny cel         | Eksploracja świata, walka z przeciwnikami, progresja postaci |
| Warunek zwycięstwa | Ukończenie głównego wątku fabularnego                        |
| Warunek porażki    | Spadek HP gracza do 0                                        |

## Setting i fabuła

**Rok 2087.** Katastrofa biologiczna skażiła środowisko. Szczury zmutowały i zorganizowały się w hierarchiczne klany. Ludzkość walczy o ostatnie schronienia — ruiny miast, podziemia, bunkry.

## Sterowanie

| Akcja | Klawisz |
|---|---|
| Ruch | LPM na ziemię |
| Atak | LPM na wroga |
| Interakcja (NPC / item) | LPM na obiekt |
| Unik | Shift + PPM |
| Ekwipunek | I |
| Drzewko umiejętności | C |

## Główna pętla rozgrywki

<!-- TODO: diagram pętli -->

1. **Eksploracja** — gracz porusza się po arenie, odkrywa obszary
2. **Walka** — wykrycie i eliminacja wrogów, zdobywanie XP
3. **Zbieranie** — podnoszenie przedmiotów, realizacja questów
4. **Progresja** — level up, wydawanie punktów umiejętności, ulepszanie ekwipunku

Przejście między etapami: wrogowie spawned w obszarach, quest wymaga znalezienia itemu → wróć do NPC → XP reward → level up.

**Status:** ✅ Zaimplementowane (Akt 1 — Building.unity)
