const INVALID = new Set(["undefined", "null"]);

export function isContentSlug(value: unknown): value is string {
  if (typeof value !== "string") return false;
  const slug = value.trim();
  return slug.length > 0 && !INVALID.has(slug.toLowerCase());
}

export function pickSlug(item: { slug?: unknown; id?: unknown } | null | undefined) {
  if (!item) return "";
  if (isContentSlug(item.slug)) return item.slug.trim();
  if (isContentSlug(item.id)) return String(item.id).trim();
  return "";
}
