#!/usr/bin/env pwsh
# Fail if a deprecated glossary alias (docs/GLOSSARY.md) appears outside its
# sanctioned historical homes (decisions.md's historical table, GLOSSARY.md
# itself, INTENT.md, docs/adr/). See docs/GLOSSARY.md's Status legend --
# an ENFORCED row is one this script actually checks.

$ProjectRoot = Join-Path $PSScriptRoot ".."
$ExcludeFiles = @("decisions.md", "GLOSSARY.md", "INTENT.md")
$ExcludeDirs = @("bin", "obj", "adr")

$Checks = @(
    @{ Pattern = 'Package 3|\bP3\b'; Label = 'deprecated alias of "Evolution Engine" -- use "Evolution Engine" or "EE-N"' },
    @{ Pattern = 'Package 1 Phase N|P1-PhaseN'; Label = 'deprecated alias of "P1-N"' },
    @{ Pattern = 'IChunkingService|SemanticChunkingService'; Label = 'deprecated alias of "IChunker / SemanticChunkerAdapter"' }
)

function Get-ScanFiles {
    param([string]$Root)
    Get-ChildItem -Path (Join-Path $Root "docs"), (Join-Path $Root "src") -Recurse -File -Include "*.md", "*.cs" |
        Where-Object {
            $relDirs = $_.DirectoryName.Split([IO.Path]::DirectorySeparatorChar)
            ($ExcludeFiles -notcontains $_.Name) -and (-not ($ExcludeDirs | Where-Object { $relDirs -contains $_ }))
        }
}

$Found = $false
$Files = Get-ScanFiles -Root $ProjectRoot

foreach ($check in $Checks) {
    $matches = $Files | Select-String -Pattern $check.Pattern
    if ($matches) {
        Write-Host "GLOSSARY VIOLATION ($($check.Label)):"
        $matches | ForEach-Object { Write-Host "  $($_.Path):$($_.LineNumber): $($_.Line.Trim())" }
        Write-Host ""
        $Found = $true
    }
}

if ($Found) {
    Write-Host "Glossary lint failed -- deprecated terms found above. See docs/GLOSSARY.md for canonical names."
    exit 1
}

Write-Host "Glossary lint passed -- no deprecated aliases found."
