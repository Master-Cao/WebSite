import { Link, NavLink, Outlet } from "react-router-dom";

export function PublicLayout() {
  return (
    <div className="shell">
      <header className="topbar">
        <Link to="/" className="brand">
          YJCabin
        </Link>
        <nav>
          <NavLink to="/works">作品</NavLink>
          <NavLink to="/articles">文章</NavLink>
          <NavLink to="/about">关于</NavLink>
          <NavLink to="/contact">联系</NavLink>
        </nav>
      </header>
      <main className="content">
        <Outlet />
      </main>
      <footer className="footer">小屋里的作品与文字 · YJCabin</footer>
    </div>
  );
}
