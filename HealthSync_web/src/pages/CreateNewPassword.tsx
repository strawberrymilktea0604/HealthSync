import React, { useState } from "react";

export default function CreateNewPassword() {
  const [pw1, setPw1] = useState("");
  const [pw2, setPw2] = useState("");
  const [show1, setShow1] = useState(false);
  const [show2, setShow2] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const minLen = 8;
  const tooShort = pw1.length > 0 && pw1.length < minLen;
  const mismatch = pw2.length > 0 && pw1 !== pw2;
  const canSubmit = pw1.length >= minLen && pw1 === pw2 && !submitting;

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!canSubmit) return;
    setSubmitting(true);
    try {
      // TODO: call your reset password API with a token from the URL
      await new Promise((r) => setTimeout(r, 700));
      alert("Password reset!");
    } finally {
      setSubmitting(false);
    }
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
            {/* Small logo (placeholder) */}
            <img
              src="https://placehold.co/28x28"
              alt="logo placeholder"
              className="w-7 h-7"
            />

            <div className="text-center mt-2">
              <h1 className="text-[28px] sm:text-[32px] font-extrabold">
                Create new password
              </h1>
            </div>

            <form onSubmit={onSubmit} className="mt-6 mx-auto max-w-[520px] grid gap-4">
              {/* New password */}
              <div className="relative">
                <input
                  type={show1 ? "text" : "password"}
                  minLength={minLen}
                  required
                  value={pw1}
                  onChange={(e) => setPw1(e.target.value)}
                  placeholder={`Choose a password (min. ${minLen} characters)`}
                  className="w-full rounded-lg border border-[#cfc9b6] bg-[#dcd7c7] text-[#1e201e] placeholder:text-[#5f615e] pl-4 pr-11 py-3 outline-none focus:border-[#1e201e] focus:ring-0"
                />
                <button
                  type="button"
                  onClick={() => setShow1((v) => !v)}
                  aria-label={show1 ? "Hide password" : "Show password"}
                  className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center justify-center w-6 h-6 text-[#50524f] bg-transparent p-0"
                >
                  {show1 ? (
                    /* Eye slash */
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M3 3l18 18" strokeLinecap="round" strokeLinejoin="round" />
                      <path d="M10.58 6.09A9.9 9.9 0 0112 6c5.2 0 9.27 3.56 10.5 6-.54 1.07-1.27 2.13-2.18 3.07M6.62 6.62C4.96 7.93 3.72 9.81 2.5 12c1.23 2.44 5.3 6 10.5 6 1.26 0 2.45-.2 3.54-.58" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                  ) : (
                    /* Eye */
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M1.5 12C3.75 7.5 7.5 5 12 5s8.25 2.5 10.5 7c-2.25 4.5-6 7-10.5 7S3.75 16.5 1.5 12z" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                  )}
                </button>
              </div>
              {tooShort && (
                <p className="text-[13px] text-[#7a2e2e]">
                  Password must be at least {minLen} characters.
                </p>
              )}

              {/* Confirm password */}
              <div className="relative mt-1">
                <input
                  type={show2 ? "text" : "password"}
                  minLength={minLen}
                  required
                  value={pw2}
                  onChange={(e) => setPw2(e.target.value)}
                  placeholder="Confirm your password"
                  className="w-full rounded-lg border border-[#cfc9b6] bg-[#dcd7c7] text-[#1e201e] placeholder:text-[#5f615e] pl-4 pr-11 py-3 outline-none focus:border-[#1e201e] focus:ring-0"
                />
                <button
                  type="button"
                  onClick={() => setShow2((v) => !v)}
                  aria-label={show2 ? "Hide password" : "Show password"}
                  className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center justify-center w-6 h-6 text-[#50524f] bg-transparent p-0"
                >
                  {show2 ? (
                    /* Eye slash */
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M3 3l18 18" strokeLinecap="round" strokeLinejoin="round" />
                      <path d="M10.58 6.09A9.9 9.9 0 0112 6c5.2 0 9.27 3.56 10.5 6-.54 1.07-1.27 2.13-2.18 3.07M6.62 6.62C4.96 7.93 3.72 9.81 2.5 12c1.23 2.44 5.3 6 10.5 6 1.26 0 2.45-.2 3.54-.58" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                  ) : (
                    /* Eye */
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M1.5 12C3.75 7.5 7.5 5 12 5s8.25 2.5 10.5 7c-2.25 4.5-6 7-10.5 7S3.75 16.5 1.5 12z" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                  )}
                </button>
              </div>
              {mismatch && (
                <p className="text-[13px] text-[#7a2e2e]">Passwords do not match.</p>
              )}

              <div className="mt-4 flex justify-center">
                <button
                  type="submit"
                  disabled={!canSubmit}
                  className="rounded-full border border-[#1e201e] px-6 py-3 font-semibold text-[#1e201e] bg-transparent disabled:opacity-60"
                >
                  {submitting ? "Resetting…" : "Reset password"}
                </button>
              </div>
            </form>
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
