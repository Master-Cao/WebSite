import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { api } from "../api/client";
import type { ArticleSummary, PagedResult, ProjectSummary } from "../api/types";
import { ArticleRow, WorkRow } from "../components/ContentRows";
import { Note, SkeletonRows } from "../components/Note";

export function HomePage() {
  const projects = useQuery({
    queryKey: ["projects", "home"],
    queryFn: () => api<PagedResult<ProjectSummary>>("/api/projects?pageSize=6")
  });
  const articles = useQuery({
    queryKey: ["articles", "home"],
    queryFn: () => api<PagedResult<ArticleSummary>>("/api/articles?pageSize=8")
  });

  return (
    <div className="wrap home-board">
      <section className="glass panel home-articles">
        <div className="chip-row" style={{ justifyContent: "space-between", marginTop: 0 }}>
          <h2>文章</h2>
          <div className="chip-row" style={{ marginTop: 0 }}>
            <Link to="/articles" className="accent">
              全部
            </Link>
            <Link to="/archive" className="accent">
              归档
            </Link>
          </div>
        </div>
        {articles.isLoading && <SkeletonRows count={4} />}
        {articles.error && <Note>暂时无法加载文章。</Note>}
        {!articles.isLoading && !articles.data?.items.length && <Note>还没有发布文章。</Note>}
        <ul className="row-list">
          {articles.data?.items.map((item) => (
            <ArticleRow key={item.id} item={item} />
          ))}
        </ul>
      </section>

      <section className="glass panel home-works">
        <div className="chip-row" style={{ justifyContent: "space-between", marginTop: 0 }}>
          <h2>作品</h2>
          <Link to="/works" className="accent">
            全部作品
          </Link>
        </div>
        {projects.isLoading && <SkeletonRows />}
        {projects.error && <Note>暂时无法加载作品。</Note>}
        {!projects.isLoading && !projects.data?.items.length && <Note>还没有发布作品。</Note>}
        <ul className="row-list">
          {projects.data?.items.map((item, index) => (
            <WorkRow key={item.id} item={item} index={index} />
          ))}
        </ul>
      </section>
    </div>
  );
}
