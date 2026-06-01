'use client';

import { useAuthStore } from '@/store/auth-store';
import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useLmsMyAchievements } from '@/hooks/use-lms';

export default function AchievementsPage() {
  const user = useAuthStore((state) => state.user);
  const numericUserId = user?.id ? Number(user.id) : null;
  const { data, isLoading } = useLmsMyAchievements(Number.isFinite(numericUserId) ? numericUserId : null);
  const achievements = data ?? [];

  return (
    <SectionShell
      title="Achievements"
      description="View milestones and badges unlocked through your learning activity."
    >
      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : achievements.length === 0 ? (
        <EmptyHint title="No achievements yet" subtitle="Keep progressing through courses to unlock achievements." />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {achievements.map((achievement: any) => (
            <Card key={achievement.id}>
              <CardHeader>
                <CardTitle>{achievement.title}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <p className="text-sm text-gray-700">{achievement.description}</p>
                <p className="text-xs text-gray-500">Points: {achievement.points ?? 0}</p>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
