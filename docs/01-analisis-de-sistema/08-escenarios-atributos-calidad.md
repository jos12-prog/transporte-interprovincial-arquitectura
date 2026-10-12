# 08. Escenarios de Atributos de Calidad

## 1. Descripción

Los escenarios de calidad permiten evaluar cómo debe responder la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial ante situaciones que afectan su funcionamiento.

Cada escenario describe una situación concreta, la respuesta esperada y una medida verificable.

## 2. Escenarios de calidad

### EC01 — Consistencia de reservas simultáneas

**Atributo:** AC01 — Consistencia e integridad  
**Driver:** DA01 — Consistencia de reservas y pagos

| Elemento | Descripción |
|---|---|
| Fuente | Dos o más pasajeros o agentes. |
| Estímulo | Intentan reservar simultáneamente el último asiento o cupo disponible de un viaje. |
| Entorno | Sistema operativo con solicitudes concurrentes. |
| Artefacto | Módulo de reservas y base de datos. |
| Respuesta | El sistema confirma únicamente las reservas que respetan la disponibilidad y rechaza las restantes. |
| Medida | Cero asientos duplicados y cero sobreventas en las pruebas de concurrencia. |

### EC02 — Seguridad y aislamiento multiempresa

**Atributo:** AC02 — Seguridad  
**Driver:** DA02 — Seguridad y aislamiento multiempresa

| Elemento | Descripción |
|---|---|
| Fuente | Usuario autenticado de una empresa. |
| Estímulo | Intenta consultar o modificar información privada de otra empresa sin autorización. |
| Entorno | Operación normal del sistema. |
| Artefacto | API, autorización y acceso a datos. |
| Respuesta | El sistema deniega la operación y evita exponer información no autorizada. |
| Medida | El 100 % de las solicitudes no autorizadas evaluadas en las pruebas de aislamiento deben ser rechazadas. |

### EC03 — Rendimiento de búsqueda de viajes

**Atributo:** AC03 — Rendimiento  
**Driver:** DA03 — Rendimiento de consultas y disponibilidad

| Elemento | Descripción |
|---|---|
| Fuente | Pasajeros. |
| Estímulo | Realizan búsquedas por origen, destino y fecha. |
| Entorno | Carga concurrente definida para las pruebas. |
| Artefacto | API de búsqueda y base de datos. |
| Respuesta | El sistema muestra empresas, horarios, tarifas y disponibilidad correspondientes. |
| Medida | Meta inicial: al menos el 95 % de las consultas responde en 3 segundos o menos, bajo la carga de prueba acordada. |

### EC04 — Disponibilidad y recuperación

**Atributos:** AC04 — Disponibilidad; AC08 — Recuperabilidad  
**Driver:** DA04 — Disponibilidad y recuperación

| Elemento | Descripción |
|---|---|
| Fuente | Fallo de infraestructura o aplicación. |
| Estímulo | Se interrumpe el servicio o se requiere recuperar información. |
| Entorno | Operación normal o procedimiento de recuperación. |
| Artefacto | Backend, base de datos y respaldos. |
| Respuesta | Se detecta el fallo, se ejecuta el procedimiento de recuperación y se verifica la integridad de los datos. |
| Medida | Meta inicial: RTO máximo de 4 horas y RPO máximo de 1 hora, sujetos a validación según infraestructura y presupuesto. |

### EC05 — Escalabilidad de la plataforma

**Atributo:** AC05 — Escalabilidad  
**Driver:** DA05 — Escalabilidad del sistema

| Elemento | Descripción |
|---|---|
| Fuente | Crecimiento de usuarios y empresas. |
| Estímulo | Aumenta la cantidad de búsquedas y operaciones simultáneas. |
| Entorno | Pruebas de carga con una configuración inicial y otra ampliada. |
| Artefacto | Backend e infraestructura. |
| Respuesta | La solución permite aumentar la capacidad de procesamiento sin modificar las reglas de negocio. |
| Medida | Verificar que una configuración ampliada soporte mayor carga manteniendo los objetivos de rendimiento e integridad establecidos. |

### EC06 — Mantenibilidad y evolución modular

**Atributo:** AC06 — Mantenibilidad  
**Driver:** DA06 — Mantenibilidad y evolución modular

| Elemento | Descripción |
|---|---|
| Fuente | Equipo de desarrollo. |
| Estímulo | Se solicita modificar una política de encomiendas. |
| Entorno | Desarrollo y mantenimiento. |
| Artefacto | Módulos de negocio y pruebas. |
| Respuesta | La modificación se realiza principalmente en el módulo correspondiente, respetando sus interfaces. |
| Medida | Las pruebas automatizadas de los módulos afectados y las pruebas de regresión deben completarse satisfactoriamente. |

### EC07 — Integridad de confirmaciones de pago

**Atributo:** AC01 — Consistencia e integridad  
**Driver:** DA01 — Consistencia de reservas y pagos

| Elemento | Descripción |
|---|---|
| Fuente | Pasarela de pago externa. |
| Estímulo | Envía dos o más veces la misma confirmación de pago. |
| Entorno | Procesamiento normal de notificaciones externas. |
| Artefacto | Módulo de pagos y reservas. |
| Respuesta | El sistema verifica el pago y procesa su confirmación una sola vez. |
| Medida | Cero confirmaciones o registros de pago duplicados en las pruebas de idempotencia. |

### EC08 — Trazabilidad de encomiendas

**Atributo:** AC07 — Auditabilidad  
**Driver relacionado:** DA06 — Mantenibilidad y evolución modular

| Elemento | Descripción |
|---|---|
| Fuente | Agente autorizado. |
| Estímulo | Registra el despacho, llegada o entrega de una encomienda. |
| Entorno | Operación normal de una agencia. |
| Artefacto | Módulo de encomiendas y registros de auditoría. |
| Respuesta | El sistema actualiza el estado y registra fecha, responsable y datos de la operación. |
| Medida | El 100 % de los cambios de estado ejecutados durante las pruebas debe contar con su registro de trazabilidad. |

## 3. Matriz de trazabilidad

| Escenario | Atributo | Driver | Requisitos relacionados |
|---|---|---|---|
| EC01 | AC01 | DA01 | RF05-RF11 |
| EC02 | AC02 | DA02 | RF40-RF44 |
| EC03 | AC03 | DA03 | RF01-RF04 |
| EC04 | AC04, AC08 | DA04 | RF05-RF19, RF30-RF39 |
| EC05 | AC05 | DA05 | RF01-RF04, RF20-RF39 |
| EC06 | AC06 | DA06 | RF37-RF39 |
| EC07 | AC01 | DA01 | RF13, RF14, RF19 |
| EC08 | AC07 | DA06 | RF34-RF39 |

## 4. Validación de escenarios

Los escenarios serán evaluados mediante:

- Pruebas de concurrencia para reservas y pagos.
- Pruebas de autorización y aislamiento multiempresa.
- Pruebas de carga y rendimiento.
- Simulaciones de fallos y restauración.
- Pruebas de integración y regresión.
- Verificación de registros de auditoría.

Las metas iniciales deberán revisarse según el volumen esperado de operaciones, los recursos disponibles y los resultados de las pruebas.

## 5. Conclusión

Los escenarios EC01-EC08 permiten evaluar los principales riesgos arquitectónicos de la plataforma.

Se prioriza la consistencia de reservas, la seguridad multiempresa y la correcta confirmación de pagos, sin descuidar el rendimiento, la recuperación, la escalabilidad y la mantenibilidad.
