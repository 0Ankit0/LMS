'use client';

import { useMemo, useState } from 'react';
import { SectionShell, StatCard, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useLmsCourses, useLmsEnrollInCourse } from '@/hooks/use-lms';

export default function CourseCatalogPage() {
  const [search, setSearch] = useState('');
  const { data, isLoading } = useLmsCourses({ page: 1, pageSize: 24, search });
  const enrollMutation = useLmsEnrollInCourse();

  const courses = data?.items ?? [];
  const categoryCount = useMemo(() => new Set(courses.map((course) => course.category)).size, [courses]);

  return (
    <SectionShell
      title="Course Catalog"
      description="Discover and enroll in available courses aligned to your learning path."
      actions={
        <Input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search by title or category"
          className="w-72"
        />
      }
    >
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <StatCard label="Available Courses" value={String(data?.total ?? 0)} />
        <StatCard label="Categories" value={String(categoryCount)} />
        <StatCard label="Current Page" value={String(data?.page ?? 1)} helper="Data is synced from LMS API" />
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {[1, 2, 3, 4].map((key) => (
            <div key={key} className="h-44 animate-pulse rounded-xl bg-gray-100" />
          ))}
        </div>
      ) : courses.length === 0 ? (
        <EmptyHint title="No courses found" subtitle="Try a different search query or check back later." />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {courses.map((course) => (
            <Card key={course.id}>
              <CardHeader>
                <CardTitle className="text-lg">{course.title}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <p className="text-sm text-gray-600">{course.description}</p>
                <div className="flex flex-wrap gap-2 text-xs">
                  <span className="rounded-full bg-blue-50 px-3 py-1 text-blue-700">{course.category}</span>
                  <span className="rounded-full bg-emerald-50 px-3 py-1 text-emerald-700">{course.level}</span>
                </div>
                <div className="flex justify-end">
                  <Button
                    onClick={() => enrollMutation.mutate(course.id)}
                    isLoading={enrollMutation.isPending}
                  >
                    Enroll
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
