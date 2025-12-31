import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import path from 'node:path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    port: 5173, // Cổng frontend của bạn
    proxy: {
      // Bất kỳ request nào bắt đầu bằng /api sẽ được chuyển hướng tới backend
      '/api': {
        target: 'http://localhost:8080', // Địa chỉ nginx (proxy tới backend)
        changeOrigin: true, // Quan trọng: thay đổi header Origin
        secure: false, // Dùng cho HTTP
        // rewrite: (path) => path.replace(/^\/api/, ''), // Có thể không cần nếu API của bạn bắt đầu bằng /api
      },
    },
  },
})