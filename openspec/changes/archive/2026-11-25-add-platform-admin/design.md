# Diseño: administración de plataforma

## Contexto

Ver `proposal.md` y las deltas `specs/platform-administration/spec.md` y `specs/trip-search/spec.md`. MI02 ya persiste `Company` y `Agency` en `companies_agencies`; MI03 ya posee `VehicleModel`, `SeatLayout`, vehículos y viajes en `transport_scheduling`. Las entidades actuales de empresa/agencia/modelo aún no tienen todos los estados y datos de presentación requeridos. Las consultas públicas se resuelven desde MI06 y el mapa consulta el plano vigente del modelo.

El diseño sigue el monolito modular y Clean Architecture de ADR-01/ADR-02: Domain no depende de EF Core ni ASP.NET; Application coordina casos de uso mediante interfaces; Infrastructure implementa persistencia; Presentation expone REST. La comunicación entre módulos usa contratos públicos en `Shared.Contracts` y no consulta directamente las tablas de otro módulo.

## Objetivos / No objetivos

**Objetivos:**

- Hacer gestionables desde la web las empresas, agencias y modelos globales, con autorización `super_admin`.
- Mantener los esquemas y migraciones independientes de MI02 y MI03.
- Excluir de la búsqueda pública los viajes de empresas o agencias inactivas y conservar la integridad de mapas de asientos ya existentes.

**No objetivos:**

- Administración de rutas, horarios, vehículos, tarifas y viajes por el administrador de agencia.
- Gestión de usuarios administrativos, reportes, reprogramación de viajes, almacenamiento de archivos de logo o login con Google.

## Decisiones

### Propiedad modular y persistencia

- MI02 es dueño de empresas y agencias. Agregar estado `IsActive` y los campos de presentación de los formularios aprobados en `design/stitch/superadmin/02-empresas/` y `03-agencias/` (nombre, RUC y datos de contacto de empresa; empresa, nombre, dirección, ciudad y teléfono de agencia). La validación de RUC es de formato; consulta a SUNAT queda fuera.
- MI03 es dueño de modelos. Agregar estado `IsActive` y categoría si se requiere mostrar los datos del formulario de `design/stitch/superadmin/05-modelos-vehiculo/`; conservar capacidad y `SeatLayout` como fuente del mapa. El nombre visible puede cambiar sin cambiar el identificador del modelo.
- Persistir en las tablas existentes de cada esquema: `companies_agencies.companies`, `companies_agencies.agencies` y `transport_scheduling.vehicle_models`. Crear migraciones mediante el `DesignTimeFactory` de cada módulo. No compartir DbContext ni crear claves foráneas entre esquemas propiedad de módulos distintos.
- No se eliminan físicamente empresas, agencias ni modelos. Desactivar es un cambio de estado reversible. Una agencia solo se crea bajo una empresa activa; la UI refleja jerarquía empresa-agencia.

### Casos de uso y API

Casos de uso explícitos en Application, por ejemplo `CreateCompanyUseCase`, `UpdateCompanyUseCase`, `SetCompanyActiveUseCase`, `CreateAgencyUseCase`, `SetAgencyActiveUseCase`, `CreateVehicleModelUseCase` y `UpdateVehicleModelUseCase`. La capa de presentación solo valida formato HTTP y delega reglas al caso de uso.

API versionada, autorizada por policy de rol `super_admin`, con rutas sugeridas:

- `GET/POST /api/v1/admin/companies`; `GET/PUT /api/v1/admin/companies/{companyId}`; `PATCH /api/v1/admin/companies/{companyId}/status`.
- `GET/POST /api/v1/admin/companies/{companyId}/agencies`; `GET/PUT /api/v1/admin/agencies/{agencyId}`; `PATCH /api/v1/admin/agencies/{agencyId}/status`.
- `GET/POST /api/v1/admin/vehicle-models`; `GET/PUT /api/v1/admin/vehicle-models/{vehicleModelId}`; `PATCH /api/v1/admin/vehicle-models/{vehicleModelId}/status`.

Los POST devuelven 201, lecturas/actualizaciones correctas 200, cambios de estado 200 y fallos de validación 400 `ProblemDetails`, recurso inexistente 404, conflicto de unicidad o plano congelado 409, no autenticado 401 y rol insuficiente 403. Swagger documenta contratos y respuestas.

### Inmutabilidad de plano y estado de modelo

MI03 verifica el uso consultando sus propios vehículos. Si existen vehículos vinculados, `SeatLayout` y capacidad quedan congelados; se permite renombrar o desactivar el modelo. El chequeo y la escritura se realizan en una transacción que serializa las actualizaciones del modelo para evitar una carrera con su uso. Crear un nuevo diseño requiere crear otro modelo.

El catálogo administrativo puede mostrar modelos activos e inactivos; el contrato destinado a seleccionar modelos para nuevos vehículos devuelve solo activos. La futura operación de alta de vehículo de `add-agency-catalog-admin` deberá validar también en backend que el modelo siga activo en el momento de guardar.

### Efecto de desactivar sobre búsqueda y caché

MI02 publica en `Shared.Contracts` consultas que informen estado activo de empresas/agencias en lote. MI06 filtra resultados mediante ese contrato; nunca lee tablas de MI02 directamente. Después de confirmar la transacción que activa o desactiva una empresa/agencia, el caso de uso llama directamente al método de invalidación del contrato existente de caché de búsqueda. La invalidación ocurre después del commit para no borrar caché por una transacción fallida. Si una búsqueda concurrente vuelve a guardar un resultado antiguo, el TTL de 60 segundos sigue siendo el límite máximo, consistente con ADR-09. Desactivar una empresa afecta a todas sus agencias; desactivar una agencia afecta solo a los viajes de esa agencia. Esta operación directa no usa Outbox.

### Interfaz

Implementar pantallas bajo el shell del superadministrador siguiendo `design/stitch/DESIGN.md` y `design/stitch/superadmin/02-empresas/`, `03-agencias/` y `05-modelos-vehiculo/`. Incluir estados vacíos, carga, error, confirmación para desactivar, formulario y validación accesible. El editor visual de plano mantiene los números y posiciones que consume el mapa; no se permite el cambio de plano de un modelo usado. No persistir archivos de logo hasta definir storage; la marca utiliza nombre o iniciales como fallback.

### Pendiente para `add-agency-catalog-admin`

Estas decisiones pertenecen al segundo change y se registran aquí para mantener coherencia, sin ampliar el alcance de `add-platform-admin`:

1. Rutas y tarifas pertenecen a la empresa y son compartidas por sus agencias. El administrador de agencia solo podrá gestionarlas dentro del `company_id` de su token. Los viajes pertenecen a una agencia.
2. Habrá una tarifa base por ruta con vigencia y precios especiales por fechas. Al crear un viaje se resolverá la tarifa vigente para su fecha y se copiará el precio al `Trip`; cambios posteriores de tarifa no modificarán viajes existentes. Un único precio aplica a cada asiento.
3. `Trip` tendrá estados `Scheduled` y `Cancelled`. Cancelar ocultará el viaje de la búsqueda y liberará holds HELD activos, cancelando sus reservas y dejando los asientos libres. Si existen ventas confirmadas, responderá 409. No habrá eliminación física; reprogramación y devolución quedan para un change posterior (RF15–RF17, RF28).
4. Un `VehicleModel` desactivado no podrá elegirse para vehículos nuevos. Si el modelo ya está en uso, su plano no podrá cambiar (409); solo se permitirá modificar su nombre visible o desactivarlo. Un plano distinto requiere crear un modelo nuevo.

## Riesgos / Compensaciones

- [La desactivación podría dejar resultados cacheados hasta su expiración] → Invalidar entradas de búsqueda por evento y mantener TTL máximo de 60 segundos como límite de consistencia.
- [El modelo global es usado por viajes de varias empresas] → Congelar plano al tener vehículos asociados y permitir renombrar/desactivar sin cambiar su identidad ni su geometría.
- [Cambios de esquema incompatibles con datos existentes] → Migraciones aditivas, valores predeterminados activos para filas existentes y pruebas de migración/integración antes de liberar.
- [La invalidación directa coincide con una búsqueda en curso que repueble una entrada] → Mantener el TTL máximo de 60 segundos y verificar la invalidación y la siguiente lectura de PostgreSQL en pruebas de integración.
- [Las mutaciones administrativas todavía no se auditan] → La auditoría y su infraestructura Outbox quedan explícitamente diferidas al change futuro `add-audit-trail` (RF43).

## Plan de migración

1. Crear migraciones aditivas de MI02 y MI03; filas preexistentes quedan activas para conservar la semilla y catálogo actual.
2. Desplegar contratos y casos de uso antes de habilitar las rutas administrativas; aplicar migraciones en Development y en el despliegue controlado.
3. Publicar API y UI; verificar autorización, activación, búsqueda pública y mapa para los modelos ya asociados.
4. Rollback: retirar la UI/API y revertir migraciones solo si no se han persistido cambios que dependan de las columnas nuevas; conservar registros, sin borrado destructivo.

## Preguntas abiertas

Ninguna para el alcance acordado. La persistencia de logos se pospone y no bloquea el catálogo funcional.
