# SessionStart: wstrzykuje punkt kontrolny STATUS do kontekstu; czyści przeterminowaną bramkę manifestu; odświeża mapę kodu i mapę dokumentacji, gdy źródła są nowsze.
$ErrorActionPreference = 'SilentlyContinue'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$root  = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }
$stan  = Join-Path $root '.claude/stan'
$lock  = Join-Path $stan 'manifest-ok.json'
if (Test-Path $lock) { Remove-Item $lock -Force }

$out = "=== STATUS projektu — punkt kontrolny (wstrzyknięty automatycznie na starcie sesji — hooki działają) ===`n"
$status = Join-Path $root '.claude/STATUS.md'
if (Test-Path $status) {
    $tekst = Get-Content $status -Raw -Encoding UTF8
    $m = [regex]::Match($tekst, '<!-- CHECKPOINT -->(.*?)<!-- /CHECKPOINT -->', 'Singleline')
    if ($m.Success) { $out += $m.Groups[1].Value.Trim() + "`n`n(Kolejka, dziennik, strefy i zamknięte — w .claude/STATUS.md; czytaj tylko, gdy potrzebne, np. /stan.)" }
    else { $out += $tekst + "`n`n!!! STATUS bez znaczników CHECKPOINT — dodaj punkt kontrolny przy najbliższej bramce. !!!" }
} else { $out += 'Brak pliku .claude/STATUS.md.' }
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
# Mapa dokumentacji — tylko gdy brak albo jakikolwiek .md w dokumentacji / zasadach jest nowszy
$mapaDok = Join-Path $root '.claude/mapa-dokumentacji.md'
$skryptDok = Join-Path $root '.claude/narzedzia/mapa-dokumentacji.ps1'
if (Test-Path $skryptDok) {
    $zrodla = @('Documentation', 'Vision', '.claude/zasady', '.claude/rules') | ForEach-Object { Join-Path $root $_ } | Where-Object { Test-Path $_ }
    $najnowszyMd = $zrodla | ForEach-Object { Get-ChildItem $_ -Recurse -Filter *.md -File } | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    $staraDok = -not (Test-Path $mapaDok) -or ($najnowszyMd -and $najnowszyMd.LastWriteTime -gt (Get-Item $mapaDok).LastWriteTime)
    if ($staraDok) { $wynikDok = & $skryptDok -Projekt $root } else { $wynikDok = 'Mapa dokumentacji aktualna.' }
    $out += "`n[mapa dokumentacji] $wynikDok" + ' — sekcje dokumentacji i zasad szukaj w .claude/mapa-dokumentacji.md.'
}
Write-Output $out
exit 0
