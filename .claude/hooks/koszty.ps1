# koszty.ps1 — hook SubagentStop i Stop: zapisuje zużycie tokenów do .claude/stan/ (bez udziału agentów).
#   SubagentStop -> dopisuje wiersz do koszty-agenci.csv (jeden wiersz na wywołanie subagenta)
#   Stop         -> aktualizuje wiersz sesji w koszty-sesje.csv (główna rozmowa, nadpisywany po każdej turze)
# Podsumowanie: /koszty (skrypt .claude/narzedzia/koszty.ps1).
# Równoważnik = tokeny przeliczone na „tokeny wejścia”: wejście×1 + zapis cache×1,25 + odczyt cache×0,1 + wyjście×5 (proporcje cennika Anthropic).
$ErrorActionPreference = 'SilentlyContinue'
$in   = [Console]::In.ReadToEnd() | ConvertFrom-Json
$root = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }
$stan = Join-Path $root '.claude/stan'
New-Item -ItemType Directory -Force -Path $stan | Out-Null
$zdarzenie = "$($in.hook_event_name)"

function Podsumuj([string]$plik) {
    if (-not $plik -or -not (Test-Path $plik)) { return $null }
    if ((Get-Item $plik).Length -gt 60MB) { return $null }
    $wiad = @{}; $narz = 0; $pierwszy = $null; $ostatni = $null; $model = ''
    foreach ($l in [System.IO.File]::ReadLines($plik)) {
        if ($l -notmatch '"type":"assistant"' -or $l -notmatch '"usage"') { continue }
        $o = $l | ConvertFrom-Json
        if (-not $o.message.usage) { continue }
        if ($o.message.model -eq '<synthetic>') { continue }
        $id = if ($o.message.id) { $o.message.id } else { $o.uuid }
        $wiad[$id] = $o.message.usage
        if ($o.message.model) { $model = $o.message.model }
        $narz += ([regex]::Matches($l, '"type":"tool_use"')).Count
        if ($o.timestamp) { if (-not $pierwszy) { $pierwszy = [datetime]$o.timestamp }; $ostatni = [datetime]$o.timestamp }
    }
    if ($wiad.Count -eq 0) { return $null }
    $we = 0; $zc = 0; $oc = 0; $wy = 0; $max = 0
    foreach ($u in $wiad.Values) {
        $a = [long]$u.input_tokens; $b = [long]$u.cache_creation_input_tokens; $c = [long]$u.cache_read_input_tokens; $d = [long]$u.output_tokens
        $we += $a; $zc += $b; $oc += $c; $wy += $d
        if (($a + $b + $c) -gt $max) { $max = $a + $b + $c }
    }
    [pscustomobject]@{
        tury = $wiad.Count; narzedzia = $narz; kontekst_max = $max
        przetworzone = $we + $zc + $oc + $wy
        rownowaznik = [long]($we + 1.25 * $zc + 0.1 * $oc + 5 * $wy)
        wyjscie = $wy; model = $model
        start = if ($pierwszy) { $pierwszy.ToLocalTime().ToString('yyyy-MM-dd HH:mm') } else { '' }
        czas_min = if ($pierwszy) { [math]::Round(($ostatni - $pierwszy).TotalMinutes, 1) } else { 0 }
    }
}

function Dopisz([string]$plik, [string]$naglowek, [string]$wiersz) {
    if (-not (Test-Path $plik)) { [System.IO.File]::WriteAllText($plik, "$naglowek`n", [System.Text.UTF8Encoding]::new($false)) }
    [System.IO.File]::AppendAllText($plik, "$wiersz`n", [System.Text.UTF8Encoding]::new($false))
}

if ($zdarzenie -eq 'SubagentStop') {
    $tr = "$($in.agent_transcript_path)"
    if (-not $tr -and $in.transcript_path -and $in.agent_id) {
        $kat = Join-Path ([System.IO.Path]::GetDirectoryName($in.transcript_path)) $in.session_id
        $tr = Join-Path $kat "subagents/agent-$($in.agent_id).jsonl"
    }
    $p = Podsumuj $tr
    if (-not $p) { exit 0 }
    $agent = if ($in.agent_type) { $in.agent_type } else { 'nieznany' }
    $sesja = "$($in.session_id)".Substring(0, [Math]::Min(8, "$($in.session_id)".Length))
    Dopisz (Join-Path $stan 'koszty-agenci.csv') 'data;sesja;agent;model;tury;narzedzia;kontekst_max;przetworzone;rownowaznik;czas_min' `
        "$($p.start);$sesja;$agent;$($p.model);$($p.tury);$($p.narzedzia);$($p.kontekst_max);$($p.przetworzone);$($p.rownowaznik);$($p.czas_min)"
}
elseif ($zdarzenie -eq 'Stop') {
    $p = Podsumuj "$($in.transcript_path)"
    if (-not $p) { exit 0 }
    $sesja = "$($in.session_id)".Substring(0, [Math]::Min(8, "$($in.session_id)".Length))
    $plik = Join-Path $stan 'koszty-sesje.csv'
    $naglowek = 'start;sesja;model;tury;narzedzia;kontekst_max;przetworzone;rownowaznik;czas_min'
    $wiersze = [System.Collections.Generic.List[string]]::new()
    if (Test-Path $plik) { foreach ($w in (Get-Content $plik -Encoding UTF8 | Select-Object -Skip 1)) { if ($w -and ($w -split ';')[1] -ne $sesja) { $wiersze.Add($w) } } }
    $wiersze.Add("$($p.start);$sesja;$($p.model);$($p.tury);$($p.narzedzia);$($p.kontekst_max);$($p.przetworzone);$($p.rownowaznik);$($p.czas_min)")
    $wszystkie = @($naglowek) + @($wiersze)
    [System.IO.File]::WriteAllText($plik, [string]::Join("`n", $wszystkie) + "`n", [System.Text.UTF8Encoding]::new($false))
}
exit 0
