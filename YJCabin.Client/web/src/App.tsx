import { matchPath, Navigate, Route, Routes, useLocation, type Location } from "react-router-dom";
import { PublicLayout } from "./layouts/PublicLayout";
import { HomePage } from "./pages/HomePage";
import { WorksPage } from "./pages/WorksPage";
import { WorkDetailPage } from "./pages/WorkDetailPage";
import { ArticlesPage } from "./pages/ArticlesPage";
import { ArticleDetailPage } from "./pages/ArticleDetailPage";
import { ArchivePage } from "./pages/ArchivePage";
import { AboutPage } from "./pages/AboutPage";
import { isContentSlug } from "./lib/content";

function readerMatch(pathname: string) {
  const article = matchPath("/articles/:slug", pathname);
  const articleSlug = article?.params.slug ? decodeURIComponent(article.params.slug) : "";
  if (isContentSlug(articleSlug)) return { kind: "article" as const, slug: articleSlug };
  const work = matchPath("/works/:slug", pathname);
  const workSlug = work?.params.slug ? decodeURIComponent(work.params.slug) : "";
  if (isContentSlug(workSlug)) return { kind: "work" as const, slug: workSlug };
  return null;
}

function routesLocation(location: Location): Location {
  const reader = readerMatch(location.pathname);
  if (!reader) return location;
  const background = (location.state as { background?: Location } | null)?.background;
  if (background) return background;
  return {
    ...location,
    pathname: reader.kind === "article" ? "/articles" : "/works",
    search: "",
    hash: "",
    state: null
  };
}

export default function App() {
  const location = useLocation();
  const reader = readerMatch(location.pathname);

  return (
    <>
      <Routes location={routesLocation(location)}>
        <Route element={<PublicLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/works" element={<WorksPage />} />
          <Route path="/articles" element={<ArticlesPage />} />
          <Route path="/archive" element={<ArchivePage />} />
          <Route path="/about" element={<AboutPage />} />
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      {reader?.kind === "article" && <ArticleDetailPage slug={reader.slug} />}
      {reader?.kind === "work" && <WorkDetailPage slug={reader.slug} />}
    </>
  );
}
