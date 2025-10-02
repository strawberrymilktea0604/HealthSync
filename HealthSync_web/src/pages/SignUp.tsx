import React, { useState } from "react";
import { EyeIcon, EyeSlashIcon } from "@heroicons/react/24/outline";
import { Link } from "react-router-dom";

export default function SignUp() {
  const [showPw, setShowPw] = useState(false);
  const [showPw2, setShowPw2] = useState(false);

  return (
    <div className="min-h-svh w-full grid grid-rows-[auto,1fr,auto] text-[#1e201e] bg-[#f2edcf]">
      {/* Header */}
      <header className="py-3">
        <div className="mx-auto w-full max-w-[1100px] px-4">
          <div className="font-extrabold inline-flex items-baseline gap-1 tracking-[.3px] text-[18px]">
            <span className="text-black">health</span>
            <span className="text-[#1b8f5a]">sync</span>
          </div>
        </div>
      </header>

      {/* Main */}
      <main className="w-full">
        <div className="mx-auto w-full max-w-[1100px] px-4">
          <section className="max-w-[660px] rounded-2xl border border-[#d7d1bc] bg-[#fff8d9] shadow-[0_8px_16px_rgba(0,0,0,.08)] p-5 sm:p-7">
            <div className="max-w-[560px] mx-auto text-center">
              <div className="font-extrabold text-xs mb-1">healthsync</div>
              <h1 className="text-[40px] font-extrabold mb-4 leading-tight text-[#1e201e]">
                Create an account
              </h1>

              <form className="grid gap-3" onSubmit={(e) => e.preventDefault()}>
                <label className="grid">
                  <span className="sr-only">Email address</span>
                  <input
                    id="email"
                    type="email"
                    placeholder="Your email address"
                    required
                    className="w-full rounded-lg border border-[#cfc9b6] bg-[#dcd7c7] text-[#1e201e] placeholder:text-[#5f615e] px-4 py-3 outline-none focus:border-[#1e201e] focus:ring-0"
                  />
                </label>

                <label className="grid relative">
                  <div className="relative w-full">
                    <input
                      type={showPw ? "text" : "password"}
                      placeholder="Choose a password (min. 8 characters)"
                      minLength={8}
                      required
                      aria-label="Password"
                      className="w-full rounded-lg border border-[#cfc9b6] bg-[#dcd7c7] text-[#1e201e] placeholder:text-[#5f615e] pl-4 pr-11 py-3 outline-none focus:border-[#1e201e] focus:ring-0"
                    />
                    <button
                      type="button"
                      aria-label={showPw ? "Hide password" : "Show password"}
                      onClick={() => setShowPw(!showPw)}
                      className="absolute right-3 top-0 h-full flex items-center justify-center w-10 text-[#50524f] bg-transparent border-0 p-0"
                    >
                      {showPw ? (
                        <EyeSlashIcon className="w-5 h-5" />
                      ) : (
                        <EyeIcon className="w-5 h-5" />
                      )}
                    </button>
                  </div>
                </label>

                <label className="grid relative">
                  <div className="relative w-full">
                    <input
                      type={showPw2 ? "text" : "password"}
                      placeholder="Confirm your password"
                      minLength={8}
                      required
                      aria-label="Confirm Password"
                      className="w-full rounded-lg border border-[#cfc9b6] bg-[#dcd7c7] text-[#1e201e] placeholder:text-[#5f615e] pl-4 pr-11 py-3 outline-none focus:border-[#1e201e] focus:ring-0"
                    />
                    <button
                      type="button"
                      aria-label={showPw2 ? "Hide password" : "Show password"}
                      onClick={() => setShowPw2(!showPw2)}
                      className="absolute right-3 top-0 h-full flex items-center justify-center w-10 text-[#50524f] bg-transparent border-0 p-0"
                    >
                      {showPw2 ? (
                        <EyeSlashIcon className="w-5 h-5" />
                      ) : (
                        <EyeIcon className="w-5 h-5" />
                      )}
                    </button>
                  </div>
                </label>

                <div className="mt-2 flex justify-center">
                  <button
                    type="submit"
                    className="rounded-full border border-[#1e201e] px-6 py-3 font-semibold text-[#1e201e] bg-transparent"
                  >
                    Sign up
                  </button>
                </div>
              </form>

              <div className="mt-5 flex items-center justify-center gap-2 text-[16px] text-[#1e201e]">
                <span>Do you have an account?</span>
                <Link
                  to="/login"
                  className="rounded-full border border-[#1e201e] px-5 py-2.5 text-inherit no-underline bg-transparent"
                >
                  Sign in
                </Link>
              </div>
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
      <footer className="w-full mt-7 border-t border-[#e4dfcf] bg-[#e9e4cd]">
        <div className="mx-auto w-full max-w-[1100px] px-4">
          <div className="grid grid-cols-1 items-center gap-3 py-4 sm:grid-cols-[auto,1fr]">
            <div className="inline-flex items-baseline gap-1 text-[18px] font-extrabold tracking-[.3px]">
              <span className="text-black">health</span>
              <span className="text-[#1b8f5a]">sync</span>
            </div>
            <ul className="flex flex-wrap items-center gap-4 text-[16px] text-[#1e201e]">
              <li><a href="#inspiration" className="no-underline text-inherit">Inspiration</a></li>
              <li><a href="#support" className="no-underline text-inherit">Support</a></li>
              <li><a href="#about" className="no-underline text-inherit">About</a></li>
              <li><a href="#blog" className="no-underline text-inherit">Blog</a></li>
              <li><a href="#pfs" className="no-underline text-inherit">Pfs</a></li>
              <li className="ml-auto inline-flex gap-3 text-lg">
                <a href="#x" aria-label="X" className="text-[#1e201e]">𝕏</a>
                <a href="#ig" aria-label="Instagram" className="text-[#1e201e]">⌾</a>
                <a href="#fb" aria-label="Facebook" className="text-[#1e201e]">f</a>
                <a href="#pi" aria-label="Pinterest" className="text-[#1e201e]">𝒑</a>
              </li>
            </ul>
          </div>

          <div className="flex flex-wrap gap-4 pb-4 text-[14px] text-[#5b5f5a]">
            <span>© healthsync 2025</span>
            <a href="#terms" className="text-inherit">Terms&Conditions</a>
            <a href="#cookies" className="text-inherit">Cookies</a>
            <a href="#privacy" className="text-inherit">Privacy</a>
            <a href="#problems" className="text-inherit">Problems</a>
            <a href="#resource" className="text-inherit">Resource</a>
            <a href="#tags" className="text-inherit">Tags</a>
          </div>
        </div>
      </footer>
    </div>
  );
}
