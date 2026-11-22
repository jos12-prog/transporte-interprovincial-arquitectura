# Tasks

## 1. Reconciliar análisis de sistema

- [x] 1.1 Actualizar `docs/01-analisis-de-sistema/00-necesidad-del-negocio.md` para describir disponibilidad y selección por asientos numerados; verificar que no se presente cupo abierto como modalidad.
- [x] 1.2 Actualizar `docs/01-analisis-de-sistema/01-actores.md` con el límite de una empresa y varias agencias autorizadas, la agencia activa y la matriz sin ventas presenciales del Superadministrador; verificar responsabilidades y tabla.
- [x] 1.3 Actualizar `docs/01-analisis-de-sistema/02-historias-de-usuario.md` para el mapa compartido, asientos numerados y multi-asiento con pasajeros; verificar trazabilidad de HU04, HU05, HU26 y HU30.
- [x] 1.4 Actualizar `docs/01-analisis-de-sistema/03-requisitos-funcionales.md` con asientos numerados, venta/pago presencial, RF49 para alertas internas y reglas pendientes; verificar RF05, RF06, RF33 y que cada requisito conserve ID único.
- [x] 1.5 Actualizar `docs/01-analisis-de-sistema/04-atributos-de-calidad.md` con AC01 para 200 usuarios concurrentes y objetivo AC10 de usabilidad; verificar coincidencia con EQ-01.
- [x] 1.6 Actualizar `docs/01-analisis-de-sistema/05-restricciones.md` con RC05 multiagencia, RC06 vehículos/asientos, máquinas de estado, canal responsive y límites de v1; verificar todas las referencias cruzadas.
- [x] 1.7 Actualizar `docs/01-analisis-de-sistema/06-drivers-arquitectonicos.md` preservando DA01–DA14 como drivers y corrigiendo trazabilidad semántica con decisiones ADR; verificar cada fila y matriz.
- [x] 1.8 Renombrar las decisiones y rehacer matrices de `docs/01-analisis-de-sistema/07-decisiones-arquitectonicas.md` como ADR-01–ADR-15; actualizar estados, integración de pagos, Outbox, expiración y cache; verificar referencias driver→decisión.
- [x] 1.9 Actualizar `docs/01-analisis-de-sistema/08-escenarios-atributos-calidad.md` a 200 usuarios para EQ-01 y alinear EQ-02 con la reserva por TripSeat; verificar resumen e IDs EQ-01–EQ-10.

## 2. Alinear arquitectura de solución

- [x] 2.1 Actualizar `docs/02-arquitectura-software/arquitectura-inicial.md` con alcance responsive, venta presencial directa, estados, pago idempotente y eventos Outbox; verificar diagramas y tablas frente a la especificación.
- [x] 2.2 Actualizar `docs/02-arquitectura-software/componentes-arquitectonicos.md` con VehicleModel/Vehicle/TripSeat, esquemas por módulo, tenant filters y nombres canónicos; verificar relaciones y eliminar la réplica de lectura de v1.
- [x] 2.3 Actualizar `docs/02-arquitectura-software/enfoque-arquitectonico.md` con referencias DA/ADR inequívocas, contratos canónicos y aislamiento por agencia activa; verificar matriz de capas y ejemplos.
- [x] 2.4 Actualizar `docs/02-arquitectura-software/estilo-arquitectonico.md` con IDs y alcance v1 corregidos; verificar que ninguna referencia a decisión use prefijo DA.
- [x] 2.5 Actualizar `docs/02-arquitectura-software/tacticas-arquitectonicas.md` con AC10/táctica 12, caché limitada a 60 s, bloqueo por TripSeat y expiración distribuida; marcar réplica de lectura como evolución futura y verificar matriz completa.
- [x] 2.6 Actualizar `docs/02-arquitectura-software/tecnologias-y-despliegue.md` con IDs ADR, web responsive y alcance sin réplica de lectura ni app nativa; verificar el diagrama y matriz de tecnología/decisiones.

## 3. Alinear diseño interno

- [x] 3.1 Actualizar `docs/03-diseño-de-software/diseño-interno/principios-de-diseño.md` con OCP para `ICancellationPolicy`, nombres canónicos, asientos numerados, quitar RG08 duplicada y corregir drivers; verificar tablas y código de ejemplo.
- [x] 3.2 Actualizar `docs/03-diseño-de-software/diseño-interno/patrones-de-diseño.md` reasignando PD03 a `ICancellationPolicy` con reglas pendientes, y actualizar interfaces, estrategia multi-asiento y estados; verificar código y trazabilidad.
- [x] 3.3 Actualizar `docs/03-diseño-de-software/diseño-interno/diseño-interno-de-modulos.md` con nombres de módulos, agregados, casos de uso y estados canónicos, tenant isolation y contratos públicos; verificar estructura, ejemplos y sección de drivers.

## 4. Alinear vistas C4

- [x] 4.1 Actualizar `docs/04-modelo-c4/Nivel1-DiagramadeContextodelSistema.md` con solo web responsive, objetivo de cero sobreventa como objetivo verificable y pagos reconciliados; verificar actores y relaciones externas.
- [x] 4.2 Actualizar `docs/04-modelo-c4/Nivel2-DiagramadeContenedores.md` con referencias ADR, persistencia modular/tenant sin “particionado lógicamente”, estados y pagos; verificar contenedores y alcance.
- [x] 4.3 Actualizar `docs/04-modelo-c4/Nivel3-Diagrama-de-Componentes.md` retirando choferes de v1 y actualizando contratos, ownership por módulo, Outbox, vehículos y asientos; verificar siete módulos MI01–MI07.
- [x] 4.4 Actualizar `docs/04-modelo-c4/Nivel4-DiagramadeCodigo.md` con DA02 para sobreventa, clases/interfaces/casos de uso canónicos, estados y pago sin notificación síncrona; verificar fragmentos, tablas PD y transacciones multi-asiento.

## 5. Cerrar consistencia global

- [x] 5.1 Actualizar `README.md` con 14 drivers `DA01–DA14`, 15 decisiones `ADR-01–ADR-15`, conteos restantes y alcance reconciliado; verificar enlaces, nombres y resumen ejecutivo contra los documentos fuente.
- [x] 5.2 Ejecutar una verificación cruzada de todos los archivos bajo `docs/` y `README.md`: comprobar que no queden referencias de decisiones `DA-`, `IPaymentPort`, `INotificationPort`, `INotificationService`, `QuotaSeatStrategy`, `CapacityQuotaStrategy`, `PostgresReservationRepository` ni “cupo” como modalidad; verificar que `IPaymentGateway`, `INotificationGateway` e `IReservationRepository` SÍ sigan existiendo; revisar también nombres canónicos, IDs únicos, estados, trazabilidad HU30→RF49 y contradicciones entre tablas, texto y diagramas; registrar cualquier excepción explícitamente marcada como evolución futura o pendiente.
- [x] 5.3 Listar los diagramas de `img/` (svg/png) que contienen nombres, IDs o conceptos obsoletos y marcarlos en README.md como “pendiente de regenerar”; no editar imágenes binarias.
