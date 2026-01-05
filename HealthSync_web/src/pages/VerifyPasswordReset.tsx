import { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { Button } from "@/components/ui/button";
import AuthLayout from "@/layouts/AuthLayout";
import { useToast } from "@/hooks/use-toast";
import authService from "@/services/authService";

export default function VerifyPasswordReset() {
  const [code, setCode] = useState("");
  const location = useLocation();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [email, setEmail] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isResending, setIsResending] = useState(false);

  useEffect(() => {
    if (location.state?.email) {
      setEmail(location.state.email);
    } else {
      // If no email in state, redirect back to forgot password
      navigate("/forgot-password");
    }
  }, [location, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!code || code.length < 6) {
      toast({
        title: "Error",
        description: "Please enter a valid 6-digit code",
        variant: "destructive",
      });
      return;
    }

    setIsLoading(true);
    try {
      // Verify OTP via API
      const response = await authService.verifyResetOtp(email.trim(), code);

      toast({
        title: "Success",
        description: "Email verified successfully.",
      });

      // Navigate to reset password page with the received token
      navigate("/reset-password", { state: { token: response.token, email } });
    } catch (error: any) {
      const errorMessage = error.response?.data?.error || "Failed to verify code. Please try again.";
      toast({
        title: "Verification Failed",
        description: errorMessage,
        variant: "destructive",
      });
      console.error(error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleResend = async () => {
    if (!email) return;
    setIsResending(true);
    try {
      await authService.forgotPassword(email.trim()); // Re-use forgot password sending logic
      toast({
        title: "Code Resent",
        description: "A new verification code has been sent to your email.",
      });
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to resend code.",
        variant: "destructive"
      });
    } finally {
      setIsResending(false);
    }
  };

  return (
    <AuthLayout showBackButton backPath="/forgot-password" maxWidth="900px">
      <div className="text-center">
        <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-6 md:mb-8">
          Confirm your email
        </h2>

        <p className="text-xl md:text-2xl lg:text-3xl xl:text-4xl mb-8 md:mb-12">
          Please enter the verification code sent to email<br />
          <strong>{email}</strong>
        </p>

        <form onSubmit={handleSubmit} className="space-y-8 md:space-y-12">
          <div>
            <input
              type="text"
              placeholder="Enter verification code"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              required
              maxLength={6}
              className="w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors text-center tracking-widest"
            />
          </div>

          <div className="text-xl md:text-2xl lg:text-3xl">
            <span className="text-black">Didn&apos;t receive the code?</span>{" "}
            <button
              type="button"
              onClick={handleResend}
              disabled={isResending}
              className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
            >
              {isResending ? "Resending..." : "Resend"}
            </button>
          </div>

          <div className="flex justify-center">
            <Button
              type="submit"
              disabled={isLoading}
              className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-12 lg:px-16 py-3 md:py-4 text-xl md:text-2xl h-auto font-normal disabled:opacity-50"
            >
              {isLoading ? "Verifying..." : "Confirm"}
            </Button>
          </div>
        </form>
      </div>
    </AuthLayout>
  );
}
