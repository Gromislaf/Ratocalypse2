# mapa-kodu.ps1 — generuje .claude/mapa-kodu.md z kodu gry (bez czytania przez agentów).
# Uruchamianie z katalogu projektu:  pwsh -NoProfile -File .claude/narzedzia/mapa-kodu.ps1
# Mapy NIE edytuje się ręcznie — zawsze generuje się ją od nowa.

param(
    [string]$Projekt = (Get-Location).Path,
    [string]$Wyjscie = '.claude/mapa-kodu.md'
)

$ErrorActionPreference = 'Stop'
Set-Location $Projekt

$korzenie = @('Assets/Ratocalypse/Scripts', 'Assets/Ratocalypse/Editor', 'Assets/Ratocalypse/Tests') |
    Where-Object { Test-Path $_ }

$utf8Scisly = [System.Text.UTF8Encoding]::new($false, $true)
$pliki = foreach ($k in $korzenie) { Get-ChildItem -Path $k -Recurse -Filter *.cs -File }
$pliki = $pliki | Sort-Object FullName

$zdarzeniaPub = @{}   # typ -> lista klas
$zdarzeniaSub = @{}
$singletony   = [System.Collections.Generic.List[string]]::new()
$soLista      = [System.Collections.Generic.List[string]]::new()
$nieUtf8      = [System.Collections.Generic.List[string]]::new()
$dbg          = [System.Collections.Generic.List[string]]::new()
$sekcje       = [System.Collections.Generic.List[string]]::new()
$sumaLinii    = 0

function Dodaj($slownik, $klucz, $wartosc) {
    if (-not $slownik.ContainsKey($klucz)) { $slownik[$klucz] = [System.Collections.Generic.List[string]]::new() }
    if (-not $slownik[$klucz].Contains($wartosc)) { $slownik[$klucz].Add($wartosc) }
}

function Skroc([string]$parametry) {
    # "DamageInfo info, float x = 1f" -> "DamageInfo info, float x = 1f" bez wielokrotnych spacji
    return ($parametry -replace '\s+', ' ').Trim()
}

foreach ($p in $pliki) {
    $wzgl = [System.IO.Path]::GetRelativePath($Projekt, $p.FullName) -replace '\\', '/'
    $bajty = [System.IO.File]::ReadAllBytes($p.FullName)
    try { $tekst = $utf8Scisly.GetString($bajty) }
    catch { $nieUtf8.Add($wzgl); $tekst = [System.Text.Encoding]::Latin1.GetString($bajty) }
    $tekst = $tekst.TrimStart([char]0xFEFF)
    if ($tekst.Contains([char]0xFFFD) -and -not $nieUtf8.Contains($wzgl)) { $nieUtf8.Add("$wzgl (uszkodzone znaki �)") }

    # usunięcie komentarzy blokowych z zachowaniem numeracji linii
    $tekst = [regex]::Replace($tekst, '/\*.*?\*/', { param($m) ($m.Value -replace '[^\n]', '') }, 'Singleline')
    $linie = $tekst -split "`n"
    $sumaLinii += $linie.Count

    $typy = [System.Collections.Generic.List[object]]::new()
    $biezacy = $null
    $atrybutSO = $false
    $wInterfejsie = $false

    for ($i = 0; $i -lt $linie.Count; $i++) {
        $nr = $i + 1
        $surowa = $linie[$i]
        if ($surowa -match '\[DBG\]') { $dbg.Add("${wzgl}:$nr") }
        $l = ($surowa -replace '//.*$', '').TrimEnd()
        if ($l -match '^\s*$') { continue }
        if ($l -match '\[CreateAssetMenu') { $atrybutSO = $true }

        # deklaracja typu
        if ($l -match '^\s*(?:\[[^\]]*\]\s*)*(?:(?:public|internal|private|protected|static|abstract|sealed|partial|readonly)\s+)*(class|struct|interface|enum)\s+(\w+)(?:<[^>]+>)?(?:\s*:\s*([^{]+?))?\s*(?:where\s+[^{]*)?(?:\{.*)?$') {
            $biezacy = [ordered]@{
                Rodzaj = $Matches[1]; Nazwa = $Matches[2]; Baza = ($Matches[3] -replace '\s+', ' ').Trim()
                Linia = $nr; Metody = [System.Collections.Generic.List[string]]::new()
                Wlasciwosci = [System.Collections.Generic.List[string]]::new()
                Pola = [System.Collections.Generic.List[string]]::new()
                Wartosci = ''; SO = $atrybutSO
            }
            $typy.Add($biezacy)
            $wInterfejsie = ($Matches[1] -eq 'interface')
            if ($biezacy.SO -or $biezacy.Baza -match '\bScriptableObject\b') { $biezacy.SO = $true; $soLista.Add("$($biezacy.Nazwa) — ``$wzgl``") }
            $atrybutSO = $false
            if ($biezacy.Rodzaj -eq 'enum') {
                $reszta = ($linie[$i..([Math]::Min($i + 30, $linie.Count - 1))] -join ' ') -replace '//[^\n]*', ''
                if ($reszta -match '\{([^}]*)\}') { $biezacy.Wartosci = (($Matches[1] -split ',') | ForEach-Object { ($_ -replace '=.*', '').Trim() } | Where-Object { $_ }) -join ', ' }
            }
            continue
        }
        if ($null -eq $biezacy) { continue }

        # zdarzenia EventBus
        foreach ($m in [regex]::Matches($l, 'EventBus\.Publish\s*(?:<(\w+)>)?\s*\(\s*new\s+(\w+)')) {
            $typZd = if ($m.Groups[1].Success) { $m.Groups[1].Value } else { $m.Groups[2].Value }
            Dodaj $zdarzeniaPub $typZd $biezacy.Nazwa
        }
        foreach ($m in [regex]::Matches($l, 'EventBus\.Publish\s*<(\w+)>\s*\((?!\s*new)')) { Dodaj $zdarzeniaPub $m.Groups[1].Value $biezacy.Nazwa }
        foreach ($m in [regex]::Matches($l, 'EventBus\.Subscribe\s*<(\w+)>')) { Dodaj $zdarzeniaSub $m.Groups[1].Value $biezacy.Nazwa }

        # singleton
        if ($l -match '^\s*public\s+static\s+(\w+)\s+Instance\b') { $singletony.Add("$($Matches[1]) — ``$wzgl``") }

        # metody w interfejsie (bez modyfikatora)
        if ($wInterfejsie -and $l -match '^\s*([\w<>\[\],\.\?]+)\s+(\w+)\s*\(([^)]*)\)\s*;') {
            $biezacy.Metody.Add("$($Matches[1]) $($Matches[2])($(Skroc $Matches[3])) :$nr"); continue
        }
        if ($wInterfejsie -and $l -match '^\s*([\w<>\[\],\.\?]+)\s+(\w+)\s*\{\s*get') {
            $biezacy.Wlasciwosci.Add("$($Matches[1]) $($Matches[2])"); continue
        }

        # jawna implementacja interfejsu: void IDamageable.TakeDamage(DamageInfo info)
        if ($l -match '^\s*([\w<>\[\],\.\?]+)\s+(I\w+)\.(\w+)\s*\(([^)]*)\)') {
            $biezacy.Metody.Add("$($Matches[1]) $($Matches[2]).$($Matches[3])($(Skroc $Matches[4])) :$nr"); continue
        }

        # publiczne metody / konstruktory
        if ($l -match '^\s*public\s+((?:(?:static|virtual|override|abstract|async|new|sealed)\s+)*)([\w<>\[\],\.\?]+(?:\s*<[^>]+>)?)\s+(\w+)\s*(?:<[^>]+>)?\s*\(([^)]*)\)?') {
            if ($Matches[2] -notin @('class', 'struct', 'interface', 'enum', 'event', 'delegate')) {
                $g1 = $Matches[1]; $g2 = $Matches[2]; $g3 = $Matches[3]; $g4 = $Matches[4]
                $mod = if ($g1 -match 'static') { 'static ' } else { '' }
                $biezacy.Metody.Add("$mod$g2 $g3($(Skroc $g4)) :$nr"); continue
            }
        }
        if ($l -match "^\s*public\s+$($biezacy.Nazwa)\s*\(([^)]*)\)") {
            $biezacy.Metody.Add("ctor($(Skroc $Matches[1])) :$nr"); continue
        }

        # publiczne właściwości
        if ($l -match '^\s*public\s+(?:(?:static|virtual|override|abstract|new)\s+)*([\w<>\[\],\.\?]+)\s+(\w+)\s*(\{|=>)') {
            $biezacy.Wlasciwosci.Add("$($Matches[1]) $($Matches[2])"); continue
        }

        # pola: publiczne i [SerializeField]
        if ($l -match '^\s*(?:\[[^\]]*\]\s*)*(public|\[SerializeField\]\s*(?:private|protected)?)\s+(?:(?:readonly|static|const)\s+)*([\w<>\[\],\.\?]+)\s+(\w+)\s*(=|;)') {
            $nazwaPola = $Matches[3]
            if ($l -notmatch '\bstatic\b' -and $l -notmatch '\bconst\b') {
                $znak = if ($l -match 'SerializeField') { '*' } else { '' }
                $biezacy.Pola.Add("$nazwaPola$znak")
            }
        }
        elseif ($l -match '^\s*\[SerializeField\]\s*$' -and $i + 1 -lt $linie.Count) {
            if ($linie[$i + 1] -match '^\s*(?:private|protected)?\s*([\w<>\[\],\.\?]+)\s+(\w+)\s*(=|;)') { $biezacy.Pola.Add("$($Matches[2])*") }
        }
    }

    # sekcja pliku
    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine("### ``$wzgl`` ($($linie.Count) l.)")
    foreach ($t in $typy) {
        $naglowek = "- **$($t.Rodzaj) $($t.Nazwa)**"
        if ($t.Baza) { $naglowek += " : $($t.Baza)" }
        if ($t.SO) { $naglowek += ' [SO]' }
        $naglowek += " :$($t.Linia)"
        [void]$sb.AppendLine($naglowek)
        if ($t.Wartosci) { [void]$sb.AppendLine("  - wartości: $($t.Wartosci)") }
        if ($t.Pola.Count) { [void]$sb.AppendLine("  - pola: $($t.Pola -join ', ')") }
        if ($t.Wlasciwosci.Count) { [void]$sb.AppendLine("  - właściwości: $($t.Wlasciwosci -join '; ')") }
        foreach ($m in $t.Metody) { [void]$sb.AppendLine("  - $m") }
    }
    $sekcje.Add($sb.ToString())
}

# --- składanie wyniku ---
$sha = try { (git rev-parse --short HEAD 2>$null) } catch { '?' }
$galaz = try { (git branch --show-current 2>$null) } catch { '?' }
$out = [System.Text.StringBuilder]::new()
[void]$out.AppendLine('# Mapa kodu — Ratocalypse 2.5')
[void]$out.AppendLine()
[void]$out.AppendLine("> Wygenerowano automatycznie: $(Get-Date -Format 'yyyy-MM-dd HH:mm') · gałąź ``$galaz`` · commit ``$sha`` · $($pliki.Count) plików, $sumaLinii linii.")
[void]$out.AppendLine('> **Nie edytuj ręcznie** — `pwsh -NoProfile -File .claude/narzedzia/mapa-kodu.ps1`. Liczba po dwukropku = numer linii (do `Read` z `offset`).')
[void]$out.AppendLine('> Pola: `*` = `[SerializeField]` (ustawiane w Inspektorze). Mapa pokazuje API publiczne — logikę czytaj w pliku.')
[void]$out.AppendLine()

[void]$out.AppendLine('## Graf zdarzeń EventBus')
[void]$out.AppendLine()
[void]$out.AppendLine('| Zdarzenie | Publikuje | Subskrybuje |')
[void]$out.AppendLine('|---|---|---|')
$wszystkie = @($zdarzeniaPub.Keys) + @($zdarzeniaSub.Keys) | Sort-Object -Unique
foreach ($z in $wszystkie) {
    $pub = if ($zdarzeniaPub.ContainsKey($z)) { $zdarzeniaPub[$z] -join ', ' } else { '—' }
    $sub = if ($zdarzeniaSub.ContainsKey($z)) { $zdarzeniaSub[$z] -join ', ' } else { '—' }
    [void]$out.AppendLine("| $z | $pub | $sub |")
}
[void]$out.AppendLine()
[void]$out.AppendLine('## Singletony (`Instance`)')
[void]$out.AppendLine()
foreach ($s in $singletony) { [void]$out.AppendLine("- $s") }
[void]$out.AppendLine()
[void]$out.AppendLine('## ScriptableObject')
[void]$out.AppendLine()
foreach ($s in $soLista) { [void]$out.AppendLine("- $s") }
[void]$out.AppendLine()

if ($nieUtf8.Count -or $dbg.Count) {
    [void]$out.AppendLine('## Uwagi automatyczne')
    [void]$out.AppendLine()
    foreach ($n in $nieUtf8) { [void]$out.AppendLine("- ⚠️ plik nie jest w UTF-8 (konwencje): ``$n``") }
    foreach ($d in $dbg) { [void]$out.AppendLine("- ⚠️ tymczasowy log ``[DBG]``: ``$d``") }
    [void]$out.AppendLine()
}

[void]$out.AppendLine('## Pliki')
[void]$out.AppendLine()
$folder = ''
foreach ($s in $sekcje) {
    if ($s -match '### `(.+)/[^/]+\.cs`') {
        if ($Matches[1] -ne $folder) { $folder = $Matches[1]; [void]$out.AppendLine("#### $folder/"); [void]$out.AppendLine() }
    }
    [void]$out.AppendLine($s)
}

$katalog = Split-Path $Wyjscie -Parent
if ($katalog -and -not (Test-Path $katalog)) { New-Item -ItemType Directory -Path $katalog | Out-Null }
[System.IO.File]::WriteAllText((Join-Path $Projekt $Wyjscie), $out.ToString(), [System.Text.UTF8Encoding]::new($false))
Write-Output "Mapa kodu: $Wyjscie ($($pliki.Count) plików, $($wszystkie.Count) zdarzeń, $($singletony.Count) singletonów$(if ($nieUtf8.Count) { ", nie-UTF-8: $($nieUtf8.Count)" })$(if ($dbg.Count) { ", [DBG]: $($dbg.Count)" }))"
