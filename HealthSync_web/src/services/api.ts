import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5274/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  // Get token from user object in localStorage
  const userJson = localStorage.getItem('user');
  if (userJson) {
    try {
      const user = JSON.parse(userJson);
      const token = user.token;
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    } catch (e) {
      console.error('Failed to parse user from localStorage', e);
    }
  }
  return config;
});

export default api;
