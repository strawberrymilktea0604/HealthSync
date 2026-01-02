import { useEffect, useState } from 'react';
import AdminLayout from '../../components/admin/AdminLayout';
import { adminService, AdminDashboardDto } from '../../services/adminService';
import StatCard from '../../components/admin/StatCard';
import { LineChartCard } from '../../components/admin/LineChartCard';
import { PieChartCard } from '../../components/admin/PieChartCard';

const NoDataDisplay = ({ message }: { message: string }) => (
  <div className="flex flex-col items-center justify-center p-6 text-gray-400 bg-gray-50 rounded-lg border border-dashed border-gray-200 min-h-[120px]">
    <span className="text-2xl mb-2">📊</span>
    <span className="text-sm font-medium">{message}</span>
  </div>
);

export default function AdminDashboard() {
  const [data, setData] = useState<AdminDashboardDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      const result = await adminService.getDashboard();
      setData(result);
    } catch (err: any) {
      setError(err?.message || 'Failed to fetch dashboard data');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <AdminLayout>
        <div className="flex items-center justify-center min-h-[80vh]">
          <div className="w-12 h-12 border-4 border-primary border-t-transparent rounded-full animate-spin"></div>
        </div>
      </AdminLayout>
    );
  }

  if (error || !data) {
    return (
      <AdminLayout>
        <div className="p-8 text-red-500 text-center">
          Error: {error || 'No data available'}
        </div>
      </AdminLayout>
    );
  }

  // Prepare Chart Data
  const userGrowthData = data.charts.userGrowth.labels.map((label, index) => ({
    name: label,
    users: data.charts.userGrowth.data[index],
  }));

  const goalData = data.charts.goalSuccessRate.labels.map((label: string, index: number) => ({
    name: label,
    value: data.charts.goalSuccessRate.data[index],
  }));

  return (
    <AdminLayout>
      <div className="p-6 space-y-6">
        <div className="flex justify-between items-center bg-white p-4 rounded-lg shadow-sm">
          <div>
            <h1 className="text-2xl font-bold text-gray-800">Admin Dashboard</h1>
            <p className="text-sm text-gray-500 mt-1">Overview of system performance and user activity</p>
          </div>
          <div className="flex items-center space-x-4">
            <span className="text-xs text-gray-400">
              Updated: {new Date(data.timestamp).toLocaleTimeString()}
            </span>
            <button
              onClick={fetchDashboardData}
              disabled={loading}
              className="px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-md hover:bg-indigo-700 transition-colors disabled:opacity-50 flex items-center"
            >
              {loading ? (
                <span className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></span>
              ) : (
                <span className="mr-2">↻</span>
              )}
              Refresh
            </button>
          </div>
        </div>

        {/* 1. KPI Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <StatCard
            title="Total Users"
            value={data.kpiStats.totalUsers.value}
            subtitle="Registered accounts"
            trend={{
              value: `${data.kpiStats.totalUsers.growthRate}%`,
              isPositive: data.kpiStats.totalUsers.trend === 'up'
            }}
            icon="👥"
          />
          <StatCard
            title="Active Users"
            value={data.kpiStats.activeUsers.monthly}
            subtitle={`Daily: ${data.kpiStats.activeUsers.daily}`}
            trend={{
              value: `${data.kpiStats.activeUsers.growthRate}%`,
              isPositive: data.kpiStats.activeUsers.trend === 'up'
            }}
            icon="🔥"
          />
          <StatCard
            title="Content Library"
            value={data.kpiStats.contentCount.total}
            subtitle={`${data.kpiStats.contentCount.exercises} Exercises, ${data.kpiStats.contentCount.foodItems} Foods`}
            icon="📚"
          />
          <StatCard
            title="AI Usage"
            value={data.kpiStats.aiUsage.totalRequests}
            subtitle={`Est. Cost: $${data.kpiStats.aiUsage.costEstimate.toFixed(2)}`}
            icon="🤖"
          />
        </div>

        {/* 2. Charts Section */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <LineChartCard
            title="User Growth (6 Months)"
            data={userGrowthData}
            dataKey="users"
            xAxisKey="name"
            color="#4F46E5"
          />
          <PieChartCard
            title="Goal Success Rate"
            data={goalData}
            dataKey="value"
            nameKey="name"
            colors={['#10B981', '#F59E0B', '#EF4444']}
          />
        </div>

        {/* 3. Heatmap & Insights */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Activity Heatmap (Simplified List) */}
          <div className="bg-white rounded-lg shadow p-6 lg:col-span-1 border border-gray-100">
            <h3 className="text-xl font-semibold mb-4 text-gray-800">Activity Peak Hours</h3>
            <div className="space-y-4">
              <p className="text-xs text-gray-500 uppercase font-bold tracking-wider mb-2">Most active times</p>
              {data.charts.activityHeatmap.length === 0 ? (
                <NoDataDisplay message="Not enough activity data" />
              ) : (
                <div className="space-y-2">
                  {data.charts.activityHeatmap
                    .sort((a, b) => b.count - a.count)
                    .slice(0, 5)
                    .map((point) => (
                      <div key={`${point.day}-${point.hour}`} className="flex justify-between items-center p-2 hover:bg-gray-50 rounded transition-colors">
                        <span className="font-medium text-gray-700 text-sm">
                          {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'][point.day]} at {point.hour}:00
                        </span>
                        <span className="bg-indigo-100 text-indigo-700 text-xs font-bold px-2 py-1 rounded-full">
                          {point.count} logs
                        </span>
                      </div>
                    ))}
                </div>
              )}
            </div>
          </div>

          {/* Content Insights */}
          <div className="bg-white rounded-lg shadow p-6 lg:col-span-1 border border-gray-100">
            <h3 className="text-xl font-semibold mb-4 text-gray-800">Top Content</h3>
            <div className="space-y-6">
              <div>
                <h4 className="text-xs font-bold text-gray-500 uppercase mb-3 tracking-wider flex items-center">
                  <span className="w-2 h-2 bg-green-500 rounded-full mr-2"></span>Top Exercises
                </h4>
                {data.contentInsights.topExercises.length === 0 ? (
                  <div className="text-sm text-gray-400 italic pl-4">No exercise data yet</div>
                ) : (
                  <ul className="space-y-2">
                    {data.contentInsights.topExercises.map(e => (
                      <li key={e.id} className="flex justify-between items-center text-sm p-2 hover:bg-gray-50 rounded">
                        <span className="text-gray-700 font-medium truncate max-w-[150px]" title={e.name}>{e.name}</span>
                        <span className="text-gray-500 bg-gray-100 px-2 py-0.5 rounded text-xs">{e.count}</span>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
              <div className="border-t pt-4">
                <h4 className="text-xs font-bold text-gray-500 uppercase mb-3 tracking-wider flex items-center">
                  <span className="w-2 h-2 bg-orange-500 rounded-full mr-2"></span>Top Foods
                </h4>
                {data.contentInsights.topFoods.length === 0 ? (
                  <div className="text-sm text-gray-400 italic pl-4">No food data yet</div>
                ) : (
                  <ul className="space-y-2">
                    {data.contentInsights.topFoods.map(f => (
                      <li key={f.id} className="flex justify-between items-center text-sm p-2 hover:bg-gray-50 rounded">
                        <span className="text-gray-700 font-medium truncate max-w-[150px]" title={f.name}>{f.name}</span>
                        <span className="text-gray-500 bg-gray-100 px-2 py-0.5 rounded text-xs">{f.count}</span>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            </div>
          </div>

          {/* System Health */}
          <div className="bg-white rounded-lg shadow p-6 lg:col-span-1 border border-gray-100">
            <h3 className="text-xl font-semibold mb-4 text-gray-800">System Health</h3>
            <div className="space-y-4">
              <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg border border-gray-100">
                <span className="font-medium text-gray-700">Overall Status</span>
                <span className={`px-3 py-1 rounded-full text-xs font-bold border ${data.systemHealth.status === 'Healthy' ? 'bg-green-50 text-green-700 border-green-200' : 'bg-red-50 text-red-700 border-red-200'
                  }`}>
                  {data.systemHealth.status}
                </span>
              </div>

              <div className="space-y-3">
                {data.systemHealth.services.map((service) => (
                  <div key={service.name} className="flex items-center justify-between p-2 hover:bg-gray-50 rounded transition-colors">
                    <div className="flex items-center space-x-3">
                      <div className={`h-2.5 w-2.5 rounded-full ${service.status === 'Online' ? 'bg-green-500 shadow-sm shadow-green-200' : 'bg-red-500 shadow-sm shadow-red-200'}`} />
                      <div>
                        <p className="text-sm font-medium text-gray-700">{service.name}</p>
                        <p className="text-xs text-gray-400">{service.latencyMs}ms latency</p>
                      </div>
                    </div>
                    <span className={`text-xs px-2 py-0.5 rounded ${service.status === 'Online' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                      {service.status}
                    </span>
                  </div>
                ))}
              </div>

              {data.systemHealth.recentErrors.length > 0 && (
                <div className="mt-4 pt-4 border-t border-gray-100">
                  <h4 className="text-xs font-bold text-red-600 mb-2 uppercase tracking-wide">Recent Errors</h4>
                  <div className="space-y-2 max-h-40 overflow-y-auto pr-1 custom-scrollbar">
                    {data.systemHealth.recentErrors.map((err) => (
                      <div key={err.id} className="text-xs bg-red-50 p-2 rounded border border-red-100">
                        <div className="flex justify-between items-start mb-1">
                          <span className="font-mono text-red-600 font-bold">[{err.code}]</span>
                          <span className="text-gray-400 text-[10px]">{new Date(err.timestamp).toLocaleTimeString()}</span>
                        </div>
                        <p className="text-gray-700">{err.message}</p>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </AdminLayout>
  );
}
