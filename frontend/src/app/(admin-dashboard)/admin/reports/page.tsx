'use client';

import { useState } from 'react';
import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useLmsAdminReports } from '@/hooks/use-lms';

export default function AdminReportsPage() {
  const [courseIdText, setCourseIdText] = useState('');
  const parsedCourseId = Number(courseIdText);
  const { data, isLoading, refetch } = useLmsAdminReports(Number.isFinite(parsedCourseId) ? parsedCourseId : undefined);

  const engagementMetrics = data?.engagement?.metrics;
  const completion = data?.completion?.items ?? [];

  return (
    <SectionShell
      title="Analytics Reports"
      description="Analyze engagement and completion metrics across LMS courses."
      actions={
        <div className="flex gap-2">
          <Input
            value={courseIdText}
            onChange={(event) => setCourseIdText(event.target.value)}
            placeholder="Optional course ID"
            className="w-56"
          />
          <Button variant="outline" onClick={() => refetch()}>Refresh</Button>
        </div>
      }
    >
      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle>Student Engagement</CardTitle>
            </CardHeader>
            <CardContent>
              {!engagementMetrics ? (
                <EmptyHint title="No engagement records" subtitle="Try a different course filter or wait for learner activity." />
              ) : (
                <div className="space-y-2">
                  <div className="rounded-lg border border-gray-200 px-3 py-2 text-sm text-gray-700">
                    Total tracked events: {engagementMetrics.events ?? 0}
                  </div>
                  <div className="rounded-lg border border-gray-200 px-3 py-2 text-sm text-gray-700">
                    Total learning time: {engagementMetrics.totalTimeSeconds ?? 0} seconds
                  </div>
                  <div className="rounded-lg border border-gray-200 px-3 py-2 text-sm text-gray-700">
                    Completed lessons: {engagementMetrics.completedLessons ?? 0}
                  </div>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Course Completion</CardTitle>
            </CardHeader>
            <CardContent>
              {completion.length === 0 ? (
                <EmptyHint title="No completion records" subtitle="Completion trends appear here once students finish modules." />
              ) : (
                <div className="space-y-2">
                  {completion.map((row: any, index: number) => (
                    <div key={index} className="rounded-lg border border-gray-200 px-3 py-2 text-sm text-gray-700">
                      Course #{row.courseId} • completion: {Math.round(row.completionRate ?? 0)}%
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      )}
    </SectionShell>
  );
}
