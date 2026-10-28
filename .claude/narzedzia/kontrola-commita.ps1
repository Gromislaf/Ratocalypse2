# kontrola-commita.ps1 — sprawdzenie zmian przed commitem (bez agentów, zero tokenów).
# Uruchamianie z katalogu projektu:
#   pwsh -NoProfile -File .claude/narzedzia/kontrola-commita.ps1 [-Plan .claude/plans/task-02-....md]
# Kod wyjścia: 0 = brak błędów (ostrzeżenia możliwe), 1 = są błędy ❌ (commit wstrzymany).

param(
    [string]$Plan = ''
)

$ErrorActionPreference = 'Stop'
$projekt = (git rev-parse --show-toplevel 2>$null)
if (-not $projekt) { Write-Output '❌ To nie jest repozytorium git.'; exit 1 }
Set-Location $projekt

$bledy = [System.Collections.Generic.List[string]]::new()
$ostrz = [System.Collections.Generic.List[string]]::new()

# --- 1. Gałąź ---
$galaz = git branch --show-current
if ($galaz -in @('main', 'master')) { $bledy.Add("commit bezpośrednio na ``$galaz`` — najpierw gałąź ``feature/<ID>-…``") }

# --- 2. Lista zmian: drzewo robocze + commity gałęzi względem main ---
$zmiany = [ordered]@{}   # ścieżka -> status (A/M/D/R/??)
foreach ($w in (git -c core.quotepath=false status --porcelain=v1 -uall)) {
    if (-not $w) { continue }
    $st = $w.Substring(0, 2).Trim(); $sc = $w.Substring(3)
    if ($sc -match ' -> ') { $sc = ($sc -split ' -> ')[1] }
    $zmiany[$sc.Trim('"')] = $st
}
$baza = git merge-base HEAD main 2>$null
if ($baza -and $galaz -notin @('main', 'master')) {
    foreach ($w in (git -c core.quotepath=false diff --name-status "$baza" HEAD)) {
        if (-not $w) { continue }
        $cz = $w -split "`t"
        $sc = $cz[-1]
        if (-not $zmiany.Contains($sc)) { $zmiany[$sc] = $cz[0].Substring(0, 1) }
    }
}
if ($zmiany.Count -eq 0) { Write-Output 'Brak zmian do commita.'; exit 0 }

# --- 3. Zakazane ścieżki ---
$zakazane = '^(Library|Temp|Logs|UserSettings|obj|Build|Builds)/|^\.claude/stan/(?!\.gitignore$)|^\.claude/settings\.local\.json$|\.csproj$|\.sln$'
$chronione = '^Packages/(manifest|packages-lock)\.json$|^ProjectSettings/ProjectVersion\.txt$'
foreach ($p in $zmiany.Keys) {
    if ($p -match $zakazane) { $bledy.Add("plik, który nie może trafić do repozytorium: ``$p``") }
    elseif ($p -match $chronione) { $ostrz.Add("plik zamrożony (Z11): ``$p`` — tylko z zatwierdzonym wnioskiem") }
}

# --- 4. Pary .meta (Assets/) ---
foreach ($p in $zmiany.Keys) {
    $st = $zmiany[$p]
    if ($p -notmatch '^Assets/') { continue }
    if ($st -in @('A', '??')) {
        if ($p -match '\.meta$') {
            $cel = $p -replace '\.meta$', ''
            if (-not (Test-Path $cel)) { $bledy.Add("osierocony ``.meta`` (brak pliku/folderu): ``$p``") }
        }
        elseif (-not (Test-Path "$p.meta")) { $bledy.Add("nowy plik bez ``.meta`` (otwórz Unity, żeby go wygenerował): ``$p``") }
        elseif (-not $zmiany.Contains("$p.meta")) { $ostrz.Add("``$p.meta`` istnieje, ale nie jest w zmianach — dodaj go do commita") }
    }
    if ($st -eq 'D' -and $p -notmatch '\.meta$' -and -not $zmiany.Contains("$p.meta") -and (Test-Path "$p.meta")) {
        $bledy.Add("usunięty plik, a jego ``.meta`` został: ``$p.meta``")
    }
}

# --- 5. Zawartość zmienionych plików tekstowych ---
$tekstowe = '\.(cs|md|json|asmdef|txt|ps1|unity|prefab|asset|mat|shader|uss|uxml|inputactions)$'
foreach ($p in $zmiany.Keys) {
    if ($zmiany[$p] -eq 'D' -or $p -notmatch $tekstowe -or -not (Test-Path $p -PathType Leaf)) { continue }
    $rozmiar = (Get-Item $p).Length
    if ($rozmiar -gt 10MB) { $ostrz.Add("duży plik ($([math]::Round($rozmiar / 1MB, 1)) MB): ``$p`` — zadanie Git LFS") }
    if ($rozmiar -gt 5MB) { continue }
    $nr = 0
    foreach ($linia in [System.IO.File]::ReadLines((Resolve-Path $p))) {
        $nr++
        if ($linia -match '^(<<<<<<<|>>>>>>>)( |$)|^=======$') { $bledy.Add("znacznik konfliktu: ``${p}:$nr``"); break }
        if ($p -match '\.cs$' -and $linia -match '\[DBG\]') { $bledy.Add("tymczasowy log ``[DBG]`` do usunięcia: ``${p}:$nr``") }
    }
    if ($p -match '\.cs$') {
        $bajty = [System.IO.File]::ReadAllBytes((Resolve-Path $p))
        try { $t = [System.Text.UTF8Encoding]::new($false, $true).GetString($bajty); if ($t.Contains([char]0xFFFD)) { throw 'x' } }
        catch { $ostrz.Add("plik nie w UTF-8 lub z uszkodzonymi znakami (konwencje): ``$p``") }
    }
}

# --- 6. Pliki spoza planu ---
if (-not $Plan -and $galaz -match '^feature/(\d+)') {
    $kand = Get-ChildItem '.claude/plans' -Filter "task-$($Matches[1])*.md" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($kand) { $Plan = [System.IO.Path]::GetRelativePath($projekt, $kand.FullName) -replace '\\', '/' }
}
$spozaPlanu = [System.Collections.Generic.List[string]]::new()
if ($Plan -and (Test-Path $Plan)) {
    $tekstPlanu = Get-Content $Plan -Raw
    $zawsze = '^\.claude/(STATUS\.md|mapa-kodu\.md|plans/|zasady/decyzje\.md)|^Documentation/|^Vision/'
    foreach ($p in $zmiany.Keys) {
        if ($p -match $zawsze) { continue }
        $rdzen = $p -replace '\.meta$', ''
        $nazwa = Split-Path $rdzen -Leaf
        if ($tekstPlanu.Contains($rdzen) -or $tekstPlanu.Contains($nazwa)) { continue }
        $spozaPlanu.Add($p)
    }
    foreach ($s in $spozaPlanu) { $ostrz.Add("plik spoza planu: ``$s`` — przypadkowa zmiana czy brak w planie?") }
}
else { $ostrz.Add('nie znaleziono planu zadania — kontrola „pliki spoza planu” pominięta (podaj -Plan)') }

# --- Raport ---
Write-Output "# Kontrola przed commitem — gałąź ``$galaz``"
Write-Output ''
Write-Output "## Pliki ($($zmiany.Count))"
foreach ($p in $zmiany.Keys) { Write-Output "- $($zmiany[$p].PadRight(2)) $p" }
Write-Output ''
Write-Output '## Wynik'
if ($bledy.Count -eq 0 -and $ostrz.Count -eq 0) { Write-Output '✅ wszystkie kontrole bez uwag' }
foreach ($b in $bledy) { Write-Output "❌ $b" }
foreach ($o in $ostrz) { Write-Output "⚠️ $o" }
if ($Plan) { Write-Output ''; Write-Output "Plan: ``$Plan``" }
if ($bledy.Count) { exit 1 } else { exit 0 }
