'use client';

import { useEffect, useState } from 'react';
import { useRouter } from '@/lib/router';
import { useAuthStore } from '@/store/auth-store';
import { apiClient } from '@/lib/api-client';
import { hasStoredSessionTokens } from '@/lib/auth-session';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const router = useRouter();
  const { isAuthenticated, _hasHydrated, setUser, logout } = useAuthStore();
  const [isInitializing, setIsInitializing] = useState(true);

  useEffect(() => {
    if (!_hasHydrated) return;

    async function initAuth() {
      if (!hasStoredSessionTokens()) {
        logout();
        router.push('/login');
        setIsInitializing(false);
        return;
      }

      try {
        const userResponse = await apiClient.get('/users/me');
        setUser(userResponse.data);
      } catch {
        logout();
        router.push('/login');
      } finally {
        setIsInitializing(false);
      }
    }

    initAuth();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [_hasHydrated]);

  // While Zustand is rehydrating from localStorage or we're attempting a refresh
  if (!_hasHydrated || isInitializing) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
      </div>
    );
  }

  // Refresh failed and router.push('/login') is in-flight — show spinner
  if (!isAuthenticated) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
      </div>
    );
  }

  return <>{children}</>;
}
