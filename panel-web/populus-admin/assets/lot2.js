/* Panel admin Populus 2.51 : onglet Monstres (modèle, grades, réglage rapide, sorts, drops, zones, groupes de ta carte). */
'use strict';

const GCOLS = [
    ['Level', 'Niv.'], ['LifePoints', 'PV'], ['ActionPoints', 'PA'], ['MovementPoints', 'PM'],
    ['Strength', 'Force'], ['Intelligence', 'Intel.'], ['Chance', 'Chance'], ['Agility', 'Agil.'], ['Wisdom', 'Sag.'], ['Vitality', 'Vita.'],
    ['NeutralResistance', '%Neu'], ['EarthResistance', '%Ter'], ['FireResistance', '%Feu'], ['WaterResistance', '%Eau'], ['AirResistance', '%Air'],
    ['PaDodge', 'Esq.PA'], ['PmDodge', 'Esq.PM'], ['TackleBlock', 'Tacle'], ['TackleEvade', 'Fuite'], ['GradeXp', 'XP'],
];
const ADJ = [['Strength', 'Terre (Force)', 1], ['Intelligence', 'Feu (Intel.)', 1], ['Chance', 'Eau (Chance)', 1], ['Agility', 'Air (Agilité)', 1],
    ['Wisdom', 'Sagesse', 0], ['LifePoints', 'PV', 0], ['RES', 'Résistances', 0], ['GradeXp', 'XP', 0]];
const RES = ['NeutralResistance', 'EarthResistance', 'FireResistance', 'WaterResistance', 'AirResistance'];
const FLAGS = [['IsBoss', 'Boss'], ['IsActive', 'Actif'], ['CanPlay', 'Joue son tour'], ['CanTackle', 'Tacle'], ['CanSwitchPos', 'Échange de place'],
    ['CanBePushed', 'Poussable'], ['canBeCarried', 'Portable'], ['UseSummonSlot', 'Prend une place d\'invocation'], ['UseBombSlot', 'Prend une place de bombe']];

let MOB = null;

function tabMobs() {
    $('#main').innerHTML = `<div class="cols">
      <div>
        <div class="card"><h3>🐾 Monstres</h3><input id="mq" placeholder="Nom ou numéro du monstre">
          <div id="mlist" class="list" style="margin-top:8px;max-height:40vh"></div></div>
        <div class="card"><h3>🗺️ Groupes sur ma carte <button id="mgRf" class="small">⟳</button></h3><div id="mgroups" class="muted">…</div>
          <div class="btns" style="margin-top:8px"><button id="mgNext">Faire apparaître le prochain groupe</button></div></div>
      </div>
      <div id="mdet"><div class="card empty" style="margin:0">Cherche un monstre par son nom.</div></div></div>`;
    const load = async () => {
        const q = valOf('mq');
        if (!q) { $('#mlist').innerHTML = '<div class="muted">Tape au moins une lettre.</div>'; return; }
        try {
            const r = await api('mobs', { q });
            $('#mlist').innerHTML = r.mobs.length ? r.mobs.map(m => `<div class="it" data-id="${m.Id}"><span>${esc(m.Name)}${+m.IsBoss ? ' <span class="pill">boss</span>' : ''}</span>
                <small>niv. ${m.MinLevel ?? '?'}${m.MaxLevel && m.MaxLevel !== m.MinLevel ? '–' + m.MaxLevel : ''} · #${m.Id}</small></div>`).join('') : '<div class="muted">Aucun monstre.</div>';
            $$('#mlist .it').forEach(el => el.onclick = () => { $$('#mlist .it').forEach(x => x.classList.remove('on')); el.classList.add('on'); mobDetail(+el.dataset.id); });
        } catch (e) { toast(e.message, true); }
    };
    $('#mq').oninput = debounce(load);
    $('#mgRf').onclick = mapGroups;
    $('#mgNext').onclick = async () => { await cmd('monster spawnnext'); mapGroups(); };
    load();
    mapGroups();
    if (MOB) mobDetail(MOB.template.Id);
}

async function mapGroups() {
    const el = $('#mgroups');
    if (!el) return;
    try {
        const r = await api('map_mobs');
        el.innerHTML = `<div class="muted" style="margin-bottom:4px">Map ${r.mapId} · ${esc(r.subAreaName || '')}</div>` + (r.groups.length ? r.groups.map(g =>
            `<div class="it" data-mid="${g.monsters[0] ? g.monsters[0].id : 0}"><span>${g.monsters.map(m => esc(r.names[m.id] || m.name) + ' <small>' + m.level + '</small>').join(', ')}</span>
             <small>case ${g.cell}</small></div>`).join('') : '<div class="muted">Aucun groupe sur ta carte.</div>');
        $$('#mgroups .it').forEach(x => x.onclick = () => { if (+x.dataset.mid) mobDetail(+x.dataset.mid); });
    } catch (e) { el.innerHTML = '<span class="err">' + esc(e.message) + '</span>'; }
}

async function mobDetail(id) {
    try { MOB = await api('mob', { id }); } catch (e) { toast(e.message, true); return; }
    const m = MOB, t = m.template;
    const base = m.grades.map(g => ({ ...g }));
    let rows = m.grades.map(g => ({ ...g }));
    let acc = {};
    const changed = (i, k) => String(rows[i][k]) !== String(base[i][k]);
    const gtable = () => `<table class="grades"><tr><th>Gr.</th>${GCOLS.map(c => `<th>${c[1]}</th>`).join('')}</tr>${rows.map((r, i) =>
        `<tr><td>${r.GradeId}</td>${GCOLS.map(c => `<td><input data-g="${i}" data-k="${c[0]}" value="${esc(r[c[0]])}" class="${changed(i, c[0]) ? 'chg' : ''}"
           title="${changed(i, c[0]) ? 'Enregistré : ' + esc(base[i][c[0]]) : ''}"></td>`).join('')}</tr>`).join('')}</table>`;

    const spellsHtml = m.grades.map(g => `<div class="row" style="margin:4px 0"><b style="width:70px">Grade ${g.GradeId}</b>
        ${(m.spells[g.Id] || []).map(s => `<span class="badge">${esc(s.Name)} #${s.SpellId} niv.${s.Level} <a href="#" data-sdel="${s.Id}" title="Retirer">✕</a></span>`).join(' ') || '<span class="muted">aucun sort</span>'}</div>`).join('');

    const dropRow = d => `<tr data-drop="${d.Id}"><td>${esc(d.ItemName)} <small class="muted">#${d.ItemId} niv.${d.ItemLevel ?? '?'}</small></td>
        ${[1, 2, 3, 4, 5].map(k => `<td><input data-k="DropRateForGrade${k}" value="${esc(d['DropRateForGrade' + k])}" style="width:62px"></td>`).join('')}
        <td><input data-k="ProspectingLock" value="${esc(d.ProspectingLock)}" style="width:60px"></td>
        <td><input data-k="DropLimit" value="${esc(d.DropLimit)}" style="width:55px"></td>
        <td><input data-k="Condition" value="${esc(d.Condition || '')}" style="width:110px"></td>
        <td class="act"><button class="small" data-dsave="${d.Id}">💾</button> <button class="small danger" data-ddel="${d.Id}">✕</button></td></tr>`;

    const zoneRow = z => `<tr><td>${+z.MapId ? 'map <b>' + z.MapId + '</b>' : esc(z.SubAreaName || 'sous-zone') + ' <small class="muted">#' + z.SubAreaId + '</small>'}</td>
        <td>${z.Frequency}</td><td>${z.MinGrade}–${z.MaxGrade}</td><td>${+z.IsDisabled ? '<span class="err">retiré</span>' : '<span class="badge ok">actif</span>'}</td>
        <td class="act"><button class="small" data-ztog="${z.Id}">${+z.IsDisabled ? 'Remettre' : 'Retirer'}</button> <button class="small danger" data-zdel="${z.Id}">Supprimer</button></td></tr>`;

    $('#mdet').innerHTML =
        card(`🐾 ${esc(m.name)} <span class="muted">#${t.Id} · ${esc(m.race)}</span>`, `
          <div class="grid">
            <label>Apparence (gfx)<input id="mt_GfxId" value="${esc(t.GfxId)}"></label>
            <label>Kamas min<input id="mt_MinDroppedKamas" value="${esc(t.MinDroppedKamas)}"></label>
            <label>Kamas max<input id="mt_MaxDroppedKamas" value="${esc(t.MaxDroppedKamas)}"></label>
            <label style="grid-column:span 2">Look<input id="mt_LookAsString" value="${esc(t.LookAsString)}"></label>
          </div>
          <div class="btns" style="margin-top:8px">${FLAGS.map(f => `<label class="row" style="gap:4px"><input type="checkbox" id="mt_${f[0]}" ${+t[f[0]] ? 'checked' : ''}>${f[1]}</label>`).join('')}</div>
          <div class="help">Présent dans ${m.zones.length} zone(s) d'apparition, ${m.fixed} groupe(s) fixe(s) et ${m.dungeonRooms} salle(s) de donjon.</div>
          <h3 style="margin-top:12px">Grades</h3><div style="overflow-x:auto" id="mgr">${gtable()}</div>
          <div class="form" style="border-style:solid">
            <div class="row"><b>⚖️ Réglage rapide</b><span id="adjInfo" class="muted">Les valeurs du tableau changent ; rien n'est enregistré avant « Enregistrer et appliquer ».</span></div>
            <div class="btns" style="margin-top:8px">${[-50, -30, -20, -10, -5].map(p => `<button class="adjb" data-p="${p}">${p} %</button>`).join('')}
              ${[5, 10, 20, 30, 50].map(p => `<button class="adjb" data-p="${p}">+${p} %</button>`).join('')}
              <span class="fit"><input id="adjPct" placeholder="ex : -15" style="width:80px"><button id="adjGo">Appliquer</button></span>
              <button id="adjReset">↺ Annuler</button></div>
            <div class="btns" style="margin-top:8px">${ADJ.map(a => `<label class="row" style="gap:4px"><input type="checkbox" data-adj="${a[0]}" ${a[2] ? 'checked' : ''}>${a[1]}</label>`).join('')}
              <select id="adjG" style="width:170px"><option value="all">Tous les grades</option>${base.map((r, i) => `<option value="${i}">Grade ${r.GradeId} (niv. ${r.Level})</option>`).join('')}</select></div>
          </div>
          <div class="row" style="margin-top:10px;justify-content:space-between">
            <span class="fit"><select id="spG" style="width:170px">${base.map(r => `<option value="${r.GradeId}">grade ${r.GradeId} (niv. ${r.Level})</option>`).join('')}</select>
              <button id="spawnHere">Faire apparaître sur ma case</button></span>
            <button class="pri" id="mobSave">💾 Enregistrer et appliquer</button></div>
          <div class="help">Appliqué tout de suite : les prochains combats et les prochains groupes utilisent les nouvelles valeurs. Les groupes déjà posés gardent les anciennes.</div>`)
        + card('✨ Sorts des grades', spellsHtml + `<div class="grid" style="margin-top:8px">
            <label>Grade<select id="msG">${m.grades.map(g => `<option value="${g.Id}">grade ${g.GradeId}</option>`).join('')}<option value="all">tous les grades</option></select></label>
            <label class="suggest">Sort<input id="msS" placeholder="nom ou numéro" autocomplete="off"><div class="drop" hidden></div></label>
            <label>Niveau du sort<input id="msL" type="number" value="1"></label><button id="msAdd">+ Ajouter</button></div>`)
        + card('💰 Drops', `<div style="overflow-x:auto"><table><tr><th>Objet</th><th>% g1</th><th>% g2</th><th>% g3</th><th>% g4</th><th>% g5</th><th>PP min</th><th>Limite</th><th>Condition</th><th></th></tr>
            ${m.drops.map(dropRow).join('') || '<tr><td colspan="10" class="muted">Aucun drop.</td></tr>'}</table></div>
            <div class="grid" style="margin-top:8px">
              <label class="suggest">Ajouter un objet<input id="mdItem" data-item="1" placeholder="nom ou numéro" autocomplete="off"><div class="drop" hidden></div></label>
              <label>% (tous les grades)<input id="mdP" placeholder="ex : 5"></label><label>Limite<input id="mdLim" value="0"></label>
              <button id="mdAdd">+ Ajouter</button></div>
            <div class="help">PP min = prospection minimale pour pouvoir le looter. Limite 0 = pas de limite.</div>`)
        + card('🗺️ Zones d\'apparition', `<table><tr><th>Où</th><th>Fréquence</th><th>Grades</th><th>État</th><th></th></tr>
            ${m.zones.map(zoneRow).join('') || '<tr><td colspan="5" class="muted">Ce monstre n\'apparaît dans aucune zone.</td></tr>'}</table>
            <div class="grid" style="margin-top:8px">
              <label class="suggest">Sous-zone<input id="mzSub" placeholder="nom ou numéro" autocomplete="off"><div class="drop" hidden></div></label>
              <label>ou une seule map<input id="mzMap" type="number" placeholder="numéro de map"></label>
              <label>Fréquence<input id="mzFreq" value="1"></label><label>Grade min<input id="mzMin" value="1"></label><label>Grade max<input id="mzMax" value="5"></label>
              <button id="mzHere">Ma sous-zone</button><button id="mzAdd" class="pri">+ Ajouter la zone</button></div>`);

    const bind = () => $$('#mgr [data-g]').forEach(x => x.onchange = () => {
        rows[+x.dataset.g][x.dataset.k] = x.value.trim();
        x.classList.toggle('chg', changed(+x.dataset.g, x.dataset.k));
    });
    const redraw = () => { $('#mgr').innerHTML = gtable(); bind(); };
    bind();

    const adjust = pct => {
        const keys = [];
        $$('[data-adj]').forEach(c => { if (c.checked) keys.push(...(c.dataset.adj === 'RES' ? RES : [c.dataset.adj])); });
        if (!keys.length) { toast('Coche au moins une caractéristique', true); return; }
        const g = $('#adjG').value;
        const grades = g === 'all' ? base.map((_, i) => i) : [+g];
        let n = 0;
        for (const i of grades) for (const k of keys) {
            const b = parseFloat(base[i][k]);
            if (isNaN(b)) continue;
            acc[i + ':' + k] = (acc[i + ':' + k] || 0) + pct;
            let v = Math.round(b * (1 + acc[i + ':' + k] / 100));
            if (!RES.includes(k)) v = Math.max(0, v);
            rows[i][k] = String(v);
            n++;
        }
        redraw();
        const tot = [...new Set(grades.flatMap(i => keys.map(k => acc[i + ':' + k] || 0)))];
        $('#adjInfo').innerHTML = tot.length === 1 ? `Écart avec les valeurs enregistrées : <b class="${tot[0] < 0 ? 'err' : ''}">${tot[0] > 0 ? '+' : ''}${tot[0]} %</b> — pense à « Enregistrer et appliquer ».`
            : 'Écarts différents selon les caractéristiques — pense à « Enregistrer et appliquer ».';
        toast(`${pct > 0 ? '+' : ''}${pct} % sur ${n} valeur(s)`);
    };
    $$('.adjb').forEach(b => b.onclick = () => adjust(+b.dataset.p));
    $('#adjGo').onclick = () => { const p = parseFloat(valOf('adjPct').replace(',', '.')); if (isNaN(p) || !p) { toast('Pourcentage invalide (ex : -15)', true); return; } adjust(p); };
    $('#adjReset').onclick = () => { rows = base.map(g => ({ ...g })); acc = {}; redraw(); $('#adjInfo').textContent = 'Valeurs enregistrées remises.'; };

    $('#mobSave').onclick = async () => {
        const template = {};
        ['GfxId', 'MinDroppedKamas', 'MaxDroppedKamas', 'LookAsString'].forEach(k => template[k] = valOf('mt_' + k));
        FLAGS.forEach(f => template[f[0]] = $('#mt_' + f[0]).checked ? 1 : 0);
        const grades = rows.map(r => { const o = { Id: r.Id }; GCOLS.forEach(c => o[c[0]] = r[c[0]]); return o; });
        const r = await act(api('mob_save', {}, { id: t.Id, template, grades }));
        if (!r) return;
        toast(r.changes ? r.changes + ' modification(s) enregistrée(s)' : 'Rien n\'a changé');
        if (r.reload) reloadStatus(r.reload);
        mobDetail(t.Id);
    };
    $('#spawnHere').onclick = () => cmd(`monster spawn ${t.Id} ${$('#spG').value}`).then(mapGroups);

    // sorts
    wireSuggest($('#msS'), 'spell_search', 'spells', s => `#${s.Id} ${esc(s.Name)}`);
    $('#msAdd').onclick = async () => {
        const spellId = +$('#msS').value, level = +valOf('msL') || 1;
        if (!spellId) { toast('Choisis un sort', true); return; }
        const gs = $('#msG').value === 'all' ? m.grades.map(g => g.Id) : [+$('#msG').value];
        for (const gradeId of gs) if (!await act(api('mob_spell_add', {}, { gradeId, spellId, level }))) return;
        toast('Sort ajouté'); mobDetail(t.Id);
    };
    $$('#mdet [data-sdel]').forEach(a => a.onclick = async e => {
        e.preventDefault();
        if (await act(api('mob_spell_delete', {}, { id: +a.dataset.sdel }), 'Sort retiré')) mobDetail(t.Id);
    });

    // drops
    wireItemFields($('#mdet'));
    $$('#mdet [data-dsave]').forEach(b => b.onclick = async () => {
        const tr = b.closest('tr'), body = { monsterId: t.Id, id: +b.dataset.dsave };
        $$('[data-k]', tr).forEach(i => body[i.dataset.k] = i.value);
        const d = m.drops.find(x => x.Id == b.dataset.dsave);
        body.RollsCounter = d.RollsCounter; body.DropGroup = d.DropGroup; body.TaxCollectorCannotLoot = d.TaxCollectorCannotLoot;
        if (await act(api('drop_save', {}, body), 'Drop enregistré')) mobDetail(t.Id);
    });
    $$('#mdet [data-ddel]').forEach(b => b.onclick = async () => {
        if (confirm('Retirer ce drop ?') && await act(api('drop_delete', {}, { id: +b.dataset.ddel }), 'Drop retiré')) mobDetail(t.Id);
    });
    $('#mdAdd').onclick = async () => {
        const p = valOf('mdP').replace(',', '.') || '1';
        const body = { monsterId: t.Id, itemId: +valOf('mdItem'), DropLimit: valOf('mdLim') || 0, RollsCounter: 1, ProspectingLock: 0, DropGroup: 0, TaxCollectorCannotLoot: 0 };
        [1, 2, 3, 4, 5].forEach(k => body['DropRateForGrade' + k] = p);
        if (await act(api('drop_save', {}, body), 'Drop ajouté')) mobDetail(t.Id);
    };

    // zones
    wireSuggest($('#mzSub'), 'subarea_search', 'subareas', s => `#${s.Id} ${esc(s.Name)}`);
    $('#mzHere').onclick = async () => {
        const s = await refreshState();
        if (s && s.me && s.me.subAreaId) { $('#mzSub').value = s.me.subAreaId; toast('Sous-zone ' + s.me.subAreaId + ' (' + (s.me.zone || '') + ')'); }
    };
    $('#mzAdd').onclick = async () => {
        const body = { monsterId: t.Id, subAreaId: +valOf('mzSub') || 0, mapId: +valOf('mzMap') || 0, frequency: valOf('mzFreq'), minGrade: valOf('mzMin'), maxGrade: valOf('mzMax') };
        if (await act(api('zone_save', {}, body), 'Zone ajoutée')) mobDetail(t.Id);
    };
    $$('#mdet [data-ztog]').forEach(b => b.onclick = async () => {
        const z = m.zones.find(x => x.Id == b.dataset.ztog);
        const body = { monsterId: t.Id, id: z.Id, subAreaId: z.SubAreaId, mapId: z.MapId, frequency: z.Frequency, minGrade: z.MinGrade, maxGrade: z.MaxGrade, disabled: +z.IsDisabled ? 0 : 1 };
        if (await act(api('zone_save', {}, body), +z.IsDisabled ? 'Remis dans la zone' : 'Retiré de la zone')) mobDetail(t.Id);
    });
    $$('#mdet [data-zdel]').forEach(b => b.onclick = async () => {
        if (confirm('Supprimer cette zone d\'apparition ?') && await act(api('zone_delete', {}, { id: +b.dataset.zdel }), 'Zone supprimée')) mobDetail(t.Id);
    });
}

/** Champ avec suggestions (garde le numéro de l'élément choisi). */
function wireSuggest(inp, action, key, label) {
    const drop = inp.parentElement.querySelector('.drop');
    let tm;
    inp.oninput = () => {
        clearTimeout(tm);
        const v = inp.value.trim();
        if (!v || /^\d+$/.test(v)) { drop.hidden = true; return; }
        tm = setTimeout(async () => {
            try {
                const r = await api(action, { q: v });
                drop.innerHTML = r[key].map(x => `<div data-id="${x.Id}">${label(x)}</div>`).join('') || '<div class="muted">Rien trouvé</div>';
                drop.hidden = false;
                $$('[data-id]', drop).forEach(d => d.onclick = () => { inp.value = d.dataset.id; drop.hidden = true; });
            } catch { }
        }, 250);
    };
    inp.onblur = () => setTimeout(() => drop.hidden = true, 200);
}

TABS.mobs = tabMobs;
