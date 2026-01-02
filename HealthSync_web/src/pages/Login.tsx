import { useState, useEffect } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { useToast } from "@/hooks/use-toast";
import { useAuth } from "@/contexts/AuthContext";
import { motion } from "framer-motion";
import AuthLayout from "@/layouts/AuthLayout";
import AnimatedLogo from "@/components/AnimatedLogo";
import PasswordInput from "@/components/PasswordInput";
import authService from "../services/authService";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { login, isLoading } = useAuth();
  const { toast } = useToast();

  useEffect(() => {
    const error = searchParams.get("error");
    if (error) {
      toast({
        title: "Đăng nhập thất bại",
        description: error,
        variant: "destructive",
      });
      // Optional: Clean up the URL
      globalThis.history.replaceState({}, '', '/login');
    }
  }, [searchParams, toast]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const userData = await login(email, password);

      console.log("Login userData:", userData);
      console.log("User role:", userData.role);

      toast({
        title: "Đăng nhập thành công!",
        description: `Chào mừng ${userData.fullName}`,
      });

      if (userData.role === "Admin") {
        console.log("Navigating to admin dashboard");
        navigate("/admin/dashboard", { replace: true });
      } else {
        console.log("Navigating to customer dashboard");
        navigate("/dashboard", { replace: true });
      }
    } catch (error) {
      toast({
        title: "Đăng nhập thất bại",
        description: error instanceof Error ? error.message : "Đã xảy ra lỗi",
        variant: "destructive",
      });
    }
  };

  return (
    <AuthLayout>
      <div className="text-center mb-8 md:mb-12">
        <AnimatedLogo size="large" className="mx-auto mb-4 md:mb-6" />
        <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-6 md:mb-8">
          Welcome back!
        </h2>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6 md:space-y-8 mb-8 md:mb-12">
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
          placeholder="Enter a password (min. 8 characters)"
          value={password}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
          required
        />

        <div className="text-right">
          <Link
            to="/forgot-password"
            className="text-xl md:text-2xl lg:text-3xl xl:text-4xl hover:opacity-70 transition-opacity"
          >
            Forgot password?
          </Link>
        </div>

        <div className="flex justify-center">
          <motion.button
            type="submit"
            disabled={isLoading}
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            transition={{ type: "spring", stiffness: 300 }}
            className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-12 lg:px-16 py-3 md:py-4 text-xl md:text-2xl font-normal disabled:opacity-50 disabled:cursor-not-allowed w-full max-w-md"
          >
            {isLoading ? "Logging in..." : "Login"}
          </motion.button>
        </div>
      </form>

      <div className="mb-8 md:mb-12 flex justify-center">
        <button
          onClick={() => globalThis.location.href = authService.getGoogleAuthUrl()}
          className="w-full max-w-md bg-white hover:bg-gray-50 text-black rounded-full border border-black py-3 md:py-4 text-xl md:text-2xl font-normal flex items-center justify-center gap-2 md:gap-3 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 48 48" className="w-6 h-6 md:w-8 md:h-8">
            <path fill="#FFC107" d="M43.611,20.083H42V20H24v8h11.303c-1.649,4.657-6.08,8-11.303,8c-6.627,0-12-5.373-12-12c0-6.627,5.373-12,12-12c3.059,0,5.842,1.154,7.961,3.039l5.657-5.657C34.046,6.053,29.268,4,24,4C12.955,4,4,12.955,4,24c0,11.045,8.955,20,20,20c11.045,0,20-8.955,20-20C44,22.659,43.862,21.35,43.611,20.083z" />
            <path fill="#FF3D00" d="M6.306,14.691l6.571,4.819C14.655,15.108,18.961,12,24,12c3.059,0,5.842,1.154,7.961,3.039l5.657-5.657C34.046,6.053,29.268,4,24,4C16.318,4,9.656,8.337,6.306,14.691z" />
            <path fill="#4CAF50" d="M24,44c5.166,0,9.86-1.977,13.409-5.192l-6.19-5.238C29.211,35.091,26.715,36,24,36c-5.202,0-9.619-3.317-11.283-7.946l-6.522,5.025C9.505,39.556,16.227,44,24,44z" />
            <path fill="#1976D2" d="M43.611,20.083H42V20H24v8h11.303c-0.792,2.237-2.231,4.166-4.087,5.571c0.001-0.001,0.002-0.001,0.003-0.002l6.19,5.238C36.971,39.205,44,34,44,24C44,22.659,43.862,21.35,43.611,20.083z" />
          </svg>
          Sign in with Google
        </button>
      </div>

      <div className="text-center flex flex-col sm:flex-row items-center justify-center gap-4 md:gap-6">
        <span className="text-xl md:text-2xl">Don&apos;t have an account?</span>
        <Link to="/register">
          <Button className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-10 py-3 md:py-4 text-xl md:text-2xl h-auto font-normal">
            Register
          </Button>
        </Link>
      </div>
    </AuthLayout>
  );
}