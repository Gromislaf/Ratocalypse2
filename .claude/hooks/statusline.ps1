# statusline.ps1 — pasek stanu Claude Code: zadanie, etap, gałąź, zapełnienie kontekstu.
# Kontekst liczony z ostatniej odpowiedzi modelu w transkrypcie (wejście + cache). Okno 200 tys.; auto-kompaktowanie ok. 83%.
$ErrorActionPreference = 'SilentlyContinue'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$in   = [Console]::In.ReadToEnd() | ConvertFrom-Json
$root = if ($in.workspace.project_dir) { $in.workspace.project_dir } elseif ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }

# zadanie i etap z punktu kontrolnego
$zadanie = ''; $etap = ''
$status = Join-Path $root '.claude/STATUS.md'
if (Test-Path $status) {
    foreach ($l in [System.IO.File]::ReadLines($status)) {
        if ($l -match '^\|\s*Aktywne zadanie\s*\|\s*(.+?)\s*\|') { $zadanie = $Matches[1] -replace '\s+—.*$', '' }
        elseif ($l -match '^\|\s*Etap\s*\|\s*(.+?)\s*\|') { $etap = $Matches[1] }
        if ($l -match '/CHECKPOINT') { break }
    }
}
$galaz = (git -C $root branch --show-current 2>$null)

# kontekst: ostatnie usage z końcówki transkryptu
$kontekst = ''
$tr = "$($in.transcript_path)"
if ($tr -and (Test-Path $tr)) {
    $fs = [System.IO.File]::Open($tr, 'Open', 'Read', 'ReadWrite')
    try {
        $n = [Math]::Min($fs.Length, 400KB); [void]$fs.Seek(-$n, 'End')
        $buf = New-Object byte[] $n; [void]$fs.Read($buf, 0, $n)
        $ogon = [System.Text.Encoding]::UTF8.GetString($buf)
    } finally { $fs.Close() }
    $m = [regex]::Matches($ogon, '"input_tokens":(\d+),"cache_creation_input_tokens":(\d+),"cache_read_input_tokens":(\d+)')
    if ($m.Count) {
        $g = $m[$m.Count - 1].Groups
        $suma = [long]$g[1].Value + [long]$g[2].Value + [long]$g[3].Value
        $proc = [math]::Round(100 * $suma / 200000)
        $znak = if ($proc -ge 60) { ' ⚠ /compact albo /clear' } else { '' }
        $kontekst = "kontekst $([math]::Round($suma / 1000)) tys. ($proc%)$znak"
    }
}

$czesci = @()
if ($zadanie -match '^brak') { $czesci += 'bez zadania' } elseif ($zadanie) { $czesci += $zadanie }
if ($etap -and $etap -ne '—') { $czesci += "etap $etap" }
if ($galaz) { $czesci += $galaz }
if ($kontekst) { $czesci += $kontekst }
Write-Output ($czesci -join ' · ')
