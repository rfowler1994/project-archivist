import QuickNoteEditor from "../_components/QuickNoteEditor";

export default async function QuickNoteByIdPage(
  props: { params: Promise<{ id: string }> }
) {
  const { id } = await props.params;
  return <QuickNoteEditor noteId={id} />;
}