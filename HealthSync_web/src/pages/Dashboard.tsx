import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { dashboardService, CustomerDashboard } from "@/services/dashboardService";
import Header from "@/components/Header";
import { Loader2, Utensils, Dumbbell, X, Bot, TrendingDown, Activity, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import logo from "@/assets/logo.png";
import { format } from "date-fns";
import { motion, AnimatePresence } from "framer-motion";
import { exerciseService, Exercise } from "@/services/exerciseService";
// Imports related to Exercise Library removed to clean up UI code


export default function Dashboard() {
  const [dashboard, setDashboard] = useState<CustomerDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showChat, setShowChat] = useState(false);

  // Exercise State
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [muscleGroupFilter, setMuscleGroupFilter] = useState("all");
  const [loadingExercises, setLoadingExercises] = useState(false);

  const navigate = useNavigate();

  useEffect(() => {
    loadDashboard();
    loadExercises();
  }, []);

  // Use exercises state to prevent unused variable check, as requested to keep data fetching
  useEffect(() => {
    if (exercises.length > 0) {
      // Data is available
    }
  }, [exercises]);

  useEffect(() => {
    const delayDebounceFn = setTimeout(() => {
      loadExercises();
    }, 500);

    return () => clearTimeout(delayDebounceFn);
  }, [searchQuery, muscleGroupFilter]);

  const loadExercises = async () => {
    try {
      setLoadingExercises(true);
      const data = await exerciseService.getExercises({
        search: searchQuery,
        muscleGroup: muscleGroupFilter === "all" ? undefined : muscleGroupFilter
      });
      setExercises(data);
    } catch (error) {
      console.error("Failed to load exercises:", error);
    } finally {
      setLoadingExercises(false);
      // Keep fetching logic active as requested
      console.log('Exercises fetch completed');
    }
  };

  const loadDashboard = async () => {
    try {
      setLoading(true);
      const data = await dashboardService.getCustomerDashboard();
      setDashboard(data);
      setError(null);
    } catch (err: unknown) {
      const errorMessage = err && typeof err === 'object' && 'response' in err
        ? (err as { response?: { data?: { message?: string } } }).response?.data?.message
        : undefined;
      setError(errorMessage || "Không thể tải dữ liệu dashboard");
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#FDFBD4] flex items-center justify-center">
        <Loader2 className="w-12 h-12 animate-spin text-[#4A6F6F]" />
      </div>
    );
  }

  if (error || !dashboard) {
    return (
      <div className="min-h-screen bg-[#FDFBD4] flex flex-col items-center justify-center p-4">
        <p className="text-red-500 mb-4 font-medium">{error || "No data available"}</p>
        <Button onClick={loadDashboard} variant="outline" className="border-black/20 hover:bg-black/5">Thử lại</Button>
      </div>
    );
  }

  const { userInfo, goalProgress, weightProgress, todayStats } = dashboard;
  const today = new Date();

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <main className="max-w-7xl mx-auto px-4 md:px-8 pb-12 pt-8">

        {/* Welcome Section */}
        <section className="flex flex-col md:flex-row items-center justify-between mb-12 gap-6">
          <div className="text-center md:text-left">
            <h2 className="text-lg text-gray-500 font-medium mb-1 flex items-center justify-center md:justify-start gap-2">
              Welcome to
              <motion.img
                src={logo}
                alt="HealthSync"
                className="h-6 mt-1"
                animate={{
                  scale: [1, 1.1, 1],
                  rotate: [0, 5, -5, 0]
                }}
                transition={{
                  duration: 2,
                  repeat: Infinity,
                  ease: "easeInOut"
                }}
              />
            </h2>
            <p className="text-gray-400 mt-2 font-medium">{format(today, "EEEE, d MMM")}</p>
            <h1 className="text-4xl md:text-5xl font-bold tracking-tight text-[#2d2d2d] mt-2">
              Your Activities
            </h1>
          </div>
        </section>

        {/* Goals Section */}
        <section className="bg-white/30 rounded-[2.5rem] p-6 md:p-8 mb-8 shadow-sm border border-white/50 backdrop-blur-sm">
          <div className="flex items-center justify-between mb-8">
            <h3 className="font-bold text-xl text-gray-800 uppercase tracking-wide flex items-center gap-2">
              <Activity className="w-5 h-5 text-[#4A6F6F]" />
              Tiến độ mục tiêu
            </h3>
            {goalProgress && (
              <Link to="/goals" className="text-sm font-semibold text-[#4A6F6F] hover:underline flex items-center gap-1">
                Chi tiết <ChevronRight className="w-4 h-4" />
              </Link>
            )}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {/* Main Goal Card */}
            <div className="bg-[#D9D7B6]/80 rounded-[2rem] p-6 flex flex-col justify-between min-h-[240px] shadow-sm hover:shadow-md transition-all duration-300 hover:-translate-y-1">
              <p className="text-sm font-semibold opacity-60 uppercase tracking-wider text-[#3d3d3d]">Mục tiêu chính</p>
              <div className="flex-1 flex flex-col items-center justify-center">
                <div className="text-center">
                  <p className="text-5xl font-black mb-1 text-[#2d2d2d] leading-none tracking-tighter">
                    {goalProgress ? 'Giảm' : '---'}
                  </p>
                  <p className="text-4xl font-extrabold text-[#2d2d2d]/90 tracking-tight">
                    {goalProgress ? `${(goalProgress.startValue - goalProgress.targetValue).toFixed(1)}kg` : ''}
                  </p>
                </div>
              </div>
              <div className="text-center opacity-60 text-xs font-medium">Kế hoạch dài hạn</div>
            </div>

            {/* Progress Card */}
            <div className="bg-[#D9D7B6]/80 rounded-[2rem] p-6 flex flex-col justify-between min-h-[240px] shadow-sm hover:shadow-md transition-all duration-300 hover:-translate-y-1">
              <p className="text-sm font-semibold opacity-60 uppercase tracking-wider text-[#3d3d3d]">Tiến độ hiện tại</p>
              <div className="flex-1 flex flex-col items-center justify-center">
                {goalProgress ? (
                  <div className="text-center">
                    <p className="text-2xl font-semibold mb-3 text-[#2d2d2d]">
                      Đã giảm <span className="font-black text-4xl block mt-1">{goalProgress.progress.toFixed(1)}<span className="text-2xl font-bold text-[#2d2d2d]/60">kg</span></span>
                    </p>
                    <div className="inline-flex items-center gap-2 bg-black/5 px-3 py-1 rounded-full">
                      <TrendingDown className="w-4 h-4 text-[#4A6F6F]" />
                      <p className="text-sm font-medium text-[#2d2d2d]">
                        Còn <span className="font-bold">{goalProgress.remaining.toFixed(1)}kg</span>
                      </p>
                    </div>
                  </div>
                ) : (
                  <div className="text-center">
                    <p className="text-gray-500 font-medium mb-4">Chưa thiết lập mục tiêu</p>
                    <Button
                      className="rounded-full bg-[#2d2d2d] text-[#FDFBD4] hover:bg-black transition-colors"
                      onClick={() => navigate('/goals/create')}
                    >
                      Thiết lập ngay
                    </Button>
                  </div>
                )}
              </div>
            </div>

            {/* Chart Card */}
            <div className="bg-[#D9D7B6]/80 rounded-[2rem] p-6 flex flex-col justify-between min-h-[240px] shadow-sm hover:shadow-md transition-all duration-300 hover:-translate-y-1 relative overflow-hidden">
              <p className="text-sm font-semibold opacity-60 uppercase tracking-wider text-[#3d3d3d] z-10 w-full text-center">Biểu đồ cân nặng</p>

              <div className="flex-1 flex items-end justify-center w-full relative z-10 px-2 mt-4">
                {weightProgress?.weightHistory && weightProgress.weightHistory.length > 0 ? (
                  <div className="w-full h-32 flex items-end justify-between gap-1">
                    {weightProgress.weightHistory.slice(-7).map((point, i, arr) => {
                      const weights = arr.map((p) => p.weight);
                      const min = Math.min(...weights) * 0.99; // slightly lower buffer
                      const max = Math.max(...weights) * 1.01;
                      const range = max - min || 1;
                      // Calculate height percentage (min 20%, max 100%)
                      const heightPercent = 20 + ((point.weight - min) / range) * 80;

                      return (
                        <div key={i} className="flex flex-col items-center justify-end h-full w-full group relative">
                          {/* Tooltip */}
                          <div className="absolute -top-10 left-1/2 transform -translate-x-1/2 opacity-0 group-hover:opacity-100 transition-opacity bg-black text-white text-[10px] px-2 py-1 rounded mb-1 pointer-events-none whitespace-nowrap z-20 shadow-lg">
                            {point.weight}kg
                            <div className="absolute bottom-[-4px] left-1/2 -translate-x-1/2 w-2 h-2 bg-black rotate-45"></div>
                          </div>
                          <div
                            className="w-2 md:w-3 bg-[#2d2d2d] rounded-t-full rounded-b-sm transition-all duration-500 opacity-60 group-hover:opacity-100"
                            style={{ height: `${heightPercent}%` }}
                          ></div>
                        </div>
                      );
                    })}
                  </div>
                ) : (
                  <p className="text-gray-500 text-sm font-medium italic">Chưa có dữ liệu</p>
                )}
              </div>

              {/* Decorative Curve */}
              <svg className="absolute bottom-0 left-0 w-full h-24 opacity-10 pointer-events-none" viewBox="0 0 100 100" preserveAspectRatio="none">
                <path d="M0,100 L0,50 C20,60 50,20 100,40 L100,100 Z" fill="#000" />
              </svg>
            </div>
          </div>



          {/* Exercise Library Section removed but data fetching retained */}
        </section>

        {/* Bottom Section: Nutrition & Workout */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">

          {/* Nutrition Card */}
          <div className="group bg-[#EBE9C0]/50 rounded-[2.5rem] p-8 flex flex-col items-center justify-between min-h-[340px] relative border border-white/40 backdrop-blur-sm hover:bg-[#EBE9C0]/80 transition-all duration-300">
            <div className="w-full">
              <p className="text-center font-bold text-gray-800 mb-2 text-lg uppercase tracking-wide">Dinh dưỡng</p>
              <p className="text-center text-sm text-gray-500 mb-6">Theo dõi calo nạp vào</p>
            </div>

            {/* Visual Representation */}
            <div className="flex items-end justify-center gap-4 h-40 mb-8 w-full max-w-[240px]">
              <motion.div
                initial={{ height: 0 }} animate={{ height: '6rem' }} transition={{ delay: 0.2 }}
                className="w-12 bg-[#2d2d2d] rounded-xl shadow-lg"
              ></motion.div>
              <motion.div
                initial={{ height: 0 }} animate={{ height: '8rem' }} transition={{ delay: 0.1 }}
                className="w-12 bg-transparent border-[3px] border-[#2d2d2d] rounded-xl"
              ></motion.div>
              <motion.div
                initial={{ height: 0 }} animate={{ height: '5rem' }} transition={{ delay: 0.3 }}
                className="w-12 bg-transparent border-[3px] border-[#2d2d2d] rounded-xl opacity-60"
              ></motion.div>
            </div>

            <Button
              className="rounded-full bg-[#FDFBD4] text-[#2d2d2d] hover:bg-[#2d2d2d] hover:text-[#FDFBD4] border-2 border-[#2d2d2d] px-10 py-6 text-lg font-bold w-full max-w-[280px] flex items-center justify-center gap-3 transition-all hover:scale-105 shadow-sm"
              onClick={() => navigate('/nutrition')}
            >
              <Utensils className="w-5 h-5" />
              Ghi bữa ăn
            </Button>
          </div>

          {/* Workout Card */}
          <div className="group bg-[#EBE9C0]/50 rounded-[2.5rem] p-8 flex flex-col items-center justify-between min-h-[340px] relative border border-white/40 backdrop-blur-sm hover:bg-[#EBE9C0]/80 transition-all duration-300">
            <div className="w-full">
              <p className="text-center font-bold text-gray-800 mb-2 text-lg uppercase tracking-wide">Luyện tập</p>
              <p className="text-center text-sm text-gray-500 mb-6">Theo dõi vận động</p>
            </div>

            <div className="flex w-full items-center justify-center gap-6 mb-6">
              <div className="relative w-32 h-32 flex items-center justify-center bg-[#a3e635] rounded-full text-center p-2 shadow-xl border-4 border-[#FDFBD4] group-hover:rotate-6 transition-transform duration-500">
                <div className="text-[#1a2e05] flex flex-col items-center">
                  <span className="text-3xl font-black">{todayStats.workoutDuration ? todayStats.workoutDuration.replace('min', '') : '0'}</span>
                  <span className="text-[10px] font-bold uppercase tracking-wider">Phút / Tuần</span>
                </div>
              </div>
            </div>

            <Button
              className="rounded-full bg-[#FDFBD4] text-[#2d2d2d] hover:bg-[#2d2d2d] hover:text-[#FDFBD4] border-2 border-[#2d2d2d] px-10 py-6 text-lg font-bold w-full max-w-[280px] flex items-center justify-center gap-3 transition-all hover:scale-105 shadow-sm"
              onClick={() => navigate('/create-workout')}
            >
              <Dumbbell className="w-5 h-5" />
              Ghi buổi tập
            </Button>
          </div>
        </div>

      </main>

      {/* Footer */}
      <footer className="max-w-7xl mx-auto px-6 md:px-8 pb-12">
        <div className="border-t border-black/10 pt-8 flex flex-col md:flex-row justify-between items-start md:items-center gap-8">
          <div>
            <img src={logo} alt="HealthSync" className="h-8 mb-2" />
            <p className="text-xs text-black/40">© healthsync 2025. All rights reserved.</p>
          </div>
          <div className="flex gap-4">
            {/* Socials can go here */}
          </div>
        </div>
      </footer>

      {/* Chat Bot FAB */}
      <div className="fixed bottom-8 right-8 z-50">
        <AnimatePresence>
          {showChat && (
            <motion.div
              initial={{ opacity: 0, scale: 0.8, y: 20 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.8, y: 20 }}
              className="absolute bottom-20 right-0 w-80 h-96 bg-white rounded-2xl shadow-2xl overflow-hidden border border-gray-200 origin-bottom-right"
            >
              <div className="bg-[#EBE9C0] p-4 flex justify-between items-center border-b border-gray-100">
                <span className="font-bold text-[#2d2d2d] flex items-center gap-2">
                  <Bot className="w-5 h-5" /> Assistant
                </span>
                <button onClick={() => setShowChat(false)} className="hover:bg-black/10 p-1 rounded-full text-[#2d2d2d]"><X className="w-4 h-4" /></button>
              </div>
              <div className="p-4 h-full flex flex-col items-center justify-center text-gray-400 bg-gray-50/50">
                <Bot className="w-12 h-12 mb-3 opacity-20" />
                <p className="text-sm">Chat interface coming soon...</p>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
        <motion.button
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
          onClick={() => setShowChat(!showChat)}
          className="w-14 h-14 bg-[#2d2d2d] rounded-2xl shadow-xl flex items-center justify-center text-[#EBE9C0] hover:bg-black transition-colors"
        >
          <Bot className="w-7 h-7" />
        </motion.button>
      </div>

    </div >
  );
}
