/* Panel admin Populus 2.51 : onglet PNJ « comme le 1.29 » (remplace les fonctions du même nom d'app.js).
   - PNJ de ma carte, orientation au placement ;
   - apparence : os, peaux (accessoires choisis par nom d'objet), couleurs, taille ;
   - dialogues : arbre des questions, recherche dans tous les textes de PNJ du jeu (début d'un mot), questions libres. */
'use strict';

const MSG_DIRECT = 0x40000000, MSG_CUSTOM = 0x50000000;

/* ------------------------------------------------------------------ liste : PNJ de ma carte */
const _tabNpcsBase = tabNpcs;
tabNpcs = function () {
    const r = _tabNpcsBase();
    const aside = $('#main aside');
    if (aside) {
        const box = document.createElement('div');
        box.innerHTML = `<button id="npcMine" style="width:100%">🗺️ PNJ de ma carte</button><div id="npcMineList" class="list" style="margin-top:4px"></div>`;
        aside.insertBefore(box, aside.firstChild);
        $('#npcMine').onclick = async () => {
            try {
                const m = await api('map_npcs');
                $('#npcMineList').innerHTML = `<div class="muted" style="font-size:12px">Map ${m.map} · ${m.npcs.length} PNJ</div>` + (m.npcs.map(n =>
                    `<div class="it" data-id="${n.NpcId}"><div>${esc(n.Name)}<div class="tags">${esc((n.Actions || 'aucune action').replace(/,/g, ' · '))}</div></div><small>case ${n.CellId}</small></div>`).join('') || '<div class="muted">Aucun PNJ ici.</div>');
                $$('#npcMineList .it').forEach(x => x.onclick = () => openNpc(+x.dataset.id));
            } catch (e) { toast(e.message, true); }
        };
    }
    return r;
};

/* ------------------------------------------------------------------ apparence (look) */
/** « {os|peaux|couleurs|tailles|sous-entités} » -> morceaux (les sous-entités peuvent contenir des | et des {}). */
function lookParts(look) {
    const s = String(look || '').trim().replace(/^\{/, '').replace(/\}$/, '');
    const parts = [];
    let depth = 0, cur = '';
    for (const ch of s) {
        if (ch === '{') depth++;
        if (ch === '}') depth--;
        if (ch === '|' && depth === 0) { parts.push(cur); cur = ''; continue; }
        cur += ch;
    }
    parts.push(cur);
    while (parts.length < 5) parts.push('');
    return { bones: parts[0], skins: parts[1].split(',').filter(Boolean), colors: parts[2], scale: parts[3], sub: parts.slice(4).join('|') };
}
function lookColors(csv) {
    const out = {};
    String(csv || '').split(',').forEach(c => {
        const [i, v] = c.split('=');
        if (!i || v === undefined) return;
        out[+i] = v.startsWith('#') ? v : '#' + (+v).toString(16).padStart(6, '0');
    });
    return out;
}
function lookBuild(p) {
    const parts = [p.bones, p.skins.join(','), p.colors, p.scale];
    if (p.sub) parts.push(p.sub);
    while (parts.length > 1 && parts[parts.length - 1] === '') parts.pop();
    return '{' + parts.join('|') + '}';
}

renderGeneral = function () {
    const n = state.npc;
    const L = lookParts(n.look);
    const cols = lookColors(L.colors);
    const COLOR_NAMES = ['', 'Couleur 1 (cheveux)', 'Couleur 2 (peau)', 'Couleur 3 (habit principal)', 'Couleur 4 (habit secondaire)', 'Couleur 5 (accessoires)'];
    $('#tab').innerHTML = `
      <div class="card">
        <h3>🎨 Apparence</h3>
        <div class="grid">
          <label>Corps (os)<input id="lkBones" value="${esc(L.bones)}"><span class="help">1 = humain (classes), sinon numéro du corps du monstre / PNJ</span></label>
          <label>Taille (%)<input id="lkScale" value="${esc(L.scale)}" placeholder="100"></label>
        </div>
        <h3 style="margin-top:10px">Peaux et accessoires portés</h3>
        <div id="lkSkins" class="btns"></div>
        <div class="grid" style="margin-top:6px">
          <label class="suggest">Ajouter un objet porté (chapeau, cape, arme…)<input id="lkItem" placeholder="nom de l'objet" autocomplete="off"><div class="drop" hidden></div></label>
          <label>…ou un numéro de peau<input id="lkSkinId" type="number" placeholder="ex. 2052"></label><button id="lkSkinAdd">+ Ajouter</button>
        </div>
        <h3 style="margin-top:10px">Couleurs</h3>
        <div class="grid">${[1, 2, 3, 4, 5].map(i => `<label>${COLOR_NAMES[i]}<span class="row"><input type="color" id="lkC${i}" value="${cols[i] || '#888888'}" style="padding:0;height:30px;width:60px">
          <label class="row" style="gap:3px"><input type="checkbox" id="lkD${i}" ${cols[i] ? '' : 'checked'}>défaut</label></span></label>`).join('')}</div>
        <label class="field" style="margin-top:10px">Code d'apparence (calculé, modifiable)<textarea id="look">${esc(n.look)}</textarea></label>
        <div class="row" style="margin-top:10px"><button id="lookPreview">👁 Aperçu en jeu (1 min)</button><button id="lookSave" class="pri">💾 Enregistrer</button></div>
      </div>
      <div class="card"><h3>Résumé</h3><div class="muted">${n.spawns.length} placement(s) · ${n.actions.length} action(s) · ${n.messages.length + (n.extraMessages || []).length} question(s) et ${n.replies.length} texte(s) de réponse.</div></div>`;
    const skins = L.skins.slice();
    const rebuild = () => {
        const colors = [1, 2, 3, 4, 5].filter(i => !$('#lkD' + i).checked).map(i => i + '=' + $('#lkC' + i).value).join(',');
        $('#look').value = lookBuild({ bones: valOf('lkBones'), skins, colors, scale: valOf('lkScale'), sub: L.sub });
    };
    const drawSkins = () => {
        $('#lkSkins').innerHTML = skins.map((s, i) => `<span class="badge">peau ${esc(s)} <a href="#" data-sk="${i}">✕</a></span>`).join('') || '<span class="muted">aucune</span>';
        $$('#lkSkins [data-sk]').forEach(a => a.onclick = e => { e.preventDefault(); skins.splice(+a.dataset.sk, 1); drawSkins(); rebuild(); });
    };
    drawSkins();
    ['lkBones', 'lkScale', ...[1, 2, 3, 4, 5].flatMap(i => ['lkC' + i, 'lkD' + i])].forEach(id => { $('#' + id).oninput = rebuild; $('#' + id).onchange = rebuild; });
    // objet -> son apparence (AppearanceId) ajoutée aux peaux
    const inp = $('#lkItem'), drop = inp.parentElement.querySelector('.drop');
    inp.oninput = debounce(async () => {
        const q = inp.value.trim();
        if (q.length < 2) { drop.hidden = true; return; }
        try {
            const r = await api('look_items', { q });
            drop.innerHTML = r.items.map(x => `<div data-ap="${x.AppearanceId}">${esc(x.Name)} <span class="muted">niv. ${x.Level} · apparence ${x.AppearanceId}</span></div>`).join('') || '<div class="muted">Aucun objet avec une apparence</div>';
            drop.hidden = false;
            $$('[data-ap]', drop).forEach(d => d.onclick = () => { skins.push(d.dataset.ap); drawSkins(); rebuild(); drop.hidden = true; inp.value = ''; });
        } catch { }
    });
    inp.onblur = () => setTimeout(() => drop.hidden = true, 200);
    $('#lkSkinAdd').onclick = () => { const v = valOf('lkSkinId'); if (v) { skins.push(v); drawSkins(); rebuild(); $('#lkSkinId').value = ''; } };
    $('#lookPreview').onclick = async () => {
        const r = await act(api('preview_look', {}, { id: n.id, look: $('#look').value.trim() }));
        if (r) r.ok ? toast(r.result) : toast(r.timeout ? 'Le serveur de jeu ne répond pas.' : r.result, true);
    };
    $('#lookSave').onclick = async () => {
        if (await act(api('look_save', {}, { id: n.id, look: $('#look').value.trim() }), 'Apparence enregistrée')) openNpc(n.id, true);
    };
};

/* ------------------------------------------------------------------ placements : orientation choisie */
const _renderSpawnsBase = renderSpawns;
renderSpawns = function () {
    _renderSpawnsBase();
    const n = state.npc;
    const btn = $('#spawnHere');
    if (!btn) return;
    const sel = document.createElement('select');
    sel.id = 'spawnDir';
    sel.style.width = '160px';
    sel.innerHTML = `<option value="-1">orientation de mon perso</option>` + DIRS.map((d, i) => `<option value="${i}">${i} · ${d}</option>`).join('');
    btn.parentElement.insertBefore(sel, btn);
    btn.onclick = async () => {
        const p = await act(api('position'));
        if (!p) return;
        const dir = +sel.value >= 0 ? +sel.value : p.direction;
        if (await act(api('spawn_save', {}, { npcId: n.id, mapId: p.mapId, cellId: p.cellId, direction: dir }), 'PNJ placé sur ta position')) openNpc(n.id, true);
    };
};

/* ------------------------------------------------------------------ dialogues */
/** Toutes les questions utilisables par ce PNJ : les siennes + celles ajoutées depuis le panel. */
function npcAllMessages() {
    const n = state.npc;
    return n.messages.map(m => ({ ...m, origin: 'pnj' })).concat((n.extraMessages || []).map(m => ({ ...m, origin: m.custom ? 'libre' : 'jeu' })));
}

messageOptions = function (selected) {
    const all = npcAllMessages();
    let opts = all.map(m => `<option value="${m.id}" ${+selected === m.id ? 'selected' : ''}>${m.origin === 'libre' ? '✎ ' : m.origin === 'jeu' ? '★ ' : ''}#${m.id >= MSG_DIRECT ? 'p' + (m.id % 100000) : m.id} — ${esc(cut(m.text || '(texte introuvable)', 110))}</option>`).join('');
    if (selected && !all.some(m => m.id == selected)) opts = `<option value="${selected}" selected>#${selected} — absente de ce PNJ</option>` + opts;
    return '<option value="">— choisir —</option>' + opts;
};

renderDialog = function () {
    const n = state.npc;
    const all = npcAllMessages();
    const byId = Object.fromEntries(all.map(m => [m.id, m]));
    const replyText = r => r.ReplyText || (n.replies.find(x => x.id == r.ReplyId) || {}).text || null;
    const starts = {};
    n.actions.filter(a => a.Type === 'Talk').forEach(a => (starts[a.Parameter0] = starts[a.Parameter0] || []).push(a));

    // ordre en arbre : départs, puis suites (réponses « Aller à une autre question »), puis le reste
    const order = [], depth = {}, seen = new Set();
    const visit = (id, d) => {
        id = +id;
        if (seen.has(id)) return;
        seen.add(id); order.push(id); depth[id] = d;
        n.dialog.filter(r => +r.MessageId === id && r.Type === 'Dialog' && r.Parameter0).forEach(r => visit(r.Parameter0, d + 1));
    };
    Object.keys(starts).forEach(id => visit(id, 0));
    all.forEach(m => { if (n.dialog.some(r => +r.MessageId === m.id)) visit(m.id, 0); });
    const rest = all.filter(m => !seen.has(m.id));

    const block = (id, d) => {
        const m = byId[id];
        const replies = n.dialog.filter(r => +r.MessageId === +id);
        const tags = (starts[id] || []).map(a => `<span class="badge ok">départ${a.Condition ? ' si ' + esc(a.Condition) : ''}</span>`).join(' ')
            + (m && m.origin === 'libre' ? ' <span class="badge blue">question libre</span>' : m && m.origin === 'jeu' ? ' <span class="badge blue">texte du jeu</span>' : '')
            + (m ? '' : ' <span class="badge" style="color:var(--err)">absente de ce PNJ</span>');
        return `<div class="msg" style="margin-left:${Math.min(d, 6) * 22}px">
          <div class="meta">${d ? '↳ ' : ''}Question ${id >= MSG_DIRECT ? '(panel)' : '#' + id} ${tags}
            ${m && m.origin === 'libre' ? `<a href="#" data-cedit="${id}">modifier</a>` : ''} ${m && m.origin !== 'pnj' ? `<a href="#" data-detach="${id}" style="color:var(--err)">retirer de ce PNJ</a>` : ''}</div>
          <div class="txt">${esc(m ? m.text || '(texte introuvable)' : '')}</div>
          <div id="ce${id}"></div>
          ${replies.map(r => {
              const spec = REPLY_TYPES[r.Type], txt = replyText(r);
              return `<div class="reply"><div class="body">
                  <div class="rt">${txt ? '→ ' + esc(cut(txt, 160)) : '→ <span class="err">texte de réponse inconnu</span>'}</div>
                  <div class="rp"><span class="badge">${esc(spec ? spec.label : r.Type)}</span> ${spec ? paramSummary(spec.params, r) : paramSummary(GENERIC, r)}${r.Criteria ? ' · si <code>' + esc(r.Criteria) + '</code>' : ''}</div>
                  <div id="rf${r.Id}"></div></div>
                <span class="row"><button class="small" data-redit="${r.Id}">Modifier</button><button class="small danger" data-rdel="${r.Id}">Supprimer</button></span></div>`;
          }).join('')}
          <div style="margin:6px 0 0 18px"><button class="small" data-radd="${id}">+ Réponse</button></div><div id="rfnew${id}"></div>
        </div>`;
    };

    $('#tab').innerHTML = `
      <div class="card"><h3>➕ Ajouter une question à ce PNJ</h3>
        <div class="grid"><label class="suggest" style="grid-column:1/-1">Chercher dans toutes les questions de PNJ du jeu (début d'un mot)
          <input id="qFind" placeholder="ex. donj, clef, bonjour…" autocomplete="off"></label></div>
        <div id="qRes" class="list" style="max-height:220px;margin-top:4px"></div>
        <label class="field" style="margin-top:8px">…ou écrire une question libre<textarea id="qCustom" placeholder="Texte affiché par le PNJ"></textarea></label>
        <div class="row" style="justify-content:flex-end;margin-top:6px"><button id="qCustomAdd" class="pri">+ Créer la question libre</button></div>
        <div class="help">Les questions ajoutées s'utilisent comme les autres : action Dialogue (onglet Actions) pour le départ, ou réponse « Aller à une autre question ».
          Elles demandent le client à jour (DofusInvoker avec le panel).</div></div>
      ${order.map(id => block(id, depth[id])).join('')}
      ${rest.length ? `<div class="card"><h3>Autres questions de ce PNJ (pas encore utilisées)</h3>${rest.map(m => block(m.id, 0)).join('')}</div>` : ''}`;

    $('#qFind').oninput = debounce(async () => {
        const q = valOf('qFind');
        if (q.length < 2) { $('#qRes').innerHTML = ''; return; }
        try {
            const r = await api('dialog_texts', { q, kind: 'q' });
            $('#qRes').innerHTML = r.texts.map(t => `<div class="it" data-tid="${t.TextId}"><span>${esc(cut(t.French, 220))}</span><small>${t.Npcs} PNJ · <b>ajouter</b></small></div>`).join('') || '<div class="muted">Aucune question.</div>';
            $$('#qRes [data-tid]').forEach(x => x.onclick = async () => {
                if (await act(api('npc_message_add', {}, { npcId: n.id, textId: +x.dataset.tid }), 'Question ajoutée')) openNpc(n.id, true);
            });
        } catch (e) { toast(e.message, true); }
    });
    $('#qCustomAdd').onclick = async () => {
        const t = valOf('qCustom');
        if (!t) { toast('Écris le texte de la question', true); return; }
        if (await act(api('npc_message_add', {}, { npcId: n.id, custom: t }), 'Question libre créée')) openNpc(n.id, true);
    };
    $$('#tab [data-detach]').forEach(a => a.onclick = async e => {
        e.preventDefault();
        if (confirm('Retirer cette question de ce PNJ ?') && await act(api('npc_message_detach', {}, { npcId: n.id, id: +a.dataset.detach }), 'Retirée')) openNpc(n.id, true);
    });
    $$('#tab [data-cedit]').forEach(a => a.onclick = e => {
        e.preventDefault();
        const id = +a.dataset.cedit, m = byId[id];
        $('#ce' + id).innerHTML = `<div class="form"><textarea id="ceT${id}">${esc(m.text)}</textarea><div class="actions"><button id="ceS${id}" class="pri">Enregistrer</button></div></div>`;
        $('#ceS' + id).onclick = async () => { if (await act(api('npc_message_custom_save', {}, { id, text: $('#ceT' + id).value }), 'Question modifiée')) openNpc(n.id, true); };
    });
    $$('#tab [data-radd]').forEach(b => b.onclick = () => replyForm(null, +b.dataset.radd, '#rfnew' + b.dataset.radd));
    $$('#tab [data-redit]').forEach(b => b.onclick = () => { const r = n.dialog.find(x => x.Id == b.dataset.redit); replyForm(r, +r.MessageId, '#rf' + r.Id); });
    $$('#tab [data-rdel]').forEach(b => b.onclick = async () => {
        if (confirm('Supprimer cette réponse ?') && await act(api('reply_delete', {}, { id: +b.dataset.rdel }), 'Réponse supprimée')) openNpc(n.id, true);
    });
};

/* réponse : texte du PNJ ou n'importe quelle réponse du jeu (recherche) */
const _replyFormBase = replyForm;
replyForm = function (r, messageId, target) {
    _replyFormBase(r, messageId, target);
    const box = $(target);
    const decorate = () => {
        const sel = $('#rfReply', box);
        if (!sel || $('#rfFind', box)) return;
        if (r && +r.ReplyId >= MSG_DIRECT && !sel.querySelector(`option[value="${r.ReplyId}"]`)) {
            const o = document.createElement('option');
            o.value = r.ReplyId; o.textContent = '★ ' + (r.ReplyText || 'réponse du jeu'); o.selected = true;
            sel.insertBefore(o, sel.children[1]);
        }
        const wrap = document.createElement('label');
        wrap.style.gridColumn = '1/-1';
        wrap.innerHTML = `…ou chercher une réponse dans tout le jeu (début d'un mot)<input id="rfFind" placeholder="ex. oui, entrer, merci…" autocomplete="off"><div id="rfRes" class="list" style="max-height:180px"></div>`;
        sel.parentElement.after(wrap);
        $('#rfFind', box).oninput = debounce(async () => {
            const q = $('#rfFind', box).value.trim();
            if (q.length < 2) { $('#rfRes', box).innerHTML = ''; return; }
            const res = await api('dialog_texts', { q, kind: 'r' });
            $('#rfRes', box).innerHTML = res.texts.map(t => `<div class="it" data-rid="${MSG_DIRECT + +t.TextId}"><span>${esc(cut(t.French, 160))}</span><small>${t.Npcs} PNJ</small></div>`).join('') || '<div class="muted">Aucune réponse.</div>';
            $$('#rfRes [data-rid]', box).forEach(x => x.onclick = () => {
                const o = document.createElement('option');
                o.value = x.dataset.rid; o.textContent = '★ ' + x.querySelector('span').textContent; o.selected = true;
                sel.insertBefore(o, sel.children[1]);
                $('#rfRes', box).innerHTML = '';
                $('#rfFind', box).value = '';
            });
        });
    };
    decorate();
    // le formulaire se redessine quand on change l'effet : on rajoute la recherche à chaque fois
    new MutationObserver(decorate).observe(box, { childList: true });
};

TABS.npcs = tabNpcs;
