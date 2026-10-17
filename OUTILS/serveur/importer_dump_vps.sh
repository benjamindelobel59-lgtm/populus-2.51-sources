#!/usr/bin/env bash
# MYSTIC : remet la base world locale a jour depuis un dump du VPS, puis rejoue nos scripts SQL par-dessus.
#  1. sauvegarde complete de la base world locale (SAVE/1_BASE_DE_DONNEES/<date>_world_complete_avant-import-vps)
#  2. import du dump le plus recent du dossier « SQL VPS » (les tables du dump remplacent les locales ;
#     les tables qui n'existent qu'en local — nekfeu_trousseau, nekfeu_ia_soins, admin_panel_*... — restent)
#  3. scripts du paquet PUSH le plus recent : 2_BDD/01_*.sql ... dans l'ordre (arret a la 1re erreur)
# Le serveur World doit etre ARRETE. Utilisation : bash _OUTILS/serveur/importer_dump_vps.sh [dump.sql]
set -u
RACINE="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$RACINE"
M=_OUTILS/logiciels/MariaDB/bin
if tasklist 2>/dev/null | grep -qi "WorldConsole"; then echo "ERREUR : le World tourne, arrete-le d'abord."; exit 1; fi
"$M/mysql.exe" -uroot -h127.0.0.1 -e "select 1" >/dev/null 2>&1 || { echo "ERREUR : MariaDB ne repond pas (1 - Demarrer la base.bat)."; exit 1; }

DUMP="${1:-$(ls -t "SQL VPS"/*.sql 2>/dev/null | head -1)}"
[ -f "$DUMP" ] || { echo "ERREUR : aucun dump .sql dans « SQL VPS »."; exit 1; }
PAQUET="$(ls -td PUSH/PUSH_*/ | head -1)"
echo "Dump VPS : $DUMP ($(du -h "$DUMP" | cut -f1))"
echo "Scripts  : ${PAQUET}2_BDD"

B="SAVE/1_BASE_DE_DONNEES/$(date +%Y-%m-%d_%Hh%M)_world_complete_avant-import-vps"
mkdir -p "$B"
echo "1/3 Sauvegarde de la base locale -> $B"
"$M/mysqldump.exe" -uroot -h127.0.0.1 --single-transaction world > "$B/world.sql" || { echo "ERREUR sauvegarde"; exit 1; }

echo "2/3 Import du dump VPS (patience)..."
"$M/mysql.exe" -uroot -h127.0.0.1 --default-character-set=utf8mb4 world < "$DUMP" || { echo "ERREUR import du dump : base locale a restaurer depuis $B/world.sql"; exit 1; }

echo "3/3 Nos scripts :"
for f in "${PAQUET}2_BDD"/[0-9][0-9]_*.sql; do
    printf '   %s ... ' "$(basename "$f")"
    if "$M/mysql.exe" -uroot -h127.0.0.1 --default-character-set=utf8mb4 world < "$f" 2> "$TEMP/import_vps_err.txt"; then
        echo ok
    else
        echo ERREUR; cat "$TEMP/import_vps_err.txt"; echo "Arret. Base locale d'avant : $B/world.sql"; exit 1
    fi
done
echo "Termine : base locale = VPS + nos scripts. Sauvegarde d'avant : $B/world.sql"
