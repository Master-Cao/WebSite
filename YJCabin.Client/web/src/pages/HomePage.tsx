import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { api } from "../api/client";
import type { AboutDto, ArticleSummary, PagedResult, ProjectSummary } from "../api/types";
import { TagList } from "../components/TagList";

export function HomePage() {
  const projects = useQuery({
    queryKey: ["projects", "home"],
    queryFn: () => api<PagedResult<ProjectSummary>>("/api/projects?pageSize=3")
  });
  const articles = useQuery({
    queryKey: ["articles", "home"],
    queryFn: () => api<PagedResult<ArticleSummary>>("/api/articles?pageSize=3")
  });
  const about = useQuery({
    queryKey: ["about"],
    queryFn: () => api<AboutDto>("/api/about")
  });

  return (
    <div className="stack">
      <section className="hero">
        <p className="eyebrow">Personal studio</p>
        <h1>{about.data?.headline ?? "YJCabin"}</h1>
        <p className="lede">作品、文章与关于我，Web 与桌面端共用同一套 API。</p>
      </section>
      <section>
        <div className="section-head">
          <h2>作品</h2>
          <Link to="/works">全部</Link>
        </div>
        <div className="cards">
          {projects.data?.items.map((item) => (
            <Link className="card" key={item.id} to={`/works/${item.slug}`}>
              <h3>{item.title}</h3>
              <p>{item.summary}</p>
              <TagList tags={item.tags} />
            </Link>
          ))}
        </div>
      </section>
      <section>
        <div className="section-head">
          <h2>文章</h2>
          <Link to="/articles">全部</Link>
        </div>
        <div className="cards">
          {articles.data?.items.map((item) => (
            <Link className="card" key={item.id} to={`/articles/${item.slug}`}>
              <h3>{item.title}</h3>
              <p>{item.summary}</p>
              <TagList tags={item.tags} />
            </Link>
          ))}
        </div>
      </section>
    </div>
  );
}
