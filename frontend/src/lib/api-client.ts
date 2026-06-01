import axios, { type InternalAxiosRequestConfig } from 'axios';

import {
  clearStoredAuthTokens,
  ensureValidAccessToken,
  getRefreshToken,
  redirectToLogin,
  refreshStoredAccessToken,
} from '@/lib/auth-session';

const baseURL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000/api/v1';
const backendOrigin = baseURL.replace(/\/api\/v1\/?$/, '');

export const apiClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

interface RetriableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

apiClient.interceptors.request.use(async (config) => {
  if (typeof config.url === 'string') {
    const isLmsEndpoint =
      config.url.startsWith('/api/') ||
      config.url === '/register' ||
      config.url === '/login' ||
      config.url === '/logout' ||
      config.url === '/manage/info';

    if (isLmsEndpoint) {
      config.baseURL = backendOrigin;
    }
  }

  const token = await ensureValidAccessToken();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  } else if (config.headers.Authorization) {
    delete config.headers.Authorization;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined;
    const isRefreshRequest = originalRequest?.url?.includes('/auth/refresh/');

    if (
      error.response?.status === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      !isRefreshRequest &&
      getRefreshToken()
    ) {
      originalRequest._retry = true;
      try {
        const access = await refreshStoredAccessToken();
        if (!access) {
          throw error;
        }
        originalRequest.headers.Authorization = `Bearer ${access}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        clearStoredAuthTokens();
        redirectToLogin();
        return Promise.reject(refreshError);
      }
    }

    if (error.response?.status === 401) {
      clearStoredAuthTokens();
      redirectToLogin();
    }

    return Promise.reject(error);
  }
);
