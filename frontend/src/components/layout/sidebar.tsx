'use client';

import { Link, usePathname } from '@/lib/router';
import {
  Home,
  Bell,
  Settings,
  Building2,
  User,
  CreditCard,
  Key,
  ArrowRight,
  Map,
  BookOpen,
  Library,
  Megaphone,
  ClipboardCheck,
  MessageSquare,
  Trophy,
  Award,
  ScrollText,
  CalendarDays,
  FolderOpen,
  Users,
} from 'lucide-react';
import { OrgSwitcher } from './org-switcher';
import { useSystemCapabilities } from '@/hooks/use-system';
import { useAuthStore } from '@/store/auth-store';
import { useLmsMyEnrollments, useLmsParentChildren } from '@/hooks/use-lms';

const mainNavigation = [
  { name: 'Dashboard', href: '/dashboard', icon: Home, audience: 'all' },
  { name: 'Course Catalog', href: '/course-catalog', icon: Library, audience: 'course' },
  { name: 'My Courses', href: '/my-courses', icon: BookOpen, audience: 'course' },
  { name: 'Announcements', href: '/announcements', icon: Megaphone, audience: 'course' },
  { name: 'Assessments', href: '/assessments', icon: ClipboardCheck, audience: 'course' },
  { name: 'Messages', href: '/messages', icon: MessageSquare, audience: 'course' },
  { name: 'Leaderboard', href: '/leaderboard', icon: Trophy, audience: 'student' },
  { name: 'Achievements', href: '/achievements', icon: Award, audience: 'student' },
  { name: 'Certificates', href: '/certificates', icon: ScrollText, audience: 'student' },
  { name: 'Notes', href: '/notes', icon: ScrollText, audience: 'course' },
  { name: 'Calendar', href: '/calendar', icon: CalendarDays, audience: 'course' },
  { name: 'My Files', href: '/files', icon: FolderOpen, audience: 'course' },
  { name: 'Parent Dashboard', href: '/parent-dashboard', icon: Users, audience: 'parent' },
  { name: 'Profile', href: '/profile', icon: User, audience: 'all' },
  { name: 'Tenants', href: '/tenants', icon: Building2, feature: 'multitenancy' },
  { name: 'Payments', href: '/finances', icon: CreditCard, feature: 'finance' },
  { name: 'Notifications', href: '/notifications', icon: Bell, feature: 'notifications', audience: 'all' },
  { name: 'Maps', href: '/maps', icon: Map, feature: 'maps' },
  { name: 'Active Sessions', href: '/tokens', icon: Key, feature: 'auth', audience: 'all' },
  { name: 'Settings', href: '/settings', icon: Settings, audience: 'all' },
];

export function Sidebar() {
  const pathname = usePathname();
  const { data: capabilities } = useSystemCapabilities();
  const user = useAuthStore((state) => state.user);
  const { data: enrollments } = useLmsMyEnrollments();
  const { data: parentChildren } = useLmsParentChildren();
  const appName = process.env.NEXT_PUBLIC_APP_NAME ?? 'Learning Management Suite';

  const hasStudentRole = (enrollments ?? []).some((row) => row.role === 'student');
  const hasInstructorRole = (enrollments ?? []).some((row) => row.role === 'instructor');
  const hasCourseAccess = hasStudentRole || hasInstructorRole || Boolean(user?.is_superuser);
  const hasParentAccess = (parentChildren?.length ?? 0) > 0;

  const visibleNavigation = mainNavigation.filter(
    (item) => {
      if (item.feature && capabilities?.modules[item.feature] === false) {
        return false;
      }

      if (item.audience === 'student' && !hasStudentRole) {
        return false;
      }

      if (item.audience === 'course' && !hasCourseAccess) {
        return false;
      }

      if (item.audience === 'parent' && !hasParentAccess) {
        return false;
      }

      return true;
    }
  );
  const showAdminSwitch = Boolean(user?.is_superuser);

  return (
    <aside className="fixed inset-y-0 left-0 z-10 w-64 bg-white border-r border-gray-200">
      <div className="flex h-16 items-center justify-center border-b border-gray-200">
        <Link href="/dashboard" className="text-xl font-bold text-blue-600">
          {appName}
        </Link>
      </div>
      <OrgSwitcher />
      <nav className="flex flex-col gap-1 p-4 pt-0">
        <div className="mb-2 px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] text-gray-400">
          Workspace
        </div>
        {visibleNavigation.map((item) => {
          const isActive =
            pathname === item.href || pathname.startsWith(`${item.href}/`);
          return (
            <Link
              key={item.name}
              href={item.href}
              className={`flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                isActive
                  ? 'bg-blue-50 text-blue-600'
                  : 'text-gray-700 hover:bg-gray-100'
              }`}
            >
              <item.icon className="h-5 w-5" />
              {item.name}
            </Link>
          );
        })}
        {showAdminSwitch ? (
          <div className="mt-4 pt-4 border-t border-gray-200">
            <Link
              href="/admin/dashboard"
              className="flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium text-gray-700 transition-colors hover:bg-gray-100"
            >
              <ArrowRight className="h-5 w-5" />
              Open Admin Panel
            </Link>
          </div>
        ) : null}
      </nav>
    </aside>
  );
}
