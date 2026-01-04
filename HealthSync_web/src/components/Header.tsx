import { useState, useRef, useEffect } from "react";
import { Search, Menu, User, LogOut, ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Link, useNavigate } from "react-router-dom";
import logo from "@/assets/logo.png";
import { motion, AnimatePresence } from "framer-motion";
import { useAuth } from "@/contexts/AuthContext";

export default function Header() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsMenuOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  return (
    <header className="w-full py-4 md:py-6 px-4 md:px-8 lg:px-12 xl:px-16 bg-[#FDFBD4]">
      <div className="max-w-[1434px] mx-auto">
        <div className="flex items-center justify-between gap-4 mb-4 lg:mb-0">
          <Link to="/">
            <h1 className="text-3xl font-bold m-0 text-gray-900 flex items-center gap-2 whitespace-nowrap">
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

          <div className="hidden lg:flex items-center gap-4 bg-[#EBE9C0] rounded-full px-4 py-3 flex-1 max-w-[800px]">
            <Menu className="w-6 h-6 text-[#49454F] flex-shrink-0" />
            <input
              type="text"
              placeholder="What are you looking for?"
              className="bg-transparent border-none outline-none flex-1 text-[#49454F] placeholder:text-[#49454F] text-sm focus:ring-0"
            />
            <Search className="w-6 h-6 text-[#49454F] flex-shrink-0" />
          </div>

          <nav className="flex items-center gap-2 flex-wrap justify-end ml-6">
            <div className="hidden md:flex items-center gap-2">
              <button className="px-3 lg:px-4 py-1.5 bg-white/50 rounded-md border border-black/[0.08] text-xs lg:text-sm font-medium hover:bg-white/80 whitespace-nowrap transition-colors">
                Explore ▼
              </button>
              <button className="px-3 lg:px-4 py-1.5 bg-white/50 rounded-md border border-black/[0.08] text-xs lg:text-sm font-medium hover:bg-white/80 whitespace-nowrap transition-colors">
                Find talent ▼
              </button>
              <button className="px-3 lg:px-4 py-1.5 bg-white/50 rounded-md border border-black/[0.08] text-xs lg:text-sm font-medium hover:bg-white/80 whitespace-nowrap transition-colors">
                Get hired ▼
              </button>
            </div>

            <Button className="hidden md:inline-flex bg-black text-white hover:bg-black/90 rounded-lg px-4 lg:px-6 h-8 lg:h-9 text-sm lg:text-base">
              Blog
            </Button>

            {user ? (
              <div className="relative" ref={menuRef}>
                <button
                  onClick={() => setIsMenuOpen(!isMenuOpen)}
                  className="bg-[#EBE9C0] rounded-2xl flex items-center p-2 pr-4 gap-3 hover:bg-[#EBE9C0]/80 transition-colors"
                >
                  <div className="w-8 h-8 aspect-square rounded-full flex items-center justify-center overflow-hidden bg-[#4A6F6F]">
                    <img
                      src={user.avatar || `https://ui-avatars.com/api/?name=${user.fullName}&background=4A6F6F&color=fff&rounded=true`}
                      alt={user.fullName}
                      className="w-full h-full object-cover rounded-full"
                    />
                  </div>
                  <span className="text-sm font-medium hidden sm:block">{user.fullName}</span>
                  <ChevronDown className={`w-4 h-4 transition-transform ${isMenuOpen ? 'rotate-180' : ''}`} />
                </button>

                <AnimatePresence>
                  {isMenuOpen && (
                    <motion.div
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: 10 }}
                      className="absolute right-0 top-full mt-2 w-48 bg-white rounded-xl shadow-xl border border-gray-100 overflow-hidden z-50"
                    >
                      <div className="p-2 space-y-1">
                        <button
                          onClick={() => {
                            navigate("/profile");
                            setIsMenuOpen(false);
                          }}
                          className="w-full flex items-center gap-2 px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded-lg transition-colors text-left"
                        >
                          <User className="w-4 h-4" />
                          Profile
                        </button>
                        <button
                          onClick={() => {
                            logout();
                            navigate("/");
                            setIsMenuOpen(false);
                          }}
                          className="w-full flex items-center gap-2 px-3 py-2 text-sm text-red-600 hover:bg-red-50 rounded-lg transition-colors text-left"
                        >
                          <LogOut className="w-4 h-4" />
                          Logout
                        </button>
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </div>
            ) : (
              <div className="bg-[#EBE9C0] rounded-2xl flex items-center p-3 gap-3">
                <Link to="/register">
                  <Button className="bg-transparent text-black hover:bg-black/5 rounded-xl px-3 lg:px-6 h-8 lg:h-9 border border-black text-xs lg:text-base shadow-none">
                    Sign up
                  </Button>
                </Link>
                <Link to="/login">
                  <Button className="bg-black text-white hover:bg-black/90 rounded-xl px-3 lg:px-6 h-8 lg:h-9 text-xs lg:text-base">
                    Login
                  </Button>
                </Link>
              </div>
            )}
          </nav>
        </div>

        <div className="lg:hidden flex items-center gap-4 bg-[#EBE9C0] rounded-full px-4 py-3 mt-4">
          <Menu className="w-6 h-6 text-[#49454F] flex-shrink-0" />
          <input
            type="text"
            placeholder="What are you looking for?"
            className="bg-transparent border-none outline-none flex-1 text-[#49454F] placeholder:text-[#49454F] text-sm"
          />
          <Search className="w-6 h-6 text-[#49454F] flex-shrink-0" />
        </div>
      </div>
    </header>
  );
}
