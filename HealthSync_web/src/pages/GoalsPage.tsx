import { useState, useEffect, ChangeEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { goalService, Goal, ProgressRecord } from '@/services/goalService';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Input } from '@/components/ui/input';
import { Plus, Search, Pencil, Trash2 } from 'lucide-react';
import Header from '@/components/Header';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { format } from 'date-fns';
import { toast } from '@/hooks/use-toast';

const GoalsPage = () => {
  const navigate = useNavigate();
  const [goals, setGoals] = useState<Goal[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'all' | 'in-progress' | 'completed' | 'overdue'>('all');
  const [searchQuery, setSearchQuery] = useState('');

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
      toast({
        title: "Lỗi",
        description: "Không thể tải danh sách mục tiêu",
        variant: "destructive",
      });
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
      'running': 'Chạy bộ', // Example extension
      'water': 'Uống nước', // Example extension
    };
    return types[type] || type;
  };

  const calculateProgress = (goal: Goal) => {
    if (goal.progressRecords.length === 0) return 0;

    // Find earliest and latest records
    const sortedRecords = [...goal.progressRecords].sort((a, b) => new Date(a.recordDate).getTime() - new Date(b.recordDate).getTime());
    const firstRecord = sortedRecords[0];
    const lastRecord = sortedRecords[sortedRecords.length - 1];

    if (!firstRecord || !lastRecord) return 0;

    const currentV = lastRecord.value;
    const startV = firstRecord.value;
    const targetV = goal.targetValue;

    // Determine if this is a decrease goal or increase goal
    // Check type keywords or compare target vs start
    const isDecreaseGoal = goal.type === 'weight_loss' ||
      goal.type === 'fat_loss' ||
      targetV < startV;

    if (isDecreaseGoal) {
      // For decrease goals: need to go DOWN from start to target
      // Progress = (start - current) / (start - target) * 100
      const totalChangeNeeded = startV - targetV;
      if (totalChangeNeeded <= 0) return 100; // Already at or past target

      const currentChange = startV - currentV;
      const progress = (currentChange / totalChangeNeeded) * 100;
      return Math.max(0, Math.min(100, progress));
    } else {
      // For increase goals: need to go UP from start to target
      // Progress = (current - start) / (target - start) * 100
      const totalChangeNeeded = targetV - startV;
      if (totalChangeNeeded <= 0) return 100; // Already at or past target

      const currentChange = currentV - startV;
      const progress = (currentChange / totalChangeNeeded) * 100;
      return Math.max(0, Math.min(100, progress));
    }
  };

  const getCurrentValue = (goal: Goal) => {
    if (goal.progressRecords.length === 0) return 0;
    const latestRecord = [...goal.progressRecords].sort(
      (a: ProgressRecord, b: ProgressRecord) => new Date(b.recordDate).getTime() - new Date(a.recordDate).getTime()
    )[0];
    return latestRecord.value;
  };

  // Helper to determine status based on progress and date
  const getStatus = (goal: Goal) => {
    // If explicitly completed in backend
    if (goal.status?.toLowerCase() === 'completed') return 'completed';

    // Check for future start date
    if (new Date(goal.startDate) > new Date()) return 'upcoming';

    const progress = calculateProgress(goal);
    if (progress >= 100) return 'completed';

    if (goal.endDate && new Date(goal.endDate) < new Date()) {
      return 'overdue';
    }

    return 'in-progress';
  };

  const filteredGoals = goals.filter(goal => {
    const status = getStatus(goal);
    const matchesTab =
      activeTab === 'all' ||
      (activeTab === 'in-progress' && status === 'in-progress') ||
      (activeTab === 'completed' && status === 'completed') ||
      (activeTab === 'overdue' && status === 'overdue');

    const matchesSearch =
      (goal.notes || '').toLowerCase().includes(searchQuery.toLowerCase()) ||
      getGoalTypeDisplay(goal.type).toLowerCase().includes(searchQuery.toLowerCase());

    return matchesTab && matchesSearch;
  });

  const getStatusConfig = (status: string) => {
    switch (status) {
      case 'completed':
        return { label: 'HOÀN THÀNH', color: 'text-emerald-500 bg-emerald-100', barColor: 'bg-emerald-500' };
      case 'overdue':
        return { label: 'QUÁ HẠN', color: 'text-orange-500 bg-orange-100', barColor: 'bg-orange-500' };
      case 'upcoming':
        return { label: 'SẮP DIỄN RA', color: 'text-yellow-600 bg-yellow-100', barColor: 'bg-yellow-500' };
      default:
        return { label: 'ĐANG TIẾN HÀNH', color: 'text-purple-600 bg-purple-100', barColor: 'bg-purple-600' };
    }
  };

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <main className="max-w-7xl mx-auto px-4 md:px-8 pb-12 pt-4">
        {/* Page Header */}
        <div className="text-center mb-10">
          <p className="text-gray-500 mb-2 font-medium">Goal for you!</p>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Mục tiêu của bạn</h1>
          <p className="text-gray-500">Theo dõi và quản lý tất cả các mục tiêu sức khỏe của mình</p>
        </div>

        {/* Main Card */}
        <Card className="bg-[#FFFDF7]/80 backdrop-blur-sm border-white/50 shadow-sm rounded-[2.5rem] p-6 md:p-8">
          {/* Controls */}
          <div className="flex flex-col md:flex-row justify-between items-center gap-4 mb-8">
            {/* Search */}
            <div className="relative w-full md:w-96">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 w-4 h-4" />
              <Input
                placeholder="Tìm kiếm mục tiêu..."
                className="pl-10 h-11 bg-white border-transparent hover:bg-white/80 transition-colors rounded-2xl shadow-sm"
                value={searchQuery}
                onChange={(e: ChangeEvent<HTMLInputElement>) => setSearchQuery(e.target.value)}
              />
            </div>

            {/* Filters */}
            <div className="flex gap-2 p-1 bg-white/50 rounded-2xl w-full md:w-auto overflow-x-auto">
              {[
                { id: 'all', label: 'Tất cả' },
                { id: 'in-progress', label: 'Đang tiến hành' },
                { id: 'completed', label: 'Hoàn thành' },
                { id: 'overdue', label: 'Quá hạn' }
              ].map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id as any)}
                  className={cn(
                    "px-4 py-2 rounded-xl text-sm font-semibold transition-all whitespace-nowrap",
                    activeTab === tab.id
                      ? "bg-[#D8B4FE] text-[#581C87] shadow-sm"
                      : "bg-transparent text-gray-500 hover:bg-white/60"
                  )}
                >
                  {tab.label}
                </button>
              ))}
            </div>

            {/* Create Button */}
            <Button
              onClick={() => navigate('/goals/create')}
              className="hidden md:flex bg-[#8B5CF6] hover:bg-[#7C3AED] text-white rounded-2xl h-11 px-6 font-semibold shadow-purple-200 shadow-lg"
            >
              <Plus className="w-5 h-5 mr-2" />
              Tạo mục tiêu mới
            </Button>
          </div>

          {/* Table Header */}
          <div className="grid grid-cols-12 gap-6 bg-gray-50/50 rounded-xl p-4 text-xs font-bold text-gray-400 uppercase tracking-wider mb-2">
            <div className="col-span-12 md:col-span-4 pl-2">Tên mục tiêu</div>
            <div className="col-span-12 md:col-span-3 hidden md:block">Tiến độ</div>
            <div className="col-span-12 md:col-span-2 hidden md:block">Trạng thái</div>
            <div className="col-span-12 md:col-span-2 hidden md:block">Ngày kết thúc</div>
            <div className="col-span-12 md:col-span-1 hidden md:block text-right pr-2">Hành động</div>
          </div>

          {/* Goal Rows */}
          <div className="space-y-3">
            {loading && (
              <div className="text-center py-12">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-500 mx-auto"></div>
              </div>
            )}
            {!loading && filteredGoals.length === 0 && (
              <div className="text-center py-12 text-gray-500 italic bg-white/30 rounded-3xl">
                Không tìm thấy mục tiêu nào phù hợp.
              </div>
            )}
            {!loading && filteredGoals.length > 0 && (
              filteredGoals.map((goal) => {
                const status = getStatus(goal);
                const config = getStatusConfig(status);
                const progress = calculateProgress(goal);
                const currentValue = getCurrentValue(goal);

                return (
                  <div
                    key={goal.goalId}
                    className="group bg-white rounded-2xl p-4 shadow-sm border border-gray-50 hover:shadow-md transition-all grid grid-cols-12 gap-6 items-center relative"
                  >
                    <Link to={`/goals/${goal.goalId}`} className="absolute inset-0 z-0"><span className="sr-only">View Goal</span></Link>
                    {/* Name */}
                    <div className="col-span-12 md:col-span-4 pl-2 pointer-events-none">
                      <h3 className="font-bold text-gray-900 text-lg">{goal.notes || getGoalTypeDisplay(goal.type)}</h3>
                      {/* Mobile Status/Date view */}
                      <div className="md:hidden flex items-center gap-2 mt-2 text-xs text-gray-500">
                        <Badge variant="secondary" className={cn("rounded-md text-[10px]", config.color)}>
                          {config.label}
                        </Badge>
                        <span>{goal.endDate ? format(new Date(goal.endDate), 'dd/MM/yyyy') : 'N/A'}</span>
                      </div>
                    </div>

                    {/* Progress */}
                    <div className="col-span-12 md:col-span-3 pointer-events-none">
                      <div className="flex items-center gap-3">
                        <Progress value={progress} className="h-2 flex-1 bg-gray-100 rounded-full" indicatorClassName={config.barColor} />
                        <span className="text-xs font-bold text-gray-500 whitespace-nowrap min-w-[60px] text-right">
                          {currentValue.toFixed(1)}/{goal.targetValue} {goal.type.includes('weight') || goal.type.includes('muscle') || goal.type.includes('fat') ? 'kg' : ''}
                        </span>
                      </div>
                    </div>

                    {/* Status Badge */}
                    <div className="col-span-12 md:col-span-2 hidden md:block pointer-events-none">
                      <Badge variant="secondary" className={cn("rounded-lg font-bold px-3 py-1.5 border-none", config.color)}>
                        {config.label}
                      </Badge>
                    </div>

                    {/* End Date */}
                    <div className="col-span-12 md:col-span-2 hidden md:block text-sm font-medium text-gray-500 pointer-events-none">
                      {goal.endDate ? format(new Date(goal.endDate), 'dd/MM/yyyy') : 'No deadline'}
                    </div>

                    {/* Actions */}
                    <div className="col-span-12 md:col-span-1 hidden md:flex justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity pr-2 relative z-10">
                      <Button variant="ghost" size="icon" className="h-9 w-9 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-xl">
                        <Pencil className="w-4 h-4" />
                      </Button>
                      <Button variant="ghost" size="icon" className="h-9 w-9 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-xl">
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
                );
              })
            )}

            {/* Mobile Create Button */}
            <Button
              onClick={() => navigate('/goals/create')}
              className="w-full md:hidden bg-[#8B5CF6] hover:bg-[#7C3AED] text-white rounded-xl mt-4 h-12 font-bold"
            >
              <Plus className="w-5 h-5 mr-2" />
              Tạo mục tiêu mới
            </Button>
          </div>
        </Card>
      </main>
    </div>
  );
};

export default GoalsPage;
