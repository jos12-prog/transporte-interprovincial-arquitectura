# Evidencia de concurrencia EQ-02

## Ejecución registrada

- Fecha: 2026-11-22.
- Entorno: Windows/PowerShell, Docker Compose local; API en `http://localhost:8080`.
- k6: la versión no aparece en la salida de esta ejecución (anteriormente se informó k6 v2.4.0).
- Perfil: 50 VUs, 50 iteraciones, una petición de retención por VU sobre el mismo viaje y asiento.
- Viaje probado: `acb4325a-5e35-111d-5a7a-340331c2fe10`.
- Asiento probado: `de231772-6144-28e8-9515-9afbd7662f1a`.
- Resultado: 1 HTTP 200, 49 HTTP 409, 0 respuestas inesperadas.
- Checks: 51/51 aprobados, incluidos la creación del hold, conflictos concurrentes y limpieza del ganador por HTTP 204.
- HTTP: 53 solicitudes; `http_req_failed` 0/53; duración media 24.17 ms y p95 32.72 ms.
- Summary export: [`concurrencia-eq02-summary.json`](concurrencia-eq02-summary.json).
- Hardware (CPU/RAM): no informado.

Comando ejecutado desde la raíz del repositorio:

```powershell
$env:API_BASE_URL='http://localhost:8080'
$env:SEARCH_ORIGIN='Ayacucho'
$env:SEARCH_DESTINATION='Lima'
k6 run --summary-export docs/evidencias/concurrencia-eq02-summary.json tests/performance/seat-hold-concurrency.js
```
