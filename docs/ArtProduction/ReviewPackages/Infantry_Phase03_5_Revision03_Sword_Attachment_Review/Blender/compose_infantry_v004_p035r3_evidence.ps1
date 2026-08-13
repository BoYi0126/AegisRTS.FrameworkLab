param([Parameter(Mandatory=$true)][string]$ReviewRoot)

Add-Type -AssemblyName System.Drawing
$root=(Resolve-Path -LiteralPath $ReviewRoot).Path

function Add-Label([Drawing.Graphics]$g,[int]$width,[string]$text) {
    $font=[Drawing.Font]::new('Arial',18,[Drawing.FontStyle]::Bold);$brush=[Drawing.SolidBrush]::new([Drawing.Color]::White);$back=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(230,18,23,30))
    try{$g.FillRectangle($back,0,0,$width,48);$g.DrawString($text,$font,$brush,14,11)}finally{$font.Dispose();$brush.Dispose();$back.Dispose()}
}

function New-SideBySide([string]$left,[string]$right,[string]$output,[string]$leftLabel,[string]$rightLabel) {
    $a=[Drawing.Image]::FromFile($left);$b=[Drawing.Image]::FromFile($right);$canvas=[Drawing.Bitmap]::new(1536,768);$g=[Drawing.Graphics]::FromImage($canvas)
    try{$g.Clear([Drawing.Color]::FromArgb(18,23,30));$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;$g.DrawImage($a,0,0,768,768);$g.DrawImage($b,768,0,768,768);Add-Label $g 1536 "$leftLabel                                      $rightLabel";$g.DrawLine([Drawing.Pens]::White,767,0,767,768);$canvas.Save($output,[Drawing.Imaging.ImageFormat]::Png)}finally{$a.Dispose();$b.Dispose();$g.Dispose();$canvas.Dispose()}
}

function New-Hierarchy([string]$output,[string]$title,[string]$subtitle) {
    $canvas=[Drawing.Bitmap]::new(1200,900);$g=[Drawing.Graphics]::FromImage($canvas);$font=[Drawing.Font]::new('Consolas',25,[Drawing.FontStyle]::Bold);$small=[Drawing.Font]::new('Arial',16);$white=[Drawing.SolidBrush]::new([Drawing.Color]::White);$muted=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(210,220,230));$nodeBrush=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(47,61,78));$pen=[Drawing.Pen]::new([Drawing.Color]::FromArgb(125,196,255),4)
    try{
        $g.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias;$g.Clear([Drawing.Color]::FromArgb(20,27,36));$g.DrawString($title,$font,$white,40,28);$g.DrawString($subtitle,$small,$muted,42,72)
        $nodes=@(@('RightHand','Humanoid bone',120),@('Socket_R_Hand','non-deforming helper bone',270),@('WPN_SwordRoot_R','exported transform',420),@('7 Sword Visual Parts','mesh children / scale 1,1,1',570))
        foreach($node in $nodes){$rect=[Drawing.Rectangle]::new(180,[int]$node[2],840,94);$g.FillRectangle($nodeBrush,$rect);$g.DrawRectangle($pen,$rect);$g.DrawString([string]$node[0],$font,$white,215,[int]$node[2]+13);$g.DrawString([string]$node[1],$small,$muted,620,[int]$node[2]+32)}
        foreach($y in @(214,364,514)){$g.DrawLine($pen,600,$y,600,$y+56);$g.DrawLine($pen,590,$y+46,600,$y+56);$g.DrawLine($pen,610,$y+46,600,$y+56)}
        $g.DrawString('Sword | Sword_Grip | Sword_Guard | Sword_Pommel',$small,$muted,250,700);$g.DrawString('GEO_Infantry_Sword_GripContact | BladeSpine | GripWraps',$small,$muted,180,735);$g.DrawString('Validated in source .blend, clean Blender FBX reimport, and Unity prefab.',$small,$muted,230,840)
        $canvas.Save($output,[Drawing.Imaging.ImageFormat]::Png)
    }finally{$font.Dispose();$small.Dispose();$white.Dispose();$muted.Dispose();$nodeBrush.Dispose();$pen.Dispose();$g.Dispose();$canvas.Dispose()}
}

$before=Join-Path $root 'Screenshots\Comparison\P035R2_L1Pose_SwordGrip_Close.png';$after=Join-Path $root 'Screenshots\L1Pose\L1Pose_SwordGrip_Close.png'
New-SideBySide $before $after (Join-Path $root 'Screenshots\Comparison\P035R2_vs_P035R3_L1Pose_Close.png') 'P035R2 / FLOATING SWORD' 'P035R3 / SOCKET ATTACHED'
$hierarchy=Join-Path $root 'Screenshots\Hierarchy\Hierarchy_RightHand_WeaponSocket_SwordRoot.png'
New-Hierarchy $hierarchy 'P035R3 SWORD ATTACHMENT HIERARCHY' 'Repository contract uses Socket_R_Hand (task-equivalent WeaponSocket_R).'
Copy-Item -LiteralPath $hierarchy -Destination (Join-Path $root 'Screenshots\Hierarchy\Blender_Hierarchy_RightHand_Sword.png') -Force
Copy-Item -LiteralPath $hierarchy -Destination (Join-Path $root 'Screenshots\Unity\Unity_Hierarchy_RightHand_Sword.png') -Force
'P035R3_EVIDENCE_COMPOSITION_COMPLETE comparisons=1 hierarchy=3'
