import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Eye, EyeOff } from "lucide-react";
import { Button } from "@/components/ui/button";
import Footer from "@/components/Footer";
import logo from "@/assets/logo.png";
import logoheader from "@/assets/logoheader.png";
import { motion } from "framer-motion";

export default function Register() {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    navigate("/verify-email");
  };

  return (
    <div className="min-h-screen bg-[#D9D7B6] flex flex-col">
      <div className="py-4 md:py-6 px-4 md:px-8">
        <Link to="/">
          <img src={logoheader} alt="HealthSync" className="w-32 h-auto" />
        </Link>
      </div>
      
      <div className="flex-1 flex items-center justify-center px-4 py-8 md:py-12">
        <div className="w-full max-w-[1100px] bg-[#FDFBD4] rounded-[50px] shadow-lg p-4 md:p-8 lg:p-12 xl:p-16">
          <div className="max-w-[918px] mx-auto">
            <div className="text-center mb-8 md:mb-12">
              <motion.img
                src={logo}
                alt="HealthSync"
                className="w-40 h-auto mx-auto mb-4 md:mb-6"
                animate={{ rotate: [0, -5, 5, 0] }}
                transition={{ repeat: Infinity, duration: 3, ease: "easeInOut" }}
              />
              <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-6 md:mb-8">
                Create an account
              </h2>
            </div>

            <form onSubmit={handleSubmit} className="space-y-6 md:space-y-8 mb-8 md:mb-12">
              <div>
                <input
                  type="email"
                  placeholder="Your email address"
                  required
                  className="w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors"
                />
              </div>

              <div className="relative">
                <input
                  type={showPassword ? "text" : "password"}
                  placeholder="Choose a password (min. 8 characters)"
                  required
                  minLength={8}
                  className="w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors pr-16 md:pr-20"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-4 md:right-6 top-1/2 -translate-y-1/2 text-black hover:opacity-70 transition-opacity"
                >
                  {showPassword ? (
                    <Eye className="w-8 h-8 md:w-12 md:h-12 lg:w-16 lg:h-16" />
                  ) : (
                    <EyeOff className="w-8 h-8 md:w-12 md:h-12 lg:w-16 lg:h-16" />
                  )}
                </button>
              </div>

              <div className="relative">
                <input
                  type={showConfirmPassword ? "text" : "password"}
                  placeholder="Confirm your password"
                  required
                  minLength={8}
                  className="w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors pr-16 md:pr-20"
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  className="absolute right-4 md:right-6 top-1/2 -translate-y-1/2 text-black hover:opacity-70 transition-opacity"
                >
                  {showConfirmPassword ? (
                    <Eye className="w-8 h-8 md:w-12 md:h-12 lg:w-16 lg:h-16" />
                  ) : (
                    <EyeOff className="w-8 h-8 md:w-12 md:h-12 lg:w-16 lg:h-16" />
                  )}
                </button>
              </div>

              <div className="flex justify-center">
                <Button
                  type="submit"
                  className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-12 lg:px-16 py-6 md:py-8 text-2xl md:text-3xl lg:text-4xl h-auto font-normal"
                >
                  Sign up
                </Button>
              </div>
            </form>

            <div className="text-center flex flex-col sm:flex-row items-center justify-center gap-4 md:gap-6">
              <span className="text-xl md:text-2xl lg:text-3xl">Do you have an account?</span>
              <Link to="/login">
                <motion.button
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                  transition={{ type: "spring", stiffness: 300 }}
                  className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-12 lg:px-16 py-6 md:py-8 text-2xl md:text-3xl lg:text-4xl h-auto font-normal"
                >
                  Sign in
                </motion.button>
              </Link>
            </div>
          </div>
        </div>
      </div>

      <Footer />
    </div>
  );
}
