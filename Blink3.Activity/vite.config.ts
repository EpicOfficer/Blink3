import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  base: "/",
  plugins: [react()],
  preview: {
    port: 8380,
    strictPort: true,
  },
  server: {
    port: 8380,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://localhost:8288',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
    hmr: {
      clientPort: 443,
    },
  },
});