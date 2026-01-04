import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { goalService, CreateGoalRequest } from '@/services/goalService';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { ArrowLeft, Target } from 'lucide-react';
import logo from '@/assets/logo.png';
import { useToast } from '@/hooks/use-toast';
import Header from '@/components/Header';

const CreateGoalPage = () => {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<CreateGoalRequest>({
    type: '',
    targetValue: 0,
    startDate: new Date().toISOString().split('T')[0],
    endDate: '',
    notes: '',
  });

  const goalTypes = [
    { value: 'weight_loss', label: 'Giảm cân' },
    { value: 'weight_gain', label: 'Tăng cân' },
    { value: 'muscle_gain', label: 'Tăng cơ' },
    { value: 'fat_loss', label: 'Giảm mỡ' },
  ];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.type || formData.targetValue <= 0) {
      toast({
        title: 'Lỗi',
        description: 'Vui lòng điền đầy đủ thông tin',
        variant: 'destructive',
      });
      return;
    }

    try {
      setLoading(true);
      await goalService.createGoal(formData);
      toast({
        title: 'Thành công',
        description: 'Đã tạo mục tiêu mới',
      });
      navigate('/goals');
    } catch (error) {
      console.error('Failed to create goal:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể tạo mục tiêu',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <div className="max-w-2xl mx-auto py-8 px-4 md:px-8">
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
          <h1 className="text-3xl font-bold text-gray-900">Tạo mục tiêu sức khỏe mới</h1>
          <p className="text-gray-600 mt-1 flex items-center gap-2">
            Hãy đặt ra 1 mục tiêu và cùng <img src={logo} alt="HealthSync" className="h-5 inline-block" /> hoàn thiện nhé
          </p>
        </div>

        {/* Form Card */}
        <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm shadow-sm rounded-3xl overflow-hidden">
          <CardHeader className="bg-white/30">
            <CardTitle className="flex items-center text-xl">
              <Target className="w-5 h-5 mr-2 text-[#4A6F6F]" />
              Thông tin mục tiêu
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-6">
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Goal Type */}
              <div className="space-y-2">
                <Label htmlFor="type" className="text-base font-semibold">
                  Loại mục tiêu <span className="text-red-500">*</span>
                </Label>
                <Select
                  value={formData.type}
                  onValueChange={(value) => setFormData({ ...formData, type: value })}
                >
                  <SelectTrigger className="bg-white/60 border-black/10 rounded-xl">
                    <SelectValue placeholder="Chọn loại mục tiêu" />
                  </SelectTrigger>
                  <SelectContent>
                    {goalTypes.map((type) => (
                      <SelectItem key={type.value} value={type.value}>
                        {type.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Target Value */}
              <div className="space-y-2">
                <Label htmlFor="targetValue" className="text-base font-semibold">
                  Cân nặng mục tiêu <span className="text-red-500">*</span>
                </Label>
                <div className="relative">
                  <Input
                    id="targetValue"
                    type="number"
                    step="0.1"
                    value={formData.targetValue || ''}
                    onChange={(e) =>
                      setFormData({ ...formData, targetValue: Number.parseFloat(e.target.value) || 0 })
                    }
                    placeholder="ví dụ: 60kg hoặc 10km"
                    className="pr-12 bg-white/60 border-black/10 rounded-xl"
                    required
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500">
                    kg
                  </span>
                </div>
              </div>

              {/* Dates */}
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="startDate" className="text-base font-semibold">
                    Ngày bắt đầu
                  </Label>
                  <Input
                    id="startDate"
                    type="date"
                    value={formData.startDate}
                    onChange={(e) =>
                      setFormData({ ...formData, startDate: e.target.value })
                    }
                    className="bg-white/60 border-black/10 rounded-xl"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="endDate" className="text-base font-semibold">
                    Ngày kết thúc
                  </Label>
                  <Input
                    id="endDate"
                    type="date"
                    value={formData.endDate}
                    onChange={(e) =>
                      setFormData({ ...formData, endDate: e.target.value })
                    }
                    className="bg-white/60 border-black/10 rounded-xl"
                    min={formData.startDate}
                  />
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
                  placeholder="Thêm ghi chú về mục tiêu của bạn..."
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
                  {loading ? 'Đang lưu...' : 'Lưu mục tiêu'}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default CreateGoalPage;
