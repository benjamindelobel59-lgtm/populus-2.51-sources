// Mystic : page « Mon compte » (compte web, comptes de jeu liés, création d'un compte de jeu)
(function(){
  const $ = s => document.querySelector(s), $$ = s => [...document.querySelectorAll(s)];
  async function api(url, corps){
    const opts = corps === undefined ? {} : {method:'POST', headers:{'Content-Type':'application/json', 'X-Mystic':'1'}, body: JSON.stringify(corps)};
    let d = null; try { const r = await fetch(url, Object.assign({credentials:'same-origin'}, opts)); d = await r.json(); } catch(e){}
    return d || {ok:false, erreur:'Le serveur ne répond pas pour le moment.'};
  }
  const texte = (el, t) => { el.textContent = t; return el; };
  const cree = (tag, cls) => { const e = document.createElement(tag); if(cls) e.className = cls; return e; };

  function erreursChamps(form, champs){
    form.querySelectorAll('.champ').forEach(c => c.classList.remove('erreur'));
    form.querySelectorAll('.err').forEach(e => e.textContent = '');
    Object.entries(champs || {}).forEach(([nom, t]) => {
      const e = form.querySelector(`.err[data-pour="${nom}"]`); if(!e) return;
      e.textContent = t; e.closest('.champ').classList.add('erreur');
    });
  }

  async function charger(){
    const d = await api('api/compte_web.php?a=moi');
    if(!d.ok || !d.connecte){ $('#vueCompte').hidden = true; $('#vueConnexion').hidden = false; return; }
    $('#vueConnexion').hidden = true; $('#vueCompte').hidden = false;
    texte($('#titrePseudo'), 'Bonjour ' + d.pseudo);
    texte($('#emailWeb'), d.email);
    texte($('#compteur'), `(${d.comptes.length} / ${d.max})`);
    $('#formJeu').hidden = d.comptes.length >= d.max;
    $('#formRattacher').hidden = d.comptes.length >= d.max;
    const liste = $('#comptesJeu'); liste.replaceChildren();
    if(!d.comptes.length){ liste.append(texte(cree('p', 'vide'), "Aucun compte de jeu pour l'instant : crée le premier ci-dessous.")); return; }
    d.comptes.forEach(c => {
      const carte = cree('article', 'compte-jeu');
      carte.append(texte(cree('h3'), c.login), texte(cree('p', 'sous'), 'Pseudo : ' + c.pseudo + ' · ' + c.personnages.length + ' personnage' + (c.personnages.length > 1 ? 's' : '')));
      if(!c.personnages.length) carte.append(texte(cree('p', 'vide'), 'Aucun personnage : crée-le en jeu.'));
      c.personnages.forEach(p => {
        const ligne = cree('div', 'perso-web'), img = cree('img'), infos = cree('div');
        img.alt = ''; img.loading = 'lazy'; if(p.avatar) img.src = p.avatar;
        infos.append(texte(cree('b'), p.nom), texte(cree('span'), p.classe + ' · niveau ' + p.niveau));
        ligne.append(img, infos); carte.append(ligne);
      });
      liste.append(carte);
    });
  }

  $('#formConnexion').addEventListener('submit', async ev => {
    ev.preventDefault();
    const msg = $('#msgConnexion'), b = $('#bConnexion'), v = Object.fromEntries(new FormData(ev.target));
    b.disabled = true; msg.className = 'message'; msg.textContent = 'Connexion…';
    const d = await api('api/compte_web.php?a=connexion', v);
    b.disabled = false;
    if(!d.ok){ msg.className = 'message ko'; msg.textContent = d.erreur; return; }
    msg.textContent = ''; ev.target.reset(); charger();
  });

  $('#bDeconnexion').addEventListener('click', async () => { await api('api/compte_web.php?a=deconnexion', {}); charger(); });

  $('#formJeu').addEventListener('submit', async ev => {
    ev.preventDefault();
    const f = ev.target, msg = $('#msgJeu'), b = $('#bJeu'), v = Object.fromEntries(new FormData(f));
    if(v.mot_de_passe !== v.confirmation){ erreursChamps(f, {confirmation:'Les deux mots de passe ne correspondent pas.'}); return; }
    erreursChamps(f); b.disabled = true; msg.className = 'message'; msg.textContent = 'Création du compte de jeu…';
    const d = await api('api/compte_web.php?a=creer_jeu', v);
    b.disabled = false;
    if(!d.ok){ msg.className = 'message ko'; msg.textContent = d.erreur; erreursChamps(f, d.champs); return; }
    msg.className = 'message ok'; msg.textContent = d.message; f.reset(); charger();
  });

  // rattacher un compte de jeu créé avant les comptes web
  $('#formRattacher').addEventListener('submit', async ev => {
    ev.preventDefault();
    const f = ev.target, msg = $('#msgRattacher'), b = $('#bRattacher'), v = Object.fromEntries(new FormData(f));
    if(!v.login.trim() || !v.mot_de_passe){ msg.className = 'message ko'; msg.textContent = 'Indique le nom de compte et le mot de passe du jeu.'; return; }
    b.disabled = true; msg.className = 'message'; msg.textContent = 'Vérification…';
    const d = await api('api/compte_web.php?a=rattacher', v);
    b.disabled = false;
    if(!d.ok){ msg.className = 'message ko'; msg.textContent = d.erreur; return; }
    msg.className = 'message ok'; msg.textContent = d.message; f.reset(); charger();
  });

  charger();
})();
