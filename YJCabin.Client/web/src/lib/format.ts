export function formatDate(value?: string | null) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return new Intl.DateTimeFormat("zh-CN", { year: "numeric", month: "2-digit", day: "2-digit" }).format(date);
}

export function formatShortDate(value?: string | null) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return new Intl.DateTimeFormat("zh-CN", { month: "2-digit", day: "2-digit" }).format(date);
}

export function firstPlainLine(markdown?: string, fallback = "个人博客：文章、作品与简介。") {
  if (!markdown) return fallback;
  const line = markdown
    .split("\n")
    .map((item) => item.replace(/^#+\s*/, "").replace(/[*_`]/g, "").trim())
    .find(Boolean);
  return line || fallback;
}

export function padIndex(index: number) {
  return String(index + 1).padStart(2, "0");
}
