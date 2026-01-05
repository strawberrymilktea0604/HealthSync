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
              initial={{ opacity: 0, scale: 0.85, y: 30 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.85, y: 30 }}
              transition={{ duration: 0.2, ease: "easeOut" }}
              className="absolute bottom-20 right-0 w-[90vw] max-w-[420px] h-[85vh] max-h-[600px] bg-white rounded-3xl shadow-2xl overflow-hidden border border-gray-200/50 origin-bottom-right flex flex-col backdrop-blur-xl"
            >
              {/* Header */}
              <div className="bg-gradient-to-br from-[#EBE9C0] via-[#E5E3B5] to-[#D9D7B6] px-4 py-3.5 flex justify-between items-center border-b border-gray-200/50 backdrop-blur-sm shadow-sm">
                <div className="flex items-center gap-2">
                  <div className="w-9 h-9 rounded-full bg-white/30 flex items-center justify-center backdrop-blur-sm">
                    <Bot className="w-6 h-6 text-[#2d2d2d]" />
                  </div>
                  <span className="font-bold text-[#2d2d2d]">Assistant</span>
                </div>
                <div className="flex items-center gap-1">
                  <button
                    onClick={handleNewChat}
                    className="hover:bg-white/30 p-2 rounded-lg transition-colors text-[#2d2d2d]"
                    title="Tạo cuộc trò chuyện mới"
                  >
                    <MessageSquarePlus className="w-5 h-5" />
                  </button>
                  <button
                    onClick={() => setShowChatHistory(!showChatHistory)}
                    className="hover:bg-white/30 p-2 rounded-lg transition-colors text-[#2d2d2d]"
                    title="Xem lịch sử chat"
                  >
                    <History className="w-5 h-5" />
                  </button>
                  <button
                    onClick={loadChatHistory}
                    className="hover:bg-white/30 p-2 rounded-lg transition-colors text-[#2d2d2d]"
                    disabled={loadingChat}
                    title="Làm mới"
                  >
                    <RefreshCw className={`w-5 h-5 ${loadingChat ? 'animate-spin' : ''}`} />
                  </button>
                  <button
                    onClick={() => setShowChat(false)}
                    className="hover:bg-white/30 p-2 rounded-lg transition-colors text-[#2d2d2d] ml-1"
                  >
                    <X className="w-5 h-5" />
                  </button>
                </div>
              </div>

              {/* Main Content Area */}
              <div className="flex-1 flex overflow-hidden">
                {/* Chat History Sidebar */}
                {showChatHistory && (
                  <div className="w-64 bg-gray-50 border-r border-gray-200 flex flex-col">
                    <div className="px-3 py-2 border-b border-gray-200">
                      <h3 className="text-sm font-semibold text-gray-700">Lịch sử</h3>
                    </div>
                    <div className="flex-1 overflow-y-auto p-2 space-y-1.5">
                      {messages.length > 0 ? (
                        <div className="space-y-1.5">
                          <div className="p-2.5 bg-white rounded-lg border border-gray-200 hover:bg-gray-50 cursor-pointer transition-colors">
                            <div className="flex items-center gap-2 mb-1">
                              <MessageSquarePlus className="w-3.5 h-3.5 text-[#4A6F6F]" />
                              <span className="text-xs font-medium text-gray-800">Hiện tại</span>
                            </div>
                            <p className="text-xs text-gray-600 line-clamp-2">
                              {messages[0]?.content.substring(0, 50)}...
                            </p>
                            <p className="text-xs text-gray-500 mt-1">
                              {messages.length} tin nhắn
                            </p>
                          </div>
                          <div className="text-xs text-gray-500 px-2 py-1">Trước đó</div>
                          <div className="p-2.5 text-center text-xs text-gray-500">
                            Chưa có lịch sử
                          </div>
                        </div>
                      ) : (
                        <div className="flex flex-col items-center justify-center h-full text-gray-400">
                          <History className="w-10 h-10 mb-2 opacity-30" />
                          <p className="text-xs">Chưa có lịch sử</p>
                        </div>
                      )}
                    </div>
                  </div>
                )}

                {/* Chat Messages */}
                <div className="flex-1 overflow-y-auto p-4 space-y-3 bg-gradient-to-b from-gray-50/30 to-gray-50/60 scrollbar-thin scrollbar-thumb-gray-300 scrollbar-track-transparent">
                  {loadingChat ? (
                    <div className="flex justify-center items-center h-full">
                      <Loader2 className="w-8 h-8 animate-spin text-[#D4C5A9]" />
                    </div>
                  ) : messages.length === 0 ? (
                    <div className="flex flex-col items-center justify-center h-full text-gray-400 animate-fade-in">
                      <motion.div 
                        initial={{ scale: 0.8, opacity: 0 }}
                        animate={{ scale: 1, opacity: 1 }}
                        transition={{ duration: 0.3 }}
                        className="w-20 h-20 rounded-full bg-gradient-to-br from-[#EBE9C0]/40 to-[#D9D7B6]/30 flex items-center justify-center mb-4 shadow-lg"
                      >
                        <Bot className="w-11 h-11 text-[#4A6F6F] opacity-60" />
                      </motion.div>
                      <p className="text-base font-semibold text-gray-700">Xin chào! 👋</p>
                      <p className="text-sm text-gray-500 mt-2 text-center px-8">Tôi là trợ lý sức khỏe của bạn.<br/>Hỏi tôi bất cứ điều gì!</p>
                    </div>
                  ) : (
                    messages.map((message, index) => (
                      <motion.div
                        key={message.id}
                        initial={{ opacity: 0, y: 10, scale: 0.95 }}
                        animate={{ opacity: 1, y: 0, scale: 1 }}
                        transition={{ duration: 0.2, delay: index * 0.05 }}
                        className={`flex gap-2.5 ${message.role === 'user' ? 'justify-end' : 'justify-start'}`}
                      >
                        {message.role === 'assistant' && (
                          <div className="w-8 h-8 rounded-full bg-gradient-to-br from-[#EBE9C0] to-[#D9D7B6] flex items-center justify-center flex-shrink-0 shadow-md ring-2 ring-white/50">
                            <Bot className="w-5 h-5 text-[#2d2d2d]" />
                          </div>
                        )}

                        <div
                          className={`max-w-[78%] rounded-2xl px-4 py-2.5 ${
                            message.role === 'user'
                              ? 'bg-gradient-to-br from-[#2d2d2d] via-[#252525] to-[#1a1a1a] text-white shadow-lg'
                              : 'bg-white border border-gray-200/80 shadow-md hover:shadow-lg transition-shadow'
                          }`}
                        >
                          <p className="text-sm whitespace-pre-wrap leading-relaxed">{message.content}</p>
                          <p
                            className={`text-[11px] mt-1.5 ${
                              message.role === 'user' ? 'text-white/60' : 'text-gray-400'
                            }`}
                          >
                            {formatTime(message.createdAt)}
                          </p>
                        </div>

                        {message.role === 'user' && (
                          <div className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center flex-shrink-0 shadow-md ring-2 ring-white/50">
                            <User className="w-5 h-5 text-white" />
                          </div>
                        )}
                      </motion.div>
                    )))
                  }
                  {isSending && (
                    <motion.div
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      className="flex gap-2.5 justify-start"
                    >
                      <div className="w-8 h-8 rounded-full bg-gradient-to-br from-[#EBE9C0] to-[#D9D7B6] flex items-center justify-center flex-shrink-0 shadow-md ring-2 ring-white/50">
                        <Bot className="w-5 h-5 text-[#2d2d2d]" />
                      </div>
                      <div className="bg-white border border-gray-200/80 shadow-md rounded-2xl px-4 py-3">
                        <div className="flex gap-1.5">
                          <motion.div
                            animate={{ scale: [1, 1.3, 1] }}
                            transition={{ duration: 0.6, repeat: Infinity, delay: 0 }}
                            className="w-2 h-2 bg-gray-400 rounded-full"
                          />
                          <motion.div
                            animate={{ scale: [1, 1.3, 1] }}
                            transition={{ duration: 0.6, repeat: Infinity, delay: 0.2 }}
                            className="w-2 h-2 bg-gray-400 rounded-full"
                          />
                          <motion.div
                            animate={{ scale: [1, 1.3, 1] }}
                            transition={{ duration: 0.6, repeat: Infinity, delay: 0.4 }}
                            className="w-2 h-2 bg-gray-400 rounded-full"
                          />
                        </div>
                      </div>
                    </motion.div>
                  )}
                  <div ref={messagesEndRef} />
                </div>
              </div>

              {/* Input Form */}
              <form onSubmit={handleSendMessage} className="p-3.5 bg-white border-t border-gray-200/50 backdrop-blur-sm">
                <div className="flex gap-2.5">
                  <input
                    type="text"
                    value={inputMessage}
                    onChange={(e) => setInputMessage(e.target.value)}
                    placeholder="Nhập câu hỏi của bạn..."
                    disabled={isSending}
                    className="flex-1 px-4 py-3 border border-gray-300 rounded-full focus:outline-none focus:ring-2 focus:ring-[#D4C5A9] focus:border-transparent text-sm disabled:bg-gray-100 disabled:cursor-not-allowed transition-all placeholder:text-gray-400"
                  />
                  <motion.button
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    type="submit"
                    disabled={isSending || !inputMessage.trim()}
                    className="w-11 h-11 bg-gradient-to-br from-[#2d2d2d] to-[#1a1a1a] text-[#EBE9C0] rounded-full flex items-center justify-center hover:shadow-lg transition-all disabled:opacity-40 disabled:cursor-not-allowed flex-shrink-0 shadow-md"
                  >
                    {isSending ? (
                      <Loader2 className="w-5 h-5 animate-spin" />
                    ) : (
                      <Send className="w-4.5 h-4.5" />
                    )}
                  </motion.button>
                </div>
              </form>
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
