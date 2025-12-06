class CustomerDashboard {
  final UserInfo userInfo;
  final GoalProgress? goalProgress;
  final WeightProgress? weightProgress;
  final TodayStats todayStats;
  final ExerciseStreak exerciseStreak;

  CustomerDashboard({
    required this.userInfo,
    this.goalProgress,
    this.weightProgress,
    required this.todayStats,
    required this.exerciseStreak,
  });

  factory CustomerDashboard.fromJson(Map<String, dynamic> json) {
    return CustomerDashboard(
      userInfo: UserInfo.fromJson(json['userInfo']),
      goalProgress: json['goalProgress'] != null
          ? GoalProgress.fromJson(json['goalProgress'])
          : null,
      weightProgress: json['weightProgress'] != null
          ? WeightProgress.fromJson(json['weightProgress'])
          : null,
      todayStats: TodayStats.fromJson(json['todayStats']),
      exerciseStreak: ExerciseStreak.fromJson(json['exerciseStreak']),
    );
  }
}

class UserInfo {
  final String fullName;
  final String avatarUrl;

  UserInfo({required this.fullName, required this.avatarUrl});

  factory UserInfo.fromJson(Map<String, dynamic> json) {
    return UserInfo(
      fullName: json['fullName'] ?? '',
      avatarUrl: json['avatarUrl'] ?? '',
    );
  }
}

class GoalProgress {
  final String goalType;
  final double startValue;
  final double currentValue;
  final double targetValue;
  final double progress;
  final double remaining;
  final String status;

  GoalProgress({
    required this.goalType,
    required this.startValue,
    required this.currentValue,
    required this.targetValue,
    required this.progress,
    required this.remaining,
    required this.status,
  });

  factory GoalProgress.fromJson(Map<String, dynamic> json) {
    return GoalProgress(
      goalType: json['goalType'] ?? '',
      startValue: (json['startValue'] ?? 0).toDouble(),
      currentValue: (json['currentValue'] ?? 0).toDouble(),
      targetValue: (json['targetValue'] ?? 0).toDouble(),
      progress: (json['progress'] ?? 0).toDouble(),
      remaining: (json['remaining'] ?? 0).toDouble(),
      status: json['status'] ?? '',
    );
  }
}

class WeightProgress {
  final double currentWeight;
  final double targetWeight;
  final double weightLost;
  final double weightRemaining;
  final double progressPercentage;
  final List<WeightDataPoint> weightHistory;
  final int daysRemaining;
  final String timeRemaining;

  WeightProgress({
    required this.currentWeight,
    required this.targetWeight,
    required this.weightLost,
    required this.weightRemaining,
    required this.progressPercentage,
    required this.weightHistory,
    required this.daysRemaining,
    required this.timeRemaining,
  });

  factory WeightProgress.fromJson(Map<String, dynamic> json) {
    return WeightProgress(
      currentWeight: (json['currentWeight'] ?? 0).toDouble(),
      targetWeight: (json['targetWeight'] ?? 0).toDouble(),
      weightLost: (json['weightLost'] ?? 0).toDouble(),
      weightRemaining: (json['weightRemaining'] ?? 0).toDouble(),
      progressPercentage: (json['progressPercentage'] ?? 0).toDouble(),
      weightHistory: (json['weightHistory'] as List<dynamic>?)
              ?.map((e) => WeightDataPoint.fromJson(e))
              .toList() ??
          [],
      daysRemaining: json['daysRemaining'] ?? 0,
      timeRemaining: json['timeRemaining'] ?? '',
    );
  }
}

class WeightDataPoint {
  final DateTime date;
  final double weight;

  WeightDataPoint({required this.date, required this.weight});

  factory WeightDataPoint.fromJson(Map<String, dynamic> json) {
    return WeightDataPoint(
      date: DateTime.parse(json['date']),
      weight: (json['weight'] ?? 0).toDouble(),
    );
  }
}

class TodayStats {
  final int caloriesConsumed;
  final int caloriesTarget;
  final int workoutMinutes;
  final String workoutDuration;

  TodayStats({
    required this.caloriesConsumed,
    required this.caloriesTarget,
    required this.workoutMinutes,
    required this.workoutDuration,
  });

  factory TodayStats.fromJson(Map<String, dynamic> json) {
    return TodayStats(
      caloriesConsumed: json['caloriesConsumed'] ?? 0,
      caloriesTarget: json['caloriesTarget'] ?? 0,
      workoutMinutes: json['workoutMinutes'] ?? 0,
      workoutDuration: json['workoutDuration'] ?? '',
    );
  }
}

class ExerciseStreak {
  final int currentStreak;
  final int totalDays;

  ExerciseStreak({required this.currentStreak, required this.totalDays});

  factory ExerciseStreak.fromJson(Map<String, dynamic> json) {
    return ExerciseStreak(
      currentStreak: json['currentStreak'] ?? 0,
      totalDays: json['totalDays'] ?? 0,
    );
  }
}
