# Prépare une mise à jour pour le launcher Dofus Nekfeu.
# Prend les fichiers du client modifiés (par défaut : le dernier paquet de PUSH, dossier 6_CLIENT_64bits\app ; sortie : 5_LAUNCHER\publication du paquet)
# et produit le dossier « publication » à copier tel quel dans C:\NekfeuMaj sur le VPS :
#   manifest.txt        : version + une ligne par fichier (sha256 <TAB> taille <TAB> chemin relatif à app)
#   files\<sha256>.bin  : le contenu de chaque fichier
#
# Utilisation :  powershell -ExecutionPolicy Bypass -File preparer_maj.ps1 [-Source <dossier app>] [-Sortie <dossier>]

param(
    [string]$Source = "",
    [string]$Sortie = (Join-Path $PSScriptRoot "publication")
)

$ErrorActionPreference = "Stop"
# par defaut : le paquet le plus recent du dossier "PUSH" (rangement du 16/10 : PUSH\<paquet>\6_CLIENT_64bits\app)
if (-not $Source) {
    $paquet = Get-ChildItem -Directory (Join-Path $PSScriptRoot "..\..\..\PUSH") | Where-Object { Test-Path (Join-Path $_.FullName "6_CLIENT_64bits\app") } | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $paquet) { throw "Aucun paquet avec 6_CLIENT_64bits\app dans le dossier PUSH" }
    $Source = Join-Path $paquet.FullName "6_CLIENT_64bits\app"
    # la publication va directement dans le paquet (5_LAUNCHER), avec le .bat a lancer sur le VPS
    if (-not $PSBoundParameters.ContainsKey('Sortie')) {
        $Sortie = Join-Path $paquet.FullName "5_LAUNCHER\publication"
        if (Test-Path $Sortie) { Remove-Item -Recurse -Force $Sortie }
        New-Item -ItemType Directory -Force -Path $Sortie | Out-Null
        Copy-Item (Join-Path $PSScriptRoot "VPS_publier_maj.bat") (Join-Path $paquet.FullName "5_LAUNCHER") -Force
    }
}
$Source = (Resolve-Path $Source).Path.TrimEnd('\')

if (-not (Test-Path $Sortie)) { New-Item -ItemType Directory -Path $Sortie | Out-Null }
$Sortie = (Resolve-Path $Sortie).Path.TrimEnd('\')
$filesDir = Join-Path $Sortie "files"
if (-not (Test-Path $filesDir)) { New-Item -ItemType Directory -Path $filesDir | Out-Null }

$version = Get-Date -Format "yyyy-MM-dd_HH\hmm"
$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("version=$version")

$total = 0
foreach ($f in Get-ChildItem -Path $Source -Recurse -File | Sort-Object FullName) {
    $rel = $f.FullName.Substring($Source.Length + 1).Replace('\', '/')
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $f.FullName).Hash.ToLowerInvariant()
    $lines.Add("$hash`t$($f.Length)`t$rel")

    $blob = Join-Path $filesDir "$hash.bin"
    if (-not (Test-Path $blob)) { Copy-Item -LiteralPath $f.FullName -Destination $blob }
    $total++
}

# UTF-8 sans BOM, fins de ligne \n
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Join-Path $Sortie "manifest.txt"), (($lines -join "`n") + "`n"), $utf8)

Write-Host "Mise à jour $version préparée : $total fichiers depuis $Source"
Write-Host "Dossier à copier sur le VPS (dans C:\NekfeuMaj) : $Sortie"
