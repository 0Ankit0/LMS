'use client';

import { useEffect, useMemo, useState } from 'react';
import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent } from '@/components/ui/card';
import {
  useLmsParentChildAttendance,
  useLmsParentChildDashboard,
  useLmsParentChildGrades,
  useLmsParentChildren,
} from '@/hooks/use-lms';

export default function ParentDashboardPage() {
  const { data, isLoading } = useLmsParentChildren();
  const children = data ?? [];
  const [selectedStudentId, setSelectedStudentId] = useState<number | null>(null);

  useEffect(() => {
    if (selectedStudentId === null && children.length > 0) {
      setSelectedStudentId(children[0].id);
    }
  }, [children, selectedStudentId]);

  const selectedChild = children.find((child) => child.id === selectedStudentId) ?? null;
  const { data: dashboard } = useLmsParentChildDashboard(selectedStudentId);
  const { data: grades } = useLmsParentChildGrades(selectedStudentId);
  const { data: attendance } = useLmsParentChildAttendance(selectedStudentId);

  const activeCount = useMemo(
    () => children.length,
    [children]
  );

  return (
    <SectionShell
      title="Parent Dashboard"
      description="Monitor linked students and track their academic activity from one place."
    >
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <div className="rounded-xl border border-gray-200 bg-white p-4">
          <p className="text-xs uppercase tracking-[0.12em] text-gray-400">Linked Students</p>
          <p className="mt-2 text-2xl font-semibold text-gray-900">{children.length}</p>
        </div>
        <div className="rounded-xl border border-gray-200 bg-white p-4">
          <p className="text-xs uppercase tracking-[0.12em] text-gray-400">Active</p>
          <p className="mt-2 text-2xl font-semibold text-gray-900">{activeCount}</p>
        </div>
      </div>

      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : children.length === 0 ? (
        <EmptyHint title="No linked children" subtitle="Ask an admin to create student-parent links to use this dashboard." />
      ) : (
        <div className="grid grid-cols-1 gap-6 xl:grid-cols-[280px_minmax(0,1fr)]">
          <div className="space-y-3">
            {children.map((child) => (
              <button
                key={child.id}
                type="button"
                onClick={() => setSelectedStudentId(child.id)}
                className={`w-full rounded-xl border p-4 text-left transition ${selectedStudentId === child.id ? 'border-blue-500 bg-blue-50' : 'border-gray-200 bg-white hover:border-blue-200'}`}
              >
                <p className="text-sm font-semibold text-gray-900">{child.username}</p>
                <p className="text-xs text-gray-500">{child.email}</p>
              </button>
            ))}
          </div>

          <div className="space-y-4">
            <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
              <div className="rounded-xl border border-gray-200 bg-white p-4">
                <p className="text-xs uppercase tracking-[0.12em] text-gray-400">Selected Student</p>
                <p className="mt-2 text-lg font-semibold text-gray-900">{selectedChild?.username ?? 'Unknown'}</p>
                <p className="text-xs text-gray-500">ID: {selectedChild?.id ?? '-'}</p>
              </div>
              <div className="rounded-xl border border-gray-200 bg-white p-4">
                <p className="text-xs uppercase tracking-[0.12em] text-gray-400">Enrollments</p>
                <p className="mt-2 text-2xl font-semibold text-gray-900">{dashboard?.enrollments ?? 0}</p>
              </div>
              <div className="rounded-xl border border-gray-200 bg-white p-4">
                <p className="text-xs uppercase tracking-[0.12em] text-gray-400">Assessment Attempts</p>
                <p className="mt-2 text-2xl font-semibold text-gray-900">{dashboard?.assessmentAttempts ?? 0}</p>
              </div>
            </div>

            <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
              <Card>
                <CardContent className="space-y-3 p-4">
                  <p className="text-sm font-semibold text-gray-900">Recent Grades</p>
                  {(grades ?? []).length === 0 ? (
                    <p className="text-sm text-gray-500">No submitted assessments yet.</p>
                  ) : (
                    (grades ?? []).slice(0, 5).map((attempt) => (
                      <div key={attempt.id} className="rounded-lg border border-gray-200 px-3 py-2 text-sm text-gray-700">
                        Assessment #{attempt.assessment_id} • score {attempt.score} • {attempt.status}
                      </div>
                    ))
                  )}
                </CardContent>
              </Card>

              <Card>
                <CardContent className="space-y-3 p-4">
                  <p className="text-sm font-semibold text-gray-900">Attendance</p>
                  {(attendance ?? []).length === 0 ? (
                    <p className="text-sm text-gray-500">No attendance sessions recorded yet.</p>
                  ) : (
                    (attendance ?? []).slice(0, 5).map((record) => (
                      <div key={record.id} className="rounded-lg border border-gray-200 px-3 py-2 text-sm text-gray-700">
                        {new Date(record.session_date).toLocaleDateString()} • {record.status}
                      </div>
                    ))
                  )}
                </CardContent>
              </Card>
            </div>
          </div>
        </div>
      )}
    </SectionShell>
  );
}
