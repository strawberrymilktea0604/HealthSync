import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Eye, EyeOff } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useToast } from "@/hooks/use-toast";
import { useAuth } from "@/contexts/AuthContext";
import Footer from "@/components/Footer";
import logo from "@/assets/logo.png";
import { motion } from "framer-motion";

export default function Register() {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [step, setStep] = useState<"email" | "verify">("email");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [verificationCode, setVerificationCode] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isSendingCode, setIsSendingCode] = useState(false);
  const navigate = useNavigate();
  const { toast } = useToast();
  const { setUser } = useAuth();

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
      const response = await fetch('http://localhost:5274/api/auth/send-verification-code', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email }),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.Error || 'Failed to send verification code');
      }

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
      const registerResponse = await fetch('http://localhost:5274/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email,
          password,
          verificationCode,
        }),
      });

      if (!registerResponse.ok) {
        let errorMessage = 'Đăng ký thất bại';
        try {
          const error = await registerResponse.json();
          errorMessage = error.Error || errorMessage;
        } catch (parseError) {
          console.error('Failed to parse register error response:', parseError);
        }
        throw new Error(errorMessage);
      }

      const data = await registerResponse.json();
      
      // Update AuthContext with user data
      const userData = {
        userId: data.UserId,
        email: data.Email,
        fullName: data.FullName,
        role: data.Role,
        token: data.Token,
        expiresAt: new Date(data.ExpiresAt),
      };
      
      setUser(userData);

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
    <div className="min-h-screen bg-[#D9D7B6] flex flex-col">
      <div className="py-4 md:py-6 px-4 md:px-8">
        <Link to="/">
          <h1 className="text-3xl font-bold m-0 text-900 flex align-items-center gap-2">
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
        </Link>
      </div>
      
      <div className="flex-1 flex items-center justify-center px-4 py-8 md:py-12">
        <div className="w-full max-w-[1100px] bg-[#FDFBD4] rounded-[50px] shadow-lg p-4 md:p-8 lg:p-12 xl:p-16">
          <div className="max-w-[918px] mx-auto">
            <div className="text-center mb-8 md:mb-12">
              <motion.img
                src={logo}
                alt="HealthSync"
                className="w-40 h-auto mx-auto mb-4 md:mb-6"
                animate={{ rotate: [0, -5, 5, 0] }}
                transition={{ repeat: Infinity, duration: 3, ease: "easeInOut" }}
              />
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

                <div className="relative">
                  <input
                    type={showPassword ? "text" : "password"}
                    placeholder="Choose a password (min. 8 characters)"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                    minLength={8}
                    className="w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors pr-16 md:pr-20"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-4 md:right-6 top-1/2 -translate-y-1/2 text-black hover:opacity-70 transition-opacity"
                  >
                    {showPassword ? (
                      <Eye className="w-8 h-8 md:w-12 md:h-12 lg:w-16 lg:h-16" />
                    ) : (
                      <EyeOff className="w-8 h-8 md:w-12 md:h-12 lg:w-16 lg:h-16" />
                    )}
                  </button>
                </div>

                <div className="relative">
                  <input
                    type={showConfirmPassword ? "text" : "password"}
                    placeholder="Confirm your password"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    required
                    minLength={8}
                    className="w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors pr-16 md:pr-20"
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    className="absolute right-4 md:right-6 top-1/2 -translate-y-1/2 text-black hover:opacity-70 transition-opacity"
                  >
                    {showConfirmPassword ? (
                      <Eye className="w-8 h-8 md:w-12 md:h-12 lg:w-16 lg:h-16" />
                    ) : (
                      <EyeOff className="w-8 h-8 md:w-12 md:h-12 lg:w-16 lg:h-16" />
                    )}
                  </button>
                </div>

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
          </div>
        </div>
      </div>

      <Footer />
    </div>
  );
}
