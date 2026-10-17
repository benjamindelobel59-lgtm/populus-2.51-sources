# NEKFEU : lance le client 64 bits apres avoir choisi le serveur (VPS ou LOCAL).
# Ne change QUE la ligne connection.host de app\config.xml (le reste de la config est garde), puis lance Dofus.exe
# avec les preferences du PC (%APPDATA%\Dofus-x64). Le dernier choix est memorise (bouton mis en avant).
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# dossier du projet : on remonte jusqu'a trouver le client 64 bits (marche quel que soit le rangement de _OUTILS)
$root = $PSScriptRoot
while ($root -and -not (Test-Path (Join-Path $root "CLIENT_64bits\app\Dofus.exe"))) { $root = Split-Path $root -Parent }
if (-not $root) { [System.Windows.Forms.MessageBox]::Show("Client 64 bits introuvable.") | Out-Null; exit 1 }
$app = Join-Path $root "CLIENT_64bits\app"
$config = Join-Path $app "config.xml"
$memo = Join-Path $PSScriptRoot "dernier_choix.txt"
$servers = [ordered]@{ "VPS" = "IP_DU_VPS"; "LOCAL" = "127.0.0.1" }

$last = ""
try { $last = (Get-Content $memo -ErrorAction Stop | Select-Object -First 1).Trim() } catch {}

$form = New-Object Windows.Forms.Form
$form.Text = "Dofus Mystic - choix du serveur"
$form.StartPosition = "CenterScreen"
$form.FormBorderStyle = "FixedDialog"
$form.MaximizeBox = $false
$form.MinimizeBox = $false
$form.ClientSize = New-Object Drawing.Size(360, 150)
$form.Font = New-Object Drawing.Font("Segoe UI", 10)
$label = New-Object Windows.Forms.Label
$label.Text = "Sur quel serveur te connecter ?"
$label.AutoSize = $true
$label.Location = New-Object Drawing.Point(20, 18)
$form.Controls.Add($label)
$script:choice = $null
$x = 20
foreach ($name in $servers.Keys) {
    $b = New-Object Windows.Forms.Button
    $b.Text = "Serveur $name`n" + $servers[$name]
    $b.Size = New-Object Drawing.Size(150, 70)
    $b.Location = New-Object Drawing.Point($x, 55)
    $b.Tag = $name
    $b.Add_Click({ $script:choice = $this.Tag; $form.Close() })
    if ($name -eq $last) { $form.AcceptButton = $b; $b.Font = New-Object Drawing.Font("Segoe UI", 10, [Drawing.FontStyle]::Bold) }
    $form.Controls.Add($b)
    $x += 170
}
[void]$form.ShowDialog()
if (-not $script:choice) { exit }

# connection.host uniquement (encodage UTF-8 sans BOM, fins de ligne gardees)
$text = [IO.File]::ReadAllText($config, [Text.Encoding]::UTF8)
$new = [Regex]::Replace($text, '(<entry key="connection\.host">)[^<]*(</entry>)', '${1}' + $servers[$script:choice] + '${2}')
if ($new -ne $text) { [IO.File]::WriteAllText($config, $new, (New-Object Text.UTF8Encoding($false))) }
Set-Content -Path $memo -Value $script:choice -Encoding ASCII

Start-Process -FilePath (Join-Path $app "Dofus.exe") -ArgumentList "--prefs=$env:APPDATA\Dofus-x64" -WorkingDirectory $app
