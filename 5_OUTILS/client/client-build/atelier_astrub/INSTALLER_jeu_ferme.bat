@echo off
chcp 65001 >nul
title Mystic - client : maison-atelier d'Astrub
rem MYSTIC 19/10 : carte 191890437 (maps0.d2p) + repère minimap (Hints.d2o) dans les DEUX clients. A lancer JEU FERME.
tasklist | findstr /i "Dofus.exe" >nul && (echo. & echo   Ferme d'abord le jeu, puis relance ce fichier. & echo. & pause & exit /b 1)
set "ICI=%~dp0client"
for %%R in ("%~dp0..\..\..\..") do set "RACINE=%%~fR"
set "STAMP=2026-10-19_avant-maison-atelier"
call :installer CLIENT_64bits 3_CLIENT_64bits
call :installer CLIENT_32bits 4_CLIENT_32bits
echo.
echo   Termine : maison-atelier installee (relance le jeu).
echo.
pause
exit /b 0

:installer
set "A=%RACINE%\%1\app"
set "S=%RACINE%\SAVE\%2\fichiers_remplaces\%STAMP%"
if not exist "%S%" mkdir "%S%"
call :un "%A%\content\maps" maps0.d2p "%S%"
call :un "%A%\data\common" Hints.d2o "%S%"
echo   %1 : installe (anciens fichiers dans SAVE\%2\fichiers_remplaces\%STAMP%)
exit /b 0

:un
if not exist "%~3\%2" copy /y "%~1\%2" "%~3\" >nul
copy /y "%ICI%\%2" "%~1\%2" >nul
exit /b 0
