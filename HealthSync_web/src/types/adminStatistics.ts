// Admin Statistics Types
export interface AdminStatistics {
  userStatistics: UserStatistics;
  workoutStatistics: WorkoutStatistics;
  nutritionStatistics: NutritionStatistics;
  goalStatistics: GoalStatistics;
}

export interface UserStatistics {
  totalUsers: number;
  activeUsers: number;
  newUsersThisMonth: number;
  newUsersThisWeek: number;
  userGrowthData: UserGrowthData[];
  userRoleDistribution: UserRoleDistribution[];
}

export interface UserGrowthData {
  period: string;
  count: number;
  date: string;
}

export interface UserRoleDistribution {
  role: string;
  count: number;
}

export interface WorkoutStatistics {
  totalWorkoutLogs: number;
  workoutLogsThisMonth: number;
  totalExercises: number;
  topExercises: PopularExercise[];
  workoutActivityData: WorkoutActivityData[];
  muscleGroupDistribution: MuscleGroupDistribution[];
}

export interface PopularExercise {
  exerciseId: number;
  exerciseName: string;
  muscleGroup: string;
  usageCount: number;
}

export interface WorkoutActivityData {
  period: string;
  count: number;
  date: string;
}

export interface MuscleGroupDistribution {
  muscleGroup: string;
  count: number;
}

export interface NutritionStatistics {
  totalNutritionLogs: number;
  nutritionLogsThisMonth: number;
  totalFoodItems: number;
  topFoods: PopularFood[];
  nutritionActivityData: NutritionActivityData[];
  averageDailyNutrition: AverageNutrition;
}

export interface PopularFood {
  foodItemId: number;
  foodName: string;
  usageCount: number;
  calories: number;
}

export interface NutritionActivityData {
  period: string;
  count: number;
  date: string;
}

export interface AverageNutrition {
  averageCalories: number;
  averageProtein: number;
  averageCarbs: number;
  averageFat: number;
}

export interface GoalStatistics {
  totalGoals: number;
  activeGoals: number;
  completedGoals: number;
  goalTypeDistribution: GoalTypeDistribution[];
  goalStatusDistribution: GoalStatusDistribution[];
  goalCompletionRate: number;
}

export interface GoalTypeDistribution {
  goalType: string;
  count: number;
}

export interface GoalStatusDistribution {
  status: string;
  count: number;
}
