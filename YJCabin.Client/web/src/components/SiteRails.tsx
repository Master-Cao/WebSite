import { FormEvent, useState } from "react";
import { Link, NavLink, useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { EnvelopeSimple, GithubLogo, WechatLogo } from "@phosphor-icons/react";
import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";
import type { AboutDto, ArticleSummary, PagedResult, ProjectSummary } from "../api/types";
import { resolveContact, type ContactKind } from "../lib/contacts";
import { pickSlug } from "../lib/content";
import { formatShortDate } from "../lib/format";
import { BeianNotice } from "./BeianNotice";
import { ReaderLink } from "./ContentRows";
import { ContactCardDialog } from "./ContactCardDialog";

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

function QqMark() {
  return (
    <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true">
      <path
        fill="currentColor"
        d="M12 2.4c-2.2 0-4.4 1.7-4.8 4.3-.2 1.4.1 2.8.7 4-1.3.6-2.3 1.7-2.7 3.1-.2.8.4 1.3 1 1 .4-.1.7-.4.9-.7.2 1.3.9 2.5 1.9 3.3-.4.3-.9.8-.8 1.4.1.7.9 1.1 1.6 1.2 1.2.2 2.4 0 3.5-.4.4.2.9.3 1.4.3s1-.1 1.4-.3c1.1.4 2.3.6 3.5.4.7-.1 1.5-.5 1.6-1.2.1-.6-.4-1.1-.8-1.4 1-.8 1.7-2 1.9-3.3.2.3.5.6.9.7.6.2 1.2-.2 1-1-.4-1.4-1.4-2.5-2.7-3.1.6-1.2.9-2.6.7-4C16.4 4.1 14.2 2.4 12 2.4Z"
      />
    </svg>
  );
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
  const articles = useQuery({
    queryKey: ["articles", "timeline"],
    queryFn: () => api<PagedResult<ArticleSummary>>("/api/articles?pageSize=12")
  });
  const projects = useQuery({
    queryKey: ["projects", "timeline"],
    queryFn: () => api<PagedResult<ProjectSummary>>("/api/projects?pageSize=8")
  });
  const scope = location.pathname.startsWith("/works") ? "works" : "articles";
  const catalogPath = scope === "works" ? "/works" : "/articles";

  const onSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const nextQ = String(form.get("q") ?? "").trim();
    navigate(nextQ ? `${catalogPath}?q=${encodeURIComponent(nextQ)}` : catalogPath);
  };

  const skills = about.data?.skills.slice(0, 6) ?? [];
  const [openKind, setOpenKind] = useState<ContactKind>();
  const links = about.data?.socialLinks;
  const openCard = openKind ? resolveContact(openKind, links) : undefined;

  return (
    <>
      <section className="rail-block">
        <div className="rail-avatar-wrap">
          <img src="/yjcabin-puppy.png" alt="" className="rail-avatar" />
        </div>
        <h2 className="rail-name">{about.data?.headline ?? (about.isFetched ? "YJCabin" : "\u00a0")}</h2>
        <nav className="rail-contacts" aria-label="联系方式">
          <button type="button" className="rail-contact" title="QQ" aria-label="QQ" onClick={() => setOpenKind("qq")}>
            <QqMark />
          </button>
          <button type="button" className="rail-contact" title="邮箱" aria-label="邮箱" onClick={() => setOpenKind("email")}>
            <EnvelopeSimple size={18} weight="duotone" />
          </button>
          <button type="button" className="rail-contact" title="微信" aria-label="微信" onClick={() => setOpenKind("wechat")}>
            <WechatLogo size={18} weight="duotone" />
          </button>
          <button type="button" className="rail-contact" title="GitHub" aria-label="GitHub" onClick={() => setOpenKind("github")}>
            <GithubLogo size={18} weight="duotone" />
          </button>
        </nav>
        {openCard && <ContactCardDialog card={openCard} onClose={() => setOpenKind(undefined)} />}
        {skills.length > 0 && (
          <ul className="rail-skills">
            {skills.map((skill) => (
              <li key={skill}>{skill}</li>
            ))}
          </ul>
        )}
        <div className="rail-stats">
          <Link to="/articles" className="rail-stat">
            <strong>{articles.data?.totalCount ?? "—"}</strong>
            <span>文章</span>
          </Link>
          <Link to="/works" className="rail-stat">
            <strong>{projects.data?.totalCount ?? "—"}</strong>
            <span>作品</span>
          </Link>
        </div>
      </section>

      <section className="rail-block">
        <p className="kicker">检索</p>
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
      </section>

      <section className="rail-block rail-legal">
        <p className="kicker">声明</p>
        <BeianNotice />
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
    ...(articles.data?.items.flatMap((item) => {
      const slug = pickSlug(item);
      return slug
        ? [{
            id: `article-${item.id}`,
            title: item.title,
            href: `/articles/${encodeURIComponent(slug)}`,
            kind: "article" as const,
            at: item.publishedAt
          }]
        : [];
    }) ?? []),
    ...(projects.data?.items.flatMap((item) => {
      const slug = pickSlug(item);
      return slug
        ? [{
            id: `work-${item.id}`,
            title: item.title,
            href: `/works/${encodeURIComponent(slug)}`,
            kind: "work" as const,
            at: item.publishedAt
          }]
        : [];
    }) ?? [])
  ].sort((a, b) => timeValue(b.at) - timeValue(a.at));

  const groups = new Map<string, TimelineEntry[]>();
  for (const entry of entries) {
    const key = yearLabel(entry.at);
    const list = groups.get(key) ?? [];
    list.push(entry);
    groups.set(key, list);
  }

  return (
    <section className="rail-block rail-timeline">
      <p className="kicker">时间线</p>
      <p className="muted rail-lead">最近发布的文章与作品。</p>
      <div className="timeline-body">
        {(articles.isLoading || projects.isLoading) && <p className="muted rail-empty">载入中…</p>}
        {!articles.isLoading && !projects.isLoading && !entries.length && <p className="muted rail-empty">还没有动态。</p>}
        {[...groups.entries()].map(([year, items]) => (
          <div key={year} className="timeline-year">
            <h3>{year}</h3>
            <ol className="timeline">
              {items.map((item) => (
                <li key={item.id}>
                  <span className="timeline-stem" aria-hidden="true" />
                  <ReaderLink to={item.href} className="timeline-card">
                    <span className="timeline-meta">
                      <time dateTime={item.at ?? undefined}>{formatShortDate(item.at) || "无日期"}</time>
                      <span>{item.kind === "article" ? "文章" : "作品"}</span>
                    </span>
                    <strong>{item.title}</strong>
                  </ReaderLink>
                </li>
              ))}
            </ol>
          </div>
        ))}
      </div>
      <nav className="rail-links">
        <NavLink to="/archive">查看归档</NavLink>
      </nav>
    </section>
  );
}

export function MessageRail() {
  const [status, setStatus] = useState<"idle" | "sending" | "ok" | "err">("idle");
  const [error, setError] = useState("");

  const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);
    setStatus("sending");
    setError("");
    try {
      await api("/api/contact", {
        method: "POST",
        body: JSON.stringify({
          name: String(data.get("name") ?? "").trim(),
          email: String(data.get("email") ?? "").trim(),
          subject: "网站留言",
          body: String(data.get("body") ?? "").trim(),
          website: String(data.get("website") ?? "")
        })
      });
      form.reset();
      setStatus("ok");
    } catch (err) {
      setStatus("err");
      setError(err instanceof Error ? err.message : "留言失败，请稍后再试。");
    }
  };

  return (
    <section className="rail-block rail-message">
      <p className="kicker">留言</p>
      <p className="muted rail-lead">想说的话可以直接留在这里。</p>
      <form className="rail-message-form" onSubmit={onSubmit}>
        <input name="website" className="sr-only" tabIndex={-1} autoComplete="off" aria-hidden="true" />
        <label className="sr-only" htmlFor="rail-msg-name">
          称呼
        </label>
        <input id="rail-msg-name" name="name" className="control" required maxLength={80} placeholder="称呼" />
        <label className="sr-only" htmlFor="rail-msg-email">
          邮箱
        </label>
        <input
          id="rail-msg-email"
          name="email"
          type="email"
          className="control"
          required
          maxLength={120}
          placeholder="邮箱"
        />
        <label className="sr-only" htmlFor="rail-msg-body">
          留言
        </label>
        <textarea
          id="rail-msg-body"
          name="body"
          className="control"
          required
          rows={4}
          maxLength={2000}
          placeholder="想说的话"
        />
        <button type="submit" className="btn btn-primary" disabled={status === "sending"}>
          {status === "sending" ? "提交中…" : "留言"}
        </button>
        {status === "ok" && <p className="muted rail-empty">已收到，谢谢。</p>}
        {status === "err" && <p className="danger rail-empty">{error}</p>}
      </form>
    </section>
  );
}
