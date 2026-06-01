'use client';

import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useLmsAnnouncements } from '@/hooks/use-lms';

export default function AnnouncementsPage() {
  const { data, isLoading } = useLmsAnnouncements();
  const announcements = data ?? [];

  return (
    <SectionShell
      title="Announcements"
      description="Stay updated with important course and system announcements."
    >
      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : announcements.length === 0 ? (
        <EmptyHint title="No announcements available" subtitle="Instructors and admins can publish updates here." />
      ) : (
        <div className="space-y-4">
          {announcements.map((announcement) => (
            <Card key={announcement.id}>
              <CardHeader>
                <CardTitle className="text-lg">{announcement.title}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <p className="text-sm text-gray-700">{announcement.content}</p>
                <p className="text-xs text-gray-500">
                  Posted on {new Date(announcement.created_at).toLocaleString()}
                </p>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
