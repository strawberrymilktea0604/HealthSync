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
      final response = await _apiService.get(
        '/nutrition/nutrition-log',
        queryParameters: {'date': date.toIso8601String()},
      );
      return response != null ? NutritionLog.fromJson(response) : null;
    } catch (e) {
      // print('Error getting nutrition log: $e');
      return null;
    }
  }

  Future<Map<String, dynamic>> addFoodEntry({
    required int foodItemId,
    required double quantity,
    required String mealType,
  }) async {
    try {
      final response = await _apiService.post(
        '/nutrition/food-entry',
        body: {
          'foodItemId': foodItemId,
          'quantity': quantity,
          'mealType': mealType,
        },
      );
      return response;
    } catch (e) {
      // print('Error adding food entry: $e');
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
        queryParams['startDate'] = startDate.toIso8601String();
      }
      if (endDate != null) {
        queryParams['endDate'] = endDate.toIso8601String();
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
