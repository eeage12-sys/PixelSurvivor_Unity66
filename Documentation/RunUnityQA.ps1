# Optional automation. Close this Unity project before running.
# Example: powershell -ExecutionPolicy Bypass -File Documentation\RunUnityQA.ps1
param([string]$UnityEditor = "C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Unity.exe")
$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot
$resultsPath = Join-Path $projectPath "TestResults"
if (-not (Test-Path $UnityEditor)) { throw "Unity.exe not found. Pass -UnityEditor with its full path." }
New-Item -ItemType Directory -Force -Path $resultsPath | Out-Null
function Invoke-Unity([string[]]$UnityArguments) {
    $quotedArguments = $UnityArguments | ForEach-Object { '"' + $_.Replace('"', '\"') + '"' }
    $unityProcess = Start-Process -FilePath $UnityEditor -ArgumentList $quotedArguments -Wait -PassThru
    if ($unityProcess.ExitCode -ne 0) { throw "Unity returned exit code $($unityProcess.ExitCode). Check TestResults logs." }
}
# This intentionally recreates the generated scene and prefabs. Run on a fresh copy.
Invoke-Unity @("-batchmode", "-projectPath", $projectPath, "-executeMethod", "PixelSurvivor.Editor.SurvivorSetup.Build", "-quit", "-logFile", (Join-Path $resultsPath "setup.log"))
foreach ($platform in @("EditMode", "PlayMode")) {
    # Do not pass -quit with -runTests: the runner exits when its work is finished.
    Invoke-Unity @("-batchmode", "-projectPath", $projectPath, "-runTests", "-testPlatform", $platform, "-testResults", (Join-Path $resultsPath "$platform.xml"), "-logFile", (Join-Path $resultsPath "$platform.log"))
}
Write-Host "Tests completed. Read both XML results and logs in $resultsPath."
