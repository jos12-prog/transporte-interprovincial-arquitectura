namespace TransportPlatform.Modules.CompaniesAgencies.Application.UseCases;

public sealed class CatalogValidationException(string message) : Exception(message);
public sealed class CatalogNotFoundException(string message) : Exception(message);
public sealed class CatalogConflictException(string message) : Exception(message);
