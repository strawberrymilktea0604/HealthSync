import React from "react";
import { Button } from "primereact/button";
import { Avatar } from "primereact/avatar";

export default function HomeGuest() {
  return (
    <div className="min-h-screen surface-ground">
      {/* Header */}
      <header className="surface-card border-bottom-1 surface-border sticky top-0 z-5 px-4 py-3">
        <div className="flex align-items-center justify-content-between max-w-7xl mx-auto">
          {/* Brand */}
          <div className="flex align-items-center gap-2">
            <h1 className="text-3xl font-bold m-0">
              <span className="text-900">health</span>
              <span className="text-primary">sync</span>
            </h1>
          </div>

          {/* Auth Buttons */}
          <div className="flex align-items-center gap-2 ml-auto">
            <Button 
              label="Sign up" 
              outlined 
              className="px-4 py-2"
              onClick={() => window.location.href = '/signup'}
            />
            <Button 
              label="Login" 
              className="px-4 py-2"
              onClick={() => window.location.href = '/login'}
            />
          </div>
        </div>
      </header>

      {/* Hero Section */}
      <section className="surface-section py-8">
        <div className="max-w-7xl mx-auto px-4">
          <div className="text-center mb-5">
            <p className="text-xl mb-2">Welcome to</p>
            <h1 className="text-6xl font-bold m-0">
              <span className="text-900">health</span>
              <span className="text-primary">sync</span>
            </h1>
          </div>

          <h2 className="text-4xl font-bold text-center mb-5">
            Dynamic Fitness Coaching Website
          </h2>

          {/* Featured Card */}
          <div className="surface-card border-round-3xl shadow-3 p-5">
            <div className="grid">
              <div className="col-12 lg:col-8">
                {/* Profile Header */}
                <div className="flex align-items-center gap-3 mb-4">
                  <Avatar 
                    image="https://placehold.co/80x80" 
                    size="large" 
                    shape="circle" 
                  />
                  <div className="flex-1">
                    <h3 className="text-2xl font-bold m-0 mb-2">Coach Natalia</h3>
                    <p className="text-600 m-0">Available for work</p>
                  </div>
                  <div className="flex gap-2">
                    <Button icon="pi pi-heart" rounded text />
                    <Button icon="pi pi-bookmark" rounded text />
                  </div>
                </div>

                {/* Main Image */}
                <div className="border-round-2xl overflow-hidden">
                  <img 
                    src="https://placehold.co/800x500" 
                    alt="Fitness coaching" 
                    className="w-full"
                  />
                </div>

                {/* Action Buttons */}
                <div className="flex gap-3 mt-4">
                  <Button icon="pi pi-heart" rounded outlined />
                  <Button icon="pi pi-comment" rounded outlined />
                  <Button icon="pi pi-share-alt" rounded outlined />
                  <Button icon="pi pi-ellipsis-v" rounded outlined className="ml-auto" />
                </div>
              </div>

              <div className="col-12 lg:col-4">
                <div className="flex flex-column gap-3">
                  <div className="border-round-2xl overflow-hidden">
                    <img 
                      src="https://placehold.co/400x300" 
                      alt="Image 1" 
                      className="w-full"
                    />
                  </div>
                  <div className="border-round-2xl overflow-hidden">
                    <img 
                      src="https://placehold.co/400x300" 
                      alt="Image 2" 
                      className="w-full"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Call to Action */}
          <div className="surface-100 border-round-3xl p-4 text-center mt-5">
            <p className="text-lg m-0">
              Wanna create something great? <strong>Feel free to contact me</strong>
            </p>
          </div>

          {/* Overview Section */}
          <div className="surface-card border-round-3xl shadow-2 p-5 mt-5">
            <h3 className="text-3xl font-bold mb-3">Overview</h3>
            <p className="text-600 line-height-3 mb-4">
              Excited to share this clean and modern wireframe for a fitness coaching website! 
              This design focuses on user engagement and a clear information hierarchy, guiding 
              potential clients through the coach's offerings seamlessly. The layout emphasizes 
              the coach's expertise, showcases services, and encourages action with strategically 
              placed CTAs.
            </p>

            <div className="border-round-2xl overflow-hidden mb-4">
              <img 
                src="https://placehold.co/1200x400" 
                alt="Overview banner" 
                className="w-full"
              />
            </div>

            {/* Coach Profile Card */}
            <div className="flex align-items-center gap-3 surface-100 border-round-2xl p-3 inline-flex">
              <Avatar 
                image="https://placehold.co/80x80" 
                size="xlarge" 
                shape="square"
              />
              <div>
                <h4 className="text-xl font-bold m-0 mb-2">Coach Natalia</h4>
                <Button label="Get in touch" outlined />
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="surface-card border-top-1 surface-border py-6 mt-8">
        <div className="max-w-7xl mx-auto px-4">
          <div className="grid align-items-center">
            <div className="col-12 md:col-4">
              <h2 className="text-3xl font-bold m-0">
                <span className="text-900">health</span>
                <span className="text-primary">sync</span>
              </h2>
            </div>
            
            <div className="col-12 md:col-4">
              <div className="flex flex-wrap gap-4 justify-content-center">
                <a href="#inspiration" className="text-600 no-underline">Inspiration</a>
                <a href="#about" className="text-600 no-underline">About</a>
                <a href="#support" className="text-600 no-underline">Support</a>
                <a href="#blog" className="text-600 no-underline">Blog</a>
                <a href="#pts" className="text-600 no-underline">PTs</a>
              </div>
            </div>

            <div className="col-12 md:col-4">
              <div className="flex gap-3 justify-content-end">
                <Button icon="pi pi-twitter" rounded text />
                <Button icon="pi pi-facebook" rounded text />
                <Button icon="pi pi-instagram" rounded text />
              </div>
            </div>
          </div>

          <div className="border-top-1 surface-border pt-4 mt-4">
            <div className="flex flex-wrap gap-4 justify-content-between text-sm text-600">
              <span>© healthsync 2025</span>
              <div className="flex gap-4">
                <a href="#terms" className="text-600 no-underline">Term&Conditions</a>
                <a href="#cookies" className="text-600 no-underline">Cookies</a>
                <a href="#resources" className="text-600 no-underline">Resources</a>
                <a href="#tags" className="text-600 no-underline">Tags</a>
                <a href="#freelancers" className="text-600 no-underline">Freelancers</a>
              </div>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}
