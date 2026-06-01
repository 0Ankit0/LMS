'use client';

import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useLmsCertificates } from '@/hooks/use-lms';

export default function CertificatesPage() {
  const { data, isLoading } = useLmsCertificates();
  const certificates = data ?? [];

  return (
    <SectionShell
      title="Certificates"
      description="Access your earned certificates and completion credentials."
    >
      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : certificates.length === 0 ? (
        <EmptyHint title="No certificates yet" subtitle="Complete courses and assessments to earn certificates." />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {certificates.map((certificate) => (
            <Card key={certificate.id}>
              <CardHeader>
                <CardTitle>Certificate #{certificate.certificate_code}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <p className="text-sm text-gray-700">Course ID: {certificate.course_id}</p>
                <p className="text-xs text-gray-500">
                  Issued at {new Date(certificate.issued_at).toLocaleDateString()}
                </p>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
