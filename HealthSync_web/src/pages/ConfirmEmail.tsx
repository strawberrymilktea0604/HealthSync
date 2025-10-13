import React, { useEffect, useMemo, useRef, useState } from "react";

/**
 * ConfirmEmail
 * - 6-digit OTP inputs with auto-advance, backspace, and paste support
 * - Resend countdown (disabled while timer > 0)
 * - Matches the light theme used in your other pages
 */
export default function ConfirmEmail() {
  // You can pass the real email via props/router and mask it here.
  const email = "im******************@gmail.com";

  const length = 6;
  const [code, setCode] = useState<string[]>(Array.from({ length }, () => ""));
  const inputsRef = useRef<Array<HTMLInputElement | null>>([]);
  const [submitting, setSubmitting] = useState(false);

  // Resend timer
  const RESEND_SECONDS = 60;
  const [secondsLeft, setSecondsLeft] = useState(58); // start at 58 like your mock

  useEffect(() => {
    if (secondsLeft <= 0) return;
    const t = setInterval(() => setSecondsLeft((s) => s - 1), 1000);
    return () => clearInterval(t);
  }, [secondsLeft]);

  const value = useMemo(() => code.join(""), [code]);

  function focusIndex(i: number) {
    inputsRef.current[i]?.focus();
    inputsRef.current[i]?.select();
  }

  function handleChange(i: number, v: string) {
    const digit = v.replace(/\D/g, "").slice(-1); // keep only last digit if user types multiple
    const next = [...code];
    next[i] = digit ?? "";
    setCode(next);

    if (digit && i < length - 1) focusIndex(i + 1);
  }

  function handleKeyDown(i: number, e: React.KeyboardEvent<HTMLInputElement>) {
    const key = e.key;
    if (key === "Backspace") {
      if (code[i]) {
        const next = [...code];
        next[i] = "";
        setCode(next);
      } else if (i > 0) {
        focusIndex(i - 1);
        const next = [...code];
        next[i - 1] = "";
        setCode(next);
      }
      e.preventDefault();
    } else if (key === "ArrowLeft" && i > 0) {
      focusIndex(i - 1);
      e.preventDefault();
    } else if (key === "ArrowRight" && i < length - 1) {
      focusIndex(i + 1);
      e.preventDefault();
    }
  }

  function handlePaste(i: number, e: React.ClipboardEvent<HTMLInputElement>) {
    e.preventDefault();
    const txt = e.clipboardData.getData("text").replace(/\D/g, "").slice(0, length);
    if (!txt) return;
    const next = [...code];
    for (let k = 0; k < length && i + k < length; k++) {
      next[i + k] = txt[k] ?? "";
    }
    setCode(next);
    const lastFilled = Math.min(i + txt.length, length - 1);
    focusIndex(lastFilled);
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (value.length !== length) return;
    setSubmitting(true);
    try {
      // TODO: verify OTP with your backend
      await new Promise((r) => setTimeout(r, 700));
      // success flow here
      alert(`Submitted code: ${value}`);
    } finally {
      setSubmitting(false);
    }
  }

  function resend() {
    if (secondsLeft > 0) return;
    // TODO: call resend endpoint
    setSecondsLeft(RESEND_SECONDS);
  }

  return (
    <div className="min-h-[100svh] w-full grid grid-rows-[auto,1fr,auto] bg-[#f2edcf] text-[#1e201e]">
      {/* Header */}
      <header className="py-3">
        <div className="mx-auto w-full max-w-[900px] px-4">
          <div className="font-extrabold inline-flex items-baseline gap-1 text-[18px] tracking-[.3px]">
            <span className="text-black">health</span>
            <span className="text-[#1b8f5a]">sync</span>
          </div>
        </div>
      </header>

      {/* Main */}
      <main className="w-full">
        <div className="mx-auto w-full max-w-[900px] px-4">
          <section className="mx-auto max-w-[720px] rounded-2xl border border-[#d7d1bc] bg-[#fff8d9] shadow-[0_8px_16px_rgba(0,0,0,.08)] p-6 sm:p-8">
            {/* Back */}
            <button
              type="button"
              onClick={() => window.history.back()}
              className="inline-flex items-center gap-2 text-[15px] text-[#1e201e] bg-transparent"
            >
              <svg width="22" height="22" viewBox="0 0 24 24" className="text-[#1e201e]">
                <path d="M15 6l-6 6 6 6" stroke="currentColor" strokeWidth="2" fill="none" strokeLinecap="round" strokeLinejoin="round" />
              </svg>
              <span className="sr-only">Back</span>
            </button>

            <div className="text-center mt-1">
              <h1 className="text-[28px] sm:text-[32px] font-extrabold">Confirm your email</h1>
              <p className="mt-2 text-[15px]">Please enter the verification code sent to email</p>
              <p className="mt-1 text-[15px] font-medium">{email}</p>

              {/* OTP inputs */}
              <form onSubmit={onSubmit} className="mt-6">
                <div className="flex justify-center gap-3 sm:gap-4">
                  {Array.from({ length }).map((_, i) => (
                    <input
                      key={`otp-${i}`}
                      ref={(el) => { inputsRef.current[i] = el }}
                      inputMode="numeric"
                      autoComplete="one-time-code"
                      maxLength={1}
                      placeholder="•"
                      aria-label={`Digit ${i + 1}`}
                      title={`Digit ${i + 1}`}
                      value={code[i]}
                      onChange={(e) => handleChange(i, e.target.value)}
                      onKeyDown={(e) => handleKeyDown(i, e)}
                      onPaste={(e) => handlePaste(i, e)}
                      className="h-14 w-10 sm:h-16 sm:w-12 text-center text-[20px] font-semibold rounded-lg border border-[#cfc9b6] bg-white outline-none focus:border-[#1e201e] focus:ring-0"
                    />
                  ))}
                </div>

                {/* Resend */}
                <div className="mt-6 text-[14px]">
                  <span className="text-[#4b4e4b]">Didn’t receive the code? </span>
                  <button
                    type="button"
                    onClick={resend}
                    disabled={secondsLeft > 0}
                    className="disabled:opacity-50 underline underline-offset-2"
                  >
                    Resend{secondsLeft > 0 ? ` (${secondsLeft})` : ""}
                  </button>
                </div>

                {/* Submit */}
                <div className="mt-6 flex justify-center">
                  <button
                    type="submit"
                    disabled={value.length !== length || submitting}
                    className="rounded-full border border-[#1e201e] px-6 py-2.5 font-semibold text-[#1e201e] bg-transparent disabled:opacity-60"
                  >
                    {submitting ? "Confirming…" : "Confirm"}
                  </button>
                </div>
              </form>
            </div>
          </section>

          {/* Decorative split lines */}
          <div className="grid grid-cols-2 gap-6 mt-4" aria-hidden="true">
            <div className="h-px bg-[#bdb79f]" />
            <div className="h-px bg-[#bdb79f]" />
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="w-full border-t border-[#e4dfcf] bg-[#e9e4cd] mt-6">
        <div className="mx-auto w-full max-w-[900px] px-4">
          <div className="py-4 grid grid-cols-1 sm:grid-cols-[auto,1fr] gap-3 sm:gap-5 items-center">
            <div className="font-extrabold inline-flex items-baseline gap-1 text-[18px] tracking-[.3px]">
              <span className="text-black">health</span>
              <span className="text-[#1b8f5a]">sync</span>
            </div>
            <ul className="flex flex-wrap items-center gap-4 sm:gap-5 text-[16px]">
              <li><a href="#inspiration" className="no-underline text-inherit">Inspiration</a></li>
              <li><a href="#support" className="no-underline text-inherit">Support</a></li>
              <li><a href="#about" className="no-underline text-inherit">About</a></li>
              <li><a href="#blog" className="no-underline text-inherit">Blog</a></li>
              <li><a href="#pfs" className="no-underline text-inherit">Pfs</a></li>
              <li className="ml-auto inline-flex gap-3 text-lg">
                <a href="#x" aria-label="X">𝕏</a>
                <a href="#ig" aria-label="Instagram">⌾</a>
                <a href="#fb" aria-label="Facebook">f</a>
                <a href="#pi" aria-label="Pinterest">𝒑</a>
              </li>
            </ul>
          </div>
          <div className="flex flex-wrap gap-4 text-[#5b5f5a] text-[14px] pb-4">
            <span>© healthsync 2025</span>
            <a href="#terms">Terms&Conditions</a>
            <a href="#cookies">Cookies</a>
            <a href="#privacy">Privacy</a>
            <a href="#problems">Problems</a>
            <a href="#resource">Resource</a>
            <a href="#tags">Tags</a>
          </div>
        </div>
      </footer>
    </div>
  );
}
