import { useState } from "react";
import { Button } from "primereact/button";
import { Avatar } from "primereact/avatar";
import { Card } from "primereact/card";
import { ProgressBar } from "primereact/progressbar";

export default function NutritionOverview() {
  const [userInfo] = useState({
    fullName: "Nguyen Duc Manh",
    avatarUrl: "https://placehold.co/100x100"
  });

  const nutritionData = {
    dailyCalories: 1800,
    targetCalories: 2000,
    protein: 80,
    targetProtein: 100,
    carbs: 200,
    targetCarbs: 250,
    fat: 50,
    targetFat: 65
  };

  return (
    <div className="min-h-screen surface-ground">
      {/* Header */}
      <header className="surface-card border-bottom-1 surface-border px-4 py-3">
        <div className="flex align-items-center justify-content-between max-w-7xl mx-auto">
          <div className="flex align-items-center gap-3">
            <h1 className="text-3xl font-bold m-0">
              <span className="text-900">health</span>
              <span className="text-primary">sync</span>
            </h1>
          </div>

          <div className="flex align-items-center gap-3">
            <Avatar 
              image={userInfo.avatarUrl} 
              size="large" 
              shape="circle" 
            />
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 py-6">
        {/* Welcome Section */}
        <div className="mb-5">
          <p className="text-xl mb-2">Welcome to</p>
          <h2 className="text-4xl font-bold m-0">
            <span className="text-900">health</span>
            <span className="text-primary">sync</span>
          </h2>
          <p className="text-600 mt-2">{userInfo.fullName}</p>
        </div>

        {/* Page Title */}
        <div className="mb-5">
          <h3 className="text-3xl font-bold m-0">Welcome to Your Nutrition</h3>
        </div>

        {/* Nutrition Overview Card */}
        <Card className="shadow-3" style={{ borderRadius: '2rem' }}>
          <div className="grid">
            {/* Main Nutrition Stats */}
            <div className="col-12 lg:col-8">
              <div className="mb-5">
                <h4 className="text-2xl font-bold mb-4">Tổng quan dinh dưỡng hôm nay</h4>
                
                {/* Calories */}
                <div className="mb-4 p-4 surface-100 border-round-xl">
                  <div className="flex align-items-center justify-content-between mb-2">
                    <span className="font-semibold">Calories</span>
                    <span className="text-xl font-bold">{nutritionData.dailyCalories}/{nutritionData.targetCalories} kcal</span>
                  </div>
                  <ProgressBar 
                    value={(nutritionData.dailyCalories / nutritionData.targetCalories) * 100} 
                    showValue={false}
                    style={{ height: '12px' }}
                  />
                </div>

                {/* Protein */}
                <div className="mb-4 p-4 surface-100 border-round-xl">
                  <div className="flex align-items-center justify-content-between mb-2">
                    <span className="font-semibold">Protein</span>
                    <span className="text-xl font-bold">{nutritionData.protein}/{nutritionData.targetProtein}g</span>
                  </div>
                  <ProgressBar 
                    value={(nutritionData.protein / nutritionData.targetProtein) * 100} 
                    showValue={false}
                    style={{ height: '12px' }}
                    color="#4CAF50"
                  />
                </div>

                {/* Carbs */}
                <div className="mb-4 p-4 surface-100 border-round-xl">
                  <div className="flex align-items-center justify-content-between mb-2">
                    <span className="font-semibold">Carbs</span>
                    <span className="text-xl font-bold">{nutritionData.carbs}/{nutritionData.targetCarbs}g</span>
                  </div>
                  <ProgressBar 
                    value={(nutritionData.carbs / nutritionData.targetCarbs) * 100} 
                    showValue={false}
                    style={{ height: '12px' }}
                    color="#2196F3"
                  />
                </div>

                {/* Fat */}
                <div className="mb-4 p-4 surface-100 border-round-xl">
                  <div className="flex align-items-center justify-content-between mb-2">
                    <span className="font-semibold">Fat</span>
                    <span className="text-xl font-bold">{nutritionData.fat}/{nutritionData.targetFat}g</span>
                  </div>
                  <ProgressBar 
                    value={(nutritionData.fat / nutritionData.targetFat) * 100} 
                    showValue={false}
                    style={{ height: '12px' }}
                    color="#FF9800"
                  />
                </div>
              </div>

              {/* Meal Breakdown */}
              <div>
                <h4 className="text-xl font-bold mb-3">Bữa ăn hôm nay</h4>
                <div className="grid">
                  <div className="col-12 md:col-4">
                    <Card className="text-center" style={{ backgroundColor: '#FFF3E0' }}>
                      <i className="pi pi-sun text-5xl text-orange-500 mb-3"></i>
                      <h5 className="text-lg font-bold m-0 mb-2">Sáng</h5>
                      <p className="text-600 m-0">450 kcal</p>
                    </Card>
                  </div>
                  <div className="col-12 md:col-4">
                    <Card className="text-center" style={{ backgroundColor: '#E3F2FD' }}>
                      <i className="pi pi-cloud text-5xl text-blue-500 mb-3"></i>
                      <h5 className="text-lg font-bold m-0 mb-2">Trưa</h5>
                      <p className="text-600 m-0">750 kcal</p>
                    </Card>
                  </div>
                  <div className="col-12 md:col-4">
                    <Card className="text-center" style={{ backgroundColor: '#F3E5F5' }}>
                      <i className="pi pi-moon text-5xl text-purple-500 mb-3"></i>
                      <h5 className="text-lg font-bold m-0 mb-2">Tối</h5>
                      <p className="text-600 m-0">600 kcal</p>
                    </Card>
                  </div>
                </div>
              </div>
            </div>

            {/* Side Panel */}
            <div className="col-12 lg:col-4">
              <Card className="h-full" style={{ backgroundColor: '#F5F5F5' }}>
                <div className="text-center mb-4">
                  <img 
                    src="https://placehold.co/300x200" 
                    alt="Nutrition tips" 
                    className="w-full border-round-2xl"
                  />
                </div>
                
                <h5 className="text-lg font-bold mb-3">Gợi ý hôm nay</h5>
                <p className="text-600 line-height-3 mb-4">
                  Bạn đang thiếu 20g protein. Hãy thêm thực phẩm giàu protein như trứng, thịt gà hoặc đậu phụ vào bữa tối nhé!
                </p>

                <Button 
                  label="Xem gợi ý món ăn" 
                  className="w-full mb-3"
                  outlined
                />
                <Button 
                  label="Ghi bữa ăn" 
                  className="w-full"
                />
              </Card>
            </div>
          </div>
        </Card>

        {/* Quick Actions */}
        <div className="grid mt-5">
          <div className="col-12 md:col-6">
            <Button 
              label="Danh sách món ăn" 
              icon="pi pi-list" 
              className="w-full p-4 text-xl"
              style={{ borderRadius: '2rem' }}
              onClick={() => window.location.href = '/food-list'}
            />
          </div>
          <div className="col-12 md:col-6">
            <Button 
              label="Tìm kiếm món ăn" 
              icon="pi pi-search" 
              className="w-full p-4 text-xl"
              outlined
              style={{ borderRadius: '2rem' }}
              onClick={() => window.location.href = '/food-search'}
            />
          </div>
        </div>
      </main>

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
