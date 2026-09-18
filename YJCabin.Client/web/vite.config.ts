import { defineConfig, type Plugin } from "vite";
import react from "@vitejs/plugin-react";
import { rmSync } from "node:fs";
import path from "node:path";

function omitUnusedPublic(): Plugin {
  const unused = [
    "yjcabin-logo.png",
    "yjcabin-logo-mark.png",
    "yjcabin-logo-horizontal.png",
    "yjcabin-puppy.png",
    "yjcabin-cat.png",
    "yjcabin-bird.png",
    "yjcabin-paw.png"
  ];
  return {
    name: "omit-unused-public",
    apply: "build",
    writeBundle(options) {
      const dir = options.dir;
      if (!dir) return;
      for (const file of unused) {
        rmSync(path.join(dir, file), { force: true });
      }
    }
  };
}

export default defineConfig({
  plugins: [react(), omitUnusedPublic()],
  css: {
    postcss: "./postcss.config.mjs"
  },
  build: {
    target: "es2020",
    sourcemap: false,
    cssCodeSplit: true,
    assetsInlineLimit: 4096,
    chunkSizeWarningLimit: 700,
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (!id.includes("node_modules")) return;
          if (id.includes("three") || id.includes("@react-three")) return "three";
          if (id.includes("highlight.js")) return "hljs";
          if (id.includes("@tanstack")) return "query";
          if (id.includes("react-router")) return "router";
        }
      }
    }
  },
  server: {
    port: 5173,
    proxy: {
      "/api": "http://localhost:5178",
      "/uploads": "http://localhost:5178"
    }
  }
});
