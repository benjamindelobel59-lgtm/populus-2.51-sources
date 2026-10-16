/* Panel admin Populus 2.51 : onglets Objets et Sorts (caractéristiques, effets, niveaux, critiques). */
'use strict';

let EFFECTS = null;          // textes de tous les effets (chargés une fois)
async function effectsList() {
    if (!EFFECTS) EFFECTS = (await api('effects_list')).effects;
    return EFFECTS;
}

/** « #1{~1~2 à }#2 Force » → « 10 à 20 Force ». */
function fxText(tpl, a, b, c) {
    if (!tpl) return '';
    const p = { 1: a, 2: b, 3: c };
    const has = n => p[n] !== undefined && p[n] !== null && p[n] !== '' && +p[n] !== 0;
    let s = tpl.replace(/\{~([0-9~]+)([^}]*)\}/g, (_, need, txt) => need.split('~').filter(Boolean).every(has) ? txt : '');
    s = s.replace(/#([123])/g, (_, n) => has(n) ? p[n] : '');
    return s.replace(/\s+/g, ' ').trim();
}

function effectLabel(id, a, b, c, texts) {
    const t = (texts && texts[id]) || (EFFECTS && EFFECTS[id]);
    return t && t.text ? fxText(t.text, a, b, c) : 'effet #' + id;
}

/* ============================================================ OBJETS */
const IFIELDS = [['Level', 'Niveau'], ['Price', 'Prix'], ['Weight', 'Poids'], ['RealWeight', 'Poids réel'], ['AppearanceId', 'Apparence'], ['CraftXpRatio', 'XP craft %']];
const IFLAGS = [['Usable', 'Utilisable'], ['Targetable', 'Ciblable'], ['Etheral', 'Éthéré'], ['Cursed', 'Maudit'], ['TwoHanded', 'Deux mains'],
    ['IsLinkedToOwner', 'Lié au compte'], ['HideEffects', 'Effets cachés'], ['BonusIsSecret', 'Bonus secret']];
const WFIELDS = [['ApCost', 'PA'], ['MinRange', 'Portée min'], ['WeaponRange', 'Portée max'], ['CriticalHitProbability', 'Critique (1/x)'],
    ['CriticalHitBonus', 'Bonus critique'], ['CriticalFailureProbability', 'Échec (1/x)'], ['MaxCastPerTurn', 'Utilisations / tour']];
const WFLAGS = [['CastInLine', 'En ligne'], ['CastInDiagonal', 'En diagonale'], ['CastTestLos', 'Ligne de vue']];

let ITEM = null;
function tabItems() {
    $('#main').innerHTML = `<div class="cols">
      <div class="card"><h3>🎒 Objets</h3><input id="iq" placeholder="Nom ou numéro de l'objet">
        <div id="ilist" class="list" style="margin-top:8px;max-height:68vh"></div></div>
      <div id="idet"><div class="card empty" style="margin:0">Cherche un objet par son nom.</div></div></div>`;
    const load = async () => {
        const q = valOf('iq');
        if (!q) { $('#ilist').innerHTML = '<div class="muted">Tape au moins une lettre.</div>'; return; }
        try {
            const r = await api('items', { q });
            $('#ilist').innerHTML = r.items.length ? r.items.map(i => `<div class="it" data-id="${i.Id}"><span>${esc(i.Name || '(sans nom)')}${+i.IsWeapon ? ' <span class="pill">arme</span>' : ''}</span>
                <small>niv. ${i.Level} · ${esc(i.TypeName || '')} · #${i.Id}</small></div>`).join('') : '<div class="muted">Aucun objet.</div>';
            $$('#ilist .it').forEach(el => el.onclick = () => { $$('#ilist .it').forEach(x => x.classList.remove('on')); el.classList.add('on'); itemDetail(+el.dataset.id); });
        } catch (e) { toast(e.message, true); }
    };
    $('#iq').oninput = debounce(load);
    effectsList().catch(() => { });
    if (ITEM) itemDetail(ITEM.item.Id);
}

async function itemDetail(id) {
    try { ITEM = await api('item', { id }); } catch (e) { toast(e.message, true); return; }
    await effectsList().catch(() => { });
    const it = ITEM.item, weapon = ITEM.table === 'items_templates_weapons';
    let fx = (ITEM.effects || []).map(e => ({ ...e }));
    const field = ([k, l]) => `<label>${l}<input data-f="${k}" value="${esc(it[k] ?? '')}"></label>`;
    const flag = ([k, l]) => `<label class="row" style="gap:4px"><input type="checkbox" data-f="${k}" ${+it[k] ? 'checked' : ''}>${l}</label>`;
    const effectOptions = sel => Object.entries(EFFECTS || {}).filter(([, e]) => e.tooltip || +sel).map(([id, e]) =>
        `<option value="${id}" ${+id === +sel ? 'selected' : ''}>#${id} ${esc(fxText(e.text || '', '#', '#') || 'effet')}</option>`).join('');
    const fxTable = () => fx.length ? `<table><tr><th>Effet</th><th>Min</th><th>Max</th><th>Aperçu</th><th>Visible</th><th></th></tr>${fx.map((e, n) => `<tr>
        <td><select data-x="${n}" data-k="effectId" style="max-width:260px">${effectOptions(e.effectId)}</select></td>
        <td><input data-x="${n}" data-k="min" value="${esc(e.min ?? '')}" style="width:70px"></td>
        <td><input data-x="${n}" data-k="max" value="${esc(e.max ?? '')}" style="width:70px"></td>
        <td class="muted">${esc(effectLabel(e.effectId, e.min, e.max, e.value, ITEM.effectTexts))}</td>
        <td><input type="checkbox" data-x="${n}" data-k="visible" ${e.visible === false ? '' : 'checked'}></td>
        <td class="act"><button class="small danger" data-xdel="${n}">✕</button></td></tr>`).join('')}</table>` : '<div class="muted">Aucun effet.</div>';

    $('#idet').innerHTML =
        card(`🎒 ${esc(ITEM.name)} <span class="muted">#${it.Id} · ${esc(ITEM.type ? ITEM.type.Name : '')}${ITEM.set ? ' · panoplie ' + esc(ITEM.set.Name) : ''}</span>`, `
          ${ITEM.description ? `<p class="muted" style="margin-bottom:8px">${esc(ITEM.description)}</p>` : ''}
          <div class="grid">${IFIELDS.map(field).join('')}${weapon ? WFIELDS.map(field).join('') : ''}
            <label style="grid-column:span 2">Conditions pour l'équiper<input data-f="Criteria" value="${esc(it.Criteria || '')}" placeholder="ex. PL>59&CS>100"></label></div>
          <div class="btns" style="margin-top:8px">${IFLAGS.map(flag).join('')}${weapon ? WFLAGS.map(flag).join('') : ''}</div>
          <div class="row" style="margin-top:10px;justify-content:space-between">
            <span class="fit"><input id="igQty" value="1" style="width:60px"><button id="igGive">Me le donner</button><button id="igGiveMax">Me le donner (jet max)</button></span>
            <button class="pri" id="itemSave">💾 Enregistrer</button></div>`)
        + card('✨ Effets', `${ITEM.effectsError ? `<p class="err">Effets illisibles : ${esc(ITEM.effectsError)}</p>` : `<div id="ifx">${fxTable()}</div>
            <div class="row" style="margin-top:8px;justify-content:space-between"><button id="ifxAdd">+ Ajouter un effet</button><button class="pri" id="ifxSave">💾 Enregistrer les effets</button></div>
            <div class="help">Min / max = fourchette du jet. Les objets déjà possédés par les joueurs gardent leur jet ; les nouveaux (drop, craft, achat) prennent la nouvelle fourchette.</div>`}`)
        + card('🔎 Où le trouver', `<div class="cols" style="grid-template-columns:1fr 1fr">
            <div><b>Monstres</b>${ITEM.drops.length ? ITEM.drops.map(d => `<div class="it" data-mob="${d.MonsterOwnerId}"><span>${esc(d.Name || '#' + d.MonsterOwnerId)}</span><small>${d.DropRateForGrade1}–${d.DropRateForGrade5} %</small></div>`).join('') : '<div class="muted">aucun</div>'}</div>
            <div><b>Boutiques de PNJ</b>${ITEM.shops.length ? ITEM.shops.map(s => `<div class="it" data-npc="${s.NpcId}"><span>${esc(s.Name || '#' + s.NpcId)}</span><small>${s.CustomPrice ?? 'prix normal'}</small></div>`).join('') : '<div class="muted">aucune</div>'}</div></div>`);

    const bindFx = () => {
        $$('#ifx [data-x]').forEach(x => x.onchange = () => {
            const e = fx[+x.dataset.x];
            e[x.dataset.k] = x.type === 'checkbox' ? x.checked : x.value;
            $('#ifx').innerHTML = fxTable(); bindFx();
        });
        $$('#ifx [data-xdel]').forEach(b => b.onclick = () => { fx.splice(+b.dataset.xdel, 1); $('#ifx').innerHTML = fxTable(); bindFx(); });
    };
    if (!ITEM.effectsError) {
        bindFx();
        $('#ifxAdd').onclick = () => { fx.push({ i: -1, effectId: 118, min: 1, max: 10, visible: true }); $('#ifx').innerHTML = fxTable(); bindFx(); };
        $('#ifxSave').onclick = async () => {
            if (await act(api('item_effects_save', {}, { id: it.Id, effects: fx }), 'Effets enregistrés')) itemDetail(it.Id);
        };
    }
    $('#itemSave').onclick = async () => {
        const fields = {};
        $$('#idet [data-f]').forEach(x => fields[x.dataset.f] = x.type === 'checkbox' ? (x.checked ? 1 : 0) : x.value.trim());
        if (await act(api('item_save', {}, { id: it.Id, fields }), 'Objet enregistré')) itemDetail(it.Id);
    };
    $('#igGive').onclick = () => cmd(`item add ${it.Id} moi ${+valOf('igQty') || 1}`);
    $('#igGiveMax').onclick = () => cmd(`item add ${it.Id} moi ${+valOf('igQty') || 1} true`);
    $$('#idet [data-mob]').forEach(x => x.onclick = () => { showTab('mobs'); setTimeout(() => mobDetail(+x.dataset.mob), 300); });
    $$('#idet [data-npc]').forEach(x => x.onclick = () => { showTab('npcs'); setTimeout(() => openNpc(+x.dataset.npc), 300); });
}

/* ============================================================ SORTS */
const SFIELDS = [['ApCost', 'PA'], ['MinRange', 'Portée min'], ['Range', 'Portée max'], ['CriticalHitProbability', 'Critique (1/x)'],
    ['CriticalFailureProbability', 'Échec (1/x)'], ['MaxCastPerTurn', 'Lancers / tour'], ['MaxCastPerTarget', 'Lancers / cible'],
    ['MinCastInterval', 'Relance (tours)'], ['InitialCooldown', 'Relance initiale'], ['GlobalCooldown', 'Relance globale'], ['MaxStack', 'Cumul max'], ['MinPlayerLevel', 'Niveau requis']];
const SFLAGS = [['RangeCanBeBoosted', 'Portée modifiable'], ['CastInLine', 'En ligne'], ['CastInDiagonal', 'En diagonale'], ['CastTestLos', 'Ligne de vue'],
    ['NeedFreeCell', 'Case libre'], ['NeedTakenCell', 'Case occupée'], ['NeedFreeTrapCell', 'Sans piège'], ['CriticalFailureEndsTurn', 'Échec = fin du tour'], ['Hidden', 'Caché']];
const SHAPES = [['P', 'point'], ['C', 'cercle'], ['L', 'ligne'], ['T', 'ligne perpendiculaire'], ['X', 'croix'], ['+', 'croix diagonale'], ['D', 'damier'],
    ['O', 'anneau'], ['Q', 'croix vide'], ['G', 'carré'], ['V', 'cône'], ['W', 'carré vide'], ['#', 'diagonales'], ['-', 'ligne perp.'], ['/', 'ligne diag.'], ['U', 'demi-cercle'], ['A', 'tout'], ['a', 'tout (alliés)'], ['R', 'rectangle'], ['*', 'étoile']];

let SPELL = null, SPELL_LEVEL = 0;
async function tabSpells() {
    let breeds = [];
    try { breeds = (await api('breeds')).breeds; } catch { }
    $('#main').innerHTML = `<div class="cols">
      <div class="card"><h3>✨ Sorts</h3>
        <select id="sbreed"><option value="">— sorts d'une classe —</option>${breeds.map(b => `<option value="${b.Id}">${esc(b.Name)}</option>`).join('')}</select>
        <input id="sq" placeholder="ou nom / numéro du sort" style="margin-top:6px">
        <div id="slist" class="list" style="margin-top:8px;max-height:62vh"></div></div>
      <div id="sdet"><div class="card empty" style="margin:0">Choisis une classe ou cherche un sort.</div></div></div>`;
    const load = async params => {
        try {
            const r = await api('spells', params);
            $('#slist').innerHTML = r.spells.length ? r.spells.map(s => `<div class="it" data-id="${s.Id}"><span>${esc(s.Name || '(sans nom)')}</span><small>${s.ObtainLevel ? 'niv. ' + s.ObtainLevel + ' · ' : ''}#${s.Id}</small></div>`).join('')
                : '<div class="muted">Aucun sort.</div>';
            $$('#slist .it').forEach(el => el.onclick = () => { $$('#slist .it').forEach(x => x.classList.remove('on')); el.classList.add('on'); SPELL_LEVEL = 0; spellDetail(+el.dataset.id); });
        } catch (e) { toast(e.message, true); }
    };
    $('#sbreed').onchange = () => { $('#sq').value = ''; if ($('#sbreed').value) load({ breed: $('#sbreed').value }); };
    $('#sq').oninput = debounce(() => { $('#sbreed').value = ''; if (valOf('sq')) load({ q: valOf('sq') }); });
    effectsList().catch(() => { });
    if (SPELL) spellDetail(SPELL.spell.Id);
}

async function spellDetail(id) {
    try { SPELL = await api('spell', { id }); } catch (e) { toast(e.message, true); return; }
    await effectsList().catch(() => { });
    const s = SPELL;
    if (!s.levels.length) { $('#sdet').innerHTML = card(esc(s.name), '<div class="muted">Ce sort n\'a aucun niveau.</div>'); return; }
    SPELL_LEVEL = Math.min(SPELL_LEVEL, s.levels.length - 1);
    const lv = s.levels[SPELL_LEVEL];
    const fx = { effects: (lv.fx ? lv.fx.effects : []).map(e => ({ ...e })), critical: (lv.fx ? lv.fx.critical : []).map(e => ({ ...e })) };
    const shapeSel = (n, kind, v) => `<select data-x="${n}" data-kind="${kind}" data-k="zoneShape" style="width:120px">${SHAPES.map(([c, l]) => `<option value="${c.charCodeAt(0)}" ${+v === c.charCodeAt(0) ? 'selected' : ''}>${c} ${l}</option>`).join('')}
        ${SHAPES.some(([c]) => c.charCodeAt(0) === +v) ? '' : `<option value="${v}" selected>${String.fromCharCode(v)}</option>`}</select>`;
    const effOpts = sel => Object.entries(EFFECTS || {}).map(([eid, e]) => `<option value="${eid}" ${+eid === +sel ? 'selected' : ''}>#${eid} ${esc(fxText(e.text || '', '#', '#').slice(0, 60) || 'effet')}</option>`).join('');
    const table = kind => fx[kind].length ? `<div style="overflow-x:auto"><table><tr><th>Effet</th><th>Min</th><th>Max</th><th>Durée</th><th>Délai</th><th>% aléa</th><th>Zone</th><th>Taille</th><th>Cibles</th><th>Aperçu</th><th></th></tr>
        ${fx[kind].map((e, n) => `<tr>
          <td><select data-x="${n}" data-kind="${kind}" data-k="effectId" style="width:230px">${effOpts(e.effectId)}</select></td>
          ${['min', 'max', 'duration', 'delay', 'random'].map(k => `<td><input data-x="${n}" data-kind="${kind}" data-k="${k}" value="${esc(e[k])}" style="width:56px"></td>`).join('')}
          <td>${shapeSel(n, kind, e.zoneShape)}</td>
          <td><input data-x="${n}" data-kind="${kind}" data-k="zoneSize" value="${esc(e.zoneSize)}" style="width:48px"></td>
          <td><input data-x="${n}" data-kind="${kind}" data-k="targetMask" value="${esc(e.targetMask || '')}" style="width:110px" title="A = ennemis, a = alliés, C = lanceur…"></td>
          <td class="muted">${esc(effectLabel(e.effectId, e.min, e.max, e.value, s.effectTexts))}${e.duration ? ' (' + e.duration + ' tour' + (e.duration > 1 ? 's' : '') + ')' : ''}</td>
          <td class="act"><button class="small danger" data-xdel="${n}" data-kind="${kind}">✕</button></td></tr>`).join('')}</table></div>`
        : '<div class="muted">Aucun effet.</div>';

    const field = ([k, l]) => `<label>${l}<input data-f="${k}" value="${esc(lv[k] ?? '')}"></label>`;
    const flag = ([k, l]) => `<label class="row" style="gap:4px"><input type="checkbox" data-f="${k}" ${+lv[k] ? 'checked' : ''}>${l}</label>`;
    $('#sdet').innerHTML =
        card(`✨ ${esc(s.name)} <span class="muted">#${s.spell.Id}${s.breeds.length ? ' · ' + s.breeds.map(b => esc(b.breed) + ' niv. ' + b.level).join(', ') : ''}${s.monsters ? ' · utilisé par ' + s.monsters + ' grade(s) de monstre' : ''}</span>`, `
          ${s.description ? `<p class="muted" style="margin-bottom:8px">${esc(s.description)}</p>` : ''}
          <div class="ltabs" style="margin-bottom:10px">${s.levels.map((l, i) => `<button data-lv="${i}" class="${i === SPELL_LEVEL ? 'on' : ''}">Niveau ${i + 1}${l.MinPlayerLevel ? ' (perso ' + l.MinPlayerLevel + ')' : ''}</button>`).join('')}</div>
          <div class="grid">${SFIELDS.map(field).join('')}</div>
          <div class="btns" style="margin-top:8px">${SFLAGS.map(flag).join('')}</div>
          <div class="row" style="margin-top:10px;justify-content:flex-end"><button class="pri" id="slSave">💾 Enregistrer ce niveau</button></div>`)
        + card('Effets', (lv.fx ? `<div id="sfx_effects">${table('effects')}</div>` : '<p class="err">Effets illisibles (serveur de jeu injoignable ?)</p>')
            + `<div class="row" style="margin-top:8px;justify-content:space-between"><button data-xadd="effects">+ Effet</button><button class="pri" data-xsave="effects">💾 Enregistrer les effets</button></div>`)
        + card('Effets en coup critique', (lv.fx ? `<div id="sfx_critical">${table('critical')}</div>` : '')
            + `<div class="row" style="margin-top:8px;justify-content:space-between"><button data-xadd="critical">+ Effet</button><button class="pri" data-xsave="critical">💾 Enregistrer les effets critiques</button></div>
            <div class="help">Appliqué tout de suite aux prochains lancers, pour les joueurs comme pour les monstres qui ont ce sort.</div>`);

    const redraw = kind => { $('#sfx_' + kind).innerHTML = table(kind); bind(); };
    const bind = () => {
        $$('#sdet [data-x]').forEach(x => x.onchange = () => {
            fx[x.dataset.kind][+x.dataset.x][x.dataset.k] = x.value;
            redraw(x.dataset.kind);
        });
        $$('#sdet [data-xdel]').forEach(b => b.onclick = () => { fx[b.dataset.kind].splice(+b.dataset.xdel, 1); redraw(b.dataset.kind); });
    };
    bind();
    $$('#sdet [data-lv]').forEach(b => b.onclick = () => { SPELL_LEVEL = +b.dataset.lv; spellDetail(s.spell.Id); });
    $$('#sdet [data-xadd]').forEach(b => b.onclick = () => {
        const k = b.dataset.xadd;
        const base = fx[k][0] || fx.effects[0] || { zoneShape: 80, zoneSize: 0, targetMask: 'A,a', triggers: 'I' };
        fx[k].push({ ...base, i: -1, effectId: 100, min: 1, max: 10, value: 0, duration: 0, delay: 0, random: 0 });
        redraw(k);
    });
    $$('#sdet [data-xsave]').forEach(b => b.onclick = async () => {
        const k = b.dataset.xsave;
        if (await act(api('spell_effects_save', {}, { id: lv.Id, critical: k === 'critical' ? 1 : 0, effects: fx[k] }), 'Effets enregistrés')) spellDetail(s.spell.Id);
    });
    $('#slSave').onclick = async () => {
        const fields = {};
        $$('#sdet [data-f]').forEach(x => fields[x.dataset.f] = x.type === 'checkbox' ? (x.checked ? 1 : 0) : x.value.trim());
        if (await act(api('spell_level_save', {}, { id: lv.Id, fields }), 'Niveau enregistré')) spellDetail(s.spell.Id);
    };
}

TABS.items = tabItems;
TABS.spells = tabSpells;
