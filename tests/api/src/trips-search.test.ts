interface TripSearchResult {
  tripId: string;
  companyId: string;
  companyName: string;
  agencyId: string;
  agencyName: string;
  agencyCity: string;
  origin: string;
  destination: string;
  departureAtUtc: string;
  arrivalAtUtc: string;
  vehicleModelName: string;
  price: number;
  currency: string;
  availableSeats: number;
}

function limaServiceDate(): string {
  const limaNow = new Date(Date.now() - 5 * 60 * 60 * 1000);
  return limaNow.toISOString().slice(0, 10);
}

describe('public trip-search API', () => {
  const baseUrl = (process.env.API_BASE_URL ?? 'http://127.0.0.1:8080').replace(/\/$/, '');

  it('returns public trip data and reports cache status', async () => {
    const query = new URLSearchParams({ origin: 'Ayacucho', destination: 'Lima', date: limaServiceDate() });
    const url = `${baseUrl}/api/v1/trips/search?${query}`;
    const firstResponse = await fetch(url);
    const firstResults = await firstResponse.json() as TripSearchResult[];

    expect(firstResponse.status).toBe(200);
    expect(['HIT', 'MISS']).toContain(firstResponse.headers.get('x-cache'));
    expect(firstResults.length).toBeGreaterThan(0);
    expect(firstResults[0]).toEqual(expect.objectContaining({
      companyName: expect.any(String),
      agencyName: expect.any(String),
      origin: 'Ayacucho',
      destination: 'Lima',
      departureAtUtc: expect.any(String),
      arrivalAtUtc: expect.any(String),
      vehicleModelName: expect.any(String),
      price: expect.any(Number),
      availableSeats: expect.any(Number),
    }));

    const repeatedResponse = await fetch(url);
    expect(repeatedResponse.status).toBe(200);
    expect(repeatedResponse.headers.get('x-cache')).toBe('HIT');
    expect(await repeatedResponse.json()).toEqual(firstResults);
  });

  it('returns a ProblemDetails validation response for an invalid service date', async () => {
    const query = new URLSearchParams({ origin: 'Ayacucho', destination: 'Lima', date: 'not-a-date' });
    const response = await fetch(`${baseUrl}/api/v1/trips/search?${query}`);
    const problem = await response.json() as { status?: number; errors?: Record<string, string[]> };

    expect(response.status).toBe(400);
    expect(problem.status).toBe(400);
    expect(problem.errors).toHaveProperty('date');
  });

  it('returns an empty list for a route with no scheduled service', async () => {
    const query = new URLSearchParams({ origin: 'Ayacucho', destination: 'NoSuchCity', date: limaServiceDate() });
    const response = await fetch(`${baseUrl}/api/v1/trips/search?${query}`);

    expect(response.status).toBe(200);
    expect(await response.json()).toEqual([]);
  });

  it('publishes the search route and response contracts in Swagger', async () => {
    const response = await fetch(`${baseUrl}/swagger/v1/swagger.json`);
    const document = await response.json() as {
      paths?: Record<string, { get?: { parameters?: Array<{ name: string }>; responses?: Record<string, unknown> } }>;
    };
    const operation = document.paths?.['/api/v1/trips/search']?.get;

    expect(response.status).toBe(200);
    expect(operation).toBeDefined();
    expect(operation?.parameters?.map(parameter => parameter.name)).toEqual(
      expect.arrayContaining(['origin', 'destination', 'date', 'companyId', 'agencyId', 'minPrice', 'maxPrice']));
    expect(operation?.responses).toEqual(expect.objectContaining({
      '200': expect.any(Object),
      '400': expect.any(Object),
      '500': expect.any(Object),
    }));
  });
});
