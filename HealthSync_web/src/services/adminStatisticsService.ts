import axios from 'axios';
import { AdminStatistics } from '@/types/adminStatistics';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export const adminStatisticsService = {
  getStatistics: async (days?: number): Promise<AdminStatistics> => {
    const token = localStorage.getItem('token');
    const response = await axios.get(`${API_URL}/api/admin/statistics`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: { days },
    });
    return response.data;
  },

  getUserStatistics: async (days?: number) => {
    const token = localStorage.getItem('token');
    const response = await axios.get(`${API_URL}/api/admin/statistics/users`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: { days },
    });
    return response.data;
  },

  getWorkoutStatistics: async (days?: number) => {
    const token = localStorage.getItem('token');
    const response = await axios.get(`${API_URL}/api/admin/statistics/workouts`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: { days },
    });
    return response.data;
  },

  getNutritionStatistics: async (days?: number) => {
    const token = localStorage.getItem('token');
    const response = await axios.get(`${API_URL}/api/admin/statistics/nutrition`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: { days },
    });
    return response.data;
  },

  getGoalStatistics: async (days?: number) => {
    const token = localStorage.getItem('token');
    const response = await axios.get(`${API_URL}/api/admin/statistics/goals`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: { days },
    });
    return response.data;
  },
};
