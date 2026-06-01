'use client';

import { useState } from 'react';
import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useLmsCreateNote, useLmsDeleteNote, useLmsNotes } from '@/hooks/use-lms';

export default function NotesPage() {
  const { data: notes, isLoading } = useLmsNotes();
  const createNote = useLmsCreateNote();
  const deleteNote = useLmsDeleteNote();

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');

  const handleCreate = () => {
    if (!title.trim() || !content.trim()) {
      return;
    }
    createNote.mutate({
      title: title.trim(),
      content: content.trim(),
      tag: 'study',
      priority: 'medium',
      isPinned: false,
    });
    setTitle('');
    setContent('');
  };

  return (
    <SectionShell
      title="Notes"
      description="Capture lesson notes and manage your personal study knowledge base."
    >
      <Card>
        <CardContent className="space-y-3 p-4">
          <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
            <Input value={title} onChange={(event) => setTitle(event.target.value)} placeholder="Note title" />
            <Input value={content} onChange={(event) => setContent(event.target.value)} placeholder="Note content" />
          </div>
          <div className="flex justify-end">
            <Button onClick={handleCreate} isLoading={createNote.isPending}>Create Note</Button>
          </div>
        </CardContent>
      </Card>

      {isLoading ? (
        <div className="h-44 animate-pulse rounded-xl bg-gray-100" />
      ) : (notes?.length ?? 0) === 0 ? (
        <EmptyHint title="No notes saved" subtitle="Create your first note to start organizing study material." />
      ) : (
        <div className="space-y-3">
          {notes?.map((note) => (
            <Card key={note.id}>
              <CardContent className="space-y-2 p-4">
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <p className="text-sm font-semibold text-gray-900">{note.title}</p>
                    <p className="text-sm text-gray-700">{note.content}</p>
                    <p className="text-xs text-gray-500">Updated {new Date(note.updated_at).toLocaleString()}</p>
                  </div>
                  <Button
                    variant="outline"
                    onClick={() => deleteNote.mutate(note.id)}
                    isLoading={deleteNote.isPending}
                  >
                    Delete
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </SectionShell>
  );
}
