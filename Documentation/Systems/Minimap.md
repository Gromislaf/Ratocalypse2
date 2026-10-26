# Minimapa

**Skrypty:** `Assets/Ratocalypse/Scripts/UI/MinimapController.cs`, `MinimapMarker.cs`  

---

## Markery

| Typ (MarkerType) | Kolor | Obiekt |
|---|---|---|
| `Player` | Biały | GameObject gracza |
| `NPC` | Żółty | Obiekty z NPCInteraction |
| `Enemy` | Czerwony | Prefab wroga |
| `QuestItem` | Złoty | ItemPickup questa |

---

## MIN-001 Rejestracja markerów

**Przebieg:** `MinimapMarker.OnEnable()` rejestruje się w `MinimapController.Instance`. `OnDisable()` wyrejestrowuje. Gdy wróg ginie i `Destroy(gameObject)` jest wywołane — marker automatycznie znika z mapy.

---

## MIN-002 Przeliczanie pozycji

Pozycja markera na minimapie przeliczana z pozycji świata na podstawie `worldMin`/`worldMax` (Vector2 — granice sceny):

```
uvX = (worldPos.x - worldMin.x) / (worldMax.x - worldMin.x)
uvY = (worldPos.z - worldMin.y) / (worldMax.y - worldMin.y)
```

Anchor markera: (0,0), pivot (0.5, 0.5) — ustawiany w kodzie.

---

## MIN-003 Obszar questa (QuestAreaCircle)

**Opis:** Zielony półprzezroczysty okrąg pokazujący obszar poszukiwań questa.

**Warunki aktywacji:** Subskrybuje `OnQuestAccepted { searchAreaCenter, searchAreaRadius }`.

**Przebieg:**
1. Przelicza `searchAreaRadius` (metry) na piksele UV minimapy wg `worldMin/worldMax`
2. Ustawia rozmiar `QuestAreaCircle` (RectTransform, Image Knob, ~30% alpha)
3. Ustawia pozycję na `searchAreaCenter`

**Warunki zakończenia:** `OnQuestCompleted` → QuestAreaCircle ukryty.
