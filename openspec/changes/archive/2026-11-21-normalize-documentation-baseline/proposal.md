# Proposal

## Why

Los documentos del proyecto contienen decisiones incompatibles sobre alcance, estados, pagos, persistencia, concurrencia, vehículos e interfaces, además de reutilizar `DA` para drivers y decisiones. Esa divergencia hace difícil determinar el comportamiento acordado y verificarlo; esta propuesta consolida las decisiones entregadas por el usuario en un contrato documental trazable.

## What Changes

- Normalizar IDs: `DA01–DA14` quedan reservados a drivers; las decisiones pasan a `ADR-01–ADR-15`, con referencias corregidas y recuentos actualizados.
- Alinear actores, alcance web responsive, políticas multiempresa, estados de reserva/pago/pasaje, expiración digital de 10 minutos, venta presencial y flujos de Culqi idempotentes con webhook y Outbox/RabbitMQ.
- Precisar persistencia modular y aislamiento por tenant; sustituir el modelo de cupos por asientos numerados, modelos y planos de vehículos, asientos por viaje y reservas de varios pasajeros.
- Consolidar concurrencia, expiración distribuida, objetivos de rendimiento, tácticas de usabilidad, límites de caché, nombres canónicos y Strategy para políticas de cancelación/reprogramación.
- Retirar o marcar explícitamente como evolución futura choferes, réplica de lectura y app móvil nativa; agregar RF49 para alertas internas y registrar reglas pendientes de validación.
- Actualizar únicamente documentación y `README.md`; no agregar funcionalidades fuera de las decisiones listadas ni crear código.

## Capabilities

### New Capabilities

- `domain-baseline`: comportamiento documental acordado para actores, reservas, pagos, pasajes, inventario de asientos, aislamiento, integraciones y atributos de calidad de la plataforma.

### Modified Capabilities

Ninguna. El proyecto no tiene especificaciones OpenSpec existentes; se crea una capacidad que captura la línea base acordada.

## Impact

- Documentación afectada: `README.md`; documentos bajo `docs/01-analisis-de-sistema/`, `docs/02-arquitectura-software/`, `docs/03-diseño-de-software/diseño-interno/` y `docs/04-modelo-c4/` según el alcance enumerado en las decisiones de entrada.
- Artefactos OpenSpec: nueva especificación `specs/domain-baseline/spec.md`, diseño de reconciliación y tareas por archivo.
- No cambia código, API implementada, dependencias ni despliegues en esta fase. Las decisiones describen el comportamiento que la documentación deberá especificar para la futura implementación.
