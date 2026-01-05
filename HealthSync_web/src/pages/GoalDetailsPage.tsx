import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { goalService, Goal } from '@/services/goalService';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

import {
  ArrowLeft,
  Plus,
  Calendar,
  TrendingUp,
  TrendingDown,
  Activity,
  Target
} from 'lucide-react';
import {
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  AreaChart,
  Line
} from 'recharts';
import Header from '@/components/Header';

const GoalDetailsPage = () => {
  const { goalId } = useParams<{ goalId: string }>();
  const navigate = useNavigate();
  const [goal, setGoal] = useState<Goal | null>(null);
  const [loading, setLoading] = useState(true);
  const [timeRange, setTimeRange] = useState<'week' | 'month' | 'all'>('month');

  const rangeLabels = {
    week: 'Tuần',
    month: 'Tháng',
    all: 'Tất cả'
  };

  useEffect(() => {
    if (goalId) {
      loadGoalDetails();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [goalId]);

  const loadGoalDetails = async () => {
    if (!goalId) return;
    try {
      setLoading(true);
      const goals = await goalService.getGoals();
      const foundGoal = goals.find((g: Goal) => g.goalId === Number.parseInt(goalId));
      if (foundGoal) {
        setGoal(foundGoal);
      }
    } catch (error) {
      console.error('Failed to load goal details:', error);
    } finally {
      setLoading(false);
    }
  };

  const getGoalTypeDisplay = (type: string) => {
    const types: Record<string, string> = {
      'weight_loss': 'Giảm cân',
      'weight_gain': 'Tăng cân',
      'muscle_gain': 'Tăng cơ',
      'fat_loss': 'Giảm mỡ',
    };
    return types[type] || type;
  };



  const getChartData = () => {
    if (!goal || goal.progressRecords.length === 0) return [];

    const sortedRecords = [...goal.progressRecords].sort(
      (a, b) => new Date(a.recordDate).getTime() - new Date(b.recordDate).getTime()
    );

    const now = new Date();
    let filteredRecords = sortedRecords;

    if (timeRange === 'week') {
      const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
      filteredRecords = sortedRecords.filter(r => new Date(r.recordDate) >= weekAgo);
    } else if (timeRange === 'month') {
      const monthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
      filteredRecords = sortedRecords.filter(r => new Date(r.recordDate) >= monthAgo);
    }

    return filteredRecords.map(record => ({
      date: new Date(record.recordDate).toLocaleDateString('vi-VN', {
        month: 'short',
        day: 'numeric'
      }),
      value: record.weightKg || record.value,
      weight: record.weightKg,
      waist: record.waistCm,
    }));
  };

  const getStatistics = () => {
    if (!goal || goal.progressRecords.length === 0) {
      return { change: 0, changePercent: 0, remaining: goal?.targetValue || 0 };
    }

    const sortedRecords = [...goal.progressRecords].sort(
      (a, b) => new Date(a.recordDate).getTime() - new Date(b.recordDate).getTime()
    );

    const startValue = sortedRecords[0].weightKg || sortedRecords[0].value;
    const currentValue = sortedRecords.at(-1)?.weightKg || sortedRecords.at(-1)?.value || 0;
    const targetValue = goal.targetValue;

    // Determine if this is a decrease goal or increase goal
    const isDecreaseGoal = goal.type === 'weight_loss' ||
      goal.type === 'fat_loss' ||
      targetValue < startValue;

    let change = 0;
    let remaining = 0;

    if (isDecreaseGoal) {
      // For decrease goals: change is how much we've decreased (start - current)
      // Remaining is how much more we need to decrease (current - target)
      change = startValue - currentValue;
      remaining = currentValue - targetValue;
    } else {
      // For increase goals: change is how much we've increased (current - start)
      // Remaining is how much more we need to increase (target - current)
      change = currentValue - startValue;
      remaining = targetValue - currentValue;
    }

    const changePercent = startValue === 0 ? 0 : ((change / startValue) * 100);

    return { change, changePercent, remaining };
  };

  const getGoalStatusInfo = () => {
    if (!goal) return { label: '', className: '' };

    const now = new Date();
    const startDate = new Date(goal.startDate);
    const status = goal.status?.toLowerCase() || '';

    // Check for future start date
    if (startDate > now && status !== 'completed') {
      return {
        label: 'Sắp diễn ra',
        className: 'bg-yellow-100 text-yellow-800'
      };
    }

    if (status === 'active' || status === 'in_progress') {
      return {
        label: 'Đang tiến hành',
        className: 'bg-blue-100 text-blue-800'
      };
    }

    if (status === 'completed') {
      return {
        label: 'Đã hoàn thành',
        className: 'bg-green-100 text-green-800'
      };
    }

    // Fallback
    return {
      // If status is empty/null, default to In Progress if within dates? 
      // User said "ghi mac dinh la Dang hoan thanh" (writes default as Completed).
      // We'll show the raw status or 'Unknown' to avoid misleading 'Completed'.
      label: goal.status || 'Không xác định',
      className: 'bg-gray-100 text-gray-600'
    };
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#FDFBD4] flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto"></div>
          <p className="mt-4 text-gray-600">Đang tải...</p>
        </div>
      </div>
    );
  }

  if (!goal) {
    return (
      <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
        <Header />
        <div className="max-w-7xl mx-auto py-8 px-4 md:px-8">
          <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm shadow-sm rounded-3xl overflow-hidden">
            <CardContent className="py-12 text-center">
              <p className="text-gray-600">Không tìm thấy mục tiêu</p>
              <Button
                onClick={() => navigate('/goals')}
                className="mt-4 bg-[#2d2d2d] hover:bg-black text-[#FDFBD4] rounded-xl"
              >
                Quay lại danh sách
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }


  const stats = getStatistics();
  const statusInfo = getGoalStatusInfo();
  const chartData = getChartData();


  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <div className="max-w-7xl mx-auto py-8 px-4 md:px-8">
        {/* Header */}
        <div className="mb-8">
          <Button
            variant="ghost"
            onClick={() => navigate('/goals')}
            className="mb-4 -ml-2 rounded-full hover:bg-black/5"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
          <div className="flex justify-between items-start">
            <div>
              <p className="text-sm text-gray-500 mb-1">Mục tiêu chính</p>
              <h1 className="text-3xl font-bold text-[#4A6F6F] mb-1">
                {getGoalTypeDisplay(goal.type)} {Math.abs(goal.targetValue - (goal.progressRecords[0]?.value || 0)).toFixed(1)}kg
              </h1>
              <p className="text-gray-400 text-sm">
                Mục tiêu: {goal.targetValue}kg
              </p>
            </div>
            {(goal.status === 'in_progress' || goal.status === 'active') && (
              <Button
                onClick={() => navigate(`/goals/${goalId}/progress`)}
                className="bg-[#2d2d2d] hover:bg-black text-[#FDFBD4] border-none rounded-xl shadow-sm"
              >
                <Plus className="w-4 h-4 mr-2" />
                Cập nhật
              </Button>
            )}
            {goal.status !== 'in_progress' && goal.status !== 'active' && (
              <Button
                disabled
                className="bg-gray-300 text-gray-500 border-none rounded-xl shadow-sm cursor-not-allowed"
              >
                <Plus className="w-4 h-4 mr-2" />
                Cập nhật
              </Button>
            )}
          </div>
        </div>

        {/* Overview Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          {/* Da giam / Da tang */}
          <Card className="bg-green-50/50 border-none shadow-sm rounded-3xl">
            <CardContent className="p-6 flex items-center gap-4">
              <div className="w-12 h-12 rounded-full bg-green-100 flex items-center justify-center text-green-600">
                {goal.type === 'weight_loss' || goal.type === 'fat_loss' || goal.targetValue < (goal.progressRecords[0]?.value || 0) ? (
                  <TrendingDown className="w-6 h-6" />
                ) : (
                  <TrendingUp className="w-6 h-6" />
                )}
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium">
                  {goal.type === 'weight_loss' || goal.type === 'fat_loss' || goal.targetValue < (goal.progressRecords[0]?.value || 0) ? 'Đã giảm' : 'Đã tăng'}
                </p>
                <div className="flex items-baseline gap-1">
                  <span className="text-2xl font-bold text-gray-900">
                    {Math.abs(stats.change).toFixed(1)}
                  </span>
                  <span className="text-sm text-gray-500">kg</span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Con lai */}
          <Card className="bg-orange-50/50 border-none shadow-sm rounded-3xl">
            <CardContent className="p-6 flex items-center gap-4">
              <div className="w-12 h-12 rounded-full bg-orange-100 flex items-center justify-center text-orange-600">
                <Target className="w-6 h-6" />
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium">Còn lại</p>
                <div className="flex items-baseline gap-1">
                  <span className="text-2xl font-bold text-gray-900">
                    {Math.abs(stats.remaining).toFixed(1)}
                  </span>
                  <span className="text-sm text-gray-500">kg</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Chart */}
        <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm shadow-sm rounded-3xl overflow-hidden mb-8">
          <CardHeader className="bg-white/30 pb-2">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
              <CardTitle className="text-lg font-bold">Biểu đồ tiến độ cân nặng</CardTitle>
              <div className="flex flex-wrap items-center gap-4">
                <div className="flex items-center gap-4 text-xs font-medium">
                  <div className="flex items-center gap-1 text-blue-500"><div className="w-2 h-2 rounded-full bg-blue-500"></div> Hiện tại</div>
                  <div className="flex items-center gap-1 text-green-500"><div className="w-2 h-2 rounded-full bg-green-500"></div> Mục tiêu</div>
                </div>
                <div className="flex bg-white/50 rounded-lg p-1">
                  {(Object.keys(rangeLabels) as Array<keyof typeof rangeLabels>).map((range) => (
                    <button
                      key={range}
                      onClick={() => setTimeRange(range)}
                      className={`px-3 py-1 text-xs font-medium rounded-md transition-all ${timeRange === range
                        ? 'bg-white text-gray-900 shadow-sm'
                        : 'text-gray-500 hover:text-gray-900'
                        }`}
                    >
                      {rangeLabels[range]}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </CardHeader>
          <CardContent className="pt-6">
            {chartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <AreaChart data={chartData}>
                  <defs>
                    <linearGradient id="colorValue" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#5FCCB4" stopOpacity={0.3} />
                      <stop offset="95%" stopColor="#5FCCB4" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                  <XAxis
                    dataKey="date"
                    stroke="#6B7280"
                    style={{ fontSize: '12px' }}
                  />
                  <YAxis
                    stroke="#9CA3AF"
                    style={{ fontSize: '12px' }}
                    domain={['dataMin - 1', 'dataMax + 1']}
                    tickLine={false}
                    axisLine={false}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: 'white',
                      border: 'none',
                      borderRadius: '8px',
                      boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)'
                    }}
                  />
                  <Line
                    type="monotone"
                    dataKey="value"
                    stroke="#3B82F6"
                    strokeWidth={3}
                    dot={false}
                    activeDot={{ r: 6, fill: "#3B82F6", stroke: "white", strokeWidth: 2 }}
                  />
                  {/* Target line */}
                  <Line
                    type="monotone"
                    dataKey={() => goal.targetValue}
                    stroke="#FF6B6B"
                    strokeWidth={2}
                    strokeDasharray="5 5"
                    dot={false}
                    name="Mục tiêu"
                  />
                </AreaChart>
              </ResponsiveContainer>
            ) : (
              <div className="text-center py-12 text-gray-500">
                Chưa có dữ liệu tiến độ
              </div>
            )}
          </CardContent>
        </Card>

        {/* Progress Records */}
        <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm shadow-sm rounded-3xl overflow-hidden mb-6">
          <CardHeader className="bg-white/30">
            <CardTitle className="text-xl">Lịch sử tiến độ</CardTitle>
          </CardHeader>
          <CardContent className="pt-6">
            {goal.progressRecords.length > 0 ? (
              <div className="space-y-4">
                {[...goal.progressRecords]
                  .sort((a, b) => new Date(b.recordDate).getTime() - new Date(a.recordDate).getTime())
                  .map((record) => (
                    <div
                      key={record.progressRecordId}
                      className="flex items-center justify-between p-4 border border-white/50 rounded-2xl hover:bg-white/40 transition-colors"
                    >
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 rounded-full bg-[#4A6F6F]/10 flex items-center justify-center">
                          <Activity className="w-6 h-6 text-[#4A6F6F]" />
                        </div>
                        <div>
                          <p className="font-semibold text-gray-900">
                            {record.value} kg
                          </p>
                          <div className="flex items-center gap-3 text-sm text-gray-600 mt-1">
                            <span className="flex items-center">
                              <Calendar className="w-3.5 h-3.5 mr-1" />
                              {new Date(record.recordDate).toLocaleDateString('vi-VN')}
                            </span>
                            {record.weightKg > 0 && (
                              <span>Cân nặng: {record.weightKg} kg</span>
                            )}
                            {record.waistCm > 0 && (
                              <span>Vòng eo: {record.waistCm} cm</span>
                            )}
                          </div>
                          {record.notes && (
                            <p className="text-sm text-gray-500 mt-1">{record.notes}</p>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
              </div>
            ) : (
              <div className="text-center py-12">
                <Activity className="w-16 h-16 mx-auto text-gray-400 mb-4" />
                <p className="text-gray-600 mb-4">Chưa có bản ghi tiến độ nào</p>
                <Button
                  onClick={() => navigate(`/goals/${goalId}/progress`)}
                  disabled={goal.status !== 'in_progress' && goal.status !== 'active'}
                  className={
                    (goal.status === 'in_progress' || goal.status === 'active')
                      ? "bg-[#2d2d2d] hover:bg-black text-[#FDFBD4] rounded-xl"
                      : "bg-gray-300 text-gray-500 rounded-xl cursor-not-allowed"
                  }
                >
                  Thêm tiến độ đầu tiên
                </Button>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Goal Info */}
        <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm shadow-sm rounded-3xl overflow-hidden">
          <CardHeader className="bg-white/30">
            <CardTitle className="text-xl">Thông tin mục tiêu</CardTitle>
          </CardHeader>
          <CardContent className="pt-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <p className="text-sm text-gray-600 mb-1">Ngày bắt đầu</p>
                <p className="font-semibold text-gray-900">
                  {new Date(goal.startDate).toLocaleDateString('vi-VN')}
                </p>
              </div>
              {goal.endDate && (
                <div>
                  <p className="text-sm text-gray-600 mb-1">Ngày kết thúc</p>
                  <p className="font-semibold text-gray-900">
                    {new Date(goal.endDate).toLocaleDateString('vi-VN')}
                  </p>
                </div>
              )}
              <div>
                <p className="text-sm text-gray-600 mb-1">Trạng thái</p>
                <span className={`inline-block px-3 py-1 rounded-full text-sm font-semibold ${statusInfo.className}`}>
                  {statusInfo.label}
                </span>
              </div>
              {goal.notes && (
                <div className="md:col-span-2">
                  <p className="text-sm text-gray-600 mb-1">Ghi chú</p>
                  <p className="text-gray-900">{goal.notes}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default GoalDetailsPage;
