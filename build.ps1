param([switch]$Portable, [switch]$WpfTests)
$ErrorActionPreference = 'Stop'
$localDotnet = Join-Path $PSScriptRoot '.dotnet/dotnet.exe'
$dotnet = if (Test-Path $localDotnet) { $localDotnet } else { 'dotnet' }
Push-Location $PSScriptRoot
try {
    & $dotnet build BasicEdu.sln -c Release
    if ($LASTEXITCODE) { throw 'Sestavení řešení selhalo.' }
    & $dotnet BasicEdu.Tests/bin/Release/net10.0/BasicEdu.Tests.dll
    if ($LASTEXITCODE) { throw 'Testy selhaly.' }
    if ($WpfTests) {
        & $dotnet BasicEdu.WpfTests/bin/Release/net10.0-windows/BasicEdu.WpfTests.dll
        if ($LASTEXITCODE) { throw 'WPF testy selhaly.' }
    }
    if ($Portable) {
        & $dotnet publish BasicEdu.App -c Release -r win-x64 --self-contained true -o dist/BasicEdu-1.0-win-x64
        if ($LASTEXITCODE) { throw 'Portable sestavení selhalo.' }
        Copy-Item samples dist/BasicEdu-1.0-win-x64 -Recurse -Force
        Copy-Item licenses dist/BasicEdu-1.0-win-x64 -Recurse -Force
        Copy-Item docs dist/BasicEdu-1.0-win-x64 -Recurse -Force
        Copy-Item README.md,LICENSE,PRIVACY.md,THIRD_PARTY_NOTICES.md,CODE_SIGNING.md,CONTRIBUTING.md dist/BasicEdu-1.0-win-x64 -Force
        $sdkVersion = & $dotnet --version
        $sdkLine = (& $dotnet --list-sdks | Where-Object { $_.StartsWith("$sdkVersion ") } | Select-Object -First 1)
        if ($sdkLine -match '\[(.+)\]') {
            $dotnetRoot = Split-Path $Matches[1] -Parent
            foreach ($licenseName in @('LICENSE.txt', 'ThirdPartyNotices.txt')) {
                $runtimeLicense = Join-Path $dotnetRoot $licenseName
                if (Test-Path $runtimeLicense) { Copy-Item $runtimeLicense "dist/BasicEdu-1.0-win-x64/licenses/dotnet-$licenseName" -Force }
            }
        }
        Compress-Archive dist/BasicEdu-1.0-win-x64 dist/BasicEdu-1.0-win-x64.zip -Force
        $hash = (Get-FileHash dist/BasicEdu-1.0-win-x64.zip -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  BasicEdu-1.0-win-x64.zip" | Set-Content dist/BasicEdu-1.0-win-x64.zip.sha256 -Encoding ascii
    }
} finally { Pop-Location }
