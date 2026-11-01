# post-cs.ps1 — PostToolUse po Edit/Write/MultiEdit pliku .cs w Assets/Ratocalypse/.
# Ostrzega (nie blokuje — plik jest już zapisany) o problemach w **zmienionych liniach**:
#   a ?. / ?? na obiektach Unity (konwencje §4)      b GetComponent/Find w Update (wydajność)
#   c LINQ w Update (alokacje)                        d Camera.main w Update
#   e zmiana nazwy pola serializowanego bez FormerlySerializedAs (konwencje §3)
#   f nazwa pliku ≠ nazwa klasy MonoBehaviour/SO      g plik nie w UTF-8 (konwencje §9)
#   h singleton spoza rejestru (Z5)                   k AudioSource/AudioClip zamiast FMOD (Z9)
# Wyłączone do czasu zadań: Input. (Z14, po #25), Random bez seeda (Z10.1, po #18).
$ErrorActionPreference = 'SilentlyContinue'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$in   = [Console]::In.ReadToEnd() | ConvertFrom-Json
$ti   = $in.tool_input
$path = "$($ti.file_path)"
if ($path -notmatch '\.cs$' -or -not (Test-Path $path -PathType Leaf)) { exit 0 }
$root = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }
$rel  = $path.Replace('\', '/')
$r    = $root.Replace('\', '/').TrimEnd('/') + '/'
if ($rel.StartsWith($r, [StringComparison]::OrdinalIgnoreCase)) { $rel = $rel.Substring($r.Length) }
if ($rel -notmatch '^Assets/Ratocalypse/') { exit 0 }
$kodGry = $rel -notmatch '/(Editor|Tests)/'

# --- zmieniony tekst ---
$nowe = [System.Collections.Generic.List[string]]::new(); $stare = [System.Collections.Generic.List[string]]::new()
switch ("$($in.tool_name)") {
    'Write'     { $nowe.Add("$($ti.content)") }
    'Edit'      { $nowe.Add("$($ti.new_string)"); $stare.Add("$($ti.old_string)") }
    'MultiEdit' { foreach ($e in $ti.edits) { $nowe.Add("$($e.new_string)"); $stare.Add("$($e.old_string)") } }
    default     { exit 0 }
}
$noweTekst = $nowe -join "`n"; $stareTekst = $stare -join "`n"
$zmienione = [System.Collections.Generic.HashSet[string]]::new()
foreach ($l in ($noweTekst -split "`n")) { $t = $l.Trim(); if ($t) { [void]$zmienione.Add($t) } }

# --- plik po zmianie ---
$bajty = [System.IO.File]::ReadAllBytes($path)
$uwagi = [System.Collections.Generic.List[string]]::new()
try { $tekst = [System.Text.UTF8Encoding]::new($false, $true).GetString($bajty); if ($tekst.Contains([char]0xFFFD)) { throw 'x' } }
catch {
    $tekst = [System.Text.Encoding]::Latin1.GetString($bajty)
    $uwagi.Add("[g, konwencje §9] plik nie jest w UTF-8 lub ma uszkodzone znaki — nie naprawiaj przy okazji (zadanie #6), tylko nie dopisuj polskich znaków w złym kodowaniu.")
}
$linie = $tekst -split "`r?`n"
function BezKomentarza([string]$l) { ($l -replace '//.*$', '') }
function Zmieniona([string]$l) { $zmienione.Contains($l.Trim()) }

# --- metody wywoływane co klatkę ---
$goraca = [System.Collections.Generic.HashSet[int]]::new()
for ($i = 0; $i -lt $linie.Count; $i++) {
    if ($linie[$i] -match '\bvoid\s+(Update|LateUpdate|FixedUpdate|OnGUI)\s*\(') {
        $glebokosc = 0; $start = $false
        for ($j = $i; $j -lt $linie.Count; $j++) {
            $bk = BezKomentarza $linie[$j]
            foreach ($c in $bk.ToCharArray()) { if ($c -eq '{') { $glebokosc++; $start = $true } elseif ($c -eq '}') { $glebokosc-- } }
            if ($j -gt $i) { [void]$goraca.Add($j) }
            if ($start -and $glebokosc -le 0) { break }
        }
    }
}
$maLinq = $tekst -match 'using\s+System\.Linq\s*;'
$singletony = @('GameManager', 'AudioManager', 'QuestManager', 'SaveSystem', 'MinimapController')

for ($i = 0; $i -lt $linie.Count; $i++) {
    $l = $linie[$i]
    if (-not (Zmieniona $l)) { continue }
    $k = BezKomentarza $l; $nr = $i + 1
    if ($k -match '\?\.(?!\s*Invoke\b)' -or $k -match '\?\?(?!=)') {
        $uwagi.Add("L${nr} [a, konwencje §4] ``?.``/``??`` — jeśli to obiekt Unity (Component, GameObject, SO), użyj jawnego ``!= null``.")
    }
    if ($kodGry -and $goraca.Contains($i)) {
        if ($k -match '\b(GetComponents?(InChildren|InParent)?\s*[<(]|FindObjectsOfType|FindObjectOfType|FindFirstObjectByType|FindAnyObjectByType|FindObjectsByType|GameObject\.Find|FindWithTag|FindGameObjectsWithTag)') {
            $uwagi.Add("L${nr} [b, wydajność] wyszukiwanie komponentu/obiektu w metodzie co klatkę — zapamiętaj referencję w Awake/Start.")
        }
        if ($maLinq -and $k -match '\.(Where|Select|Any|All|First|FirstOrDefault|Last|OrderBy|OrderByDescending|ToList|ToArray|Count|Sum|Max|Min|GroupBy|Distinct)\s*\(') {
            $uwagi.Add("L${nr} [c, wydajność] LINQ w metodzie co klatkę — alokacje; użyj pętli.")
        }
        if ($k -match '\bCamera\.main\b') { $uwagi.Add("L${nr} [d, wydajność] ``Camera.main`` co klatkę — zapamiętaj kamerę w polu.") }
    }
    if ($k -match '\bstatic\s+(\w+)\s+Instance\b' -and $singletony -notcontains $Matches[1]) {
        $uwagi.Add("L${nr} [h, Z5] singleton ``$($Matches[1])`` spoza rejestru (dozwolone: $($singletony -join ', ')) — wymaga wniosku.")
    }
    if ($kodGry -and $k -match '\b(AudioSource|AudioClip|AudioMixer)\b') {
        $uwagi.Add("L${nr} [k, Z9] ``$($Matches[1])`` — dźwięk wyłącznie przez FMOD (AudioManager / StudioEventEmitter / FMODEvents).")
    }
}

# --- e: zmiana nazwy pola serializowanego ---
if ($stareTekst) {
    $wzor = '(?m)^\s*(?:\[[^\]]*\]\s*)*(?:public|\[SerializeField\][^;=\n]*?)\s+(?:readonly\s+)?[\w<>\[\],\.]+\s+(\w+)\s*(?:=[^;]*)?;'
    $stareP = @([regex]::Matches($stareTekst, $wzor) | ForEach-Object { $_.Groups[1].Value })
    $noweP  = @([regex]::Matches($noweTekst, $wzor) | ForEach-Object { $_.Groups[1].Value })
    $usuniete = @($stareP | Where-Object { $noweP -notcontains $_ })
    $dodane   = @($noweP | Where-Object { $stareP -notcontains $_ })
    if ($usuniete.Count -and $dodane.Count -and $noweTekst -notmatch 'FormerlySerializedAs') {
        $uwagi.Add("[e, konwencje §3] zmiana nazwy pola serializowanego ($($usuniete -join ', ') → $($dodane -join ', ')) bez ``[FormerlySerializedAs]`` — wartości z Inspektora i assetów przepadną.")
    }
}

# --- f: nazwa pliku vs klasa ---
if ($noweTekst -match '\bclass\s') {
    $nazwaPliku = [System.IO.Path]::GetFileNameWithoutExtension($path)
    $klasy = @([regex]::Matches($tekst, '\bclass\s+(\w+)\s*(?:<[^>]+>)?\s*:\s*[^{]*\b(MonoBehaviour|ScriptableObject)\b') | ForEach-Object { $_.Groups[1].Value })
    if ($klasy.Count -and $klasy -notcontains $nazwaPliku) {
        $uwagi.Add("[f] plik ``$nazwaPliku.cs`` zawiera klasę Unity ``$($klasy -join ', ')`` o innej nazwie — Unity nie podepnie skryptu.")
    }
}

if ($uwagi.Count -eq 0) { exit 0 }
[Console]::Error.WriteLine("[kontrola jakości] $rel — ostrzeżenia (plik zapisany):")
foreach ($u in $uwagi) { [Console]::Error.WriteLine("- $u") }
[Console]::Error.WriteLine("Popraw, jeśli dotyczy. Fałszywy alarm (np. ``?.`` na zwykłym obiekcie C#) — pomiń i wspomnij w raporcie.")
exit 2
