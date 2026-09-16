import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { api } from "../../api/client";
import type { ArticleSummary, PagedResult } from "../../api/types";
import { Note } from "../../components/Note";
import { StatusBadge } from "../../components/StatusBadge";
import { btnDanger, btnPrimary, btnText, tableClass } from "../../ui";

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
    <div className="glass panel">
      <div className="chip-row" style={{ justifyContent: "space-between" }}>
        <h1 className="page-title">文章</h1>
        <Link to="/admin/articles/new" className={btnPrimary}>
          新建
        </Link>
      </div>
      {!query.data?.items.length && !query.isLoading && (
        <div className="mt-8">
          <Note>还没有文章。</Note>
        </div>
      )}
      <table className={tableClass}>
        <thead>
          <tr className="border-b border-line text-muted">
            <th className="py-3 font-normal">标题</th>
            <th className="py-3 font-normal">状态</th>
            <th className="py-3 font-normal" />
          </tr>
        </thead>
        <tbody>
          {query.data?.items.map((item) => (
            <tr key={item.id} className="border-b border-line">
              <td className="py-3">{item.title}</td>
              <td className="py-3">
                <StatusBadge status={item.status} />
              </td>
              <td className="py-3 text-right">
                <Link to={`/admin/articles/${item.slug}`} className={btnText}>
                  编辑
                </Link>
                <button type="button" className={`${btnDanger} ml-4`} onClick={() => remove.mutate(item.slug)}>
                  删除
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
