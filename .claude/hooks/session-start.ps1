# SessionStart: wstrzykuje STATUS do kontekstu; czyści przeterminowaną bramkę manifestu.
$ErrorActionPreference = 'SilentlyContinue'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$root  = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }
$stan  = Join-Path $root '.claude/stan'
$lock  = Join-Path $stan 'manifest-ok.json'
if (Test-Path $lock) { Remove-Item $lock -Force }

$out = "=== STATUS projektu (wstrzyknięty automatycznie na starcie sesji) ===`n"
$status = Join-Path $root '.claude/STATUS.md'
if (Test-Path $status) { $out += (Get-Content $status -Raw -Encoding UTF8) } else { $out += 'Brak pliku .claude/STATUS.md.' }
if (Test-Path (Join-Path $stan 'pomiary.json')) {
    $out += "`n`n!!! TRWA SERIA POMIARÓW — system pomiarowy jest chroniony (Z12), aktualizacje wstrzymane (Z11.3). !!!"
}
Write-Output $out
exit 0
