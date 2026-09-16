import { FormEvent, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { api } from "../../api/client";
import type { ArticleDetail, ContentStatus } from "../../api/types";

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
    <form className="form" onSubmit={onSubmit}>
      <h1>{isNew ? "新建文章" : "编辑文章"}</h1>
      <label>
        标题
        <input name="title" defaultValue={data?.title} required />
      </label>
      <label>
        Slug
        <input name="slug" defaultValue={data?.slug} />
      </label>
      <label>
        摘要
        <textarea name="summary" defaultValue={data?.summary} required />
      </label>
      <label>
        封面 URL
        <input name="coverUrl" defaultValue={data?.coverUrl ?? ""} />
      </label>
      <label>
        标签（逗号分隔）
        <input name="tags" defaultValue={data?.tags.map((x) => x.name).join(", ")} />
      </label>
      <label>
        状态
        <select value={status} onChange={(event) => setStatus(event.target.value as ContentStatus)}>
          <option value="Draft">Draft</option>
          <option value="Published">Published</option>
          <option value="Archived">Archived</option>
        </select>
      </label>
      <label>
        Markdown
        <textarea name="markdown" rows={16} defaultValue={data?.markdown} required />
      </label>
      <button type="submit">保存</button>
      {error && <p className="error">{error}</p>}
    </form>
  );
}
