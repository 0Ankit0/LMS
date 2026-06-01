'use client';

import { useState } from 'react';
import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useLmsCompetencies, useLmsCreateCompetency } from '@/hooks/use-lms';

export default function AdminCompetenciesPage() {
  const { data, isLoading } = useLmsCompetencies();
  const createCompetency = useLmsCreateCompetency();

  const [code, setCode] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');

  const competencies = data ?? [];

  const handleCreate = () => {
    if (!code.trim() || !title.trim()) {
      return;
    }
    createCompetency.mutate({
      code: code.trim(),
      title: title.trim(),
      description: description.trim(),
    });
    setCode('');
    setTitle('');
    setDescription('');
  };

  return (
    <SectionShell
      title="Competencies"
      description="Define and manage competency frameworks used for outcome-based education."
    >
      <Card>
        <CardContent className="space-y-3 p-4">
          <div className="grid grid-cols-1 gap-3 md:grid-cols-3">
            <Input value={code} onChange={(event) => setCode(event.target.value)} placeholder="Code" />
            <Input value={title} onChange={(event) => setTitle(event.target.value)} placeholder="Title" />
            <Input value={description} onChange={(event) => setDescription(event.target.value)} placeholder="Description" />
          </div>
          <div className="flex justify-end">
            <Button onClick={handleCreate} isLoading={createCompetency.isPending}>Create Competency</Button>
          </div>
        </CardContent>
      </Card>

      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : competencies.length === 0 ? (
        <EmptyHint title="No competencies found" subtitle="Create competencies to map them to courses and assessments." />
      ) : (
        <div className="space-y-3">
          {competencies.map((competency: any) => (
            <Card key={competency.id}>
              <CardContent className="space-y-1 p-4">
                <p className="text-sm font-semibold text-gray-900">{competency.code} - {competency.title}</p>
                <p className="text-sm text-gray-700">{competency.description}</p>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
