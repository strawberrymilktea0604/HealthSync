import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { exerciseService, Exercise } from '@/services/exerciseService';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Card, CardContent } from '@/components/ui/card';
import { Search, Filter, Clock, Flame, ChevronLeft, ChevronRight, LayoutGrid, List } from 'lucide-react';
import Header from '@/components/Header';
import { toast } from 'sonner';
import { useAuth } from '@/contexts/AuthContext';

const itemsPerPage = 6;

export default function ExerciseLibraryPage() {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [exercises, setExercises] = useState<Exercise[]>([]);
    const [filteredExercises, setFilteredExercises] = useState<Exercise[]>([]);
    const [loading, setLoading] = useState(true);

    // Filters
    const [search, setSearch] = useState('');
    const [muscleGroup, setMuscleGroup] = useState('all');
    const [difficulty, setDifficulty] = useState('all');

    // Pagination
    const [currentPage, setCurrentPage] = useState(1);

    useEffect(() => {
        loadExercises();
    }, []);

    useEffect(() => {
        filterExercises();
    }, [search, muscleGroup, difficulty, exercises]);

    const loadExercises = async () => {
        try {
            const data = await exerciseService.getExercises();
            setExercises(data);
            setFilteredExercises(data);
        } catch (error) {
            console.error(error);
            toast.error('Không thể tải danh sách bài tập');
        } finally {
            setLoading(false);
        }
    };

    const filterExercises = () => {
        let result = exercises;

        if (search) {
            result = result.filter(ex =>
                ex.name.toLowerCase().includes(search.toLowerCase()) ||
                ex.description?.toLowerCase().includes(search.toLowerCase())
            );
        }

        if (muscleGroup !== 'all') {
            result = result.filter(ex => ex.muscleGroup === muscleGroup);
        }

        if (difficulty !== 'all') {
            result = result.filter(ex => ex.difficulty === difficulty);
        }

        setFilteredExercises(result);
        setCurrentPage(1); // Reset to first page on filter change
    };

    // Pagination Logic
    const totalPages = Math.ceil(filteredExercises.length / itemsPerPage);
    const currentExercises = filteredExercises.slice(
        (currentPage - 1) * itemsPerPage,
        currentPage * itemsPerPage
    );

    // Featured Exercises (Mock or filtered from list)
    const featuredNames = ['Squat', 'Push-up', 'Plank', 'Deadlift'];
    const featuredExercises = exercises.filter(ex => featuredNames.some(n => ex.name.toLowerCase().includes(n.toLowerCase()))).slice(0, 4);

    // If no specific featured found, take random 4
    const displayFeatured = featuredExercises.length > 0 ? featuredExercises : exercises.slice(0, 4);

    // Get distinct muscle groups for filter
    const muscleGroups = Array.from(new Set(exercises.map(ex => ex.muscleGroup))).filter(Boolean);

    return (
        <div className="min-h-screen bg-[#EBE9C0] font-sans selection:bg-[#FDFBD4] selection:text-black">
            <Header />

            <main className="max-w-7xl mx-auto px-4 md:px-8 pb-12 pt-4">
                {/* Header Titles */}
                <div className="text-center mb-8">
                    <div className="flex items-center justify-center gap-2 mb-1">
                        {user && <span className="text-xs font-bold text-gray-500 uppercase tracking-widest">{user.fullName}</span>}
                    </div>
                    <h4 className="text-sm font-bold text-gray-600 mb-1">Your Activities</h4>
                    <p className="text-xs text-gray-500 mb-4">Exercise for you!</p>

                    <h1 className="text-4xl font-black text-[#2d2d2d] mb-2 uppercase tracking-tight">Thư viện bài tập</h1>
                    <p className="text-gray-600 font-medium">Khám phá và lựa chọn các bài tập phù hợp với mục tiêu của bạn</p>
                </div>

                {/* Main Search & Featured Card */}
                <Card className="bg-[#FFFDF7] border-none shadow-xl rounded-[2.5rem] p-6 md:p-10 mb-12 relative overflow-hidden">
                    {/* Search Bar Row */}
                    <div className="flex flex-col md:flex-row gap-4 mb-8">
                        <div className="relative flex-1">
                            <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
                            <Input
                                placeholder="Tìm kiếm bài tập..."
                                className="pl-12 h-14 bg-gray-50 border-transparent hover:bg-gray-100 transition-colors rounded-2xl text-lg"
                                value={search}
                                onChange={(e) => setSearch(e.target.value)}
                            />
                        </div>
                        <div className="w-full md:w-64">
                            <Select value={muscleGroup} onValueChange={setMuscleGroup}>
                                <SelectTrigger className="h-14 bg-gray-50 border-transparent hover:bg-gray-100 rounded-2xl px-4 font-medium text-gray-600">
                                    <SelectValue placeholder="Tất cả nhóm cơ" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">Tất cả nhóm cơ</SelectItem>
                                    {muscleGroups.map(group => (
                                        <SelectItem key={group} value={group}>{group}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>
                        <Button variant="ghost" className="h-14 w-14 rounded-2xl bg-gray-50 hover:bg-gray-100 text-gray-600">
                            <Filter className="w-6 h-6" />
                        </Button>
                    </div>

                    {/* Featured Grid */}
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                        {displayFeatured.map((ex, index) => {
                            // Mocking colors and icons for aesthetic match
                            const colors = ['bg-[#4A6F6F]', 'bg-[#A8D5BA]', 'bg-[#4FB3D9]', 'bg-[#3B5998]'];
                            const color = colors[index % colors.length];

                            return (
                                <div key={ex.exerciseId} className="bg-white rounded-3xl p-4 shadow-sm border border-gray-100 flex flex-col h-full hover:shadow-md transition-shadow">
                                    <div className={`${color} h-32 rounded-2xl mb-4 flex items-center justify-center relative overflow-hidden group`}>
                                        {ex.imageUrl ? (
                                            <img src={ex.imageUrl} alt={ex.name} className="w-full h-full object-cover opacity-90 group-hover:scale-105 transition-transform" />
                                        ) : (
                                            // Fallback icon based on index or name
                                            <Flame className="w-12 h-12 text-white opacity-80" />
                                        )}
                                    </div>

                                    <h3 className="font-bold text-lg text-gray-900 mb-1">{ex.name}</h3>
                                    <p className="text-xs text-gray-500 mb-3">Nhóm cơ: {ex.muscleGroup}</p>

                                    <div className="flex gap-2 mb-4 text-xs flex-wrap">
                                        <span className="bg-gray-100 text-gray-600 px-2 py-1 rounded-md font-bold uppercase">
                                            {ex.difficulty}
                                        </span>
                                        {ex.equipment && ex.equipment !== 'None' && (
                                            <span className="bg-gray-100 text-gray-600 px-2 py-1 rounded-md font-medium">
                                                {ex.equipment}
                                            </span>
                                        )}
                                    </div>

                                    <Button className="w-full bg-[#5FB25F] hover:bg-[#4E9E4E] text-white rounded-xl font-bold mt-auto">
                                        Xem chi tiết
                                    </Button>
                                </div>
                            );
                        })}
                    </div>

                    {/* Pagination for featured? No, usually featured is static or random. Let's keep it simple. */}
                    <div className="flex justify-center mt-6 gap-2">
                        <span className="text-sm font-medium text-gray-500">Trước</span>
                        <div className="flex gap-1">
                            <span className="w-6 h-6 flex items-center justify-center bg-[#5FB25F] text-white rounded text-xs font-bold">1</span>
                            <span className="w-6 h-6 flex items-center justify-center text-gray-400 rounded text-xs font-bold">2</span>
                            <span className="w-6 h-6 flex items-center justify-center text-gray-400 rounded text-xs font-bold">3</span>
                        </div>
                        <span className="text-sm font-medium text-gray-500">Sau</span>
                    </div>
                </Card>

                {/* Results Section */}
                <div className="flex justify-between items-center mb-6 px-2">
                    <h2 className="font-bold text-gray-700">Kết quả tìm kiếm ({filteredExercises.length})</h2>
                    <div className="flex gap-2 text-gray-400">
                        <LayoutGrid className="w-5 h-5 cursor-pointer hover:text-gray-600" />
                        <List className="w-5 h-5 cursor-pointer hover:text-gray-600" />
                    </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {currentExercises.map((ex) => (
                        <div key={ex.exerciseId} className="bg-white rounded-3xl p-3 shadow-sm hover:shadow-lg transition-all flex gap-4 h-40 overflow-hidden cursor-pointer border border-transparent hover:border-green-100">
                            <div className="w-1/3 h-full rounded-2xl bg-gray-100 overflow-hidden relative shrink-0">
                                {ex.imageUrl ? (
                                    <img src={ex.imageUrl} alt={ex.name} className="w-full h-full object-cover" />
                                ) : (
                                    <div className="w-full h-full flex items-center justify-center bg-gray-200">
                                        <Dumbbell className="w-8 h-8 text-gray-400" />
                                    </div>
                                )}
                            </div>
                            <div className="flex-1 flex flex-col justify-between py-1">
                                <div>
                                    <h3 className="font-bold text-gray-900 border-b border-gray-100 pb-2 mb-2">{ex.name}</h3>
                                    <p className="text-xs text-gray-500 line-clamp-2">{ex.description || 'Bài tập phát triển sức mạnh và cơ bắp.'}</p>
                                </div>

                                <div className="flex justify-between items-center mt-2">
                                    <span className="text-[10px] bg-gray-100 text-gray-600 px-2 py-1 rounded-md font-bold uppercase">
                                        {ex.muscleGroup}
                                    </span>
                                    <span className="text-[10px] bg-gray-100 text-gray-600 px-2 py-1 rounded-md font-bold">
                                        {ex.difficulty}
                                    </span>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Pagination Bottom */}
                {totalPages > 1 && (
                    <div className="flex justify-center mt-12 gap-2">
                        <Button
                            variant="ghost"
                            disabled={currentPage === 1}
                            onClick={() => setCurrentPage(c => Math.max(1, c - 1))}
                            className="text-gray-500"
                        >
                            <ChevronLeft className="w-4 h-4 mr-1" /> Trước
                        </Button>

                        {Array.from({ length: totalPages }).map((_, i) => (
                            <button
                                key={i}
                                onClick={() => setCurrentPage(i + 1)}
                                className={`w-8 h-8 rounded-lg text-sm font-bold transition-all ${currentPage === i + 1
                                    ? 'bg-[#5FB25F] text-white shadow-lg shadow-green-200'
                                    : 'text-gray-400 hover:bg-white'
                                    }`}
                            >
                                {i + 1}
                            </button>
                        ))}

                        <Button
                            variant="ghost"
                            disabled={currentPage === totalPages}
                            onClick={() => setCurrentPage(c => Math.min(totalPages, c + 1))}
                            className="text-gray-500"
                        >
                            Sau <ChevronRight className="w-4 h-4 ml-1" />
                        </Button>
                    </div>
                )}
            </main>
        </div>
    );
}

// Helper component for fallback icon
function Dumbbell({ className }: { className?: string }) {
    return (
        <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            className={className}
        >
            <path d="M6.5 6.5h11" />
            <path d="M6.5 17.5h11" />
            <path d="M6.5 6.5v11" />
            <path d="M17.5 6.5v11" />
            <path d="M2 12h20" />
        </svg>
    )
}
