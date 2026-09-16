import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "../../api/client";
import type { ContactMessage, PagedResult } from "../../api/types";
import { Note } from "../../components/Note";
import { btnGhost } from "../../ui";

export function MessagesAdminPage() {
  const client = useQueryClient();
  const query = useQuery({
    queryKey: ["admin", "messages"],
    queryFn: () => api<PagedResult<ContactMessage>>("/api/admin/contact-messages", {}, true)
  });
  const patch = useMutation({
    mutationFn: ({ id, isRead, isReplied }: { id: string; isRead?: boolean; isReplied?: boolean }) =>
      api(`/api/admin/contact-messages/${id}`, { method: "PATCH", body: JSON.stringify({ isRead, isReplied }) }, true),
    onSuccess: () => client.invalidateQueries({ queryKey: ["admin", "messages"] })
  });

  return (
    <div className="glass panel">
      <h1 className="page-title">留言</h1>
      {!query.data?.items.length && !query.isLoading && <div className="mt-8"><Note>暂无留言。</Note></div>}
      <div className="mt-8 grid gap-8">
        {query.data?.items.map((item) => (
          <article key={item.id} className="border-t border-line pt-5">
            <div className="flex items-baseline justify-between gap-4">
              <h2 className="text-xl tracking-tight">{item.subject}</h2>
              <span className="text-sm text-muted">{item.isRead ? "已读" : "未读"}{item.isReplied ? " · 已回复" : ""}</span>
            </div>
            <p className="mt-1 text-sm text-muted">
              {item.name} · {item.email}
            </p>
            <p className="mt-3 max-w-[65ch] text-muted">{item.body}</p>
            <div className="mt-4 flex gap-3">
              <button type="button" className={btnGhost} onClick={() => patch.mutate({ id: item.id, isRead: !item.isRead })}>
                {item.isRead ? "标为未读" : "标为已读"}
              </button>
              <button type="button" className={btnGhost} onClick={() => patch.mutate({ id: item.id, isReplied: !item.isReplied })}>
                {item.isReplied ? "取消已回复" : "标为已回复"}
              </button>
            </div>
          </article>
        ))}
      </div>
    </div>
  );
}
