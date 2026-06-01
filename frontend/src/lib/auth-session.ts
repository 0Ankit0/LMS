import axios from 'axios';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000/api/v1';
const ACCESS_TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';
const REFRESH_SKEW_MS = 30_000;

interface RefreshResponse {
  access: string;
  refresh: string;
}

let refreshPromise: Promise<string | null> | null = null;

function isBrowser() {
  return typeof window !== 'undefined';
}

function decodeBase64Url(value: string) {
  const normalized = value.replace(/-/g, '+').replace(/_/g, '/');
  const padding = normalized.length % 4 === 0 ? '' : '='.repeat(4 - (normalized.length % 4));
  return atob(`${normalized}${padding}`);
}

function parseTokenExpiry(token: string) {
  try {
    const [, payload] = token.split('.');
    if (!payload) {
      return null;
    }
    const parsed = JSON.parse(decodeBase64Url(payload)) as { exp?: number };
    return typeof parsed.exp === 'number' ? parsed.exp * 1000 : null;
  } catch {
    return null;
  }
}

export function getAccessToken() {
  if (!isBrowser()) {
    return null;
  }
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function getRefreshToken() {
  if (!isBrowser()) {
    return null;
  }
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function hasStoredSessionTokens() {
  return Boolean(getAccessToken() || getRefreshToken());
}

export function setStoredAuthTokens(access: string, refresh: string) {
  if (!isBrowser()) {
    return;
  }
  localStorage.setItem(ACCESS_TOKEN_KEY, access);
  localStorage.setItem(REFRESH_TOKEN_KEY, refresh);
}

export function clearStoredAuthTokens() {
  if (!isBrowser()) {
    return;
  }
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

export function isTokenExpired(token: string, skewMs = REFRESH_SKEW_MS) {
  const expiresAt = parseTokenExpiry(token);
  if (!expiresAt) {
    return true;
  }
  return expiresAt <= Date.now() + skewMs;
}

export async function refreshStoredAccessToken() {
  const refreshToken = getRefreshToken();
  if (!refreshToken) {
    return null;
  }

  if (!refreshPromise) {
    refreshPromise = axios
      .post<RefreshResponse>(
        `${API_BASE_URL}/auth/refresh/`,
        { refresh_token: refreshToken },
        { params: { set_cookie: false } }
      )
      .then(({ data }) => {
        setStoredAuthTokens(data.access, data.refresh);
        return data.access;
      })
      .catch((error) => {
        clearStoredAuthTokens();
        throw error;
      })
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
}

export async function ensureValidAccessToken() {
  const accessToken = getAccessToken();
  if (accessToken && !isTokenExpired(accessToken)) {
    return accessToken;
  }

  if (!getRefreshToken()) {
    if (accessToken) {
      clearStoredAuthTokens();
    }
    return null;
  }

  try {
    return await refreshStoredAccessToken();
  } catch {
    return null;
  }
}

export function redirectToLogin() {
  if (!isBrowser()) {
    return;
  }
  if (window.location.pathname !== '/login') {
    window.location.href = '/login';
  }
}
