# Kamera i okluzja

**Skrypty:** `CameraController.cs`, `CameraZoom.cs`, `OcclusionFader.cs`  


---

## CAM-001 CameraController

**Opis:** Główna kamera izometryczna gry — bez Cinemachine. Śledzi gracza przez Lerp, zoom scrollem myszy.

**Warunki aktywacji:** Aktywna przez cały czas rozgrywki.

**Parametry:**

| Parametr | Opis |
|---|---|
| Follow target | Transform gracza |
| Lerp speed | Płynne nadążanie za graczem |
| Zoom | Scroll myszy — zmiana wysokości kamery |

---

## CAM-002 CameraZoom (Cinemachine)

**Opis:** Zoom dla kamery opartej na CinemachineFollow. Skaluje `FollowOffset` zachowując kierunek.

**Zastosowanie:** Zachowany dla cutscen i Timeline. W rozgrywce aktywny jest CameraController.

---

## CAM-003 OcclusionFader

**Skrypt:** `Assets/Ratocalypse/Scripts/Player/OcclusionFader.cs`  
**Komponent na:** Main Camera  

**Opis:** Ukrywa ściany budynków zasłaniające gracza. Wyłącza zarówno Renderer jak i Collider — gracz może klikać przez ukrytą ścianę na ziemię za nią.

**Warunki aktywacji:** Co klatkę, gdy gracz jest w pobliżu budynków.

**Trzy filtry — element musi spełnić wszystkie:**

| Filtr | Opis |
|---|---|
| Wysokość Y | Tylko obiekty wyżej niż `hideAbovePlayerY` nad graczem |
| Odległość XZ | Tylko w promieniu `occlusionRadiusXZ` od gracza |
| Kierunek (dot product) | Tylko ściany po stronie kamery — próg `occlusionDotThreshold` |

**Wymagania sceny:**
- Warstwa `Buildings` przypisana do ścian budynków
- Main Camera Culling Mask zawiera `Buildings`

**Planowany upgrade:** Przejście z instant-hide na płynne fade (Shader Graph, Surface Transparent + `_Alpha` + MaterialPropertyBlock).
