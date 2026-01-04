import axios from 'axios';
import { ChatMessage, ChatResponse, ChatRequest } from '../types/chat';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';

const chatApi = axios.create({
  baseURL: `${API_BASE_URL}/api/Chat`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
chatApi.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const chatService = {
  sendMessage: async (question: string): Promise<ChatResponse> => {
    const request: ChatRequest = { question };
    const response = await chatApi.post<ChatResponse>('/ask', request);
    return response.data;
  },

  getChatHistory: async (pageSize: number = 20, pageNumber: number = 1): Promise<ChatMessage[]> => {
    const response = await chatApi.get<ChatMessage[]>('/history', {
      params: { pageSize, pageNumber },
    });
    return response.data;
  },
};
