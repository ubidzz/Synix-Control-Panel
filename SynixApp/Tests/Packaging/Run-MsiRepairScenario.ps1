# Exercise Windows Installer against a disposable, uniquely identified fixture.
# Never install, repair, uninstall, or replace the user's Synix application.
[CmdletBinding()]
param([string]$ProjectDirectory = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)

$ErrorActionPreference = 'Stop'
$scenarioId = [Guid]::NewGuid().ToString('N')
$scenarioRoot = Join-Path ([IO.Path]::GetTempPath()) "SynixMsiRepairScenario-$scenarioId"
$fixtureName = "Synix Repair Fixture $scenarioId"
$fixtureRegistry = "Software\ubidzz\SynixRepairFixture\$scenarioId"
$upgradeCode = [Guid]::NewGuid().ToString('B')
$componentCode = [Guid]::NewGuid().ToString('B')
$products = [Collections.Generic.List[string]]::new()
$productSources = @{}
$installer = New-Object -ComObject WindowsInstaller.Installer

function Assert-Scenario([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
    Write-Output "PASS: $message"
}
function Read-Msi([string]$path, [string]$query) {
    $database = $installer.OpenDatabase($path, 0)
    $view = $database.OpenView($query)
    try {
        [void]$view.Execute()
        while ($null -ne ($record = $view.Fetch())) {
            try { $record.StringData(1) } finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($record) }
        }
    } finally {
        [void]$view.Close()
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($database)
    }
}
function Invoke-Msi([string]$arguments, [string]$name, [int[]]$expected = @(0, 3010)) {
    $log = Join-Path $scenarioRoot "$name.log"
    $process = Start-Process -FilePath (Join-Path $env:WINDIR 'System32/msiexec.exe') -ArgumentList "$arguments /qn /norestart /L*v `"$log`"" -WindowStyle Hidden -Wait -PassThru
    Assert-Scenario ($process.ExitCode -in $expected) "$name returned expected result ($($process.ExitCode)); log: $log"
    Assert-Scenario (-not (Select-String -LiteralPath $log -Pattern 'Error 1706|Error 1714' -Quiet)) "$name did not request the old installation source"
}
function Invoke-DotNet([string[]]$arguments) {
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) { throw "Fixture build failed: $LASTEXITCODE" }
}
function New-FixtureMsi([string]$version, [string]$buildName) {
    $output = Join-Path $scenarioRoot $buildName
    Invoke-DotNet @('build', (Join-Path $scenarioRoot 'package/SynixInstaller.wixproj'), '-c', 'Release', '--no-incremental', '--nologo', "-p:Version=$version", "-p:SynixPublishDir=$scenarioRoot/payload", "-p:SynixProjectDir=$ProjectDirectory", '-p:SynixAssemblyName=Fixture', "-p:OutputPath=$output/") | Out-Host
    $msi = Join-Path $output 'en-US/SynixSetup.msi'
    $product = Read-Msi $msi 'SELECT `Value` FROM `Property` WHERE `Property` = ''ProductCode'''
    if ($product -isnot [string] -or $product -notmatch '^\{[0-9A-Fa-f-]{36}\}$') { throw "Invalid fixture ProductCode: $product" }
    $products.Add($product)
    $productSources[$product] = $msi
    return $msi
}
try {
    [void](New-Item -ItemType Directory -Path $scenarioRoot)
    foreach ($folder in @('package', 'dummy', 'source', 'preserved', 'servers')) {
        [void](New-Item -ItemType Directory -Path (Join-Path $scenarioRoot $folder))
    }
    # XML fixture generation changes every identity that could overlap Synix.
    foreach ($name in @('Package.wxs', 'SameVersionMaintenance.wxs', 'Package.en-us.wxl', 'SynixInstaller.wixproj')) {
        $source = [IO.File]::ReadAllText((Join-Path $ProjectDirectory "Packaging/MSI/$name"))
        $source = $source.Replace('Software\ubidzz\Synix Control Panel', $fixtureRegistry).
            Replace('Synix Control Panel', $fixtureName).
            Replace('{e369556b-db95-4d9b-8e86-2b7d50dcd328}', $upgradeCode).
            Replace('{015b398b-e6e1-48e6-b03a-f6ddb52f0998}', $componentCode)
        if ($name -eq 'Package.wxs') {
            $source = $source.Replace('<InstallExecuteSequence>', '<CustomAction Id="FixtureForceRollback" FileRef="SynixExecutable" ExeCommand="--fail" Execute="deferred" Impersonate="yes" Return="check" /><InstallExecuteSequence><Custom Action="FixtureForceRollback" After="InstallFiles" Condition="FIXTURE_FAIL = &quot;1&quot;" />')
        }
        [IO.File]::WriteAllText((Join-Path $scenarioRoot "package/$name"), $source)
    }
    [IO.File]::WriteAllText((Join-Path $scenarioRoot 'dummy/Fixture.csproj'), @'
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><AssemblyName>Fixture</AssemblyName><Version>1.0.26</Version><PublishSingleFile>true</PublishSingleFile></PropertyGroup>
</Project>
'@)
    [IO.File]::WriteAllText((Join-Path $scenarioRoot 'dummy/Program.cs'), @'
if (args.Contains("--synix-uninstall")) File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "cleanup-invoked"), "fixture only");
if (args.Contains("--fail")) Environment.Exit(1);
#if REBUILT
Console.WriteLine("rebuilt payload");
#else
Console.WriteLine("original payload");
#endif
'@)
    $appFolder = Join-Path $scenarioRoot 'app'
    $serverMarker = Join-Path $scenarioRoot 'servers/keep-server-and-backup.txt'
    [IO.File]::WriteAllText($serverMarker, 'These files must survive every fixture operation.')
    Invoke-DotNet @('publish', (Join-Path $scenarioRoot 'dummy/Fixture.csproj'), '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-o', (Join-Path $scenarioRoot 'payload')) | Out-Host
    $original = New-FixtureMsi '1.0.26' 'original'
    $sourceMsi = Join-Path $scenarioRoot 'source/SynixSetup.msi'
    Copy-Item -LiteralPath $original -Destination $sourceMsi
    Invoke-Msi "/i `"$sourceMsi`" INSTALLFOLDER=`"$appFolder`"" 'initial-install'
    $oldHash = (Get-FileHash -LiteralPath (Join-Path $appFolder 'Fixture.exe')).Hash

    Invoke-DotNet @('publish', (Join-Path $scenarioRoot 'dummy/Fixture.csproj'), '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-p:DefineConstants=REBUILT', '-o', (Join-Path $scenarioRoot 'payload')) | Out-Host
    $rebuilt = New-FixtureMsi '1.0.26' 'rebuilt'
    $repairNext = Read-Msi $rebuilt 'SELECT `Argument` FROM `ControlEvent` WHERE `Dialog_` = ''SynixSameVersionDlg'' AND `Control_` = ''Repair'' AND `Event` = ''NewDialog'''
    Assert-Scenario ($repairNext -eq 'LicenseAgreementDlg') 'The compiled Repair button continues through license acceptance in the current installer'
    $welcomeCondition = Read-Msi $rebuilt 'SELECT `Condition` FROM `InstallUISequence` WHERE `Action` = ''WelcomeDlg'''
    Assert-Scenario ($welcomeCondition -eq '(NOT Installed OR PATCH) AND NOT SYNIX_SAME_VERSION_FOUND') 'Repair does not open a second Install wizard'
    $newHash = (Get-FileHash -LiteralPath (Join-Path $scenarioRoot 'payload/Fixture.exe')).Hash
    Assert-Scenario ($oldHash -ne $newHash) 'The rebuilt payload differs despite the same displayed version'
    # Reproduce Publish replacing SynixSetup.msi at the original source location.
    # All paths are literal children of the newly created fixture directory.
    Copy-Item -LiteralPath $rebuilt -Destination $sourceMsi -Force
    Invoke-Msi "/i `"$sourceMsi`"" 'same-version-without-consent' @(1603)
    Assert-Scenario ((Get-FileHash -LiteralPath (Join-Path $appFolder 'Fixture.exe')).Hash -eq $oldHash) 'No replacement occurs without explicit Repair'
    Invoke-Msi "/i `"$sourceMsi`" SYNIX_REPAIR_CONFIRMED=1 INSTALLFOLDER=`"$scenarioRoot/other-app`"" 'repair-cannot-relocate' @(1603)
    Invoke-Msi "/i `"$sourceMsi`" SYNIX_REPAIR_CONFIRMED=1 FIXTURE_FAIL=1" 'failed-repair-rolls-back' @(1603)
    Assert-Scenario ((Get-FileHash -LiteralPath (Join-Path $appFolder 'Fixture.exe')).Hash -eq $oldHash) 'A failed replacement restores the old application even after its original source was overwritten'
    Assert-Scenario ($installer.ProductState($products[0]) -eq 5) 'Rollback preserves the original product registration'
    Invoke-Msi "/i `"$sourceMsi`" SYNIX_REPAIR_CONFIRMED=1" 'rebuilt-repair'
    Assert-Scenario ((Get-FileHash -LiteralPath (Join-Path $appFolder 'Fixture.exe')).Hash -eq $newHash) 'Repair used the new payload in the original app location'
    Assert-Scenario (-not (Test-Path -LiteralPath (Join-Path $appFolder 'cleanup-invoked'))) 'Repair did not invoke uninstall data cleanup'
    Assert-Scenario ($installer.ProductState($products[0]) -ne 5 -and $installer.ProductState($products[1]) -eq 5) 'Exactly the replacement product is registered'
    Invoke-Msi "/fa `"$sourceMsi`"" 'exact-package-repair'

    $higher = New-FixtureMsi '1.0.27' 'higher'
    Invoke-Msi "/i `"$higher`" INSTALLFOLDER=`"$scenarioRoot/other-app`"" 'update-cannot-relocate' @(1603)
    Invoke-Msi "/i `"$higher`"" 'higher-version-update'
    Assert-Scenario ($installer.ProductState($products[1]) -ne 5 -and $installer.ProductState($products[2]) -eq 5) 'A higher version updates the existing installation'
    Assert-Scenario (-not (Test-Path -LiteralPath (Join-Path $appFolder 'cleanup-invoked'))) 'Update did not invoke uninstall data cleanup'
    Invoke-Msi "/i `"$sourceMsi`" SYNIX_REPAIR_CONFIRMED=1" 'downgrade-blocked' @(1603)
    Assert-Scenario ([IO.File]::ReadAllText($serverMarker) -eq 'These files must survive every fixture operation.') 'Server and backup sentinel files are unchanged'
    Invoke-Msi "/x $($products[2])" 'uninstall'
    Assert-Scenario (Test-Path -LiteralPath (Join-Path $appFolder 'cleanup-invoked')) 'Explicit uninstall invokes the cleanup helper'
    Assert-Scenario (-not (Test-Path -LiteralPath (Join-Path $appFolder 'Fixture.exe'))) 'Uninstall removes the application payload'
    Assert-Scenario ([IO.File]::ReadAllText($serverMarker) -eq 'These files must survive every fixture operation.') 'Uninstall keeps the server and backup sentinel'
    Write-Output "Scenario completed. Fixture files and logs: $scenarioRoot"
} finally {
    # Only the GUIDs read from these newly built fixture packages may be removed.
    foreach ($product in $products) {
        if ($installer.ProductState($product) -in @(1, 5)) {
            # An interrupted test can leave an advertised fixture. Restore its
            # original source solely inside this fixture before removing it.
            Copy-Item -LiteralPath $productSources[$product] -Destination (Join-Path $scenarioRoot 'source/SynixSetup.msi') -Force
            # Test cleanup is not part of the application workflow. Suppress the
            # fixture helper if a deliberately broken transaction damaged it.
            Invoke-Msi "/x $product UPGRADINGPRODUCTCODE=$upgradeCode" ('cleanup-' + $product.Trim('{}'))
        }
    }
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)
}
