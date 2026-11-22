# Spec Delta

## Purpose

Proporcionar un endpoint de readiness verificable para que operadores, contenedores y pruebas determinen si el host puede atender solicitudes según el estado de sus dependencias.

## ADDED Requirements

### Requirement: Readiness agregado de dependencias
El API SHALL exponer `GET /health` sin requerir autenticación de usuario y SHALL comprobar PostgreSQL, Redis y RabbitMQ. SHALL responder HTTP 200 cuando todas las dependencias estén saludables y HTTP 503 cuando al menos una no lo esté, identificando el estado por componente sin exponer credenciales ni detalles de conexión.

#### Scenario: Todas las dependencias están disponibles
- **WHEN** PostgreSQL, Redis y RabbitMQ responden a sus comprobaciones de salud
- **THEN** `GET /health` responde HTTP 200 e incluye el estado saludable de los tres componentes.

#### Scenario: Una dependencia no está disponible
- **WHEN** PostgreSQL, Redis o RabbitMQ falla su comprobación de salud
- **THEN** `GET /health` responde HTTP 503 e identifica qué componente no está saludable sin revelar secretos.

### Requirement: Contrato de health documentado en OpenAPI
La definición OpenAPI SHALL describir `GET /health`, el significado de sus respuestas HTTP 200 y 503 y los estados agregados de PostgreSQL, Redis y RabbitMQ.

#### Scenario: Consultar el contrato de salud
- **WHEN** un consumidor abre la documentación OpenAPI
- **THEN** encuentra la operación `GET /health` con sus códigos de respuesta y los componentes comprobados.
