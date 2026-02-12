import { redirect } from "next/navigation";

export default async function HomePage() {
  // IMPORTANT: This runs on the server.
  // Use the absolute backend URL directly so it works reliably.
  const apiBase = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5185";

  const res = await fetch(
    `${apiBase}/api/QuickNotes?view=inbox&page=1&pageSize=1&previewLength=1`,
    { cache: "no-store" }
  );

  if (!res.ok) {
    redirect("/quicknotes/new");
  }

  const items: Array<{ id: string }> = await res.json();

  if (items.length > 0) {
    redirect(`/quicknotes/${items[0].id}`);
  }

  redirect("/quicknotes/new");
}
