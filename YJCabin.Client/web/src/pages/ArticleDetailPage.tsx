import { useCallback, useEffect } from "react";
import { createPortal } from "react-dom";
import { useQuery } from "@tanstack/react-query";
import { X } from "@phosphor-icons/react";
import { useLocation, useNavigate, type Location } from "react-router-dom";
import { api } from "../api/client";
import type { ArticleDetail } from "../api/types";
import { Note } from "../components/Note";
import { TagList } from "../components/TagList";
import { MarkdownHtml } from "../components/MarkdownHtml";
import { isContentSlug } from "../lib/content";
import { formatDate } from "../lib/format";

export function ArticleDetailPage({ slug }: { slug: string }) {
  const navigate = useNavigate();
  const location = useLocation();
  const background = (location.state as { background?: Location } | null)?.background;
  const query = useQuery({
    queryKey: ["article", slug],
    queryFn: () => api<ArticleDetail>(`/api/articles/${encodeURIComponent(slug)}`),
    enabled: isContentSlug(slug)
  });

  const onClose = useCallback(() => {
    if (background) navigate(-1);
    else navigate("/articles", { replace: true });
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

  const article = query.data;

  return createPortal(
    <div className="reader-overlay" onClick={onClose} role="presentation">
      <article
        className="reader-dialog glass"
        role="dialog"
        aria-modal="true"
        aria-labelledby={article ? "article-reader-title" : undefined}
        aria-busy={query.isLoading}
        onClick={(event) => event.stopPropagation()}
      >
        <header className="reader-head">
          <div>
            {article ? (
              <>
                <h1 id="article-reader-title" className="page-title">
                  {article.title}
                </h1>
                <div className="reader-meta">
                  {article.publishedAt && (
                    <time className="row-meta" dateTime={article.publishedAt}>
                      {formatDate(article.publishedAt)}
                    </time>
                  )}
                  <TagList tags={article.tags} />
                </div>
              </>
            ) : (
              <h1 className="page-title">{query.isLoading ? "加载中" : "未找到该文章"}</h1>
            )}
          </div>
          <button type="button" className="reader-close" onClick={onClose} aria-label="关闭">
            <X size={18} />
          </button>
        </header>
        <div className="reader-body">
          {query.isLoading && <Note>加载中</Note>}
          {!query.isLoading && (query.error || !article) && <Note>未找到该文章。</Note>}
          {article && (
            <>
              {article.summary && <p className="lede">{article.summary}</p>}
              <MarkdownHtml html={article.html} />
            </>
          )}
        </div>
      </article>
    </div>,
    document.body
  );
}
