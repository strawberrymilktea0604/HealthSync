import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { foodItemService, FoodItem } from '@/services/foodItemService';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Plus, Search } from 'lucide-react';
import Header from '@/components/Header';
import { cn } from '@/lib/utils';
import { toast } from '@/hooks/use-toast';
import { useAuth } from '@/contexts/AuthContext';

const FoodList = () => {
  const { user } = useAuth();
  const [foodItems, setFoodItems] = useState<FoodItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [activeCategory, setActiveCategory] = useState('all');

  const categories = [
    { id: 'all', label: 'Tất cả' },
    { id: 'breakfast', label: 'Bữa sáng' },
    { id: 'lunch', label: 'Bữa trưa' },
    { id: 'dinner', label: 'Bữa tối' },
    { id: 'snack', label: 'Ăn vặt' },
  ];

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

  const filteredItems = foodItems.filter(item => {
    // Determine category based on name or arbitrary logic if backend doesn't support 'mealType' on FoodItem
    // Since FoodItem interface doesn't have 'mealType' or 'category', we will use 'all' for now or mock it.
    // In a real app, we would add 'category' to FoodItem.
    // For now, let's just show all items unless searched.
    return true;
  });

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
          <h3 className="text-2xl font-bold text-gray-900">Danh sách món ăn</h3>
        </div>

        {/* Search Bar */}
        <div className="relative max-w-2xl mx-auto mb-8">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
          <Input
            placeholder="Tìm kiếm món ăn..."
            className="pl-12 h-14 bg-white/60 backdrop-blur-sm border-transparent hover:bg-white transition-colors rounded-2xl shadow-sm"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>

        {/* Categories */}
        <div className="flex justify-center gap-2 mb-8 flex-wrap">
          {categories.map((cat) => (
            <button
              key={cat.id}
              onClick={() => setActiveCategory(cat.id)}
              className={cn(
                "px-6 py-2 rounded-full font-bold text-sm transition-all border border-transparent",
                activeCategory === cat.id
                  ? "bg-[#65A30D] text-white shadow-lg shadow-green-200 scale-105"
                  : "bg-white text-gray-600 hover:bg-gray-50"
              )}
            >
              {cat.label}
            </button>
          ))}
        </div>

        {/* Results Grid */}
        {loading ? (
          <div className="text-center py-20">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-green-500 mx-auto"></div>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {filteredItems.map((item) => (
              <button
                type="button"
                key={item.foodItemId}
                className="w-full text-left group bg-white rounded-[2rem] p-4 pb-6 shadow-sm hover:shadow-xl transition-all duration-300 border border-transparent hover:border-green-100 cursor-pointer"
              >
                {/* Image Container */}
                <div className="relative aspect-[4/3] mb-4 overflow-hidden rounded-[1.5rem] bg-gray-100">
                  <img
                    src={item.imageUrl || "https://placehold.co/400x300?text=No+Image"}
                    alt={item.name}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                  />
                  <div className="absolute top-3 right-3 bg-white/90 p-1.5 rounded-full shadow-sm hover:bg-green-100 transition-colors">
                    <Plus className="w-5 h-5 text-gray-600" />
                  </div>
                </div>

                {/* Content */}
                <div className="px-1">
                  <h3 className="font-bold text-gray-900 text-lg mb-1 line-clamp-1">{item.name}</h3>
                  <p className="text-xs text-gray-500 mb-3">
                    {item.servingSize} {item.servingUnit}
                  </p>

                  <div className="font-bold text-lg text-gray-900">
                    {item.caloriesKcal.toFixed(0)} kcal
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}

        <div className="flex justify-center mt-12">
          <Button className="bg-[#EBE9C0] text-[#4A6F6F] hover:bg-[#EBE9C0]/80 rounded-full px-8 font-bold">
            Tải thêm
          </Button>
        </div>
      </main>
    </div>
  );
};

export default FoodList;
