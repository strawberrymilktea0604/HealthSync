import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import AuthLayout from "@/layouts/AuthLayout";

export default function ForgotPassword() {
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    navigate("/verify-password-reset");
  };

  return (
    <AuthLayout maxWidth="900px">
      <div className="text-center">
        <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-6 md:mb-8">
          Account recovery
        </h2>

        <p className="text-xl md:text-2xl lg:text-3xl xl:text-4xl mb-8 md:mb-12">
          Enter email to send password recovery request
        </p>

        <form onSubmit={handleSubmit} className="space-y-8 md:space-y-12">
          <div>
            <input
              type="email"
              placeholder="Your email address"
              required
              className="w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors"
            />
          </div>

          <div className="flex justify-center">
            <Button
              type="submit"
              className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-12 lg:px-16 py-6 md:py-8 text-2xl md:text-3xl lg:text-4xl h-auto font-normal"
            >
              Send request
            </Button>
          </div>
        </form>
      </div>
    </AuthLayout>
  );
}
