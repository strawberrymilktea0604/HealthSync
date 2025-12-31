import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import AdminLayout from "@/components/admin/AdminLayout";
import { Card } from 'primereact/card';
import { Chart } from 'primereact/chart';
import { adminStatisticsService } from '@/services/adminStatisticsService';
import { AdminStatistics } from '@/types/adminStatistics';
import { toast } from 'sonner';
import 'primeflex/primeflex.css';

export default function AdminDashboard() {
  const [statistics, setStatistics] = useState<AdminStatistics | null>(null);
  const [loading, setLoading] = useState(true);
  const [timeRange] = useState<number>(365);
  const [exerciseSearch, setExerciseSearch] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    fetchStatistics();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeRange]);

  const fetchStatistics = async () => {
    try {
      setLoading(true);
      console.log('Fetching statistics...');
      const data = await adminStatisticsService.getStatistics(timeRange);
      console.log('Statistics data received:', data);
      console.log('Top exercises:', data.workoutStatistics?.topExercises);
      setStatistics(data);
    } catch (error: unknown) {
      console.error('Error fetching statistics:', error);
      const errorResponse = error && typeof error === 'object' && 'response' in error ? (error as { response?: { status?: number } }).response : undefined;
      if (errorResponse?.status === 401 || errorResponse?.status === 403) {
        toast.error('Bạn không có quyền truy cập trang này');
        navigate('/login');
      } else {
        toast.error('Không thể tải dữ liệu thống kê');
      }
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <AdminLayout>
        <div className="flex align-items-center justify-content-center" style={{ minHeight: '80vh' }}>
          <i className="pi pi-spin pi-spinner" style={{ fontSize: '3rem', color: '#4A6C6F' }}></i>
        </div>
      </AdminLayout>
    );
  }

  if (!statistics) {
    return (
      <AdminLayout>
        <div className="flex align-items-center justify-content-center" style={{ minHeight: '80vh' }}>
          <p className="text-500">Không có dữ liệu thống kê</p>
        </div>
      </AdminLayout>
    );
  }

  // Ensure statistics has the required structure and valid data
  if (!statistics.userStatistics || !statistics.workoutStatistics || !statistics.nutritionStatistics || !statistics.goalStatistics) {
    return (
      <AdminLayout>
        <div className="flex align-items-center justify-content-center" style={{ minHeight: '80vh' }}>
          <p className="text-500">Dữ liệu thống kê không đầy đủ</p>
        </div>
      </AdminLayout>
    );
  }

  // Check for required numeric fields
  if (typeof statistics.userStatistics.totalUsers !== 'number' ||
    typeof statistics.userStatistics.activeUsers !== 'number' ||
    typeof statistics.userStatistics.newUsersThisMonth !== 'number' ||
    typeof statistics.workoutStatistics.totalWorkoutLogs !== 'number') {
    console.error('Statistics data contains invalid numeric values:', statistics);
    return (
      <AdminLayout>
        <div className="flex align-items-center justify-content-center" style={{ minHeight: '80vh' }}>
          <p className="text-500">Dữ liệu thống kê không hợp lệ</p>
        </div>
      </AdminLayout>
    );
  }

  const { userStatistics, workoutStatistics } = statistics;

  // User Role Distribution Chart
  const userRoleData = {
    labels: userStatistics.userRoleDistribution?.map(r => r.role) || [],
    datasets: [{
      data: userStatistics.userRoleDistribution?.map(r => r.count) || [],
      backgroundColor: ['#4A6C6F', '#6B8E23', '#8B7355'],
    }]
  };

  const userRoleOptions = {
    plugins: {
      legend: {
        position: 'bottom' as const
      }
    }
  };

  return (
    <AdminLayout>
      <div className="p-4">
        {/* Stats Cards Grid */}
        <div className="grid">
          <div className="col-12 md:col-6 lg:col-3">
            <Card className="border-round-xl shadow-2">
              <div className="flex align-items-center justify-content-between mb-3">
                <div className="text-500 font-medium text-sm">Total Users</div>
                <div className="w-3rem h-3rem border-circle bg-primary flex align-items-center justify-content-center">
                  <i className="pi pi-users text-white text-xl"></i>
                </div>
              </div>
              <div className="text-900 font-bold text-4xl mb-2">{(userStatistics.totalUsers || 0).toLocaleString()}</div>
              <div className="text-500 text-sm font-medium">
                Tổng số người dùng trong hệ thống
              </div>
            </Card>
          </div>

          <div className="col-12 md:col-6 lg:col-3">
            <Card className="border-round-xl shadow-2">
              <div className="flex align-items-center justify-content-between mb-3">
                <div className="text-500 font-medium text-sm">New Users</div>
                <div className="w-3rem h-3rem border-circle bg-primary flex align-items-center justify-content-center">
                  <i className="pi pi-user-plus text-white text-xl"></i>
                </div>
              </div>
              <div className="text-900 font-bold text-4xl mb-2">{userStatistics.newUsersThisMonth || 0}</div>
              <div className="text-500 text-sm font-medium">
                Người dùng mới tháng này
              </div>
            </Card>
          </div>

          <div className="col-12 md:col-6 lg:col-3">
            <Card className="border-round-xl shadow-2">
              <div className="flex align-items-center justify-content-between mb-3">
                <div className="text-500 font-medium text-sm">Active Users</div>
                <div className="w-3rem h-3rem border-circle bg-primary flex align-items-center justify-content-center">
                  <i className="pi pi-chart-line text-white text-xl"></i>
                </div>
              </div>
              <div className="text-900 font-bold text-4xl mb-2">{(userStatistics.activeUsers || 0).toLocaleString()}</div>
              <div className="text-500 text-sm font-medium">
                Người dùng hoạt động
              </div>
            </Card>
          </div>

          <div className="col-12 md:col-6 lg:col-3">
            <Card className="border-round-xl shadow-2">
              <div className="flex align-items-center justify-content-between mb-3">
                <div className="text-500 font-medium text-sm">Workout Logs</div>
                <div className="w-3rem h-3rem border-circle bg-primary flex align-items-center justify-content-center">
                  <i className="pi pi-chart-bar text-white text-xl"></i>
                </div>
              </div>
              <div className="text-900 font-bold text-4xl mb-2">{(workoutStatistics.totalWorkoutLogs || 0).toLocaleString()}</div>
              <div className="text-500 text-sm font-medium">
                Tổng số lượt tập luyện
              </div>
            </Card>
          </div>
        </div>

        {/* Charts Grid */}
        <div className="grid mt-4">
          <div className="col-12 lg:col-6">
            <Card className="border-round-xl shadow-2">
              <h3 className="text-xl font-semibold mb-4 text-900">Phân bổ vai trò người dùng</h3>
              {userStatistics.userRoleDistribution && userStatistics.userRoleDistribution.length > 0 ? (
                <Chart type="pie" data={userRoleData} options={userRoleOptions} style={{ height: '300px' }} />
              ) : (
                <p className="text-500 text-center">Chưa có dữ liệu phân bổ vai trò</p>
              )}
            </Card>
          </div>

          <div className="col-12 lg:col-6">
            <Card className="border-round-xl shadow-2">
              <h3 className="text-xl font-semibold mb-4 text-900">Most Popular Exercises</h3>
              <div className="mb-3">
                <input
                  type="text"
                  placeholder="Search exercises..."
                  value={exerciseSearch}
                  onChange={(e) => setExerciseSearch(e.target.value)}
                  className="w-full p-2 border-1 border-200 border-round"
                />
              </div>
              <div className="flex flex-column gap-3">
                {(workoutStatistics.topExercises || [])
                  .filter(exercise => exercise && typeof exercise.exerciseName === 'string' && exercise.exerciseName.trim() !== '' && exercise.exerciseName.toLowerCase().includes(exerciseSearch.toLowerCase()))
                  .slice(0, 5)
                  .map((exercise) => (
                    <div key={exercise.exerciseId} className="flex align-items-center gap-3">
                      <div className="w-1rem h-1rem border-circle" style={{ backgroundColor: '#4A6C6F' }}></div>
                      <span className="font-medium text-900">{exercise.exerciseName || 'Unknown Exercise'}</span>
                      <div className="flex-1 mx-3">
                        <div className="w-full h-0-5rem bg-gray-200 border-round">
                          <div
                            className="h-full bg-primary border-round"
                            style={{
                              width: `${(() => {
                                const filteredExercises = (workoutStatistics.topExercises || []).filter(e => e && typeof e.exerciseName === 'string' && e.exerciseName.trim() !== '' && e.exerciseName.toLowerCase().includes(exerciseSearch.toLowerCase()));
                                const maxUsage = filteredExercises.length > 0 ? Math.max(...filteredExercises.map(e => e.usageCount)) : 1;
                                return (exercise.usageCount / maxUsage) * 100;
                              })()}%`
                            }}
                          ></div>
                        </div>
                      </div>
                      <span className="text-500 text-sm">{exercise.usageCount.toLocaleString()}</span>
                    </div>
                  ))}
                {(workoutStatistics.topExercises || []).filter(exercise => exercise && typeof exercise.exerciseName === 'string' && exercise.exerciseName.trim() !== '' && exercise.exerciseName.toLowerCase().includes(exerciseSearch.toLowerCase())).length === 0 && (
                  <p className="text-500 text-center">Chưa có dữ liệu bài tập</p>
                )}
              </div>
            </Card>
          </div>
        </div>
      </div>
    </AdminLayout>
  );
}
