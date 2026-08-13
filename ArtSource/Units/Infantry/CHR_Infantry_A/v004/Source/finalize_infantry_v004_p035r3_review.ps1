param(
    [Parameter(Mandatory=$true)][string]$ReviewRoot,
    [Parameter(Mandatory=$true)][string]$ZipPath
)

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root=(Resolve-Path -LiteralPath $ReviewRoot).Path;$name=Split-Path $root -Leaf;$manifest=Join-Path $root 'Manifests'
New-Item -ItemType Directory -Force -Path $manifest | Out-Null

$shots=Get-ChildItem (Join-Path $root 'Screenshots') -Recurse -File -Filter '*.png' | Sort-Object FullName | ForEach-Object {
    $bitmap=[Drawing.Bitmap]::FromFile($_.FullName);$colors=[Collections.Generic.HashSet[int]]::new()
    try {
        for($y=0;$y -lt $bitmap.Height;$y+=40){for($x=0;$x -lt $bitmap.Width;$x+=40){[void]$colors.Add($bitmap.GetPixel($x,$y).ToArgb())}}
        if($colors.Count -lt 2){throw "Blank/uniform screenshot: $($_.FullName)"}
        [pscustomobject]@{Path=$_.FullName.Substring($root.Length+1).Replace('\','/');Width=$bitmap.Width;Height=$bitmap.Height;Bytes=$_.Length;SampledColors=$colors.Count;SHA256=(Get-FileHash $_.FullName -Algorithm SHA256).Hash}
    } finally {$bitmap.Dispose()}
}
$shots | Export-Csv (Join-Path $manifest 'Screenshot_Manifest.csv') -NoTypeInformation -Encoding UTF8

$exclude=@('Manifests/File_Manifest.csv','Manifests/SHA256SUMS.txt')
$rows=Get-ChildItem $root -Recurse -File | Sort-Object FullName | ForEach-Object {$relative=$_.FullName.Substring($root.Length+1).Replace('\','/');if($exclude -notcontains $relative){[pscustomobject]@{Path=$relative;Bytes=$_.Length;SHA256=(Get-FileHash $_.FullName -Algorithm SHA256).Hash}}}
$rows | Export-Csv (Join-Path $manifest 'File_Manifest.csv') -NoTypeInformation -Encoding UTF8
$rows | ForEach-Object {"$($_.SHA256)  $($_.Path)"} | Set-Content (Join-Path $manifest 'SHA256SUMS.txt') -Encoding UTF8

if(Test-Path -LiteralPath $ZipPath){Remove-Item -LiteralPath $ZipPath -Force}
Compress-Archive -LiteralPath $root -DestinationPath $ZipPath -CompressionLevel Optimal
$archive=[IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $ZipPath).Path)
try {
    $entries=@($archive.Entries | Where-Object {$_.Length -gt 0});$disk=@(Get-ChildItem $root -Recurse -File)
    if($entries.Count -ne $disk.Count){throw "ZIP count mismatch: $($entries.Count)/$($disk.Count)"}
    $names=@($entries | ForEach-Object {$_.FullName.Replace('\','/')})
    $required=@(
        "$name/README.md","$name/00_Revision_Report.md","$name/01_Sword_Object_Hierarchy_Before.md","$name/02_Sword_Attachment_Transform_Report.md","$name/03_FBX_Attachment_Validation.md","$name/04_Unity_Attachment_Validation.md","$name/05_Runtime_Weapon_Contract_Audit.md","$name/06_Open_Issues.md",
        "$name/Blender/CHR_Infantry_A_v004_P035R3.blend","$name/FBX/SK_Infantry_A_v004_P035R3_Apose_Review.fbx","$name/FBX/SK_Infantry_A_v004_P035R3_L1Pose_Review.fbx",
        "$name/Screenshots/Apose/Apose_SwordGrip_Close.png","$name/Screenshots/L1Pose/L1Pose_SwordGrip_Close.png","$name/Screenshots/Follow/SwordFollow_Neutral.png","$name/Screenshots/Follow/SwordFollow_TestUp.png","$name/Screenshots/Follow/SwordFollow_TestDown.png","$name/Screenshots/Follow/SwordFollow_3Q.png",
        "$name/Screenshots/Hierarchy/Blender_Hierarchy_RightHand_Sword.png","$name/Screenshots/Hierarchy/Hierarchy_RightHand_WeaponSocket_SwordRoot.png","$name/Screenshots/Comparison/P035R2_vs_P035R3_L1Pose_Close.png",
        "$name/Screenshots/Unity/Unity_Apose_Close.png","$name/Screenshots/Unity/Unity_L1Pose_Close.png","$name/Screenshots/Unity/Unity_L1Pose_RTS_Normal.png","$name/Screenshots/Unity/Unity_SwordGrip_Close.png","$name/Screenshots/Unity/Unity_SwordFollow_TestUp.png","$name/Screenshots/Unity/Unity_SwordFollow_TestDown.png","$name/Screenshots/Unity/Unity_Hierarchy_RightHand_Sword.png",
        "$name/Manifests/Geometry_Hierarchy_Follow_Summary.json","$name/Manifests/FBX_Apose_Reimport_Validation.json","$name/Manifests/FBX_L1Pose_Reimport_Validation.json","$name/Manifests/Unity_Attachment_Result.json"
    )
    $missing=@($required | Where-Object {$names -notcontains $_});if($missing.Count){throw "ZIP missing: $($missing -join ', ')"}
    $hashMismatch=@()
    foreach($entry in $entries){$relative=$entry.FullName.Replace('\','/').Substring($name.Length+1);$diskPath=Join-Path $root $relative.Replace('/','\');$stream=$entry.Open();try{$sha=[Security.Cryptography.SHA256]::Create();try{$archiveHash=([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','')}finally{$sha.Dispose()}}finally{$stream.Dispose()};$diskHash=(Get-FileHash -LiteralPath $diskPath -Algorithm SHA256).Hash;if($archiveHash -ne $diskHash){$hashMismatch+=$relative}}
    if($hashMismatch.Count){throw "ZIP hash mismatch: $($hashMismatch -join ', ')"}
} finally {$archive.Dispose()}

[ordered]@{status='READY FOR PHASE03_5 REVISION03 REVIEW';folder_files=$disk.Count;zip_entries=$entries.Count;screenshots=$shots.Count;required_verified=$required.Count;hash_mismatches=0;zip_bytes=(Get-Item -LiteralPath $ZipPath).Length;zip_sha256=(Get-FileHash -LiteralPath $ZipPath -Algorithm SHA256).Hash} | ConvertTo-Json
