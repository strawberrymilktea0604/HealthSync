import { useState } from "react";
import { Button } from "primereact/button";
import { Avatar } from "primereact/avatar";
import { Card } from "primereact/card";
import { InputText } from "primereact/inputtext";
import { Dropdown } from "primereact/dropdown";

interface FoodSearchResult {
  id: number;
  name: string;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  servingSize: string;
  category: string;
}

export default function FoodSearch() {
  const [userInfo] = useState({
    fullName: "Nguyen Duc Manh",
    avatarUrl: "https://placehold.co/100x100"
  });

  const [searchQuery, setSearchQuery] = useState("");
  const [mealType, setMealType] = useState("breakfast");
  const [searchResults, setSearchResults] = useState<FoodSearchResult[]>([]);

  const mealTypes = [
    { label: "Bữa sáng", value: "breakfast" },
    { label: "Bữa trưa", value: "lunch" },
    { label: "Bữa tối", value: "dinner" },
    { label: "Bữa phụ", value: "snack" }
  ];

  const sampleResults: FoodSearchResult[] = [
    {
      id: 1,
      name: "Cơm gạo lứt",
      calories: 350,
      protein: 7,
      carbs: 76,
      fat: 3,
      servingSize: "1 chén (200g)",
      category: "Carbs"
    },
    {
      id: 2,
      name: "Ức gà luộc",
      calories: 165,
      protein: 31,
      carbs: 0,
      fat: 4,
      servingSize: "100g",
      category: "Protein"
    },
    {
      id: 3,
      name: "Salad rau củ",
      calories: 120,
      protein: 3,
      carbs: 15,
      fat: 6,
      servingSize: "1 bát (150g)",
      category: "Vegetables"
    },
    {
      id: 4,
      name: "Trứng chiên",
      calories: 155,
      protein: 13,
      carbs: 1,
      fat: 11,
      servingSize: "1 quả",
      category: "Protein"
    },
    {
      id: 5,
      name: "Chuối",
      calories: 105,
      protein: 1,
      carbs: 27,
      fat: 0,
      servingSize: "1 quả trung bình",
      category: "Fruit"
    },
    {
      id: 6,
      name: "Sữa chua Hy Lạp",
      calories: 100,
      protein: 10,
      carbs: 6,
      fat: 4,
      servingSize: "1 hộp (170g)",
      category: "Dairy"
    }
  ];

  const handleSearch = () => {
    if (searchQuery.trim()) {
      const filtered = sampleResults.filter(item =>
        item.name.toLowerCase().includes(searchQuery.toLowerCase())
      );
      setSearchResults(filtered);
    } else {
      setSearchResults(sampleResults);
    }
  };

  const handleAddFood = async (food: FoodSearchResult) => {
    try {
      const response = await fetch('/api/nutrition/food-entry', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          foodItemId: food.id,
          quantity: 1, // or some quantity
          mealType: mealType
        })
      });
      if (response.ok) {
        alert(`Added ${food.name} to ${mealType}`);
      } else {
        alert("Failed to add food");
      }
    } catch {
      alert("Error adding food");
    }
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
          <h3 className="text-3xl font-bold m-0 mb-3">Tìm kiếm món ăn</h3>
          <p className="text-600">Tìm và thêm món ăn vào dinh dưỡng của bạn</p>
        </div>

        {/* Search Section */}
        <Card className="mb-4">
          <div className="grid">
            <div className="col-12 md:col-8">
              <div className="p-inputgroup flex-1">
                <span className="p-inputgroup-addon">
                  <i className="pi pi-search"></i>
                </span>
                <InputText 
                  placeholder="Tìm kiếm món ăn (ví dụ: cơm, gà, trứng...)" 
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                  className="w-full"
                />
              </div>
            </div>
            <div className="col-12 md:col-4">
              <Dropdown
                value={mealType}
                onChange={(e) => setMealType(e.value)}
                options={mealTypes}
                placeholder="Chọn bữa ăn"
                className="w-full"
              />
            </div>
            <div className="col-12">
              <Button 
                label="Tìm kiếm" 
                icon="pi pi-search" 
                onClick={handleSearch}
                className="w-full md:w-auto"
              />
            </div>
          </div>
        </Card>

        {/* Search Results */}
        {searchResults.length > 0 ? (
          <>
            <div className="mb-3">
              <p className="text-xl font-semibold">
                Kết quả tìm kiếm: {searchResults.length} món
              </p>
            </div>

            <div className="grid">
              {searchResults.map((food) => (
                <div key={food.id} className="col-12">
                  <Card className="hover:shadow-3 transition-all transition-duration-300">
                    <div className="grid align-items-center">
                      {/* Food Info */}
                      <div className="col-12 md:col-4">
                        <h4 className="text-xl font-bold m-0 mb-2">{food.name}</h4>
                        <span className="px-2 py-1 border-round text-xs font-semibold surface-100">
                          {food.category}
                        </span>
                        <p className="text-600 mt-2 mb-0">{food.servingSize}</p>
                      </div>

                      {/* Nutrition Stats */}
                      <div className="col-12 md:col-6">
                        <div className="grid">
                          <div className="col-6 lg:col-3 text-center">
                            <p className="text-sm text-600 m-0">Calories</p>
                            <p className="text-lg font-bold m-0">{food.calories}</p>
                            <p className="text-xs text-600 m-0">kcal</p>
                          </div>
                          <div className="col-6 lg:col-3 text-center">
                            <p className="text-sm text-600 m-0">Protein</p>
                            <p className="text-lg font-bold m-0">{food.protein}</p>
                            <p className="text-xs text-600 m-0">g</p>
                          </div>
                          <div className="col-6 lg:col-3 text-center">
                            <p className="text-sm text-600 m-0">Carbs</p>
                            <p className="text-lg font-bold m-0">{food.carbs}</p>
                            <p className="text-xs text-600 m-0">g</p>
                          </div>
                          <div className="col-6 lg:col-3 text-center">
                            <p className="text-sm text-600 m-0">Fat</p>
                            <p className="text-lg font-bold m-0">{food.fat}</p>
                            <p className="text-xs text-600 m-0">g</p>
                          </div>
                        </div>
                      </div>

                      {/* Action Button */}
                      <div className="col-12 md:col-2">
                        <Button 
                          label="Thêm" 
                          icon="pi pi-plus" 
                          onClick={() => handleAddFood(food)}
                          className="w-full"
                        />
                      </div>
                    </div>
                  </Card>
                </div>
              ))}
            </div>
          </>
        ) : searchQuery && searchResults.length === 0 ? (
          <div className="text-center py-8">
            <i className="pi pi-search text-6xl text-400 mb-3"></i>
            <p className="text-xl text-600">Không tìm thấy kết quả</p>
            <p className="text-600">Hãy thử tìm kiếm với từ khóa khác</p>
          </div>
        ) : (
          <Card className="text-center">
            <i className="pi pi-search text-6xl text-400 mb-3"></i>
            <p className="text-xl font-semibold mb-2">Tìm kiếm món ăn</p>
            <p className="text-600">
              Nhập tên món ăn bạn muốn tìm và chọn bữa ăn để bắt đầu
            </p>
          </Card>
        )}
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
