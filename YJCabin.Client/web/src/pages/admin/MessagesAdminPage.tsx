import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "../../api/client";
import type { ContactMessage, PagedResult } from "../../api/types";

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
    <div className="stack">
      <h1>留言</h1>
      {query.data?.items.map((item) => (
        <article className="card" key={item.id}>
          <h3>{item.subject}</h3>
          <p>
            {item.name} · {item.email}
          </p>
          <p>{item.body}</p>
          <div className="actions">
            <button onClick={() => patch.mutate({ id: item.id, isRead: !item.isRead })}>
              {item.isRead ? "标为未读" : "标为已读"}
            </button>
            <button onClick={() => patch.mutate({ id: item.id, isReplied: !item.isReplied })}>
              {item.isReplied ? "取消已回复" : "标为已回复"}
            </button>
          </div>
        </article>
      ))}
    </div>
  );
}
