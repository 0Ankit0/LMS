'use client';

import { useEffect } from 'react';

import { useRouter } from '@/lib/router';
import { useAuthStore } from '@/store/auth-store';
import { useLmsMyEnrollments, useLmsParentChildren } from '@/hooks/use-lms';

type LmsAudience = 'course' | 'student' | 'parent';

interface LmsAudienceRouteProps {
  audience: LmsAudience;
  children: React.ReactNode;
}

export function LmsAudienceRoute({ audience, children }: LmsAudienceRouteProps) {
  const router = useRouter();
  const user = useAuthStore((state) => state.user);
  const { data: enrollments, isLoading: loadingEnrollments } = useLmsMyEnrollments();
  const { data: parentChildren, isLoading: loadingParentChildren } = useLmsParentChildren();

  const hasCourseAccess = Boolean(user?.is_superuser || (enrollments ?? []).some((row) => row.role === 'student' || row.role === 'instructor'));
  const hasStudentAccess = Boolean(user?.is_superuser || (enrollments ?? []).some((row) => row.role === 'student'));
  const hasParentAccess = Boolean(user?.is_superuser || (parentChildren ?? []).length > 0);

  const allowed = audience === 'course'
    ? hasCourseAccess
    : audience === 'student'
      ? hasStudentAccess
      : hasParentAccess;

  const isLoading = loadingEnrollments || loadingParentChildren;

  useEffect(() => {
    if (!isLoading && !allowed) {
      router.replace('/dashboard');
    }
  }, [allowed, isLoading, router]);

  if (isLoading || !allowed) {
    return (
      <div className="flex min-h-[240px] items-center justify-center rounded-2xl border border-gray-200 bg-white">
        <div className="h-8 w-8 animate-spin rounded-full border-b-2 border-blue-600" />
      </div>
    );
  }

  return <>{children}</>;
}