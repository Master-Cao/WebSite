---
name: frontend-design
description: >-
  Creates distinctive, production-grade frontend interfaces and avoids generic
  AI aesthetics. Use when building or restyling web pages, React components,
  CSS, Avalonia XAML, layouts, or when the user mentions UI, UX, 界面, 样式, 设计.
paths:
  - YJCabin.Client/**
---

# Frontend Design

把每次界面改动当成有明确客户的设计委托，而不是套一套通用组件。先想清楚「这是谁的站、给谁看、这一屏要完成什么」，再动代码。

## 先出设计计划，再写代码

用 8–12 行写清：

- **Color**：4–6 个具名色值
- **Type**：最多两个字体族及用途
- **Layout**：一句话布局概念 + 对齐方式
- **Accent**：只允许一处成为记忆点

对照下面的「套模板清单」自检：若计划长得像任意产品都能用的默认稿，先改计划再实现。

## 禁止当作默认的套路

除非用户点名要，否则不要用：

- 奶油底 + 衬线大标题 + 陶土橙强调
- 近黑底 + 荧光绿 / 朱红单强调
- 所有内容切成同一圆角卡片 + 同一浅灰阴影
- 每个标题上方 ALL-CAPS eyebrow、中点分隔元数据、链接尾随 `→`
- 每个区块 fade-slide 入场、每张卡片都有 hover 动画
- 标题里只把某一个词做成斜体/异色

## 质量底线

- 移动端可用，焦点可见，尊重 `prefers-reduced-motion`
- 正文行宽控制在约 80 字以内
- 文案用界面语言：按钮说清动作（「保存」不是「提交」），空态告诉下一步
- 装饰不够就删，不要再加一层
- 3D / 场景动画走 [threejs](../threejs/SKILL.md)，且全站最多一处成为记忆点
- 公开站、落地页、作品集级页面走 [taste-skill](../taste-skill/SKILL.md)，先做 Design Read 再写代码
