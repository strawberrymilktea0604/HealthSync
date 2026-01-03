import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { dashboardService, CustomerDashboard } from "@/services/dashboardService";
import Header from "@/components/Header";
import { Loader2, Utensils, Dumbbell, X, Bot } from "lucide-react";
import { Button } from "@/components/ui/button";
import { format } from "date-fns";
import { motion, AnimatePresence } from "framer-motion";

export default function Dashboard() {
  const [dashboard, setDashboard] = useState<CustomerDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showChat, setShowChat] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    loadDashboard();
  }, []);

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
        <Loader2 className="w-12 h-12 animate-spin text-black" />
      </div>
    );
  }

  if (error || !dashboard) {
    return (
      <div className="min-h-screen bg-[#FDFBD4] flex flex-col items-center justify-center p-4">
        <p className="text-red-500 mb-4">{error || "No data available"}</p>
        <Button onClick={loadDashboard}>Retry</Button>
      </div>
    );
  }

  const { userInfo, goalProgress, weightProgress, todayStats } = dashboard;
  const today = new Date();

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans">
      <Header />

      <main className="max-w-[1434px] mx-auto px-4 md:px-8 lg:px-12 xl:px-16 pb-12">

        {/* Welcome Section */}
        <div className="text-center mb-8 pt-4">
          <h2 className="text-xl text-gray-600 mb-1">Welcome to <span className="font-bold text-black">healthsync</span></h2>
          <p className="text-sm text-gray-500 mb-4">{format(today, "Today, d MMM")}</p>
          <h1 className="text-3xl md:text-4xl font-bold mb-4">Your Activities</h1>

          <div className="flex items-center justify-center gap-2 mb-8">
            <div className="w-8 h-8 rounded-full overflow-hidden bg-gray-200">
              <img
                src={userInfo.avatarUrl || `https://ui-avatars.com/api/?name=${userInfo.fullName}`}
                alt="avatar"
                className="w-full h-full object-cover"
              />
            </div>
            <span className="font-medium">{userInfo.fullName}</span>
          </div>
        </div>

        {/* Goals Section */}
        <div className="bg-[#EBE9C0] rounded-[3rem] p-6 mb-8 shadow-sm">
          <h3 className="text-center font-medium mb-6">Mục tiêu</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">

            {/* Main Goal Card */}
            <div className="bg-[#D9D7B6] rounded-[2rem] p-6 flex flex-col items-center justify-center aspect-square md:aspect-auto h-full min-h-[200px] shadow-inner">
              <p className="text-sm font-medium opacity-70 mb-2">Mục tiêu chính</p>
              <div className="text-center">
                <p className="text-5xl font-bold mb-1">
                  {goalProgress ? 'Giảm' : 'Chưa có'}
                </p>
                <p className="text-4xl font-bold">
                  {goalProgress ? `${goalProgress.targetValue - (goalProgress.startValue || 0)}kg` : ''}
                  {/* Note: Logic here assumes 'Giảm' means diff. Adjust based on actual data structure if needed */}
                </p>
              </div>
            </div>

            {/* Progress Card */}
            <div className="bg-[#D9D7B6] rounded-[2rem] p-6 flex flex-col items-center justify-center aspect-square md:aspect-auto h-full min-h-[200px] shadow-inner">
              <p className="text-sm font-medium opacity-70 mb-2">Tiến độ</p>
              {goalProgress ? (
                <div className="text-center">
                  <p className="text-2xl font-medium mb-1">
                    Đã giảm <span className="font-bold">{goalProgress.progress.toFixed(1)}kg</span>
                  </p>
                  <p className="text-xl text-gray-600">
                    còn <span className="font-bold text-black">{goalProgress.remaining.toFixed(1)}kg</span> nữa
                  </p>
                </div>
              ) : (
                <p className="text-gray-500">Thiết lập mục tiêu để theo dõi</p>
              )}
            </div>

            {/* Chart Card */}
            <div className="bg-[#D9D7B6] rounded-[2rem] p-4 flex items-center justify-center shadow-inner overflow-hidden relative min-h-[200px]">
              {weightProgress?.weightHistory && weightProgress.weightHistory.length > 0 ? (
                <>
                  <div className="w-full h-24 flex items-end justify-between px-4 gap-1">
                    {weightProgress.weightHistory.slice(-10).map((point: { weight: number, date: string }, i: number, arr: { weight: number, date: string }[]) => {
                      // Normalize height based on min/max weight in the set
                      const weights = arr.map(p => p.weight);
                      const min = Math.min(...weights);
                      const max = Math.max(...weights);
                      const range = max - min || 1; // Prevent divide by zero
                      const heightPercent = 20 + ((point.weight - min) / range) * 60; // 20% to 80%

                      return (
                        <div
                          key={i}
                          className="w-2 bg-purple-500/50 rounded-full transition-all duration-500"
                          style={{ height: `${heightPercent}%` }}
                          title={`${point.weight}kg on ${new Date(point.date).toLocaleDateString()}`}
                        ></div>
                      );
                    })}
                  </div>
                  {/* Simple Svg Curve overlay for effect */}
                  <svg className="absolute inset-0 w-full h-full pointer-events-none opacity-30" viewBox="0 0 100 100" preserveAspectRatio="none">
                    <path d="M0,80 Q50,20 100,60" fill="none" stroke="#A78BFA" strokeWidth="2" />
                  </svg>
                </>
              ) : (
                <p className="text-gray-500 text-sm">Chưa có dữ liệu cân nặng</p>
              )}
            </div>
          </div>
        </div>

        {/* Bottom Section: Nutrition & Workout */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

          {/* Nutrition Card */}
          <div className="bg-[#EBE9C0] rounded-[3rem] p-8 flex flex-col items-center justify-between min-h-[300px] relative">
            <p className="text-center font-medium mb-4">Dinh dưỡng</p>

            {/* Bar Chart Icon Representation */}
            <div className="flex items-end justify-center gap-4 h-32 mb-8">
              <div className="w-8 h-16 bg-black rounded-lg border-2 border-black"></div>
              <div className="w-8 h-24 bg-transparent border-4 border-black rounded-lg"></div>
              <div className="w-8 h-20 bg-transparent border-4 border-black rounded-lg"></div>
            </div>

            <Button
              variant="outline"
              className="rounded-full border-black bg-[#FDFBD4] hover:bg-[#FDFBD4]/80 text-black px-8 py-6 text-lg w-full max-w-[200px] flex items-center justify-center gap-2 transition-transform hover:scale-105"
              onClick={() => navigate('/nutrition')}
            >
              <Utensils className="w-5 h-5" />
              Ghi bữa ăn
            </Button>
          </div>

          {/* Workout Card */}
          <div className="bg-[#EBE9C0] rounded-[3rem] p-8 flex flex-col items-center justify-between min-h-[300px]">
            <p className="text-center font-medium mb-4">Luyện tập</p>

            <div className="flex w-full items-center justify-between px-4 mb-4">
              <div className="text-left">
                <h4 className="text-xl font-bold leading-tight">Tổng thời gian<br />tập tuần này</h4>
              </div>
              <div className="relative w-24 h-24 flex items-center justify-center bg-green-500 rounded-full text-center p-2 shadow-lg">
                <div className="text-xs font-bold leading-tight">
                  <span className="text-lg">25/25</span><br />giờ
                </div>
              </div>
            </div>

            <Button
              variant="outline"
              className="rounded-full border-black bg-[#FDFBD4] hover:bg-[#FDFBD4]/80 text-black px-8 py-6 text-lg w-full max-w-[200px] flex items-center justify-center gap-2 transition-transform hover:scale-105"
              onClick={() => navigate('/create-workout')}
            >
              <Dumbbell className="w-5 h-5" />
              Ghi buổi tập
            </Button>
          </div>
        </div>

      </main>

      {/* Footer (Simplified matching image bottom lines) */}
      <footer className="max-w-[1434px] mx-auto px-16 pb-12">
        <div className="border-t border-black/20 pt-8 flex flex-wrap justify-between items-start gap-8">
          <div>
            <h3 className="font-bold text-xl mb-4">healthsync</h3>
            <p className="text-xs text-black/50">© healthsync 2025</p>
          </div>

          <div className="flex gap-12 text-xs font-bold uppercase tracking-wider">
            <div className="space-y-4">
              <p>Inspiration</p>
              <p className="opacity-50 font-normal normal-case">Term & Conditions</p>
            </div>
            <div className="space-y-4">
              <p>Support</p>
              <p className="opacity-50 font-normal normal-case">Cookies</p>
            </div>
            <div className="space-y-4">
              <p>About</p>
              <p className="opacity-50 font-normal normal-case">Freelancers</p>
            </div>
            <div className="space-y-4">
              <p>Blog</p>
              <p className="opacity-50 font-normal normal-case">Resources</p>
            </div>
            <div className="space-y-4">
              <p>PTs</p>
              <p className="opacity-50 font-normal normal-case">Tags</p>
            </div>
          </div>

          <div className="flex gap-2">
            {/* Social Icons Placeholder */}
            <div className="w-8 h-8 border border-black rounded-lg flex items-center justify-center hover:bg-black hover:text-white transition-colors cursor-pointer">
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true"><path d="M8.29 20.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0022 5.92a8.19 8.19 0 01-2.357.646 4.118 4.118 0 001.804-2.27 8.224 8.224 0 01-2.605.996 4.107 4.107 0 00-6.993 3.743 11.65 11.65 0 01-8.457-4.287 4.106 4.106 0 001.27 5.477A4.072 4.072 0 012.8 9.713v.052a4.105 4.105 0 003.292 4.022 4.095 4.095 0 01-1.853.07 4.108 4.108 0 003.834 2.85A8.233 8.233 0 012 18.407a11.616 11.616 0 006.29 1.84" /></svg>
            </div>
            <div className="w-8 h-8 border border-black rounded-lg flex items-center justify-center hover:bg-black hover:text-white transition-colors cursor-pointer">
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true"><path fillRule="evenodd" d="M22 12c0-5.523-4.477-10-10-10S2 6.477 2 12c0 4.991 3.657 9.128 8.438 9.878v-6.987h-2.54V12h2.54V9.797c0-2.506 1.492-3.89 3.777-3.89 1.094 0 2.238.195 2.238.195v2.46h-1.26c-1.243 0-1.63.771-1.63 1.562V12h2.773l-.443 2.89h-2.33v6.988C18.343 21.128 22 16.991 22 12z" clipRule="evenodd" /></svg>
            </div>
            <div className="w-8 h-8 border border-black rounded-lg flex items-center justify-center hover:bg-black hover:text-white transition-colors cursor-pointer">
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true"><path fillRule="evenodd" d="M12.315 2c2.43 0 2.784.013 3.808.06 1.064.049 1.791.218 2.427.465a4.902 4.902 0 011.772 1.153 4.902 4.902 0 011.153 1.772c.247.636.416 1.363.465 2.427.048 1.067.06 1.407.06 4.123v.08c0 2.643-.012 2.987-.06 4.043-.049 1.064-.218 1.791-.465 2.427a4.902 4.902 0 01-1.153 1.772 4.902 4.902 0 01-1.772 1.153c-.636.247-1.363.416-2.427.465-1.067.048-1.407.06-4.123.06h-.08c-2.643 0-2.987-.012-4.043-.06-1.064-.049-1.791-.218-2.427-.465a4.902 4.902 0 01-1.772-1.153 4.902 4.902 0 01-1.153-1.772c-.247-.636-.416-1.363-.465-2.427-.047-1.024-.06-1.379-.06-3.808v-.63c0-2.43.013-2.784.06-3.808.049-1.064.218-1.791.465-2.427a4.902 4.902 0 011.153-1.772 4.902 4.902 0 011.772-1.153c.636-.247 1.363-.416 2.427-.465 1.067-.047 1.407-.06 4.123-.06h.08zm-1.848 3.7c-3.125 0-5.632 2.507-5.632 5.632 0 3.125 2.507 5.632 5.632 5.632 3.125 0 5.632-2.507 5.632-5.632 0-3.125-2.507-5.632-5.632-5.632zm0 8.018a2.386 2.386 0 110-4.772 2.386 2.386 0 010 4.772zm5.722-7.072a1.082 1.082 0 11-2.164 0 1.082 1.082 0 012.164 0z" clipRule="evenodd" /></svg>
            </div>
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
              className="absolute bottom-20 right-0 w-80 h-96 bg-white rounded-2xl shadow-xl overflow-hidden border border-gray-200"
            >
              <div className="bg-[#EBE9C0] p-4 flex justify-between items-center">
                <span className="font-bold">Chat Assistant</span>
                <button onClick={() => setShowChat(false)}><X className="w-5 h-5" /></button>
              </div>
              <div className="p-4 h-full flex items-center justify-center text-gray-400">
                Chat interface coming soon...
              </div>
            </motion.div>
          )}
        </AnimatePresence>
        <motion.button
          whileHover={{ scale: 1.1 }}
          whileTap={{ scale: 0.9 }}
          onClick={() => setShowChat(!showChat)}
          className="w-16 h-16 bg-[#FDFBD4] rounded-2xl shadow-lg border-2 border-white flex items-center justify-center relative overflow-hidden group"
        >
          <div className="absolute inset-0 bg-blue-100 opacity-0 group-hover:opacity-20 transition-opacity"></div>
          {/* Robot Icon similar to image */}
          <Bot className="w-10 h-10 text-blue-600" />
        </motion.button>
      </div>

    </div>
  );
}
