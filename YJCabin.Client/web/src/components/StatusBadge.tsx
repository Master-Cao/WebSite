import type { ContentStatus } from "../api/types";
import { statusLabel } from "../lib/format";

export function StatusBadge({ status }: { status: ContentStatus }) {
  return <span className={status === "Published" ? "accent" : "muted"}>{statusLabel[status]}</span>;
}
