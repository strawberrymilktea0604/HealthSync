import axios from 'axios';
import { AdminStatistics } from '@/types/adminStatistics';

const API_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export const adminStatisticsService = {
  getStatistics: async (days?: number): Promise<AdminStatistics> => {
    const userJson = localStorage.getItem('user');
    const token = userJson ? JSON.parse(userJson).token : null;
    const response = await axios.get(`${API_URL}/admin/statistics`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: { days },
    });
    return response.data;
  },

  getUserStatistics: async (days?: number) => {
    const userJson = localStorage.getItem('user');
    const token = userJson ? JSON.parse(userJson).token : null;
    const response = await axios.get(`${API_URL}/admin/statistics/users`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: { days },
    });
    return response.data;
  },

  getWorkoutStatistics: async (days?: number) => {
    const userJson = localStorage.getItem('user');
    const token = userJson ? JSON.parse(userJson).token : null;
    const response = await axios.get(`${API_URL}/admin/statistics/workouts`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: { days },
    });
    return response.data;
  },

  getNutritionStatistics: async (days?: number) => {
    const userJson = localStorage.getItem('user');
    const token = userJson ? JSON.parse(userJson).token : null;
    const response = await axios.get(`${API_URL}/admin/statistics/nutrition`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: { days },
    });
    return response.data;
  },

  getGoalStatistics: async (days?: number) => {
    const userJson = localStorage.getItem('user');
    const token = userJson ? JSON.parse(userJson).token : null;
    const response = await axios.get(`${API_URL}/admin/statistics/goals`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: { days },
    });
    return response.data;
  },
};
