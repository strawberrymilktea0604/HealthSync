import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Header from "@/components/Header";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ArrowLeft, Calendar, TrendingUp, TrendingDown } from "lucide-react";
import nutritionService, { NutritionLog } from "@/services/nutritionService";
import { format, subDays } from "date-fns";
import { vi } from "date-fns/locale";
import { LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";

export default function NutritionHistory() {
  const navigate = useNavigate();
  const [logs, setLogs] = useState<NutritionLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState<"week" | "month">("week");

  useEffect(() => {
    loadHistory();
  }, [viewMode]);

  const loadHistory = async () => {
    try {
      setLoading(true);
      const endDate = new Date();
      const startDate = viewMode === "week" ? subDays(endDate, 7) : subDays(endDate, 30);

      const data = await nutritionService.getNutritionLogs(startDate, endDate);
      // Sort by date ascending (oldest first)
      const sortedData = data.sort((a, b) => new Date(a.logDate).getTime() - new Date(b.logDate).getTime());
      setLogs(sortedData);
    } catch (error) {
      console.error("Failed to load nutrition history:", error);
    } finally {
      setLoading(false);
    }
  };

  // Calculate averages
  const calculateAverages = () => {
    if (logs.length === 0) return { calories: 0, protein: 0, carbs: 0, fat: 0 };

    const totals = logs.reduce((acc, log) => ({
      calories: acc.calories + log.totalCalories,
      protein: acc.protein + log.proteinG,
      carbs: acc.carbs + log.carbsG,
      fat: acc.fat + log.fatG
    }), { calories: 0, protein: 0, carbs: 0, fat: 0 });

    return {
      calories: totals.calories / logs.length,
      protein: totals.protein / logs.length,
      carbs: totals.carbs / logs.length,
      fat: totals.fat / logs.length
    };
  };

  const averages = calculateAverages();

  // Prepare chart data (already sorted from oldest to newest)
  const chartData = logs.map(log => ({
    date: format(new Date(log.logDate), "dd/MM", { locale: vi }),
    fullDate: format(new Date(log.logDate), "dd/MM/yyyy", { locale: vi }),
    calories: Math.round(log.totalCalories),
    protein: Number.parseFloat(log.proteinG.toFixed(1)),
    carbs: Number.parseFloat(log.carbsG.toFixed(1)),
    fat: Number.parseFloat(log.fatG.toFixed(1))
  }));

  // Weekly comparison
  const getWeekComparison = () => {
    if (logs.length < 7) return { change: 0, trend: "stable" as const };

    const thisWeek = logs.slice(-7);
    const lastWeek = logs.slice(-14, -7);

    const thisWeekAvg = thisWeek.reduce((sum, log) => sum + log.totalCalories, 0) / 7;
    const lastWeekAvg = lastWeek.reduce((sum, log) => sum + log.totalCalories, 0) / 7;

    const change = ((thisWeekAvg - lastWeekAvg) / lastWeekAvg) * 100;
    let trend: "up" | "down" | "stable" = "stable";
    if (change > 5) {
      trend = "up";
    } else if (change < -5) {
      trend = "down";
    }

    return { change: Math.abs(change), trend };
  };

  const weekComparison = getWeekComparison();

  if (loading) {
    return (
      <div className="min-h-screen bg-[#FDFBD4] flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#4A6F6F]"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <main className="max-w-7xl mx-auto px-4 md:px-8 pb-12 pt-4">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Button variant="ghost" className="rounded-full hover:bg-black/5" onClick={() => navigate(-1)}>
              <ArrowLeft className="w-5 h-5" />
            </Button>
            <h1 className="text-2xl font-bold text-gray-800 uppercase tracking-wide">Lịch sử dinh dưỡng</h1>
          </div>

          <div className="flex gap-3">
            <Button
              variant={viewMode === "week" ? "default" : "outline"}
              size="lg"
              className={`rounded-2xl px-6 font-semibold transition-all ${viewMode === "week"
                  ? "bg-[#4A6F6F] hover:bg-[#3d5c5c] text-white shadow-md"
                  : "bg-white/80 hover:bg-white border-2 border-[#4A6F6F]/20"
                }`}
              onClick={() => setViewMode("week")}
            >
              <Calendar className="w-4 h-4 mr-2" />
              7 ngày
            </Button>
            <Button
              variant={viewMode === "month" ? "default" : "outline"}
              size="lg"
              className={`rounded-2xl px-6 font-semibold transition-all ${viewMode === "month"
                  ? "bg-[#4A6F6F] hover:bg-[#3d5c5c] text-white shadow-md"
                  : "bg-white/80 hover:bg-white border-2 border-[#4A6F6F]/20"
                }`}
              onClick={() => setViewMode("month")}
            >
              <Calendar className="w-4 h-4 mr-2" />
              30 ngày
            </Button>
          </div>
        </div>

        {/* Summary Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm rounded-3xl">
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-gray-600">Calories trung bình</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-[#2d2d2d]">{Math.round(averages.calories)}</div>
              <p className="text-xs text-gray-500 mt-1">kcal/ngày</p>
              {weekComparison.trend !== "stable" && (
                <div className={`flex items-center gap-1 mt-2 text-xs ${weekComparison.trend === "up" ? "text-red-500" : "text-green-500"}`}>
                  {weekComparison.trend === "up" ? <TrendingUp className="w-3 h-3" /> : <TrendingDown className="w-3 h-3" />}
                  {weekComparison.change.toFixed(1)}% so với tuần trước
                </div>
              )}
            </CardContent>
          </Card>

          <Card className="bg-white/80 border-white/50 backdrop-blur-sm rounded-3xl">
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-gray-600">Protein trung bình</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-pink-500">{Math.round(averages.protein)}</div>
              <p className="text-xs text-gray-500 mt-1">g/ngày</p>
            </CardContent>
          </Card>

          <Card className="bg-white/80 border-white/50 backdrop-blur-sm rounded-3xl">
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-gray-600">Carbs trung bình</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-blue-500">{Math.round(averages.carbs)}</div>
              <p className="text-xs text-gray-500 mt-1">g/ngày</p>
            </CardContent>
          </Card>

          <Card className="bg-white/80 border-white/50 backdrop-blur-sm rounded-3xl">
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-gray-600">Fat trung bình</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-yellow-500">{Math.round(averages.fat)}</div>
              <p className="text-xs text-gray-500 mt-1">g/ngày</p>
            </CardContent>
          </Card>
        </div>

        {/* Charts */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
          {/* Calories Chart */}
          <Card className="bg-white/80 border-white/50 backdrop-blur-sm rounded-3xl">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Calendar className="w-5 h-5 text-[#4A6F6F]" />
                Calories theo ngày
              </CardTitle>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis dataKey="date" tick={{ fontSize: 12 }} stroke="#6b7280" />
                  <YAxis tick={{ fontSize: 12 }} stroke="#6b7280" />
                  <Tooltip
                    contentStyle={{ backgroundColor: '#fff', border: '1px solid #e5e7eb', borderRadius: '0.5rem' }}
                    labelFormatter={(label) => {
                      const item = chartData.find(d => d.date === label);
                      return item?.fullDate || label;
                    }}
                  />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="calories"
                    stroke="#ff8a80"
                    strokeWidth={3}
                    name="Calories"
                    dot={{ fill: '#ff8a80', r: 4 }}
                    activeDot={{ r: 6 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>

          {/* Macros Chart */}
          <Card className="bg-white/80 border-white/50 backdrop-blur-sm rounded-3xl">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <TrendingUp className="w-5 h-5 text-[#4A6F6F]" />
                Macros theo ngày
              </CardTitle>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis dataKey="date" tick={{ fontSize: 12 }} stroke="#6b7280" />
                  <YAxis tick={{ fontSize: 12 }} stroke="#6b7280" />
                  <Tooltip
                    contentStyle={{ backgroundColor: '#fff', border: '1px solid #e5e7eb', borderRadius: '0.5rem' }}
                    labelFormatter={(label) => {
                      const item = chartData.find(d => d.date === label);
                      return item?.fullDate || label;
                    }}
                  />
                  <Legend />
                  <Bar dataKey="protein" fill="#f472b6" name="Protein (g)" />
                  <Bar dataKey="carbs" fill="#60a5fa" name="Carbs (g)" />
                  <Bar dataKey="fat" fill="#fbbf24" name="Fat (g)" />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </div>

        {/* Daily Breakdown */}
        <Card className="bg-white/80 border-white/50 backdrop-blur-sm rounded-3xl">
          <CardHeader>
            <CardTitle>Chi tiết theo ngày</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {logs.slice().reverse().map((log) => (
                <button
                  key={log.nutritionLogId}
                  className="w-full flex items-center justify-between p-4 bg-[#FDFBD4]/50 rounded-2xl hover:bg-[#EBE9C0]/50 transition-colors cursor-pointer text-left"
                  onClick={() => {
                    // Navigate to diary with this date
                    navigate('/nutrition-diary');
                  }}
                >
                  <div className="flex items-center gap-4">
                    <div className="text-center">
                      <div className="text-2xl font-bold text-gray-800">
                        {format(new Date(log.logDate), "dd")}
                      </div>
                      <div className="text-xs text-gray-500 uppercase">
                        {format(new Date(log.logDate), "MMM", { locale: vi })}
                      </div>
                    </div>
                    <div>
                      <p className="font-semibold text-gray-800">
                        {format(new Date(log.logDate), "EEEE, dd MMMM yyyy", { locale: vi })}
                      </p>
                      <p className="text-sm text-gray-500">
                        {log.foodEntries.length} món ăn
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-6">
                    <div className="text-right">
                      <p className="text-2xl font-bold text-[#ff8a80]">{Math.round(log.totalCalories)}</p>
                      <p className="text-xs text-gray-500">kcal</p>
                    </div>
                    <div className="flex gap-3 text-xs">
                      <div className="text-center">
                        <div className="w-2 h-2 rounded-full bg-pink-400 mx-auto mb-1"></div>
                        <p className="font-semibold text-gray-700">{log.proteinG.toFixed(1)}g</p>
                      </div>
                      <div className="text-center">
                        <div className="w-2 h-2 rounded-full bg-blue-400 mx-auto mb-1"></div>
                        <p className="font-semibold text-gray-700">{log.carbsG.toFixed(1)}g</p>
                      </div>
                      <div className="text-center">
                        <div className="w-2 h-2 rounded-full bg-yellow-400 mx-auto mb-1"></div>
                        <p className="font-semibold text-gray-700">{log.fatG.toFixed(1)}g</p>
                      </div>
                    </div>
                  </div>
                </div>
              ))}

              {logs.length === 0 && (
                <div className="text-center py-12 text-gray-400">
                  <Calendar className="w-16 h-16 mx-auto mb-4 opacity-30" />
                  <p className="text-lg font-medium">Chưa có dữ liệu</p>
                  <p className="text-sm mt-2">Bắt đầu ghi nhật ký dinh dưỡng của bạn!</p>
                  <Button
                    className="mt-4 bg-[#2d2d2d] hover:bg-black text-[#FDFBD4] rounded-xl"
                    onClick={() => navigate('/nutrition')}
                  >
                    Thêm bữa ăn
                  </Button>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </main>
    </div>
  );
}
