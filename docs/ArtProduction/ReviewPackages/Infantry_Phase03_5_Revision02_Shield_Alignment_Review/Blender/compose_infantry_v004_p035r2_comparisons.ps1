param(
    [Parameter(Mandatory = $true)][string]$ReviewRoot,
    [Parameter(Mandatory = $true)][string]$ConceptPath,
    [Parameter(Mandatory = $true)][string]$P035R1UnityClose,
    [Parameter(Mandatory = $true)][string]$P035R1UnityRts
)

Add-Type -AssemblyName System.Drawing
$root=(Resolve-Path -LiteralPath $ReviewRoot).Path
$concept=[Drawing.Image]::FromFile((Resolve-Path -LiteralPath $ConceptPath).Path)
$overlayDir=Join-Path $root 'Screenshots\Overlay'
$comparisonDir=Join-Path $root 'Screenshots\Comparison'
$unityDir=Join-Path $root 'Screenshots\Unity'
New-Item -ItemType Directory -Force -Path $overlayDir,$comparisonDir,$unityDir | Out-Null

function Add-Label([Drawing.Graphics]$Graphics,[int]$Width,[string]$Text) {
    $font=[Drawing.Font]::new('Arial',16,[Drawing.FontStyle]::Bold)
    $brush=[Drawing.SolidBrush]::new([Drawing.Color]::White)
    $back=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(220,18,23,30))
    try {$Graphics.FillRectangle($back,0,0,$Width,42);$Graphics.DrawString($Text,$font,$brush,12,9)}
    finally {$font.Dispose();$brush.Dispose();$back.Dispose()}
}

function Get-AlphaBounds([Drawing.Bitmap]$Bitmap) {
    $minX=$Bitmap.Width;$minY=$Bitmap.Height;$maxX=-1;$maxY=-1
    for($y=0;$y -lt $Bitmap.Height;$y+=2){for($x=0;$x -lt $Bitmap.Width;$x+=2){if($Bitmap.GetPixel($x,$y).A -gt 8){if($x -lt $minX){$minX=$x};if($x -gt $maxX){$maxX=$x};if($y -lt $minY){$minY=$y};if($y -gt $maxY){$maxY=$y}}}}
    if($maxY -lt 0){throw 'Model render has no non-transparent pixels'}
    [Drawing.Rectangle]::new($minX,$minY,$maxX-$minX+1,$maxY-$minY+1)
}

function New-Overlay([string]$ModelPath,[string]$Output,[bool]$Focus) {
    $canvas=[Drawing.Bitmap]::new(768,768);$graphics=[Drawing.Graphics]::FromImage($canvas);$model=[Drawing.Bitmap]::FromFile($ModelPath);$model.RotateFlip([Drawing.RotateFlipType]::RotateNoneFlipX);$attributes=[Drawing.Imaging.ImageAttributes]::new()
    try {
        $graphics.Clear([Drawing.Color]::FromArgb(242,240,235));$graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.DrawImage($concept,[Drawing.Rectangle]::new(149,60,470,680),[Drawing.Rectangle]::new(140,125,235,340),[Drawing.GraphicsUnit]::Pixel)
        $matrix=[Drawing.Imaging.ColorMatrix]::new();$matrix.Matrix00=.30;$matrix.Matrix11=.72;$matrix.Matrix22=1.0;$matrix.Matrix33=.58;$attributes.SetColorMatrix($matrix)
        $alpha=Get-AlphaBounds $model;$targetTop=76.0;$targetGround=720.0;$scale=($targetGround-$targetTop)/$alpha.Height;$drawX=[int](360-(369*$scale));$drawY=[int]($targetGround-(($alpha.Y+$alpha.Height)*$scale))
        $graphics.DrawImage($model,[Drawing.Rectangle]::new($drawX,$drawY,[int]($model.Width*$scale),[int]($model.Height*$scale)),0,0,$model.Width,$model.Height,[Drawing.GraphicsUnit]::Pixel,$attributes)
        Add-Label $graphics 768 $(if($Focus){'L1 + P035R2 / SHIELD TOP-CENTER-BOTTOM FOCUS'}else{'L1 50% + P035R2 L1-COMPARE 50% / FRONT'})
        if($Focus){$pen=[Drawing.Pen]::new([Drawing.Color]::FromArgb(235,255,72,55),3);try{$graphics.DrawRectangle($pen,190,175,390,390)}finally{$pen.Dispose()}}
        $canvas.Save($Output,[Drawing.Imaging.ImageFormat]::Png)
    } finally {$attributes.Dispose();$model.Dispose();$graphics.Dispose();$canvas.Dispose()}
}

function New-SideBySide([string]$Left,[string]$Right,[string]$Output,[string]$LeftLabel,[string]$RightLabel,[int]$Width=768,[int]$Height=768) {
    $a=[Drawing.Image]::FromFile($Left);$b=[Drawing.Image]::FromFile($Right);$canvas=[Drawing.Bitmap]::new($Width*2,$Height);$g=[Drawing.Graphics]::FromImage($canvas)
    try {$g.Clear([Drawing.Color]::FromArgb(22,27,34));$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;$g.DrawImage($a,0,0,$Width,$Height);$g.DrawImage($b,$Width,0,$Width,$Height);Add-Label $g ($Width*2) "$LeftLabel                              $RightLabel";$g.DrawLine([Drawing.Pens]::White,$Width-1,0,$Width-1,$Height);$canvas.Save($Output,[Drawing.Imaging.ImageFormat]::Png)}
    finally {$a.Dispose();$b.Dispose();$g.Dispose();$canvas.Dispose()}
}

$front=Join-Path $root 'Screenshots\L1Pose\01_L1Pose_Front.png'
New-Overlay $front (Join-Path $overlayDir 'Final_Overlay_L1_vs_P035R2_Front.png') $false
New-Overlay $front (Join-Path $overlayDir 'Final_Overlay_L1_vs_P035R2_ShieldFocus.png') $true
New-SideBySide (Join-Path $comparisonDir 'P035R1_L1Pose_Front.png') (Join-Path $comparisonDir 'P035R2_L1Pose_Front.png') (Join-Path $comparisonDir 'P035R1_vs_P035R2_Front.png') 'P035R1 / FRONT' 'P035R2 / FRONT'
New-SideBySide (Join-Path $comparisonDir 'P035R1_L1Pose_3Q.png') (Join-Path $comparisonDir 'P035R2_L1Pose_3Q.png') (Join-Path $comparisonDir 'P035R1_vs_P035R2_3Q.png') 'P035R1 / 3Q' 'P035R2 / 3Q'
New-SideBySide (Join-Path $comparisonDir 'P035R1_L1Pose_Front.png') (Join-Path $root 'Screenshots\ShieldFocus\Shield_Front_Focus.png') (Join-Path $comparisonDir 'P035R1_vs_P035R2_ShieldFocus.png') 'P035R1 / FULL' 'P035R2 / SHIELD FOCUS'

$newClose=Join-Path $unityDir 'Unity_L1Pose_Close.png';$newRts=Join-Path $unityDir 'Unity_L1Pose_RTS_Normal.png'
if(Test-Path -LiteralPath $newClose){New-SideBySide (Resolve-Path -LiteralPath $P035R1UnityClose).Path $newClose (Join-Path $unityDir 'Unity_P035R1_vs_P035R2_Close.png') 'P035R1 / UNITY CLOSE' 'P035R2 / UNITY CLOSE' 960 540}
if(Test-Path -LiteralPath $newRts){New-SideBySide (Resolve-Path -LiteralPath $P035R1UnityRts).Path $newRts (Join-Path $unityDir 'Unity_P035R1_vs_P035R2_RTS_Normal.png') 'P035R1 / UNITY RTS' 'P035R2 / UNITY RTS' 960 540}
$concept.Dispose()
'P035R2_COMPARISONS_COMPLETE overlays=2 comparisons=3 unity=2'
