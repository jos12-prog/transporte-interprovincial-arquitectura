# 00. Necesidad del Negocio

## 1. ¿Qué problema queremos resolver?

El transporte interprovincial terrestre en la región de Ayacucho y sus conexiones interregionales (hacia Lima, Ica, Huancayo, Cusco, Apurímac, entre otros) presenta deficiencias estructurales en sus canales de atención y operación:

1. **Venta fragmentada y presencial:** Los pasajeros se ven obligados a desplazarse físicamente a terminales terrestres o agencias dispersas para consultar horarios, precios y disponibilidad, generando congestión, colas y pérdidas de tiempo.
2. **Falta de visibilidad de la oferta:** No existe una plataforma centralizada que permita a los usuarios comparar itinerarios, tarifas y tipos de servicio entre múltiples empresas de transporte.
3. **Riesgo crítico de sobreventa (Overbooking):** En temporadas de alta demanda (Semana Santa, Fiestas Patrias, Carnavales, festividades patronales), la asignación simultánea de un mismo asiento o cupo en ventanillas distintas genera conflictos operativos, reclamos y sanciones ante INDECOPI y SUTRAN.
4. **Ausencia de medios de pago digitales integrados:** Las ventas en agencias dependen mayoritariamente del efectivo, limitando el acceso de usuarios que prefieren transacciones electrónicas seguras.
5. **Gestión manual y opaca de encomiendas:** El servicio de carga y encomiendas se registra en talonarios físicos o libros de control manuales, imposibilitando el seguimiento en tiempo real para remitentes y destinatarios y aumentando el riesgo de extravíos.
6. **Aislamiento administrativo de las empresas:** Cada empresa y agencia opera con procesos heterogéneos, sin herramientas digitales para planificar frecuencias, gestionar flotas o auditar ingresos en tiempo real.

---

## 2. Objetivos del Negocio que Guían el Sistema

Para solucionar la problemática identificada, la plataforma persigue los siguientes objetivos estratégicos de negocio:

* **OBJ01 — Centralizar la oferta de servicios:** Consolidar en un único canal digital la oferta de viajes, horarios, tarifas y servicios de múltiples empresas de transporte interprovincial.
* **OBJ02 — Habilitar reservas y ventas digitales 24/7:** Permitir a los pasajeros buscar, reservar y comprar pasajes en cualquier momento desde aplicaciones web o móviles con pasarelas de pago seguras.
* **OBJ03 — Garantizar cero sobreventas:** Asegurar consistencia absoluta e inmediata en la retención y venta de asientos y cupos, protegiendo la reputación comercial de las empresas asociadas.
* **OBJ04 — Digitalizar la trazabilidad de encomiendas:** Proporcionar un código único de seguimiento para cada envío, informando estados en tiempo real desde la recepción hasta la entrega.
* **OBJ05 — Fortalecer la gestión operativa de las empresas:** Dotar a las empresas y agencias de herramientas de administración de flota, programación de viajes, control de agentes y reportes financieros.

---

## 3. Resultados Esperados

```mermaid
flowchart LR
    subgraph PROBLEMA["Problemática Actual"]
        P1["Colas y compra presencial"]
        P2["Sobreventa en festividades"]
        P3["Sin seguimiento de encomiendas"]
        P4["Múltiples terminales dispersos"]
    end

    subgraph SISTEMA["Plataforma Centralizada"]
        S["Reserva Online + Encomiendas<br/>+ Gestión Multiempresa"]
    end

    subgraph RESULTADO["Resultados del Negocio"]
        R1["Mayor alcance e ingresos comerciales"]
        R2["Satisfacción y fidelización del cliente"]
        R3["Cero reclamos por sobreventa"]
        R4["Control operativo y auditoría 24/7"]
    end

    PROBLEMA --> SISTEMA --> RESULTADO

    classDef prob fill:#FFEBEE,stroke:#E57373,color:#B71C1C
    classDef sis fill:#E3F2FD,stroke:#64B5F6,color:#0D47A1
    classDef res fill:#E8F5E9,stroke:#81C784,color:#1B5E20
    class PROBLEMA,P1,P2,P3,P4 prob
    class SISTEMA,S sis
    class RESULTADO,R1,R2,R3,R4 res
```

* **Mayor alcance y ventas:** Crecimiento en el volumen de pasajes vendidos al captar usuarios fuera de ventanilla.
* **Reducción del abandono de compra:** Proceso fluido de selección de asiento y pago electrónico verificado.
* **Eficiencia operativa regional:** Reducción de tiempos de atención en counter para ventas físicas y despachos de encomiendas.
* **Confianza institucional:** Cumplimiento de normativas de transporte y protección al consumidor mediante comprobantes digitales y auditoría inmutable.
