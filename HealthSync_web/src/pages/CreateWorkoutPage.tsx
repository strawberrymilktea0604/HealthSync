import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { workoutService, Exercise, ExerciseSession } from '@/services/workoutService';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Trash2, Plus, Search } from 'lucide-react';
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
  const [muscleGroupFilter, setMuscleGroupFilter] = useState('');
  const [difficultyFilter, setDifficultyFilter] = useState('');
  
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

    if (muscleGroupFilter) {
      filtered = filtered.filter(ex => ex.muscleGroup === muscleGroupFilter);
    }

    if (difficultyFilter) {
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
      navigate('/workout-history');
    } catch (error) {
      toast.error('Không thể lưu nhật ký luyện tập');
      console.error(error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="container mx-auto py-8 px-4 max-w-7xl">
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Ghi nhật ký luyện tập</h1>
        <p className="text-muted-foreground">Thêm buổi tập mới vào lịch sử của bạn</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Left Column - Workout Details */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Thông tin buổi tập</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <Label htmlFor="workoutDate">Ngày tập</Label>
                <Input
                  id="workoutDate"
                  type="date"
                  value={workoutDate}
                  onChange={(e) => setWorkoutDate(e.target.value)}
                />
              </div>

              <div>
                <Label htmlFor="duration">Thời gian (phút)</Label>
                <Input
                  id="duration"
                  type="number"
                  value={durationMin}
                  onChange={(e) => setDurationMin(parseInt(e.target.value) || 0)}
                  min="1"
                />
              </div>

              <div>
                <Label htmlFor="notes">Ghi chú</Label>
                <Textarea
                  id="notes"
                  placeholder="Cảm nhận về buổi tập..."
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  rows={3}
                />
              </div>
            </CardContent>
          </Card>

          {/* Selected Exercises */}
          <Card>
            <CardHeader>
              <CardTitle>Các bài tập đã chọn ({selectedExercises.length})</CardTitle>
            </CardHeader>
            <CardContent>
              {selectedExercises.length === 0 ? (
                <p className="text-muted-foreground text-center py-8">
                  Chưa có bài tập nào. Chọn bài tập từ thư viện bên phải.
                </p>
              ) : (
                <div className="space-y-4">
                  {selectedExercises.map((session) => {
                    const exercise = exercises.find(ex => ex.exerciseId === session.exerciseId);
                    return (
                      <Card key={session.exerciseId} className="p-4">
                        <div className="flex justify-between items-start mb-3">
                          <div>
                            <h3 className="font-semibold">{exercise?.name}</h3>
                            <p className="text-sm text-muted-foreground">{exercise?.muscleGroup}</p>
                          </div>
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => removeExercise(session.exerciseId)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                        
                        <div className="grid grid-cols-3 gap-3">
                          <div>
                            <Label className="text-xs">Sets</Label>
                            <Input
                              type="number"
                              value={session.sets}
                              onChange={(e) => updateExerciseSession(session.exerciseId, 'sets', parseInt(e.target.value) || 0)}
                              min="1"
                              className="h-9"
                            />
                          </div>
                          <div>
                            <Label className="text-xs">Reps</Label>
                            <Input
                              type="number"
                              value={session.reps}
                              onChange={(e) => updateExerciseSession(session.exerciseId, 'reps', parseInt(e.target.value) || 0)}
                              min="1"
                              className="h-9"
                            />
                          </div>
                          <div>
                            <Label className="text-xs">Tạ (kg)</Label>
                            <Input
                              type="number"
                              value={session.weightKg}
                              onChange={(e) => updateExerciseSession(session.exerciseId, 'weightKg', Number.parseFloat(e.target.value) || 0)}
                              min="0"
                              step="0.5"
                              className="h-9"
                            />
                          </div>
                        </div>
                      </Card>
                    );
                  })}
                </div>
              )}

              <div className="mt-6 pt-4 border-t space-y-3">
                <Button
                  className="w-full"
                  onClick={handleSubmit}
                  disabled={isLoading || selectedExercises.length === 0}
                >
                  {isLoading ? 'Đang lưu...' : 'Lưu buổi tập'}
                </Button>
                <Button
                  variant="outline"
                  className="w-full"
                  onClick={() => navigate('/workout-history')}
                >
                  Hủy
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Right Column - Exercise Library */}
        <Card>
          <CardHeader>
            <CardTitle>Thư viện bài tập</CardTitle>
            <CardDescription>Tìm và thêm bài tập vào buổi tập của bạn</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {/* Search */}
              <div className="relative">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Tìm kiếm bài tập..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-9"
                />
              </div>

              {/* Filters */}
              <div className="grid grid-cols-2 gap-3">
                <Select value={muscleGroupFilter} onValueChange={setMuscleGroupFilter}>
                  <SelectTrigger>
                    <SelectValue placeholder="Nhóm cơ" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="">Tất cả</SelectItem>
                    {muscleGroups.map(group => (
                      <SelectItem key={group} value={group}>{group}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>

                <Select value={difficultyFilter} onValueChange={setDifficultyFilter}>
                  <SelectTrigger>
                    <SelectValue placeholder="Độ khó" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="">Tất cả</SelectItem>
                    {difficulties.map(diff => (
                      <SelectItem key={diff} value={diff}>{diff}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Exercise List */}
              <div className="space-y-2 max-h-[600px] overflow-y-auto">
                {filteredExercises.map(exercise => (
                  <Card key={exercise.exerciseId} className="p-4 hover:bg-accent transition-colors">
                    <div className="flex justify-between items-start">
                      <div className="flex-1">
                        <h4 className="font-medium">{exercise.name}</h4>
                        <div className="flex gap-2 mt-1">
                          <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded">
                            {exercise.muscleGroup}
                          </span>
                          <span className="text-xs bg-secondary text-secondary-foreground px-2 py-1 rounded">
                            {exercise.difficulty}
                          </span>
                        </div>
                        {exercise.description && (
                          <p className="text-sm text-muted-foreground mt-2 line-clamp-2">
                            {exercise.description}
                          </p>
                        )}
                      </div>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => addExercise(exercise)}
                        className="ml-3"
                      >
                        <Plus className="h-4 w-4" />
                      </Button>
                    </div>
                  </Card>
                ))}
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
