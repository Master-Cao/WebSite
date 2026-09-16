import { FormEvent, useState } from "react";
import { api } from "../api/client";

export function ContactPage() {
  const [status, setStatus] = useState<string>();
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
      setStatus("已收到，我会尽快回复。");
      event.currentTarget.reset();
    } catch (error) {
      setStatus(error instanceof Error ? error.message : "提交失败");
    }
  };

  return (
    <div className="stack">
      <h1>联系</h1>
      <form className="form" onSubmit={onSubmit}>
        <label>
          姓名
          <input name="name" required />
        </label>
        <label>
          邮箱
          <input name="email" type="email" required />
        </label>
        <label>
          主题
          <input name="subject" required />
        </label>
        <label>
          内容
          <textarea name="body" rows={6} required />
        </label>
        <label className="hp" aria-hidden="true">
          网站
          <input name="website" tabIndex={-1} autoComplete="off" />
        </label>
        <button type="submit">发送</button>
      </form>
      {status && <p>{status}</p>}
    </div>
  );
}
