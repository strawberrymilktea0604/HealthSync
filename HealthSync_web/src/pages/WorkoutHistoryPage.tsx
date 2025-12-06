import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { workoutService, WorkoutLog } from '@/services/workoutService';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Plus, Calendar, Clock, Dumbbell } from 'lucide-react';
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
      <div className="container mx-auto py-8 px-4">
        <div className="flex justify-center items-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 px-4 max-w-4xl">
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold mb-2">Lịch sử luyện tập</h1>
          <p className="text-muted-foreground">
            Tổng cộng {workoutLogs.length} buổi tập
          </p>
        </div>
        <Button onClick={() => navigate('/create-workout')} size="lg">
          <Plus className="mr-2 h-5 w-5" />
          Thêm buổi tập
        </Button>
      </div>

      {workoutLogs.length === 0 ? (
        <Card className="p-12">
          <div className="text-center space-y-4">
            <Dumbbell className="h-16 w-16 mx-auto text-muted-foreground" />
            <div>
              <h3 className="text-xl font-semibold mb-2">Chưa có buổi tập nào</h3>
              <p className="text-muted-foreground mb-6">
                Bắt đầu ghi lại hành trình luyện tập của bạn ngay hôm nay!
              </p>
              <Button onClick={() => navigate('/create-workout')} size="lg">
                <Plus className="mr-2 h-5 w-5" />
                Tạo buổi tập đầu tiên
              </Button>
            </div>
          </div>
        </Card>
      ) : (
        <div className="space-y-8">
          {Object.entries(groupedLogs).map(([date, logs]) => (
            <div key={date}>
              <h2 className="text-xl font-semibold mb-4 flex items-center gap-2">
                <Calendar className="h-5 w-5" />
                {formatDate(date)}
              </h2>
              <div className="space-y-4">
                {logs.map(log => (
                  <Card key={log.workoutLogId} className="hover:shadow-lg transition-shadow">
                    <CardHeader className="pb-3">
                      <div className="flex justify-between items-start">
                        <CardTitle className="text-lg flex items-center gap-2">
                          <Dumbbell className="h-5 w-5" />
                          Buổi tập
                        </CardTitle>
                        <div className="flex items-center gap-1 text-muted-foreground">
                          <Clock className="h-4 w-4" />
                          <span className="text-sm">{log.durationMin} phút</span>
                        </div>
                      </div>
                      {log.notes && (
                        <p className="text-sm text-muted-foreground mt-2">{log.notes}</p>
                      )}
                    </CardHeader>
                    <CardContent>
                      <div className="space-y-3">
                        {log.exerciseSessions.map(session => (
                          <div 
                            key={session.exerciseSessionId}
                            className="flex justify-between items-center p-3 bg-accent/50 rounded-lg"
                          >
                            <div>
                              <p className="font-medium">{session.exerciseName}</p>
                              <p className="text-sm text-muted-foreground">
                                {session.sets} sets × {session.reps} reps
                                {session.weightKg > 0 && ` @ ${session.weightKg}kg`}
                              </p>
                            </div>
                            {session.restSec && (
                              <span className="text-xs bg-background px-2 py-1 rounded">
                                Nghỉ: {session.restSec}s
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
  );
}
