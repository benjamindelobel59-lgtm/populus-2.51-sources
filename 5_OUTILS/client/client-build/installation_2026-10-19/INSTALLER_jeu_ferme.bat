@echo off
chcp 65001 >nul
title Mystic - fichiers client du 19-10 (PNJ Oto Mustam + skis de Frigost)
rem MYSTIC 19/10 : installe Npcs.d2o + NpcMessages.d2o (PNJ Oto Mustam), Items.d2o (skis de Frigost), Quests/QuestSteps/QuestStepRewards/QuestObjectives.d2o (Le tour du monde) et i18n_fr.d2i (textes) dans les DEUX clients.
rem A lancer JEU FERME. Les anciens fichiers sont sauvegardes dans SAVE\3_CLIENT_64bits et SAVE\4_CLIENT_32bits.
tasklist | find /i "Dofus.exe" >nul && (echo. & echo   Ferme d'abord le jeu, puis relance ce fichier. & echo. & pause & exit /b 1)
set "ICI=%~dp0"
for %%R in ("%ICI%..\..\..\..") do set "RACINE=%%~fR"
set "STAMP=2026-10-19_avant-installation-19-10"
call :installer CLIENT_64bits 3_CLIENT_64bits
call :installer CLIENT_32bits 4_CLIENT_32bits
echo.
echo   Termine : PNJ, skis et quetes du tour du monde actifs au prochain lancement du jeu.
echo.
pause
exit /b 0

:installer
set "C=%RACINE%\%1\app\data"
set "S=%RACINE%\SAVE\%2\fichiers_remplaces\%STAMP%"
if not exist "%S%" mkdir "%S%"
if not exist "%S%\Npcs.d2o" copy /y "%C%\common\Npcs.d2o" "%S%\" >nul
if not exist "%S%\NpcMessages.d2o" copy /y "%C%\common\NpcMessages.d2o" "%S%\" >nul
if not exist "%S%\Quests.d2o" copy /y "%C%\common\Quests.d2o" "%S%\" >nul
if not exist "%S%\QuestSteps.d2o" copy /y "%C%\common\QuestSteps.d2o" "%S%\" >nul
if not exist "%S%\QuestStepRewards.d2o" copy /y "%C%\common\QuestStepRewards.d2o" "%S%\" >nul
if not exist "%S%\QuestObjectives.d2o" copy /y "%C%\common\QuestObjectives.d2o" "%S%\" >nul
if not exist "%S%\Items.d2o" copy /y "%C%\common\Items.d2o" "%S%\" >nul
if not exist "%S%\i18n_fr.d2i" copy /y "%C%\i18n\i18n_fr.d2i" "%S%\" >nul
copy /y "%ICI%Npcs.d2o" "%C%\common\" >nul
copy /y "%ICI%NpcMessages.d2o" "%C%\common\" >nul
copy /y "%ICI%Items.d2o" "%C%\common\" >nul
copy /y "%ICI%Quests.d2o" "%C%\common\" >nul
copy /y "%ICI%QuestSteps.d2o" "%C%\common\" >nul
copy /y "%ICI%QuestObjectives.d2o" "%C%\common\" >nul
copy /y "%ICI%QuestStepRewards.d2o" "%C%\common\" >nul
copy /y "%ICI%i18n_fr.d2i" "%C%\i18n\" >nul
echo   %1 : fichiers installes (ancienne version dans SAVE\%2\fichiers_remplaces\%STAMP%)
exit /b 0
