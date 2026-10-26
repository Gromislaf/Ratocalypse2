# Audio (FMOD)

**Skrypty:** `Assets/Ratocalypse/Scripts/Core/AudioManager.cs`, `FMODEvents.cs`  
**Status:** ✅ Zaimplementowane

---

## Zasada

Całe audio przez FMOD Studio. `AudioSource` ani `AudioClip` nie są używane nigdzie w projekcie. Dostęp przez `GameManager.FMODEvents` (przypisany w Inspektorze — nie `Resources.Load`).

---

## Metody AudioManager

| Metoda | Kiedy używać |
|---|---|
| `PlayOneShot(ref, pos)` | Krótki dźwięk 3D (ataki, footstepy, pickup) |
| `PlayOneShot(ref)` | Krótki dźwięk 2D bez pozycji (UI) |
| `CreateInstance(ref)` | Pętle z kontrolą (muzyka, ambient) — wymaga `Release()` |
| `CreateAndAttach(ref, transform)` | Dźwięk przywiązany do obiektu (wróg, ambient punktowy) |
| `PlayMusic(ref)` | Muzyka — auto-stop poprzedniej + fadeout |

**Alternatywa:** `StudioEventEmitter` — komponent bezpośrednio na GameObject dla dźwięków przestrzennych.

---

## Katalog zdarzeń FMOD (FMODEvents.cs)

<!-- TODO: uzupełnić event paths po konfiguracji FMOD Studio -->

| Kategoria | Zdarzenia |
|---|---|
| Ruch gracza | Footstepy |
| Walka gracza | Atak, trafienie krytyczne, unik |
| Walka wrogów | Atak wroga, śmierć wroga |
| Muzyka | Eksploracja, walka, boss |
| Ambient | Miasto (Akt 1), Kanały (Akt 2), Gniazdo (Akt 3) |
| Status efekty | Trucizna, ogłuszenie |
| UI | Klik menu, toggle ekwipunku, level up |
