import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import Footer from "@/components/Footer";
import logo from "@/assets/logo.png";
import { motion } from "framer-motion";

export default function VerifyEmail() {
  const [code, setCode] = useState(["", "", "", "", "", ""]);
  const navigate = useNavigate();

  const handleCodeChange = (index: number, value: string) => {
    if (value.length <= 1 && /^\d*$/.test(value)) {
      const newCode = [...code];
      newCode[index] = value;
      setCode(newCode);

      if (value && index < 5) {
        const nextInput = document.getElementById(`code-${index + 1}`);
        nextInput?.focus();
      }
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    navigate("/register-success");
  };

  return (
    <div className="min-h-screen bg-[#D9D7B6] flex flex-col">
      <div className="py-4 md:py-6 px-4 md:px-8">
        <Link to="/">
          <h1 className="text-3xl font-bold m-0 text-900 flex align-items-center gap-2">
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
      </div>
      
      <div className="flex-1 flex items-center justify-center px-4 py-8">
        <div className="w-full max-w-[900px] bg-[#FDFBD4] rounded-[50px] shadow-lg p-4 md:p-8 lg:p-12 xl:p-16 relative">
          <Link to="/register" className="absolute left-6 md:left-12 top-6 md:top-12">
            <ArrowLeft className="w-10 h-10 md:w-12 md:h-12 text-black hover:opacity-70 transition-opacity" />
          </Link>

          <div className="max-w-[759px] mx-auto text-center">
            <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-6 md:mb-8">
              Confirm your email
            </h2>

            <p className="text-xl md:text-2xl lg:text-3xl xl:text-4xl mb-8 md:mb-12">
              Please enter the verification code sent to email<br />
              lm*******************@gmail.com
            </p>

            <form onSubmit={handleSubmit} className="space-y-8 md:space-y-12">
              <div className="flex justify-center gap-2 md:gap-4">
                {code.map((digit, index) => (
                  <input
                    key={`otp-${index}`}
                    id={`code-${index}`}
                    type="text"
                    maxLength={1}
                    value={digit}
                    onChange={(e) => handleCodeChange(index, e.target.value)}
                    className="w-12 h-16 md:w-16 md:h-20 lg:w-20 lg:h-24 text-center text-2xl md:text-3xl lg:text-4xl bg-transparent rounded-2xl border-4 md:border-6 border-[#9E9D85] outline-none focus:border-black transition-colors"
                  />
                ))}
              </div>

              <div className="text-xl md:text-2xl lg:text-3xl">
                <span className="text-black">Didn't receive the code?</span>{" "}
                <button type="button" className="text-gray-400 hover:text-gray-600">
                  Resend (58)
                </button>
              </div>

              <div className="flex justify-center">
                <Button
                  type="submit"
                  className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-12 lg:px-16 py-6 md:py-8 text-2xl md:text-3xl lg:text-4xl h-auto font-normal"
                >
                  Confirm
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
