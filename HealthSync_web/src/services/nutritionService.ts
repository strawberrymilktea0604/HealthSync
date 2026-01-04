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
  logDate?: string;
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
    // Convert to UTC date (without time) to avoid timezone issues
    const utcDate = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
    const response = await api.get('/nutrition/nutrition-log', {
      params: { date: utcDate.toISOString() }
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
    if (startDate) {
      // Convert to UTC date (without time) to avoid timezone issues
      const utcDate = new Date(Date.UTC(startDate.getFullYear(), startDate.getMonth(), startDate.getDate()));
      params.startDate = utcDate.toISOString();
    }
    if (endDate) {
      // Convert to UTC date (without time) to avoid timezone issues
      const utcDate = new Date(Date.UTC(endDate.getFullYear(), endDate.getMonth(), endDate.getDate()));
      params.endDate = utcDate.toISOString();
    }
    const response = await api.get('/nutrition/nutrition-logs', { params });
    return response.data;
  },
};

export default nutritionService;
