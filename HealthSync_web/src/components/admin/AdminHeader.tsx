import { useNavigate } from "react-router-dom";

interface AdminHeaderProps {
  title?: string;
}

export default function AdminHeader({ title = "Dashboard" }: Readonly<AdminHeaderProps>) {
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  return (
    <header className="bg-white shadow-sm px-8 py-4 flex items-center justify-between">
      <h1 className="text-2xl font-bold text-[#4A6F6F]">{title}</h1>
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
  );
}
