/* Panel admin Populus 2.51 : navigation par onglets, Accueil, Joueurs, Console, Historique, Journal. */
'use strict';

const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];
const card = (title, body) => `<div class="card"><h3>${title}</h3>${body}</div>`;
const valOf = id => ($('#' + id) ? $('#' + id).value.trim() : '');
const debounce = (fn, ms = 250) => { let t; return (...a) => { clearTimeout(t); t = setTimeout(() => fn(...a), ms); }; };

/* ------------------------------------------------------------------ réponses du serveur (bas de page) */
function out(text, ok = true) {
    const d = document.createElement('div');
    d.className = 'l ' + (ok ? 'ok' : 'err');
    d.textContent = '[' + new Date().toLocaleTimeString() + '] ' + text;
    $('#outBody').prepend(d);
}
$('#outClear').onclick = () => $('#outBody').innerHTML = '';

/** Lance une commande du serveur avec tes droits (comme si tu la tapais en jeu). */
async function cmd(text, opts = {}) {
    try {
        const r = await api('cmd', {}, { cmd: text });
        if (!opts.quiet) out('.' + text.replace(/^\./, '') + '\n' + (r.out || ''), r.ok);
        return r;
    } catch (e) {
        out('.' + text + ' → ' + e.message, false);
        if (!opts.quiet) toast(e.message, true);
        return { ok: false, out: e.message };
    }
}

/* ------------------------------------------------------------------ position / état */
let STATE = null;
async function refreshState() {
    try {
        STATE = await api('state');
        const m = STATE.me;
        $('#where').innerHTML = m
            ? `<b>${esc(m.name)}</b> · map <b>${m.mapId}</b> [${m.x},${m.y}] ${esc(m.zone || '')} · case ${m.cellId} · ${STATE.online} en ligne`
            : `<b>${esc(window.ME)}</b> hors ligne · ${STATE.online} en ligne`;
    } catch (e) {
        $('#where').textContent = e.message;
    }
    return STATE;
}
$('#btnState').onclick = refreshState;

/* ------------------------------------------------------------------ accueil */
async function tabHome() {
    const s = await refreshState();
    const up = s ? Math.floor(s.uptime / 3600) + ' h ' + Math.floor(s.uptime % 3600 / 60) + ' min' : '?';
    $('#main').innerHTML = `
      <div class="stats" style="margin-bottom:14px">
        <div class="stat"><b>${s ? s.online : '?'}</b><span>joueurs en ligne</span></div>
        <div class="stat"><b>${up}</b><span>serveur lancé depuis</span></div>
        <div class="stat"><b>${s ? s.memoryMb + ' Mo' : '?'}</b><span>mémoire du serveur</span></div>
        <div class="stat"><b>${s ? s.changes24h : '?'}</b><span>modifications (24 h)</span></div>
      </div>
      ${card('Raccourcis', `<div class="btns">
        <button data-go="players">👥 Joueurs en ligne</button>
        <button data-go="npcs">🧙 PNJ</button>
        <button data-go="console">⌨ Console</button>
        <button data-go="history">🕘 Historique</button>
        <button data-c="save world">💾 Sauvegarder le monde</button>
        <button data-c="invisible">👻 Invisible</button>
        <button data-c="god on">⚡ Mode dieu</button>
        <button data-c="god off">Mode dieu off</button>
      </div>`)}
      ${card('Annonce à tous les joueurs', `<div class="row"><input id="annText" placeholder="Message affiché à tous les joueurs"><button id="annGo" class="pri">Envoyer</button></div>`)}
      ${card('Me téléporter', `<div class="row"><input id="tpMap" type="number" placeholder="numéro de map" style="width:180px"><input id="tpCell" type="number" placeholder="case (facultatif)" style="width:150px"><button id="tpGo">Y aller</button>
        <input id="tpX" type="number" placeholder="x" style="width:80px"><input id="tpY" type="number" placeholder="y" style="width:80px"><button id="tpXY">Aller en [x,y]</button></div>`)}`;
    $$('#main [data-go]').forEach(b => b.onclick = () => showTab(b.dataset.go));
    $$('#main [data-c]').forEach(b => b.onclick = () => cmd(b.dataset.c));
    $('#annGo').onclick = () => { const t = valOf('annText'); if (t && confirm('Envoyer à tous : « ' + t + ' » ?')) cmd('announce ' + t); };
    $('#tpGo').onclick = () => { const m = valOf('tpMap'); if (m) cmd('go ' + m + ' moi' + (valOf('tpCell') ? ' ' + valOf('tpCell') : '')).then(refreshState); };
    $('#tpXY').onclick = () => { if (valOf('tpX') && valOf('tpY')) cmd('gopos ' + valOf('tpX') + ' ' + valOf('tpY')).then(refreshState); };
}

/* ------------------------------------------------------------------ joueurs */
let PLAYERS = [];
function tabPlayers() {
    $('#main').innerHTML = `<div class="cols">
      <div class="card"><h3>👥 Joueurs</h3><input id="pq" placeholder="Nom du perso ou du compte (vide = en ligne)">
        <div id="plist" class="list" style="margin-top:8px;max-height:65vh"></div></div>
      <div id="pdet"><div class="card empty" style="margin:0">Choisis un joueur dans la liste.</div></div></div>`;
    const load = async () => {
        try {
            PLAYERS = (await api('players', { q: valOf('pq') })).players;
        } catch (e) { toast(e.message, true); return; }
        $('#plist').innerHTML = PLAYERS.length ? PLAYERS.map((p, i) => `<div class="it" data-i="${i}"><span><span class="dot ${p.online ? 'on' : ''}"></span>${esc(p.name)}
            ${p.role && p.role !== 'Player' ? '<span class="pill">' + esc(p.role) + '</span>' : ''}</span><small>${esc(p.breedName || '')} ${p.level || ''}</small></div>`).join('')
            : '<div class="muted">Aucun joueur.</div>';
        $$('#plist .it').forEach(el => el.onclick = () => { $$('#plist .it').forEach(x => x.classList.remove('on')); el.classList.add('on'); playerDetail(PLAYERS[+el.dataset.i]); });
    };
    $('#pq').oninput = debounce(load);
    return load();
}

function playerDetail(p) {
    const n = p.name;
    const act = (label, c, cls = '') => `<button class="${cls}" data-c="${esc(c)}">${label}</button>`;
    const inp = (id, ph, w = 90) => `<input id="${id}" placeholder="${ph}" style="width:${w}px">`;
    const f = (id, ph, label, tpl, w) => `<span class="fit">${inp(id, ph, w)}<button data-f="${esc(tpl)}">${label}</button></span>`;
    $('#pdet').innerHTML =
        card(`${p.online ? '🟢' : '⚫'} ${esc(n)}`, `<div class="kv">
            <span>Compte</span><span>${esc(p.account || '?')} ${p.accountId ? '(#' + p.accountId + ')' : ''}</span>
            <span>Classe</span><span>${esc(p.breedName || '')}</span><span>Niveau</span><span>${p.level || '?'}</span>
            <span>Map</span><span>${p.mapId || '?'} ${p.x !== undefined && p.x !== null ? '[' + p.x + ',' + p.y + ']' : ''}</span>
            <span>Kamas</span><span>${(p.kamas || 0).toLocaleString('fr-FR')}</span>
            ${p.online ? `<span>IP</span><span>${esc(p.ip || '')}</span><span>Rôle</span><span>${esc(p.role || '')}</span><span>En combat</span><span>${p.fighting ? 'oui' : 'non'}</span>`
                       : `<span>Dernière connexion</span><span>${esc(p.lastUsage || '')}</span>`}</div>
            ${p.online ? '' : '<p class="muted" style="margin-top:8px">Hors ligne : la plupart des actions demandent qu\'il soit connecté.</p>'}`)
        + card('🧭 Déplacement', `<div class="btns">${act('Aller vers lui', 'goname ' + n)}${act('L\'amener à moi', 'goname ' + window.ME + ' ' + n)}
            ${act('Infos', 'info character ' + n)}${act('Ouvrir sa banque', 'bank open ' + n)}${act('Voir son inventaire', 'item showinv ' + n)}</div>`)
        + card('🎁 Donner', `<div class="btns">
            ${f('pv_k', 'kamas', 'Kamas', 'kamas {pv_k} ' + n, 110)}
            ${f('pv_l', 'niveaux', 'Niveaux', 'levelup {pv_l} ' + n)}
            <span class="fit">${inp('pv_i', 'objet n°', 90)}${inp('pv_iq', 'qté', 60)}<button data-f="item add {pv_i} ${esc(n)} {pv_iq}">Objet</button>
              <button data-f="item add {pv_i} ${esc(n)} {pv_iq} true" title="Objet aux stats maximum">Objet jet max</button></span></div>`)
        + card('🎭 Apparence et état', `<div class="btns">
            ${f('pv_look', '{1|…}', 'Changer le look', 'look {pv_look} ' + n, 160)}
            ${act('Invisible on/off', 'invisible ' + n)}${act('Mode dieu', 'god on ' + n)}${act('Fin mode dieu', 'god off ' + n)}</div>`)
        + card('⚖️ Sanctions', `<div class="btns">
            ${f('pv_mu', 'minutes', 'Rendre muet', 'mute ' + n + ' {pv_mu}')}${act('Rendre la parole', 'unmute ' + n)}
            ${f('pv_j', 'minutes', 'Prison', 'jail ' + n + ' {pv_j}')}
            ${act('Expulser', 'kick ' + n, 'danger')}
            <span class="fit">${inp('pv_bm', 'minutes')}${inp('pv_br', 'raison', 160)}<button class="danger" data-ban="1">Bannir</button></span>
            ${act('Débannir', 'unban ' + n)}</div>`);
    $$('#pdet [data-c]').forEach(b => b.onclick = () => {
        if (b.classList.contains('danger') && !confirm('Confirmer : ' + b.dataset.c + ' ?')) return;
        cmd(b.dataset.c);
    });
    $$('#pdet [data-f]').forEach(b => b.onclick = () => {
        let miss = false;
        const c = b.dataset.f.replace(/\{(\w+)\}/g, (_, id) => { const v = valOf(id); if (!v && id !== 'pv_iq') miss = true; return v || (id === 'pv_iq' ? '1' : ''); });
        if (miss) { toast('Remplis la valeur', true); return; }
        cmd(c);
    });
    $('#pdet [data-ban]').onclick = () => {
        const m = valOf('pv_bm'), r = valOf('pv_br') || 'Décision du staff';
        if (!m) { toast('Durée du ban en minutes', true); return; }
        if (confirm(`Bannir ${n} pendant ${m} minutes ?`)) cmd(`ban ${n} ${m} ${r}`);
    };
}

/* ------------------------------------------------------------------ console */
const HIST = [];
let histI = 0;
function tabConsole() {
    $('#main').innerHTML = card('⌨ Console', `<p class="muted">N'importe quelle commande du serveur, avec tes droits (sans le point). <code>help</code> donne la liste de tes commandes.</p>
      <div class="row" style="margin-top:8px"><input id="cin" placeholder="ex : item add 2469 moi 1 true" autocomplete="off"><button class="pri" id="cgo">Envoyer</button></div>
      <pre id="cres" style="margin-top:10px;white-space:pre-wrap;font:12.5px Consolas,monospace;max-height:58vh;overflow:auto"></pre>`);
    const go = async () => {
        const c = valOf('cin');
        if (!c) return;
        HIST.push(c); histI = HIST.length;
        $('#cin').value = '';
        const r = await cmd(c, { quiet: true });
        $('#cres').textContent = '> ' + c + '\n' + (r.out || '') + '\n\n' + $('#cres').textContent;
    };
    $('#cgo').onclick = go;
    $('#cin').onkeydown = e => {
        if (e.key === 'Enter') go();
        else if (e.key === 'ArrowUp' && histI > 0) { histI--; $('#cin').value = HIST[histI]; }
        else if (e.key === 'ArrowDown') { histI = Math.min(HIST.length, histI + 1); $('#cin').value = HIST[histI] || ''; }
    };
    $('#cin').focus();
}

/* ------------------------------------------------------------------ historique (annulable) */
async function tabHistory() {
    let rows;
    try { rows = (await api('history')).history; } catch (e) { toast(e.message, true); return; }
    $('#main').innerHTML = card('🕘 Historique des modifications', rows.length ? `<table><tr><th>Date</th><th>MJ</th><th>Modification</th><th>Détail</th><th></th></tr>
        ${rows.map(r => `<tr class="${+r.Undone ? 'undone' : ''}"><td>${esc(r.At)}</td><td>${esc(r.CharacterName)}</td><td>${esc(r.Action)}</td>
          <td class="muted">${esc(r.Details || '')} <small>(${esc(r.TableName)} #${r.RowId})</small></td>
          <td class="act">${+r.Undone ? 'annulé' : `<button class="small" data-undo="${r.Id}">↶ Annuler</button>`}</td></tr>`).join('')}</table>`
        : '<div class="muted">Aucune modification pour l\'instant.</div>');
    $$('#main [data-undo]').forEach(b => b.onclick = async () => {
        if (!confirm('Annuler cette modification ?')) return;
        try {
            let r = await api('history_undo', {}, { id: +b.dataset.undo });
            if (r.confirm) {
                if (!confirm(r.confirm)) return;
                r = await api('history_undo', {}, { id: +b.dataset.undo, force: 1 });
            }
            toast('Modification annulée');
            if (r.reload) reloadStatus(r.reload);
            tabHistory();
        } catch (e) { toast(e.message, true); }
    });
}

/* ------------------------------------------------------------------ journal */
async function tabLog() {
    let rows;
    try { rows = (await api('log')).log; } catch (e) { toast(e.message, true); return; }
    $('#main').innerHTML = card('📜 Journal du panel', rows.length ? `<table><tr><th>Date</th><th>MJ</th><th>Quoi</th><th>Détails</th></tr>
        ${rows.map(r => `<tr><td>${esc(r.At)}</td><td>${esc(r.CharacterName)}</td><td>${esc(r.Action)}</td><td><code style="white-space:pre-wrap;word-break:break-all">${esc(r.Details || '')}</code></td></tr>`).join('')}</table>`
        : '<div class="muted">Rien pour l\'instant.</div>');
}

/* ------------------------------------------------------------------ onglets */
const soon = name => () => { $('#main').innerHTML = `<div class="soon">L'onglet <b>${name}</b> arrive dans un prochain lot.</div>`; };
const TABS = {
    home: tabHome, players: tabPlayers, npcs: tabNpcs, console: tabConsole, history: tabHistory, log: tabLog,
    items: soon('Objets'), mobs: soon('Monstres'), spells: soon('Sorts'), maps: soon('Cartes'),
    server: soon('Serveur'), watch: soon('Surveillance'), support: soon('Support'),
};

function showTab(t) {
    t = TABS[t] ? t : 'home';
    $$('#tabs button').forEach(b => b.classList.toggle('on', b.dataset.tab === t));
    history.replaceState(null, '', '#' + t);
    Promise.resolve(TABS[t]()).catch(e => toast(e.message, true));
}
$$('#tabs button').forEach(b => b.onclick = () => showTab(b.dataset.tab));
window.addEventListener('load', () => showTab(location.hash.slice(1) || 'home'));
