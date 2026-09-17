import { MouseEvent, useLayoutEffect, useRef } from "react";
import { highlightCode, languageLabel } from "../lib/highlight";

type MarkdownHtmlProps = {
  html: string;
  className?: string;
};

function enhanceBlocks(root: HTMLElement) {
  for (const code of root.querySelectorAll("pre > code")) {
    const pre = code.parentElement;
    if (!(code instanceof HTMLElement) || !pre || pre.closest(".md-code")) continue;

    const detected = highlightCode(code);
    const wrap = document.createElement("div");
    wrap.className = "md-code";

    const bar = document.createElement("div");
    bar.className = "md-code-bar";

    const lang = document.createElement("span");
    lang.className = "md-code-lang";
    lang.textContent = languageLabel(detected);

    const copy = document.createElement("button");
    copy.type = "button";
    copy.className = "md-code-copy";
    copy.dataset.copy = "true";
    copy.setAttribute("aria-label", "复制代码");
    copy.textContent = "复制";

    bar.append(lang, copy);
    pre.replaceWith(wrap);
    wrap.append(bar, pre);
  }
}

async function copyText(text: string) {
  try {
    await navigator.clipboard.writeText(text);
    return true;
  } catch {
    try {
      const area = document.createElement("textarea");
      area.value = text;
      area.setAttribute("readonly", "");
      area.style.position = "fixed";
      area.style.left = "-9999px";
      document.body.append(area);
      area.select();
      const ok = document.execCommand("copy");
      area.remove();
      return ok;
    } catch {
      return false;
    }
  }
}

export function MarkdownHtml({ html, className }: MarkdownHtmlProps) {
  const ref = useRef<HTMLDivElement>(null);

  useLayoutEffect(() => {
    if (ref.current) enhanceBlocks(ref.current);
  }, [html]);

  const onClick = async (event: MouseEvent<HTMLDivElement>) => {
    const button = (event.target as HTMLElement).closest<HTMLButtonElement>("[data-copy]");
    if (!button) return;
    const block = button.closest(".md-code");
    const text = block?.querySelector("code")?.textContent ?? "";
    if (!text) return;
    const ok = await copyText(text);
    const previous = button.textContent;
    button.textContent = ok ? "已复制" : "复制失败";
    window.setTimeout(() => {
      if (button.isConnected) button.textContent = previous || "复制";
    }, 1600);
  };

  return (
    <div
      ref={ref}
      className={className ? `markdown ${className}` : "markdown"}
      dangerouslySetInnerHTML={{ __html: html }}
      onClick={onClick}
    />
  );
}
