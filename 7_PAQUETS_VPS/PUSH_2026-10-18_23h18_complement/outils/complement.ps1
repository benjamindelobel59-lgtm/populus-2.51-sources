# MYSTIC — complément du 18/10/2026 23h18 au gros paquet PUSH_2026-10-18_22h37.
# Contient seulement ce qui a changé après la copie du gros paquet sur le VPS :
#   - AuthServer launcher-auth5 (personnages trouvés via worlds_characters) ;
#   - site : « Rattacher un compte existant » (compte.html, compte.js, api/compte_web.php) + index.html ;
#   - launcher avec l'icône Mystic, Dofus.exe avec l'œuf Dofus (listes manifest/complet + fichier).
# À lancer APRÈS TOUT_INSTALLER.bat du gros paquet. Journal : complement_<date>.log dans ce dossier.

$ErrorActionPreference = 'Stop'
$Paquet = Split-Path -Parent $PSScriptRoot
$Horo = Get-Date -Format 'yyyy-MM-dd_HH\hmm'
Start-Transcript -Path (Join-Path $Paquet "complement_$Horo.log") | Out-Null

$Populus = 'C:\Populus\VPS\Populus'
$Auth    = Join-Path $Populus 'AuthServer'
$Htdocs  = 'C:\xampp\htdocs'
$Maj     = 'C:\NekfeuMaj'
$Save    = "C:\Populus\SAVE\2026-10-18_avant-complement-23h18"
$PortApi = 8090

$bilan = New-Object System.Collections.ArrayList
function Ok($t)        { Write-Host "  [OK]  $t" -ForegroundColor Green;  [void]$bilan.Add("OK      $t") }
function Attention($t) { Write-Host "  [!!]  $t" -ForegroundColor Yellow; [void]$bilan.Add("A VOIR  $t") }
function Erreur($t)    { Write-Host "  [XX]  $t" -ForegroundColor Red;    [void]$bilan.Add("ERREUR  $t") }
function Titre($t)     { Write-Host ""; Write-Host "=== $t" -ForegroundColor Cyan }

Write-Host ""
Write-Host "  MYSTIC - complement du 18/10/2026 23h18 (rattacher un compte, persos, icones)" -ForegroundColor Magenta
Write-Host ""

try {
    Titre '1. Verifications'
    foreach ($d in @($Auth, $Htdocs)) { if (-not (Test-Path $d)) { throw "Dossier introuvable : $d" } }
    if (-not (Test-Path "$Htdocs\atelier.html") -or -not (Test-Path "$Maj\complet.txt")) {
        throw "Le gros paquet n'est pas encore installe. Copie ce dossier PAR-DESSUS PUSH_2026-10-18_22h37 (remplacer) puis lance TOUT_INSTALLER.bat : le complement sera inclus."
    }
    Ok 'Gros paquet deja installe (site + installation du launcher presents)'
    while (Get-Process -Name 'Stump.GUI.AuthConsole' -ErrorAction SilentlyContinue) {
        Write-Host ""
        Write-Host "  L'AuthServer est ouvert. Ferme sa fenetre (le WorldServer peut rester ouvert), puis appuie sur Entree." -ForegroundColor Yellow
        [void](Read-Host)
    }
    Ok 'AuthServer ferme'

    Titre "2. Sauvegarde -> $Save"
    New-Item -ItemType Directory -Force -Path "$Save\AuthServer", "$Save\htdocs\assets", "$Save\htdocs\api", "$Save\NekfeuMaj" | Out-Null
    Get-ChildItem $Auth -Filter 'Stump.*' -File | Copy-Item -Destination "$Save\AuthServer"
    foreach ($f in 'index.html', 'compte.html', 'assets\compte.js', 'api\compte_web.php') { if (Test-Path "$Htdocs\$f") { Copy-Item "$Htdocs\$f" "$Save\htdocs\$f" } }
    foreach ($f in 'MysticLauncher.exe', 'manifest.txt', 'complet.txt') { if (Test-Path "$Maj\$f") { Copy-Item "$Maj\$f" "$Save\NekfeuMaj\$f" } }
    Ok 'AuthServer, fichiers du site et listes du launcher sauvegardes'

    Titre '3. AuthServer'
    robocopy (Join-Path $Paquet '3_SERVEUR\AuthServer') $Auth /E /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Copie de l'AuthServer en echec (robocopy $LASTEXITCODE)" }
    Ok 'AuthServer launcher-auth5 copie'

    Titre '4. Site'
    robocopy (Join-Path $Paquet '4_SITE\htdocs') $Htdocs /E /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Copie du site en echec (robocopy $LASTEXITCODE)" }
    Ok 'Mon compte (Rattacher un compte existant) + API + accueil copies'

    Titre '5. Launcher et Dofus.exe'
    $src = Join-Path $Paquet '5_LAUNCHER\maj'
    robocopy "$src\files" "$Maj\files" /E /XC /XN /XO /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Copie de Dofus.exe en echec (robocopy $LASTEXITCODE)" }
    Copy-Item "$src\MysticLauncher.exe" "$Maj\MysticLauncher.exe" -Force
    Copy-Item "$src\complet.txt" "$Maj\complet.txt" -Force
    Copy-Item "$src\manifest.txt" "$Maj\manifest.txt" -Force   # en dernier
    Ok "Launcher (icone Mystic) + Dofus.exe (oeuf Dofus) publies ; $((Get-Content "$Maj\manifest.txt" -TotalCount 1))"

    Titre "6. Redemarrage de l'AuthServer"
    Start-Process -FilePath (Join-Path $Auth 'Stump.GUI.AuthConsole.exe') -WorkingDirectory $Auth
    $api = $null
    for ($i = 0; $i -lt 30 -and -not $api; $i++) { Start-Sleep -Seconds 2; try { $api = Invoke-WebRequest "http://127.0.0.1:$PortApi/statut" -UseBasicParsing -TimeoutSec 3 } catch { } }
    if ($api) { Ok "API du launcher en ligne : $($api.Content)" } else { Erreur "L'API du launcher ne repond pas sur le port $PortApi : regarder la fenetre de l'AuthServer" }

    Titre '7. Tests'
    foreach ($t in @(
        @{ n = 'Mon compte (rattacher)'; u = 'http://127.0.0.1/compte.html'; c = 'Rattacher un compte existant' },
        @{ n = 'Liste du launcher';      u = 'http://127.0.0.1/maj/manifest.txt'; c = 'Dofus.exe' },
        @{ n = 'Installation complete';  u = 'http://127.0.0.1/maj/complet.txt'; c = 'Dofus.exe' })) {
        try { $r = Invoke-WebRequest $t.u -UseBasicParsing -TimeoutSec 15; if ($r.Content -match [regex]::Escape($t.c)) { Ok "$($t.n) ($($t.u))" } else { Erreur "$($t.n) : reponse inattendue" } }
        catch { Erreur "$($t.n) : $($_.Exception.Message)" }
    }
    try {
        $r = Invoke-WebRequest 'http://127.0.0.1/api/compte_web.php?a=rattacher' -Method Post -Body '{}' -ContentType 'application/json' -Headers @{ 'X-Mystic' = '1' } -UseBasicParsing -TimeoutSec 15
        Erreur "API rattacher : reponse inattendue ($($r.StatusCode))"
    } catch {
        $code = [int]$_.Exception.Response.StatusCode
        if ($code -eq 401) { Ok 'API rattacher en place (refuse sans connexion, normal)' } else { Erreur "API rattacher : code $code" }
    }
}
catch {
    Erreur "ARRET : $($_.Exception.Message)"
    Write-Host "Rien n'est perdu : sauvegarde dans $Save." -ForegroundColor Yellow
}
finally {
    Titre 'Bilan'
    $bilan | ForEach-Object { Write-Host "  $_" }
    Stop-Transcript | Out-Null
    Write-Host ""
    Read-Host 'Appuie sur Entree pour fermer'
}
