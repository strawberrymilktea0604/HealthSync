import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { workoutService, Exercise, ExerciseSession } from '@/services/workoutService';
import Header from '@/components/Header';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Trash2, Plus, Search, Dumbbell, ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';

const muscleGroups = ['Chest', 'Back', 'Shoulders', 'Arms', 'Legs', 'Core', 'Full Body'];
const difficulties = ['Beginner', 'Intermediate', 'Advanced'];

export default function CreateWorkoutPage() {
  const navigate = useNavigate();
  const [workoutDate, setWorkoutDate] = useState(new Date().toISOString().split('T')[0]);
  const [durationMin, setDurationMin] = useState(60);
  const [notes, setNotes] = useState('');
  const [selectedExercises, setSelectedExercises] = useState<ExerciseSession[]>([]);

  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [filteredExercises, setFilteredExercises] = useState<Exercise[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [muscleGroupFilter, setMuscleGroupFilter] = useState('all');
  const [difficultyFilter, setDifficultyFilter] = useState('all');

  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    loadExercises();
  }, []);

  useEffect(() => {
    filterExercises();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchTerm, muscleGroupFilter, difficultyFilter, exercises]);

  const loadExercises = async () => {
    try {
      const data = await workoutService.getExercises();
      setExercises(data);
      setFilteredExercises(data);
    } catch (error) {
      toast.error('Không thể tải danh sách bài tập');
      console.error(error);
    }
  };

  const filterExercises = () => {
    let filtered = exercises;

    if (searchTerm) {
      filtered = filtered.filter(ex =>
        ex.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        ex.description?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    if (muscleGroupFilter && muscleGroupFilter !== 'all') {
      filtered = filtered.filter(ex => ex.muscleGroup === muscleGroupFilter);
    }

    if (difficultyFilter && difficultyFilter !== 'all') {
      filtered = filtered.filter(ex => ex.difficulty === difficultyFilter);
    }

    setFilteredExercises(filtered);
  };

  const addExercise = (exercise: Exercise) => {
    const existing = selectedExercises.find(ex => ex.exerciseId === exercise.exerciseId);
    if (existing) {
      toast.warning('Bài tập này đã được thêm');
      return;
    }

    setSelectedExercises([
      ...selectedExercises,
      {
        exerciseId: exercise.exerciseId,
        sets: 3,
        reps: 10,
        weightKg: 0,
        restSec: 60
      }
    ]);
  };

  const removeExercise = (exerciseId: number) => {
    setSelectedExercises(selectedExercises.filter(ex => ex.exerciseId !== exerciseId));
  };

  const updateExerciseSession = (exerciseId: number, field: keyof ExerciseSession, value: number) => {
    setSelectedExercises(selectedExercises.map(ex =>
      ex.exerciseId === exerciseId ? { ...ex, [field]: value } : ex
    ));
  };

  const handleSubmit = async () => {
    if (selectedExercises.length === 0) {
      toast.error('Vui lòng thêm ít nhất một bài tập');
      return;
    }

    setIsLoading(true);
    try {
      await workoutService.createWorkoutLog({
        workoutDate,
        durationMin,
        notes,
        exerciseSessions: selectedExercises
      });

      toast.success('Đã lưu nhật ký luyện tập!');
      navigate('/workout-history'); // Or redirect to dashboard/history
    } catch (error) {
      toast.error('Không thể lưu nhật ký luyện tập');
      console.error(error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <div className="max-w-7xl mx-auto py-8 px-4 md:px-8">
        <div className="flex items-center gap-2 mb-6">
          <Button variant="ghost" className="rounded-full hover:bg-black/5" onClick={() => navigate(-1)}>
            <ArrowLeft className="w-5 h-5" />
          </Button>
          <h1 className="text-xl font-bold text-gray-800 uppercase tracking-wide">Tạo buổi tập</h1>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Left Column - Workout Details & Selected */}
          <div className="space-y-6">
            <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm shadow-sm rounded-3xl overflow-hidden">
              <CardHeader className="bg-white/30">
                <CardTitle className="flex items-center gap-2">
                  <Dumbbell className="w-5 h-5 text-[#4A6F6F]" />
                  Thông tin buổi tập
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4 pt-4">
                <div>
                  <Label htmlFor="workoutDate" className="text-gray-600">Ngày tập</Label>
                  <Input
                    id="workoutDate"
                    type="date"
                    value={workoutDate}
                    onChange={(e) => setWorkoutDate(e.target.value)}
                    className="bg-white/60 border-black/10 focus:ring-[#4A6F6F]"
                  />
                </div>

                <div>
                  <Label htmlFor="duration" className="text-gray-600">Thời gian (phút)</Label>
                  <Input
                    id="duration"
                    type="number"
                    value={durationMin}
                    onChange={(e) => setDurationMin(Number.parseInt(e.target.value) || 0)}
                    min="1"
                    className="bg-white/60 border-black/10 focus:ring-[#4A6F6F]"
                  />
                </div>

                <div>
                  <Label htmlFor="notes" className="text-gray-600">Ghi chú</Label>
                  <Textarea
                    id="notes"
                    placeholder="Cảm nhận về buổi tập..."
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    rows={3}
                    className="bg-white/60 border-black/10 focus:ring-[#4A6F6F]"
                  />
                </div>
              </CardContent>
            </Card>

            {/* Selected Exercises */}
            <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm shadow-sm rounded-3xl overflow-hidden">
              <CardHeader className="bg-white/30">
                <CardTitle>Các bài tập đã chọn ({selectedExercises.length})</CardTitle>
                <CardDescription>Danh sách bài tập trong buổi này</CardDescription>
              </CardHeader>
              <CardContent className="pt-4">
                {selectedExercises.length === 0 ? (
                  <p className="text-center py-8 text-gray-400 italic">
                    Chưa có bài tập nào. Chọn bài tập từ thư viện bên phải.
                  </p>
                ) : (
                  <div className="space-y-4">
                    {selectedExercises.map((session) => {
                      const exercise = exercises.find(ex => ex.exerciseId === session.exerciseId);
                      return (
                        <div key={session.exerciseId} className="bg-white/60 rounded-2xl p-4 border border-white/50 shadow-sm">
                          <div className="flex justify-between items-start mb-3">
                            <div>
                              <h3 className="font-bold text-[#2d2d2d]">{exercise?.name}</h3>
                              <p className="text-xs text-gray-500 uppercase tracking-wide">{exercise?.muscleGroup}</p>
                            </div>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => removeExercise(session.exerciseId)}
                              className="text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-full"
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>

                          <div className="grid grid-cols-3 gap-3">
                            <div className="space-y-1">
                              <Label className="text-[10px] uppercase text-gray-400 font-bold">Sets</Label>
                              <Input
                                type="number"
                                value={session.sets}
                                onChange={(e) => updateExerciseSession(session.exerciseId, 'sets', Number.parseInt(e.target.value) || 0)}
                                min="1"
                                className="h-8 bg-white/50 text-center font-medium"
                              />
                            </div>
                            <div className="space-y-1">
                              <Label className="text-[10px] uppercase text-gray-400 font-bold">Reps</Label>
                              <Input
                                type="number"
                                value={session.reps}
                                onChange={(e) => updateExerciseSession(session.exerciseId, 'reps', Number.parseInt(e.target.value) || 0)}
                                min="1"
                                className="h-8 bg-white/50 text-center font-medium"
                              />
                            </div>
                            <div className="space-y-1">
                              <Label className="text-[10px] uppercase text-gray-400 font-bold">Kg</Label>
                              <Input
                                type="number"
                                value={session.weightKg}
                                onChange={(e) => updateExerciseSession(session.exerciseId, 'weightKg', Number.parseFloat(e.target.value) || 0)}
                                min="0"
                                step="0.5"
                                className="h-8 bg-white/50 text-center font-medium"
                              />
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}

                <div className="mt-8 pt-4 border-t border-black/5 space-y-3">
                  <Button
                    className="w-full bg-[#2d2d2d] hover:bg-black text-[#FDFBD4] rounded-xl font-bold py-6 text-lg shadow-lg"
                    onClick={handleSubmit}
                    disabled={isLoading || selectedExercises.length === 0}
                  >
                    {isLoading ? 'Đang lưu...' : 'Hoàn tất buổi tập'}
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Right Column - Exercise Library */}
          <Card className="bg-[#FFFFE0]/80 border-white/50 backdrop-blur-sm shadow-sm rounded-3xl overflow-hidden flex flex-col h-[calc(100vh-140px)] sticky top-24">
            <CardHeader className="bg-white/30 shrink-0">
              <CardTitle>Thư viện bài tập</CardTitle>
              <CardDescription>Tìm và thêm bài tập</CardDescription>
            </CardHeader>
            <CardContent className="flex flex-col flex-1 overflow-hidden pt-4">
              <div className="space-y-4 mb-4 shrink-0">
                {/* Search */}
                <div className="relative">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                  <Input
                    placeholder="Tìm kiếm bài tập..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="pl-9 bg-white/60 border-black/10 rounded-xl"
                  />
                </div>

                {/* Filters */}
                <div className="grid grid-cols-2 gap-3">
                  <Select value={muscleGroupFilter} onValueChange={setMuscleGroupFilter}>
                    <SelectTrigger className="bg-white/60 border-black/10 rounded-xl">
                      <SelectValue placeholder="Nhóm cơ" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">Tất cả</SelectItem>
                      {muscleGroups.map(group => (
                        <SelectItem key={group} value={group}>{group}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>

                  <Select value={difficultyFilter} onValueChange={setDifficultyFilter}>
                    <SelectTrigger className="bg-white/60 border-black/10 rounded-xl">
                      <SelectValue placeholder="Độ khó" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">Tất cả</SelectItem>
                      {difficulties.map(diff => (
                        <SelectItem key={diff} value={diff}>{diff}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              {/* Exercise List */}
              <div className="space-y-2 overflow-y-auto pr-1 flex-1 custom-scrollbar">
                {filteredExercises.map(exercise => (
                  <div key={exercise.exerciseId} className="group bg-white/40 hover:bg-white/80 transition-all rounded-2xl p-3 border border-transparent hover:border-black/5 flex justify-between items-center cursor-pointer" onClick={() => addExercise(exercise)}>
                    <div className="flex-1">
                      <h4 className="font-bold text-[#2d2d2d] text-sm">{exercise.name}</h4>
                      <div className="flex gap-2 mt-1">
                        <span className="text-[10px] bg-[#4A6F6F]/10 text-[#4A6F6F] px-2 py-0.5 rounded-full font-bold uppercase">
                          {exercise.muscleGroup}
                        </span>
                        <span className="text-[10px] bg-yellow-500/10 text-yellow-700 px-2 py-0.5 rounded-full font-bold uppercase">
                          {exercise.difficulty}
                        </span>
                      </div>
                    </div>
                    <Button
                      size="sm"
                      variant="ghost"
                      className="ml-2 rounded-full h-8 w-8 p-0 bg-[#2d2d2d] text-white opacity-0 group-hover:opacity-100 transition-opacity"
                    >
                      <Plus className="h-4 w-4" />
                    </Button>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

