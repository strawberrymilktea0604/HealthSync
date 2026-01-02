import { useEffect, useState } from 'react';
import { adminService, AdminDashboardDto } from '../../services/adminService';
import StatCard from '../../components/admin/StatCard';
import { LineChartCard } from '../../components/admin/LineChartCard';
import { PieChartCard } from '../../components/admin/PieChartCard';

export default function DashboardPage() {
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

    if (loading) return <div className="p-8 text-center">Loading dashboard...</div>;
    if (error) return <div className="p-8 text-red-500">Error: {error}</div>;
    if (!data) return null;

    // Prepare Chart Data
    const userGrowthData = data.charts.userGrowth.labels.map((label, index) => ({
        name: label,
        users: data.charts.userGrowth.data[index],
    }));

    const goalData = data.charts.goalSuccessRate.labels.map((label, index) => ({
        name: label,
        value: data.charts.goalSuccessRate.data[index],
    }));

    return (
        <div className="p-6 space-y-6 bg-gray-50 min-h-screen">
            <div className="flex justify-between items-center">
                <h1 className="text-2xl font-bold text-gray-800">Admin Dashboard</h1>
                <span className="text-sm text-gray-500">Last updated: {new Date(data.timestamp).toLocaleString()}</span>
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
                {/* Activity Heatmap (Simplified List/Grid) */}
                <div className="bg-white rounded-lg shadow p-6 lg:col-span-1">
                    <h3 className="text-xl font-semibold mb-4">Activity Peak Hours</h3>
                    <div className="space-y-4">
                        <p className="text-sm text-gray-600">Most active times during the week:</p>
                        {/* Find top 5 active slots from heatmap data */}
                        {data.charts.activityHeatmap
                            .sort((a, b) => b.count - a.count)
                            .slice(0, 5)
                            .map((point) => (
                                <div key={`${point.day}-${point.hour}`} className="flex justify-between items-center border-b pb-2">
                                    <span className="font-medium text-gray-700">
                                        {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'][point.day]} at {point.hour}:00
                                    </span>
                                    <span className="bg-blue-100 text-blue-800 text-xs font-semibold px-2.5 py-0.5 rounded">
                                        {point.count} logs
                                    </span>
                                </div>
                            ))}
                    </div>
                </div>

                {/* Content Insights */}
                <div className="bg-white rounded-lg shadow p-6 lg:col-span-1">
                    <h3 className="text-xl font-semibold mb-4">Top Content</h3>
                    <div className="space-y-6">
                        <div>
                            <h4 className="text-sm font-bold text-gray-500 uppercase mb-2">Top Exercises</h4>
                            <ul className="space-y-2">
                                {data.contentInsights.topExercises.map(e => (
                                    <li key={e.id} className="flex justify-between text-sm">
                                        <span>{e.name}</span>
                                        <span className="text-gray-500">{e.count} logs</span>
                                    </li>
                                ))}
                            </ul>
                        </div>
                        <div>
                            <h4 className="text-sm font-bold text-gray-500 uppercase mb-2">Top Foods</h4>
                            <ul className="space-y-2">
                                {data.contentInsights.topFoods.map(f => (
                                    <li key={f.id} className="flex justify-between text-sm">
                                        <span>{f.name}</span>
                                        <span className="text-gray-500">{f.count} logs</span>
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>
                </div>

                {/* System Health */}
                <div className="bg-white rounded-lg shadow p-6 lg:col-span-1">
                    <h3 className="text-xl font-semibold mb-4">System Health</h3>
                    <div className="space-y-4">
                        <div className="flex items-center justify-between p-3 bg-gray-50 rounded">
                            <span className="font-medium">Overall Status</span>
                            <span className={`px-2 py-1 rounded text-xs font-bold ${data.systemHealth.status === 'Healthy' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                                }`}>
                                {data.systemHealth.status}
                            </span>
                        </div>

                        {data.systemHealth.services.map((service, idx) => (
                            <div key={idx} className="flex items-center justify-between border-l-4 border-green-500 pl-3">
                                <div>
                                    <p className="text-sm font-medium">{service.name}</p>
                                    <p className="text-xs text-gray-500">{service.latencyMs}ms latency</p>
                                </div>
                                <div className="h-3 w-3 rounded-full bg-green-500" title={service.status}></div>
                            </div>
                        ))}

                        {data.systemHealth.recentErrors.length > 0 && (
                            <div className="mt-4 pt-4 border-t">
                                <h4 className="text-sm font-bold text-red-600 mb-2">Recent Errors</h4>
                                <div className="space-y-2 max-h-40 overflow-y-auto">
                                    {data.systemHealth.recentErrors.map((err, idx) => (
                                        <div key={idx} className="text-xs text-gray-600 bg-red-50 p-2 rounded">
                                            <span className="font-mono text-red-500">[{err.code}]</span> {err.message}
                                            <br />
                                            <span className="text-gray-400">{new Date(err.timestamp).toLocaleTimeString()}</span>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
