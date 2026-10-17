@echo off
rem Double-cliquer ici (sur le VPS) : installe le serveur web des mises a jour du launcher.
rem Demande les droits administrateur puis lance VPS_installer_serveur_web.ps1 (meme dossier).
net session >nul 2>&1
if %errorlevel% neq 0 (
  echo Demande des droits administrateur...
  powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0VPS_installer_serveur_web.ps1"
echo.
pause
