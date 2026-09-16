import { Navigate, Route, Routes } from "react-router-dom";
import { PublicLayout } from "./layouts/PublicLayout";
import { AdminLayout } from "./layouts/AdminLayout";
import { HomePage } from "./pages/HomePage";
import { WorksPage } from "./pages/WorksPage";
import { WorkDetailPage } from "./pages/WorkDetailPage";
import { ArticlesPage } from "./pages/ArticlesPage";
import { ArticleDetailPage } from "./pages/ArticleDetailPage";
import { AboutPage } from "./pages/AboutPage";
import { ContactPage } from "./pages/ContactPage";
import { LoginPage } from "./pages/admin/LoginPage";
import { DashboardPage } from "./pages/admin/DashboardPage";
import { ProjectsAdminPage } from "./pages/admin/ProjectsAdminPage";
import { ProjectEditPage } from "./pages/admin/ProjectEditPage";
import { ArticlesAdminPage } from "./pages/admin/ArticlesAdminPage";
import { ArticleEditPage } from "./pages/admin/ArticleEditPage";
import { AboutAdminPage } from "./pages/admin/AboutAdminPage";
import { MessagesAdminPage } from "./pages/admin/MessagesAdminPage";

export default function App() {
  return (
    <Routes>
      <Route element={<PublicLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/works" element={<WorksPage />} />
        <Route path="/works/:slug" element={<WorkDetailPage />} />
        <Route path="/articles" element={<ArticlesPage />} />
        <Route path="/articles/:slug" element={<ArticleDetailPage />} />
        <Route path="/about" element={<AboutPage />} />
        <Route path="/contact" element={<ContactPage />} />
      </Route>
      <Route path="/admin/login" element={<LoginPage />} />
      <Route path="/admin" element={<AdminLayout />}>
        <Route index element={<DashboardPage />} />
        <Route path="projects" element={<ProjectsAdminPage />} />
        <Route path="projects/:slug" element={<ProjectEditPage />} />
        <Route path="articles" element={<ArticlesAdminPage />} />
        <Route path="articles/:slug" element={<ArticleEditPage />} />
        <Route path="about" element={<AboutAdminPage />} />
        <Route path="messages" element={<MessagesAdminPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
