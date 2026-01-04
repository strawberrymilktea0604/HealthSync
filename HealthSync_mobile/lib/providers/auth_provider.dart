import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'dart:convert';
import 'dart:io';
import '../models/user_model.dart';
import '../services/api_service.dart';

class AuthProvider with ChangeNotifier {
  User? _user;
  bool _isLoading = false;
  String? _error;
  final ApiService _apiService = ApiService();
  // Khởi tạo GoogleSignIn KHÔNG CẦN clientId - sẽ lấy từ google-services.json
  final GoogleSignIn _googleSignIn = GoogleSignIn(
    scopes: ['email', 'profile'],
  );

  User? get user => _user;
  bool get isLoading => _isLoading;
  String? get error => _error;
  bool get isAuthenticated => _user != null;

  AuthProvider() {
    // Constructor no longer auto-calls _loadUser since we will call it explicitly in Splash
  }

  // Load user from shared preferences
  Future<bool> tryAutoLogin() async {
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString('user');
    if (userJson != null) {
      try {
        final userData = jsonDecode(userJson);
        final expiresAt = DateTime.parse(userData['expiresAt']);
        // Check if token is still valid (add strict check)
        if (expiresAt.isAfter(DateTime.now())) {
          final tempUser = User.fromJson(userData);
          
          // FIX: Only auto-login if profile is complete. 
          // If profile is incomplete, we force logout so user is returned to Welcome/Login screen on restart.
          if (tempUser.isProfileComplete || tempUser.role == 'Admin') {
            _user = tempUser;
            notifyListeners();
            return true;
          } else {
             await logout(); // Incomplete profile -> Logout
          }
        } else {
          await logout();
        }
      } catch (e) {
        await logout();
      }
    }
    return false;
  }

  // Save user to shared preferences
  Future<void> _saveUser(User user) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('user', jsonEncode(user.toJson()));
    _user = user;
    notifyListeners();
  }

  // Login
  Future<void> login(String email, String password) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final user = await _apiService.login(email, password);

      if (user.role == 'Admin') {
        throw Exception('Tài khoản Admin không được phép đăng nhập trên ứng dụng mobile.');
      }

      await _saveUser(user);
    } catch (e) {
      _error = e.toString().replaceAll('Exception: ', '');
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  // Send verification code
  Future<void> sendVerificationCode(String email) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      await _apiService.sendVerificationCode(email);
    } catch (e) {
      _error = e.toString().replaceAll('Exception: ', '');
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  // Forgot Password
  Future<void> forgotPassword(String email) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      await _apiService.forgotPassword(email);
    } catch (e) {
      _error = e.toString().replaceAll('Exception: ', '');
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  // Verify Reset OTP
  Future<String> verifyResetOtp(String email, String otp) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final token = await _apiService.verifyResetOtp(email, otp);
      return token;
    } catch (e) {
      _error = e.toString().replaceAll('Exception: ', '');
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  // Register
  Future<void> register({
    required String email,
    required String password,
    required String verificationCode,
  }) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final user = await _apiService.register(
        email: email,
        password: password,
        verificationCode: verificationCode,
      );
      await _saveUser(user);
    } catch (e) {
      _error = e.toString().replaceAll('Exception: ', '');
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }



  // Google Sign In
  Future<void> signInWithGoogle() async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      // Sign out first to force account selection
      await _googleSignIn.signOut();
      
      final GoogleSignInAccount? googleUser = await _googleSignIn.signIn();
      if (googleUser == null) {
        throw Exception('Google sign in cancelled');
      }

      final GoogleSignInAuthentication googleAuth = await googleUser.authentication;
      final String? idToken = googleAuth.idToken;

      if (idToken == null) {
        throw Exception(
          'Failed to get ID token.\n\n'
          'Kiểm tra:\n'
          '1. File google-services.json đã được thêm vào android/app/\n'
          '2. OAuth Client ID đã được tạo với SHA-1 fingerprint\n'
          '3. Package name: com.example.healthsync_mobile'
        );
      }

      final user = await _apiService.googleLoginMobile(idToken);
      
      if (user.role == 'Admin') {
        throw Exception('Tài khoản Admin không được phép đăng nhập trên ứng dụng mobile.');
      }
      
      await _saveUser(user);
    } catch (e) {
      _error = e.toString().replaceAll('Exception: ', '');
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  // Update user profile
  Future<void> updateProfile({
    required String fullName,
    required DateTime dob,
    required String gender,
    required double heightCm,
    required double weightKg,
    required String activityLevel,
  }) async {
    if (_user == null) throw Exception('User not authenticated');

    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      await _apiService.updateProfile(
        fullName: fullName,
        dob: dob,
        gender: gender,
        heightCm: heightCm,
        weightKg: weightKg,
        activityLevel: activityLevel,
      );

      // Update user data
      _user = User(
        userId: _user!.userId,
        email: _user!.email,
        fullName: fullName,
        role: _user!.role,
        token: _user!.token,
        expiresAt: _user!.expiresAt,
        requiresPassword: _user!.requiresPassword,
        isProfileComplete: true,
      );
      await _saveUser(_user!);
    } catch (e) {
      _error = e.toString().replaceAll('Exception: ', '');
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  // Upload Avatar
  Future<void> uploadAvatar(File file) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final avatarUrl = await _apiService.uploadAvatar(file);
      updateUserAvatar(avatarUrl);
    } catch (e) {
      _error = e.toString().replaceAll('Exception: ', '');
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  // Logout
  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('user');
    await _googleSignIn.signOut();
    _user = null;
    notifyListeners();
  }

  // Set password for Google OAuth users
  Future<void> setPassword({
    required int userId,
    required String password,
  }) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      await _apiService.setPassword(userId: userId, password: password);
      
      // Update user's requiresPassword flag
      if (_user != null) {
        _user = User(
          userId: _user!.userId,
          email: _user!.email,
          fullName: _user!.fullName,
          role: _user!.role,
          token: _user!.token,
          expiresAt: _user!.expiresAt,
          requiresPassword: false,
        );
        await _saveUser(_user!);
      }
    } catch (e) {
      _error = e.toString().replaceAll('Exception: ', '');
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  // Reset password
  Future<void> resetPassword({
    required String token,
    required String newPassword,
  }) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      await _apiService.resetPassword(
        token: token,
        newPassword: newPassword,
      );
    } catch (e) {
      _error = e.toString().replaceAll('Exception: ', '');
      rethrow;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  void clearError() {
    _error = null;
    notifyListeners();
  }

  // Update user avatar URL
  void updateUserAvatar(String avatarUrl) {
    if (_user != null) {
      _user = User(
        userId: _user!.userId,
        email: _user!.email,
        fullName: _user!.fullName,
        role: _user!.role,
        token: _user!.token,
        expiresAt: _user!.expiresAt,
        requiresPassword: _user!.requiresPassword,
        isProfileComplete: _user!.isProfileComplete,
        avatarUrl: avatarUrl,
      );
      _saveUser(_user!);
    }
  }
}
