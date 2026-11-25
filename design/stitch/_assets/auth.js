/* RutaAndina · comportamiento compartido de las pantallas de acceso (DESIGN.md §9).
   Cada página define window.RA_AUTH antes de cargar este archivo:
     slides    fotos del carrusel {file, focus:{x,y}, side, zoom?, place}
     fields    ids de los campos que valida el formulario
     validate  (valores) => {campo: 'mensaje' | ''}
     accept    (valores) => boolean   (simulación; en el frontend real lo decide el backend)
     wrong     {field, text}          error del servidor
     ok        texto de éxito anunciado a lectores de pantalla
     strength  true para el medidor de contraseña (registro)
   Todo lo que se mueve respeta prefers-reduced-motion. */
(() => {
  const C = window.RA_AUTH || {};
  const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const $ = id => document.getElementById(id);
  const live = $('live');
  const say = t => { if (live) live.textContent = t; };

  /* ---------- Carrusel de fondo ---------- */
  const A = C.assets || '../../_assets/login/', SLIDES = C.slides || [], DURATION = 8000;
  const pan = $('pan'), cap = $('caption'), where = $('where'), whereTxt = $('where-txt'), dots = $('dots');
  if (pan && SLIDES.length) {
    const ready = []; let current = -1, timer = null;
    const load = src => new Promise(r => { const i = new Image(); i.decoding = 'async'; i.onload = () => r(i); i.onerror = () => r(null); i.src = src; });

    /* Encuadre: lleva el vehículo (focus) a un punto visible sin pasar el zoom máximo de la foto */
    const frame = it => {
      const img = it.img, f = it.focus, M = 24, w = img.naturalWidth, h = img.naturalHeight;
      const vw = innerWidth + M*2, vh = innerHeight + M*2, side = innerWidth >= 980 && it.side;
      const tx = M + innerWidth * (side ? .80 : .5), ty = M + innerHeight * (side ? .72 : (innerWidth < 520 ? .88 : .62));
      const cover = Math.max(vw/w, vh/h);
      const s = Math.min(cover*(it.zoom||1.2), Math.max(cover, tx/(f.x*w), (vw-tx)/((1-f.x)*w), ty/(f.y*h), (vh-ty)/((1-f.y)*h)));
      const W = w*s, H = h*s, clamp = (v, lo) => Math.min(0, Math.max(lo, v));
      const L = clamp(tx-f.x*W, vw-W), T = clamp(ty-f.y*H, vh-H);
      Object.assign(img.style, {width:W+'px', height:H+'px', left:(L-M)+'px', top:(T-M)+'px', transformOrigin:`${f.x*100}% ${f.y*100}%`});
    };
    addEventListener('resize', () => ready.forEach(frame));

    const show = i => {
      const prev = ready[current]; current = i; const it = ready[i];
      frame(it); it.el.classList.remove('active'); void it.el.offsetWidth; it.el.classList.add('active');
      if (prev && prev !== it) setTimeout(() => prev.el.classList.remove('active'), 60);
      where.classList.add('out'); setTimeout(() => { whereTxt.textContent = it.place; where.classList.remove('out'); }, 280);
      [...dots.children].forEach((d, k) => d.className = k < i ? 'done' : '');
      const d = dots.children[i]; if (d && !reduce) { d.style.setProperty('--dur', DURATION + 'ms'); void d.offsetWidth; d.className = 'run'; }
    };
    const play = () => { clearInterval(timer); if (!reduce && ready.length > 1) timer = setInterval(() => show((current + 1) % ready.length), DURATION); };
    document.addEventListener('visibilitychange', () => document.hidden ? clearInterval(timer) : play());   // pausa con la pestaña oculta

    const loads = SLIDES.map(cfg => load(A + cfg.file));       // la primera foto de la lista siempre abre el carrusel
    SLIDES.forEach((cfg, idx) => (async () => {
      const img = await loads[idx]; if (idx > 0) await loads[0]; if (!img) return;
      const el = document.createElement('div'); el.className = 'slide'; img.alt = ''; el.appendChild(img); pan.appendChild(el);
      const it = {...cfg, img, el, order: idx}, active = ready[current];
      ready.push(it); ready.sort((a, b) => a.order - b.order);
      const pos = ready.indexOf(it); dots.insertBefore(document.createElement('i'), dots.children[pos] || null);
      dots.style.display = ready.length > 1 ? 'flex' : 'none';
      if (current === -1) { cap.hidden = false; show(pos); }
      else { current = ready.indexOf(active); if (pos < current) dots.children[pos].className = 'done'; }
      if (!timer) play();
    })());

    /* Parallax decorativo: solo con mouse, se suaviza con la transición de .pan (no se actualiza "en seco") */
    if (!reduce && matchMedia('(hover:hover) and (pointer:fine)').matches) {
      let raf; addEventListener('pointermove', e => { cancelAnimationFrame(raf); raf = requestAnimationFrame(() => {
        pan.style.transform = `translate3d(${-(e.clientX/innerWidth-.5)*14}px,${-(e.clientY/innerHeight-.5)*8}px,0)`; }); });
    }
  }

  /* ---------- Botones con estados (normal → cargando → listo) ---------- */
  const setState = (btn, s) => {
    btn.dataset.state = s;
    btn.querySelectorAll('.layer').forEach(l => { const on = l.dataset.layer === s; l.classList.toggle('on', on); l.setAttribute('aria-hidden', !on); });
    btn.setAttribute('aria-busy', s === 'loading');
  };
  document.querySelectorAll('[data-state]').forEach(b => setState(b, b.dataset.state));

  /* ---------- Mensajes bajo el campo (se abren, no aparecen de golpe) ---------- */
  const reveal = (id, text) => {
    const r = $('r-' + id); if (!r) return;
    if (text) r.querySelector('p').textContent = text;   // al ocultar se conserva el texto para que se cierre con él
    r.classList.toggle('show', !!text);
  };
  const setErr = (id, text) => {
    const f = $('f-' + id); if (f) f.classList.toggle('invalid', !!text);
    const input = $(id); if (input) input.setAttribute('aria-invalid', !!text);
    reveal(id, text);
  };
  const card = $('card');
  const shake = () => { if (!card) return; card.classList.remove('shake'); void card.offsetWidth; card.classList.add('shake'); };

  /* ---------- Contraseña: mostrar/ocultar (sin animación: es un toggle frecuente) y Bloq Mayús ---------- */
  const pass = $('pass'), tg = $('toggle'), eye = $('eye');
  const OPEN = '<path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z"/><circle cx="12" cy="12" r="3"/>';
  const SHUT = '<path d="M3 3l18 18"/><path d="M10.6 5.1A10.6 10.6 0 0 1 12 5c6.5 0 10 7 10 7a17 17 0 0 1-3.2 4.1M6.6 6.6C3.9 8.4 2 12 2 12s3.5 7 10 7a9.7 9.7 0 0 0 5.4-1.6"/><path d="M9.9 9.9a3 3 0 0 0 4.2 4.2"/>';
  if (pass && tg) {
    tg.addEventListener('click', () => {
      const s = pass.type === 'password'; pass.type = s ? 'text' : 'password'; if (eye) eye.innerHTML = s ? SHUT : OPEN;
      tg.setAttribute('aria-pressed', s); tg.setAttribute('aria-label', s ? 'Ocultar contraseña' : 'Mostrar contraseña');
      pass.focus({preventScroll:true}); const n = pass.value.length; pass.setSelectionRange(n, n);
    });
    if ($('r-caps')) ['keydown','keyup'].forEach(ev => pass.addEventListener(ev, e => {
      if (e.getModifierState) reveal('caps', e.getModifierState('CapsLock') ? 'Bloq Mayús está activado.' : '');
    }));
  }

  /* ---------- Medidor de contraseña (registro) ---------- */
  if (C.strength && pass) {
    const bars = [...document.querySelectorAll('.meter i')], hint = $('strength');
    const TXT = ['Usa al menos 8 caracteres.', 'Contraseña débil. Combina mayúsculas y minúsculas.', 'Contraseña regular. Agrega un número.',
                 'Contraseña buena. Agrega un símbolo para hacerla fuerte.', 'Contraseña fuerte.'];
    const rate = v => v.length < 8 ? 0 : 1 + (/[a-z]/.test(v) && /[A-Z]/.test(v)) + /\d/.test(v) + /[^A-Za-z0-9]/.test(v);
    const paint = () => { const n = rate(pass.value); bars.forEach((b, k) => b.classList.toggle('on', k < n)); bars.forEach(b => b.dataset.level = n); if (hint) hint.textContent = TXT[n]; };
    pass.addEventListener('input', paint); paint();
  }

  /* ---------- Google (simulado) ----------
     En el frontend real abre Google Identity Services y el backend verifica el ID token.
     Aquí solo se muestra el estado "Conectando…" y vuelve al estado normal. */
  const g = $('google');
  if (g) g.addEventListener('click', () => {
    if (g.dataset.state !== 'idle') return;
    setState(g, 'loading'); say('Conectando con Google…');
    setTimeout(() => { setState(g, 'idle'); say(''); }, 1600);
  });

  /* ---------- Envío del formulario (simulado) ---------- */
  const form = $('form'), btn = $('submit');
  if (form && btn) {
    const F = C.fields || [];
    F.forEach(id => $(id) && $(id).addEventListener('input', () => setErr(id, '')));
    form.addEventListener('submit', e => {
      e.preventDefault();
      if (btn.dataset.state !== 'idle') return;                 // evita doble envío sin deshabilitar (no se pierde el foco)
      const v = Object.fromEntries(F.map(id => { const el = $(id); return [id, el.type === 'password' ? el.value : el.value.trim()]; }));   // la contraseña nunca se recorta
      const errs = C.validate ? C.validate(v) : {};
      let first = null;
      F.forEach(id => { setErr(id, errs[id] || ''); if (errs[id] && !first) first = $(id); });
      if (first) { shake(); first.focus(); return; }
      setState(btn, 'loading'); say('Verificando…');
      setTimeout(() => {
        if (!C.accept || C.accept(v)) { setState(btn, 'done'); say(C.ok || 'Listo'); setTimeout(() => setState(btn, 'idle'), 2400); }
        else { setState(btn, 'idle'); setErr(C.wrong.field, C.wrong.text); shake(); say(C.wrong.text); $(C.wrong.field).select(); }
      }, 1200);
    });
  }
})();
