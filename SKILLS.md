# Cursor Skills

本仓库用 **项目级 skill** 规范界面。它们放在 `.cursor/skills/`，会随 Git 一起走。另一台电脑 `git clone` / `git pull` 后，用 Cursor 打开本仓库即可，一般不用再下载。

Cursor 自带的 skill（`~/.cursor/skills-cursor/`，例如 create-skill、create-rule）由 Cursor 安装，不要拷进这个仓库。

## 仓库里有什么

| 路径 | 用途 |
|---|---|
| `.cursor/skills/frontend-design/SKILL.md` | 改界面时先定色彩/字体/布局，避开常见 AI 套模板 |
| `.cursor/skills/taste-skill/SKILL.md` | 公开站、作品集、专业前端改版 |
| `.cursor/skills/threejs/SKILL.md` | Web 端 Three.js / R3F（需要时再装 npm 依赖） |
| `.cursor/rules/frontend-design.mdc` | 编辑 `YJCabin.Client` 时提醒读取上面几条 |

打开项目后，在 Cursor 设置里确认 Project Skills 已启用。改 `YJCabin.Client` 的页面或样式时，Agent 应自动读这些文件。

## 另一台电脑丢了文件时怎么补

在仓库根目录执行（PowerShell）：

```powershell
New-Item -ItemType Directory -Force -Path .cursor/skills/taste-skill | Out-Null
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/Leonxlnx/taste-skill/main/skills/taste-skill/SKILL.md" -OutFile .cursor/skills/taste-skill/SKILL.md
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/Leonxlnx/taste-skill/main/LICENSE" -OutFile .cursor/skills/taste-skill/LICENSE
```

`frontend-design` 和 `threejs` 已按本仓库改过（含 YJCabin 路径和中文约定），不要用网上原版覆盖。以 Git 里的版本为准。

来源备份：

- [taste-skill](https://github.com/Leonxlnx/taste-skill)（MIT）
- [Anthropic frontend-design](https://github.com/anthropics/skills)（本仓库为改写版）
