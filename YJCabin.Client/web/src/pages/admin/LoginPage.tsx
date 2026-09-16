import { FormEvent, useState } from "react";
import { Navigate, useNavigate } from "react-router-dom";
import { BrandLogo } from "../../components/BrandLogo";
import { useAuth } from "../../auth/AuthContext";
import { btnPrimary, controlClass, fieldClass } from "../../ui";

export function LoginPage() {
  const { isAuthenticated, login } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string>();
  if (isAuthenticated) {
    return <Navigate to="/admin" replace />;
  }

  const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      await login(String(form.get("userName")), String(form.get("password")));
      navigate("/admin");
    } catch (err) {
      setError(err instanceof Error ? err.message : "登录失败");
    }
  };

  return (
    <div className="login-shell">
      <form className="login-card glass" onSubmit={onSubmit} style={{ display: "grid", gap: 16 }}>
        <p className="brand">
          <BrandLogo />
        </p>
        <h1 className="page-title">登录</h1>
        <p className="muted">登录后管理作品、文章、关于我与留言。</p>
        <label className={fieldClass}>
          用户名
          <input name="userName" className={controlClass} defaultValue="admin" required autoComplete="username" />
        </label>
        <label className={fieldClass}>
          密码
          <input name="password" type="password" className={controlClass} required autoComplete="current-password" />
        </label>
        <button type="submit" className={btnPrimary}>
          进入后台
        </button>
        {error && <p className="danger">{error}</p>}
      </form>
    </div>
  );
}
