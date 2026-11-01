# koszty.ps1 — podsumowanie zużycia tokenów z .claude/stan/koszty-*.csv (zapisują je hooki).
# Uruchamianie: pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/narzedzia/koszty.ps1 [-Dni 7]
param([int]$Dni = 7)
$ErrorActionPreference = 'SilentlyContinue'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$root = (git rev-parse --show-toplevel 2>$null); if (-not $root) { $root = (Get-Location).Path }
$stan = Join-Path $root '.claude/stan'
$od = (Get-Date).AddDays(-$Dni)

function Wczytaj([string]$nazwa) {
    $p = Join-Path $stan $nazwa
    if (-not (Test-Path $p)) { return @() }
    @(Import-Csv $p -Delimiter ';' -Encoding UTF8 | Where-Object { $_.start -or $_.data } | Where-Object {
        $d = if ($_.data) { $_.data } else { $_.start }; try { [datetime]$d -ge $od } catch { $true } })
}
function Tys($n) { '{0:N0}' -f ([double]$n / 1000) + ' tys.' }

$sesje = Wczytaj 'koszty-sesje.csv'
$agenci = Wczytaj 'koszty-agenci.csv'
if (-not $sesje.Count -and -not $agenci.Count) { Write-Output "Brak danych z ostatnich $Dni dni (hooki zapisują je od instalacji poprawki 3.1)."; exit 0 }

$sumaS = ($sesje | Measure-Object rownowaznik -Sum).Sum
$sumaA = ($agenci | Measure-Object rownowaznik -Sum).Sum
$razem = [double]$sumaS + [double]$sumaA
Write-Output "# Koszty — ostatnie $Dni dni"
Write-Output ''
Write-Output "Równoważnik tokenów wejścia (wejście×1 + zapis cache×1,25 + odczyt cache×0,1 + wyjście×5). Ceny modeli różnią się — patrz kolumna model."
Write-Output ''
Write-Output "| Źródło | Równoważnik | Udział |"
Write-Output "|---|---|---|"
if ($razem -gt 0) {
    Write-Output ("| Główna rozmowa ({0} sesji) | {1} | {2:P0} |" -f $sesje.Count, (Tys $sumaS), ([double]$sumaS / $razem))
    Write-Output ("| Subagenci ({0} wywołań) | {1} | {2:P0} |" -f $agenci.Count, (Tys $sumaA), ([double]$sumaA / $razem))
}
Write-Output ''
if ($agenci.Count) {
    Write-Output '## Subagenci'
    Write-Output ''
    Write-Output '| Agent | Wywołania | Suma | Średnio | Maks. kontekst | Narzędzia (średnio) |'
    Write-Output '|---|---|---|---|---|---|'
    $agenci | Group-Object agent | ForEach-Object {
        $s = ($_.Group | Measure-Object rownowaznik -Sum -Average)
        $k = ($_.Group | Measure-Object kontekst_max -Maximum).Maximum
        $n = ($_.Group | Measure-Object narzedzia -Average).Average
        [pscustomobject]@{ a = $_.Name; c = $_.Count; s = $s.Sum; avg = $s.Average; k = $k; n = $n }
    } | Sort-Object s -Descending | ForEach-Object {
        Write-Output ("| {0} | {1} | {2} | {3} | {4} | {5:N0} |" -f $_.a, $_.c, (Tys $_.s), (Tys $_.avg), (Tys $_.k), $_.n)
    }
    Write-Output ''
}
if ($sesje.Count) {
    Write-Output '## Sesje (ostatnie 10)'
    Write-Output ''
    Write-Output '| Start | Sesja | Tury | Maks. kontekst | Równoważnik | Czas (min) |'
    Write-Output '|---|---|---|---|---|---|'
    $sesje | Sort-Object start -Descending | Select-Object -First 10 | ForEach-Object {
        Write-Output ("| {0} | {1} | {2} | {3} | {4} | {5} |" -f $_.start, $_.sesja, $_.tury, (Tys $_.kontekst_max), (Tys $_.rownowaznik), $_.czas_min)
    }
}
