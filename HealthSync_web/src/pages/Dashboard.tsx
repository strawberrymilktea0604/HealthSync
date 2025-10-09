import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
// Import các component icon từ lucide-react
import { LogOut, HeartPulse, Scale, Activity, ArrowRight, User, LucideIcon } from 'lucide-react'; 

// --- KHAI BÁO KIỂU DỮ LIỆU (INTERFACE) ĐỂ KHẮC PHỤC LỖI TYPESCRIPT ---

interface Metric {
    icon: React.ReactElement; // Icon là một React Element
    title: string;
    value: string | number;
    unit: string;
}

interface ActivityData {
    id: number;
    name: string;
    duration: string;
    calories: number;
    date: string;
}

interface UserData {
    fullName: string;
    email: string;
    lastLogin: string;
    bmi: number;
    status: string;
    metrics: Metric[];
}

// Mock Data cho Dashboard (đã áp dụng kiểu dữ liệu)
const mockUserData: UserData = {
    fullName: "Nguyễn Văn A",
    email: "vana@healthsync.com",
    lastLogin: "09/10/2025",
    bmi: 21.5,
    status: "Bình thường",
    metrics: [
        { icon: <Scale className="h-6 w-6 text-indigo-500" />, title: "Cân nặng hiện tại", value: "72 kg", unit: "kg" },
        { icon: <HeartPulse className="h-6 w-6 text-red-500" />, title: "Nhịp tim trung bình", value: "68 bpm", unit: "nhịp/phút" },
        { icon: <Activity className="h-6 w-6 text-yellow-500" />, title: "Mục tiêu bước chân", value: "8,000", unit: "bước/ngày" },
    ]
};

const mockActivities: ActivityData[] = [
    { id: 1, name: "Chạy bộ ngoài trời", duration: "30 phút", calories: 350, date: "Hôm nay, 18:00" },
    { id: 2, name: "Tập gym", duration: "60 phút", calories: 500, date: "Hôm qua, 19:30" },
    { id: 3, name: "Yoga cơ bản", duration: "45 phút", calories: 150, date: "07/10/2025" },
];

// Component DashboardCard đã nhận kiểu props rõ ràng
const DashboardCard: React.FC<Metric> = ({ icon, title, value, unit }) => (
    <div className="rounded-xl bg-white p-5 shadow-lg transition hover:shadow-xl">
        <div className="flex items-center gap-4">
            {icon}
            <div>
                <p className="text-xs font-medium uppercase text-zinc-500">{title}</p>
                <p className="text-2xl font-bold text-zinc-900">{value}</p>
                <p className="text-sm text-zinc-600">{unit}</p>
            </div>
        </div>
    </div>
);

// Component ActivityItem đã nhận kiểu props rõ ràng
const ActivityItem: React.FC<{ activity: ActivityData }> = ({ activity }) => (
    <div className="flex items-center justify-between border-b border-zinc-100 py-3 last:border-b-0">
        <div className="flex items-center gap-3">
            <Activity className="h-5 w-5 text-emerald-600" />
            <div>
                <p className="font-medium text-zinc-800">{activity.name}</p>
                <p className="text-xs text-zinc-500">{activity.date}</p>
            </div>
        </div>
        <div className="text-right">
            <p className="font-semibold text-emerald-600">{activity.calories} Kcal</p>
            <p className="text-xs text-zinc-500">{activity.duration}</p>
        </div>
    </div>
);

export default function Dashboard() {
    const nav = useNavigate();
    const [isAuth, setIsAuth] = useState(false);

    // Kiểm tra trạng thái đăng nhập
    useEffect(() => {
        const token = localStorage.getItem("token");
        if (!token) {
            // Nếu không có token, chuyển hướng về trang đăng nhập
            nav("/login", { replace: true });
        } else {
            setIsAuth(true);
        }
    }, [nav]);

    const handleLogout = () => {
        localStorage.removeItem("token");
        nav("/login");
    };

    if (!isAuth) {
        return (
            <div className="flex h-screen items-center justify-center bg-zinc-50">
                <p className="text-lg text-zinc-600">Đang kiểm tra trạng thái đăng nhập...</p>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-zinc-50 font-sans">
            {/* Header */}
            <header className="sticky top-0 z-20 bg-white shadow-md">
                <div className="mx-auto flex max-w-7xl items-center justify-between p-4">
                    <div className="text-2xl font-extrabold text-zinc-900">
                        Health<span className="text-emerald-600">Sync</span>
                    </div>
                    <div className="flex items-center gap-4">
                        <button
                            onClick={handleLogout}
                            className="inline-flex items-center gap-2 rounded-xl bg-red-500 px-4 py-2 text-sm font-medium text-white shadow-md transition hover:bg-red-600"
                        >
                            <LogOut className="h-4 w-4" />
                            Đăng xuất
                        </button>
                    </div>
                </div>
            </header>

            {/* Main Content */}
            <main className="mx-auto max-w-7xl px-4 py-8 md:py-12">
                
                {/* Greeting and User Info */}
                <div className="mb-8 rounded-2xl bg-white p-6 shadow-xl md:flex md:items-center md:justify-between">
                    <div>
                        <h1 className="text-3xl font-bold text-zinc-900">
                            Chào mừng trở lại, {mockUserData.fullName}!
                        </h1>
                        <p className="mt-1 text-zinc-600">
                            Dashboard tổng quan sức khỏe của bạn tính đến hôm nay.
                        </p>
                    </div>
                    <div className="mt-4 md:mt-0 flex items-center gap-3 p-3 rounded-full bg-emerald-50 text-emerald-700">
                        <User className="h-5 w-5" />
                        <span className="text-sm font-medium">{mockUserData.email}</span>
                    </div>
                </div>

                {/* Metrics Grid */}
                <div className="grid gap-6 md:grid-cols-3">
                    {mockUserData.metrics.map((metric, index) => (
                        <DashboardCard key={index} {...metric} />
                    ))}
                    
                    {/* BMI Card */}
                    <div className="rounded-xl bg-emerald-600 p-5 shadow-lg text-white md:col-span-1">
                        <div className="flex items-center gap-4">
                            <Scale className="h-6 w-6 text-white" />
                            <div>
                                <p className="text-xs font-medium uppercase text-emerald-200">Chỉ số BMI</p>
                                <p className="text-2xl font-bold">{mockUserData.bmi}</p>
                                <p className="text-sm">{mockUserData.status}</p>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Recent Activities and Tips Section */}
                <div className="mt-12 grid gap-8 lg:grid-cols-2">
                    
                    {/* Recent Activities */}
                    <div className="rounded-2xl bg-white p-6 shadow-xl">
                        <h2 className="text-xl font-semibold text-zinc-900 mb-4 border-b border-zinc-100 pb-3">Hoạt động gần đây</h2>
                        <div className="space-y-2">
                            {mockActivities.map(activity => (
                                <ActivityItem key={activity.id} activity={activity} />
                            ))}
                        </div>
                        <button className="mt-4 flex items-center gap-2 text-emerald-600 hover:text-emerald-700 text-sm font-medium transition">
                            Xem tất cả hoạt động
                            <ArrowRight className="h-4 w-4" />
                        </button>
                    </div>

                    {/* Health Tips / Summary */}
                    <div className="rounded-2xl bg-white p-6 shadow-xl">
                        <h2 className="text-xl font-semibold text-zinc-900 mb-4 border-b border-zinc-100 pb-3">Đánh giá nhanh & Lời khuyên</h2>
                        <div className="space-y-4 text-zinc-700">
                            <p>
                                Dựa trên chỉ số BMI **${mockUserData.bmi}** (Bình thường), bạn đang duy trì trạng thái tốt.
                            </p>
                            <div className="rounded-lg bg-emerald-50 p-4 border-l-4 border-emerald-500">
                                <p className="font-semibold text-emerald-700">Lời khuyên hôm nay:</p>
                                <p className="text-sm mt-1 text-emerald-600">
                                    Tiếp tục duy trì hoạt động đều đặn và đảm bảo bạn ngủ đủ 7-8 tiếng để tối ưu hóa việc phục hồi cơ bắp.
                                </p>
                            </div>
                            <p className="text-sm text-zinc-500">Đăng nhập lần cuối: {mockUserData.lastLogin}</p>
                        </div>
                    </div>
                </div>
            </main>

            {/* Footer (Optional) */}
            <footer className="py-4 text-center text-sm text-zinc-500 border-t border-zinc-200 mt-12">
                © {new Date().getFullYear()} HealthSync. All rights reserved.
            </footer>
        </div>
    );
}
