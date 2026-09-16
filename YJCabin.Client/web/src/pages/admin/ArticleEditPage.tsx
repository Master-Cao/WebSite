import { FormEvent, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { api } from "../../api/client";
import type { ArticleDetail, ContentStatus } from "../../api/types";
import { btnPrimary, controlClass, fieldClass } from "../../ui";

export function ArticleEditPage() {
  const { slug } = useParams();
  const navigate = useNavigate();
  const isNew = !slug || slug === "new";
  const existing = useQuery({
    queryKey: ["admin", "article", slug],
    queryFn: () => api<ArticleDetail>(`/api/admin/articles/${slug}`, {}, true),
    enabled: !isNew
  });
  const [error, setError] = useState<string>();
  const [status, setStatus] = useState<ContentStatus>("Draft");

  useEffect(() => {
    if (existing.data) {
      setStatus(existing.data.status);
    }
  }, [existing.data]);

  const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const payload = {
      slug: String(form.get("slug") || ""),
      title: String(form.get("title")),
      summary: String(form.get("summary")),
      coverUrl: String(form.get("coverUrl") || "") || null,
      status,
      tags: String(form.get("tags") || "")
        .split(",")
        .map((x) => x.trim())
        .filter(Boolean),
      markdown: String(form.get("markdown"))
    };
    try {
      const saved = isNew
        ? await api<ArticleDetail>("/api/admin/articles", { method: "POST", body: JSON.stringify(payload) }, true)
        : await api<ArticleDetail>(`/api/admin/articles/${slug}`, { method: "PUT", body: JSON.stringify(payload) }, true);
      navigate(`/admin/articles/${saved.slug}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "保存失败");
    }
  };

  const data = existing.data;
  return (
    <form className="glass panel" onSubmit={onSubmit} style={{ display: "grid", gap: 16, maxWidth: 720 }}>
      <h1 className="page-title">{isNew ? "新建文章" : "编辑文章"}</h1>
      <label className={fieldClass}>
        标题
        <input name="title" className={controlClass} defaultValue={data?.title} required />
      </label>
      <label className={fieldClass}>
        Slug
        <input name="slug" className={controlClass} defaultValue={data?.slug} />
      </label>
      <label className={fieldClass}>
        摘要
        <textarea name="summary" className={controlClass} defaultValue={data?.summary} required />
      </label>
      <label className={fieldClass}>
        封面 URL
        <input name="coverUrl" className={controlClass} defaultValue={data?.coverUrl ?? ""} />
      </label>
      <label className={fieldClass}>
        标签（逗号分隔）
        <input name="tags" className={controlClass} defaultValue={data?.tags.map((x) => x.name).join(", ")} />
      </label>
      <label className={fieldClass}>
        状态
        <select value={status} className={controlClass} onChange={(event) => setStatus(event.target.value as ContentStatus)}>
          <option value="Draft">草稿</option>
          <option value="Published">已发布</option>
          <option value="Archived">已归档</option>
        </select>
      </label>
      <label className={fieldClass}>
        Markdown
        <textarea name="markdown" rows={16} className={controlClass} defaultValue={data?.markdown} required />
      </label>
      <button type="submit" className={`${btnPrimary} w-fit`}>
        保存
      </button>
      {error && <p className="danger">{error}</p>}
    </form>
  );
}
