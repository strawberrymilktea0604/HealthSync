import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/contexts/AuthContext";
import { useToast } from "@/hooks/use-toast";

interface ProtectedRouteProps {
  children: React.ReactNode;
  requireAdmin?: boolean;
}

export default function ProtectedRoute({ children, requireAdmin = false }: ProtectedRouteProps) {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { toast } = useToast();

  useEffect(() => {
    // If not logged in, redirect to login
    if (!user) {
      toast({
        title: "Yêu cầu đăng nhập",
        description: "Vui lòng đăng nhập để tiếp tục",
        variant: "destructive",
      });
      navigate("/login");
      return;
    }

    // If page requires admin but user is not admin
    if (requireAdmin && user.role !== "Admin") {
      toast({
        title: "Không có quyền truy cập",
        description: "Bạn không có quyền truy cập trang này",
        variant: "destructive",
      });
      navigate("/dashboard");
      return;
    }

    // If user is admin but trying to access customer dashboard
    if (!requireAdmin && user.role === "Admin") {
      console.log("Admin detected on customer page, redirecting to admin dashboard");
      navigate("/admin/dashboard", { replace: true });
      return;
    }
  }, [user, requireAdmin, navigate, toast]);

  // If no user, show nothing (will redirect)
  if (!user) {
    return null;
  }

  // If admin trying to access customer page, show nothing (will redirect)
  if (!requireAdmin && user.role === "Admin") {
    return null;
  }

  // If non-admin trying to access admin page, show nothing (will redirect)
  if (requireAdmin && user.role !== "Admin") {
    return null;
  }

  return <>{children}</>;
}
