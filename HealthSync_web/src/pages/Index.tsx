import { Heart, Bookmark, MessageCircle, Share2, MoreVertical } from "lucide-react";
import { Button } from "@/components/ui/button";
import Header from "@/components/Header";
import Footer from "@/components/Footer";

export default function Index() {
  return (
    <div className="min-h-screen bg-[#D9D7B6]">
      <Header />

      <main className="max-w-[1434px] mx-auto px-4 md:px-8 lg:px-12 xl:px-16">
        <section className="mb-6 md:mb-8">
          <h2 className="font-serif text-base md:text-lg lg:text-xl mb-4">
            Dynamic Fitness Coaching Website
          </h2>

          <div className="flex items-start md:items-center justify-between gap-4 mb-6 flex-wrap">
            <div className="flex items-center gap-3 md:gap-4 flex-wrap">
              <img
                src="https://api.builder.io/api/v1/image/assets/TEMP/203282ca523b38453ad8c2b8dd91921f06024637?width=80"
                alt="Coach Natalia"
                className="w-10 h-10 rounded-full object-cover"
              />
              <span className="font-serif text-base md:text-lg lg:text-xl">Coach Natalia</span>
              <span className="font-serif text-base md:text-lg lg:text-xl">Available for work</span>
              <Button className="bg-black text-white hover:bg-black/90 rounded-lg px-4 md:px-6 h-9 md:h-10 text-sm md:text-base">
                Follow
              </Button>
            </div>

            <div className="flex items-center gap-2 md:gap-3">
              <button className="w-10 h-10 md:w-12 md:h-12 rounded-full bg-[#FDFBD4] flex items-center justify-center hover:bg-[#FDFBD4]/80 transition-colors">
                <Heart className="w-5 h-5 md:w-6 md:h-6" />
              </button>
              <button className="w-10 h-10 md:w-12 md:h-12 rounded-full bg-[#FDFBD4] flex items-center justify-center hover:bg-[#FDFBD4]/80 transition-colors">
                <Bookmark className="w-5 h-5 md:w-6 md:h-6" />
              </button>
              <button className="w-10 h-10 md:w-12 md:h-12 rounded-full bg-[#FDFBD4] flex items-center justify-center hover:bg-[#FDFBD4]/80 transition-colors">
                <MessageCircle className="w-5 h-5 md:w-6 md:h-6" />
              </button>
              <button className="w-10 h-10 md:w-12 md:h-12 rounded-full bg-[#FDFBD4] flex items-center justify-center hover:bg-[#FDFBD4]/80 transition-colors">
                <Share2 className="w-5 h-5 md:w-6 md:h-6" />
              </button>
              <button className="w-10 h-10 md:w-12 md:h-12 rounded-full bg-[#FDFBD4] flex items-center justify-center hover:bg-[#FDFBD4]/80 transition-colors">
                <MoreVertical className="w-5 h-5 md:w-6 md:h-6" />
              </button>
            </div>
          </div>

          <Button className="bg-[#FDFBD4] text-black hover:bg-[#FDFBD4]/90 rounded-lg px-4 md:px-6 h-9 md:h-10 border border-black text-sm md:text-base mb-6">
            Get in touch
          </Button>
        </section>

        <section className="mb-8 md:mb-12">
          <div className="relative rounded-xl overflow-hidden bg-white shadow-sm">
            <img
              src="https://api.builder.io/api/v1/image/assets/TEMP/e562a8c59d085e50c143f29d2bcf90c09bdd82b3?width=1682"
              alt="Fitness coaching hero - It's Time to Regain Your Fitness"
              className="w-full h-auto object-cover"
            />
          </div>
        </section>

        <section className="mb-8 md:mb-12">
          <div className="relative rounded-xl overflow-hidden shadow-sm">
            <img
              src="https://api.builder.io/api/v1/image/assets/TEMP/49f9d7dfe50aa221da51fe9ae3eed0392c09d95e?width=1680"
              alt="Fitness coaching portfolio showcase"
              className="w-full h-auto object-cover"
            />
          </div>
        </section>

        <section className="mb-8 md:mb-12">
          <h2 className="text-2xl md:text-3xl lg:text-4xl font-medium mb-4 md:mb-6 leading-tight">
            Wanna create something great? Feel free to contact me
          </h2>

          <h3 className="text-2xl md:text-3xl lg:text-4xl font-medium mb-4 md:mb-6 leading-tight">Overview</h3>

          <p className="text-xl md:text-2xl lg:text-3xl xl:text-4xl font-light leading-tight mb-8 md:mb-12 max-w-5xl">
            Excited to share this clean and modern wireframe for a fitness coaching website! 
            This design focuses on user engagement and a clear information hierarchy, guiding 
            potential clients through the coach&apos;s offerings seamlessly. The layout emphasizes 
            the coach&apos;s expertise, showcases services, and encourages action with strategically 
            placed CTAs.
          </p>

          <div className="relative rounded-xl overflow-hidden mb-8 md:mb-12 shadow-sm">
            <img
              src="https://api.builder.io/api/v1/image/assets/TEMP/6c4ce5b8cdddf2258704790f473ac199bd21cad7?width=1690"
              alt="Where do we sweat - Fitness locations"
              className="w-full h-auto object-cover"
            />
          </div>
        </section>

        <section className="mb-12 md:mb-16 text-center">
          <div className="inline-block mb-4 md:mb-6">
            <img
              src="https://api.builder.io/api/v1/image/assets/TEMP/4e07f9ce70a0a210464bed673b7acb705fc502e6?width=286"
              alt="Coach Natalia"
              className="w-28 h-28 md:w-36 md:h-36 rounded-3xl object-cover mb-4"
            />
          </div>
          <h3 className="font-serif text-lg md:text-xl mb-4 md:mb-6">Coach Natalia</h3>
          <Button className="bg-black text-white hover:bg-black/90 rounded-lg px-6 md:px-8 py-4 md:py-6 text-lg md:text-xl font-bold h-auto">
            Get in touch
          </Button>
        </section>
      </main>

      <Footer />
    </div>
  );
}
