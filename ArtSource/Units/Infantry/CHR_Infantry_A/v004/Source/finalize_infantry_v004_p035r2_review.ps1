param(
    [Parameter(Mandatory = $true)][string]$ReviewRoot,
    [Parameter(Mandatory = $true)][string]$ZipPath
)

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root=(Resolve-Path -LiteralPath $ReviewRoot).Path;$name=Split-Path $root -Leaf;$man=Join-Path $root 'Manifests'
New-Item -ItemType Directory -Force -Path $man | Out-Null
$shots=Get-ChildItem (Join-Path $root 'Screenshots') -Recurse -File -Filter '*.png' | Sort-Object FullName | ForEach-Object {$img=[Drawing.Image]::FromFile($_.FullName);try{[pscustomobject]@{Path=$_.FullName.Substring($root.Length+1).Replace('\','/');Width=$img.Width;Height=$img.Height;Bytes=$_.Length;SHA256=(Get-FileHash $_.FullName -Algorithm SHA256).Hash}}finally{$img.Dispose()}}
$shots | Export-Csv (Join-Path $man 'Screenshot_Manifest.csv') -NoTypeInformation -Encoding UTF8
$exclude=@('Manifests/File_Manifest.csv','Manifests/SHA256SUMS.txt')
$rows=Get-ChildItem $root -Recurse -File | Sort-Object FullName | ForEach-Object {$rel=$_.FullName.Substring($root.Length+1).Replace('\','/');if($exclude -notcontains $rel){[pscustomobject]@{Path=$rel;Bytes=$_.Length;SHA256=(Get-FileHash $_.FullName -Algorithm SHA256).Hash}}}
$rows | Export-Csv (Join-Path $man 'File_Manifest.csv') -NoTypeInformation -Encoding UTF8
$rows | ForEach-Object {"$($_.SHA256)  $($_.Path)"} | Set-Content (Join-Path $man 'SHA256SUMS.txt') -Encoding UTF8
if(Test-Path -LiteralPath $ZipPath){Remove-Item -LiteralPath $ZipPath -Force}
Compress-Archive -LiteralPath $root -DestinationPath $ZipPath -CompressionLevel Optimal
$archive=[IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $ZipPath).Path)
try {
    $entries=@($archive.Entries | Where-Object {$_.Length -gt 0});$disk=@(Get-ChildItem $root -Recurse -File)
    if($entries.Count -ne $disk.Count){throw "ZIP count mismatch: $($entries.Count)/$($disk.Count)"}
    $names=@($entries | ForEach-Object {$_.FullName.Replace('\','/')})
    $required=@("$name/README.md","$name/00_Revision_Report.md","$name/01_Shield_Vertical_Alignment_Report.md","$name/02_Shield_Grip_Alignment_Report.md","$name/03_Unity_Review_Status.md","$name/04_Open_Issues.md","$name/Blender/CHR_Infantry_A_v004_P035R2.blend","$name/Measurements/Shield_Alignment_Before.json","$name/Measurements/Shield_Alignment_After.json","$name/Measurements/L1_Shield_Alignment_Estimate.json","$name/Screenshots/Overlay/Final_Overlay_L1_vs_P035R2_Front.png","$name/Screenshots/Grip/Shield_Grip_Close.png","$name/Screenshots/Unity/Unity_L1Pose_Close.png","$name/Screenshots/Unity/Unity_L1Pose_RTS_Normal.png","$name/Screenshots/Unity/Unity_L1Pose_Far.png")
    $missing=@($required | Where-Object {$names -notcontains $_});if($missing.Count){throw "ZIP missing: $($missing -join ', ')"}
    $hashMismatch=@();foreach($entry in $entries){$relative=$entry.FullName.Replace('\','/').Substring($name.Length+1);$diskPath=Join-Path $root $relative.Replace('/','\');$stream=$entry.Open();try{$sha=[Security.Cryptography.SHA256]::Create();try{$archiveHash=([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','')}finally{$sha.Dispose()}}finally{$stream.Dispose()};$diskHash=(Get-FileHash -LiteralPath $diskPath -Algorithm SHA256).Hash;if($archiveHash -ne $diskHash){$hashMismatch+=$relative}}
    if($hashMismatch.Count){throw "ZIP hash mismatch: $($hashMismatch -join ', ')"}
} finally {$archive.Dispose()}
[ordered]@{status='READY FOR PHASE03_5 REVISION02 REVIEW';folder_files=$disk.Count;zip_entries=$entries.Count;screenshots=$shots.Count;required_verified=$required.Count;hash_mismatches=0;zip_bytes=(Get-Item -LiteralPath $ZipPath).Length;zip_sha256=(Get-FileHash -LiteralPath $ZipPath -Algorithm SHA256).Hash} | ConvertTo-Json
