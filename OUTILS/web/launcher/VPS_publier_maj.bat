@echo off
rem A double-cliquer sur le VPS, dans le dossier "launcher" (celui qui contient le dossier "publication").
rem Copie la mise a jour (publication\manifest.txt + publication\files) dans C:\NekfeuMaj, servi par Apache a /maj/.
net session >nul 2>&1
if %errorlevel% neq 0 (
  echo Demande des droits administrateur...
  powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)

set SRC=%~dp0publication
set MAJ=C:\NekfeuMaj

if not exist "%SRC%\manifest.txt" (
  echo Introuvable : %SRC%\manifest.txt
  echo Ce fichier doit etre dans le dossier "launcher", a cote du dossier "publication".
  pause
  exit /b 1
)

if not exist "%MAJ%" mkdir "%MAJ%"
if not exist "%MAJ%\files" mkdir "%MAJ%\files"

echo Copie des fichiers de mise a jour vers %MAJ% ...
xcopy "%SRC%\files\*" "%MAJ%\files\" /Y /Q
rem le manifest en dernier : les joueurs ne voient la nouvelle version qu'une fois tous les fichiers en place
copy /Y "%SRC%\manifest.txt" "%MAJ%\manifest.txt" >nul

echo.
echo Version publiee :
findstr /B "version=" "%MAJ%\manifest.txt"
echo Test : ouvrir http://localhost/maj/manifest.txt
pause
