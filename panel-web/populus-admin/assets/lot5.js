/* Panel admin Populus 2.51 : onglets Serveur, Surveillance, Support. */
'use strict';

const fmtK = n => (+n || 0).toLocaleString('fr-FR');
const fmtDur = s => s >= 86400 ? Math.floor(s / 86400) + ' j ' + Math.floor(s % 86400 / 3600) + ' h' : Math.floor(s / 3600) + ' h ' + Math.floor(s % 3600 / 60) + ' min';

/** Petite courbe (canvas) : points = valeurs, labels = libellés de l'axe. */
function drawChart(cv, series, labels) {
    const g = cv.getContext('2d'), W = cv.width, H = cv.height, pad = 28;
    g.clearRect(0, 0, W, H);
    const all = series.flatMap(s => s.values);
    const max = Math.max(1, ...all);
    g.strokeStyle = '#3a2c22'; g.fillStyle = '#a8968a'; g.font = '11px Segoe UI';
    for (let i = 0; i <= 4; i++) {
        const y = pad + (H - 2 * pad) * i / 4;
        g.beginPath(); g.moveTo(pad, y); g.lineTo(W - 8, y); g.stroke();
        g.fillText(Math.round(max * (4 - i) / 4), 2, y + 4);
    }
    series.forEach(s => {
        g.strokeStyle = s.color; g.lineWidth = 2; g.beginPath();
        s.values.forEach((v, i) => {
            const x = pad + (W - pad - 8) * (s.values.length > 1 ? i / (s.values.length - 1) : 0);
            const y = H - pad - (H - 2 * pad) * v / max;
            i ? g.lineTo(x, y) : g.moveTo(x, y);
        });
        g.stroke();
    });
    if (labels.length) {
        g.fillStyle = '#a8968a';
        [0, Math.floor(labels.length / 2), labels.length - 1].forEach(i => {
            const x = pad + (W - pad - 8) * (labels.length > 1 ? i / (labels.length - 1) : 0);
            g.fillText(labels[i], Math.min(x, W - 70), H - 8);
        });
    }
}

/* ============================================================ SERVEUR */
let SRV_TIMER = null;
async function tabServer() {
    clearInterval(SRV_TIMER);
    let r;
    try { r = await api('server_health'); } catch (e) { toast(e.message, true); return; }
    const L = r.live;
    const now = new Date(), pad2 = n => String(n).padStart(2, '0');
    const local = d => `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}T${pad2(d.getHours())}:${pad2(d.getMinutes())}`;
    $('#main').innerHTML = `
      <div class="stats" style="margin-bottom:14px">
        <div class="stat"><b>${L ? L.online : '—'}</b><span>joueurs en ligne (pic 24 h : ${r.peak ?? '—'})</span></div>
        <div class="stat"><b>${L ? L.fights : '—'}</b><span>combats en cours</span></div>
        <div class="stat"><b>${L ? L.memoryMb + ' Mo' : '—'}</b><span>mémoire (${L ? L.threads : '?'} threads)</span></div>
        <div class="stat"><b>${L ? fmtDur(L.uptime) : 'hors ligne'}</b><span>serveur lancé depuis</span></div>
        <div class="stat"><b>${L ? 'x' + L.rates.xp + ' / x' + L.rates.drops : '—'}</b><span>taux XP / drop actuels (kamas x${L ? L.rates.kamas : '?'})</span></div>
      </div>
      ${card('📈 Dernières 24 h', `<canvas id="hcv" width="900" height="200" style="width:100%;max-width:900px"></canvas>
        <div class="btns" style="font-size:12px"><span><span class="lg" style="background:#ffb547"></span>joueurs</span><span><span class="lg" style="background:#6aa8ff"></span>combats</span></div>`)}
      ${card('🔁 Redémarrage propre', L && L.restartIn !== null
        ? `<p>Redémarrage dans <b>${Math.max(0, Math.round(L.restartIn))} s</b>${L.restartReason ? ' : ' + esc(L.restartReason) : ''}.</p><button id="rsCancel" class="danger">Annuler le redémarrage</button>`
        : `<div class="row"><input id="rsMin" type="number" value="5" style="width:90px"><span class="muted">minutes</span><input id="rsWhy" placeholder="raison affichée aux joueurs (facultatif)"><button id="rsGo" class="danger">Programmer</button></div>
           <div class="help">Annonces automatiques à 10, 5, 2 et 1 min puis 30 et 10 s ; sauvegarde du monde puis arrêt. Sur le VPS, Start_WORLD.bat relance le serveur tout seul.</div>`)}
      ${card('📣 Annonce maintenant', `<div class="row"><input id="anMsg" placeholder="Message à tous les joueurs"><button id="anGo" class="pri">Envoyer</button></div>`)}
      ${card('📅 Événements programmés', `<div class="grid">
          <label>Type<select id="evKind"><option value="taux">Taux bonus (XP / drop / kamas)</option><option value="annonce">Annonce</option></select></label>
          <label>Début<input id="evStart" type="datetime-local" value="${local(now)}"></label>
          <label>Fin<input id="evEnd" type="datetime-local" value="${local(new Date(now.getTime() + 2 * 3600e3))}"></label>
          <label>XP x<input id="evXp" placeholder="ex. 18"></label><label>Drop x<input id="evDrop" placeholder="ex. 8"></label><label>Kamas x<input id="evKamas" placeholder="ex. 2"></label>
          <label style="grid-column:span 2">Message<input id="evMsg" placeholder="ex. Week-end XP x2 !"></label>
          <label>Répéter l'annonce (min)<input id="evRep" type="number" value="0"></label><button id="evAdd" class="pri">+ Programmer</button></div>
        <table style="margin-top:10px"><tr><th>Type</th><th>Début</th><th>Fin</th><th>Taux</th><th>Message</th><th>État</th><th></th></tr>
        ${r.events.map(e => `<tr><td>${esc(e.Kind)}</td><td>${esc(e.StartAt)}</td><td>${esc(e.EndAt || '—')}</td>
          <td>${e.Kind === 'taux' ? `XP ${e.XpRate ?? '='} · drop ${e.DropsRate ?? '='} · kamas ${e.KamasRate ?? '='}` : (e.RepeatMinutes > 0 ? 'toutes les ' + e.RepeatMinutes + ' min' : '')}</td>
          <td>${esc(e.Message || '')}</td><td>${e.Status === 'en cours' ? '<span class="badge ok">en cours</span>' : esc(e.Status)}</td>
          <td class="act">${['prevu', 'en cours'].includes(e.Status) ? `<button class="small danger" data-evc="${e.Id}">${e.Status === 'en cours' ? 'Arrêter' : 'Annuler'}</button>` : ''}</td></tr>`).join('') || '<tr><td colspan="7" class="muted">Aucun événement.</td></tr>'}</table>`)}`;
    drawChart($('#hcv'), [{ color: '#ffb547', values: r.history.map(h => +h.Online) }, { color: '#6aa8ff', values: r.history.map(h => +h.Fights) }], r.history.map(h => h.t));
    if ($('#rsGo')) $('#rsGo').onclick = async () => {
        const m = +valOf('rsMin');
        if (!confirm(`Redémarrer le serveur dans ${m} minute(s) ? Tous les joueurs seront déconnectés.`)) return;
        if (await act(api('server_restart', {}, { minutes: m, reason: valOf('rsWhy') }), 'Redémarrage programmé')) tabServer();
    };
    if ($('#rsCancel')) $('#rsCancel').onclick = async () => { if (await act(api('server_restart_cancel', {}, {}), 'Redémarrage annulé')) tabServer(); };
    $('#anGo').onclick = async () => { const m = valOf('anMsg'); if (m && confirm('Envoyer à tous : « ' + m + ' » ?')) await act(api('server_announce', {}, { message: m }), 'Annonce envoyée'); };
    $('#evAdd').onclick = async () => {
        const body = { kind: $('#evKind').value, start: valOf('evStart'), end: valOf('evEnd'), xp: valOf('evXp'), drops: valOf('evDrop'), kamas: valOf('evKamas'), message: valOf('evMsg'), repeat: valOf('evRep') };
        if (await act(api('event_save', {}, body), 'Événement programmé')) tabServer();
    };
    $$('#main [data-evc]').forEach(b => b.onclick = async () => { if (confirm('Arrêter / annuler cet événement ?') && await act(api('event_cancel', {}, { id: +b.dataset.evc }), 'Fait')) tabServer(); });
    if (L && L.restartIn !== null) SRV_TIMER = setInterval(() => { if (!$('#rsCancel')) { clearInterval(SRV_TIMER); return; } tabServer(); }, 15000);
}

/* ============================================================ SURVEILLANCE */
const WATCH = [['alerts', '🚨 Alertes'], ['progress', '📈 Progression'], ['ip', '🌐 IP'], ['trades', '🤝 Échanges'], ['hdv', '🏪 HDV'], ['money', '💰 Kamas'], ['sanctions', '⚖️ Sanctions'], ['commands', '⌨ Commandes MJ']];
let WATCH_TAB = 'alerts';
function tabWatch() {
    $('#main').innerHTML = `<div class="ltabs" style="margin-bottom:12px">${WATCH.map(([k, l]) => `<button data-w="${k}" class="${k === WATCH_TAB ? 'on' : ''}">${l}</button>`).join('')}</div><div id="wbody"></div>`;
    $$('#main [data-w]').forEach(b => b.onclick = () => { WATCH_TAB = b.dataset.w; tabWatch(); });
    watchDraw();
}

async function watchDraw(q = '') {
    let r;
    try { r = await api('watch', { what: WATCH_TAB, q }); } catch (e) { $('#wbody').innerHTML = `<p class="err">${esc(e.message)}</p>`; return; }
    const search = ph => `<div class="row" style="margin-bottom:8px"><input id="wq" placeholder="${ph}" value="${esc(q)}"></div>`;
    const body = $('#wbody');
    const who = (id, name) => `<a href="#" data-player="${esc(name)}">${esc(name)}</a>`;
    switch (WATCH_TAB) {
        case 'alerts': {
            const t = r.thresholds;
            body.innerHTML = card(`Niveaux gagnés en 24 h (≥ ${t.levels})`, r.levels.length ? `<table><tr><th>Perso</th><th>Niveaux</th></tr>${r.levels.map(x => `<tr><td>${who(x.CharacterId, x.Name)}</td><td>${x.a} → ${x.b} (+${x.d})</td></tr>`).join('')}</table>` : '<div class="muted">Rien d\'anormal.</div>')
                + card(`Kamas gagnés en 24 h (≥ ${fmtK(t.kamas)})`, r.money.length ? `<table><tr><th>Perso</th><th>Gain (perso + banque)</th></tr>${r.money.map(x => `<tr><td>${who(x.CharacterId, x.Name)}</td><td>+${fmtK(x.d)}</td></tr>`).join('')}</table>` : '<div class="muted">Rien d\'anormal.</div>')
                + card(`IP avec ${t.ip} comptes ou plus (24 h)`, r.ips.length ? `<table><tr><th>IP</th><th>Comptes</th><th>Persos</th></tr>${r.ips.map(x => `<tr><td>${esc(x.Ip)}</td><td>${x.n}</td><td>${esc(x.names)}</td></tr>`).join('')}</table>` : '<div class="muted">Rien d\'anormal.</div>')
                + card(`Gros échanges (≥ ${fmtK(t.trade)} kamas, 3 jours)`, r.trades.length ? `<table><tr><th>Date</th><th>Perso</th><th>Kamas</th><th></th></tr>${r.trades.map(x => { const d = JSON.parse(x.Details || '{}'); return `<tr class="${+x.Handled ? 'undone' : ''}"><td>${esc(x.At)}</td><td>${esc(x.Name)} ↔ ${esc(d.avec || '?')}</td><td>${fmtK(x.Value)}</td><td class="act">${+x.Handled ? '' : `<button class="small" data-h="${x.Id}">Vu</button>`}</td></tr>`; }).join('')}</table>` : '<div class="muted">Aucun.</div>');
            break;
        }
        case 'progress':
            body.innerHTML = card('Progression sur 7 jours', search('Nom du perso') + `<table><tr><th>Perso</th><th>Niveau</th><th>Kamas (perso + banque)</th><th>Dernier relevé</th></tr>
                ${r.rows.map(x => `<tr><td>${who(x.CharacterId, x.Name)}</td><td>${x.a} → ${x.b}</td><td>${fmtK(x.ka)} → ${fmtK(x.kb)}</td><td>${esc(x.last)}</td></tr>`).join('') || '<tr><td colspan="4" class="muted">Pas encore de relevés (un relevé des connectés toutes les 10 min).</td></tr>'}</table>`);
            break;
        case 'ip':
            body.innerHTML = card('IP des 7 derniers jours', `<table><tr><th>IP</th><th>Comptes</th><th>Persos</th><th>Noms</th><th>Dernière connexion</th></tr>
                ${r.rows.map(x => `<tr><td>${esc(x.Ip)}</td><td>${x.accounts}</td><td>${x.chars}</td><td>${esc(x.names)}</td><td>${esc(x.last)}</td></tr>`).join('') || '<tr><td colspan="5" class="muted">Aucune connexion enregistrée.</td></tr>'}</table>`);
            break;
        case 'trades': {
            const it = l => (l || []).map(x => `${x.stack} × ${esc((r.items[x.id] || {}).name || '#' + x.id)}`).join(', ');
            body.innerHTML = card('Échanges entre joueurs', search('Nom d\'un des deux persos') + `<table><tr><th>Date</th><th>De</th><th>Donne</th><th>À</th><th>Donne</th></tr>
                ${r.rows.map(x => { const d = x.Details || {}; return `<tr><td>${esc(x.At)}</td><td>${esc(x.Name)}<div class="muted">${esc(x.Ip || '')}</div></td><td>${fmtK(d.donneKamas)} k ${it(d.donne)}</td>
                  <td>${esc(d.avec || '?')}<div class="muted">${esc(d.avecIp || '')}</div></td><td>${fmtK(d.recoitKamas)} k ${it(d.recoit)}</td></tr>`; }).join('') || '<tr><td colspan="5" class="muted">Aucun échange.</td></tr>'}</table>`);
            break;
        }
        case 'hdv':
            body.innerHTML = card(`Hôtels de vente — ${fmtK(r.onSale.n)} objets en vente (${fmtK(r.onSale.total)} kamas)`, `<table><tr><th>Vendu le</th><th>Objet</th><th>Qté</th><th>Prix</th><th>Prix moyen / u.</th></tr>
                ${r.rows.map(x => `<tr class="${x.suspect ? 'err' : ''}"><td>${esc(x.SellDate || '')}</td><td>${esc(x.Name)}</td><td>${x.Stack}</td><td>${fmtK(x.Price)}${x.suspect ? ' ⚠️' : ''}</td><td>${fmtK(Math.round(x.avgUnit || 0))}</td></tr>`).join('') || '<tr><td colspan="5" class="muted">Aucune vente.</td></tr>'}</table>
                <div class="help">⚠️ = vendu plus de ${r.ratio} fois le prix moyen de l'objet (possible transfert de kamas).</div>`);
            break;
        case 'money':
            body.innerHTML = `<div class="stats" style="margin-bottom:12px"><div class="stat"><b>${fmtK(r.total)}</b><span>kamas sur le serveur (persos + banques)</span></div></div>`
                + card('Masse de kamas des joueurs connectés (par jour)', `<canvas id="mcv" width="900" height="200" style="width:100%;max-width:900px"></canvas>`)
                + card('Les plus riches', `<table><tr><th>Perso</th><th>Sur lui</th><th>Banque</th><th>Total</th></tr>${r.rich.map(x => `<tr><td>${who(x.Id, x.Name)}</td><td>${fmtK(x.Kamas)}</td><td>${fmtK(x.Bank)}</td><td><b>${fmtK(x.Total)}</b></td></tr>`).join('')}</table>`);
            drawChart($('#mcv'), [{ color: '#ffb547', values: r.days.map(d => +d.total) }], r.days.map(d => d.d));
            break;
        case 'sanctions':
            body.innerHTML = card('Comptes bannis / en prison', `<table><tr><th>Compte</th><th>État</th><th>Raison</th><th>Fin</th><th></th></tr>
                ${r.bans.map(x => `<tr><td>${esc(x.Login)} <span class="muted">${esc(x.Nickname || '')}</span></td><td>${+x.IsBanned ? 'banni' : ''}${+x.IsJailed ? ' prison' : ''}</td><td>${esc(x.BanReason || '')}</td><td>${esc(x.BanEndDate || 'définitif')}</td>
                  <td class="act">${+x.IsBanned ? `<button class="small" data-c="unbanacc ${esc(x.Login)}">Débannir</button>` : ''}</td></tr>`).join('') || '<tr><td colspan="5" class="muted">Personne.</td></tr>'}</table>`)
                + card('Dernières sanctions données', `<table>${r.history.map(x => `<tr><td>${esc(x.At)}</td><td>${esc(x.Name)}</td><td><code>${esc(x.Details)}</code></td></tr>`).join('') || '<tr><td class="muted">Aucune.</td></tr>'}</table>`);
            break;
        case 'commands':
            body.innerHTML = card('Commandes MJ (en jeu et depuis le panel)', search('Nom du MJ ou commande') + `<table><tr><th>Date</th><th>MJ</th><th>IP</th><th>Commande</th></tr>
                ${r.rows.map(x => `<tr><td>${esc(x.At)}</td><td>${esc(x.Name)}</td><td>${esc(x.Ip || '')}</td><td><code>${esc(x.Details)}</code></td></tr>`).join('') || '<tr><td colspan="4" class="muted">Aucune.</td></tr>'}</table>`);
            break;
    }
    if ($('#wq')) { $('#wq').oninput = debounce(() => watchDraw(valOf('wq'))); $('#wq').focus(); $('#wq').setSelectionRange(q.length, q.length); }
    $$('#wbody [data-h]').forEach(b => b.onclick = async () => { await act(api('watch_handle', {}, { id: +b.dataset.h })); watchDraw(q); });
    $$('#wbody [data-c]').forEach(b => b.onclick = async () => { if (confirm(b.dataset.c + ' ?')) { await cmd(b.dataset.c); watchDraw(q); } });
    $$('#wbody [data-player]').forEach(a => a.onclick = e => { e.preventDefault(); showTab('players'); setTimeout(() => { $('#pq').value = a.dataset.player; $('#pq').oninput(); }, 300); });
}

/* ============================================================ SUPPORT */
async function tabSupport() {
    let copies = [];
    try { copies = (await api('support_copies')).copies; } catch (e) { toast(e.message, true); }
    $('#main').innerHTML = card('🛟 Support : comparer un personnage avec une copie', `
        <div class="grid">
          <label class="suggest">Personnage<input id="spName" placeholder="nom du perso" autocomplete="off"><div class="drop" hidden></div></label>
          <label>Copie<select id="spCopy">${copies.map(c => `<option value="${c.stamp}">${c.label}</option>`).join('') || '<option value="">aucune copie</option>'}</select></label>
          <button id="spGo" class="pri">Comparer</button><button id="spNow">Faire une copie maintenant</button></div>
        <div class="help">Une copie des personnages, de leurs objets (avec leurs jets) et des banques est faite chaque jour après 5 h, gardée 14 jours.
          La comparaison montre ce qui a disparu ou est apparu depuis ; un objet perdu peut être rendu à l'identique (perso connecté).</div>`)
        + '<div id="spRes"></div>';
    let chosen = 0;
    const inp = $('#spName'), drop = inp.parentElement.querySelector('.drop');
    inp.oninput = debounce(async () => {
        chosen = 0;
        if (!inp.value.trim()) { drop.hidden = true; return; }
        try {
            const r = await api('players', { q: inp.value.trim() });
            drop.innerHTML = r.players.map(p => `<div data-id="${p.id}" data-n="${esc(p.name)}">${esc(p.name)} <span class="muted">${esc(p.account || '')} · niv. ${p.level || '?'}</span></div>`).join('') || '<div class="muted">Aucun</div>';
            drop.hidden = false;
            $$('[data-id]', drop).forEach(d => d.onclick = () => { chosen = +d.dataset.id; inp.value = d.dataset.n; drop.hidden = true; });
        } catch { }
    });
    $('#spNow').onclick = async () => { const r = await act(api('support_copy_now', {}, {}), 'Copie faite'); if (r) tabSupport(); };
    $('#spGo').onclick = () => { if (!chosen) { toast('Choisis un personnage dans la liste', true); return; } supportCompare(chosen, $('#spCopy').value); };
}

async function supportCompare(id, copy) {
    let r;
    try { r = await api('support_compare', { id, copy }); } catch (e) { toast(e.message, true); return; }
    const name = i => (r.items[i.ItemId] || {}).name || '#' + i.ItemId;
    const fx = l => l ? l.map(e => effectLabel(e.id, ...(e.v || []))).filter(Boolean).join(' · ') : '';
    const kTot = x => x ? (+x.Kamas) + (+x.BankKamas) : 0;
    const row = (i, give) => `<tr><td>${i.diff} × ${esc(name(i))}<div class="muted" style="font-size:12px">${esc(fx(i.effects))}</div></td><td>${i.rows.map(x => x.where).filter((v, k, a) => a.indexOf(v) === k).join(', ')}</td>
        <td class="act">${give ? `<button class="small" data-give="${i.rows[0].id}" data-where="${i.rows[0].where}">Rendre</button>` : ''}</td></tr>`;
    await effectsList().catch(() => { });
    $('#spRes').innerHTML =
        card(`${esc((r.now || r.then).Name)} — copie du ${copy.slice(6, 8)}/${copy.slice(4, 6)} ${copy.slice(8, 10)}h${copy.slice(10, 12)}`, `<div class="kv">
            <span>Kamas (perso + banque)</span><span>${fmtK(kTot(r.then))} → <b>${fmtK(kTot(r.now))}</b> (${kTot(r.now) - kTot(r.then) >= 0 ? '+' : ''}${fmtK(kTot(r.now) - kTot(r.then))})</span>
            <span>Expérience</span><span>${fmtK(r.then ? r.then.Experience : 0)} → ${fmtK(r.now ? r.now.Experience : 0)}</span></div>
          ${r.then && r.now && kTot(r.now) < kTot(r.then) ? `<div class="btns" style="margin-top:8px"><button id="spKamas">Rendre ${fmtK(kTot(r.then) - kTot(r.now))} kamas</button></div>` : ''}`)
        + card(`Disparus depuis la copie (${r.lost.length})`, r.lost.length ? `<table>${r.lost.map(i => row(i, true)).join('')}</table>` : '<div class="muted">Rien n\'a disparu.</div>')
        + card(`Apparus depuis la copie (${r.gained.length})`, r.gained.length ? `<table>${r.gained.map(i => row(i, false)).join('')}</table>` : '<div class="muted">Rien de nouveau.</div>')
        + card(`Échanges depuis la copie (${r.trades.length})`, r.trades.length ? `<table>${r.trades.map(t => { const d = JSON.parse(t.Details || '{}'); return `<tr><td>${esc(t.At)}</td><td>${esc(t.Name)} ↔ ${esc(d.avec || '?')}</td><td>${fmtK(d.donneKamas)} / ${fmtK(d.recoitKamas)} kamas</td></tr>`; }).join('')}</table>` : '<div class="muted">Aucun.</div>');
    $$('#spRes [data-give]').forEach(b => b.onclick = async () => {
        if (!confirm('Rendre cet objet (mêmes jets) au personnage ? Il doit être connecté.')) return;
        const res = await act(api('support_give', {}, { characterId: id, copy, rowId: +b.dataset.give, where: b.dataset.where }));
        if (res) toast(res.message);
    });
    if ($('#spKamas')) $('#spKamas').onclick = () => { if (confirm('Rendre la différence de kamas ?')) cmd(`kamas ${kTot(r.then) - kTot(r.now)} ${(r.now || r.then).Name}`); };
}

TABS.server = tabServer;
TABS.watch = tabWatch;
TABS.support = tabSupport;
