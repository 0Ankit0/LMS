'use client';

import { SectionShell, StatCard, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Link } from '@/lib/router';
import { useLmsCourses, useLmsMyEnrollments } from '@/hooks/use-lms';

export default function MyCoursesPage() {
  const { data: enrollments, isLoading: loadingEnrollments } = useLmsMyEnrollments();
  const { data: coursesData } = useLmsCourses({ page: 1, pageSize: 100 });

  const courseById = new Map((coursesData?.items ?? []).map((course) => [course.id, course]));
  const myCourses = (enrollments ?? []).map((enrollment) => ({
    enrollment,
    course: courseById.get(enrollment.course_id),
  }));

  return (
    <SectionShell
      title="My Courses"
      description="Track your enrolled courses and jump back into active classes."
    >
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <StatCard label="Enrolled" value={String(myCourses.length)} />
        <StatCard
          label="Student Roles"
          value={String(myCourses.filter((item) => item.enrollment.role === 'student').length)}
        />
        <StatCard label="Instructor Roles" value={String(myCourses.filter((item) => item.enrollment.role === 'instructor').length)} />
      </div>

      {loadingEnrollments ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : myCourses.length === 0 ? (
        <EmptyHint title="No enrollments yet" subtitle="Enroll from the Course Catalog to start learning." />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {myCourses.map((item) => (
            <Card key={item.enrollment.id}>
              <CardHeader>
                <CardTitle>{item.course?.title ?? `Course #${item.enrollment.course_id}`}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <p className="text-sm text-gray-600">Role: {item.enrollment.role}</p>
                <p className="text-xs text-gray-500">
                  Enrolled on {new Date(item.enrollment.enrolled_at).toLocaleDateString()}
                </p>
                <Link
                  href={`/my-courses/${item.enrollment.course_id}`}
                  className="inline-flex rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 hover:bg-gray-50"
                >
                  Open Course Workspace
                </Link>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
