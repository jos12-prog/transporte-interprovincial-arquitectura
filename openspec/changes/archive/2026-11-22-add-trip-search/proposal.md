# Proposal

## Why

Los pasajeros necesitan comparar salidas interprovinciales por origen, destino y fecha, incluyendo empresa, horario, tarifa y asientos libres, sin depender de datos ficticios en la interfaz. La búsqueda pública hará verificable el flujo de consulta y los objetivos documentados de rendimiento y caché.

La capacidad cubre **HU01**, **RF01–RF03** y los campos comparables de resultados de **RF04** (docs/01-analisis-de-sistema/02-historias-de-usuario.md, § búsqueda de viajes; docs/01-analisis-de-sistema/03-requisitos-funcionales.md, § 2.1); el detalle de condiciones del viaje de RF04 y el mapa de asientos de RF05 quedan fuera. Se apoya en **RC02, RC04, RC06, RC12 y RC15** (docs/01-analisis-de-sistema/05-restricciones.md, § 1), **EQ-01 y EQ-08** (docs/01-analisis-de-sistema/08-escenarios-atributos-calidad.md), y **ADR-01, ADR-02, ADR-03, ADR-04, ADR-05 y ADR-09** (docs/01-analisis-de-sistema/07-decisiones-arquitectonicas.md). La prueba de rampa a 2.000 VUs no verifica EQ-09, que requiere comparar distintas capacidades/instancias. La propiedad de búsqueda en MI06 y el uso de contratos públicos entre módulos se sustentan además en docs/03-diseño-de-software/diseño-interno/diseño-interno-de-modulos.md (§ 2) y docs/04-modelo-c4/Nivel3-Diagrama-de-Componentes.md (§§ 3–5).

## What Changes

- Añadir los datos mínimos de empresas y agencias (MI02), rutas, modelos y vehículos (MI03), viajes (MI03) y asientos de viaje (MI04), con persistencia PostgreSQL en esquema y `DbContext` por módulo, migraciones EF Core y datos semilla de Ayacucho, Lima, Huanta, Andahuaylas y Cusco para varias empresas y viajes de los siguientes siete días.
- Incorporar búsqueda pública en MI06 mediante `GET /api/v1/trips/search`, con origen, destino y fecha obligatorios, filtros opcionales de empresa, agencia, horario y tarifa, validación, ProblemDetails y contrato Swagger. Los resultados expondrán empresa, agencia, salida/llegada, modelo del vehículo, precio y cantidad de asientos libres.
- Consultar módulos de datos mediante contratos públicos y respetar sus límites de propiedad; MI04 será propietario de `TripSeat` y MI03 consultará la disponibilidad por contrato.
- Añadir cache-aside Redis de 60 segundos exclusivamente para respuestas de búsqueda, incluyendo cantidad agregada de asientos libres como dato informativo; incluir `X-Cache: HIT|MISS`. No cachear ni exponer el mapa de asientos ni usar el conteo para confirmar reservas.
- Agregar índices de consulta y evidencias reproducibles en `docs/evidencias/bd-y-cache.md` con `EXPLAIN ANALYZE` antes/después del índice y medición de latencia con y sin Redis.
- Crear las pantallas de búsqueda y resultados del pasajero según Stitch, consumir la API real y cubrir estados de carga y sin resultados.
- Añadir pruebas unitarias, de integración, Jest de frontend/API y una carga k6 progresiva hasta 2.000 VUs; aplicar el umbral documentado de EQ-01 desde 200 VUs y reportar caché conforme EQ-08, sin fijar un SLO nuevo para 2.000 VUs.

## Capabilities

### New Capabilities

- `trip-search`: búsqueda pública de viajes por ruta y fecha, filtros y presentación de resultados con caché de búsqueda y disponibilidad agregada.

### Modified Capabilities

- Ninguna. `domain-baseline` ya establece inventario numerado, límites de módulos, uso de Redis y prohibición de cachear el mapa de asientos; esta propuesta concreta esos comportamientos en la capacidad de búsqueda.

## Impact

- **Módulos:** MI02 Empresas y Agencias; MI03 Transporte y Programación; MI04 Reservas y Pasajes (propietario de `TripSeat`/disponibilidad); MI06 Consultas y Reportes (propietario del caso de uso de búsqueda). La participación de MI02–MI04 se limita a sus modelos y contratos públicos, no a consultas directas de tablas ajenas.
- **Backend:** proyectos Domain/Application/Infrastructure/Presentation de los módulos afectados, configuración de DbContext, migraciones y semilla; endpoint versionado y documentación OpenAPI; Redis.
- **Frontend:** pantallas del pasajero bajo `src/frontend`, cliente HTTP y estados de carga/vacío.
- **Pruebas y evidencias:** `tests/Unit`, `tests/Integration`, `tests/api`, pruebas Jest del frontend, `tests/performance/search-load.js` y resultados bajo `docs/evidencias/`.
- **Alcance excluido:** login/JWT, mapa de asientos, retenciones, pagos, escritura de reservas y CRUD administrativo de rutas o viajes. La búsqueda es pública y de solo lectura; el conteo de asientos no es garantía transaccional.
