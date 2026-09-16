import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { api } from "../../api/client";
import type { ArticleSummary, ContactMessage, PagedResult, ProjectSummary } from "../../api/types";

export function DashboardPage() {
  const projects = useQuery({
    queryKey: ["admin", "projects"],
    queryFn: () => api<PagedResult<ProjectSummary>>("/api/admin/projects", {}, true)
  });
  const articles = useQuery({
    queryKey: ["admin", "articles"],
    queryFn: () => api<PagedResult<ArticleSummary>>("/api/admin/articles", {}, true)
  });
  const messages = useQuery({
    queryKey: ["admin", "messages"],
    queryFn: () => api<PagedResult<ContactMessage>>("/api/admin/contact-messages", {}, true)
  });

  return (
    <div className="stack">
      <h1>概览</h1>
      <div className="cards">
        <div className="card">
          <h3>作品</h3>
          <p>{projects.data?.totalCount ?? 0} 个</p>
          <Link to="/admin/projects">管理</Link>
        </div>
        <div className="card">
          <h3>文章</h3>
          <p>{articles.data?.totalCount ?? 0} 篇</p>
          <Link to="/admin/articles">管理</Link>
        </div>
        <div className="card">
          <h3>留言</h3>
          <p>{messages.data?.totalCount ?? 0} 条</p>
          <Link to="/admin/messages">处理</Link>
        </div>
      </div>
    </div>
  );
}
