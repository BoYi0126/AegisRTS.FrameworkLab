param(
    [Parameter(Mandatory = $false)]
    [string]$RepoRoot = "C:\projects\Unity\AegisRTS.FrameworkLab"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$baselineRoot = Join-Path $RepoRoot "docs\ArtProduction\ReviewPackages\Infantry_Remaster_Review_Package_v001\Screenshots\Blender"
$reviewRoot = Join-Path $RepoRoot "docs\ArtProduction\ReviewPackages\Infantry_Phase02_PrimaryForms_Review_v001"
$currentRoot = Join-Path $reviewRoot "Screenshots\Clay"
$outputRoot = Join-Path $reviewRoot "Screenshots\Comparison"
[System.IO.Directory]::CreateDirectory($outputRoot) | Out-Null

function New-ComparisonSheet {
    param(
        [Parameter(Mandatory = $true)][string]$BaselinePath,
        [Parameter(Mandatory = $true)][string]$CurrentPath,
        [Parameter(Mandatory = $true)][string]$OutputPath
    )

    $baseline = [System.Drawing.Image]::FromFile($BaselinePath)
    $current = [System.Drawing.Image]::FromFile($CurrentPath)
    try {
        $panelWidth = 768
        $panelHeight = 768
        $headerHeight = 72
        $canvas = New-Object System.Drawing.Bitmap ($panelWidth * 2), ($panelHeight + $headerHeight)
        try {
            $graphics = [System.Drawing.Graphics]::FromImage($canvas)
            try {
                $graphics.Clear([System.Drawing.Color]::FromArgb(22, 24, 29))
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                $graphics.DrawImage($baseline, 0, $headerHeight, $panelWidth, $panelHeight)
                $graphics.DrawImage($current, $panelWidth, $headerHeight, $panelWidth, $panelHeight)
                $font = New-Object System.Drawing.Font("Arial", 23, [System.Drawing.FontStyle]::Bold)
                $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
                try {
                    $graphics.DrawString("v002 PROTOTYPE BASELINE", $font, $brush, 24, 20)
                    $graphics.DrawString("v003 PRIMARY FORMS", $font, $brush, ($panelWidth + 24), 20)
                }
                finally {
                    $brush.Dispose()
                    $font.Dispose()
                }
            }
            finally {
                $graphics.Dispose()
            }
            $canvas.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $canvas.Dispose()
        }
    }
    finally {
        $baseline.Dispose()
        $current.Dispose()
    }
}

New-ComparisonSheet `
    -BaselinePath (Join-Path $baselineRoot "Clay_Front.png") `
    -CurrentPath (Join-Path $currentRoot "01_Clay_Front.png") `
    -OutputPath (Join-Path $outputRoot "v002_vs_v003_Front.png")

New-ComparisonSheet `
    -BaselinePath (Join-Path $baselineRoot "Clay_3Q.png") `
    -CurrentPath (Join-Path $currentRoot "04_Clay_3Q_Front.png") `
    -OutputPath (Join-Path $outputRoot "v002_vs_v003_3Q.png")

Write-Output "AEGIS_PHASE02_COMPARISON_COMPLETE"
