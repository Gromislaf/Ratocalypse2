# UserPromptSubmit: reaguje WYŁĄCZNIE na tekst wpisany przez autora.
#   /manifest-ok      -> odblokowuje zapis sceny przez MCP na 60 minut
#   /pomiary start    -> włącza czasową ochronę systemu pomiarowego
#   /pomiary stop     -> wyłącza ją
$ErrorActionPreference = 'SilentlyContinue'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$in   = [Console]::In.ReadToEnd() | ConvertFrom-Json
$p    = "$($in.prompt)".Trim()
$root = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }
$stan = Join-Path $root '.claude/stan'
New-Item -ItemType Directory -Force -Path $stan | Out-Null

if ($p -match '^/manifest-ok\b') {
    $exp = (Get-Date).AddMinutes(60)
    @{ created = (Get-Date).ToString('o'); expires = $exp.ToString('o') } | ConvertTo-Json | Set-Content (Join-Path $stan 'manifest-ok.json') -Encoding UTF8
    Write-Output "[hook] Bramka manifestu ODBLOKOWANA do $($exp.ToString('HH:mm')) — zapis sceny dozwolony tylko dla zatwierdzonego manifestu."
}
elseif ($p -match '^/pomiary\s+start\b') {
    @{ started = (Get-Date).ToString('o') } | ConvertTo-Json | Set-Content (Join-Path $stan 'pomiary.json') -Encoding UTF8
    Write-Output "[hook] Seria pomiarów AKTYWNA — pliki systemu pomiarowego zablokowane do /pomiary stop."
}
elseif ($p -match '^/pomiary\s+stop\b') {
    Remove-Item (Join-Path $stan 'pomiary.json') -Force
    Write-Output "[hook] Seria pomiarów ZAKOŃCZONA — ochrona czasowa wyłączona."
}
exit 0
