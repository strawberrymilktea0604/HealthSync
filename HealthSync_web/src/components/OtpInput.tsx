interface OtpInputProps {
  readonly value: string[];
  readonly onChange: (index: number, value: string) => void;
}

export default function OtpInput({ value, onChange }: OtpInputProps) {
  return (
    <div className="flex justify-center gap-2 md:gap-4">
      {value.map((digit, index) => (
        <input
          key={`otp-digit-${index}`} // eslint-disable-line react/no-array-index-key
          id={`code-${index}`}
          type="text"
          maxLength={1}
          value={digit}
          onChange={(e) => onChange(index, e.target.value)}
          className="w-12 h-16 md:w-16 md:h-20 lg:w-20 lg:h-24 text-center text-2xl md:text-3xl lg:text-4xl bg-transparent rounded-2xl border-4 md:border-6 border-[#9E9D85] outline-none focus:border-black transition-colors"
        />
      ))}
    </div>
  );
}
