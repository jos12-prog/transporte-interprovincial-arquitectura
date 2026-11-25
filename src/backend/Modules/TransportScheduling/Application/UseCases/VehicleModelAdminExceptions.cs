namespace TransportPlatform.Modules.TransportScheduling.Application.UseCases;

public sealed class VehicleModelValidationException(string message) : Exception(message);
public sealed class VehicleModelNotFoundException(string message) : Exception(message);
public sealed class VehicleModelConflictException(string message) : Exception(message);
