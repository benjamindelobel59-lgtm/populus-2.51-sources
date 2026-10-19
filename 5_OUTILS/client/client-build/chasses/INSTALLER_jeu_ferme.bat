@echo off
chcp 65001 >nul
title Mystic - client : lien « Y aller » des chasses au tresor
rem MYSTIC 19/10 : installe DofusInvoker.swf avec le lien {chasse,...} du chat (AuthorizedFrame modifie) dans les DEUX clients.
rem A lancer JEU FERME. Les anciens fichiers sont sauvegardes dans SAVE\3_CLIENT_64bits et SAVE\4_CLIENT_32bits.
tasklist | find /i "Dofus.exe" >nul && (echo. & echo   Ferme d'abord le jeu, puis relance ce fichier. & echo. & pause & exit /b 1)
set "ICI=%~dp0"
for %%R in ("%ICI%..\..\..\..") do set "RACINE=%%~fR"
set "STAMP=2026-10-19_avant-chasses-lien"
call :installer CLIENT_64bits 3_CLIENT_64bits 64
call :installer CLIENT_32bits 4_CLIENT_32bits 32
echo.
echo   Termine : le lien « Y aller » des chasses fonctionnera au prochain lancement du jeu.
echo.
pause
exit /b 0

:installer
set "C=%RACINE%\%1\app"
set "S=%RACINE%\SAVE\%2\fichiers_remplaces\%STAMP%"
if not exist "%S%" mkdir "%S%"
if not exist "%S%\DofusInvoker.swf" copy /y "%C%\DofusInvoker.swf" "%S%\" >nul
copy /y "%ICI%DofusInvoker.%3.chasses.swf" "%C%\DofusInvoker.swf" >nul
echo   %1 : DofusInvoker.swf installe (ancien dans SAVE\%2\fichiers_remplaces\%STAMP%)
exit /b 0
