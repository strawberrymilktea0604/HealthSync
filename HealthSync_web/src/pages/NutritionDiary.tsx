import { useState } from "react";
import { Button } from "primereact/button";
import { Avatar } from "primereact/avatar";
import { Card } from "primereact/card";
import { Calendar } from "primereact/calendar";

interface DiaryEntry {
  id: number;
  date: string;
  meals: {
    breakfast: FoodEntry[];
    lunch: FoodEntry[];
    dinner: FoodEntry[];
    snack: FoodEntry[];
  };
  totalCalories: number;
  totalProtein: number;
  totalCarbs: number;
  totalFat: number;
}

interface FoodEntry {
  name: string;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  servingSize: string;
}

export default function NutritionDiary() {
  const [userInfo] = useState({
    fullName: "Nguyen Duc Manh",
    avatarUrl: "https://placehold.co/100x100"
  });

  const [selectedDate, setSelectedDate] = useState<Date>(new Date());

  const diaryEntries: DiaryEntry[] = [
    {
      id: 1,
      date: "2025-01-20",
      meals: {
        breakfast: [
          { name: "Bánh mì", calories: 200, protein: 6, carbs: 35, fat: 5, servingSize: "1 ổ" },
          { name: "Trứng chiên", calories: 155, protein: 13, carbs: 1, fat: 11, servingSize: "1 quả" }
        ],
        lunch: [
          { name: "Cơm gạo lứt", calories: 350, protein: 7, carbs: 76, fat: 3, servingSize: "1 chén" },
          { name: "Ức gà luộc", calories: 165, protein: 31, carbs: 0, fat: 4, servingSize: "100g" }
        ],
        dinner: [
          { name: "Salad rau củ", calories: 120, protein: 3, carbs: 15, fat: 6, servingSize: "1 bát" },
          { name: "Cá hồi nướng", calories: 280, protein: 25, carbs: 0, fat: 18, servingSize: "150g" }
        ],
        snack: [
          { name: "Chuối", calories: 105, protein: 1, carbs: 27, fat: 0, servingSize: "1 quả" }
        ]
      },
      totalCalories: 1375,
      totalProtein: 86,
      totalCarbs: 154,
      totalFat: 47
    }
  ];

  const currentEntry = diaryEntries.find(
    entry => entry.date === selectedDate.toISOString().split('T')[0]
  ) || {
    id: 0,
    date: selectedDate.toISOString().split('T')[0],
    meals: { breakfast: [], lunch: [], dinner: [], snack: [] },
    totalCalories: 0,
    totalProtein: 0,
    totalCarbs: 0,
    totalFat: 0
  };

  const renderMealSection = (mealName: string, foods: FoodEntry[], icon: string, color: string) => (
    <Card className="mb-3">
      <div className="flex align-items-center gap-3 mb-3">
        <div 
          className="flex align-items-center justify-content-center border-circle"
          style={{ width: '48px', height: '48px', backgroundColor: color }}
        >
          <i className={`pi ${icon} text-white text-2xl`}></i>
        </div>
        <h4 className="text-xl font-bold m-0">{mealName}</h4>
      </div>

      {foods.length > 0 ? (
        <div className="flex flex-column gap-3">
          {foods.map((food, idx) => (
            <div key={`${food.name}-${idx}`} className="surface-100 border-round-2xl p-3">
              <div className="flex justify-content-between align-items-start mb-2">
                <div>
                  <p className="font-semibold m-0 mb-1">{food.name}</p>
                  <p className="text-sm text-600 m-0">{food.servingSize}</p>
                </div>
                <Button 
                  icon="pi pi-times" 
                  rounded 
                  text 
                  severity="danger"
                  size="small"
                />
              </div>
              
              <div className="grid mt-2">
                <div className="col-3 text-center">
                  <p className="text-xs text-600 m-0">Calo</p>
                  <p className="font-bold m-0">{food.calories}</p>
                </div>
                <div className="col-3 text-center">
                  <p className="text-xs text-600 m-0">Protein</p>
                  <p className="font-bold m-0">{food.protein}g</p>
                </div>
                <div className="col-3 text-center">
                  <p className="text-xs text-600 m-0">Carbs</p>
                  <p className="font-bold m-0">{food.carbs}g</p>
                </div>
                <div className="col-3 text-center">
                  <p className="text-xs text-600 m-0">Fat</p>
                  <p className="font-bold m-0">{food.fat}g</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="text-center py-4 text-600">
          <i className="pi pi-info-circle text-3xl mb-2"></i>
          <p className="m-0">Chưa có món ăn nào</p>
        </div>
      )}

      <Button 
        label="Thêm món" 
        icon="pi pi-plus" 
        className="w-full mt-3"
        outlined
      />
    </Card>
  );

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
          <h3 className="text-3xl font-bold m-0 mb-3">Nhật ký dinh dưỡng</h3>
          <p className="text-600">Theo dõi chi tiết dinh dưỡng hàng ngày của bạn</p>
        </div>

        <div className="grid">
          {/* Left Column - Diary Entries */}
          <div className="col-12 lg:col-8">
            {/* Date Selector */}
            <Card className="mb-4">
              <div className="flex align-items-center gap-3">
                <i className="pi pi-calendar text-2xl"></i>
                <div className="flex-1">
                  <Calendar
                    value={selectedDate}
                    onChange={(e) => setSelectedDate(e.value as Date)}
                    dateFormat="dd/mm/yy"
                    showIcon
                    className="w-full"
                  />
                </div>
              </div>
            </Card>

            {/* Daily Summary */}
            <Card className="mb-4" style={{ backgroundColor: '#e3f2fd' }}>
              <h4 className="text-xl font-bold mb-3">Tổng kết ngày</h4>
              <div className="grid">
                <div className="col-6 md:col-3 text-center">
                  <p className="text-sm text-600 m-0 mb-1">Tổng Calories</p>
                  <p className="text-2xl font-bold m-0">{currentEntry.totalCalories}</p>
                  <p className="text-xs text-600 m-0">kcal</p>
                </div>
                <div className="col-6 md:col-3 text-center">
                  <p className="text-sm text-600 m-0 mb-1">Protein</p>
                  <p className="text-2xl font-bold m-0">{currentEntry.totalProtein}</p>
                  <p className="text-xs text-600 m-0">g</p>
                </div>
                <div className="col-6 md:col-3 text-center">
                  <p className="text-sm text-600 m-0 mb-1">Carbs</p>
                  <p className="text-2xl font-bold m-0">{currentEntry.totalCarbs}</p>
                  <p className="text-xs text-600 m-0">g</p>
                </div>
                <div className="col-6 md:col-3 text-center">
                  <p className="text-sm text-600 m-0 mb-1">Fat</p>
                  <p className="text-2xl font-bold m-0">{currentEntry.totalFat}</p>
                  <p className="text-xs text-600 m-0">g</p>
                </div>
              </div>
            </Card>

            {/* Meal Sections */}
            {renderMealSection("Bữa sáng", currentEntry.meals.breakfast, "pi-sun", "#FFA726")}
            {renderMealSection("Bữa trưa", currentEntry.meals.lunch, "pi-sun", "#66BB6A")}
            {renderMealSection("Bữa tối", currentEntry.meals.dinner, "pi-moon", "#42A5F5")}
            {renderMealSection("Bữa phụ", currentEntry.meals.snack, "pi-apple", "#AB47BC")}
          </div>

          {/* Right Column - Tips & Actions */}
          <div className="col-12 lg:col-4">
            <Card className="mb-3">
              <h4 className="text-xl font-bold mb-3">Lời khuyên hôm nay</h4>
              <div className="flex flex-column gap-3">
                <div className="p-3 border-round-2xl" style={{ backgroundColor: '#fff3e0' }}>
                  <div className="flex align-items-start gap-2">
                    <i className="pi pi-lightbulb text-xl" style={{ color: '#ff9800' }}></i>
                    <div>
                      <p className="font-semibold m-0 mb-1">Uống đủ nước</p>
                      <p className="text-sm text-600 m-0">
                        Đảm bảo uống ít nhất 2 lít nước mỗi ngày
                      </p>
                    </div>
                  </div>
                </div>

                <div className="p-3 border-round-2xl" style={{ backgroundColor: '#e8f5e9' }}>
                  <div className="flex align-items-start gap-2">
                    <i className="pi pi-heart text-xl" style={{ color: '#4caf50' }}></i>
                    <div>
                      <p className="font-semibold m-0 mb-1">Ăn nhiều rau củ</p>
                      <p className="text-sm text-600 m-0">
                        Tăng cường chất xơ từ rau xanh và trái cây
                      </p>
                    </div>
                  </div>
                </div>

                <div className="p-3 border-round-2xl" style={{ backgroundColor: '#e3f2fd' }}>
                  <div className="flex align-items-start gap-2">
                    <i className="pi pi-clock text-xl" style={{ color: '#2196f3' }}></i>
                    <div>
                      <p className="font-semibold m-0 mb-1">Ăn đúng giờ</p>
                      <p className="text-sm text-600 m-0">
                        Duy trì thời gian ăn đều đặn mỗi ngày
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </Card>

            <Card>
              <h4 className="text-xl font-bold mb-3">Thao tác nhanh</h4>
              <div className="flex flex-column gap-2">
                <Button 
                  label="Thêm món ăn" 
                  icon="pi pi-plus" 
                  className="w-full"
                />
                <Button 
                  label="Xem báo cáo tuần" 
                  icon="pi pi-chart-bar" 
                  className="w-full"
                  outlined
                />
                <Button 
                  label="Xuất dữ liệu" 
                  icon="pi pi-download" 
                  className="w-full"
                  outlined
                />
              </div>
            </Card>
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
