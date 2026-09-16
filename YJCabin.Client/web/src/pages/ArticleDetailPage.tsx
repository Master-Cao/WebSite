import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import { api } from "../api/client";
import type { ArticleDetail } from "../api/types";
import { TagList } from "../components/TagList";

export function ArticleDetailPage() {
  const { slug = "" } = useParams();
  const query = useQuery({
    queryKey: ["article", slug],
    queryFn: () => api<ArticleDetail>(`/api/articles/${slug}`)
  });
  if (query.isLoading) return <p>加载中…</p>;
  if (query.error || !query.data) return <p>未找到该文章。</p>;
  const article = query.data;
  return (
    <article className="stack prose">
      <h1>{article.title}</h1>
      <TagList tags={article.tags} />
      <p className="lede">{article.summary}</p>
      <div className="markdown" dangerouslySetInnerHTML={{ __html: article.html }} />
    </article>
  );
}
