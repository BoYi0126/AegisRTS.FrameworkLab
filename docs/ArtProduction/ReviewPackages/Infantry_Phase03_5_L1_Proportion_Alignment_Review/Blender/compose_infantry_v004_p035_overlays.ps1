param(
    [Parameter(Mandatory = $true)][string]$ReviewRoot,
    [Parameter(Mandatory = $true)][string]$ConceptPath
)

Add-Type -AssemblyName System.Drawing
$root = (Resolve-Path -LiteralPath $ReviewRoot).Path
$concept = [Drawing.Image]::FromFile((Resolve-Path -LiteralPath $ConceptPath).Path)
$overlayDir = Join-Path $root 'Screenshots\Overlay'
$annotatedDir = Join-Path $root 'Screenshots\Annotated'
$comparisonDir = Join-Path $root 'Screenshots\Comparison'
New-Item -ItemType Directory -Force -Path $overlayDir,$annotatedDir,$comparisonDir | Out-Null

$views = @{
    Front = @{ Crop = [Drawing.Rectangle]::new(140,125,235,340); Dest = [Drawing.Rectangle]::new(149,60,470,680); ModelAnchor = 369; ConceptAnchor = 425 }
    Side  = @{ Crop = [Drawing.Rectangle]::new(390,125,210,340); Dest = [Drawing.Rectangle]::new(174,60,420,680); ModelAnchor = 384; ConceptAnchor = 404 }
    Back  = @{ Crop = [Drawing.Rectangle]::new(630,125,225,340); Dest = [Drawing.Rectangle]::new(159,60,450,680); ModelAnchor = 399; ConceptAnchor = 401 }
}

function Get-AlphaBounds([Drawing.Bitmap]$Bitmap) {
    $minX=$Bitmap.Width;$minY=$Bitmap.Height;$maxX=-1;$maxY=-1
    for($y=0;$y -lt $Bitmap.Height;$y+=2){for($x=0;$x -lt $Bitmap.Width;$x+=2){if($Bitmap.GetPixel($x,$y).A -gt 8){if($x -lt $minX){$minX=$x};if($x -gt $maxX){$maxX=$x};if($y -lt $minY){$minY=$y};if($y -gt $maxY){$maxY=$y}}}}
    if($maxY -lt 0){throw 'Model render has no non-transparent pixels'}
    [Drawing.Rectangle]::new($minX,$minY,$maxX-$minX+1,$maxY-$minY+1)
}

function Add-Label([Drawing.Graphics]$Graphics,[string]$Text) {
    $font=[Drawing.Font]::new('Arial',16,[Drawing.FontStyle]::Bold)
    $brush=[Drawing.SolidBrush]::new([Drawing.Color]::White)
    $back=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(210,18,23,30))
    try {$Graphics.FillRectangle($back,0,0,768,42);$Graphics.DrawString($Text,$font,$brush,12,9)} finally {$font.Dispose();$brush.Dispose();$back.Dispose()}
}

function New-Overlay([string]$View,[string]$ModelPath,[string]$Output,[string]$Label) {
    $canvas=[Drawing.Bitmap]::new(768,768)
    $graphics=[Drawing.Graphics]::FromImage($canvas)
    $model=[Drawing.Bitmap]::FromFile($ModelPath)
    $attributes=[Drawing.Imaging.ImageAttributes]::new()
    try {
        $graphics.Clear([Drawing.Color]::FromArgb(242,240,235))
        $graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.DrawImage($concept,$views[$View].Dest,$views[$View].Crop,[Drawing.GraphicsUnit]::Pixel)
        $matrix=[Drawing.Imaging.ColorMatrix]::new()
        $matrix.Matrix00=0.30;$matrix.Matrix11=0.72;$matrix.Matrix22=1.0;$matrix.Matrix33=0.58
        $attributes.SetColorMatrix($matrix)
        # Normalize total silhouette height and align ground before comparison.
        # Only uniform scale + translation are used; X/Y are never stretched independently.
        $alpha=Get-AlphaBounds $model
        $targetTop=76.0;$targetGround=720.0;$scale=($targetGround-$targetTop)/$alpha.Height
        $drawX=[int]($views[$View].ConceptAnchor-($views[$View].ModelAnchor*$scale))
        $drawY=[int]($targetGround-(($alpha.Y+$alpha.Height)*$scale))
        $drawW=[int]($model.Width*$scale);$drawH=[int]($model.Height*$scale)
        $graphics.DrawImage($model,[Drawing.Rectangle]::new($drawX,$drawY,$drawW,$drawH),0,0,$model.Width,$model.Height,[Drawing.GraphicsUnit]::Pixel,$attributes)
        Add-Label $graphics $Label
        $canvas.Save($Output,[Drawing.Imaging.ImageFormat]::Png)
    } finally {$attributes.Dispose();$model.Dispose();$graphics.Dispose();$canvas.Dispose()}
}

function New-SideBySide([string]$Left,[string]$Right,[string]$Output,[string]$LeftLabel,[string]$RightLabel) {
    $canvas=[Drawing.Bitmap]::new(1536,768);$g=[Drawing.Graphics]::FromImage($canvas)
    $a=[Drawing.Image]::FromFile($Left);$b=[Drawing.Image]::FromFile($Right)
    try {
        $g.Clear([Drawing.Color]::FromArgb(22,27,34));$g.DrawImage($a,0,0,768,768);$g.DrawImage($b,768,0,768,768)
        $font=[Drawing.Font]::new('Arial',16,[Drawing.FontStyle]::Bold);$brush=[Drawing.SolidBrush]::new([Drawing.Color]::White);$back=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(210,18,23,30))
        try {$g.FillRectangle($back,0,0,1536,42);$g.DrawString($LeftLabel,$font,$brush,12,9);$g.DrawString($RightLabel,$font,$brush,780,9)} finally {$font.Dispose();$brush.Dispose();$back.Dispose()}
        $g.DrawLine([Drawing.Pens]::White,767,0,767,768);$canvas.Save($Output,[Drawing.Imaging.ImageFormat]::Png)
    } finally {$a.Dispose();$b.Dispose();$g.Dispose();$canvas.Dispose()}
}

$diagnostic=Join-Path $root 'Screenshots\Diagnostic'
$finalPose=Join-Path $root 'Screenshots\L1Pose'
foreach($view in @('Front','Side','Back')) {
    $diagnosticView = if($view -eq 'Side'){'Left'}else{$view}
    New-Overlay $view (Join-Path $diagnostic "Apose_$diagnosticView.png") (Join-Path $overlayDir "Overlay_Apose_$view.png") "L1 50% + CURRENT P03R1 A-POSE 50% / $view"
    New-Overlay $view (Join-Path $diagnostic "L1Pose_$diagnosticView.png") (Join-Path $overlayDir "Overlay_L1Pose_$view.png") "L1 50% + CURRENT P03R1 L1-COMPARE 50% / $view"
    New-Overlay $view (Join-Path $finalPose "Final_L1Pose_$diagnosticView.png") (Join-Path $overlayDir "Final_Overlay_L1Pose_$view.png") "L1 50% + FINAL P035 L1-COMPARE 50% / $view"
}

New-SideBySide (Join-Path $comparisonDir 'Before_Apose_Front.png') (Join-Path $comparisonDir 'After_Apose_Front.png') (Join-Path $comparisonDir 'Before_vs_After_Apose_Front.png') 'P03R1 / A-POSE' 'P035 / A-POSE'
New-SideBySide (Join-Path $comparisonDir 'Before_Apose_3Q.png') (Join-Path $comparisonDir 'After_Apose_3Q.png') (Join-Path $comparisonDir 'Before_vs_After_Apose_3Q.png') 'P03R1 / A-POSE 3Q' 'P035 / A-POSE 3Q'
New-SideBySide (Join-Path $comparisonDir 'Before_L1Pose_Front.png') (Join-Path $comparisonDir 'After_L1Pose_Front.png') (Join-Path $comparisonDir 'Before_vs_After_L1Pose_Front.png') 'P03R1 / L1-COMPARE' 'P035 / L1-COMPARE'
New-SideBySide (Join-Path $comparisonDir 'Before_L1Pose_3Q.png') (Join-Path $comparisonDir 'After_L1Pose_3Q.png') (Join-Path $comparisonDir 'Before_vs_After_L1Pose_3Q.png') 'P03R1 / L1-COMPARE 3Q' 'P035 / L1-COMPARE 3Q'

$annotatedSource=Join-Path $overlayDir 'Final_Overlay_L1Pose_Front.png'
$annotated=[Drawing.Bitmap]::FromFile($annotatedSource);$g=[Drawing.Graphics]::FromImage($annotated)
try {
    $font=[Drawing.Font]::new('Arial',12,[Drawing.FontStyle]::Bold);$pen=[Drawing.Pen]::new([Drawing.Color]::FromArgb(220,255,76,76),2);$brush=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(240,255,76,76))
    try {
        $landmarks=@{head=151;shoulder=204;elbow=263;wrist=316;belt=275;hip=338;knee=372;ankle=425}
        foreach($entry in $landmarks.GetEnumerator()){$y=60+(($entry.Value-125)*2);$g.DrawLine($pen,130,$y,638,$y);$g.DrawString($entry.Key,$font,$brush,640,$y-8)}
    } finally {$font.Dispose();$pen.Dispose();$brush.Dispose()}
    $annotated.Save((Join-Path $annotatedDir 'Annotated_L1_vs_3D_Front.png'),[Drawing.Imaging.ImageFormat]::Png)
} finally {$g.Dispose();$annotated.Dispose();$concept.Dispose()}

"P035_OVERLAYS_COMPLETE overlays=9 comparisons=4 annotated=1"
