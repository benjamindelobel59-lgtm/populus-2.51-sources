/* Panel admin Populus 2.51 : aide pour les admins qui débutent.
   - un encadré repliable « Comment ça marche ? » en haut de chaque onglet ;
   - un onglet ❓ Aide qui regroupe tout (premiers pas, chaque onglet, mots utilisés, questions fréquentes). */
'use strict';

const HELP = {
    home: {
        title: '🏠 Accueil',
        intro: 'La page de départ : un résumé du serveur et des raccourcis vers ce qu’on utilise le plus.',
        steps: [
            'Les chiffres du haut disent combien de joueurs sont en ligne, depuis quand le serveur tourne et la mémoire qu’il utilise.',
            'Les boutons « Raccourcis » envoient une commande en jeu à ta place (mode dieu, invisible, sauvegarde du monde…).',
            'Le bandeau tout en haut montre ton personnage, ta map et ta case. Clique « ◎ Position » pour le mettre à jour après un déplacement.',
        ],
        tips: ['La réponse du serveur à chaque action s’affiche en bas de l’écran, dans « Réponses du serveur ».'],
    },
    players: {
        title: '👥 Joueurs',
        intro: 'Voir les joueurs connectés et agir sur eux : téléporter, donner, sanctionner.',
        steps: [
            'Clique un joueur dans la liste de gauche : sa fiche s’ouvre à droite.',
            '« 🧭 Déplacement » : aller vers lui, l’amener à toi, voir ses infos, ouvrir sa banque ou son inventaire.',
            '« 🎁 Donner » : des kamas, des niveaux, ou un objet (mets son numéro, trouvé dans l’onglet Objets, et la quantité). « Objet jet max » donne l’objet avec les meilleures stats possibles.',
            '« 🎭 Apparence et état » : changer son look, le rendre invisible, mode dieu.',
            '« ⚖️ Sanctions » : rendre muet ou mettre en prison pour un nombre de minutes, expulser (déconnecter) ou bannir. Tout est gardé dans Surveillance › Sanctions.',
            'Un joueur hors ligne ne peut presque rien recevoir : la plupart des actions demandent qu’il soit connecté.',
        ],
        warn: ['Bannir coupe l’accès au compte : vérifie bien le joueur avant de cliquer.'],
    },
    items: {
        title: '🎒 Objets',
        intro: 'Modifier un objet du jeu : niveau, prix, conditions et surtout ses effets (les stats).',
        steps: [
            'Tape le nom de l’objet (ou son numéro) dans la recherche, puis clique-le.',
            'Le haut de la fiche : niveau, prix, poids, conditions pour l’équiper (ex. PL>59 = niveau 60 minimum). Clique « 💾 Enregistrer ».',
            '« ✨ Effets » : chaque ligne est une stat. Min et Max = la fourchette du jet (ex. 10 à 20 Force). « + Ajouter un effet » pour une nouvelle stat, ✕ pour en retirer une, puis « 💾 Enregistrer les effets ».',
            '« Me le donner » met l’objet dans ton inventaire pour tester.',
            '« 🔎 Où le trouver » liste les monstres qui le lâchent et les PNJ qui le vendent : un clic ouvre leur fiche.',
        ],
        tips: ['Les objets déjà possédés par les joueurs gardent leurs stats. Seuls les nouveaux (drop, craft, achat) prennent la nouvelle fourchette.'],
    },
    sets: {
        title: '🧩 Panoplies',
        intro: 'Modifier les bonus qu’on gagne en portant plusieurs objets d’une même panoplie.',
        steps: [
            'Cherche la panoplie par son nom (ou ouvre-la depuis la fiche d’un de ses objets).',
            'Chaque bloc correspond à un nombre d’objets portés : « 2 objets portés », « 3 objets portés »…',
            '« + Bonus » ajoute une ligne : choisis la stat et sa valeur. ✕ la retire. Termine par « 💾 Enregistrer les bonus ».',
        ],
        tips: ['Les bonus ne s’additionnent pas d’un bloc à l’autre : le bloc « 4 objets » doit contenir TOUT ce que le joueur gagne avec 4 objets.',
            'Le joueur reçoit les nouveaux bonus la prochaine fois qu’il change son équipement.'],
    },
    mobs: {
        title: '🐾 Monstres',
        intro: 'Modifier un monstre (stats de chaque grade, sorts, drops) et choisir où il apparaît.',
        steps: [
            'Cherche le monstre par son nom, puis clique-le.',
            'Les « grades » sont les niveaux possibles du monstre (grade 1 = le plus faible, grade 5 = le plus fort). Chaque grade a ses propres PV, PA, PM et résistances.',
            '« ✨ Sorts des grades » : les sorts qu’il lance et leur niveau.',
            '« 💰 Drops » : les objets qu’il peut lâcher, avec un pourcentage de chance par grade. « + Ajouter » pour un nouveau drop.',
            '« 🗺️ Zones d’apparition » : les sous-zones (ou maps) où il apparaît. « Ma sous-zone » remplit avec l’endroit où tu es.',
            '« Faire apparaître le prochain groupe » fait apparaître un groupe tout de suite sur la map, pour vérifier.',
        ],
        tips: ['« Enregistrer et appliquer » met le monstre à jour en jeu sans redémarrer le serveur.'],
    },
    npcs: {
        title: '🧙 PNJ',
        intro: 'Créer et modifier les PNJ : leur apparence, où ils sont, ce qu’ils font et ce qu’ils disent.',
        steps: [
            'Cherche un PNJ par son nom, ou clique « 🗺️ PNJ de ma carte » pour voir ceux autour de toi.',
            '« Apparence » : choisis le corps, la taille, les couleurs, les objets portés. « 👁 Aperçu en jeu » fait apparaître une copie à côté de toi pendant 1 minute.',
            '« Placements » : où le PNJ se trouve. « Placer sur ma position » le met sur ta case actuelle, avec la direction que tu choisis.',
            '« Actions » : ce qui se passe quand on clique le PNJ. Dialogue (il parle), Boutique (il vend), Hôtel de vente, Échange…',
            '« Dialogues » : un dialogue est fait de questions (ce que dit le PNJ) et de réponses (ce que le joueur peut cliquer). Chaque réponse fait quelque chose : aller à une autre question, fermer, téléporter, donner un objet, etc.',
            'Pour ajouter une question, tape le début d’un mot : le panel cherche parmi tous les textes du jeu. Si rien ne convient, écris ton propre texte avec « + Créer la question libre ».',
        ],
        tips: ['Pour qu’un PNJ parle, il faut une action « Dialogue » qui pointe vers sa première question.',
            'Le bouton « ⟳ PNJ » en haut recharge tous les PNJ en jeu si quelque chose n’a pas suivi.',
            'Les textes ajoutés depuis le panel ne s’affichent que chez les joueurs qui ont le client à jour.'],
    },
    spells: {
        title: '✨ Sorts',
        intro: 'Modifier les sorts des classes et des monstres : coût, portée, effets, zone.',
        steps: [
            'Choisis une classe dans la liste, ou tape le nom du sort.',
            'Les boutons « Niveau 1, 2, 3… » choisissent le niveau du sort à modifier.',
            'Le haut : PA, portée, chances de critique, nombre de lancers par tour, relance… Clique « 💾 Enregistrer ce niveau ».',
            '« Effets » : ce que fait le sort (dommages, soin, poussée…). Min et Max = la fourchette, Durée = nombre de tours, Zone et Taille = la forme touchée.',
            '« Effets en coup critique » : la même chose quand le sort fait un coup critique.',
            '« 👁 Aperçu des zones » dessine les cases touchées : bleu = le lanceur, orange = les cases touchées.',
        ],
        tips: ['Le changement compte tout de suite pour les prochains lancers, chez les joueurs comme chez les monstres qui ont ce sort.'],
    },
    maps: {
        title: '🗺️ Cartes',
        intro: 'Voir une map comme en jeu et régler ses placements de combat, ses passages vers les autres maps et ses cases.',
        steps: [
            'Clique « Ma carte » pour ouvrir la map où tu es, ou tape le numéro d’une map, ou ses coordonnées [x,y].',
            'La grille montre les cases par-dessus l’image de la map. La légende sous la grille explique chaque couleur.',
            'Choisis d’abord un MODE avec les boutons au-dessus de la grille, puis clique sur les cases :',
            '— 🔍 Inspecter : clic sur une case = voir ce qu’elle contient.',
            '— 🔵 / 🔴 Placement : les cases de départ des combats (équipe bleue / rouge). ⌫ pour en retirer. Puis « 💾 Enregistrer les placements ».',
            '— ✨ Case téléport : le joueur qui marche sur cette case est envoyé ailleurs (comme un soleil). Remplis « Vers la map » et « Case d’arrivée » (🎯 pour cliquer la case sur l’autre map), puis « + Ajouter ».',
            '— ↔ Passage de bord : les flèches bleues au bord de la map, là où le joueur peut passer à la map d’à côté. Clic = ajouter / retirer.',
            '— 🧱 Cases : clic = rendre une case praticable ou bloquée. Sous la grille : bloquer la vue, interdire en combat…',
            'Après les modes Passage de bord et Cases, clique « 💾 Enregistrer les cases ».',
            'À gauche, « ↔ Changements de map par le bord » : pour chaque côté (haut, bas, gauche, droite), la map où l’on arrive et la case d’arrivée (vide = la case juste en face). 🎯 pour la cliquer. Puis « 💾 Enregistrer ».',
            '« 🧩 Toutes les maps de la zone » montre toutes les maps du coin en petites images : un clic en ouvre une.',
        ],
        warn: ['Le jeu de chaque joueur garde sa propre copie des cases et des flèches de bord. Après avoir modifié des CASES ou des PASSAGES DE BORD : ferme le jeu, lance « Mettre-a-jour-cases-client.bat » (dossier Panel-admin\\rendu-cartes, sur le PC de l’admin principal), puis donne le dossier « maj-cartes-client » aux joueurs. Sans ça, un joueur peut rester bloqué au bord de la map.',
            'Les cases téléport et les maps d’arrivée des bords, elles, marchent tout de suite pour tout le monde.'],
        tips: ['L’image de la map est dessinée par le PC de l’admin principal : si elle affiche « rendu en cours », c’est que ce PC est éteint ou que « Lancer-rendu-cartes.bat » n’est pas ouvert.'],
    },
    server: {
        title: '🩺 Serveur',
        intro: 'La santé du serveur, les taux, les annonces et les redémarrages.',
        steps: [
            'Les chiffres du haut : joueurs, combats, mémoire, taux actuels.',
            '« 📣 Annonce maintenant » : un message affiché à tous les joueurs connectés.',
            '« 📅 Événements programmés » : un bonus de taux (ex. XP x2 le week-end) ou une annonce qui revient toute seule, avec une date de début et de fin.',
            '« 🔁 Redémarrage propre » : prévient les joueurs avec un compte à rebours, sauvegarde, puis redémarre. « Annuler le redémarrage » arrête le compte à rebours.',
        ],
        warn: ['Un redémarrage déconnecte tout le monde : préviens les joueurs et évite pendant les combats importants.'],
    },
    watch: {
        title: '🕵️ Surveillance',
        intro: 'Repérer ce qui est anormal : triche, multi-comptes, kamas qui sortent de nulle part.',
        steps: [
            '🚨 Alertes : ce qui dépasse les seuils (gros gains de kamas ou de niveaux en 24 h, gros échanges…).',
            '📈 Progression : niveaux et kamas gagnés sur 7 jours par personnage.',
            '🌐 IP : quelles IP utilisent plusieurs comptes (multi-comptes).',
            '🤝 Échanges : les échanges entre joueurs, avec ce qui a été donné.',
            '🏪 HDV : ce qui se vend dans les hôtels de vente et à quel prix.',
            '💰 Kamas : les plus riches et la masse totale de kamas, jour par jour.',
            '⚖️ Sanctions : qui a été sanctionné, par qui et pourquoi.',
            '⌨ Commandes MJ : toutes les commandes tapées par les admins.',
        ],
        tips: ['Plusieurs comptes sur une même IP n’est pas forcément de la triche (frères, colocation) : regarde les échanges avant de sanctionner.'],
    },
    support: {
        title: '🛟 Support',
        intro: 'Aider un joueur qui dit avoir perdu des objets ou des kamas.',
        steps: [
            'Le serveur fait une copie de sauvegarde des personnages tous les jours. « Faire une copie maintenant » en fait une tout de suite.',
            'Choisis le joueur et une copie, puis « Comparer » : tu vois ce qui a disparu, ce qui est apparu et les échanges depuis cette copie.',
            '« Rendre » redonne un objet disparu au joueur, avec exactement les mêmes stats.',
        ],
        tips: ['Avant de rendre quelque chose, vérifie dans « Échanges depuis la copie » que le joueur ne l’a pas simplement donné ou vendu.'],
    },
    history: {
        title: '🕘 Historique',
        intro: 'Toutes les modifications faites avec le panel, et le bouton pour les annuler.',
        steps: [
            'Chaque ligne = une modification : qui, quand, quoi.',
            '« ↶ Annuler » remet l’élément comme il était juste avant cette modification, et le recharge en jeu.',
        ],
        tips: ['Si l’élément a été modifié plusieurs fois depuis, le panel te prévient : annuler écrasera aussi les modifications suivantes.'],
    },
    console: {
        title: '⌨ Console',
        intro: 'Taper une commande de jeu comme dans le chat (sans le point du début).',
        steps: ['Écris la commande, par exemple « go 154010371 » ou « item add 2411 moi 1 », puis « Envoyer ».', 'La réponse s’affiche en bas, dans « Réponses du serveur ».'],
        tips: ['« moi » remplace ton nom de personnage dans les commandes.'],
    },
    log: {
        title: '📜 Journal',
        intro: 'La liste brute de tout ce qui s’est passé dans le panel (connexions, commandes, modifications), du plus récent au plus ancien.',
        steps: ['Sert surtout à retrouver qui a fait quoi. Pour annuler une modification, utilise plutôt l’Historique.'],
    },
};

const HELP_START = [
    'Ouvrir le panel : en jeu, avec un personnage administrateur, tape .admin dans le chat. Le panel s’ouvre dans une fenêtre du jeu (ou clique le lien dans le chat).',
    'Le lien ne marche qu’une fois et pendant 10 minutes : retape .admin si le panel dit « session expirée ».',
    'La fenêtre se déplace en glissant la barre du haut, se redimensionne par les bords, — la réduit, ⬜ l’agrandit, ✕ la ferme.',
    'Presque tout s’applique en direct, sans redémarrer le serveur. Après chaque action, un petit message en bas à droite dit si c’est réussi (vert) ou non (rouge).',
    'Une erreur ? Tout est annulable depuis 🕘 Historique.',
    'Sur chaque onglet, l’encadré « ❓ Comment ça marche ? » en haut explique l’onglet.',
];

const HELP_WORDS = [
    ['Map', 'Un écran du jeu. Chaque map a un numéro (ex. 154010371) et des coordonnées [x,y].'],
    ['Case', 'Un losange de la map. Il y en a 560, numérotées de 0 (en haut à gauche) à 559 (en bas à droite).'],
    ['Sous-zone', 'Un groupe de maps qui ont le même nom (ex. « Calanques d’Astrub »). Les monstres apparaissent souvent par sous-zone.'],
    ['Grade', 'Le niveau d’un monstre parmi ses versions possibles (1 = faible, 5 = fort).'],
    ['PNJ', 'Personnage non joueur : marchand, garde, donneur de quête…'],
    ['Jet', 'La valeur tirée au hasard d’une stat d’objet, entre le Min et le Max.'],
    ['Numéro (#)', 'Chaque chose du jeu (objet, monstre, sort, PNJ, map) a un numéro unique, affiché après #. On peut chercher par numéro partout.'],
    ['Recharger', 'Faire prendre en compte une modification par le serveur sans le redémarrer. Le panel le fait tout seul.'],
];

const HELP_FAQ = [
    ['J’ai modifié un PNJ mais rien ne change en jeu', 'Change de map puis reviens, ou clique « ⟳ PNJ » en haut du panel.'],
    ['Mon PNJ ne parle pas', 'Il lui faut une action « Dialogue » (onglet Actions du PNJ) qui pointe vers sa première question, et au moins une réponse à cette question.'],
    ['Le panel dit « serveur de jeu injoignable »', 'Le serveur de jeu est éteint ou en train de démarrer. Attends une minute et réessaie.'],
    ['L’image de la map ne s’affiche pas', 'Les images sont dessinées par le PC de l’admin principal (Lancer-rendu-cartes.bat). La grille marche quand même.'],
    ['Un joueur est bloqué au bord d’une map', 'Son jeu n’a sans doute pas les dernières cases : donne-lui le dossier maj-cartes-client (Installer-cartes.bat).'],
    ['J’ai fait une bêtise', 'Onglet 🕘 Historique, trouve la ligne, clique « ↶ Annuler ».'],
];

function helpSection(k, open) {
    const h = HELP[k];
    if (!h) return '';
    const list = (arr, cls) => arr && arr.length ? `<ul class="${cls || ''}">${arr.map(x => `<li>${esc(x)}</li>`).join('')}</ul>` : '';
    return `<p>${esc(h.intro)}</p>
        ${h.steps ? `<b>Comment faire</b>${list(h.steps)}` : ''}
        ${h.warn ? `<div class="hwarn"><b>⚠ Attention</b>${list(h.warn)}</div>` : ''}
        ${h.tips ? `<div class="htip"><b>💡 Bon à savoir</b>${list(h.tips)}</div>` : ''}`;
}

function tabHelp() {
    const keys = Object.keys(HELP);
    $('#main').innerHTML = `<div class="helpdoc">
        ${card('👋 Premiers pas', `<ul>${HELP_START.map(x => `<li>${esc(x)}</li>`).join('')}</ul>
            <div class="btns" style="margin-top:8px">${keys.map(k => `<a href="#h_${k}" class="pill hl" data-h="${k}">${esc(HELP[k].title)}</a>`).join('')}</div>`)}
        ${keys.map(k => `<div id="h_${k}">${card(esc(HELP[k].title) + ` <button class="small" data-go="${k}">Ouvrir l’onglet →</button>`, helpSection(k))}</div>`).join('')}
        ${card('📖 Les mots utilisés', `<div class="kv">${HELP_WORDS.map(([a, b]) => `<span><b>${esc(a)}</b></span><span>${esc(b)}</span>`).join('')}</div>`)}
        ${card('❔ Questions fréquentes', HELP_FAQ.map(([q, a]) => `<details class="faq"><summary>${esc(q)}</summary><p>${esc(a)}</p></details>`).join(''))}
      </div>`;
    $$('#main [data-h]').forEach(a => a.onclick = e => { e.preventDefault(); $('#h_' + a.dataset.h).scrollIntoView({ behavior: 'smooth' }); });
    $$('#main [data-go]').forEach(b => b.onclick = () => showTab(b.dataset.go));
}
TABS.help = tabHelp;

// encadré « Comment ça marche ? » au-dessus de chaque onglet (hors de #main : il reste quand l'onglet se redessine)
let HELP_OPEN = {};
try { HELP_OPEN = JSON.parse(localStorage.getItem('populusHelpOpen') || '{}'); } catch { }
const showTabBase = showTab;
showTab = function (t) {
    showTabBase(t);
    let box = $('#tabhelp');
    if (!box) {
        box = document.createElement('details');
        box.id = 'tabhelp';
        $('#main').before(box);
        box.addEventListener('toggle', () => {
            HELP_OPEN[box.dataset.k] = box.open;
            try { localStorage.setItem('populusHelpOpen', JSON.stringify(HELP_OPEN)); } catch { }
        });
    }
    const k = location.hash.slice(1);
    if (!HELP[k]) { box.hidden = true; return; }
    box.hidden = false;
    box.dataset.k = k;
    // ouvert la première fois qu'on visite l'onglet, puis comme on l'a laissé
    box.open = HELP_OPEN[k] === undefined ? true : HELP_OPEN[k];
    box.innerHTML = `<summary>❓ Comment ça marche ? <span class="muted">${esc(HELP[k].title)}</span></summary><div class="tabhelpbody">${helpSection(k)}
        <div style="margin-top:6px"><a href="#" id="helpAll">Voir toute l’aide du panel →</a></div></div>`;
    $('#helpAll').onclick = e => { e.preventDefault(); showTab('help'); };
};
