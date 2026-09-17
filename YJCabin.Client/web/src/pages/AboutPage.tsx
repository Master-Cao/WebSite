import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";
import type { AboutDto } from "../api/types";
import { MarkdownHtml } from "../components/MarkdownHtml";
import { Note } from "../components/Note";

export function AboutPage() {
  const query = useQuery({ queryKey: ["about"], queryFn: () => api<AboutDto>("/api/about") });
  if (query.isLoading) {
    return (
      <div className="wrap">
        <div className="glass panel">
          <Note>加载中</Note>
        </div>
      </div>
    );
  }
  if (!query.data) {
    return (
      <div className="wrap">
        <div className="glass panel">
          <Note>暂无介绍。</Note>
        </div>
      </div>
    );
  }
  return (
    <div className="wrap about-grid">
      <article className="glass panel">
        <p className="kicker">简介</p>
        <h1 className="page-title">{query.data.headline}</h1>
        <MarkdownHtml html={query.data.bioHtml} />
      </article>
      <aside className="glass panel">
        <h2 className="muted" style={{ display: "flex", alignItems: "center", gap: 8 }}>
          <img src="/yjcabin-cat.png" alt="" width={28} height={28} style={{ objectFit: "contain" }} />
          技能
        </h2>
        <ul className="tags" style={{ marginTop: 12 }}>
          {query.data.skills.map((skill) => (
            <li key={skill}>{skill}</li>
          ))}
        </ul>
        <h2 className="muted" style={{ marginTop: 28 }}>
          链接
        </h2>
        <ul style={{ display: "grid", gap: 8, marginTop: 12, padding: 0, listStyle: "none" }}>
          {query.data.socialLinks.map((link) => (
            <li key={link.url}>
              <a className="accent" href={link.url} target="_blank" rel="noreferrer">
                {link.name}
              </a>
            </li>
          ))}
        </ul>
      </aside>
    </div>
  );
}
