import { useQuery } from "@tanstack/react-query";
import { Link, useSearchParams } from "react-router-dom";
import { api } from "../api/client";
import type { ArticleSummary, PagedResult, TagDto } from "../api/types";
import { TagList } from "../components/TagList";

export function ArticlesPage() {
  const [params, setParams] = useSearchParams();
  const tag = params.get("tag") ?? "";
  const q = params.get("q") ?? "";
  const query = useQuery({
    queryKey: ["articles", tag, q],
    queryFn: () => {
      const search = new URLSearchParams();
      if (tag) search.set("tag", tag);
      if (q) search.set("q", q);
      return api<PagedResult<ArticleSummary>>(`/api/articles?${search.toString()}`);
    }
  });
  const tags = useQuery({ queryKey: ["tags"], queryFn: () => api<TagDto[]>("/api/tags") });

  return (
    <div className="stack">
      <h1>文章</h1>
      <form
        className="filters"
        onSubmit={(event) => {
          event.preventDefault();
          const form = new FormData(event.currentTarget);
          const nextQ = String(form.get("q") ?? "");
          const next = new URLSearchParams();
          if (nextQ) next.set("q", nextQ);
          if (tag) next.set("tag", tag);
          setParams(next);
        }}
      >
        <input name="q" defaultValue={q} placeholder="搜索文章" />
        <button type="submit">搜索</button>
      </form>
      <div className="tag-filter">
        <button className={!tag ? "active" : ""} onClick={() => setParams(q ? { q } : {})}>
          全部
        </button>
        {tags.data?.map((item) => (
          <button
            key={item.id}
            className={tag === item.slug ? "active" : ""}
            onClick={() => setParams({ ...(q ? { q } : {}), tag: item.slug })}
          >
            {item.name}
          </button>
        ))}
      </div>
      <div className="cards">
        {query.data?.items.map((item) => (
          <Link className="card" key={item.id} to={`/articles/${item.slug}`}>
            <h3>{item.title}</h3>
            <p>{item.summary}</p>
            <TagList tags={item.tags} />
          </Link>
        ))}
      </div>
    </div>
  );
}
