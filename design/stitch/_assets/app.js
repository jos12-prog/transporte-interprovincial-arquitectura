/* RutaAndina · utilidades de las maquetas: íconos (trazos Lucide), mapas de asientos por VehicleModel, gráficos SVG y QR de muestra.
   Solo para revisión visual. En el frontend: lucide-react, componente <SeatMap model=… /> y una librería de gráficos (p. ej. Recharts). */
(() => {
  /* ---------- Íconos: <i data-i="nombre" class="sm|lg"></i> ---------- */
  const P = {
    search:'<circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/>',
    user:'<circle cx="12" cy="8" r="4"/><path d="M4 20c1.5-3.5 4.5-5 8-5s6.5 1.5 8 5"/>',
    users:'<circle cx="9" cy="8" r="3.5"/><path d="M2.5 20c1-3.2 3.6-5 6.5-5s5.5 1.8 6.5 5"/><path d="M16 4.5a3.5 3.5 0 0 1 0 7M18.5 15c1.6.8 2.6 2.4 3 5"/>',
    lock:'<rect x="4" y="10" width="16" height="11" rx="2.5"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/>',
    mail:'<rect x="3" y="5" width="18" height="14" rx="2.5"/><path d="m4 7 8 6 8-6"/>',
    phone:'<path d="M5 4h4l2 5-2.5 1.5a11 11 0 0 0 5 5L15 13l5 2v4a2 2 0 0 1-2 2A16 16 0 0 1 3 6a2 2 0 0 1 2-2"/>',
    card:'<rect x="2.5" y="5" width="19" height="14" rx="2.5"/><path d="M2.5 10h19M6 15h4"/>',
    id:'<rect x="3" y="4" width="18" height="16" rx="2.5"/><circle cx="9" cy="10" r="2.2"/><path d="M5.5 16c.7-1.7 2-2.5 3.5-2.5s2.8.8 3.5 2.5M15 9h3M15 13h3"/>',
    pin:'<path d="M12 21s-7-6.2-7-12a7 7 0 0 1 14 0c0 5.8-7 12-7 12Z"/><circle cx="12" cy="9" r="2.5"/>',
    calendar:'<rect x="3" y="5" width="18" height="16" rx="2.5"/><path d="M3 10h18M8 3v4M16 3v4"/>',
    clock:'<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
    swap:'<path d="M7 4 3 8l4 4M3 8h14M17 20l4-4-4-4M21 16H7"/>',
    arrow:'<path d="M5 12h14M13 6l6 6-6 6"/>',
    back:'<path d="M19 12H5M11 6l-6 6 6 6"/>',
    chevron:'<path d="m6 9 6 6 6-6"/>', right:'<path d="m9 6 6 6-6 6"/>',
    check:'<path d="m5 12 5 5 9-10"/>', x:'<path d="M18 6 6 18M6 6l12 12"/>', plus:'<path d="M12 5v14M5 12h14"/>',
    flame:'<path d="M12 3c1 3.5 5 5.5 5 10a5 5 0 0 1-10 0c0-2 1-3.5 2-4.5.3 1.6 1.2 2.5 2 2.5-1-3 0-6 1-8Z"/>',
    bus:'<rect x="4" y="3" width="16" height="15" rx="3"/><path d="M4 11h16M8 18v2.5M16 18v2.5"/><circle cx="8" cy="14.5" r="1"/><circle cx="16" cy="14.5" r="1"/>',
    car:'<path d="M5 16V11l2-5h10l2 5v5"/><path d="M3 16h18v2H3zM5 11h14"/><circle cx="7.5" cy="18.5" r="1.5"/><circle cx="16.5" cy="18.5" r="1.5"/>',
    ticket:'<path d="M3 8a2 2 0 0 0 2-2h14a2 2 0 0 0 2 2v2a2 2 0 0 0 0 4v2a2 2 0 0 0-2 2H5a2 2 0 0 0-2-2v-2a2 2 0 0 0 0-4Z"/><path d="M14 6v12" stroke-dasharray="2 2"/>',
    download:'<path d="M12 4v11M7 10l5 5 5-5M5 20h14"/>', print:'<path d="M7 9V3h10v6"/><rect x="3" y="9" width="18" height="8" rx="2"/><path d="M7 14h10v7H7z"/>',
    whatsapp:'<path d="M4 20l1.3-3.8A8 8 0 1 1 8 19z"/><path d="M9 9.5c0 3 2.5 5.5 5.5 5.5l1-1.5-2-1-1 .8c-1-.5-1.8-1.3-2.3-2.3l.8-1-1-2Z"/>',
    shield:'<path d="M12 3 5 6v6c0 4.4 3 7.8 7 9 4-1.2 7-4.6 7-9V6Z"/><path d="m9 12 2 2 4-4"/>',
    bell:'<path d="M6 16V11a6 6 0 0 1 12 0v5l2 2H4Z"/><path d="M10 21h4"/>',
    home:'<path d="m3 11 9-8 9 8"/><path d="M5 10v10h14V10"/>',
    grid:'<rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/>',
    route:'<circle cx="6" cy="19" r="2.5"/><circle cx="18" cy="5" r="2.5"/><path d="M8.5 19H16a3.5 3.5 0 0 0 0-7H8a3.5 3.5 0 0 1 0-7h7.5"/>',
    tag:'<path d="M3 12V4h8l10 10-8 8Z"/><circle cx="7.5" cy="8.5" r="1.5"/>',
    chart:'<path d="M4 20V10M10 20V4M16 20v-7M22 20H2"/>', report:'<path d="M6 3h9l4 4v14H6z"/><path d="M14 3v5h5M9 13h7M9 17h5"/>',
    alert:'<path d="M12 3 2 20h20Z"/><path d="M12 10v4M12 17.5v.01"/>',
    building:'<rect x="4" y="3" width="16" height="18" rx="1.5"/><path d="M9 7h1M14 7h1M9 11h1M14 11h1M9 15h1M14 15h1M10 21v-3h4v3"/>',
    store:'<path d="M4 9h16l-1.5-5h-13Z"/><path d="M5 9v11h14V9M9 20v-6h6v6"/>',
    settings:'<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.8l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-2.9 1.2V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-2.9-1.2l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0-1.2-2.9H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.2-2.9l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 2.9-1.2V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 2.9 1.2l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0 1.2 2.9H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1Z"/>',
    log:'<path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01"/>',
    help:'<circle cx="12" cy="12" r="9"/><path d="M9.5 9a2.5 2.5 0 1 1 3.5 2.3c-.6.3-1 .9-1 1.6V14M12 17.5v.01"/>',
    logout:'<path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><path d="m16 17 5-5-5-5"/><path d="M21 12H9"/>',
    more:'<circle cx="5" cy="12" r="1.2"/><circle cx="12" cy="12" r="1.2"/><circle cx="19" cy="12" r="1.2"/>',
    edit:'<path d="M4 20h4L19 9l-4-4L4 16Z"/><path d="m14 6 4 4"/>', trash:'<path d="M4 7h16M10 11v6M14 11v6M6 7l1 13h10l1-13M9 7V4h6v3"/>',
    eye:'<path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z"/><circle cx="12" cy="12" r="3"/>',
    refresh:'<path d="M20 11a8 8 0 0 0-14-5L4 8M4 4v4h4M4 13a8 8 0 0 0 14 5l2-2M20 20v-4h-4"/>',
    filter:'<path d="M3 5h18l-7 8v6l-4 2v-8Z"/>', sort:'<path d="M7 4v16M3 16l4 4 4-4M17 20V4M13 8l4-4 4 4"/>',
    cash:'<rect x="2.5" y="6" width="19" height="12" rx="2"/><circle cx="12" cy="12" r="2.5"/><path d="M6 9v.01M18 15v.01"/>',
    qr:'<rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><path d="M14 14h3v3M21 14v.01M14 21h3M21 18v3h-3"/>',
    pos:'<rect x="5" y="2.5" width="14" height="19" rx="2.5"/><path d="M8 6h8v4H8zM8 14h.01M12 14h.01M16 14h.01M8 17.5h.01M12 17.5h.01M16 17.5h.01"/>',
    upload:'<path d="M12 16V4M7 9l5-5 5 5M5 20h14"/>', layers:'<path d="m12 3 9 5-9 5-9-5Z"/><path d="m3 13 9 5 9-5"/>',
    wifi:'<path d="M2 9a15 15 0 0 1 20 0M5.5 12.5a10 10 0 0 1 13 0M9 16a5 5 0 0 1 6 0M12 19.5v.01"/>',
    keyboard:'<rect x="2.5" y="6" width="19" height="12" rx="2"/><path d="M6 10h.01M10 10h.01M14 10h.01M18 10h.01M7 14h10"/>',
    sparkle:'<path d="M12 3v4M12 17v4M3 12h4M17 12h4"/>',
    seat:'<path d="M7 4h7a2 2 0 0 1 2 2v7H7Z"/><path d="M5 13h13l1 4H6ZM7 17v3M17 17v3"/>',
    star:'<path d="m12 3 2.7 5.6 6.3.9-4.5 4.4 1 6.1L12 17l-5.6 3 1-6.1L3 9.5l6.3-.9Z"/>',
    map:'<path d="M9 4 3 6v14l6-2 6 2 6-2V4l-6 2Z"/><path d="M9 4v14M15 6v14"/>',
    hourglass:'<path d="M6 3h12M6 21h12M7 3c0 5 10 5 10 9s-10 4-10 9M17 3c0 5-10 5-10 9"/>',
    receipt:'<path d="M5 3h14v18l-3-2-2 2-2-2-2 2-2-2-3 2Z"/><path d="M9 8h6M9 12h6"/>'
  };
  const icon = (n, cls = '', style = '') => `<svg class="ico ${cls}" viewBox="0 0 24 24" aria-hidden="true"${style ? ` style="${style}"` : ''}>${P[n] || ''}</svg>`;
  const paintIcons = (root = document) => root.querySelectorAll('i[data-i]').forEach(el => { el.outerHTML = icon(el.dataset.i, el.className, el.getAttribute('style') || ''); });

  /* ---------- Mapas de asientos (plano heredado del VehicleModel) ----------
     C = conductor, _ = vacío, | = pasillo, E = escalera, - = separación entre filas, número = asiento.
     Estados de TripSeat: libre · sel (elegido por ti) · held (retenido 10 min, pendiente de pago) · taken (ocupado, ya pagado) */
  const MODELS = {
    'yaris':   {name:'Toyota Yaris', cat:'Sedán', cap:4, car:true, foot:'Banca trasera', floors:[[ 'C_1', '-', '234' ]]},
    'avanza':  {name:'Toyota Avanza', cat:'Familiar', cap:6, car:true, foot:'Pasajeros', floors:[[ 'C_1', '234', '5_6' ]]},
    'minivan': {name:'Minivan', cat:'Familiar', cap:7, car:true, foot:'Pasajeros', floors:[[ 'C_1', '234', '567' ]]},
    'van':     {name:'Van', cat:'Transporte', cap:15, floors:[[ ['C','_',1,2], [3,4,5,'_'], [6,7,'_',8], [9,10,'_',11], [12,13,14,15] ]]},
    'bus1':    {name:'Bus 1 piso', cat:'Bus', cap:44, floors:[ Array.from({length:11}, (_, r) => [r*4+1, r*4+2, '|', r*4+3, r*4+4]) ]},
    'bus2':    {name:'Bus 2 pisos', cat:'Bus', cap:52, floors:[
                 [ ['C','_','|','_','_'], ...Array.from({length:4}, (_, r) => [r*3+1, '_', '|', r*3+2, r*3+3]), ['E','_','|','_','_'] ],
                 [ ['E','_','|','_','_'], ...Array.from({length:10}, (_, r) => [13+r*4, 14+r*4, '|', 15+r*4, 16+r*4]) ] ]}
  };
  const rowsOf = floor => floor.map(r => Array.isArray(r) ? r : r === '-' ? '-' : [...r].map(ch => /\d/.test(ch) ? +ch : ch));
  const pad = n => String(n).padStart(2, '0');
  const SEAT_SVG = '<svg class="seat-shape" viewBox="0 0 48 50" aria-hidden="true"><rect class="arm" x="1.5" y="18" width="9" height="25" rx="4.5"/><rect class="arm" x="37.5" y="18" width="9" height="25" rx="4.5"/><rect class="back" x="8" y="1.5" width="32" height="35" rx="9"/><rect class="cush" x="6.5" y="31" width="35" height="15" rx="6"/></svg>';
  const BADGE = {
    sel:  '<path d="m6 12.5 4 4 8-9"/>',
    held: '<circle cx="12" cy="12" r="7"/><path d="M12 8.5V12l2.5 1.6"/>',
    taken:'<path d="m7.5 7.5 9 9M16.5 7.5l-9 9"/>'
  };
  const STATE_TXT = {'':'libre', sel:'elegido por ti', held:'pendiente de pago (retenido)', taken:'ocupado'};
  function seatEl(n, st = '', tag = 'button') {
    const e = document.createElement(tag); e.className = 'seat' + (st ? ' ' + st : '');
    if (tag === 'button') e.type = 'button';
    e.innerHTML = SEAT_SVG + (n !== '' ? `<span>${pad(n)}</span>` : '') + (BADGE[st] ? `<em><svg viewBox="0 0 24 24">${BADGE[st]}</svg></em>` : '');
    return e;
  }

  function seatMap(el) {
    const m = MODELS[el.dataset.model]; if (!m) return;
    const floorIdx = +(el.dataset.floor || 0), mini = el.classList.contains('seat-mini');
    const set = k => new Set((el.dataset[k] || '').split(',').filter(Boolean).map(Number));
    const sel = set('sel'), held = set('held'), taken = set('taken');
    const wrap = document.createElement('div'); wrap.className = 'vwrap';
    const front = document.createElement('div'); front.className = 'vfront'; front.textContent = 'Frente';
    const v = document.createElement('div'); v.className = 'vehicle' + (m.car ? ' car' : '') + (m.floors.length > 1 ? ' bus' : '');
    rowsOf(m.floors[floorIdx]).forEach(r => {
      if (r === '-') { const sp = document.createElement('div'); sp.className = 'vspace'; v.appendChild(sp); return; }
      const row = document.createElement('div'); row.className = 'vrow';
      r.forEach(c => {
        let n;
        if (c === 'C') { n = document.createElement('div'); n.className = 'driver'; n.innerHTML = '<b>C</b><small>Conductor</small>'; n.title = 'Conductor'; }
        else if (c === '|') { n = document.createElement('div'); n.className = 'gap aisle'; }
        else if (c === '_') { n = document.createElement('div'); n.className = 'gap'; }
        else if (c === 'E') { n = document.createElement('div'); n.className = 'stairs'; n.title = 'Escalera'; n.innerHTML = '<small>Escalera</small>'; }
        else {
          const st = sel.has(c) ? 'sel' : held.has(c) ? 'held' : taken.has(c) ? 'taken' : '';
          n = seatEl(c, st); n.dataset.n = c;
          n.setAttribute('aria-label', `Asiento ${pad(c)}, ${STATE_TXT[st]}`);
          n.title = `Asiento ${pad(c)} · ${STATE_TXT[st]}`;
          if (st === 'taken' || st === 'held') n.disabled = true;
          if (!mini) n.addEventListener('click', () => {
            const on = n.classList.toggle('sel');
            n.querySelector('em')?.remove();
            if (on) n.insertAdjacentHTML('beforeend', `<em><svg viewBox="0 0 24 24">${BADGE.sel}</svg></em>`);
            n.setAttribute('aria-label', `Asiento ${pad(c)}, ${STATE_TXT[on ? 'sel' : '']}`);
            el.dispatchEvent(new CustomEvent('seatchange', {bubbles:true}));
          });
        }
        row.appendChild(n);
      });
      v.appendChild(row);
    });
    if (m.foot) { const f = document.createElement('div'); f.className = 'vfoot'; f.textContent = m.foot; v.appendChild(f); }
    wrap.append(front, v);
    el.replaceChildren(wrap);
  }

  /* Leyenda: <div data-legend></div> (pasajero) o <div data-legend="agente"></div> */
  const LEGEND = {
    pasajero: [['', 'Libre', 'Tócalo para elegirlo'], ['sel', 'Tu selección', 'Asientos que estás comprando'], ['held', 'Pendiente de pago', 'Alguien lo está pagando · se libera en 10 min'], ['taken', 'Ocupado', 'Ya pagado, no disponible']],
    agente:   [['', 'Libre', 'Disponible para vender'], ['sel', 'Elegido', 'En esta venta'], ['held', 'Pendiente de pago', 'Retenido en la web · 10 min'], ['taken', 'Vendido', 'Pagado y emitido']]
  };
  function legend(el) {
    const items = LEGEND[el.dataset.legend] || LEGEND.pasajero;
    el.classList.add('seat-legend'); el.setAttribute('role', 'list'); el.setAttribute('aria-label', 'Qué significa cada color');
    el.replaceChildren(...items.map(([st, t, d]) => {
      const li = document.createElement('div'); li.className = 'li' + (st ? ' ' + st : ''); li.setAttribute('role', 'listitem');
      li.append(seatEl('', st, 'span'));
      li.insertAdjacentHTML('beforeend', `<p><b>${t}</b><span>${d}</span></p>`);
      return li;
    }));
  }

  /* ---------- Gráficos ---------- */
  const NS = 'http://www.w3.org/2000/svg';
  const svgEl = (w, h) => { const s = document.createElementNS(NS, 'svg'); s.setAttribute('viewBox', `0 0 ${w} ${h}`); s.setAttribute('class', 'chart'); return s; };
  const add = (p, tag, attrs) => { const e = document.createElementNS(NS, tag); for (const k in attrs) e.setAttribute(k, attrs[k]); p.appendChild(e); return e; };
  const nums = s => s.split(',').map(Number);
  const width = el => { const cs = getComputedStyle(el); return Math.max(300, Math.round(el.clientWidth - parseFloat(cs.paddingLeft) - parseFloat(cs.paddingRight))); };

  function spark(el) {
    const d = nums(el.dataset.spark), w = 120, h = 36, mx = Math.max(...d), mn = Math.min(...d);
    const s = svgEl(w, h), pts = d.map((v, i) => [i * w / (d.length - 1), h - 4 - (v - mn) / (mx - mn || 1) * (h - 8)]);
    const c = el.dataset.color || '#0E6B5C';
    add(s, 'path', {d: `M${pts.map(p => p.join(' ')).join('L')}L${w} ${h}L0 ${h}Z`, fill: c, opacity: .08});
    add(s, 'path', {d: `M${pts.map(p => p.join(' ')).join('L')}`, fill: 'none', stroke: c, 'stroke-width': 2, 'stroke-linejoin': 'round', 'stroke-linecap': 'round'});
    el.replaceChildren(s);
  }

  function line(el) {
    const series = JSON.parse(el.dataset.line), labels = (el.dataset.labels || '').split(','), w = width(el), h = 240, L = 40, B = 24, T = 10;
    const all = series.flatMap(s => s.data), mx = Math.ceil(Math.max(...all) / 100) * 100;
    const s = svgEl(w, h), g = add(s, 'g', {class: 'grid'});
    for (let i = 0; i <= 4; i++) { const y = T + (h - T - B) * i / 4; add(g, 'line', {x1: L, x2: w, y1: y, y2: y}); add(s, 'text', {x: L - 8, y: y + 4, 'text-anchor': 'end'}).textContent = Math.round(mx * (1 - i / 4)).toLocaleString('es-PE'); }
    labels.forEach((t, i) => { if (!t) return; add(s, 'text', {x: L + (w - L) * i / (labels.length - 1), y: h - 6, 'text-anchor': 'middle'}).textContent = t; });
    series.forEach(se => {
      const pts = se.data.map((v, i) => [L + (w - L) * i / (se.data.length - 1), T + (h - T - B) * (1 - v / mx)]);
      if (se.area) add(s, 'path', {d: `M${pts.map(p => p.join(' ')).join('L')}L${w} ${h - B}L${L} ${h - B}Z`, fill: se.color, opacity: .08});
      const p = add(s, 'path', {d: `M${pts.map(p => p.join(' ')).join('L')}`, fill: 'none', stroke: se.color, 'stroke-width': 2.5, 'stroke-linejoin': 'round', 'stroke-linecap': 'round', class: 'draw'});
      if (se.dash) p.setAttribute('stroke-dasharray', '5 5');
    });
    el.replaceChildren(s);
  }

  function bars(el) {
    const items = JSON.parse(el.dataset.bars), w = width(el), rowH = 34, h = items.length * rowH, L = 180, mx = el.dataset.max ? +el.dataset.max : Math.max(...items.map(i => i.v));
    const s = svgEl(w, h), suf = el.dataset.suffix || '';
    items.forEach((it, i) => {
      const y = i * rowH + 8, bw = (w - L - 60) * it.v / mx;
      add(s, 'text', {x: 0, y: y + 13, style: 'font-size:12.5px;fill:#33403D'}).textContent = it.k;
      add(s, 'rect', {x: L, y, width: w - L - 60, height: 18, rx: 6, fill: '#EEF2F0'});
      add(s, 'rect', {x: L, y, width: bw, height: 18, rx: 6, fill: it.c || (suf === '%' && it.v >= 70 ? '#F2A93B' : '#0E6B5C')});
      add(s, 'text', {x: L + (w - L - 60) + 10, y: y + 13, style: 'font-size:12.5px;font-weight:600;fill:#111B19'}).textContent = it.v.toLocaleString('es-PE') + suf;
    });
    el.replaceChildren(s);
  }

  function columns(el) {
    const d = JSON.parse(el.dataset.columns), w = width(el), h = 220, B = 24, T = 10, L = 36;
    const keys = d.series, mx = Math.max(...d.data.map(r => r.v.reduce((a, b) => a + b, 0))) * 1.1;
    const s = svgEl(w, h), g = add(s, 'g', {class: 'grid'}), bw = (w - L) / d.data.length;
    for (let i = 0; i <= 3; i++) add(g, 'line', {x1: L, x2: w, y1: T + (h - T - B) * i / 3, y2: T + (h - T - B) * i / 3});
    d.data.forEach((r, i) => {
      let y = h - B;
      r.v.forEach((v, k) => { const bh = (h - T - B) * v / mx; y -= bh; add(s, 'rect', {x: L + i * bw + bw * .2, y, width: bw * .6, height: bh, fill: keys[k].color, rx: k === r.v.length - 1 ? 4 : 0}); });
      add(s, 'text', {x: L + i * bw + bw / 2, y: h - 6, 'text-anchor': 'middle'}).textContent = r.k;
    });
    el.replaceChildren(s);
  }

  function heat(el) {
    const days = ['Lun','Mar','Mié','Jue','Vie','Sáb','Dom'], hours = ['05','07','09','11','13','15','17','19','21','23'];
    const w = width(el), cw = (w - 40) / hours.length, ch = 26, s = svgEl(w, days.length * ch + 22);
    let seed = 7; const rnd = () => (seed = (seed * 9301 + 49297) % 233280) / 233280;
    days.forEach((d, r) => {
      add(s, 'text', {x: 0, y: r * ch + 17}).textContent = d;
      hours.forEach((hh, c) => {
        const base = (c === 0 || c >= 7 ? .75 : .35) + (r >= 4 ? .2 : 0), v = Math.min(1, base * (.55 + rnd() * .6));
        add(s, 'rect', {x: 40 + c * cw + 2, y: r * ch + 2, width: cw - 4, height: ch - 4, rx: 5, fill: v > .7 ? '#F2A93B' : '#0E6B5C', opacity: v > .7 ? .35 + v * .6 : .1 + v * .8});
      });
    });
    hours.forEach((hh, c) => add(s, 'text', {x: 40 + c * cw + cw / 2, y: days.length * ch + 16, 'text-anchor': 'middle'}).textContent = hh + ':00');
    el.replaceChildren(s);
  }

  function donut(el) {
    const v = +el.dataset.donut, s = svgEl(64, 64), r = 26, c = 2 * Math.PI * r;
    add(s, 'circle', {cx: 32, cy: 32, r, fill: 'none', stroke: '#EEF2F0', 'stroke-width': 7});
    add(s, 'circle', {cx: 32, cy: 32, r, fill: 'none', stroke: v >= 70 ? '#F2A93B' : '#0E6B5C', 'stroke-width': 7, 'stroke-linecap': 'round', 'stroke-dasharray': `${c * v / 100} ${c}`, transform: 'rotate(-90 32 32)'});
    add(s, 'text', {x: 32, y: 36, 'text-anchor': 'middle', style: 'font:700 13px Inter;fill:#111B19'}).textContent = v + '%';
    el.replaceChildren(s);
  }

  /* ---------- QR de muestra (patrón determinista; el real lo genera el backend) ---------- */
  function qr(el) {
    const N = 29, c = document.createElement('canvas'); c.width = c.height = N; c.className = 'qr';
    const x = c.getContext('2d'); x.fillStyle = '#fff'; x.fillRect(0, 0, N, N); x.fillStyle = '#111B19';
    let seed = [...(el.dataset.qr || 'RA')].reduce((a, ch) => a + ch.charCodeAt(0), 0);
    const rnd = () => (seed = (seed * 9301 + 49297) % 233280) / 233280;
    const finder = (ox, oy) => { for (let i = 0; i < 7; i++) for (let j = 0; j < 7; j++) if (i === 0 || i === 6 || j === 0 || j === 6 || (i > 1 && i < 5 && j > 1 && j < 5)) x.fillRect(ox + i, oy + j, 1, 1); };
    for (let i = 1; i < N - 1; i++) for (let j = 1; j < N - 1; j++) {
      const inF = (i < 9 && j < 9) || (i > N - 10 && j < 9) || (i < 9 && j > N - 10);
      if (!inF && rnd() > .52) x.fillRect(i, j, 1, 1);
    }
    finder(1, 1); finder(N - 8, 1); finder(1, N - 8);
    if (el.dataset.size) c.style.width = c.style.height = el.dataset.size + 'px';
    el.replaceChildren(c);
  }

  /* ---------- Contadores (count-up) ---------- */
  function countUp(el) {
    const to = +el.dataset.count, dec = +(el.dataset.dec || 0), pre = el.dataset.pre || '', suf = el.dataset.suf || '';
    const fmt = v => pre + v.toLocaleString('es-PE', {minimumFractionDigits: dec, maximumFractionDigits: dec}) + suf;
    if (matchMedia('(prefers-reduced-motion: reduce)').matches) { el.textContent = fmt(to); return; }
    const t0 = performance.now(); const tick = t => { const k = Math.min(1, (t - t0) / 800); el.textContent = fmt(to * (1 - Math.pow(1 - k, 3))); if (k < 1) requestAnimationFrame(tick); }; requestAnimationFrame(tick);
  }

  /* ---------- Contador de retención HELD ---------- */
  function hold(el) {
    let s = +el.dataset.hold; const out = el.querySelector('b') || el;
    const draw = () => { out.textContent = `${String(Math.floor(s / 60)).padStart(2, '0')}:${String(s % 60).padStart(2, '0')}`; el.classList.toggle('late', s <= 60); };
    draw(); setInterval(() => { if (s > 0) { s--; draw(); } }, 1000);
  }

  window.RA = {icon, paintIcons, MODELS, seatMap, legend};
  const run = () => {
    paintIcons();
    document.querySelectorAll('[data-model]').forEach(seatMap);
    document.querySelectorAll('[data-legend]').forEach(legend);
    document.querySelectorAll('[data-spark]').forEach(spark);
    document.querySelectorAll('[data-line]').forEach(line);
    document.querySelectorAll('[data-bars]').forEach(bars);
    document.querySelectorAll('[data-columns]').forEach(columns);
    document.querySelectorAll('[data-heat]').forEach(heat);
    document.querySelectorAll('[data-donut]').forEach(donut);
    document.querySelectorAll('[data-qr]').forEach(qr);
    document.querySelectorAll('[data-count]').forEach(countUp);
    document.querySelectorAll('[data-hold]').forEach(hold);
    document.querySelectorAll('.segmented').forEach(sg => sg.addEventListener('click', e => { const b = e.target.closest('button'); if (!b) return; sg.querySelectorAll('button').forEach(x => x.setAttribute('aria-pressed', x === b)); }));
    document.querySelectorAll('.chips').forEach(cg => cg.addEventListener('click', e => { const b = e.target.closest('.chip'); if (b) b.classList.toggle('on'); }));
  };
  document.readyState === 'loading' ? document.addEventListener('DOMContentLoaded', run) : run();
})();

/* Menú de usuario (pasajero): botón con aria-expanded, cierra con Esc o clic fuera.
   La pestaña "Cuenta" de la barra inferior (móvil) abre el mismo menú. #menu-usuario lo deja abierto (captura). */
(() => {
  document.querySelectorAll('[data-menu]').forEach(btn => {
    const m = btn.nextElementSibling;
    const set = (open, kb) => {
      btn.setAttribute('aria-expanded', open); m.hidden = !open;
      if (open && kb) m.querySelector('[role=menuitem]')?.focus({preventScroll:true});
    };
    btn.addEventListener('click', e => { e.stopPropagation(); set(m.hidden, e.detail === 0); });
    document.addEventListener('click', e => { if (!m.hidden && !m.contains(e.target)) set(false); });
    document.addEventListener('keydown', e => { if (e.key === 'Escape' && !m.hidden) { set(false); btn.focus(); } });
    document.querySelectorAll('.tabbar a[href="#cuenta"]').forEach(a => a.addEventListener('click', e => { e.preventDefault(); e.stopPropagation(); scrollTo({top:0}); set(true); }));
    if (location.hash === '#menu-usuario') set(true);
  });
})();

