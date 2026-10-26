# Ruch gracza i unik

**Skrypt:** `Assets/Ratocalypse/Scripts/Player/PlayerController.cs`  

---

## MOV-001 Ruch click-to-move

**Opis:** Gracz porusza się klikając LPM na cel w scenie. Ruch realizowany przez NavMeshAgent.

**Warunki aktywacji:** Kliknięcie LPM poza UI, gdy `EventSystem.IsPointerOverGameObject()` == false.

**Priorytety kliknięcia:**

| Warstwa | Efekt |
|---|---|
| Warstwa `Interactable` lub `NPC` | Gracz podchodzi i wywołuje `IInteractable.Interact()` |
| Warstwa `EnemyLayer` | Ustawia `CurrentTarget`, gracz podchodzi i atakuje |
| Warstwa `GroundLayer` | Gracz idzie do punktu, `CurrentTarget` czyszczony |

**Parametry:**

| Parametr | Wartość | Źródło |
|---|---|---|
| Prędkość ruchu | `PlayerStats.TotalMoveSpeed` | PlayerStats SO |
| Zasięg interakcji | 2 m (`interactRange`) | PlayerController |
| Zasięg ataku | 1.5 m (`attackRange`) | PlayerController |

**Interakcje:** `PlayerStats.TotalMoveSpeed` aktualizowany po equip i level up → `NavMeshAgent.speed`.

**Informacja zwrotna:** Animator parameter `Speed = velocity.magnitude / TotalMoveSpeed` → BlendTree Idle/Walk.

---

## MOV-002 Unik (Dodge)

**Warunki aktywacji:** Shift + PPM, gdy postać posiada co najmniej 25 pkt staminy.

**Parametry:**

| Parametr | Wartość |
|---|---|
| Koszt staminy | 25 pkt |
| Prędkość uniku | 12 m/s |
| Czas trwania | 0.3 s |

**Przebieg działania:**
1. Odejmuje 25 pkt staminy, włącza iframes (gracz nie otrzymuje obrażeń)
2. Wyłącza NavMesh — ruch bezpośrednio przez Transform
3. Gracz przesuwa się przez 0.3 s z prędkością 12 m/s w kierunku aktualnego ruchu (lub do przodu jeśli stoi)
4. NavMesh.SamplePosition — gracz wraca na siatkę NavMesh
5. Iframes wyłączone, NavMesh wznowiony

**Warunki zakończenia:** Upływ 0.3 s lub powrót na siatkę NavMesh.

**Ograniczenia:** Unik niedostępny gdy stamina < 25 pkt.

**Interakcje:** Stamina zużyta → publikuje `OnPlayerStaminaChanged` → HUD aktualizuje pasek staminy.

**Przypadek brzegowy:** Jeśli po uniku gracz wyląduje poza NavMesh — `SamplePosition` w promieniu 2 m przywraca go na siatkę.

**Informacja zwrotna dla gracza:** Animator trigger `Dodge`, pasek staminy spada w HUD.

---

## MOV-003 Obrót gracza

**Opis:** Gracz obraca się płynnie przez `Quaternion.Slerp` z prędkością 15/s.

**Wyjątki:** Podczas uniku i gdy ma `CurrentTarget` — obraca się w stronę celu, nie w kierunku ruchu.
