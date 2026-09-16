---
title: Hello, YJCabin
summary: 站点第一篇文章：为什么用混合存储，以及 Web / 桌面如何共用 API。
tags:
  - Architecture
  - C#
draft: false
publishedAt: 2026-09-16T00:00:00Z
---

站点采用混合内容模型：

- 文章正文存在 Markdown 文件中，便于 Git 与本地编辑。
- 标题、摘要、标签等元数据写入 PostgreSQL，方便筛选与分页。
- React 展示站与 Avalonia 管理端都只调用同一套 REST API。
