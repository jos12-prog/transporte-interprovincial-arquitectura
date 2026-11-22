# DESIGN.md — Sistema de diseño

> Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial (Ayacucho).
> Este archivo es la referencia visual y de interacción obligatoria para todo el frontend (ver `AGENTS.md`).
> Las pantallas de referencia están en `design/stitch/<rol>/<NN-pantalla>/` (`code.html` + capturas). Índice en `design/stitch/README.md`. Estilos y componentes compartidos en `design/stitch/_assets/app.css` y `app.js`.

---

## 1. Marca

| Elemento | Valor |
|---|---|
| Nombre comercial (provisional) | **RutaAndina** — "Viaja seguro por el Perú" |
| Personalidad | Confiable, moderna, cálida, andina sin folclor recargado |
| Logotipo | Isotipo: silueta de la región Ayacucho (degradado `#5CC2AC → #1B8A75 → --primary-600`, contorno blanco) atravesada por una carretera oscura con línea central ámbar que termina en un pin `--accent-500` sobre Huamanga. Logotipo "Ruta" + "Andina" (Andina en ámbar sobre fondo oscuro, en `--primary-600` sobre fondo claro), Plus Jakarta Sans 800. Archivos: `design/stitch/_assets/brand/logo-mark.svg`, `logo-horizontal.svg`, `logo-horizontal-dark.svg`. Tamaño mínimo del isotipo: 32px |
| Motivo gráfico | Líneas de ruta punteadas y curvas de nivel (topografía) en baja opacidad |

El nombre es editable: reemplazar "RutaAndina" en todas las pantallas si el equipo elige otro.

---

## 2. Color (tokens)

| Token | Claro | Uso |
|---|---|---|
| `--primary-900` | `#063F36` | Fondos de héroe, sidebar |
| `--primary-700` | `#0B5C4E` | Botón primario hover, títulos de marca |
| `--primary-600` | `#0E6B5C` | **Color primario** (botones, links, asiento seleccionado) |
| `--primary-100` | `#DDF2EC` | Fondos suaves, chips |
| `--accent-500` | `#F2A93B` | Acento "sol andino": badges de alta demanda, highlights, contador |
| `--accent-100` | `#FDF1DC` | Fondo de alertas de demanda |
| `--neutral-900` | `#0F172A` | Texto principal |
| `--neutral-600` | `#475569` | Texto secundario |
| `--neutral-200` | `#E2E8F0` | Bordes |
| `--neutral-50` | `#F8FAFC` | Fondo de página |
| `--success-600` | `#16A34A` | Pago aprobado, pasaje emitido |
| `--danger-600` | `#DC2626` | Errores, cancelación |
| `--warning-500` | `#F59E0B` | Reserva por expirar |

Estados del mapa de asientos (iguales en web pasajero y ventanilla). Cada asiento es una butaca dibujada (respaldo, cojín y apoyabrazos) con número de 2 cifras (`01`, `02`…) y una insignia en la esquina para que el estado se entienda sin depender solo del color:

| Estado | Texto en la leyenda (pasajero / agente) | Estilo |
|---|---|---|
| Libre | "Libre · Tócalo para elegirlo" / "Libre · Disponible para vender" | Butaca blanca, borde `--primary-600`, número `--primary-700`, sin insignia |
| Seleccionado | "Tu selección" / "Elegido" | Butaca `--primary-700` (teal oscuro), número blanco, insignia blanca con ✓ |
| Pendiente de pago (HELD, 10 min) | "Pendiente de pago · Alguien lo está pagando · se libera en 10 min" / "Retenido en la web · 10 min" | Butaca `--accent-100`, borde punteado `--accent-500`, insignia ámbar con reloj |
| Ocupado (CONFIRMED) | "Ocupado · Ya pagado" / "Vendido · Pagado y emitido" | Butaca `--n-200`, número `--n-400` tachado, insignia gris con ✕ |
| Conductor | — | Cuadro gris con "C" y la etiqueta "Conductor" debajo |

El plano va dentro de una carrocería gris claro redondeada, con la etiqueta **FRENTE** arriba y, en autos, una etiqueta al pie (**BANCA TRASERA** en el Yaris, **PASAJEROS** en Avanza y Minivan). Distribución por modelo (columnas de izquierda a derecha):

| Modelo | Tarjeta | Filas |
|---|---|---|
| Toyota Yaris | `04` · Sedán · 4 pasajeros | C · — · 01 / (espacio) / 02 · 03 · 04 |
| Toyota Avanza | `06` · Familiar · 6 pasajeros | C · — · 01 / 02 · 03 · 04 / 05 · — · 06 |
| Minivan | `07` · Familiar · 7 pasajeros | C · — · 01 / 02 · 03 · 04 / 05 · 06 · 07 |
| Van | `15` · Transporte · 15 pasajeros | C · — · 01 · 02 / 03 · 04 · 05 · — / 06 · 07 · — · 08 / 09 · 10 · — · 11 / 12 · 13 · 14 · 15 |

La leyenda (`data-legend`) es una fila de 4 tarjetas, cada una con la butaca de ejemplo, el nombre del estado y una frase que explica qué significa.

Pasos de compra (`.stepper`): tarjeta con "Paso N de 4 · Nombre" arriba y, a la derecha, el contador de reserva cuando aplica ("Tus asientos están reservados 09:42"). Cuatro círculos de 38px unidos por una línea de 4px que se pinta de teal al avanzar. Hecho = teal con ✓ y un resumen ("Asientos 31 y 32"); actual = ámbar con halo; pendiente = gris con una ayuda corta ("Paga seguro con Culqi"). En `08-pasaje-emitido` todos aparecen completos. En móvil se ocultan las ayudas.

Datos de pasajeros (`06`): antes de los formularios, dos opciones tipo tarjeta-radio. **Un pasajero por asiento** (por defecto) muestra un formulario por asiento. **Un solo nombre para todos** muestra un único formulario "Titular de los asientos" con los asientos como etiquetas, la confirmación "Este nombre se usará en los N asientos" y un aviso: al abordar pueden pedir el DNI de cada viajero, así que si viajan personas distintas se recomienda la primera opción. Vale para cualquier cantidad de asientos.

Contraste mínimo WCAG AA (4.5:1 en texto).

---

## 3. Tipografía

| Rol | Fuente | Tamaño / peso |
|---|---|---|
| Display (héroe) | Plus Jakarta Sans | 48–56px / 800 |
| H1 | Plus Jakarta Sans | 32px / 700 |
| H2 | Plus Jakarta Sans | 24px / 700 |
| H3 | Plus Jakarta Sans | 18px / 600 |
| Cuerpo | Inter | 16px / 400, interlineado 1.5 |
| Pequeño / etiquetas | Inter | 13–14px / 500 |
| Números (precio, asiento, código) | Inter tabular-nums | 600 |

---

## 4. Espaciado, forma y elevación

- Grid de 8px. Contenedor máximo 1200px. Gutter móvil 16px.
- Radios: botones e inputs 12px, tarjetas 16px, modales 24px, chips 999px.
- Sombras: `sm` 0 1px 2px rgba(15,23,42,.06); `md` 0 8px 24px rgba(15,23,42,.08); `lg` 0 24px 48px rgba(6,63,54,.18).
- Íconos: Lucide, trazo 1.75px, 20px.

---

## 5. Componentes base

| Componente | Especificación |
|---|---|
| Botón primario | Fondo `--primary-600`, texto blanco, alto 48px, flecha → que se desplaza 4px al hover |
| Botón secundario | Borde `--primary-600`, fondo transparente |
| Input | Alto 48px, ícono a la izquierda, borde `--neutral-200`, foco con anillo `--primary-100` 4px |
| Tarjeta de viaje | Empresa + logo, hora salida → llegada con línea de ruta, duración, tipo de vehículo, asientos libres, precio grande, botón "Elegir asiento" |
| Mapa de asientos | Silueta del vehículo según `VehicleModel` (Yaris 4, Avanza 6, Minivan 7, Van 15, Bus 1 y 2 pisos), frente arriba, leyenda de estados debajo |
| Contador de retención | Píldora `--accent-500` con reloj y mm:ss; cambia a `--danger-600` en el último minuto |
| Badge alta demanda | Píldora `--accent-100` con llama/ícono y texto "Alta demanda" |
| Pasaje | Tarjeta tipo boleto con muesca lateral, QR grande, código, pasajero, asiento, fecha |
| Sidebar (staff) | Fondo `--primary-900`, ítems blancos 70% opacidad, activo con barra `--accent-500` a la izquierda |
| Selector de agencia | Dropdown en la barra superior con nombre de empresa y agencia activa |
| Tabla | Cabecera fija, filas 56px, zebra suave, acciones en menú "⋯" |
| Toast | Esquina superior derecha, ícono de estado, se cierra solo en 4s |
| Estado vacío | Ilustración lineal + texto + acción |

---

## 6. Movimiento y animaciones (implementar con Framer Motion)

Stitch entrega pantallas estáticas; estas animaciones las implementa el frontend.

| Momento | Animación | Duración / curva |
|---|---|---|
| Login (fondo) | Carrusel de fotos con fundido y zoom lento 1.06→1; indicador de progreso; parallax de 14px con el puntero. Sin fotos: degradado verde de marca | 8s por foto, 1.4s fundido |
| Login (tarjeta) | Entrada fade + slide-up 16px; contenido en cascada (stagger 50ms); el pin del logo cae 8px al cargar | 600ms `cubic-bezier(.2,.8,.2,1)` |
| Botón "Ingresar" | Hover: elevación + flecha avanza; al enviar: spinner → check | 200ms |
| Error de login | Sacudida horizontal de la tarjeta + mensaje bajo el campo | 400ms |
| Cambio de página | Fade + slide 12px | 250ms |
| Resultados de búsqueda | Tarjetas aparecen en cascada (stagger 50ms); skeleton mientras carga | 300ms |
| Selección de asiento | Asiento hace "pop" (scale 1 → 1.12 → 1) y aparece el check | 220ms, spring |
| Asiento tomado por otro | Transición de color a "retenido" + tooltip "Alguien lo está reservando" | 300ms |
| Contador HELD | Pulso suave en el último minuto | 1s, infinito |
| Pago aprobado | Check animado que se dibuja (stroke) + confeti discreto en colores de marca | 900ms |
| Pasaje emitido | El boleto "se imprime" (slide-down con clip) | 600ms |
| KPIs del dashboard | Números cuentan desde 0 (count-up) y gráficos se dibujan | 800ms |
| Sidebar | Colapsa/expande con ancho animado | 200ms |

Reglas: respetar `prefers-reduced-motion` (desactivar parallax, bucles y confeti); ninguna animación bloquea la interacción; máximo 1 animación en bucle por pantalla.

---

## 7. Responsive

- Pasajero: **mobile-first** (360px → 1440px). Mapa de asientos centrado y con zoom táctil en móvil; resumen de compra como barra fija inferior.
- Ventanilla y administración: **desktop-first** (1280px), utilizables en tablet (768px).

---

## 8. Voz y textos

- Español (Perú), tuteo para pasajeros ("Elige tu asiento"), usted en portal de personal ("Ingrese sus credenciales").
- Moneda `S/ 45.00`; fechas `sáb 21 nov 2026`; hora 24h `22:30`.
- Errores claros y accionables: "Ese asiento acaba de ser reservado. Elige otro."

---

## 9. Inicio de sesión (pantallas finales, hechas a mano)

Referencias: `design/stitch/pasajero/01-login/` y `design/stitch/personal/01-login/` (`code.html`, `screen.png` 1440×900, `screen-mobile.png` 390×844). No se generan en Stitch. Recursos compartidos en `design/stitch/_assets/` (`login/fondo*.jpg`, `brand/logo-*.svg`).

**Criterio visual:** sobrio y fotográfico, como un producto real. Sin bordes luminosos animados, halos, brillos que cruzan el botón ni ilustraciones genéricas. El impacto lo dan las fotos reales de Ayacucho y la tipografía.

**Composición:** foto a pantalla completa + degradado oscuro + tarjeta **centrada** con contenido alineado a la izquierda. Es la única pantalla con superficie oscura; el resto de la app usa fondo claro.

| Elemento | Especificación |
|---|---|
| Fondo | Carrusel de 4 fotos propias (1920px, JPEG ~80, 0.2–1 MB). Cada foto define `focus` (posición del vehículo), `side` (en pantallas ≥ 980px el vehículo se lleva a la derecha de la tarjeta) y `zoom` máximo (1.2 por defecto) para no pixelar. En el frontend: `src/frontend/src/assets/login/` |
| Leyenda | Abajo a la izquierda: pin ámbar + lugar + indicador de progreso. Abajo a la derecha: "© 2026 RutaAndina · Ayacucho, Perú" (oculto en móvil) |
| Degradado | Radial oscuro detrás de la tarjeta + vertical (55% arriba, 80% abajo) + lateral suave de marca |
| Tarjeta | Máx. 420px, padding 32px (24/20px móvil), radio 24px, fondo `rgba(7,28,25,.74)` + `blur(20px)`, borde blanco 12%, sombra `0 24px 64px -24px rgba(0,0,0,.65)` |
| Cabecera | Isotipo 46px + "RutaAndina" 20px/800; en personal, insignia "AGENCIAS" a la derecha. Título 26px/700 y bajada en 72% de opacidad |
| Campos | Etiqueta arriba (13.5px/500), input 48px, radio 12px, ícono a la izquierda, relleno blanco 6%, borde blanco 22%; foco: borde `--accent-500` + anillo 4px ámbar 18%. "¿Olvidaste…?" a la derecha de la etiqueta Contraseña |
| Botón | 50px, sólido `--primary-600` (hover `#11806D`), texto blanco 15.5px/700, flecha avanza 3px; estados: cargando (spinner) → éxito (check + texto) |
| Pie de tarjeta | Separador + enlaces secundarios centrados |
| Accesibilidad | Errores con `role="alert"` y `aria-invalid`, región `aria-live`, aviso de Bloq Mayús, mostrar/ocultar contraseña con `aria-pressed`, foco visible ámbar, `prefers-reduced-motion` desactiva carrusel, parallax y animaciones |

**Diferencias por rol**

| | Pasajero | Personal |
|---|---|---|
| Voz | Tuteo | Usted |
| Título / bajada | "Inicia sesión" / "Reserva tu asiento y revisa tus pasajes en un solo lugar." | "Portal de agencias" / "Ingrese con su cuenta de personal para acceder a ventanilla o administración." + insignia AGENCIAS |
| Usuario | "Correo o DNI" (correo o 8 dígitos) | "Correo corporativo" |
| Recordar | "Mantener la sesión iniciada" | "Recordar este equipo" |
| Botón / éxito | "Iniciar sesión" / "Sesión iniciada" | "Ingresar al panel" / "Acceso concedido" |
| Pie | "¿No tienes cuenta? Regístrate gratis" + "¿Trabajas en una agencia? Portal de agencias" | "Las cuentas del personal las crea el administrador de su empresa." + enlace al login de pasajeros |
| Orden de fotos | Carretera, bus, puna, Vilcashuamán | Bus, puna, carretera, Vilcashuamán |

**Fuera de alcance v1:** inicio de sesión con Google u otras redes. **Integración:** el envío llama `POST /api/auth/login`; la validación del cliente no reemplaza la del servidor; tras el éxito se redirige según el rol del JWT (pasajero → búsqueda; agente → panel de ventanilla; admin → selector de agencia; superadmin → dashboard global). Las fotos con placas o marcas de empresas reales deben reemplazarse o difuminarse antes de publicar.

---

## 10. Pantallas y componentes compartidos

Las 30 pantallas están hechas a mano sobre `_assets/app.css` y `_assets/app.js`. Nada se genera en Stitch.

- **Mapa de asientos:** un solo componente para el pasajero y el agente. Recibe el modelo (`yaris`, `avanza`, `minivan`, `van`, `bus1`, `bus2`), el piso y los asientos por estado. El plano de cada modelo es una grilla con conductor (`C`), pasillo, vacío, escalera y asientos numerados, igual a lo que define el superadmin en el editor de modelos.
- **Shell del personal:** sidebar `--primary-900` de 248px (72px en tablet), barra superior con agencia (fija para el agente y con selector para el admin), buscador con atajo `/` y notificaciones.
- **Estados de una pantalla:** los modales y paneles laterales se documentan en el mismo `code.html` y se abren con `#estado` en la URL (captura `screen-<estado>.png`).
- **Pasajero:** navegación superior en escritorio y barra inferior en móvil; en la compra, resumen fijo a la derecha (barra inferior en móvil) y pasos 1–4.
- **Gráficos:** línea (ventas por canal), barras horizontales (ocupación; ámbar desde el 70 %), columnas apiladas (ventas por empresa), mapa de calor (demanda por día y hora), dona (ocupación por agencia) y sparklines en los KPI.
- **Datos de ejemplo:** empresas y personas ficticias (Transportes Qori Ñan, Expreso Sachapuyo, Turismo Wayra). Las políticas de cancelación no tienen montos fijos: la pantalla solo anuncia que se mostrará el monto según la política vigente.
