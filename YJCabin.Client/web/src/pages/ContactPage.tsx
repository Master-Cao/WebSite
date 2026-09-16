import { FormEvent, useState } from "react";
import { api } from "../api/client";
import { btnPrimary, controlClass, fieldClass } from "../ui";

export function ContactPage() {
  const [status, setStatus] = useState<string>();
  const [error, setError] = useState<string>();
  const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      await api("/api/contact", {
        method: "POST",
        body: JSON.stringify({
          name: form.get("name"),
          email: form.get("email"),
          subject: form.get("subject"),
          body: form.get("body"),
          website: form.get("website")
        })
      });
      setError(undefined);
      setStatus("已收到，我会尽快回复。");
      event.currentTarget.reset();
    } catch (err) {
      setStatus(undefined);
      setError(err instanceof Error ? err.message : "发送失败");
    }
  };

  return (
    <div className="wrap contact-grid">
      <div className="glass panel">
        <h1 className="page-title" style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <img src="/yjcabin-bird.png" alt="" width={36} height={36} style={{ objectFit: "contain" }} />
          联系
        </h1>
        <p className="muted">有合作、问题或想看某段实现，直接写给我。</p>
      </div>
      <form className="glass panel" onSubmit={onSubmit} style={{ display: "grid", gap: 16 }}>
        <label className={fieldClass}>
          姓名
          <input name="name" className={controlClass} required />
        </label>
        <label className={fieldClass}>
          邮箱
          <input name="email" type="email" className={controlClass} required />
        </label>
        <label className={fieldClass}>
          主题
          <input name="subject" className={controlClass} required />
        </label>
        <label className={fieldClass}>
          内容
          <textarea name="body" rows={6} className={controlClass} required />
        </label>
        <label className="sr-only" aria-hidden="true">
          网站
          <input name="website" tabIndex={-1} autoComplete="off" />
        </label>
        <button type="submit" className={btnPrimary} style={{ width: "fit-content" }}>
          发送
        </button>
        {status && <p className="accent">{status}</p>}
        {error && <p className="danger">{error}</p>}
      </form>
    </div>
  );
}
