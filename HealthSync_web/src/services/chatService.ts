import axios from 'axios';
import { ChatMessage, ChatResponse, ChatRequest } from '../types/chat';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';

const chatApi = axios.create({
  baseURL: `${API_BASE_URL}/Chat`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
chatApi.interceptors.request.use(
  (config) => {
    // Get token from user object in localStorage (same as api.ts)
    const userJson = localStorage.getItem('user');
    if (userJson) {
      try {
        const user = JSON.parse(userJson);
        const token = user.token;
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        } else {
          console.warn('No token found in user object for chat request');
        }
      } catch (e) {
        console.error('Failed to parse user from localStorage', e);
      }
    } else {
      console.warn('No user found in localStorage for chat request');
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle token expiration
chatApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      console.warn('Chat API unauthorized - user needs to log in');
    }
    return Promise.reject(error);
  }
);

export const chatService = {
  sendMessage: async (question: string): Promise<ChatResponse> => {
    try {
      const request: ChatRequest = { question };
      const response = await chatApi.post<ChatResponse>('/ask', request);
      return response.data;
    } catch (error: any) {
      console.error('Chat API Error:', error.response?.data || error.message);
      throw error;
    }
  },

  getChatHistory: async (pageSize: number = 20, pageNumber: number = 1): Promise<ChatMessage[]> => {
    try {
      const response = await chatApi.get<ChatMessage[]>('/history', {
        params: { pageSize, pageNumber },
      });
      return response.data;
    } catch (error: any) {
      console.error('Chat History API Error:', error.response?.data || error.message);
      
      // Return empty array if unauthorized or other error
      if (error.response?.status === 401) {
        console.warn('Unauthorized access to chat history. User may need to log in again.');
      }
      
      throw error;
    }
  },

  clearChatHistory: async (): Promise<void> => {
    try {
      await chatApi.delete('/history');
    } catch (error: any) {
      console.error('Clear Chat History Error:', error.response?.data || error.message);
      throw error;
    }
  },
};
