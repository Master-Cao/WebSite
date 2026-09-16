import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";
import type { AboutDto } from "../api/types";

export function AboutPage() {
  const query = useQuery({ queryKey: ["about"], queryFn: () => api<AboutDto>("/api/about") });
  if (query.isLoading) return <p>加载中…</p>;
  if (!query.data) return <p>暂无介绍。</p>;
  return (
    <article className="stack prose">
      <h1>关于我</h1>
      <p className="lede">{query.data.headline}</p>
      <div className="markdown" dangerouslySetInnerHTML={{ __html: query.data.bioHtml }} />
      <h2>技能</h2>
      <ul className="tags">
        {query.data.skills.map((skill) => (
          <li key={skill}>{skill}</li>
        ))}
      </ul>
      <h2>链接</h2>
      <ul>
        {query.data.socialLinks.map((link) => (
          <li key={link.url}>
            <a href={link.url} target="_blank" rel="noreferrer">
              {link.name}
            </a>
          </li>
        ))}
      </ul>
    </article>
  );
}
