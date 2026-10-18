// MYSTIC 18/10/2026 : Atelier du site Mystic (builder, panoplies, craft, recherche, bestiaire, encyclopédie, armoire).
// Tout tourne dans le navigateur : données générées depuis la base du serveur (atelier/donnees/*.json, voir
// _OUTILS/web/atelier/exporteur), stuffs gardés dans le localStorage, partage par lien (?b=…) sans serveur.
'use strict';
(() => {

// ============================================================ outils
const $ = (s, r = document) => r.querySelector(s);
const $$ = (s, r = document) => [...r.querySelectorAll(s)];
const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
const fmt = n => (Math.round(n * 100) / 100).toLocaleString('fr-FR');
const signe = n => (n > 0 ? '+' : '') + fmt(n);
const norm = s => String(s || '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();
const uid = () => Math.random().toString(36).slice(2, 10) + Date.now().toString(36);
const VUE = $('#vue');

function toast(t) {
  const e = $('#toast');
  e.textContent = t;
  e.classList.add('on');
  clearTimeout(toast.m);
  toast.m = setTimeout(() => e.classList.remove('on'), 2200);
}

async function copier(t, message) {
  try { await navigator.clipboard.writeText(t); }
  catch {
    const z = document.createElement('textarea');
    z.value = t; document.body.appendChild(z); z.select();
    try { document.execCommand('copy'); } catch { }
    z.remove();
  }
  toast(message || 'Copié');
}

function lireStock(cle, defaut) {
  try { const v = localStorage.getItem(cle); return v ? JSON.parse(v) : defaut; } catch { return defaut; }
}
function ecrireStock(cle, v) { try { localStorage.setItem(cle, JSON.stringify(v)); } catch { } }

// ============================================================ données
const D = { objets: [], OBJ: new Map(), TYPE: new Map(), EFF: {}, PANO: new Map(), REC: new Map(), UTIL: new Map(), METIER: new Map(), CLASSE: new Map() };
let SOURCES = null, BESTIAIRE = null;

async function json(f) {
  const r = await fetch('atelier/donnees/' + f + '.json', { cache: 'no-cache' });
  if (!r.ok) throw new Error(f + ' : ' + r.status);
  return r.json();
}

async function chargerData() {
  const d = await json('data');
  D.genere = d.genere;
  d.types.forEach(([id, n, s]) => D.TYPE.set(id, { id, n, s }));
  d.metiers.forEach(([id, n]) => D.METIER.set(id, n));
  d.classes.forEach(c => D.CLASSE.set(c[0], { id: c[0], n: c[1], couts: c.slice(2, 8), pv: c[8] || 55, pa: c[9] || 6, pm: c[10] || 3, pp: c[11] || 100, lookM: c[12] || '', lookF: c[13] || '' }));
  D.EFF = d.effets;
  D.objets = d.objets;
  d.objets.forEach(o => D.OBJ.set(o.id, o));
  d.panoplies.forEach(([id, n, pieces, bonus]) => D.PANO.set(id, { id, n, pieces: pieces.filter(p => D.OBJ.has(p)), bonus }));
  d.recettes.forEach(([id, metier, ing]) => {
    if (!D.OBJ.has(id)) return;
    D.REC.set(id, { metier, ing });
    ing.forEach(([i]) => { if (!D.UTIL.has(i)) D.UTIL.set(i, []); D.UTIL.get(i).push(id); });
  });
}
async function chargerSources() { if (!SOURCES) SOURCES = await json('sources'); return SOURCES; }
async function chargerBestiaire() {
  if (BESTIAIRE) return BESTIAIRE;
  const b = await json('bestiaire');
  b.MON = new Map(b.monstres.map(m => [m[0], m]));
  b.RACE = new Map(b.races.map(r => [r[0], r[1]]));
  BESTIAIRE = b;
  return b;
}

const icoObjet = o => o ? 'atelier/icones/objets/' + o.i + '.png' : '';
const icoMonstre = id => 'atelier/icones/monstres/' + id + '.png';
const nomType = o => D.TYPE.get(o.t)?.n || '';
const superType = o => D.TYPE.get(o.t)?.s || 0;
const imgObjet = (o, c = '') => `<span class="ico ${c}"><img src="${icoObjet(o)}" alt="" loading="lazy" onerror="this.style.visibility='hidden'"></span>`;

// ============================================================ caractéristiques
// identifiant de caractéristique (effects_templates.Characteristic) -> clé de stat
const CAR = { 1: 'pa', 23: 'pm', 19: 'po', 11: 'vita', 12: 'sag', 10: 'for', 15: 'int', 13: 'cha', 14: 'agi', 44: 'ini', 48: 'pp', 26: 'invoc', 18: 'crit',
  25: 'puis', 16: 'do', 88: 'doTerre', 89: 'doFeu', 90: 'doEau', 91: 'doAir', 92: 'doNeutre', 86: 'doCrit', 49: 'soins',
  33: 'rTerreP', 34: 'rFeuP', 35: 'rEauP', 36: 'rAirP', 37: 'rNeutreP', 54: 'rTerre', 55: 'rFeu', 56: 'rEau', 57: 'rAir', 58: 'rNeutre',
  87: 'rCrit', 84: 'doPou', 85: 'rPou', 79: 'tacle', 78: 'fuite', 82: 'retPA', 83: 'retPM', 27: 'esqPA', 28: 'esqPM', 40: 'pods', 50: 'renvoi',
  69: 'puisPieges', 70: 'doPieges', 112: 'doMelee', 113: 'doDist', 114: 'doArmes', 116: 'doSorts', 115: 'rMelee', 117: 'rDist' };
// clé -> [libellé court, libellé long, icône, couleur]
const LIB = {
  pv: ['PV', 'Points de vie', 'lifePoints', '#ff6f91'], pa: ['PA', 'PA', 'actionPoints', '#7cc4ff'], pm: ['PM', 'PM', 'movementPoints', '#86efac'],
  po: ['PO', 'Portée', 'range', '#5eead4'], vita: ['Vita', 'Vitalité', 'vitality', '#ff6f91'], sag: ['Sagesse', 'Sagesse', 'wisdom', '#c4a5ff'],
  for: ['Force', 'Force', 'strength', '#d6a35c'], int: ['Intel.', 'Intelligence', 'intelligence', '#ff7b54'], cha: ['Chance', 'Chance', 'chance', '#5fb6ff'],
  agi: ['Agi.', 'Agilité', 'agility', '#7ee08a'], ini: ['Ini.', 'Initiative', 'initiative', '#ece8f2'], pp: ['PP', 'Prospection', 'prospecting', '#ece8f2'],
  invoc: ['Invoc.', 'Invocations', 'summonableCreaturesBoost', '#ffb39e'], crit: ['Crit.', 'Coups critiques', 'criticalHit', '#f87171'],
  puis: ['Puis.', 'Puissance', 'damagesPercent', '#fcd34d'], do: ['Do', 'Dommages', 'damage', '#fcd34d'],
  doTerre: ['Do Terre', 'Dommages Terre', 'strength', '#d6a35c'], doFeu: ['Do Feu', 'Dommages Feu', 'intelligence', '#ff7b54'],
  doEau: ['Do Eau', 'Dommages Eau', 'chance', '#5fb6ff'], doAir: ['Do Air', 'Dommages Air', 'agility', '#7ee08a'],
  doNeutre: ['Do Neutre', 'Dommages Neutre', 'neutral', '#c9c3d6'], doCrit: ['Do Crit.', 'Dommages critiques', 'criticalDamage', '#f87171'],
  soins: ['Soins', 'Soins', 'heal', '#f87171'],
  rTerreP: ['% Terre', '% Résistance Terre', 'earthReduction', '#d6a35c'], rFeuP: ['% Feu', '% Résistance Feu', 'fireReduction', '#ff7b54'],
  rEauP: ['% Eau', '% Résistance Eau', 'waterReduction', '#5fb6ff'], rAirP: ['% Air', '% Résistance Air', 'airReduction', '#7ee08a'],
  rNeutreP: ['% Neutre', '% Résistance Neutre', 'neutralReduction', '#c9c3d6'],
  rTerre: ['Ré Terre', 'Résistance Terre', 'earthReduction', '#d6a35c'], rFeu: ['Ré Feu', 'Résistance Feu', 'fireReduction', '#ff7b54'],
  rEau: ['Ré Eau', 'Résistance Eau', 'waterReduction', '#5fb6ff'], rAir: ['Ré Air', 'Résistance Air', 'airReduction', '#7ee08a'],
  rNeutre: ['Ré Neutre', 'Résistance Neutre', 'neutralReduction', '#c9c3d6'],
  rCrit: ['Ré Crit.', 'Résistance critiques', 'criticalReduction', '#ece8f2'], doPou: ['Do Pou.', 'Dommages poussée', 'push', '#ece8f2'],
  rPou: ['Ré Pou.', 'Résistance poussée', 'pushReduction', '#ece8f2'], tacle: ['Tacle', 'Tacle', 'tackle', '#ece8f2'], fuite: ['Fuite', 'Fuite', 'escape', '#ece8f2'],
  retPA: ['Ret. PA', 'Retrait PA', 'attackAP', '#7cc4ff'], retPM: ['Ret. PM', 'Retrait PM', 'attackMP', '#86efac'],
  esqPA: ['Esq. PA', 'Esquive PA', 'dodgeAP', '#7cc4ff'], esqPM: ['Esq. PM', 'Esquive PM', 'dodgeMP', '#86efac'],
  pods: ['Pods', 'Pods', 'pods', '#ece8f2'], renvoi: ['Renvoi', 'Renvoi de dommages', 'return', '#ece8f2'],
  puisPieges: ['Puis. pièges', 'Puissance des pièges', 'trapPercent', '#ece8f2'], doPieges: ['Do pièges', 'Dommages des pièges', 'trap', '#ece8f2'],
  doMelee: ['% Do mêlée', '% Dommages mêlée', 'weaponDamage', '#ece8f2'], doDist: ['% Do dist.', '% Dommages distance', 'range', '#ece8f2'],
  doArmes: ["% Do armes", "% Dommages d'armes", 'weaponDamage', '#ece8f2'], doSorts: ['% Do sorts', '% Dommages aux sorts', 'spellDamage', '#ece8f2'],
  rMelee: ['% Ré mêlée', '% Résistance mêlée', 'pushReduction', '#ece8f2'], rDist: ['% Ré dist.', '% Résistance distance', 'pushReduction', '#ece8f2'],
};
const ICO = k => 'atelier/icones/caracs/' + (LIB[k]?.[2] || 'damage') + '.png';
const ORDRE_PASTILLES = ['pv', 'pa', 'pm', 'po', 'invoc', 'ini', 'pp', 'crit', 'vita', 'sag', 'for', 'int', 'cha', 'agi', 'puis', 'do', 'doTerre', 'doFeu', 'doEau', 'doAir', 'doNeutre',
  'doCrit', 'soins', 'rTerreP', 'rFeuP', 'rEauP', 'rAirP', 'rNeutreP', 'rTerre', 'rFeu', 'rEau', 'rAir', 'rNeutre', 'rCrit', 'rPou', 'doPou', 'tacle', 'fuite', 'retPA', 'retPM', 'esqPA', 'esqPM', 'pods',
  'renvoi', 'puisPieges', 'doPieges', 'doMelee', 'doDist', 'doArmes', 'doSorts', 'rMelee', 'rDist'];
const CARACS6 = ['vita', 'sag', 'for', 'int', 'cha', 'agi'];

/** Texte d'un effet : « #1{~1~2 à }#2 Force » -> « 31 à 50 Force ». */
function texteEffet(id, min, max) {
  const t = D.EFF[id];
  let s = t ? t[0] : 'Effet #' + id;
  s = s.replace(/\{~1~2([^}]*)\}/g, max ? '$1' : '').replace(/#1/g, min).replace(/#2/g, max || '').replace(/#3/g, '').replace(/\{[^}]*\}/g, '').replace(/\s+/g, ' ').trim();
  return s;
}
/** Effet -> clé de stat et signe (null si ce n'est pas une caractéristique). */
function statEffet(id) {
  const t = D.EFF[id];
  if (!t) return null;
  const k = CAR[t[1]];
  if (!k || (t[2] !== '+' && t[2] !== '-')) return null;
  return { k, s: t[2] === '-' ? -1 : 1 };
}
/** Valeur d'un jet selon le mode (max = meilleur jet, comme le serveur : « + » -> max, « - » -> min). */
function jet(e, mode) {
  const [id, min, max] = e;
  if (!max) return min;
  const moins = D.EFF[id]?.[2] === '-';
  if (mode === 'moy') return Math.round((min + max) / 2);
  if (mode === 'min') return moins ? max : min;
  return moins ? min : max;
}

// ============================================================ stuffs
const EMPL = [
  { k: 'coiffe', st: [10], n: 'Coiffe', c: 'g' }, { k: 'cape', st: [11], n: 'Cape', c: 'g' }, { k: 'amulette', st: [1], n: 'Amulette', c: 'g' },
  { k: 'anneau1', st: [3], n: 'Anneau', c: 'g' }, { k: 'ceinture', st: [4], n: 'Ceinture', c: 'g' },
  { k: 'arme', st: [2], n: 'Arme', c: 'd' }, { k: 'bouclier', st: [7], n: 'Bouclier', c: 'd' }, { k: 'anneau2', st: [3], n: 'Anneau', c: 'd' },
  { k: 'bottes', st: [5], n: 'Bottes', c: 'd' }, { k: 'familier', st: [12], n: 'Familier', c: 'd' },
  ...[1, 2, 3, 4, 5, 6].map(i => ({ k: 'dofus' + i, st: [13], n: 'Dofus / Trophée', c: 'b' })),
];
const EMPL_K = new Map(EMPL.map((e, i) => [e.k, { ...e, i }]));
const TYPES_EXCLUS = new Set([83, 99, 20, 21, 22]); // pierres d'âme, filets, outils

const STOCK = 'mystic-atelier';
let ETAT = lireStock(STOCK, null);

function nouveauStuff(nom, classe = 8, niveau = 200) {
  const z = () => ({ vita: 0, sag: 0, for: 0, int: 0, cha: 0, agi: 0 });
  return { id: uid(), nom, classe, niveau, base: z(), parcho: z(), slots: {}, roll: 'max', off: [], maj: Date.now() };
}
function etatParDefaut() {
  const s = nouveauStuff('Stuff principal');
  return { persos: [{ id: uid(), nom: 'Mon personnage', stuffs: [s], actif: s.id }], perso: null, liste: [], possede: {}, achetes: [], recents: [] };
}
if (!ETAT || !Array.isArray(ETAT.persos) || !ETAT.persos.length) ETAT = etatParDefaut();
ETAT.liste ||= []; ETAT.possede ||= {}; ETAT.achetes ||= []; ETAT.recents ||= [];
if (!ETAT.persos.some(p => p.id === ETAT.perso)) ETAT.perso = ETAT.persos[0].id;

const sauver = () => ecrireStock(STOCK, ETAT);
const persoActif = () => ETAT.persos.find(p => p.id === ETAT.perso) || ETAT.persos[0];
function stuffActif() {
  const p = persoActif();
  if (!p.stuffs.length) p.stuffs.push(nouveauStuff('Stuff principal'));
  return p.stuffs.find(s => s.id === p.actif) || p.stuffs[0];
}
function touche(s) { s.maj = Date.now(); sauver(); majEntete(); }

// ---- partage : JSON compact en base64url, sans serveur
function encoder(s) {
  const slots = Object.entries(s.slots).map(([k, v]) => [EMPL_K.get(k)?.i ?? -1, v.id, v.jets || 0, v.exo || 0]).filter(x => x[0] >= 0);
  const c = [1, s.nom, s.classe, s.niveau, CARACS6.map(k => s.base[k] || 0), CARACS6.map(k => s.parcho[k] || 0), slots, s.roll, s.off || [], s.sexe ? 1 : 0];
  const b = new TextEncoder().encode(JSON.stringify(c));
  let t = '';
  b.forEach(x => t += String.fromCharCode(x));
  return btoa(t).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}
function decoder(code) {
  code = String(code).trim();
  const m = code.match(/[?&]b=([\w-]+)/);
  if (m) code = m[1];
  const t = atob(code.replace(/-/g, '+').replace(/_/g, '/'));
  const c = JSON.parse(new TextDecoder().decode(Uint8Array.from(t, x => x.charCodeAt(0))));
  if (!Array.isArray(c) || c[0] !== 1) throw new Error('code');
  const s = nouveauStuff(String(c[1] || 'Stuff importé').slice(0, 40), D.CLASSE.has(c[2]) ? c[2] : 8, Math.min(200, Math.max(1, c[3] | 0)));
  CARACS6.forEach((k, i) => { s.base[k] = Math.max(0, c[4]?.[i] | 0); s.parcho[k] = Math.min(100, Math.max(0, c[5]?.[i] | 0)); });
  (c[6] || []).forEach(([i, id, jets, exo]) => {
    const e = EMPL[i];
    if (e && D.OBJ.has(id)) s.slots[e.k] = { id, ...(Array.isArray(jets) ? { jets } : {}), ...(Array.isArray(exo) ? { exo } : {}) };
  });
  s.roll = ['max', 'moy', 'min'].includes(c[7]) ? c[7] : 'max';
  s.off = Array.isArray(c[8]) ? c[8].filter(Number.isInteger) : [];
  s.sexe = c[9] ? 1 : 0;
  return s;
}
const lienStuff = s => location.origin + location.pathname + '?b=' + encoder(s) + '#builder';

// ---- calcul des caractéristiques
function coutCarac(valeur, paliers) {
  if (!paliers || !paliers.length) return valeur;
  let cout = 0;
  for (let n = 0; n < valeur; n++) {
    let c = paliers[0][1];
    for (const [seuil, x] of paliers) if (n >= seuil) c = x;
    cout += c;
  }
  return cout;
}
function pointsDepenses(s) {
  const cl = D.CLASSE.get(s.classe);
  return CARACS6.reduce((t, k, i) => t + coutCarac(s.base[k] || 0, cl?.couts[i]), 0);
}
const pointsTotaux = s => (s.niveau - 1) * 5;

function effetsObjet(o, slot, mode) {
  const r = {};
  (o.e || []).forEach((e, i) => {
    const st = statEffet(e[0]);
    if (!st) return;
    const v = slot?.jets && slot.jets[i] != null && slot.jets[i] !== '' ? +slot.jets[i] : jet(e, mode);
    r[st.k] = (r[st.k] || 0) + st.s * v;
  });
  if (slot?.exo) {
    const st = statEffet(slot.exo[0]);
    if (st) r[st.k] = (r[st.k] || 0) + st.s * (+slot.exo[1] || 0);
  }
  return r;
}

/** Totaux d'un stuff (objets + panoplies + caracs), avec les formules du serveur. */
function calculer(s, remplacer) {
  const slots = { ...s.slots, ...(remplacer || {}) };
  for (const k in slots) if (!slots[k]) delete slots[k];
  const b = {};
  const ajoute = (o) => { for (const k in o) b[k] = (b[k] || 0) + o[k]; };
  const panos = new Map();
  for (const [k, v] of Object.entries(slots)) {
    const o = D.OBJ.get(v.id);
    if (!o) continue;
    ajoute(effetsObjet(o, v, s.roll));
    if (o.s && D.PANO.has(o.s)) {
      if (!panos.has(o.s)) panos.set(o.s, new Set());
      panos.get(o.s).add(o.id);
    }
  }
  const actives = [];
  for (const [id, set] of panos) {
    const p = D.PANO.get(id);
    const n = set.size;
    if (n < 2 || (s.off || []).includes(id)) { actives.push({ p, n, compte: false }); continue; }
    const bonus = p.bonus[n - 1] || [];
    bonus.forEach(([eid, v]) => { const st = statEffet(eid); if (st) b[st.k] = (b[st.k] || 0) + st.s * v; });
    actives.push({ p, n, compte: true });
  }
  const cl = D.CLASSE.get(s.classe) || { pv: 55, pa: 6, pm: 3, pp: 100 };
  const t = { ...b };
  CARACS6.forEach(k => t[k] = (s.base[k] || 0) + (s.parcho[k] || 0) + (b[k] || 0));
  t.pv = cl.pv + (s.niveau - 1) * 5 + t.vita;
  t.pa = cl.pa + (s.niveau >= 100 ? 1 : 0) + (b.pa || 0);
  t.pm = cl.pm + (b.pm || 0);
  t.po = b.po || 0;
  t.ini = t.for + t.int + t.cha + t.agi + (b.ini || 0);
  t.pp = cl.pp + Math.floor(t.cha / 10) + (b.pp || 0);
  t.invoc = 1 + (b.invoc || 0);
  t.crit = b.crit || 0;
  t.tacle = Math.floor(t.agi / 10) + (b.tacle || 0);
  t.fuite = Math.floor(t.agi / 10) + (b.fuite || 0);
  for (const k of ['retPA', 'retPM', 'esqPA', 'esqPM']) t[k] = Math.floor(t.sag / 10) + (b[k] || 0);
  t.pods = 1000 + t.for * 5 + (b.pods || 0);
  t._panos = actives;
  t._nb = Object.keys(slots).length;
  return t;
}
function differences(a, b) {
  const d = {};
  ORDRE_PASTILLES.forEach(k => { const x = (b[k] || 0) - (a[k] || 0); if (Math.abs(x) > 0.001) d[k] = x; });
  return d;
}
const pastille = (k, v, couleur = true) => `<span class="eff ${couleur ? (v > 0 ? 'plus' : 'moins') : ''}"><img src="${ICO(k)}" alt="">${signe(v)} ${esc(LIB[k][0])}</span>`;
function pastilles(d, max = 12) {
  const k = Object.keys(d);
  return k.slice(0, max).map(x => pastille(x, d[x])).join('') + (k.length > max ? `<span class="eff plus-n">+${k.length - max}</span>` : '');
}
/** Pastilles « brutes » d'un objet (jet max). */
function pastillesObjet(o, max = 10) {
  const r = effetsObjet(o, null, 'max');
  return pastilles(Object.fromEntries(ORDRE_PASTILLES.filter(k => r[k]).map(k => [k, r[k]])), max);
}

// ============================================================ fenêtres, menus, tiroir
function ouvrirModale(html, apres) {
  $('#modaleIn').innerHTML = html;
  $('#modale').hidden = false;
  apres?.($('#modaleIn'));
}
function fermerModale() { $('#modale').hidden = true; $('#modaleIn').innerHTML = ''; }
$('#modale').addEventListener('mousedown', e => { if (e.target.id === 'modale') fermerModale(); });

function ouvrirTiroir(html, apres) {
  $('#tiroirIn').innerHTML = html;
  $('#tiroir').hidden = false;
  $('#voile').hidden = false;
  apres?.($('#tiroirIn'));
}
function fermerTiroir() { $('#tiroir').hidden = true; $('#voile').hidden = true; $('#tiroirIn').innerHTML = ''; }
$('#voile').addEventListener('click', fermerTiroir);

/** Menu flottant : [{t, f, danger, coche, sep, titre}] */
function menu(ancre, items) {
  const m = $('#menuFlottant');
  m.innerHTML = items.map((x, i) => x.sep ? '<div class="sep"></div>' : x.titre ? `<div class="titre">${esc(x.titre)}</div>`
    : `<button type="button" data-i="${i}" class="${x.danger ? 'danger' : ''}"><span class="coche">${x.coche ? '✓' : ''}</span>${esc(x.t)}</button>`).join('');
  m.hidden = false;
  const r = ancre.getBoundingClientRect();
  const l = Math.min(window.innerWidth - m.offsetWidth - 8, Math.max(8, r.right - m.offsetWidth));
  m.style.left = l + 'px';
  m.style.top = Math.min(window.innerHeight - m.offsetHeight - 8, r.bottom + 6) + 'px';
  m.onclick = e => { const b = e.target.closest('button'); if (!b) return; m.hidden = true; items[+b.dataset.i].f?.(); };
}
document.addEventListener('click', e => { if (!e.target.closest('.enjeu,.chip[data-src]')) cacherBulle(); });
document.addEventListener('mousedown', e => { const m = $('#menuFlottant'); if (!m.hidden && !m.contains(e.target)) m.hidden = true; });
document.addEventListener('keydown', e => {
  if (e.key !== 'Escape') return;
  $('#menuFlottant').hidden = true;
  if (!$('#modale').hidden) fermerModale(); else if (!$('#tiroir').hidden) fermerTiroir();
});

function saisir(titre, valeur, ok) {
  ouvrirModale(`<h3>${esc(titre)}</h3><div style="margin:14px 0"><input class="champ" id="saisie" style="width:100%" maxlength="40" value="${esc(valeur)}"></div>
    <div class="rangee" style="justify-content:flex-end"><button class="btn fantome" id="sAnn">Annuler</button><button class="btn violet" id="sOk">Valider</button></div>`, m => {
    const c = $('#saisie', m);
    c.focus(); c.select();
    const fin = () => { const v = c.value.trim(); if (!v) return; fermerModale(); ok(v); };
    $('#sOk', m).onclick = fin;
    $('#sAnn', m).onclick = fermerModale;
    c.onkeydown = e => { if (e.key === 'Enter') fin(); };
  });
}
function confirmer(titre, texte, ok) {
  ouvrirModale(`<h3>${esc(titre)}</h3><p class="mini" style="margin:10px 0 16px">${esc(texte)}</p>
    <div class="rangee" style="justify-content:flex-end"><button class="btn fantome" id="cAnn">Annuler</button><button class="btn danger" id="cOk">Confirmer</button></div>`, m => {
    $('#cAnn', m).onclick = fermerModale;
    $('#cOk', m).onclick = () => { fermerModale(); ok(); };
  });
}

// ============================================================ menu latéral (style « La Forge »)
function majEntete() {
  const p = persoActif(), s = stuffActif();
  $('#choixStuffNom').textContent = p.nom + ' · ' + s.nom;
  const n = ETAT.persos.reduce((a, x) => a + x.stuffs.length, 0);
  $('#nbStuffs').textContent = n;
}
$('#choixStuff').addEventListener('click', e => {
  const p = persoActif();
  menu(e.currentTarget, [
    { titre: p.nom },
    ...p.stuffs.map(s => ({ t: s.nom + '  ·  niv. ' + s.niveau, coche: s.id === p.actif, f: () => { p.actif = s.id; sauver(); majEntete(); allerA('builder'); } })),
    { sep: true },
    ...ETAT.persos.filter(x => x !== p).map(x => ({ t: 'Passer sur « ' + x.nom + ' »', f: () => { ETAT.perso = x.id; sauver(); majEntete(); rendre(); } })),
    { t: '+ Nouveau stuff', f: () => ajouterStuff(p) },
    { t: '+ Nouveau personnage', f: nouveauPerso },
    { t: "Gérer l'armoire…", f: () => allerA('armoire') },
  ]);
});
/** Encart « Personnages liés » du menu : tes persos en jeu (compte web Mystic). */
async function majLies() {
  const b = $('#persosLies');
  let persos;
  try { persos = await listePersos(); }
  catch (e) {
    b.innerHTML = `<b>Personnages liés</b><p>${e.connexion ? 'Connecte-toi pour partir de tes vrais persos et utiliser tes sacs et banques dans le craft.' : esc(e.message)}</p>
      ${e.connexion ? '<a class="btn braise petit" href="compte.html">Se connecter</a>' : ''}`;
    return;
  }
  b.innerHTML = `<b>Personnages liés</b>${persos.length ? persos.slice(0, 4).map(x => `<div class="pl"><img src="atelier/icones/classes/tete_${x.classe * 10 + (x.sexe ? 1 : 0)}.png" alt="" onerror="this.style.visibility='hidden'"><span>${esc(x.nom)}<small>compte ${esc(x.compte)} · niv. ${x.niveau}</small></span></div>`).join('')
    + (persos.length > 4 ? `<small>+${persos.length - 4} autre(s)</small>` : '') : '<p>Aucun personnage sur tes comptes de jeu.</p>'}
    <button class="btn petit" id="lierPerso" type="button">Lier mon perso</button>`;
  $('#lierPerso').onclick = () => lierPerso(ROUTE.vue === 'craft' ? 'craft' : 'builder');
}

function ajouterStuff(p) {
  saisir('Nouveau stuff', 'Stuff ' + (p.stuffs.length + 1), nom => {
    const base = stuffActif();
    const s = nouveauStuff(nom, base.classe, base.niveau);
    p.stuffs.push(s); p.actif = s.id; sauver(); majEntete(); allerA('builder');
  });
}
function nouveauPerso() {
  saisir('Nouveau personnage', 'Personnage ' + (ETAT.persos.length + 1), nom => {
    const s = nouveauStuff('Stuff principal');
    const p = { id: uid(), nom, stuffs: [s], actif: s.id };
    ETAT.persos.push(p); ETAT.perso = p.id; sauver(); majEntete(); rendre();
  });
}

// ============================================================ routeur
let ROUTE = { vue: 'builder', arg: null };
function allerA(vue, arg) { const h = '#' + vue + (arg != null ? '/' + arg : ''); if (location.hash === h) rendre(); else location.hash = h; }
window.addEventListener('hashchange', rendre);

async function rendre() {
  const [vue, arg] = location.hash.replace(/^#/, '').split('/');
  ROUTE = { vue: VUES[vue] ? vue : 'builder', arg: arg ?? null };
  $$('.side-nav a').forEach(a => a.classList.toggle('on', a.dataset.vue === ROUTE.vue));
  window.scrollTo(0, 0);
  $('#menuFlottant').hidden = true;
  fermerTiroir();
  try { await VUES[ROUTE.vue](ROUTE.arg); }
  catch (e) { console.error(e); VUE.innerHTML = `<div class="at-charge">Impossible d'afficher cette page (${esc(e.message)}).</div>`; }
}

// ---- apparence (moteur de rendu officiel d'Ankama, comme le launcher)
function lookDe(s) {
  if (s.look) return s.look;
  const c = D.CLASSE.get(s.classe);
  return (s.sexe ? c?.lookF : c?.lookM) || c?.lookM || '';
}
function urlRendu(look, mode = 'full/1/300_420-10') {
  if (!look || look.length > 2000) return '';
  const hex = [...new TextEncoder().encode(look)].map(x => x.toString(16).padStart(2, '0')).join('');
  return 'https://static.ankama.com/dofus/renderer/look/' + hex + '/' + mode + '.png';
}

// ============================================================ BUILDER
function vueBuilder() {
  const s = stuffActif();
  const cl = D.CLASSE.get(s.classe);
  const t = calculer(s);
  const dispo = pointsTotaux(s), dep = pointsDepenses(s);
  const comp = compareA(s);
  const tc = comp ? calculer(comp) : null;
  const caseHtml = e => {
    const v = s.slots[e.k], o = v && D.OBJ.get(v.id);
    return `<div class="case ${o ? 'plein' : ''} ${v?.exo || v?.jets ? 'exo' : ''}" data-empl="${e.k}" title="${esc(o ? o.n : e.n)}">${o
      ? `<img class="obj" src="${icoObjet(o)}" alt=""><span class="lv">${o.l}</span>` : `<span class="fond">${esc(e.n)}</span>`}</div>`;
  };
  VUE.innerHTML = `<div class="col3">
    <section class="carte">
      <h3>Personnage</h3>
      <div class="rangee" style="margin-bottom:10px">
        <div class="segment" id="bSexe" style="flex:0 0 auto" title="Apparence">${['♂', '♀'].map((x, i) => `<button type="button" data-v="${i}" class="${(s.sexe | 0) === i ? 'on' : ''}" style="width:30px">${x}</button>`).join('')}</div>
        <select class="champ" id="bClasse" style="flex:1">${[...D.CLASSE.values()].map(c => `<option value="${c.id}" ${c.id === s.classe ? 'selected' : ''}>${esc(c.n)}</option>`).join('')}</select>
        <input class="champ num" id="bNiveau" type="number" min="1" max="200" value="${s.niveau}" aria-label="Niveau">
      </div>
      <input type="range" id="bNiveauR" min="1" max="200" value="${s.niveau}" aria-label="Niveau">
      <div class="rangee" style="justify-content:space-between;margin-top:8px;font-size:13px"><span class="mini">Points de caractéristiques</span><b class="num" style="color:${dep > dispo ? 'var(--rouge)' : 'inherit'}">${dispo - dep} / ${dispo}</b></div>
      <div class="jauge"><i style="width:${Math.min(100, dep / Math.max(1, dispo) * 100)}%"></i></div>
      <div class="perso-carac">
        <span class="t">Carac.</span><span class="t" style="text-align:right">Base</span><span></span><span class="t" style="text-align:right">Parcho</span>
        ${CARACS6.map(k => `<label style="color:${LIB[k][3]}"><img class="ic" src="${ICO(k)}" alt="">${LIB[k][1]}</label>
          <input class="champ num" data-base="${k}" type="number" min="0" max="9999" value="${s.base[k] || 0}" aria-label="${LIB[k][1]} de base">
          <span class="mini" style="text-align:center">+</span>
          <input class="champ num" data-parcho="${k}" type="number" min="0" max="100" value="${s.parcho[k] || 0}" aria-label="${LIB[k][1]} parchotée">`).join('')}
      </div>
      <div class="rangee" style="margin-top:12px"><button class="btn" id="bParcho">Parchotage complet</button><button class="btn fantome" id="bZero">Remettre à zéro</button></div>
      <div class="titre-sec">Jets des objets</div>
      <div class="segment" id="bRoll">${[['max', 'Jets max'], ['moy', 'Jets moyens'], ['min', 'Jets min']].map(([v, n]) => `<button data-v="${v}" class="${s.roll === v ? 'on' : ''}">${n}</button>`).join('')}</div>
      <p class="mini" style="margin:8px 0 0">Les jets personnalisés et les exos se règlent objet par objet depuis l'équipement.</p>
    </section>

    <section class="carte">
      <div class="stuff-tete">
        <div class="nom"><small>${esc(persoActif().nom)}</small><b id="bNom" title="Renommer">${esc(s.nom)}</b></div>
        <span class="compteur">${t._nb}/16</span>
        <button class="btn fantome" id="bPerso">Partir de mon perso</button>
        <button class="btn" id="bCraft">Crafter ce stuff</button>
        <button class="btn fantome" id="bPlus" aria-label="Plus d'actions">•••</button>
      </div>
      <div class="scene">
        <div class="cases">${EMPL.filter(e => e.c === 'g').map(caseHtml).join('')}</div>
        <div class="portrait podium">
          <img class="fond" src="atelier/icones/classes/bg_${s.classe * 10 + (s.sexe ? 1 : 0)}.jpg" alt="" onerror="this.remove()">
          <img class="rendu" referrerpolicy="no-referrer" src="${urlRendu(lookDe(s))}" alt="Apparence de ${esc(s.look ? persoActif().nom : cl?.n || '')}" onerror="this.remove()">
          <div class="resume"><span style="color:#ff8fab">${fmt(t.pv)}<small>PV</small></span><span style="color:#9fd3ff">${t.pa}<small>PA</small></span><span style="color:#86efac">${t.pm}<small>PM</small></span><span>${t.po}<small>PO</small></span></div>
          <div class="legende">${esc(cl?.n || '')} · niveau ${s.niveau}${s.look ? ' · apparence en jeu' : ''}</div>
        </div>
        <div class="cases">${EMPL.filter(e => e.c === 'd').map(caseHtml).join('')}</div>
      </div>
      <div class="dofus">${EMPL.filter(e => e.c === 'b').map(caseHtml).join('')}</div>
      ${t._panos.length ? `<div class="titre-sec">Panoplies</div>${t._panos.map(x => `<div class="rangee" style="justify-content:space-between;padding:4px 0">
        <a class="lien" href="#panoplies/${x.p.id}">${esc(x.p.n)}</a><span class="mini">${x.n} / ${x.p.pieces.length} pièces ${x.compte ? '' : (s.off || []).includes(x.p.id) ? '· bonus ignorés' : ''}</span></div>`).join('')}` : ''}
    </section>

    <section class="carte c3">
      <div class="rangee" style="justify-content:space-between;margin-bottom:12px"><h3 style="margin:0">Caractéristiques</h3>
        <select class="champ" id="bComp" style="height:30px;font-size:12.5px">
          <option value="">Sans comparaison</option>
          ${persoActif().stuffs.filter(x => x !== s).map(x => `<option value="${x.id}" ${comp === x ? 'selected' : ''}>vs ${esc(x.nom)}</option>`).join('')}
        </select></div>
      ${blocStats(t, tc)}
      <div class="titre-sec">Radar du build</div>
      <p class="mini" style="margin:0 0 6px">Forces et faiblesses par rapport à un stuff de référence de ton niveau.</p>
      ${radar(t, s.niveau)}
    </section>
  </div>`;

  // --- événements
  const changer = f => { f(); touche(s); vueBuilder(); };
  $('#bClasse').onchange = e => changer(() => { s.classe = +e.target.value; delete s.look; });
  $$('#bSexe button').forEach(b => b.onclick = () => changer(() => { s.sexe = +b.dataset.v; delete s.look; }));
  const niv = v => changer(() => s.niveau = Math.min(200, Math.max(1, v | 0)));
  $('#bNiveau').onchange = e => niv(e.target.value);
  $('#bNiveauR').onchange = e => niv(e.target.value);
  $('#bNiveauR').oninput = e => { $('#bNiveau').value = e.target.value; };
  $$('[data-base]').forEach(i => i.onchange = () => changer(() => s.base[i.dataset.base] = Math.max(0, i.value | 0)));
  $$('[data-parcho]').forEach(i => i.onchange = () => changer(() => s.parcho[i.dataset.parcho] = Math.min(100, Math.max(0, i.value | 0))));
  $('#bParcho').onclick = () => changer(() => CARACS6.forEach(k => s.parcho[k] = 100));
  $('#bZero').onclick = () => changer(() => CARACS6.forEach(k => { s.base[k] = 0; s.parcho[k] = 0; }));
  $$('#bRoll button').forEach(b => b.onclick = () => changer(() => s.roll = b.dataset.v));
  $('#bComp').onchange = e => { s.comp = e.target.value || null; sauver(); vueBuilder(); };
  $('#bNom').onclick = () => saisir('Renommer le stuff', s.nom, n => changer(() => s.nom = n));
  $('#bCraft').onclick = () => { ETAT.craftMode = 'stuff'; sauver(); allerA('craft'); };
  $('#bPerso').onclick = () => lierPerso('builder');
  $('#bPlus').onclick = e => menu(e.currentTarget, [
    { t: 'Copier le lien de partage', f: () => copier(lienStuff(s), 'Lien du build copié') },
    { t: 'Copier le code du build', f: () => copier(encoder(s), 'Code du build copié') },
    { t: 'Dupliquer ce stuff', f: () => { const c = JSON.parse(JSON.stringify(s)); c.id = uid(); c.nom = s.nom + ' (copie)'; persoActif().stuffs.push(c); persoActif().actif = c.id; touche(c); vueBuilder(); } },
    { sep: true },
    { t: "Vider l'équipement", danger: true, f: () => changer(() => s.slots = {}) },
  ]);
  $$('.case').forEach(c => c.onclick = () => choisirObjet(c.dataset.empl));
}
function compareA(s) { return s.comp ? persoActif().stuffs.find(x => x.id === s.comp && x !== s) || null : null; }

function blocStats(t, tc) {
  const v = k => {
    const x = t[k] || 0;
    const d = tc ? x - (tc[k] || 0) : 0;
    return `<b class="num ${x ? '' : 'z'}">${fmt(x)}${k === 'crit' ? '%' : ''}</b>${d ? `<span class="diff ${d > 0 ? 'plus' : 'moins'}">${signe(d)}</span>` : ''}`;
  };
  const tuile = k => `<div class="tuile"><img src="${ICO(k)}" alt=""><b class="num" style="color:${LIB[k][3]}">${fmt(t[k] || 0)}${k === 'crit' ? '%' : ''}</b><small>${LIB[k][0]}</small>${tc && (t[k] || 0) !== (tc[k] || 0) ? `<span class="diff ${(t[k] || 0) > (tc[k] || 0) ? 'plus' : 'moins'}">${signe((t[k] || 0) - (tc[k] || 0))}</span>` : ''}</div>`;
  const ligne = k => `<div><img class="ic" src="${ICO(k)}" alt=""><span>${LIB[k][1]}</span>${v(k)}</div>`;
  const el = [['Terre', 'Terre', 'var(--terre)'], ['Feu', 'Feu', 'var(--feu)'], ['Eau', 'Eau', 'var(--eau)'], ['Air', 'Air', 'var(--air)'], ['Neutre', 'Neutre', 'var(--neutre)']];
  return `<div class="tuiles">${['pv', 'pa', 'pm', 'po', 'ini', 'pp', 'invoc', 'crit'].map(tuile).join('')}</div>
    <div class="titre-sec">Caractéristiques</div><div class="stats">${['vita', 'sag', 'for', 'int', 'cha', 'agi'].map(ligne).join('')}</div>
    <div class="titre-sec">Dommages</div><div class="stats">${['puis', 'do', 'doTerre', 'doFeu', 'doEau', 'doAir', 'doNeutre', 'doCrit', 'soins', 'doPou'].map(ligne).join('')}</div>
    <div class="titre-sec">Résistances</div>
    <table class="res"><tr><th></th>${el.map(([, n, c]) => `<th style="color:${c}">${n}</th>`).join('')}</tr>
      <tr><td>%</td>${el.map(([k]) => `<td class="num">${fmt(t['r' + k + 'P'] || 0)}</td>`).join('')}</tr>
      <tr><td>Fixe</td>${el.map(([k]) => `<td class="num">${fmt(t['r' + k] || 0)}</td>`).join('')}</tr></table>
    <div class="stats" style="margin-top:8px">${['rCrit', 'rPou'].map(ligne).join('')}</div>
    <div class="titre-sec">Tacle, fuite, retraits</div><div class="stats">${['tacle', 'fuite', 'retPA', 'retPM', 'esqPA', 'esqPM'].map(ligne).join('')}</div>
    <div class="titre-sec">Divers</div><div class="stats">${['pods', 'renvoi'].map(ligne).join('')}</div>`;
}

function radar(t, niveau) {
  const n = Math.max(1, niveau);
  const axes = [
    ['Vitalité', t.vita / (n * 12)], ['Sagesse', t.sag / (n * 1.6)], ['Force', t.for / (n * 2.2)], ['Intelligence', t.int / (n * 2.2)],
    ['Chance', t.cha / (n * 2.2)], ['Agilité', t.agi / (n * 2.2)],
    ['Dommages', ((t.do || 0) + Math.max(t.doTerre || 0, t.doFeu || 0, t.doEau || 0, t.doAir || 0) + (t.puis || 0) / 10) / (n * 0.45)],
    ['Résistances', (['Terre', 'Feu', 'Eau', 'Air', 'Neutre'].reduce((a, k) => a + (t['r' + k + 'P'] || 0), 0) / 5) / 30],
    ['PA / PM', ((t.pa - 6) + (t.pm - 3)) / 6],
  ];
  const W = 300, C = W / 2, R = 104;
  const pt = (i, r) => { const a = -Math.PI / 2 + i * 2 * Math.PI / axes.length; return [C + Math.cos(a) * r, C + Math.sin(a) * r]; };
  const grille = [0.25, 0.5, 0.75, 1].map(f => `<polygon points="${axes.map((_, i) => pt(i, R * f).join(',')).join(' ')}" fill="none" stroke="#2f2645" stroke-width="1"/>`).join('');
  const rayons = axes.map((_, i) => `<line x1="${C}" y1="${C}" x2="${pt(i, R)[0]}" y2="${pt(i, R)[1]}" stroke="#2f2645"/>`).join('');
  const forme = axes.map(([, v], i) => pt(i, R * Math.max(0.02, Math.min(1.15, v || 0))).join(',')).join(' ');
  const noms = axes.map(([n], i) => { const [x, y] = pt(i, R + 20); return `<text x="${x}" y="${y}" fill="#8f86a3" font-size="10.5" text-anchor="middle" dominant-baseline="middle">${n}</text>`; }).join('');
  return `<svg class="radar" viewBox="0 0 ${W} ${W}" width="100%" style="max-width:320px" role="img" aria-label="Radar du build">
    <defs><linearGradient id="rg" x1="0" x2="1"><stop offset="0" stop-color="#a9cbff"/><stop offset=".5" stop-color="#8b5cf6"/><stop offset="1" stop-color="#ff6a4a"/></linearGradient></defs>
    ${grille}${rayons}<polygon points="${forme}" fill="rgba(139,92,246,.25)" stroke="url(#rg)" stroke-width="2"/>${noms}</svg>`;
}

// ---- tiroir de choix d'objet
function objetsPour(empl) {
  const e = EMPL_K.get(empl);
  return D.objets.filter(o => e.st.includes(superType(o)) && !TYPES_EXCLUS.has(o.t));
}
function choisirObjet(empl) {
  const s = stuffActif();
  const e = EMPL_K.get(empl);
  const tous = objetsPour(empl);
  const base = calculer(s);
  const filtre = { q: '', carac: '', tri: 'niveau', niv: true };
  const actuel = s.slots[empl] && D.OBJ.get(s.slots[empl].id);
  ouvrirTiroir(`<div class="tiroir-tete"><h3>${esc(e.n)}</h3><p id="tCompte"></p><button class="btn fantome petit tiroir-fermer" id="tFermer" aria-label="Fermer">✕</button></div>
    ${actuel ? `<div class="equipe">${imgObjet(actuel)}<div style="flex:1;min-width:0"><b>${esc(actuel.n)}</b><div class="mini">Équipé · niveau ${actuel.l}</div></div>
      <button class="btn" id="tJets">Jets</button><button class="btn danger" id="tRetirer">Retirer</button></div>` : ''}
    <div style="padding:12px 18px 6px" class="filtres">
      <div class="recherche"><input class="champ" id="tQ" placeholder="Nom de l'objet ou de la panoplie…" autocomplete="off"></div>
      <div class="rangee">
        <select class="champ" id="tCarac" style="flex:1"><option value="">Avec la carac…</option>${ORDRE_PASTILLES.filter(k => k !== 'pv').map(k => `<option value="${k}">${esc(LIB[k][1])}</option>`).join('')}</select>
        <select class="champ" id="tTri"><option value="niveau">Tri : niveau</option><option value="gain">Tri : gain sur le build</option><option value="nom">Tri : nom</option></select>
        <span class="inter on" id="tNiv"><i></i>Niveau ≤ ${s.niveau}</span>
      </div>
    </div>
    <div class="tiroir-corps" id="tListe"></div>`, m => {
    const liste = $('#tListe', m);
    const score = d => ORDRE_PASTILLES.reduce((a, k) => a + (d[k] || 0) * ({ pa: 100, pm: 90, po: 40, invoc: 40, crit: 8, puis: 1, do: 6, vita: 0.25, pv: 0 }[k] ?? 1), 0);
    const dessiner = () => {
      const q = norm(filtre.q);
      let l = tous.filter(o => (!filtre.niv || o.l <= s.niveau) && (!q || norm(o.n).includes(q) || (o.s && norm(D.PANO.get(o.s)?.n).includes(q))));
      const avec = l.map(o => {
        const t2 = calculer(s, { [empl]: { id: o.id } });
        return { o, d: differences(base, t2) };
      }).filter(x => !filtre.carac || (effetsObjet(x.o, null, 'max')[filtre.carac] || 0) > 0);
      if (filtre.tri === 'niveau') avec.sort((a, b) => b.o.l - a.o.l || a.o.n.localeCompare(b.o.n));
      else if (filtre.tri === 'nom') avec.sort((a, b) => a.o.n.localeCompare(b.o.n));
      else avec.sort((a, b) => score(b.d) - score(a.d));
      $('#tCompte', m).textContent = avec.length + ' objet(s) · les pastilles indiquent l\'effet sur ton build (panoplies comprises)';
      liste.innerHTML = avec.slice(0, 150).map(({ o, d }) => `<div class="choix" data-id="${o.id}">${imgObjet(o)}<div class="txt">
        <b>${esc(o.n)}</b> <span class="mini">niv. ${o.l}</span>${actuel?.id === o.id ? ' <span class="niv">équipé</span>' : ''}
        ${o.s && D.PANO.has(o.s) ? `<span class="pano">${esc(D.PANO.get(o.s).n)}</span>` : '<span style="display:block;height:5px"></span>'}
        <div class="effs">${actuel?.id === o.id ? '<span class="mini">Aucun changement</span>' : pastilles(d)}</div></div></div>`).join('')
        + (avec.length > 150 ? `<div class="vide">${avec.length - 150} autres : affine la recherche.</div>` : '') + (avec.length ? '' : '<div class="vide">Aucun objet.</div>');
    };
    dessiner();
    $('#tQ', m).oninput = e => { filtre.q = e.target.value; dessiner(); };
    $('#tQ', m).focus();
    $('#tCarac', m).onchange = e => { filtre.carac = e.target.value; dessiner(); };
    $('#tTri', m).onchange = e => { filtre.tri = e.target.value; dessiner(); };
    $('#tNiv', m).onclick = e => { filtre.niv = !filtre.niv; e.currentTarget.classList.toggle('on', filtre.niv); dessiner(); };
    $('#tFermer', m).onclick = fermerTiroir;
    if (actuel) {
      $('#tRetirer', m).onclick = () => { delete s.slots[empl]; touche(s); fermerTiroir(); vueBuilder(); };
      $('#tJets', m).onclick = () => reglerJets(empl);
    }
    liste.onclick = ev => {
      const c = ev.target.closest('.choix');
      if (!c) return;
      equiper(s, empl, +c.dataset.id);
    };
  });
}
function equiper(s, empl, id, silencieux) {
  const o = D.OBJ.get(id);
  const e = EMPL_K.get(empl);
  // même anneau ou même Dofus deux fois : interdit en jeu
  const double = Object.entries(s.slots).find(([k, v]) => k !== empl && v.id === id && EMPL_K.get(k).st[0] === e.st[0]);
  if (double && (e.st[0] === 3 || e.st[0] === 13)) { if (!silencieux) toast('Cet objet est déjà équipé.'); return false; }
  s.slots[empl] = { id };
  touche(s);
  if (!silencieux) { fermerTiroir(); vueBuilder(); toast(o.n + ' équipé'); }
  return true;
}

function reglerJets(empl) {
  const s = stuffActif();
  const v = s.slots[empl];
  const o = D.OBJ.get(v.id);
  const effs = (o.e || []).map((e, i) => ({ e, i, st: statEffet(e[0]) })).filter(x => x.st);
  const exos = ['pa', 'pm', 'po', 'invoc', 'crit', 'puis', 'do', 'vita', 'sag', 'for', 'int', 'cha', 'agi', 'ini', 'pp', 'soins', 'doCrit', 'rCrit', 'rPou', 'doPou', 'tacle', 'fuite',
    'retPA', 'retPM', 'esqPA', 'esqPM', 'doTerre', 'doFeu', 'doEau', 'doAir', 'doNeutre', 'rTerreP', 'rFeuP', 'rEauP', 'rAirP', 'rNeutreP', 'rTerre', 'rFeu', 'rEau', 'rAir', 'rNeutre'];
  const effetDe = k => Object.keys(D.EFF).find(id => CAR[D.EFF[id][1]] === k && D.EFF[id][2] === '+' && D.EFF[id][5]);
  ouvrirModale(`<h3>${imgObjet(o, 'p')}${esc(o.n)}</h3>
    <p class="mini" style="margin:6px 0 14px">Ajuste chaque jet (vide = jet « ${s.roll === 'max' ? 'max' : s.roll === 'min' ? 'min' : 'moyen'} » du stuff) et ajoute un exo ou une forgemagie.</p>
    <div class="titre-sec" style="margin-top:0">Jets</div>
    <div class="jets">${effs.map(({ e, i, st }) => `<span style="color:${LIB[st.k][3]};display:flex;gap:6px;align-items:center"><img class="ic" src="${ICO(st.k)}" alt="">${esc(LIB[st.k][1])}</span>
      <span class="plage">${e[2] ? e[1] + ' à ' + e[2] : e[1]}</span>
      <input class="champ num" data-j="${i}" type="number" placeholder="${jet(e, s.roll)}" value="${v.jets?.[i] ?? ''}" style="width:70px">
      <button class="btn fantome petit" data-r="${i}" title="Remettre">↺</button>`).join('') || '<span class="mini">Aucun jet réglable.</span>'}</div>
    <div class="titre-sec">Exo / forgemagie</div>
    <div class="rangee"><select class="champ" id="jExo" style="flex:1"><option value="">Aucun exo</option>${exos.map(k => { const id = effetDe(k); return id ? `<option value="${id}" ${v.exo?.[0] == id ? 'selected' : ''}>${esc(LIB[k][1])}</option>` : ''; }).join('')}</select>
      <input class="champ num" id="jExoV" type="number" value="${v.exo?.[1] ?? 0}" style="width:70px"></div>
    <div class="rangee" style="justify-content:flex-end;margin-top:18px"><button class="btn fantome" id="jRaz">Tout réinitialiser</button><button class="btn violet" id="jOk">Terminé</button></div>`, m => {
    $$('[data-r]', m).forEach(b => b.onclick = () => { $(`[data-j="${b.dataset.r}"]`, m).value = ''; });
    $('#jRaz', m).onclick = () => { $$('[data-j]', m).forEach(i => i.value = ''); $('#jExo', m).value = ''; $('#jExoV', m).value = 0; };
    $('#jOk', m).onclick = () => {
      const jets = [];
      let perso = false;
      $$('[data-j]', m).forEach(i => { if (i.value !== '') { jets[+i.dataset.j] = +i.value; perso = true; } });
      if (perso) v.jets = Array.from({ length: (o.e || []).length }, (_, i) => jets[i] ?? null); else delete v.jets;
      const ex = $('#jExo', m).value;
      if (ex && +$('#jExoV', m).value) v.exo = [+ex, +$('#jExoV', m).value]; else delete v.exo;
      touche(s); fermerModale(); fermerTiroir(); vueBuilder();
    };
  });
}

// ============================================================ PANOPLIES
const ETAT_PANO = { q: '', min: 1, max: 200, tri: 'niveau', portees: false };
const niveauPano = p => Math.max(0, ...p.pieces.map(i => D.OBJ.get(i)?.l || 0));
function vuePanoplies(arg) {
  const s = stuffActif();
  const portees = new Set(Object.values(s.slots).map(v => D.OBJ.get(v.id)?.s).filter(Boolean));
  const id = arg ? +arg : null;
  VUE.innerHTML = `<div class="col2">
    <aside class="carte collant">
      <div class="filtres">
        <div class="recherche"><input class="champ" id="pQ" placeholder="Rechercher parmi ${D.PANO.size} panoplies…" value="${esc(ETAT_PANO.q)}"></div>
        <div class="rangee"><span class="mini">Niv.</span><input class="champ num" id="pMin" type="number" value="${ETAT_PANO.min}"><span class="mini">à</span><input class="champ num" id="pMax" type="number" value="${ETAT_PANO.max}">
          <select class="champ" id="pTri"><option value="niveau">Par niveau</option><option value="nom" ${ETAT_PANO.tri === 'nom' ? 'selected' : ''}>Par nom</option></select></div>
        <span class="inter ${ETAT_PANO.portees ? 'on' : ''}" id="pPortees"><i></i>Portées dans ce stuff</span>
      </div>
      <div class="liste" id="pListe"></div>
    </aside>
    <section id="pFiche"></section></div>`;
  const dessiner = () => {
    const q = norm(ETAT_PANO.q);
    const l = [...D.PANO.values()].filter(p => p.pieces.length && (!q || norm(p.n).includes(q)) && (!ETAT_PANO.portees || portees.has(p.id)))
      .map(p => ({ p, l: niveauPano(p) })).filter(x => x.l >= ETAT_PANO.min && x.l <= ETAT_PANO.max)
      .sort((a, b) => ETAT_PANO.tri === 'nom' ? a.p.n.localeCompare(b.p.n) : b.l - a.l || a.p.n.localeCompare(b.p.n));
    $('#pListe').innerHTML = l.map(({ p, l }) => `<div class="ligne ${p.id === id ? 'on' : ''}" data-id="${p.id}"><div class="txt"><b>${esc(p.n)}</b>
      <div class="rangee" style="gap:3px;margin-top:3px">${p.pieces.slice(0, 7).map(i => `<img src="${icoObjet(D.OBJ.get(i))}" alt="" style="width:20px;height:20px" loading="lazy">`).join('')}</div></div><span class="niv">niv. ${l}</span></div>`).join('') || '<div class="vide">Aucune panoplie.</div>';
  };
  dessiner();
  $('#pQ').oninput = e => { ETAT_PANO.q = e.target.value; dessiner(); };
  $('#pMin').onchange = e => { ETAT_PANO.min = +e.target.value || 1; dessiner(); };
  $('#pMax').onchange = e => { ETAT_PANO.max = +e.target.value || 200; dessiner(); };
  $('#pTri').onchange = e => { ETAT_PANO.tri = e.target.value; dessiner(); };
  $('#pPortees').onclick = e => { ETAT_PANO.portees = !ETAT_PANO.portees; e.currentTarget.classList.toggle('on'); dessiner(); };
  $('#pListe').onclick = e => { const l = e.target.closest('.ligne'); if (l) allerA('panoplies', l.dataset.id); };
  const p = id && D.PANO.get(id);
  if (!p) { $('#pFiche').innerHTML = `<div class="carte"><h3>Panoplies</h3><p class="mini">${D.PANO.size} panoplies du serveur : choisis-en une pour voir ses pièces et ses bonus, l'équiper d'un coup ou l'envoyer dans l'atelier de craft.</p></div>`; return; }
  const comptees = !(s.off || []).includes(p.id);
  const dans = p.pieces.filter(i => Object.values(s.slots).some(v => v.id === i)).length;
  const total = {};
  (p.bonus[p.pieces.length - 1] || []).forEach(([e, v]) => { const st = statEffet(e); if (st) total[st.k] = (total[st.k] || 0) + st.s * v; });
  p.pieces.forEach(i => { const r = effetsObjet(D.OBJ.get(i), null, 'max'); for (const k in r) total[k] = (total[k] || 0) + r[k]; });
  $('#pFiche').innerHTML = `<div class="carte">
    <div class="fiche-tete"><div><h2>${esc(p.n)}</h2><div class="sous">Niveau ${niveauPano(p)} · ${p.pieces.length} pièces · ${dans} portée(s) dans « ${esc(s.nom)} »</div></div>
      <div class="act"><button class="btn violet" id="pEquiper">Équiper la panoplie</button><button class="btn" id="pCraft">Ajouter au craft (${p.pieces.length})</button></div></div>
    <span class="inter ${comptees ? 'on' : ''}" id="pCompter"><i></i>Compter les bonus de cette panoplie dans le build</span>
    <div class="pieces" style="margin-top:14px">${p.pieces.map(i => { const o = D.OBJ.get(i); const r = D.REC.get(i); return `<div class="piece" data-objet="${i}">${imgObjet(o)}<div class="txt"><b>${esc(o.n)}</b><small>${esc(nomType(o))} · niv. ${o.l}${r ? ' · ' + esc(D.METIER.get(r.metier) || '') : ''}</small></div></div>`; }).join('')}</div>
  </div>
  <div class="carte" style="margin-top:14px"><div class="titre-sec" style="margin-top:0">Bonus par palier</div>
    ${p.bonus.map((b, i) => b.length ? `<div class="palier"><span>${i + 1} pièces</span><div class="effs">${b.map(([e, v]) => { const st = statEffet(e); return st ? pastille(st.k, st.s * v, false) : `<span class="eff">${esc(texteEffet(e, v))}</span>`; }).join('')}</div></div>` : '').join('') || '<div class="vide">Aucun bonus.</div>'}
  </div>
  <div class="carte" style="margin-top:14px"><div class="titre-sec" style="margin-top:0">Panoplie complète (jets max + bonus max)</div><div class="effs">${pastilles(Object.fromEntries(ORDRE_PASTILLES.filter(k => total[k]).map(k => [k, total[k]])), 60)}</div></div>`;
  $('#pCompter').onclick = () => { s.off = comptees ? [...(s.off || []), p.id] : (s.off || []).filter(x => x !== p.id); touche(s); vuePanoplies(arg); };
  $('#pEquiper').onclick = () => {
    let n = 0;
    p.pieces.forEach(i => {
      const o = D.OBJ.get(i);
      const libres = EMPL.filter(e => e.st.includes(superType(o)));
      const cible = libres.find(e => !s.slots[e.k] || D.OBJ.get(s.slots[e.k].id)?.s !== p.id) || libres[0];
      if (cible && !Object.values(s.slots).some(v => v.id === i) && equiper(s, cible.k, i, true)) n++;
    });
    toast(n + ' pièce(s) équipée(s)'); allerA('builder');
  };
  $('#pCraft').onclick = () => { p.pieces.forEach(i => ajouterListe(i, 1)); ETAT.craftMode = 'perso'; sauver(); toast('Panoplie ajoutée au craft'); allerA('craft'); };
  $$('.piece').forEach(x => x.onclick = () => allerA('encyclopedie', x.dataset.objet));
}

// ============================================================ ATELIER DE CRAFT
function ajouterListe(id, n) {
  const l = ETAT.liste.find(x => x[0] === id);
  if (l) l[1] += n; else ETAT.liste.push([id, n]);
}
function objetsACrafter() {
  if (ETAT.craftMode === 'perso') return ETAT.liste.filter(([id]) => D.OBJ.has(id)).map(([id, n]) => [id, n]);
  const m = ETAT.craftExemplaires || 1;
  const c = new Map();
  Object.values(stuffActif().slots).forEach(v => c.set(v.id, (c.get(v.id) || 0) + m));
  return [...c];
}
/** Décompose en ressources de base : les intermédiaires craftables sont fabriqués sauf s'ils sont marqués « achetés » ou déjà possédés. */
function possedeTotal(id, lie) { return (+ETAT.possede[id] || 0) + (lie?.get(id) || 0); }
function besoins(liste, lie) {
  const total = new Map();
  const possede = new Map();
  for (const k of new Set([...Object.keys(ETAT.possede).map(Number), ...(lie ? lie.keys() : [])])) possede.set(k, possedeTotal(k, lie));
  const achetes = new Set(ETAT.achetes);
  const visite = (id, n, prof) => {
    const r = D.REC.get(id);
    if (!r || achetes.has(id) || prof > 6 || prof > 0 && !r) { total.set(id, (total.get(id) || 0) + n); return; }
    if (prof > 0) {
      const p = possede.get(id) || 0;
      const pris = Math.min(p, n);
      if (pris) { possede.set(id, p - pris); total.set(id, (total.get(id) || 0) + pris); n -= pris; }
      if (!n) return;
    }
    r.ing.forEach(([i, q]) => visite(i, q * n, prof + 1));
  };
  liste.forEach(([id, n]) => { if (D.REC.has(id)) visite(id, n, 0); else total.set(id, (total.get(id) || 0) + n); });
  return total;
}
async function vueCraft(relire) {
  ETAT.craftMode ||= 'stuff';
  ETAT.craftExemplaires ||= 1;
  ETAT.stock ||= { inv: true, banque: true, persos: [], tous: true };
  if (ETAT.stock.tous == null) ETAT.stock.tous = true;
  const defile = window.scrollY;
  const lie = await stockLie(relire === true);
  DERNIER_STOCK = lie.t;
  cacherBulle();
  const liste = objetsACrafter();
  const tot = besoins(liste, lie.total);
  const res = [...tot].map(([id, n]) => ({ o: D.OBJ.get(id), n, p: Math.min(n, possedeTotal(id, lie.total)), l: lie.total.get(id) || 0 })).filter(x => x.o);
  const requis = res.reduce((a, x) => a + x.n, 0), reunis = res.reduce((a, x) => a + x.p, 0);
  const masquer = !!ETAT.masquerReunis;
  const comptes = new Set([...lie.detail.values()].flat().filter(x => x.t === 'banque').map(x => x.compte));
  // avancement de chaque objet à forger (sa part de ressources déjà réunies)
  const avance = id => {
    const b = besoins(liste.filter(x => x[0] === id), lie.total);
    let r = 0, q = 0;
    b.forEach((n, i) => { q += n; r += Math.min(n, possedeTotal(i, lie.total)); });
    return q ? Math.round(r / q * 100) : 100;
  };
  const anneau = v => `<span class="rond" style="--v:${v};--c:${v >= 100 ? 'var(--vert)' : v >= 50 ? 'var(--braise)' : 'var(--rouge)'}"><span>${v >= 100 ? '✓' : v + '%'}</span></span>`;
  const chips = x => {
    const src = lie.detail.get(x.o.id) || [];
    return src.filter(y => y.t === 'perso').sort((a, b) => b.n - a.n).map(y => `<span class="chip" data-src="${x.o.id}" title="${esc(y.nom)}"><img src="atelier/icones/classes/tete_${y.classe * 10 + (y.sexe ? 1 : 0)}.png" alt="" onerror="this.style.visibility='hidden'">${fmt(y.n)}</span>`).join('')
      + src.filter(y => y.t === 'banque').map(y => `<span class="chip bq" data-src="${x.o.id}">🏦 ${fmt(y.n)}</span>`).join('');
  };
  const lignes = res.filter(x => !masquer || x.p < x.n).sort((a, b) => (a.p >= a.n) - (b.p >= b.n) || a.o.n.localeCompare(b.o.n));
  VUE.innerHTML = `<section class="etabli">
    <div class="eta-tete"><h2>⚒ Établi de craft</h2>
      <span class="pill ${ETAT.stock.inv ? 'on' : ''}" id="cInv">Inventaires · ${lie.nb} perso${lie.nb > 1 ? 's' : ''}</span>
      <span class="pill ${ETAT.stock.banque ? 'on' : ''}" id="cBanque">Banques · ${comptes.size} compte${comptes.size > 1 ? 's' : ''}</span>
      <span class="pill ${ETAT.stock.tous ? 'on' : ''}" id="cTous">Tous mes persos</span>
      ${ETAT.stock.tous ? '' : '<span class="pill" id="cPersos">Choisir les persos…</span>'}
      <div class="segment petit" id="cMode"><button data-v="stuff" class="${ETAT.craftMode === 'stuff' ? 'on' : ''}" type="button">Stuff actif</button><button data-v="perso" class="${ETAT.craftMode === 'perso' ? 'on' : ''}" type="button">Liste perso</button></div>
      <div class="eta-droite">${lie.connexion ? '<a class="btn braise petit" href="compte.html">Se connecter</a>' : ''}${lie.erreur ? `<span class="mini" style="color:var(--rouge)">${esc(lie.erreur)}</span>` : ''}
        ${lie.nb ? `<span class="direct"><i></i>En direct · relu <span id="cMaj">à l'instant</span></span><button class="btn fantome petit" id="cRafraichir" title="Relire maintenant" type="button">↻</button>` : ''}</div>
    </div>
    <div class="eta-grille">
      <div class="eta-objets">
        <div class="t2">À FORGER ${ETAT.craftMode === 'stuff' ? `<span class="mini" style="letter-spacing:0;font:400 12px var(--f-texte)">× <input class="champ num" id="cEx" type="number" min="1" value="${ETAT.craftExemplaires}" style="width:52px;height:26px"> « ${esc(stuffActif().nom)} »</span>` : ''}</div>
        ${liste.map(([id, n]) => { const o = D.OBJ.get(id); const r = D.REC.get(id); return `<div class="obj">${imgObjet(o)}<div class="n"><b class="lien" data-objet="${id}">${esc(o.n)}</b><small>${r ? esc(D.METIER.get(r.metier) || '') : 'pas de recette'} · ×${n}</small>
          ${ETAT.craftMode === 'perso' ? `<span class="rangee" style="gap:4px;margin-top:4px"><input class="champ num" data-q="${id}" type="number" min="1" value="${n}" style="width:54px;height:24px"><button class="btn fantome petit" data-x="${id}" type="button">Retirer</button></span>` : ''}</div>${r ? anneau(avance(id)) : ''}</div>`; }).join('')
          || `<div class="vide">${ETAT.craftMode === 'stuff' ? 'Le stuff actif est vide : équipe des objets dans « Mon stuff ».' : 'Ajoute des objets à forger.'}</div>`}
        ${ETAT.craftMode === 'perso' ? `<div class="rangee"><button class="btn petit" id="cAjouter" type="button">+ Ajouter un objet</button>${liste.length ? '<button class="btn fantome petit" id="cVider" type="button">Vider</button>' : ''}</div>` : ''}
        <details class="arbre-bloc" ${ETAT.arbreOuvert ? 'open' : ''}><summary>Arbre de fabrication</summary>
          <div class="rangee" style="justify-content:space-between;margin:8px 0"><span class="mini">« Acheté » : l'intermédiaire est acheté au lieu d'être fabriqué.</span><span class="inter ${ETAT.toutDeplier ? 'on' : ''}" id="cDeplier"><i></i>Tout déplier</span></div>
          <div class="arbre">${liste.map(([id, n]) => arbre(id, n, 0)).join('') || '<div class="vide">Rien à fabriquer.</div>'}</div></details>
      </div>
      <div class="eta-table">
        <div class="rangee" style="justify-content:space-between;margin-bottom:6px"><span class="mini">${res.length} ressource(s) · ${fmt(requis - reunis)} unité(s) à trouver</span>
          <span class="rangee"><span class="inter ${masquer ? 'on' : ''}" id="cMasquer"><i></i>Masquer ce qui est réuni</span><button class="btn petit" id="cCopier" type="button">Copier la liste</button></span></div>
        <div class="jauge"><i style="width:${requis ? reunis / requis * 100 : 0}%"></i></div>
        <div class="tr h"><span>RESSOURCE</span><span>REQUIS</span><span>OÙ ELLE EST</span><span>MANQUE</span></div>
        ${lignes.map(x => { const m = x.n - x.p; return `<div class="tr ${m ? '' : 'ok'}"><div class="r">${imgObjet(x.o, 'p')}<span class="lien" data-objet="${x.o.id}">${esc(x.o.n)}</span></div><span class="num">${fmt(x.n)}</span>
          <div class="src">${chips(x)}<label class="chip note" title="Quantité notée à la main"><span>✎</span><input class="champ num" data-p="${x.o.id}" type="number" min="0" value="${+ETAT.possede[x.o.id] || 0}"></label></div>
          <span class="manque">${m ? `<b class="moins">${fmt(m)}</b><button class="btn fantome petit" data-ok="${x.o.id}" data-n="${Math.max(0, x.n - x.l)}" title="Marquer comme réuni" type="button">✓</button>` : '<b class="plus">✓</b>'}</span></div>`; }).join('') || '<div class="vide">Rien à réunir.</div>'}
        ${lie.nb ? '<p class="mini" style="margin:12px 0 0">Survole une pastille pour voir sur quel perso ou dans quelle banque se trouve la ressource. Le serveur enregistre sacs et banques toutes les 3 minutes (et à la déconnexion).</p>' : ''}
      </div>
    </div></section>`;

  $$('#cMode button').forEach(b => b.onclick = () => { ETAT.craftMode = b.dataset.v; sauver(); vueCraft(); });
  $('#cEx')?.addEventListener('change', e => { ETAT.craftExemplaires = Math.max(1, e.target.value | 0); sauver(); vueCraft(); });
  $('#cAjouter')?.addEventListener('click', () => chercherObjet('Ajouter à forger', o => D.REC.has(o.id), o => { ajouterListe(o.id, 1); sauver(); vueCraft(); }));
  $('#cVider')?.addEventListener('click', () => { ETAT.liste = []; sauver(); vueCraft(); });
  $$('[data-q]').forEach(i => i.onchange = () => { const l = ETAT.liste.find(x => x[0] === +i.dataset.q); if (l) l[1] = Math.max(1, i.value | 0); sauver(); vueCraft(); });
  $$('[data-x]').forEach(b => b.onclick = () => { ETAT.liste = ETAT.liste.filter(x => x[0] !== +b.dataset.x); sauver(); vueCraft(); });
  $$('[data-p]').forEach(i => i.onchange = () => { ETAT.possede[i.dataset.p] = Math.max(0, i.value | 0); sauver(); vueCraft(); });
  $$('[data-ok]').forEach(b => b.onclick = () => { ETAT.possede[b.dataset.ok] = +b.dataset.n; sauver(); vueCraft(); });
  $('#cMasquer').onclick = () => { ETAT.masquerReunis = !masquer; sauver(); vueCraft(); };
  $('#cInv').onclick = () => { ETAT.stock.inv = !ETAT.stock.inv; sauver(); vueCraft(); };
  $('#cBanque').onclick = () => { ETAT.stock.banque = !ETAT.stock.banque; sauver(); vueCraft(); };
  $('#cPersos')?.addEventListener('click', () => lierPerso('craft'));
  $('#cTous').onclick = () => { ETAT.stock.tous = !ETAT.stock.tous; sauver(); vueCraft(); };
  $('#cRafraichir')?.addEventListener('click', () => vueCraft(true));
  $('#cDeplier').onclick = () => { ETAT.toutDeplier = !ETAT.toutDeplier; sauver(); vueCraft(); };
  $('.arbre-bloc').ontoggle = e => { ETAT.arbreOuvert = e.target.open; sauver(); };
  $$('.arbre .pli').forEach(p => p.onclick = () => { const f = p.closest('.n').nextElementSibling; if (f?.classList.contains('f')) { f.hidden = !f.hidden; p.textContent = f.hidden ? '▸' : '▾'; } });
  $$('[data-achete]').forEach(b => b.onclick = () => { const id = +b.dataset.achete; ETAT.achetes = ETAT.achetes.includes(id) ? ETAT.achetes.filter(x => x !== id) : [...ETAT.achetes, id]; sauver(); vueCraft(); });
  $$('[data-objet]', VUE).forEach(x => x.onclick = () => allerA('encyclopedie', x.dataset.objet));
  $$('.chip[data-src]').forEach(e => {
    const montrer = ev => { ev?.stopPropagation(); bulleStock(e, lie.detail.get(+e.dataset.src) || [], D.OBJ.get(+e.dataset.src)?.n || ''); };
    e.onmouseenter = montrer;
    e.onmouseleave = cacherBulle;
    e.onclick = montrer;
  });
  window.scrollTo(0, defile);
  suivreStock();
  $('#cCopier').onclick = () => copier(res.filter(x => x.n > x.p).sort((a, b) => a.o.n.localeCompare(b.o.n)).map(x => `${x.n - x.p} × ${x.o.n}`).join('\n') || 'Rien à réunir', 'Liste de courses copiée');
}
function arbre(id, n, prof) {
  const o = D.OBJ.get(id);
  if (!o) return '';
  const r = D.REC.get(id);
  const achete = ETAT.achetes.includes(id);
  const ouvert = ETAT.toutDeplier || prof === 0;
  const peut = r && !achete && prof < 6;
  return `<div class="n">${peut ? `<span class="pli">${ouvert ? '▾' : '▸'}</span>` : '<span class="pli"></span>'}<img src="${icoObjet(o)}" alt="" loading="lazy">
    <span class="lien" data-objet="${id}">${esc(o.n)}</span><span class="q">× ${fmt(n)}</span>
    ${r && prof > 0 ? `<button class="btn fantome petit" data-achete="${id}">${achete ? '✓ acheté' : 'acheté ?'}</button>` : ''}
    ${r && prof === 0 ? `<span class="niv">${esc(D.METIER.get(r.metier) || '')}</span>` : ''}</div>
    ${peut ? `<div class="f" ${ouvert ? '' : 'hidden'}>${r.ing.map(([i, q]) => arbre(i, q * n, prof + 1)).join('')}</div>` : ''}`;
}
function chercherObjet(titre, filtre, choisi) {
  ouvrirTiroir(`<div class="tiroir-tete"><h3>${esc(titre)}</h3><p>Objets craftables</p><button class="btn fantome petit tiroir-fermer" id="tFermer">✕</button></div>
    <div style="padding:12px 18px 6px"><div class="recherche"><input class="champ" id="tQ" placeholder="Nom de l'objet…"></div></div><div class="tiroir-corps" id="tListe"></div>`, m => {
    const tous = D.objets.filter(filtre);
    const dessiner = q => {
      q = norm(q);
      const l = tous.filter(o => !q || norm(o.n).includes(q)).sort((a, b) => b.l - a.l).slice(0, 120);
      $('#tListe', m).innerHTML = l.map(o => `<div class="choix" data-id="${o.id}">${imgObjet(o)}<div class="txt"><b>${esc(o.n)}</b> <span class="mini">niv. ${o.l} · ${esc(nomType(o))}</span><div class="effs" style="margin-top:4px">${pastillesObjet(o, 6)}</div></div></div>`).join('') || '<div class="vide">Aucun objet.</div>';
    };
    dessiner('');
    $('#tQ', m).oninput = e => dessiner(e.target.value);
    $('#tQ', m).focus();
    $('#tFermer', m).onclick = fermerTiroir;
    $('#tListe', m).onclick = e => { const c = e.target.closest('.choix'); if (c) { choisi(D.OBJ.get(+c.dataset.id)); toast('Ajouté'); } };
  });
}

// ============================================================ RECHERCHE
const EMPL_RECH = [['Coiffe', [10]], ['Cape', [11]], ['Amulette', [1]], ['Anneau', [3]], ['Ceinture', [4]], ['Bottes', [5]], ['Arme', [2]], ['Bouclier', [7]], ['Familier', [12]], ['Dofus / Trophée', [13]]];
const COLS = ['vita', 'for', 'int', 'cha', 'agi', 'sag', 'pa', 'pm', 'po', 'invoc', 'crit', 'puis', 'do', 'ini', 'pp'];
const ETAT_RECH = { q: '', empl: new Set(), min: 1, max: 200, criteres: [], pano: false, craft: false, tri: 'niveau', sens: -1, voir: 100 };
let CACHE_STATS = null;
function statsMax(o) {
  CACHE_STATS ||= new Map();
  if (!CACHE_STATS.has(o.id)) CACHE_STATS.set(o.id, effetsObjet(o, null, 'max'));
  return CACHE_STATS.get(o.id);
}
function vueRecherche() {
  const equipables = new Set([1, 2, 3, 4, 5, 7, 10, 11, 12, 13]);
  const tous = D.objets.filter(o => equipables.has(superType(o)) && !TYPES_EXCLUS.has(o.t));
  const R = ETAT_RECH;
  VUE.innerHTML = `<div class="col2" style="grid-template-columns:270px minmax(0,1fr)">
    <aside class="carte collant"><div class="filtres" style="overflow-y:auto">
      <div class="recherche"><input class="champ" id="rQ" placeholder="Objet ou panoplie…" value="${esc(R.q)}"></div>
      <div class="titre-sec" style="margin:4px 0 0">Emplacements</div>
      <div class="puces">${EMPL_RECH.map(([n], i) => `<span class="puce ${R.empl.has(i) ? 'on' : ''}" data-e="${i}">${n}</span>`).join('')}</div>
      <div class="titre-sec" style="margin:4px 0 0">Niveau</div>
      <div class="rangee"><input class="champ num" id="rMin" type="number" value="${R.min}"><span class="mini">à</span><input class="champ num" id="rMax" type="number" value="${R.max}"></div>
      <div class="titre-sec" style="margin:4px 0 0">Caractéristiques minimales</div>
      <div id="rCrit">${R.criteres.map(([k, v], i) => `<div class="rangee" style="margin-bottom:6px"><select class="champ" data-ck="${i}" style="flex:1">${COLS.concat(ORDRE_PASTILLES.filter(x => !COLS.includes(x) && x !== 'pv')).map(x => `<option value="${x}" ${x === k ? 'selected' : ''}>${esc(LIB[x][1])}</option>`).join('')}</select>
        <input class="champ num" data-cv="${i}" type="number" value="${v}" style="width:60px"><button class="btn fantome petit" data-cx="${i}">✕</button></div>`).join('')}</div>
      <button class="btn petit" id="rAjCrit">+ Ajouter un critère</button>
      <span class="inter ${R.pano ? 'on' : ''}" id="rPano"><i></i>Objets de panoplie</span>
      <span class="inter ${R.craft ? 'on' : ''}" id="rCraft"><i></i>Craftables uniquement</span>
      <button class="btn fantome" id="rRaz">↺ Réinitialiser les filtres</button>
    </div></aside>
    <section><div class="rangee" style="justify-content:space-between;margin-bottom:8px"><b id="rCompte"></b><span class="mini">Valeurs au jet maximum · clic sur une colonne pour trier</span></div>
      <div class="tableau-env"><table class="tab"><thead><tr><th data-t="nom">Objet</th><th data-t="niveau">Niv.</th>${COLS.map(k => `<th data-t="${k}" title="${esc(LIB[k][1])}"><img class="ic" src="${ICO(k)}" alt="${esc(LIB[k][0])}"></th>`).join('')}<th></th></tr></thead><tbody id="rCorps"></tbody></table></div>
      <div style="text-align:center;margin-top:10px"><button class="btn" id="rPlus" hidden>Afficher plus</button></div></section></div>`;
  const dessiner = () => {
    const q = norm(R.q);
    const types = new Set([...R.empl].flatMap(i => EMPL_RECH[i][1]));
    let l = tous.filter(o => o.l >= R.min && o.l <= R.max && (!types.size || types.has(superType(o))) && (!q || norm(o.n).includes(q) || (o.s && norm(D.PANO.get(o.s)?.n).includes(q)))
      && (!R.pano || (o.s && D.PANO.has(o.s))) && (!R.craft || D.REC.has(o.id)) && R.criteres.every(([k, v]) => (statsMax(o)[k] || 0) >= v));
    const cle = R.tri;
    l.sort((a, b) => cle === 'nom' ? R.sens * -a.n.localeCompare(b.n) : cle === 'niveau' ? R.sens * (a.l - b.l) || a.n.localeCompare(b.n) : R.sens * ((statsMax(a)[cle] || 0) - (statsMax(b)[cle] || 0)));
    $('#rCompte').textContent = fmt(l.length) + ' équipement(s)';
    $$('th[data-t]').forEach(t => t.classList.toggle('on', t.dataset.t === cle));
    $('#rCorps').innerHTML = l.slice(0, R.voir).map(o => {
      const st = statsMax(o);
      return `<tr><td><div class="obj" data-objet="${o.id}">${imgObjet(o, 'p')}<div><b style="font-weight:500">${esc(o.n)}</b><small>${esc(nomType(o))}${o.s && D.PANO.has(o.s) ? ' · ' + esc(D.PANO.get(o.s).n) : ''}</small></div></div></td><td>${o.l}</td>
        ${COLS.map(k => `<td class="num" style="color:${st[k] ? (st[k] > 0 ? LIB[k][3] : 'var(--rouge)') : ''}">${st[k] ? fmt(st[k]) : '<span class="z">·</span>'}</td>`).join('')}
        <td><button class="btn fantome petit" data-eq="${o.id}" title="Équiper dans le stuff actif">Équiper</button>${D.REC.has(o.id) ? `<button class="btn fantome petit" data-cr="${o.id}" title="Ajouter au craft">Craft</button>` : ''}</td></tr>`;
    }).join('') || `<tr><td colspan="${COLS.length + 3}" class="vide" style="text-align:left">Aucun objet ne correspond.</td></tr>`;
    $('#rPlus').hidden = l.length <= R.voir;
  };
  dessiner();
  const refaire = () => { R.voir = 100; dessiner(); };
  $('#rQ').oninput = e => { R.q = e.target.value; refaire(); };
  $$('[data-e]').forEach(p => p.onclick = () => { const i = +p.dataset.e; R.empl.has(i) ? R.empl.delete(i) : R.empl.add(i); p.classList.toggle('on'); refaire(); });
  $('#rMin').onchange = e => { R.min = +e.target.value || 1; refaire(); };
  $('#rMax').onchange = e => { R.max = +e.target.value || 200; refaire(); };
  $('#rAjCrit').onclick = () => { R.criteres.push(['vita', 100]); vueRecherche(); };
  $$('[data-ck]').forEach(s => s.onchange = () => { R.criteres[+s.dataset.ck][0] = s.value; refaire(); });
  $$('[data-cv]').forEach(s => s.onchange = () => { R.criteres[+s.dataset.cv][1] = +s.value || 0; refaire(); });
  $$('[data-cx]').forEach(b => b.onclick = () => { R.criteres.splice(+b.dataset.cx, 1); vueRecherche(); });
  $('#rPano').onclick = e => { R.pano = !R.pano; e.currentTarget.classList.toggle('on'); refaire(); };
  $('#rCraft').onclick = e => { R.craft = !R.craft; e.currentTarget.classList.toggle('on'); refaire(); };
  $('#rRaz').onclick = () => { Object.assign(R, { q: '', empl: new Set(), min: 1, max: 200, criteres: [], pano: false, craft: false, tri: 'niveau', sens: -1 }); vueRecherche(); };
  $$('th[data-t]').forEach(t => t.onclick = () => { if (R.tri === t.dataset.t) R.sens *= -1; else { R.tri = t.dataset.t; R.sens = -1; } dessiner(); });
  $('#rPlus').onclick = () => { R.voir += 100; dessiner(); };
  $('#rCorps').onclick = e => {
    const eq = e.target.closest('[data-eq]'), cr = e.target.closest('[data-cr]'), ob = e.target.closest('[data-objet]');
    if (eq) {
      const o = D.OBJ.get(+eq.dataset.eq), s = stuffActif();
      const libres = EMPL.filter(x => x.st.includes(superType(o)));
      const cible = libres.find(x => !s.slots[x.k]) || libres[0];
      if (cible && equiper(s, cible.k, o.id, true)) toast(o.n + ' équipé dans « ' + s.nom + ' »');
    } else if (cr) { ajouterListe(+cr.dataset.cr, 1); ETAT.craftMode = 'perso'; sauver(); toast('Ajouté à la liste de craft'); }
    else if (ob) allerA('encyclopedie', ob.dataset.objet);
  };
}

// ============================================================ ENCYCLOPÉDIE
const CATS = [
  ['Équipements', [1, 3, 4, 5, 7, 10, 11, 12, 13]], ['Armes', [2]], ['Consommables', [6, 16, 17, 18, 19]], ['Ressources', [9]],
  ['Objets de quête', [14]], ['Apparat', [22, 25, 26]],
];
const catDe = o => { const s = superType(o); const i = CATS.findIndex(c => c[1].includes(s)); return i < 0 ? CATS.length : i; };
const ETAT_ENC = { q: '', cat: -1, type: 0, min: 0, max: 0, drop: false, craft: false, tri: 'niv' };
async function vueEncyclopedie(arg) {
  const E = ETAT_ENC;
  const sources = await chargerSources().catch(() => null);
  const butins = sources?.butins || {};
  const nb = CATS.map(c => D.objets.filter(o => c[1].includes(superType(o))).length);
  const types = [...D.TYPE.values()].filter(t => E.cat < 0 || (E.cat < CATS.length ? CATS[E.cat][1].includes(t.s) : !CATS.some(c => c[1].includes(t.s)))).filter(t => t.n);
  VUE.innerHTML = `<div class="col2" style="grid-template-columns:330px minmax(0,1fr)">
    <aside class="carte collant"><div class="filtres">
      <div class="recherche"><input class="champ" id="eQ" placeholder="Nom de l'objet ou #id" value="${esc(E.q)}"></div>
      <div class="puces"><span class="puce ${E.cat < 0 ? 'on' : ''}" data-c="-1">Tout <small>${fmt(D.objets.length)}</small></span>${CATS.map((c, i) => `<span class="puce ${E.cat === i ? 'on' : ''}" data-c="${i}">${c[0]} <small>${fmt(nb[i])}</small></span>`).join('')}<span class="puce ${E.cat === CATS.length ? 'on' : ''}" data-c="${CATS.length}">Autres</span></div>
      <div class="rangee"><select class="champ" id="eType" style="flex:1"><option value="0">Tous les types</option>${types.sort((a, b) => a.n.localeCompare(b.n)).map(t => `<option value="${t.id}" ${E.type === t.id ? 'selected' : ''}>${esc(t.n)}</option>`).join('')}</select>
        <select class="champ" id="eTri"><option value="niv">Niveau croissant</option><option value="nivd" ${E.tri === 'nivd' ? 'selected' : ''}>Niveau décroissant</option><option value="nom" ${E.tri === 'nom' ? 'selected' : ''}>Nom</option></select></div>
      <div class="rangee"><span class="mini">Niveau</span><input class="champ num" id="eMin" type="number" value="${E.min || ''}" placeholder="0"><span class="mini">à</span><input class="champ num" id="eMax" type="number" value="${E.max || ''}" placeholder="200">
        <span class="puce ${E.drop ? 'on' : ''}" id="eDrop">Se drop</span><span class="puce ${E.craft ? 'on' : ''}" id="eCraft">Craftable</span></div>
    </div><div class="liste" id="eListe"></div></aside>
    <section id="eFiche"></section></div>`;
  const id = arg ? +arg : null;
  const dessiner = () => {
    const q = norm(E.q.replace(/^#/, ''));
    const nid = /^\d+$/.test(q) ? +q : null;
    let l = D.objets.filter(o => (E.cat < 0 || catDe(o) === E.cat) && (!E.type || o.t === E.type) && (!E.min || o.l >= E.min) && (!E.max || o.l <= E.max)
      && (!E.drop || butins[o.id]) && (!E.craft || D.REC.has(o.id)) && (!q || o.id === nid || norm(o.n).includes(q)));
    l.sort((a, b) => E.tri === 'nom' ? a.n.localeCompare(b.n) : E.tri === 'nivd' ? b.l - a.l || a.n.localeCompare(b.n) : a.l - b.l || a.n.localeCompare(b.n));
    $('#eListe').innerHTML = l.slice(0, 250).map(o => `<div class="ligne ${o.id === id ? 'on' : ''}" data-id="${o.id}">${imgObjet(o, 'p')}<div class="txt"><b>${esc(o.n)}</b><small>${esc(nomType(o))}</small></div><span class="niv">Niv. ${o.l}</span></div>`).join('')
      + (l.length > 250 ? `<div class="vide">${fmt(l.length - 250)} autres résultats : affine la recherche.</div>` : '') + (l.length ? '' : '<div class="vide">Aucun objet.</div>');
  };
  dessiner();
  $('#eQ').oninput = e => { E.q = e.target.value; dessiner(); };
  $$('[data-c]').forEach(p => p.onclick = () => { E.cat = +p.dataset.c; E.type = 0; vueEncyclopedie(arg); });
  $('#eType').onchange = e => { E.type = +e.target.value; dessiner(); };
  $('#eTri').onchange = e => { E.tri = e.target.value; dessiner(); };
  $('#eMin').onchange = e => { E.min = +e.target.value || 0; dessiner(); };
  $('#eMax').onchange = e => { E.max = +e.target.value || 0; dessiner(); };
  $('#eDrop').onclick = e => { E.drop = !E.drop; e.currentTarget.classList.toggle('on'); dessiner(); };
  $('#eCraft').onclick = e => { E.craft = !E.craft; e.currentTarget.classList.toggle('on'); dessiner(); };
  $('#eListe').onclick = e => { const l = e.target.closest('.ligne'); if (l) allerA('encyclopedie', l.dataset.id); };

  const o = id && D.OBJ.get(id);
  if (!o) {
    const dofus = D.objets.filter(x => x.t === 23).sort((a, b) => a.l - b.l);
    $('#eFiche').innerHTML = `<div class="carte"><h3 style="font-size:20px">Encyclopédie de Mystic</h3><p class="mini">${fmt(D.objets.length)} objets du serveur, classés par type : clique sur un objet pour voir ses effets, ses conditions, sa recette et les monstres qui le lâchent, avec leurs taux.</p>
      <div class="cat-grille" style="margin:14px 0">${CATS.map((c, i) => `<div class="cat" data-c="${i}"><b>${c[0]}</b><small>${fmt(nb[i])} objets</small></div>`).join('')}</div>
      <div class="titre-sec">Dofus</div><div class="grille-ico">${dofus.map(x => `<div class="cat" data-objet="${x.id}"><img src="${icoObjet(x)}" alt="" loading="lazy"><b style="font-size:12.5px">${esc(x.n)}</b></div>`).join('')}</div></div>`;
    $$('#eFiche [data-c]').forEach(p => p.onclick = () => { E.cat = +p.dataset.c; vueEncyclopedie(); });
    $$('#eFiche [data-objet]').forEach(p => p.onclick = () => allerA('encyclopedie', p.dataset.objet));
    return;
  }
  const r = D.REC.get(o.id);
  const util = (D.UTIL.get(o.id) || []).map(i => D.OBJ.get(i)).filter(Boolean);
  const lache = (butins[o.id] || []).slice().sort((a, b) => b[2] - a[2]);
  const nomsMonstres = new Map((sources?.monstres || []).map(m => [m[0], m]));
  const p = o.s && D.PANO.get(o.s);
  const w = o.w;
  $('#eFiche').innerHTML = `<div class="carte">
    <div class="fiche-tete">${imgObjet(o)}<div><h2>${esc(o.n)}</h2><div class="sous">Niveau ${o.l} · ${esc(nomType(o))} · #${o.id}${o.p ? ' · ' + o.p + ' pods' : ''}</div>
      ${p ? `<a class="lien" href="#panoplies/${p.id}" style="font-size:13px">${esc(p.n)}</a>` : ''}</div>
      <div class="act"><button class="btn" id="fLien">Lien</button>${[1, 2, 3, 4, 5, 7, 10, 11, 12, 13].includes(superType(o)) && !TYPES_EXCLUS.has(o.t) ? '<button class="btn violet" id="fEquiper">Équiper</button>' : ''}${r ? '<button class="btn" id="fCraft">Ajouter au craft</button>' : ''}</div></div>
    ${o.d ? `<p class="mini" style="font-size:13.5px;color:var(--argent2)">${esc(o.d)}</p>` : ''}
    ${w ? `<div class="titre-sec">Arme</div><div class="effs"><span class="eff">${w[0]} PA</span><span class="eff">Portée ${w[1]}${w[2] > w[1] ? ' à ' + w[2] : ''}</span>${w[3] ? `<span class="eff">Critique 1/${w[3]} (+${w[4]})</span>` : ''}${w[5] ? `<span class="eff">${w[5]} par tour</span>` : ''}</div>` : ''}
    ${o.e?.length && !o.h ? `<div class="titre-sec">Effets</div><div class="effets-liste">${o.e.map(([e, a, b]) => { const st = statEffet(e); return `<div style="color:${st ? (st.s > 0 ? LIB[st.k][3] : 'var(--rouge)') : 'var(--argent2)'}">${st ? `<img class="ic" src="${ICO(st.k)}" alt="">` : '<span style="width:20px"></span>'}${esc(texteEffet(e, a, b))}</div>`; }).join('')}</div>` : ''}
    ${o.c ? `<div class="titre-sec">Conditions</div><div class="mini">${esc(conditions(o.c))}</div>` : ''}
    ${r ? `<div class="sep-h"></div><div class="titre-sec" style="margin-top:0">Recette · ${esc(D.METIER.get(r.metier) || '')}</div><div>${r.ing.map(([i, q]) => { const x = D.OBJ.get(i); return x ? `<span class="ingr" data-objet="${i}"><img src="${icoObjet(x)}" alt="" loading="lazy">${q}× ${esc(x.n)}</span>` : ''; }).join('')}</div>` : ''}
    ${util.length ? `<div class="titre-sec">Utilisé dans ${util.length} recette(s)</div><div>${util.slice(0, 40).map(x => `<span class="ingr" data-objet="${x.id}"><img src="${icoObjet(x)}" alt="" loading="lazy">${esc(x.n)}</span>`).join('')}${util.length > 40 ? `<span class="mini">+${util.length - 40}</span>` : ''}</div>` : ''}
    <div class="sep-h"></div><div class="titre-sec" style="margin-top:0">Où l'obtenir</div>
    ${lache.length ? lache.map(([m, a, b]) => { const x = nomsMonstres.get(m); return `<div class="butin" data-monstre="${m}"><span class="ico p"><img src="${icoMonstre(m)}" alt="" loading="lazy" onerror="this.style.visibility='hidden'"></span><div class="txt"><b style="font-weight:500">${esc(x?.[1] || 'Monstre #' + m)}</b><div class="mini">${x ? 'Niv. ' + x[3] + (x[4] > x[3] ? ' à ' + x[4] : '') : ''}</div></div><span class="taux">${fmt(a)}${b > a ? ' à ' + fmt(b) : ''} %</span></div>`; }).join('')
      : `<p class="mini">${r ? 'Ne se drop pas : il se fabrique (recette ci-dessus).' : 'Ni drop ni recette connus : quête, échange, boutique ou événement.'}</p>`}
  </div>`;
  $('#fLien').onclick = () => copier(location.origin + location.pathname + '#encyclopedie/' + o.id, 'Lien copié');
  $('#fEquiper')?.addEventListener('click', () => { const s = stuffActif(); const libres = EMPL.filter(x => x.st.includes(superType(o))); const c = libres.find(x => !s.slots[x.k]) || libres[0]; if (c && equiper(s, c.k, o.id, true)) toast(o.n + ' équipé dans « ' + s.nom + ' »'); });
  $('#fCraft')?.addEventListener('click', () => { ajouterListe(o.id, 1); ETAT.craftMode = 'perso'; sauver(); toast('Ajouté à la liste de craft'); });
  $$('#eFiche [data-objet]').forEach(x => x.onclick = () => allerA('encyclopedie', x.dataset.objet));
  $$('#eFiche [data-monstre]').forEach(x => x.onclick = () => allerA('bestiaire', x.dataset.monstre));
  $('.ligne.on')?.scrollIntoView({ block: 'nearest' });
}
/** Conditions d'objet lisibles (« PL>50&CS>20 » -> « Niveau > 50 et Force > 20 »). */
function conditions(c) {
  const n = { PL: 'Niveau', CS: 'Force', CI: 'Intelligence', CC: 'Chance', CA: 'Agilité', CW: 'Sagesse', CV: 'Vitalité', Ps: 'Alignement', PS: 'Sexe', PB: 'Sous-zone', PO: 'Objet possédé', Pa: "Grade d'alignement", PP: 'Points PvP', Qa: 'Quête', PR: 'Marié', PX: 'Abonné' };
  return c.replace(/([A-Za-z]{2})([<>=!~]+)([^&|]+)/g, (m, k, op, v) => (n[k] || k) + ' ' + op.replace('!', '≠') + ' ' + v).replace(/&/g, ' et ').replace(/\|/g, ' ou ');
}

// ============================================================ BESTIAIRE
const ETAT_BEST = { q: '', race: 0, min: 0, max: 0, boss: false, archi: false };
async function vueBestiaire(arg) {
  VUE.innerHTML = '<div class="at-charge">Chargement du bestiaire…</div>';
  const B = await chargerBestiaire();
  const S = await chargerSources().catch(() => null);
  const zones = new Map((S?.sousZones || []).map(z => [z[0], z]));
  const E = ETAT_BEST;
  const id = arg ? +arg : null;
  if (id && B.MON.has(id)) { ETAT.recents = [id, ...ETAT.recents.filter(x => x !== id)].slice(0, 8); sauver(); }
  const nomRace = m => B.RACE.get(m[2]) || '';
  const niveaux = m => { const n = m[10].map(g => g[0][1]); return n.length ? [Math.min(...n), Math.max(...n)] : [0, 0]; };
  const archi = m => /archi/i.test(nomRace(m));
  const lig = m => { const [a, b] = niveaux(m); return `<div class="ligne ${m[0] === id ? 'on' : ''}" data-id="${m[0]}"><span class="ico p"><img src="${icoMonstre(m[0])}" alt="" loading="lazy" onerror="this.style.visibility='hidden'"></span><div class="txt"><b>${esc(m[1])}${m[4] ? ' 👑' : ''}</b><small>${esc(nomRace(m))}</small></div><span class="niv">Niv. ${a}${b > a ? ' à ' + b : ''}</span></div>`; };
  const races = [...new Set(B.monstres.map(m => m[2]))].map(r => [r, B.RACE.get(r) || 'Race ' + r]).sort((a, b) => a[1].localeCompare(b[1]));
  VUE.innerHTML = `<div class="col2" style="grid-template-columns:330px minmax(0,1fr)">
    <aside class="carte collant"><div class="filtres">
      <div class="recherche"><input class="champ" id="mQ" placeholder="Nom du monstre ou #id" value="${esc(E.q)}"></div>
      <select class="champ" id="mRace"><option value="0">Toutes les races</option>${races.map(([r, n]) => `<option value="${r}" ${E.race && E.race === r ? 'selected' : ''}>${esc(n)}</option>`).join('')}</select>
      <div class="rangee"><span class="mini">Niveau</span><input class="champ num" id="mMin" type="number" value="${E.min || ''}" placeholder="0"><span class="mini">à</span><input class="champ num" id="mMax" type="number" value="${E.max || ''}" placeholder="—"></div>
      <div class="puces"><span class="puce ${E.boss ? 'on' : ''}" id="mBoss">Boss</span><span class="puce ${E.archi ? 'on' : ''}" id="mArchi">Archimonstres</span></div>
    </div>
    ${ETAT.recents.length ? `<div class="rangee" style="justify-content:space-between"><span class="titre-sec" style="margin:4px 0">Récemment consultés</span><button class="btn fantome petit" id="mEff">Effacer</button></div><div class="liste" style="flex:none;max-height:160px">${ETAT.recents.map(x => B.MON.get(x)).filter(Boolean).map(lig).join('')}</div>` : ''}
    <span class="titre-sec" style="margin:8px 0 4px">Tous les monstres</span><div class="liste" id="mListe"></div></aside>
    <section id="mFiche"></section></div>`;
  const dessiner = () => {
    const q = norm(E.q.replace(/^#/, ''));
    const nid = /^\d+$/.test(q) ? +q : null;
    const l = B.monstres.filter(m => (!q || m[0] === nid || norm(m[1]).includes(q)) && (!E.race || m[2] === E.race) && (!E.boss || m[4]) && (!E.archi || archi(m))
      && (!E.min || niveaux(m)[1] >= E.min) && (!E.max || niveaux(m)[0] <= E.max)).sort((a, b) => a[1].localeCompare(b[1]));
    $('#mListe').innerHTML = l.slice(0, 200).map(lig).join('') + (l.length > 200 ? `<div class="vide">${l.length - 200} autres résultats : affine la recherche.</div>` : '') + (l.length ? '' : '<div class="vide">Aucun monstre.</div>');
  };
  dessiner();
  $('#mQ').oninput = e => { E.q = e.target.value; dessiner(); };
  $('#mRace').onchange = e => { E.race = +e.target.value; dessiner(); };
  $('#mMin').onchange = e => { E.min = +e.target.value || 0; dessiner(); };
  $('#mMax').onchange = e => { E.max = +e.target.value || 0; dessiner(); };
  $('#mBoss').onclick = e => { E.boss = !E.boss; e.currentTarget.classList.toggle('on'); dessiner(); };
  $('#mArchi').onclick = e => { E.archi = !E.archi; e.currentTarget.classList.toggle('on'); dessiner(); };
  $('#mEff')?.addEventListener('click', () => { ETAT.recents = []; sauver(); vueBestiaire(arg); });
  VUE.querySelector('aside').addEventListener('click', e => { const l = e.target.closest('.ligne'); if (l) allerA('bestiaire', l.dataset.id); });

  const m = id && B.MON.get(id);
  if (!m) {
    const boss = B.monstres.filter(x => x[4]).map(x => ({ x, n: niveaux(x)[0] })).sort((a, b) => a.n - b.n);
    $('#mFiche').innerHTML = `<div class="carte"><h3 style="font-size:20px">Bestiaire de Mystic</h3><p class="mini">${fmt(B.monstres.length)} monstres avec leurs grades, caractéristiques, butins et sorts, tels que le serveur les applique.</p>
      <div class="titre-sec">Boss de donjon</div><div class="grille-ico">${boss.map(({ x, n }) => `<div class="cat" data-id="${x[0]}"><img src="${icoMonstre(x[0])}" alt="" loading="lazy" onerror="this.style.visibility='hidden'"><b style="font-size:12.5px">${esc(x[1])}</b><small>Niv. ${n}</small></div>`).join('')}</div></div>`;
    $$('#mFiche [data-id]').forEach(c => c.onclick = () => allerA('bestiaire', c.dataset.id));
    return;
  }
  let g = Math.min(+(sessionStorage.getItem('grade') || 0), m[10].length - 1);
  const fiche = () => {
    const [v, st, sorts] = m[10][g] || [[], null, []];
    const c = Object.fromEntries(B.champs.map((k, i) => [k, v[i] || 0]));
    const extra = st || {};
    const X = k => extra[k] ?? 0;
    const [nmin, nmax] = niveaux(m);
    const sz = m[9].map(z => zones.get(z)).filter(Boolean);
    $('#mFiche').innerHTML = `<div class="carte">
      <div class="fiche-tete"><div class="monstre-img"><img src="${icoMonstre(m[0])}" alt="" onerror="this.style.visibility='hidden'"></div>
        <div><h2>${esc(m[1])}</h2><div class="sous">${esc(nomRace(m))} · #${m[0]} · niveau ${nmin}${nmax > nmin ? ' à ' + nmax : ''}</div>
        <div style="margin-top:8px">${m[4] ? '<span class="etiq boss">Boss de donjon</span>' : ''}${m[6] ? '<span class="etiq">Compte comme invocation</span>' : ''}${m[7] ? '<span class="etiq">Idoles désactivées</span>' : ''}${archi(m) ? '<span class="etiq">Archimonstre</span>' : ''}</div></div>
        <div class="act"><button class="btn" id="mPart">Partager</button></div></div>
      ${sz.length ? `<div class="mini" style="margin-bottom:10px">📍 ${sz.slice(0, 8).map(z => esc(z[1] + (z[2] ? ' (' + z[2] + ')' : ''))).join(' · ')}${sz.length > 8 ? ' · +' + (sz.length - 8) : ''}</div>` : '<div class="mini" style="margin-bottom:10px">Aucune zone d\'apparition connue (donjon scripté, invocation ou événement).</div>'}
      <div class="onglets-grades">${m[10].map((x, i) => `<button class="btn ${i === g ? 'violet' : ''} petit" data-g="${i}">Grade ${x[0][0]} · niv. ${x[0][1]}</button>`).join('')}</div>
      <div class="tuiles6">${[['pv', c.LifePoints, 'PV'], ['pa', c.ActionPoints, 'PA'], ['pm', c.MovementPoints, 'PM'], ['ini', X('Initiative'), 'Initiative'], ['pp', c.GradeXp, 'Expérience'], ['pods', `${m[5][0]}${m[5][1] > m[5][0] ? '-' + m[5][1] : ''}`, 'Kamas']]
        .map(([k, val, n]) => `<div class="tuile"><img src="${ICO(k)}" alt=""><b class="num">${typeof val === 'number' ? fmt(val) : esc(val)}</b><small>${n}</small></div>`).join('')}</div>
      <div class="titre-sec">Caractéristiques</div>
      <div class="stats">${[['for', c.Strength], ['int', c.Intelligence], ['cha', c.Chance], ['agi', c.Agility], ['sag', c.Wisdom], ['vita', c.Vitality]].map(([k, val]) => `<div><img class="ic" src="${ICO(k)}" alt=""><span>${LIB[k][1]}</span><b class="${val ? '' : 'z'}">${fmt(val)}</b></div>`).join('')}</div>
      <div class="titre-sec">Dommages</div>
      <div class="stats">${[['puis', 'DamagesPercent'], ['do', 'DamageBonus'], ['doTerre', 'EarthDamageBonus'], ['doFeu', 'FireDamageBonus'], ['doEau', 'WaterDamageBonus'], ['doAir', 'AirDamageBonus'], ['doNeutre', 'NeutralDamageBonus'], ['doCrit', 'CriticalDamageBonus'], ['soins', 'HealBonus'], ['crit', 'CriticalHit']]
        .map(([k, f]) => `<div><img class="ic" src="${ICO(k)}" alt=""><span>${LIB[k][1]}</span><b class="${X(f) ? '' : 'z'}">${fmt(X(f))}</b></div>`).join('')}</div>
      <div class="titre-sec">Tacle, fuite, retraits</div>
      <div class="stats">${[['tacle', c.TackleBlock], ['fuite', c.TackleEvade], ['esqPA', c.PaDodge], ['esqPM', c.PmDodge], ['retPA', X('ApAttack')], ['retPM', X('MpAttack')]].map(([k, val]) => `<div><img class="ic" src="${ICO(k)}" alt=""><span>${LIB[k][1]}</span><b class="${val ? '' : 'z'}">${fmt(val)}</b></div>`).join('')}</div>
      <div class="titre-sec">Résistances</div>
      <table class="res"><tr><th></th><th style="color:var(--terre)">Terre</th><th style="color:var(--feu)">Feu</th><th style="color:var(--eau)">Eau</th><th style="color:var(--air)">Air</th><th style="color:var(--neutre)">Neutre</th></tr>
        <tr><td>%</td>${['EarthResistance', 'FireResistance', 'WaterResistance', 'AirResistance', 'NeutralResistance'].map(f => `<td>${c[f]}%</td>`).join('')}</tr>
        <tr><td>Fixe</td>${['EarthElementReduction', 'FireElementReduction', 'WaterElementReduction', 'AirElementReduction', 'NeutralElementReduction'].map(f => `<td>${fmt(X(f))}</td>`).join('')}</tr></table>
    </div>
    <div class="carte" style="margin-top:14px"><div class="titre-sec" style="margin-top:0">Sorts</div>
      ${sorts.length ? `<div class="effs">${sorts.map(([sp, lv]) => `<span class="eff" style="font-size:12.5px;padding:4px 9px">${esc(B.sorts[sp] || 'Sort #' + sp)} <small class="mini">rang ${lv}</small></span>`).join('')}</div>` : '<p class="mini">Aucun sort.</p>'}</div>
    <div class="carte" style="margin-top:14px"><div class="titre-sec" style="margin-top:0">Butins</div><p class="mini" style="margin:0 0 6px">Taux du grade ${g + 1}, hors bonus de prospection.</p>
      ${m[11].length ? m[11].slice().sort((a, b) => b[1][g] - a[1][g]).map(([oid, t, lim, pros, cond]) => { const o = D.OBJ.get(oid); return o ? `<div class="butin" data-objet="${oid}">${imgObjet(o, 'p')}<div class="txt"><b style="font-weight:500">${esc(o.n)}</b><div class="mini">Niv. ${o.l}${lim > 0 && lim < 50 ? ' · au plus ' + lim + ' par combat' : ''}${pros ? ' · PP ≥ ' + pros : ''}${cond ? ' · ' + esc(conditions(cond)) : ''}</div></div><span class="taux">${fmt(t[g] ?? t[0])} %</span></div>` : ''; }).join('') : '<p class="mini">Aucun butin.</p>'}
    </div>`;
    $$('[data-g]').forEach(b => b.onclick = () => { g = +b.dataset.g; sessionStorage.setItem('grade', g); fiche(); });
    $$('#mFiche [data-objet]').forEach(x => x.onclick = () => allerA('encyclopedie', x.dataset.objet));
    $('#mPart').onclick = () => copier(location.origin + location.pathname + '#bestiaire/' + m[0], 'Lien du monstre copié');
  };
  fiche();
  $('.ligne.on')?.scrollIntoView({ block: 'nearest' });
}

// ============================================================ ARMOIRE
function vueArmoire() {
  VUE.innerHTML = `<div class="rangee" style="justify-content:space-between;margin-bottom:16px">
      <div><h2 style="margin:0;font:600 24px var(--f-titre)">Armoire</h2><p class="mini" style="margin:2px 0 0">Tes personnages et leurs stuffs, sauvegardés dans ce navigateur. Un stuff se partage par lien ou par code.</p></div>
      <div class="rangee"><button class="btn" id="aImporter">Importer un build</button><button class="btn" id="aSauve">Sauvegarde</button><button class="btn violet" id="aNouveau">+ Nouveau personnage</button></div></div>
    <div class="armoire">${ETAT.persos.map(p => {
      const s0 = p.stuffs.find(s => s.id === p.actif) || p.stuffs[0];
      return `<section class="carte perso-carte" data-p="${p.id}"><div class="tete"><img src="atelier/icones/classes/tete_${(s0?.classe || 8) * 10}.png" alt="" onerror="this.style.visibility='hidden'">
        <b>${esc(p.nom)}</b>${p.id === ETAT.perso ? '<span class="actif">actif</span>' : ''}<button class="btn fantome petit" data-pm="${p.id}" aria-label="Actions">•••</button></div>
        <div class="mini" style="margin:-6px 0 8px 48px">${p.stuffs.length} stuff(s)</div>
        ${p.stuffs.map(s => `<div class="stuff-l ${p.id === ETAT.perso && s.id === p.actif ? 'on' : ''}" data-s="${s.id}"><div class="txt"><b>${esc(s.nom)}</b><small>${esc(D.CLASSE.get(s.classe)?.n || '')} · niv. ${s.niveau} · ${Object.keys(s.slots).length}/16 objets · modifié le ${new Date(s.maj || Date.now()).toLocaleDateString('fr-FR')}</small></div>
          <button class="btn fantome petit" data-sm="${s.id}" aria-label="Actions">•••</button></div>`).join('')}
        <button class="btn fantome petit" data-aj="${p.id}" style="margin-top:6px">+ Ajouter un stuff</button></section>`;
    }).join('')}</div>`;
  const perso = id => ETAT.persos.find(p => p.id === id);
  $('#aNouveau').onclick = nouveauPerso;
  $('#aImporter').onclick = importer;
  $('#aSauve').onclick = e => menu(e.currentTarget, [
    { t: 'Télécharger une sauvegarde (.json)', f: () => { const b = new Blob([JSON.stringify(ETAT)], { type: 'application/json' }); const a = document.createElement('a'); a.href = URL.createObjectURL(b); a.download = 'atelier-mystic.json'; a.click(); setTimeout(() => URL.revokeObjectURL(a.href), 2000); } },
    { t: 'Restaurer une sauvegarde…', f: () => { const i = document.createElement('input'); i.type = 'file'; i.accept = '.json,application/json'; i.onchange = async () => { try { const e2 = JSON.parse(await i.files[0].text()); if (!Array.isArray(e2.persos)) throw 0; ETAT = Object.assign(etatParDefaut(), e2); sauver(); majEntete(); vueArmoire(); toast('Sauvegarde restaurée'); } catch { toast('Fichier de sauvegarde invalide'); } }; i.click(); } },
  ]);
  $$('[data-aj]').forEach(b => b.onclick = () => { ETAT.perso = b.dataset.aj; ajouterStuff(perso(b.dataset.aj)); });
  $$('.stuff-l').forEach(l => l.onclick = e => { if (e.target.closest('[data-sm]')) return; const p = perso(l.closest('[data-p]').dataset.p); ETAT.perso = p.id; p.actif = l.dataset.s; sauver(); majEntete(); allerA('builder'); });
  $$('[data-pm]').forEach(b => b.onclick = () => {
    const p = perso(b.dataset.pm);
    menu(b, [
      { t: 'Définir comme actif', f: () => { ETAT.perso = p.id; sauver(); majEntete(); vueArmoire(); } },
      { t: 'Renommer…', f: () => saisir('Renommer le personnage', p.nom, n => { p.nom = n; sauver(); majEntete(); vueArmoire(); }) },
      { sep: true },
      { t: 'Supprimer le personnage', danger: true, f: () => confirmer('Supprimer « ' + p.nom + ' » ?', 'Ses ' + p.stuffs.length + ' stuff(s) seront supprimés de ce navigateur.', () => {
        ETAT.persos = ETAT.persos.filter(x => x !== p);
        if (!ETAT.persos.length) ETAT.persos = etatParDefaut().persos;
        if (ETAT.perso === p.id) ETAT.perso = ETAT.persos[0].id;
        sauver(); majEntete(); vueArmoire();
      }) },
    ]);
  });
  $$('[data-sm]').forEach(b => b.onclick = () => {
    const p = perso(b.closest('[data-p]').dataset.p);
    const s = p.stuffs.find(x => x.id === b.dataset.sm);
    menu(b, [
      { t: 'Ouvrir dans le Builder', f: () => { ETAT.perso = p.id; p.actif = s.id; sauver(); majEntete(); allerA('builder'); } },
      { t: 'Renommer…', f: () => saisir('Renommer le stuff', s.nom, n => { s.nom = n; touche(s); vueArmoire(); }) },
      { t: 'Dupliquer', f: () => { const c = JSON.parse(JSON.stringify(s)); c.id = uid(); c.nom = s.nom + ' (copie)'; p.stuffs.push(c); touche(c); vueArmoire(); } },
      { t: 'Copier le lien de partage', f: () => copier(lienStuff(s), 'Lien du build copié') },
      { t: 'Copier le code du build', f: () => copier(encoder(s), 'Code du build copié') },
      { sep: true },
      { t: 'Supprimer le stuff', danger: true, f: () => confirmer('Supprimer « ' + s.nom + ' » ?', 'Ce stuff sera supprimé de ce navigateur.', () => {
        p.stuffs = p.stuffs.filter(x => x !== s);
        if (!p.stuffs.length) p.stuffs.push(nouveauStuff('Stuff principal'));
        if (p.actif === s.id) p.actif = p.stuffs[0].id;
        sauver(); majEntete(); vueArmoire();
      }) },
    ]);
  });
}
function importer() {
  ouvrirModale(`<h3>Importer un build</h3><p class="mini" style="margin:6px 0 12px">Colle un lien de partage de l'Atelier Mystic ou un code de build.</p>
    <textarea class="champ" id="iCode" style="width:100%;height:90px;padding:8px;resize:vertical"></textarea>
    <div class="rangee" style="justify-content:flex-end;margin-top:14px"><button class="btn fantome" id="iAnn">Annuler</button><button class="btn violet" id="iOk">Importer</button></div>`, m => {
    $('#iCode', m).focus();
    $('#iAnn', m).onclick = fermerModale;
    $('#iOk', m).onclick = () => {
      try {
        const s = decoder($('#iCode', m).value);
        const p = persoActif();
        p.stuffs.push(s); p.actif = s.id; touche(s); fermerModale(); toast('Build importé'); allerA('builder');
      } catch { toast('Code ou lien invalide'); }
    };
  });
}

// ============================================================ LIER MON PERSO (compte web Mystic, lecture seule)
const CACHE_PERSOS = new Map(); // id -> détail du personnage (équipement, inventaire, banque)
async function api(a, extra = '') {
  try {
    const r = await fetch('api/atelier.php?a=' + a + extra, { headers: { 'X-Mystic': '1' }, credentials: 'same-origin', cache: 'no-store' });
    const j = await r.json().catch(() => ({ ok: false, erreur: 'Réponse illisible du site.' }));
    if (r.status === 401) j.connexion = true;
    return j;
  } catch { return { ok: false, erreur: 'Site injoignable.' }; }
}
async function detailPerso(id, forcer) {
  if (!forcer && CACHE_PERSOS.has(id)) return CACHE_PERSOS.get(id);
  const r = await api('perso', '&id=' + id);
  if (!r.ok) throw new Error(r.erreur || 'Personnage introuvable');
  CACHE_PERSOS.set(id, r.perso);
  return r.perso;
}

/** Panneau « Mes personnages Mystic » : partir d'un perso (Builder) ou choisir les persos qui servent de stock (Craft). */
async function lierPerso(mode = 'builder') {
  ouvrirTiroir(`<div class="tiroir-tete"><h3>Mes personnages Mystic</h3><p>Pars de ton personnage (caracs et stuff actuels) pour construire ton objectif ; ses inventaires et la banque du compte servent de stock dans l'atelier de craft.</p>
    <button class="btn fantome petit tiroir-fermer" id="tFermer" aria-label="Fermer">✕</button></div><div class="tiroir-corps" id="tListe"><div class="vide">Chargement de tes personnages…</div></div>`, async m => {
    $('#tFermer', m).onclick = fermerTiroir;
    const r = await api('persos');
    const liste = $('#tListe', m);
    if (r.connexion) {
      liste.innerHTML = `<div style="padding:16px 8px"><p>Connecte-toi à ton compte Mystic pour retrouver tes personnages.</p><a class="btn violet" href="compte.html">Se connecter</a></div>`;
      return;
    }
    if (!r.ok) { liste.innerHTML = `<div class="vide">${esc(r.erreur || 'Erreur')}</div>`; return; }
    if (!r.persos.length) { liste.innerHTML = `<div class="vide">Aucun personnage sur les comptes de jeu rattachés à ton compte web. Crée ou rattache un compte de jeu depuis ton espace <a class="lien" href="compte.html">Compte</a>.</div>`; return; }
    ETAT.stock ||= { inv: true, banque: true, persos: [] };
    const dessiner = () => {
      liste.innerHTML = `${mode === 'craft' ? '<p class="mini" style="padding:8px 8px 0">Coche les personnages dont l\'inventaire sert de stock (la banque du compte est comptée une seule fois).</p>' : ''}
        ${r.persos.map(p => `<div class="choix" style="cursor:default;align-items:center"><span class="ico"><img src="atelier/icones/classes/tete_${p.classe * 10 + (p.sexe ? 1 : 0)}.png" alt="" onerror="this.style.visibility='hidden'"></span>
          <div class="txt"><b>${esc(p.nom)}</b><div class="mini">${esc(D.CLASSE.get(p.classe)?.n || '')} · niveau ${p.niveau} · compte ${esc(p.compte)}</div></div>
          <span class="inter ${ETAT.stock.persos.includes(p.id) ? 'on' : ''}" data-stock="${p.id}" title="Utiliser comme stock dans le craft"><i></i>Stock</span>
          <button class="btn ${mode === 'builder' ? 'violet' : ''} petit" data-partir="${p.id}">Partir de ce perso</button></div>`).join('')}`;
      $$('[data-stock]', liste).forEach(b => b.onclick = () => {
        const id = +b.dataset.stock;
        ETAT.stock.persos = ETAT.stock.persos.includes(id) ? ETAT.stock.persos.filter(x => x !== id) : [...ETAT.stock.persos, id];
        sauver(); dessiner(); if (ROUTE.vue === 'craft') vueCraft();
      });
      $$('[data-partir]', liste).forEach(b => b.onclick = async () => {
        b.disabled = true; b.textContent = 'Import…';
        try { await partirDuPerso(+b.dataset.partir); } catch (e) { toast(e.message); b.disabled = false; b.textContent = 'Partir de ce perso'; }
      });
    };
    dessiner();
  });
}

/** Crée un stuff à partir du vrai personnage : classe, niveau, caracs, parchos et objets équipés avec leurs jets réels. */
async function partirDuPerso(id) {
  const p = await detailPerso(id, true);
  const s = nouveauStuff(p.nom + ' (en jeu)', D.CLASSE.has(p.classe) ? p.classe : 8, Math.max(1, Math.min(200, p.niveau)));
  s.sexe = p.sexe ? 1 : 0;
  if (p.look) s.look = p.look;
  CARACS6.forEach(k => { s.base[k] = Math.max(0, p.base?.[k] | 0); s.parcho[k] = Math.max(0, Math.min(100, p.parcho?.[k] | 0)); });
  let n = 0;
  for (const [empl, it] of Object.entries(p.equipement || {})) {
    const o = D.OBJ.get(it.id);
    if (!o || !EMPL_K.has(empl)) continue;
    const reel = (it.effets || []).map(e => [...e]);
    const jets = (o.e || []).map(e => { const i = reel.findIndex(r => r[0] === e[0]); if (i < 0) return null; const v = Math.abs(reel[i][1]); reel.splice(i, 1); return v; });
    const slot = { id: o.id };
    if (jets.some(x => x != null)) slot.jets = jets;
    const exo = reel.find(r => statEffet(r[0]) && r[1]);
    if (exo) slot.exo = [exo[0], Math.abs(exo[1])];
    s.slots[empl] = slot; n++;
  }
  const perso = persoActif();
  perso.stuffs.push(s); perso.actif = s.id; touche(s);
  fermerTiroir(); toast(`${p.nom} importé : ${n} objet(s) avec leurs jets réels`); allerA('builder');
}

let CACHE_LISTE = null; // { t, persos }
async function listePersos(forcer) {
  if (!forcer && CACHE_LISTE && Date.now() - CACHE_LISTE.t < 15000) return CACHE_LISTE.persos;
  const r = await api('persos');
  if (r.connexion) throw Object.assign(new Error('Connecte-toi à ton compte Mystic pour utiliser ton stock en jeu.'), { connexion: true });
  if (!r.ok) throw new Error(r.erreur || 'Personnages indisponibles');
  CACHE_LISTE = { t: Date.now(), persos: r.persos };
  return r.persos;
}
/** Stock issu des personnages liés : inventaires + banque (une fois par compte), avec le détail de chaque source. */
async function stockLie(forcer) {
  const st = ETAT.stock;
  const total = new Map(), detail = new Map();
  const res = { total, detail, nb: 0, erreur: null, connexion: false, t: Date.now() };
  if (!st || (!st.inv && !st.banque)) return res;
  let ids = st.persos;
  if (st.tous) {
    try { ids = (await listePersos(forcer)).map(p => p.id); }
    catch (e) { res.erreur = e.message; res.connexion = !!e.connexion; return res; }
  }
  if (!ids.length) return res;
  const ajoute = (id, n, src) => {
    total.set(id, (total.get(id) || 0) + n);
    if (!detail.has(id)) detail.set(id, []);
    detail.get(id).push({ ...src, n });
  };
  const comptes = new Set();
  const lus = await Promise.all(ids.map(id => detailPerso(id, forcer).catch(e => ({ erreur: e.message }))));
  for (const p of lus) {
    if (p.erreur) { res.erreur = p.erreur; continue; }
    res.nb++;
    if (st.inv) for (const [k, v] of Object.entries(p.inventaire || {})) ajoute(+k, v, { t: 'perso', nom: p.nom, compte: p.compte, classe: p.classe, sexe: p.sexe });
    if (st.banque && !comptes.has(p.compte)) {
      comptes.add(p.compte);
      for (const [k, v] of Object.entries(p.banque || {})) ajoute(+k, v, { t: 'banque', compte: p.compte });
    }
  }
  return res;
}
/** Bulle au survol : où se trouvent les ressources (personnage, compte, banque). */
function bulleStock(ancre, sources, nomObjet) {
  let b = $('#bulleStock');
  if (!b) { b = document.createElement('div'); b.id = 'bulleStock'; b.className = 'bulle'; document.body.appendChild(b); }
  const persos = sources.filter(x => x.t === 'perso').sort((a, c) => c.n - a.n), banques = sources.filter(x => x.t === 'banque');
  b.innerHTML = `<b>${esc(nomObjet)}</b>
    ${persos.length ? `<div class="bulle-t">Sur tes personnages</div>${persos.map(x => `<div class="bulle-l"><img src="atelier/icones/classes/tete_${x.classe * 10 + (x.sexe ? 1 : 0)}.png" alt="" onerror="this.style.visibility='hidden'"><span>${esc(x.nom)}<small>compte ${esc(x.compte)}</small></span><b>${fmt(x.n)}</b></div>`).join('')}` : ''}
    ${banques.length ? `<div class="bulle-t">En banque</div>${banques.map(x => `<div class="bulle-l"><span class="bulle-banque">🏦</span><span>Banque<small>compte ${esc(x.compte)}</small></span><b>${fmt(x.n)}</b></div>`).join('')}` : ''}`;
  b.hidden = false;
  const r = ancre.getBoundingClientRect();
  b.style.left = Math.max(8, Math.min(window.innerWidth - b.offsetWidth - 8, r.left + r.width / 2 - b.offsetWidth / 2)) + 'px';
  b.style.top = (r.top - b.offsetHeight - 8 > 8 ? r.top - b.offsetHeight - 8 : r.bottom + 8) + 'px';
}
function cacherBulle() { const b = $('#bulleStock'); if (b) b.hidden = true; }
/** « +12 sur Otifapu », « +30 en banque », « +42 · 3 sources » */
function resumeSources(l) {
  if (!l || !l.length) return '';
  const n = l.reduce((a, x) => a + x.n, 0);
  if (l.length === 1) return '+' + fmt(n) + (l[0].t === 'banque' ? ' en banque' : ' sur ' + esc(l[0].nom));
  return '+' + fmt(n) + ' · ' + l.length + ' sources';
}

// relecture automatique du stock en jeu tant que l'atelier de craft est affiché
let MINUTEUR_CRAFT = null, DERNIER_STOCK = 0;
function suivreStock() {
  clearInterval(MINUTEUR_CRAFT);
  MINUTEUR_CRAFT = setInterval(() => {
    if (ROUTE.vue !== 'craft') { clearInterval(MINUTEUR_CRAFT); cacherBulle(); return; }
    const t = $('#cMaj');
    if (t && DERNIER_STOCK) { const x = Math.round((Date.now() - DERNIER_STOCK) / 1000); t.textContent = x < 3 ? "à l'instant" : 'il y a ' + x + ' s'; }
    if (document.hidden || document.activeElement?.matches('input,select,textarea') || !$('#tiroir').hidden || !$('#modale').hidden || $('#bulleStock:not([hidden])')) return;
    if (Date.now() - DERNIER_STOCK >= 20000) vueCraft(true);
  }, 1000);
}

// ============================================================ démarrage
const VUES = { builder: vueBuilder, panoplies: vuePanoplies, craft: vueCraft, recherche: vueRecherche, bestiaire: vueBestiaire, encyclopedie: vueEncyclopedie, armoire: vueArmoire };

(async () => {
  try { await chargerData(); }
  catch (e) { VUE.innerHTML = `<div class="at-charge">Les données de l'Atelier sont introuvables (${esc(e.message)}).</div>`; return; }
  $('#pied').textContent = `Données issues du serveur Mystic · export du ${D.genere || '—'} · les formules de caractéristiques suivent celles du serveur (PV, PA au niveau 100, sagesse, agilité).`;
  // build partagé par lien : ajouté dans le personnage actif
  const b = new URLSearchParams(location.search).get('b');
  if (b) {
    try {
      const s = decoder(b);
      s.nom = s.nom + ' (partagé)';
      const p = persoActif();
      p.stuffs.push(s); p.actif = s.id; sauver();
      setTimeout(() => toast('Build partagé ajouté à ton armoire'), 300);
    } catch { setTimeout(() => toast('Lien de build invalide'), 300); }
    history.replaceState(null, '', location.pathname + (location.hash || '#builder'));
  }
  $('#nbPanos').textContent = D.PANO.size;
  $('#nbObjets').textContent = fmt(D.objets.length);
  majEntete();
  rendre();
  majLies();
  chargerBestiaire().then(b => { $('#nbMonstres').textContent = fmt(b.monstres.length); }).catch(() => { });
})();
})();
