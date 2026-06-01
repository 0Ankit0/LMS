'use client';

import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent } from '@/components/ui/card';
import { useLmsAdminAdmissions } from '@/hooks/use-lms';

export default function AdminAdmissionsPage() {
  const { data, isLoading } = useLmsAdminAdmissions({ page: 1, pageSize: 100 });
  const applications = data?.items ?? [];

  return (
    <SectionShell
      title="Admissions"
      description="Review admission applications and monitor enrollment pipeline status."
    >
      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : applications.length === 0 ? (
        <EmptyHint title="No applications found" subtitle="Applications will appear here after candidates apply." />
      ) : (
        <div className="space-y-3">
          {applications.map((application: any) => (
            <Card key={application.id}>
              <CardContent className="flex items-center justify-between gap-4 p-4">
                <div>
                  <p className="text-sm font-semibold text-gray-900">
                    Applicant #{application.applicant_id ?? application.id}
                  </p>
                  <p className="text-xs text-gray-500">Program: {application.program_id}</p>
                </div>
                <span className="rounded-full bg-blue-50 px-3 py-1 text-xs text-blue-700">
                  {application.status}
                </span>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
