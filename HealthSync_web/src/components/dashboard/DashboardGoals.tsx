import { Link, useNavigate } from "react-router-dom";
import { Activity, ChevronRight, BarChart3, ChevronDown, TrendingDown, TrendingUp } from "lucide-react";
import { Button } from "@/components/ui/button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { GoalSummary } from "@/services/dashboardService";

interface DashboardGoalsProps {
    goalProgress: any;
    activeGoals: GoalSummary[];
    weightProgress: any;
    selectedGoalId: number | null;
    setSelectedGoalId: (id: number | null) => void;
    selectedGoalDetails: any;
}

export default function DashboardGoals({
    goalProgress,
    activeGoals,
    weightProgress,
    selectedGoalId,
    setSelectedGoalId,
    selectedGoalDetails
}: Readonly<DashboardGoalsProps>) {
    const navigate = useNavigate();

    // Helper để lấy tên hiển thị của goal type
    const getGoalTypeDisplay = (type: string) => {
        const types: Record<string, string> = {
            'weight_loss': 'Giảm cân',
            'weight_gain': 'Tăng cân',
            'muscle_gain': 'Tăng cơ',
            'fat_loss': 'Giảm mỡ',
        };
        return types[type] || type;
    };

    // Lấy goal đã chọn hoặc goal mặc định
    const displayedGoal = selectedGoalId && activeGoals?.length > 0
        ? activeGoals.find((g: GoalSummary) => g.goalId === selectedGoalId)
        : null;

    // Helper: Get consolidated goal data from selectedGoalDetails or goalProgress
    const getGoalData = () => {
        const source = selectedGoalDetails || goalProgress;
        if (!source) return null;
        return {
            goalType: source.goalType || '',
            targetValue: source.targetValue || 0,
            startValue: source.startValue || 0,
            progressAmount: source.progressAmount || 0,
            remaining: source.remaining || 0,
        };
    };

    // Helper: Check if goal is a decrease goal
    const isDecreaseGoal = (goalData: any) => {
        if (!goalData) return false;
        const type = goalData.goalType.toLowerCase();
        return type.includes('loss') || type.includes('giảm') || goalData.targetValue < goalData.startValue;
    };

    // Helper: Get direction text
    const getGoalDirection = (goalData: any, prefix: boolean = false) => {
        const isDecrease = isDecreaseGoal(goalData);
        if (prefix) return isDecrease ? 'Đã giảm' : 'Đã tăng';
        return isDecrease ? 'Giảm' : 'Tăng';
    };

    // Helper: Calculate total weight change
    const getTotalChange = (goalData: any) => {
        if (!goalData) return 0;
        return Math.abs(goalData.startValue - goalData.targetValue);
    };

    const currentGoalData = getGoalData();

    return (
        <section className="bg-white/30 rounded-[2.5rem] p-6 md:p-8 mb-8 shadow-sm border border-white/50 backdrop-blur-sm">
            <div className="flex items-center justify-between mb-4">
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

            {/* Goal Selector */}
            {activeGoals && activeGoals.length > 1 && (
                <div className="mb-6">
                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Button variant="outline" className="w-full md:w-auto justify-between rounded-xl border-gray-300 hover:bg-gray-50">
                                <span className="flex items-center gap-2">
                                    <BarChart3 className="w-4 h-4" />
                                    {displayedGoal ? getGoalTypeDisplay(displayedGoal.type) : 'Chọn mục tiêu để xem'}
                                </span>
                                <ChevronDown className="w-4 h-4 ml-2" />
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="start" className="w-[250px]">
                            {activeGoals.map((goal: GoalSummary) => (
                                <DropdownMenuItem
                                    key={goal.goalId}
                                    onClick={() => setSelectedGoalId(goal.goalId)}
                                    className="cursor-pointer"
                                >
                                    <div className="flex flex-col w-full">
                                        <div className="flex items-center justify-between">
                                            <span className="font-medium">{getGoalTypeDisplay(goal.type)}</span>
                                            <span className="text-xs text-gray-500">{goal.progress.toFixed(0)}%</span>
                                        </div>
                                        {goal.notes && (
                                            <span className="text-xs text-gray-500 truncate">{goal.notes}</span>
                                        )}
                                    </div>
                                </DropdownMenuItem>
                            ))}
                        </DropdownMenuContent>
                    </DropdownMenu>
                </div>
            )}

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                {/* Main Goal Card */}
                <div className="bg-[#D9D7B6]/80 rounded-[2rem] p-6 flex flex-col justify-between min-h-[240px] shadow-sm hover:shadow-md transition-all duration-300 hover:-translate-y-1">
                    <p className="text-sm font-semibold opacity-60 uppercase tracking-wider text-[#3d3d3d]">Mục tiêu chính</p>
                    <div className="flex-1 flex flex-col items-center justify-center">
                        <div className="text-center">
                            <p className="text-5xl font-black mb-1 text-[#2d2d2d] leading-none tracking-tighter">
                                {currentGoalData ? getGoalDirection(currentGoalData) : '---'}
                            </p>
                            <p className="text-4xl font-extrabold text-[#2d2d2d]/90 tracking-tight">
                                {currentGoalData ? `${getTotalChange(currentGoalData).toFixed(1)}kg` : ''}
                            </p>
                        </div>
                    </div>
                    <div className="text-center opacity-60 text-xs font-medium">Kế hoạch dài hạn</div>
                </div>

                {/* Progress Card */}
                <div className="bg-[#D9D7B6]/80 rounded-[2rem] p-6 flex flex-col justify-between min-h-[240px] shadow-sm hover:shadow-md transition-all duration-300 hover:-translate-y-1">
                    <p className="text-sm font-semibold opacity-60 uppercase tracking-wider text-[#3d3d3d]">Tiến độ hiện tại</p>
                    <div className="flex-1 flex flex-col items-center justify-center">
                        {currentGoalData ? (
                            <div className="text-center">
                                <p className="text-2xl font-semibold mb-3 text-[#2d2d2d]">
                                    {getGoalDirection(currentGoalData, true)} <span className="font-black text-4xl block mt-1">{currentGoalData.progressAmount.toFixed(1)}<span className="text-2xl font-bold text-[#2d2d2d]/60">kg</span></span>
                                </p>
                                <div className="inline-flex items-center gap-2 bg-black/5 px-3 py-1 rounded-full">
                                    {isDecreaseGoal(currentGoalData) ? (
                                        <TrendingDown className="w-4 h-4 text-[#4A6F6F]" />
                                    ) : (
                                        <TrendingUp className="w-4 h-4 text-[#4A6F6F]" />
                                    )}
                                    <p className="text-sm font-medium text-[#2d2d2d]">
                                        Còn <span className="font-bold">{Math.abs(currentGoalData.remaining).toFixed(1)}kg</span>
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
                                {weightProgress.weightHistory.slice(-7).map((point: any, i: number, arr: any[]) => {
                                    const weights = arr.map((p) => p.weight);
                                    const min = Math.min(...weights) * 0.99; // slightly lower buffer
                                    const max = Math.max(...weights) * 1.01;
                                    const range = max - min || 1;
                                    // Calculate height percentage (min 20%, max 100%)
                                    const heightPercent = 20 + ((point.weight - min) / range) * 80;

                                    return (
                                        <div key={point.date || point.id || i} className="flex flex-col items-center justify-end h-full w-full group relative">
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
        </section>
    );
}
