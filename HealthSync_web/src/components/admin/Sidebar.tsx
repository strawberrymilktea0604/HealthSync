import { useNavigate, useLocation } from "react-router-dom";

interface SidebarProps {
  isCollapsed: boolean;
}

export default function Sidebar({ isCollapsed }: SidebarProps) {
  const navigate = useNavigate();
  const location = useLocation();

  const menuItems = [
    { path: "/admin/dashboard", label: "Dashboard", icon: "📊" },
    { path: "/admin/users", label: "User Management", icon: "👥" },
    { path: "/admin/content", label: "Content Library", icon: "📚" },
  ];

  const isActive = (path: string) => location.pathname === path;

  return (
    <nav className="flex-1 py-6">
      {menuItems.map((item) => (
        <button
          key={item.path}
          onClick={() => navigate(item.path)}
          className={`w-full flex items-center gap-4 px-6 py-4 transition-colors ${
            isActive(item.path)
              ? "bg-[#5A7F7F] border-l-4 border-[#E8E4D9]"
              : "hover:bg-[#5A7F7F]"
          }`}
        >
          <span className="text-2xl">{item.icon}</span>
          {!isCollapsed && <span className="font-medium">{item.label}</span>}
        </button>
      ))}
    </nav>
  );
}
