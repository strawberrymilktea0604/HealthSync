import { useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "@/contexts/AuthContext";
import { useToast } from "@/hooks/use-toast";

export default function GoogleCallback() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { setUser } = useAuth();
  const { toast } = useToast();

  useEffect(() => {
    const handleCallback = async () => {
      const token = searchParams.get("token");
      const userId = searchParams.get("userId");
      const email = searchParams.get("email");
      const fullName = searchParams.get("fullName");
      const role = searchParams.get("role");
      const expiresAt = searchParams.get("expiresAt");
      const requiresPassword = searchParams.get("requiresPassword");

      if (!token || !userId || !email || !fullName || !role || !expiresAt) {
        toast({
          title: "Google Login Failed",
          description: "Invalid callback parameters",
          variant: "destructive",
        });
        navigate("/login");
        return;
      }

      try {
        // Set user data directly since we got it from backend
        const userData = {
          userId: parseInt(userId),
          email,
          fullName,
          role,
          token,
          expiresAt: new Date(expiresAt),
        };

        // Update auth context with user data
        setUser(userData);

        // Check if user needs to create password (first-time Google login)
        if (requiresPassword === "true") {
          toast({
            title: "Tạo mật khẩu",
            description: "Vui lòng tạo mật khẩu cho tài khoản của bạn",
          });
          navigate("/create-password-google");
          return;
        }

        // Successfully logged in via Google - navigate to dashboard
        toast({
          title: "Đăng nhập thành công!",
          description: `Chào mừng ${fullName}`,
        });
        navigate("/dashboard");
      } catch (error) {
        toast({
          title: "Google Login Failed",
          description: "Failed to process login",
          variant: "destructive",
        });
        navigate("/login");
      }
    };

    handleCallback();
  }, [searchParams, navigate, setUser, toast]);

  return (
    <div className="min-h-screen bg-[#D9D7B6] flex items-center justify-center">
      <div className="text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-black mx-auto mb-4"></div>
        <p className="text-xl">Processing Google login...</p>
      </div>
    </div>
  );
}