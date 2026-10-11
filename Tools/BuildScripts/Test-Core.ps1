param([string]$Compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe')
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Push-Location -LiteralPath $projectRoot
try {
    if (-not (Test-Path -LiteralPath $Compiler)) { throw "C# compiler missing: $Compiler" }
    New-Item -ItemType Directory -Force -Path 'TestResults' | Out-Null
    $sources = @(Get-ChildItem -LiteralPath 'Assets/_WARWW3/Scripts/Core' -Filter '*.cs' | ForEach-Object { $_.FullName })
    $sources += Join-Path $projectRoot 'Assets/_WARWW3/Tests/Shared/CoreChecks.cs'
    $sources += Join-Path $projectRoot 'Tools/Tests/StandaloneRunner.cs'
    & $Compiler /nologo /warnaserror /optimize+ /codepage:65001 /out:TestResults/CoreChecks.exe $sources
    if ($LASTEXITCODE -ne 0) { throw 'C# domain compilation failed.' }
    & './TestResults/CoreChecks.exe'
    if ($LASTEXITCODE -ne 0) { throw 'Core tests failed.' }
} finally { Pop-Location }
