import http from 'k6/http';
import { check, fail } from 'k6';

const baseUrl = (__ENV.BASE_URL || 'http://localhost:8080').replace(/\/$/, '');

export const options = {
  vus: 1,
  iterations: 1,
};

export default function () {
  const response = http.get(`${baseUrl}/health`);
  const healthy = check(response, {
    'GET /health responds HTTP 200': (result) => result.status === 200,
  });

  if (!healthy) {
    fail(`GET ${baseUrl}/health returned HTTP ${response.status}; expected 200`);
  }
}
