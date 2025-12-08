import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import AuthLayout from "@/layouts/AuthLayout";
import PasswordInput from "@/components/PasswordInput";

export default function ResetPassword() {
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    navigate("/reset-success");
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
            required
            minLength={8}
          />

          <PasswordInput
            placeholder="Confirm your password"
            required
            minLength={8}
          />

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
    </AuthLayout>
  );
}
