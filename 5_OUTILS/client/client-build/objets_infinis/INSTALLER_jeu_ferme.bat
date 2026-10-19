@echo off
chcp 65001 >nul
title Mystic - client : objets infinis
rem MYSTIC 19/10 : Items.d2o, i18n_fr.d2i, icones (items\bitmap1_1.d2p, items\vector1_1.d2p) dans les DEUX clients. A lancer JEU FERME.
tasklist | find /i "Dofus.exe" >nul && (echo. & echo   Ferme d'abord le jeu, puis relance ce fichier. & echo. & pause & exit /b 1)
set "ICI=%~dp0client"
for %%R in ("%~dp0..\..\..\..") do set "RACINE=%%~fR"
set "STAMP=2026-10-19_avant-objets-infinis"
call :installer CLIENT_64bits 3_CLIENT_64bits
call :installer CLIENT_32bits 4_CLIENT_32bits
echo.
echo   Termine : objets infinis installes (relance le jeu).
echo.
pause
exit /b 0

:installer
set "A=%RACINE%\%1\app"
set "S=%RACINE%\SAVE\%2\fichiers_remplaces\%STAMP%"
if not exist "%S%" mkdir "%S%"
call :un "%A%\data\common" Items.d2o "%S%"
call :un "%A%\data\i18n" i18n_fr.d2i "%S%"
call :un "%A%\content\gfx\items" bitmap1_1.d2p "%S%"
call :un "%A%\content\gfx\items" vector1_1.d2p "%S%"
echo   %1 : installe (anciens fichiers dans SAVE\%2\fichiers_remplaces\%STAMP%)
exit /b 0

:un
if not exist "%~3\%2" copy /y "%~1\%2" "%~3\" >nul
copy /y "%ICI%\%2" "%~1\%2" >nul
exit /b 0
