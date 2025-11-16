import { ReactNode, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";

interface AdminLayoutProps {
  children: ReactNode;
}

export default function AdminLayout({ children }: AdminLayoutProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);

  const menuItems = [
    { path: "/admin/dashboard", label: "Dashboard", icon: "📊" },
    { path: "/admin/users", label: "User Management", icon: "👥" },
    { path: "/admin/content", label: "Content Library", icon: "📚" },
  ];

  const isActive = (path: string) => location.pathname === path;

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  return (
    <div className="flex h-screen bg-[#E8E4D9]">
      <aside
        className={`${
          isSidebarCollapsed ? "w-20" : "w-64"
        } bg-[#4A6F6F] text-white transition-all duration-300 flex flex-col`}
      >
        <div className="p-6 flex items-center gap-3 border-b border-[#5A7F7F]">
          <div className="w-10 h-10 bg-[#E8E4D9] rounded-lg flex items-center justify-center text-[#4A6F6F] font-bold text-xl">
            H
          </div>
          {!isSidebarCollapsed && (
            <span className="text-xl font-semibold">HealthSync</span>
          )}
        </div>

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
              {!isSidebarCollapsed && (
                <span className="font-medium">{item.label}</span>
              )}
            </button>
          ))}
        </nav>

        <button
          onClick={() => setIsSidebarCollapsed(!isSidebarCollapsed)}
          className="p-4 border-t border-[#5A7F7F] hover:bg-[#5A7F7F] transition-colors"
        >
          {isSidebarCollapsed ? "→" : "←"}
        </button>
      </aside>

      <div className="flex-1 flex flex-col overflow-hidden">
        <header className="bg-white shadow-sm px-8 py-4 flex items-center justify-between">
          <h1 className="text-2xl font-bold text-[#4A6F6F]">Dashboard</h1>
          <div className="flex items-center gap-4">
            <button className="w-10 h-10 rounded-full bg-[#4A6F6F] text-white flex items-center justify-center hover:bg-[#5A7F7F] transition-colors">
              👤
            </button>
            <button
              onClick={handleLogout}
              className="px-4 py-2 bg-[#4A6F6F] text-white rounded-lg hover:bg-[#5A7F7F] transition-colors"
            >
              Logout
            </button>
          </div>
        </header>

        <main className="flex-1 overflow-auto p-8">{children}</main>
      </div>
    </div>
  );
}
