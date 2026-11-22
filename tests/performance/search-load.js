import http from 'k6/http';
import { check } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';

const acceptanceProfile = 'acceptance-200-vus';
const exploratoryProfile = 'exploratory-to-2000-vus';
const redisMode = (__ENV.REDIS_MODE || 'enabled').toLowerCase();
const apiBaseUrl = (__ENV.API_BASE_URL || 'http://localhost:8080').replace(/\/$/, '');
const origin = __ENV.SEARCH_ORIGIN || 'Ayacucho';
const destination = __ENV.SEARCH_DESTINATION || 'Lima';
const serviceDate = __ENV.SEARCH_DATE || new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString().slice(0, 10);

const cacheHits = new Counter('search_cache_hit');
const cacheMisses = new Counter('search_cache_miss');
const cacheHitRate = new Rate('search_cache_hit_rate');
const searchLatency = new Trend('search_latency', true);

const thresholds = {
  [`http_req_duration{profile:${acceptanceProfile}}`]: ['p(95)<=2000'],
  [`http_req_failed{profile:${acceptanceProfile}}`]: ['rate<0.01'],
  [`checks{profile:${acceptanceProfile}}`]: ['rate>0.99'],
};

if (redisMode === 'enabled') {
  thresholds[`search_cache_hit_rate{profile:${acceptanceProfile},redis:enabled}`] = ['rate>=0.8'];
}

export const options = {
  thresholds,
  scenarios: {
    acceptance: {
      executor: 'constant-vus',
      vus: 200,
      duration: '45s',
      exec: 'acceptance',
      tags: { profile: acceptanceProfile },
    },
    exploratory: {
      executor: 'ramping-vus',
      startTime: '50s',
      startVUs: 0,
      stages: [
        { duration: '10s', target: 50 },
        { duration: '10s', target: 100 },
        { duration: '10s', target: 200 },
        { duration: '10s', target: 500 },
        { duration: '10s', target: 1000 },
        { duration: '10s', target: 1500 },
        { duration: '10s', target: 2000 },
        { duration: '15s', target: 2000 },
        { duration: '10s', target: 0 },
      ],
      exec: 'exploratory',
      tags: { profile: exploratoryProfile },
      gracefulRampDown: '10s',
    },
  },
};

function search(profile) {
  const url = `${apiBaseUrl}/api/v1/trips/search?origin=${encodeURIComponent(origin)}&destination=${encodeURIComponent(destination)}&date=${encodeURIComponent(serviceDate)}`;
  const tags = { profile, redis: redisMode };
  const response = http.get(url, { tags, timeout: '10s' });

  searchLatency.add(response.timings.duration, tags);
  const successful = check(response, {
    'search responds HTTP 200': (res) => res.status === 200,
    'search returns a JSON array': (res) => {
      try {
        return Array.isArray(res.json());
      } catch (_) {
        return false;
      }
    },
  }, { profile, redis: redisMode });

  const cacheHeader = response.headers['X-Cache'];
  if (cacheHeader === 'HIT') {
    cacheHits.add(1, tags);
    cacheHitRate.add(true, tags);
  } else if (cacheHeader === 'MISS') {
    cacheMisses.add(1, tags);
    cacheHitRate.add(false, tags);
  } else {
    cacheHitRate.add(false, tags);
  }

  check(response, {
    'search returns X-Cache HIT or MISS': () => cacheHeader === 'HIT' || cacheHeader === 'MISS',
  }, { profile, redis: redisMode });

  return successful;
}

export function acceptance() {
  search(acceptanceProfile);
}

export function exploratory() {
  search(exploratoryProfile);
}
