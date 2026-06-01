'use client';

import { useState } from 'react';
import { SectionShell, EmptyHint } from '@/components/lms/section-shell';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  useLmsCategories,
  useLmsTags,
  useLmsCreateCategory,
  useLmsDeleteCategory,
  useLmsCreateTag,
  useLmsDeleteTag,
} from '@/hooks/use-lms';

export default function AdminTaxonomyPage() {
  const { data: categories } = useLmsCategories();
  const { data: tags } = useLmsTags();
  const createCategory = useLmsCreateCategory();
  const deleteCategory = useLmsDeleteCategory();
  const createTag = useLmsCreateTag();
  const deleteTag = useLmsDeleteTag();

  const [categoryName, setCategoryName] = useState('');
  const [tagName, setTagName] = useState('');

  return (
    <SectionShell
      title="Taxonomy Management"
      description="Organize LMS content with categories and tags for navigation and filtering."
    >
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Categories</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex gap-2">
              <Input
                value={categoryName}
                onChange={(event) => setCategoryName(event.target.value)}
                placeholder="New category"
              />
              <Button
                onClick={() => {
                  if (!categoryName.trim()) return;
                  createCategory.mutate({ name: categoryName.trim() });
                  setCategoryName('');
                }}
                isLoading={createCategory.isPending}
              >
                Add
              </Button>
            </div>
            {(categories ?? []).length === 0 ? (
              <EmptyHint title="No categories" subtitle="Create categories to classify courses." />
            ) : (
              <div className="space-y-2">
                {(categories ?? []).map((category: any) => (
                  <div key={category.id} className="flex items-center justify-between rounded-lg border border-gray-200 px-3 py-2">
                    <span className="text-sm text-gray-900">{category.name}</span>
                    <Button variant="outline" onClick={() => deleteCategory.mutate(category.id)} isLoading={deleteCategory.isPending}>Delete</Button>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Tags</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex gap-2">
              <Input
                value={tagName}
                onChange={(event) => setTagName(event.target.value)}
                placeholder="New tag"
              />
              <Button
                onClick={() => {
                  if (!tagName.trim()) return;
                  createTag.mutate({ name: tagName.trim() });
                  setTagName('');
                }}
                isLoading={createTag.isPending}
              >
                Add
              </Button>
            </div>
            {(tags ?? []).length === 0 ? (
              <EmptyHint title="No tags" subtitle="Create tags to support flexible content discovery." />
            ) : (
              <div className="space-y-2">
                {(tags ?? []).map((tag: any) => (
                  <div key={tag.id} className="flex items-center justify-between rounded-lg border border-gray-200 px-3 py-2">
                    <span className="text-sm text-gray-900">{tag.name}</span>
                    <Button variant="outline" onClick={() => deleteTag.mutate(tag.id)} isLoading={deleteTag.isPending}>Delete</Button>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </SectionShell>
  );
}
