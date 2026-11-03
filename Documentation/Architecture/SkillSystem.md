# System umiejętności — architektura

---

## GDD-ARC-001 System definicji umiejętności (ScriptableObject)

**Status:** Zaimplementowana
**Wersja:** 0.1

**Cel:** Zastąpienie hard-coded `struct SkillDefinition` i statycznej klasy `SkillDatabase` trwałymi assetami ScriptableObject, zgodnie z Z2.1 (definicje w SO, tylko do odczytu w runtime, trwały unikalny ID).

**Odpowiedzialność:**
- Za: trwałe przechowywanie definicji umiejętności jako assety Unity; rejestr dostępny przez Inspector; wyszukiwanie po ID.
- NIE za: logikę odblokowania, stan odblokowanych skillów gracza, punkty umiejętności.

**Elementy:**
| Klasa / plik | Rola |
|---|---|
| `SkillDefinitionSO` | ScriptableObject — definicja jednej umiejętności (8 pól) |
| `SkillRegistrySO` | ScriptableObject — lista wszystkich `SkillDefinitionSO`; wyszukiwanie po ID |
| `SkillTreeManager` (zaktualizowany) | Konsumuje `SkillRegistrySO` przez `[SerializeField]`; usuwa zależność od `SkillDatabase` |
| `SkillNodeUI` (zaktualizowany) | Przyjmuje `SkillDefinitionSO` zamiast `in SkillDefinition` |

**Kontrakt (API):**
```csharp
// SkillDefinitionSO : ScriptableObject
public string Id { get; }
public string DisplayName { get; }
public string Description { get; }
public SkillBranch Branch { get; }
public int Tier { get; }
public SkillEffectType EffectType { get; }
public float EffectValue { get; }
public SkillDefinitionSO Prerequisite { get; }  // null dla Tier 1

// SkillRegistrySO : ScriptableObject
public IReadOnlyList<SkillDefinitionSO> All { get; }
public SkillDefinitionSO GetById(string id);
```

**Zasady użycia:**
1. Pola `SkillDefinitionSO` są read-only w runtime (Z2.1).
2. `Id` jest trwały i nie zmienia się między wersjami assetów — jest kluczem w zapisie gry.
3. `SkillTreeManager` otrzymuje rejestr przez `[SerializeField] SkillRegistrySO _registry` — bez singletona (Z5).
4. Zapis stanu gracza (`SaveData.unlockedSkills`) pozostaje `List<string>` z wartościami `Id` — kompatybilność wsteczna zachowana.
5. `SkillDatabase` zostaje usunięta.

**Parametry i ograniczenia:**
| Parametr | Wartość | Jednostka |
|---|---|---|
| Liczba umiejętności | 12 | szt. |
| Gałęzie | 3 (Warrior, Hunter, Survivor) | szt. |
| Tiery na gałąź | 4 | szt. |

**Interakcje:**
- [GDD-SKL] `Systems/SkillTree.md` — reguły odblokowania używają `SkillDefinitionSO.Prerequisite` i `Id`
- `Systems/SaveSystem.md` — `SaveData.unlockedSkills List<string>` przechowuje `Id` skillów; `RestoreFromSave` wyszukuje SO przez `SkillRegistrySO.GetById`

**Sytuacje wyjątkowe:**
- `GetById(id)` gdy ID nie ma w rejestrze → zwraca `null`; `SkillTreeManager.CanUnlock` traktuje `null` jak brak prereq (bezpieczny fallback dla Tier 1)
- `Prerequisite == null` → Tier 1, brak wymagań wstępnych (wariant normalny)

**Decyzje projektowe:**
| Decyzja | Rozważane alternatywy | Uzasadnienie |
|---|---|---|
| `Prerequisite` jako referencja SO | `string prerequisiteId` | Referencja SO eliminuje możliwość literówki w ID; Unity serializuje i waliduje referencję w edytorze |
| Rejestr przez `[SerializeField]` w `SkillTreeManager` | Singleton z rejestru Z5 | Prostsze, Z5-compliant bez wniosku; SkillTreeManager jest MonoBehaviour — pole Inspektora naturalne |
| Usunięcie `SkillDatabase` | Adapter wrapping SO | Po migracji klasa staje się martwym kodem; brak wartości w utrzymaniu |

**Implementacja:**
- `Assets/Ratocalypse/Scripts/Core/SkillDefinitionSO.cs` (nowy; zawiera też `SkillEffectType` i `SkillBranch`)
- `Assets/Ratocalypse/Scripts/Core/SkillRegistrySO.cs` (nowy)
- `Assets/Ratocalypse/Scripts/Player/SkillTreeManager.cs` (zaktualizowany; pole `[SerializeField] SkillRegistrySO _registry`, getter `Registry`)
- `Assets/Ratocalypse/Scripts/UI/SkillNodeUI.cs` (zaktualizowany; przyjmuje `SkillDefinitionSO`)
- `Assets/Ratocalypse/Scripts/UI/SkillTreeUI.cs` (zaktualizowany; iteracja przez `SkillTreeManager.Registry.All`)
- `Assets/Ratocalypse/Scripts/Core/SkillDefinition.cs` (usunięty)
- Assety SO: `Assets/Ratocalypse/Data/Skills/` (12 × `SkillDefinitionSO`, 1 × `SkillRegistry.asset`)
- Data implementacji: 2026-11-03 | Gałąź: `feature/12-skille-so`

**Historia zmian:**
| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. Refaktor #12 — struct/SkillDatabase → ScriptableObject (Z2.1). |
