# SessionStart: wstrzykuje STATUS do kontekstu; czyści przeterminowaną bramkę manifestu; odświeża mapę kodu, gdy kod jest nowszy.
$ErrorActionPreference = 'SilentlyContinue'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$root  = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }
$stan  = Join-Path $root '.claude/stan'
$lock  = Join-Path $stan 'manifest-ok.json'
if (Test-Path $lock) { Remove-Item $lock -Force }

$out = "=== STATUS projektu (wstrzyknięty automatycznie na starcie sesji — hooki działają) ===`n"
$status = Join-Path $root '.claude/STATUS.md'
if (Test-Path $status) { $out += (Get-Content $status -Raw -Encoding UTF8) } else { $out += 'Brak pliku .claude/STATUS.md.' }
if (Test-Path (Join-Path $stan 'pomiary.json')) {
    $out += "`n`n!!! TRWA SERIA POMIARÓW — system pomiarowy jest chroniony (Z12), aktualizacje wstrzymane (Z11.3). !!!"
}

# Mapa kodu — tylko gdy brak albo jakikolwiek .cs jest nowszy
$mapa = Join-Path $root '.claude/mapa-kodu.md'
$skrypt = Join-Path $root '.claude/narzedzia/mapa-kodu.ps1'
if (Test-Path $skrypt) {
    $najnowszy = Get-ChildItem (Join-Path $root 'Assets/Ratocalypse') -Recurse -Filter *.cs -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    $staraMapa = -not (Test-Path $mapa) -or ($najnowszy -and $najnowszy.LastWriteTime -gt (Get-Item $mapa).LastWriteTime)
    if ($staraMapa) { $wynik = & $skrypt -Projekt $root } else { $wynik = 'Mapa kodu aktualna.' }
    $out += "`n`n[mapa kodu] $wynik" + ' — czytaj .claude/mapa-kodu.md przed kodem.'
}
Write-Output $out
exit 0
