# Prompts para Google Stitch — RutaAndina

> ✅ **Actualización 22 nov 2026:** las 30 pantallas ya están hechas a mano en `design/stitch/` (ver `README.md`). Este archivo queda solo como referencia de contenido; no hace falta generar nada en Stitch.

Cómo usar este archivo:

1. Crea **un solo proyecto** en Stitch llamado `RutaAndina` (así todas las pantallas comparten estilo).
2. Pega primero el **PROMPT BASE** (sección 0) y genera la pantalla de login. Si el estilo te gusta, ese es el ancla visual.
3. Para cada pantalla siguiente, pega su prompt **dentro del mismo proyecto**. Si una sale fuera de estilo, pídele a Stitch: *"Match the exact visual style of the login screen"*.
4. Pasajero: genera en **Mobile** y luego en **Web**. Staff (agente, admin, superadmin): genera en **Web**.
5. Exporta cada pantalla (imagen + código) a su carpeta, como se indica en la sección 5.
6. Ajusta con prompts cortos de iteración (ej.: *"Make the seat map bigger and center it"*), no regeneres desde cero.

Los prompts están en inglés porque Stitch responde mejor, pero **todo el texto visible de la UI sale en español**.

---

## 0. PROMPT BASE (pegar antes de cualquier pantalla)

```text
Design system for "RutaAndina", a premium interprovincial bus and van ticketing platform in Ayacucho, Peru. All UI text in Spanish (Peru). Style: clean, modern, high-end SaaS, generous white space, soft shadows, rounded corners (12px inputs/buttons, 16px cards), subtle Andean topographic contour lines as a decorative background pattern at very low opacity.
Colors: primary deep teal #0E6B5C (hover #0B5C4E, dark #063F36, light tint #DDF2EC), accent warm Andean sun amber #F2A93B (tint #FDF1DC), neutrals slate #0F172A / #475569 / #E2E8F0 / page background #F8FAFC, success #16A34A, danger #DC2626.
Typography: Plus Jakarta Sans for headings (bold, tight tracking), Inter for body text, tabular numbers for prices and times.
Icons: Lucide line icons. Logo: a curved road climbing between two mountains ending in a location dot, next to the wordmark "RutaAndina".
Currency format "S/ 45.00", 24h times, Spanish dates like "sáb 21 nov 2026".
Accessibility: WCAG AA contrast, 48px touch targets.
```

---

## 1. Acceso (pantallas de alto impacto)

### 1.1 `pasajero/01-login` — Inicio de sesión del pasajero
> ✅ **Hecho a mano, no generar en Stitch.** Referencia final: `design/stitch/pasajero/01-login/` (`code.html`, `screen.png`, `screen-mobile.png`). Especificación en `DESIGN.md` §9.

### 1.2 `pasajero/02-registro` — Crear cuenta
```text
Passenger sign-up screen with the SAME visual style as the RutaAndina login: full-screen photo of an Andean mountain road at dusk, dark teal gradient overlay (#063F36 at 55% top, 70% bottom), centered glass card (max-width 480px, 28px radius, backdrop blur 24px, translucent dark teal, 1px white 18% border), white text, logo + wordmark "RUTAANDINA" on top, inputs with translucent white fill and amber (#F2A93B) focus ring, gradient teal primary button. Form: "Crea tu cuenta", fields Nombres, Apellidos, Tipo de documento (DNI / CE / Pasaporte) + Número, Celular (+51) with WhatsApp icon, Correo, Contraseña with strength meter, checkbox "Acepto los términos y la política de privacidad", checkbox (unchecked by default) "Quiero recibir alertas y recordatorios por WhatsApp", primary button "Crear cuenta", link "¿Ya tienes cuenta? Inicia sesión".
```

### 1.3 `personal/01-login` — Portal del personal (agente, administrador, superadministrador)
> ✅ **Hecho a mano, no generar en Stitch.** Referencia final: `design/stitch/personal/01-login/`. Variante del login de pasajero (ver `DESIGN.md` §9).

---

## 2. Pasajero (generar Mobile y Web)

### 2.1 `pasajero/03-inicio-busqueda`
```text
Passenger home after login. Top navbar: logo, links "Buscar viajes", "Mis viajes", avatar menu. Hero with teal gradient and mountain silhouette, greeting "Hola, Ana 👋 ¿A dónde viajamos?", and a large floating search card: Origen (default Ayacucho), swap button, Destino, Fecha (date picker), Pasajeros (1–6 stepper), primary button "Buscar viajes". Below: amber banner with flame icon "Alta demanda: Semana Santa 2026 — reserva con anticipación". Section "Rutas populares" with 4 image cards (Lima, Huancayo, Andahuaylas, Cusco) showing "desde S/ 45". Section "Tu próximo viaje" with a compact ticket card (Ayacucho → Lima, sáb 21 nov 22:30, asiento 12, button "Ver pasaje").
```

### 2.2 `pasajero/04-resultados`
```text
Search results: "Ayacucho → Lima · sáb 21 nov · 1 pasajero" with edit button. Left sidebar filters (desktop) / bottom sheet (mobile): Empresa (checkboxes), Horario (Madrugada, Mañana, Tarde, Noche chips), Tipo de vehículo (Bus, Van, Minivan, Auto), Precio range slider. Sort dropdown "Más temprano / Menor precio / Más rápido". Result cards: company logo and name, agency "Terminal Norte", departure 22:30 and arrival 07:15 joined by a dotted route line with duration "8 h 45 min", vehicle chip "Bus 2 pisos · 52 asientos", availability "14 libres" (amber "Últimos 3" when low), optional badge "Alta demanda", price "S/ 85.00" big, primary button "Elegir asiento". Include one skeleton loading card at the bottom.
```

### 2.3 `pasajero/05-seleccion-asiento` (pantalla clave)
```text
Seat selection screen. Header with trip summary (Empresa, Ayacucho 22:30 → Lima 07:15, Bus 2 pisos) and an amber countdown pill hidden for now. Center: a top-down vehicle silhouette with rounded front labeled "FRENTE", driver seat block "C · Conductor", and numbered seats in a realistic layout; tabs "Piso 1 / Piso 2" for double-decker buses. Seat states with legend: Disponible (white with teal border), Seleccionado (solid teal #0E6B5C with white check), Retenido (amber tint with dashed border and clock icon), Ocupado (light gray with ✕). Show seats 12 and 13 selected.
Right panel (desktop) / sticky bottom bar (mobile): "Tu selección": Asiento 12 – S/ 85.00, Asiento 13 – S/ 85.00, Total S/ 170.00, note "Tus asientos se reservarán por 10 minutos al continuar", primary button "Continuar".
Also show small variants row below titled "Otros vehículos" with compact seat maps for: Toyota Yaris (4 pasajeros), Toyota Avanza (6), Minivan (7), Van (15), to illustrate that the map adapts to the vehicle model.
```

### 2.4 `pasajero/06-datos-pasajeros`
```text
Passenger details step. Stepper on top: 1 Asientos ✓, 2 Pasajeros (active), 3 Pago, 4 Pasaje. Amber countdown pill "Reserva retenida 09:42". One card per seat: "Asiento 12" with fields Tipo de documento, Número de documento, Nombres, Apellidos, Edad; first card has toggle "Soy yo" that autofills. Contact card: Celular (+51) and Correo for sending the ticket, checkbox "Enviarme el pasaje y recordatorios por WhatsApp". Right summary panel with route, seats and total S/ 170.00. Buttons "Volver" and primary "Ir a pagar".
```

### 2.5 `pasajero/07-pago`
```text
Payment step (Culqi). Stepper at step 3. Countdown pill "08:15" (turns red in last minute). Left: payment method tabs "Tarjeta", "Yape", "PagoEfectivo"; card form inside a secure-looking box with lock icon and "Pago procesado de forma segura por Culqi": Número de tarjeta, Vencimiento, CVV, Nombre del titular, Correo. Right: order summary card styled as a ticket stub: route, date, seats 12 and 13, passengers, subtotal, total "S/ 170.00", primary button "Pagar S/ 170.00". Small note "No cierres esta ventana mientras confirmamos tu pago".
Also show a modal state variant: "Confirmando tu pago…" with spinner and text "Esto puede tardar unos segundos".
```

### 2.6 `pasajero/08-pasaje-emitido`
```text
Success screen. Large animated-looking green check drawn in a circle, title "¡Listo, Ana! Tu viaje está confirmado", subtitle "Enviamos tu pasaje a tu correo y WhatsApp". Below, a premium boarding-pass style ticket card with side notches: company logo, "AYC → LIM", big times 22:30 / 07:15, date, passenger name, document, seat "12", floor, agency "Terminal Norte", ticket code "RA-7K2P9", large QR code. Two tickets shown stacked for seats 12 and 13. Buttons "Descargar PDF", "Agregar recordatorio", secondary "Ir a mis viajes".
```

### 2.7 `pasajero/09-mis-viajes`
```text
"Mis viajes" page with tabs "Próximos", "Pasados", "Cancelados". Cards: route, date/time, company, seats, status badge (Confirmado green, Pendiente de pago amber, Cancelado red, Expirado gray, Reprogramado blue), actions "Ver pasaje", "Reprogramar", "Cancelar". Empty state illustration for "Cancelados": "No tienes viajes cancelados".
```

### 2.8 `pasajero/10-detalle-reserva`
```text
Reservation detail: header with code RA-7K2P9 and status "Confirmado". Timeline of states (Reservado 22:01 → Pago aprobado 22:04 → Pasaje emitido 22:04). Sections: Viaje, Pasajeros y asientos, Pago (Culqi, tarjeta •••• 4242, S/ 170.00). Buttons "Reprogramar" and "Cancelar reserva". Show the cancel confirmation modal: "¿Cancelar tu reserva?", text "Las condiciones de cancelación dependen de la política de la empresa", buttons "Mantener reserva" and danger "Sí, cancelar".
```

---

## 3. Agente / Vendedor (Web, ventanilla)

Shell común para staff: sidebar `#063F36` a la izquierda, barra superior con selector de agencia, buscador y avatar.

### 3.1 `agente/01-panel-ventanilla`
```text
Box-office dashboard for a sales agent ("Ventanilla — Agencia Terminal Norte"). Dark teal sidebar with items: Ventanilla, Ventas del día, Pasajeros, Ayuda. Top bar: agency name chip (fixed, not switchable for agents), clock, avatar "Luis · Agente". Main: quick search bar for trips (Destino, Fecha, Hora) optimized for keyboard with shortcut hints (F2 Nueva venta, F4 Buscar pasajero). Today's departures list as a timeline table: hora, destino, vehículo, ocupación progress bar (green → amber at 70% with "Alta demanda" badge), asientos libres, button "Vender". KPIs on top: Ventas hoy S/ 3,480, Pasajes emitidos 41, Próxima salida 14:30.
```

### 3.2 `agente/02-venta-asientos`
```text
Counter sale screen. Left: same seat map component as the passenger app (vehicle silhouette, states Disponible / Seleccionado / Retenido / Ocupado), live indicator "Disponibilidad en tiempo real". Right: sale form: search passenger by DNI with button "Buscar" (autofills if exists) or create new; per seat: DNI, Nombres, Apellidos, Celular. Summary with total. Primary button "Cobrar" with shortcut hint F9.
```

### 3.3 `agente/03-cobro-emision`
```text
Payment and ticket issuing modal over the counter sale screen. Payment method segmented buttons with icons: Efectivo, Yape/Plin, Tarjeta POS. For Efectivo: "Monto recibido" input and computed "Vuelto S/ 30.00". For Yape/Plin and POS: "Código de operación" input. Primary button "Confirmar venta y emitir pasaje". Success state: printed-ticket preview with QR, buttons "Imprimir" and "Enviar por WhatsApp", "Nueva venta".
```

### 3.4 `agente/04-operaciones-del-dia`
```text
"Mis operaciones de hoy" table for the agent: hora, código, pasajero, viaje, asientos, método de pago, monto, estado badge, acciones (Ver, Reimprimir, Cancelar/Reprogramar only if permitted, otherwise disabled with tooltip "Requiere autorización"). Filters by estado and método de pago. Totals footer by payment method.
```

---

## 4. Administrador de Agencia (Web)

### 4.1 `admin-agencia/01-selector-agencia`
```text
Agency selector shown right after an agency admin logs in. Centered title "¿En qué agencia trabajarás hoy?", company name "Transportes Ayacucho S.A.". Grid of 3 large selectable cards: "Terminal Norte", "Terminal Sur", "Huanta" each with address, today's departures count and occupancy mini-chart; selected card has teal border and check. Primary button "Continuar". Note "Podrás cambiar de agencia desde la barra superior".
```

### 4.2 `admin-agencia/02-dashboard`
```text
Agency admin dashboard. Sidebar: Dashboard, Viajes y horarios, Rutas, Vehículos, Tarifas, Agentes, Reportes, Alertas. Top bar with agency switcher dropdown "Terminal Norte ▾" listing only the admin's authorized agencies. KPI cards with sparklines: Ventas del mes, Ocupación promedio, Pasajes emitidos, Cancelaciones. Line chart "Ventas últimos 30 días" (web vs ventanilla), bar chart "Ocupación por ruta", list "Alertas" (amber: "Salida 22:30 a Lima supera 70 % de ocupación").
```

### 4.3 `admin-agencia/03-rutas`
```text
Routes management: table of routes (Origen, Destino, Paradas, Distancia, Duración estimada, Estado toggle). Button "Nueva ruta" opens a side drawer form: Origen, Destino, paradas intermedias (sortable list), duración estimada, estado. Include a small static map-like illustration of the route.
```

### 4.4 `admin-agencia/04-vehiculos`
```text
Vehicles management. Table: Placa, Modelo, Categoría, Capacidad, Estado. Button "Registrar vehículo" opens a drawer: Placa input, "Modelo de vehículo" selectable cards with name and seat layout thumbnail: Toyota Yaris (4), Toyota Avanza (6), Minivan (7), Van (15), Bus 1 piso (44), Bus 2 pisos (52); the selected model shows a larger live seat-map preview with driver position. Helper text "El plano de asientos se hereda del modelo".
```

### 4.5 `admin-agencia/05-viajes-horarios`
```text
Trips and schedules: weekly calendar view with trip blocks colored by occupancy, toggle to list view. Button "Programar viaje" opens a stepper modal: 1 Ruta, 2 Fecha y hora (with option "Repetir: diario / días de la semana"), 3 Vehículo (dropdown by plate + model with seat-map thumbnail), 4 Tarifa, 5 Revisión. On review show "Se generarán 15 asientos para este viaje".
```

### 4.6 `admin-agencia/06-tarifas`
```text
Fares page: table by route with price per trip, valid from/to dates, and special-date overrides (e.g., "Semana Santa +30 %") shown as amber chips. Drawer to edit fare with price input "S/" and date range.
```

### 4.7 `admin-agencia/07-agentes`
```text
Agents management: cards or table with avatar, name, email, assigned agency, status, last login, permission toggles "Puede cancelar" and "Puede reprogramar". Button "Nuevo agente" with drawer form.
```

### 4.8 `admin-agencia/08-reportes`
```text
Reports: filters (rango de fechas, ruta, canal web/ventanilla), KPI row, charts: ventas por día, ocupación por ruta, demanda por horario (heatmap days x hours), top rutas. Export buttons "Excel" and "PDF".
```

---

## 5. Superadministrador (Web)

### 5.1 `superadmin/01-dashboard-global`
```text
Super admin global dashboard with dark teal sidebar: Dashboard, Empresas, Agencias, Usuarios administrativos, Modelos de vehículo, Festividades y alta demanda, Auditoría, Configuración. KPIs: Empresas activas, Agencias activas, Ventas totales del mes, Pasajeros registrados. Charts: ventas por empresa (stacked bars), mapa/lista de rutas más demandadas, panel "Estado de integraciones" with Culqi and WhatsApp health indicators (Operativo / Degradado).
```

### 5.2 `superadmin/02-empresas`
```text
Companies list: logo, razón social, RUC, número de agencias, estado (Activa/Inactiva toggle with confirmation). Button "Registrar empresa" opens a drawer: Razón social, RUC, logo upload, contacto, estado.
```

### 5.3 `superadmin/03-agencias`
```text
Agencies list grouped by company: nombre, dirección, ciudad, administradores asignados (avatars), estado toggle. Drawer "Nueva agencia": empresa (select), nombre, dirección, ciudad, teléfono.
```

### 5.4 `superadmin/04-usuarios-administrativos`
```text
Administrative users: table with name, role badge (Administrador de agencia / Agente), company, authorized agencies as chips, status. Drawer to assign an agency admin: user, company (single), authorized agencies (multi-select limited to that company), with warning text "Solo puede asignar agencias de la misma empresa".
```

### 5.5 `superadmin/05-modelos-vehiculo`
```text
Vehicle models catalog: cards for Toyota Yaris (4), Toyota Avanza (6), Minivan (7), Van (15), Bus 1 piso, Bus 2 pisos, each with a seat layout thumbnail. "Nuevo modelo" opens a seat layout editor: grid canvas where cells can be set as Asiento, Pasillo, Conductor or Vacío, auto-numbered seats, floor tabs, live capacity counter, name and category fields.
```

### 5.6 `superadmin/06-festividades-alta-demanda`
```text
High-demand events: calendar with highlighted festivities (Semana Santa, Fiestas Patrias, Carnaval Ayacuchano), list with name, date range, affected routes, threshold "70 %". Drawer to create event. Preview card of the WhatsApp alert message: "⚠ Se prevé alta demanda Ayacucho → Lima del 13 al 20 de abril. Reserva con anticipación." with note "Solo se envía a pasajeros que aceptaron alertas".
```

### 5.7 `superadmin/07-auditoria`
```text
Audit log: searchable, filterable table (fecha/hora, usuario, rol, empresa, agencia, operación, recurso, resultado badge). Row click opens a side panel with before/after JSON diff. Filters by date range, user, operation and result. Export CSV.
```

---

## 6. Exportar a tu repositorio

Por cada pantalla: en Stitch usa **Export → Code** (HTML) y descarga la **imagen**. Guárdalas así:

```text
design/
  stitch/
    DESIGN.md
    README.md                      (este índice de pantallas)
    pasajero/
      01-login/            screen.png  code.html
      02-registro/
      03-inicio-busqueda/
      04-resultados/
      05-seleccion-asiento/
      06-datos-pasajeros/
      07-pago/
      08-pasaje-emitido/
      09-mis-viajes/
      10-detalle-reserva/
    personal/
      01-login/
    agente/
      01-panel-ventanilla/
      02-venta-asientos/
      03-cobro-emision/
      04-operaciones-del-dia/
    admin-agencia/
      01-selector-agencia/  …  08-reportes/
    superadmin/
      01-dashboard-global/  …  07-auditoria/
```

Si generas versión Mobile y Web de una pantalla de pasajero, guarda `screen-mobile.png` y `screen-web.png` en la misma carpeta.

Luego:
```powershell
git add design/
git commit -m "design: pantallas Stitch y sistema de diseño"
```
