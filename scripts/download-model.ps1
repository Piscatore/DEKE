#!/usr/bin/env pwsh
# Downloads the all-MiniLM-L6-v2 ONNX model and vocabulary for DEKE embeddings.

$ModelDir = Join-Path $PSScriptRoot ".." "models" "all-MiniLM-L6-v2"
$ModelFile = Join-Path $ModelDir "model.onnx"
$VocabFile = Join-Path $ModelDir "vocab.txt"

$BaseUrl = "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx"

if (-not (Test-Path $ModelDir)) {
    New-Item -ItemType Directory -Path $ModelDir -Force | Out-Null
    Write-Host "Created directory: $ModelDir"
}

if (Test-Path $ModelFile) {
    Write-Host "Model already exists: $ModelFile (skipping)"
} else {
    Write-Host "Downloading model.onnx..."
    Invoke-WebRequest -Uri "$BaseUrl/model.onnx" -OutFile $ModelFile
    Write-Host "Downloaded model.onnx"
}

if (Test-Path $VocabFile) {
    Write-Host "Vocab already exists: $VocabFile (skipping)"
} else {
    Write-Host "Downloading vocab.txt..."
    Invoke-WebRequest -Uri "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/vocab.txt" -OutFile $VocabFile
    Write-Host "Downloaded vocab.txt"
}

Write-Host ""
Write-Host "Files:"
Get-Item $ModelFile, $VocabFile | ForEach-Object {
    Write-Host ("  {0} ({1:N2} MB)" -f $_.Name, ($_.Length / 1MB))
}
Write-Host ""
Write-Host "Done."
