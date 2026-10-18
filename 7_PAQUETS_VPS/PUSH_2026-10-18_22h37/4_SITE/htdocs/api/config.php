<?php
// Mystic : réglages de l'API du site.
// Sur le VPS, mettre ici les identifiants MariaDB du serveur (ou créer config.local.php qui écrase ces valeurs).
return [
    'bdd' => [
        'hote'        => '127.0.0.1',
        'port'        => 3306,
        'utilisateur' => 'root',
        'mot_de_passe'=> '',
        'base_auth'   => 'auth',
        'base_monde'  => 'world',
    ],
    // ports testés pour le statut (adresse vue depuis le site, donc le VPS lui-même)
    'serveur' => [
        'hote'        => '127.0.0.1',
        'port_auth'   => 446,
        'port_monde'  => 5556,
    ],
    // classements : comptes au rôle strictement supérieur exclus (1 = joueur ; les comptes du staff ne s'affichent pas)
    'role_max_classement' => 1,
    'taille_classement'   => 50,
    // inscription
    'inscription' => [
        'ouverte'          => true,
        'max_par_ip_heure' => 3,       // comptes créés au plus par adresse IP et par heure
        'delai_min_s'      => 4,       // un formulaire envoyé plus vite est un robot
    ],
    'cache_s' => 60,                    // durée de cache des classements et du statut
    'secret'  => 'CHANGER-CE-SECRET-SUR-LE-VPS',  // sert à signer le jeton du formulaire
];
