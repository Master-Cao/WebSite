import { useEffect, useState } from "react";
import { Link, NavLink, Outlet } from "react-router-dom";
import { List, X } from "@phosphor-icons/react";
import { Critters, PetStage } from "../components/Animals";
import { ICP_HREF, ICP_NO } from "../components/BeianNotice";
import { BrandLogo } from "../components/BrandLogo";
import { LoadingOverlay } from "../components/LoadingOverlay";
import { ProfileSearchRail, MessageRail, TimelineRail } from "../components/SiteRails";
import { useSiteBoot } from "../lib/useSiteBoot";

const links = [
  { to: "/articles", label: "文章" },
  { to: "/archive", label: "归档" },
  { to: "/works", label: "作品" },
  { to: "/about", label: "简介" }
];

export function PublicLayout() {
  const [open, setOpen] = useState(false);
  const booting = useSiteBoot();

  useEffect(() => {
    document.documentElement.classList.toggle("site-booting", booting);
    return () => document.documentElement.classList.remove("site-booting");
  }, [booting]);

  return (
    <>
      <LoadingOverlay show={booting} message="正在加载站点…" />
      <div className="site" {...(booting ? { inert: "" } : {})}>
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
          <MessageRail />
          <PetStage />
          <TimelineRail />
        </aside>

        <footer className="site-footer">
          <div className="wrap">
            <div className="site-footer-inner glass">
              <div className="footer-brand">
                <BrandLogo compact />
              </div>
              <nav className="footer-links">
                <a className="accent" href={ICP_HREF} target="_blank" rel="noreferrer">
                  {ICP_NO}
                </a>
                <Link to="/archive" className="accent">
                  归档
                </Link>
                <span>2026</span>
              </nav>
            </div>
          </div>
        </footer>
      </div>
    </>
  );
}
