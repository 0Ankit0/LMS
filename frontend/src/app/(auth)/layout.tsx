import { Link } from '@/lib/router';
import { CheckCircle2 } from 'lucide-react';

const authHighlights = [
  'Course delivery, assessments, and progress tracking in one workspace',
  'Typed React + Vite frontend with shared LMS auth flows',
  'RBAC, notifications, and admin operations included',
];

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="relative min-h-screen overflow-hidden bg-[linear-gradient(180deg,#f8fbff_0%,#eef4ff_44%,#ffffff_100%)]">
      <div className="pointer-events-none absolute inset-x-0 top-0 h-[36rem] bg-[radial-gradient(circle_at_top_left,rgba(37,99,235,0.18),transparent_34%),radial-gradient(circle_at_center_right,rgba(14,165,233,0.14),transparent_28%)]" />

      <div className="relative mx-auto grid min-h-screen max-w-6xl gap-12 px-4 py-10 sm:px-6 lg:grid-cols-[minmax(0,1fr)_minmax(420px,460px)] lg:items-center lg:px-8">
        <div className="hidden max-w-xl lg:block">
          <Link href="/" className="inline-flex items-center gap-3" aria-label="LMS home">
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-slate-950 text-base font-semibold text-white shadow-lg shadow-slate-900/15">
              F
            </div>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.3em] text-blue-600">Learning</p>
              <p className="text-lg font-semibold text-slate-950">Management Suite</p>
            </div>
          </Link>

          <div className="mt-10 space-y-6">
            <p className="text-sm font-semibold uppercase tracking-[0.3em] text-blue-600">
              Secure LMS access
            </p>
            <h1 className="text-5xl font-semibold tracking-tight text-slate-950">
              Sign in to the learning platform used by students, instructors, parents, and admins.
            </h1>
            <p className="text-lg leading-8 text-slate-600">
              Authentication sits on top of the same FastAPI, PostgreSQL, and React foundation
              as the rest of the LMS, so each dashboard opens directly into the live learning workflows.
            </p>
          </div>

          <div className="mt-8 space-y-4">
            {authHighlights.map((highlight) => (
              <div
                key={highlight}
                className="flex items-center gap-3 rounded-2xl border border-white/70 bg-white/75 px-4 py-3 text-sm text-slate-700 shadow-sm backdrop-blur"
              >
                <CheckCircle2 className="h-4 w-4 shrink-0 text-blue-600" />
                <span>{highlight}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="flex items-center justify-center">
          <div className="w-full max-w-md space-y-6">
            <Link href="/" className="inline-flex items-center gap-3 lg:hidden" aria-label="LMS home">
              <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-slate-950 text-base font-semibold text-white shadow-lg shadow-slate-900/15">
                F
              </div>
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.3em] text-blue-600">Learning</p>
                <p className="text-lg font-semibold text-slate-950">Management Suite</p>
              </div>
            </Link>

            {children}
          </div>
        </div>
      </div>
    </div>
  );
}
