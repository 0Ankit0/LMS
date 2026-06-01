import { Buffer } from 'node:buffer';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

const { axiosPostMock } = vi.hoisted(() => ({
  axiosPostMock: vi.fn(),
}));

vi.mock('axios', () => ({
  default: {
    post: axiosPostMock,
  },
}));

import {
  clearStoredAuthTokens,
  ensureValidAccessToken,
  getAccessToken,
  getRefreshToken,
  hasStoredSessionTokens,
  isTokenExpired,
  setStoredAuthTokens,
} from './auth-session';

const storage = new Map<string, string>();
const localStorageMock = {
  getItem: vi.fn((key: string) => storage.get(key) ?? null),
  setItem: vi.fn((key: string, value: string) => {
    storage.set(key, value);
  }),
  removeItem: vi.fn((key: string) => {
    storage.delete(key);
  }),
  clear: vi.fn(() => {
    storage.clear();
  }),
};

function createToken(expiresInSecondsFromNow: number) {
  const header = Buffer.from(JSON.stringify({ alg: 'HS256', typ: 'JWT' })).toString('base64url');
  const payload = Buffer.from(
    JSON.stringify({ exp: Math.floor(Date.now() / 1000) + expiresInSecondsFromNow })
  ).toString('base64url');
  return `${header}.${payload}.signature`;
}

describe('auth session helpers', () => {
  beforeEach(() => {
    axiosPostMock.mockReset();
    storage.clear();
    localStorageMock.getItem.mockClear();
    localStorageMock.setItem.mockClear();
    localStorageMock.removeItem.mockClear();
    localStorageMock.clear.mockClear();
    vi.stubGlobal('localStorage', localStorageMock);
  });

  afterEach(() => {
    clearStoredAuthTokens();
    vi.unstubAllGlobals();
  });

  it('stores and clears the current auth session', () => {
    setStoredAuthTokens('access-token', 'refresh-token');

    expect(getAccessToken()).toBe('access-token');
    expect(getRefreshToken()).toBe('refresh-token');
    expect(hasStoredSessionTokens()).toBe(true);

    clearStoredAuthTokens();

    expect(getAccessToken()).toBeNull();
    expect(getRefreshToken()).toBeNull();
    expect(hasStoredSessionTokens()).toBe(false);
  });

  it('detects expired JWTs with a small clock skew', () => {
    expect(isTokenExpired(createToken(-1))).toBe(true);
    expect(isTokenExpired(createToken(60))).toBe(false);
  });

  it('refreshes an expired access token once for concurrent requests', async () => {
    setStoredAuthTokens(createToken(-30), 'refresh-token');
    axiosPostMock.mockResolvedValue({
      data: {
        access: createToken(300),
        refresh: 'rotated-refresh-token',
      },
    });

    const [first, second] = await Promise.all([
      ensureValidAccessToken(),
      ensureValidAccessToken(),
    ]);

    expect(first).toBe(second);
    expect(axiosPostMock).toHaveBeenCalledTimes(1);
    expect(getRefreshToken()).toBe('rotated-refresh-token');
    expect(getAccessToken()).toBe(first);
  });

  it('clears stored tokens when refresh rotation fails', async () => {
    setStoredAuthTokens(createToken(-30), 'refresh-token');
    axiosPostMock.mockRejectedValue(new Error('refresh failed'));

    await expect(ensureValidAccessToken()).resolves.toBeNull();
    expect(hasStoredSessionTokens()).toBe(false);
  });
});
