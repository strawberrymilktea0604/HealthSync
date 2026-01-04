import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import Header from '@/components/Header';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Plus, Search, Trash2, Calendar, ArrowLeft } from 'lucide-react';
import nutritionService, { FoodItem, FoodEntry, NutritionLog } from '@/services/nutritionService';
import { toast } from '@/hooks/use-toast';
import { PieChart, Pie, Cell, ResponsiveContainer } from "recharts";
import { useNavigate } from 'react-router-dom';
import { format } from "date-fns";
import { dashboardService } from '@/services/dashboardService';

const NutritionPage: React.FC = () => {
  const navigate = useNavigate();
  const [nutritionLog, setNutritionLog] = useState<NutritionLog | null>(null);
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [isAddFoodOpen, setIsAddFoodOpen] = useState(false);
  const [foodItems, setFoodItems] = useState<FoodItem[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedFoodItem, setSelectedFoodItem] = useState<FoodItem | null>(null);
  const [quantity, setQuantity] = useState<number>(1);
  const [mealType, setMealType] = useState<string>('Breakfast');
  const [isLoading, setIsLoading] = useState(false);

  // Nutrition targets - Fetch from dashboard API
  const [targetCalories, setTargetCalories] = useState(2000);
  const [targetProtein, setTargetProtein] = useState(150);
  const [targetCarbs, setTargetCarbs] = useState(220);
  const [targetFat, setTargetFat] = useState(60);

  useEffect(() => {
    loadDashboardTargets();
  }, []);


  useEffect(() => {
    loadNutritionLog();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedDate]);

  const loadDashboardTargets = async () => {
    try {
      const dashboard = await dashboardService.getCustomerDashboard();
      const targetCal = dashboard.todayStats.caloriesTarget || 2000;

      setTargetCalories(targetCal);
      // Calculate macro targets based on standard percentages
      // Protein: 30% of calories / 4 cal per gram
      // Carbs: 45% of calories / 4 cal per gram  
      // Fat: 25% of calories / 9 cal per gram
      setTargetProtein(Math.round((targetCal * 0.30) / 4));
      setTargetCarbs(Math.round((targetCal * 0.45) / 4));
      setTargetFat(Math.round((targetCal * 0.25) / 9));
    } catch (error) {
      console.error('Error loading dashboard targets:', error);
      // Keep default values on error
    }
  };


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

  const currentCalories = nutritionLog?.totalCalories || 0;
  const currentProtein = nutritionLog?.proteinG || 0;
  const currentCarbs = nutritionLog?.carbsG || 0;
  const currentFat = nutritionLog?.fatG || 0;

  // Data for the gauge
  const gaugeData = [
    { name: "Consumed", value: Math.min(currentCalories, targetCalories) },
    { name: "Remaining", value: Math.max(0, targetCalories - currentCalories) },
  ];

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <main className="max-w-4xl mx-auto px-4 md:px-8 pb-12 pt-4">
        {/* Top Nav */}
        <div className="flex justify-between items-center mb-6">
          <div className="flex items-center gap-2">
            <Button variant="ghost" className="rounded-full hover:bg-black/5" onClick={() => navigate(-1)}>
              <ArrowLeft className="w-5 h-5" />
            </Button>
            <h1 className="text-xl font-bold text-gray-800 uppercase tracking-wide">Nhật ký Dinh dưỡng</h1>
          </div>

          <div className="flex items-center gap-2">
            <Input
              type="date"
              value={selectedDate.toISOString().split('T')[0]}
              onChange={(e) => setSelectedDate(new Date(e.target.value))}
              className="w-auto bg-white/60 border-black/10 rounded-xl"
            />
          </div>
        </div>

        {/* Main Overview Card */}
        <div className="bg-[#FFFFE0]/80 rounded-[2.5rem] p-8 shadow-sm border border-white/50 backdrop-blur-sm relative overflow-hidden mb-8">
          {/* Header Text */}
          <div className="text-center mb-8 relative z-10">
            <h2 className="text-3xl md:text-4xl font-bold text-[#2d2d2d] mb-2">Dinh dưỡng hôm nay</h2>
            <p className="text-gray-400 font-medium">{format(selectedDate, "'Cập nhật lúc' HH:mm, dd/MM/yyyy")}</p>
          </div>

          {/* Gauge Chart */}
          <div className="relative h-64 w-full flex items-center justify-center mb-8 z-10">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={gaugeData}
                  cx="50%"
                  cy="70%"
                  startAngle={180}
                  endAngle={0}
                  innerRadius={100}
                  outerRadius={120}
                  paddingAngle={0}
                  dataKey="value"
                  stroke="none"
                  cornerRadius={10}
                >
                  <Cell fill="url(#colorGradient)" />
                  <Cell fill="#e5e7eb" />
                </Pie>
                <defs>
                  <linearGradient id="colorGradient" x1="0" y1="0" x2="1" y2="0">
                    <stop offset="0%" stopColor="#ebb305" />
                    <stop offset="50%" stopColor="#f472b6" />
                    <stop offset="100%" stopColor="#60a5fa" />
                  </linearGradient>
                </defs>
              </PieChart>
            </ResponsiveContainer>

            <div className="absolute top-[60%] left-1/2 transform -translate-x-1/2 -translate-y-1/2 text-center">
              <p className="text-5xl font-black text-[#ff8a80]">{Math.round(currentCalories)}</p>
              <p className="text-gray-400 font-medium text-lg">/ {targetCalories} kcal</p>
            </div>
          </div>

          {/* Stats Row */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10 z-10 relative">
            <div className="bg-white/60 rounded-3xl p-4 flex flex-col items-center justify-center text-center backdrop-blur-sm">
              <div className="flex items-center gap-2 mb-2">
                <div className="w-3 h-3 rounded-full bg-pink-400"></div>
                <span className="font-bold text-gray-700">Protein</span>
              </div>
              <p className="text-2xl font-black text-[#2d2d2d] mb-1">{Math.round(currentProtein)}g</p>
              <p className="text-xs text-gray-500">Mục tiêu: {targetProtein}g</p>
            </div>
            <div className="bg-white/60 rounded-3xl p-4 flex flex-col items-center justify-center text-center backdrop-blur-sm">
              <div className="flex items-center gap-2 mb-2">
                <div className="w-3 h-3 rounded-full bg-blue-400"></div>
                <span className="font-bold text-gray-700">Carb</span>
              </div>
              <p className="text-2xl font-black text-[#2d2d2d] mb-1">{Math.round(currentCarbs)}g</p>
              <p className="text-xs text-gray-500">Mục tiêu: {targetCarbs}g</p>
            </div>
            <div className="bg-white/60 rounded-3xl p-4 flex flex-col items-center justify-center text-center backdrop-blur-sm">
              <div className="flex items-center gap-2 mb-2">
                <div className="w-3 h-3 rounded-full bg-yellow-400"></div>
                <span className="font-bold text-gray-700">Fat</span>
              </div>
              <p className="text-2xl font-black text-[#2d2d2d] mb-1">{Math.round(currentFat)}g</p>
              <p className="text-xs text-gray-500">Mục tiêu: {targetFat}g</p>
            </div>
          </div>

          {/* Add Button */}
          <div className="flex justify-center z-10 relative">
            <Dialog open={isAddFoodOpen} onOpenChange={setIsAddFoodOpen}>
              <DialogTrigger asChild>
                <Button
                  className="bg-[#ffab91] hover:bg-[#ff8a65] text-white rounded-full px-8 py-6 text-lg font-bold shadow-lg shadow-[#ffab91]/30 transition-all hover:scale-105 flex items-center gap-2"
                >
                  <div className="bg-white/20 rounded-full p-1">
                    <Plus className="w-5 h-5" />
                  </div>
                  Thêm bữa ăn
                </Button>
              </DialogTrigger>
              <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto bg-[#FDFBD4] border-none rounded-3xl">
                <DialogHeader>
                  <DialogTitle className="text-2xl font-bold text-[#2d2d2d]">Thêm món ăn</DialogTitle>
                </DialogHeader>
                {/* Reuse Add Food Logic */}
                <div className="space-y-4">
                  <div className="flex gap-2">
                    <Input
                      placeholder="Tìm kiếm món ăn..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
                      className="bg-white/60 border-black/10 rounded-xl"
                    />
                    <Button onClick={handleSearch} variant="outline" className="rounded-xl border-black/10 bg-white/60">
                      <Search className="h-4 w-4" />
                    </Button>
                  </div>

                  <Select value={mealType} onValueChange={setMealType}>
                    <SelectTrigger className="bg-white/60 border-black/10 rounded-xl">
                      <SelectValue placeholder="Chọn bữa ăn" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Breakfast">Bữa sáng</SelectItem>
                      <SelectItem value="Lunch">Bữa trưa</SelectItem>
                      <SelectItem value="Dinner">Bữa tối</SelectItem>
                      <SelectItem value="Snack">Ăn vặt</SelectItem>
                    </SelectContent>
                  </Select>

                  <div className="space-y-2 max-h-60 overflow-y-auto custom-scrollbar">
                    {foodItems.map((item) => (
                      <div
                        key={item.foodItemId}
                        className={`cursor-pointer transition-all p-4 rounded-xl border ${selectedFoodItem?.foodItemId === item.foodItemId
                          ? 'bg-[#EBE9C0] border-[#4A6F6F]'
                          : 'bg-white/40 border-transparent hover:bg-white/60'
                          }`}
                        onClick={() => setSelectedFoodItem(item)}
                      >
                        <div className="flex justify-between items-start">
                          <div>
                            <h3 className="font-bold text-[#2d2d2d]">{item.name}</h3>
                            <p className="text-xs text-gray-500">
                              {item.servingSize} {item.servingUnit}
                            </p>
                          </div>
                          <div className="text-right">
                            <p className="font-bold text-[#4A6F6F]">{item.caloriesKcal.toFixed(0)} kcal</p>
                            <p className="text-[10px] text-gray-500 uppercase">
                              P: {item.proteinG.toFixed(0)} | C: {item.carbsG.toFixed(0)} | F: {item.fatG.toFixed(0)}
                            </p>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>

                  {selectedFoodItem && (
                    <div className="space-y-2 bg-white/40 p-4 rounded-xl">
                      <label htmlFor="quantity-input" className="text-sm font-medium text-gray-600">Số lượng</label>
                      <Input
                        id="quantity-input"
                        type="number"
                        min="0.1"
                        step="0.1"
                        value={quantity}
                        onChange={(e) => setQuantity(Number.parseFloat(e.target.value))}
                        className="bg-white/80 border-black/10 rounded-lg"
                      />
                      <div className="mt-2 text-sm text-[#2d2d2d]">
                        <p className="font-bold">Tổng: {(selectedFoodItem.caloriesKcal * quantity).toFixed(0)} kcal</p>
                      </div>
                    </div>
                  )}

                  <Button onClick={handleAddFood} disabled={!selectedFoodItem || isLoading} className="w-full bg-[#2d2d2d] hover:bg-black text-[#FDFBD4] rounded-xl font-bold py-6">
                    {isLoading ? 'Đang thêm...' : 'Thêm vào nhật ký'}
                  </Button>
                </div>
              </DialogContent>
            </Dialog>
          </div>
        </div>

        {/* Meal List */}
        <div className="space-y-6">
          {mealTypeOrder.map((mealType) => {
            const entries = groupedEntries[mealType] || [];
            if (entries.length === 0) return null;

            const totals = calculateMealTotals(entries);

            return (
              <div key={mealType} className="bg-white/40 border border-white/50 rounded-[2rem] p-6 backdrop-blur-sm">
                <div className="flex justify-between items-center mb-4">
                  <h3 className="font-bold text-xl text-[#2d2d2d]">{getMealTypeLabel(mealType)}</h3>
                  <div className="text-sm font-bold text-[#4A6F6F] bg-[#EBE9C0] px-3 py-1 rounded-full">
                    {totals.calories.toFixed(0)} kcal
                  </div>
                </div>
                <div className="space-y-3">
                  {entries.map((entry) => (
                    <div
                      key={entry.foodEntryId}
                      className="flex justify-between items-center p-4 bg-white/60 rounded-2xl hover:bg-white/80 transition-colors"
                    >
                      <div>
                        <p className="font-bold text-[#2d2d2d]">{entry.foodItemName}</p>
                        <p className="text-sm text-gray-500 mt-1">
                          <span className="font-bold">{entry.caloriesKcal.toFixed(0)} kcal</span>
                          <span className="mx-2 text-gray-300">|</span>
                          Số lượng: {entry.quantity.toFixed(1)}
                        </p>
                        <p className="text-[10px] text-gray-400 mt-1 uppercase">
                          P: {entry.proteinG.toFixed(0)}g | C: {entry.carbsG.toFixed(0)}g | F: {entry.fatG.toFixed(0)}g
                        </p>
                      </div>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleDeleteFood(entry.foodEntryId)}
                        className="text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-full"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  ))}
                </div>
              </div>
            );
          })}
        </div>

        {!nutritionLog || nutritionLog.foodEntries.length === 0 && (
          <div className="text-center py-12">
            <p className="text-gray-400 italic">Chưa có món ăn nào được ghi lại hôm nay.</p>
          </div>
        )}

      </main>
    </div>
  );
};

export default NutritionPage;

