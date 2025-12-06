import { useState } from "react";
import { Button } from "primereact/button";
import { Avatar } from "primereact/avatar";
import { Card } from "primereact/card";
import { InputText } from "primereact/inputtext";

interface FoodItem {
  id: number;
  name: string;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  image: string;
  category: string;
}

export default function FoodList() {
  const [userInfo] = useState({
    fullName: "Nguyen Duc Manh",
    avatarUrl: "https://placehold.co/100x100"
  });

  const [searchTerm, setSearchTerm] = useState("");

  const foodItems: FoodItem[] = [
    {
      id: 1,
      name: "Cơm gạo lứt",
      calories: 350,
      protein: 7,
      carbs: 76,
      fat: 3,
      image: "https://placehold.co/300x200",
      category: "Carbs"
    },
    {
      id: 2,
      name: "Ức gà luộc",
      calories: 165,
      protein: 31,
      carbs: 0,
      fat: 4,
      image: "https://placehold.co/300x200",
      category: "Protein"
    },
    {
      id: 3,
      name: "Salad rau củ",
      calories: 120,
      protein: 3,
      carbs: 15,
      fat: 6,
      image: "https://placehold.co/300x200",
      category: "Vegetables"
    },
    {
      id: 4,
      name: "Trứng chiên",
      calories: 155,
      protein: 13,
      carbs: 1,
      fat: 11,
      image: "https://placehold.co/300x200",
      category: "Protein"
    },
    {
      id: 5,
      name: "Chuối",
      calories: 105,
      protein: 1,
      carbs: 27,
      fat: 0,
      image: "https://placehold.co/300x200",
      category: "Fruit"
    },
    {
      id: 6,
      name: "Sữa chua Hy Lạp",
      calories: 100,
      protein: 10,
      carbs: 6,
      fat: 4,
      image: "https://placehold.co/300x200",
      category: "Dairy"
    }
  ];

  const filteredFoods = foodItems.filter(food =>
    food.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    food.category.toLowerCase().includes(searchTerm.toLowerCase())
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
          <h3 className="text-3xl font-bold m-0 mb-3">Danh sách món ăn</h3>
          <p className="text-600">Chọn món ăn để thêm vào nhật ký dinh dưỡng của bạn</p>
        </div>

        {/* Search Bar */}
        <Card className="mb-4">
          <div className="p-inputgroup flex-1">
            <span className="p-inputgroup-addon">
              <i className="pi pi-search"></i>
            </span>
            <InputText 
              placeholder="Tìm kiếm món ăn..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full"
            />
          </div>
        </Card>

        {/* Food Grid */}
        <div className="grid">
          {filteredFoods.map((food) => (
            <div key={food.id} className="col-12 md:col-6 lg:col-4">
              <Card className="h-full hover:shadow-4 transition-all transition-duration-300">
                <div className="flex flex-column gap-3">
                  {/* Food Image */}
                  <div className="border-round-2xl overflow-hidden">
                    <img 
                      src={food.image} 
                      alt={food.name} 
                      className="w-full"
                      style={{ height: '200px', objectFit: 'cover' }}
                    />
                  </div>

                  {/* Food Info */}
                  <div>
                    <div className="flex align-items-center justify-content-between mb-2">
                      <h4 className="text-xl font-bold m-0">{food.name}</h4>
                      <span className="px-2 py-1 border-round text-xs font-semibold surface-100">
                        {food.category}
                      </span>
                    </div>

                    <div className="grid mb-3">
                      <div className="col-6">
                        <p className="text-sm text-600 m-0">Calories</p>
                        <p className="text-lg font-bold m-0">{food.calories} kcal</p>
                      </div>
                      <div className="col-6">
                        <p className="text-sm text-600 m-0">Protein</p>
                        <p className="text-lg font-bold m-0">{food.protein}g</p>
                      </div>
                      <div className="col-6">
                        <p className="text-sm text-600 m-0">Carbs</p>
                        <p className="text-lg font-bold m-0">{food.carbs}g</p>
                      </div>
                      <div className="col-6">
                        <p className="text-sm text-600 m-0">Fat</p>
                        <p className="text-lg font-bold m-0">{food.fat}g</p>
                      </div>
                    </div>

                    <Button 
                      label="Thêm vào nhật ký" 
                      icon="pi pi-plus" 
                      className="w-full"
                    />
                  </div>
                </div>
              </Card>
            </div>
          ))}
        </div>

        {filteredFoods.length === 0 && (
          <div className="text-center py-8">
            <i className="pi pi-search text-6xl text-400 mb-3"></i>
            <p className="text-xl text-600">Không tìm thấy món ăn nào</p>
            <p className="text-600">Hãy thử tìm kiếm với từ khóa khác</p>
          </div>
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
