import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { useToast } from "@/hooks/use-toast";
import { useAuth } from "@/contexts/AuthContext";
import { motion } from "framer-motion";
import AuthLayout from "@/layouts/AuthLayout";
import AnimatedLogo from "@/components/AnimatedLogo";
import PasswordInput from "@/components/PasswordInput";
import authService from "../services/authService";

export default function Register() {
  const [step, setStep] = useState<"email" | "verify">("email");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [verificationCode, setVerificationCode] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isSendingCode, setIsSendingCode] = useState(false);
  const navigate = useNavigate();
  const { toast } = useToast();
  useAuth();

  // Gửi mã xác thực
  const handleSendCode = async (e: React.FormEvent) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      toast({
        title: "Lỗi",
        description: "Mật khẩu không khớp",
        variant: "destructive",
      });
      return;
    }

    if (password.length < 8) {
      toast({
        title: "Lỗi",
        description: "Mật khẩu phải có ít nhất 8 ký tự",
        variant: "destructive",
      });
      return;
    }

    setIsSendingCode(true);
    try {
      await authService.sendVerificationCode({ email });

      toast({
        title: "Thành công!",
        description: "Mã xác thực đã được gửi đến email của bạn",
      });
      setStep("verify");
    } catch (error) {
      toast({
        title: "Lỗi",
        description: error instanceof Error ? error.message : "Không thể gửi mã xác thực",
        variant: "destructive",
      });
    } finally {
      setIsSendingCode(false);
    }
  };

  // Xác thực và tạo tài khoản
  const handleVerifyAndRegister = async (e: React.FormEvent) => {
    e.preventDefault();

    setIsLoading(true);
    try {
      // Register user (backend will verify code automatically)
      await authService.register({
        email,
        password,
        verificationCode,
      });

      toast({
        title: "Đăng ký thành công!",
        description: "Chào mừng bạn đến với HealthSync",
      });

      // Redirect to success page, then to dashboard
      navigate("/register-success");
    } catch (error) {
      console.error('Registration error:', error);
      toast({
        title: "Lỗi",
        description: error instanceof Error ? error.message : "Đăng ký thất bại",
        variant: "destructive",
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout>
      <div className="text-center mb-8 md:mb-12">
        <AnimatedLogo size="large" className="mx-auto mb-4 md:mb-6" />
        <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-6 md:mb-8">
          Create an account
        </h2>
      </div>

      {step === "email" ? (
        <form onSubmit={handleSendCode} className="space-y-6 md:space-y-8 mb-8 md:mb-12">
          <div>
            <input
              type="email"
              placeholder="Your email address"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors"
            />
          </div>

          <PasswordInput
            placeholder="Choose a password (min. 8 characters)"
            value={password}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
            required
            minLength={8}
          />

          <PasswordInput
            placeholder="Confirm your password"
            value={confirmPassword}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setConfirmPassword(e.target.value)}
            required
            minLength={8}
          />

          <div className="flex justify-center">
            <Button
              type="submit"
              disabled={isSendingCode}
              className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-10 py-2 text-xl h-auto font-normal disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isSendingCode ? "Đang gửi..." : "Sign up"}
            </Button>
          </div>
        </form>
      ) : (
        <form onSubmit={handleVerifyAndRegister} className="space-y-6 md:space-y-8 mb-8 md:mb-12">
          <div className="text-center mb-6">
            <p className="text-xl md:text-2xl">
              Mã xác thực đã được gửi đến <strong>{email}</strong>
            </p>
          </div>

          <div>
            <input
              type="text"
              placeholder="Enter verification code"
              value={verificationCode}
              onChange={(e) => setVerificationCode(e.target.value)}
              required
              maxLength={6}
              className="w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors text-center tracking-widest"
            />
          </div>

          <div className="flex justify-center gap-4">
            <Button
              type="button"
              onClick={() => setStep("email")}
              className="bg-white text-black hover:bg-gray-100 rounded-full border border-black px-8 py-2 text-lg h-auto font-normal"
            >
              Back
            </Button>
            <Button
              type="submit"
              disabled={isLoading}
              className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-10 py-2 text-xl h-auto font-normal disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? "Đang xử lý..." : "Sign up"}
            </Button>
          </div>

          <div className="text-center">
            <button
              type="button"
              onClick={handleSendCode}
              disabled={isSendingCode}
              className="text-lg md:text-xl hover:underline disabled:opacity-50"
            >
              {isSendingCode ? "Đang gửi..." : "Resend code"}
            </button>
          </div>
        </form>
      )}

      <div className="text-center flex flex-row items-center justify-center gap-4">
        <span className="text-lg">Do you have an account?</span>
        <Link to="/login">
          <motion.button
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            transition={{ type: "spring", stiffness: 300 }}
            className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-10 py-2 text-xl h-auto font-normal"
          >
            Sign in
          </motion.button>
        </Link>
      </div>
    </AuthLayout>
  );
}
