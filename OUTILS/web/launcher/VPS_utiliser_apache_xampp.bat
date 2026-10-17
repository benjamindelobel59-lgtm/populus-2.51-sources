@echo off
rem A double-cliquer sur le VPS. Le port 80 du VPS est deja utilise par Apache (XAMPP).
rem Ce script publie C:\NekfeuMaj a l'adresse http://<IP du VPS>/maj/ via Apache, et desactive IIS
rem (installe par VPS_installer_serveur_web.bat) pour qu'il ne prenne jamais le port 80 a la place d'Apache.
rem Pour annuler : rmdir "C:\xampp\htdocs\maj"  (supprime seulement le lien, pas C:\NekfeuMaj)
net session >nul 2>&1
if %errorlevel% neq 0 (
  echo Demande des droits administrateur...
  powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)

set HTDOCS=C:\xampp\htdocs
set MAJ=C:\NekfeuMaj

if not exist "%HTDOCS%" (
  echo XAMPP introuvable dans %HTDOCS%.
  echo Ouvrez ce fichier avec le Bloc-notes et corrigez la ligne "set HTDOCS=" avec le bon dossier htdocs.
  pause
  exit /b 1
)

echo 1. Desactivation d'IIS (service W3SVC)...
sc query W3SVC >nul 2>&1
if %errorlevel% equ 0 (
  sc stop W3SVC >nul 2>&1
  sc config W3SVC start= disabled
) else (
  echo    IIS n'est pas installe, rien a faire.
)

echo 2. Dossier des mises a jour : %MAJ%
if not exist "%MAJ%" mkdir "%MAJ%"
rem pas de liste du contenu du dossier
echo Options -Indexes> "%MAJ%\.htaccess"

echo 3. Lien %HTDOCS%\maj vers %MAJ%
if exist "%HTDOCS%\maj" (
  echo    %HTDOCS%\maj existe deja, on n'y touche pas.
) else (
  mklink /J "%HTDOCS%\maj" "%MAJ%"
)

echo.
echo Termine. Test : ouvrir http://localhost/maj/manifest.txt sur le VPS.
pause
