# MYSTIC — installation automatique du paquet PUSH_2026-10-18_22h37 sur le VPS.
# Lancé par TOUT_INSTALLER.bat (droits administrateur). Ne touche pas au WorldServer (inchangé).
# Étapes : vérifications -> sauvegarde -> base auth (SQL 57) -> AuthServer -> site -> launcher -> pare-feu 8090 -> redémarrage Auth -> tests.
# Un journal complet est écrit à côté du .bat (installation_<date>.log).

$ErrorActionPreference = 'Stop'
$Paquet = Split-Path -Parent $PSScriptRoot
$Horo = Get-Date -Format 'yyyy-MM-dd_HH\hmm'
Start-Transcript -Path (Join-Path $Paquet "installation_$Horo.log") | Out-Null

# ------------------------------------------------------------------ réglages (modifiables ici si le VPS change)
$Populus = 'C:\Populus\VPS\Populus'
$Auth    = Join-Path $Populus 'AuthServer'
$World   = Join-Path $Populus 'WorldServer'
$Htdocs  = 'C:\xampp\htdocs'
$Maj     = 'C:\NekfeuMaj'
$Save    = "C:\Populus\SAVE\2026-10-18_avant-push-22h37"
$PortApi = 8090

$bilan = New-Object System.Collections.ArrayList
function Ok($t)     { Write-Host "  [OK]  $t" -ForegroundColor Green;  [void]$bilan.Add("OK      $t") }
function Attention($t) { Write-Host "  [!!]  $t" -ForegroundColor Yellow; [void]$bilan.Add("A VOIR  $t") }
function Erreur($t) { Write-Host "  [XX]  $t" -ForegroundColor Red;    [void]$bilan.Add("ERREUR  $t") }
function Titre($t)  { Write-Host ""; Write-Host "=== $t" -ForegroundColor Cyan }
function Question($t) { do { $r = Read-Host "$t (O/N)" } until ($r -match '^[oOnN]$'); return $r -match '^[oO]$' }

Write-Host ""
Write-Host "  MYSTIC - installation du paquet du 18/10/2026 22h37" -ForegroundColor Magenta
Write-Host "  AuthServer (launcher, comptes web, partage) + site + launcher + installation complete du jeu"
Write-Host "  Le WorldServer n'est PAS touche : les joueurs en jeu ne sont pas deconnectes."
Write-Host ""

try {
    # ============================================================== 1. vérifications
    Titre '1. Verifications'
    foreach ($d in @($Auth, $World, $Htdocs)) { if (-not (Test-Path $d)) { throw "Dossier introuvable : $d (corriger les reglages en haut de outils\tout_installer.ps1)" } }
    Ok "Dossiers du VPS trouves ($Populus, $Htdocs)"
    foreach ($d in @('2_BDD', '3_SERVEUR\AuthServer', '4_SITE\htdocs', '5_LAUNCHER\maj')) { if (-not (Test-Path (Join-Path $Paquet $d))) { throw "Paquet incomplet : $d manque" } }
    Ok 'Paquet complet'

    # accès MySQL : lus dans la config de l'AuthServer du VPS (jamais écrits ailleurs que dans config.local.php du site)
    [xml]$cfg = Get-Content (Join-Path $Auth 'auth_config.xml') -Raw
    $db = $cfg.SelectNodes("//Variable[@name='DatabaseConfiguration']/DatabaseConfiguration") | Select-Object -First 1
    if (-not $db) { throw "DatabaseConfiguration introuvable dans auth_config.xml" }
    $Bdd = @{ Hote = $db.Host; Port = [int]$db.Port; User = $db.User; Mdp = $db.Password; Auth = $db.DbName }
    [xml]$cfgW = Get-Content (Join-Path $World 'world_config.xml') -Raw
    $dbW = $cfgW.SelectNodes("//Variable[@name='DatabaseConfiguration']/DatabaseConfiguration") | Select-Object -First 1
    $Bdd.Monde = if ($dbW) { $dbW.DbName } else { 'world' }
    Ok "Acces MySQL lus dans auth_config.xml (base $($Bdd.Auth) / $($Bdd.Monde), utilisateur $($Bdd.User))"

    $mysql = @('C:\xampp\mysql\bin\mysql.exe') + (Get-ChildItem 'C:\Program Files\MariaDB*\bin\mysql.exe', 'C:\Program Files\MySQL\MySQL Server*\bin\mysql.exe' -ErrorAction SilentlyContinue | ForEach-Object FullName) |
        Where-Object { Test-Path $_ } | Select-Object -First 1
    if ($mysql) { Ok "Client MySQL : $mysql" } else { Attention "mysql.exe introuvable : le SQL 57 sera a lancer dans Navicat (l'AuthServer cree aussi ces tables tout seul)" }
    $mysqldump = if ($mysql) { Join-Path (Split-Path $mysql) 'mysqldump.exe' } else { $null }

    $php = 'C:\xampp\php\php.exe'
    if (Test-Path $php) {
        $v = & $php -r 'echo PHP_VERSION;'
        if ([version]($v -replace '[^0-9.].*$', '') -lt [version]'7.4') { Erreur "PHP $v trop ancien pour le site (7.4 minimum)" } else { Ok "PHP $v" }
        if (-not ((& $php -m) -match '^pdo_mysql$')) { Attention 'Extension pdo_mysql non activee dans C:\xampp\php\php.ini (decommenter extension=pdo_mysql puis redemarrer Apache)' }
    } else { Attention "PHP de XAMPP introuvable ($php) : verifier que le site PHP fonctionne" }

    # l'AuthServer doit être fermé (le World peut rester ouvert)
    while (Get-Process -Name 'Stump.GUI.AuthConsole' -ErrorAction SilentlyContinue) {
        Write-Host ""
        Write-Host "  L'AuthServer est ouvert. Ferme sa fenetre (le WorldServer peut rester ouvert), puis appuie sur Entree." -ForegroundColor Yellow
        [void](Read-Host)
    }
    Ok 'AuthServer ferme'

    # ============================================================== 2. sauvegarde
    Titre "2. Sauvegarde -> $Save"
    New-Item -ItemType Directory -Force -Path $Save | Out-Null
    New-Item -ItemType Directory -Force -Path "$Save\AuthServer" | Out-Null
    Get-ChildItem $Auth -File | Where-Object { $_.Extension -in '.dll', '.exe', '.pdb', '.xml', '.config' } | Copy-Item -Destination "$Save\AuthServer"
    if (Test-Path "$Auth\swf_patchs") { Copy-Item "$Auth\swf_patchs" "$Save\AuthServer\swf_patchs" -Recurse -Force }
    Ok 'AuthServer sauvegarde (DLL, configs, swf_patchs)'
    if ($mysqldump -and (Test-Path $mysqldump)) {
        $env:MYSQL_PWD = $Bdd.Mdp
        & $mysqldump -h $Bdd.Hote -P $Bdd.Port -u $Bdd.User --single-transaction --default-character-set=utf8 $Bdd.Auth --result-file="$Save\auth.sql"
        if ($LASTEXITCODE -eq 0) { Ok "Base $($Bdd.Auth) sauvegardee ($Save\auth.sql)" } else { Erreur 'Echec de la sauvegarde de la base auth : arret' ; throw 'mysqldump' }
    } else { Attention 'Base auth NON sauvegardee automatiquement : la sauvegarder dans Navicat avant de continuer' ; if (-not (Question 'Continuer quand meme ?')) { throw 'Arret demande' } }
    New-Item -ItemType Directory -Force -Path "$Save\htdocs" | Out-Null
    Get-ChildItem $Htdocs -File -ErrorAction SilentlyContinue | Copy-Item -Destination "$Save\htdocs"
    foreach ($d in 'api', 'assets', 'img', 'atelier') { if (Test-Path "$Htdocs\$d") { robocopy "$Htdocs\$d" "$Save\htdocs\$d" /E /NFL /NDL /NJH /NJS /NP | Out-Null } }
    Ok 'Fichiers du site deja presents dans htdocs sauvegardes'
    New-Item -ItemType Directory -Force -Path "$Save\NekfeuMaj" | Out-Null
    Get-ChildItem $Maj -File -ErrorAction SilentlyContinue | Where-Object { $_.Extension -in '.txt', '.exe' } | Copy-Item -Destination "$Save\NekfeuMaj"
    Ok 'Listes du launcher (manifest / complet) sauvegardees'

    # ============================================================== 3. base auth : SQL 57
    Titre '3. Base auth (SQL 57)'
    if ($mysql) {
        foreach ($sql in Get-ChildItem (Join-Path $Paquet '2_BDD') -Filter *.sql | Sort-Object Name) {
            $env:MYSQL_PWD = $Bdd.Mdp
            Get-Content $sql.FullName -Raw -Encoding UTF8 | & $mysql -h $Bdd.Hote -P $Bdd.Port -u $Bdd.User --default-character-set=utf8 $Bdd.Auth
            if ($LASTEXITCODE -eq 0) { Ok "$($sql.Name) applique" } else { Erreur "$($sql.Name) en erreur (voir le journal)" }
        }
    } else { Attention '57_auth_comptes_web_launcher.sql a lancer a la main dans Navicat (base auth)' }
    Remove-Item Env:\MYSQL_PWD -ErrorAction SilentlyContinue

    # ============================================================== 4. AuthServer
    Titre '4. AuthServer'
    robocopy (Join-Path $Paquet '3_SERVEUR\AuthServer') $Auth /E /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Copie de l'AuthServer en echec (robocopy $LASTEXITCODE)" }
    Ok "DLL de l'AuthServer + swf_patchs\AuthPatch.swf copiees (configs du VPS inchangees)"

    # ============================================================== 5. site
    Titre "5. Site Mystic -> $Htdocs"
    if (Test-Path "$Htdocs\index.php") {
        Move-Item "$Htdocs\index.php" "$Save\htdocs\index.php.xampp" -Force
        Ok "index.php de XAMPP mis de cote (sinon il passerait avant le site) -> $Save\htdocs\index.php.xampp"
    }
    robocopy (Join-Path $Paquet '4_SITE\htdocs') $Htdocs /E /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Copie du site en echec (robocopy $LASTEXITCODE)" }
    Ok 'Site copie (accueil, mon compte, atelier + donnees et icones, API)'
    $local = "$Htdocs\api\config.local.php"
    if (-not (Test-Path $local)) {
        $b = New-Object byte[] 24; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b)
        $secret = -join ($b | ForEach-Object { $_.ToString('x2') })
        # chaîne PHP entre apostrophes : \ -> \\ et ' -> \'
        $q = { param($s) "'" + ($s -replace '\\', '\\' -replace "'", "\'") + "'" }
        $php_cfg = @"
<?php
// Réglages du site sur le VPS (créé par tout_installer.ps1 le $Horo). Ne jamais partager ce fichier.
return [
    'bdd' => [
        'hote'         => $(& $q $Bdd.Hote),
        'port'         => $($Bdd.Port),
        'utilisateur'  => $(& $q $Bdd.User),
        'mot_de_passe' => $(& $q $Bdd.Mdp),
        'base_auth'    => $(& $q $Bdd.Auth),
        'base_monde'   => $(& $q $Bdd.Monde),
    ],
    'secret' => '$secret',
];
"@
        [IO.File]::WriteAllText($local, $php_cfg, (New-Object Text.UTF8Encoding $false))
        Ok 'api\config.local.php cree (acces MySQL du VPS + secret aleatoire)'
    } else { Ok 'api\config.local.php deja present : conserve' }
    New-Item -ItemType Directory -Force -Path "$Htdocs\api\cache" | Out-Null
    icacls "$Htdocs\api\cache" /grant '*S-1-5-32-545:(OI)(CI)M' | Out-Null
    Ok 'Dossier api\cache inscriptible'

    # ============================================================== 6. launcher + installation complète
    Titre "6. Launcher et installation du jeu -> $Maj"
    New-Item -ItemType Directory -Force -Path "$Maj\files" | Out-Null
    $src = Join-Path $Paquet '5_LAUNCHER\maj'
    $n = (Get-ChildItem "$src\files" -File).Count
    Write-Host "  Copie de $n fichiers du jeu (quelques minutes)..."
    robocopy "$src\files" "$Maj\files" /E /XC /XN /XO /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Copie des fichiers du jeu en echec (robocopy $LASTEXITCODE)" }
    Copy-Item "$src\MysticLauncher.exe" "$Maj\MysticLauncher.exe" -Force
    Copy-Item "$src\complet.txt" "$Maj\complet.txt" -Force
    Copy-Item "$src\manifest.txt" "$Maj\manifest.txt" -Force   # en dernier : la version n'est visible qu'une fois tout en place
    if (-not (Test-Path "$Htdocs\maj")) { cmd /c mklink /J "$Htdocs\maj" $Maj | Out-Null; Ok "Lien $Htdocs\maj -> $Maj cree" }
    Ok "Launcher Mystic + installation complete ($n fichiers ajoutes) publies sur /maj/"

    # ============================================================== 7. pare-feu
    Titre "7. Pare-feu Windows : port $PortApi (API du launcher)"
    if (-not (Get-NetFirewallRule -DisplayName "Mystic launcher API (TCP $PortApi)" -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule -DisplayName "Mystic launcher API (TCP $PortApi)" -Direction Inbound -Protocol TCP -LocalPort $PortApi -Action Allow | Out-Null
        Ok "Regle de pare-feu creee (TCP $PortApi entrant)"
    } else { Ok 'Regle de pare-feu deja presente' }
    if (-not (Get-NetFirewallRule -DisplayName 'Nekfeu MAJ (HTTP 80)' -ErrorAction SilentlyContinue) -and -not (Get-NetFirewallRule -ErrorAction SilentlyContinue | Where-Object { $_.Enabled -eq 'True' -and $_.Direction -eq 'Inbound' } | Get-NetFirewallPortFilter | Where-Object { $_.LocalPort -eq '80' })) {
        New-NetFirewallRule -DisplayName 'Mystic site (HTTP 80)' -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow | Out-Null
        Ok 'Regle de pare-feu creee (TCP 80 entrant)'
    }

    # ============================================================== 8. redémarrage de l'AuthServer
    Titre "8. Redemarrage de l'AuthServer"
    Start-Process -FilePath (Join-Path $Auth 'Stump.GUI.AuthConsole.exe') -WorkingDirectory $Auth
    $api = $null
    for ($i = 0; $i -lt 30 -and -not $api; $i++) {
        Start-Sleep -Seconds 2
        try { $api = Invoke-WebRequest "http://127.0.0.1:$PortApi/statut" -UseBasicParsing -TimeoutSec 3 } catch { }
    }
    if ($api) { Ok "AuthServer demarre, API du launcher en ligne : $($api.Content)" } else { Erreur "L'API du launcher ne repond pas sur le port $PortApi apres 60 s : regarder la fenetre de l'AuthServer" }

    # ============================================================== 9. tests
    Titre '9. Tests'
    $tests = @(
        @{ n = 'Accueil du site';         u = 'http://127.0.0.1/';                       c = 'Mystic' },
        @{ n = 'Statut (API PHP + base)';  u = 'http://127.0.0.1/api/statut.php';         c = '"ok"' },
        @{ n = 'Mon compte';              u = 'http://127.0.0.1/compte.html';            c = 'Mon compte' },
        @{ n = 'Atelier';                 u = 'http://127.0.0.1/atelier.html';           c = 'La Forge' },
        @{ n = "Donnees de l'atelier";    u = 'http://127.0.0.1/atelier/donnees/data.json'; c = '"objets"' },
        @{ n = 'Liste du launcher';       u = 'http://127.0.0.1/maj/manifest.txt';       c = 'version=' },
        @{ n = 'Installation complete';   u = 'http://127.0.0.1/maj/complet.txt';        c = 'version=' }
    )
    foreach ($t in $tests) {
        try {
            $r = Invoke-WebRequest $t.u -UseBasicParsing -TimeoutSec 15
            if ($r.Content -match [regex]::Escape($t.c)) { Ok "$($t.n) ($($t.u))" } else { Erreur "$($t.n) : reponse inattendue ($($t.u))" }
        } catch { Erreur "$($t.n) : $($_.Exception.Message) ($($t.u))" }
    }
    try { $r = Invoke-WebRequest 'http://127.0.0.1/maj/MysticLauncher.exe' -Method Head -UseBasicParsing -TimeoutSec 15; Ok "Telechargement du launcher ($([math]::Round($r.Headers['Content-Length'] / 1MB, 1)) Mo)" }
    catch { Erreur "Telechargement du launcher : $($_.Exception.Message)" }
}
catch {
    Erreur "ARRET : $($_.Exception.Message)"
    Write-Host "Rien n'est perdu : la sauvegarde est dans $Save (voir LISEZ-MOI.md, « Annuler »)." -ForegroundColor Yellow
}
finally {
    Remove-Item Env:\MYSQL_PWD -ErrorAction SilentlyContinue
    Titre 'Bilan'
    $bilan | ForEach-Object { Write-Host "  $_" }
    Write-Host ""
    Write-Host "  A faire depuis TON PC : ouvrir http://IP_DU_VPS/ et http://IP_DU_VPS:8090/statut" -ForegroundColor Cyan
    Write-Host "  Si la 2e adresse ne repond pas : ouvrir le port 8090 (TCP) dans le panneau de l'hebergeur du VPS." -ForegroundColor Cyan
    Stop-Transcript | Out-Null
    Write-Host ""
    Read-Host 'Appuie sur Entree pour fermer'
}
