'use client';

import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Link } from '@/lib/router';
import { useLmsCourses, useLmsMyEnrollments } from '@/hooks/use-lms';

export default function AssessmentsPage() {
  const { data: enrollments, isLoading } = useLmsMyEnrollments();
  const { data: courses } = useLmsCourses({ page: 1, pageSize: 100 });

  const courseById = new Map((courses?.items ?? []).map((course) => [course.id, course]));
  const rows = (enrollments ?? []).map((enrollment) => ({
    enrollment,
    course: courseById.get(enrollment.course_id),
  }));

  return (
    <SectionShell
      title="Assessments"
      description="Open each course workspace to answer teacher-authored exercises and review attempt history."
    >
      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : rows.length === 0 ? (
        <EmptyHint title="No assessment data yet" subtitle="Enroll in a course to access quizzes and graded exercises." />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {rows.map((item) => (
            <Card key={item.enrollment.id}>
              <CardHeader>
                <CardTitle>{item.course?.title ?? `Course #${item.enrollment.course_id}`}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <p className="text-sm text-gray-700">{item.course?.description ?? 'Open the workspace to load exercises and submission history.'}</p>
                <p className="text-sm text-gray-600">Role in course: {item.enrollment.role}</p>
                <Link
                  href={`/my-courses/${item.enrollment.course_id}`}
                  className="inline-flex rounded-lg bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-700"
                >
                  Open Assessment Workspace
                </Link>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
