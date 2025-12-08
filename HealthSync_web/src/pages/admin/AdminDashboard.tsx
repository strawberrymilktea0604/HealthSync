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
  const navigate = useNavigate();

  useEffect(() => {
    fetchStatistics();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeRange]);

  const fetchStatistics = async () => {
    try {
      setLoading(true);
      const data = await adminStatisticsService.getStatistics(timeRange);
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

  const { userStatistics, workoutStatistics } = statistics;

  // Prepare chart data for New User Registrations (Bar Chart)
  const registrationData = {
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
    datasets: [
      {
        data: [45, 52, 48, 78, 65, 89, 82],
        backgroundColor: '#4A6C6F',
        borderRadius: 4,
      }
    ]
  };

  const registrationOptions = {
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false }
    },
    scales: {
      x: { grid: { display: false } },
      y: { grid: { color: '#F5F5F5' }, ticks: { display: false } }
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
              <div className="text-900 font-bold text-4xl mb-2">{userStatistics.totalUsers.toLocaleString()}</div>
              <div className="text-green-500 text-sm font-medium">
                <i className="pi pi-arrow-up text-xs mr-1"></i> +12% from last month
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
              <div className="text-900 font-bold text-4xl mb-2">{userStatistics.newUsersThisMonth}</div>
              <div className="text-green-500 text-sm font-medium">
                <i className="pi pi-arrow-up text-xs mr-1"></i> +8% from last month
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
              <div className="text-900 font-bold text-4xl mb-2">{userStatistics.activeUsers.toLocaleString()}</div>
              <div className="text-green-500 text-sm font-medium">
                <i className="pi pi-arrow-up text-xs mr-1"></i> +15% from last month
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
              <div className="text-900 font-bold text-4xl mb-2">{workoutStatistics.totalWorkoutLogs.toLocaleString()}</div>
              <div className="text-green-500 text-sm font-medium">
                <i className="pi pi-arrow-up text-xs mr-1"></i> This month
              </div>
            </Card>
          </div>
        </div>

        {/* Charts Grid */}
        <div className="grid mt-4">
          <div className="col-12 lg:col-6">
            <Card className="border-round-xl shadow-2">
              <h3 className="text-xl font-semibold mb-4 text-900">New User Registrations</h3>
              <Chart type="bar" data={registrationData} options={registrationOptions} style={{ height: '300px' }} />
            </Card>
          </div>

          <div className="col-12 lg:col-6">
            <Card className="border-round-xl shadow-2">
              <h3 className="text-xl font-semibold mb-4 text-900">Most Popular Exercises</h3>
              <div className="flex flex-column gap-3">
                {workoutStatistics.topExercises.slice(0, 5).map((exercise) => (
                  <div key={exercise.exerciseId} className="flex align-items-center gap-3">
                    <div className="w-1rem h-1rem border-circle" style={{ backgroundColor: '#4A6C6F' }}></div>
                    <span className="font-medium text-900">{exercise.exerciseName}</span>
                    <div className="flex-1 mx-3">
                      <div className="w-full h-0-5rem bg-gray-200 border-round">
                        <div 
                          className="h-full bg-primary border-round" 
                          style={{ width: `${(exercise.usageCount / Math.max(...workoutStatistics.topExercises.map(e => e.usageCount))) * 100}%` }}
                        ></div>
                      </div>
                    </div>
                    <span className="text-500 text-sm">{exercise.usageCount.toLocaleString()}</span>
                  </div>
                ))}
              </div>
            </Card>
          </div>
        </div>
      </div>
    </AdminLayout>
  );
}
