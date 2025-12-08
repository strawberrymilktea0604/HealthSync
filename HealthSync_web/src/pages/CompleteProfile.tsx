import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useToast } from "@/hooks/use-toast";
import { useAuth } from "@/contexts/AuthContext";
import Footer from "@/components/Footer";
import logo from "@/assets/logo.png";
import { motion } from "framer-motion";

export default function CompleteProfile() {
  const [fullName, setFullName] = useState("");
  const [dateOfBirth, setDateOfBirth] = useState("");
  const [gender, setGender] = useState("");
  const [heightCm, setHeightCm] = useState("");
  const [weightKg, setWeightKg] = useState("");
  const [activityLevel, setActivityLevel] = useState("Moderate");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { toast } = useToast();
  const { user, setUser } = useAuth();

  useEffect(() => {
    if (!user) {
      navigate("/login");
      return;
    }
    if (user.isProfileComplete) {
      navigate("/dashboard");
      return;
    }
  }, [user, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!fullName.trim()) {
      toast({
        title: "Lỗi",
        description: "Vui lòng nhập họ tên",
        variant: "destructive",
      });
      return;
    }

    if (!dateOfBirth) {
      toast({
        title: "Lỗi",
        description: "Vui lòng chọn ngày sinh",
        variant: "destructive",
      });
      return;
    }

    if (!gender) {
      toast({
        title: "Lỗi",
        description: "Vui lòng chọn giới tính",
        variant: "destructive",
      });
      return;
    }

    const height = Number.parseFloat(heightCm);
    const weight = Number.parseFloat(weightKg);

    if (!heightCm || height <= 0 || height > 300) {
      toast({
        title: "Lỗi",
        description: "Vui lòng nhập chiều cao hợp lệ (cm)",
        variant: "destructive",
      });
      return;
    }

    if (!weightKg || weight <= 0 || weight > 500) {
      toast({
        title: "Lỗi",
        description: "Vui lòng nhập cân nặng hợp lệ (kg)",
        variant: "destructive",
      });
      return;
    }

    setIsLoading(true);
    try {
      const response = await fetch(`http://localhost:5274/api/user/profile`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${user.token}`,
        },
        body: JSON.stringify({
          fullName: fullName.trim(),
          dob: dateOfBirth,
          gender,
          heightCm: height,
          weightKg: weight,
          activityLevel,
        }),
      });

      if (!response.ok) {
        throw new Error('Failed to update profile');
      }

      // Update user context
      const updatedUser = { ...user, isProfileComplete: true, fullName: fullName.trim() };
      setUser(updatedUser);

      toast({
        title: "Thành công",
        description: "Hồ sơ đã được cập nhật!",
      });

      navigate("/dashboard");
    } catch (error) {
      console.error('Error updating profile:', error);
      toast({
        title: "Lỗi",
        description: "Không thể cập nhật hồ sơ. Vui lòng thử lại.",
        variant: "destructive",
      });
    } finally {
      setIsLoading(false);
    }
  };

  if (!user) return null;

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex flex-col">
      <div className="flex-1 flex items-center justify-center p-4">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="w-full max-w-md"
        >
          <div className="bg-white rounded-2xl shadow-xl p-8">
            <div className="text-center mb-8">
              <h1 className="text-3xl font-bold m-0 text-900 flex align-items-center gap-2 justify-center mb-4">
                Welcome to 
                <motion.img 
                  src={logo} 
                  alt="healthsync" 
                  style={{ height: '24px', marginTop: '4px' }}
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
              </h1>
              <h1 className="text-2xl font-bold text-gray-900 mb-2">
                Hoàn thiện hồ sơ
              </h1>
              <p className="text-gray-600">
                Vui lòng điền đầy đủ thông tin để tiếp tục sử dụng ứng dụng
              </p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-6">
              <div>
                <Label htmlFor="fullName">Họ và tên *</Label>
                <Input
                  id="fullName"
                  type="text"
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  placeholder="Nhập họ và tên"
                  required
                />
              </div>

              <div>
                <Label htmlFor="dateOfBirth">Ngày sinh *</Label>
                <Input
                  id="dateOfBirth"
                  type="date"
                  value={dateOfBirth}
                  onChange={(e) => setDateOfBirth(e.target.value)}
                  required
                />
              </div>

              <div>
                <Label htmlFor="gender">Giới tính *</Label>
                <Select value={gender} onValueChange={setGender}>
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn giới tính" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Male">Nam</SelectItem>
                    <SelectItem value="Female">Nữ</SelectItem>
                    <SelectItem value="Other">Khác</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="height">Chiều cao (cm) *</Label>
                  <Input
                    id="height"
                    type="number"
                    value={heightCm}
                    onChange={(e) => setHeightCm(e.target.value)}
                    placeholder="170"
                    min="50"
                    max="300"
                    required
                  />
                </div>
                <div>
                  <Label htmlFor="weight">Cân nặng (kg) *</Label>
                  <Input
                    id="weight"
                    type="number"
                    value={weightKg}
                    onChange={(e) => setWeightKg(e.target.value)}
                    placeholder="70"
                    min="20"
                    max="500"
                    required
                  />
                </div>
              </div>

              <div>
                <Label htmlFor="activityLevel">Mức độ hoạt động</Label>
                <Select value={activityLevel} onValueChange={setActivityLevel}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Sedentary">Ít vận động</SelectItem>
                    <SelectItem value="Light">Vận động nhẹ</SelectItem>
                    <SelectItem value="Moderate">Vận động vừa phải</SelectItem>
                    <SelectItem value="Active">Vận động nhiều</SelectItem>
                    <SelectItem value="VeryActive">Vận động rất nhiều</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <Button
                type="submit"
                className="w-full bg-blue-600 hover:bg-blue-700"
                disabled={isLoading}
              >
                {isLoading ? "Đang cập nhật..." : "Hoàn thành"}
              </Button>
            </form>
          </div>
        </motion.div>
      </div>
      <Footer />
    </div>
  );
}