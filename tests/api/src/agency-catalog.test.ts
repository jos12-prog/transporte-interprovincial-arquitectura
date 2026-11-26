export {};

const baseUrl = (process.env.API_BASE_URL ?? 'http://127.0.0.1:8080').replace(/\/$/, '');
const jsonHeaders = (token?: string) => ({ 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) });

describe('agency catalog HTTP contract', () => {
  it('scopes catalog, copies the active fare into trips, rejects vehicle overlap, and cancels without deletion', async () => {
    const login = await fetch(`${baseUrl}/api/v1/auth/login`, { method: 'POST', headers: jsonHeaders(), body: JSON.stringify({
      identifier: 'admin.agencia@andesexpress.test', password: process.env.IDENTITY_SEED_AGENCY_ADMIN_PASSWORD ?? 'CI-agencyadmin-password-2026',
    }) });
    expect(login.status).toBe(200);
    const admin = await login.json() as { accessToken: string };
    const headers = jsonHeaders(admin.accessToken);
    const agenciesResponse = await fetch(`${baseUrl}/api/v1/staff/agencies`, { headers });
    expect(agenciesResponse.status).toBe(200);
    const agencies = await agenciesResponse.json() as Array<{ agencyId: string }>;
    const agencyId = agencies[0].agencyId;

    const agentLogin = await fetch(`${baseUrl}/api/v1/auth/login`, { method: 'POST', headers: jsonHeaders(), body: JSON.stringify({
      identifier: 'agente@andesexpress.test', password: process.env.IDENTITY_SEED_AGENT_PASSWORD ?? 'CI-agent-password-2026',
    }) });
    const agent = await agentLogin.json() as { accessToken: string };
    const agentDenied = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/routes`, { headers: jsonHeaders(agent.accessToken) });
    expect(agentDenied.status).toBe(403);
    expect(agentDenied.headers.get('content-type')).toContain('application/problem+json');

    const notFoundScope = await fetch(`${baseUrl}/api/v1/admin/agencies/00000000-0000-0000-0000-000000000000/routes`, { headers });
    expect(notFoundScope.status).toBe(403);

    const suffix = `${Date.now()}-${Math.floor(Math.random() * 10000)}`;
    const routeResponse = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/routes`, { method: 'POST', headers, body: JSON.stringify({
      originCity: `[Prueba] Origen ${suffix}`, destinationCity: `[Prueba] Destino ${suffix}`,
    }) });
    expect(routeResponse.status).toBe(201);
    const route = await routeResponse.json() as { routeId: string };
    const from = new Date(Date.now() - 86_400_000).toISOString().slice(0, 10);
    const fareResponse = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/routes/${route.routeId}/fares`, { method: 'POST', headers, body: JSON.stringify({ type: 'Base', validFrom: from, validTo: null, price: 82, currency: 'PEN' }) });
    expect(fareResponse.status).toBe(201);
    const fare = await fareResponse.json() as { fareId: string };

    const modelsResponse = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/vehicle-models`, { headers });
    const models = await modelsResponse.json() as Array<{ vehicleModelId: string; isActive: boolean }>;
    const model = models.find(value => value.isActive)!;
    const vehicleResponse = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/vehicles`, { method: 'POST', headers, body: JSON.stringify({ plate: `TST${Date.now()}`.slice(-10), displayName: `[Prueba] Avanza ${suffix}`, vehicleModelId: model.vehicleModelId }) });
    expect(vehicleResponse.status).toBe(201);
    const vehicle = await vehicleResponse.json() as { vehicleId: string; displayName: string };

    const departure = new Date(Date.now() + 2 * 86_400_000);
    const schedule = { routeId: route.routeId, vehicleId: vehicle.vehicleId, departureAtUtc: departure.toISOString(), arrivalAtUtc: new Date(departure.getTime() + 9 * 3_600_000).toISOString() };
    const tripResponse = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/trips`, { method: 'POST', headers, body: JSON.stringify(schedule) });
    expect(tripResponse.status).toBe(201);
    const trip = await tripResponse.json() as { tripId: string; price: number; vehicleDisplayName: string };
    expect(trip.price).toBe(82);
    expect(trip.vehicleDisplayName).toContain('[Prueba]');

    const overlap = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/trips`, { method: 'POST', headers, body: JSON.stringify(schedule) });
    expect(overlap.status).toBe(409);
    expect(overlap.headers.get('content-type')).toContain('application/problem+json');
    const cancel = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/trips/${trip.tripId}/cancel`, { method: 'POST', headers });
    expect(cancel.status).toBe(200);
    expect((await cancel.json() as { status: string }).status).toBe('Cancelled');
    const deactivateVehicle = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/vehicles/${vehicle.vehicleId}/active`, { method: 'PATCH', headers, body: JSON.stringify({ isActive: false }) });
    const deactivateRoute = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/routes/${route.routeId}/active`, { method: 'PATCH', headers, body: JSON.stringify({ isActive: false }) });
    const deactivateFare = await fetch(`${baseUrl}/api/v1/admin/agencies/${agencyId}/routes/${route.routeId}/fares/${fare.fareId}/active`, { method: 'PATCH', headers, body: JSON.stringify({ isActive: false }) });
    expect([deactivateVehicle.status, deactivateRoute.status, deactivateFare.status]).toEqual([200, 200, 200]);
  }, 30_000);
});
