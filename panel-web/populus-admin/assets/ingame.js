/* Panel admin Populus 2.51 : barre de fenêtre quand le panel est ouvert dans le jeu (vue web Edge du client).
   La page envoie ses ordres au client par chrome.webview.postMessage (« action|a|b ») :
   w|largeur (mesure de l'échelle), move|dx|dy, resize|dw|dh, max, mini|1|hauteur / mini|0, end (enregistre), close.
   Redimensionnement par les 4 bords et les 4 coins, comme une fenêtre Windows. */
'use strict';
(function () {
    const wv = window.chrome && window.chrome.webview;
    if (!wv) return;
    const send = s => { try { wv.postMessage(s); } catch { } };
    document.body.classList.add('ingame');

    const header = document.querySelector('#app > header');
    if (!header) return;
    const box = document.createElement('div');
    box.className = 'winbtns';
    box.innerHTML = '<button id="wMin" title="Réduire à la barre / rouvrir">—</button>'
        + '<button id="wMax" title="Agrandir / taille normale">⬜</button>'
        + '<button id="wClose" title="Fermer le panel">✕</button>';
    header.appendChild(box);
    header.title = 'Glisser pour déplacer la fenêtre';

    const sendWidth = () => { if (!document.body.classList.contains('mini')) send('w|' + window.innerWidth); };
    sendWidth();
    window.addEventListener('resize', sendWidth);

    /** Glisser avec la souris (coordonnées écran : stables même quand la fenêtre bouge sous la souris). */
    function dragger(el, onDelta, skip) {
        let last = null;
        el.addEventListener('pointerdown', e => {
            if (e.button !== 0 || (skip && e.target.closest(skip))) return;
            last = { x: e.screenX, y: e.screenY };
            try { el.setPointerCapture(e.pointerId); } catch { }
            e.preventDefault();
        });
        el.addEventListener('pointermove', e => {
            if (!last) return;
            const dx = e.screenX - last.x, dy = e.screenY - last.y;
            if (!dx && !dy) return;
            last = { x: e.screenX, y: e.screenY };
            onDelta(dx, dy);
        });
        const stop = () => { if (last) { last = null; send('end'); setTimeout(sendWidth, 50); } };
        el.addEventListener('pointerup', stop);
        el.addEventListener('pointercancel', stop);
        el.addEventListener('lostpointercapture', stop);
    }

    dragger(header, (dx, dy) => send('move|' + dx + '|' + dy), 'button, input, select, a, label, .winbtns');

    // poignées : [classe, déplace x, déplace y, change largeur, change hauteur]
    const HANDLES = [
        ['r', 0, 0, 1, 0], ['b', 0, 0, 0, 1], ['l', 1, 0, -1, 0], ['t', 0, 1, 0, -1],
        ['br', 0, 0, 1, 1], ['bl', 1, 0, -1, 1], ['tr', 0, 1, 1, -1], ['tl', 1, 1, -1, -1],
    ];
    for (const [cls, mx, my, rw, rh] of HANDLES) {
        const h = document.createElement('div');
        h.className = 'wh wh-' + cls;
        document.body.appendChild(h);
        dragger(h, (dx, dy) => {
            if (mx || my) send('move|' + dx * mx + '|' + dy * my);
            send('resize|' + dx * rw + '|' + dy * rh);
        });
    }

    $('#wMin').onclick = () => {
        const mini = document.body.classList.toggle('mini');
        $('#wMin').textContent = mini ? '▢' : '—';
        send(mini ? 'mini|1|' + header.offsetHeight : 'mini|0|0');
        if (!mini) setTimeout(sendWidth, 100);
    };
    $('#wMax').onclick = () => { send('max'); setTimeout(sendWidth, 150); };
    header.addEventListener('dblclick', e => { if (!e.target.closest('button, input, select, a')) $('#wMax').click(); });
    $('#wClose').onclick = () => send('close');
})();
