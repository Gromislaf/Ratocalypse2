# PreToolUse: kontrole, których nie da się wyrazić samymi uprawnieniami.
#  1. Skille Unity kolidujące z zasadami (Z9, Z11, Z12) -> zablokowane.
#  2. MCP Unity: akcje zapisu w narzędziach wielofunkcyjnych tylko po /manifest-ok (i tylko dla integratora, jeśli znana nazwa agenta);
#     manage_editor tylko play/pause/stop; manage_camera tylko screenshot.
#  3. Zapis plików: blokada systemu pomiarowego w trakcie serii; ścieżki zapisu per agent (jeśli hook zna nazwę agenta).
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

# --- 1. Skille ---------------------------------------------------------------
if ($tool -eq 'Skill') {
    $name = "$($ti.skill ?? $ti.name ?? $ti.command)".ToLower()
    if ($name -match '(^|:)(unity-cli|unity-package-management|new-unity-project)$' -or $name -match 'mixer') {
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

    if (Test-Path (Join-Path $stan 'pomiary.json')) {
        if ($rel -match '(BenchmarkRunner|BenchmarkInputRecorder|PerformanceMonitor)\.cs$' -or $rel -match 'Scenes/Benchmark\.unity$') {
            Deny "trwa seria pomiarów — '$rel' jest chroniony czasowo (Z12). Zakończ serię: /pomiary stop."
        }
    }

    if ($agent) {
        $allowed = @{
            'programista' = @('Assets/Ratocalypse/Scripts/','Assets/Ratocalypse/Editor/')
            'tester'      = @('Assets/Ratocalypse/Tests/')
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
