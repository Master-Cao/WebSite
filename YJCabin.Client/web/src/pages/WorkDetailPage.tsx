import { useCallback, useEffect } from "react";
import { createPortal } from "react-dom";
import { useQuery } from "@tanstack/react-query";
import { X } from "@phosphor-icons/react";
import { useLocation, useNavigate, type Location } from "react-router-dom";
import { api } from "../api/client";
import type { ProjectDetail } from "../api/types";
import { Note } from "../components/Note";
import { TagList } from "../components/TagList";
import { isContentSlug } from "../lib/content";
import { formatDate } from "../lib/format";

export function WorkDetailPage({ slug }: { slug: string }) {
  const navigate = useNavigate();
  const location = useLocation();
  const background = (location.state as { background?: Location } | null)?.background;
  const query = useQuery({
    queryKey: ["project", slug],
    queryFn: () => api<ProjectDetail>(`/api/projects/${encodeURIComponent(slug)}`),
    enabled: isContentSlug(slug)
  });

  const onClose = useCallback(() => {
    if (background) navigate(-1);
    else navigate("/works", { replace: true });
  }, [background, navigate]);

  useEffect(() => {
    const previous = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") onClose();
    };
    window.addEventListener("keydown", onKey);
    return () => {
      document.body.style.overflow = previous;
      window.removeEventListener("keydown", onKey);
    };
  }, [onClose]);

  const project = query.data;

  return createPortal(
    <div className="reader-overlay" onClick={onClose} role="presentation">
      <article
        className="reader-dialog glass"
        role="dialog"
        aria-modal="true"
        aria-labelledby={project ? "work-reader-title" : undefined}
        aria-busy={query.isLoading}
        onClick={(event) => event.stopPropagation()}
      >
        <header className="reader-head">
          <div>
            {project ? (
              <>
                <h1 id="work-reader-title" className="page-title">
                  {project.title}
                </h1>
                <div className="reader-meta">
                  {project.publishedAt && (
                    <time className="row-meta" dateTime={project.publishedAt}>
                      {formatDate(project.publishedAt)}
                    </time>
                  )}
                  <TagList tags={project.tags} />
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
              </>
            ) : (
              <h1 className="page-title">{query.isLoading ? "加载中" : "未找到该作品"}</h1>
            )}
          </div>
          <button type="button" className="reader-close" onClick={onClose} aria-label="关闭">
            <X size={18} />
          </button>
        </header>
        <div className="reader-body">
          {query.isLoading && <Note>加载中</Note>}
          {!query.isLoading && (query.error || !project) && <Note>未找到该作品。</Note>}
          {project && (
            <>
              {project.summary && <p className="lede">{project.summary}</p>}
              {project.description && <p className="reader-copy">{project.description}</p>}
              {project.images.map((image) => (
                <figure key={image.id}>
                  <img src={image.url} alt={image.caption ?? project.title} />
                  {image.caption && <figcaption className="muted">{image.caption}</figcaption>}
                </figure>
              ))}
            </>
          )}
        </div>
      </article>
    </div>,
    document.body
  );
}
