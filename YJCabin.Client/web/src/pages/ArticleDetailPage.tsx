import { useQuery } from "@tanstack/react-query";
import { Link, useParams } from "react-router-dom";
import { api } from "../api/client";
import type { ArticleDetail } from "../api/types";
import { Note } from "../components/Note";
import { TagList } from "../components/TagList";
import { formatDate } from "../lib/format";

export function ArticleDetailPage() {
  const { slug = "" } = useParams();
  const query = useQuery({
    queryKey: ["article", slug],
    queryFn: () => api<ArticleDetail>(`/api/articles/${slug}`)
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
          <Note>未找到该文章。</Note>
        </div>
      </div>
    );
  }
  const article = query.data;
  return (
    <article className="wrap detail-grid">
      <div className="glass panel">
        <p className="muted">
          <Link to="/articles" className="accent">
            文章
          </Link>
        </p>
        <h1 className="page-title">{article.title}</h1>
        <p className="lede">{article.summary}</p>
        <div className="markdown" dangerouslySetInnerHTML={{ __html: article.html }} />
      </div>
      <aside className="glass panel">
        <h2 className="muted">信息</h2>
        {article.publishedAt && (
          <time className="row-meta" dateTime={article.publishedAt}>
            {formatDate(article.publishedAt)}
          </time>
        )}
        <div style={{ marginTop: 16 }}>
          <TagList tags={article.tags} />
        </div>
      </aside>
    </article>
  );
}
