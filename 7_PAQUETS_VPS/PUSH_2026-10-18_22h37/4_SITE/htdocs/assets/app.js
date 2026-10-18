// Mystic : animations de la page (accueil « L'Équilibre », révélations au défilement, cartes, jauge de guerre)
(function(){
  const $ = s => document.querySelector(s), $$ = s => [...document.querySelectorAll(s)];
  const hero = $('#accueil');

  // ---------- en-tête : fond au défilement, menu mobile, lien actif ----------
  const entete = $('#entete');
  addEventListener('scroll', () => entete.classList.toggle('colle', scrollY > 40), {passive:true});
  const burger = $('#burger');
  burger.addEventListener('click', () => { const o = document.body.classList.toggle('menu-ouvert'); burger.setAttribute('aria-expanded', o); });
  $$('#menu a').forEach(a => a.addEventListener('click', () => { document.body.classList.remove('menu-ouvert'); burger.setAttribute('aria-expanded', 'false'); }));
  const liens = $$('#menu a');
  const obs = new IntersectionObserver(es => es.forEach(e => { if(e.isIntersecting) liens.forEach(l => l.classList.toggle('actif', l.getAttribute('href') === '#' + e.target.id)); }), {rootMargin:'-45% 0px -50% 0px'});
  ['serveur','camps','classements','nouveautes','rejoindre'].forEach(id => { const el = document.getElementById(id); if(el) obs.observe(el); });

  // ---------- accroche tapée lettre par lettre ----------
  const acc = $('#accroche'), texte = acc.textContent; acc.textContent = '';
  let i = 0; setTimeout(function tape(){ acc.textContent = texte.slice(0, ++i); if(i < texte.length) setTimeout(tape, 32); }, 2000);

  // ---------- survol : un camp s'éclaire, parallaxe ----------
  hero.addEventListener('pointermove', e => {
    const r = hero.getBoundingClientRect(), x = (e.clientX - r.left)/r.width, y = (e.clientY - r.top)/r.height;
    hero.classList.toggle('focusG', x < .44); hero.classList.toggle('focusD', x > .56);
    $('#mg .peint').style.transform = `translate(${(.5-x)*22}px,${(.5-y)*12}px)`;
    $('#md .peint').style.transform = `translate(${(.5-x)*22}px,${(y-.5)*12}px)`;
  });
  hero.addEventListener('pointerleave', () => hero.classList.remove('focusG','focusD'));

  // ---------- étincelles violettes le long de la couture ----------
  const cv = $('#etincelles'), ctx = cv.getContext('2d'); let parts = [], L = 0, H = 0;
  function taille(){ const d = Math.min(devicePixelRatio || 1, 2); L = cv.clientWidth; H = cv.clientHeight; cv.width = L*d; cv.height = H*d; ctx.setTransform(d,0,0,d,0,0); }
  taille(); addEventListener('resize', taille);
  (function pas(){
    if(hero.getBoundingClientRect().bottom > 0){
      ctx.clearRect(0,0,L,H);
      const cx = $('#couture').getBoundingClientRect().left - hero.getBoundingClientRect().left;
      for(let k=0;k<2;k++){ const g = Math.random() < .5; parts.push({x:cx, y:Math.random()*H, vx:(g?-1:1)*(.8+Math.random()*3.2), vy:(Math.random()-.5)*1.6, v:1, c: g ? '200,220,255' : (Math.random()<.5 ? '255,160,110' : '190,160,255')}); }
      parts = parts.filter(p => { p.x += p.vx; p.y += p.vy; p.vy += .02; p.v -= .018; return p.v > 0; });
      for(const p of parts){ ctx.fillStyle = `rgba(${p.c},${p.v})`; ctx.fillRect(p.x, p.y, 2, 2); }
    }
    requestAnimationFrame(pas);
  })();

  // ---------- cartes : inclinaison 3D et reflet ; règles : lueur qui suit la souris ----------
  $$('.carte').forEach(c => {
    c.addEventListener('pointermove', e => { const r = c.getBoundingClientRect(), x = (e.clientX - r.left)/r.width, y = (e.clientY - r.top)/r.height;
      c.style.transform = `perspective(1000px) rotateY(${(x-.5)*12}deg) rotateX(${(.5-y)*12}deg)`; c.style.setProperty('--mx', x*100+'%'); c.style.setProperty('--my', y*100+'%'); });
    c.addEventListener('pointerleave', () => c.style.transform = '');
  });
  $$('.regle').forEach(c => c.addEventListener('pointermove', e => { const r = c.getBoundingClientRect(); c.style.setProperty('--mx', (e.clientX-r.left)+'px'); c.style.setProperty('--my', (e.clientY-r.top)+'px'); }));

  // ---------- outils ----------
  const echap = t => String(t ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const nombre = n => Number(n || 0).toLocaleString('fr-FR');
  async function api(url, opts){ let d = null; try { const r = await fetch(url, opts); d = await r.json(); } catch(e){} return d || {ok:false, erreur:'Le serveur ne répond pas pour le moment.'}; }

  // ---------- guerre des alignements : jauge réelle, fil d'exemple ----------
  const zones = ['Astrub','Amakna','Cania','Sidimote','Frigost','Otomaï','Pandala','Sufokia','Koalak','Moon','Bord de la forêt','Montagne des Koalaks','Landes de Sidimote','Plaines de Cania'];
  const fil = $('#fil');
  function jauge(b, k){
    const t = b + k, p = t ? Math.round(b * 100 / t) : 50;
    $('#jauge').style.width = p + '%'; $('#pointe').style.left = p + '%';
    $('#pb').textContent = 'Bonta ' + p + ' %'; $('#pk').textContent = 'Brâkmar ' + (100-p) + ' %';
    $('#guerreTexte').textContent = t ? `${nombre(b)} personnages pour Bonta, ${nombre(k)} pour Brâkmar.` : "Aucun personnage aligné pour l'instant : à toi d'ouvrir le bal.";
  }
  function evenement(){
    const b = Math.random() < .5, li = document.createElement('li'), h = new Date().toTimeString().slice(0,5);
    li.innerHTML = `<time>${h}</time><span><b style="color:${b ? 'var(--ange)' : 'var(--demon)'}">${b ? 'Bonta' : 'Brâkmar'}</b> prend le prisme de ${zones[Math.random()*zones.length|0]}</span>`;
    fil.prepend(li); while(fil.children.length > 4) fil.lastChild.remove();
  }
  jauge(0, 0); for(let k=0;k<4;k++) evenement(); setInterval(evenement, 3800);

  // ---------- statut du serveur ----------
  async function statut(){
    const d = await api('api/statut.php'), pastille = $('#pastille');
    const enLigne = !!(d.ok && d.auth && d.monde);
    pastille.classList.toggle('en-ligne', enLigne); pastille.classList.toggle('hors-ligne', !enLigne);
    pastille.querySelector('span').textContent = !d.ok ? 'Statut inconnu' : enLigne ? 'En ligne' : 'Hors ligne';
    if(!d.ok){ $('#tVerif').textContent = 'Impossible de joindre le serveur pour le moment.'; return; }
    const tuile = (id, ok) => { const t = $(id); t.classList.toggle('ok', ok); t.classList.toggle('ko', !ok); t.querySelector('.valeur').textContent = ok ? 'En ligne' : 'Hors ligne'; };
    tuile('#tAuth', !!d.auth); tuile('#tMonde', !!d.monde);
    if(d.bdd){
      $('#tComptes').textContent = nombre(d.comptes); $('#tPerso').textContent = nombre(d.personnages);
      $('#tActifs').textContent = nombre(d.actifs_24h); $('#tGuildes').textContent = nombre(d.guildes);
      jauge(d.bonta || 0, d.brakmar || 0);
    }
    $('#tVerif').textContent = 'Vérifié à ' + d.verifie_a + '. Mis à jour toutes les minutes.';
  }
  statut(); setInterval(statut, 60000);

  // ---------- classements ----------
  const tete = $('#cTete'), corps = $('#cCorps'), fClasse = $('#fClasse'), fCamp = $('#fCamp');
  let typeC = 'joueurs', classesChargees = false, requete = 0;
  const colonnes = {
    joueurs: ['#','Personnage','Classe','Niveau','Camp'],
    pvp: ['#','Personnage','Camp','Grade','Honneur','Niveau'],
    guildes: ['#','Guilde','Niveau','Membres']
  };
  const campCss = c => c === 'Bonta' ? 'camp-b' : c === 'Brâkmar' ? 'camp-k' : 'camp-n';
  const perso = l => `<div class="perso">${l.avatar ? `<img src="${echap(l.avatar)}" alt="" loading="lazy" onerror="this.style.visibility='hidden'">` : ''}<b>${echap(l.nom)}</b></div>`;
  function ligne(l){
    if(typeC === 'guildes') return `<td class="rang">${l.rang}</td><td><b>${echap(l.nom)}</b></td><td>${l.niveau}</td><td>${nombre(l.membres)}</td>`;
    if(typeC === 'pvp') return `<td class="rang">${l.rang}</td><td>${perso(l)}</td><td class="${campCss(l.camp)}">${echap(l.camp)}</td><td>${l.grade}</td><td>${nombre(l.honneur)}</td><td>${l.niveau}</td>`;
    return `<td class="rang">${l.rang}</td><td>${perso(l)}</td><td>${echap(l.classe)}</td><td>${l.niveau}</td><td class="${campCss(l.camp)}">${echap(l.camp)}</td>`;
  }
  async function classement(){
    const n = ++requete, cols = colonnes[typeC];
    tete.innerHTML = '<tr>' + cols.map(c => `<th>${c}</th>`).join('') + '</tr>';
    corps.innerHTML = `<tr><td class="vide" colspan="${cols.length}">Chargement…</td></tr>`;
    $('#filtres').hidden = typeC === 'guildes';
    const q = new URLSearchParams({type: typeC, classe: fClasse.value, camp: fCamp.value});
    const d = await api('api/classement.php?' + q);
    if(n !== requete) return;
    if(!d.ok){ corps.innerHTML = `<tr><td class="vide" colspan="${cols.length}">${echap(d.erreur)}</td></tr>`; return; }
    if(!classesChargees && d.classes){
      classesChargees = true;
      Object.entries(d.classes).sort((a, b) => a[1].localeCompare(b[1], 'fr')).forEach(([id, nom]) => fClasse.add(new Option(nom, id)));
    }
    corps.innerHTML = d.lignes.length
      ? d.lignes.map((l, i) => `<tr style="animation-delay:${Math.min(i, 20) * 30}ms">${ligne(l)}</tr>`).join('')
      : `<tr><td class="vide" colspan="${cols.length}">Personne dans ce classement pour l'instant.</td></tr>`;
  }
  $$('.onglets button').forEach(b => b.addEventListener('click', () => {
    $$('.onglets button').forEach(x => x.setAttribute('aria-selected', x === b));
    typeC = b.dataset.type; classement();
  }));
  fClasse.addEventListener('change', classement); fCamp.addEventListener('change', classement);
  classement();

  // ---------- inscription ----------
  const form = $('#formInscription'), msg = $('#msgInscription'), bouton = $('#bInscription');
  let jeton = '';
  async function nouveauJeton(){
    const d = await api('api/compte_web.php?a=jeton'); jeton = d.ok ? d.jeton : '';
    if(!d.ok){ msg.className = 'message ko'; msg.textContent = d.erreur; bouton.disabled = true; }
  }
  nouveauJeton();
  function erreursChamps(champs){
    $$('#formInscription .champ').forEach(c => c.classList.remove('erreur'));
    $$('#formInscription .err').forEach(e => e.textContent = '');
    Object.entries(champs || {}).forEach(([nom, texte]) => {
      const e = $(`#formInscription .err[data-pour="${nom}"]`); if(!e) return;
      e.textContent = texte; e.closest('.champ').classList.add('erreur');
    });
  }
  form.addEventListener('submit', async ev => {
    ev.preventDefault();
    const v = Object.fromEntries(new FormData(form));
    if(v.mot_de_passe !== v.confirmation){ erreursChamps({confirmation: 'Les deux mots de passe ne correspondent pas.'}); return; }
    erreursChamps(); bouton.disabled = true; msg.className = 'message'; msg.textContent = 'Création du compte…';
    // MYSTIC 18/10 : compte web (les comptes de jeu se créent ensuite dans « Mon compte »)
    const d = await api('api/compte_web.php?a=inscription', {method:'POST', headers:{'Content-Type':'application/json', 'X-Mystic':'1'}, body: JSON.stringify({...v, jeton})});
    bouton.disabled = false;
    if(d.ok){ msg.className = 'message ok'; msg.textContent = d.message; form.reset(); setTimeout(() => location.href = 'compte.html', 900); return; }
    msg.className = 'message ko'; msg.textContent = d.erreur; erreursChamps(d.champs);
    if(/expiré/.test(d.erreur || '')) nouveauJeton();
  });

  // ---------- défilement (GSAP) ----------
  if(!(window.gsap && window.ScrollTrigger)) return;
  gsap.registerPlugin(ScrollTrigger);
  gsap.timeline({scrollTrigger:{trigger:'#accueil', start:'top top', end:'+=110%', scrub:.6, pin:true}})
    .to('#mg', {xPercent:-100, yPercent:-10, rotate:-4, ease:'power2.in'}, 0)
    .to('#md', {xPercent:100, yPercent:10, rotate:4, ease:'power2.in'}, 0)
    .to('#couture', {scaleY:0, opacity:0}, 0)
    .to('#heroCentre', {y:-140, opacity:0, scale:.92}, 0)
    .fromTo('#derriere', {scale:.85}, {scale:1}, 0);
  // révélations : les éléments montent en place (toujours lisibles même sans animation)
  $$('.revele').forEach(el => gsap.fromTo(el, {y:46, opacity:.15}, {y:0, opacity:1, duration:1, ease:'power3.out', scrollTrigger:{trigger:el, start:'top 88%'}}));
  // parallaxe des villes et du fond Discord
  gsap.to('.ville.bonta', {yPercent:-12, ease:'none', scrollTrigger:{trigger:'#camps', start:'top bottom', end:'bottom top', scrub:true}});
  gsap.to('.ville.brak', {yPercent:12, ease:'none', scrollTrigger:{trigger:'#camps', start:'top bottom', end:'bottom top', scrub:true}});
  gsap.fromTo('.discord', {'--py':'-40px'}, {'--py':'40px', ease:'none', scrollTrigger:{trigger:'#discord', start:'top bottom', end:'bottom top', scrub:true}});
})();
