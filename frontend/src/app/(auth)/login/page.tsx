"use client";

import { useEffect, useState } from 'react';
import { LoginForm } from '@/components/auth/login-form';
import { getEnabledProviders, type OAuthProvider } from '@/lib/oauth';

export default function LoginPage() {
  const [enabledProviders, setEnabledProviders] = useState<OAuthProvider[]>([]);

  useEffect(() => {
    let isMounted = true;

    void getEnabledProviders().then((providers) => {
      if (isMounted) {
        setEnabledProviders(providers);
      }
    });

    return () => {
      isMounted = false;
    };
  }, []);

  return <LoginForm enabledProviders={enabledProviders} />;
}
