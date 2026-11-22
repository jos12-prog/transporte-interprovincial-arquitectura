# Pantallas de referencia · RutaAndina

Maquetas HTML hechas a mano (no generadas en Stitch) a partir de `DESIGN.md`. Son la referencia visual para Codex: cada carpeta tiene `code.html` (abrir con doble clic) y sus capturas.

- `screen.png`: escritorio 1440 px (página completa).
- `screen-mobile.png`: móvil 390 px (solo pantallas del pasajero).
- `screen-<estado>.png`: el mismo `code.html` con un modal o panel abierto. En el navegador se ve agregando `#<estado>` a la URL.

Los datos son de ejemplo (empresas y personas ficticias). Los textos, estados y reglas siguen `openspec/specs/domain-baseline`.

## Recursos compartidos (`_assets/`)

| Archivo | Uso |
|---|---|
| `app.css` | Tokens y componentes: botones, campos, tarjetas, badges, tablas, mapa de asientos, boleto, modal, panel lateral, shell del personal |
| `app.js` | Íconos (trazos Lucide), **mapa de asientos por `VehicleModel`**, gráficos SVG, QR de muestra, contadores |
| `brand/` | Logo: `logo-mark.svg`, `logo-horizontal.svg`, `logo-horizontal-dark.svg` |
| `login/` | Fotos de fondo optimizadas (1920 px) |

En el frontend: tokens en `tailwind.config`, íconos con `lucide-react`, el mapa como componente `<SeatMap model floor seats />` y gráficos con una librería (p. ej. Recharts). El QR real lo genera el backend.

## Índice

| Rol | Pantalla | Estados extra |
|---|---|---|
| Pasajero | `01-login` · Iniciar sesión | — |
| | `02-registro` · Crear cuenta | — |
| | `03-inicio-busqueda` · Buscador, alta demanda, próximo viaje | — |
| | `04-resultados` · Resultados con filtros y skeleton | — |
| | `05-seleccion-asiento` · Mapa del bus de 2 pisos + modelos | — |
| | `06-datos-pasajeros` · Un pasajero por asiento o un solo nombre para todos + contador HELD | `#un-titular` |
| | `07-pago` · Pago con Culqi | `#confirmando` |
| | `08-pasaje-emitido` · Boletos con QR | — |
| | `09-mis-viajes` · Próximos, estados y estado vacío | — |
| | `10-detalle-reserva` · Historial y pago | `#cancelar` |
| Personal | `01-login` · Portal de agencias | — |
| Agente | `01-panel-ventanilla` · KPIs y salidas del día | — |
| | `02-venta-asientos` · Venta con el mismo mapa | — |
| | `03-cobro-emision` · Cobro (efectivo/Yape/POS) | `#emitido` |
| | `04-operaciones-del-dia` · Mis ventas y totales por método | — |
| Admin de agencia | `01-selector-agencia` · Elegir entre sus N agencias | — |
| | `02-dashboard` · KPIs, ventas por canal, alertas, ocupación | — |
| | `03-rutas` | `#nueva-ruta` |
| | `04-vehiculos` · Registro por modelo con vista previa | `#registrar` |
| | `05-viajes-horarios` · Calendario semanal | `#programar` |
| | `06-tarifas` · Precio único + fechas especiales | `#editar` |
| | `07-agentes` · Permisos cancelar / reprogramar | `#nuevo` |
| | `08-reportes` · Ingresos, mapa de calor, top rutas | — |
| Superadmin | `01-dashboard-global` · Integraciones Culqi / WhatsApp / RabbitMQ | — |
| | `02-empresas` | `#registrar` |
| | `03-agencias` · Agrupadas por empresa | `#nueva` |
| | `04-usuarios-administrativos` · Agencias solo de una empresa | `#asignar` |
| | `05-modelos-vehiculo` · Catálogo + editor de plano | `#nuevo` |
| | `06-festividades-alta-demanda` · Calendario y aviso WhatsApp | — |
| | `07-auditoria` · Registro y diferencias antes/después | `#detalle` |
