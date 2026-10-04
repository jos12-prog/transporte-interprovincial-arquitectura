# ADR-001: Monolito Modular

## Estado

Aceptado.

## Contexto

La Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial debe gestionar diferentes funcionalidades relacionadas con usuarios, empresas, agencias, servicios, reservas, pagos, notificaciones y reportes.

La plataforma debe soportar el crecimiento progresivo de usuarios y operaciones, además de facilitar la modificación de funcionalidades sin afectar innecesariamente otros módulos.

## Decisión

Se adopta un **monolito modular** como organización inicial de la aplicación.

La solución estará organizada en módulos con responsabilidades claramente separadas, manteniendo inicialmente una única aplicación desplegable.

Los principales módulos serán:

- Usuarios.
- Empresas y agencias.
- Servicios.
- Reservas.
- Pagos.
- Notificaciones.
- Reportes.

## Drivers relacionados

- **DA03 – Escalabilidad.**
- **DA10 – Mantenibilidad.**

## Justificación

El monolito modular permite mantener una solución inicialmente simple de desarrollar, desplegar y mantener, sin perder la separación de responsabilidades entre los diferentes módulos.

Además, permite realizar escalamiento horizontal mediante múltiples instancias de la aplicación distribuidas mediante un balanceador de carga.

## Consecuencias

### Positivas

- Menor complejidad inicial de despliegue.
- Separación clara entre módulos.
- Facilita el mantenimiento y evolución.
- Permite escalamiento horizontal.
- Reduce la complejidad frente a una arquitectura inicial de microservicios.

### Consideraciones

- Los módulos deben mantener responsabilidades claramente definidas.
- Se debe evitar el acoplamiento innecesario entre módulos.
- La aplicación se despliega inicialmente como una sola unidad.