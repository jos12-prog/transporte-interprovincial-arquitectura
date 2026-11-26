using System.Data;
using System.Data.Common;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Host;

/// <summary>Uses MI03's lifecycle contract to lock and validate a trip in MI04's open transaction.</summary>
public sealed class EfTripReservationGuard : ITripReservationGuard
{
    public async Task<bool> LockActiveTripAsync(Guid tripId, DbTransaction transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        var connection = transaction.Connection ?? throw new InvalidOperationException("The reservation transaction has no database connection.");
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT status FROM transport_scheduling.trips WHERE id = @trip_id FOR UPDATE";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "trip_id";
        parameter.Value = tripId;
        command.Parameters.Add(parameter);
        var status = await command.ExecuteScalarAsync(cancellationToken);
        return string.Equals(Convert.ToString(status), "Scheduled", StringComparison.Ordinal);
    }
}
