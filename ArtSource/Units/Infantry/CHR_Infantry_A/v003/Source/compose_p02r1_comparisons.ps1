param(
    [Parameter(Mandatory = $false)]
    [string]$RepoRoot = "C:\projects\Unity\AegisRTS.FrameworkLab"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$initialRoot = Join-Path $RepoRoot "docs\ArtProduction\ReviewPackages\Infantry_Phase02_PrimaryForms_Review_v001\Screenshots\Clay"
$revisionRoot = Join-Path $RepoRoot "docs\ArtProduction\ReviewPackages\Infantry_Phase02_PrimaryForms_Revision01_Review\Screenshots\Clay"
$comparisonRoot = Join-Path $RepoRoot "docs\ArtProduction\ReviewPackages\Infantry_Phase02_PrimaryForms_Revision01_Review\Screenshots\Comparison"
$l1Path = Join-Path $RepoRoot "ArtSource\Units\Infantry\CHR_Infantry_A\v001\Concepts\Unit_03_Infantry_L1_Concept_Final.png"
[System.IO.Directory]::CreateDirectory($comparisonRoot) | Out-Null

function Draw-ContainedImage {
    param(
        [System.Drawing.Graphics]$Graphics,
        [System.Drawing.Image]$Image,
        [System.Drawing.Rectangle]$Source,
        [System.Drawing.Rectangle]$Destination
    )
    $scale = [Math]::Min($Destination.Width / $Source.Width, $Destination.Height / $Source.Height)
    $width = [int]($Source.Width * $scale)
    $height = [int]($Source.Height * $scale)
    $x = $Destination.X + [int](($Destination.Width - $width) / 2)
    $y = $Destination.Y + [int](($Destination.Height - $height) / 2)
    $target = New-Object System.Drawing.Rectangle($x, $y, $width, $height)
    $Graphics.DrawImage($Image, $target, $Source, [System.Drawing.GraphicsUnit]::Pixel)
}

function New-ComparisonSheet {
    param(
        [string]$LeftPath,
        [string]$RightPath,
        [string]$OutputPath,
        [string]$LeftLabel,
        [string]$RightLabel,
        [System.Drawing.Rectangle]$LeftCrop
    )
    $left = [System.Drawing.Image]::FromFile($LeftPath)
    $right = [System.Drawing.Image]::FromFile($RightPath)
    try {
        $panelWidth = 768
        $panelHeight = 768
        $headerHeight = 72
        $canvas = New-Object System.Drawing.Bitmap ($panelWidth * 2), ($panelHeight + $headerHeight)
        try {
            $graphics = [System.Drawing.Graphics]::FromImage($canvas)
            try {
                $graphics.Clear([System.Drawing.Color]::FromArgb(24, 27, 32))
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                if ($LeftCrop.Width -le 0) {
                    $LeftCrop = New-Object System.Drawing.Rectangle(0, 0, $left.Width, $left.Height)
                }
                $rightCrop = New-Object System.Drawing.Rectangle(0, 0, $right.Width, $right.Height)
                $leftTarget = New-Object System.Drawing.Rectangle(0, $headerHeight, $panelWidth, $panelHeight)
                $rightTarget = New-Object System.Drawing.Rectangle($panelWidth, $headerHeight, $panelWidth, $panelHeight)
                Draw-ContainedImage -Graphics $graphics -Image $left -Source $LeftCrop -Destination $leftTarget
                Draw-ContainedImage -Graphics $graphics -Image $right -Source $rightCrop -Destination $rightTarget
                $font = New-Object System.Drawing.Font("Arial", 22, [System.Drawing.FontStyle]::Bold)
                $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
                try {
                    $graphics.DrawString($LeftLabel, $font, $brush, 22, 20)
                    $graphics.DrawString($RightLabel, $font, $brush, ($panelWidth + 22), 20)
                }
                finally {
                    $brush.Dispose()
                    $font.Dispose()
                }
            }
            finally { $graphics.Dispose() }
            $canvas.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $canvas.Dispose() }
    }
    finally {
        $left.Dispose()
        $right.Dispose()
    }
}

$emptyCrop = New-Object System.Drawing.Rectangle(0, 0, 0, 0)
New-ComparisonSheet `
    -LeftPath (Join-Path $initialRoot "01_Clay_Front.png") `
    -RightPath (Join-Path $revisionRoot "Clay_Front.png") `
    -OutputPath (Join-Path $comparisonRoot "v003_initial_vs_P02R1_Front.png") `
    -LeftLabel "v003 INITIAL" -RightLabel "P02R1" -LeftCrop $emptyCrop
New-ComparisonSheet `
    -LeftPath (Join-Path $initialRoot "04_Clay_3Q_Front.png") `
    -RightPath (Join-Path $revisionRoot "Clay_3Q_Front.png") `
    -OutputPath (Join-Path $comparisonRoot "v003_initial_vs_P02R1_3Q.png") `
    -LeftLabel "v003 INITIAL" -RightLabel "P02R1" -LeftCrop $emptyCrop

$l1FrontCrop = New-Object System.Drawing.Rectangle(165, 88, 230, 405)
$l13QCrop = New-Object System.Drawing.Rectangle(885, 82, 285, 420)
New-ComparisonSheet `
    -LeftPath $l1Path -RightPath (Join-Path $revisionRoot "Clay_Front.png") `
    -OutputPath (Join-Path $comparisonRoot "L1_vs_P02R1_Front.png") `
    -LeftLabel "L1 CONCEPT - FRONT" -RightLabel "P02R1 - FRONT" -LeftCrop $l1FrontCrop
New-ComparisonSheet `
    -LeftPath $l1Path -RightPath (Join-Path $revisionRoot "Clay_3Q_Front.png") `
    -OutputPath (Join-Path $comparisonRoot "L1_vs_P02R1_3Q.png") `
    -LeftLabel "L1 CONCEPT - 3Q" -RightLabel "P02R1 - 3Q" -LeftCrop $l13QCrop

Write-Output "AEGIS_P02R1_COMPARISONS_COMPLETE"
