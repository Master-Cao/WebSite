import hljs from "highlight.js/lib/common";
import dockerfile from "highlight.js/lib/languages/dockerfile";
import powershell from "highlight.js/lib/languages/powershell";

hljs.registerLanguage("dockerfile", dockerfile);
hljs.registerLanguage("powershell", powershell);

const LABELS: Record<string, string> = {
  bash: "Bash",
  c: "C",
  cpp: "C++",
  csharp: "C#",
  cs: "C#",
  css: "CSS",
  dockerfile: "Dockerfile",
  go: "Go",
  html: "HTML",
  java: "Java",
  javascript: "JavaScript",
  js: "JavaScript",
  json: "JSON",
  jsx: "JSX",
  kotlin: "Kotlin",
  markdown: "Markdown",
  md: "Markdown",
  php: "PHP",
  plaintext: "Text",
  powershell: "PowerShell",
  ps1: "PowerShell",
  python: "Python",
  py: "Python",
  rust: "Rust",
  scss: "SCSS",
  sh: "Shell",
  shell: "Shell",
  sql: "SQL",
  text: "Text",
  ts: "TypeScript",
  tsx: "TSX",
  typescript: "TypeScript",
  xml: "XML",
  yaml: "YAML",
  yml: "YAML"
};

const LANG_RE = /(?:^|\s)(?:language-|lang-)([a-z0-9+#._-]+)/i;

export function languageFromClass(className: string) {
  const match = className.match(LANG_RE);
  return match?.[1]?.toLowerCase() ?? "";
}

export function languageLabel(lang: string) {
  if (!lang) return "Code";
  return LABELS[lang] ?? lang.replace(/[_-]+/g, " ");
}

export function specifiedLanguage(code: HTMLElement) {
  return languageFromClass(code.className) || languageFromClass(code.parentElement?.className ?? "");
}

export function highlightCode(code: HTMLElement) {
  const text = code.textContent ?? "";
  const specified = specifiedLanguage(code);
  const lang = specified && hljs.getLanguage(specified) ? specified : "";
  if (lang) {
    const result = hljs.highlight(text, { language: lang, ignoreIllegals: true });
    code.innerHTML = result.value;
    code.className = `hljs language-${lang}`;
    return lang;
  }
  code.className = "hljs";
  return "";
}
