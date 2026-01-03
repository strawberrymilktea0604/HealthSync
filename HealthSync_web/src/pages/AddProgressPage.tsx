import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { goalService, AddProgressRequest } from '@/services/goalService';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { ArrowLeft, TrendingUp } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';

const AddProgressPage = () => {
  const { goalId } = useParams<{ goalId: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<AddProgressRequest>({
    recordDate: new Date().toISOString().split('T')[0],
    value: 0,
    notes: '',
    weightKg: undefined,
    waistCm: undefined,
  });

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

    try {
      setLoading(true);
      await goalService.addProgress(Number.parseInt(goalId), formData);
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

  return (
    <div className="min-h-screen p-6" style={{ backgroundColor: '#E8E4D9' }}>
      <div className="max-w-2xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <Button
            variant="ghost"
            onClick={() => navigate(`/goals/${goalId}`)}
            className="mb-4 -ml-2"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
          <h1 className="text-3xl font-bold text-gray-900">Cập nhật tiến độ mục tiêu</h1>
          <p className="text-gray-600 mt-1">
            Ghi lại tiến độ của bạn để theo dõi sự tiến bộ
          </p>
        </div>

        {/* Form Card */}
        <Card className="bg-white shadow-lg">
          <CardHeader className="border-b" style={{ backgroundColor: '#F5F3ED' }}>
            <CardTitle className="flex items-center text-xl">
              <TrendingUp className="w-5 h-5 mr-2 text-[#5FCCB4]" />
              Ghi lại tiến độ
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-6">
            <form onSubmit={handleSubmit} className="space-y-6">
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
                  className="bg-white"
                  max={new Date().toISOString().split('T')[0]}
                />
              </div>

              {/* Value (Main Metric) */}
              <div className="space-y-2">
                <Label htmlFor="value" className="text-base font-semibold">
                  Giá trị đo được <span className="text-red-500">*</span>
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
                    placeholder="Nhập giá trị"
                    className="pr-12 bg-white"
                    required
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500">
                    kg
                  </span>
                </div>
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
                    className="pr-12 bg-white"
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
                    className="pr-12 bg-white"
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
                  {loading ? 'Đang lưu...' : 'Lưu tiến độ'}
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
