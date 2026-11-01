# post-md.ps1 — PostToolUse po Edit/Write/MultiEdit pliku .md w dokumentacji lub zasadach: odświeża .claude/mapa-dokumentacji.md
# (żeby kolejny agent, np. Redaktor po /spec, znalazł nową sekcję pod właściwymi numerami linii). Zero tokenów, nic nie wypisuje.
$ErrorActionPreference = 'SilentlyContinue'
$in   = [Console]::In.ReadToEnd() | ConvertFrom-Json
$path = "$($in.tool_input.file_path)"
if ($path -notmatch '\.md$') { exit 0 }
$root = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { (Get-Location).Path }
$rel  = $path.Replace('\', '/')
$r    = $root.Replace('\', '/').TrimEnd('/') + '/'
if ($rel.StartsWith($r, [StringComparison]::OrdinalIgnoreCase)) { $rel = $rel.Substring($r.Length) }
if ($rel -notmatch '^(Documentation/|Vision/|\.claude/(zasady|rules)/)') { exit 0 }
$skrypt = Join-Path $root '.claude/narzedzia/mapa-dokumentacji.ps1'
if (Test-Path $skrypt) { $null = & $skrypt -Projekt $root }
exit 0
