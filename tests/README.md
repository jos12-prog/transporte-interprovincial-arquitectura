# Pruebas .NET y cobertura

Los proyectos xUnit están en `Unit`, `Architecture` e `Integration`. Se puede generar Cobertura con `dotnet test --collect:"XPlat Code Coverage"`; ReportGenerator produce el informe HTML a partir de esos archivos Cobertura.

El manifiesto local `.config/dotnet-tools.json` instala ReportGenerator. Desde la raíz, ejecutar `dotnet tool restore` y después `dotnet tool run reportgenerator -reports:"tests/**/TestResults/**/coverage.cobertura.xml" -targetdir:"artifacts/coverage/backend" -reporttypes:Html`.
