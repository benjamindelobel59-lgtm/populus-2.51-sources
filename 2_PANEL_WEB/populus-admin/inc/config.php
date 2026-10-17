<?php
/*
 * Panel admin Populus 2.51 : réglages.
 * Sur le VPS, crée config.local.php à côté de ce fichier pour changer la base (il n'est jamais écrasé) :
 *   <?php return ['db_user' => 'populus', 'db_pass' => '...'];
 */
if (!defined('POPULUS_ADMIN'))
    exit;

$config = [
    'db_host' => '127.0.0.1',
    'db_port' => 3306,
    'db_user' => 'root',
    'db_pass' => '',
    'db_name' => 'world',        // base du serveur de jeu (PNJ, maps, objets, textes)
    'auth_db' => 'auth',         // base des comptes
    'session_hours' => 12,       // durée d'une session ouverte par .admin
    'queue_wait_ms' => 8000,     // attente max de la réponse du serveur de jeu
    // alertes de l'onglet Surveillance
    'alert_levels_24h' => 40,           // niveaux gagnés en 24 h
    'alert_kamas_24h' => 20000000,      // kamas gagnés en 24 h (perso + banque)
    'alert_accounts_per_ip' => 4,       // comptes connectés depuis la même IP (24 h)
    'alert_trade_kamas' => 1000000,     // échange avec au moins autant de kamas
    'alert_hdv_price_ratio' => 10,      // vente HDV à plus de 10 fois le prix moyen de l'objet
];

if (is_file(__DIR__ . '/config.local.php'))
    $config = array_merge($config, require __DIR__ . '/config.local.php');

return $config;
