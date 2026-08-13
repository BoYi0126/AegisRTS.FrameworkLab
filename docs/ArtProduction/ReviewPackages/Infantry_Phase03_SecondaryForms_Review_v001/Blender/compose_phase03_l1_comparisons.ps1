param([string]$RepoRoot="C:\projects\Unity\AegisRTS.FrameworkLab")
$ErrorActionPreference='Stop';Add-Type -AssemblyName System.Drawing
$l1=Join-Path $RepoRoot 'ArtSource\Units\Infantry\CHR_Infantry_A\v001\Concepts\Unit_03_Infantry_L1_Concept_Final.png'
$clay=Join-Path $RepoRoot 'docs\ArtProduction\ReviewPackages\Infantry_Phase03_SecondaryForms_Review_v001\Screenshots\Clay'
$p02=Join-Path $RepoRoot 'docs\ArtProduction\ReviewPackages\Infantry_Phase02_PrimaryForms_Revision01_Review\Screenshots\Clay'
$out=Join-Path $RepoRoot 'docs\ArtProduction\ReviewPackages\Infantry_Phase03_SecondaryForms_Review_v001\Screenshots\Comparison'
[IO.Directory]::CreateDirectory($out)|Out-Null
function Sheet($right,$output,$leftLabel,$rightLabel,[Drawing.Rectangle]$crop){
 $a=[Drawing.Image]::FromFile($l1);$b=[Drawing.Image]::FromFile((Join-Path $clay $right));try{
  $canvas=New-Object Drawing.Bitmap 1536,840;try{$g=[Drawing.Graphics]::FromImage($canvas);try{$g.Clear([Drawing.Color]::FromArgb(24,27,32));$g.InterpolationMode='HighQualityBicubic';$g.SmoothingMode='HighQuality';
   function Draw($img,[Drawing.Rectangle]$src,[Drawing.Rectangle]$dst){$scale=[Math]::Min($dst.Width/$src.Width,$dst.Height/$src.Height);$w=[int]($src.Width*$scale);$h=[int]($src.Height*$scale);$r=New-Object Drawing.Rectangle ($dst.X+[int](($dst.Width-$w)/2)),($dst.Y+[int](($dst.Height-$h)/2)),$w,$h;$g.DrawImage($img,$r,$src,[Drawing.GraphicsUnit]::Pixel)}
   Draw $a $crop (New-Object Drawing.Rectangle 0,72,768,768);Draw $b (New-Object Drawing.Rectangle 0,0,$b.Width,$b.Height) (New-Object Drawing.Rectangle 768,72,768,768)
   $font=New-Object Drawing.Font 'Arial',22,([Drawing.FontStyle]::Bold);$brush=New-Object Drawing.SolidBrush ([Drawing.Color]::White);try{$g.DrawString($leftLabel,$font,$brush,22,20);$g.DrawString($rightLabel,$font,$brush,790,20)}finally{$font.Dispose();$brush.Dispose()}
  }finally{$g.Dispose()};$canvas.Save((Join-Path $out $output),[Drawing.Imaging.ImageFormat]::Png)}finally{$canvas.Dispose()}
 }finally{$a.Dispose();$b.Dispose()}
}
function Pair($leftPath,$right,$output,$leftLabel,$rightLabel){
 $a=[Drawing.Image]::FromFile($leftPath);$b=[Drawing.Image]::FromFile((Join-Path $clay $right));try{
  $canvas=New-Object Drawing.Bitmap 1536,840;try{$g=[Drawing.Graphics]::FromImage($canvas);try{$g.Clear([Drawing.Color]::FromArgb(24,27,32));$g.InterpolationMode='HighQualityBicubic';$g.DrawImage($a,(New-Object Drawing.Rectangle 0,72,768,768));$g.DrawImage($b,(New-Object Drawing.Rectangle 768,72,768,768));$font=New-Object Drawing.Font 'Arial',22,([Drawing.FontStyle]::Bold);$brush=New-Object Drawing.SolidBrush ([Drawing.Color]::White);try{$g.DrawString($leftLabel,$font,$brush,22,20);$g.DrawString($rightLabel,$font,$brush,790,20)}finally{$font.Dispose();$brush.Dispose()}}finally{$g.Dispose()};$canvas.Save((Join-Path $out $output),[Drawing.Imaging.ImageFormat]::Png)}finally{$canvas.Dispose()}
 }finally{$a.Dispose();$b.Dispose()}
}
Sheet '01_Clay_Front.png' 'L1_vs_v004_Front.png' 'L1 CONCEPT - FRONT' 'v004 - FRONT' (New-Object Drawing.Rectangle 165,88,230,405)
Sheet '04_Clay_3Q_Front.png' 'L1_vs_v004_3Q.png' 'L1 CONCEPT - 3Q' 'v004 - 3Q' (New-Object Drawing.Rectangle 885,82,285,420)
Sheet '03_Clay_Back.png' 'L1_vs_v004_Back.png' 'L1 CONCEPT - BACK' 'v004 - BACK' (New-Object Drawing.Rectangle 650,88,220,405)
Pair (Join-Path $p02 'Clay_Front.png') '01_Clay_Front.png' 'P02R1_vs_v004_Front.png' 'P02R1 - FRONT' 'v004 - FRONT'
Pair (Join-Path $p02 'Clay_3Q_Front.png') '04_Clay_3Q_Front.png' 'P02R1_vs_v004_3Q.png' 'P02R1 - 3Q' 'v004 - 3Q'
'AEGIS_PHASE03_L1_COMPARISONS_COMPLETE'
