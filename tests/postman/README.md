# Colección Postman del Sprint 2

La colección `RutaAndina-Sprint2.postman_collection.json` contiene comprobaciones de salud, búsqueda, mapa y holds, además de registro/login, perfil, autorización por agencia y límites de login. Las pruebas leen/escriben variables durante la corrida (`serviceDate`, IDs de viaje/asiento/hold, `passengerEmail`, `passengerDni`, `accessToken`, `agentToken`, `agencyId` y `otherAgencyId`).

## Postman

1. Copia `.env.example` a `.env`, completa las contraseñas de las tres cuentas de Development y genera las claves con `./scripts/generate-dev-jwt-keys.ps1` (PowerShell) o `bash scripts/generate-dev-jwt-keys.sh`.
2. Arranca el stack según los pasos de Docker Compose de la raíz del repositorio y espera que `GET http://localhost:8080/health` responda `healthy`.
3. En Postman, importa el archivo JSON de esta carpeta.
4. Conserva `baseUrl=http://localhost:8080` y define `agentPassword` con el valor usado en `IDENTITY_SEED_AGENT_PASSWORD` del `.env`.
5. Abre la colección y selecciona **Run**. Ejecuta los requests en orden; la búsqueda y el mapa preparan los requests de hold, y los requests de identidad preparan los tokens y agencias.
6. Ejecuta la carpeta **Límites de login** en orden y contra una instancia recién iniciada (o espera un minuto antes de ejecutarla otra vez): comprueba cinco fallos de identificador y las solicitudes 20/21 por IP.
7. Revisa que todas las aserciones pasen. La búsqueda puede devolver `X-Cache: HIT` o `MISS`; ambas respuestas son válidas.

## Newman

Desde la raíz del repositorio, con Node.js/npm disponibles y la API del Compose arriba. El valor de `agentPassword` debe ser el mismo que Compose usa para sembrar la cuenta de agente:

```powershell
npx newman run tests/postman/RutaAndina-Sprint2.postman_collection.json --env-var "agentPassword=$env:IDENTITY_SEED_AGENT_PASSWORD" --reporters cli,json --reporter-json-export docs/evidencias/postman-sprint2.json
```

El comando devuelve código distinto de cero si falla un request o una aserción. El reporte JSON se guarda en `docs/evidencias/postman-sprint2.json`.
