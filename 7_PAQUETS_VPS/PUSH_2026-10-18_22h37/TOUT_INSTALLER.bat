@echo off
rem MYSTIC - a double-cliquer SUR LE VPS, dans ce dossier (PUSH_2026-10-18_22h37).
rem Installe tout le paquet : sauvegarde, base auth, AuthServer, site, launcher, pare-feu, tests.
rem Le detail et l'annulation sont dans LISEZ-MOI.md. Journal : installation_<date>.log dans ce dossier.
net session >nul 2>&1
if %errorlevel% neq 0 (
  echo Demande des droits administrateur...
  powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0outils\tout_installer.ps1"
