# PreToolUse: kontrole, których nie da się wyrazić samymi uprawnieniami.
#  G. Bash/PowerShell: git commit/merge/push wykonuje wyłącznie wydawca.
#  0. Read: pliki zasad i dokumentacji > 8 KB tylko fragmentami (lista nagłówków w odpowiedzi).
#  1. Skille Unity kolidujące z zasadami (Z9, Z11, Z12) -> zablokowane.
#  2. MCP Unity: akcje zapisu w narzędziach wielofunkcyjnych tylko po /manifest-ok (i tylko dla integratora, jeśli znana nazwa agenta);
#     manage_editor tylko play/pause/stop; manage_camera tylko screenshot.
#  3. Zapis plików: UnityEditor poza #if UNITY_EDITOR w kodzie gry; blokada systemu pomiarowego w trakcie serii; ścieżki zapisu per agent (jeśli hook zna nazwę agenta).
# Wyjście z kodem 2 = blokada, komunikat na stderr trafia do agenta.
$ErrorActionPreference = 'SilentlyContinue'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$in    = [Console]::In.ReadToEnd() | ConvertFrom-Json
$tool  = "$($in.tool_name)"
$ti    = $in.tool_input
$agent = "$($in.agent_type ?? $in.subagent_type ?? $in.agent_name)".ToLower()
$root  = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }
$stan  = Join-Path $root '.claude/stan'

function Deny([string]$msg) { [Console]::Error.WriteLine("[hook] ZABLOKOWANE: $msg"); exit 2 }

function ManifestOpen {
    $lock = Join-Path $stan 'manifest-ok.json'
    if (-not (Test-Path $lock)) { return $false }
    $j = Get-Content $lock -Raw | ConvertFrom-Json
    return ([datetime]$j.expires -gt (Get-Date))
}

# --- G. Git: commit / merge / push tylko przez wydawcę -------------------------
if ($tool -in @('Bash','PowerShell')) {
    $cmd = "$($ti.command)"
    if ($cmd -notmatch '\bgit\b') { exit 0 }
    if ($cmd -match '\bgit\b[^|;&\n]*?\s(commit|merge|push|rebase)(\s|$)') {
        $op = $Matches[1]
        if ($agent -ne 'wydawca') {
            $kto = if ($agent) { "agent '$agent'" } else { 'główna sesja (Prowadzący)' }
            Deny "git $op wykonuje wyłącznie wydawca — $kto tego nie robi. Przekaż zlecenie wydawcy (jeden krok, dokładna lista plików i komunikat)."
        }
    }
    exit 0
}

# --- 0. Strażnik czytania: duże pliki zasad i dokumentacji tylko fragmentami ---------
if ($tool -eq 'Read') {
    $path = "$($ti.file_path)"
    if (-not $path -or $null -ne $ti.offset -or $null -ne $ti.limit) { exit 0 }
    $rel = $path.Replace('\','/')
    $r   = $root.Replace('\','/').TrimEnd('/') + '/'
    if ($rel.StartsWith($r, [StringComparison]::OrdinalIgnoreCase)) { $rel = $rel.Substring($r.Length) }
    if ($rel -notmatch '^(\.claude/(zasady|rules)/|Documentation/|Vision/)' -or $rel -notmatch '\.md$') { exit 0 }
    if (-not (Test-Path $path -PathType Leaf)) { exit 0 }
    $kb = [math]::Ceiling((Get-Item $path).Length / 1KB)
    if ($kb -le 8) { exit 0 }
    $nr = 0; $naglowki = [System.Collections.Generic.List[string]]::new()
    foreach ($l in [System.IO.File]::ReadLines($path)) {
        $nr++
        if ($l -match '^#{1,4} ' -and $naglowki.Count -lt 40) { $naglowki.Add(('{0,5}  {1}' -f $nr, $l)) }
    }
    [Console]::Error.WriteLine("[hook] Odmowa czytania w całości: $rel ma $kb KB (próg 8 KB dla zasad i dokumentacji). Przeczytaj potrzebną sekcję: Read z offset = linia nagłówka i limit do następnego nagłówka.")
    [Console]::Error.WriteLine("Nagłówki (linia  tytuł):")
    foreach ($n in $naglowki) { [Console]::Error.WriteLine($n) }
    [Console]::Error.WriteLine("Całość tylko z uzasadnieniem — czytaj zakresami (offset/limit).")
    exit 2
}

# --- 1. Skille ---------------------------------------------------------------
if ($tool -eq 'Skill') {
    $name = "$($ti.skill ?? $ti.name ?? $ti.command)".ToLower()
    if ($name -match '(^|:)(unity-cli|unity-package-management|new-unity-project)$' -or $name -match 'mixer' -or $name -match '(^|:)unity-mcp-skill$') {
        Deny "skill '$name' jest wyłączony w tym projekcie (rejestr wtyczek, Z9/Z11/Z12)."
    }
    exit 0
}

# --- 2. MCP Unity --------------------------------------------------------------
if ($tool -like 'mcp__UnityMCP__*') {
    $short  = $tool.Substring('mcp__UnityMCP__'.Length)
    $action = "$($ti.action)".ToLower()
    $isRead = $action -match '^(get|find|search|list|read|inspect|info|describe|query|exists|validate)'

    $hardDeny = @('execute_code','execute_custom_tool','execute_menu_item','batch_execute','create_script','delete_script',
                  'apply_text_edits','script_apply_edits','manage_script','manage_packages','manage_build','manage_physics',
                  'manage_graphics','manage_tools','set_active_instance','blender_bridge','generate_audio','generate_image',
                  'generate_model','import_model','import_model_file','manage_material','manage_shader','manage_texture',
                  'manage_animation','manage_ui','manage_vfx','manage_probuilder')
    if ($hardDeny -contains $short) { Deny "narzędzie MCP '$short' jest wyłączone (rejestr narzędzi MCP)." }

    if ($short -eq 'manage_editor') {
        if ($isRead -or $action -in @('play','pause','stop')) { exit 0 }
        Deny "manage_editor: dozwolone tylko play/pause/stop i odczyt (akcja '$action')."
    }
    if ($short -eq 'manage_camera') {
        if ($isRead -or $action -match 'screenshot') { exit 0 }
        Deny "manage_camera: dozwolony tylko zrzut ekranu i odczyt (akcja '$action')."
    }

    $writeTools = @('manage_scene','manage_gameobject','manage_components','manage_asset','manage_prefabs','manage_scriptable_object')
    if ($writeTools -contains $short -and -not $isRead) {
        if (-not (ManifestOpen)) { Deny "zapis '$short/$action' wymaga zatwierdzonego manifestu — autor musi wpisać /manifest-ok." }
        if ($agent -and $agent -ne 'integrator') { Deny "zapis sceny i assetów wykonuje wyłącznie integrator (agent: '$agent')." }
    }
    exit 0
}

# --- 3. Zapis plików -----------------------------------------------------------
if ($tool -in @('Edit','Write','MultiEdit','NotebookEdit')) {
    $path = "$($ti.file_path ?? $ti.notebook_path)"
    $rel  = $path.Replace('\','/')
    $r    = $root.Replace('\','/').TrimEnd('/') + '/'
    if ($rel.StartsWith($r, [StringComparison]::OrdinalIgnoreCase)) { $rel = $rel.Substring($r.Length) }

    # UnityEditor w kodzie gry bez #if UNITY_EDITOR — psuje build (sprawdzane tylko w zmienianym tekście)
    if ($rel -match '^Assets/Ratocalypse/.*\.cs$' -and $rel -notmatch '/Editor/') {
        $wynik = $null; $nowy = ''
        if ($tool -eq 'Write') { $wynik = "$($ti.content)"; $nowy = $wynik }
        elseif (Test-Path $path -PathType Leaf) {
            $wynik = [System.IO.File]::ReadAllText($path)
            $zmiany = if ($tool -eq 'MultiEdit') { @($ti.edits) } else { @($ti) }
            foreach ($e in $zmiany) {
                $nowy += "`n$($e.new_string)"
                $o = "$($e.old_string)"
                if ($o -and $wynik.Contains($o)) {
                    if ($e.replace_all) { $wynik = $wynik.Replace($o, "$($e.new_string)") }
                    else { $ix = $wynik.IndexOf($o); $wynik = $wynik.Substring(0, $ix) + "$($e.new_string)" + $wynik.Substring($ix + $o.Length) }
                }
            }
        }
        if ($wynik -and $nowy -match 'UnityEditor') {
            $zm = [System.Collections.Generic.HashSet[string]]::new()
            foreach ($l in ($nowy -split "`r?`n")) { if ($l.Trim()) { [void]$zm.Add($l.Trim()) } }
            $stos = [System.Collections.Generic.List[object]]::new(); $nr = 0
            foreach ($l in ($wynik -split "`r?`n")) {
                $nr++; $t = $l.Trim()
                if ($t -match '^#if\s+(.*)$') { $w = $Matches[1]; $stos.Add(@{ e = $w; on = ($w -match 'UNITY_EDITOR' -and $w -notmatch '!\s*UNITY_EDITOR') }); continue }
                if ($t -match '^#elif\s+(.*)$' -and $stos.Count) { $w = $Matches[1]; $stos[$stos.Count - 1].on = ($w -match 'UNITY_EDITOR' -and $w -notmatch '!\s*UNITY_EDITOR'); continue }
                if ($t -match '^#else\b' -and $stos.Count) { $g = $stos[$stos.Count - 1]; $g.on = ($g.e -match '!\s*UNITY_EDITOR'); continue }
                if ($t -match '^#endif\b' -and $stos.Count) { $stos.RemoveAt($stos.Count - 1); continue }
                $k = $t -replace '//.*$', ''
                if ($k -match '\bUnityEditor\b' -and $zm.Contains($t)) {
                    $chronione = $false; foreach ($g in $stos) { if ($g.on) { $chronione = $true } }
                    if (-not $chronione) { Deny "linia $nr w '$rel' używa UnityEditor poza #if UNITY_EDITOR — w zbudowanej grze tej biblioteki nie ma (błąd buildu). Otocz fragment #if UNITY_EDITOR … #endif albo przenieś kod do Assets/Ratocalypse/Editor/." }
                }
            }
        }
    }

    if (Test-Path (Join-Path $stan 'pomiary.json')) {
        if ($rel -match '(BenchmarkRunner|BenchmarkInputRecorder|PerformanceMonitor)\.cs$' -or $rel -match 'Scenes/Benchmark\.unity$') {
            Deny "trwa seria pomiarów — '$rel' jest chroniony czasowo (Z12). Zakończ serię: /pomiary stop."
        }
    }

    # Testy automatyczne zablokowane do zadania #26 (konwencje §12) — dla wszystkich. Usunąć ten blok w zadaniu #26.
    if ($rel -match '^Assets/Ratocalypse/Tests/') {
        Deny "do zadania #26 nie piszemy testów automatycznych (konwencje §12) — tester działa w trybie ręcznym (scenariusze + logi [DBG])."
    }

    # Główna sesja nie edytuje kodu gry — robi to programista
    if (-not $agent -and $rel -match '^Assets/Ratocalypse/(Scripts|Editor)/.*\.cs$') {
        Deny "Prowadzący nie edytuje kodu — zleć to programiście."
    }

    if ($agent) {
        $allowed = @{
            'programista' = @('Assets/Ratocalypse/Scripts/','Assets/Ratocalypse/Editor/')
            'tester'      = @()   # do #26 bez zapisu; potem: 'Assets/Ratocalypse/Tests/'
            'kronikarz'   = @('Documentation/','Vision/GDD.md','.claude/zasady/dokumentacja-rejestr.md')
        }
        if ($allowed.ContainsKey($agent)) {
            $ok = $false
            foreach ($pre in $allowed[$agent]) { if ($rel.StartsWith($pre, [StringComparison]::OrdinalIgnoreCase)) { $ok = $true } }
            if (-not $ok) { Deny "agent '$agent' nie może zapisywać w '$rel'." }
        }
        elseif ($agent -in @('architekt','redaktor','inspektor','audytor','adwokat','wydawca','integrator')) {
            Deny "agent '$agent' nie zapisuje plików."
        }
    }
    exit 0
}
exit 0
