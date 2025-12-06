import api from './api';

export interface UserInfo {
  fullName: string;
  avatarUrl: string;
}

export interface GoalProgress {
  goalType: string;
  startValue: number;
  currentValue: number;
  targetValue: number;
  progress: number;
  remaining: number;
  status: string;
}

export interface WeightDataPoint {
  date: string;
  weight: number;
}

export interface WeightProgress {
  currentWeight: number;
  targetWeight: number;
  weightLost: number;
  weightRemaining: number;
  progressPercentage: number;
  weightHistory: WeightDataPoint[];
  daysRemaining: number;
  timeRemaining: string;
}

export interface TodayStats {
  caloriesConsumed: number;
  caloriesTarget: number;
  workoutMinutes: number;
  workoutDuration: string;
}

export interface ExerciseStreak {
  currentStreak: number;
  totalDays: number;
}

export interface CustomerDashboard {
  userInfo: UserInfo;
  goalProgress: GoalProgress | null;
  weightProgress: WeightProgress | null;
  todayStats: TodayStats;
  exerciseStreak: ExerciseStreak;
}

export const dashboardService = {
  getCustomerDashboard: async (): Promise<CustomerDashboard> => {
    const response = await api.get('/dashboard/customer');
    return response.data;
  }
};
