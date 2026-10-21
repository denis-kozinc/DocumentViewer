$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent
$publishDir = Join-Path $root 'publish'

$iscc = Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'
if (-not (Test-Path $iscc)) {
    throw "Inno Setup 6 not found at $iscc"
}

dotnet publish (Join-Path $root 'DocumentViewer.csproj') -c Release -r win-x64 --self-contained true -o $publishDir
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

& $iscc (Join-Path $PSScriptRoot 'DocumentViewer.iss')
if ($LASTEXITCODE -ne 0) { throw 'Inno Setup compile failed' }

Write-Host "Installer: $(Join-Path $PSScriptRoot 'output\DocumentViewer-Setup.exe')"
