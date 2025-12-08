import { Link } from "react-router-dom";
import Footer from "@/components/Footer";
import AnimatedLogo from "@/components/AnimatedLogo";
import { ArrowLeft } from "lucide-react";

interface AuthLayoutProps {
  readonly children: React.ReactNode;
  readonly showBackButton?: boolean;
  readonly backPath?: string;
  readonly maxWidth?: string;
}

export default function AuthLayout({ 
  children, 
  showBackButton, 
  backPath,
  maxWidth = "1100px"
}: AuthLayoutProps) {
  return (
    <div className="min-h-screen bg-[#D9D7B6] flex flex-col">
      {/* Header */}
      <div className="py-4 md:py-6 px-4 md:px-8">
        <Link to="/">
          <h1 className="text-3xl font-bold m-0 text-900 flex align-items-center gap-2">
            Welcome to <AnimatedLogo size="small" />
          </h1>
        </Link>
      </div>

      {/* Main Content Container */}
      <div className="flex-1 flex items-center justify-center px-4 py-8 md:py-12">
        <div 
          className="w-full bg-[#FDFBD4] rounded-[50px] shadow-lg p-4 md:p-8 lg:p-12 xl:p-16 relative"
          style={{ maxWidth }}
        >
          
          {/* Back Button Logic */}
          {showBackButton && backPath && (
             <Link to={backPath} className="absolute left-6 md:left-12 top-6 md:top-12">
               <ArrowLeft className="w-10 h-10 md:w-12 md:h-12 text-black hover:opacity-70 transition-opacity" />
             </Link>
          )}

          <div className="max-w-[918px] mx-auto">
            {children}
          </div>
        </div>
      </div>

      <Footer />
    </div>
  );
}
