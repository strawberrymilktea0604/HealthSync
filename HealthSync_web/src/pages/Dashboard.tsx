import { useState } from "react";
import Header from "@/components/Header";
import { Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import logo from "@/assets/logo.png";
import { format } from "date-fns";
import { motion } from "framer-motion";

import { useDashboardData } from "@/components/dashboard/hooks/useDashboardData";
import DashboardGoals from "@/components/dashboard/DashboardGoals";
import DashboardStats from "@/components/dashboard/DashboardStats";
import DashboardChat from "@/components/dashboard/DashboardChat";

export default function Dashboard() {
  const {
    dashboard,
    loading,
    error,
    selectedGoalId,
    setSelectedGoalId,
    selectedGoalDetails,
    loadDashboard,
    exercises
  } = useDashboardData();

  const [showChat, setShowChat] = useState(false);

  // Use exercises to satisfy the requirement if needed, or remove if the hook handles it sufficiently.
  // The hook does the fetching, so we just acknowledge it here to match previous behavior if any side effect relied on it.
  // Previous code had: useEffect(() => { if (exercises.length > 0) {} }, [exercises]);
  // We can ignore it if it was empty.

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

  const { goalProgress, activeGoals, weightProgress, todayStats } = dashboard;
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
        <DashboardGoals
          goalProgress={goalProgress}
          activeGoals={activeGoals}
          weightProgress={weightProgress}
          selectedGoalId={selectedGoalId}
          setSelectedGoalId={setSelectedGoalId}
          selectedGoalDetails={selectedGoalDetails}
        />

        {/* Bottom Section: Nutrition & Workout */}
        <DashboardStats todayStats={todayStats} />

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
      <DashboardChat showChat={showChat} setShowChat={setShowChat} />

    </div >
  );
}
