import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/contexts/AuthContext";
import { useToast } from "@/hooks/use-toast";
import { usePermissions } from "@/hooks/usePermissions";
import { Permission } from "@/types/rbac";

interface ProtectedRouteProps {
  children: React.ReactNode;
  requireAdmin?: boolean;
  requiredPermission?: Permission;
  requiredPermissions?: Permission[];
  requireAllPermissions?: boolean; // true = need all, false = need any
}

export default function ProtectedRoute({ 
  children, 
  requireAdmin = false,
  requiredPermission,
  requiredPermissions = [],
  requireAllPermissions = true,
}: Readonly<ProtectedRouteProps>) {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { toast } = useToast();
  const { hasPermission, hasAllPermissions, hasAnyPermission, isAdmin } = usePermissions();

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
    if (requireAdmin && !isAdmin()) {
      toast({
        title: "Không có quyền truy cập",
        description: "Bạn không có quyền truy cập trang này",
        variant: "destructive",
      });
      navigate("/dashboard");
      return;
    }

    // Check specific permission if provided
    if (requiredPermission && !hasPermission(requiredPermission)) {
      toast({
        title: "Không có quyền truy cập",
        description: "Bạn không có quyền truy cập trang này",
        variant: "destructive",
      });
      navigate("/dashboard");
      return;
    }

    // Check multiple permissions if provided
    if (requiredPermissions.length > 0) {
      const hasAccess = requireAllPermissions
        ? hasAllPermissions(requiredPermissions)
        : hasAnyPermission(requiredPermissions);

      if (!hasAccess) {
        toast({
          title: "Không có quyền truy cập",
          description: "Bạn không có quyền truy cập trang này",
          variant: "destructive",
        });
        navigate("/dashboard");
        return;
      }
    }

    // If user profile is not complete and not admin, redirect to complete profile
    if (!requireAdmin && !isAdmin() && !user.isProfileComplete) {
      navigate("/complete-profile");
    }
  }, [user, requireAdmin, requiredPermission, requiredPermissions, requireAllPermissions, navigate, toast, hasPermission, hasAllPermissions, hasAnyPermission, isAdmin]);

  // If no user, show nothing (will redirect)
  if (!user) {
    return null;
  }

  // Check all permission requirements before rendering
  if (requireAdmin && !isAdmin()) {
    return null;
  }

  if (requiredPermission && !hasPermission(requiredPermission)) {
    return null;
  }

  if (requiredPermissions.length > 0) {
    const hasAccess = requireAllPermissions
      ? hasAllPermissions(requiredPermissions)
      : hasAnyPermission(requiredPermissions);
    
    if (!hasAccess) {
      return null;
    }
  }

  return <>{children}</>;
}
