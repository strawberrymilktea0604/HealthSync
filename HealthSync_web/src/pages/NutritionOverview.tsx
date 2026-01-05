import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import nutritionService, { NutritionLog } from "@/services/nutritionService";
import Header from "@/components/Header";
import { Loader2, Plus, ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { format } from "date-fns";
import { PieChart, Pie, Cell, ResponsiveContainer } from "recharts";
import { useAuth } from "@/contexts/AuthContext";
import { motion } from "framer-motion";
import { dashboardService } from "@/services/dashboardService";

export default function NutritionOverview() {
  useAuth();
  const navigate = useNavigate();
  const [nutritionLog, setNutritionLog] = useState<NutritionLog | null>(null);
  const [loading, setLoading] = useState(true);

  // Nutrition targets - Fetch from dashboard API
  const [targetCalories, setTargetCalories] = useState(2000);
  const [targetProtein, setTargetProtein] = useState(150);
  const [targetCarbs, setTargetCarbs] = useState(220);
  const [targetFat, setTargetFat] = useState(60);

  useEffect(() => {
    loadDashboardTargets();
    loadNutritionLog();
  }, []);

  const loadDashboardTargets = async () => {
    try {
      const dashboard = await dashboardService.getCustomerDashboard();
      const targetCal = dashboard.todayStats.caloriesTarget || 2000;

      setTargetCalories(targetCal);
      // Calculate macro targets based on standard percentages
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
      setLoading(true);
      const data = await nutritionService.getNutritionLogByDate(new Date());
      setNutritionLog(data);
    } catch (error) {
      console.error("Failed to load nutrition log", error);
    } finally {
      setLoading(false);
    }
  };

  const currentCalories = nutritionLog?.totalCalories || 0;
  const currentProtein = nutritionLog?.proteinG || 0;
  const currentCarbs = nutritionLog?.carbsG || 0;
  const currentFat = nutritionLog?.fatG || 0;

  // Data for the gauge
  const gaugeData = [
    { name: "Consumed", value: Math.min(currentCalories, targetCalories) },
    { name: "Remaining", value: Math.max(0, targetCalories - currentCalories) },
  ];


  // Custom detailed gauge data
  // We want a gradient-like effect or multiple segments if possible, but for now simple 2 segments
  // Actually, to match Image 0 (rainbow/pink-blue-yellow), we might want a segmented colorful arc if it's the specific design.
  // The image description says "Pink Protein, Blue Carb, Yellow Fat".
  // The gauge itself in Image 0 seems to be "Calories".
  // Let's stick to a clean calories gauge for now.

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

      <main className="max-w-4xl mx-auto px-4 md:px-8 pb-12 pt-4">
        <div className="mb-6 flex items-center gap-2">
          <Button variant="ghost" className="rounded-full hover:bg-black/5" onClick={() => navigate(-1)}>
            <ArrowLeft className="w-5 h-5" />
          </Button>
          <h1 className="text-xl font-bold text-gray-800 uppercase tracking-wide">Dinh dưỡng hôm nay</h1>
        </div>

        {/* Main Card */}
        <div className="bg-[#FFFFE0]/80 rounded-[2.5rem] p-8 shadow-sm border border-white/50 backdrop-blur-sm relative overflow-hidden">

          {/* Header Text */}
          <div className="text-center mb-8 relative z-10">
            <h2 className="text-3xl md:text-4xl font-bold text-[#2d2d2d] mb-2">Dinh dưỡng hôm nay</h2>
            <p className="text-gray-400 font-medium">{format(new Date(), "'Cập nhật lúc' HH:mm, dd/MM/yyyy")}</p>
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

            {/* Protein */}
            <div className="bg-white/60 rounded-3xl p-4 flex flex-col items-center justify-center text-center backdrop-blur-sm">
              <div className="flex items-center gap-2 mb-2">
                <div className="w-3 h-3 rounded-full bg-pink-400"></div>
                <span className="font-bold text-gray-700">Protein</span>
              </div>
              <p className="text-2xl font-black text-[#2d2d2d] mb-1">{Math.round(currentProtein)}g</p>
              <p className="text-xs text-gray-500">Mục tiêu: {targetProtein}g</p>
            </div>

            {/* Carb */}
            <div className="bg-white/60 rounded-3xl p-4 flex flex-col items-center justify-center text-center backdrop-blur-sm">
              <div className="flex items-center gap-2 mb-2">
                <div className="w-3 h-3 rounded-full bg-blue-400"></div>
                <span className="font-bold text-gray-700">Carb</span>
              </div>
              <p className="text-2xl font-black text-[#2d2d2d] mb-1">{Math.round(currentCarbs)}g</p>
              <p className="text-xs text-gray-500">Mục tiêu: {targetCarbs}g</p>
            </div>

            {/* Fat */}
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
          <div className="flex flex-col items-center justify-center z-10 relative gap-4">
            <Button
              className="bg-[#ffab91] hover:bg-[#ff8a65] text-white rounded-full px-8 py-6 text-lg font-bold shadow-lg shadow-[#ffab91]/30 transition-all hover:scale-105 flex items-center gap-2"
              onClick={() => navigate('/nutrition')}
            >
              <div className="bg-white/20 rounded-full p-1">
                <Plus className="w-5 h-5" />
              </div>
              Thêm bữa ăn
            </Button>

            <Button
              variant="ghost"
              className="text-gray-500 hover:text-gray-800"
              onClick={() => navigate('/nutrition-diary')}
            >
              Xem chi tiết nhật ký
            </Button>
          </div>

          {/* Recent Entries Preview */}
          {nutritionLog?.foodEntries && nutritionLog.foodEntries.length > 0 && (
            <div className="mt-8 pt-6 border-t border-black/5">
              <h3 className="text-lg font-bold text-gray-700 mb-4">Đã ăn hôm nay</h3>
              <div className="space-y-3">
                {nutritionLog.foodEntries.slice(0, 3).map((entry, index) => (
                  <div key={entry.id || index} className="flex items-center gap-3 bg-white/40 p-3 rounded-xl border border-white/50">
                    <div className="w-12 h-12 rounded-lg bg-white overflow-hidden shrink-0 border border-black/5 relative">
                      {entry.imageUrl ? (
                        <img src={entry.imageUrl} alt={entry.foodItemName} className="w-full h-full object-cover" />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center text-[10px] text-gray-400">No img</div>
                      )}
                    </div>
                    <div className="flex-1">
                      <p className="font-bold text-gray-800 text-sm">{entry.foodItemName}</p>
                      <p className="text-xs text-gray-500">{entry.caloriesKcal.toFixed(0)} kcal • {entry.mealType}</p>
                    </div>
                  </div>
                ))}
                {nutritionLog.foodEntries.length > 3 && (
                  <p className="text-center text-xs text-gray-400 mt-2">
                    ...và {nutritionLog.foodEntries.length - 3} món khác
                  </p>
                )}
              </div>
            </div>
          )}

        </div>

      </main>

      {/* Footer Line */}
      <footer className="max-w-7xl mx-auto px-6 md:px-8 pb-12 mt-auto">
        <div className="border-t border-black/10 pt-8 flex flex-col md:flex-row justify-between items-center gap-4">
          {/* ... reuse footer content or leave simple ... */}
        </div>
      </footer>

    </div>
  );
}

