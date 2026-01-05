import { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { Button } from "@/components/ui/button";
import AuthLayout from "@/layouts/AuthLayout";
import PasswordInput from "@/components/PasswordInput";
import { useToast } from "@/hooks/use-toast";
import authService from "@/services/authService";

export default function ResetPassword() {
  const navigate = useNavigate();
  const location = useLocation();
  const { toast } = useToast();

  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [token, setToken] = useState("");

  useEffect(() => {
    if (location.state?.token) {
      setToken(location.state.token);
    } else {
      // If no token, redirect back to forgot password or login
      toast({
        title: "Error",
        description: "Invalid session. Please start over.",
        variant: "destructive",
      });
      navigate("/forgot-password");
    }
  }, [location, navigate, toast]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      toast({
        title: "Error",
        description: "Passwords do not match.",
        variant: "destructive",
      });
      return;
    }

    if (password.length < 8) {
      toast({
        title: "Error",
        description: "Password must be at least 8 characters.",
        variant: "destructive",
      });
      return;
    }

    setIsLoading(true);
    try {
      await authService.resetPassword(token, password);

      toast({
        title: "Success",
        description: "Your password has been reset successfully.",
      });
      navigate("/reset-success");
    } catch (error: any) {
      const errorMessage = error.response?.data?.error || "Failed to reset password.";
      toast({
        title: "Error",
        description: errorMessage,
        variant: "destructive",
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout showBackButton backPath="/verify-password-reset" maxWidth="900px">
      <div className="text-center">
        <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-8 md:mb-12">
          Create new password
        </h2>

        <form onSubmit={handleSubmit} className="space-y-6 md:space-y-8">
          <PasswordInput
            placeholder="Choose a password (min. 8 characters)"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={8}
          />

          <PasswordInput
            placeholder="Confirm your password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            minLength={8}
          />

          <div className="flex justify-center pt-6 md:pt-8">
            <Button
              type="submit"
              disabled={isLoading}
              className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-full border border-black px-8 md:px-12 lg:px-16 py-3 md:py-4 text-xl md:text-2xl h-auto font-normal disabled:opacity-50"
            >
              {isLoading ? "Resetting..." : "Reset password"}
            </Button>
          </div>
        </form>
      </div>
    </AuthLayout>
  );
}
