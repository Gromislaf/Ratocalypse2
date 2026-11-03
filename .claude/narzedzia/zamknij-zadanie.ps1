# zamknij-zadanie.ps1 — zamknięcie zadania w STATUS po pushu (bez agentów, zero tokenów).
# Uruchamianie z katalogu projektu:
#   pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/narzedzia/zamknij-zadanie.ps1 -Zadanie 4 -Commit "3741ef4 → merge 002a21c → push"
# Robi: dziennik bramek ze STATUS → na koniec planu zadania (sekcja „Dziennik bramek”), czyści dziennik w STATUS,
#       usuwa zadanie z kolejki, dopisuje je do „Zamknięte” (najwyżej 10), nadpisuje punkt kontrolny (brak zadania + 3 następne z kolejki).
# Kod wyjścia: 0 = zrobione, 1 = błąd (STATUS bez zmian).

param(
    [Parameter(Mandatory)][int]$Zadanie,
    [Parameter(Mandatory)][string]$Commit,
    [string]$Opis = ''
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$projekt = Split-Path (Split-Path $PSScriptRoot)
$plikStatus = Join-Path $projekt '.claude/STATUS.md'
if (-not (Test-Path $plikStatus)) { Write-Output '❌ Brak .claude/STATUS.md'; exit 1 }

$tekst = [System.IO.File]::ReadAllText($plikStatus)
$nl = if ($tekst.Contains("`r`n")) { "`r`n" } else { "`n" }
$W = [System.Collections.Generic.List[string]]::new()
foreach ($x in ($tekst -split "`r?`n")) { $W.Add($x) }

function Naglowek([string]$wzor) {
    for ($i = 0; $i -lt $W.Count; $i++) { if ($W[$i] -match $wzor) { return $i } }
    return -1
}
# zwraca @{ sep = indeks wiersza |---|; od = pierwszy wiersz danych; do = ostatni wiersz danych (do < od, gdy pusta) }
function Tabela([int]$odNaglowka) {
    $i = $odNaglowka + 1
    while ($i -lt $W.Count -and $W[$i] -notmatch '^\|' -and $W[$i] -notmatch '^## ') { $i++ }
    if ($i -ge $W.Count -or $W[$i] -notmatch '^\|') { return $null }
    $sep = $i + 1
    if ($sep -ge $W.Count -or $W[$sep] -notmatch '^\|\s*-') { return $null }
    $j = $sep + 1
    while ($j -lt $W.Count -and $W[$j] -match '^\|') { $j++ }
    return @{ sep = $sep; od = $sep + 1; do = $j - 1 }
}
function Kolumny([string]$w) { return @(($w.Trim().Trim('|')) -split '\|' | ForEach-Object { $_.Trim() }) }
function Pusty([string]$w) { return ($w -replace '[|\s—-]', '') -eq '' }

$iDz = Naglowek '^## Dziennik bramek'
$iKol = Naglowek '^## Kolejka'
$iZam = Naglowek '^## Zamknięte'
$iCp = Naglowek '<!-- CHECKPOINT -->'
$iCpK = Naglowek '<!-- /CHECKPOINT -->'
foreach ($para in @(@('Dziennik bramek', $iDz), @('Kolejka', $iKol), @('Zamknięte', $iZam), @('<!-- CHECKPOINT -->', $iCp), @('<!-- /CHECKPOINT -->', $iCpK))) {
    if ($para[1] -lt 0) { Write-Output "❌ Nie znaleziono w STATUS: $($para[0])"; exit 1 }
}

# --- 0. Najpierw sprawdzenia (zanim cokolwiek zmienimy): wiersz w kolejce albo wcześniejszy wpis w Zamknięte ---
$tKol = Tabela $iKol
if (-not $tKol) { Write-Output '❌ Nie znaleziono tabeli kolejki'; exit 1 }
$iWiersz = -1
for ($i = $tKol.od; $i -le $tKol.do; $i++) { if ($W[$i] -match "^\|\s*#?$Zadanie\s*\|") { $iWiersz = $i; break } }
$tZam = Tabela $iZam
$iZamWiersz = -1
if ($tZam) { for ($i = $tZam.od; $i -le $tZam.do; $i++) { if ($W[$i] -match "^\|[^|]*\|\s*#$Zadanie(\s|\|)") { $iZamWiersz = $i; break } } }
$aktualizacja = ($iWiersz -lt 0 -and $iZamWiersz -ge 0)   # drugi przebieg (np. po commicie, potem po pushu) — tylko aktualizacja kolumny Commit
$ostrz = [System.Collections.Generic.List[string]]::new()
if ($iWiersz -ge 0) {
    if (-not $Opis) { $Opis = (Kolumny $W[$iWiersz])[1] }
}
elseif ($aktualizacja) { $ostrz.Add("zadanie #$Zadanie było już zamknięte — zaktualizowano tylko kolumnę Commit w Zamknięte") }
elseif (-not $Opis) { Write-Output "❌ Zadania #$Zadanie nie ma w kolejce ani w Zamknięte — sprawdź numer albo podaj -Opis (STATUS bez zmian)."; exit 1 }
else { $ostrz.Add("zadania #$Zadanie nie było w kolejce — wpis w Zamknięte z opisem z -Opis") }

# --- 1. Dziennik ze STATUS ---
$tDz = Tabela $iDz
$dziennik = [System.Collections.Generic.List[string]]::new()
if ($tDz) { for ($i = $tDz.od; $i -le $tDz.do; $i++) { if (-not (Pusty $W[$i])) { $dziennik.Add($W[$i]) } } }

# --- 2. Plan zadania ---
$plan = $null
$katPlany = Join-Path $projekt '.claude/plans'
if (Test-Path $katPlany) {
    $plan = Get-ChildItem $katPlany -Filter '*.md' | Where-Object { $_.BaseName -match "(^|[^0-9])0*$Zadanie([^0-9]|$)" } | Select-Object -First 1
}
$planRel = ''
if ($plan) {
    $planRel = '.claude/plans/' + $plan.Name
    if ($dziennik.Count) {
        $pt = [System.IO.File]::ReadAllText($plan.FullName)
        $pnl = if ($pt.Contains("`r`n")) { "`r`n" } else { "`n" }
        $PL = [System.Collections.Generic.List[string]]::new(); foreach ($x in ($pt -split "`r?`n")) { $PL.Add($x) }
        $ip = -1; for ($i = 0; $i -lt $PL.Count; $i++) { if ($PL[$i] -match '^## Dziennik bramek') { $ip = $i; break } }
        $nowe = @($dziennik | Where-Object { -not $PL.Contains($_) })
        if ($ip -lt 0) {
            while ($PL.Count -and $PL[$PL.Count - 1].Trim() -eq '') { $PL.RemoveAt($PL.Count - 1) }
            $PL.AddRange([string[]]@('', '## Dziennik bramek', '', '| Data | Etap | Zdarzenie / decyzja |', '|---|---|---|'))
            $PL.AddRange([string[]]$nowe); $PL.Add('')
        }
        elseif ($nowe.Count) {
            $i = $ip + 1
            while ($i -lt $PL.Count -and $PL[$i] -notmatch '^\|' -and $PL[$i] -notmatch '^## ') { $i++ }
            if ($i -lt $PL.Count -and $PL[$i] -match '^\|') {
                while ($i -lt $PL.Count -and $PL[$i] -match '^\|') { $i++ }
                $PL.InsertRange($i, [string[]]$nowe)
            }
            else { $PL.InsertRange($ip + 1, [string[]](@('', '| Data | Etap | Zdarzenie / decyzja |', '|---|---|---|') + $nowe)) }
        }
        [System.IO.File]::WriteAllText($plan.FullName, ($PL -join $pnl), [System.Text.UTF8Encoding]::new($false))
    }
}

# --- 3. Kolejka: 3 następne ---
$nastepne = [System.Collections.Generic.List[string]]::new()
for ($i = $tKol.od; $i -le $tKol.do -and $nastepne.Count -lt 3; $i++) {
    if ($i -eq $iWiersz -or (Pusty $W[$i])) { continue }
    $k = Kolumny $W[$i]
    $meta = @($k[2], $k[3]) | Where-Object { $_ -and $_ -ne '—' }
    $nastepne.Add("**#$($k[0].TrimStart('#'))** $($k[1])" + $(if ($meta) { " ($($meta -join ', '))" } else { '' }))
}

# --- 4. Gałąź i strefy (do punktu kontrolnego) ---
$galaz = ''
$head = Join-Path $projekt '.git/HEAD'
if (Test-Path $head) { $h = (Get-Content $head -Raw).Trim(); if ($h -match '^ref: refs/heads/(.+)$') { $galaz = $Matches[1] } }
$strefy = 'Strefy czasowe: brak.'
for ($i = $iCp; $i -le $iCpK; $i++) { if ($W[$i] -match '^Strefy czasowe:') { $strefy = $W[$i] } }

# --- 5. Zmiany w STATUS: od dołu do góry, żeby indeksy się nie przesuwały ---
$data = Get-Date -Format 'yyyy-MM-dd'
$wpis = "| $data | #$Zadanie $Opis" + $(if ($planRel) { " (dziennik: ``$planRel``)" } else { '' }) + " | $Commit |"

# Zamknięte (najniżej w pliku z trzech tabel — kolejność sekcji sprawdzana)
$operacje = @(
    @{ i = $iZam; f = {
        if ($aktualizacja) {
            $k = Kolumny $W[$iZamWiersz]; $k[$k.Count - 1] = $Commit
            $W[$iZamWiersz] = '| ' + ($k -join ' | ') + ' |'
        }
        elseif ($tZam) {
            $W.Insert($tZam.od, $wpis)
            $dane = @(); for ($i = $tZam.od; $i -le $tZam.do + 1; $i++) { $dane += $i }
            $nadmiar = $dane.Count - 10
            for ($n = 0; $n -lt $nadmiar; $n++) { $W.RemoveAt($tZam.do + 1 - $n) }
        }
        else { $W.InsertRange($iZam + 1, [string[]]@('', '| Data | Zadanie | Commit |', '|---|---|---|', $wpis)) }
    } },
    @{ i = $iKol; f = { if ($iWiersz -ge 0) { $W.RemoveAt($iWiersz) } } },
    @{ i = $iDz; f = { if ($tDz -and $tDz.do -ge $tDz.od) { $W.RemoveRange($tDz.od, $tDz.do - $tDz.od + 1) } } },
    @{ i = $iCp; f = {
        $cp = @(
            '## Punkt kontrolny', '',
            '| | |', '|---|---|',
            '| Aktywne zadanie | brak |',
            "| Gałąź | ``$galaz``$(if ($galaz -and $galaz -notin @('main','master')) { ' — następne zadanie zaczyna `wydawca` krokiem gałąź' }) |",
            "| Czeka na | wybór kolejnego zadania (zamknięte: #$Zadanie) |", '',
            "Następne w kolejce: $($nastepne -join ' · '). Pełna kolejka: ``/stan``.",
            $strefy
        )
        $W.RemoveRange($iCp + 1, $iCpK - $iCp - 1)
        $W.InsertRange($iCp + 1, [string[]]$cp)
    } }
)
foreach ($op in ($operacje | Sort-Object { $_.i } -Descending)) { & $op.f }

[System.IO.File]::WriteAllText($plikStatus, ($W -join $nl), [System.Text.UTF8Encoding]::new($false))

Write-Output $(if ($aktualizacja) { "# Zadanie #$Zadanie — aktualizacja wpisu w Zamknięte" } else { "# Zamknięto zadanie #$Zadanie" })
Write-Output "- dziennik: $($dziennik.Count) wierszy → $(if ($planRel) { $planRel } elseif ($dziennik.Count) { 'brak planu (XS) — wiersze tylko tutaj:' } else { 'brak planu (XS)' })"
if (-not $planRel) { foreach ($d in $dziennik) { Write-Output "  $d" } }
Write-Output "- kolejka: $(if ($iWiersz -ge 0) { "usunięto #$Zadanie" } else { 'bez zmian' })"
Write-Output "- zamknięte: $(if ($aktualizacja) { $W | Where-Object { $_ -match "^\|[^|]*\|\s*#$Zadanie(\s|\|)" } | Select-Object -First 1 } else { $wpis })"
Write-Output "- punkt kontrolny: brak zadania; następne: $($nastepne -join ' · ')"
foreach ($o in $ostrz) { Write-Output "⚠️ $o" }
exit 0
