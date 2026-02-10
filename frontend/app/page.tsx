"use client";

import { useEffect, useState } from "react";

export default function Home() {
  // Holds what the user typed in the textarea
  const [body, setBody] = useState("");

  // Holds the last saved note returned by the API (for display)
  const [savedNoteJson, setSavedNoteJson] = useState<string>("");

  // Holds all quick notes
  const [inbox, setInbox] = useState<any[]>([]);

  async function loadInbox() {
    try {
      const res = await fetch("http://localhost:5185/api/QuickNotes?page=1&pageSize=20");
          if (!res.ok) return;

          const data = await res.json();
          setInbox(data);
    } catch {
      // ignore for now (backend might be off)
    }  
  }

  useEffect(() => {
    (async () => {
      await loadInbox();
    })();
  }, []);

  return (
    <main className="min-h-screen p-6">
      <h1 className="text-2xl font-semibold mb-4">Quick Note</h1>

      <textarea
        className="w-full h-48 p-3 border rounded-md"
        placeholder="Write a note..."
        value={body}
        onChange={(e) => setBody(e.target.value)}
      />

      <div className="mt-4 flex gap-3">
        <button
          className="px-4 py-2 rounded-md bg-black text-white disabled:opacity-50"
          disabled={body.trim().length === 0}
          onClick={async () => {
            setSavedNoteJson(""); // clear previous

            const res = await fetch("http://localhost:5185/api/QuickNotes", {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify({ body }),
            });

            if (!res.ok) {
              const errText = await res.text();
              setSavedNoteJson(`Error ${res.status}: ${errText}`);
              return;
            }

            const saved = await res.json();
            setSavedNoteJson(JSON.stringify(saved, null, 2));
            setBody("");
            await loadInbox();
          }}
        >
          Save
        </button>
      </div>

      <div className="mt-8">
        <div className="text-lg font-semibold mb-2">Inbox (latest 20)</div>

        <div className="space-y-2">
          {inbox.map((n) => (
            <div key={n.id} className="p-3 border rounded-md">
              <div className="text-sm text-gray-500">Updated: {n.updatedAt}</div>
              <div className="mt-1 whitespace-pre-wrap">
                {"preview" in n ? n.preview : n.body}
              </div>
            </div>
          ))}
        </div>
      </div>

      {savedNoteJson && (
        <pre className="mt-6 p-4 bg-gray-100 text-black rounded-md whitespace-pre-wrap">
          {savedNoteJson}
        </pre>
      )}

    </main>
  );
}