@echo off
chcp 65001 >nul
title Mystic · Serveur de jeu
color 0F
cd /d "%~dp0run\debug\WorldServer"
if not exist "%cd%\Stump.GUI.WorldConsole.exe" (
    echo.
    echo   [Mystic] Stump.GUI.WorldConsole.exe introuvable dans "%cd%".
    echo   Compile d'abord les sources ^(voir LISEZ-MOI^).
    echo.
    pause
    exit /b 1
)

:boucle
"%cd%\Stump.GUI.WorldConsole.exe"
echo.
echo   ────────────────────────────────────────────────────────────
echo    ✖  Le serveur de jeu s'est arrêté ^(%date% %time:~0,8%^).
echo       Redémarrage automatique dans 10 secondes...
echo       Ferme cette fenêtre pour l'arrêter définitivement.
echo   ────────────────────────────────────────────────────────────
echo.
timeout /t 10
goto boucle
