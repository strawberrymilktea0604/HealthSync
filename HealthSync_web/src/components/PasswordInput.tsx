import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";

type PasswordInputProps = React.InputHTMLAttributes<HTMLInputElement>;

export default function PasswordInput(props: PasswordInputProps) {
  const [showPassword, setShowPassword] = useState(false);

  return (
    <div className="relative">
      <input
        {...props}
        type={showPassword ? "text" : "password"}
        className={`w-full px-4 md:px-6 py-4 md:py-6 text-lg md:text-2xl lg:text-3xl bg-[#D9D7B6] rounded-lg md:rounded-xl border-2 md:border-[3px] border-white/30 outline-none focus:border-white/50 transition-colors pr-16 md:pr-20 ${props.className || ''}`}
      />
      <button
        type="button"
        onClick={() => setShowPassword(!showPassword)}
        className="absolute right-4 md:right-6 top-1/2 -translate-y-1/2 text-black hover:opacity-70 transition-opacity"
      >
        {showPassword ? (
          <Eye className="w-8 h-8 md:w-12 md:h-12 lg:w-16 lg:h-16" />
        ) : (
          <EyeOff className="w-8 h-8 md:w-12 md:h-12 lg:w-16 lg:h-16" />
        )}
      </button>
    </div>
  );
}
