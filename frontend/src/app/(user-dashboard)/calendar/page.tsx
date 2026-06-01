'use client';

import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent } from '@/components/ui/card';
import { useLmsCalendar } from '@/hooks/use-lms';

export default function CalendarPage() {
  const { data, isLoading } = useLmsCalendar();
  const events = data ?? [];

  return (
    <SectionShell
      title="Calendar"
      description="Review upcoming classes, deadlines, and key learning events."
    >
      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : events.length === 0 ? (
        <EmptyHint title="No events scheduled" subtitle="Calendar events will appear when they are created." />
      ) : (
        <div className="space-y-3">
          {events.map((event: any) => (
            <Card key={event.id}>
              <CardContent className="space-y-2 p-4">
                <p className="text-sm font-semibold text-gray-900">{event.title}</p>
                <p className="text-sm text-gray-700">{event.description}</p>
                <p className="text-xs text-gray-500">
                  {new Date(event.starts_at).toLocaleString()} - {new Date(event.ends_at).toLocaleString()}
                </p>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
