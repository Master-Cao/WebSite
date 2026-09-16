import { useState } from "react";
import { Link, NavLink, Outlet } from "react-router-dom";
import { List, X } from "@phosphor-icons/react";
import { Critters } from "../components/Animals";
import { BrandLogo } from "../components/BrandLogo";
import { ProfileSearchRail, TimelineRail } from "../components/SiteRails";

const links = [
  { to: "/articles", label: "文章" },
  { to: "/archive", label: "归档" },
  { to: "/works", label: "作品" },
  { to: "/about", label: "简介" }
];

export function PublicLayout() {
  const [open, setOpen] = useState(false);

  return (
    <div className="site">
      <Critters />
      <a className="skip-link" href="#content">
        跳到内容
      </a>
      <header className="site-header glass">
        <div className="site-header-inner">
          <Link to="/" className="brand" aria-label="YJCabin 首页">
            <BrandLogo />
          </Link>
          <nav className="nav">
            {links.map((link) => (
              <NavLink key={link.to} to={link.to} className={({ isActive }) => (isActive ? "active" : undefined)}>
                {link.label}
              </NavLink>
            ))}
          </nav>
          <button
            type="button"
            className="menu-btn"
            onClick={() => setOpen((value) => !value)}
            aria-expanded={open}
            aria-label={open ? "关闭菜单" : "打开菜单"}
          >
            {open ? <X size={22} /> : <List size={22} />}
          </button>
        </div>
        {open && (
          <nav className="mobile-nav">
            {links.map((link) => (
              <NavLink key={link.to} to={link.to} onClick={() => setOpen(false)} className="nav-link">
                {link.label}
              </NavLink>
            ))}
          </nav>
        )}
      </header>

      <aside className="site-rail site-rail-left glass">
        <ProfileSearchRail />
      </aside>

      <main id="content" className="site-main">
        <Outlet />
      </main>

      <aside className="site-rail site-rail-right glass">
        <TimelineRail />
      </aside>

      <footer className="site-footer">
        <div className="wrap">
          <div className="site-footer-inner glass">
            <div className="footer-brand">
              <BrandLogo compact />
            </div>
            <nav className="footer-links">
              <Link to="/archive" className="accent">
                归档
              </Link>
              <Link to="/contact" className="accent">
                联系
              </Link>
              <span>2026</span>
            </nav>
          </div>
        </div>
      </footer>
    </div>
  );
}
