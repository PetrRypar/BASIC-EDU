param([string]$Destination = 'dist/BasicEdu-1.0-source-public.zip')
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$target = [IO.Path]::GetFullPath((Join-Path $root $Destination))
$requiredRoot = [IO.Path]::GetFullPath((Join-Path $root 'dist')) + [IO.Path]::DirectorySeparatorChar
if (!$target.StartsWith($requiredRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Archiv musí být uvnitř dist.' }
$files = @('README.md','LICENSE','PRIVACY.md','THIRD_PARTY_NOTICES.md','CODE_SIGNING.md',
    'CONTRIBUTING.md','Directory.Build.props','NuGet.Config','BasicEdu.sln','build.ps1','export-source.ps1','.gitignore')
# Explicit source extensions and roots exclude SDKs, credentials, IDE settings and build outputs.
foreach ($folder in @('BasicEdu.App','BasicEdu.Core','BasicEdu.Tests','BasicEdu.WpfTests','samples','docs','licenses','.github')) {
    $files += Get-ChildItem -LiteralPath (Join-Path $root $folder) -File -Recurse |
        Where-Object { $_.FullName -notmatch '\\(bin|obj|\.vs|\.git)\\' -and $_.Extension -in @('.cs','.csproj','.xaml','.bas','.md','.txt','.yml') } |
        ForEach-Object { [IO.Path]::GetRelativePath($root, $_.FullName) }
}
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target)) | Out-Null
# Replace this one validated archive only; never delete source directories.
if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target }
$archive = [IO.Compression.ZipFile]::Open($target, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in $files | Sort-Object -Unique) {
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,(Join-Path $root $file),$file.Replace('\','/')) | Out-Null
    }
} finally { $archive.Dispose() }
Write-Output $target
