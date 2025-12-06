class Exercise {
  final int exerciseId;
  final String name;
  final String muscleGroup;
  final String difficulty;
  final String? equipment;
  final String? description;

  Exercise({
    required this.exerciseId,
    required this.name,
    required this.muscleGroup,
    required this.difficulty,
    this.equipment,
    this.description,
  });

  factory Exercise.fromJson(Map<String, dynamic> json) {
    return Exercise(
      exerciseId: json['exerciseId'],
      name: json['name'],
      muscleGroup: json['muscleGroup'],
      difficulty: json['difficulty'],
      equipment: json['equipment'],
      description: json['description'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'exerciseId': exerciseId,
      'name': name,
      'muscleGroup': muscleGroup,
      'difficulty': difficulty,
      'equipment': equipment,
      'description': description,
    };
  }
}

class ExerciseSession {
  final int exerciseId;
  final int sets;
  final int reps;
  final double weightKg;
  final int? restSec;
  final double? rpe;

  ExerciseSession({
    required this.exerciseId,
    required this.sets,
    required this.reps,
    required this.weightKg,
    this.restSec,
    this.rpe,
  });

  factory ExerciseSession.fromJson(Map<String, dynamic> json) {
    return ExerciseSession(
      exerciseId: json['exerciseId'],
      sets: json['sets'],
      reps: json['reps'],
      weightKg: (json['weightKg'] as num).toDouble(),
      restSec: json['restSec'],
      rpe: json['rpe'] != null ? (json['rpe'] as num).toDouble() : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'exerciseId': exerciseId,
      'sets': sets,
      'reps': reps,
      'weightKg': weightKg,
      'restSec': restSec,
      'rpe': rpe,
    };
  }
}

class WorkoutLog {
  final int workoutLogId;
  final int userId;
  final DateTime workoutDate;
  final int durationMin;
  final String? notes;
  final List<WorkoutExerciseSession> exerciseSessions;

  WorkoutLog({
    required this.workoutLogId,
    required this.userId,
    required this.workoutDate,
    required this.durationMin,
    this.notes,
    required this.exerciseSessions,
  });

  factory WorkoutLog.fromJson(Map<String, dynamic> json) {
    return WorkoutLog(
      workoutLogId: json['workoutLogId'],
      userId: json['userId'],
      workoutDate: DateTime.parse(json['workoutDate']),
      durationMin: json['durationMin'],
      notes: json['notes'],
      exerciseSessions: (json['exerciseSessions'] as List)
          .map((e) => WorkoutExerciseSession.fromJson(e))
          .toList(),
    );
  }
}

class WorkoutExerciseSession {
  final int exerciseSessionId;
  final int exerciseId;
  final String exerciseName;
  final int sets;
  final int reps;
  final double weightKg;
  final int? restSec;
  final double? rpe;

  WorkoutExerciseSession({
    required this.exerciseSessionId,
    required this.exerciseId,
    required this.exerciseName,
    required this.sets,
    required this.reps,
    required this.weightKg,
    this.restSec,
    this.rpe,
  });

  factory WorkoutExerciseSession.fromJson(Map<String, dynamic> json) {
    return WorkoutExerciseSession(
      exerciseSessionId: json['exerciseSessionId'],
      exerciseId: json['exerciseId'],
      exerciseName: json['exerciseName'],
      sets: json['sets'],
      reps: json['reps'],
      weightKg: (json['weightKg'] as num).toDouble(),
      restSec: json['restSec'],
      rpe: json['rpe'] != null ? (json['rpe'] as num).toDouble() : null,
    );
  }
}

class CreateWorkoutLog {
  final DateTime workoutDate;
  final int durationMin;
  final String? notes;
  final List<ExerciseSession> exerciseSessions;

  CreateWorkoutLog({
    required this.workoutDate,
    required this.durationMin,
    this.notes,
    required this.exerciseSessions,
  });

  Map<String, dynamic> toJson() {
    return {
      'workoutDate': workoutDate.toIso8601String(),
      'durationMin': durationMin,
      'notes': notes,
      'exerciseSessions': exerciseSessions.map((e) => e.toJson()).toList(),
    };
  }
}
