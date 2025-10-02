import React, { useState } from "react";
import "../styles/global.css";

document.documentElement.classList.add("light");
document.documentElement.style.setProperty("--base-theme-color", "#d9d7b6");

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [sent, setSent] = useState(false);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!email) return;
    setSubmitting(true);
    try {
      // TODO: call your password-reset endpoint here
      // await api.auth.sendReset(email)
      await new Promise((r) => setTimeout(r, 600)); // demo delay
      setSent(true);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="min-h-screen w-full grid grid-rows-[auto,1fr,auto] bg-[#f2edcf] text-[#1e201e] px-4 sm:px-6 lg:px-8">
      {/* Header */}
      <header className="py-3">
        <div className="mx-auto w-full max-w-[1100px] px-4">
          <div className="font-extrabold inline-flex items-baseline gap-1 text-[18px] tracking-[.3px]">
            <span className="text-black">health</span>
            <span className="text-[#1b8f5a]">sync</span>
          </div>
        </div>
      </header>

      {/* Main */}
      <main className="w-full">
        <div className="mx-auto w-full max-w-[1100px] px-4">
          {/* Card */}
          <section className="max-w-[760px] rounded-2xl border border-[#d7d1bc] bg-[#fff8d9] shadow-[0_8px_16px_rgba(0,0,0,.08)] p-5 sm:p-8">
            <div className="max-w-[620px] mx-auto text-center">
              <div className="font-extrabold text-xs mb-1">healthsync</div>
              <h1 className="text-[30px] sm:text-[34px] font-extrabold mb-3 leading-tight">
                Account recovery
              </h1>
              <p className="text-[15px] mb-8">
                Enter email to send password recovery request
              </p>

              {sent ? (
                <div
                  role="status"
                  className="mx-auto max-w-[520px] rounded-xl border border-[#cfc9b6] bg-white p-4 text-left"
                >
                  <p className="font-semibold">
                    Check your inbox
                  </p>
                  <p className="text-[#5f615e] text-[14px]">
                    If an account exists for <span className="font-medium text-[#1e201e]">{email}</span>, you’ll receive a link to reset your password.
                  </p>
                </div>
              ) : (
                <form
                  onSubmit={onSubmit}
                  className="mx-auto max-w-[520px] grid gap-6"
                >
                  <label className="grid text-left">
                    <span className="sr-only">Email address</span>
                    <input
                      type="email"
                      required
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      placeholder="Your email address"
                      className="w-full rounded-lg border border-[#cfc9b6] bg-[#dcd7c7] text-[#1e201e] placeholder:text-[#5f615e] px-4 py-3 outline-none focus:border-[#1e201e] focus:ring-0"
                    />
                  </label>

                  <div className="flex justify-center">
                    <button
                      type="submit"
                      disabled={submitting}
                      className="rounded-full border border-[#1e201e] px-6 py-3 font-semibold text-[#1e201e] bg-transparent disabled:opacity-60"
                    >
                      {submitting ? "Sending…" : "Send request"}
                    </button>
                  </div>
                </form>
              )}
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
      <footer className="w-full border-t border-[#e4dfcf] bg-[#e9e4cd]">
        <div className="mx-auto w-full max-w-[1100px] px-4">
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
