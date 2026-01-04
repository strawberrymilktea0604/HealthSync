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

export interface FoodEntry {
  foodEntryId: number;
  foodItemId: number;
  foodItemName: string;
  quantity: number;
  mealType: string;
  caloriesKcal: number;
  proteinG: number;
  carbsG: number;
  fatG: number;
  imageUrl?: string;
}

export interface NutritionLog {
  nutritionLogId: number;
  logDate: string;
  totalCalories: number;
  proteinG: number;
  carbsG: number;
  fatG: number;
  foodEntries: FoodEntry[];
}

export interface AddFoodEntryRequest {
  foodItemId: number;
  quantity: number;
  mealType: string;
}

const nutritionService = {
  // Get food items with optional search
  getFoodItems: async (search?: string): Promise<FoodItem[]> => {
    const params = search ? { search } : {};
    const response = await api.get('/nutrition/food-items', { params });
    return response.data;
  },

  // Get nutrition log for a specific date
  getNutritionLogByDate: async (date: Date): Promise<NutritionLog | null> => {
    const response = await api.get('/nutrition/nutrition-log', {
      params: { date: date.toISOString() }
    });
    return response.data;
  },

  // Add food entry to today's log
  addFoodEntry: async (data: AddFoodEntryRequest): Promise<{ foodEntryId: number }> => {
    const response = await api.post('/nutrition/food-entry', data);
    return response.data;
  },

  // Delete a food entry
  deleteFoodEntry: async (foodEntryId: number): Promise<void> => {
    await api.delete(`/nutrition/food-entry/${foodEntryId}`);
  },

  // Get nutrition logs in a date range
  getNutritionLogs: async (startDate?: Date, endDate?: Date): Promise<NutritionLog[]> => {
    const params: Record<string, string> = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    const response = await api.get('/nutrition/nutrition-logs', { params });
    return response.data;
  },
};

export default nutritionService;
