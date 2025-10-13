import { Link } from "react-router-dom";
import Header from "@/components/Header";
import Footer from "@/components/Footer";

export default function Dashboard() {
  return (
    <div className="min-h-screen bg-[#D9D7B6]">
      <Header />

      <main className="max-w-[1434px] mx-auto px-4 md:px-8 lg:px-12 xl:px-16 py-12">
        <div className="text-center mb-8">
          <h1 className="text-4xl md:text-5xl font-bold mb-4">Your Activities</h1>
          <p className="text-xl text-gray-600">Today, 8 Jul</p>
        </div>

        <div className="bg-[#FDFBD4] rounded-[50px] p-8 md:p-12 shadow-lg">
          <div className="text-center">
            <h2 className="text-3xl font-bold mb-6">Welcome to Your Dashboard!</h2>
            <p className="text-xl mb-8">
              This is your personal fitness tracking dashboard. Start your journey to better health today!
            </p>
            
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-12">
              <div className="bg-white rounded-2xl p-6 shadow">
                <h3 className="text-2xl font-bold mb-2">Daily Workouts</h3>
                <p className="text-gray-600">Track your daily exercise routines</p>
              </div>
              
              <div className="bg-white rounded-2xl p-6 shadow">
                <h3 className="text-2xl font-bold mb-2">Calories</h3>
                <p className="text-gray-600">Monitor your calorie intake and burn</p>
              </div>
              
              <div className="bg-white rounded-2xl p-6 shadow">
                <h3 className="text-2xl font-bold mb-2">Progress</h3>
                <p className="text-gray-600">View your fitness progress over time</p>
              </div>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
