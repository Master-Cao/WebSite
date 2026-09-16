import { FormEvent, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { api } from "../../api/client";
import type { AboutDto } from "../../api/types";

export function AboutAdminPage() {
  const query = useQuery({
    queryKey: ["admin", "about"],
    queryFn: () => api<AboutDto>("/api/admin/about", {}, true)
  });
  const [status, setStatus] = useState<string>();

  const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const payload = {
      headline: String(form.get("headline")),
      bioMarkdown: String(form.get("bioMarkdown")),
      skills: String(form.get("skills") || "")
        .split(",")
        .map((x) => x.trim())
        .filter(Boolean),
      socialLinks: String(form.get("social") || "")
        .split("\n")
        .map((line) => line.trim())
        .filter(Boolean)
        .map((line) => {
          const [name, url] = line.split("|").map((x) => x.trim());
          return { name, url };
        })
    };
    try {
      await api("/api/admin/about", { method: "PUT", body: JSON.stringify(payload) }, true);
      setStatus("已保存");
    } catch (err) {
      setStatus(err instanceof Error ? err.message : "保存失败");
    }
  };

  const data = query.data;
  return (
    <form className="form" onSubmit={onSubmit}>
      <h1>关于我</h1>
      <label>
        标题
        <input name="headline" defaultValue={data?.headline} required />
      </label>
      <label>
        简介 Markdown
        <textarea name="bioMarkdown" rows={8} defaultValue={data?.bioMarkdown} required />
      </label>
      <label>
        技能（逗号分隔）
        <input name="skills" defaultValue={data?.skills.join(", ")} />
      </label>
      <label>
        社交链接（每行 name|url）
        <textarea
          name="social"
          rows={4}
          defaultValue={data?.socialLinks.map((x) => `${x.name}|${x.url}`).join("\n")}
        />
      </label>
      <button type="submit">保存</button>
      {status && <p>{status}</p>}
    </form>
  );
}
