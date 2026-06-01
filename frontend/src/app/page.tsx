'use client';

import { useEffect } from 'react';
import { Link, useRouter } from '@/lib/router';
import { useAuthStore } from '@/store/auth-store';
import {
  ArrowRight,
  BookOpen,
  CalendarDays,
  ClipboardCheck,
  Files,
  GraduationCap,
  LineChart,
  MessageSquare,
  School,
  ShieldCheck,
  Trophy,
  Users,
} from 'lucide-react';

const learnerHighlights = [
  {
    icon: BookOpen,
    title: 'Course catalog and enrollment',
    description: 'Discover courses, enroll instantly, and continue from My Courses with synchronized progress.',
  },
  {
    icon: ClipboardCheck,
    title: 'Assessments and certificates',
    description: 'Submit assessments, receive grading outcomes, and access earned certificates in one flow.',
  },
  {
    icon: MessageSquare,
    title: 'Messages and announcements',
    description: 'Stay connected with course conversations and institution-wide announcements.',
  },
  {
    icon: Trophy,
    title: 'Gamification and leaderboard',
    description: 'Track achievements, compare rankings, and keep learners motivated with measurable milestones.',
  },
];

const adminHighlights = [
  {
    icon: School,
    title: 'Course administration',
    description: 'Create, update, and manage catalog content, taxonomies, and competency mappings.',
  },
  {
    icon: Users,
    title: 'Admissions and user operations',
    description: 'Review applications, manage users, and link parent-student relationships with auditability.',
  },
  {
    icon: CalendarDays,
    title: 'Attendance and scheduling',
    description: 'Monitor attendance data and keep academic calendars aligned to delivery plans.',
  },
  {
    icon: LineChart,
    title: 'Reporting and analytics',
    description: 'Analyze student engagement and completion trends for evidence-based decisions.',
  },
];

const workflowChips = [
  'Course Catalog',
  'My Courses',
  'Announcements',
  'Assessments',
  'Messages',
  'Leaderboard',
  'Achievements',
  'Certificates',
  'Notes',
  'Calendar',
  'File Management',
  'Parent Dashboard',
  'Admissions',
  'Competencies',
  'Attendance',
  'Taxonomy',
  'Reports',
];

const statTiles = [
  { value: '91/91', label: 'endpoint parity checks passing' },
  { value: 'FastAPI', label: 'async LMS backend' },
  { value: 'React + Vite', label: 'typed frontend dashboards' },
];

const primaryLinkClass =
  'inline-flex items-center justify-center gap-2 rounded-full bg-slate-950 px-6 py-3 text-sm font-semibold text-white transition hover:bg-slate-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400';

const secondaryLinkClass =
  'inline-flex items-center justify-center gap-2 rounded-full border border-slate-300 bg-white/75 px-6 py-3 text-sm font-semibold text-slate-900 shadow-sm transition hover:border-slate-400 hover:bg-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-300';

export default function Home() {
  const router = useRouter();
  const { isAuthenticated } = useAuthStore();

  useEffect(() => {
    if (isAuthenticated) {
      router.replace('/dashboard');
    }
  }, [isAuthenticated, router]);

  return (
    <div className="min-h-screen overflow-x-hidden bg-[linear-gradient(180deg,#f6fbff_0%,#edf7ff_45%,#ffffff_100%)] text-slate-950">
      <div className="pointer-events-none absolute inset-x-0 top-0 h-[34rem] bg-[radial-gradient(circle_at_top_left,rgba(2,132,199,0.2),transparent_35%),radial-gradient(circle_at_top_right,rgba(14,116,144,0.15),transparent_30%)]" />

      <header className="sticky top-0 z-50 border-b border-slate-200/80 bg-white/85 backdrop-blur-xl">
        <div className="mx-auto flex h-20 max-w-6xl items-center justify-between px-4 sm:px-6 lg:px-8">
          <Link href="/" className="flex items-center gap-3" aria-label="LMS home">
            <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-slate-950 text-base font-semibold text-white shadow-lg shadow-slate-900/15">
              L
            </div>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.3em] text-cyan-700">Learning</p>
              <p className="text-lg font-semibold text-slate-950">Management Suite</p>
            </div>
          </Link>

          <nav className="flex items-center gap-3 sm:gap-4">
            <Link href="/login" className="text-sm font-medium text-slate-700 transition hover:text-slate-950">
              Sign in
            </Link>
            <Link href="/signup" className={`${primaryLinkClass} px-5 py-2.5`}>
              Create account
            </Link>
          </nav>
        </div>
      </header>

      <main className="relative">
        <section className="px-4 pb-16 pt-14 sm:px-6 lg:px-8 lg:pb-24 lg:pt-20">
          <div className="mx-auto grid max-w-6xl gap-12 lg:grid-cols-[minmax(0,1.1fr)_minmax(360px,0.9fr)] lg:items-center">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full border border-cyan-200 bg-white/80 px-4 py-2 text-sm font-medium text-slate-700 shadow-sm backdrop-blur">
                <span className="h-2.5 w-2.5 rounded-full bg-emerald-500" />
                End-to-end LMS workflows are now live
              </div>

              <h1 className="mt-6 max-w-3xl text-5xl font-semibold tracking-tight text-slate-950 sm:text-6xl">
                Deliver learning operations on a complete LMS platform.
              </h1>

              <p className="mt-6 max-w-2xl text-lg leading-8 text-slate-600 sm:text-xl">
                This platform combines learner journeys, instructor operations, admissions, parent visibility,
                and reporting in one integrated FastAPI and React architecture.
              </p>

              <div className="mt-8 flex flex-col gap-3 sm:flex-row">
                <Link href="/signup" className={`${primaryLinkClass} w-full px-7 py-3.5 text-base sm:w-auto`}>
                  Start learning
                  <ArrowRight className="h-4 w-4" />
                </Link>
                <Link href="/course-catalog" className={`${secondaryLinkClass} w-full px-7 py-3.5 text-base sm:w-auto`}>
                  Browse catalog
                </Link>
              </div>

              <div className="mt-10 grid gap-4 sm:grid-cols-3">
                {statTiles.map((stat) => (
                  <div
                    key={stat.label}
                    className="rounded-3xl border border-white/70 bg-white/75 px-5 py-4 shadow-sm shadow-slate-900/5 backdrop-blur"
                  >
                    <p className="text-lg font-semibold text-slate-950">{stat.value}</p>
                    <p className="mt-1 text-sm text-slate-600">{stat.label}</p>
                  </div>
                ))}
              </div>
            </div>

            <div className="relative">
              <div className="absolute inset-0 -z-10 rounded-[2rem] bg-[linear-gradient(135deg,rgba(6,78,59,0.92),rgba(8,47,73,0.9),rgba(8,145,178,0.85))] shadow-[0_30px_80px_rgba(8,47,73,0.24)]" />
              <div className="rounded-[2rem] border border-white/40 bg-white/10 p-6 backdrop-blur-xl">
                <div className="rounded-[1.5rem] border border-white/10 bg-slate-950/20 p-6">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-[0.3em] text-cyan-100/90">
                        Primary learner workflows
                      </p>
                      <h2 className="mt-3 text-2xl font-semibold text-white">
                        Learn, submit, collaborate, and progress.
                      </h2>
                    </div>
                    <div className="rounded-2xl bg-white/10 p-3 text-cyan-100">
                      <GraduationCap className="h-5 w-5" />
                    </div>
                  </div>

                  <div className="mt-6 space-y-4">
                    {learnerHighlights.map((item) => (
                      <div key={item.title} className="rounded-3xl border border-white/10 bg-white/6 px-5 py-4">
                        <div className="flex items-start gap-4">
                          <div className="mt-0.5 rounded-2xl bg-white/10 p-3 text-cyan-100">
                            <item.icon className="h-5 w-5" />
                          </div>
                          <div>
                            <h3 className="text-base font-semibold text-white">{item.title}</h3>
                            <p className="mt-1 text-sm leading-6 text-slate-200/90">{item.description}</p>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <section className="px-4 py-16 sm:px-6 lg:px-8">
          <div className="mx-auto max-w-6xl">
            <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
              <div className="max-w-3xl">
                <p className="text-sm font-semibold uppercase tracking-[0.3em] text-cyan-700">
                  Administration and governance
                </p>
                <h2 className="mt-3 text-3xl font-semibold tracking-tight text-slate-950 sm:text-4xl">
                  Built for schools, academies, and training operations.
                </h2>
              </div>
              <p className="max-w-2xl text-base leading-7 text-slate-600 sm:text-lg">
                Control admissions, configure taxonomy, manage competencies, audit attendance,
                and keep analytics visible for both delivery and leadership teams.
              </p>
            </div>

            <div className="mt-10 grid gap-6 lg:grid-cols-4">
              {adminHighlights.map((item) => (
                <div
                  key={item.title}
                  className="rounded-[1.75rem] border border-slate-200/80 bg-white/85 p-7 shadow-[0_18px_45px_rgba(15,23,42,0.06)] backdrop-blur"
                >
                  <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-cyan-50 text-cyan-700">
                    <item.icon className="h-7 w-7" />
                  </div>
                  <h3 className="mt-6 text-xl font-semibold text-slate-950">{item.title}</h3>
                  <p className="mt-3 text-sm leading-7 text-slate-600 sm:text-base">{item.description}</p>
                </div>
              ))}
            </div>
          </div>
        </section>

        <section className="px-4 pb-24 sm:px-6 lg:px-8">
          <div className="mx-auto grid max-w-6xl gap-6 lg:grid-cols-[minmax(0,1.05fr)_minmax(320px,0.95fr)]">
            <div className="rounded-[2rem] bg-slate-950 px-8 py-10 text-white shadow-[0_30px_80px_rgba(15,23,42,0.18)] sm:px-10 sm:py-12">
              <p className="text-sm font-semibold uppercase tracking-[0.3em] text-cyan-200/90">
                Use the LMS now
              </p>
              <h2 className="mt-4 text-3xl font-semibold tracking-tight sm:text-4xl">
                Start with learner workflows or jump straight to admin controls.
              </h2>
              <p className="mt-5 max-w-2xl text-base leading-8 text-slate-300 sm:text-lg">
                Authentication, dashboards, and route structure are already aligned with LMS endpoints.
                You can move from onboarding to active operations without placeholder screens.
              </p>

              <div className="mt-8 flex flex-col gap-3 sm:flex-row">
                <Link href="/dashboard" className="inline-flex items-center justify-center gap-2 rounded-full bg-white px-6 py-3 text-sm font-semibold text-slate-950 transition hover:bg-slate-100 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white/70">
                  Open workspace
                  <ArrowRight className="h-4 w-4" />
                </Link>
                <Link href="/admin/dashboard" className="inline-flex items-center justify-center rounded-full border border-white/20 px-6 py-3 text-sm font-semibold text-white transition hover:bg-white/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white/40">
                  Open admin panel
                </Link>
              </div>
            </div>

            <div className="rounded-[2rem] border border-slate-200/80 bg-white/90 p-8 shadow-[0_18px_45px_rgba(15,23,42,0.06)] backdrop-blur">
              <p className="text-sm font-semibold uppercase tracking-[0.3em] text-cyan-700">
                Workflow coverage
              </p>
              <h3 className="mt-4 text-2xl font-semibold text-slate-950">Pages implemented across LMS modules.</h3>
              <div className="mt-6 flex flex-wrap gap-2">
                {workflowChips.map((chip) => (
                  <span
                    key={chip}
                    className="inline-flex items-center rounded-full border border-slate-200 bg-slate-50 px-3 py-1.5 text-xs font-medium text-slate-700"
                  >
                    {chip}
                  </span>
                ))}
              </div>
              <div className="mt-6 rounded-2xl border border-cyan-100 bg-cyan-50 px-4 py-3 text-sm text-cyan-900">
                <div className="flex items-start gap-2">
                  <ShieldCheck className="mt-0.5 h-4 w-4 shrink-0" />
                  <p>
                    Security-aware auth, role-aware dashboards, and LMS API connectivity are integrated as one delivery surface.
                  </p>
                </div>
              </div>
              <div className="mt-4 rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-700">
                <div className="flex items-start gap-2">
                  <Files className="mt-0.5 h-4 w-4 shrink-0" />
                  <p>
                    File management, notes, calendar events, and notifications are available in dedicated LMS views.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </section>
      </main>

      <footer className="border-t border-slate-200/80 px-4 py-8 sm:px-6 lg:px-8">
        <div className="mx-auto flex max-w-6xl flex-col gap-2 text-sm text-slate-500 sm:flex-row sm:items-center sm:justify-between">
          <p>© {new Date().getFullYear()} Learning Management Suite</p>
          <p>FastAPI backend. Typed frontend. End-to-end LMS workflows.</p>
        </div>
      </footer>
    </div>
  );
}
