"use client";

import { useEffect, useState } from "react";
import { getQuickNote, QuickNote } from "@/lib/api";
import { useRouter } from "next/navigation";
import { createQuickNote, updateQuickNote } from "@/lib/api";

type Props = {
  noteId?: string; 
};

export default function QuickNoteEditor({ noteId }: Props) {
  const isNew = !noteId;

  const [loading, setLoading] = useState(!isNew);
  const [error, setError] = useState<string>("");

  const [note, setNote] = useState<QuickNote | null>(null);

  // Editable fields
  const [title, setTitle] = useState<string>("");
  const [body, setBody] = useState<string>("");

  const router = useRouter();
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!noteId) return;

    (async () => {
      setLoading(true);
      setError("");

      try {
        const n = await getQuickNote(noteId);
        setNote(n);
        setTitle(n.title ?? "");
        setBody(n.body ?? "");
      } catch (e: unknown) {
        const message = e instanceof Error ? e.message : "Failed to load note";
        setError(message);
      } finally {
        setLoading(false);
      }
    })();
  }, [noteId]);

  async function onSave() {
  setSaving(true);
  setError("");

  try {
    if (!body.trim()) {
      setError("Body is required.");
      return;
    }

    const titleValue = title.trim() ? title.trim() : null;

    if (isNew) {
      const created = await createQuickNote({
        title: titleValue,
        body,
        state: "Open",
      });

      setNote(created);
      router.replace(`/quicknotes/${created.id}`);
    } else {
      const updated = await updateQuickNote(noteId!, {
        title: titleValue,
        body,
      });

      setNote(updated);
    }
  } catch (e: unknown) {
    const message = e instanceof Error ? e.message : "Save failed";
    setError(message);
  } finally {
    setSaving(false);
  }
}

  if (loading) {
    return <main className="p-6">Loading...</main>;
  }

  if (error) {
    return <main className="p-6 text-red-600">Error: {error}</main>;
  }

  return (
    <main className="min-h-screen bg-white text-black p-6">
      <div className="max-w-3xl mx-auto space-y-4">
        <input
          className="w-full text-2xl font-semibold border rounded-md p-3"
          placeholder="Untitled"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />

        <textarea
          className="w-full min-h-[60vh] border rounded-md p-3"
          placeholder="Write a note..."
          value={body}
          onChange={(e) => setBody(e.target.value)}
        />

        <div className="w-full flex items-center justify-between">
          <button
            className="px-4 py-2 rounded-md border bg-black text-white disabled:opacity-50"
            onClick={onSave}
            disabled={saving}
          >
            {saving ? "Saving..." : "Save"}
          </button>

          {note?.updatedAt ? (
            <div className="text-xs text-gray-500">
              Updated: {new Date(note.updatedAt).toLocaleString()}
            </div>
          ) : (
            <div />
          )}
        </div>
        {error && <div className="text-sm text-red-600">{error}</div>}
      </div>
    </main>
  );
}


