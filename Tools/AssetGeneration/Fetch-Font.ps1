param()
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$target = Join-Path $projectRoot 'Assets/_WARWW3/Resources/Fonts'
New-Item -ItemType Directory -Force -Path $target | Out-Null
$files = @{ 'Sarabun-Regular.ttf' = 'https://raw.githubusercontent.com/google/fonts/main/ofl/sarabun/Sarabun-Regular.ttf'; 'OFL.txt' = 'https://raw.githubusercontent.com/google/fonts/main/ofl/sarabun/OFL.txt' }
foreach ($name in $files.Keys) {
    $path = Join-Path $target $name
    if (-not (Test-Path -LiteralPath $path)) { Invoke-WebRequest -UseBasicParsing -Uri $files[$name] -OutFile $path }
    Get-FileHash -LiteralPath $path -Algorithm SHA256 | Select-Object Path,Hash
}
