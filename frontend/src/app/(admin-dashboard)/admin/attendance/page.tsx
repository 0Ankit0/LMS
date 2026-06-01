'use client';

import { useState } from 'react';
import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { useLmsAttendanceByCourse } from '@/hooks/use-lms';

export default function AdminAttendancePage() {
  const [courseIdInput, setCourseIdInput] = useState('');
  const parsedCourseId = Number(courseIdInput);
  const { data, isLoading } = useLmsAttendanceByCourse(Number.isFinite(parsedCourseId) ? parsedCourseId : undefined);

  const attendance = data ?? [];

  return (
    <SectionShell
      title="Attendance Tracking"
      description="Audit attendance logs by course and identify absentee patterns."
      actions={
        <Input
          value={courseIdInput}
          onChange={(event) => setCourseIdInput(event.target.value)}
          placeholder="Enter course ID"
          className="w-56"
        />
      }
    >
      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : attendance.length === 0 ? (
        <EmptyHint title="No attendance entries" subtitle="Provide a valid course ID to load attendance records." />
      ) : (
        <div className="space-y-3">
          {attendance.map((entry: any) => (
            <Card key={entry.id}>
              <CardContent className="flex items-center justify-between gap-4 p-4">
                <div>
                  <p className="text-sm font-semibold text-gray-900">User #{entry.user_id}</p>
                  <p className="text-xs text-gray-500">Date: {entry.date}</p>
                </div>
                <span className="rounded-full bg-blue-50 px-3 py-1 text-xs text-blue-700">{entry.status}</span>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
