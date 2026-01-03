import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { workoutService, WorkoutLog } from '@/services/workoutService';
import Header from '@/components/Header';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Plus, Calendar, Clock, Dumbbell, ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';

export default function WorkoutHistoryPage() {
  const navigate = useNavigate();
  const [workoutLogs, setWorkoutLogs] = useState<WorkoutLog[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadWorkoutLogs();
  }, []);

  const loadWorkoutLogs = async () => {
    try {
      setIsLoading(true);
      const data = await workoutService.getWorkoutLogs();
      setWorkoutLogs(data.sort((a, b) =>
        new Date(b.workoutDate).getTime() - new Date(a.workoutDate).getTime()
      ));
    } catch (error) {
      toast.error('Không thể tải lịch sử luyện tập');
      console.error(error);
    } finally {
      setIsLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const groupByDate = (logs: WorkoutLog[]) => {
    const grouped: { [key: string]: WorkoutLog[] } = {};
    logs.forEach(log => {
      const dateKey = log.workoutDate.split('T')[0];
      if (!grouped[dateKey]) {
        grouped[dateKey] = [];
      }
      grouped[dateKey].push(log);
    });
    return grouped;
  };

  const groupedLogs = groupByDate(workoutLogs);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-[#FDFBD4] flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#4A6F6F]"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#FDFBD4] font-sans selection:bg-[#EBE9C0] selection:text-black">
      <Header />

      <div className="max-w-4xl mx-auto py-8 px-4 md:px-8">
        <div className="flex justify-between items-center mb-8">
          <div className="flex items-center gap-2">
            <Button variant="ghost" className="rounded-full hover:bg-black/5" onClick={() => navigate(-1)}>
              <ArrowLeft className="w-5 h-5" />
            </Button>
            <div>
              <h1 className="text-3xl font-bold text-[#2d2d2d]">Lịch sử luyện tập</h1>
              <p className="text-gray-500 font-medium">
                Tổng cộng {workoutLogs.length} buổi tập
              </p>
            </div>
          </div>
          <Button
            onClick={() => navigate('/create-workout')}
            size="lg"
            className="rounded-full bg-[#2d2d2d] hover:bg-black text-[#FDFBD4] font-bold shadow-lg"
          >
            <Plus className="mr-2 h-5 w-5" />
            Thêm buổi tập
          </Button>
        </div>

        {workoutLogs.length === 0 ? (
          <Card className="p-12 bg-white/60 border-white/50 backdrop-blur-sm rounded-[2.5rem] text-center shadow-sm">
            <div className="flex flex-col items-center space-y-4">
              <div className="w-24 h-24 rounded-full bg-[#EBE9C0] flex items-center justify-center mb-2">
                <Dumbbell className="h-10 w-10 text-[#4A6F6F]" />
              </div>
              <div>
                <h3 className="text-xl font-bold text-[#2d2d2d] mb-2">Chưa có buổi tập nào</h3>
                <p className="text-gray-500 mb-6 max-w-sm mx-auto">
                  Bắt đầu ghi lại hành trình luyện tập của bạn ngay hôm nay để theo dõi tiến độ!
                </p>
                <Button
                  onClick={() => navigate('/create-workout')}
                  size="lg"
                  className="rounded-full bg-[#2d2d2d] hover:bg-black text-[#FDFBD4] font-bold"
                >
                  <Plus className="mr-2 h-5 w-5" />
                  Tạo buổi tập đầu tiên
                </Button>
              </div>
            </div>
          </Card>
        ) : (
          <div className="space-y-8 pb-12">
            {Object.entries(groupedLogs).map(([date, logs]) => (
              <div key={date}>
                <h2 className="text-lg font-bold text-gray-500 mb-4 flex items-center gap-2 uppercase tracking-wider ml-2">
                  <Calendar className="h-4 w-4" />
                  {formatDate(date)}
                </h2>
                <div className="space-y-4">
                  {logs.map(log => (
                    <Card key={log.workoutLogId} className="hover:shadow-md transition-all duration-300 bg-white/60 border-white/50 backdrop-blur-sm rounded-3xl overflow-hidden hover:-translate-y-1">
                      <CardHeader className="pb-3 bg-white/40 border-b border-black/5">
                        <div className="flex justify-between items-start">
                          <CardTitle className="text-lg flex items-center gap-2 font-bold text-[#2d2d2d]">
                            <div className="w-8 h-8 rounded-full bg-[#EBE9C0] flex items-center justify-center">
                              <Dumbbell className="h-4 w-4 text-[#4A6F6F]" />
                            </div>
                            Buổi tập
                          </CardTitle>
                          <div className="flex items-center gap-1 text-gray-500 bg-white/50 px-2 py-1 rounded-full text-xs font-semibold">
                            <Clock className="h-3 w-3" />
                            <span>{log.durationMin} phút</span>
                          </div>
                        </div>
                        {log.notes && (
                          <p className="text-sm text-gray-500 mt-2 ml-10 italic">"{log.notes}"</p>
                        )}
                      </CardHeader>
                      <CardContent className="pt-4">
                        <div className="space-y-3">
                          {log.exerciseSessions.map(session => (
                            <div
                              key={session.exerciseSessionId}
                              className="flex justify-between items-center p-3 bg-white/50 rounded-2xl hover:bg-white/80 transition-colors"
                            >
                              <div>
                                <p className="font-bold text-[#2d2d2d]">{session.exerciseName}</p>
                                <p className="text-xs text-gray-500 font-medium mt-1">
                                  <span className="bg-black/5 px-2 py-0.5 rounded-full">{session.sets} sets</span>
                                  <span className="mx-1">×</span>
                                  <span className="bg-black/5 px-2 py-0.5 rounded-full">{session.reps} reps</span>
                                  {session.weightKg > 0 && (
                                    <>
                                      <span className="mx-1">@</span>
                                      <span className="bg-[#EBE9C0] text-[#4A6F6F] px-2 py-0.5 rounded-full font-bold">{session.weightKg}kg</span>
                                    </>
                                  )}
                                </p>
                              </div>
                              {session.restSec && (
                                <span className="text-[10px] text-gray-400 bg-white px-2 py-1 rounded-full border border-black/5">
                                  Nghỉ {session.restSec}s
                                </span>
                              )}
                            </div>
                          ))}
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

