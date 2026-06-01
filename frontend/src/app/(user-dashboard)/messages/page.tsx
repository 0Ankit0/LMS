'use client';

import { useMemo, useState } from 'react';
import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useLmsConversations, useLmsConversationMessages, useLmsSendMessage } from '@/hooks/use-lms';

export default function MessagesPage() {
  const { data: conversations, isLoading } = useLmsConversations();
  const [selectedConversationId, setSelectedConversationId] = useState<number | null>(null);
  const [messageText, setMessageText] = useState('');

  const activeConversationId = selectedConversationId ?? conversations?.[0]?.id ?? null;
  const { data: messages } = useLmsConversationMessages(activeConversationId);
  const sendMutation = useLmsSendMessage();

  const sortedMessages = useMemo(
    () => [...(messages ?? [])].sort((a, b) => new Date(a.created_at).getTime() - new Date(b.created_at).getTime()),
    [messages]
  );

  const handleSend = () => {
    if (!activeConversationId || !messageText.trim()) {
      return;
    }
    sendMutation.mutate({ conversationId: activeConversationId, encryptedContent: messageText.trim() });
    setMessageText('');
  };

  return (
    <SectionShell
      title="Messages"
      description="Collaborate in real-time conversations with students, instructors, and staff."
    >
      {isLoading ? (
        <div className="h-72 animate-pulse rounded-xl bg-gray-100" />
      ) : (conversations?.length ?? 0) === 0 ? (
        <EmptyHint title="No conversations available" subtitle="Start a new conversation to collaborate with your team." />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
          <Card className="lg:col-span-1">
            <CardContent className="p-3">
              <div className="space-y-2">
                {conversations?.map((conversation) => (
                  <button
                    key={conversation.id}
                    type="button"
                    onClick={() => setSelectedConversationId(conversation.id)}
                    className={`w-full rounded-lg border px-3 py-2 text-left transition-colors ${
                      (activeConversationId ?? null) === conversation.id
                        ? 'border-blue-200 bg-blue-50'
                        : 'border-gray-200 hover:bg-gray-50'
                    }`}
                  >
                    <p className="text-sm font-medium text-gray-900">{conversation.title}</p>
                    <p className="text-xs text-gray-500">{conversation.is_group ? 'Group chat' : 'Direct message'}</p>
                  </button>
                ))}
              </div>
            </CardContent>
          </Card>

          <Card className="lg:col-span-2">
            <CardContent className="space-y-4 p-4">
              <div className="max-h-[440px] space-y-3 overflow-y-auto rounded-xl border border-gray-200 bg-gray-50 p-3">
                {sortedMessages.length === 0 ? (
                  <p className="text-sm text-gray-500">No messages in this conversation yet.</p>
                ) : (
                  sortedMessages.map((message) => (
                    <div key={message.id} className="rounded-lg bg-white p-3 shadow-sm">
                      <p className="text-sm text-gray-900">{message.encrypted_content}</p>
                      <p className="mt-1 text-xs text-gray-500">
                        Sender #{message.sender_id} • {new Date(message.created_at).toLocaleString()}
                      </p>
                    </div>
                  ))
                )}
              </div>
              <div className="flex gap-2">
                <Input
                  value={messageText}
                  onChange={(event) => setMessageText(event.target.value)}
                  placeholder="Write a message"
                />
                <Button onClick={handleSend} isLoading={sendMutation.isPending}>Send</Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </SectionShell>
  );
}
