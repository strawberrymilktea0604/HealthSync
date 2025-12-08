import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import AuthLayout from "@/layouts/AuthLayout";
import OtpInput from "@/components/OtpInput";

export default function VerifyPasswordReset() {
  const [code, setCode] = useState(["", "", "", "", "", ""]);
  const navigate = useNavigate();

  const handleCodeChange = (index: number, value: string) => {
    if (value.length <= 1 && /^\d*$/.test(value)) {
      const newCode = [...code];
      newCode[index] = value;
      setCode(newCode);

      if (value && index < 5) {
        const nextInput = document.getElementById(`reset-code-${index + 1}`);
        nextInput?.focus();
      }
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    navigate("/reset-password");
  };

  return (
    <AuthLayout showBackButton backPath="/forgot-password" maxWidth="900px">
      <div className="text-center">
        <h2 className="text-3xl md:text-5xl lg:text-6xl xl:text-7xl font-bold mb-6 md:mb-8">
          Confirm your email
        </h2>

        <p className="text-xl md:text-2xl lg:text-3xl xl:text-4xl mb-8 md:mb-12">
          Please enter the verification code sent to email<br />
          lm*******************@gmail.com
        </p>

        <form onSubmit={handleSubmit} className="space-y-8 md:space-y-12">
          <OtpInput value={code} onChange={handleCodeChange} />

          <div className="text-xl md:text-2xl lg:text-3xl">
            <span className="text-black">Didn&apos;t receive the code?</span>{" "}
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
    </AuthLayout>
  );
}
