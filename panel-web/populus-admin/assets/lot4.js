/* Panel admin Populus 2.51 : onglet Cartes (grille isométrique, placements de combat, téléportations, voisins, contenu de la map). */
'use strict';

const CW = 43, CH = 21.5;               // taille d'une case à l'écran (losange) = image du rendu à 50 %
let MAPVIS = true, MAPIMG = null, MAPIMG_TIMER = null;
let MAP = null, MAPMODE = 'inspect', PLACES = null, SELCELL = -1;
let CELLEDIT = {};                       // cases modifiées pas encore enregistrées : id -> [losMov, mapChange, floor]
const cellData = id => CELLEDIT[id] || MAP.cells[id] || [0, 0, 0];
// passages de bord (octet mapChangeData) : bit -> [nom, sens de la flèche x, y]
const EDGE = { 64: ['haut', 0, -1], 4: ['bas', 0, 1], 16: ['gauche', -1, 0], 1: ['droite', 1, 0] };
const EDGE_SIDE = { Top: 64, Bottom: 4, Left: 16, Right: 1 };
/** Direction de bord naturelle d'une case (0 si elle n'est pas au bord). */
function edgeOf(id) {
    if (id < 28) return 64;
    if (id >= 532) return 4;
    if (id % 28 === 0) return 16;
    if (id % 28 === 27) return 1;
    return 0;
}

const cellXY = id => { const row = Math.floor(id / 14), col = id % 14; return [col * CW + (row % 2) * CW / 2 + CW / 2, row * CH / 2 + CH / 2]; };
const flags = c => ({                  // cartes version >= 9
    walk: (c[0] & 1) === 0, los: (c[0] & 8) === 0, noFight: (c[0] & 2) !== 0, noRp: (c[0] & 4) !== 0, farm: (c[0] & 128) !== 0, visible: (c[0] & 64) !== 0,
    change: c[1],
});

function tabMaps() {
    $('#main').innerHTML = `<div class="cols" style="grid-template-columns:330px 1fr">
      <div>
        <div class="card"><h3>🗺️ Cartes</h3>
          <div class="row"><input id="mapId" type="number" placeholder="numéro de map"><button id="mapGo">Ouvrir</button></div>
          <div class="row" style="margin-top:6px"><input id="mapX" type="number" placeholder="x" style="width:70px"><input id="mapY" type="number" placeholder="y" style="width:70px"><button id="mapXY">[x,y]</button><button id="mapMine" class="pri">Ma carte</button></div>
          <div id="mapAt" class="list" style="margin-top:6px"></div></div>
        <div id="mapSide"></div>
      </div>
      <div id="mapMain"><div class="card empty" style="margin:0">Ouvre ta carte ou une carte par son numéro.</div></div></div>`;
    $('#mapGo').onclick = () => valOf('mapId') && openMap(+valOf('mapId'));
    $('#mapId').onkeydown = e => { if (e.key === 'Enter') $('#mapGo').click(); };
    $('#mapMine').onclick = async () => { const s = await refreshState(); if (s && s.me && s.me.mapId) openMap(s.me.mapId); else toast('Ton personnage n\'est pas en ligne', true); };
    $('#mapXY').onclick = async () => {
        try {
            const r = await api('maps_at', { x: valOf('mapX'), y: valOf('mapY') });
            $('#mapAt').innerHTML = r.maps.length ? r.maps.map(m => `<div class="it" data-id="${m.Id}"><span>${m.Id}</span><small>${esc(m.Zone)} · ${+m.Outdoor ? 'extérieur' : 'intérieur'} · monde ${m.WorldMap}</small></div>`).join('') : '<div class="muted">Aucune map à ces coordonnées.</div>';
            $$('#mapAt .it').forEach(x => x.onclick = () => openMap(+x.dataset.id));
            if (r.maps.length === 1) openMap(+r.maps[0].Id);
        } catch (e) { toast(e.message, true); }
    };
    if (MAP) openMap(MAP.map.Id);
}

async function openMap(id) {
    try { MAP = await api('map', { id }); } catch (e) { toast(e.message, true); return; }
    if (!$('#mapSide')) return;
    $('#mapId').value = id;
    const parse = csv => (csv || '').split(',').filter(x => x !== '').map(Number);
    PLACES = { blue: parse(MAP.map.BlueCellsCSV), red: parse(MAP.map.RedCellsCSV), dirty: false };
    SELCELL = -1;
    CELLEDIT = {};
    loadMapImage(id, MAP.image);
    renderMapSide();
    renderMapMain();
}

function renderMapSide() {
    const m = MAP.map, p = MAP.pos || {};
    const edgeCount = bit => MAP.cells.reduce((k, c, i) => k + ((cellData(i)[1] & bit) ? 1 : 0), 0);
    const nb = (d, label) => {
        const n = MAP.neighbours[d], cl = +m['Client' + d + 'NeighbourId'], cnt = edgeCount(EDGE_SIDE[d]);
        return `<div class="form" style="margin:0;padding:8px">
          <div class="row" style="justify-content:space-between"><b>${label}</b><small class="muted">${cnt} case(s) de passage${cl > 0 && n && cl !== n.id ? ' · le client envoie ' + cl + ', redirigé ici' : ''}</small></div>
          <div class="row" style="margin-top:4px"><input id="nb${d}" type="number" value="${n ? n.id : ''}" placeholder="pas de map" title="Map d'arrivée">
            <input id="nbc${d}" type="number" value="${m[d + 'NeighbourCellId'] ?? ''}" placeholder="case auto" title="Case d'arrivée (vide = la case en face)" style="width:90px">
            ${n ? `<button class="small" data-pick="${d}" title="Choisir la case d'arrivée sur la map">🎯</button><button class="small" data-open="${n.id}" title="${n.pos ? '[' + n.pos.x + ',' + n.pos.y + '] ' + esc(n.pos.zone) : ''}">→</button>` : ''}</div></div>`;
    };
    $('#mapSide').innerHTML =
        card(`Map ${m.Id}`, `<div class="kv"><span>Position</span><span>[${p.x},${p.y}] ${p.outdoor ? 'extérieur' : 'intérieur'}</span><span>Zone</span><span>${esc(p.zone || '')} (#${p.subAreaId || '?'})</span></div>
            <div class="btns" style="margin-top:8px"><button id="mapTp">M'y téléporter</button></div>
            <h4 style="margin:10px 0 4px">↔ Changements de map par le bord</h4>
            <div style="display:grid;gap:6px">${nb('Top', '⬆ Haut')}${nb('Bottom', '⬇ Bas')}${nb('Left', '⬅ Gauche')}${nb('Right', '➡ Droite')}</div>
            <div class="help">Map et case où arrive le joueur qui sort par ce bord (case vide = la case en face). Les cases de passage se règlent sur la grille (mode « ↔ Passage de bord »).</div>
            <label class="row" style="gap:4px;margin-top:6px"><input type="checkbox" id="spawnDis" ${+m.SpawnDisabled ? 'checked' : ''}>Pas de monstres sur cette map</label>
            <div class="row" style="justify-content:flex-end;margin-top:6px"><button id="propsSave">💾 Enregistrer</button></div>`)
        + card(`Téléportations (${MAP.triggers.length})`, (MAP.triggers.length ? MAP.triggers.map(t => `<div class="it" data-cell="${t.CellId}"><span>case ${t.CellId} → <b>${esc(t.Parameter1 || '')}</b> case ${esc(t.Parameter0 || '')}</span>
              <small>${esc(t.Type)}${t.Condition ? ' · si ' + esc(t.Condition) : ''} <a href="#" data-tdel="${t.Id}" style="color:var(--err)">✕</a></small></div>`).join('') : '<div class="muted">Aucune.</div>')
            + `<div class="help">Mode « ✨ Case téléport » puis clic sur une case pour en ajouter une ; clic sur une téléportation de la liste pour la modifier.</div>`
            + (MAP.arrivals.length ? `<div class="muted" style="margin-top:6px">Arrivées depuis : ${MAP.arrivals.map(a => `<a href="#" data-open="${a.MapId}">${a.MapId}</a>`).join(', ')}</div>` : ''))
        + card(`Monstres de la zone (${MAP.mobs.length})`, MAP.mobs.length ? MAP.mobs.map(x => `<div class="it" data-mob="${x.MonsterId}"><span class="${+x.IsDisabled ? 'undone' : ''}">${esc(x.Name || '#' + x.MonsterId)}</span><small>${+x.MapId ? 'cette map' : 'sous-zone'} · fréq. ${x.Frequency}</small></div>`).join('') : '<div class="muted">Aucun.</div>')
        + card(`PNJ (${MAP.npcs.length}) · interactifs (${MAP.interactives.length})`, MAP.npcs.map(n => `<div class="it" data-npc="${n.NpcId}"><span>${esc(n.Name)}</span><small>case ${n.CellId}</small></div>`).join('')
            + MAP.interactives.filter(i => i.CellId !== null).map(i => `<div class="it" data-cell="${i.CellId}"><span>${esc(i.Name)}${interTpText(i)}</span><small>case ${i.CellId}</small></div>`).join(''));
    $$('#mapSide [data-open]').forEach(b => b.onclick = e => { e.preventDefault(); openMap(+b.dataset.open); });
    $$('#mapSide [data-cell]').forEach(b => b.onclick = () => { SELCELL = +b.dataset.cell; drawMap(); cellInfo(SELCELL); });
    $$('#mapSide [data-mob]').forEach(b => b.onclick = () => { showTab('mobs'); setTimeout(() => mobDetail(+b.dataset.mob), 300); });
    $$('#mapSide [data-npc]').forEach(b => b.onclick = () => { showTab('npcs'); setTimeout(() => openNpc(+b.dataset.npc), 300); });
    $$('#mapSide [data-tdel]').forEach(a => a.onclick = async e => {
        e.preventDefault(); e.stopPropagation();
        if (confirm('Supprimer cette téléportation ?') && await act(api('trigger_delete', {}, { id: +a.dataset.tdel }), 'Téléportation supprimée')) openMap(m.Id);
    });
    $('#mapTp').onclick = () => cmd('go ' + m.Id + ' moi').then(refreshState);
    $$('#mapSide [data-pick]').forEach(b => b.onclick = () => pickCell(+valOf('nb' + b.dataset.pick), c => { $('#nbc' + b.dataset.pick).value = c; }));
    $('#propsSave').onclick = async () => {
        const body = { id: m.Id, SpawnDisabled: $('#spawnDis').checked ? 1 : 0 };
        ['Top', 'Bottom', 'Left', 'Right'].forEach(d => { body[d + 'NeighbourId'] = +valOf('nb' + d) || -1; body[d + 'NeighbourCellId'] = valOf('nbc' + d); });
        if (await act(api('map_props_save', {}, body), 'Map enregistrée')) openMap(m.Id);
    };
}

function renderMapMain() {
    const live = MAP.live;
    const modes = [['inspect', '🔍 Inspecter'], ['blue', '🔵 Placement bleu'], ['red', '🔴 Placement rouge'], ['erase', '⌫ Effacer placement'], ['tp', '✨ Case téléport'], ['edge', '↔ Passage de bord'], ['cell', '🧱 Cases']];
    $('#mapMain').innerHTML = card(`Grille de la map ${MAP.map.Id}`, `
        <div class="btns">${modes.map(([k, l]) => `<button data-mode="${k}" class="${MAPMODE === k ? 'pri' : ''}">${l}</button>`).join('')}
          <button id="placeSave" ${PLACES.dirty ? '' : 'disabled'}>💾 Enregistrer les placements</button>
          <button id="cellSave" ${Object.keys(CELLEDIT).length ? '' : 'disabled'}>💾 Enregistrer les cases</button>
          <button id="mapRefresh">⟳</button>
          <button id="mapVis">${MAPVIS ? '🖼 Visuel : oui' : '🖼 Visuel : non'}</button><button id="mapRedo" title="Redessiner l'image de la carte">↻ image</button>
          <span id="mapImgState" class="muted"></span></div>
        <div class="help">${{ edge: 'Clic sur une case du bord : ajoute / retire le passage vers la map voisine. Les directions se règlent aussi sous la grille.',
            cell: 'Clic sur une case : praticable ↔ bloquée. Les autres réglages (vue, combat) sont sous la grille.',
            tp: 'Clic sur une case : le joueur qui marche dessus est téléporté (comme un soleil). Choisis la map et la case d’arrivée sous la grille.' }[MAPMODE] || ''}</div>
        ${MAPMODE === 'edge' || MAPMODE === 'cell' ? `<div class="help" style="color:var(--acc2)">Case praticable / bloquée : effet <b>immédiat en jeu</b> après « Enregistrer les cases » (le serveur l'envoie au client comme obstacle, sans relancer).
            Passages de bord et ligne de vue : le client a sa propre copie des cases → lance <b>Mettre-a-jour-cases-client.bat</b> sur le PC (clients fermés), puis les fichiers de maps partent aux joueurs avec le launcher.</div>` : ''}
        <div class="help">${live ? (live.keepsRecordPlacements
            ? 'Cette map garde les placements enregistrés (donjon / arène) : ce que tu modifies ici sert directement en combat.'
            : 'Hors donjon, le serveur tire un modèle de placement au hasard à chaque combat : les placements enregistrés ne servent que si aucun modèle ne rentre.') : 'Serveur de jeu injoignable : contenu en direct indisponible.'}</div>
        <div style="overflow:auto;margin-top:8px"><canvas id="mapCv" width="${14 * CW + CW}" height="${40 * CH / 2 + CH}"></canvas></div>
        <div class="btns" style="margin-top:6px;font-size:12px">
          <span><span class="lg" style="background:#5b4a3a"></span>marchable</span><span><span class="lg" style="background:#1d1611"></span>bloquée</span>
          <span><span class="lg" style="background:#000"></span>bloque la vue</span><span><span class="lg" style="background:#6b4f20"></span>interdite en combat</span>
          <span><span class="lg" style="background:#3a7bd5"></span>placement bleu</span><span><span class="lg" style="background:#d54a3a"></span>placement rouge</span>
          <span><span class="lg" style="background:#ffd34d"></span>téléportation</span><span><span class="lg" style="background:#ff7ad9"></span>PNJ</span>
          <span><span class="lg" style="background:#a259ff"></span>interactif</span><span><span class="lg" style="background:#fff"></span>joueur</span><span><span class="lg" style="background:#ff4040"></span>groupe de monstres</span></div>
        <div id="cellBox" style="margin-top:8px"></div>`)
        + (live ? card('En direct', `<div class="muted">${live.players.length} joueur(s) : ${live.players.map(x => esc(x.name)).join(', ') || '—'}</div>
            <div class="muted">${live.groups.length} groupe(s) : ${live.groups.map(g => g.monsters.map(m => esc(m.name) + ' ' + m.level).join(', ') + ' (case ' + g.cell + ')').join(' · ') || '—'}</div>`) : '');
    $$('#mapMain [data-mode]').forEach(b => b.onclick = () => { MAPMODE = b.dataset.mode; renderMapMain(); });
    $('#mapRefresh').onclick = () => openMap(MAP.map.Id);
    $('#mapVis').onclick = () => { MAPVIS = !MAPVIS; renderMapMain(); };
    $('#mapRedo').onclick = async () => { if (await act(api('map_image_redo', {}, { id: MAP.map.Id }), 'Image redemandée')) loadMapImage(MAP.map.Id, { status: 'demande' }); };
    mapImgState();
    $('#cellSave').onclick = async () => {
        const cells = Object.entries(CELLEDIT).map(([c, d]) => ({ c: +c, los: d[0], mc: d[1] }));
        if (await act(api('map_cells_save', {}, { id: MAP.map.Id, cells }), cells.length + ' case(s) enregistrée(s) côté serveur'))
            openMap(MAP.map.Id).then(() => toast('Praticable / bloquée : déjà actif en jeu. Passages de bord : lance Mettre-a-jour-cases-client.bat (clients fermés).'));
    };
    $('#placeSave').onclick = async () => {
        if (await act(api('map_places_save', {}, { id: MAP.map.Id, blue: PLACES.blue, red: PLACES.red }), 'Placements enregistrés')) openMap(MAP.map.Id);
    };
    const cv = $('#mapCv');
    cv.onclick = e => {
        const r = cv.getBoundingClientRect();
        const id = cellAt(e.clientX - r.left, e.clientY - r.top);
        if (id < 0) return;
        SELCELL = id;
        if (MAPMODE === 'blue' || MAPMODE === 'red' || MAPMODE === 'erase') {
            PLACES.blue = PLACES.blue.filter(c => c !== id);
            PLACES.red = PLACES.red.filter(c => c !== id);
            if (MAPMODE !== 'erase') PLACES[MAPMODE].push(id);
            PLACES.dirty = true;
            $('#placeSave').disabled = false;
        }
        if (MAPMODE === 'edge' || MAPMODE === 'cell') {
            const d = [...cellData(id)];
            if (MAPMODE === 'edge') {
                const bit = edgeOf(id);
                if (d[1]) d[1] = 0;                        // retire tous les passages de la case
                else if (bit) d[1] = bit;
                else toast('Case hors du bord : coche la direction voulue sous la grille.');
            } else d[0] ^= 1;                            // praticable <-> bloquée
            return setCell(id, d);
        }
        drawMap();
        cellInfo(id);
    };
    drawMap();
    if (SELCELL >= 0) cellInfo(SELCELL);
}

function cellAt(px, py) {
    let best = -1, bd = 1e9;
    for (let id = 0; id < 560; id++) {
        const [x, y] = cellXY(id);
        const d = Math.abs(px - x) / (CW / 2) + Math.abs(py - y) / (CH / 2);
        if (d < bd) { bd = d; best = id; }
    }
    return bd <= 1.05 ? best : -1;
}

function drawMap() {
    const cv = $('#mapCv');
    if (!cv) return;
    const g = cv.getContext('2d');
    g.clearRect(0, 0, cv.width, cv.height);
    if (MAPVIS && MAPIMG) g.drawImage(MAPIMG, 0, 0);
    const trig = new Set(MAP.triggers.map(t => +t.CellId));
    const npc = new Set(MAP.npcs.map(n => +n.CellId));
    const inter = new Set(MAP.interactives.filter(i => i.CellId !== null).map(i => +i.CellId));
    const live = MAP.live || { players: [], groups: [] };
    const blue = new Set(PLACES.blue), red = new Set(PLACES.red);
    MAP.cells.forEach((c0, id) => {
        const c = cellData(id), f = flags(c);
        const [x, y] = cellXY(id);
        const vis = MAPVIS && MAPIMG;
        let col = !f.walk ? (f.los ? '#1d1611' : '#000') : f.noFight ? '#6b4f20' : '#5b4a3a';
        if (vis) col = !f.walk ? 'rgba(0,0,0,.28)' : f.noFight ? 'rgba(107,79,32,.35)' : null;
        if (blue.has(id)) col = vis ? 'rgba(58,123,213,.6)' : '#3a7bd5';
        if (red.has(id)) col = vis ? 'rgba(213,74,58,.6)' : '#d54a3a';
        g.beginPath();
        g.moveTo(x, y - CH / 2); g.lineTo(x + CW / 2, y); g.lineTo(x, y + CH / 2); g.lineTo(x - CW / 2, y); g.closePath();
        if (col) { g.fillStyle = col; g.fill(); }
        g.strokeStyle = id === SELCELL ? '#ffb547' : vis ? 'rgba(255,255,255,.12)' : '#2a2019'; g.lineWidth = id === SELCELL ? 2.5 : 1; g.stroke();
        if (f.change) {
            g.fillStyle = 'rgba(80,220,255,.28)'; g.fill();
            for (const bit in EDGE) if (f.change & bit) {           // flèche vers la map voisine
                const [, ax, ay] = EDGE[bit], tx = x + ax * 13, ty = y + ay * 7;
                g.beginPath(); g.moveTo(tx, ty); g.lineTo(x + ax * 5 - ay * 5, y + ay * 2 - ax * 4); g.lineTo(x + ax * 5 + ay * 5, y + ay * 2 + ax * 4); g.closePath();
                g.fillStyle = '#50dcff'; g.fill();
            }
        }
        if (CELLEDIT[id]) { g.beginPath(); g.moveTo(x, y - CH / 2); g.lineTo(x + CW / 2, y); g.lineTo(x, y + CH / 2); g.lineTo(x - CW / 2, y); g.closePath(); g.strokeStyle = '#ff8a3d'; g.lineWidth = 2; g.stroke(); }
        const dot = (color, dx = 0) => { g.beginPath(); g.arc(x + dx, y, 4.5, 0, 7); g.fillStyle = color; g.fill(); };
        if (trig.has(id)) dot('#ffd34d', -10);
        if (npc.has(id)) dot('#ff7ad9');
        if (inter.has(id)) dot('#a259ff', 10);
    });
    live.players.forEach(p => { if (p.cell === null) return; const [x, y] = cellXY(p.cell); g.beginPath(); g.arc(x, y, 5.5, 0, 7); g.fillStyle = '#fff'; g.fill(); });
    live.groups.forEach(gr => { if (gr.cell === null) return; const [x, y] = cellXY(gr.cell); g.beginPath(); g.arc(x, y, 6, 0, 7); g.fillStyle = '#ff4040'; g.fill(); });
}

function cellInfo(id) {
    const cd = cellData(id), f = flags(cd);
    const t = MAP.triggers.filter(x => +x.CellId === id);
    const n = MAP.npcs.filter(x => +x.CellId === id);
    const it = MAP.interactives.filter(x => +x.CellId === id);
    const dirs = [];
    if (f.change & 1) dirs.push('droite'); if (f.change & 16) dirs.push('gauche'); if (f.change & 64) dirs.push('haut'); if (f.change & 4) dirs.push('bas');
    const tr = t[0];
    $('#cellBox').innerHTML = `<div class="form" style="border-style:solid">
        <b>Case ${id}</b> · ${f.walk ? 'marchable' : 'bloquée'} · ${f.los ? 'ne bloque pas la vue' : 'bloque la vue'}${f.noFight ? ' · interdite en combat' : ''}${f.farm ? ' · enclos' : ''}
        ${dirs.length ? ' · changement de map vers ' + dirs.join(', ') : ''}
        <div class="btns" style="margin-top:6px">
          ${[[64, '⬆ passage haut'], [4, '⬇ passage bas'], [16, '⬅ passage gauche'], [1, '➡ passage droite']].map(([b, l]) => `<label class="row" style="gap:4px"><input type="checkbox" data-mc="${b}" ${cd[1] & b ? 'checked' : ''}>${l}</label>`).join('')}</div>
        <div class="btns">
          <label class="row" style="gap:4px"><input type="checkbox" data-los="1" data-inv="1" ${f.walk ? 'checked' : ''}>praticable</label>
          <label class="row" style="gap:4px"><input type="checkbox" data-los="8" ${!f.los ? 'checked' : ''}>bloque la vue</label>
          <label class="row" style="gap:4px"><input type="checkbox" data-los="2" ${f.noFight ? 'checked' : ''}>interdite en combat</label>
          <label class="row" style="gap:4px"><input type="checkbox" data-los="4" ${f.noRp ? 'checked' : ''}>interdite hors combat</label></div>
        ${n.map(x => `<div>PNJ : ${esc(x.Name)}</div>`).join('')}${it.map(interTpForm).join('')}
        ${MAPMODE === 'tp' || tr ? `<div class="grid" style="margin-top:8px">
            <label>Vers la map<input id="tpDest" type="number" value="${tr ? esc(tr.Parameter1) : ''}"></label>
            <label>Case d'arrivée<div class="row"><input id="tpCell" type="number" value="${tr ? esc(tr.Parameter0) : ''}"><button class="small" id="tpPick" title="Choisir sur la map d'arrivée">🎯</button></div></label>
            <label>Condition (facultatif)<input id="tpCond" value="${tr ? esc(tr.Condition || '') : ''}"></label>
            <button id="tpSave" class="pri">${tr ? '💾 Modifier' : '+ Ajouter la téléportation'}</button>
            ${tr ? `<button id="tpOpen">Ouvrir la map d'arrivée</button><button id="tpDel" class="danger">Supprimer</button>` : ''}</div>` : ''}
      </div>`;
    bindInterTp();
    if ($('#tpSave')) $('#tpSave').onclick = async () => {
        const body = { mapId: MAP.map.Id, id: tr ? +tr.Id : 0, cellId: id, destMap: +valOf('tpDest'), destCell: +valOf('tpCell'), condition: valOf('tpCond') };
        if (await act(api('trigger_save', {}, body), 'Téléportation enregistrée')) openMap(MAP.map.Id);
    };
    if ($('#tpOpen')) $('#tpOpen').onclick = () => openMap(+tr.Parameter1);
    if ($('#tpDel')) $('#tpDel').onclick = async () => {
        if (confirm('Supprimer cette téléportation ?') && await act(api('trigger_delete', {}, { id: +tr.Id }), 'Téléportation supprimée')) openMap(MAP.map.Id);
    };
    if ($('#tpPick')) $('#tpPick').onclick = () => {
        if (!+valOf('tpDest')) return toast('Indique d’abord la map d’arrivée.', true);
        pickCell(+valOf('tpDest'), c => { $('#tpCell').value = c; });
    };
    $$('#cellBox [data-mc]').forEach(x => x.onchange = () => { const d = [...cellData(id)]; d[1] = x.checked ? d[1] | +x.dataset.mc : d[1] & ~+x.dataset.mc; setCell(id, d); });
    $$('#cellBox [data-los]').forEach(x => x.onchange = () => {
        const d = [...cellData(id)], bit = +x.dataset.los, on = x.dataset.inv ? !x.checked : x.checked;
        d[0] = on ? d[0] | bit : d[0] & ~bit;
        setCell(id, d);
    });
}

/** Modification d'une case en attente d'enregistrement (retirée si elle revient à l'original). */
function setCell(id, d) {
    const o = MAP.cells[id];
    if (o && o[0] === d[0] && o[1] === d[1]) delete CELLEDIT[id]; else CELLEDIT[id] = d;
    if ($('#cellSave')) $('#cellSave').disabled = !Object.keys(CELLEDIT).length;
    SELCELL = id;
    drawMap();
    cellInfo(id);
}

/** Fenêtre : grille d'une autre map pour cliquer la case d'arrivée. */
async function pickCell(mapId, done) {
    if (!mapId) return toast('Map d’arrivée inconnue.', true);
    let other;
    try { other = await api('map', { id: mapId }); } catch (e) { return toast(e.message, true); }
    const ov = document.createElement('div');
    ov.className = 'pickov';
    ov.innerHTML = `<div class="card" style="max-width:96vw;max-height:94vh;overflow:auto">
        <div class="row" style="justify-content:space-between"><b>Map ${mapId} : clique la case d'arrivée</b><span id="pkInfo" class="muted"></span><button id="pkClose">✕</button></div>
        <canvas id="pkCv" width="${14 * CW + CW}" height="${40 * CH / 2 + CH}" style="margin-top:6px;cursor:crosshair"></canvas></div>`;
    document.body.appendChild(ov);
    const cv = $('#pkCv'), g = cv.getContext('2d');
    let img = null, hover = -1;
    const draw = () => {
        g.clearRect(0, 0, cv.width, cv.height);
        if (img) g.drawImage(img, 0, 0);
        other.cells.forEach((c, id) => {
            const f = flags(c), [x, y] = cellXY(id);
            g.beginPath(); g.moveTo(x, y - CH / 2); g.lineTo(x + CW / 2, y); g.lineTo(x, y + CH / 2); g.lineTo(x - CW / 2, y); g.closePath();
            const col = id === hover ? (f.walk ? 'rgba(123,216,143,.8)' : 'rgba(255,107,107,.8)') : !f.walk ? (img ? 'rgba(0,0,0,.35)' : '#1d1611') : (img ? null : '#5b4a3a');
            if (col) { g.fillStyle = col; g.fill(); }
            g.strokeStyle = img ? 'rgba(255,255,255,.12)' : '#2a2019'; g.lineWidth = 1; g.stroke();
        });
    };
    if (other.image && other.image.url) { const im = new Image(); im.onload = () => { img = im; draw(); }; im.src = other.image.url; }
    draw();
    const close = () => ov.remove();
    $('#pkClose').onclick = close;
    ov.onclick = e => { if (e.target === ov) close(); };
    cv.onmousemove = e => {
        const r = cv.getBoundingClientRect(), id = cellAt(e.clientX - r.left, e.clientY - r.top);
        if (id !== hover) { hover = id; draw(); $('#pkInfo').textContent = id >= 0 ? 'case ' + id + (flags(other.cells[id] || [1]).walk ? '' : ' (bloquée)') : ''; }
    };
    cv.onclick = e => {
        const r = cv.getBoundingClientRect(), id = cellAt(e.clientX - r.left, e.clientY - r.top);
        if (id < 0) return;
        if (!flags(other.cells[id] || [1]).walk && !confirm('Cette case est bloquée : le joueur y serait coincé. La garder quand même ?')) return;
        done(id);
        close();
    };
}

/** Image de la carte (service de rendu sur le PC qui a le client) : chargée, ou attendue si elle est en cours de rendu. */
function loadMapImage(id, info) {
    clearTimeout(MAPIMG_TIMER);
    MAPIMG = null;
    const wait = (tries) => {
        MAPIMG_TIMER = setTimeout(async () => {
            if (!MAP || MAP.map.Id !== id) return;
            try {
                const r = await api('map_image', { id });
                if (r.url) return loadMapImage(id, r);
                MAP.image = r;
                mapImgState();
                if (r.status === 'demande' && tries < 40) wait(tries + 1);
            } catch { }
        }, 2500);
    };
    if (info && info.url) {
        const img = new Image();
        img.onload = () => { if (MAP && MAP.map.Id === id) { MAPIMG = img; MAP.image = info; mapImgState(); drawMap(); } };
        img.src = info.url;
    } else if (info && info.status === 'demande') {
        if (MAP) MAP.image = info;
        wait(0);
    }
    mapImgState();
}

function mapImgState() {
    const el = $('#mapImgState');
    if (!el || !MAP) return;
    const st = MAP.image ? MAP.image.status : null;
    el.textContent = MAPIMG ? '' : st === 'demande' ? 'image en cours de rendu… (service de rendu lancé sur le PC ?)' : st === 'absent' ? 'carte absente du client : pas d’image' : '';
}

TABS.maps = tabMaps;


// NEKFEU : téléportations des interactifs (portes, escaliers, portails...) dans l'onglet Cartes
function interTpText(i) {
    const tps = (i.Skills || []).filter(k => k.Type === 'Teleport');
    return tps.length ? ' → ' + tps.map(k => `<b>${esc(k.Parameter0 || '')}</b>`).join(', ') : '';
}

function interTpForm(i) {
    const tps = (i.Skills || []).filter(k => k.Type === 'Teleport');
    const others = (i.Skills || []).filter(k => k.Type !== 'Teleport');
    const row = (k, n) => `<div class="grid" style="margin-top:6px" data-itp="${i.Id}" data-link="${k ? k.LinkId : 0}">
        <label>Vers la map<input data-f="dest" type="number" value="${k ? esc(k.Parameter0 || '') : ''}"></label>
        <label>Case d'arrivée<div class="row"><input data-f="cell" type="number" value="${k ? esc(k.Parameter1 || '') : ''}" placeholder="-1 = centre"><button class="small" data-f="pick" title="Choisir sur la map d'arrivée">🎯</button></div></label>
        <label>Condition (facultatif)<input data-f="cond" value="${k ? esc(k.Condition || '') : ''}"></label>
        <button class="pri" data-f="save">${k ? '💾 Modifier' : '+ Ajouter une destination'}</button>
        ${k ? `<button data-f="open">Ouvrir la map d'arrivée</button><button class="danger" data-f="del">Supprimer</button>` : ''}
        ${k && +k.Shared > 1 ? `<div class="help" style="grid-column:1/-1">Action partagée avec ${+k.Shared - 1} autre(s) interactif(s) : en modifiant, cet interactif reçoit sa propre copie (les autres ne changent pas).</div>` : ''}</div>`;
    return `<div class="form" style="margin-top:8px;border-color:#a259ff"><b style="color:#a259ff">Interactif : ${esc(i.Name)}</b> <small class="muted">#${i.Id}${others.length ? ' · autres actions : ' + others.map(k => esc(k.Type)).join(', ') : ''}</small>
        ${tps.map(row).join('')}${row(null)}
        <div class="help">${tps.length > 1 ? 'Plusieurs destinations : le joueur choisit dans le menu de l’interactif.' : 'Le joueur qui utilise l’interactif est téléporté. Case -1 = centre de la map d’arrivée ; le serveur arrive toujours sur une case libre à côté.'}</div></div>`;
}

function bindInterTp() {
    $$('#cellBox [data-itp]').forEach(box => {
        const f = n => box.querySelector(`[data-f="${n}"]`);
        const spawnId = +box.dataset.itp, linkId = +box.dataset.link;
        f('save').onclick = async () => {
            if (!+f('dest').value) return toast('Indique la map d’arrivée.', true);
            const body = { spawnId, linkId, destMap: +f('dest').value, destCell: f('cell').value === '' ? -1 : +f('cell').value, condition: f('cond').value };
            if (await act(api('inter_tp_save', {}, body), 'Téléportation de l’interactif enregistrée')) openMap(MAP.map.Id);
        };
        f('pick').onclick = () => {
            if (!+f('dest').value) return toast('Indique d’abord la map d’arrivée.', true);
            pickCell(+f('dest').value, c => { f('cell').value = c; });
        };
        if (f('open')) f('open').onclick = () => openMap(+f('dest').value);
        if (f('del')) f('del').onclick = async () => {
            if (confirm('Supprimer cette destination de l’interactif ?') && await act(api('inter_tp_delete', {}, { linkId }), 'Destination supprimée')) openMap(MAP.map.Id);
        };
    });
}
