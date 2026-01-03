import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import AuthLayout from "@/layouts/AuthLayout";
import { useToast } from "@/hooks/use-toast";
import authService from "@/services/authService";

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { toast } = useToast();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!email) {
      toast({
        title: "Error",
        description: "Please enter your email",
        variant: "destructive",
      });
      return;
    }

    setIsLoading(true);
    try {
      await authService.forgotPassword(email.trim());

      toast({
        title: "Request Sent",
        description: "If an account exists with this email, you will receive a verification code.",
      });

      // Navigate to verification page, passing the email in state
      navigate("/verify-password-reset", { state: { email } });
    } catch (error: any) {
      const errorMessage = error.response?.data?.error || error.response?.data?.Error || "Failed to send request. Please try again later.";
      toast({
        title: "Error",
        description: errorMessage,
        variant: "destructive",
      });
      console.error(error);
    } finally {
      setIsLoading(false);
    }
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
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors"
            />
          </div>

          <div className="flex justify-center">
            <Button
              type="submit"
              disabled={isLoading}
              className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-12 lg:px-16 py-3 md:py-4 text-xl md:text-2xl h-auto font-normal disabled:opacity-50"
            >
              {isLoading ? "Sending..." : "Send request"}
            </Button>
          </div>
        </form>
      </div>
    </AuthLayout>
  );
}
