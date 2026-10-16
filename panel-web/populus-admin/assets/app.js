/* Panel admin Populus 2.51 : PNJ (liste, placements, apparence, actions, dialogues, boutiques). */
'use strict';

const $ = (sel, root = document) => root.querySelector(sel);
const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
const cut = (s, n = 90) => { s = String(s ?? ''); return s.length > n ? s.slice(0, n - 1) + '…' : s; };

const DIRS = ['Est', 'Sud-Est', 'Sud', 'Sud-Ouest', 'Ouest', 'Nord-Ouest', 'Nord', 'Nord-Est'];

/* Paramètres de chaque type d'action (clic sur le PNJ). kind : message | item | int | text | bool | map */
const ACTION_TYPES = {
    Talk: { label: 'Dialogue', params: [{ p: 0, label: 'Question de départ', kind: 'message' }] },
    Shop: { label: 'Boutique', params: [{ p: 0, label: 'Monnaie (objet, vide = kamas)', kind: 'item' }, { p: 1, label: 'Le joueur peut vendre', kind: 'bool' }, { p: 2, label: 'Objets aux stats max', kind: 'bool' }] },
    BidBuy: { label: 'Hôtel de vente (achat)', params: [{ p: 0, label: 'Types d\'objets (ex. 2|3|4)', kind: 'text' }, { p: 1, label: 'Niveau max', kind: 'int' }] },
    BidSell: { label: 'Hôtel de vente (vente)', params: [{ p: 0, label: 'Types d\'objets (ex. 2|3|4)', kind: 'text' }, { p: 1, label: 'Niveau max', kind: 'int' }] },
    Exchange: { label: 'Échange d\'objets', params: [{ p: 0, label: 'Objets demandés (id,qté;…)', kind: 'text' }, { p: 1, label: 'Objets donnés (id,qté;…)', kind: 'text' }] },
};

/* Effet de chaque type de réponse de dialogue */
const REPLY_TYPES = {
    Dialog: { label: 'Aller à une autre question', params: [{ p: 0, label: 'Question suivante', kind: 'message' }] },
    EndDialog: { label: 'Fermer le dialogue', params: [] },
    Teleport: { label: 'Téléporter', params: [{ p: 0, label: 'Map', kind: 'map' }, { p: 1, label: 'Case (0-559)', kind: 'int' }, { p: 2, label: 'Direction (0-7)', kind: 'int' }, { p: 3, label: 'Objets à payer (id,qté)', kind: 'text' }, { p: 4, label: 'Kamas à payer', kind: 'int' }] },
    DungeonEnter: { label: 'Entrer dans le donjon', params: [], help: 'Le donjon est trouvé d\'après la map du PNJ (table dungeons_entries) ; la clé est consommée.' },
    DungeonExit: { label: 'Sortir du donjon', params: [], help: 'La sortie est trouvée d\'après la salle de fin (table dungeons_exits).' },
    AddItem: { label: 'Donner un objet', params: [{ p: 0, label: 'Objet', kind: 'item' }, { p: 1, label: 'Quantité', kind: 'int' }] },
    DeleteItem: { label: 'Prendre un objet', params: [{ p: 0, label: 'Objet', kind: 'item' }, { p: 1, label: 'Quantité', kind: 'int' }] },
    AddKamas: { label: 'Donner des kamas', params: [{ p: 0, label: 'Montant', kind: 'int' }, { p: 1, label: 'Minimum', kind: 'int' }] },
    DeleteKamas: { label: 'Prendre des kamas', params: [{ p: 0, label: 'Montant', kind: 'int' }] },
    Quest: { label: 'Lancer une quête', params: [{ p: 0, label: 'Étape de quête (StepId)', kind: 'int' }] },
    Objective: { label: 'Valider un objectif de quête', params: [{ p: 0, label: 'Objectif', kind: 'int' }] },
    StartFight: { label: 'Lancer un combat', params: [{ p: 0, label: 'Monstres (id,grade|id,grade)', kind: 'text' }] },
    Bank: { label: 'Ouvrir la banque', params: [] },
    GiveTitle: { label: 'Donner un titre', params: [{ p: 0, label: 'Titre', kind: 'int' }] },
    GiveOrnament: { label: 'Donner un ornement', params: [{ p: 0, label: 'Ornement', kind: 'int' }] },
    AddSpell: { label: 'Apprendre un sort', params: [{ p: 0, label: 'Sort', kind: 'int' }] },
    Cinematic: { label: 'Jouer une cinématique', params: [{ p: 0, label: 'Cinématique', kind: 'int' }] },
    Restat: { label: 'Restat des caractéristiques', params: [{ p: 1, label: 'Objets à payer', kind: 'text' }, { p: 2, label: 'Kamas à payer', kind: 'int' }] },
    SpellRestat: { label: 'Restat des sorts', params: [] },
    Alignment: { label: 'Choisir un alignement', params: [{ p: 0, label: 'Camp (1 Bonta, 2 Brâkmar)', kind: 'int' }, { p: 1, label: 'Objets à payer', kind: 'text' }, { p: 2, label: 'Kamas à payer', kind: 'int' }] },
    GuildCreation: { label: 'Créer une guilde', params: [] },
};
const GENERIC = [0, 1, 2, 3, 4].map(p => ({ p, label: 'Paramètre ' + p, kind: 'text' }));

const state = { list: [], npc: null, tab: 'general', editing: null };

/* ------------------------------------------------------------------ outils */
async function api(a, params = {}, body = null) {
    const url = 'api.php?a=' + encodeURIComponent(a) + Object.entries(params).map(([k, v]) => '&' + k + '=' + encodeURIComponent(v)).join('');
    const opt = body ? { method: 'POST', headers: { 'Content-Type': 'application/json', 'X-CSRF': window.CSRF }, body: JSON.stringify(body) } : {};
    const res = await fetch(url, opt);
    let data;
    try { data = await res.json(); } catch { throw new Error('Réponse invalide du panel (' + res.status + ')'); }
    if (res.status === 401) { document.body.innerHTML = '<div class="gate"><h1>Session expirée</h1><p>Tape <code>.admin</code> en jeu pour rouvrir le panel.</p></div>'; throw new Error(data.error); }
    if (data.error) throw new Error(data.error);
    return data;
}

function toast(msg, err = false) {
    const d = document.createElement('div');
    if (err) d.className = 'err';
    d.textContent = msg;
    $('#toast').appendChild(d);
    setTimeout(() => d.remove(), err ? 7000 : 3500);
}

function status(text, cls = '') {
    const s = $('#status');
    s.textContent = text;
    s.className = 'status ' + cls;
}

/** Affiche le résultat du rechargement en jeu renvoyé par une modification. */
function reloadStatus(r) {
    if (!r) return;
    if (r.ok) status('✓ Appliqué en jeu (' + new Date().toLocaleTimeString() + ')', 'ok');
    else if (r.timeout) status('Enregistré. Le serveur de jeu ne répond pas : ce sera appliqué à son prochain démarrage.', 'err');
    else status('Enregistré, mais le rechargement a échoué : ' + (r.result || '?'), 'err');
}

async function act(promise, okMsg) {
    try {
        const r = await promise;
        if (okMsg) toast(okMsg);
        if (r && r.reload) reloadStatus(r.reload);
        return r;
    } catch (e) {
        toast(e.message, true);
        return null;
    }
}

/* ------------------------------------------------------------------ liste */
let searchTimer;
async function loadList() {
    const q = $('#search').value.trim();
    const filter = document.querySelector('input[name=filter]:checked').value;
    try {
        const r = await api('npcs', { q, filter });
        state.list = r.npcs;
        renderList();
    } catch (e) { toast(e.message, true); }
}

function renderList() {
    if (!$('#npcList')) return;
    const cur = state.npc ? state.npc.id : 0;
    $('#npcList').innerHTML = state.list.length ? state.list.map(n => `
        <div class="it ${+n.Id === cur ? 'on' : ''}" data-id="${n.Id}">
          <div>${esc(n.Name || '(sans nom)')}<div class="tags">${esc((n.Actions || '').replace(/,/g, ' · '))}</div></div>
          <small>#${n.Id}${+n.Spawns ? ' · ' + n.Spawns + ' pl.' : ''}</small>
        </div>`).join('') : '<div class="muted">Aucun PNJ.</div>';
}

/* ------------------------------------------------------------------ fiche */
async function openNpc(id, keepTab = false) {
    try {
        state.npc = await api('npc', { id });
        state.editing = null;
        if (!keepTab) state.tab = 'general';
        renderList();
        renderNpc();
    } catch (e) { toast(e.message, true); }
}

function renderNpc() {
    if (!$('#detail')) return;
    const n = state.npc;
    const tabs = [['general', 'Apparence'], ['spawns', 'Placements (' + n.spawns.length + ')'], ['actions', 'Actions (' + n.actions.length + ')'], ['dialog', 'Dialogues (' + n.dialog.length + ' rép.)']];
    $('#detail').innerHTML = `
      <div class="npc-head"><h2>${esc(n.name)}</h2><span class="muted">PNJ #${n.id}</span></div>
      <div class="tabs">${tabs.map(([k, l]) => `<button data-tab="${k}" class="${state.tab === k ? 'on' : ''}">${l}</button>`).join('')}</div>
      <div id="tab"></div>`;
    ({ general: renderGeneral, spawns: renderSpawns, actions: renderActions, dialog: renderDialog })[state.tab]();
}

/* ---------- apparence */
function renderGeneral() {
    const n = state.npc;
    $('#tab').innerHTML = `
      <div class="card">
        <h3>Apparence</h3>
        <label class="field">Code d'apparence (look)<textarea id="look">${esc(n.look)}</textarea></label>
        <div class="help">Format <code>{bones|skins|couleurs|taille}</code>, par ex. <code>{1|1102||140}</code>.
          « Aperçu en jeu » fait apparaître ce look à côté de ton personnage pendant une minute, sans rien enregistrer.</div>
        <div class="row" style="margin-top:10px">
          <button id="lookPreview">Aperçu en jeu</button>
          <button id="lookSave" class="pri">Enregistrer</button>
        </div>
      </div>
      <div class="card">
        <h3>Résumé</h3>
        <div class="muted">${n.spawns.length} placement(s) · ${n.actions.length} action(s) · ${n.messages.length} question(s) et ${n.replies.length} texte(s) de réponse disponibles pour ce PNJ.</div>
      </div>`;
    $('#lookPreview').onclick = async () => {
        const r = await act(api('preview_look', {}, { id: n.id, look: $('#look').value.trim() }));
        if (r) r.ok ? toast(r.result) : toast(r.timeout ? 'Le serveur de jeu ne répond pas.' : r.result, true);
    };
    $('#lookSave').onclick = async () => {
        if (await act(api('look_save', {}, { id: n.id, look: $('#look').value.trim() }), 'Apparence enregistrée')) openNpc(n.id, true);
    };
}

/* ---------- placements */
function renderSpawns() {
    const n = state.npc;
    const rows = n.spawns.map(s => {
        const m = s.map || {};
        return `<tr data-id="${s.Id}">
          <td><b>${s.MapId}</b>${m.exists === false ? ' <span class="badge" style="color:var(--err)">map absente</span>' : ''}<div class="muted">${m.x !== null && m.x !== undefined ? '[' + m.x + ',' + m.y + '] ' : ''}${esc(m.zone || '')}</div></td>
          <td>${s.CellId}</td><td>${DIRS[s.Direction] || s.Direction}</td>
          <td class="act"><button class="small" data-go="${s.Id}">Y aller</button> <button class="small" data-edit="${s.Id}">Modifier</button> <button class="small danger" data-del="${s.Id}">Retirer</button></td></tr>`;
    }).join('');
    $('#tab').innerHTML = `
      <div class="card">
        <h3>Placements <span class="row"><button id="spawnHere" class="pri">Placer sur ma position</button><button id="spawnNew">Ajouter à la main</button></span></h3>
        ${n.spawns.length ? `<table><tr><th>Map</th><th>Case</th><th>Direction</th><th></th></tr>${rows}</table>` : '<div class="muted">Ce PNJ n\'est placé nulle part.</div>'}
        <div id="spawnForm"></div>
        <div class="help">« Placer sur ma position » utilise la map, la case et l'orientation de ton personnage en jeu.</div>
      </div>`;
    $('#spawnHere').onclick = async () => {
        const p = await act(api('position'));
        if (!p) return;
        if (await act(api('spawn_save', {}, { npcId: n.id, mapId: p.mapId, cellId: p.cellId, direction: p.direction }), 'PNJ placé sur ta position')) openNpc(n.id, true);
    };
    $('#spawnNew').onclick = () => spawnForm(null);
    $('#tab').querySelectorAll('[data-edit]').forEach(b => b.onclick = () => spawnForm(n.spawns.find(s => s.Id == b.dataset.edit)));
    $('#tab').querySelectorAll('[data-go]').forEach(b => b.onclick = async () => {
        const s = n.spawns.find(x => x.Id == b.dataset.go);
        const r = await act(api('teleport', {}, { mapId: s.MapId, cellId: s.CellId }));
        if (r) r.ok ? toast('Téléporté') : toast(r.timeout ? 'Le serveur de jeu ne répond pas.' : r.result, true);
    });
    $('#tab').querySelectorAll('[data-del]').forEach(b => b.onclick = async () => {
        if (!confirm('Retirer ce placement ?')) return;
        if (await act(api('spawn_delete', {}, { id: +b.dataset.del }), 'Placement retiré')) openNpc(n.id, true);
    });
}

function spawnForm(s) {
    const n = state.npc;
    $('#spawnForm').innerHTML = `
      <div class="form">
        <div class="grid">
          <label>Map<input id="sfMap" type="number" value="${s ? s.MapId : ''}"></label>
          <label>Case (0-559)<input id="sfCell" type="number" min="0" max="559" value="${s ? s.CellId : ''}"></label>
          <label>Direction<select id="sfDir">${DIRS.map((d, i) => `<option value="${i}" ${(s ? +s.Direction : 1) === i ? 'selected' : ''}>${i} · ${d}</option>`).join('')}</select></label>
          <button id="sfPos">Prendre ma position</button>
        </div>
        <div id="sfInfo" class="help"></div>
        <div class="actions"><button id="sfCancel">Annuler</button><button id="sfSave" class="pri">Enregistrer</button></div>
      </div>`;
    const info = async () => {
        const m = +$('#sfMap').value;
        if (!m) return;
        try {
            const r = await api('map_info', { mapId: m });
            $('#sfInfo').textContent = r.map && r.map.exists ? `[${r.map.x},${r.map.y}] ${r.map.zone}` : 'Cette map n\'existe pas.';
        } catch { }
    };
    $('#sfMap').onchange = info;
    info();
    $('#sfPos').onclick = async () => {
        const p = await act(api('position'));
        if (!p) return;
        $('#sfMap').value = p.mapId; $('#sfCell').value = p.cellId; $('#sfDir').value = p.direction;
        info();
    };
    $('#sfCancel').onclick = () => $('#spawnForm').innerHTML = '';
    $('#sfSave').onclick = async () => {
        const body = { npcId: n.id, id: s ? s.Id : 0, mapId: +$('#sfMap').value, cellId: +$('#sfCell').value, direction: +$('#sfDir').value };
        if (await act(api('spawn_save', {}, body), 'Placement enregistré')) openNpc(n.id, true);
    };
}

/* ---------- champs de paramètres (actions et réponses) */
function messageOptions(selected) {
    const n = state.npc;
    const ids = new Set(n.messages.map(m => m.id));
    let opts = n.messages.map(m => `<option value="${m.id}" ${+selected === m.id ? 'selected' : ''}>#${m.id} — ${esc(cut(m.text || '(texte introuvable)', 110))}</option>`).join('');
    if (selected && !ids.has(+selected)) opts = `<option value="${selected}" selected>#${selected} — absent de la liste de ce PNJ (ne s'affichera pas)</option>` + opts;
    return '<option value="">— choisir —</option>' + opts;
}

function paramField(spec, value) {
    const id = 'p' + spec.p;
    const v = value ?? '';
    switch (spec.kind) {
        case 'message':
            return `<label style="grid-column:1/-1">${spec.label}<select id="${id}">${messageOptions(v)}</select></label>`;
        case 'bool':
            return `<label>${spec.label}<select id="${id}"><option value="0" ${v == 1 ? '' : 'selected'}>Non</option><option value="1" ${v == 1 ? 'selected' : ''}>Oui</option></select></label>`;
        case 'item':
            return `<label class="suggest">${spec.label}<input id="${id}" value="${esc(v)}" placeholder="numéro ou nom" data-item="1" autocomplete="off"><div class="drop" hidden></div></label>`;
        case 'map':
            return `<label>${spec.label}<input id="${id}" type="number" value="${esc(v)}"></label><button type="button" data-pos="${id}">Ma position</button>`;
        default:
            return `<label>${spec.label}<input id="${id}" ${spec.kind === 'int' ? 'type="number"' : ''} value="${esc(v)}"></label>`;
    }
}

/** Champ objet : recherche par nom, garde le numéro. */
function wireItemFields(root) {
    root.querySelectorAll('input[data-item]').forEach(inp => {
        const drop = inp.parentElement.querySelector('.drop');
        let t;
        inp.oninput = () => {
            clearTimeout(t);
            const v = inp.value.trim();
            if (!v || /^\d+$/.test(v)) { drop.hidden = true; return; }
            t = setTimeout(async () => {
                try {
                    const r = await api('item_search', { q: v });
                    drop.innerHTML = r.items.map(i => `<div data-id="${i.Id}">#${i.Id} ${esc(i.Name)} <span class="muted">niv. ${i.Level}</span></div>`).join('') || '<div class="muted">Aucun objet</div>';
                    drop.hidden = false;
                    drop.querySelectorAll('[data-id]').forEach(d => d.onclick = () => { inp.value = d.dataset.id; drop.hidden = true; });
                } catch { }
            }, 250);
        };
        inp.onblur = () => setTimeout(() => drop.hidden = true, 200);
    });
    root.querySelectorAll('[data-pos]').forEach(b => b.onclick = async () => {
        const p = await act(api('position'));
        if (!p) return;
        $('#' + b.dataset.pos, root).value = p.mapId;
        const cell = $('#p1', root);
        if (cell && b.dataset.pos === 'p0') cell.value = p.cellId;
        const dir = $('#p2', root);
        if (dir && b.dataset.pos === 'p0') dir.value = p.direction;
    });
}

function readParams(root) {
    const out = {};
    for (let i = 0; i <= 4; i++) {
        const el = $('#p' + i, root);
        if (el) out['p' + i] = el.value.trim();
    }
    return out;
}

function paramSummary(specs, row) {
    return specs.map(s => {
        const v = row['Parameter' + s.p];
        if (v === null || v === undefined || v === '') return '';
        let shown = v;
        if (s.kind === 'message') shown = '#' + v;
        if (s.kind === 'bool') shown = v == 1 ? 'oui' : 'non';
        return `${s.label} : <code>${esc(shown)}</code>`;
    }).filter(Boolean).join(' · ');
}

/* ---------- actions */
function renderActions() {
    const n = state.npc;
    const rows = n.actions.map(a => {
        const spec = ACTION_TYPES[a.Type];
        let extra = '';
        if (a.Type === 'Talk') {
            const m = n.messages.find(x => x.id == a.Parameter0);
            extra = `<div class="muted">« ${esc(cut(m ? m.text : 'message #' + a.Parameter0 + ' absent de ce PNJ', 140))} »</div>`;
        }
        if (a.Type === 'Shop') extra = shopBlock(a);
        return `<div class="card"><h3><span><span class="badge">${esc(spec ? spec.label : a.Type)}</span> action #${a.Id}</span>
            <span class="row"><button class="small" data-edit="${a.Id}">Modifier</button><button class="small danger" data-del="${a.Id}">Supprimer</button></span></h3>
            <div class="muted">${a.Condition ? 'Condition : <code>' + esc(a.Condition) + '</code> · ' : ''}Priorité ${a.Priority}${spec ? ' · ' + paramSummary(spec.params, a) : ''}</div>
            ${extra}<div id="af${a.Id}"></div></div>`;
    }).join('');
    $('#tab').innerHTML = `
      <div class="row" style="margin-bottom:12px"><button id="actNew" class="pri">+ Ajouter une action</button>
        <span class="muted">Ce qui se passe quand un joueur clique sur le PNJ.</span></div>
      <div id="afnew"></div>${rows || '<div class="muted">Aucune action : le PNJ ne réagit pas au clic.</div>'}`;
    $('#actNew').onclick = () => actionForm(null, '#afnew');
    $('#tab').querySelectorAll('[data-edit]').forEach(b => b.onclick = () => actionForm(n.actions.find(a => a.Id == b.dataset.edit), '#af' + b.dataset.edit));
    $('#tab').querySelectorAll('[data-del]').forEach(b => b.onclick = async () => {
        if (!confirm('Supprimer cette action ?')) return;
        if (await act(api('action_delete', {}, { id: +b.dataset.del }), 'Action supprimée')) openNpc(n.id, true);
    });
    wireShops();
}

function actionForm(a, target) {
    const n = state.npc;
    const type = a ? a.Type : 'Talk';
    const box = $(target);
    const draw = t => {
        const spec = ACTION_TYPES[t] || { params: GENERIC };
        box.innerHTML = `<div class="form">
          <div class="grid">
            <label>Type<select id="afType">${Object.entries(ACTION_TYPES).map(([k, s]) => `<option value="${k}" ${k === t ? 'selected' : ''}>${s.label}</option>`).join('')}</select></label>
            <label>Condition (facultatif)<input id="afCond" value="${esc(a ? a.Condition || '' : '')}" placeholder="ex. Mp=190056961"></label>
            <label>Priorité<input id="afPrio" type="number" value="${a ? a.Priority : 0}"></label>
            ${spec.params.map(s => paramField(s, a && a.Type === t ? a['Parameter' + s.p] : '')).join('')}
          </div>
          <div class="help">Conditions : <code>Mp=map</code> (sur cette map), <code>PL>50</code> (niveau), <code>Qa=quête</code> (quête en cours), <code>Qf=quête</code> (finie), <code>PO=objet</code> (possède) ; <code>&amp;</code> = et, <code>|</code> = ou.</div>
          <div class="actions"><button id="afCancel">Annuler</button><button id="afSave" class="pri">Enregistrer</button></div></div>`;
        $('#afType', box).onchange = e => draw(e.target.value);
        $('#afCancel', box).onclick = () => box.innerHTML = '';
        $('#afSave', box).onclick = async () => {
            const body = { npcId: n.id, id: a ? a.Id : 0, type: $('#afType', box).value, condition: $('#afCond', box).value, priority: +$('#afPrio', box).value || 0, ...readParams(box) };
            if (await act(api('action_save', {}, body), 'Action enregistrée')) openNpc(n.id, true);
        };
        wireItemFields(box);
    };
    draw(type);
}

function shopBlock(a) {
    const items = state.npc.shopItems[a.Id] || [];
    return `<table style="margin-top:8px"><tr><th>Objet</th><th>Niveau</th><th>Prix</th><th></th></tr>
      ${items.map(i => `<tr><td>#${i.ItemId} ${esc(i.Name)}</td><td>${i.Level ?? ''}</td><td>${i.CustomPrice ?? '<span class="muted">' + Math.round(i.Price || 0) + ' (normal)</span>'}</td>
        <td class="act"><button class="small danger" data-itemdel="${i.Id}">Retirer</button></td></tr>`).join('')}
      </table>
      <div class="grid" style="margin-top:8px" data-shop="${a.Id}">
        <label class="suggest">Ajouter un objet<input class="shopItem" id="p9_${a.Id}" data-item="1" placeholder="numéro ou nom" autocomplete="off"><div class="drop" hidden></div></label>
        <label>Prix (vide = prix normal)<input class="shopPrice" type="number"></label>
        <button class="shopAdd">Ajouter</button>
      </div>`;
}

function wireShops() {
    const n = state.npc;
    $('#tab').querySelectorAll('[data-shop]').forEach(g => {
        wireItemFields(g);
        $('.shopAdd', g).onclick = async () => {
            const body = { shopId: +g.dataset.shop, itemId: +$('.shopItem', g).value, price: $('.shopPrice', g).value };
            if (await act(api('shop_item_add', {}, body), 'Objet ajouté')) openNpc(n.id, true);
        };
    });
    $('#tab').querySelectorAll('[data-itemdel]').forEach(b => b.onclick = async () => {
        if (await act(api('shop_item_delete', {}, { id: +b.dataset.itemdel }), 'Objet retiré')) openNpc(n.id, true);
    });
}

/* ---------- dialogues */
function renderDialog() {
    const n = state.npc;
    const replyText = id => { const r = n.replies.find(x => x.id == id); return r ? r.text : null; };
    const starts = {};
    n.actions.filter(a => a.Type === 'Talk').forEach(a => (starts[a.Parameter0] = starts[a.Parameter0] || []).push(a));
    const targets = {};
    n.dialog.filter(r => r.Type === 'Dialog').forEach(r => targets[r.Parameter0] = true);

    const msgIds = n.messages.map(m => m.id);
    Object.keys(starts).forEach(id => { if (!msgIds.includes(+id)) msgIds.push(+id); });

    const blocks = msgIds.map(id => {
        const m = n.messages.find(x => x.id === id);
        const replies = n.dialog.filter(r => +r.MessageId === id);
        const used = starts[id] || targets[id] || replies.length;
        const startTags = (starts[id] || []).map(a => `<span class="badge ok">départ${a.Condition ? ' si ' + esc(a.Condition) : ''}</span>`).join(' ');
        return `<div class="msg ${used ? '' : 'unused'}">
          <div class="meta">Question #${id} ${startTags} ${targets[id] ? '<span class="badge blue">suite d\'une réponse</span>' : ''}
            ${m ? '' : '<span class="badge" style="color:var(--err)">absente de la liste du PNJ : ne s\'affiche pas</span>'}</div>
          <div class="txt">${esc(m ? m.text || '(texte introuvable)' : '')}</div>
          ${replies.map(r => {
              const spec = REPLY_TYPES[r.Type];
              const txt = replyText(r.ReplyId);
              return `<div class="reply"><div class="body">
                  <div class="rt">${txt ? '→ ' + esc(cut(txt, 160)) : '→ <span class="err">texte de réponse #' + r.ReplyId + ' inconnu de ce PNJ</span>'}</div>
                  <div class="rp"><span class="badge">${esc(spec ? spec.label : r.Type)}</span> ${spec ? paramSummary(spec.params, r) : paramSummary(GENERIC, r)}
                    ${r.Criteria ? ' · si <code>' + esc(r.Criteria) + '</code>' : ''}</div>
                  <div id="rf${r.Id}"></div></div>
                <span class="row"><button class="small" data-redit="${r.Id}">Modifier</button><button class="small danger" data-rdel="${r.Id}">Supprimer</button></span></div>`;
          }).join('')}
          <div style="margin:6px 0 0 18px"><button class="small" data-radd="${id}">+ Réponse</button></div>
          <div id="rfnew${id}"></div>
        </div>`;
    }).join('');

    $('#tab').innerHTML = `
      <div class="card"><div class="help" style="margin:0">Le jeu ne peut afficher que les textes de <b>ce PNJ</b> : questions et réponses se choisissent dans sa liste
        (${n.messages.length} questions, ${n.replies.length} réponses). Une question « départ » s'ouvre au clic via une action Dialogue (onglet Actions).</div></div>
      ${blocks || '<div class="muted">Ce PNJ n\'a aucun texte de dialogue dans le jeu.</div>'}`;

    $('#tab').querySelectorAll('[data-radd]').forEach(b => b.onclick = () => replyForm(null, +b.dataset.radd, '#rfnew' + b.dataset.radd));
    $('#tab').querySelectorAll('[data-redit]').forEach(b => b.onclick = () => { const r = n.dialog.find(x => x.Id == b.dataset.redit); replyForm(r, +r.MessageId, '#rf' + r.Id); });
    $('#tab').querySelectorAll('[data-rdel]').forEach(b => b.onclick = async () => {
        if (!confirm('Supprimer cette réponse ?')) return;
        if (await act(api('reply_delete', {}, { id: +b.dataset.rdel }), 'Réponse supprimée')) openNpc(n.id, true);
    });
}

function replyForm(r, messageId, target) {
    const n = state.npc;
    const box = $(target);
    const types = Object.keys(REPLY_TYPES);
    const draw = t => {
        const spec = REPLY_TYPES[t] || { params: GENERIC };
        const replyOpts = n.replies.map(x => `<option value="${x.id}" ${r && +r.ReplyId === x.id ? 'selected' : ''}>#${x.id} — ${esc(cut(x.text || '(texte introuvable)', 100))}</option>`).join('');
        box.innerHTML = `<div class="form">
          <div class="grid">
            <label style="grid-column:1/-1">Texte de la réponse<select id="rfReply"><option value="">— choisir —</option>${r && !n.replies.some(x => x.id == r.ReplyId) ? `<option value="${r.ReplyId}" selected>#${r.ReplyId} — inconnu de ce PNJ</option>` : ''}${replyOpts}</select></label>
            <label>Effet<select id="rfType">${types.map(k => `<option value="${k}" ${k === t ? 'selected' : ''}>${REPLY_TYPES[k].label}</option>`).join('')}${types.includes(t) ? '' : `<option value="${esc(t)}" selected>${esc(t)}</option>`}</select></label>
            <label>Condition d'affichage (facultatif)<input id="rfCrit" value="${esc(r ? r.Criteria || '' : '')}" placeholder="ex. PO=8437"></label>
            ${spec.params.map(s => paramField(s, r && r.Type === t ? r['Parameter' + s.p] : '')).join('')}
          </div>
          ${spec.help ? `<div class="help">${spec.help}</div>` : ''}
          <div class="actions"><button id="rfCancel">Annuler</button><button id="rfSave" class="pri">Enregistrer</button></div></div>`;
        $('#rfType', box).onchange = e => draw(e.target.value);
        $('#rfCancel', box).onclick = () => box.innerHTML = '';
        $('#rfSave', box).onclick = async () => {
            const body = { id: r ? r.Id : 0, messageId, replyId: +$('#rfReply', box).value, type: $('#rfType', box).value, criteria: $('#rfCrit', box).value,
                additional: r && r.Type === $('#rfType', box).value ? r.AdditionalParameters || '' : '', ...readParams(box) };
            if (await act(api('reply_save', {}, body), 'Réponse enregistrée')) openNpc(n.id, true);
        };
        wireItemFields(box);
    };
    draw(r ? r.Type : 'EndDialog');
}

/* ------------------------------------------------------------------ onglet PNJ */
function tabNpcs() {
    $('#main').innerHTML = `<div class="layout">
      <aside>
        <input id="search" type="search" placeholder="Nom ou numéro du PNJ…" autocomplete="off">
        <div class="filters">
          <label><input type="radio" name="filter" value="placed" checked> Placés</label>
          <label><input type="radio" name="filter" value="all"> Tous</label>
          <label><input type="radio" name="filter" value="mute"> Placés muets</label>
        </div>
        <div id="npcList" class="list"></div>
      </aside>
      <main id="detail"><div class="empty">Choisis un PNJ dans la liste.</div></main>
    </div>`;
    $('#search').addEventListener('input', () => { clearTimeout(searchTimer); searchTimer = setTimeout(loadList, 250); });
    document.querySelectorAll('input[name=filter]').forEach(r => r.addEventListener('change', loadList));
    if (state.npc) openNpc(state.npc.id, true);
    return loadList();
}

document.addEventListener('click', e => {
    const it = e.target.closest('#npcList .it');
    if (it) openNpc(+it.dataset.id);
    const tab = e.target.closest('.tabs button');
    if (tab) { state.tab = tab.dataset.tab; renderNpc(); }
});
$('#btnReload').onclick = async () => {
    status('Rechargement…');
    try { reloadStatus(await api('reload', {}, {})); } catch (e) { toast(e.message, true); }
};
