import { useEffect, useState } from "react";
import { Button } from "primereact/button";
import { Avatar } from "primereact/avatar";
import { Card } from "primereact/card";
import { ProgressBar } from "primereact/progressbar";
import { dashboardService, CustomerDashboard } from "@/services/dashboardService";

export default function Dashboard() {
  const [dashboard, setDashboard] = useState<CustomerDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      const data = await dashboardService.getCustomerDashboard();
      setDashboard(data);
      setError(null);
    } catch (err: unknown) {
      const errorMessage = err && typeof err === 'object' && 'response' in err 
        ? (err as { response?: { data?: { message?: string } } }).response?.data?.message 
        : undefined;
      setError(errorMessage || "Không thể tải dữ liệu dashboard");
      console.error("Error loading dashboard:", err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen surface-ground">
        <div className="flex align-items-center justify-content-center" style={{ minHeight: '400px' }}>
          <i className="pi pi-spin pi-spinner" style={{ fontSize: '3rem' }}></i>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen surface-ground p-4">
        <Card className="bg-red-50">
          <p className="text-red-600 m-0">{error}</p>
        </Card>
      </div>
    );
  }

  if (!dashboard) return null;

  const { userInfo, goalProgress, weightProgress, todayStats } = dashboard;

  return (
    <div className="min-h-screen surface-ground">
      {/* Header */}
      <header className="surface-card border-bottom-1 surface-border px-4 py-3">
        <div className="flex align-items-center justify-content-between max-w-7xl mx-auto">
          <div className="flex align-items-center gap-3">
            <h1 className="text-3xl font-bold m-0">
              <span className="text-900">health</span>
              <span className="text-primary">sync</span>
            </h1>
          </div>

          <div className="flex align-items-center gap-3">
            <Avatar 
              image={userInfo.avatarUrl || "https://placehold.co/100x100"} 
              size="large" 
              shape="circle" 
            />
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 py-6">
        {/* Welcome Section */}
        <div className="mb-5">
          <p className="text-xl mb-2">Welcome to</p>
          <h2 className="text-4xl font-bold m-0">
            <span className="text-900">health</span>
            <span className="text-primary">sync</span>
          </h2>
          <p className="text-600 mt-2">{userInfo.fullName}</p>
        </div>

        {/* Your Activities */}
        <div className="mb-5">
          <div className="flex align-items-center justify-content-between mb-3">
            <h3 className="text-2xl font-semibold m-0">Your Activities</h3>
            <span className="text-600">Today, 8 Jul</span>
          </div>

          <div className="grid">
            {/* Mục tiêu */}
            <div className="col-12 lg:col-4">
              <Card className="h-full" style={{ backgroundColor: '#E8F5E9', borderRadius: '2rem' }}>
                <div className="flex flex-column gap-3">
                  <div className="flex align-items-center justify-content-between">
                    <h4 className="text-xl font-bold m-0">Mục tiêu</h4>
                    <i className="pi pi-chart-line text-4xl text-green-500"></i>
                  </div>
                  
                  {goalProgress ? (
                    <div>
                      <div className="mb-3">
                        <p className="text-sm text-600 m-0 mb-2">Mục tiêu chính</p>
                        <div className="flex align-items-center gap-3">
                          <div className="flex-1">
                            <p className="text-4xl font-bold m-0">{goalProgress.currentValue}kg</p>
                            <p className="text-sm text-600">Giảm</p>
                          </div>
                          <div className="text-right">
                            <p className="text-2xl font-bold text-green-600">{goalProgress.progress.toFixed(1)}kg</p>
                            <p className="text-xs text-600">Đã giảm {goalProgress.progress.toFixed(1)}kg<br/>còn {goalProgress.remaining.toFixed(1)}kg nữa</p>
                          </div>
                        </div>
                      </div>
                      
                      <div className="mb-3">
                        <p className="text-sm font-semibold mb-2">Tiến độ</p>
                        <ProgressBar 
                          value={Math.min(weightProgress?.progressPercentage || 0, 100)} 
                          showValue={false}
                          style={{ height: '8px' }}
                        />
                        <p className="text-xs text-600 mt-1">{Math.round(weightProgress?.progressPercentage || 0)}%</p>
                      </div>
                      
                      <img 
                        src="https://placehold.co/300x200" 
                        alt="Progress chart" 
                        className="w-full border-round-2xl"
                      />
                    </div>
                  ) : (
                    <div className="text-center py-4">
                      <p className="text-600 mb-3">Chưa có mục tiêu</p>
                      <Button label="Tạo mục tiêu" outlined />
                    </div>
                  )}
                </div>
              </Card>
            </div>

            {/* Dinh dưỡng */}
            <div className="col-12 lg:col-4">
              <Card className="h-full" style={{ backgroundColor: '#FFF3E0', borderRadius: '2rem' }}>
                <div className="flex flex-column gap-3">
                  <div className="flex align-items-center justify-content-between">
                    <h4 className="text-xl font-bold m-0">Dinh dưỡng</h4>
                  </div>
                  
                  <div className="text-center py-4">
                    <img 
                      src="https://placehold.co/300x200" 
                      alt="Nutrition" 
                      className="w-full border-round-2xl mb-3"
                    />
                    <div className="text-left">
                      <p className="text-sm text-600 mb-2">Calo hôm nay</p>
                      <p className="text-3xl font-bold m-0">{todayStats.caloriesConsumed}/{todayStats.caloriesTarget}</p>
                    </div>
                  </div>
                  
                  <Button 
                    label="Ghi bữa ăn" 
                    className="w-full" 
                    style={{ borderRadius: '2rem' }}
                  />
                </div>
              </Card>
            </div>

            {/* Luyện tập */}
            <div className="col-12 lg:col-4">
              <Card className="h-full" style={{ backgroundColor: '#E3F2FD', borderRadius: '2rem' }}>
                <div className="flex flex-column gap-3">
                  <div className="flex align-items-center justify-content-between">
                    <h4 className="text-xl font-bold m-0">Luyện tập</h4>
                  </div>
                  
                  <div className="text-center py-4">
                    <img 
                      src="https://placehold.co/300x200" 
                      alt="Exercise" 
                      className="w-full border-round-2xl mb-3"
                    />
                    <div className="text-left">
                      <p className="text-sm text-600 mb-2">Tổng thời gian tập tuần này</p>
                      <p className="text-3xl font-bold m-0">{todayStats.workoutDuration || "0"}</p>
                      <p className="text-sm text-600">25/25 giờ</p>
                    </div>
                  </div>
                  
                  <Button 
                    label="Ghi buổi tập" 
                    className="w-full"
                    style={{ borderRadius: '2rem' }}
                  />
                </div>
              </Card>
            </div>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="surface-card border-top-1 surface-border py-6 mt-8">
        <div className="max-w-7xl mx-auto px-4">
          <div className="grid align-items-center">
            <div className="col-12 md:col-4">
              <h2 className="text-3xl font-bold m-0">
                <span className="text-900">health</span>
                <span className="text-primary">sync</span>
              </h2>
            </div>
            
            <div className="col-12 md:col-4">
              <div className="flex flex-wrap gap-4 justify-content-center">
                <a href="#inspiration" className="text-600 no-underline">Inspiration</a>
                <a href="#about" className="text-600 no-underline">About</a>
                <a href="#support" className="text-600 no-underline">Support</a>
                <a href="#blog" className="text-600 no-underline">Blog</a>
                <a href="#pts" className="text-600 no-underline">PTs</a>
              </div>
            </div>

            <div className="col-12 md:col-4">
              <div className="flex gap-3 justify-content-end">
                <Button icon="pi pi-twitter" rounded text />
                <Button icon="pi pi-facebook" rounded text />
                <Button icon="pi pi-instagram" rounded text />
              </div>
            </div>
          </div>

          <div className="border-top-1 surface-border pt-4 mt-4">
            <div className="flex flex-wrap gap-4 justify-content-between text-sm text-600">
              <span>© healthsync 2025</span>
              <div className="flex gap-4">
                <a href="#terms" className="text-600 no-underline">Term&Conditions</a>
                <a href="#cookies" className="text-600 no-underline">Cookies</a>
                <a href="#resources" className="text-600 no-underline">Resources</a>
                <a href="#tags" className="text-600 no-underline">Tags</a>
                <a href="#freelancers" className="text-600 no-underline">Freelancers</a>
              </div>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}
