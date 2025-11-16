import AdminLayout from "@/components/admin/AdminLayout";
import StatCard from "@/components/admin/StatCard";

export default function AdminDashboard() {
  const stats = [
    {
      title: "Total Users",
      value: "2,847",
      subtitle: "+12% from last month",
      icon: "👥",
      trend: { value: "+12%", isPositive: true },
    },
    {
      title: "New Users",
      value: "342",
      subtitle: "+8% from last month",
      icon: "➕",
      trend: { value: "+8%", isPositive: true },
    },
    {
      title: "Active Users",
      value: "1,923",
      subtitle: "+15% from last month",
      icon: "❤️",
      trend: { value: "+15%", isPositive: true },
    },
  ];

  const weeklyRegistrations = [
    { day: "Mon", count: 45 },
    { day: "Tue", count: 52 },
    { day: "Wed", count: 48 },
    { day: "Thu", count: 61 },
    { day: "Fri", count: 55 },
    { day: "Sat", count: 68 },
    { day: "Sun", count: 58 },
  ];

  const popularExercises = [
    { name: "Push-ups", count: 1247 },
    { name: "Squats", count: 1089 },
    { name: "Deadlifts", count: 892 },
    { name: "Bench Press", count: 673 },
    { name: "Pull-ups", count: 456 },
  ];

  const maxCount = Math.max(...weeklyRegistrations.map((d) => d.count));

  return (
    <AdminLayout>
      <div className="space-y-8">
        <div>
          <h2 className="text-3xl font-bold text-gray-900 mb-2">Dashboard</h2>
          <p className="text-gray-600">Overview of platform statistics</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {stats.map((stat, index) => (
            <StatCard key={index} {...stat} />
          ))}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-6">
              New User Registrations
            </h3>
            <div className="space-y-4">
              {weeklyRegistrations.map((item) => (
                <div key={item.day} className="flex items-center gap-4">
                  <span className="text-sm font-medium text-gray-600 w-12">
                    {item.day}
                  </span>
                  <div className="flex-1 bg-gray-200 rounded-full h-8 relative overflow-hidden">
                    <div
                      className="bg-[#4A6F6F] h-full rounded-full transition-all duration-500"
                      style={{ width: `${(item.count / maxCount) * 100}%` }}
                    />
                  </div>
                  <span className="text-sm font-semibold text-gray-900 w-12 text-right">
                    {item.count}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-6">
              Most Popular Exercises
            </h3>
            <div className="space-y-4">
              {popularExercises.map((exercise, index) => (
                <div
                  key={exercise.name}
                  className="flex items-center justify-between pb-3 border-b border-gray-100 last:border-0"
                >
                  <div className="flex items-center gap-3">
                    <span className="w-6 h-6 rounded-full bg-[#4A6F6F] text-white flex items-center justify-center text-xs font-bold">
                      {index + 1}
                    </span>
                    <span className="text-sm font-medium text-gray-900">
                      {exercise.name}
                    </span>
                  </div>
                  <div className="flex items-center gap-3">
                    <div className="w-32 bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-[#4A6F6F] h-full rounded-full"
                        style={{
                          width: `${
                            (exercise.count / popularExercises[0].count) * 100
                          }%`,
                        }}
                      />
                    </div>
                    <span className="text-sm font-semibold text-gray-600 w-12 text-right">
                      {exercise.count}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </AdminLayout>
  );
}
