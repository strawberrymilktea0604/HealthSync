import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";
import Header from "@/components/Header";
import Footer from "@/components/Footer";

export default function NotFound() {
  return (
    <div className="min-h-screen bg-[#D9D7B6] flex flex-col">
      <Header />
      
      <main className="flex-1 flex items-center justify-center px-4">
        <div className="text-center max-w-2xl">
          <h1 className="text-6xl md:text-8xl font-bold mb-4">404</h1>
          <h2 className="text-2xl md:text-4xl font-medium mb-6">Page Not Found</h2>
          <p className="text-lg md:text-xl mb-8 text-black/80">
            The page you&apos;re looking for doesn&apos;t exist or has been moved.
          </p>
          <Link to="/">
            <Button className="bg-black text-white hover:bg-black/90 rounded-lg px-8 py-6 text-lg font-medium h-auto">
              Go Back Home
            </Button>
          </Link>
        </div>
      </main>
      
      <Footer />
    </div>
  );
}
