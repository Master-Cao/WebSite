import { FormEvent, useState } from "react";
import { Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";

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
    <div className="login-wrap">
      <form className="form" onSubmit={onSubmit}>
        <h1>站长登录</h1>
        <label>
          用户名
          <input name="userName" defaultValue="admin" required />
        </label>
        <label>
          密码
          <input name="password" type="password" required />
        </label>
        <button type="submit">进入后台</button>
        {error && <p className="error">{error}</p>}
      </form>
    </div>
  );
}
