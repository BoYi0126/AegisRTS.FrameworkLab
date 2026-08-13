param(
    [Parameter(Mandatory = $false)]
    [string]$RepoRoot = "C:\projects\Unity\AegisRTS.FrameworkLab"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Drawing

$packageName = "Infantry_Phase02_PrimaryForms_Review_v001"
$reviewParent = Join-Path $RepoRoot "docs\ArtProduction\ReviewPackages"
$packageRoot = Join-Path $reviewParent $packageName
$manifestRoot = Join-Path $packageRoot "Manifests"
$zipPath = Join-Path $reviewParent ($packageName + ".zip")
$screenshotManifest = Join-Path $manifestRoot "Screenshot_Manifest.csv"
$fileManifest = Join-Path $manifestRoot "File_Manifest.csv"
$shaList = Join-Path $manifestRoot "SHA256SUMS.txt"

$required = @(
    "README.md", "00_Phase02_Report.md", "01_Geometry_Stats.md",
    "02_Changes_From_v002.md", "03_Open_Issues.md", "04_Visual_Evidence_Index.md",
    "05_Review_Checklist.md", "Blender\CHR_Infantry_A_v003.blend",
    "Manifests\Build_Result.json", "Manifests\Geometry_Summary.json",
    "Manifests\Object_Manifest.csv", "Manifests\Bone_Manifest.csv",
    "Screenshots\Clay\01_Clay_Front.png",
    "Screenshots\Silhouette\Silhouette_Front.png",
    "Screenshots\Wireframe\Wireframe_Front.png",
    "Screenshots\ScreenSize\Silhouette_32px.png",
    "Screenshots\Comparison\v002_vs_v003_Front.png"
)
foreach ($relative in $required) {
    $candidate = Join-Path $packageRoot $relative
    if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
        throw "Missing required review file: $relative"
    }
}

$screenshotRows = foreach ($file in Get-ChildItem -LiteralPath (Join-Path $packageRoot "Screenshots") -Recurse -File -Filter "*.png" | Sort-Object FullName) {
    $image = [System.Drawing.Image]::FromFile($file.FullName)
    try {
        [pscustomobject]@{
            Path = $file.FullName.Substring($packageRoot.Length + 1).Replace("\", "/")
            Width = $image.Width
            Height = $image.Height
            Bytes = $file.Length
            SHA256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $file.FullName).Hash
        }
    }
    finally {
        $image.Dispose()
    }
}
$screenshotRows | Export-Csv -LiteralPath $screenshotManifest -NoTypeInformation -Encoding utf8

if (Test-Path -LiteralPath $fileManifest) { Remove-Item -LiteralPath $fileManifest -Force }
if (Test-Path -LiteralPath $shaList) { Remove-Item -LiteralPath $shaList -Force }
$fileRows = foreach ($file in Get-ChildItem -LiteralPath $packageRoot -Recurse -File | Sort-Object FullName) {
    if ($file.FullName -eq $fileManifest -or $file.FullName -eq $shaList) { continue }
    [pscustomobject]@{
        Path = $file.FullName.Substring($packageRoot.Length + 1).Replace("\", "/")
        Bytes = $file.Length
        SHA256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $file.FullName).Hash
    }
}
$fileRows | Export-Csv -LiteralPath $fileManifest -NoTypeInformation -Encoding utf8

$shaRows = foreach ($file in Get-ChildItem -LiteralPath $packageRoot -Recurse -File | Sort-Object FullName) {
    if ($file.FullName -eq $shaList) { continue }
    $relative = $file.FullName.Substring($packageRoot.Length + 1).Replace("\", "/")
    "{0} *{1}" -f (Get-FileHash -Algorithm SHA256 -LiteralPath $file.FullName).Hash, $relative
}
[System.IO.File]::WriteAllLines($shaList, $shaRows, [System.Text.UTF8Encoding]::new($false))

$resolvedPackage = [System.IO.Path]::GetFullPath($packageRoot)
$resolvedParent = [System.IO.Path]::GetFullPath($reviewParent)
$resolvedZip = [System.IO.Path]::GetFullPath($zipPath)
if (-not $resolvedPackage.StartsWith($resolvedParent + [System.IO.Path]::DirectorySeparatorChar)) {
    throw "Unsafe package path: $resolvedPackage"
}
if (-not $resolvedZip.StartsWith($resolvedParent + [System.IO.Path]::DirectorySeparatorChar)) {
    throw "Unsafe ZIP path: $resolvedZip"
}
if (Test-Path -LiteralPath $resolvedZip) { Remove-Item -LiteralPath $resolvedZip -Force }
[System.IO.Compression.ZipFile]::CreateFromDirectory($resolvedPackage, $resolvedZip, [System.IO.Compression.CompressionLevel]::Optimal, $true)

$folderFiles = Get-ChildItem -LiteralPath $resolvedPackage -Recurse -File | Sort-Object FullName
$folderMap = @{}
foreach ($file in $folderFiles) {
    $relative = $file.FullName.Substring($resolvedPackage.Length + 1).Replace("\", "/")
    $folderMap[$relative] = (Get-FileHash -Algorithm SHA256 -LiteralPath $file.FullName).Hash
}

$archive = [System.IO.Compression.ZipFile]::OpenRead($resolvedZip)
try {
    $zipMap = @{}
    foreach ($entry in $archive.Entries) {
        if ([string]::IsNullOrEmpty($entry.Name)) { continue }
        $normalizedEntry = $entry.FullName.Replace("\", "/")
        $prefix = $packageName + "/"
        if (-not $normalizedEntry.StartsWith($prefix)) { throw "Unexpected ZIP top-level path: $($entry.FullName)" }
        $relative = $normalizedEntry.Substring($prefix.Length)
        $stream = $entry.Open()
        try {
            $hasher = [System.Security.Cryptography.SHA256]::Create()
            try { $zipMap[$relative] = [System.BitConverter]::ToString($hasher.ComputeHash($stream)).Replace("-", "") }
            finally { $hasher.Dispose() }
        }
        finally { $stream.Dispose() }
    }
}
finally { $archive.Dispose() }

$missing = @($folderMap.Keys | Where-Object { -not $zipMap.ContainsKey($_) })
$extra = @($zipMap.Keys | Where-Object { -not $folderMap.ContainsKey($_) })
$mismatch = @($folderMap.Keys | Where-Object { $zipMap.ContainsKey($_) -and $zipMap[$_] -ne $folderMap[$_] })
if ($missing.Count -or $extra.Count -or $mismatch.Count) {
    throw "ZIP verification failed: missing=$($missing.Count), extra=$($extra.Count), mismatch=$($mismatch.Count)"
}

[pscustomobject]@{
    Status = "READY FOR REVIEW"
    Package = $resolvedPackage
    FolderFiles = $folderMap.Count
    FolderBytes = ($folderFiles | Measure-Object Length -Sum).Sum
    ScreenshotPngs = $screenshotRows.Count
    Zip = $resolvedZip
    ZipEntries = $zipMap.Count
    ZipBytes = (Get-Item -LiteralPath $resolvedZip).Length
    ZipSHA256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $resolvedZip).Hash
    Missing = $missing.Count
    Extra = $extra.Count
    HashMismatch = $mismatch.Count
} | ConvertTo-Json
