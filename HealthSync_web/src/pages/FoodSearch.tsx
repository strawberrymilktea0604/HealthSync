import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { foodItemService, FoodItem } from '@/services/foodItemService';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Card, CardContent } from '@/components/ui/card';
import { Plus, Search, Filter } from 'lucide-react';
import Header from '@/components/Header';
import { toast } from '@/hooks/use-toast';
import { useAuth } from '@/contexts/AuthContext';

const FoodSearch = () => {
  const { user } = useAuth();
  const [foodItems, setFoodItems] = useState<FoodItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  // Filters state
  const [category, setCategory] = useState('all');
  const [calorieRange, setCalorieRange] = useState('all');
  const [proteinRange, setProteinRange] = useState('all');
  const [carbRange, setCarbRange] = useState('all');

  useEffect(() => {
    loadFoodItems();
  }, []);

  const loadFoodItems = async () => {
    try {
      setLoading(true);
      const data = await foodItemService.getFoodItems({ search: searchQuery });
      setFoodItems(data);
    } catch (error) {
      console.error('Failed to load food items:', error);
      toast({
        title: "Lỗi",
        description: "Không thể tải danh sách món ăn",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = () => {
    loadFoodItems();
  };

  // Helper functions for filtering (reduces cognitive complexity)
  const matchesCategory = (name: string, cat: string): boolean => {
    if (cat === 'all') return true;
    const lowerName = name.toLowerCase();
    const categoryPatterns: Record<string, RegExp> = {
      main: /(cơm|phở|bún|miến|hủ tiếu|bánh mì|mì|canh|lẩu)/,
      side: /(rau|canh|súp|xào|luộc|gỏi|salad)/,
      snack: /(trái cây|bánh|chè|kem|sữa chua|trà|nước)/
    };
    return categoryPatterns[cat]?.test(lowerName) ?? true;
  };

  const matchesCalorieRange = (cal: number, range: string): boolean => {
    const ranges: Record<string, (c: number) => boolean> = {
      all: () => true,
      low: (c) => c <= 200,
      medium: (c) => c > 200 && c <= 500,
      high: (c) => c > 500
    };
    return ranges[range]?.(cal) ?? true;
  };

  const matchesProteinRange = (protein: number, range: string): boolean => {
    const ranges: Record<string, (p: number) => boolean> = {
      all: () => true,
      high: (p) => p >= 20,
      low: (p) => p < 20
    };
    return ranges[range]?.(protein) ?? true;
  };

  const matchesCarbRange = (carbs: number, range: string): boolean => {
    const ranges: Record<string, (c: number) => boolean> = {
      all: () => true,
      low: (c) => c <= 30,
      high: (c) => c > 30
    };
    return ranges[range]?.(carbs) ?? true;
  };

  // Client-side filtering logic using helper functions
  const filteredItems = foodItems.filter(item =>
    matchesCategory(item.name, category) &&
    matchesCalorieRange(item.caloriesKcal, calorieRange) &&
    matchesProteinRange(item.proteinG, proteinRange) &&
    matchesCarbRange(item.carbsG, carbRange)
  );

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <main className="max-w-7xl mx-auto px-4 md:px-8 pb-12 pt-4">
        {/* Header Section */}
        <div className="text-center mb-8">
          <div className="flex items-center justify-center gap-2 mb-2">
            <h2 className="text-2xl font-serif text-gray-700 italic">Welcome back,</h2>
            {user && <span className="font-bold text-gray-900">{user.fullName}</span>}
          </div>

          <h1 className="text-4xl font-serif font-medium text-gray-800 mb-2">Welcome to Your Nutrition</h1>
          <h3 className="text-2xl font-bold text-gray-900">Tìm kiếm món ăn</h3>
          <p className="text-gray-500 mt-2">Tìm và thêm món ăn vào dinh dưỡng của bạn</p>
        </div>

        {/* Search & Filter Card */}
        <Card className="bg-[#FFFDF7]/80 backdrop-blur-md border-white/50 shadow-sm rounded-[2.5rem] p-6 mb-8">
          <div className="space-y-4">
            {/* Search Bar */}
            <div className="relative">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
              <Input
                placeholder="Tìm kiếm món ăn (ví dụ: phở bò, cơm tấm...)"
                className="pl-12 h-14 text-lg bg-gray-50/50 border-transparent hover:bg-white transition-colors rounded-2xl"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
              />
            </div>

            {/* Filters Row */}
            <div className="grid grid-cols-1 md:grid-cols-5 gap-3">
              <Select value={category} onValueChange={setCategory}>
                <SelectTrigger className="h-12 rounded-xl bg-gray-50/50 border-transparent">
                  <SelectValue placeholder="Loại món ăn" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả loại</SelectItem>
                  <SelectItem value="main">Món chính</SelectItem>
                  <SelectItem value="side">Món phụ</SelectItem>
                  <SelectItem value="snack">Ăn vặt</SelectItem>
                </SelectContent>
              </Select>

              <Select value={calorieRange} onValueChange={setCalorieRange}>
                <SelectTrigger className="h-12 rounded-xl bg-gray-50/50 border-transparent">
                  <SelectValue placeholder="Calories" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả Calo</SelectItem>
                  <SelectItem value="low">Thấp (&lt; 200)</SelectItem>
                  <SelectItem value="medium">Vừa (200 - 500)</SelectItem>
                  <SelectItem value="high">Cao (&gt; 500)</SelectItem>
                </SelectContent>
              </Select>

              <Select value={proteinRange} onValueChange={setProteinRange}>
                <SelectTrigger className="h-12 rounded-xl bg-gray-50/50 border-transparent">
                  <SelectValue placeholder="Protein" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả Protein</SelectItem>
                  <SelectItem value="high">Giàu Protein (&gt; 20g)</SelectItem>
                  <SelectItem value="low">Protein thường</SelectItem>
                </SelectContent>
              </Select>

              <Select value={carbRange} onValueChange={setCarbRange}>
                <SelectTrigger className="h-12 rounded-xl bg-gray-50/50 border-transparent">
                  <SelectValue placeholder="Carb" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả Carb</SelectItem>
                  <SelectItem value="low">Low Carb</SelectItem>
                  <SelectItem value="high">High Carb</SelectItem>
                </SelectContent>
              </Select>

              <Button
                onClick={handleSearch}
                className="h-12 rounded-xl bg-[#FDBA74] hover:bg-[#FB923C] text-white font-bold text-base shadow-sm"
              >
                Áp dụng
              </Button>
            </div>
          </div>
        </Card>

        {/* Results Info */}
        <div className="mb-6 font-semibold text-gray-700">
          Kết quả tìm kiếm ({filteredItems.length})
        </div>

        {/* Grid Results */}
        {loading ? (
          <div className="text-center py-20">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-orange-400 mx-auto"></div>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {filteredItems.map((item) => (
              <div
                key={item.foodItemId}
                className="group bg-white rounded-[2rem] p-4 pb-6 shadow-sm hover:shadow-xl transition-all duration-300 border border-transparent hover:border-orange-100 cursor-pointer"
              >
                {/* Image Container */}
                <div className="relative aspect-[4/3] mb-4 overflow-hidden rounded-[1.5rem] bg-gray-100">
                  <img
                    src={item.imageUrl || "https://placehold.co/400x300?text=No+Image"}
                    alt={item.name}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                  />
                  <div className="absolute top-3 right-3 bg-white/90 p-1.5 rounded-full shadow-sm hover:bg-orange-100 transition-colors">
                    <Plus className="w-5 h-5 text-gray-600" />
                  </div>
                </div>

                {/* Content */}
                <div className="px-1">
                  <h3 className="font-bold text-gray-900 text-lg mb-1 line-clamp-1">{item.name}</h3>
                  <p className="text-xs text-gray-500 mb-3">
                    {item.servingSize} {item.servingUnit}
                  </p>

                  {/* Macros Tags */}
                  <div className="flex flex-wrap gap-2 text-[10px] font-bold uppercase tracking-wider">
                    <span className="bg-[#FEF08A] text-[#854D0E] px-2 py-1 rounded-md">
                      {item.caloriesKcal.toFixed(0)} kcal
                    </span>
                    <span className="bg-gray-100 text-gray-600 px-2 py-1 rounded-md">
                      P: {item.proteinG.toFixed(1)}g
                    </span>
                    <span className="bg-gray-100 text-gray-600 px-2 py-1 rounded-md">
                      C: {item.carbsG.toFixed(1)}g
                    </span>
                    <span className="bg-gray-100 text-gray-600 px-2 py-1 rounded-md">
                      F: {item.fatG.toFixed(1)}g
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  );
};

export default FoodSearch;
