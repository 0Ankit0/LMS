'use client';

import { useState } from 'react';
import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  useLmsAdminCourses,
  useLmsCreateCourse,
  useLmsDeleteCourse,
  useLmsDropdownUsers,
} from '@/hooks/use-lms';

export default function AdminCoursesPage() {
  const { data } = useLmsAdminCourses({ page: 1, pageSize: 50 });
  const { data: users } = useLmsDropdownUsers();
  const createCourse = useLmsCreateCourse();
  const deleteCourse = useLmsDeleteCourse();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');

  const courses = data?.items ?? [];

  const handleCreate = () => {
    if (!title.trim() || !description.trim()) {
      return;
    }
    createCourse.mutate({
      title: title.trim(),
      description: description.trim(),
      category: 'general',
      level: 'beginner',
      instructor_id: Number(users?.[0]?.value ?? 0) || null,
    });
    setTitle('');
    setDescription('');
  };

  return (
    <SectionShell
      title="Course Administration"
      description="Create, review, and retire courses in the LMS catalog."
    >
      <Card>
        <CardContent className="space-y-3 p-4">
          <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
            <Input value={title} onChange={(event) => setTitle(event.target.value)} placeholder="Course title" />
            <Input value={description} onChange={(event) => setDescription(event.target.value)} placeholder="Course description" />
          </div>
          <div className="flex justify-end">
            <Button onClick={handleCreate} isLoading={createCourse.isPending}>Create Course</Button>
          </div>
        </CardContent>
      </Card>

      {courses.length === 0 ? (
        <EmptyHint title="No courses yet" subtitle="Create the first course to initialize the catalog." />
      ) : (
        <div className="space-y-3">
          {courses.map((course) => (
            <Card key={course.id}>
              <CardContent className="flex items-center justify-between gap-4 p-4">
                <div>
                  <p className="text-sm font-semibold text-gray-900">{course.title}</p>
                  <p className="text-xs text-gray-500">{course.category} • {course.level}</p>
                </div>
                <Button
                  variant="outline"
                  onClick={() => deleteCourse.mutate(course.id)}
                  isLoading={deleteCourse.isPending}
                >
                  Delete
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
