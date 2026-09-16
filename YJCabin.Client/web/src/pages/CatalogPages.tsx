import { FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";
import type { ArticleSummary, PagedResult, ProjectSummary, TagDto } from "../api/types";
import { ArticleRow, WorkRow } from "../components/ContentRows";
import { Note, SkeletonRows } from "../components/Note";
import { btnPrimary, controlClass } from "../ui";

type Kind = "works" | "articles";

export function CatalogPage({ kind }: { kind: Kind }) {
  const [params, setParams] = useSearchParams();
  const tag = params.get("tag") ?? "";
  const q = params.get("q") ?? "";
  const endpoint = kind === "works" ? "/api/projects" : "/api/articles";
  const path = kind === "works" ? "/works" : "/articles";
  const title = kind === "works" ? "作品" : "文章";
  const query = useQuery({
    queryKey: [kind, tag, q],
    queryFn: () => {
      const search = new URLSearchParams();
      if (tag) search.set("tag", tag);
      if (q) search.set("q", q);
      return api<PagedResult<ProjectSummary | ArticleSummary>>(`${endpoint}?${search.toString()}`);
    }
  });
  const tags = useQuery({ queryKey: ["tags"], queryFn: () => api<TagDto[]>("/api/tags") });

  const onSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const nextQ = String(form.get("q") ?? "");
    const next = new URLSearchParams();
    if (nextQ) next.set("q", nextQ);
    if (tag) next.set("tag", tag);
    setParams(next);
  };

  return (
    <div className="wrap">
      <div className="glass panel">
        <div className="page-head">
          <h1 className="page-title">{title}</h1>
          <p className="muted">{kind === "works" ? "按项目查看可运行的作品。" : "按时间阅读工程笔记。"}</p>
        </div>
        <form className="search-row" onSubmit={onSearch}>
          <label className="sr-only" htmlFor="q">
            搜索{title}
          </label>
          <input id="q" name="q" defaultValue={q} className={controlClass} placeholder={`搜索${title}`} />
          <button type="submit" className={btnPrimary}>
            搜索
          </button>
        </form>
        <div className="chip-row">
          <button type="button" className={`chip ${!tag ? "is-on" : ""}`} onClick={() => setParams(q ? { q } : {})}>
            全部
          </button>
          {tags.data?.map((item) => (
            <button
              key={item.id}
              type="button"
              className={`chip ${tag === item.slug ? "is-on" : ""}`}
              onClick={() => setParams({ ...(q ? { q } : {}), tag: item.slug })}
            >
              {item.name}
            </button>
          ))}
        </div>
        <div style={{ marginTop: 18 }}>
          {query.isLoading && <SkeletonRows count={4} />}
          {query.error && <Note>加载失败，请稍后重试。</Note>}
          {!query.isLoading && !query.data?.items.length && <Note>{`没有符合条件的${title}。`}</Note>}
          <ul className="row-list catalog-list">
            {query.data?.items.map((item, index) =>
              kind === "works" ? (
                <WorkRow key={item.id} item={item as ProjectSummary} index={index} href={`${path}/${item.slug}`} />
              ) : (
                <ArticleRow key={item.id} item={item as ArticleSummary} href={`${path}/${item.slug}`} />
              )
            )}
          </ul>
        </div>
      </div>
    </div>
  );
}
