import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { goalService, Goal } from '@/services/goalService';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Plus, TrendingUp, Target, Calendar } from 'lucide-react';

const GoalsPage = () => {
  const navigate = useNavigate();
  const [goals, setGoals] = useState<Goal[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'in-progress' | 'completed'>('in-progress');

  useEffect(() => {
    loadGoals();
  }, []);

  const loadGoals = async () => {
    try {
      setLoading(true);
      const data = await goalService.getGoals();
      setGoals(data);
    } catch (error) {
      console.error('Failed to load goals:', error);
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

  const calculateProgress = (goal: Goal) => {
    if (goal.progressRecords.length === 0) return 0;
    
    const latestRecord = goal.progressRecords.sort(
      (a, b) => new Date(b.recordDate).getTime() - new Date(a.recordDate).getTime()
    )[0];
    
    const startValue = goal.progressRecords[0]?.value || latestRecord.value;
    const currentValue = latestRecord.value;
    const targetValue = goal.targetValue;
    
    if (goal.type === 'weight_loss' || goal.type === 'fat_loss') {
      const progress = ((startValue - currentValue) / (startValue - targetValue)) * 100;
      return Math.max(0, Math.min(100, progress));
    } else {
      const progress = ((currentValue - startValue) / (targetValue - startValue)) * 100;
      return Math.max(0, Math.min(100, progress));
    }
  };

  const getCurrentValue = (goal: Goal) => {
    if (goal.progressRecords.length === 0) return null;
    const latestRecord = goal.progressRecords.sort(
      (a, b) => new Date(b.recordDate).getTime() - new Date(a.recordDate).getTime()
    )[0];
    return latestRecord.value;
  };

  const filteredGoals = goals.filter(goal => 
    activeTab === 'in-progress' 
      ? goal.status === 'active' 
      : goal.status === 'completed'
  );

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

  return (
    <div className="min-h-screen p-6" style={{ backgroundColor: '#E8E4D9' }}>
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Mục tiêu của bạn</h1>
            <p className="text-gray-600 mt-1">Theo dõi và quản lý các mục tiêu sức khỏe</p>
          </div>
          <Button 
            onClick={() => navigate('/goals/create')}
            className="bg-[#5FCCB4] hover:bg-[#4DB89E] text-white"
          >
            <Plus className="w-4 h-4 mr-2" />
            Tạo mục tiêu mới
          </Button>
        </div>

        {/* Tabs */}
        <div className="flex gap-2 mb-6">
          <Button
            variant={activeTab === 'in-progress' ? 'default' : 'outline'}
            onClick={() => setActiveTab('in-progress')}
            className={activeTab === 'in-progress' 
              ? 'bg-[#5FCCB4] hover:bg-[#4DB89E] text-white' 
              : 'bg-white hover:bg-gray-50'}
          >
            Đang thực hiện
          </Button>
          <Button
            variant={activeTab === 'completed' ? 'default' : 'outline'}
            onClick={() => setActiveTab('completed')}
            className={activeTab === 'completed' 
              ? 'bg-[#5FCCB4] hover:bg-[#4DB89E] text-white' 
              : 'bg-white hover:bg-gray-50'}
          >
            Đã hoàn thành
          </Button>
        </div>

        {/* Goals Grid */}
        {filteredGoals.length === 0 ? (
          <Card className="bg-white">
            <CardContent className="py-12 text-center">
              <Target className="w-16 h-16 mx-auto text-gray-400 mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Chưa có mục tiêu nào
              </h3>
              <p className="text-gray-600 mb-4">
                {activeTab === 'in-progress' 
                  ? 'Hãy tạo mục tiêu đầu tiên của bạn'
                  : 'Bạn chưa hoàn thành mục tiêu nào'}
              </p>
              {activeTab === 'in-progress' && (
                <Button 
                  onClick={() => navigate('/goals/create')}
                  className="bg-[#5FCCB4] hover:bg-[#4DB89E] text-white"
                >
                  Tạo mục tiêu mới
                </Button>
              )}
            </CardContent>
          </Card>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredGoals.map((goal) => {
              const progress = calculateProgress(goal);
              const currentValue = getCurrentValue(goal);

              return (
                <Card 
                  key={goal.goalId} 
                  className="bg-white hover:shadow-lg transition-shadow cursor-pointer"
                  onClick={() => navigate(`/goals/${goal.goalId}`)}
                >
                  <CardHeader>
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <CardTitle className="text-xl font-bold text-gray-900">
                          {getGoalTypeDisplay(goal.type)}
                        </CardTitle>
                        <p className="text-sm text-gray-600 mt-1">
                          Mục tiêu: {goal.targetValue} kg
                        </p>
                      </div>
                      <Target className="w-6 h-6 text-[#5FCCB4]" />
                    </div>
                  </CardHeader>
                  <CardContent>
                    {/* Current Progress */}
                    {currentValue && (
                      <div className="mb-4">
                        <div className="flex justify-between text-sm mb-2">
                          <span className="text-gray-600">Hiện tại</span>
                          <span className="font-semibold text-gray-900">
                            {currentValue} kg
                          </span>
                        </div>
                        <Progress value={progress} className="h-2" />
                        <p className="text-xs text-gray-500 mt-1">
                          {progress.toFixed(0)}% hoàn thành
                        </p>
                      </div>
                    )}

                    {/* Date Range */}
                    <div className="flex items-center text-sm text-gray-600 mb-2">
                      <Calendar className="w-4 h-4 mr-2" />
                      <span>
                        {new Date(goal.startDate).toLocaleDateString('vi-VN')}
                        {goal.endDate && ` - ${new Date(goal.endDate).toLocaleDateString('vi-VN')}`}
                      </span>
                    </div>

                    {/* Stats */}
                    <div className="flex items-center text-sm text-gray-600">
                      <TrendingUp className="w-4 h-4 mr-2" />
                      <span>{goal.progressRecords.length} bản ghi tiến độ</span>
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};

export default GoalsPage;
