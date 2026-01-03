import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { goalService, Goal } from '@/services/goalService';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
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
  Area,
  AreaChart,
  Line
} from 'recharts';

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
      const foundGoal = goals.find(g => g.goalId === Number.parseInt(goalId));
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

  const calculateProgress = () => {
    if (!goal || goal.progressRecords.length === 0) return 0;

    const sortedRecords = [...goal.progressRecords].sort(
      (a, b) => new Date(a.recordDate).getTime() - new Date(b.recordDate).getTime()
    );

    const startValue = sortedRecords[0].value;
    const currentValue = sortedRecords.at(-1)?.value ?? 0;
    const targetValue = goal.targetValue;

    if (goal.type === 'weight_loss' || goal.type === 'fat_loss') {
      const progress = ((startValue - currentValue) / (startValue - targetValue)) * 100;
      return Math.max(0, Math.min(100, progress));
    } else {
      const progress = ((currentValue - startValue) / (targetValue - startValue)) * 100;
      return Math.max(0, Math.min(100, progress));
    }
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
      value: record.value,
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

    const startValue = sortedRecords[0].value;
    const currentValue = sortedRecords.at(-1)?.value ?? 0;
    const targetValue = goal.targetValue;

    const change = currentValue - startValue;
    const changePercent = ((change / startValue) * 100);
    const remaining = targetValue - currentValue;

    return { change, changePercent, remaining };
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen" style={{ backgroundColor: '#E8E4D9' }}>
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto"></div>
          <p className="mt-4 text-gray-600">Đang tải...</p>
        </div>
      </div>
    );
  }

  if (!goal) {
    return (
      <div className="min-h-screen p-6" style={{ backgroundColor: '#E8E4D9' }}>
        <div className="max-w-7xl mx-auto">
          <Card className="bg-white">
            <CardContent className="py-12 text-center">
              <p className="text-gray-600">Không tìm thấy mục tiêu</p>
              <Button
                onClick={() => navigate('/goals')}
                className="mt-4 bg-[#5FCCB4] hover:bg-[#4DB89E] text-white"
              >
                Quay lại danh sách
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  const progress = calculateProgress();
  const stats = getStatistics();
  const chartData = getChartData();
  const latestRecord = goal.progressRecords.length > 0
    ? [...goal.progressRecords].sort((a, b) =>
      new Date(b.recordDate).getTime() - new Date(a.recordDate).getTime()
    )[0]
    : null;

  return (
    <div className="min-h-screen p-6" style={{ backgroundColor: '#E8E4D9' }}>
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <Button
            variant="ghost"
            onClick={() => navigate('/goals')}
            className="mb-4 -ml-2"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
          <div className="flex justify-between items-start">
            <div>
              <p className="text-sm text-gray-500 mb-1">Mục tiêu chính</p>
              <h1 className="text-3xl font-bold text-[#5FCCB4] mb-1">
                {getGoalTypeDisplay(goal.type)} {Math.abs(goal.targetValue - (goal.progressRecords[0]?.value || 0))}kg
              </h1>
              <p className="text-gray-400 text-sm">
                Mục tiêu: {goal.targetValue}kg
              </p>
            </div>
            <Button
              onClick={() => navigate(`/goals/${goalId}/progress`)}
              className="bg-white text-gray-900 border border-gray-200 hover:bg-gray-50 shadow-sm"
            >
              <Plus className="w-4 h-4 mr-2" />
              Cập nhật
            </Button>
          </div>
        </div>

        {/* Overview Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          {/* Da giam / Da tang */}
          <Card className="bg-green-50/50 border-none shadow-sm">
            <CardContent className="p-6 flex items-center gap-4">
              <div className="w-12 h-12 rounded-full bg-green-100 flex items-center justify-center text-green-600">
                <TrendingDown className="w-6 h-6" />
              </div>
              <div>
                <p className="text-sm text-gray-500 font-medium">Đã giảm</p>
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
          <Card className="bg-orange-50/50 border-none shadow-sm">
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
        <Card className="bg-white mb-8 border-none shadow-sm">
          <CardHeader className="border-b-0 pb-2">
            <div className="flex justify-between items-center">
              <CardTitle className="text-lg font-bold">Biểu đồ tiến độ cân nặng</CardTitle>
              <div className="flex gap-2">
                <div className="flex items-center gap-4 text-xs font-medium mr-4">
                  <div className="flex items-center gap-1 text-blue-500"><div className="w-2 h-2 rounded-full bg-blue-500"></div> Hiện tại</div>
                  <div className="flex items-center gap-1 text-green-500"><div className="w-2 h-2 rounded-full bg-green-500"></div> Mục tiêu</div>
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
        <Card className="bg-white">
          <CardHeader className="border-b" style={{ backgroundColor: '#F5F3ED' }}>
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
                      className="flex items-center justify-between p-4 border rounded-lg hover:bg-gray-50 transition-colors"
                    >
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 rounded-full bg-[#5FCCB4]/10 flex items-center justify-center">
                          <Activity className="w-6 h-6 text-[#5FCCB4]" />
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
                  className="bg-[#5FCCB4] hover:bg-[#4DB89E] text-white"
                >
                  Thêm tiến độ đầu tiên
                </Button>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Goal Info */}
        <Card className="bg-white mt-6">
          <CardHeader className="border-b" style={{ backgroundColor: '#F5F3ED' }}>
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
                <span className={`inline-block px-3 py-1 rounded-full text-sm font-semibold ${goal.status === 'active'
                  ? 'bg-green-100 text-green-700'
                  : 'bg-gray-100 text-gray-700'
                  }`}>
                  {goal.status === 'active' ? 'Đang hoạt động' : 'Đã hoàn thành'}
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
