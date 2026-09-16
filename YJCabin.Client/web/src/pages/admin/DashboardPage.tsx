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

  const unread = messages.data?.items.filter((item) => !item.isRead).length ?? 0;
  const stats = [
    { label: "作品", value: projects.data?.totalCount ?? 0, hint: "全部条目", to: "/admin/projects" },
    { label: "文章", value: articles.data?.totalCount ?? 0, hint: "全部条目", to: "/admin/articles" },
    { label: "留言", value: messages.data?.totalCount ?? 0, hint: unread ? `${unread} 条未读` : "暂无未读", to: "/admin/messages" }
  ];

  return (
    <div>
      <h1 className="page-title">概览</h1>
      <p className="muted">站点内容的当前规模。</p>
      <dl className="split section-gap">
        {stats.map((item) => (
          <div key={item.label} className="glass panel">
            <dt className="muted">{item.label}</dt>
            <dd className="page-title">{item.value}</dd>
            <p className="muted">{item.hint}</p>
            <Link to={item.to} className="accent">
              管理
            </Link>
          </div>
        ))}
      </dl>
    </div>
  );
}
