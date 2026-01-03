import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useToast } from "@/hooks/use-toast";
import { useAuth } from "@/contexts/AuthContext";
import { motion } from "framer-motion";
import AuthLayout from "@/layouts/AuthLayout";
import AnimatedLogo from "@/components/AnimatedLogo";
import authService from "../services/authService";

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
      await authService.updateProfile({
        fullName: fullName.trim(),
        dob: dateOfBirth,
        gender,
        heightCm: Number.parseFloat(heightCm),
        weightKg: Number.parseFloat(weightKg),
        activityLevel,
      });

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

  const inputClasses = "w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors placeholder:text-gray-500/70";
  const labelClasses = "block text-lg md:text-xl font-medium mb-2 pl-1 italic";

  return (
    <AuthLayout>
      <div className="text-center mb-8 md:mb-12">
        <AnimatedLogo size="large" className="mx-auto mb-4 md:mb-6" />
        <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-4 md:mb-6">
          Hoàn thiện hồ sơ
        </h2>
        <p className="text-xl md:text-2xl lg:text-3xl">
          Vui lòng điền đầy đủ thông tin để tiếp tục
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6 md:space-y-8 mb-8 md:mb-12">
        <div>
          <label htmlFor="fullName" className={labelClasses}>Họ và tên *</label>
          <input
            id="fullName"
            type="text"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="Nhập họ và tên"
            required
            className={inputClasses}
          />
        </div>

        <div>
          <label htmlFor="dateOfBirth" className={labelClasses}>Ngày sinh *</label>
          <input
            id="dateOfBirth"
            type="date"
            value={dateOfBirth}
            onChange={(e) => setDateOfBirth(e.target.value)}
            required
            className={inputClasses}
          />
        </div>

        <div>
          <label htmlFor="gender" className={labelClasses}>Giới tính *</label>
          <Select value={gender} onValueChange={setGender}>
            <SelectTrigger className={`${inputClasses} h-auto`}>
              <SelectValue placeholder="Chọn giới tính" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="Male">Nam</SelectItem>
              <SelectItem value="Female">Nữ</SelectItem>
              <SelectItem value="Other">Khác</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div className="grid grid-cols-2 gap-4 md:gap-6">
          <div>
            <label htmlFor="height" className={labelClasses}>Chiều cao (cm) *</label>
            <input
              id="height"
              type="number"
              value={heightCm}
              onChange={(e) => setHeightCm(e.target.value)}
              placeholder="170"
              min="50"
              max="300"
              required
              className={inputClasses}
            />
          </div>
          <div>
            <label htmlFor="weight" className={labelClasses}>Cân nặng (kg) *</label>
            <input
              id="weight"
              type="number"
              value={weightKg}
              onChange={(e) => setWeightKg(e.target.value)}
              placeholder="70"
              min="20"
              max="500"
              required
              className={inputClasses}
            />
          </div>
        </div>

        <div>
          <label htmlFor="activityLevel" className={labelClasses}>Mức độ hoạt động</label>
          <Select value={activityLevel} onValueChange={setActivityLevel}>
            <SelectTrigger className={`${inputClasses} h-auto`}>
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

        <div className="flex justify-center pt-4">
          <motion.button
            type="submit"
            disabled={isLoading}
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            transition={{ type: "spring", stiffness: 300 }}
            className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-12 lg:px-16 py-3 md:py-4 text-xl md:text-2xl font-normal disabled:opacity-50 disabled:cursor-not-allowed w-full max-w-md"
          >
            {isLoading ? "Đang cập nhật..." : "Hoàn thành"}
          </motion.button>
        </div>
      </form>
    </AuthLayout>
  );
}