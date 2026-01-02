import { ReactNode, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "@/contexts/AuthContext";
import { usePermissions } from "@/hooks/usePermissions";
import { Permission } from "@/types/rbac";
import {
  LayoutDashboard,
  Users,
  Menu,
  X,
  LogOut,
  ChevronLeft,
  ChevronRight,
  User,
  Dumbbell,
  Apple,
} from "lucide-react";
import logoHeader from "@/assets/logoheader.png";
import AdminHeader from "./AdminHeader";

interface AdminLayoutProps {
  children: ReactNode;
}

interface MenuItem {
  path: string;
  label: string;
  icon: React.ElementType;
  permission: Permission;
}

export default function AdminLayout({ children }: Readonly<AdminLayoutProps>) {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuth();
  const { hasPermission } = usePermissions();
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  // Define menu items with required permissions
  const menuItems: MenuItem[] = [
    {
      path: "/admin/dashboard",
      label: "Dashboard",
      icon: LayoutDashboard,
      permission: Permission.VIEW_ADMIN_DASHBOARD,
    },
    {
      path: "/admin/users",
      label: "User Management",
      icon: Users,
      permission: Permission.VIEW_USERS,
    },
    {
      path: "/admin/exercises",
      label: "Exercise Management",
      icon: Dumbbell,
      permission: Permission.VIEW_EXERCISES,
    },
    {
      path: "/admin/foods",
      label: "Food Management",
      icon: Apple,
      permission: Permission.VIEW_FOODS,
    },
  ];

  // Filter menu items based on user permissions
  const accessibleMenuItems = menuItems.filter((item) =>
    hasPermission(item.permission)
  );

  const isActive = (path: string) => location.pathname === path;

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const handleMenuClick = (path: string) => {
    navigate(path);
    setIsMobileMenuOpen(false);
  };

  return (
    <div className="flex h-screen bg-[#E8E4D9] overflow-hidden">
      {/* Mobile Menu Overlay */}
      {isMobileMenuOpen && (
        <button
          type="button"
          className="fixed inset-0 bg-black bg-opacity-50 z-40 lg:hidden cursor-default"
          onClick={() => setIsMobileMenuOpen(false)}
          aria-label="Close mobile menu"
        />
      )}

      {/* Sidebar */}
      <aside
        className={`
          ${isSidebarCollapsed ? "w-20" : "w-64"} 
          bg-[#4A6F6F] text-white transition-all duration-300 flex flex-col
          fixed lg:static inset-y-0 left-0 z-50
          ${isMobileMenuOpen ? "translate-x-0" : "-translate-x-full lg:translate-x-0"}
        `}
      >
        {/* Logo Section */}
        <div className="p-6 flex items-center justify-between border-b border-[#5A7F7F]">
          <div className="flex items-center gap-3">
            <img src={logoHeader} alt="HealthSync Logo" className="w-10 h-10 rounded-lg" />
            {!isSidebarCollapsed && (
              <span className="text-xl font-semibold">HealthSync Admin</span>
            )}
          </div>
          {/* Mobile Close Button */}
          <button
            onClick={() => setIsMobileMenuOpen(false)}
            className="lg:hidden text-white hover:bg-[#5A7F7F] p-2 rounded-lg"
          >
            <X size={20} />
          </button>
        </div>

        {/* Navigation Menu */}
        <nav className="flex-1 py-6 overflow-y-auto">
          {accessibleMenuItems.map((item) => {
            const Icon = item.icon;
            return (
              <button
                key={item.path}
                onClick={() => handleMenuClick(item.path)}
                className={`w-full flex items-center gap-4 px-6 py-4 transition-colors ${isActive(item.path)
                  ? "bg-[#5A7F7F] border-l-4 border-[#E8E4D9]"
                  : "hover:bg-[#5A7F7F]"
                  }`}
                title={isSidebarCollapsed ? item.label : ""}
              >
                <Icon size={24} />
                {!isSidebarCollapsed && (
                  <span className="font-medium">{item.label}</span>
                )}
              </button>
            );
          })}
        </nav>

        {/* Collapse Toggle Button */}
        <button
          onClick={() => setIsSidebarCollapsed(!isSidebarCollapsed)}
          className="hidden lg:flex p-4 border-t border-[#5A7F7F] hover:bg-[#5A7F7F] transition-colors items-center justify-center"
          title={isSidebarCollapsed ? "Mở rộng" : "Thu gọn"}
        >
          {isSidebarCollapsed ? (
            <ChevronRight size={20} />
          ) : (
            <ChevronLeft size={20} />
          )}
        </button>
      </aside>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <AdminHeader
          title={accessibleMenuItems.find((item) => isActive(item.path))?.label || "Admin Panel"}
          onMobileMenuOpen={() => setIsMobileMenuOpen(true)}
        />

        {/* Main Content */}
        <main className="flex-1 overflow-auto p-4 lg:p-8">{children}</main>
      </div>
    </div>
  );
}
