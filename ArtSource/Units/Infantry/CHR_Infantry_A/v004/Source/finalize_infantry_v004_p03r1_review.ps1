param(
    [Parameter(Mandatory = $true)][string]$ReviewRoot,
    [Parameter(Mandatory = $true)][string]$ZipPath
)

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = (Resolve-Path -LiteralPath $ReviewRoot).Path
$rootName = Split-Path $root -Leaf
$manifestDir = Join-Path $root 'Manifests'
New-Item -ItemType Directory -Force -Path $manifestDir | Out-Null

$screenshotRows = Get-ChildItem -LiteralPath (Join-Path $root 'Screenshots') -Recurse -File -Filter '*.png' |
    Sort-Object FullName | ForEach-Object {
        $relative = $_.FullName.Substring($root.Length + 1).Replace('\', '/')
        $bitmap = [System.Drawing.Image]::FromFile($_.FullName)
        try {
            [PSCustomObject]@{
                Path = $relative
                Width = $bitmap.Width
                Height = $bitmap.Height
                Bytes = $_.Length
                SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
            }
        } finally { $bitmap.Dispose() }
    }
$screenshotRows | Export-Csv -LiteralPath (Join-Path $manifestDir 'Screenshot_Manifest.csv') -NoTypeInformation -Encoding UTF8

$excluded = @('Manifests/File_Manifest.csv', 'Manifests/SHA256SUMS.txt')
$fileRows = Get-ChildItem -LiteralPath $root -Recurse -File | Sort-Object FullName | ForEach-Object {
    $relative = $_.FullName.Substring($root.Length + 1).Replace('\', '/')
    if ($excluded -contains $relative) { return }
    [PSCustomObject]@{
        Path = $relative
        Bytes = $_.Length
        SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
    }
}
$fileRows | Export-Csv -LiteralPath (Join-Path $manifestDir 'File_Manifest.csv') -NoTypeInformation -Encoding UTF8
$fileRows | ForEach-Object { "$($_.SHA256)  $($_.Path)" } | Set-Content -LiteralPath (Join-Path $manifestDir 'SHA256SUMS.txt') -Encoding UTF8

if (Test-Path -LiteralPath $ZipPath) { Remove-Item -LiteralPath $ZipPath -Force }
Compress-Archive -LiteralPath $root -DestinationPath $ZipPath -CompressionLevel Optimal

$zip = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $ZipPath).Path)
try {
    $entries = @($zip.Entries | Where-Object { $_.Length -gt 0 })
    $zipNames = @($entries | ForEach-Object { $_.FullName.Replace('\', '/') })
    $diskFiles = @(Get-ChildItem -LiteralPath $root -Recurse -File)
    if ($entries.Count -ne $diskFiles.Count) { throw "ZIP count mismatch: zip=$($entries.Count), disk=$($diskFiles.Count)" }
    foreach ($required in @(
        "$rootName/README.md",
        "$rootName/Blender/CHR_Infantry_A_v004_P03R1.blend",
        "$rootName/Screenshots/Detail/Detail_WaistCloth.png",
        "$rootName/Screenshots/Detail/Detail_Scarf.png",
        "$rootName/Screenshots/Detail/Detail_Shield_Back.png",
        "$rootName/Screenshots/Detail/Detail_Boot.png",
        "$rootName/Screenshots/Comparison/v004_vs_P03R1_Front.png",
        "$rootName/Screenshots/Unity/Unity_RTS_Normal.png",
        "$rootName/04_Unity_Review_Status.md"
    )) {
        if ($zipNames -notcontains $required) { throw "ZIP missing required entry: $required" }
    }
} finally { $zip.Dispose() }

$result = [ordered]@{
    status = 'READY FOR PHASE03 REVISION REVIEW'
    folder = $root
    folder_file_count = $diskFiles.Count
    zip = (Resolve-Path -LiteralPath $ZipPath).Path
    zip_bytes = (Get-Item -LiteralPath $ZipPath).Length
    zip_sha256 = (Get-FileHash -LiteralPath $ZipPath -Algorithm SHA256).Hash
    zip_entry_count = $entries.Count
    required_entries_verified = 9
}
$result | ConvertTo-Json | Write-Output
