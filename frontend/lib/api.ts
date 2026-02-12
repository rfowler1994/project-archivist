// frontend/lib/api.ts
// Central place for talking to the backend API.

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5185";

/**
 * Makes an HTTP request to the backend and returns parsed JSON.
 * - path should start with "/api/..."
 * - throws an Error if the response is not OK
 */
async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const url = `${API_BASE}${path}`;

  const res = await fetch(url, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {}),
    },
  });

  if (!res.ok) {
    // Try to give a useful error message for debugging.
    const text = await res.text();
    throw new Error(`Request failed (${res.status}) ${url}: ${text}`);
  }

  if (res.status === 204) {
    return undefined as T;
  }

  // If backend ever returns empty body with 200/201, this avoids json() crashing.
  const contentType = res.headers.get("content-type") ?? "";
  if (!contentType.includes("application/json")) {
    return undefined as T;
  }

  return (await res.json()) as T;
}

export type QuickNoteState = "Open" | "Closed" | "Pinned" | "Archived";
export type QuickNoteListView = "inbox" | "trash";

export type QuickNote = {
  id: string;
  title: string | null;
  body: string;
  state: QuickNoteState;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
};

export type QuickNoteInboxItem = {
    id: string;
    title: string | null;
    updatedAt: string;
    preview: string;
};

export type QuickNoteCreateRequest = {
  title?: string | null;
  body: string;
  state?: number | QuickNoteState | null;
};

export type QuickNoteUpdateRequest = {
  title: string | null;
  body: string;
  state?: number | QuickNoteState | null;
};

// POST /api/QuickNotes 
export async function createQuickNote(dto: QuickNoteCreateRequest) {
  return request<QuickNote>("/api/QuickNotes", {
    method: "POST",
    body: JSON.stringify(dto),
  });
}

// GET /api/QuickNotes/{id} 
export async function getQuickNote(id: string) {
  return request<QuickNote>(`/api/QuickNotes/${id}`);
}

// GET /api/QuickNotes?view=inbox|trash&page=1&pageSize=20&previewLength=100
export async function listQuickNotes(options?: {
  view?: QuickNoteListView;
  page?: number;
  pageSize?: number;
  previewLength?: number;
}) {
  const view = options?.view ?? "inbox";
  const page = options?.page ?? 1;
  const pageSize = options?.pageSize ?? 20;
  const previewLength = options?.previewLength ?? 100;

  const qs = new URLSearchParams({
    view,
    page: String(page),
    pageSize: String(pageSize),
    previewLength: String(previewLength),
  });

  return request<QuickNoteInboxItem[]>(`/api/QuickNotes?${qs.toString()}`);
}

/** PUT /api/QuickNotes/{id} */
export async function updateQuickNote(id: string, dto: QuickNoteUpdateRequest) {
  return request<QuickNote>(`/api/QuickNotes/${id}`, {
    method: "PUT",
    body: JSON.stringify(dto),
  });
}

/** DELETE /api/QuickNotes/{id} (soft delete) */
export async function softDeleteQuickNote(id: string) {
  await request<void>(`/api/QuickNotes/${id}`, { method: "DELETE" });
}

/** POST /api/QuickNotes/{id}/restore */
export async function restoreQuickNote(id: string) {
  await request<void>(`/api/QuickNotes/${id}/restore`, { method: "POST" });
}

/** DELETE /api/QuickNotes/{id}/hard (permanent) */
export async function hardDeleteQuickNote(id: string) {
  await request<void>(`/api/QuickNotes/${id}/hard`, { method: "DELETE" });
}