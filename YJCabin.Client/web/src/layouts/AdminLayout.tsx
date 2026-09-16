import { Navigate, NavLink, Outlet } from "react-router-dom";
import { SignOut } from "@phosphor-icons/react";
import { BrandLogo } from "../components/BrandLogo";
import { useAuth } from "../auth/AuthContext";

const links = [
  { to: "/admin", label: "概览", end: true },
  { to: "/admin/projects", label: "作品" },
  { to: "/admin/articles", label: "文章" },
  { to: "/admin/about", label: "关于我" },
  { to: "/admin/messages", label: "留言" }
];

export function AdminLayout() {
  const { isAuthenticated, logout, userName } = useAuth();
  if (!isAuthenticated) {
    return <Navigate to="/admin/login" replace />;
  }

  return (
    <div className="admin-shell">
      <aside className="admin-side glass">
        <div className="admin-brand">
          <BrandLogo compact />
        </div>
        <nav className="admin-nav">
          {links.map((link) => (
            <NavLink key={link.to} to={link.to} end={link.end} className={({ isActive }) => (isActive ? "active" : undefined)}>
              {link.label}
            </NavLink>
          ))}
        </nav>
        <div className="admin-foot">
          <p className="muted">{userName}</p>
          <button type="button" className="btn-text" onClick={logout}>
            <SignOut size={16} /> 退出
          </button>
        </div>
      </aside>
      <section className="admin-main">
        <Outlet />
      </section>
    </div>
  );
}
