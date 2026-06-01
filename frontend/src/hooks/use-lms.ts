'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import type {
  LmsAnnouncement,
  LmsAssessment,
  LmsAssessmentAttempt,
  LmsAssessmentDetail,
  LmsCertificate,
  LmsConversation,
  LmsCourse,
  LmsCourseStructure,
  LmsDropdownOption,
  LmsEnrollment,
  LmsGradebookEntry,
  LmsInstructorCourseSummary,
  LmsAttendanceRecord,
  LmsLeaderboardEntry,
  LmsMessage,
  LmsNote,
  LmsNotification,
  LmsPaginated,
  LmsParentChild,
  LmsParentChildDashboard,
} from '@/types/lms';

export function useLmsCourses(params?: {
  page?: number;
  pageSize?: number;
  search?: string;
  category?: string;
  level?: string;
}) {
  return useQuery({
    queryKey: ['lms-courses', params],
    queryFn: async () => {
      const response = await apiClient.get<LmsPaginated<LmsCourse>>('/api/courses', { params });
      return response.data;
    },
  });
}

export function useLmsCourse(courseId: number | null) {
  return useQuery({
    queryKey: ['lms-course', courseId],
    queryFn: async () => {
      const response = await apiClient.get<LmsCourse>(`/api/courses/${courseId}`);
      return response.data;
    },
    enabled: courseId !== null,
  });
}

export function useLmsMyEnrollments() {
  return useQuery({
    queryKey: ['lms-my-enrollments'],
    queryFn: async () => {
      const response = await apiClient.get<LmsEnrollment[]>('/api/my-enrollments');
      return response.data;
    },
  });
}

export function useLmsEnrollInCourse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (courseId: number) => {
      const response = await apiClient.post<{ message: string; enrollmentId: number }>(`/api/courses/${courseId}/enroll`);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-my-enrollments'] });
    },
  });
}

export function useLmsAnnouncements(courseId?: number) {
  return useQuery({
    queryKey: ['lms-announcements', courseId],
    queryFn: async () => {
      const response = await apiClient.get<LmsAnnouncement[]>('/api/announcements', {
        params: courseId ? { courseId } : undefined,
      });
      return response.data;
    },
  });
}

export function useLmsAssessments() {
  return useQuery({
    queryKey: ['lms-assessments-sample'],
    queryFn: async () => {
      const response = await apiClient.get<LmsPaginated<LmsCourse>>('/api/courses', { params: { page: 1, pageSize: 20 } });
      const courses = response.data.items;
      const detailed = await Promise.all(
        courses.slice(0, 5).map(async (course: LmsCourse) => {
          const modules = await apiClient.get<Array<{ id: number }>>(`/api/courses/${course.id}/modules`);
          return {
            course,
            moduleCount: modules.data.length,
          };
        })
      );
      return detailed;
    },
  });
}

export function useLmsCourseStructure(courseId: number | null) {
  return useQuery({
    queryKey: ['lms-course-structure', courseId],
    queryFn: async () => {
      const response = await apiClient.get<LmsCourseStructure>(`/api/courses/${courseId}/structure`);
      return response.data;
    },
    enabled: courseId !== null,
  });
}

export function useLmsUpdateLessonProgress(courseId: number | null) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: { lessonId: number; timeSpent: string; isCompleted: boolean }) => {
      const response = await apiClient.post<{ unlockedAchievements: string[] }>(`/api/lessons/${payload.lessonId}/progress`, {
        timeSpent: payload.timeSpent,
        isCompleted: payload.isCompleted,
      });
      return response.data;
    },
    onSuccess: () => {
      if (courseId !== null) {
        queryClient.invalidateQueries({ queryKey: ['lms-course-structure', courseId] });
      }
      queryClient.invalidateQueries({ queryKey: ['lms-achievements'] });
    },
  });
}

export function useLmsCourseAssessments(courseId: number | null) {
  return useQuery({
    queryKey: ['lms-course-assessments', courseId],
    queryFn: async () => {
      const response = await apiClient.get<LmsAssessment[]>(`/api/courses/${courseId}/assessments`);
      return response.data;
    },
    enabled: courseId !== null,
  });
}

export function useLmsCourseGradebook(courseId: number | null) {
  return useQuery({
    queryKey: ['lms-course-gradebook', courseId],
    queryFn: async () => {
      const response = await apiClient.get<LmsGradebookEntry[]>(`/api/courses/${courseId}/gradebook`);
      return response.data;
    },
    enabled: courseId !== null,
  });
}

export function useLmsAssessment(assessmentId: number | null) {
  return useQuery({
    queryKey: ['lms-assessment', assessmentId],
    queryFn: async () => {
      const response = await apiClient.get<LmsAssessmentDetail>(`/api/assessments/${assessmentId}`);
      return response.data;
    },
    enabled: assessmentId !== null,
  });
}

export function useLmsMyAssessmentAttempts(assessmentId: number | null) {
  return useQuery({
    queryKey: ['lms-assessment-attempts-me', assessmentId],
    queryFn: async () => {
      const response = await apiClient.get<LmsAssessmentAttempt[]>(`/api/assessments/${assessmentId}/attempts/me`);
      return response.data;
    },
    enabled: assessmentId !== null,
  });
}

export function useLmsSubmitAssessment(assessmentId: number | null) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (responses: Record<string, string>) => {
      const response = await apiClient.post<LmsAssessmentAttempt>(`/api/assessments/${assessmentId}/submit`, { responses });
      return response.data;
    },
    onSuccess: () => {
      if (assessmentId !== null) {
        queryClient.invalidateQueries({ queryKey: ['lms-assessment-attempts-me', assessmentId] });
      }
    },
  });
}

export function useLmsGradeAssessmentAttempt(courseId: number | null, assessmentId: number | null) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ attemptId, manualScores }: { attemptId: number; manualScores: Record<string, number> }) => {
      const response = await apiClient.post<LmsAssessmentAttempt>(`/api/assessment-attempts/${attemptId}/grade`, {
        manualScores,
      });
      return response.data;
    },
    onSuccess: () => {
      if (courseId !== null) {
        queryClient.invalidateQueries({ queryKey: ['lms-course-gradebook', courseId] });
      }
      if (assessmentId !== null) {
        queryClient.invalidateQueries({ queryKey: ['lms-assessment-attempts-me', assessmentId] });
      }
    },
  });
}

export function useLmsCertificates() {
  return useQuery({
    queryKey: ['lms-certificates-me'],
    queryFn: async () => {
      const response = await apiClient.get<LmsCertificate[]>('/api/users/me/certificates');
      return response.data;
    },
  });
}

export function useLmsLeaderboard() {
  return useQuery({
    queryKey: ['lms-leaderboard'],
    queryFn: async () => {
      const response = await apiClient.get<LmsLeaderboardEntry[]>('/api/leaderboard');
      return response.data;
    },
  });
}

export function useLmsNotifications() {
  return useQuery({
    queryKey: ['lms-notifications'],
    queryFn: async () => {
      const response = await apiClient.get<LmsNotification[]>('/api/notifications');
      return response.data;
    },
  });
}

export function useLmsMarkNotificationRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (notificationId: number) => {
      const response = await apiClient.post<LmsNotification>(`/api/notifications/${notificationId}/mark-as-read`);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-notifications'] });
    },
  });
}

export function useLmsConversations() {
  return useQuery({
    queryKey: ['lms-conversations'],
    queryFn: async () => {
      const response = await apiClient.get<LmsConversation[]>('/api/conversations');
      return response.data;
    },
  });
}

export function useLmsConversationMessages(conversationId: number | null) {
  return useQuery({
    queryKey: ['lms-conversation-messages', conversationId],
    queryFn: async () => {
      const response = await apiClient.get<LmsMessage[]>(`/api/conversations/${conversationId}/messages`);
      return response.data;
    },
    enabled: conversationId !== null,
  });
}

export function useLmsSendMessage() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: { conversationId: number; encryptedContent: string }) => {
      const response = await apiClient.post<LmsMessage>('/api/messages', payload);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['lms-conversation-messages', variables.conversationId] });
    },
  });
}

export function useLmsNotes(params?: {
  courseId?: number;
  lessonId?: number;
  searchTerm?: string;
  tag?: string;
  priority?: string;
  isPinned?: boolean;
}) {
  return useQuery({
    queryKey: ['lms-notes', params],
    queryFn: async () => {
      const response = await apiClient.get<LmsNote[]>('/api/notes', { params });
      return response.data;
    },
  });
}

export function useLmsCreateNote() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: {
      title: string;
      content: string;
      courseId?: number;
      lessonId?: number;
      tag?: string;
      priority?: string;
      isPinned?: boolean;
      attachmentFileId?: number;
    }) => {
      const response = await apiClient.post<LmsNote>('/api/notes', payload);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-notes'] });
    },
  });
}

export function useLmsUpdateNote() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ noteId, payload }: { noteId: number; payload: Partial<LmsNote> }) => {
      const response = await apiClient.put<LmsNote>(`/api/notes/${noteId}`, payload);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-notes'] });
    },
  });
}

export function useLmsDeleteNote() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (noteId: number) => {
      await apiClient.delete(`/api/notes/${noteId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-notes'] });
    },
  });
}

export function useLmsDropdownCategories() {
  return useQuery({
    queryKey: ['lms-dropdown-categories'],
    queryFn: async () => {
      const response = await apiClient.get<LmsDropdownOption[]>('/api/dropdown/categories');
      return response.data;
    },
  });
}

export function useLmsDropdownUsers() {
  return useQuery({
    queryKey: ['lms-dropdown-users'],
    queryFn: async () => {
      const response = await apiClient.get<LmsDropdownOption[]>('/api/dropdown/users');
      return response.data;
    },
  });
}

export function useLmsAdminUsers(params?: { page?: number; pageSize?: number }) {
  return useQuery({
    queryKey: ['lms-admin-users', params],
    queryFn: async () => {
      const response = await apiClient.get<LmsPaginated<any>>('/api/admin/users', { params });
      return response.data;
    },
  });
}

export function useLmsAdminCourses(params?: { page?: number; pageSize?: number }) {
  return useLmsCourses(params);
}

export function useLmsCreateCourse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: {
      title: string;
      description: string;
      category: string;
      level: string;
      instructor_id?: number | null;
    }) => {
      const response = await apiClient.post<LmsCourse>('/api/courses', payload);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-courses'] });
    },
  });
}

export function useLmsDeleteCourse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (courseId: number) => {
      await apiClient.delete(`/api/courses/${courseId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-courses'] });
    },
  });
}

export function useLmsAdminAdmissions(params?: { status?: string; programId?: string; page?: number; pageSize?: number }) {
  return useQuery({
    queryKey: ['lms-admin-admissions', params],
    queryFn: async () => {
      const response = await apiClient.get<LmsPaginated<any>>('/api/admin/admissions/applications', { params });
      return response.data;
    },
  });
}

export function useLmsAdminReports(courseId?: number) {
  return useQuery({
    queryKey: ['lms-admin-reports', courseId],
    queryFn: async () => {
      const [engagement, completion] = await Promise.all([
        apiClient.get('/api/reports/student-engagement', { params: courseId ? { courseId } : undefined }),
        apiClient.get('/api/reports/course-completion', { params: courseId ? { courseId } : undefined }),
      ]);
      return {
        engagement: engagement.data,
        completion: completion.data,
      };
    },
  });
}

export function useLmsCalendar() {
  return useQuery({
    queryKey: ['lms-calendar'],
    queryFn: async () => {
      const now = new Date();
      const startDate = new Date(now.getFullYear(), now.getMonth(), 1).toISOString();
      const endDate = new Date(now.getFullYear(), now.getMonth() + 2, 0, 23, 59, 59).toISOString();
      const response = await apiClient.get<any[]>('/api/calendar', {
        params: { startDate, endDate },
      });
      return response.data;
    },
  });
}

export function useLmsMyFiles() {
  return useQuery({
    queryKey: ['lms-my-files'],
    queryFn: async () => {
      const response = await apiClient.get<any[]>('/api/my-files');
      return response.data;
    },
  });
}

export function useLmsMyAchievements(userId: number | null) {
  return useQuery({
    queryKey: ['lms-achievements', userId],
    queryFn: async () => {
      const response = await apiClient.get<any[]>(`/api/users/${userId}/achievements`);
      return response.data;
    },
    enabled: userId !== null,
  });
}

export function useLmsCategories() {
  return useQuery({
    queryKey: ['lms-categories'],
    queryFn: async () => {
      const response = await apiClient.get<any[]>('/api/categories');
      return response.data;
    },
  });
}

export function useLmsTags() {
  return useQuery({
    queryKey: ['lms-tags'],
    queryFn: async () => {
      const response = await apiClient.get<any[]>('/api/tags');
      return response.data;
    },
  });
}

export function useLmsCompetencies() {
  return useQuery({
    queryKey: ['lms-competencies'],
    queryFn: async () => {
      const response = await apiClient.get<any[]>('/api/competencies');
      return response.data;
    },
  });
}

export function useLmsCreateCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: { name: string; parent_id?: number | null }) => {
      const response = await apiClient.post('/api/categories', payload);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-categories'] });
    },
  });
}

export function useLmsDeleteCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (categoryId: number) => {
      await apiClient.delete(`/api/categories/${categoryId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-categories'] });
    },
  });
}

export function useLmsCreateTag() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: { name: string }) => {
      const response = await apiClient.post('/api/tags', payload);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-tags'] });
    },
  });
}

export function useLmsDeleteTag() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (tagId: number) => {
      await apiClient.delete(`/api/tags/${tagId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-tags'] });
    },
  });
}

export function useLmsCreateCompetency() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: { code: string; title: string; description: string }) => {
      const response = await apiClient.post('/api/competencies', payload);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lms-competencies'] });
    },
  });
}

export function useLmsAttendanceByCourse(courseId?: number) {
  return useQuery({
    queryKey: ['lms-attendance-course', courseId],
    queryFn: async () => {
      const response = await apiClient.get<any[]>(`/api/courses/${courseId}/attendance`);
      return response.data;
    },
    enabled: typeof courseId === 'number',
  });
}

export function useLmsParentChildren() {
  return useQuery({
    queryKey: ['lms-parent-children'],
    queryFn: async () => {
      const response = await apiClient.get<LmsParentChild[]>('/api/parent/my-children');
      return response.data;
    },
  });
}

export function useLmsParentChildDashboard(studentId: number | null) {
  return useQuery({
    queryKey: ['lms-parent-child-dashboard', studentId],
    queryFn: async () => {
      const response = await apiClient.get<LmsParentChildDashboard>(`/api/parent/children/${studentId}/dashboard`);
      return response.data;
    },
    enabled: studentId !== null,
  });
}

export function useLmsParentChildGrades(studentId: number | null) {
  return useQuery({
    queryKey: ['lms-parent-child-grades', studentId],
    queryFn: async () => {
      const response = await apiClient.get<LmsAssessmentAttempt[]>(`/api/parent/children/${studentId}/grades`);
      return response.data;
    },
    enabled: studentId !== null,
  });
}

export function useLmsParentChildAttendance(studentId: number | null) {
  return useQuery({
    queryKey: ['lms-parent-child-attendance', studentId],
    queryFn: async () => {
      const response = await apiClient.get<LmsAttendanceRecord[]>(`/api/parent/children/${studentId}/attendance`);
      return response.data;
    },
    enabled: studentId !== null,
  });
}

export function useLmsInstructorCourseSummaries(courseIds: number[]) {
  return useQuery({
    queryKey: ['lms-instructor-course-summaries', courseIds],
    queryFn: async () => {
      const uniqueIds = [...new Set(courseIds)];
      const courses = await Promise.all(
        uniqueIds.map(async (courseId) => {
          const [course, enrollments, structure, assessments] = await Promise.all([
            apiClient.get<LmsCourse>(`/api/courses/${courseId}`),
            apiClient.get<LmsEnrollment[]>(`/api/courses/${courseId}/enrollments`),
            apiClient.get<LmsCourseStructure>(`/api/courses/${courseId}/structure`),
            apiClient.get<LmsAssessment[]>(`/api/courses/${courseId}/assessments`),
          ]);

          return {
            course: course.data,
            enrollmentCount: enrollments.data.length,
            lessonCount: structure.data.modules.reduce((total, module) => total + module.lessons.length, 0),
            assessmentCount: assessments.data.length,
          } satisfies LmsInstructorCourseSummary;
        })
      );
      return courses;
    },
    enabled: courseIds.length > 0,
  });
}
