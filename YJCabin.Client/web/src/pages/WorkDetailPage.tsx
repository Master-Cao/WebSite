import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import { api } from "../api/client";
import type { ProjectDetail } from "../api/types";
import { TagList } from "../components/TagList";

export function WorkDetailPage() {
  const { slug = "" } = useParams();
  const query = useQuery({
    queryKey: ["project", slug],
    queryFn: () => api<ProjectDetail>(`/api/projects/${slug}`)
  });
  if (query.isLoading) return <p>加载中…</p>;
  if (query.error || !query.data) return <p>未找到该作品。</p>;
  const project = query.data;
  return (
    <article className="stack prose">
      <h1>{project.title}</h1>
      <TagList tags={project.tags} />
      <p className="lede">{project.summary}</p>
      <p>{project.description}</p>
      <div className="actions">
        {project.repoUrl && (
          <a href={project.repoUrl} target="_blank" rel="noreferrer">
            仓库
          </a>
        )}
        {project.liveUrl && (
          <a href={project.liveUrl} target="_blank" rel="noreferrer">
            在线地址
          </a>
        )}
      </div>
      {project.images.map((image) => (
        <figure key={image.id}>
          <img src={image.url} alt={image.caption ?? project.title} />
          {image.caption && <figcaption>{image.caption}</figcaption>}
        </figure>
      ))}
    </article>
  );
}
