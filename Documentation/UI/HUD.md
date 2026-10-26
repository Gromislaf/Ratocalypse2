# HUD gracza

**Skrypty:** `Assets/Ratocalypse/Scripts/UI/PlayerHUD.cs`, `XPBarUI.cs`  

---

## Pasek HP

| Cecha | Zachowanie |
|---|---|
| Widoczność | Zawsze podczas rozgrywki |
| Wartość | `currentHp / maxHp` |
| Typ | Pionowy `Image.fillAmount` (HealthFull nad HealthEmpty) |
| Aktualizacja | Subskrybuje `OnPlayerDamaged`, `OnPlayerHealed` |
| Inicjalizacja | `Start()` — czyta z PlayerStats, ustawia pasek do właściwego stanu |

---

## Pasek Staminy

| Cecha | Zachowanie |
|---|---|
| Widoczność | Zawsze podczas rozgrywki |
| Wartość | `currentStamina / maxStamina` |
| Typ | Poziomy `Image.fillAmount` |
| Aktualizacja | Subskrybuje `OnPlayerStaminaChanged` |

---

## Pasek XP

| Cecha | Zachowanie |
|---|---|
| Widoczność | Zawsze podczas rozgrywki |
| Wartość | `currentXP / xpToNextLevel` |
| Typ | Poziomy `Image.fillAmount` + tekst poziomu |
| Aktualizacja | Subskrybuje `OnPlayerXPGained` |
| Level up | Aktywuje GameObject "LEVEL UP!" na `levelUpDisplayTime` sekund |
| Inicjalizacja | `Start()` — czyta z PlayerStats |
