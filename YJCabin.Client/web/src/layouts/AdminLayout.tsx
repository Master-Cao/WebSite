import { Navigate, NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function AdminLayout() {
  const { isAuthenticated, logout, userName } = useAuth();
  if (!isAuthenticated) {
    return <Navigate to="/admin/login" replace />;
  }

  return (
    <div className="admin-shell">
      <aside className="sidenav">
        <strong>后台</strong>
        <NavLink to="/admin">概览</NavLink>
        <NavLink to="/admin/projects">作品</NavLink>
        <NavLink to="/admin/articles">文章</NavLink>
        <NavLink to="/admin/about">关于我</NavLink>
        <NavLink to="/admin/messages">留言</NavLink>
        <button className="linkish" onClick={logout}>
          退出 {userName}
        </button>
      </aside>
      <section className="admin-main">
        <Outlet />
      </section>
    </div>
  );
}
