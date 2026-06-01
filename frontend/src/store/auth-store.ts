import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { User, Tenant } from '@/types';
import { clearStoredAuthTokens, setStoredAuthTokens } from '@/lib/auth-session';

interface AuthState {
  user: User | null;
  tenant: Tenant | null;
  isAuthenticated: boolean;
  _hasHydrated: boolean;
  setUser: (user: User | null) => void;
  setTokens: (access: string, refresh: string) => void;
  setTenant: (tenant: Tenant | null) => void;
  logout: () => void;
  setHasHydrated: (state: boolean) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      tenant: null,
      isAuthenticated: false,
      _hasHydrated: false,
      setUser: (user) => set({ user, isAuthenticated: !!user }),
      setTokens: (access, refresh) => {
        setStoredAuthTokens(access, refresh);
      },
      setTenant: (tenant) => {
        set({ tenant });
      },
      logout: () => {
        clearStoredAuthTokens();
        set({ user: null, tenant: null, isAuthenticated: false });
      },
      setHasHydrated: (state) => set({ _hasHydrated: state }),
    }),
    {
      name: 'auth-storage',
      // Only persist tokens (stored separately) — never persist user data or
      // isAuthenticated to localStorage. User state must always come from the
      // server, preventing client-side spoofing of roles/permissions.
      partialize: () => ({}),
      onRehydrateStorage: () => (state) => {
        state?.setHasHydrated(true);
      },
    }
  )
);
