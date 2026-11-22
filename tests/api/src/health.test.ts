interface HealthResponse {
  status: string;
  components: Record<string, string>;
}

describe('real API readiness endpoint', () => {
  const baseUrl = (process.env.API_BASE_URL ?? 'http://127.0.0.1:8080').replace(/\/$/, '');

  it('reports healthy PostgreSQL, Redis, and RabbitMQ without exposing connection details', async () => {
    const response = await fetch(`${baseUrl}/health`);
    const bodyText = await response.text();
    const body = JSON.parse(bodyText) as HealthResponse;

    expect(response.status).toBe(200);
    expect(body.status).toBe('healthy');
    expect(body.components).toEqual({
      postgresql: 'healthy',
      redis: 'healthy',
      rabbitmq: 'healthy',
    });
    expect(bodyText).not.toMatch(/password|connection string|host=/i);
  });
});
