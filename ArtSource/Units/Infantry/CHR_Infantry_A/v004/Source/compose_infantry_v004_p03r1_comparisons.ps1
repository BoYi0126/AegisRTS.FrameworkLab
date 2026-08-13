param(
    [Parameter(Mandatory = $true)][string]$ReviewRoot,
    [Parameter(Mandatory = $true)][string]$BaselineRoot,
    [Parameter(Mandatory = $true)][string]$ConceptPath
)

Add-Type -AssemblyName System.Drawing
$review = (Resolve-Path -LiteralPath $ReviewRoot).Path
$baseline = (Resolve-Path -LiteralPath $BaselineRoot).Path
$comparison = Join-Path $review 'Screenshots\Comparison'
$l1Comparison = Join-Path $review 'Screenshots\L1Comparison'
New-Item -ItemType Directory -Force -Path $comparison, $l1Comparison | Out-Null

function Add-Panel {
    param([System.Drawing.Graphics]$Graphics, [string]$Path, [int]$X, [int]$Y, [int]$W, [int]$H, [string]$Label)
    $image = [System.Drawing.Image]::FromFile($Path)
    try {
        $scale = [Math]::Min($W / $image.Width, $H / $image.Height)
        $dw = [int]($image.Width * $scale)
        $dh = [int]($image.Height * $scale)
        $dx = $X + [int](($W - $dw) / 2)
        $dy = $Y + [int](($H - $dh) / 2)
        $Graphics.DrawImage($image, $dx, $dy, $dw, $dh)
        $font = [System.Drawing.Font]::new('Arial', 18, [System.Drawing.FontStyle]::Bold)
        $brush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
        $shadow = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(180, 0, 0, 0))
        try {
            $Graphics.FillRectangle($shadow, $X, $Y, $W, 38)
            $Graphics.DrawString($Label, $font, $brush, $X + 12, $Y + 7)
        } finally { $font.Dispose(); $brush.Dispose(); $shadow.Dispose() }
    } finally { $image.Dispose() }
}

function New-Comparison {
    param([string]$Left, [string]$Right, [string]$LeftLabel, [string]$RightLabel, [string]$Output)
    $canvas = [System.Drawing.Bitmap]::new(1536, 840)
    $graphics = [System.Drawing.Graphics]::FromImage($canvas)
    try {
        $graphics.Clear([System.Drawing.Color]::FromArgb(28, 31, 36))
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        Add-Panel $graphics $Left 0 0 768 840 $LeftLabel
        Add-Panel $graphics $Right 768 0 768 840 $RightLabel
        $graphics.DrawLine([System.Drawing.Pens]::White, 767, 0, 767, 840)
        $canvas.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)
    } finally { $graphics.Dispose(); $canvas.Dispose() }
}

$pairs = @(
    @('Screenshots\Clay\01_Clay_Front.png', 'Screenshots\Clay\01_Clay_Front.png', 'v004 BASELINE / FRONT', 'v004_P03R1 / FRONT', 'v004_vs_P03R1_Front.png'),
    @('Screenshots\Clay\04_Clay_3Q_Front.png', 'Screenshots\Clay\04_Clay_3Q_Front.png', 'v004 BASELINE / 3Q', 'v004_P03R1 / 3Q', 'v004_vs_P03R1_3Q.png'),
    @('Screenshots\Detail\Detail_WaistCloth.png', 'Screenshots\Detail\Detail_WaistCloth.png', 'v004 / WAIST', 'P03R1 / WAIST', 'v004_vs_P03R1_WaistCloth.png'),
    @('Screenshots\Detail\Detail_Scarf.png', 'Screenshots\Detail\Detail_Scarf.png', 'v004 / SCARF', 'P03R1 / SCARF', 'v004_vs_P03R1_Scarf.png'),
    @('Screenshots\Detail\Detail_UpperArm.png', 'Screenshots\Detail\Detail_UpperArm.png', 'v004 / UPPER ARM', 'P03R1 / UPPER ARM', 'v004_vs_P03R1_UpperArm.png'),
    @('Screenshots\Detail\Detail_Shield_Back_WithArm.png', 'Screenshots\Detail\Detail_Shield_Back_WithArm.png', 'v004 / SHIELD BACK', 'P03R1 / SHIELD BACK', 'v004_vs_P03R1_ShieldBack.png'),
    @('Screenshots\Detail\Detail_Boot.png', 'Screenshots\Detail\Detail_Boot.png', 'v004 / BOOT', 'P03R1 / BOOT', 'v004_vs_P03R1_Boot.png')
)
foreach ($pair in $pairs) {
    New-Comparison (Join-Path $baseline $pair[0]) (Join-Path $review $pair[1]) $pair[2] $pair[3] (Join-Path $comparison $pair[4])
}

New-Comparison $ConceptPath (Join-Path $review 'Screenshots\Clay\01_Clay_Front.png') 'L1 CONCEPT' 'v004_P03R1 / FRONT' (Join-Path $l1Comparison 'L1_vs_P03R1_Front.png')
New-Comparison $ConceptPath (Join-Path $review 'Screenshots\Clay\04_Clay_3Q_Front.png') 'L1 CONCEPT' 'v004_P03R1 / 3Q' (Join-Path $l1Comparison 'L1_vs_P03R1_3Q.png')

@"
Generated comparison evidence.
Left baseline source: $baseline
Right revision source: $review
L1 concept source: $ConceptPath
No geometry or production asset was modified by this script.
"@ | Set-Content -LiteralPath (Join-Path $comparison 'COMPOSITION_SOURCE.txt') -Encoding UTF8

Write-Output "P03R1_COMPARISONS_COMPLETE count=9"
