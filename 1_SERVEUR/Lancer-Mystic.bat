@echo off
chcp 65001 >nul
title Mystic · Lancement
color 0D
echo.
echo        ▀▀▀▀▀▀▀▀▄▄     M Y S T I C     ▄▄▀▀▀▀▀▀▀▀
echo           ▀▀▀▀▀▀▄▄   Dofus  2.51   ▄▄▀▀▀▀▀▀
echo.
echo     ✦  Démarrage du serveur de connexion...
start "Mystic · Serveur de connexion" "%~dp0Start_AUTH.bat"
echo     ✦  Le serveur de jeu démarre dans 10 secondes...
timeout /t 10 /nobreak >nul
start "Mystic · Serveur de jeu" "%~dp0Start_WORLD.bat"
echo     ✔  Les deux serveurs sont lancés, chacun dans sa fenêtre.
echo.
timeout /t 5 >nul
