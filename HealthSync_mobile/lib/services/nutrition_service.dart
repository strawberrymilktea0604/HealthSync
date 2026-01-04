import 'api_service.dart';

class FoodItem {
  final int foodItemId;
  final String name;
  final double servingSize;
  final String servingUnit;
  final double caloriesKcal;
  final double proteinG;
  final double carbsG;
  final double fatG;
  final String? imageUrl;

  FoodItem({
    required this.foodItemId,
    required this.name,
    required this.servingSize,
    required this.servingUnit,
    required this.caloriesKcal,
    required this.proteinG,
    required this.carbsG,
    required this.fatG,
    this.imageUrl,
  });

  factory FoodItem.fromJson(Map<String, dynamic> json) {
    String? imageUrl = json['imageUrl'];
    // Transform localhost to 10.0.2.2 for Android emulator
    if (imageUrl != null && imageUrl.contains('localhost')) {
      imageUrl = imageUrl.replaceAll('localhost', '10.0.2.2');
    }
    
    return FoodItem(
      foodItemId: json['foodItemId'],
      name: json['name'],
      servingSize: (json['servingSize'] as num).toDouble(),
      servingUnit: json['servingUnit'],
      caloriesKcal: (json['caloriesKcal'] as num).toDouble(),
      proteinG: (json['proteinG'] as num).toDouble(),
      carbsG: (json['carbsG'] as num).toDouble(),
      fatG: (json['fatG'] as num).toDouble(),
      imageUrl: imageUrl,
    );
  }
}

class FoodEntry {
  final int foodEntryId;
  final int foodItemId;
  final String foodItemName;
  final double quantity;
  final String mealType;
  final double caloriesKcal;
  final double proteinG;
  final double carbsG;
  final double fatG;
  final String? imageUrl;

  FoodEntry({
    required this.foodEntryId,
    required this.foodItemId,
    required this.foodItemName,
    required this.quantity,
    required this.mealType,
    required this.caloriesKcal,
    required this.proteinG,
    required this.carbsG,
    required this.fatG,
    this.imageUrl,
  });

  factory FoodEntry.fromJson(Map<String, dynamic> json) {
    String? imageUrl = json['imageUrl'];
    // Transform localhost to 10.0.2.2 for Android emulator
    if (imageUrl != null && imageUrl.contains('localhost')) {
      imageUrl = imageUrl.replaceAll('localhost', '10.0.2.2');
    }
    
    return FoodEntry(
      foodEntryId: json['foodEntryId'],
      foodItemId: json['foodItemId'],
      foodItemName: json['foodItemName'],
      quantity: (json['quantity'] as num).toDouble(),
      mealType: json['mealType'],
      caloriesKcal: (json['caloriesKcal'] as num).toDouble(),
      proteinG: (json['proteinG'] as num).toDouble(),
      carbsG: (json['carbsG'] as num).toDouble(),
      fatG: (json['fatG'] as num).toDouble(),
      imageUrl: imageUrl,
    );
  }
}

class NutritionLog {
  final int nutritionLogId;
  final DateTime logDate;
  final double totalCalories;
  final double proteinG;
  final double carbsG;
  final double fatG;
  final List<FoodEntry> foodEntries;

  NutritionLog({
    required this.nutritionLogId,
    required this.logDate,
    required this.totalCalories,
    required this.proteinG,
    required this.carbsG,
    required this.fatG,
    required this.foodEntries,
  });

  factory NutritionLog.fromJson(Map<String, dynamic> json) {
    return NutritionLog(
      nutritionLogId: json['nutritionLogId'],
      logDate: DateTime.parse(json['logDate']),
      totalCalories: (json['totalCalories'] as num).toDouble(),
      proteinG: (json['proteinG'] as num).toDouble(),
      carbsG: (json['carbsG'] as num).toDouble(),
      fatG: (json['fatG'] as num).toDouble(),
      foodEntries: (json['foodEntries'] as List)
          .map((e) => FoodEntry.fromJson(e))
          .toList(),
    );
  }
}

class NutritionService {
  final ApiService _apiService = ApiService();

  Future<List<FoodItem>> getFoodItems({String? search}) async {
    try {
      final queryParams = search != null ? {'search': search} : null;
      final response = await _apiService.get('/nutrition/food-items', queryParameters: queryParams);
      return (response as List).map((e) => FoodItem.fromJson(e)).toList();
    } catch (e) {
      // print('Error getting food items: $e');
      rethrow;
    }
  }

  Future<NutritionLog?> getNutritionLogByDate(DateTime date) async {
    try {
      // Convert to UTC and format as ISO string to match backend expectations
      final utcDate = DateTime.utc(date.year, date.month, date.day);
      final dateString = utcDate.toIso8601String();
      final response = await _apiService.get(
        '/nutrition/nutrition-log',
        queryParameters: {'date': dateString},
      );
      return response != null ? NutritionLog.fromJson(response) : null;
    } catch (e) {
      return null;
    }
  }

  Future<Map<String, dynamic>> addFoodEntry({
    required int foodItemId,
    required double quantity,
    required String mealType,
    DateTime? logDate,
  }) async {
    try {
      // Use provided date or current date in UTC
      final date = logDate ?? DateTime.now();
      final utcDate = DateTime.utc(date.year, date.month, date.day);
      final response = await _apiService.post(
        '/nutrition/food-entry',
        body: {
          'foodItemId': foodItemId,
          'quantity': quantity,
          'mealType': mealType,
          'logDate': utcDate.toIso8601String(),
        },
      );
      return response;
    } catch (e) {
      rethrow;
    }
  }

  Future<void> deleteFoodEntry(int foodEntryId) async {
    try {
      await _apiService.delete('/nutrition/food-entry/$foodEntryId');
    } catch (e) {
      // print('Error deleting food entry: $e');
      rethrow;
    }
  }

  Future<List<NutritionLog>> getNutritionLogs({
    DateTime? startDate,
    DateTime? endDate,
  }) async {
    try {
      final queryParams = <String, String>{};
      if (startDate != null) {
        // Convert to UTC date to avoid timezone issues
        final utcDate = DateTime.utc(startDate.year, startDate.month, startDate.day);
        queryParams['startDate'] = utcDate.toIso8601String();
      }
      if (endDate != null) {
        // Convert to UTC date to avoid timezone issues
        final utcDate = DateTime.utc(endDate.year, endDate.month, endDate.day);
        queryParams['endDate'] = utcDate.toIso8601String();
      }
      
      final response = await _apiService.get(
        '/nutrition/nutrition-logs',
        queryParameters: queryParams.isNotEmpty ? queryParams : null,
      );
      
      if (response == null) {
        return [];
      }
      
      if (response is! List) {
        return [];
      }
      
      final logs = (response as List).map((e) => NutritionLog.fromJson(e)).toList();
      return logs;
    } catch (e) {
      rethrow;
    }
  }
}
