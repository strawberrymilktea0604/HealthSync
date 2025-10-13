import React from "react";
import { Link } from "react-router-dom";

const Img = ({
  w = 600,
  h = 400,
  alt = "placeholder",
  className = "",
}: {
  w?: number;
  h?: number;
  alt?: string;
  className?: string;
}) => (
  <img
    src={`https://placehold.co/${w}x${h}`}
    alt={alt}
    className={className}
    loading="lazy"
  />
);

export default function HomeGuest() {
  return (
    <div className="min-h-svh w-full grid grid-rows-[auto,1fr,auto] bg-theme text-ink">
      {/* Header */}
      <header className="sticky top-0 z-40 bg-paper border-b border-theme">
        <div className="mx-auto w-full max-w-[900px] px-4 py-3 flex items-center gap-4">
          {/* Brand */}
          <div className="font-extrabold inline-flex items-baseline gap-1 text-[20px]">
            <span className="text-ink">health</span>
            <span className="text-accent">sync</span>
          </div>

          {/* Search */}
          <div className="flex items-center flex-1 bg-paper border border-theme rounded-full overflow-hidden max-w-[360px] ml-4">
            <input
              type="text"
              placeholder="What are you looking for?"
              className="w-full px-3 py-2 text-[14px] placeholder:text-muted outline-none"
            />
            <button className="px-3" aria-label="Search">
              🔍
            </button>
          </div>

          {/* Nav */}
          <nav className="hidden md:flex items-center gap-3 ml-6 text-[14px]">
            <select className="rounded-md border border-theme bg-paper px-2 py-1" title="Explore options">
              <option>Explore…</option>
            </select>
            <select className="rounded-md border border-theme bg-paper px-2 py-1" title="Find talent options">
              <option>Find talent</option>
            </select>
            <select className="rounded-md border border-theme bg-paper px-2 py-1" title="Get hired options">
              <option>Get hired</option>
            </select>
            <a
              href="#blog"
              className="px-3 py-1 rounded-md bg-ink text-paper no-underline"
            >
              Blog
            </a>
          </nav>

          {/* Auth */}
          <div className="flex gap-2 ml-auto">
            <Link
              to="/signup"
              className="px-4 py-2 rounded-lg border border-ink bg-paper text-center"
            >
              Sign up
            </Link>
            <Link
              to="/login"
              className="px-4 py-2 rounded-lg bg-ink text-paper text-center"
            >
              Login
            </Link>
          </div>
        </div>
      </header>

      {/* Main */}
      <main className="mx-auto w-full max-w-[900px] px-4 pt-6 pb-16">
        {/* Hero Section */}
        <section className="py-8">
          <h1 className="text-3xl font-bold">Dynamic Fitness Coaching Website</h1>
        </section>

        {/* Hero / Gallery card */}
        <section className="rounded-2xl bg-gray-50 border border-gray-300 shadow-md p-5 grid gap-5 md:grid-cols-[1.2fr,0.8fr]">
          <div>
            <span className="inline-block text-[12px] mb-3 px-3 py-1 rounded-full bg-green-100 text-green-600">
              Dynamic Fitness Coaching Website
            </span>

            {/* Author line */}
            <div className="flex items-center gap-3 mb-3">
              <div className="h-8 w-8 rounded-full bg-gray-300" />
              <div className="flex flex-col text-[12px]">
                <span className="font-bold">Coach Natalia</span>
                <span className="text-gray-500">Available for work</span>
              </div>
              <span className="ml-auto h-1 w-1 rounded-full bg-gray-500" />
              <div className="flex gap-2">
                <button className="px-2 py-1 rounded-md bg-gray-100">♡</button>
                <button className="px-2 py-1 rounded-md bg-gray-100">💾</button>
                <button className="px-2 py-1 rounded-md bg-gray-100">↗</button>
              </div>
            </div>

            {/* Big image */}
            <div className="rounded-xl border border-gray-300 p-2 bg-white">
              <div className="overflow-hidden rounded-lg">
                <Img w={770} h={470} alt="Hero Mock" className="w-full h-auto" />
              </div>
            </div>
          </div>

          {/* Right thumbs */}
          <aside className="grid gap-4">
            <div className="rounded-xl border border-gray-300 p-2 bg-white">
              <Img w={520} h={320} alt="Thumbnail 1" className="w-full h-auto rounded-lg" />
            </div>
            <div className="rounded-xl border border-gray-300 p-2 bg-white">
              <Img w={520} h={320} alt="Thumbnail 2" className="w-full h-auto rounded-lg" />
            </div>
          </aside>
        </section>

        {/* CTA line */}
        <section className="mt-5">
          <p className="text-center text-[14px] bg-yellow-50 border border-dashed border-yellow-300 rounded-xl px-4 py-3">
            Wanna create something great? <strong>Feel free to contact me</strong>
          </p>
        </section>

        {/* Overview */}
        <section className="mt-4 rounded-2xl bg-gray-50 border border-gray-300 p-6">
          <h2 className="text-[20px] font-bold mb-2">Overview</h2>
          <p className="text-[14px]">
            Excited to share this clean and modern wireframe for a fitness coaching website! This design focuses on user
            engagement and a clear information hierarchy, guiding potential clients through the coach’s offerings seamlessly.
            The layout emphasizes the coach’s expertise, showcases services, and encourages action with strategically placed CTAs.
          </p>

          <div className="mt-4 rounded-xl overflow-hidden bg-white border border-gray-300">
            <Img w={1040} h={360} alt="Where do we sweat banner" className="w-full h-auto" />
          </div>

          {/* Coach card */}
          <div className="mt-3 inline-flex items-center gap-3 px-3 py-3 rounded-xl border border-gray-300 bg-white">
            <Img w={72} h={72} alt="Coach avatar" className="h-12 w-12 rounded-full" />
            <div className="flex items-center gap-3">
              <div className="font-bold">Coach Natalia</div>
              <button className="px-3 py-2 rounded-lg border border-black bg-transparent">
                Get in touch
              </button>
            </div>
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="w-full bg-paper border-t border-theme">
        <div className="mx-auto w-full max-w-[900px] px-4 py-6 grid items-center gap-4 md:grid-cols-[1fr,auto,auto]">
          <div className="font-extrabold tracking-[.3px] inline-flex items-baseline gap-1.5">
            <span className="text-black">health</span>
            <span className="text-green-600">sync</span>
          </div>

          <ul className="flex flex-wrap gap-4 m-0 p-0 list-none text-[14px]">
            <li><a className="no-underline text-inherit" href="#inspiration">Inspiration</a></li>
            <li><a className="no-underline text-inherit" href="#support">Support</a></li>
            <li><a className="no-underline text-inherit" href="#about">About</a></li>
            <li><a className="no-underline text-inherit" href="#blog">Blog</a></li>
            <li><a className="no-underline text-inherit" href="#pit">Pit</a></li>
          </ul>

          <div className="flex gap-3 text-[18px]">
            <a aria-label="Twitter/X" href="#tw">𝕏</a>
            <a aria-label="Instagram" href="#ig">⌾</a>
            <a aria-label="Facebook" href="#fb">f</a>
            <a aria-label="Pinterest" href="#pi">𝒑</a>
          </div>
        </div>

        <div className="mx-auto w-full max-w-[900px] px-4 pb-6 flex flex-wrap gap-4 text-[12px] text-gray-500">
          <span>© healthsync 2025</span>
          <a className="text-inherit" href="#terms">Terms</a>
          <a className="text-inherit" href="#cookies">Cookies</a>
          <a className="text-inherit" href="#privacy">Privacy</a>
        </div>
      </footer>
    </div>
  );
}
