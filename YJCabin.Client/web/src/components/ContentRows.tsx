import { Link, matchPath, useLocation, type LinkProps, type Location } from "react-router-dom";
import type { ArticleSummary, ProjectSummary } from "../api/types";
import { pickSlug } from "../lib/content";
import { formatDate, padIndex } from "../lib/format";
import { TagList } from "./TagList";

function readerBackground(location: Location): Location {
  const existing = (location.state as { background?: Location } | null)?.background;
  if (existing) return existing;
  if (matchPath("/articles/:slug", location.pathname)) {
    return { ...location, pathname: "/articles", search: "", hash: "", state: null };
  }
  if (matchPath("/works/:slug", location.pathname)) {
    return { ...location, pathname: "/works", search: "", hash: "", state: null };
  }
  return location;
}

export function ReaderLink({ state, ...props }: LinkProps) {
  const location = useLocation();
  const extra = state && typeof state === "object" ? state : {};
  return <Link {...props} state={{ ...extra, background: readerBackground(location) }} />;
}

export function WorkRow({ item, index }: { item: ProjectSummary; index: number }) {
  const slug = pickSlug(item);
  const body = (
    <>
      <span className="row-meta">{padIndex(index)}</span>
      <div>
        <h3>{item.title}</h3>
        <p className="muted">{item.summary}</p>
        <TagList tags={item.tags} />
      </div>
      <time className="row-meta" dateTime={item.publishedAt ?? undefined}>
        {formatDate(item.publishedAt)}
      </time>
    </>
  );

  return (
    <li>
      {slug ? (
        <ReaderLink to={`/works/${encodeURIComponent(slug)}`} className="work-row">
          {body}
        </ReaderLink>
      ) : (
        <div className="work-row">{body}</div>
      )}
    </li>
  );
}

export function ArticleRow({ item }: { item: ArticleSummary }) {
  const slug = pickSlug(item);
  const body = (
    <>
      <time className="row-meta" dateTime={item.publishedAt ?? undefined}>
        {formatDate(item.publishedAt) || "未标注日期"}
      </time>
      <div>
        <h3>{item.title}</h3>
        <p className="muted">{item.summary}</p>
        <TagList tags={item.tags} />
      </div>
    </>
  );

  return (
    <li>
      {slug ? (
        <ReaderLink to={`/articles/${encodeURIComponent(slug)}`} className="row-link">
          {body}
        </ReaderLink>
      ) : (
        <div className="row-link">{body}</div>
      )}
    </li>
  );
}
