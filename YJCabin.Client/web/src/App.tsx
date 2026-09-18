import { lazy, Suspense } from "react";
import { matchPath, Navigate, Route, Routes, useLocation, type Location } from "react-router-dom";
import { PublicLayout } from "./layouts/PublicLayout";
import { HomePage } from "./pages/HomePage";
import { isContentSlug } from "./lib/content";

const WorksPage = lazy(() => import("./pages/WorksPage").then((module) => ({ default: module.WorksPage })));
const ArticlesPage = lazy(() => import("./pages/ArticlesPage").then((module) => ({ default: module.ArticlesPage })));
const ArchivePage = lazy(() => import("./pages/ArchivePage").then((module) => ({ default: module.ArchivePage })));
const AboutPage = lazy(() => import("./pages/AboutPage").then((module) => ({ default: module.AboutPage })));
const ArticleDetailPage = lazy(() =>
  import("./pages/ArticleDetailPage").then((module) => ({ default: module.ArticleDetailPage }))
);
const WorkDetailPage = lazy(() =>
  import("./pages/WorkDetailPage").then((module) => ({ default: module.WorkDetailPage }))
);

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
      <Suspense fallback={null}>
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
      </Suspense>
      {reader?.kind === "article" && (
        <Suspense fallback={null}>
          <ArticleDetailPage slug={reader.slug} />
        </Suspense>
      )}
      {reader?.kind === "work" && (
        <Suspense fallback={null}>
          <WorkDetailPage slug={reader.slug} />
        </Suspense>
      )}
    </>
  );
}
