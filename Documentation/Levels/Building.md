# Poziom: Building (Akt 1 — Ruiny miasta)

**Scena Unity:** `Assets/Ratocalypse/Scenes/Building.unity`  

---

## Podstawowe informacje

| Pole | Wartość |
|---|---|
| Akt | 1 — Ruiny miasta |
| Typ | Eksploracja + walka + quest |

---

## Elementy sceny

<!-- TODO: uzupełnić po zmapowaniu sceny — pozycje, wymiary, bounds NavMesh -->

### Rozmieszczenie (do uzupełnienia)

- **Pozycja startowa gracza:** [TODO]
- **NPC z questem:** [TODO — pozycja, przypisany QuestData asset]
- **Obszar poszukiwań questa:** [TODO — searchAreaCenter, searchAreaRadius]
- **ItemPickup (przedmiot questa):** [TODO — pozycja, przypisany ItemData asset]
- **SavePoint:** [TODO — pozycja, savePointId]
- **Wrogowie:** [TODO — liczba, pozycje spawn, EnemyData asset]
- **Granice sceny NavMesh (worldMin/worldMax):** [TODO — do MinimapController]

---

## Warunki ukończenia poziomu

<!-- TODO: określić po rozbudowie questów o questy główne -->

---

## Ograniczenia przemieszczania

- Gracz ograniczony do siatki NavMesh
- Ściany z warstwą `Buildings` — nieprzekraczalne, ukrywane przez OcclusionFader gdy zasłaniają gracza
