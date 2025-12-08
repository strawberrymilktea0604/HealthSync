import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Plus, Search, Trash2, Calendar } from 'lucide-react';
import nutritionService, { FoodItem, NutritionLog } from '@/services/nutritionService';
import { toast } from '@/hooks/use-toast';

const NutritionPage: React.FC = () => {
  const [nutritionLog, setNutritionLog] = useState<NutritionLog | null>(null);
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [isAddFoodOpen, setIsAddFoodOpen] = useState(false);
  const [foodItems, setFoodItems] = useState<FoodItem[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedFoodItem, setSelectedFoodItem] = useState<FoodItem | null>(null);
  const [quantity, setQuantity] = useState<number>(1);
  const [mealType, setMealType] = useState<string>('Breakfast');
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    loadNutritionLog();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedDate]);

  const loadNutritionLog = async () => {
    try {
      const log = await nutritionService.getNutritionLogByDate(selectedDate);
      setNutritionLog(log);
    } catch (error) {
      console.error('Error loading nutrition log:', error);
    }
  };

  const loadFoodItems = async (search?: string) => {
    try {
      const items = await nutritionService.getFoodItems(search);
      setFoodItems(items);
    } catch (error) {
      console.error('Error loading food items:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể tải danh sách món ăn',
        variant: 'destructive',
      });
    }
  };

  useEffect(() => {
    if (isAddFoodOpen) {
      loadFoodItems();
    }
  }, [isAddFoodOpen]);

  const handleSearch = () => {
    loadFoodItems(searchQuery);
  };

  const handleAddFood = async () => {
    if (!selectedFoodItem || quantity <= 0) {
      toast({
        title: 'Lỗi',
        description: 'Vui lòng chọn món ăn và nhập số lượng hợp lệ',
        variant: 'destructive',
      });
      return;
    }

    setIsLoading(true);
    try {
      await nutritionService.addFoodEntry({
        foodItemId: selectedFoodItem.foodItemId,
        quantity,
        mealType,
      });

      toast({
        title: 'Thành công',
        description: 'Đã thêm món ăn vào nhật ký',
      });

      setIsAddFoodOpen(false);
      setSelectedFoodItem(null);
      setQuantity(1);
      loadNutritionLog();
    } catch (error) {
      console.error('Error adding food:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể thêm món ăn',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteFood = async (foodEntryId: number) => {
    try {
      await nutritionService.deleteFoodEntry(foodEntryId);
      toast({
        title: 'Thành công',
        description: 'Đã xóa món ăn',
      });
      loadNutritionLog();
    } catch (error) {
      console.error('Error deleting food:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể xóa món ăn',
        variant: 'destructive',
      });
    }
  };

  const getMealTypeLabel = (mealType: string) => {
    const labels: Record<string, string> = {
      Breakfast: 'Bữa sáng',
      Lunch: 'Bữa trưa',
      Dinner: 'Bữa tối',
      Snack: 'Ăn vặt',
    };
    return labels[mealType] || mealType;
  };

  const groupByMealType = (entries: FoodEntry[]) => {
    return entries.reduce((acc, entry) => {
      const mealType = entry.mealType;
      if (!acc[mealType]) {
        acc[mealType] = [];
      }
      acc[mealType].push(entry);
      return acc;
    }, {} as Record<string, FoodEntry[]>);
  };

  const calculateMealTotals = (entries: FoodEntry[]) => {
    return entries.reduce(
      (acc, entry) => ({
        calories: acc.calories + entry.caloriesKcal,
        protein: acc.protein + entry.proteinG,
        carbs: acc.carbs + entry.carbsG,
        fat: acc.fat + entry.fatG,
      }),
      { calories: 0, protein: 0, carbs: 0, fat: 0 }
    );
  };

  const mealTypeOrder = ['Breakfast', 'Lunch', 'Dinner', 'Snack'];
  const groupedEntries = nutritionLog?.foodEntries ? groupByMealType(nutritionLog.foodEntries) : {};

  return (
    <div className="container mx-auto p-6 max-w-7xl">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Nhật ký Dinh dưỡng</h1>
        <div className="flex items-center gap-4">
          <Input
            type="date"
            value={selectedDate.toISOString().split('T')[0]}
            onChange={(e) => setSelectedDate(new Date(e.target.value))}
            className="w-48"
          />
          <Dialog open={isAddFoodOpen} onOpenChange={setIsAddFoodOpen}>
            <DialogTrigger asChild>
              <Button>
                <Plus className="mr-2 h-4 w-4" /> Thêm bữa ăn
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
              <DialogHeader>
                <DialogTitle>Thêm món ăn</DialogTitle>
              </DialogHeader>
              <div className="space-y-4">
                <div className="flex gap-2">
                  <Input
                    placeholder="Tìm kiếm món ăn..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
                  />
                  <Button onClick={handleSearch} variant="outline">
                    <Search className="h-4 w-4" />
                  </Button>
                </div>

                <Select value={mealType} onValueChange={setMealType}>
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn bữa ăn" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Breakfast">Bữa sáng</SelectItem>
                    <SelectItem value="Lunch">Bữa trưa</SelectItem>
                    <SelectItem value="Dinner">Bữa tối</SelectItem>
                    <SelectItem value="Snack">Ăn vặt</SelectItem>
                  </SelectContent>
                </Select>

                <div className="space-y-2 max-h-96 overflow-y-auto">
                  {foodItems.map((item) => (
                    <Card
                      key={item.foodItemId}
                      className={`cursor-pointer transition-all ${
                        selectedFoodItem?.foodItemId === item.foodItemId
                          ? 'border-primary border-2'
                          : 'hover:border-gray-300'
                      }`}
                      onClick={() => setSelectedFoodItem(item)}
                    >
                      <CardContent className="p-4">
                        <div className="flex justify-between items-start">
                          <div>
                            <h3 className="font-semibold">{item.name}</h3>
                            <p className="text-sm text-gray-500">
                              {item.servingSize} {item.servingUnit}
                            </p>
                          </div>
                          <div className="text-right">
                            <p className="font-bold">{item.caloriesKcal.toFixed(0)} kcal</p>
                            <p className="text-xs text-gray-500">
                              P: {item.proteinG.toFixed(0)}g | C: {item.carbsG.toFixed(0)}g | F: {item.fatG.toFixed(0)}g
                            </p>
                          </div>
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>

                {selectedFoodItem && (
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Số lượng</label>
                    <Input
                      type="number"
                      min="0.1"
                      step="0.1"
                      value={quantity}
                      onChange={(e) => setQuantity(Number.parseFloat(e.target.value))}
                    />
                    <div className="bg-gray-50 p-3 rounded">
                      <p className="text-sm font-semibold mb-1">Tổng dinh dưỡng:</p>
                      <p className="text-sm">
                        Calories: {(selectedFoodItem.caloriesKcal * quantity).toFixed(0)} kcal
                      </p>
                      <p className="text-sm">
                        Protein: {(selectedFoodItem.proteinG * quantity).toFixed(1)}g | 
                        Carbs: {(selectedFoodItem.carbsG * quantity).toFixed(1)}g | 
                        Fat: {(selectedFoodItem.fatG * quantity).toFixed(1)}g
                      </p>
                    </div>
                  </div>
                )}

                <Button onClick={handleAddFood} disabled={!selectedFoodItem || isLoading} className="w-full">
                  {isLoading ? 'Đang thêm...' : 'Thêm vào nhật ký'}
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      <Card className="mb-6">
        <CardHeader>
          <CardTitle>Tổng kết hôm nay</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="text-center p-4 bg-orange-50 rounded-lg">
              <p className="text-3xl font-bold text-orange-600">
                {nutritionLog?.totalCalories.toFixed(0) || 0}
              </p>
              <p className="text-sm text-gray-600">Calories (kcal)</p>
            </div>
            <div className="text-center p-4 bg-pink-50 rounded-lg">
              <p className="text-3xl font-bold text-pink-600">
                {nutritionLog?.proteinG.toFixed(0) || 0}g
              </p>
              <p className="text-sm text-gray-600">Protein</p>
            </div>
            <div className="text-center p-4 bg-blue-50 rounded-lg">
              <p className="text-3xl font-bold text-blue-600">
                {nutritionLog?.carbsG.toFixed(0) || 0}g
              </p>
              <p className="text-sm text-gray-600">Carbs</p>
            </div>
            <div className="text-center p-4 bg-yellow-50 rounded-lg">
              <p className="text-3xl font-bold text-yellow-600">
                {nutritionLog?.fatG.toFixed(0) || 0}g
              </p>
              <p className="text-sm text-gray-600">Fat</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="space-y-4">
        {mealTypeOrder.map((mealType) => {
          const entries = groupedEntries[mealType] || [];
          if (entries.length === 0) return null;

          const totals = calculateMealTotals(entries);

          return (
            <Card key={mealType}>
              <CardHeader>
                <div className="flex justify-between items-center">
                  <CardTitle className="text-xl">{getMealTypeLabel(mealType)}</CardTitle>
                  <div className="text-sm text-gray-600">
                    {totals.calories.toFixed(0)} kcal
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {entries.map((entry) => (
                    <div
                      key={entry.foodEntryId}
                      className="flex justify-between items-center p-3 bg-gray-50 rounded-lg"
                    >
                      <div>
                        <p className="font-medium">{entry.foodItemName}</p>
                        <p className="text-sm text-gray-600">
                          Số lượng: {entry.quantity.toFixed(1)} | {entry.caloriesKcal.toFixed(0)} kcal
                        </p>
                        <p className="text-xs text-gray-500">
                          P: {entry.proteinG.toFixed(0)}g | C: {entry.carbsG.toFixed(0)}g | F: {entry.fatG.toFixed(0)}g
                        </p>
                      </div>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDeleteFood(entry.foodEntryId)}
                      >
                        <Trash2 className="h-4 w-4 text-red-500" />
                      </Button>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          );
        })}

        {!nutritionLog || nutritionLog.foodEntries.length === 0 ? (
          <Card>
            <CardContent className="p-12 text-center">
              <Calendar className="h-12 w-12 mx-auto mb-4 text-gray-400" />
              <p className="text-gray-500">Chưa có món ăn nào được ghi lại cho ngày này</p>
              <p className="text-sm text-gray-400 mt-2">Nhấn "Thêm bữa ăn" để bắt đầu</p>
            </CardContent>
          </Card>
        ) : null}
      </div>
    </div>
  );
};

export default NutritionPage;
