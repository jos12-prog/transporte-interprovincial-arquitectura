# Colección Postman del Sprint 2

La colección `RutaAndina-Sprint2.postman_collection.json` contiene comprobaciones de salud, búsqueda, validación de fecha, mapa de asientos, creación de hold, límite de 10 asientos, conflicto concurrente y liberación. Las pruebas leen/escriben variables de colección durante la corrida (`serviceDate`, `tripId`, `tripSeatId`, `elevenSeatIds`, `holdId`, `holdToken`).

## Postman

1. Arranca el stack según los pasos de Docker Compose de la raíz del repositorio y espera que `GET http://localhost:8080/health` responda `healthy`.
2. En Postman, importa el archivo JSON de esta carpeta.
3. Conserva `baseUrl=http://localhost:8080` o cámbialo si publicaste la API en otro puerto.
4. Abre la colección y selecciona **Run**. Ejecuta los requests en orden; la búsqueda selecciona un viaje y asientos del mapa para los requests posteriores.
5. Revisa que todas las aserciones de cada request pasen. La búsqueda puede devolver `X-Cache: HIT` o `MISS`; ambas respuestas son válidas.

## Newman

Desde la raíz del repositorio, con Node.js/npm disponibles y la API del Compose arriba:

```powershell
npx newman run tests/postman/RutaAndina-Sprint2.postman_collection.json --reporters cli,json --reporter-json-export docs/evidencias/postman-sprint2.json
```

El comando devuelve código distinto de cero si falla un request o una aserción. El reporte JSON se guarda en `docs/evidencias/postman-sprint2.json`.
