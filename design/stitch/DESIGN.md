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
| `--neutral-900` | `#111B19` | Texto principal |
| `--neutral-600` | `#4A5754` | Texto secundario |
| `--neutral-500` | `#62706C` | Texto terciario, ayudas (5.2:1 sobre blanco) |
| `--neutral-200` | `#E0E6E4` | Bordes |
| `--neutral-50` | `#F6F8F7` | Fondo de página |

Los neutros llevan un toque verde de la marca (no el gris azulado de Tailwind *slate*): tienen la misma luminosidad, así que el contraste no cambia, pero los grises ya no compiten con el teal. Escala completa en `_assets/app.css` (`--n-50` … `--n-900`). No usar hex sueltos: siempre el token.
| `--success-600` | `#16A34A` | Pago aprobado, pasaje emitido |
| `--danger-600` | `#DC2626` | Errores, cancelación |
| `--warning-500` | `#F59E0B` | Reserva por expirar |

Estados del mapa de asientos (iguales en web pasajero y ventanilla). Cada asiento es una butaca dibujada (respaldo, cojín y apoyabrazos) con número de 2 cifras (`01`, `02`…) y una insignia en la esquina para que el estado se entienda sin depender solo del color:

| Estado | Texto en la leyenda (pasajero / agente) | Estilo |
|---|---|---|
| Libre | "Libre · Tócalo para elegirlo" / "Libre · Disponible para vender" | Butaca blanca, borde `--primary-600`, número `--primary-700`, sin insignia |
| Seleccionado | "Tu selección" / "Elegido" | Butaca `--primary-700` (teal oscuro), número blanco, insignia blanca con ✓ |
| Pendiente de pago (HELD, 10 min) | "Pendiente de pago · Alguien lo está pagando · se libera en 10 min" / "Retenido en la web · 10 min" | Butaca `--accent-100`, borde punteado `--accent-500`, insignia ámbar con reloj |
| Ocupado (CONFIRMED) | "Ocupado · Ya pagado" / "Vendido · Pagado y emitido" | Butaca `--n-200`, número `--n-400` tachado, **sin insignia** (el gris y el tachado ya lo distinguen sin depender del color; con ✕ en cada asiento vendido el plano se llenaba de ruido) |
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
- Sombras en capas (un contacto nítido + una difusa): `sm` `0 1px 2px rgba(17,27,25,.05), 0 1px 1px rgba(17,27,25,.03)`; `md` `0 1px 2px rgba(17,27,25,.04), 0 8px 24px -6px rgba(17,27,25,.10)`; `lg` `0 2px 6px rgba(6,63,54,.06), 0 24px 48px -12px rgba(6,63,54,.22)`.
- Íconos: Lucide, trazo 1.75px, 20px.

---

## 5. Componentes base

| Componente | Especificación |
|---|---|
| Botón primario | Fondo `--primary-600`, texto blanco, alto 48px, flecha → que se desplaza 3px al hover. Sombra mínima y nítida, sin halo de color. **Un solo primario por vista**: en listas y tablas ("Vender" en ventanilla) va el secundario |
| Botón secundario | Borde `--primary-600`, fondo transparente |
| Input | Alto 48px, ícono a la izquierda, borde `--neutral-200`, foco con anillo `--primary-100` 4px |
| Tarjeta de viaje | Empresa + logo, hora salida → llegada con línea de ruta, duración, tipo de vehículo, asientos libres, precio grande, botón "Elegir asiento" |
| Mapa de asientos | Silueta del vehículo según `VehicleModel` (Yaris 4, Avanza 6, Minivan 7, Van 15, Bus 1 y 2 pisos), frente arriba, leyenda de estados debajo |
| Contador de retención | Píldora `--accent-500` con reloj y mm:ss; cambia a `--danger-600` en el último minuto |
| Badge alta demanda | Píldora `--accent-100` con llama/ícono y texto "Alta demanda" |
| Pasaje | Tarjeta tipo boleto con muesca lateral, QR grande, código, pasajero, asiento, fecha |
| Sidebar (staff) | Fondo verde casi negro `#0B2824` (la marca sigue presente sin pesar en jornadas largas), ítems blancos 72% opacidad, activo con barra `--accent-500` a la izquierda. Fija a la altura de la pantalla: la tarjeta del usuario y **Cerrar sesión** quedan siempre visibles al hacer scroll |
| Cerrar sesión | **Personal** (agente, admin, superadmin): botón con ícono y texto "Cerrar sesión" dentro de la tarjeta del usuario, al pie de la barra lateral (en tablet, solo el ícono). En el selector de agencia: "Ingresó como … · Cerrar sesión". **Pasajero**: el avatar abre un menú (Mis viajes, Mi cuenta, Cerrar sesión en rojo); en móvil también lo abre la pestaña "Cuenta". El menú sale de su botón (150ms), se cierra con Esc o clic fuera |
| Regreso | Enlace `.back` arriba a la izquierda, siempre en el mismo lugar, que dice a dónde vuelve: "Volver a los horarios" (asientos), "Cambiar asientos" (pasajeros), "Editar datos de pasajeros" (pago), "Mis viajes" (detalle), "Volver a ventanilla" / "Volver a asientos y pasajeros" (venta en ventanilla). El pasaje emitido no tiene regreso (la compra terminó). Paneles y modales se cierran con ✕ / Cancelar |
| Selector de agencia | Dropdown en la barra superior con nombre de empresa y agencia activa |
| Tabla | Cabecera fija, filas 56px, zebra suave, acciones en menú "⋯". Dentro de una tarjeta respeta sus esquinas redondeadas |
| KPI | Etiqueta en una línea arriba; valor y sparkline (72px) en la fila siguiente; variación debajo. Todos los valores de una fila quedan a la misma altura |
| Contenedor | Máx. 1200px con 24px de margen lateral (16px móvil). El contenido siempre alinea con el logo de la barra superior |
| Toast | Esquina superior derecha, ícono de estado, se cierra solo en 4s. En el frontend con Sonner (transiciones interrumpibles, entra y sale por arriba) |
| Botón de Google | Solo pasajero (login y registro). Estilo oscuro oficial de Google Identity: relleno `#131314`, borde `#8E918F`, texto `#E3E3E3` Roboto 500, logo "G" a color sin modificar, 48px, radio 12px. Textos aprobados por Google: "Continuar con Google" / "Registrarse con Google" |
| Estado vacío | Ilustración lineal + texto + acción |

---

## 6. Movimiento y animaciones

Criterio: la filosofía de diseño de interfaces de Emil Kowalski (`.agents/skills/emil-design-eng`, `animate`). Antes de animar algo se responde, en orden: **¿se ve tan seguido que no debería animarse? → ¿para qué sirve? → ¿qué curva? → ¿cuánto dura?** Lo que se usa decenas de veces al día (atajos, hover de filas, mostrar contraseña) no se anima o casi no se nota. La "delicia" se reserva para lo que pasa una vez: la entrada al login, el pago aprobado, el pasaje emitido.

**Tokens** (en `_assets/app.css` y `_assets/auth.css`; en el frontend, `tailwind.config` → `transitionTimingFunction` y `transitionDuration`):

| Token | Valor | Uso |
|---|---|---|
| `--ease-out` | `cubic-bezier(.23,1,.32,1)` | Todo lo que entra o sale, y la respuesta al presionar |
| `--ease-in-out` | `cubic-bezier(.77,0,.175,1)` | Algo que se mueve o cambia de forma estando en pantalla |
| `--ease-drawer` | `cubic-bezier(.32,.72,0,1)` | Paneles laterales y hojas (curva tipo iOS) |
| `ease` | nativo | Cambios de color y hover |
| `linear` | nativo | Movimiento constante: spinner, skeleton, barra de progreso, zoom lento del fondo |
| `--dur-press` / `--dur-fast` / `--dur-base` | 160 / 150 / 200 ms | Presionar / color / mensajes y estados |
| `--dur-modal` / `--dur-drawer` | 250 / 420 ms | Modal / panel lateral |

**Reglas fijas**

- Nunca `ease-in` en la interfaz (arranca lento justo cuando el usuario mira). Nunca `transition: all`: se nombran las propiedades.
- Solo se animan `transform` y `opacity` (más `clip-path` y `filter`). La única excepción es el alto de los mensajes bajo un campo, que se abren como acordeón (`grid-template-rows 0fr → 1fr`).
- Nada aparece desde `scale(0)`: mínimo `.6` para insignias y `.95–.97` para superficies, siempre con opacidad.
- Todo lo presionable responde con `scale(.97)` en 160ms `--ease-out` (iconos `.94`, asientos `.94`, casillas `.9`).
- El hover solo existe con puntero real: `@media (hover:hover) and (pointer:fine)`. En táctil el tap deja el hover pegado.
- Las animaciones de entrada usan `animation-fill-mode: backwards`, no `both`. Si no, el último fotograma "congela" el `transform` y el `:active` o la sacudida dejan de funcionar.
- Cascadas con intervalos que se acortan (50 → 40 → 30 → 25 → 20ms): se ven como una sola ola. Nunca bloquean la interacción.
- Cuando dos estados se cruzan (texto → spinner → check), se suma `blur(3px)` en la transición. Así el ojo ve una transformación y no dos cosas superpuestas.
- Lo que el usuario puede disparar muy rápido (toasts, toggles) usa transiciones, que se pueden interrumpir, no keyframes.
- **Menos movimiento** (`prefers-reduced-motion`) significa más suave, no cero: se quedan los fundidos y cambios de color que explican un estado; se quitan desplazamientos, escalas, desenfoques, parallax, zoom, sacudidas y pulsos.

| Momento | Animación | Duración / curva |
|---|---|---|
| Login (fondo) | Carrusel de fotos con fundido y zoom lento 1.06→1; indicador de progreso; parallax de 14px solo con mouse, suavizado (no sigue al puntero en seco); se pausa si la pestaña está oculta. Sin fotos: degradado verde de marca | 8s por foto, 1.4s fundido `ease`, zoom 12s `linear`, parallax 900ms `--ease-out` |
| Login (tarjeta) | Sube 14px, escala desde .985 y se aclara de `blur(6px)` | 560ms `--ease-out`, una vez |
| Login (contenido) | Cascada: cada bloque sube 6px con opacidad; retrasos 140, 186, 228, 266… ms (cada salto más corto) | 420ms `--ease-out` |
| Login (logo) | El pin cae sobre Huamanga y se asienta 1px | 620ms `--ease-out`, retraso 420ms |
| Botón "Iniciar sesión" / Google | Presionar: `scale(.97)`. Hover (mouse): color + flecha avanza 3px. Estados: texto → spinner → check que se dibuja, cruzados con opacidad + 3px + `blur(3px)` | 160ms presionar · 200ms cambio de estado · 320ms trazo del check · spinner 600ms `linear` |
| Error de login | El mensaje se abre bajo el campo (alto + baja 4px) y la tarjeta se sacude con amplitud decreciente (7 → 5 → 3 → 1.5px) | 200ms `--ease-out` · 380ms `--ease-out` |
| Bloq Mayús | Mismo acordeón que el error | 200ms |
| Casillas | Fondo y borde con `ease`; el check entra desde `.7` | 150 / 200ms |
| Cambio de página | Opacidad + 8px, una sola ola por página | 320ms `--ease-out` |
| Resultados de búsqueda | Tarjetas en cascada; skeleton mientras carga | 320ms · skeleton 1.4s `linear` |
| Selección de asiento | "Pop" a 1.08 y la insignia ✓ entra desde .6 justo después | 260ms `--ease-out` · insignia 240ms (+60ms) |
| Asiento tomado por otro | El color pasa a "retenido" sin saltos + tooltip "Alguien lo está reservando" | 200ms `ease` |
| Contador HELD | Pulso suave en el último minuto (se quita con menos movimiento) | 1s, infinito |
| Modal | Fondo se funde; modal escala desde .96, **centrado** (no sale de un botón) | 250ms `--ease-out` |
| Panel lateral | Entra con su propio ancho (`translateX(100%)`) | 420ms `--ease-drawer` |
| Toast | Entra y sale por arriba: −12px + escala .98 | 400ms `ease` (Sonner) |
| Interruptor | El pulgar se desliza; al presionar se estira 3px | 200ms · 160ms |
| Pago aprobado | Check que se dibuja (stroke) + confeti discreto en colores de marca | 900ms |
| Pasaje emitido | El boleto "se imprime" (slide-down con clip) | 600ms `--ease-out` |
| KPIs del dashboard | Números cuentan desde 0 (count-up) y gráficos se dibujan | 800ms `--ease-out` |
| Sidebar | Colapsa/expande | 200ms `--ease-in-out` |
| Sin animación | Mostrar/ocultar contraseña, atajo `/` del buscador, hover de filas de tabla, cambio de pestañas del segmentado | — |

Máximo 1 animación en bucle por pantalla. En el frontend: CSS para lo predeterminado (entradas, presionar, spinner; corre fuera del hilo principal) y Motion (`motion/react`) solo para lo interrumpible o con gestos (arrastrar el mapa en móvil, listas que entran y salen).

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

Referencias: `design/stitch/pasajero/01-login/`, `pasajero/02-registro/` y `personal/01-login/` (`code.html`, `screen.png` 1440×900, `screen-mobile.png` 390×844). No se generan en Stitch. Las tres comparten `design/stitch/_assets/auth.css` (estilos y movimiento) y `auth.js` (carrusel, estados del botón, mensajes, Google simulado); cada página solo declara sus textos y fotos en `window.RA_AUTH`. En el frontend: `<AuthLayout>`, `<AuthCard>`, `<SubmitButton state="idle|loading|done">` y `<GoogleButton>`. Otros recursos: `_assets/login/fondo*.jpg`, `_assets/brand/logo-*.svg`.

**Criterio visual:** sobrio y fotográfico, como un producto real. Sin bordes luminosos animados, halos, brillos que cruzan el botón ni ilustraciones genéricas. El impacto lo dan las fotos reales de Ayacucho y la tipografía.

**Composición:** foto a pantalla completa + degradado oscuro + tarjeta **centrada** con contenido alineado a la izquierda. Es la única pantalla con superficie oscura; el resto de la app usa fondo claro.

| Elemento | Especificación |
|---|---|
| Fondo | Carrusel de 4 fotos propias (1920px, JPEG ~80, 0.2–1 MB). Cada foto define `focus` (posición del vehículo), `side` (en pantallas ≥ 980px el vehículo se lleva a la derecha de la tarjeta) y `zoom` máximo (1.2 por defecto) para no pixelar. En el frontend: `src/frontend/src/assets/login/` |
| Leyenda | Abajo a la izquierda: pin ámbar + lugar + indicador de progreso. Abajo a la derecha: "© 2026 RutaAndina · Ayacucho, Perú" (oculto en móvil) |
| Degradado | Radial oscuro detrás de la tarjeta + vertical (55% arriba, 80% abajo) + lateral suave de marca |
| Tarjeta | Máx. 420px (520px en registro), padding 32px (24/20px móvil), radio 24px, fondo `rgba(8,26,23,.76)` + `blur(20px)`, borde blanco 10%, sombra en capas (contorno oscuro 1px + brillo interior arriba + `0 24px 64px -24px rgba(0,0,0,.70)`) |
| Google (solo pasajero) | Botón "Continuar con Google" ("Registrarse con Google" en registro) **antes** del formulario, porque es la vía más corta. Debajo, un separador "o con tu correo o DNI" ("o completa tus datos" en registro). Estados: normal → "Conectando con Google…" con spinner |
| Cabecera | Isotipo 46px + "RutaAndina" 20px/800; en personal, insignia "AGENCIAS" a la derecha. Título 26px/700 y bajada en 72% de opacidad |
| Campos | Etiqueta arriba (13.5px/500), input 48px, radio 12px, ícono a la izquierda, relleno blanco 5%, borde blanco 18% (30% en hover con mouse); foco: borde `--accent-500` + anillo 4px ámbar 16% y el ícono se pinta de ámbar. Con error, borde, anillo e ícono en coral. "¿Olvidaste…?" a la derecha de la etiqueta Contraseña. El autocompletado del navegador mantiene el fondo oscuro |
| Botón | 50px, sólido `--primary-600` (hover `#11806D`, solo con mouse), texto blanco 15.5px/700, flecha avanza 3px; estados: normal → cargando (spinner) → éxito (check que se dibuja + texto) en la misma celda, cruzados con blur. No se deshabilita al cargar (no se pierde el foco); el doble envío se ignora |
| Pie de tarjeta | Separador + enlaces secundarios centrados |
| Accesibilidad | Errores con `role="alert"` y `aria-invalid`, región `aria-live`, `aria-busy` en el botón que carga y `aria-hidden` en las capas inactivas, aviso de Bloq Mayús, mostrar/ocultar contraseña con `aria-pressed`, foco visible ámbar, la contraseña nunca se recorta. Con `prefers-reduced-motion` el carrusel no avanza y no hay parallax, zoom, sacudida ni desplazamientos; quedan solo fundidos |

**Diferencias por rol**

| | Pasajero | Personal |
|---|---|---|
| Voz | Tuteo | Usted |
| Título / bajada | "Inicia sesión" / "Reserva tu asiento y revisa tus pasajes en un solo lugar." | "Portal de agencias" / "Ingrese con su cuenta de personal para acceder a ventanilla o administración." + insignia AGENCIAS |
| Usuario | "Correo o DNI" (correo o 8 dígitos) | "Correo corporativo" |
| Recordar | "Mantener la sesión iniciada" | "Recordar este equipo" |
| Botón / éxito | "Iniciar sesión" / "Sesión iniciada" | "Ingresar al panel" / "Acceso concedido" |
| Google | "Continuar con Google" arriba del formulario | No. Las cuentas del personal las crea el administrador de su empresa |
| Pie | "¿No tienes cuenta? Regístrate gratis" + "¿Trabajas en una agencia? Portal de agencias" | "Las cuentas del personal las crea el administrador de su empresa." + enlace al login de pasajeros |
| Orden de fotos | Carretera, bus, puna, Vilcashuamán | Bus, puna, carretera, Vilcashuamán |

**Google (pendiente de especificar):** la pantalla ya muestra la opción, pero el backend no existe todavía. Antes de implementarla hace falta un change de OpenSpec que defina: el flujo (Google Identity Services y verificación del ID token en el servidor), qué pasa si el correo ya tiene una cuenta con contraseña y cómo se completan el documento y el celular de quien entra por primera vez con Google. Otras redes siguen fuera de alcance. **Integración:** el envío llama `POST /api/auth/login`; la validación del cliente no reemplaza la del servidor; tras el éxito se redirige según el rol del JWT (pasajero → búsqueda; agente → panel de ventanilla; admin → selector de agencia; superadmin → dashboard global). Las fotos con placas o marcas de empresas reales deben reemplazarse o difuminarse antes de publicar.

---

## 10. Pantallas y componentes compartidos

Las 30 pantallas están hechas a mano sobre `_assets/app.css` y `_assets/app.js`. Nada se genera en Stitch.

- **Mapa de asientos:** un solo componente para el pasajero y el agente. Recibe el modelo (`yaris`, `avanza`, `minivan`, `van`, `bus1`, `bus2`), el piso y los asientos por estado. El plano de cada modelo es una grilla con conductor (`C`), pasillo, vacío, escalera y asientos numerados, igual a lo que define el superadmin en el editor de modelos.
- **Shell del personal:** sidebar `--primary-900` de 248px (72px en tablet), barra superior con agencia (fija para el agente y con selector para el admin), buscador con atajo `/` y notificaciones.
- **Estados de una pantalla:** los modales y paneles laterales se documentan en el mismo `code.html` y se abren con `#estado` en la URL (captura `screen-<estado>.png`).
- **Pasajero:** navegación superior en escritorio y barra inferior en móvil; en la compra, resumen fijo a la derecha (barra inferior en móvil) y pasos 1–4.
- **Gráficos:** línea (ventas por canal), barras horizontales (ocupación; ámbar desde el 70 %), columnas apiladas (ventas por empresa), mapa de calor (demanda por día y hora), dona (ocupación por agencia) y sparklines en los KPI.
- **Datos de ejemplo:** empresas y personas ficticias (Transportes Qori Ñan, Expreso Sachapuyo, Turismo Wayra). Las políticas de cancelación no tienen montos fijos: la pantalla solo anuncia que se mostrará el monto según la política vigente.
