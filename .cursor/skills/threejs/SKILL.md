---
name: threejs
description: >-
  Builds Three.js animations in the React web client with React Three Fiber.
  Use when adding 3D, WebGL, Canvas scenes, shaders, GLTF, hero motion, or
  when the user mentions Three.js, R3F, drei, useFrame, 动画, 三维.
paths:
  - YJCabin.Client/web/**
---

# Three.js（YJCabin Web）

只在 `YJCabin.Client/web` 用 Three.js。公开站是 React + Vite，默认 **React Three Fiber + drei**，不要在页面里手写 `new THREE.WebGLRenderer()`。Avalonia 桌面端不做 WebGL。

界面审美仍以 [frontend-design](../frontend-design/SKILL.md) 为准：3D 是记忆点，不是每页背景。

## 先判断要不要上 3D

用 CSS / SVG 能完成的（下划线、页内滚动、按钮反馈）不要上 Three.js。

适合 Three.js 的：首页一处氛围场景、作品详情的模型展示、一篇文章的交互示意。

同一时刻全站只跑 **一个** `Canvas`。文章长页不要边读边渲染 3D。

## 安装（需要时再加）

```bash
npm install three @react-three/fiber @react-three/drei
```

后处理、物理（`@react-three/postprocessing`、`@react-three/rapier`）默认不加。

## 场景骨架

```tsx
import { Canvas, useFrame } from "@react-three/fiber";
import { useRef } from "react";
import type { Mesh } from "three";

function Scene() {
  const mesh = useRef<Mesh>(null);
  useFrame((_, delta) => {
    if (mesh.current) mesh.current.rotation.y += delta * 0.25;
  });
  return (
    <mesh ref={mesh}>
      <icosahedronGeometry args={[1, 1]} />
      <meshStandardMaterial color="#2C4A3E" roughness={0.45} metalness={0.1} />
    </mesh>
  );
}

export function HeroCanvas() {
  return (
    <div className="hero-canvas" aria-hidden="true">
      <Canvas camera={{ position: [0, 0, 4], fov: 45 }} dpr={[1, 2]} gl={{ antialias: true, alpha: true }}>
        <ambientLight intensity={0.6} />
        <directionalLight position={[4, 6, 3]} intensity={1.1} />
        <Scene />
      </Canvas>
    </div>
  );
}
```

- 容器自己设宽高；`Canvas` 默认填满父级
- `useFrame` 里只改 ref，**禁止** `setState`
- 模型用 `useGLTF` + `Suspense`；同一 URL 要多份时 `clone()`
- 交互用 R3F 指针事件（`onClick` / `onPointerOver`），不要自己 raycast

## 动画与无障碍

- 尊重 `prefers-reduced-motion`：为真则不挂 `Canvas`，改静态图或纯 CSS
- `useFrame` 用 `delta`，不要假定 60fps
- 离开路由必须停循环：卸载 `Canvas` 即可；自定义 renderer 要 `dispose()`
- 文档流里的说明文字不要放进 `Html` 3D 标注，以免读屏丢失

## 性能

- 几何与材质 `useMemo`；卸载时 `geometry.dispose()` / `material.dispose()`
- 重复物体用 `Instances`，不要循环出几百个 `<mesh>`
- 移动端 `dpr={[1, 1.5]}`，阴影默认关
- 开发可加 `<Stats />`，提交前去掉

## 不要

- 每张卡片、每个 section 都放 Canvas
- Bloom / 色差 / 粒子作为默认滤镜
- 在后台管理页做 3D
- 把 Three.js 写进 Avalonia
