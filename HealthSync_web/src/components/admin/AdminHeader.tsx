import { useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { LogOut, User, Menu } from "lucide-react";
import authService, { ProfileResponse } from "../../services/authService";
import { useAuth } from "../../contexts/AuthContext";

interface AdminHeaderProps {
  title?: string;
  onMobileMenuOpen?: () => void;
}

export default function AdminHeader({ title = "Dashboard", onMobileMenuOpen }: Readonly<AdminHeaderProps>) {
  const navigate = useNavigate();
  const { logout } = useAuth();
  const [profile, setProfile] = useState<ProfileResponse | null>(null);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const data = await authService.getProfile();
        setProfile(data);
      } catch (error) {
        console.error("Failed to fetch profile:", error);
      }
    };
    fetchProfile();
  }, []);

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <header className="bg-white shadow-sm px-4 lg:px-8 py-4 flex items-center justify-between">
      <div className="flex items-center gap-2 lg:gap-4">
        {onMobileMenuOpen && (
          <button
            onClick={onMobileMenuOpen}
            className="lg:hidden text-[#4A6F6F] hover:bg-[#E8E4D9] p-2 rounded-lg"
          >
            <Menu size={24} />
          </button>
        )}
        <h1 className="text-xl lg:text-2xl font-bold text-[#4A6F6F]">{title}</h1>
      </div>

      <div className="flex items-center gap-4 lg:gap-6">
        {/* User Info Section */}
        <div className="flex items-center gap-3">
          {/* Avatar - Fixed width/height to avoid PrimeFlex conflicts */}
          <div className="w-[40px] h-[40px] lg:w-[48px] lg:h-[48px] rounded-full bg-[#4A6F6F] text-white flex items-center justify-center shrink-0 border border-[#4A6F6F] shadow-sm overflow-hidden isolate">
            {profile?.avatarUrl ? (
              <img
                src={profile.avatarUrl}
                alt={profile.fullName || "User"}
                className="w-full h-full object-cover"
              />
            ) : (
              <User size={24} />
            )}
          </div>

          {/* Name and Role */}
          <div className="hidden sm:flex flex-col items-end">
            <span className="font-bold text-gray-800 text-sm leading-tight">
              {profile?.fullName || "Loading..."}
            </span>
            <span className="text-xs text-[#4A6F6F] font-bold mt-0.5">
              {profile?.role || "Admin"}
            </span>
          </div>
        </div>

        {/* Divider */}
        <div className="hidden sm:block h-8 w-px bg-gray-200"></div>

        {/* Logout Button */}
        <button
          onClick={handleLogout}
          className="flex items-center gap-2 px-4 py-2 bg-[#4A6F6F] text-white rounded-lg hover:bg-[#3A5F5F] transition-all shadow-sm font-medium text-sm"
          title="Đăng xuất"
        >
          <LogOut size={18} />
          <span className="hidden sm:inline">Đăng xuất</span>
        </button>
      </div>
    </header>
  );
}
