# Rejestr zasad technicznych

> Stan obecny pozycji, do których odwołują się zasady w `.claude/rules/zasady-techniczne.md`.
> Edycja wyłącznie w celu wprowadzenia decyzji zatwierdzonej przez autora (Z13.5).

---

## Singletony (Z5)

| Klasa | Czas życia | `DontDestroyOnLoad` | Odpowiedzialność | Od |
|---|---|---|---|---|
| `GameManager` | cała gra | tak | stan gry, pauza, przejścia scen, dostęp do `PlayerStats` i `FMODEvents` | stan zastany 2026-10-26 |
| `AudioManager` | cała gra | tak | odtwarzanie FMOD, muzyka | stan zastany 2026-10-26 |
| `QuestManager` | scena | nie | stan questów | stan zastany 2026-10-26 |
| `SaveSystem` | scena | nie | zapis i wczytywanie | stan zastany 2026-10-26 |
| `MinimapController` | scena | nie | markery i obszar questa na minimapie | stan zastany 2026-10-26 |

---

## Strefy chronione (Z12)

| Strefa | Typ | Powód | Warunek wygaśnięcia |
|---|---|---|---|
| `Assets/Cartoon Heroes/` | zewnętrzna | paczka z Asset Store | — |
| `Assets/Modular Industrial Set/` | zewnętrzna | paczka z Asset Store | — |
| `Assets/Plugins/FMOD/` | zewnętrzna | wtyczka FMOD | — |
| `Assets/TextMesh Pro/` | zewnętrzna | zasoby dołączane przez Unity | — |
| `Assets/Ratocalypse/Scripts/Player/OcclusionFader.cs` | chroniona | 7+ iteracji do działania; wrażliwe filtry (wysokość, odległość XZ, kierunek) | wdrożenie płynnego zanikania (Shader Graph + MaterialPropertyBlock) |
| `PlayerAnimator.controller` | chroniona | działająca konfiguracja przejść (Any State → Attack, exit time) | po przeniesieniu do `Assets/Ratocalypse/Animations/` ochrona trwa do przeglądu przy pierwszej zmianie animacji gracza |
| `Scripts/Core/BenchmarkRunner.cs`, `BenchmarkInputRecorder.cs`, `PerformanceMonitor.cs`, `Scenes/Benchmark.unity` | chroniona **czasowo** | zmiana w trakcie serii pomiarów unieważnia wyniki | aktywna tylko w trakcie serii pomiarów; włączana i wyłączana wpisem w STATUS |

---

## Wyjątki od zasad

| Data | Zasada | Wyjątek | Uzasadnienie | Warunek wygaśnięcia |
|---|---|---|---|---|
| 2026-10-25 | Z2.1 | `PlayerStats` (SO) przechowuje stan runtime w polach `[NonSerialized]` | gra single player; reset w `OnEnable()` zapobiega zapisowi stanu do assetu; obowiązuje reguła jednego właściciela (Z2.4) | przegląd przy przekroczeniu progu Z7.4 albo przy drugiej postaci |

---

## Narzędzia MCP Unity (Z11.6, Z12)

Serwer: MCP for Unity (CoplayDev, `unity-mcp#v10.3.0`). Nazwy w Claude Code: `mcp__UnityMCP__<nazwa>`.
Zakazy wymusza `.claude/settings.json` (deny) i hook `.claude/hooks/pre-tool.ps1`.

| Grupa | Narzędzia | Dostęp |
|---|---|---|
| Zakaz | `execute_code`, `execute_custom_tool`, `execute_menu_item`, `batch_execute`, `create_script`, `delete_script`, `apply_text_edits`, `script_apply_edits`, `manage_script`, `manage_packages`, `manage_build`, `manage_physics`, `manage_graphics`, `manage_tools`, `set_active_instance`, `blender_bridge`, `generate_audio`, `generate_image`, `generate_model`, `import_model`, `import_model_file` | nikt |
| Odczyt | `find_gameobjects`, `find_in_file`, `read_console`, `validate_script`, `manage_script_capabilities`, `get_sha`, `get_test_job`, `unity_docs`, `unity_reflect`, `debug_request_context` | agenci analizujący i wykonawcy (wg definicji) |
| Kompilacja | `refresh_unity` | `programista`, `integrator` |
| Testy | `run_tests`, `get_test_job`, `manage_editor` (tylko play/pause/stop + odczyt), `manage_camera` (tylko screenshot + odczyt), `manage_profiler` | `tester` |
| Zapis sceny i assetów | `manage_scene`, `manage_gameobject`, `manage_components`, `manage_asset`, `manage_prefabs`, `manage_scriptable_object` | `integrator`; akcje zapisu tylko po `/manifest-ok` (hook); akcje odczytu (`get*`, `find*`, `list*`…) dla agentów z odczytem sceny |
| Wyłączone do wniosku | `manage_material`, `manage_shader`, `manage_texture`, `manage_animation`, `manage_ui`, `manage_vfx`, `manage_probuilder` | nikt; odblokowanie przez wniosek dla konkretnego zadania |

---

## Wtyczki i skille Claude Code (Z11.6)

Zainstalowane w zakresie użytkownika (stan na 2026-10-26). Blokady obowiązują w tym projekcie (hook `pre-tool.ps1`).

| Wtyczka | Status | Kto korzysta | Ograniczenia |
|---|---|---|---|
| `csharp-lsp` | dozwolona | `programista`, `architekt`, `inspektor`, `audytor`, `kronikarz` (narzędzie `LSP`) | — |
| Unity (`unity-agent-plugin`) | dozwolona częściowo | skille wiedzy wg definicji agentów (lista do uzupełnienia — zadanie 28) | **zablokowane:** `unity-cli`, `unity-package-management`, `new-unity-project`, skille miksera audio Unity (Z9). Bez Unity CLI i pakietu Pipeline w projekcie |
| `claude-md-management` | dozwolona | autor, Prowadzący — tylko audyt | zmiany w `CLAUDE.md` jak zmiana pliku zasad (Z13) |
| `document-skills` | dozwolona | Prowadzący (eksport dokumentacji), tor pracy inżynierskiej | — |
| `hookify` | planowana | etap automatów | do instalacji przez wniosek |
| Serena | wyłączona przez autora | — | — |
