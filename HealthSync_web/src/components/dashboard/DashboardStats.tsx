import { useNavigate, Link } from "react-router-dom";
import { Utensils, Dumbbell, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";

interface DashboardStatsProps {
    todayStats: {
        caloriesConsumed: number;
        caloriesTarget: number;
        workoutDuration: string;
    };
}

export default function DashboardStats({ todayStats }: Readonly<DashboardStatsProps>) {
    const navigate = useNavigate();

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {/* Nutrition Card */}
            <div className="group bg-[#EBE9C0]/50 rounded-[2.5rem] p-8 flex flex-col items-center justify-between min-h-[340px] relative border border-white/40 backdrop-blur-sm hover:bg-[#EBE9C0]/80 transition-all duration-300">
                <div className="w-full flex justify-between items-center mb-4">
                    <div>
                        <p className="text-center font-bold text-gray-800 mb-1 text-lg uppercase tracking-wide">Dinh dưỡng</p>
                        <p className="text-center text-sm text-gray-500">Theo dõi calo nạp vào</p>
                    </div>
                    <Link to="/nutrition-overview" className="text-sm font-semibold text-[#4A6F6F] hover:underline flex items-center gap-1">
                        Chi tiết <ChevronRight className="w-4 h-4" />
                    </Link>
                </div>

                {/* Visual Representation */}
                <div className="flex w-full items-center justify-center gap-6 mb-6">
                    <div className="relative w-32 h-32 flex items-center justify-center bg-[#ffab91] rounded-full text-center p-2 shadow-xl border-4 border-[#FDFBD4] group-hover:rotate-6 transition-transform duration-500">
                        <div className="text-white flex flex-col items-center">
                            <span className="text-3xl font-black">{Math.round(todayStats.caloriesConsumed || 0)}</span>
                            <span className="text-[10px] font-bold uppercase tracking-wider">/ {todayStats.caloriesTarget || 2000} kcal</span>
                        </div>
                    </div>
                </div>

                <div className="flex gap-3 w-full max-w-[280px]">
                    <Button
                        className="rounded-full bg-[#FDFBD4] text-[#2d2d2d] hover:bg-[#2d2d2d] hover:text-[#FDFBD4] border-2 border-[#2d2d2d] px-8 py-6 text-lg font-bold flex-1 flex items-center justify-center gap-3 transition-all hover:scale-105 shadow-sm"
                        onClick={() => navigate('/nutrition')}
                    >
                        <Utensils className="w-5 h-5" />
                        Ghi bữa ăn
                    </Button>
                    <button
                        className="rounded-full border-2 border-[#4A6F6F] bg-white text-[#4A6F6F] hover:bg-[#4A6F6F] hover:text-white w-14 h-14 flex items-center justify-center transition-all hover:scale-110 shadow-md cursor-pointer font-bold text-xl"
                        onClick={() => navigate('/nutrition-history')}
                        aria-label="Lịch sử dinh dưỡng"
                        type="button"
                    >
                        📊
                    </button>
                </div>
            </div>

            {/* Workout Card */}
            <div className="group bg-[#EBE9C0]/50 rounded-[2.5rem] p-8 flex flex-col items-center justify-between min-h-[340px] relative border border-white/40 backdrop-blur-sm hover:bg-[#EBE9C0]/80 transition-all duration-300">
                <div className="w-full flex justify-between items-center mb-4">
                    <div>
                        <p className="text-center font-bold text-gray-800 mb-1 text-lg uppercase tracking-wide">Luyện tập</p>
                        <p className="text-center text-sm text-gray-500">Theo dõi vận động</p>
                    </div>
                    <Link to="/workout-history" className="text-sm font-semibold text-[#4A6F6F] hover:underline flex items-center gap-1">
                        Lịch sử <ChevronRight className="w-4 h-4" />
                    </Link>
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
    );
}
