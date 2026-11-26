# Tareas

## 1. Dominio MI03

- [x] 1.1 Extender `Route`, `Vehicle` y `Trip` con propiedad, activos, nombre visible y ciclo `Scheduled`/`Cancelled`; agregar `Fare` con periodo inclusivo base/especial y reglas de dominio. Verificar invariantes con pruebas unitarias.
- [x] 1.2 Agregar pruebas unitarias de dominio para periodos solapados, resolución de precio base/especial, snapshots, estados del viaje e intervalos inválidos; ejecutar el proyecto Unit.

## 2. Aplicación MI03

- [x] 2.1 Implementar casos de uso para consultar/crear/editar/desactivar rutas, vehículos y tarifas, derivados de `UseCase`; verificar autorización y validaciones con dobles de contratos.
- [x] 2.2 Probar casos de uso de catálogo: éxito, datos inválidos, recurso inexistente, registro inactivo, duplicidad y `AgencyScope`; ejecutar Unit.
- [x] 2.3 Implementar programación y cancelación de viajes con resolución especial/base, conflicto sin tarifa vigente, bloqueo de vehículo, snapshots y coordinación con MI04; verificar códigos de resultado y ausencia de escrituras parciales con Unit.
- [x] 2.4 Probar programación y cancelación: tarifa no disponible (`409` y detalle acordado), vehículo ocupado, holds liberados, asiento confirmado, cancelación repetida y rechazo de un hold sobre viaje cancelado; ejecutar Unit.

## 3. Contratos y persistencia

- [x] 3.1 Extender contratos públicos de Shared para validación de empresa/agencia y para que MI04 provisione `TripSeat`, consulte elegibilidad y libere holds/reporte asientos confirmados; verificar referencias de capas con Architecture.
- [x] 3.2 Configurar las columnas, tablas, índices y restricciones nuevas de MI03 y su `DesignTimeFactory`; crear una migración solo de esquema, sin duplicar ni reasignar rutas ni transformar datos; actualizar Development para crear rutas y tarifas base por empresa desde el inicio. Verificar con Testcontainers sobre base nueva que ambas empresas tienen rutas y tarifas propias.
- [x] 3.3 Implementar en infraestructura la coordinación de una transacción PostgreSQL entre MI03 y MI04 para programar/cancelar sin parciales, impedir holds en viajes cancelados y serializar conflictos de horario/tarifa; invalidar Redis únicamente tras commit. Verificar rollback, bloqueo y siguiente búsqueda actualizada con Integration.

## 4. Presentación y búsqueda pública

- [x] 4.1 Exponer endpoints `/api/v1/admin/...` para rutas, vehículos, tarifas y viajes, incluido cancelar/desactivar; aplicar rol `agency_admin`, `AgencyScope`, ProblemDetails y respuestas Swagger `200/201/204/400/403/404/409`; verificar contratos HTTP con Integration.
- [x] 4.2 Ajustar búsqueda y mapa para omitir viajes `Cancelled` de resultados y devolver el nombre visible del vehículo junto con el modelo; verificar resultados y contratos OpenAPI con Integration/Jest API.

## 5. Frontend

- [x] 5.1 Mover en `StaffShell` la tarjeta de usuario y `Cerrar sesión` al pie del sidebar, dejar título/insignia en la barra superior y añadir al login pasajero el enlace a `/personal/login`; verificar navegación, accesibilidad y pruebas RTL.
- [x] 5.2 Implementar las pantallas de rutas, vehículos, viajes/horarios y tarifas según `design/stitch/admin-agencia/` y `DESIGN.md`, dentro de `StaffShell` y `RequireRole`; usar solo modelos activos y `SeatLayoutView`. Verificar formularios, plano y respuestas ProblemDetails.
- [x] 5.3 Agregar/ajustar pruebas Jest/RTL del catálogo, estados de carga/error/409, rol protegido, cierre de sesión, enlace de login y plano; ejecutar Jest frontend y `npm run build`.

## 6. Pruebas y evidencias

- [x] 6.1 Agregar Integration/Testcontainers sobre base nueva para comprobar que la migración solo crea el esquema y que el seeder crea rutas y tarifas base separadas para Andes Express y Wari Bus; cubrir CRUD, `400/403/404/409`, empresa/agencia cruzada, referencias inactivas, duplicidad de placa, cambios a vehículos con viajes y precio copiado; ejecutar Integration.
- [x] 6.2 Agregar Integration/Testcontainers para que se creen exactamente los asientos del plano, viajes concurrentes no compartan vehículo en horarios solapados, cancelación libere holds atómicamente y falle sin cambios ante asientos confirmados; cubrir carrera hold/cancel y caché después de commit; ejecutar Integration.
- [x] 6.3 Agregar pruebas Jest API y requests/tests Postman de rutas, vehículos, tarifas, viajes, conflictos, aislamiento, búsqueda/mapa y cancelar; verificar Jest contra API real y Newman sin fallos.
- [x] 6.4 Actualizar `docs/evidencias/README.md` para enlazar `docs/criterios-evaluacion.md` y registrar resultados reales, pendientes y evidencia de las nuevas pantallas, migración y pruebas.

## 7. Verificación final

- [ ] 7.1 Ejecutar `dotnet build`, toda la suite `dotnet test`, build y Jest frontend, Jest API/Newman contra Compose y `openspec validate --all --strict`; corregir fallos y conservar resultados antes de cerrar el change.

## 8. Ajustes de revisión

- [x] 8.1 Barra lateral con estado activo (`NavLink`, `aria-current`, barra ámbar), grupos por rol y bloque de usuario según `design/stitch/`.
- [x] 8.2 Selector de agencia en la barra superior (`.top .agency`) con autoselección, `sessionStorage` y estados de carga, vacío y error.
- [x] 8.3 Páginas de rutas, vehículos, viajes y tarifas con `.page-head`, tablas, drawers, badges y formato de fechas `es-PE`.
- [x] 8.4 Login devuelve `fullName` y `email`; el bloque de usuario nunca muestra el ID. Verificado por Jest API e Integration.
- [x] 8.5 Seed de desarrollo de Ayacucho hacia sus provincias (34 rutas, 238 viajes, 2.688 `trip_seats`) más una ruta a Lima. Verificado por Integration sobre base nueva.
- [x] 8.6 Barra lateral sin scroll horizontal (`overflow-x-hidden`, `min-w-0`, `truncate` y `title` en el bloque de usuario, ancho fijo `shrink-0`) y estado de ruta con switch accesible más badge.
- [x] 8.7 Ajustar los tests de Integration al seed regional: calcular `TripSeats` desde las capacidades sembradas, seleccionar un viaje determinista y alinear búsqueda/caché/concurrencia con las salidas diarias. Integration completado con Docker por el usuario.
- [x] 8.8 Acciones visibles por fila con ícono/color, confirmación y toast en Rutas, Vehículos, Viajes, Tarifas y superadmin (Empresas, Agencias, Modelos); badges con ícono y pruebas RTL actualizadas.
