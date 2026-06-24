#Requires -Version 5.1
<#
.SYNOPSIS
  构建 RocketModFix Release 模块产物（支持 win-x64 / linux-x64）。

.EXAMPLE
  .\build-release.ps1 -Runtime win-x64 -Zip
  .\build-release.ps1 -Runtime linux-x64 -Version 4.25.2 -Zip
  .\build-release.ps1 -Runtime win-x64 -DeployTo "F:\SteamLibrary\steamapps\common\Unturned\Modules"
  .\build-release.ps1 -DeployU3DSTest
    构建 win-x64、部署到本机 U3DS Modules，并同步测试启动脚本
#>
[CmdletBinding()]
param(
    [string]$Version = "4.25.2",
    [ValidateSet("win-x64", "linux-x64")]
    [string]$Runtime = "win-x64",
    [ValidateSet("Both", "Unturned", "AutoInstaller")]
    [string]$Target = "Both",
    [switch]$Zip,
    [string]$DeployTo,
    [string]$U3DSRoot = $(if ($env:ROCKETMODFIX_U3DS_ROOT) { $env:ROCKETMODFIX_U3DS_ROOT } else { "F:\SteamLibrary\steamapps\common\U3DS" }),
    [switch]$DeployU3DSTest,
    [switch]$DeployTestConfigs,
    [string]$TestServerId = "RocketModTest",
    [switch]$ForceTestConfigs,
    [switch]$ForceTestPermissions,
    [switch]$SyncTestScript
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot
$VersionProp = "-p:RocketModFixVersion=$Version"
$BuildArgs = @("build", "-c", "Release", "-r", $Runtime, $VersionProp)

function Sync-U3DSTestStartScript {
    param([string]$DestinationRoot)
    $src = Join-Path $Root "scripts\start-u3ds-test-server.bat"
    if (-not (Test-Path $src)) {
        throw "Test start script not found: $src"
    }
    if (-not (Test-Path (Join-Path $DestinationRoot "Unturned.exe"))) {
        throw "U3DS root not found (Unturned.exe missing): $DestinationRoot"
    }
    $dst = Join-Path $DestinationRoot "start-u3ds-test-server.bat"
    Copy-Item $src $dst -Force
    Write-Host "Test script -> $dst" -ForegroundColor Green
}

if ($DeployU3DSTest) {
    if ($Runtime -ne "win-x64") {
        Write-Warning "DeployU3DSTest forces Runtime=win-x64"
        $Runtime = "win-x64"
    }
    if ($Target -eq "Both") {
        $Target = "Unturned"
    }
    $DeployTo = Join-Path $U3DSRoot "Modules"
    $SyncTestScript = $true
}

function Invoke-ModuleBuild {
    param([string]$ProjectPath)
    Write-Host ">> dotnet build $ProjectPath ($Runtime)" -ForegroundColor Cyan
    & dotnet @BuildArgs $ProjectPath
    if ($LASTEXITCODE -ne 0) { throw "Build failed: $ProjectPath" }
}

Push-Location $Root
try {
    if ($Target -in @("Both", "AutoInstaller")) {
        Invoke-ModuleBuild "Rocket.AutoInstaller/Rocket.AutoInstaller.csproj"
    }
    if ($Target -in @("Both", "Unturned")) {
        Invoke-ModuleBuild "Rocket.Unturned/Rocket.Unturned.csproj"
    }

    $OutRoot = Join-Path $Root "dist\$Runtime"
    New-Item -ItemType Directory -Force -Path $OutRoot | Out-Null

    if ($Target -in @("Both", "AutoInstaller")) {
        $src = Join-Path $Root "Rocket.AutoInstaller\bin\Release\net481\$Runtime\Rocket.AutoInstaller"
        $dst = Join-Path $OutRoot "Rocket.AutoInstaller"
        if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }
        Copy-Item $src $dst -Recurse -Force
        Write-Host "AutoInstaller -> $dst" -ForegroundColor Green
    }

    if ($Target -in @("Both", "Unturned")) {
        $src = Join-Path $Root "Rocket.Unturned\bin\Release\net481\$Runtime\Rocket.Unturned"
        $dst = Join-Path $OutRoot "Rocket.Unturned"
        if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }
        Copy-Item $src $dst -Recurse -Force
        Write-Host "Rocket.Unturned -> $dst" -ForegroundColor Green
    }

    if ($Zip) {
        Get-ChildItem $OutRoot -Directory | ForEach-Object {
            $zipPath = Join-Path $OutRoot ("{0}.zip" -f $_.Name)
            if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
            Compress-Archive -Path (Join-Path $_.FullName "*") -DestinationPath $zipPath -Force
            Write-Host "Zip -> $zipPath" -ForegroundColor Green
        }
    }

    if ($DeployTo) {
        $moduleSrc = Join-Path $OutRoot "Rocket.Unturned\Rocket.Unturned"
        $moduleDst = Join-Path $DeployTo "Rocket.Unturned"
        if (-not (Test-Path $moduleSrc)) {
            throw "Deploy source not found: $moduleSrc (build Target must include Unturned)"
        }
        New-Item -ItemType Directory -Force -Path $DeployTo | Out-Null
        if (Test-Path $moduleDst) { Remove-Item $moduleDst -Recurse -Force }
        Copy-Item $moduleSrc $moduleDst -Recurse -Force
        Write-Host "Deployed Rocket.Unturned -> $moduleDst" -ForegroundColor Green
    }

    if ($SyncTestScript) {
        Sync-U3DSTestStartScript -DestinationRoot $U3DSRoot
    }

    if ($DeployTestConfigs -or $DeployU3DSTest) {
        $cfgScript = Join-Path $Root "scripts\deploy-test-configs.ps1"
        if (-not (Test-Path $cfgScript)) {
            throw "deploy-test-configs.ps1 not found: $cfgScript"
        }
        & $cfgScript -ServerId $TestServerId -U3DSRoot $U3DSRoot -ForceAll:$ForceTestConfigs -ForcePermissions:$ForceTestPermissions
    }

    Write-Host "`nDone. Version=$Version Runtime=$Runtime Output=$OutRoot" -ForegroundColor Cyan
}
finally {
    Pop-Location
}
