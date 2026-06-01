"use client";

import { useEffect, useState } from 'react';
import { SignupForm } from '@/components/auth/signup-form';
import { getEnabledProviders, type OAuthProvider } from '@/lib/oauth';

export default function SignupPage() {
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

  return <SignupForm enabledProviders={enabledProviders} />;
}
