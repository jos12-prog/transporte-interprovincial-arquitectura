
# 04. Atributos de Calidad

## 1. Descripción

Los atributos de calidad establecen las características no funcionales que debe cumplir la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

Orientan el diseño arquitectónico para garantizar operaciones confiables, seguras y eficientes en un entorno con múltiples empresas, agencias, pasajeros y servicios de encomiendas.

## 2. Atributos de calidad

| ID | Atributo | Descripción | Prioridad |
|---|---|---|---|
| AC01 | Consistencia e integridad | Evitar reservas duplicadas, sobreventa de cupos e inconsistencias en pagos y encomiendas. | Crítica |
| AC02 | Seguridad | Proteger los datos y restringir las operaciones según rol, empresa y agencia. | Alta |
| AC03 | Rendimiento | Ofrecer respuestas eficientes en búsquedas, disponibilidad y operaciones frecuentes. | Alta |
| AC04 | Disponibilidad | Mantener accesibles las funciones esenciales durante los periodos de operación. | Alta |
| AC05 | Escalabilidad | Soportar el crecimiento de empresas, agencias, usuarios, viajes y transacciones. | Alta |
| AC06 | Mantenibilidad | Facilitar modificaciones y nuevas funcionalidades sin afectar innecesariamente otros módulos. | Alta |
| AC07 | Auditabilidad | Registrar y consultar la trazabilidad de operaciones críticas. | Alta |
| AC08 | Recuperabilidad | Permitir restaurar información y servicios ante fallos. | Alta |
| AC09 | Interoperabilidad | Facilitar la integración con proveedores externos de pagos y notificaciones. | Media |

## 3. Requisitos de calidad

### AC01 — Consistencia e integridad

- Impedir que dos reservas confirmadas ocupen el mismo asiento de un viaje.
- Evitar que las reservas superen la capacidad disponible de cupos.
- Mantener la validez y el vencimiento de las reservas temporales.
- Evitar confirmaciones duplicadas de pagos.
- Mantener estados coherentes de reservas y encomiendas.

**Relación:** RF05-RF11, RF14, RF19, RF34, RF36-RF39.

### AC02 — Seguridad

- Autenticar a los usuarios que acceden a funciones privadas.
- Aplicar autorización según rol y ámbito organizacional.
- Impedir el acceso no autorizado a información de otras empresas.
- Proteger los datos personales de pasajeros, remitentes y destinatarios.
- Proteger las comunicaciones y operaciones sensibles.

**Relación:** RF20-RF23, RF35, RF40-RF44.

### AC03 — Rendimiento

- Optimizar la búsqueda de empresas, rutas, horarios y tarifas.
- Consultar disponibilidad sin generar carga innecesaria.
- Mantener tiempos de respuesta adecuados ante consultas concurrentes.
- Evitar que las tareas secundarias bloqueen operaciones críticas.

**Relación:** RF01-RF04, RF07-RF08, RF30-RF31.

### AC04 — Disponibilidad

- Mantener operativas las funciones esenciales de consulta y reserva.
- Detectar fallos de componentes y servicios.
- Reducir interrupciones durante periodos de alta demanda.
- Manejar fallos de proveedores externos sin comprometer la integridad de las operaciones.

**Relación:** RF01-RF19, RF30-RF39.

### AC05 — Escalabilidad

- Permitir incorporar nuevas empresas y agencias.
- Soportar el crecimiento de servicios programados y encomiendas.
- Permitir aumentar la capacidad de procesamiento sin rediseñar la lógica de negocio.
- Evitar dependencias innecesarias entre módulos.

**Relación:** RF01-RF04, RF20-RF29, RF30-RF39.

### AC06 — Mantenibilidad

- Separar las responsabilidades de administración, reservas, pagos, encomiendas y reportes.
- Mantener reglas de negocio independientes de interfaces y proveedores externos.
- Facilitar pruebas automatizadas y modificaciones controladas.
- Permitir incorporar nuevas modalidades de transporte y políticas empresariales.

**Relación:** RF05-RF48.

### AC07 — Auditabilidad

- Registrar operaciones críticas con fecha, hora y responsable cuando corresponda.
- Mantener historial de modificaciones de reservas y servicios programados.
- Registrar cambios de estado de encomiendas.
- Facilitar la consulta autorizada de eventos de auditoría.

**Relación:** RF17, RF29, RF39, RF44, RF46.

### AC08 — Recuperabilidad

- Realizar respaldos de la información persistente.
- Disponer de procedimientos de restauración.
- Verificar la integridad de los datos recuperados.
- Evitar la pérdida o duplicación de operaciones críticas durante la recuperación.

**Relación:** RF05-RF19, RF30-RF39.

### AC09 — Interoperabilidad

- Integrar pasarelas de pago mediante interfaces definidas.
- Procesar y verificar respuestas de servicios externos.
- Integrar proveedores de notificaciones sin acoplarlos directamente a las reglas de negocio.
- Facilitar el reemplazo de proveedores cuando sea necesario.

**Relación:** RF13, RF14, RF19, RF47, RF48.

## 4. Priorización

El atributo AC01 — Consistencia e integridad tiene prioridad crítica debido al riesgo de sobreventa durante reservas simultáneas.

Los atributos AC02-AC08 tienen prioridad alta porque influyen en la protección, operación y evolución del sistema.

AC09 tiene prioridad media, aunque la integración de pagos necesaria para confirmar reservas debe funcionar correctamente desde su implementación.

## 5. Verificación

Cada atributo será desarrollado mediante escenarios medibles en:

`08-escenarios-atributos-calidad.md`

Los escenarios definirán estímulo, entorno, respuesta esperada y medida de respuesta.

Las metas cuantitativas de rendimiento, disponibilidad, concurrencia y recuperación se establecerán como objetivos verificables antes de realizar pruebas de calidad.
