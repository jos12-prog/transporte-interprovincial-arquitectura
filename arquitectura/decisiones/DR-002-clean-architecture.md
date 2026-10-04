# ADR-002: Clean Architecture

## Estado

Aceptado.

## Contexto

La plataforma contiene diferentes módulos y funcionalidades que deberán evolucionar progresivamente.

Se requiere evitar que las reglas principales del negocio dependan directamente de tecnologías externas como PostgreSQL, Redis, servicios de pago o servicios de notificaciones.

## Decisión

Se adopta **Clean Architecture** como enfoque para organizar las responsabilidades y dependencias internas de la aplicación.

La solución se organizará considerando:

- **Entidades / Dominio:** reglas principales del negocio.
- **Casos de uso / Aplicación:** operaciones que ejecuta el sistema.
- **Adaptadores de interfaz:** conexión entre los casos de uso y elementos externos.
- **Frameworks e infraestructura:** base de datos, caché, servicios externos y tecnologías específicas.

Las dependencias deberán orientarse hacia el núcleo de la aplicación.

## Driver relacionado

- **DA10 – Mantenibilidad.**

## Justificación

Clean Architecture permite mantener las reglas de negocio independientes de tecnologías específicas y facilita modificar componentes externos sin afectar innecesariamente el núcleo de la aplicación.

## Consecuencias

### Positivas

- Mayor separación de responsabilidades.
- Menor acoplamiento con tecnologías externas.
- Facilita mantenimiento y pruebas.
- Permite reemplazar componentes de infraestructura con menor impacto.

### Consideraciones

- Requiere respetar las dependencias entre capas.
- Introduce una estructura interna más organizada que debe mantenerse durante el desarrollo.