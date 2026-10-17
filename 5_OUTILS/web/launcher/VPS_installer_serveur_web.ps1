# À exécuter UNE FOIS sur le VPS (Windows Server 2022), dans PowerShell lancé « en tant qu'administrateur ».
# Installe le serveur web IIS et publie le dossier C:\NekfeuMaj à l'adresse http://<IP du VPS>/maj/
# (utilisé par le launcher Dofus Nekfeu pour télécharger les mises à jour).
#
# Pour annuler : Uninstall-WindowsFeature Web-Server ; supprimer C:\NekfeuMaj ; supprimer la règle de pare-feu « Nekfeu MAJ (HTTP 80) ».

$ErrorActionPreference = "Stop"
$dossier = "C:\NekfeuMaj"

# 1. IIS (serveur web de Windows), sans module ASP : fichiers statiques seulement
Install-WindowsFeature Web-Server, Web-Static-Content, Web-Mgmt-Console | Out-Null
Import-Module WebAdministration

# 2. dossier des mises à jour
if (-not (Test-Path $dossier)) { New-Item -ItemType Directory -Path $dossier | Out-Null }
if (-not (Test-Path "$dossier\files")) { New-Item -ItemType Directory -Path "$dossier\files" | Out-Null }

# 3. adresse http://<IP>/maj/ -> C:\NekfeuMaj
if (-not (Get-WebVirtualDirectory -Site "Default Web Site" -Name "maj")) {
    New-WebVirtualDirectory -Site "Default Web Site" -Name "maj" -PhysicalPath $dossier | Out-Null
}

# pas de liste du contenu du dossier ; manifest jamais mis en cache
Set-WebConfigurationProperty -PSPath "IIS:\Sites\Default Web Site\maj" -Filter "system.webServer/directoryBrowse" -Name enabled -Value $false
Set-WebConfigurationProperty -PSPath "IIS:\Sites\Default Web Site\maj" -Filter "system.webServer/staticContent/clientCache" -Name cacheControlMode -Value DisableCache

# types de fichiers servis (.bin et .txt existent normalement déjà dans IIS)
foreach ($ext in @(".bin", ".txt")) {
    $existe = Get-WebConfigurationProperty -PSPath "IIS:\" -Filter "system.webServer/staticContent/mimeMap[@fileExtension='$ext']" -Name fileExtension
    if (-not $existe) {
        Add-WebConfigurationProperty -PSPath "IIS:\" -Filter "system.webServer/staticContent" -Name "." -Value @{ fileExtension = $ext; mimeType = $(if ($ext -eq ".txt") { "text/plain" } else { "application/octet-stream" }) }
    }
}

# 4. pare-feu Windows : port 80 entrant
if (-not (Get-NetFirewallRule -DisplayName "Nekfeu MAJ (HTTP 80)" -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName "Nekfeu MAJ (HTTP 80)" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow | Out-Null
}

Write-Host "Serveur web prêt. Copiez le contenu du dossier « publication » dans $dossier"
Write-Host "Test : ouvrir http://localhost/maj/manifest.txt sur le VPS (après la copie)."
Write-Host "Si le port 80 reste fermé depuis l'extérieur, l'ouvrir aussi dans le panneau de l'hébergeur du VPS."
