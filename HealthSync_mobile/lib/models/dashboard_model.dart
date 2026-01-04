class CustomerDashboardDto {
  final UserInfoDto userInfo;
  final GoalProgressDto goalProgress;
  final List<GoalSummaryDto> activeGoals;
  final WeightProgressDto weightProgress;
  final TodayStatsDto todayStats;
  final ExerciseStreakDto exerciseStreak;

  CustomerDashboardDto({
    required this.userInfo,
    required this.goalProgress,
    required this.activeGoals,
    required this.weightProgress,
    required this.todayStats,
    required this.exerciseStreak,
  });

  factory CustomerDashboardDto.fromJson(Map<String, dynamic> json) {
    return CustomerDashboardDto(
      userInfo: UserInfoDto.fromJson(json['userInfo']),
      goalProgress: GoalProgressDto.fromJson(json['goalProgress']),
      activeGoals: (json['activeGoals'] as List<dynamic>?)
              ?.map((e) => GoalSummaryDto.fromJson(e))
              .toList() ??
          [],
      weightProgress: WeightProgressDto.fromJson(json['weightProgress']),
      todayStats: TodayStatsDto.fromJson(json['todayStats']),
      exerciseStreak: ExerciseStreakDto.fromJson(json['exerciseStreak']),
    );
  }
}

class UserInfoDto {
  final int userId;
  final String email;
  final String fullName;
  final String avatarUrl;

  UserInfoDto({
    required this.userId,
    required this.email,
    required this.fullName,
    required this.avatarUrl,
  });

  factory UserInfoDto.fromJson(Map<String, dynamic> json) {
    String avatarUrl = json['avatarUrl'] ?? '';
    // Transform localhost to 10.0.2.2 for Android emulator
    if (avatarUrl.contains('localhost')) {
      avatarUrl = avatarUrl.replaceAll('localhost', '10.0.2.2');
    }
    
    return UserInfoDto(
      userId: json['userId'] ?? 0,
      email: json['email'] ?? '',
      fullName: json['fullName'] ?? '',
      avatarUrl: avatarUrl,
    );
  }
}

class GoalProgressDto {
  final String goalType;
  final double startValue;
  final double currentValue;
  final double targetValue;
  final double progress;
  final double progressAmount;
  final double remaining;
  final String status;

  GoalProgressDto({
    required this.goalType,
    required this.startValue,
    required this.currentValue,
    required this.targetValue,
    required this.progress,
    required this.progressAmount,
    required this.remaining,
    required this.status,
  });

  factory GoalProgressDto.fromJson(Map<String, dynamic> json) {
    return GoalProgressDto(
      goalType: json['goalType'] ?? 'None',
      startValue: (json['startValue'] as num?)?.toDouble() ?? 0.0,
      currentValue: (json['currentValue'] as num?)?.toDouble() ?? 0.0,
      targetValue: (json['targetValue'] as num?)?.toDouble() ?? 0.0,
      progress: (json['progress'] as num?)?.toDouble() ?? 0.0,
      progressAmount: (json['progressAmount'] as num?)?.toDouble() ?? 0.0,
      remaining: (json['remaining'] as num?)?.toDouble() ?? 0.0,
      status: json['status'] ?? '',
    );
  }

  String getGoalTypeDisplay() {
    switch (goalType) {
      case 'weight_loss':
        return 'Giảm cân';
      case 'weight_gain':
        return 'Tăng cân';
      case 'muscle_gain':
        return 'Tăng cơ';
      case 'fat_loss':
        return 'Giảm mỡ';
      default:
        return goalType;
    }
  }
}

class GoalSummaryDto {
  final int goalId;
  final String type;
  final String notes;
  final double targetValue;
  final double progress;

  GoalSummaryDto({
    required this.goalId,
    required this.type,
    required this.notes,
    required this.targetValue,
    required this.progress,
  });

  factory GoalSummaryDto.fromJson(Map<String, dynamic> json) {
    return GoalSummaryDto(
      goalId: json['goalId'] ?? 0,
      type: json['type'] ?? '',
      notes: json['notes'] ?? '',
      targetValue: (json['targetValue'] as num?)?.toDouble() ?? 0.0,
      progress: (json['progress'] as num?)?.toDouble() ?? 0.0,
    );
  }
  
  String getTypeDisplay() {
    switch (type) {
      case 'weight_loss':
        return 'Giảm cân';
      case 'weight_gain':
        return 'Tăng cân';
      case 'muscle_gain':
        return 'Tăng cơ';
      case 'fat_loss':
        return 'Giảm mỡ';
      default:
        return type;
    }
  }
}

class WeightProgressDto {
  final double currentWeight;
  final double targetWeight;
  final double weightLost;
  final double weightRemaining;
  final double progressPercentage;
  final List<WeightDataPointDto> weightHistory;
  final String timeRemaining;

  WeightProgressDto({
    required this.currentWeight,
    required this.targetWeight,
    required this.weightLost,
    required this.weightRemaining,
    required this.progressPercentage,
    required this.weightHistory,
    required this.timeRemaining,
  });

  factory WeightProgressDto.fromJson(Map<String, dynamic> json) {
    return WeightProgressDto(
      currentWeight: (json['currentWeight'] as num?)?.toDouble() ?? 0.0,
      targetWeight: (json['targetWeight'] as num?)?.toDouble() ?? 0.0,
      weightLost: (json['weightLost'] as num?)?.toDouble() ?? 0.0,
      weightRemaining: (json['weightRemaining'] as num?)?.toDouble() ?? 0.0,
      progressPercentage: (json['progressPercentage'] as num?)?.toDouble() ?? 0.0,
      weightHistory: (json['weightHistory'] as List<dynamic>?)
              ?.map((e) => WeightDataPointDto.fromJson(e))
              .toList() ??
          [],
      timeRemaining: json['timeRemaining'] ?? '',
    );
  }
}

class WeightDataPointDto {
  final DateTime date;
  final double weight;

  WeightDataPointDto({
    required this.date,
    required this.weight,
  });

  factory WeightDataPointDto.fromJson(Map<String, dynamic> json) {
    return WeightDataPointDto(
      date: DateTime.parse(json['date']),
      weight: (json['weight'] as num).toDouble(),
    );
  }
}

class TodayStatsDto {
  final int caloriesConsumed;
  final int caloriesTarget;
  final String workoutDuration;

  TodayStatsDto({
    required this.caloriesConsumed,
    required this.caloriesTarget,
    required this.workoutDuration,
  });

  factory TodayStatsDto.fromJson(Map<String, dynamic> json) {
    return TodayStatsDto(
      caloriesConsumed: json['caloriesConsumed'] ?? 0,
      caloriesTarget: json['caloriesTarget'] ?? 2000,
      workoutDuration: json['workoutDuration'] ?? '0 min',
    );
  }
}

class ExerciseStreakDto {
  final int currentStreak;

  ExerciseStreakDto({
    required this.currentStreak,
  });

  factory ExerciseStreakDto.fromJson(Map<String, dynamic> json) {
    return ExerciseStreakDto(
      currentStreak: json['currentStreak'] ?? 0,
    );
  }
}
