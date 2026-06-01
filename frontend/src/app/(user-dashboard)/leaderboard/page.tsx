'use client';

import { SectionShell, StatCard, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent } from '@/components/ui/card';
import { useLmsLeaderboard } from '@/hooks/use-lms';

export default function LeaderboardPage() {
  const { data, isLoading } = useLmsLeaderboard();
  const leaderboard = (data ?? []).slice().sort((a, b) => b.points - a.points);

  return (
    <SectionShell
      title="Leaderboard"
      description="Track top performers and celebrate learning progress across the platform."
    >
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <StatCard label="Total Ranked" value={String(leaderboard.length)} />
        <StatCard label="Top Score" value={String(leaderboard[0]?.points ?? 0)} />
        <StatCard label="Top User" value={leaderboard[0] ? `#${leaderboard[0].userId}` : 'N/A'} />
      </div>

      {isLoading ? (
        <div className="h-40 animate-pulse rounded-xl bg-gray-100" />
      ) : leaderboard.length === 0 ? (
        <EmptyHint title="No scores available" subtitle="Complete activities to appear on the leaderboard." />
      ) : (
        <Card>
          <CardContent className="p-4">
            <div className="space-y-2">
              {leaderboard.map((entry, index) => (
                <div
                  key={`${entry.userId}-${index}`}
                  className="flex items-center justify-between rounded-lg border border-gray-200 px-3 py-2"
                >
                  <div className="flex items-center gap-3">
                    <span className="w-8 text-sm font-semibold text-gray-700">#{index + 1}</span>
                    <span className="text-sm text-gray-900">User #{entry.userId}</span>
                  </div>
                  <span className="text-sm font-semibold text-blue-700">{entry.points} pts</span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </SectionShell>
  );
}
