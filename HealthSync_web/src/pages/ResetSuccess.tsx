import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Check } from "lucide-react";
import Footer from "@/components/Footer";
import logo from "@/assets/logo.png";
import { motion } from "framer-motion";

export default function ResetSuccess() {
  const navigate = useNavigate();

  useEffect(() => {
    const timer = setTimeout(() => {
      navigate("/dashboard");
    }, 3000);

    return () => clearTimeout(timer);
  }, [navigate]);

  return (
    <div className="min-h-screen bg-[#D9D7B6] flex flex-col">
      <div className="py-4 md:py-6 px-4 md:px-8">
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
      </div>
      
      <div className="flex-1 flex items-center justify-center px-4 py-8">
        <div className="w-full max-w-[900px] bg-[#FDFBD4] rounded-[50px] shadow-lg p-4 md:p-8 lg:p-12 xl:p-16">
          <div className="max-w-[1010px] mx-auto text-center">
            <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-6 md:mb-8">
              Congratulations!
            </h2>

            <p className="text-2xl md:text-3xl lg:text-4xl xl:text-5xl font-bold mb-12 md:mb-16">
              You have successfully created an account.
            </p>

            <div className="flex justify-center">
              <div className="w-32 h-32 md:w-48 md:h-48 lg:w-64 lg:h-64 flex items-center justify-center">
                <Check className="w-full h-full text-black" strokeWidth={1.5} />
              </div>
            </div>
          </div>
        </div>
      </div>

      <Footer />
    </div>
  );
}
