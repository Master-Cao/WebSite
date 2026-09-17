import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";
import type { ArticleSummary, PagedResult } from "../api/types";
import { ReaderLink } from "../components/ContentRows";
import { pickSlug } from "../lib/content";
import { Note, SkeletonRows } from "../components/Note";
import { TagList } from "../components/TagList";
import { formatDate } from "../lib/format";

async function fetchAllArticles() {
  const first = await api<PagedResult<ArticleSummary>>("/api/articles?page=1&pageSize=50");
  const items = [...first.items];
  for (let page = 2; page <= first.totalPages; page += 1) {
    const next = await api<PagedResult<ArticleSummary>>(`/api/articles?page=${page}&pageSize=50`);
    items.push(...next.items);
  }
  return items;
}

function yearOf(item: ArticleSummary) {
  if (!item.publishedAt) return "未标注日期";
  const date = new Date(item.publishedAt);
  if (Number.isNaN(date.getTime())) return "未标注日期";
  return `${date.getFullYear()} 年`;
}

function groupByYear(items: ArticleSummary[]) {
  const groups = new Map<string, ArticleSummary[]>();
  for (const item of items) {
    const key = yearOf(item);
    const list = groups.get(key) ?? [];
    list.push(item);
    groups.set(key, list);
  }
  return [...groups.entries()];
}

export function ArchivePage() {
  const query = useQuery({
    queryKey: ["articles", "archive"],
    queryFn: fetchAllArticles
  });
  const groups = query.data ? groupByYear(query.data) : [];

  return (
    <div className="wrap">
      <div className="glass panel">
        <h1 className="page-title">归档</h1>
        <p className="muted">按年份查看全部文章。</p>
        <div style={{ marginTop: 28 }}>
          {query.isLoading && <SkeletonRows count={5} />}
          {query.error && <Note>暂时无法加载归档。</Note>}
          {!query.isLoading && !groups.length && <Note>还没有可归档的文章。</Note>}
          <div className="archive-years">
            {groups.map(([year, items]) => (
              <section key={year}>
                <h2>{year}</h2>
                <p className="row-meta">{items.length} 篇</p>
                <ul className="row-list">
                  {items.map((item) => {
                    const slug = pickSlug(item);
                    return (
                    <li key={item.id}>
                      {slug ? (
                      <ReaderLink to={`/articles/${encodeURIComponent(slug)}`} className="row-link">
                        <time className="row-meta" dateTime={item.publishedAt ?? undefined}>
                          {formatDate(item.publishedAt) || "无日期"}
                        </time>
                        <h3>{item.title}</h3>
                        <TagList tags={item.tags} />
                      </ReaderLink>
                      ) : (
                        <div className="row-link">
                          <time className="row-meta" dateTime={item.publishedAt ?? undefined}>
                            {formatDate(item.publishedAt) || "无日期"}
                          </time>
                          <h3>{item.title}</h3>
                          <TagList tags={item.tags} />
                        </div>
                      )}
                    </li>
                    );
                  })}
                </ul>
              </section>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
