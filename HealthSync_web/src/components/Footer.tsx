import { Twitter, Facebook, Instagram } from "lucide-react";
import logo from "@/assets/logo.png";

export default function Footer() {
  return (
    <footer className="w-full py-8 md:py-12 px-4 md:px-8 lg:px-12 xl:px-16 mt-12 md:mt-20">
      <div className="max-w-[1434px] mx-auto">
        <div className="flex items-center justify-between mb-6 md:mb-8 flex-wrap gap-4">
          <img src={logo} alt="HealthSync" className="w-32 h-auto" />
          
          <div className="bg-[#FDFBD4] rounded-2xl p-2 md:p-3 flex items-center gap-3 md:gap-4">
            <button className="hover:opacity-80 transition-opacity">
              <Twitter className="w-9 h-9 md:w-11 md:h-11 stroke-black stroke-[1.5]" />
            </button>
            <button className="hover:opacity-80 transition-opacity">
              <Facebook className="w-9 h-9 md:w-11 md:h-11 stroke-black stroke-[1.5]" />
            </button>
            <button className="hover:opacity-80 transition-opacity">
              <Instagram className="w-9 h-9 md:w-11 md:h-11 stroke-black stroke-[1.5]" />
            </button>
          </div>
        </div>

        <div className="border-t border-black mb-6 md:mb-8"></div>

        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-6 md:gap-8 mb-6 md:mb-8">
          <div>
            <h3 className="font-bold text-lg md:text-xl mb-3 md:mb-4">Inspiration</h3>
            <ul className="space-y-2 text-sm md:text-base lg:text-lg font-light">
              <li className="hover:opacity-80 transition-opacity cursor-pointer">Term&Conditions</li>
            </ul>
          </div>

          <div>
            <h3 className="font-bold text-lg md:text-xl mb-3 md:mb-4">Support</h3>
            <ul className="space-y-2 text-sm md:text-base lg:text-lg font-light">
              <li className="hover:opacity-80 transition-opacity cursor-pointer">Cookies</li>
            </ul>
          </div>

          <div>
            <h3 className="font-bold text-lg md:text-xl mb-3 md:mb-4">About</h3>
            <ul className="space-y-2 text-sm md:text-base lg:text-lg font-light">
              <li className="hover:opacity-80 transition-opacity cursor-pointer">Freelancers</li>
            </ul>
          </div>

          <div>
            <h3 className="font-bold text-lg md:text-xl mb-3 md:mb-4">Blog</h3>
          </div>

          <div>
            <h3 className="font-bold text-lg md:text-xl mb-3 md:mb-4">PTs</h3>
            <ul className="space-y-2 text-sm md:text-base lg:text-lg font-light">
              <li className="hover:opacity-80 transition-opacity cursor-pointer">Resources</li>
              <li className="hover:opacity-80 transition-opacity cursor-pointer">Tags</li>
            </ul>
          </div>
        </div>

        <div className="border-t border-black pt-4">
          <p className="text-sm md:text-base lg:text-lg font-light">© healthsync 2025</p>
        </div>
      </div>
    </footer>
  );
}
