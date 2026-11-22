# Pruebas de API

Este paquete Jest + TypeScript llama por HTTP al Host real. Desde la raíz del repositorio, crea el lockfile y luego instala dependencias:

```powershell
npm --prefix tests/api install --package-lock-only --ignore-scripts
npm --prefix tests/api ci
```

Con el stack levantado y saludable, ejecuta:

```powershell
$env:API_BASE_URL = 'http://localhost:8080'
npm --prefix tests/api run test -- --coverage
```

Jest escribe los reportes Cobertura, LCOV y HTML en `tests/api/coverage/`.
