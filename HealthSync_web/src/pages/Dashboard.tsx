import { useEffect, useState, useRef } from "react";
import { Link, useNavigate } from "react-router-dom";
import { dashboardService, CustomerDashboard, GoalSummary } from "@/services/dashboardService";
import { goalService, Goal } from "@/services/goalService";
import Header from "@/components/Header";
import { Loader2, Utensils, Dumbbell, X, Bot, TrendingDown, TrendingUp, Activity, ChevronRight, BarChart3, ChevronDown, Send, User, MessageSquarePlus, History, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import logo from "@/assets/logo.png";
import { format } from "date-fns";
import { motion, AnimatePresence } from "framer-motion";
import { exerciseService, Exercise } from "@/services/exerciseService";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { chatService } from "@/services/chatService";
import { ChatMessage } from "@/types/chat";
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
// Imports related to Exercise Library removed to clean up UI code


export default function Dashboard() {
  const [dashboard, setDashboard] = useState<CustomerDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showChat, setShowChat] = useState(false);
  const [selectedGoalId, setSelectedGoalId] = useState<number | null>(null);
  const [selectedGoalDetails, setSelectedGoalDetails] = useState<any>(null);

  // Exercise State
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [muscleGroupFilter, setMuscleGroupFilter] = useState("all");
  const [loadingExercises, setLoadingExercises] = useState(false);

  // Chat State
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputMessage, setInputMessage] = useState("");
  const [isSending, setIsSending] = useState(false);
  const [loadingChat, setLoadingChat] = useState(false);
  const [showChatHistory, setShowChatHistory] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const navigate = useNavigate();

  useEffect(() => {
    loadDashboard();
    loadExercises();

    // Auto-refresh dashboard every 30 seconds
    const refreshInterval = setInterval(() => {
      loadDashboard();
    }, 30000); // 30 seconds

    return () => clearInterval(refreshInterval);
  }, []);

  // Load selected goal details when a goal is selected
  useEffect(() => {
    if (selectedGoalId) {
      loadSelectedGoalDetails(selectedGoalId);
    } else {
      setSelectedGoalDetails(null);
    }
  }, [selectedGoalId]);

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

  const loadSelectedGoalDetails = async (goalId: number) => {
    try {
      const goals = await goalService.getGoals();
      const goal = goals.find((g: Goal) => g.goalId === goalId);

      if (!goal) return;

      // Calculate progress from goal's progress records
      const sortedRecords = goal.progressRecords.sort((a, b) =>
        new Date(a.recordDate).getTime() - new Date(b.recordDate).getTime()
      );

      const firstRecord = sortedRecords[0];
      const latestRecord = sortedRecords[sortedRecords.length - 1];

      const startValue = firstRecord?.weightKg || firstRecord?.value || 0;
      const currentValue = latestRecord?.weightKg || latestRecord?.value || startValue;
      const targetValue = goal.targetValue;

      const isDecreaseGoal = goal.type.toLowerCase().includes('loss') ||
        goal.type.toLowerCase().includes('giảm') ||
        targetValue < startValue;

      const progressAmount = isDecreaseGoal
        ? startValue - currentValue
        : currentValue - startValue;

      const remaining = isDecreaseGoal
        ? currentValue - targetValue
        : targetValue - currentValue;

      const totalChange = Math.abs(targetValue - startValue);
      const progress = totalChange > 0 ? (Math.abs(progressAmount) / totalChange) * 100 : 0;

      setSelectedGoalDetails({
        goalType: goal.type,
        startValue,
        currentValue,
        targetValue,
        progress: Math.min(100, Math.max(0, progress)),
        progressAmount,
        remaining,
        status: goal.status
      });
    } catch (error) {
      console.error('Failed to load goal details:', error);
    }
  };

  // Chat functions
  useEffect(() => {
    if (showChat) {
      // Only load chat history if user is logged in
      const userJson = localStorage.getItem('user');
      if (userJson) {
        loadChatHistory();
      } else {
        console.warn('User not logged in - cannot load chat history');
      }
    }
  }, [showChat]);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const loadChatHistory = async () => {
    setLoadingChat(true);
    try {
      const history = await chatService.getChatHistory();
      setMessages(history);
    } catch (error: any) {
      console.error('Error loading chat history:', error);
      // Don't show error to user, just set empty messages
      if (error.response?.status === 401) {
        console.warn('User not authenticated for chat');
      }
      setMessages([]);
    } finally {
      setLoadingChat(false);
    }
  };

  const handleNewChat = () => {
    if (messages.length > 0) {
      const confirmNew = window.confirm('Bạn có muốn bắt đầu cuộc trò chuyện mới? Cuộc trò chuyện hiện tại sẽ được lưu vào lịch sử.');
      if (!confirmNew) return;
    }
    setMessages([]);
    setInputMessage('');
    setShowChatHistory(false);
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();

    const question = inputMessage.trim();
    if (!question || isSending) return;

    // Add user message immediately
    const userMessage: ChatMessage = {
      id: Date.now().toString(),
      role: 'user',
      content: question,
      createdAt: new Date().toISOString(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setInputMessage('');
    setIsSending(true);

    try {
      const response = await chatService.sendMessage(question);

      const aiMessage: ChatMessage = {
        id: response.messageId,
        role: 'assistant',
        content: response.response,
        createdAt: response.timestamp,
      };

      setMessages((prev) => [...prev, aiMessage]);
    } catch (error: any) {
      console.error('Error sending message:', error);

      // Add error message to chat
      const errorMessage: ChatMessage = {
        id: (Date.now() + 1).toString(),
        role: 'assistant',
        content: 'Xin lỗi, tôi không thể trả lời câu hỏi của bạn lúc này. Vui lòng thử lại sau.',
        createdAt: new Date().toISOString(),
      };
      setMessages((prev) => [...prev, errorMessage]);
    } finally {
      setIsSending(false);
    }
  };

  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
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

  const { userInfo, goalProgress, activeGoals, weightProgress, todayStats } = dashboard;
  const today = new Date();

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

  // Hàm tính toán chi tiết progress cho goal được chọn
  const calculateGoalProgress = (goalSummary: GoalSummary) => {
    if (!goalSummary) return null;

    // Lấy goal details từ activeGoals để tính toán
    const isDecreaseGoal = goalSummary.type.toLowerCase().includes('loss') ||
      goalSummary.type.toLowerCase().includes('giảm') ||
      goalSummary.targetValue < (goalSummary as any).startValue;

    // Nếu đã có cached details, dùng luôn
    if (selectedGoalDetails && selectedGoalDetails.goalId === goalSummary.goalId) {
      return selectedGoalDetails;
    }

    // Giả sử progress là % đã hoàn thành, tính ngược lại các giá trị
    // Đây là workaround vì API không trả về đầy đủ thông tin
    return {
      goalType: goalSummary.type,
      targetValue: goalSummary.targetValue,
      progress: goalSummary.progress,
      // Những giá trị này cần load từ API riêng hoặc cache
      startValue: 0,
      currentValue: 0,
      progressAmount: 0,
      remaining: 0
    };
  };

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

          {/* Goal Selector - Hiển thị nếu có nhiều mục tiêu */}
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
                    {(selectedGoalDetails || goalProgress) ? (
                      (selectedGoalDetails?.goalType || goalProgress?.goalType || '').toLowerCase().includes('loss') ||
                        (selectedGoalDetails?.goalType || goalProgress?.goalType || '').toLowerCase().includes('giảm') ||
                        ((selectedGoalDetails?.targetValue || goalProgress?.targetValue || 0) < (selectedGoalDetails?.startValue || goalProgress?.startValue || 0)) ? 'Giảm' : 'Tăng'
                    ) : '---'}
                  </p>
                  <p className="text-4xl font-extrabold text-[#2d2d2d]/90 tracking-tight">
                    {(selectedGoalDetails || goalProgress) ? `${Math.abs((selectedGoalDetails?.startValue || goalProgress?.startValue || 0) - (selectedGoalDetails?.targetValue || goalProgress?.targetValue || 0)).toFixed(1)}kg` : ''}
                  </p>
                </div>
              </div>
              <div className="text-center opacity-60 text-xs font-medium">Kế hoạch dài hạn</div>
            </div>

            {/* Progress Card */}
            <div className="bg-[#D9D7B6]/80 rounded-[2rem] p-6 flex flex-col justify-between min-h-[240px] shadow-sm hover:shadow-md transition-all duration-300 hover:-translate-y-1">
              <p className="text-sm font-semibold opacity-60 uppercase tracking-wider text-[#3d3d3d]">Tiến độ hiện tại</p>
              <div className="flex-1 flex flex-col items-center justify-center">
                {(selectedGoalDetails || goalProgress) ? (
                  <div className="text-center">
                    <p className="text-2xl font-semibold mb-3 text-[#2d2d2d]">
                      {(selectedGoalDetails?.goalType || goalProgress?.goalType || '').toLowerCase().includes('loss') ||
                        (selectedGoalDetails?.goalType || goalProgress?.goalType || '').toLowerCase().includes('giảm') ||
                        ((selectedGoalDetails?.targetValue || goalProgress?.targetValue || 0) < (selectedGoalDetails?.startValue || goalProgress?.startValue || 0)) ? 'Đã giảm' : 'Đã tăng'} <span className="font-black text-4xl block mt-1">{(selectedGoalDetails?.progressAmount || goalProgress?.progressAmount || 0).toFixed(1)}<span className="text-2xl font-bold text-[#2d2d2d]/60">kg</span></span>
                    </p>
                    <div className="inline-flex items-center gap-2 bg-black/5 px-3 py-1 rounded-full">
                      {(selectedGoalDetails?.goalType || goalProgress?.goalType || '').toLowerCase().includes('loss') ||
                        (selectedGoalDetails?.goalType || goalProgress?.goalType || '').toLowerCase().includes('giảm') ||
                        ((selectedGoalDetails?.targetValue || goalProgress?.targetValue || 0) < (selectedGoalDetails?.startValue || goalProgress?.startValue || 0)) ? (
                        <TrendingDown className="w-4 h-4 text-[#4A6F6F]" />
                      ) : (
                        <TrendingUp className="w-4 h-4 text-[#4A6F6F]" />
                      )}
                      <p className="text-sm font-medium text-[#2d2d2d]">
                        Còn <span className="font-bold">{Math.abs((selectedGoalDetails?.remaining || goalProgress?.remaining || 0)).toFixed(1)}kg</span>
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
              <div
                className="rounded-full border-2 border-[#4A6F6F] bg-white text-[#4A6F6F] hover:bg-[#4A6F6F] hover:text-white w-14 h-14 flex items-center justify-center transition-all hover:scale-110 shadow-md cursor-pointer font-bold text-xl"
                onClick={() => navigate('/nutrition-history')}
              >
                📊
              </div>
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
              initial={{ opacity: 0, scale: 0.9, y: 20 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.9, y: 20 }}
              transition={{ duration: 0.2 }}
              className="absolute bottom-20 right-0 w-[420px] h-[650px] bg-white rounded-3xl shadow-2xl overflow-hidden border border-gray-100 flex flex-col font-sans"
            >
              {/* Header */}
              <div className="bg-[#EBE9C0] px-6 py-4 flex justify-between items-center shadow-sm sticky top-0 z-10">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-[#2d2d2d] flex items-center justify-center shadow-sm">
                    <Bot className="w-6 h-6 text-[#EBE9C0]" />
                  </div>
                  <div>
                    <h3 className="font-bold text-[#2d2d2d] text-lg">Assistant</h3>
                    <p className="text-xs text-[#2d2d2d]/60 font-medium flex items-center gap-1.5">
                      <span className="w-2 h-2 bg-green-500 rounded-full animate-pulse shadow-[0_0_8px_rgba(34,197,94,0.6)]"></span>
                      Online
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-1">
                  <button
                    onClick={handleNewChat}
                    className="p-2.5 hover:bg-black/5 rounded-xl transition-colors text-[#2d2d2d]"
                    title="Mới"
                  >
                    <MessageSquarePlus className="w-7 h-7" />
                  </button>
                  <button
                    onClick={() => setShowChatHistory(!showChatHistory)}
                    className={`p-2.5 hover:bg-black/5 rounded-xl transition-colors text-[#2d2d2d] ${showChatHistory ? 'bg-black/10' : ''}`}
                    title="Lịch sử"
                  >
                    <History className="w-7 h-7" />
                  </button>
                  <button
                    onClick={() => setShowChat(false)}
                    className="p-2.5 hover:bg-red-500/10 hover:text-red-600 rounded-xl transition-colors text-[#2d2d2d]/60 ml-1"
                  >
                    <X className="w-8 h-8" />
                  </button>
                </div>
              </div>

              {/* Main Content Area */}
              <div className="flex-1 flex overflow-hidden bg-gray-50/30 relative">

                {/* Background Pattern */}
                <div className="absolute inset-0 opacity-[0.03] pointer-events-none"
                  style={{ backgroundImage: `radial-gradient(#2d2d2d 1px, transparent 1px)`, backgroundSize: '24px 24px' }}>
                </div>

                {/* Chat History Sidebar */}
                {showChatHistory && (
                  <motion.div
                    initial={{ width: 0, opacity: 0 }}
                    animate={{ width: 240, opacity: 1 }}
                    exit={{ width: 0, opacity: 0 }}
                    className="bg-white/95 backdrop-blur-md border-r border-gray-100 flex flex-col h-full overflow-hidden z-20 shadow-lg"
                  >
                    <div className="p-4 border-b border-gray-100 bg-gray-50/50">
                      <span className="text-xs font-bold text-gray-400 uppercase tracking-widest">Lịch sử</span>
                    </div>
                    <div className="flex-1 overflow-y-auto p-3 space-y-2">
                      {messages.length > 0 ? (
                        <>
                          <div className="p-3 bg-[#EBE9C0] rounded-xl border border-[#Dcdbb0] shadow-sm">
                            <div className="flex items-center gap-2 mb-1.5">
                              <div className="w-2 h-2 rounded-full bg-green-500 shadow-sm"></div>
                              <span className="text-sm font-bold text-[#2d2d2d]">Hiện tại</span>
                            </div>
                            <p className="text-xs text-[#2d2d2d]/70 truncate font-medium">
                              {messages[messages.length - 1]?.content || "Đang trò chuyện..."}
                            </p>
                          </div>

                          <div className="text-center py-8">
                            <History className="w-10 h-10 text-gray-200 mx-auto mb-3" />
                            <p className="text-xs font-medium text-gray-400">Không có lịch sử trước đó</p>
                          </div>
                        </>
                      ) : (
                        <div className="flex flex-col items-center justify-center h-40 text-gray-400">
                          <p className="text-xs font-medium">Chưa có dữ liệu</p>
                        </div>
                      )}
                    </div>
                  </motion.div>
                )}

                {/* Chat Messages */}
                <div className="flex-1 overflow-y-auto p-5 space-y-6 scrollbar-thin scrollbar-thumb-gray-200 scrollbar-track-transparent z-10">
                  {loadingChat ? (
                    <div className="flex justify-center items-center h-full">
                      <div className="p-4 bg-white rounded-2xl shadow-sm border border-gray-100">
                        <Loader2 className="w-8 h-8 animate-spin text-[#2d2d2d]" />
                      </div>
                    </div>
                  ) : messages.length === 0 ? (
                    <div className="flex flex-col items-center justify-center h-full text-gray-400 animate-in fade-in zoom-in duration-500">
                      <div className="w-24 h-24 rounded-[2rem] bg-gradient-to-br from-[#EBE9C0] to-[#E5E3B5] shadow-xl flex items-center justify-center mb-6 group transform transition-transform hover:scale-105 duration-300">
                        <Bot className="w-12 h-12 text-[#2d2d2d] group-hover:rotate-12 transition-transform duration-300" />
                      </div>
                      <h3 className="text-xl font-bold text-[#2d2d2d] mb-2">Xin chào! 👋</h3>
                      <p className="text-sm text-gray-500 text-center max-w-[200px] leading-relaxed">
                        Tôi là trợ lý sức khỏe AI của bạn. Hãy hỏi tôi bất cứ điều gì!
                      </p>
                    </div>
                  ) : (
                    messages.map((message, index) => (
                      <motion.div
                        key={message.id}
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.3 }}
                        className={`flex gap-4 items-end ${message.role === 'user' ? 'flex-row-reverse' : 'flex-row'}`}
                      >
                        {/* Avatar Block */}
                        <div className={`w-[70px] h-[60px] rounded-[1.25rem] flex items-center justify-center flex-shrink-0 shadow-sm border-2 ${message.role === 'assistant'
                          ? 'bg-[#EBE9C0] border-white'
                          : 'bg-[#2563EB] border-blue-400'
                          }`}>
                          {message.role === 'assistant' ? (
                            <Bot className="w-8 h-8 text-[#2d2d2d]" />
                          ) : (
                            <User className="w-8 h-8 text-white" />
                          )}
                        </div>

                        {/* Content Bubble */}
                        <div
                          className={`max-w-[70%] rounded-[1.5rem] px-5 py-4 shadow-md text-[15px] leading-relaxed relative ${message.role === 'user'
                            ? 'bg-[#1a1a1a] text-white rounded-tr-md'
                            : 'bg-white border border-gray-100 text-gray-800 rounded-tl-md'
                            }`}
                        >
                          {message.role === 'user' ? (
                            <p className="whitespace-pre-wrap font-medium">{message.content}</p>
                          ) : (
                            <div className="prose prose-neutral max-w-none 
                              prose-headings:text-gray-900 prose-headings:font-bold prose-headings:mb-2 prose-headings:mt-4
                              prose-p:text-gray-800 prose-p:leading-7 
                              prose-strong:text-black prose-strong:font-black
                              prose-ul:list-disc prose-ul:pl-4 prose-ul:my-2
                              prose-li:my-1
                              prose-code:bg-gray-100 prose-code:px-1.5 prose-code:py-0.5 prose-code:rounded-md prose-code:text-sm prose-code:font-mono prose-code:text-pink-600
                              first:mt-0 last:mb-0"
                            >
                              <ReactMarkdown remarkPlugins={[remarkGfm]}>
                                {message.content}
                              </ReactMarkdown>
                            </div>
                          )}
                          <p
                            className={`text-[10px] mt-2 font-medium ${message.role === 'user' ? 'text-white/40 text-left' : 'text-gray-400 text-right'
                              }`}
                          >
                            {formatTime(message.createdAt)}
                          </p>
                        </div>
                      </motion.div>
                    ))
                  )}
                  {isSending && (
                    <motion.div
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      className="flex gap-4 items-end"
                    >
                      <div className="w-[70px] h-[60px] rounded-[1.25rem] flex items-center justify-center flex-shrink-0 shadow-sm border-2 bg-[#EBE9C0] border-white">
                        <Bot className="w-8 h-8 text-[#2d2d2d]" />
                      </div>
                      <div className="bg-white border border-gray-100 rounded-[1.5rem] rounded-tl-md px-6 py-5 shadow-sm">
                        <div className="flex gap-2 items-center h-4">
                          <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce [animation-delay:-0.3s]"></div>
                          <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce [animation-delay:-0.15s]"></div>
                          <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"></div>
                        </div>
                      </div>
                    </motion.div>
                  )}
                  <div ref={messagesEndRef} />
                </div>
              </div>

              {/* Input Form */}
              <div className="p-4 bg-white border-t border-gray-100">
                <form onSubmit={handleSendMessage} className="flex items-center gap-3">
                  <input
                    type="text"
                    value={inputMessage}
                    onChange={(e) => setInputMessage(e.target.value)}
                    placeholder="Nhập câu hỏi của bạn..."
                    disabled={isSending}
                    className="flex-1 bg-gray-50 text-gray-800 placeholder-gray-400 border-2 border-gray-100 rounded-full py-4 pl-6 pr-6 focus:ring-0 focus:border-[#2d2d2d] transition-all text-base font-medium shadow-inner h-14"
                  />
                  <motion.button
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    type="submit"
                    disabled={isSending || !inputMessage.trim()}
                    className="w-20 h-14 bg-[#2d2d2d] text-[#FDFBD4] rounded-full flex items-center justify-center hover:bg-black transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-lg hover:shadow-xl"
                  >
                    {isSending ? <Loader2 className="w-6 h-6 animate-spin" /> : <Send className="w-6 h-6" />}
                  </motion.button>
                </form>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
        <motion.button
          whileHover={{ scale: 1.08, rotate: 5 }}
          whileTap={{ scale: 0.92 }}
          onClick={() => setShowChat(!showChat)}
          className="w-16 h-16 bg-gradient-to-br from-[#2d2d2d] to-[#1a1a1a] rounded-2xl shadow-2xl flex items-center justify-center text-[#EBE9C0] hover:shadow-[#2d2d2d]/30 transition-all ring-2 ring-white/10"
        >
          <Bot className="w-8 h-8" />
        </motion.button>
      </div>

    </div >
  );
}
