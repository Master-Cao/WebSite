import { Link } from "react-router-dom";
import type { ArticleSummary, ProjectSummary } from "../api/types";
import { formatDate, padIndex } from "../lib/format";
import { TagList } from "./TagList";

export function WorkRow({ item, index, href }: { item: ProjectSummary; index: number; href: string }) {
  return (
    <li>
      <Link to={href} className="work-row">
        <span className="row-meta">{padIndex(index)}</span>
        <div>
          <h3>{item.title}</h3>
          <p className="muted">{item.summary}</p>
          <TagList tags={item.tags} />
        </div>
        <time className="row-meta" dateTime={item.publishedAt ?? undefined}>
          {formatDate(item.publishedAt)}
        </time>
      </Link>
    </li>
  );
}

export function ArticleRow({ item, href }: { item: ArticleSummary; href: string }) {
  return (
    <li>
      <Link to={href} className="row-link">
        <time className="row-meta" dateTime={item.publishedAt ?? undefined}>
          {formatDate(item.publishedAt) || "未标注日期"}
        </time>
        <div>
          <h3>{item.title}</h3>
          <p className="muted">{item.summary}</p>
        </div>
      </Link>
    </li>
  );
}
