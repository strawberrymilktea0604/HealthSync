import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { ArrowLeft, Eye, EyeOff } from "lucide-react";
import { Button } from "@/components/ui/button";
import Footer from "@/components/Footer";
import logoheader from "@/assets/logoheader.png";

export default function ResetPassword() {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    navigate("/reset-success");
  };

  return (
    <div className="min-h-screen bg-[#D9D7B6] flex flex-col">
      <div className="py-4 md:py-6 px-4 md:px-8">
        <Link to="/">
          <img src={logoheader} alt="HealthSync" className="w-32 h-auto" />
        </Link>
      </div>
      
      <div className="flex-1 flex items-center justify-center px-4 py-8">
        <div className="w-full max-w-[900px] bg-[#FDFBD4] rounded-[50px] shadow-lg p-4 md:p-8 lg:p-12 xl:p-16 relative">
          <Link to="/verify-password-reset" className="absolute left-6 md:left-12 top-6 md:top-12">
            <ArrowLeft className="w-10 h-10 md:w-12 md:h-12 text-black hover:opacity-70 transition-opacity" />
          </Link>

          <div className="max-w-[883px] mx-auto text-center">
            <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-8 md:mb-12">
              Create new password
            </h2>

            <form onSubmit={handleSubmit} className="space-y-6 md:space-y-8">
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

              <div className="flex justify-center pt-6 md:pt-8">
                <Button
                  type="submit"
                  className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-12 lg:px-16 py-6 md:py-8 text-2xl md:text-3xl lg:text-4xl h-auto font-normal"
                >
                  Reset password
                </Button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <Footer />
    </div>
  );
}
