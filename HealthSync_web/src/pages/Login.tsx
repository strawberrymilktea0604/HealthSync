import React, { useState } from "react";
import { Link } from "react-router-dom";

export default function Login() {
  const [showPw, setShowPw] = useState(false);

  return (
    <div className="min-h-[100svh] w-full bg-[#f2edcf] text-[#1e201e] flex flex-col">
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
      <main className="flex-1">
        <div className="container">
          {/* Card content */}
          <div className="mx-auto max-w-[720px] rounded-2xl border border-[#d7d1bc] bg-[#fff8d9] shadow-[0_8px_16px_rgba(0,0,0,.08)] p-5 sm:p-7">
            <div className="mx-auto max-w-[560px] text-center">
              <div className="font-extrabold text-xs mb-1">healthsync</div>
              <h1 className="text-[32px] sm:text-[36px] font-extrabold mb-5 leading-tight">Welcome back!</h1>

              <form className="grid gap-3 text-left" onSubmit={(e) => e.preventDefault()}>
                {/* Email */}
                <label className="grid">
                  <span className="sr-only">Email address</span>
                  <input
                    type="email"
                    id="email"
                    placeholder="Your email address"
                    required
                    className="w-full rounded-lg border border-[#cfc9b6] bg-[#dcd7c7] text-[#1e201e] placeholder:text-[#5f615e] px-4 py-3 outline-none focus:border-[#1e201e] focus:ring-0"
                  />
                </label>

                {/* Password + toggle */}
                <div className="relative">
                  <input
                    type={showPw ? "text" : "password"}
                    placeholder="Enter a password (min. 8 characters)"
                    minLength={8}
                    required
                    className="w-full rounded-lg border border-[#cfc9b6] bg-[#dcd7c7] text-[#1e201e] placeholder:text-[#5f615e] pl-4 pr-12 py-3 outline-none focus:border-[#1e201e] focus:ring-0"
                  />
                  <button
                    type="button"
                    aria-label={showPw ? "Hide password" : "Show password"}
                    onClick={() => setShowPw(!showPw)}
                    className="absolute right-3 top-0 h-full flex items-center justify-center w-10 text-[#50524f] bg-transparent border-0 p-0"
                  >
                    {showPw ? (
                      <svg xmlns="http://www.w3.org/2000/svg" className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.875 18.825A10.05 10.05 0 0112 19c-5 0-9-3.5-10.5-7 1.05-2.25 3.15-4.5 6-5.625M9.88 9.88A3 3 0 1114.12 14.12M15 12a3 3 0 01-3 3" />
                      </svg>
                    ) : (
                      <svg xmlns="http://www.w3.org/2000/svg" className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M1.5 12C3.75 7.5 7.5 5 12 5s8.25 2.5 10.5 7c-2.25 4.5-6 7-10.5 7S3.75 16.5 1.5 12z" />
                        <circle cx="12" cy="12" r="3" />
                      </svg>
                    )}
                  </button>
                </div>

                {/* Forgot password */}
                <div className="text-right -mt-1 mb-2">
                  <Link
                    to="/forgot-password"
                    className="text-[#1e201e] no-underline"
                  >
                    Forgot password?
                  </Link>
                </div>

                {/* Login button */}
                <div className="flex justify-center">
                  <button
                    type="submit"
                    className="rounded-full border border-[#1e201e] px-7 py-3 font-semibold text-[#1e201e] bg-transparent"
                  >
                    Login
                  </button>
                </div>

                {/* OAuth buttons */}
                <div className="grid gap-3 mt-3">
                  <button
                    type="button"
                    className="flex items-center justify-center gap-3 rounded-full border border-[#cfc9b6] bg-white px-5 py-2.5"
                  >
                    <img
                      src="https://placehold.co/20x20"
                      alt="Google"
                      className="w-5 h-5"
                    />
                    <span className="font-medium">Sign in with Google</span>
                  </button>

                  <button
                    type="button"
                    className="flex items-center justify-center gap-3 rounded-full bg-[#1877F2] text-white px-5 py-2.5"
                  >
                    <img
                      src="https://placehold.co/20x20/1877F2/fff"
                      alt="Facebook"
                      className="w-5 h-5"
                    />
                    <span className="font-medium">Sign in with Facebook</span>
                  </button>
                </div>
              </form>

              {/* Register link */}
              <div className="mt-5 flex items-center justify-center gap-2 text-[16px]">
                <span>Don’t have an account?</span>
                <Link
                  to="/signup"
                  className="rounded-full border border-[#1e201e] px-5 py-2.5 text-inherit no-underline bg-transparent"
                >
                  Register
                </Link>
              </div>
            </div>
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
