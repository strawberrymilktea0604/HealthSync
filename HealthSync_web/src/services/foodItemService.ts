import api from './api';

export interface FoodItem {
  foodItemId: number;
  name: string;
  servingSize: number;
  servingUnit: string;
  caloriesKcal: number;
  proteinG: number;
  carbsG: number;
  fatG: number;
  imageUrl?: string;
}

export interface CreateFoodItemDto {
  name: string;
  servingSize: number;
  servingUnit: string;
  caloriesKcal: number;
  proteinG: number;
  carbsG: number;
  fatG: number;
}

export interface UpdateFoodItemDto {
  foodItemId: number;
  name: string;
  servingSize: number;
  servingUnit: string;
  caloriesKcal: number;
  proteinG: number;
  carbsG: number;
  fatG: number;
}

export const foodItemService = {
  // Lấy danh sách món ăn với filter
  async getFoodItems(params?: {
    search?: string;
  }): Promise<FoodItem[]> {
    const response = await api.get('/FoodItems', { params });
    return response.data;
  },

  // Lấy chi tiết 1 món ăn
  async getFoodItemById(id: number): Promise<FoodItem> {
    const response = await api.get(`/FoodItems/${id}`);
    return response.data;
  },

  // Tạo món ăn mới
  async createFoodItem(data: CreateFoodItemDto): Promise<{ foodItemId: number }> {
    const response = await api.post('/FoodItems', data);
    return response.data;
  },

  // Cập nhật món ăn
  async updateFoodItem(id: number, data: UpdateFoodItemDto): Promise<void> {
    await api.put(`/FoodItems/${id}`, data);
  },

  // Xóa món ăn
  async deleteFoodItem(id: number): Promise<void> {
    await api.delete(`/FoodItems/${id}`);
  },

  // Upload ảnh món ăn
  async uploadFoodItemImage(id: number, file: File): Promise<{ imageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.put(`/FoodItems/${id}/image`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
};
