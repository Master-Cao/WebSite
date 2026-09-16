import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { api } from "../../api/client";
import type { ArticleSummary, PagedResult } from "../../api/types";

export function ArticlesAdminPage() {
  const client = useQueryClient();
  const query = useQuery({
    queryKey: ["admin", "articles"],
    queryFn: () => api<PagedResult<ArticleSummary>>("/api/admin/articles", {}, true)
  });
  const remove = useMutation({
    mutationFn: (slug: string) => api(`/api/admin/articles/${slug}`, { method: "DELETE" }, true),
    onSuccess: () => client.invalidateQueries({ queryKey: ["admin", "articles"] })
  });

  return (
    <div className="stack">
      <div className="section-head">
        <h1>文章</h1>
        <Link to="/admin/articles/new">新建</Link>
      </div>
      <table className="table">
        <thead>
          <tr>
            <th>标题</th>
            <th>状态</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {query.data?.items.map((item) => (
            <tr key={item.id}>
              <td>{item.title}</td>
              <td>{item.status}</td>
              <td>
                <Link to={`/admin/articles/${item.slug}`}>编辑</Link>{" "}
                <button onClick={() => remove.mutate(item.slug)}>删除</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
