import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Check } from "lucide-react";
import AuthLayout from "@/layouts/AuthLayout";

export default function ResetSuccess() {
  const navigate = useNavigate();

  useEffect(() => {
    const timer = setTimeout(() => {
      navigate("/dashboard");
    }, 3000);

    return () => clearTimeout(timer);
  }, [navigate]);

  return (
    <AuthLayout maxWidth="900px">
      <div className="text-center">
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
    </AuthLayout>
  );
}
