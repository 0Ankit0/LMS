'use client';

import { useDeferredValue, useMemo, useState } from 'react';
import { Activity, Radio, Search, ShieldAlert } from 'lucide-react';

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useSearchParams } from '@/lib/router';
import { mergeObservabilityLogs } from '@/lib/observability';
import { useLiveObservabilityLogs, useObservabilityLogs, useObservabilitySummary } from '@/hooks/use-observability';

const LEVEL_OPTIONS = ['all', 'info', 'warning', 'error'];

export default function AdminLogsPage() {
  const searchParams = useSearchParams();
  const linkedLogId = searchParams.get('logId');
  const [search, setSearch] = useState('');
  const [level, setLevel] = useState('all');
  const [liveMode, setLiveMode] = useState(true);
  const deferredSearch = useDeferredValue(search);

  const filters = useMemo(
    () => ({
      limit: 40,
      search: deferredSearch || undefined,
      level: level === 'all' ? undefined : level,
      log_id: linkedLogId || undefined,
    }),
    [deferredSearch, level, linkedLogId]
  );

  const summaryQuery = useObservabilitySummary();
  const logsQuery = useObservabilityLogs(filters);
  const latestId = logsQuery.data?.items[0]?.id ?? null;
  const liveLogsQuery = useLiveObservabilityLogs(
    latestId,
    liveMode && !linkedLogId,
    { level: filters.level }
  );

  const logs = useMemo(
    () => mergeObservabilityLogs(logsQuery.data?.items ?? [], liveLogsQuery.data ?? []),
    [liveLogsQuery.data, logsQuery.data?.items]
  );

  return (
    <div className="space-y-6">
      <Card className="overflow-hidden border-none bg-[linear-gradient(135deg,#0f172a_0%,#1e293b_55%,#2563eb_100%)] text-white shadow-[0_24px_80px_rgba(15,23,42,0.24)]">
        <CardContent className="relative px-6 py-8">
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(96,165,250,0.22),transparent_34%),radial-gradient(circle_at_bottom_right,rgba(59,130,246,0.18),transparent_28%)]" />
          <div className="relative grid gap-6 lg:grid-cols-[1.2fr,0.8fr]">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.26em] text-blue-200">Observability</p>
              <h1 className="mt-3 text-3xl font-semibold tracking-tight">Live logs</h1>
              <p className="mt-3 max-w-2xl text-sm text-slate-200">
                Inspect API traffic, auth events, and error signals as they hit the platform. Use the live feed for recent issues and the persisted list for investigation.
              </p>
            </div>

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3 lg:grid-cols-1">
              <div className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3">
                <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-200">Logs 24h</p>
                <p className="mt-2 text-2xl font-semibold">{summaryQuery.data?.total_logs_24h ?? 0}</p>
              </div>
              <div className="rounded-2xl border border-amber-300/20 bg-amber-300/10 px-4 py-3 text-amber-100">
                <p className="text-xs font-semibold uppercase tracking-[0.18em]">Warnings</p>
                <p className="mt-2 text-2xl font-semibold">{summaryQuery.data?.warning_logs_24h ?? 0}</p>
              </div>
              <div className="rounded-2xl border border-red-300/20 bg-red-300/10 px-4 py-3 text-red-100">
                <p className="text-xs font-semibold uppercase tracking-[0.18em]">Errors</p>
                <p className="mt-2 text-2xl font-semibold">{summaryQuery.data?.error_logs_24h ?? 0}</p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-xl">
            <Search className="h-5 w-5" />
            Log filters
          </CardTitle>
          <CardDescription>Search persisted logs, filter by level, and optionally watch live entries flow into the list.</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-3 lg:grid-cols-[minmax(0,1fr)_180px_auto]">
          <Input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="search message, path, request id" />
          <select
            value={level}
            onChange={(event) => setLevel(event.target.value)}
            className="rounded-xl border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm"
          >
            {LEVEL_OPTIONS.map((option) => (
              <option key={option} value={option}>{option}</option>
            ))}
          </select>
          <Button variant={liveMode ? 'primary' : 'outline'} onClick={() => setLiveMode((current) => !current)}>
            <Radio className="mr-2 h-4 w-4" />
            {liveMode ? 'Live on' : 'Live off'}
          </Button>
        </CardContent>
      </Card>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.15fr)_minmax(320px,0.85fr)]">
        <Card>
          <CardHeader>
            <CardTitle className="text-xl">Event stream</CardTitle>
            <CardDescription>
              {linkedLogId ? 'Focused on the log entry linked from an incident.' : 'Newest entries first, merging persisted results with the live feed when enabled.'}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {logs.length === 0 ? (
              <div className="rounded-2xl border border-dashed border-gray-200 bg-gray-50 px-4 py-10 text-center">
                <Activity className="mx-auto h-7 w-7 text-gray-400" />
                <p className="mt-3 text-sm font-medium text-gray-900">No logs match the current filters.</p>
                <p className="mt-1 text-xs text-gray-500">Change the filter values or wait for new events to arrive.</p>
              </div>
            ) : (
              logs.map((entry) => (
                <div key={entry.id} className={`rounded-2xl border px-4 py-4 ${entry.id === linkedLogId ? 'border-blue-300 bg-blue-50' : 'border-gray-200 bg-white'}`}>
                  <div className="flex flex-wrap items-center gap-2">
                    <span className={`rounded-full px-2.5 py-1 text-[11px] font-semibold uppercase tracking-[0.18em] ${entry.level === 'error' ? 'bg-red-100 text-red-700' : entry.level === 'warning' ? 'bg-amber-100 text-amber-700' : 'bg-slate-100 text-slate-700'}`}>
                      {entry.level}
                    </span>
                    <span className="rounded-full bg-slate-100 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-700">
                      {entry.source}
                    </span>
                    <span className="text-xs text-gray-500">{new Date(entry.timestamp).toLocaleString()}</span>
                  </div>
                  <p className="mt-3 text-sm font-medium text-gray-900">{entry.message}</p>
                  <div className="mt-3 grid gap-2 text-xs text-gray-500 md:grid-cols-2 xl:grid-cols-4">
                    <span>Event: {entry.event_code || 'n/a'}</span>
                    <span>Request: {entry.request_id || 'n/a'}</span>
                    <span>Route: {entry.method} {entry.path}</span>
                    <span>Status: {entry.status_code ?? 'n/a'}</span>
                  </div>
                </div>
              ))
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-xl">
              <ShieldAlert className="h-5 w-5 text-red-600" />
              Feed status
            </CardTitle>
            <CardDescription>Operational context for the currently loaded log window.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="rounded-2xl border border-gray-200 bg-gray-50 px-4 py-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">Persisted rows</p>
              <p className="mt-2 text-2xl font-semibold text-gray-900">{logsQuery.data?.items.length ?? 0}</p>
            </div>
            <div className="rounded-2xl border border-gray-200 bg-gray-50 px-4 py-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">Live additions</p>
              <p className="mt-2 text-2xl font-semibold text-gray-900">{liveLogsQuery.data?.length ?? 0}</p>
            </div>
            <div className="rounded-2xl border border-gray-200 bg-gray-50 px-4 py-4 text-sm text-gray-700">
              <p className="font-medium text-gray-900">Mode</p>
              <p className="mt-2">{liveMode ? 'Live polling every 2 seconds' : 'Persisted-only mode'}</p>
              {linkedLogId ? <p className="mt-2 text-xs text-gray-500">Focused log id: {linkedLogId}</p> : null}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}