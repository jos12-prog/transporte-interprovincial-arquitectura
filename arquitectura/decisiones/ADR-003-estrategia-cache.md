# ADR-003: Estrategia de Caché

## Estado

Aceptado.

## Contexto

La plataforma realizará consultas frecuentes de información como rutas, horarios, tarifas y otros datos utilizados durante la búsqueda de servicios.

Durante periodos de alta demanda, estas consultas pueden incrementar la carga sobre la base de datos y afectar los tiempos de respuesta.

## Decisión

Se utilizará **Redis** como mecanismo de caché para almacenar temporalmente información consultada frecuentemente.

La información crítica relacionada con la confirmación de reservas y asignación de cupos continuará siendo validada mediante la persistencia transaccional.

## Driver relacionado

- **DA02 – Rendimiento.**

## Justificación

El uso de caché permite reducir consultas repetitivas sobre la base de datos y mejorar los tiempos de respuesta en operaciones de lectura frecuente.

## Consecuencias

### Positivas

- Reduce la carga sobre la base de datos.
- Mejora los tiempos de respuesta.
- Favorece el rendimiento durante periodos de alta demanda.

### Consideraciones

- La información almacenada en caché puede requerir mecanismos de expiración o actualización.
- La caché no será la fuente definitiva para confirmar una reserva.