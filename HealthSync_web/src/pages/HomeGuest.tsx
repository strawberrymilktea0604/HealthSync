import React from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "primereact/button";
import { Avatar } from "primereact/avatar";
import { motion } from "framer-motion";
import logo from "../assets/logo.png";
import { useAuth } from "@/contexts/AuthContext";
import { useEffect } from "react";

export default function HomeGuest() {
  const navigate = useNavigate();
  const { user } = useAuth();

  useEffect(() => {
    if (user) {
      navigate('/dashboard');
    }
  }, [user, navigate]);

  return (
    <div className="min-h-screen" style={{ backgroundColor: '#E3E3C6' }}>
      {/* Header */}
      <header className="px-4 py-4">
        <div className="flex align-items-center justify-content-between max-w-7xl mx-auto">
          {/* Brand */}
          <div className="flex align-items-center gap-2">
            <h1 className="text-3xl font-bold m-0 text-900 flex align-items-center gap-2">
              Welcome to
              <motion.img
                src={logo}
                alt="healthsync"
                style={{ height: '24px', marginTop: '4px' }}
                animate={{
                  scale: [1, 1.1, 1],
                  rotate: [0, 5, -5, 0]
                }}
                transition={{
                  duration: 2,
                  repeat: Infinity,
                  ease: "easeInOut"
                }}
              />
            </h1>
          </div>

          {/* Auth Buttons */}
          <div className="flex align-items-center gap-3 p-3" style={{ backgroundColor: '#FFF9C4', borderRadius: '30px' }}>
            <Button
              label="Sign up"
              outlined
              className="text-900 border-900 px-4 py-2 border-1 hover:bg-transparent"
              style={{ color: '#000', borderColor: '#000', backgroundColor: 'transparent', borderRadius: '12px' }}
              onClick={() => navigate('/register')}
            />
            <Button
              label="Login"
              className="bg-900 border-900 text-white px-4 py-2"
              style={{ backgroundColor: '#000', borderColor: '#000', borderRadius: '12px' }}
              onClick={() => navigate('/login')}
            />
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 pb-8">

        <div className="mb-4">
          <p className="text-xl m-0 mb-3">Dynamic Fitness Coaching Website</p>

          {/* Coach Header Line */}
          <div className="flex flex-wrap align-items-center justify-content-between gap-3 mb-4">
            <div className="flex align-items-center gap-3">
              <Avatar
                image="https://api.builder.io/api/v1/image/assets/TEMP/203282ca523b38453ad8c2b8dd91921f06024637?width=80"
                size="large"
                shape="circle"
              />
              <div className="flex align-items-center gap-3">
                <span className="font-bold text-900">Coach Natalia</span>
                <span className="text-700">Available for work</span>
                <Button
                  label="Follow"
                  className="p-button-rounded bg-900 text-white text-sm py-1 px-3 border-none"
                  style={{ backgroundColor: '#333' }}
                />
              </div>
            </div>

            <div className="flex align-items-center gap-2">
              <Button icon="pi pi-heart" rounded text className="text-900 border-none" style={{ width: '50px', height: '50px', backgroundColor: '#FFF9C4' }} />
              <Button icon="pi pi-save" rounded text className="text-900 border-none" style={{ width: '50px', height: '50px', backgroundColor: '#FFF9C4' }} />
              <Button label="Get in touch" outlined className="text-900 border-900 px-4" style={{ backgroundColor: '#FFF9C4', borderRadius: '16px', height: '50px' }} />
            </div>
          </div>
        </div>

        {/* Hero Image Section */}
        <div className="flex gap-3 mb-6">
          <div className="flex-1">
            <img
              src="https://api.builder.io/api/v1/image/assets/TEMP/e562a8c59d085e50c143f29d2bcf90c09bdd82b3?width=1682"
              alt="Fitness coaching hero"
              className="w-full border-round-2xl shadow-2"
            />
          </div>
          <div className="flex flex-column gap-3 pt-4">
            <Button icon="pi pi-comment" rounded text className="text-900" style={{ width: '50px', height: '50px', backgroundColor: '#FFF9C4' }} />
            <Button icon="pi pi-upload" rounded text className="text-900" style={{ width: '50px', height: '50px', backgroundColor: '#FFF9C4' }} />
            <Button icon="pi pi-ellipsis-v" rounded text className="text-900" style={{ width: '50px', height: '50px', backgroundColor: '#FFF9C4' }} />
          </div>
        </div>

        {/* Secondary Image Section */}
        <div className="mb-6">
          <img
            src="https://api.builder.io/api/v1/image/assets/TEMP/49f9d7dfe50aa221da51fe9ae3eed0392c09d95e?width=1682"
            alt="Fitness coaching hero"
            className="w-full border-round-2xl shadow-2"
          />
        </div>

        {/* Contact Text */}
        <div className="text-center mb-6">
          <h2 className="text-2xl font-normal m-0">
            Wanna create something great? <span className="font-bold">Feel free to contact me</span>
          </h2>
        </div>

        {/* Overview Section */}
        <div className="mb-6 max-w-4xl">
          <h3 className="text-3xl font-normal mb-3">Overview</h3>
          <p className="text-xl line-height-3 text-800">
            Excited to share this clean and modern wireframe for a fitness coaching website!
            This design focuses on user engagement and a clear information hierarchy, guiding
            potential clients through the coach&apos;s offerings seamlessly. The layout emphasizes
            the coach&apos;s expertise, showcases services, and encourages action with strategically
            placed CTAs.
          </p>
        </div>

        {/* Where Do We Sweat Image */}
        <div className="mb-8">
          <img
            src="https://api.builder.io/api/v1/image/assets/TEMP/6c4ce5b8cdddf2258704790f473ac199bd21cad7?width=1690"
            alt="Where do we sweat"
            className="w-full border-round-2xl shadow-2"
          />
        </div>

        {/* Bottom Coach Profile with Lines */}
        <div className="flex align-items-center w-full mb-8">
          <div className="flex-1 border-top-1 border-900" style={{ opacity: 0.5 }}></div>
          <div className="mx-5 flex flex-column align-items-center gap-3">
            <Avatar
              image="https://api.builder.io/api/v1/image/assets/TEMP/4e07f9ce70a0a210464bed673b7acb705fc502e6?width=286"
              size="xlarge"
              shape="square"
              className="border-round-xl"
              style={{ width: '100px', height: '100px' }}
            />
            <div className="text-center">
              <h4 className="text-xl font-normal m-0 mb-3">Coach Natalia</h4>
              <Button
                label="Get in touch"
                className="p-button-rounded bg-900 text-white border-none px-4 font-bold"
                style={{ backgroundColor: '#333' }}
              />
            </div>
          </div>
          <div className="flex-1 border-top-1 border-900" style={{ opacity: 0.5 }}></div>
        </div>

        {/* Footer */}
        <footer className="w-full pb-6">
          <div className="flex flex-wrap justify-content-between">
            {/* Logo & Copyright */}
            <div className="flex flex-column justify-content-between gap-6 mb-4 md:mb-0" style={{ minHeight: '120px' }}>
              <h2 className="text-2xl font-bold m-0">healthsync</h2>
              <span className="text-600 text-sm">© healthsync 2025</span>
            </div>

            {/* Links Columns */}
            <div className="flex gap-8 flex-wrap">
              <div className="flex flex-column justify-content-between gap-6" style={{ minHeight: '120px' }}>
                <span className="font-bold">Inspiration</span>
                <button className="text-600 no-underline text-sm bg-transparent border-none p-0 cursor-pointer">Term&Conditions</button>
              </div>
              <div className="flex flex-column justify-content-between gap-6" style={{ minHeight: '120px' }}>
                <span className="font-bold">Support</span>
                <button className="text-600 no-underline text-sm bg-transparent border-none p-0 cursor-pointer">Cookies</button>
              </div>
              <div className="flex flex-column justify-content-between gap-6" style={{ minHeight: '120px' }}>
                <span className="font-bold">About</span>
                {/* Empty spacer */}
              </div>
              <div className="flex flex-column justify-content-between gap-6" style={{ minHeight: '120px' }}>
                <span className="font-bold">Blog</span>
                <button className="text-600 no-underline text-sm bg-transparent border-none p-0 cursor-pointer">Freelancers</button>
              </div>
              <div className="flex flex-column justify-content-between gap-6" style={{ minHeight: '120px' }}>
                <span className="font-bold">PTs</span>
                <button className="text-600 no-underline text-sm bg-transparent border-none p-0 cursor-pointer">Resources</button>
              </div>
            </div>

            {/* Social & Tags */}
            <div className="flex flex-column justify-content-between align-items-end gap-6 mt-4 md:mt-0" style={{ minHeight: '120px' }}>
              <div className="border-round-2xl px-3 py-2 flex gap-3" style={{ backgroundColor: '#FFF9C4' }}>
                <i className="pi pi-twitter text-2xl text-900"></i>
                <i className="pi pi-facebook text-2xl text-900"></i>
                <i className="pi pi-instagram text-2xl text-900"></i>
              </div>
              <button className="text-600 no-underline text-sm bg-transparent border-none p-0 cursor-pointer">Tags</button>
            </div>
          </div>
        </footer>
      </main>
    </div>
  );
}
