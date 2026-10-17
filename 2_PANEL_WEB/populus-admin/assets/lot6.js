/* Panel admin Populus 2.51 : panoplies (bonus par nombre d'objets portés), vue d'une zone (toutes ses maps en images)
   et aperçu dessiné des zones d'effet des sorts. */
'use strict';

/* ============================================================ PANOPLIES */
let SET = null;
function tabSets() {
    $('#main').innerHTML = `<div class="cols">
      <div class="card"><h3>🧩 Panoplies</h3><input id="setq" placeholder="Nom ou numéro de la panoplie">
        <div id="setlist" class="list" style="margin-top:8px;max-height:68vh"></div></div>
      <div id="setdet"><div class="card empty" style="margin:0">Cherche une panoplie par son nom.</div></div></div>`;
    const load = async () => {
        const q = valOf('setq');
        if (!q) { $('#setlist').innerHTML = '<div class="muted">Tape au moins une lettre.</div>'; return; }
        try {
            const r = await api('sets', { q });
            $('#setlist').innerHTML = r.sets.length ? r.sets.map(s => `<div class="it" data-id="${s.Id}"><span>${esc(s.Name || '(sans nom)')}</span><small>${s.Count} objets · #${s.Id}</small></div>`).join('')
                : '<div class="muted">Aucune panoplie.</div>';
            $$('#setlist .it').forEach(el => el.onclick = () => { $$('#setlist .it').forEach(x => x.classList.remove('on')); el.classList.add('on'); setDetail(+el.dataset.id); });
        } catch (e) { toast(e.message, true); }
    };
    $('#setq').oninput = debounce(load);
    effectsList().catch(() => { });
    if (SET) setDetail(SET.set.Id);
}

async function setDetail(id) {
    try { SET = await api('set', { id }); } catch (e) { toast(e.message, true); return; }
    await effectsList().catch(() => { });
    if (!$('#setdet')) return;
    const n = SET.items.length;
    // une étape par nombre d'objets portés (1 à n), même vide
    const steps = Array.from({ length: Math.max(n, (SET.effects || []).length) }, (_, i) => ((SET.effects || [])[i] || []).map(e => ({ ...e })));
    const opts = sel => Object.entries(EFFECTS || {}).filter(([eid, e]) => e.tooltip || +eid === +sel).map(([eid, e]) =>
        `<option value="${eid}" ${+eid === +sel ? 'selected' : ''}>#${eid} ${esc(fxText(e.text || '', '#', '#').slice(0, 60) || 'effet')}</option>`).join('');
    const stepHtml = (st, k) => `<div class="card" style="margin:8px 0;padding:10px">
        <div class="row" style="justify-content:space-between"><b>${k + 1} objet${k ? 's' : ''} porté${k ? 's' : ''}</b><button class="small" data-add="${k}">+ Bonus</button></div>
        ${st.length ? `<table>${st.map((e, j) => `<tr>
          <td><select data-s="${k}" data-j="${j}" data-k="effectId" style="max-width:280px">${opts(e.effectId)}</select></td>
          <td><input data-s="${k}" data-j="${j}" data-k="value" value="${esc(e.value)}" style="width:70px"></td>
          <td class="muted">${esc(effectLabel(e.effectId, e.value, null, null, SET.effectTexts))}</td>
          <td class="act"><button class="small danger" data-del="${k}" data-j="${j}">✕</button></td></tr>`).join('')}</table>` : '<div class="muted">Aucun bonus.</div>'}</div>`;
    const draw = () => {
        $('#setfx').innerHTML = steps.map(stepHtml).join('');
        $$('#setfx [data-s]').forEach(x => x.onchange = () => { steps[+x.dataset.s][+x.dataset.j][x.dataset.k] = +x.value || 0; draw(); });
        $$('#setfx [data-add]').forEach(b => b.onclick = () => { steps[+b.dataset.add].push({ effectId: 118, value: 10 }); draw(); });
        $$('#setfx [data-del]').forEach(b => b.onclick = () => { steps[+b.dataset.del].splice(+b.dataset.j, 1); draw(); });
    };
    $('#setdet').innerHTML =
        card(`🧩 ${esc(SET.set.Name || '(sans nom)')} <span class="muted">#${SET.set.Id}${+SET.set.BonusIsSecret ? ' · bonus secrets' : ''}</span>`,
            `<b>Objets (${n})</b>${SET.items.map(i => `<div class="it" data-item="${i.Id}"><span>${esc(i.Name || '#' + i.Id)}</span><small>${i.Level ? 'niv. ' + i.Level + ' · ' : ''}#${i.Id}</small></div>`).join('') || '<div class="muted">aucun</div>'}`)
        + card('✨ Bonus de panoplie', SET.effectsError ? `<p class="err">Bonus illisibles : ${esc(SET.effectsError)}</p>` : `<div id="setfx"></div>
            <div class="row" style="margin-top:8px;justify-content:flex-end"><button class="pri" id="setSave">💾 Enregistrer les bonus</button></div>
            <div class="help">Les bonus d'une ligne s'appliquent quand le joueur porte exactement ce nombre d'objets de la panoplie (ils ne s'additionnent pas d'une ligne à l'autre).
              Pris en compte au prochain changement d'équipement du joueur.</div>`);
    $$('#setdet [data-item]').forEach(x => x.onclick = () => { showTab('items'); setTimeout(() => itemDetail(+x.dataset.item), 300); });
    if (SET.effectsError) return;
    draw();
    $('#setSave').onclick = async () => {
        if (await act(api('set_effects_save', {}, { id: SET.set.Id, effects: steps }), 'Bonus enregistrés')) setDetail(SET.set.Id);
    };
}

// fiche d'un objet : lien vers sa panoplie
const itemDetailBase = itemDetail;
itemDetail = async function (id) {
    await itemDetailBase(id);
    if (!ITEM || !ITEM.set || !$('#idet') || $('#idetSet')) return;
    const box = document.createElement('div');
    box.id = 'idetSet';
    box.innerHTML = card('🧩 Panoplie', `<div class="row" style="justify-content:space-between"><span>${esc(ITEM.set.Name || '#' + ITEM.set.Id)}</span><button id="openSet">Modifier la panoplie et ses bonus →</button></div>`);
    $('#idet').firstElementChild.after(...box.childNodes);
    $('#openSet').onclick = () => { SET = { set: { Id: ITEM.set.Id } }; showTab('sets'); };
};

TABS.sets = tabSets;

/* ============================================================ VUE D'UNE ZONE */
let ZONE_TIMER = null;
const renderMapSideBase = renderMapSide;
renderMapSide = function () {
    renderMapSideBase();
    const p = MAP && MAP.pos;
    if (!p || !p.subAreaId || !$('#mapTp')) return;
    const b = document.createElement('button');
    b.textContent = '🧩 Toutes les maps de la zone';
    b.onclick = () => openZone(p.subAreaId);
    $('#mapTp').after(b);
};

async function openZone(sub) {
    clearTimeout(ZONE_TIMER);
    let r;
    try { r = await api('subarea_maps', { id: sub }); } catch (e) { toast(e.message, true); return; }
    if (!$('#mapMain')) return;
    const TW = 160, TH = 110;                       // vignette (image du rendu 638×438 réduite)
    const worlds = [...new Set(r.maps.map(m => m.world))];
    const block = w => {
        const ms = r.maps.filter(m => m.world === w);
        const xs = ms.map(m => m.x), ys = ms.map(m => m.y);
        const x0 = Math.min(...xs), y0 = Math.min(...ys), cols = Math.max(...xs) - x0 + 1, rows = Math.max(...ys) - y0 + 1;
        // deux maps aux mêmes coordonnées (intérieur / extérieur) : décalées dans la case
        const seen = {};
        return `<div class="zonegrid" style="position:relative;width:${cols * TW}px;height:${rows * TH}px">${ms.map(m => {
            const k = m.x + ',' + m.y, dup = seen[k] = (seen[k] || 0) + 1;
            const left = (m.x - x0) * TW + (dup - 1) * 10, top = (m.y - y0) * TH + (dup - 1) * 10;
            return `<div class="zm${MAP && MAP.map.Id === m.id ? ' on' : ''}" data-open="${m.id}" title="map ${m.id} [${m.x},${m.y}]${m.outdoor ? '' : ' intérieur'}"
                style="left:${left}px;top:${top}px;width:${TW - 2}px;height:${TH - 2}px">
                ${m.url ? `<img src="${m.url}" loading="lazy">` : `<span class="muted">${m.status === 'absent' ? 'pas d\'image' : 'rendu en cours…'}</span>`}
                <small>[${m.x},${m.y}]${m.mobs ? ' 🐾' + m.mobs : ''}${m.npcs ? ' 🧙' + m.npcs : ''}</small></div>`;
        }).join('')}</div>`;
    };
    $('#mapMain').innerHTML = card(`🧩 ${esc(r.name || 'Zone')} <span class="muted">#${sub} · ${r.maps.length} maps</span>`,
        `<div class="row" style="justify-content:space-between;margin-bottom:8px"><span class="muted">Clique une map pour l'ouvrir.</span>${MAP ? '<button id="zoneBack">← Revenir à la map</button>' : ''}</div>
         <div style="overflow:auto;max-height:75vh">${worlds.map(w => (worlds.length > 1 ? `<div class="muted" style="margin:6px 0">monde ${w}</div>` : '') + block(w)).join('')}</div>`);
    $$('#mapMain [data-open]').forEach(x => x.onclick = () => openMap(+x.dataset.open));
    if ($('#zoneBack')) $('#zoneBack').onclick = () => renderMapMain();
    // images encore en cours de rendu : on revient voir
    if (r.maps.some(m => !m.url && m.status === 'demande'))
        ZONE_TIMER = setTimeout(() => { if ($('#mapMain .zonegrid')) openZone(sub); }, 4000);
}

/* ============================================================ APERÇU DES ZONES DE SORTS */
/** Cases touchées autour de la cible (dx, dy en coordonnées de carte), lanceur vers -x. Approximation des formes du client. */
function zoneCells(shape, size, minSize) {
    const c = String.fromCharCode(+shape || 80), out = [];
    size = Math.max(0, +size || 0); minSize = Math.max(0, +minSize || 0);
    const R = Math.min(size, 8);
    for (let dx = -8; dx <= 8; dx++) for (let dy = -8; dy <= 8; dy++) {
        const ax = Math.abs(dx), ay = Math.abs(dy), man = ax + ay, che = Math.max(ax, ay);
        let hit;
        switch (c) {
            case 'P': hit = !dx && !dy; break;
            case 'C': hit = man <= R && man >= minSize; break;
            case 'O': hit = man === R; break;
            case 'D': hit = man <= R && man % 2 === 0; break;
            case 'X': hit = (!dx || !dy) && man <= R && man >= minSize; break;
            case 'Q': hit = (!dx || !dy) && man <= R && man > Math.max(0, minSize - 1) && man > 0; break;
            case '+': hit = ax === ay && ax <= R && ax >= minSize; break;
            case '#': hit = ax === ay && ax <= R && ax > 0; break;
            case '*': hit = ((!dx || !dy) || ax === ay) && che <= R; break;
            case 'G': hit = che <= R; break;
            case 'W': hit = che === R; break;
            case 'L': hit = !dy && dx >= 0 && dx <= R; break;
            case 'T': case '-': hit = !dx && ay <= R; break;
            case '/': hit = dx === dy && dx >= 0 && dx <= R; break;
            case 'V': hit = dx >= 0 && dx <= R && ay <= dx; break;
            case 'U': hit = man <= R && dx >= 0; break;
            case 'R': hit = ax <= R && ay <= Math.max(0, minSize); break;
            case 'A': case 'a': hit = true; break;
            default: hit = !dx && !dy;
        }
        if (hit) out.push([dx, dy]);
    }
    return out;
}

function drawZone(canvas, shape, size, minSize) {
    const ctx = canvas.getContext('2d'), W = canvas.width, H = canvas.height, hw = 9, hh = 4.5, N = 6;
    ctx.clearRect(0, 0, W, H);
    const hits = new Set(zoneCells(shape, size, minSize).map(([x, y]) => x + ',' + y));
    const css = getComputedStyle(document.body);
    const line = css.getPropertyValue('--line').trim() || '#8884', acc = css.getPropertyValue('--acc').trim() || '#e8a33d';
    const poly = (cx, cy) => { ctx.beginPath(); ctx.moveTo(cx, cy - hh); ctx.lineTo(cx + hw, cy); ctx.lineTo(cx, cy + hh); ctx.lineTo(cx - hw, cy); ctx.closePath(); };
    for (let dx = -N; dx <= N; dx++) for (let dy = -N; dy <= N; dy++) {
        const cx = W / 2 + (dx - dy) * hw, cy = H / 2 + (dx + dy) * hh;
        poly(cx, cy);
        const k = dx + ',' + dy;
        if (hits.has(k)) { ctx.fillStyle = acc; ctx.globalAlpha = dx || dy ? .55 : .9; ctx.fill(); ctx.globalAlpha = 1; }
        if (dx === -3 && dy === 0) { ctx.fillStyle = '#3b82f6'; ctx.fill(); }
        ctx.strokeStyle = line; ctx.stroke();
    }
}

const spellDetailBase = spellDetail;
spellDetail = async function (id) {
    await spellDetailBase(id);
    if (!$('#sdet') || !SPELL || !SPELL.levels.length) return;
    const box = document.createElement('div');
    box.innerHTML = card('👁 Aperçu des zones', `<div id="zprev" class="row" style="flex-wrap:wrap;gap:10px"></div>
        <div class="help">Bleu = lanceur, orange = cases touchées autour de la cible. Se met à jour quand tu changes la forme ou la taille (avant d'enregistrer).</div>`);
    $('#sdet').append(...box.childNodes);
    const refresh = () => {
        if (!$('#zprev')) return;
        const rows = [];
        ['effects', 'critical'].forEach(kind => {
            $$(`#sfx_${kind} select[data-k="zoneShape"]`).forEach(sel => {
                const n = sel.dataset.x, size = $(`#sfx_${kind} input[data-x="${n}"][data-k="zoneSize"]`);
                const eff = $(`#sfx_${kind} select[data-x="${n}"][data-k="effectId"]`);
                rows.push({ kind, shape: +sel.value, size: size ? size.value : 0, label: eff ? eff.selectedOptions[0]?.textContent || '' : '' });
            });
        });
        // une vignette par zone différente
        const uniq = [];
        rows.forEach(r => { if (!uniq.some(u => u.shape === r.shape && +u.size === +r.size)) uniq.push(r); });
        $('#zprev').innerHTML = uniq.length ? uniq.map((r, i) => `<div style="text-align:center"><canvas id="zc${i}" width="230" height="125"></canvas>
            <div class="muted" style="font-size:12px;max-width:230px">${esc(String.fromCharCode(r.shape))} taille ${esc(r.size)}${r.kind === 'critical' ? ' (critique)' : ''} · ${esc(r.label.slice(0, 40))}</div></div>`).join('')
            : '<div class="muted">Aucun effet.</div>';
        uniq.forEach((r, i) => drawZone($('#zc' + i), r.shape, r.size, 0));
    };
    refresh();
    $('#sdet').onchange = () => setTimeout(refresh, 0);   // une seule écoute même après plusieurs ouvertures
};
