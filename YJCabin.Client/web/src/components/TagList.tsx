import type { TagDto } from "../api/types";

export function TagList({ tags }: { tags: TagDto[] }) {
  if (!tags.length) {
    return null;
  }
  return (
    <ul className="tags">
      {tags.map((tag) => (
        <li key={tag.id}>{tag.name}</li>
      ))}
    </ul>
  );
}
