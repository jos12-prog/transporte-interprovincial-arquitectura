export {};

const baseUrl = (process.env.API_BASE_URL ?? 'http://127.0.0.1:8080').replace(/\/$/, '');

describe('identity-access HTTP contract', () => {
  const unique = `${Date.now()}-${Math.random().toString(16).slice(2)}`;
  const email = `passenger-${unique}@example.test`;
  const dni = String(10_000_000 + Math.floor(Math.random() * 89_999_999));
  const password = 'passenger-password';

  it('validates registration, creates a passenger, rejects duplicates, and publishes the Bearer contract', async () => {
    const invalid = await fetch(`${baseUrl}/api/v1/auth/register`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ fullName: 'Pasajero API', dni: '123', email, phoneNumber: '999111222', password }),
    });
    expect(invalid.status).toBe(400);
    expect(invalid.headers.get('content-type')).toContain('application/problem+json');

    const request = { fullName: 'Pasajero API', dni, email, phoneNumber: '999111222', password };
    const created = await fetch(`${baseUrl}/api/v1/auth/register`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(request),
    });
    expect(created.status).toBe(201);
    expect((await created.json()).userId).toEqual(expect.any(String));

    const duplicate = await fetch(`${baseUrl}/api/v1/auth/register`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(request),
    });
    expect(duplicate.status).toBe(409);
    expect((await duplicate.json()).status).toBe(409);

    const swaggerResponse = await fetch(`${baseUrl}/swagger/v1/swagger.json`);
    const swagger = await swaggerResponse.json();
    expect(swagger.components.securitySchemes.Bearer.scheme).toBe('bearer');
    expect(swagger.paths['/api/v1/auth/me'].get.security).toEqual(expect.arrayContaining([expect.objectContaining({ Bearer: [] })]));
    expect(swagger.paths['/api/v1/trips/search'].get.security).toBeUndefined();
  });

  it('issues the stable passenger role, supports /me, and rejects requests without a token', async () => {
    const login = await fetch(`${baseUrl}/api/v1/auth/login`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ identifier: email, password }),
    });
    expect(login.status).toBe(200);
    const session = await login.json();
    expect(session.user.role).toBe('passenger');
    expect(session.user.fullName).toBe('Pasajero API');
    expect(session.user.email).toBe(email.toLowerCase());
    expect(session.tokenType).toBe('Bearer');
    expect(new Date(session.expiresAtUtc).getTime()).toBeGreaterThan(Date.now());

    const me = await fetch(`${baseUrl}/api/v1/auth/me`, { headers: { Authorization: `Bearer ${session.accessToken}` } });
    expect(me.status).toBe(200);
    expect((await me.json()).role).toBe('passenger');
    expect((await fetch(`${baseUrl}/api/v1/auth/me`)).status).toBe(401);
    expect((await fetch(`${baseUrl}/api/v1/staff/agencies`, { headers: { Authorization: `Bearer ${session.accessToken}` } })).status).toBe(403);
  });

  it('returns only assigned agencies and denies an agency outside the agent token scope', async () => {
    const agentPassword = process.env.IDENTITY_SEED_AGENT_PASSWORD ?? 'CI-agent-password-2026';
    const login = await fetch(`${baseUrl}/api/v1/auth/login`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ identifier: 'agente@andesexpress.test', password: agentPassword }),
    });
    expect(login.status).toBe(200);
    const session = await login.json();
    expect(session.user.role).toBe('agent');
    expect(session.user.fullName).toEqual(expect.any(String));
    expect(session.user.email).toBe('agente@andesexpress.test');
    const headers = { Authorization: `Bearer ${session.accessToken}` };

    const agencies = await fetch(`${baseUrl}/api/v1/staff/agencies`, { headers });
    expect(agencies.status).toBe(200);
    const assigned = await agencies.json();
    expect(assigned.length).toBeGreaterThan(0);
    const inScope = await fetch(`${baseUrl}/api/v1/staff/agencies/${assigned[0].agencyId}`, { headers });
    expect(inScope.status).toBe(200);
    const outOfScope = await fetch(`${baseUrl}/api/v1/staff/agencies/${crypto.randomUUID()}`, { headers });
    expect(outOfScope.status).toBe(403);
    expect((await outOfScope.json()).status).toBe(403);
  });

  it('blocks the sixth failed identifier attempt and the twenty-first request from one IP', async () => {
    const account = `locked-${unique}@example.test`;
    for (let attempt = 1; attempt <= 5; attempt += 1) {
      const response = await fetch(`${baseUrl}/api/v1/auth/login`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ identifier: account, password: 'wrong-password' }),
      });
      expect(response.status).toBe(401);
    }
    const accountBlocked = await fetch(`${baseUrl}/api/v1/auth/login`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ identifier: account, password: 'wrong-password' }),
    });
    expect(accountBlocked.status).toBe(429);
    expect(accountBlocked.headers.get('retry-after')).toBeTruthy();
    expect((await accountBlocked.json()).status).toBe(429);

    for (let requestNumber = 9; requestNumber <= 20; requestNumber += 1) {
      const response = await fetch(`${baseUrl}/api/v1/auth/login`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ identifier: `ip-${unique}-${requestNumber}@example.test`, password: 'wrong-password' }),
      });
      expect(response.status).toBe(401);
    }
    const ipBlocked = await fetch(`${baseUrl}/api/v1/auth/login`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ identifier: `over-ip-${unique}@example.test`, password: 'wrong-password' }),
    });
    expect(ipBlocked.status).toBe(429);
    expect(ipBlocked.headers.get('retry-after')).toBeTruthy();
    expect((await ipBlocked.json()).status).toBe(429);
  });
});
