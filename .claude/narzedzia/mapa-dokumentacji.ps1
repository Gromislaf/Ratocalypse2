# mapa-dokumentacji.ps1 — generuje .claude/mapa-dokumentacji.md: spis nagłówków z zakresami linii dla dokumentacji i zasad.
# Uruchamianie z katalogu projektu:  pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/narzedzia/mapa-dokumentacji.ps1
# Mapy NIE edytuje się ręcznie — zawsze generuje się ją od nowa (hook na starcie sesji, gdy jakiś .md jest nowszy).

param(
    [string]$Projekt = (Get-Location).Path,
    [string]$Wyjscie = '.claude/mapa-dokumentacji.md'
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Set-Location $Projekt

$korzenie = @('Documentation', 'Vision', '.claude/zasady', '.claude/rules') | Where-Object { Test-Path $_ }
$pliki = foreach ($k in $korzenie) { Get-ChildItem -Path $k -Recurse -Filter *.md -File }
$pliki = @($pliki | Sort-Object FullName)

$gdd = [System.Collections.Generic.List[string]]::new()
$sekcje = [System.Collections.Generic.List[string]]::new()
$liczbaNagl = 0

foreach ($p in $pliki) {
    $rel = [System.IO.Path]::GetRelativePath($Projekt, $p.FullName) -replace '\\', '/'
    $W = [System.IO.File]::ReadAllLines($p.FullName)
    $n = $W.Count
    # nagłówki # … ### poza blokami kodu
    $nagl = [System.Collections.Generic.List[object]]::new()
    $wKodzie = $false
    for ($i = 0; $i -lt $n; $i++) {
        $t = $W[$i]
        if ($t -match '^\s*```') { $wKodzie = -not $wKodzie; continue }
        if ($wKodzie) { continue }
        if ($t -match '^(#{1,3})\s+(.+?)\s*#*\s*$') { $nagl.Add(@{ poz = $Matches[1].Length; tytul = $Matches[2]; od = $i + 1 }) }
    }
    # koniec zakresu = linia przed następnym nagłówkiem tego samego lub wyższego poziomu
    for ($j = 0; $j -lt $nagl.Count; $j++) {
        $do = $n
        for ($k = $j + 1; $k -lt $nagl.Count; $k++) { if ($nagl[$k].poz -le $nagl[$j].poz) { $do = $nagl[$k].od - 1; break } }
        $nagl[$j].do = $do
    }
    $kb = [math]::Round($p.Length / 1KB, 1)
    $sekcje.Add('')
    $sekcje.Add("### $rel — $kb KB, $n linii$(if ($kb -gt 8) { ' (> 8 KB: tylko fragmentami)' })")
    if ($nagl.Count -eq 0) { $sekcje.Add('- (brak nagłówków)'); continue }
    foreach ($h in $nagl) {
        $liczbaNagl++
        $dop = ''
        if ($h.tytul -match '^(GDD-[A-Z]+-\d+)\s*(.*)$') {
            $id = $Matches[1]; $nazwa = $Matches[2]
            $status = ''; $wersja = ''
            for ($i = $h.od; $i -lt [math]::Min($h.od + 8, $h.do); $i++) {
                if ($W[$i] -match '^\*\*Status:?\*\*:?\s*(.+)$') { $status = $Matches[1].Trim() }
                if ($W[$i] -match '^\*\*Wersja:?\*\*:?\s*(.+)$') { $wersja = $Matches[1].Trim() }
            }
            $dop = " [$status$(if ($wersja) { " $wersja" })]"
            $gdd.Add("| $id | $nazwa | $status | $wersja | ``${rel}:$($h.od)-$($h.do)`` |")
        }
        $wciecie = '  ' * [math]::Max(0, $h.poz - 1)
        $sekcje.Add("- $wciecie$($h.od)-$($h.do) $('#' * $h.poz) $($h.tytul)$dop")
    }
}

$out = [System.Collections.Generic.List[string]]::new()
$out.Add('# Mapa dokumentacji — generowana automatycznie, nie edytuj')
$out.Add('')
$out.Add('> Najpierw `Grep` w tym pliku (ID `GDD-…`, nazwa systemu, `Z7`, `§3a`), potem `Read` pliku z `offset` = pierwsza liczba i `limit` = różnica + 1.')
$out.Add("> Pliki: $($pliki.Count) · nagłówki: $liczbaNagl · elementy GDD: $($gdd.Count). Źródła: $($korzenie -join ', ').")
$out.Add('')
$out.Add('## Elementy GDD')
$out.Add('')
if ($gdd.Count) {
    $out.Add('| ID | Nazwa | Status | Wersja | Plik:linie |')
    $out.Add('|---|---|---|---|---|')
    $out.AddRange([string[]]$gdd)
} else { $out.Add('(brak)') }
$out.Add('')
$out.Add('## Pliki (linie od-do, nagłówek)')
$out.AddRange([string[]]$sekcje)

$cel = Join-Path $Projekt $Wyjscie
[System.IO.File]::WriteAllText($cel, (($out -join "`n") + "`n"), [System.Text.UTF8Encoding]::new($false))
Write-Output "Mapa dokumentacji: $Wyjscie ($($pliki.Count) plików, $liczbaNagl nagłówków, $($gdd.Count) elementów GDD)"
