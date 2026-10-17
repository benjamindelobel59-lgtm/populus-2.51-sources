@echo off
rem Double-cliquer ici (sur le PC) : prepare la mise a jour du launcher dans le dossier "publication".
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0preparer_maj.ps1"
echo.
pause
