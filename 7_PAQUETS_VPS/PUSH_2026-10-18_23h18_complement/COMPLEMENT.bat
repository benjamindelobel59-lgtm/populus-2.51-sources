@echo off
rem MYSTIC - complement du 18/10/2026 23h18, a double-cliquer SUR LE VPS APRES TOUT_INSTALLER.bat du gros paquet.
rem Voir LISEZ-MOI_COMPLEMENT.md. Journal : complement_<date>.log dans ce dossier.
net session >nul 2>&1
if %errorlevel% neq 0 (
  echo Demande des droits administrateur...
  powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0outils\complement.ps1"
