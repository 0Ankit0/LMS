export interface LmsCourse {
  id: number;
  title: string;
  description: string;
  category: string;
  level: string;
  instructor_id: number | null;
}

export interface LmsEnrollment {
  id: number;
  course_id: number;
  user_id: number;
  role: string;
  enrolled_at: string;
}

export interface LmsAnnouncement {
  id: number;
  title: string;
  content: string;
  course_id: number | null;
  created_by: number | null;
  created_at: string;
}

export interface LmsAssessment {
  id: number;
  course_id: number;
  title: string;
  description: string;
  max_score: number;
}

export interface LmsAssessmentQuestion {
  id: number;
  assessment_id: number;
  prompt: string;
  options_json: string[];
  correct_answer: string;
  points: number;
  is_manual: boolean;
}

export interface LmsAssessmentDetail extends LmsAssessment {
  questions: LmsAssessmentQuestion[];
}

export interface LmsAssessmentAttempt {
  id: number;
  assessment_id: number;
  user_id: number;
  responses_json: Record<string, string>;
  score: number;
  status: string;
  graded_by: number | null;
  submitted_at: string;
}

export interface LmsCertificate {
  id: number;
  course_id: number;
  user_id: number;
  certificate_code: string;
  issued_at: string;
}

export interface LmsLeaderboardEntry {
  userId: number;
  points: number;
}

export interface LmsNotification {
  id: number;
  user_id: number;
  title: string;
  message: string;
  type: string;
  is_read: boolean;
  read_at: string | null;
  created_at: string;
}

export interface LmsConversation {
  id: number;
  title: string;
  is_group: boolean;
  created_by: number;
  created_at: string;
}

export interface LmsMessage {
  id: number;
  conversation_id: number;
  sender_id: number;
  encrypted_content: string;
  created_at: string;
}

export interface LmsNote {
  id: number;
  user_id: number;
  course_id: number | null;
  lesson_id: number | null;
  title: string;
  content: string;
  tag: string;
  priority: string;
  is_pinned: boolean;
  attachment_file_id: number | null;
  created_at: string;
  updated_at: string;
}

export interface LmsPaginated<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
}

export interface LmsDropdownOption {
  value: number | string;
  label: string;
}

export interface LmsLesson {
  id: number;
  module_id: number;
  title: string;
  content: string;
  duration_minutes: number;
  isCompleted: boolean;
  timeSpentSeconds: number;
}

export interface LmsModuleStructure {
  id: number;
  course_id: number;
  title: string;
  description: string;
  order_index: number;
  lessons: LmsLesson[];
}

export interface LmsCourseStructure {
  courseId: number;
  modules: LmsModuleStructure[];
}

export interface LmsParentChild {
  id: number;
  username: string;
  email: string;
}

export interface LmsParentChildDashboard {
  studentId: number;
  enrollments: number;
  assessmentAttempts: number;
}

export interface LmsAttendanceRecord {
  id: number;
  course_id: number;
  user_id: number;
  session_date: string;
  status: string;
  notes: string;
  marked_by: number | null;
}

export interface LmsInstructorCourseSummary {
  course: LmsCourse;
  enrollmentCount: number;
  lessonCount: number;
  assessmentCount: number;
}

export interface LmsGradebookScore {
  attemptId: number;
  assessmentId: number;
  score: number;
}

export interface LmsGradebookEntry {
  userId: number;
  scores: LmsGradebookScore[];
  averageScore: number;
}
