import { Search, Menu } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Link } from "react-router-dom";
import logoheader from "@/assets/logoheader.png";
import { motion } from "framer-motion";

export default function Header() {
  return (
    <header className="w-full py-4 md:py-6 px-4 md:px-8 lg:px-12 xl:px-16">
      <div className="max-w-[1434px] mx-auto">
        <div className="flex items-center justify-between gap-4 mb-4 lg:mb-0">
          <Link to="/">
            <motion.img
              src={logoheader}
              alt="HealthSync"
              className="w-32 h-auto"
              animate={{ rotate: [0, -3, 3, 0] }}
              transition={{ repeat: Infinity, duration: 4, ease: "easeInOut" }}
            />
          </Link>

          <div className="hidden lg:flex items-center gap-4 bg-[#ECE6F0] rounded-full px-4 py-3 flex-1 max-w-[355px]">
            <Menu className="w-6 h-6 text-[#49454F] flex-shrink-0" />
            <input
              type="text"
              placeholder="What are you looking for?"
              className="bg-transparent border-none outline-none flex-1 text-[#49454F] placeholder:text-[#49454F] text-sm"
            />
            <Search className="w-6 h-6 text-[#49454F] flex-shrink-0" />
          </div>

          <nav className="flex items-center gap-2 flex-wrap justify-end ml-6">
            <div className="hidden md:flex items-center gap-2">
              <button className="px-3 lg:px-4 py-1.5 bg-white rounded-md border border-black/[0.08] text-xs lg:text-sm font-medium hover:bg-gray-50 whitespace-nowrap">
                Explore ▼
              </button>
              <button className="px-3 lg:px-4 py-1.5 bg-white rounded-md border border-black/[0.08] text-xs lg:text-sm font-medium hover:bg-gray-50 whitespace-nowrap">
                Find talent ▼
              </button>
              <button className="px-3 lg:px-4 py-1.5 bg-white rounded-md border border-black/[0.08] text-xs lg:text-sm font-medium hover:bg-gray-50 whitespace-nowrap">
                Get hired ▼
              </button>
            </div>

            <Button className="hidden md:inline-flex bg-black text-white hover:bg-black/90 rounded-lg px-4 lg:px-6 h-8 lg:h-9 text-sm lg:text-base">
              Blog
            </Button>

            <div className="bg-[#FDFBD4] rounded-2xl flex items-center p-3 gap-3">
              <Link to="/register">
                <Button className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-xl px-3 lg:px-6 h-8 lg:h-9 border border-black text-xs lg:text-base">
                  Sign up
                </Button>
              </Link>
              <Link to="/login">
                <Button className="bg-black text-white hover:bg-black/90 rounded-xl px-3 lg:px-6 h-8 lg:h-9 text-xs lg:text-base">
                  Login
                </Button>
              </Link>
            </div>
          </nav>
        </div>

        <div className="lg:hidden flex items-center gap-4 bg-[#ECE6F0] rounded-full px-4 py-3 mt-4">
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
