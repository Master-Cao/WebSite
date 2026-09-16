import { useQuery } from "@tanstack/react-query";
import { Link, useParams } from "react-router-dom";
import { api } from "../api/client";
import type { ProjectDetail } from "../api/types";
import { Note } from "../components/Note";
import { TagList } from "../components/TagList";
import { formatDate } from "../lib/format";

export function WorkDetailPage() {
  const { slug = "" } = useParams();
  const query = useQuery({
    queryKey: ["project", slug],
    queryFn: () => api<ProjectDetail>(`/api/projects/${slug}`)
  });
  if (query.isLoading) {
    return (
      <div className="wrap">
        <div className="glass panel">
          <Note>加载中</Note>
        </div>
      </div>
    );
  }
  if (query.error || !query.data) {
    return (
      <div className="wrap">
        <div className="glass panel">
          <Note>未找到该作品。</Note>
        </div>
      </div>
    );
  }
  const project = query.data;
  return (
    <article className="wrap detail-grid">
      <div className="glass panel">
        <p className="muted">
          <Link to="/works" className="accent">
            作品
          </Link>
        </p>
        <h1 className="page-title">{project.title}</h1>
        <p className="lede">{project.summary}</p>
        <p className="muted">{project.description}</p>
        {project.images.map((image) => (
          <figure key={image.id} style={{ marginTop: 28 }}>
            <img src={image.url} alt={image.caption ?? project.title} />
            {image.caption && <figcaption className="muted">{image.caption}</figcaption>}
          </figure>
        ))}
      </div>
      <aside className="glass panel">
        <h2 className="muted">信息</h2>
        {project.publishedAt && <p className="row-meta">{formatDate(project.publishedAt)}</p>}
        <TagList tags={project.tags} />
        <div style={{ display: "grid", gap: 10, marginTop: 20 }}>
          {project.repoUrl && (
            <a className="accent" href={project.repoUrl} target="_blank" rel="noreferrer">
              仓库
            </a>
          )}
          {project.liveUrl && (
            <a className="accent" href={project.liveUrl} target="_blank" rel="noreferrer">
              在线地址
            </a>
          )}
        </div>
      </aside>
    </article>
  );
}
