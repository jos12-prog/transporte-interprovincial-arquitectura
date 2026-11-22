# Smoke test con k6

1. Desde la raíz del repositorio, inicia el stack: `docker compose up --build -d`.
2. Confirma que los servicios estén saludables con `docker compose ps`.
3. Ejecuta el smoke test con k6:

   ```powershell
   $env:BASE_URL = 'http://localhost:8080'
   k6 run tests/performance/smoke-health.js
   ```

El script solicita `GET /health` una vez y falla si la API no devuelve HTTP 200. Al terminar, detén el stack con `docker compose down`.
