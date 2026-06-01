'use client';

import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent } from '@/components/ui/card';
import { useLmsMyFiles } from '@/hooks/use-lms';

export default function FilesPage() {
  const { data, isLoading } = useLmsMyFiles();
  const files = data ?? [];

  return (
    <SectionShell
      title="My Files"
      description="Manage uploaded files used in courses, notes, and submissions."
    >
      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : files.length === 0 ? (
        <EmptyHint title="No files uploaded" subtitle="Upload files from supported LMS workflows to see them here." />
      ) : (
        <Card>
          <CardContent className="p-4">
            <div className="space-y-2">
              {files.map((file: any) => (
                <div key={file.id} className="flex items-center justify-between rounded-lg border border-gray-200 px-3 py-2">
                  <div>
                    <p className="text-sm font-medium text-gray-900">{file.file_name}</p>
                    <p className="text-xs text-gray-500">Type: {file.mime_type}</p>
                  </div>
                  <p className="text-xs text-gray-500">{Math.round((file.file_size ?? 0) / 1024)} KB</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </SectionShell>
  );
}
