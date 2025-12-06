class Goal {
  final int goalId;
  final String type;
  final double targetValue;
  final DateTime startDate;
  final DateTime? endDate;
  final String status;
  final String? notes;
  final List<ProgressRecord> progressRecords;

  Goal({
    required this.goalId,
    required this.type,
    required this.targetValue,
    required this.startDate,
    this.endDate,
    required this.status,
    this.notes,
    required this.progressRecords,
  });

  factory Goal.fromJson(Map<String, dynamic> json) {
    return Goal(
      goalId: json['goalId'] as int,
      type: json['type'] as String,
      targetValue: (json['targetValue'] as num).toDouble(),
      startDate: DateTime.parse(json['startDate'] as String),
      endDate: json['endDate'] != null 
          ? DateTime.parse(json['endDate'] as String) 
          : null,
      status: json['status'] as String,
      notes: json['notes'] as String?,
      progressRecords: (json['progressRecords'] as List<dynamic>?)
              ?.map((e) => ProgressRecord.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'goalId': goalId,
      'type': type,
      'targetValue': targetValue,
      'startDate': startDate.toIso8601String(),
      'endDate': endDate?.toIso8601String(),
      'status': status,
      'notes': notes,
      'progressRecords': progressRecords.map((e) => e.toJson()).toList(),
    };
  }

  String getTypeDisplay() {
    const types = {
      'weight_loss': 'Giảm cân',
      'weight_gain': 'Tăng cân',
      'muscle_gain': 'Tăng cơ',
      'fat_loss': 'Giảm mỡ',
    };
    return types[type] ?? type;
  }

  double calculateProgress() {
    if (progressRecords.isEmpty) return 0;

    final sortedRecords = List<ProgressRecord>.from(progressRecords)
      ..sort((a, b) => a.recordDate.compareTo(b.recordDate));

    final startValue = sortedRecords.first.value;
    final currentValue = sortedRecords.last.value;

    if (type == 'weight_loss' || type == 'fat_loss') {
      final progress = ((startValue - currentValue) / (startValue - targetValue)) * 100;
      return progress.clamp(0, 100);
    } else {
      final progress = ((currentValue - startValue) / (targetValue - startValue)) * 100;
      return progress.clamp(0, 100);
    }
  }

  double? getCurrentValue() {
    if (progressRecords.isEmpty) return null;
    final sortedRecords = List<ProgressRecord>.from(progressRecords)
      ..sort((a, b) => b.recordDate.compareTo(a.recordDate));
    return sortedRecords.first.value;
  }
}

class ProgressRecord {
  final int progressRecordId;
  final DateTime recordDate;
  final double value;
  final String? notes;
  final double weightKg;
  final double waistCm;

  ProgressRecord({
    required this.progressRecordId,
    required this.recordDate,
    required this.value,
    this.notes,
    required this.weightKg,
    required this.waistCm,
  });

  factory ProgressRecord.fromJson(Map<String, dynamic> json) {
    return ProgressRecord(
      progressRecordId: json['progressRecordId'] as int,
      recordDate: DateTime.parse(json['recordDate'] as String),
      value: (json['value'] as num).toDouble(),
      notes: json['notes'] as String?,
      weightKg: (json['weightKg'] as num?)?.toDouble() ?? 0.0,
      waistCm: (json['waistCm'] as num?)?.toDouble() ?? 0.0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'progressRecordId': progressRecordId,
      'recordDate': recordDate.toIso8601String(),
      'value': value,
      'notes': notes,
      'weightKg': weightKg,
      'waistCm': waistCm,
    };
  }
}

class CreateGoalRequest {
  final String type;
  final double targetValue;
  final DateTime startDate;
  final DateTime? endDate;
  final String? notes;

  CreateGoalRequest({
    required this.type,
    required this.targetValue,
    required this.startDate,
    this.endDate,
    this.notes,
  });

  Map<String, dynamic> toJson() {
    return {
      'type': type,
      'targetValue': targetValue,
      'startDate': startDate.toIso8601String(),
      'endDate': endDate?.toIso8601String(),
      'notes': notes,
    };
  }
}

class AddProgressRequest {
  final DateTime recordDate;
  final double value;
  final String? notes;
  final double? weightKg;
  final double? waistCm;

  AddProgressRequest({
    required this.recordDate,
    required this.value,
    this.notes,
    this.weightKg,
    this.waistCm,
  });

  Map<String, dynamic> toJson() {
    return {
      'recordDate': recordDate.toIso8601String(),
      'value': value,
      'notes': notes,
      'weightKg': weightKg,
      'waistCm': waistCm,
    };
  }
}
