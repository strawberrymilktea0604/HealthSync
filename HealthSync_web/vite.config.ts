import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173, // Cổng frontend của bạn
    proxy: {
      // Bất kỳ request nào bắt đầu bằng /api sẽ được chuyển hướng tới backend
      '/api': {
        target: 'http://localhost:5274', // Địa chỉ backend Kestrel
        changeOrigin: true, // Quan trọng: thay đổi header Origin để Kestrel nghĩ request đến từ 5274
        secure: false, // Dùng cho HTTP
        // rewrite: (path) => path.replace(/^\/api/, ''), // Có thể không cần nếu API của bạn bắt đầu bằng /api
      },
    },
  },
})