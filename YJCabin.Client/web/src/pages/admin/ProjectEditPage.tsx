import { FormEvent, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { api } from "../../api/client";
import type { ContentStatus, ProjectDetail } from "../../api/types";

export function ProjectEditPage() {
  const { slug } = useParams();
  const navigate = useNavigate();
  const isNew = !slug || slug === "new";
  const existing = useQuery({
    queryKey: ["admin", "project", slug],
    queryFn: () => api<ProjectDetail>(`/api/admin/projects/${slug}`, {}, true),
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
      description: String(form.get("description")),
      coverUrl: String(form.get("coverUrl") || "") || null,
      repoUrl: String(form.get("repoUrl") || "") || null,
      liveUrl: String(form.get("liveUrl") || "") || null,
      status,
      sortOrder: Number(form.get("sortOrder") || 0),
      tags: String(form.get("tags") || "")
        .split(",")
        .map((x) => x.trim())
        .filter(Boolean),
      images: []
    };
    try {
      const saved = isNew
        ? await api<ProjectDetail>("/api/admin/projects", { method: "POST", body: JSON.stringify(payload) }, true)
        : await api<ProjectDetail>(`/api/admin/projects/${slug}`, { method: "PUT", body: JSON.stringify(payload) }, true);
      navigate(`/admin/projects/${saved.slug}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "保存失败");
    }
  };

  const data = existing.data;
  return (
    <form className="form" onSubmit={onSubmit}>
      <h1>{isNew ? "新建作品" : "编辑作品"}</h1>
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
        描述
        <textarea name="description" rows={8} defaultValue={data?.description} required />
      </label>
      <label>
        封面 URL
        <input name="coverUrl" defaultValue={data?.coverUrl ?? ""} />
      </label>
      <label>
        仓库
        <input name="repoUrl" defaultValue={data?.repoUrl ?? ""} />
      </label>
      <label>
        在线地址
        <input name="liveUrl" defaultValue={data?.liveUrl ?? ""} />
      </label>
      <label>
        标签（逗号分隔）
        <input name="tags" defaultValue={data?.tags.map((x) => x.name).join(", ")} />
      </label>
      <label>
        排序
        <input name="sortOrder" type="number" defaultValue={data?.sortOrder ?? 0} />
      </label>
      <label>
        状态
        <select value={status} onChange={(event) => setStatus(event.target.value as ContentStatus)}>
          <option value="Draft">Draft</option>
          <option value="Published">Published</option>
          <option value="Archived">Archived</option>
        </select>
      </label>
      <button type="submit">保存</button>
      {error && <p className="error">{error}</p>}
    </form>
  );
}
