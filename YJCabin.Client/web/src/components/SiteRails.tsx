import { FormEvent } from "react";
import { Link, NavLink, useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";
import type { AboutDto, ArticleSummary, PagedResult, ProjectSummary, TagDto } from "../api/types";
import { firstPlainLine, formatShortDate } from "../lib/format";

type TimelineEntry = {
  id: string;
  title: string;
  href: string;
  kind: "article" | "work";
  at?: string | null;
};

function timeValue(value?: string | null) {
  if (!value) return 0;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? 0 : date.getTime();
}

function yearLabel(value?: string | null) {
  if (!value) return "未标注";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "未标注";
  return `${date.getFullYear()}`;
}

export function ProfileSearchRail() {
  const navigate = useNavigate();
  const location = useLocation();
  const [params] = useSearchParams();
  const about = useQuery({ queryKey: ["about"], queryFn: () => api<AboutDto>("/api/about") });
  const tags = useQuery({ queryKey: ["tags"], queryFn: () => api<TagDto[]>("/api/tags") });
  const scope = location.pathname.startsWith("/works") ? "works" : "articles";
  const activeTag = params.get("tag") ?? "";
  const catalogPath = scope === "works" ? "/works" : "/articles";

  const onSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const nextQ = String(form.get("q") ?? "").trim();
    const next = new URLSearchParams();
    if (nextQ) next.set("q", nextQ);
    if (activeTag) next.set("tag", activeTag);
    const query = next.toString();
    navigate(query ? `${catalogPath}?${query}` : catalogPath);
  };

  const goCatalog = (nextTag = "") => {
    const next = new URLSearchParams();
    const currentQ = params.get("q");
    if (currentQ) next.set("q", currentQ);
    if (nextTag) next.set("tag", nextTag);
    const query = next.toString();
    navigate(query ? `${catalogPath}?${query}` : catalogPath);
  };

  const skills = about.data?.skills.slice(0, 6) ?? [];

  return (
    <>
      <section className="rail-block">
        <div className="rail-avatar-wrap">
          <img src="/yjcabin-puppy.png" alt="" className="rail-avatar" />
        </div>
        <h2 className="rail-name">{about.data?.headline ?? "YJCabin"}</h2>
        <p className="rail-bio">{firstPlainLine(about.data?.bioMarkdown)}</p>
        {skills.length > 0 && (
          <ul className="rail-skills">
            {skills.map((skill) => (
              <li key={skill}>{skill}</li>
            ))}
          </ul>
        )}
        <nav className="rail-links">
          <NavLink to="/about">完整简介</NavLink>
          <NavLink to="/contact">联系</NavLink>
          {about.data?.socialLinks.slice(0, 3).map((link) => (
            <a key={link.url} href={link.url} target="_blank" rel="noreferrer">
              {link.name}
            </a>
          ))}
        </nav>
      </section>

      <section className="rail-block">
        <p className="kicker">分类检索</p>
        <div className="rail-scope">
          <Link to="/articles" className={scope === "articles" ? "is-on" : undefined}>
            文章
          </Link>
          <Link to="/works" className={scope === "works" ? "is-on" : undefined}>
            作品
          </Link>
        </div>
        <form className="rail-search" onSubmit={onSearch} key={`${scope}-${params.get("q") ?? ""}`}>
          <label className="sr-only" htmlFor="rail-q">
            检索
          </label>
          <input
            id="rail-q"
            name="q"
            defaultValue={params.get("q") ?? ""}
            className="control"
            placeholder={scope === "works" ? "搜索作品" : "搜索文章"}
          />
          <button type="submit" className="btn btn-primary">
            检索
          </button>
        </form>
        <div className="chip-row rail-chips">
          <button type="button" className={`chip ${!activeTag ? "is-on" : ""}`} onClick={() => goCatalog()}>
            全部
          </button>
          {tags.data?.map((item) => (
            <button
              key={item.id}
              type="button"
              className={`chip ${activeTag === item.slug ? "is-on" : ""}`}
              onClick={() => goCatalog(item.slug)}
            >
              {item.name}
            </button>
          ))}
        </div>
        {!tags.isLoading && !tags.data?.length && <p className="muted rail-empty">暂无分类。</p>}
      </section>
    </>
  );
}

export function TimelineRail() {
  const articles = useQuery({
    queryKey: ["articles", "timeline"],
    queryFn: () => api<PagedResult<ArticleSummary>>("/api/articles?pageSize=12")
  });
  const projects = useQuery({
    queryKey: ["projects", "timeline"],
    queryFn: () => api<PagedResult<ProjectSummary>>("/api/projects?pageSize=8")
  });

  const entries: TimelineEntry[] = [
    ...(articles.data?.items.map((item) => ({
      id: `article-${item.id}`,
      title: item.title,
      href: `/articles/${item.slug}`,
      kind: "article" as const,
      at: item.publishedAt
    })) ?? []),
    ...(projects.data?.items.map((item) => ({
      id: `work-${item.id}`,
      title: item.title,
      href: `/works/${item.slug}`,
      kind: "work" as const,
      at: item.publishedAt
    })) ?? [])
  ].sort((a, b) => timeValue(b.at) - timeValue(a.at));

  const groups = new Map<string, TimelineEntry[]>();
  for (const entry of entries) {
    const key = yearLabel(entry.at);
    const list = groups.get(key) ?? [];
    list.push(entry);
    groups.set(key, list);
  }

  return (
    <section className="rail-block">
      <p className="kicker">时间线</p>
      <p className="muted rail-lead">最近发布的文章与作品。</p>
      {(articles.isLoading || projects.isLoading) && <p className="muted rail-empty">载入中…</p>}
      {!articles.isLoading && !projects.isLoading && !entries.length && <p className="muted rail-empty">还没有动态。</p>}
      {[...groups.entries()].map(([year, items]) => (
        <div key={year} className="timeline-year">
          <h3>{year}</h3>
          <ol className="timeline">
            {items.map((item) => (
              <li key={item.id}>
                <span className="timeline-stem" aria-hidden="true" />
                <Link to={item.href} className="timeline-card">
                  <span className="timeline-meta">
                    <time dateTime={item.at ?? undefined}>{formatShortDate(item.at) || "无日期"}</time>
                    <span>{item.kind === "work" ? "作品" : "文章"}</span>
                  </span>
                  <strong>{item.title}</strong>
                </Link>
              </li>
            ))}
          </ol>
        </div>
      ))}
      <nav className="rail-links">
        <NavLink to="/archive">查看归档</NavLink>
      </nav>
    </section>
  );
}
