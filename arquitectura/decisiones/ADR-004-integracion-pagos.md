# ADR-004: Integración de Pagos mediante Interfaces y Adaptadores

## Estado

Aceptado.

## Contexto

La plataforma requiere integrarse con una pasarela de pago externa para procesar las operaciones relacionadas con el pago de reservas.

Una dependencia directa entre la lógica de negocio y un proveedor específico dificultaría el mantenimiento y una posible sustitución futura del servicio.

## Decisión

La integración con la pasarela de pago se realizará mediante **interfaces y adaptadores**.

Los casos de uso relacionados con pagos dependerán de una abstracción interna, mientras que la comunicación específica con el proveedor externo será implementada mediante un adaptador de infraestructura.

## Driver relacionado

- **DA06 – Integración de pagos.**

## Justificación

El uso de interfaces y adaptadores permite desacoplar la lógica de negocio del proveedor externo de pagos y mantener las dependencias de acuerdo con Clean Architecture.

## Consecuencias

### Positivas

- Reduce el acoplamiento con el proveedor externo.
- Facilita sustituir la pasarela de pago.
- Facilita las pruebas de los casos de uso.
- Mantiene independiente la lógica principal del sistema.

### Consideraciones

- Se requiere implementar y mantener el adaptador correspondiente.
- Los errores y respuestas del servicio externo deben ser tratados por la aplicación.