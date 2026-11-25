const baseUrl = (process.env.API_BASE_URL ?? 'http://127.0.0.1:8080').replace(/\/$/, '');

describe('platform administration HTTP contract', () => {
  it('protects and manages companies, agencies, and vehicle models', async () => {
    const superAdminPassword = process.env.IDENTITY_SEED_SUPER_ADMIN_PASSWORD ?? 'CI-superadmin-password-2026';
    const login = await fetch(`${baseUrl}/api/v1/auth/login`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ identifier: 'superadmin@rutaandina.test', password: superAdminPassword }),
    });
    expect(login.status).toBe(200);
    const superAdmin = await login.json() as { accessToken: string; user: { role: string } };
    expect(superAdmin.user.role).toBe('super_admin');
    const headers = { Authorization: `Bearer ${superAdmin.accessToken}`, 'Content-Type': 'application/json' };

    const noToken = await fetch(`${baseUrl}/api/v1/admin/companies`);
    expect(noToken.status).toBe(401);
    expect(noToken.headers.get('content-type')).toContain('application/problem+json');
    const agentLogin = await fetch(`${baseUrl}/api/v1/auth/login`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ identifier: 'agente@andesexpress.test', password: process.env.IDENTITY_SEED_AGENT_PASSWORD ?? 'CI-agent-password-2026' }),
    });
    expect(agentLogin.status).toBe(200);
    const agent = await agentLogin.json() as { accessToken: string };
    const forbidden = await fetch(`${baseUrl}/api/v1/admin/companies`, { headers: { Authorization: `Bearer ${agent.accessToken}` } });
    expect(forbidden.status).toBe(403);
    expect(forbidden.headers.get('content-type')).toContain('application/problem+json');

    const companies = await fetch(`${baseUrl}/api/v1/admin/companies`, { headers });
    expect(companies.status).toBe(200);
    expect((await companies.json()).length).toBeGreaterThan(0);

    const suffix = `${Date.now()}${Math.floor(Math.random() * 1000)}`.slice(-11).padStart(11, '1');
    const companyResponse = await fetch(`${baseUrl}/api/v1/admin/companies`, {
      method: 'POST', headers, body: JSON.stringify({ name: `[Prueba] Empresa API plataforma ${suffix}`, taxId: suffix, contactName: null, contactPhone: null }),
    });
    expect(companyResponse.status).toBe(201);
    const company = await companyResponse.json() as { companyId: string; isActive: boolean };
    expect(company.isActive).toBe(true);
    const inactiveCompany = await fetch(`${baseUrl}/api/v1/admin/companies/${company.companyId}/status`, {
      method: 'PATCH', headers, body: JSON.stringify({ isActive: false }),
    });
    expect(inactiveCompany.status).toBe(200);
    expect((await inactiveCompany.json()).isActive).toBe(false);
    await fetch(`${baseUrl}/api/v1/admin/companies/${company.companyId}/status`, {
      method: 'PATCH', headers, body: JSON.stringify({ isActive: true }),
    });

    const agencyResponse = await fetch(`${baseUrl}/api/v1/admin/companies/${company.companyId}/agencies`, {
      method: 'POST', headers, body: JSON.stringify({ name: `[Prueba] Agencia ${suffix}`, city: 'Ayacucho', address: null, phone: null }),
    });
    expect(agencyResponse.status).toBe(201);
    const agency = await agencyResponse.json() as { agencyId: string; isActive: boolean };
    expect(agency.isActive).toBe(true);
    const inactiveAgency = await fetch(`${baseUrl}/api/v1/admin/agencies/${agency.agencyId}/status`, {
      method: 'PATCH', headers, body: JSON.stringify({ isActive: false }),
    });
    expect(inactiveAgency.status).toBe(200);
    expect((await inactiveAgency.json()).isActive).toBe(false);

    const modelResponse = await fetch(`${baseUrl}/api/v1/admin/vehicle-models`, {
      method: 'POST', headers, body: JSON.stringify({ name: `[Prueba] Modelo ${suffix}`, category: 'Familiar', seats: [
        { number: 1, floor: 1, row: 1, column: 1 }, { number: 2, floor: 1, row: 1, column: 2 },
      ] }),
    });
    expect(modelResponse.status).toBe(201);
    const model = await modelResponse.json() as { vehicleModelId: string; capacity: number };
    expect(model.capacity).toBe(2);
    const deactivatedModel = await fetch(`${baseUrl}/api/v1/admin/vehicle-models/${model.vehicleModelId}/status`, {
      method: 'PATCH', headers, body: JSON.stringify({ isActive: false }),
    });
    expect(deactivatedModel.status).toBe(200);
    expect((await deactivatedModel.json()).isActive).toBe(false);

    const finalCompanyDeactivation = await fetch(`${baseUrl}/api/v1/admin/companies/${company.companyId}/status`, {
      method: 'PATCH', headers, body: JSON.stringify({ isActive: false }),
    });
    expect(finalCompanyDeactivation.status).toBe(200);
    expect((await finalCompanyDeactivation.json()).isActive).toBe(false);

    const invalidCompany = await fetch(`${baseUrl}/api/v1/admin/companies`, {
      method: 'POST', headers, body: JSON.stringify({ name: '', taxId: 'bad', contactName: null, contactPhone: null }),
    });
    expect(invalidCompany.status).toBe(400);
    expect(invalidCompany.headers.get('content-type')).toContain('application/problem+json');

    const swaggerResponse = await fetch(`${baseUrl}/swagger/v1/swagger.json`);
    const swagger = await swaggerResponse.json() as { paths: Record<string, unknown> };
    expect(swagger.paths).toHaveProperty('/api/v1/admin/companies');
    expect(swagger.paths).toHaveProperty('/api/v1/admin/companies/{companyId}/agencies');
    expect(swagger.paths).toHaveProperty('/api/v1/admin/vehicle-models');
  }, 30_000);
});
