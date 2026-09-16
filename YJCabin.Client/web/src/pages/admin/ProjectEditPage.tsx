import { FormEvent, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { api } from "../../api/client";
import type { ContentStatus, ProjectDetail } from "../../api/types";
import { btnPrimary, controlClass, fieldClass } from "../../ui";

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
    <form className="glass panel" onSubmit={onSubmit} style={{ display: "grid", gap: 16, maxWidth: 720 }}>
      <h1 className="page-title">{isNew ? "新建作品" : "编辑作品"}</h1>
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
        描述
        <textarea name="description" rows={8} className={controlClass} defaultValue={data?.description} required />
      </label>
      <label className={fieldClass}>
        封面 URL
        <input name="coverUrl" className={controlClass} defaultValue={data?.coverUrl ?? ""} />
      </label>
      <label className={fieldClass}>
        仓库
        <input name="repoUrl" className={controlClass} defaultValue={data?.repoUrl ?? ""} />
      </label>
      <label className={fieldClass}>
        在线地址
        <input name="liveUrl" className={controlClass} defaultValue={data?.liveUrl ?? ""} />
      </label>
      <label className={fieldClass}>
        标签（逗号分隔）
        <input name="tags" className={controlClass} defaultValue={data?.tags.map((x) => x.name).join(", ")} />
      </label>
      <label className={fieldClass}>
        排序
        <input name="sortOrder" type="number" className={controlClass} defaultValue={data?.sortOrder ?? 0} />
      </label>
      <label className={fieldClass}>
        状态
        <select value={status} className={controlClass} onChange={(event) => setStatus(event.target.value as ContentStatus)}>
          <option value="Draft">草稿</option>
          <option value="Published">已发布</option>
          <option value="Archived">已归档</option>
        </select>
      </label>
      <button type="submit" className={`${btnPrimary} w-fit`}>
        保存
      </button>
      {error && <p className="danger">{error}</p>}
    </form>
  );
}
