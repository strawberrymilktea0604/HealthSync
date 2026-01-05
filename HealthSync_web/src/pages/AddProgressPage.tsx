import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { goalService, AddProgressRequest, Goal } from '@/services/goalService';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { ArrowLeft, TrendingUp, Flag } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import Header from '@/components/Header';

const AddProgressPage = () => {
  const { goalId } = useParams<{ goalId: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [goal, setGoal] = useState<Goal | null>(null);
  const [formData, setFormData] = useState<AddProgressRequest>({
    recordDate: new Date().toISOString().split('T')[0],
    value: 0,
    notes: '',
    weightKg: undefined,
    waistCm: undefined,
  });

  useEffect(() => {
    if (goalId) {
      goalService.getGoals().then(goals => {
        const found = goals.find((g: Goal) => g.goalId === Number(goalId));
        if (found) setGoal(found);
      });
    }
  }, [goalId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!goalId || formData.value <= 0) {
      toast({
        title: 'Lỗi',
        description: 'Vui lòng điền đầy đủ thông tin',
        variant: 'destructive',
      });
      return;
    }

    // Check if goal status allows progress updates
    if (goal && goal.status !== 'active' && goal.status !== 'in_progress') {
      toast({
        title: 'Lỗi',
        description: `Chỉ có thể cập nhật tiến độ cho mục tiêu đang hoạt động. Trạng thái hiện tại: ${goal.status}`,
        variant: 'destructive',
      });
      return;
    }

    try {
      setLoading(true);

      // Convert date to UTC to avoid timezone issues
      const dateObj = new Date(formData.recordDate);
      const utcDate = new Date(Date.UTC(dateObj.getFullYear(), dateObj.getMonth(), dateObj.getDate()));

      await goalService.addProgress(Number.parseInt(goalId), {
        ...formData,
        recordDate: utcDate.toISOString(),
      });
      toast({
        title: 'Thành công',
        description: 'Đã cập nhật tiến độ',
      });
      navigate(`/goals/${goalId}`);
    } catch (error) {
      console.error('Failed to add progress:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể cập nhật tiến độ',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const getGoalActionText = (goalType: string) => {
    const type = goalType.toLowerCase();
    if (type === 'weight_loss' || type === 'fat_loss') return 'Giảm';
    if (type === 'weight_gain' || type === 'muscle_gain') return 'Tăng';
    return 'Mục tiêu';
  };

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <div className="max-w-2xl mx-auto py-8 px-4 md:px-8">
        {/* Header */}
        <div className="mb-8">
          <Button
            variant="ghost"
            onClick={() => navigate(`/goals/${goalId}`)}
            className="mb-4 -ml-2 rounded-full hover:bg-black/5"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
          <h1 className="text-3xl font-bold text-gray-900">Cập nhật tiến độ mục tiêu</h1>
          <p className="text-gray-600 mt-1">
            Ghi lại tiến trình của bạn để ra mục tiêu
          </p>
        </div>

        {/* Form Card */}
        <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm shadow-sm rounded-3xl overflow-hidden">
          <CardHeader className="bg-white/30">
            <CardTitle className="flex items-center text-xl">
              <TrendingUp className="w-5 h-5 mr-2 text-[#4A6F6F]" />
              Ghi lại tiến độ
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-6">
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Current Goal Display (Read-only) */}
              <div className="bg-white/40 p-4 rounded-2xl border border-white/50 flex items-center gap-3 backdrop-blur-sm">
                <div className="bg-[#4A6F6F]/10 p-2 rounded-full">
                  <Flag className="w-5 h-5 text-[#4A6F6F]" />
                </div>
                <div>
                  <p className="text-xs text-gray-500 font-medium">Mục tiêu hiện tại</p>
                  <p className="font-bold text-gray-900">
                    {goal ? (
                      <>
                        <>
                          {getGoalActionText(goal.type)} <span className="text-[#4A6F6F]">{goal.targetValue}kg</span>
                        </>
                      </>
                    ) : '...'}
                  </p>
                </div>
              </div>

              {/* Value (Main Metric) */}
              <div className="space-y-2">
                <Label htmlFor="value" className="text-base font-semibold">
                  Giá trị đo lường mới (kg) <span className="text-red-500">*</span>
                </Label>
                <div className="relative">
                  <Input
                    id="value"
                    type="number"
                    step="0.1"
                    value={formData.value || ''}
                    onChange={(e) =>
                      setFormData({ ...formData, value: Number.parseFloat(e.target.value) || 0 })
                    }
                    placeholder="Nhập cân nặng hiện tại"
                    className="pr-12 bg-white/60 border-black/10 rounded-xl"
                    required
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500">
                    kg
                  </span>
                </div>
              </div>

              {/* Record Date */}
              <div className="space-y-2">
                <Label htmlFor="recordDate" className="text-base font-semibold">
                  Ngày ghi nhận
                </Label>
                <Input
                  id="recordDate"
                  type="date"
                  value={formData.recordDate}
                  onChange={(e) =>
                    setFormData({ ...formData, recordDate: e.target.value })
                  }
                  className="bg-white/60 border-black/10 rounded-xl"
                  max={new Date().toISOString().split('T')[0]}
                />
              </div>

              {/* Weight */}
              <div className="space-y-2">
                <Label htmlFor="weightKg" className="text-base font-semibold">
                  Cân nặng hiện tại
                </Label>
                <div className="relative">
                  <Input
                    id="weightKg"
                    type="number"
                    step="0.1"
                    value={formData.weightKg || ''}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        weightKg: e.target.value ? Number.parseFloat(e.target.value) : undefined
                      })
                    }
                    placeholder="Nhập cân nặng"
                    className="pr-12 bg-white/60 border-black/10 rounded-xl"
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500">
                    kg
                  </span>
                </div>
              </div>

              {/* Waist */}
              <div className="space-y-2">
                <Label htmlFor="waistCm" className="text-base font-semibold">
                  Vòng eo
                </Label>
                <div className="relative">
                  <Input
                    id="waistCm"
                    type="number"
                    step="0.1"
                    value={formData.waistCm || ''}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        waistCm: e.target.value ? Number.parseFloat(e.target.value) : undefined
                      })
                    }
                    placeholder="Nhập vòng eo"
                    className="pr-12 bg-white/60 border-black/10 rounded-xl"
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500">
                    cm
                  </span>
                </div>
              </div>

              {/* Notes */}
              <div className="space-y-2">
                <Label htmlFor="notes" className="text-base font-semibold">
                  Ghi chú
                </Label>
                <Textarea
                  id="notes"
                  value={formData.notes}
                  onChange={(e) =>
                    setFormData({ ...formData, notes: e.target.value })
                  }
                  placeholder="Thêm ghi chú về tiến độ hôm nay..."
                  className="bg-white/60 border-black/10 rounded-xl resize-none"
                  rows={4}
                />
              </div>

              {/* Submit Button */}
              <div className="pt-4">
                <Button
                  type="submit"
                  disabled={loading}
                  className="w-full bg-[#2d2d2d] hover:bg-black text-[#FDFBD4] py-6 text-lg font-bold rounded-xl shadow-lg"
                >
                  {loading ? 'Đang cập nhật...' : 'Cập nhật tiến độ'}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default AddProgressPage;
