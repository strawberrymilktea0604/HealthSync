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
import { useToast } from '@/hooks/use-toast';

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
    <div className="min-h-screen p-6" style={{ backgroundColor: '#E8E4D9' }}>
      <div className="max-w-2xl mx-auto">
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
          <h1 className="text-3xl font-bold text-gray-900">Tạo mục tiêu sức khỏe mới</h1>
          <p className="text-gray-600 mt-1">
            Hãy đặt ra một mục tiêu rõ ràng để hoàn thành nó cùng HealthSync
          </p>
        </div>

        {/* Form Card */}
        <Card className="bg-white shadow-lg">
          <CardHeader className="border-b" style={{ backgroundColor: '#F5F3ED' }}>
            <CardTitle className="flex items-center text-xl">
              <Target className="w-5 h-5 mr-2 text-[#5FCCB4]" />
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
                  <SelectTrigger className="bg-white">
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
                    placeholder="Ví dụ: 65"
                    className="pr-12 bg-white"
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
                    className="bg-white"
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
                    className="bg-white"
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
                  className="bg-white resize-none"
                  rows={4}
                />
              </div>

              {/* Submit Button */}
              <div className="pt-4">
                <Button
                  type="submit"
                  disabled={loading}
                  className="w-full bg-[#5FCCB4] hover:bg-[#4DB89E] text-white py-3 text-lg font-semibold"
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
