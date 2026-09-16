import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  css: {
    postcss: "./postcss.config.mjs"
  },
  server: {
    port: 5173,
    proxy: {
      "/api": "http://localhost:5178",
      "/uploads": "http://localhost:5178"
    }
  }
});
