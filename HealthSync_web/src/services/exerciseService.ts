import api from './api';

export interface Exercise {
  exerciseId: number;
  name: string;
  muscleGroup: string;
  difficulty: string;
  equipment?: string;
  description?: string;
  imageUrl?: string;
}

export interface CreateExerciseDto {
  name: string;
  muscleGroup: string;
  difficulty: string;
  equipment?: string;
  description?: string;
}

export interface UpdateExerciseDto {
  exerciseId: number;
  name: string;
  muscleGroup: string;
  difficulty: string;
  equipment?: string;
  description?: string;
}

export const exerciseService = {
  // Lấy danh sách bài tập với filter
  async getExercises(params?: {
    muscleGroup?: string;
    difficulty?: string;
    search?: string;
  }): Promise<Exercise[]> {
    const response = await api.get('/Exercises', { params });
    return response.data;
  },

  // Lấy chi tiết 1 bài tập
  async getExerciseById(id: number): Promise<Exercise> {
    const response = await api.get(`/Exercises/${id}`);
    return response.data;
  },

  // Tạo bài tập mới
  async createExercise(data: CreateExerciseDto): Promise<{ exerciseId: number }> {
    const response = await api.post('/Exercises', data);
    return response.data;
  },

  // Cập nhật bài tập
  async updateExercise(id: number, data: UpdateExerciseDto): Promise<void> {
    await api.put(`/Exercises/${id}`, data);
  },

  // Xóa bài tập
  async deleteExercise(id: number): Promise<void> {
    await api.delete(`/Exercises/${id}`);
  },

  // Upload ảnh bài tập
  async uploadExerciseImage(id: number, file: File): Promise<{ imageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.put(`/Exercises/${id}/image`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
};
