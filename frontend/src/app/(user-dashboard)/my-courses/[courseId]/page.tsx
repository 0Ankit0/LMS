'use client';

import { useEffect, useMemo, useState } from 'react';
import { useParams, Link } from '@/lib/router';
import { SectionShell, EmptyHint, StatCard } from '@/components/lms/section-shell';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import {
  useLmsAssessment,
  useLmsCourse,
  useLmsCourseAssessments,
  useLmsCourseGradebook,
  useLmsCourseStructure,
  useLmsGradeAssessmentAttempt,
  useLmsMyEnrollments,
  useLmsMyAssessmentAttempts,
  useLmsSubmitAssessment,
  useLmsUpdateLessonProgress,
} from '@/hooks/use-lms';

function formatSeconds(totalSeconds: number) {
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  return [hours, minutes, seconds].map((value) => String(value).padStart(2, '0')).join(':');
}

export default function CourseWorkspacePage() {
  const params = useParams<{ courseId: string }>();
  const courseId = Number(params.courseId);
  const safeCourseId = Number.isFinite(courseId) ? courseId : null;

  const { data: course, isLoading: loadingCourse } = useLmsCourse(safeCourseId);
  const { data: structure, isLoading: loadingStructure } = useLmsCourseStructure(safeCourseId);
  const { data: assessments } = useLmsCourseAssessments(safeCourseId);
  const { data: enrollments } = useLmsMyEnrollments();
  const { data: gradebook } = useLmsCourseGradebook(safeCourseId);
  const updateProgress = useLmsUpdateLessonProgress(safeCourseId);

  const [selectedAssessmentId, setSelectedAssessmentId] = useState<number | null>(null);
  const { data: assessment } = useLmsAssessment(selectedAssessmentId);
  const { data: attempts } = useLmsMyAssessmentAttempts(selectedAssessmentId);
  const submitAssessment = useLmsSubmitAssessment(selectedAssessmentId);
  const gradeAttempt = useLmsGradeAssessmentAttempt(safeCourseId, selectedAssessmentId);
  const [responses, setResponses] = useState<Record<string, string>>({});
  const [feedback, setFeedback] = useState<string | null>(null);

  useEffect(() => {
    if (!selectedAssessmentId && assessments && assessments.length > 0) {
      setSelectedAssessmentId(assessments[0].id);
    }
  }, [assessments, selectedAssessmentId]);

  useEffect(() => {
    setResponses({});
  }, [selectedAssessmentId]);

  const lessons = useMemo(
    () => structure?.modules.flatMap((module) => module.lessons) ?? [],
    [structure]
  );
  const isInstructor = (enrollments ?? []).some((row) => row.course_id === safeCourseId && row.role === 'instructor');
  const completedLessons = lessons.filter((lesson) => lesson.isCompleted).length;
  const completionRate = lessons.length === 0 ? 0 : Math.round((completedLessons / lessons.length) * 100);

  async function handleProgressUpdate(lessonId: number, nextSeconds: number, isCompleted: boolean) {
    const result = await updateProgress.mutateAsync({
      lessonId,
      timeSpent: formatSeconds(nextSeconds),
      isCompleted,
    });
    setFeedback(result.unlockedAchievements.length > 0 ? `Unlocked: ${result.unlockedAchievements.join(', ')}` : 'Progress updated.');
  }

  async function handleAssessmentSubmit() {
    const result = await submitAssessment.mutateAsync(responses);
    setFeedback(`Assessment submitted. Score: ${result.score}/${assessment?.max_score ?? 0}.`);
  }

  async function handleGradeAttempt(attemptId: number) {
    const result = await gradeAttempt.mutateAsync({ attemptId, manualScores: {} });
    setFeedback(`Attempt #${result.id} marked as ${result.status}.`);
  }

  return (
    <SectionShell
      title={course?.title ?? 'Course Workspace'}
      description={course?.description ?? 'Track lesson progress and complete course assessments.'}
      actions={<Link href="/my-courses" className="text-sm font-medium text-blue-600 hover:underline">Back to My Courses</Link>}
    >
      {loadingCourse || loadingStructure ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : !course || !structure ? (
        <EmptyHint title="Course not found" subtitle="Return to My Courses and reopen a valid workspace." />
      ) : (
        <div className="space-y-6">
          <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
            <StatCard label="Modules" value={String(structure.modules.length)} />
            <StatCard label="Lessons" value={String(lessons.length)} />
            <StatCard label="Completed" value={String(completedLessons)} />
            <StatCard label="Progress" value={`${completionRate}%`} helper={`${completedLessons}/${lessons.length || 0} lessons complete`} />
          </div>

          {feedback ? <div className="rounded-xl border border-blue-200 bg-blue-50 px-4 py-3 text-sm text-blue-700">{feedback}</div> : null}

          <div className="grid grid-cols-1 gap-6 xl:grid-cols-[minmax(0,1.2fr)_minmax(340px,0.8fr)]">
            <div className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle className="text-xl">Lessons and Progress</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {structure.modules.length === 0 ? (
                    <EmptyHint title="No modules yet" subtitle="Add course modules and lessons from the admin tools." />
                  ) : (
                    structure.modules.map((module) => (
                      <div key={module.id} className="space-y-3 rounded-2xl border border-gray-200 p-4">
                        <div>
                          <p className="text-base font-semibold text-gray-900">{module.title}</p>
                          <p className="text-sm text-gray-500">{module.description}</p>
                        </div>

                        <div className="space-y-3">
                          {module.lessons.map((lesson) => (
                            <div key={lesson.id} className="rounded-xl border border-gray-200 bg-gray-50 p-4">
                              <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                                <div className="space-y-2">
                                  <p className="text-sm font-semibold text-gray-900">{lesson.title}</p>
                                  <p className="text-sm text-gray-600">{lesson.content}</p>
                                  <div className="flex flex-wrap gap-2 text-xs text-gray-500">
                                    <span>{lesson.duration_minutes} minutes planned</span>
                                    <span>{formatSeconds(lesson.timeSpentSeconds)} tracked</span>
                                    <span>{lesson.isCompleted ? 'Completed' : 'In progress'}</span>
                                  </div>
                                </div>

                                <div className="flex flex-wrap gap-2">
                                  <Button
                                    size="sm"
                                    variant="outline"
                                    isLoading={updateProgress.isPending}
                                    onClick={() => handleProgressUpdate(lesson.id, lesson.timeSpentSeconds + 900, lesson.isCompleted)}
                                  >
                                    Add 15 min
                                  </Button>
                                  <Button
                                    size="sm"
                                    isLoading={updateProgress.isPending}
                                    onClick={() => handleProgressUpdate(lesson.id, Math.max(lesson.timeSpentSeconds, lesson.duration_minutes * 60), !lesson.isCompleted)}
                                  >
                                    {lesson.isCompleted ? 'Mark Incomplete' : 'Mark Complete'}
                                  </Button>
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    ))
                  )}
                </CardContent>
              </Card>

              {isInstructor ? (
                <Card>
                  <CardHeader>
                    <CardTitle className="text-xl">Instructor Gradebook</CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    {(gradebook ?? []).length === 0 ? (
                      <p className="text-sm text-gray-500">No learner attempts have been recorded for this course yet.</p>
                    ) : (
                      (gradebook ?? []).map((entry) => (
                        <div key={entry.userId} className="rounded-2xl border border-gray-200 p-4">
                          <p className="text-sm font-semibold text-gray-900">Learner #{entry.userId}</p>
                          <p className="mt-1 text-xs text-gray-500">Average score: {Math.round(entry.averageScore)}</p>
                          <div className="mt-3 space-y-2">
                            {entry.scores.map((score) => (
                              <div key={score.attemptId} className="flex items-center justify-between rounded-xl border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-700">
                                <div>
                                  <p className="font-medium text-gray-900">Attempt #{score.attemptId}</p>
                                  <p className="text-xs text-gray-500">Assessment #{score.assessmentId} • score {score.score}</p>
                                </div>
                                <Button
                                  size="sm"
                                  variant="outline"
                                  isLoading={gradeAttempt.isPending}
                                  onClick={() => handleGradeAttempt(score.attemptId)}
                                >
                                  Mark graded
                                </Button>
                              </div>
                            ))}
                          </div>
                        </div>
                      ))
                    )}
                  </CardContent>
                </Card>
              ) : null}
            </div>

            <div className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle className="text-xl">Interactive Assessments</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {(assessments ?? []).length === 0 ? (
                    <EmptyHint title="No assessments yet" subtitle="Teacher-authored exercises will appear here once they are attached to this course." />
                  ) : (
                    <>
                      <div className="flex flex-wrap gap-2">
                        {(assessments ?? []).map((item) => (
                          <button
                            key={item.id}
                            type="button"
                            onClick={() => setSelectedAssessmentId(item.id)}
                            className={`rounded-full px-3 py-2 text-sm transition ${selectedAssessmentId === item.id ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}
                          >
                            {item.title}
                          </button>
                        ))}
                      </div>

                      {assessment ? (
                        <div className="space-y-4 rounded-2xl border border-gray-200 p-4">
                          <div>
                            <p className="text-base font-semibold text-gray-900">{assessment.title}</p>
                            <p className="text-sm text-gray-600">{assessment.description}</p>
                          </div>

                          {assessment.questions.map((question, index) => (
                            <div key={question.id} className="space-y-3 rounded-xl border border-gray-200 bg-gray-50 p-4">
                              <p className="text-sm font-semibold text-gray-900">Question {index + 1}</p>
                              <p className="text-sm text-gray-700">{question.prompt}</p>

                              {question.is_manual ? (
                                <textarea
                                  value={responses[String(question.id)] ?? ''}
                                  onChange={(event) => setResponses((current) => ({ ...current, [String(question.id)]: event.target.value }))}
                                  className="min-h-28 w-full rounded-xl border border-gray-300 bg-white px-4 py-3 text-sm text-slate-950 outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-100"
                                  placeholder="Write your answer"
                                />
                              ) : (
                                <div className="space-y-2">
                                  {question.options_json.map((option) => (
                                    <label key={option} className="flex cursor-pointer items-center gap-3 rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm text-gray-700 hover:border-blue-300">
                                      <input
                                        type="radio"
                                        name={`question-${question.id}`}
                                        checked={(responses[String(question.id)] ?? '') === option}
                                        onChange={() => setResponses((current) => ({ ...current, [String(question.id)]: option }))}
                                      />
                                      <span>{option}</span>
                                    </label>
                                  ))}
                                </div>
                              )}
                            </div>
                          ))}

                          <Button isLoading={submitAssessment.isPending} onClick={handleAssessmentSubmit}>
                            Submit Assessment
                          </Button>
                        </div>
                      ) : null}
                    </>
                  )}
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="text-xl">Attempt History</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  {(attempts ?? []).length === 0 ? (
                    <p className="text-sm text-gray-500">No attempts submitted yet for the selected assessment.</p>
                  ) : (
                    (attempts ?? []).map((attempt) => (
                      <div key={attempt.id} className="rounded-xl border border-gray-200 px-4 py-3 text-sm text-gray-700">
                        <p className="font-medium text-gray-900">Attempt #{attempt.id}</p>
                        <p className="text-xs text-gray-500">Submitted {new Date(attempt.submitted_at).toLocaleString()}</p>
                        <p className="mt-1">Score: {attempt.score}</p>
                        <p>Status: {attempt.status}</p>
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