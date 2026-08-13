param(
    [Parameter(Mandatory = $true)][string]$ReviewRoot,
    [Parameter(Mandatory = $true)][string]$ConceptPath,
    [Parameter(Mandatory = $true)][string]$P035UnityClose
)

Add-Type -AssemblyName System.Drawing
$root = (Resolve-Path -LiteralPath $ReviewRoot).Path
$concept = [Drawing.Image]::FromFile((Resolve-Path -LiteralPath $ConceptPath).Path)
$overlayDir = Join-Path $root 'Screenshots\Overlay'
$comparisonDir = Join-Path $root 'Screenshots\Comparison'
$unityDir = Join-Path $root 'Screenshots\Unity'
New-Item -ItemType Directory -Force -Path $overlayDir,$comparisonDir,$unityDir | Out-Null

function Get-AlphaBounds([Drawing.Bitmap]$Bitmap) {
    $minX=$Bitmap.Width;$minY=$Bitmap.Height;$maxX=-1;$maxY=-1
    for($y=0;$y -lt $Bitmap.Height;$y+=2){for($x=0;$x -lt $Bitmap.Width;$x+=2){
        if($Bitmap.GetPixel($x,$y).A -gt 8){
            if($x -lt $minX){$minX=$x};if($x -gt $maxX){$maxX=$x}
            if($y -lt $minY){$minY=$y};if($y -gt $maxY){$maxY=$y}
        }
    }}
    if($maxY -lt 0){throw 'Model render has no non-transparent pixels'}
    [Drawing.Rectangle]::new($minX,$minY,$maxX-$minX+1,$maxY-$minY+1)
}

function Add-Label([Drawing.Graphics]$Graphics,[int]$Width,[string]$Text) {
    $font=[Drawing.Font]::new('Arial',16,[Drawing.FontStyle]::Bold)
    $brush=[Drawing.SolidBrush]::new([Drawing.Color]::White)
    $back=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(220,18,23,30))
    try {$Graphics.FillRectangle($back,0,0,$Width,42);$Graphics.DrawString($Text,$font,$brush,12,9)}
    finally {$font.Dispose();$brush.Dispose();$back.Dispose()}
}

function New-Overlay([string]$ModelPath,[string]$Output) {
    $canvas=[Drawing.Bitmap]::new(768,768)
    $graphics=[Drawing.Graphics]::FromImage($canvas)
    $model=[Drawing.Bitmap]::FromFile($ModelPath)
    $attributes=[Drawing.Imaging.ImageAttributes]::new()
    try {
        $graphics.Clear([Drawing.Color]::FromArgb(242,240,235))
        $graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.DrawImage($concept,[Drawing.Rectangle]::new(149,60,470,680),[Drawing.Rectangle]::new(140,125,235,340),[Drawing.GraphicsUnit]::Pixel)
        $matrix=[Drawing.Imaging.ColorMatrix]::new()
        $matrix.Matrix00=0.30;$matrix.Matrix11=0.72;$matrix.Matrix22=1.0;$matrix.Matrix33=0.58
        $attributes.SetColorMatrix($matrix)
        $alpha=Get-AlphaBounds $model
        $targetTop=76.0;$targetGround=720.0;$scale=($targetGround-$targetTop)/$alpha.Height
        $drawX=[int](425-(369*$scale))
        $drawY=[int]($targetGround-(($alpha.Y+$alpha.Height)*$scale))
        $graphics.DrawImage($model,[Drawing.Rectangle]::new($drawX,$drawY,[int]($model.Width*$scale),[int]($model.Height*$scale)),0,0,$model.Width,$model.Height,[Drawing.GraphicsUnit]::Pixel,$attributes)
        Add-Label $graphics 768 'L1 50% + FINAL P035R1 L1-COMPARE 50% / FRONT'
        $canvas.Save($Output,[Drawing.Imaging.ImageFormat]::Png)
    } finally {$attributes.Dispose();$model.Dispose();$graphics.Dispose();$canvas.Dispose()}
}

function New-ArmFocus([string]$Source,[string]$Output,[string]$Label) {
    $image=[Drawing.Image]::FromFile($Source)
    $canvas=[Drawing.Bitmap]::new(960,720)
    $g=[Drawing.Graphics]::FromImage($canvas)
    try {
        $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.DrawImage($image,[Drawing.Rectangle]::new(0,0,960,720),[Drawing.Rectangle]::new(95,145,575,430),[Drawing.GraphicsUnit]::Pixel)
        Add-Label $g 960 $Label
        $canvas.Save($Output,[Drawing.Imaging.ImageFormat]::Png)
    } finally {$image.Dispose();$g.Dispose();$canvas.Dispose()}
}

function New-SideBySide([string]$Left,[string]$Right,[string]$Output,[string]$LeftLabel,[string]$RightLabel) {
    $a=[Drawing.Image]::FromFile($Left);$b=[Drawing.Image]::FromFile($Right)
    $height=768;$width=1536
    $canvas=[Drawing.Bitmap]::new($width,$height);$g=[Drawing.Graphics]::FromImage($canvas)
    try {
        $g.Clear([Drawing.Color]::FromArgb(22,27,34));$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.DrawImage($a,0,0,768,768);$g.DrawImage($b,768,0,768,768)
        $font=[Drawing.Font]::new('Arial',16,[Drawing.FontStyle]::Bold);$brush=[Drawing.SolidBrush]::new([Drawing.Color]::White);$back=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(220,18,23,30))
        try {$g.FillRectangle($back,0,0,$width,42);$g.DrawString($LeftLabel,$font,$brush,12,9);$g.DrawString($RightLabel,$font,$brush,780,9)} finally {$font.Dispose();$brush.Dispose();$back.Dispose()}
        $g.DrawLine([Drawing.Pens]::White,767,0,767,$height);$canvas.Save($Output,[Drawing.Imaging.ImageFormat]::Png)
    } finally {$a.Dispose();$b.Dispose();$g.Dispose();$canvas.Dispose()}
}

function New-UnitySideBySide([string]$Left,[string]$Right,[string]$Output) {
    $a=[Drawing.Image]::FromFile($Left);$b=[Drawing.Image]::FromFile($Right)
    $canvas=[Drawing.Bitmap]::new(1920,540);$g=[Drawing.Graphics]::FromImage($canvas)
    try {
        $g.DrawImage($a,0,0,960,540);$g.DrawImage($b,960,0,960,540)
        $font=[Drawing.Font]::new('Arial',16,[Drawing.FontStyle]::Bold);$brush=[Drawing.SolidBrush]::new([Drawing.Color]::White);$back=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(220,18,23,30))
        try {$g.FillRectangle($back,0,0,1920,42);$g.DrawString('P035 / UNITY L1POSE CLOSE',$font,$brush,12,9);$g.DrawString('P035R1 / UNITY L1POSE CLOSE',$font,$brush,972,9)} finally {$font.Dispose();$brush.Dispose();$back.Dispose()}
        $g.DrawLine([Drawing.Pens]::White,959,0,959,540);$canvas.Save($Output,[Drawing.Imaging.ImageFormat]::Png)
    } finally {$a.Dispose();$b.Dispose();$g.Dispose();$canvas.Dispose()}
}

$finalFront=Join-Path $root 'Screenshots\L1Pose\Final_L1Pose_Front.png'
$overlay=Join-Path $overlayDir 'Final_Overlay_L1Pose_Front.png'
New-Overlay $finalFront $overlay
New-ArmFocus $overlay (Join-Path $overlayDir 'Final_Overlay_L1Pose_Front_ArmFocus.png') 'L1 + P035R1 / SHOULDER TO HAND FOCUS'
New-SideBySide (Join-Path $comparisonDir 'P035_L1Pose_Front.png') (Join-Path $comparisonDir 'P035R1_L1Pose_Front.png') (Join-Path $comparisonDir 'P035_vs_P035R1_L1Pose_Front.png') 'P035 / L1-COMPARE' 'P035R1 / L1-COMPARE'
New-SideBySide (Join-Path $comparisonDir 'P035_L1Pose_3Q.png') (Join-Path $comparisonDir 'P035R1_L1Pose_3Q.png') (Join-Path $comparisonDir 'P035_vs_P035R1_L1Pose_3Q.png') 'P035 / L1-COMPARE 3Q' 'P035R1 / L1-COMPARE 3Q'

$beforeFocus=Join-Path $comparisonDir 'P035_ArmFocus_Source.png'
$afterFocus=Join-Path $comparisonDir 'P035R1_ArmFocus_Source.png'
New-ArmFocus (Join-Path $comparisonDir 'P035_L1Pose_Front.png') $beforeFocus 'P035 / ARM FOCUS'
New-ArmFocus (Join-Path $comparisonDir 'P035R1_L1Pose_Front.png') $afterFocus 'P035R1 / ARM FOCUS'
New-SideBySide $beforeFocus $afterFocus (Join-Path $comparisonDir 'P035_vs_P035R1_ArmFocus.png') 'P035 / ARM FOCUS' 'P035R1 / ARM FOCUS'
Remove-Item -LiteralPath $beforeFocus,$afterFocus -Force

$newUnity=Join-Path $unityDir 'Unity_L1Pose_Close.png'
if(Test-Path -LiteralPath $newUnity){New-UnitySideBySide (Resolve-Path -LiteralPath $P035UnityClose).Path $newUnity (Join-Path $unityDir 'Unity_P035_vs_P035R1_Close.png')}
$concept.Dispose()
'P035R1_COMPARISONS_COMPLETE overlays=2 comparisons=3'
