import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import Header from "@/components/Header";
import { useAuth } from "@/contexts/AuthContext";
import nutritionService, { NutritionLog } from "@/services/nutritionService";
import { format } from "date-fns";
import { vi } from "date-fns/locale";
import { useNavigate } from "react-router-dom";
import { Calendar as CalendarIcon, ArrowLeft, Plus, Trash2, Sun, Moon, Apple, Utensils, BarChart3, Loader2 } from "lucide-react";
import Calendar from "react-calendar";
import "react-calendar/dist/Calendar.css";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

export default function NutritionDiary() {
  useAuth();
  const navigate = useNavigate();
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [nutritionLog, setNutritionLog] = useState<NutritionLog | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadNutritionLog();
  }, [selectedDate]);

  const loadNutritionLog = async () => {
    try {
      setLoading(true);
      const log = await nutritionService.getNutritionLogByDate(selectedDate);
      setNutritionLog(log);
    } catch (error) {
      console.error("Failed to load nutrition log:", error);
      setNutritionLog(null);
    } finally {
      setLoading(false);
    }
  };

  const getMealEntries = (mealType: string) => {
    return nutritionLog?.foodEntries?.filter((e: any) => e.mealType === mealType) || [];
  };

  const currentTotals = {
    calories: nutritionLog?.totalCalories || 0,
    protein: nutritionLog?.proteinG || 0,
    carbs: nutritionLog?.carbsG || 0,
    fat: nutritionLog?.fatG || 0
  };

  const handleDeleteEntry = async (id: number) => {
    try {
      await nutritionService.deleteFoodEntry(id);
      loadNutritionLog();
    } catch (error) {
      console.error("Failed to delete entry:", error);
    }
  };

  const getMealIcon = (mealType: string) => {
    switch (mealType) {
      case "Breakfast": return <Sun className="w-5 h-5" />;
      case "Lunch": return <Utensils className="w-5 h-5" />;
      case "Dinner": return <Moon className="w-5 h-5" />;
      case "Snack": return <Apple className="w-5 h-5" />;
      default: return <Utensils className="w-5 h-5" />;
    }
  };

  const getMealColor = (mealType: string) => {
    switch (mealType) {
      case "Breakfast": return "bg-orange-100 text-orange-700";
      case "Lunch": return "bg-green-100 text-green-700";
      case "Dinner": return "bg-blue-100 text-blue-700";
      case "Snack": return "bg-purple-100 text-purple-700";
      default: return "bg-gray-100 text-gray-700";
    }
  };

  const getMealName = (mealType: string) => {
    switch (mealType) {
      case "Breakfast": return "Bữa sáng";
      case "Lunch": return "Bữa trưa";
      case "Dinner": return "Bữa tối";
      case "Snack": return "Bữa phụ";
      default: return mealType;
    }
  };

  const renderMealSection = (mealType: string) => {
    const foods = getMealEntries(mealType);
    console.log(`renderMealSection ${mealType} - foods count:`, foods.length);
    const mealTotals = foods.reduce((acc, food) => ({
      calories: acc.calories + food.caloriesKcal,
      protein: acc.protein + food.proteinG,
      carbs: acc.carbs + food.carbsG,
      fat: acc.fat + food.fatG
    }), { calories: 0, protein: 0, carbs: 0, fat: 0 });

    return (
      <Card className="bg-white/80 border-white/50 backdrop-blur-sm rounded-3xl mb-6">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className={`w-12 h-12 rounded-2xl ${getMealColor(mealType)} flex items-center justify-center`}>
                {getMealIcon(mealType)}
              </div>
              <div>
                <CardTitle className="text-lg">{getMealName(mealType)}</CardTitle>
                <p className="text-xs text-gray-500">{foods.length} món</p>
              </div>
            </div>
            {foods.length > 0 && (
              <div className="text-right">
                <p className="text-xl font-bold text-[#ff8a80]">{mealTotals.calories.toFixed(0)}</p>
                <p className="text-xs text-gray-500">kcal</p>
              </div>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {foods.length > 0 ? (
            <div className="space-y-3">
              {foods.map((food) => (
                <div key={food.foodEntryId} className="bg-[#FDFBD4]/30 rounded-2xl p-3 hover:bg-[#EBE9C0]/40 transition-colors">
                  <div className="flex items-start gap-3">
                    <div className="w-16 h-16 rounded-xl bg-white overflow-hidden shrink-0 border border-black/5 relative">
                      {food.imageUrl ? (
                        <img src={food.imageUrl} alt={food.foodItemName} className="w-full h-full object-cover" />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center text-xs text-gray-400 bg-gray-50">
                          <Utensils className="w-6 h-6 opacity-30" />
                        </div>
                      )}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-start justify-between mb-2">
                        <div>
                          <p className="font-bold text-gray-900">{food.foodItemName}</p>
                          <p className="text-xs text-gray-500">Số lượng: {food.quantity.toFixed(1)}</p>
                        </div>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => handleDeleteEntry(food.foodEntryId)}
                          className="text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-full h-8 w-8 shrink-0"
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      </div>

                      <div className="flex gap-2 flex-wrap">
                        <div className="bg-white/80 px-2 py-1 rounded-lg text-xs">
                          <span className="text-gray-500">Calo:</span>
                          <span className="font-bold text-gray-900 ml-1">{food.caloriesKcal.toFixed(0)}</span>
                        </div>
                        <div className="bg-pink-50 px-2 py-1 rounded-lg text-xs">
                          <span className="text-pink-600">P:</span>
                          <span className="font-bold text-pink-700 ml-1">{food.proteinG.toFixed(1)}g</span>
                        </div>
                        <div className="bg-blue-50 px-2 py-1 rounded-lg text-xs">
                          <span className="text-blue-600">C:</span>
                          <span className="font-bold text-blue-700 ml-1">{food.carbsG.toFixed(1)}g</span>
                        </div>
                        <div className="bg-yellow-50 px-2 py-1 rounded-lg text-xs">
                          <span className="text-yellow-600">F:</span>
                          <span className="font-bold text-yellow-700 ml-1">{food.fatG.toFixed(1)}g</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8 text-gray-400">
              <Utensils className="w-12 h-12 mx-auto mb-3 opacity-20" />
              <p className="text-sm">Chưa có món ăn nào</p>
            </div>
          )}

          <Button
            variant="outline"
            className="w-full mt-4 rounded-xl border-dashed hover:bg-[#EBE9C0]/30"
            onClick={() => navigate('/nutrition')}
          >
            <Plus className="w-4 h-4 mr-2" />
            Thêm món
          </Button>
        </CardContent>
      </Card>
    );
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#FDFBD4] flex items-center justify-center">
        <Loader2 className="w-12 h-12 animate-spin text-[#4A6F6F]" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <main className="max-w-7xl mx-auto px-4 md:px-8 pb-12 pt-4">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between flex-wrap gap-4">
          <div className="flex items-center gap-2">
            <Button variant="ghost" className="rounded-full hover:bg-black/5" onClick={() => navigate(-1)}>
              <ArrowLeft className="w-5 h-5" />
            </Button>
            <div>
              <h1 className="text-2xl font-bold text-gray-800 uppercase tracking-wide">Nhật ký dinh dưỡng</h1>
              <p className="text-sm text-gray-500">Theo dõi chi tiết dinh dưỡng hàng ngày</p>
            </div>
          </div>

          <div className="flex gap-2">
            <Button
              variant="outline"
              className="rounded-xl"
              onClick={() => navigate('/nutrition-overview')}
            >
              Tổng quan
            </Button>
            <Button
              variant="outline"
              className="rounded-xl"
              onClick={() => navigate('/nutrition-history')}
            >
              <BarChart3 className="w-4 h-4 mr-2" />
              Lịch sử
            </Button>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Left Column - Diary Entries */}
          <div className="lg:col-span-2 space-y-6">
            {/* Date Selector */}
            <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm rounded-3xl">
              <CardContent className="pt-6">
                <div className="flex items-center gap-3">
                  <CalendarIcon className="w-5 h-5 text-[#4A6F6F]" />
                  <Popover>
                    <PopoverTrigger asChild>
                      <Button variant="outline" className="w-full justify-start text-left font-normal rounded-xl h-12 text-base">
                        {selectedDate ? format(selectedDate, "dd/MM/yyyy - EEEE", { locale: vi }) : "Chọn ngày"}
                      </Button>
                    </PopoverTrigger>
                    <PopoverContent className="w-auto p-4" align="start">
                      <Calendar
                        value={selectedDate}
                        onChange={(date) => date && setSelectedDate(date as Date)}
                        locale="vi-VN"
                        className="text-base custom-calendar"
                      />
                    </PopoverContent>
                  </Popover>
                </div>
              </CardContent>
            </Card>

            {/* Daily Summary */}
            <Card className="bg-gradient-to-br from-[#FFFFE0]/80 to-[#EBE9C0]/80 border-white/50 backdrop-blur-sm rounded-3xl">
              <CardHeader>
                <CardTitle className="text-xl">Tổng kết ngày</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div className="text-center bg-white/60 rounded-2xl p-4">
                    <p className="text-xs text-gray-500 mb-1">Tổng Calories</p>
                    <p className="text-3xl font-black text-[#ff8a80]">{currentTotals.calories.toFixed(0)}</p>
                    <p className="text-xs text-gray-500">kcal</p>
                  </div>
                  <div className="text-center bg-pink-50 rounded-2xl p-4">
                    <p className="text-xs text-pink-600 mb-1">Protein</p>
                    <p className="text-3xl font-black text-pink-600">{currentTotals.protein.toFixed(1)}</p>
                    <p className="text-xs text-pink-600">g</p>
                  </div>
                  <div className="text-center bg-blue-50 rounded-2xl p-4">
                    <p className="text-xs text-blue-600 mb-1">Carbs</p>
                    <p className="text-3xl font-black text-blue-600">{currentTotals.carbs.toFixed(1)}</p>
                    <p className="text-xs text-blue-600">g</p>
                  </div>
                  <div className="text-center bg-yellow-50 rounded-2xl p-4">
                    <p className="text-xs text-yellow-600 mb-1">Fat</p>
                    <p className="text-3xl font-black text-yellow-600">{currentTotals.fat.toFixed(1)}</p>
                    <p className="text-xs text-yellow-600">g</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Meal Sections */}
            {renderMealSection("Breakfast")}
            {renderMealSection("Lunch")}
            {renderMealSection("Dinner")}
            {renderMealSection("Snack")}
          </div>

          {/* Right Column - Quick Actions */}
          <div className="space-y-6">
            <Card className="bg-white/80 border-white/50 backdrop-blur-sm rounded-3xl sticky top-24">
              <CardHeader>
                <CardTitle>Thao tác nhanh</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <Button
                  className="w-full bg-[#2d2d2d] hover:bg-black text-[#FDFBD4] rounded-xl font-bold"
                  onClick={() => navigate('/nutrition')}
                >
                  <Plus className="w-4 h-4 mr-2" />
                  Thêm món ăn
                </Button>
                <Button
                  variant="outline"
                  className="w-full rounded-xl"
                  onClick={() => navigate('/nutrition-history')}
                >
                  <BarChart3 className="w-4 h-4 mr-2" />
                  Xem báo cáo tuần
                </Button>
                <Button
                  variant="outline"
                  className="w-full rounded-xl"
                  onClick={() => navigate('/nutrition-overview')}
                >
                  <CalendarIcon className="w-4 h-4 mr-2" />
                  Tổng quan hôm nay
                </Button>
              </CardContent>
            </Card>
          </div>
        </div>
      </main>
    </div>
  );
}
