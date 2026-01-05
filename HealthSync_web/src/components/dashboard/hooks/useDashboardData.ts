import { useState, useEffect } from "react";
import { dashboardService, CustomerDashboard } from "@/services/dashboardService";
import { goalService, Goal } from "@/services/goalService";
import { exerciseService, Exercise } from "@/services/exerciseService";

export function useDashboardData() {
    const [dashboard, setDashboard] = useState<CustomerDashboard | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [selectedGoalId, setSelectedGoalId] = useState<number | null>(null);
    const [selectedGoalDetails, setSelectedGoalDetails] = useState<any>(null);

    const [exercises, setExercises] = useState<Exercise[]>([]);
    const [searchQuery] = useState("");
    const [muscleGroupFilter] = useState("all");

    useEffect(() => {
        loadDashboard();
        loadExercises();

        // Auto-refresh dashboard every 30 seconds
        const refreshInterval = setInterval(() => {
            loadDashboard();
        }, 30000); // 30 seconds

        return () => clearInterval(refreshInterval);
    }, []);

    // Load selected goal details when a goal is selected
    useEffect(() => {
        if (selectedGoalId) {
            loadSelectedGoalDetails(selectedGoalId);
        } else {
            setSelectedGoalDetails(null);
        }
    }, [selectedGoalId]);

    const loadExercises = async () => {
        try {
            const data = await exerciseService.getExercises({
                search: searchQuery,
                muscleGroup: muscleGroupFilter === "all" ? undefined : muscleGroupFilter
            });
            setExercises(data);
        } catch (error) {
            console.error("Failed to load exercises:", error);
        } finally {
            console.log('Exercises fetch completed');
        }
    };

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
        } finally {
            setLoading(false);
        }
    };

    const calculateGoalProgressFromGoal = (goal: Goal) => {
        const sortedRecords = goal.progressRecords.sort((a, b) =>
            new Date(a.recordDate).getTime() - new Date(b.recordDate).getTime()
        );

        const firstRecord = sortedRecords[0];
        const latestRecord = sortedRecords[sortedRecords.length - 1];

        const startValue = firstRecord?.weightKg || firstRecord?.value || 0;
        const currentValue = latestRecord?.weightKg || latestRecord?.value || startValue;
        const targetValue = goal.targetValue;

        const isDecreaseGoal = goal.type.toLowerCase().includes('loss') ||
            goal.type.toLowerCase().includes('giảm') ||
            targetValue < startValue;

        const progressAmount = isDecreaseGoal ? startValue - currentValue : currentValue - startValue;
        const remaining = isDecreaseGoal ? currentValue - targetValue : targetValue - currentValue;
        const totalChange = Math.abs(targetValue - startValue);
        const progress = totalChange > 0 ? (Math.abs(progressAmount) / totalChange) * 100 : 0;

        return {
            goalType: goal.type,
            startValue,
            currentValue,
            targetValue,
            progress: Math.min(100, Math.max(0, progress)),
            progressAmount,
            remaining,
            status: goal.status
        };
    };

    const loadSelectedGoalDetails = async (goalId: number) => {
        try {
            const goals = await goalService.getGoals();
            const goal = goals.find((g: Goal) => g.goalId === goalId);
            if (!goal) return;
            setSelectedGoalDetails(calculateGoalProgressFromGoal(goal));
        } catch (error) {
            console.error('Failed to load goal details:', error);
        }
    };

    return {
        dashboard,
        loading,
        error,
        selectedGoalId,
        setSelectedGoalId,
        selectedGoalDetails,
        loadDashboard,
        exercises
    };
}
