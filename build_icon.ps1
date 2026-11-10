# Match the Stolen Goods Filter thumbnail's palette and Segoe UI Bold exactly.
Add-Type -AssemblyName System.Drawing
$bitmap = [System.Drawing.Bitmap]::new(1024, 1024)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$graphics.Clear([System.Drawing.ColorTranslator]::FromHtml('#141820'))
$panelBrush = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#222B35'))
$textBrush = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#F1F1EC'))
$border = [System.Drawing.Pen]::new([System.Drawing.ColorTranslator]::FromHtml('#778490'), 5)
$path = [System.Drawing.Drawing2D.GraphicsPath]::new()
$path.AddArc(60, 246, 96, 96, 180, 90)
$path.AddArc(868, 246, 96, 96, 270, 90)
$path.AddArc(868, 634, 96, 96, 0, 90)
$path.AddArc(60, 634, 96, 96, 90, 90)
$path.CloseFigure()
$graphics.FillPath($panelBrush, $path)
$graphics.DrawPath($border, $path)
$font = [System.Drawing.Font]::new('Segoe UI', 78, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$format = [System.Drawing.StringFormat]::new()
$format.Alignment = [System.Drawing.StringAlignment]::Center
$format.LineAlignment = [System.Drawing.StringAlignment]::Center
$graphics.DrawString("Auto-Explore`nShearing`nExtension", $font, $textBrush, [System.Drawing.RectangleF]::new(100, 266, 824, 444), $format)
$bitmap.Save((Join-Path $PSScriptRoot 'preview.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object MimeType -eq 'image/jpeg'
$encoding = [System.Drawing.Imaging.EncoderParameters]::new(1)
$encoding.Param[0] = [System.Drawing.Imaging.EncoderParameter]::new([System.Drawing.Imaging.Encoder]::Quality, [long]92)
$bitmap.Save((Join-Path $PSScriptRoot 'preview.jpg'), $codec, $encoding)
$encoding.Dispose()
$format.Dispose()
$font.Dispose()
$path.Dispose()
$border.Dispose()
$textBrush.Dispose()
$panelBrush.Dispose()
$graphics.Dispose()
$bitmap.Dispose()
$thumbnail = Get-Item -LiteralPath (Join-Path $PSScriptRoot 'preview.jpg')
if ($thumbnail.Length -ge 1MB) { throw 'Workshop thumbnail must be below 1 MB' }
Write-Output "Created 1024x1024 preview.png and preview.jpg ($($thumbnail.Length) JPEG bytes)."
