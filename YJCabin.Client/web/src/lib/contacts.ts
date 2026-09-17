import type { SocialLink } from "../api/types";

export type ContactKind = "qq" | "email" | "wechat" | "github";

export type ContactCard = {
  kind: ContactKind;
  title: string;
  hint: string;
  value: string;
  href?: string;
};

function matchLink(links: SocialLink[] | undefined, keys: string[]) {
  return links?.find((item) => {
    const hay = `${item.name} ${item.url}`.toLowerCase();
    return keys.some((key) => hay.includes(key));
  });
}

function qqValue(raw?: string) {
  if (!raw) return "暂未填写";
  if (/^\d+$/.test(raw.trim())) return raw.trim();
  const match = raw.match(/uin=(\d+)/i);
  return match?.[1] ?? raw;
}

function mailValue(raw?: string) {
  if (!raw) return "暂未填写";
  return raw.replace(/^mailto:/i, "");
}

function githubValue(raw?: string) {
  if (!raw) return "github.com";
  return raw.replace(/^https?:\/\//i, "").replace(/^www\./i, "");
}

function wechatValue(raw?: string) {
  if (!raw) return "YJCabin";
  return raw.replace(/^weixin:/i, "");
}

function qqHref(raw?: string) {
  if (!raw) return undefined;
  if (/^(https?:|tencent:)/i.test(raw)) return raw;
  if (/^\d+$/.test(raw.trim())) return `https://wpa.qq.com/msgrd?v=3&uin=${raw.trim()}&site=qq&menu=yes`;
  return undefined;
}

function mailHref(raw?: string) {
  if (!raw) return undefined;
  if (/^mailto:/i.test(raw) || /^https?:/i.test(raw)) return raw;
  if (raw.includes("@")) return `mailto:${raw}`;
  return undefined;
}

function githubHref(raw?: string) {
  if (!raw) return "https://github.com";
  if (/^https?:/i.test(raw)) return raw;
  return `https://github.com/${raw.replace(/^@/, "")}`;
}

export function resolveContact(kind: ContactKind, links?: SocialLink[]): ContactCard {
  if (kind === "qq") {
    const raw = matchLink(links, ["qq", "腾讯"])?.url;
    return { kind, title: "QQ", hint: "号码", value: qqValue(raw), href: qqHref(raw) };
  }
  if (kind === "email") {
    const raw = matchLink(links, ["mail", "邮箱", "email"])?.url;
    return { kind, title: "邮箱", hint: "地址", value: mailValue(raw), href: mailHref(raw) };
  }
  if (kind === "wechat") {
    const raw = matchLink(links, ["微信", "wechat", "weixin"])?.url;
    const href = raw && /^(https?:|weixin:)/i.test(raw) ? raw : undefined;
    return { kind, title: "微信", hint: "微信号", value: wechatValue(raw), href };
  }
  const raw = matchLink(links, ["github", "git"])?.url;
  return { kind, title: "GitHub", hint: "主页", value: githubValue(raw), href: githubHref(raw) };
}
